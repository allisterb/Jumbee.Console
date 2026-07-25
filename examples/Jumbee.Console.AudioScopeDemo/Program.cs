using System.CommandLine;

using Jumbee.Console;
using ScopeTui;

// --- scope-tui as THREE simultaneous panes, all fed from ONE audio source ---------------------------------------
// An oscilloscope across the top, a spectroscope (bottom-left) and a vectorscope (bottom-right) below -- each its
// own ScopeView control, each driven by its OWN Control.Feed, all reading the SAME audio from one ChannelBus. The
// IAudioSource (a decoded file, or a live capture device) is single-threaded, so exactly ONE pump reads and
// publishes; the three panes fan out from the bus, computing their (different) transforms off the UI thread in
// parallel -- a deliberate stress test of Control.Feed: four concurrent feeds, three consumers marshalling onto the
// one UI thread.
//
// Audio input (the 'input' argument): 'file' decodes the mp3 argument (MP3 via NLayer + WAV, both fully-managed/
// cross-platform); 'live' captures a recording device live -- WASAPI on Windows, ALSA on Linux (NAudio 3), so the
// scope works on a real box (e.g. an industrial-automation Linux target) as well as from a file. 'live' defaults to
// the platform's default recording endpoint; --loopback scopes what is PLAYING instead, and --device picks a
// specific endpoint (by index, partial name, or raw ID) out of --list-devices.
//
// CLI (System.CommandLine): file|live [--path FILE] [--fps N] [--sample-rate HZ] [--buffer N] [--scatter]
// [--device D] [--loopback] [--mono] [--follow] [--ticks N] [--list-devices]. Two independent clocks
// drive the scope: the FEED period (how often the source is sampled and the waveforms recompute) and the UI paint
// FPS cap. By default the feed follows --fps (1000/N ms); --sample-rate decouples it (1000/HZ ms) so the DATA can
// refresh at its own rate, and --follow ties it to the audio instead (one buffer per tick, so every frame is fresh
// samples rather than a mostly-overlapping window -- see the feed-period note below). Both are distinct from the PCM
// rate, which only sets the waveform's scroll speed. --buffer sets how many samples/channel each frame carries (the
// oscilloscope window and the FFT size).

var inputArg = new Argument<string?>("input")
{
    Arity = ArgumentArity.ExactlyOne,
    Description = "Selects the audio input: 'file' decodes the mp3 argument; 'live' captures the default recording device.",
}
.AcceptOnlyFromAmong("file", "live"); ;
var pathOpt = new Option<string?>("path")
{
    Arity = ArgumentArity.ZeroOrOne,
    Description = "Path to an MP3 file to decode. Defaults to the bundled sample track.",
}.AcceptLegalFilePathsOnly();
var fpsOpt = new Option<int>("--fps")
{
    Description = "UI paint-rate cap in frames/sec. Also sets the data refresh rate unless --sample-rate is given.",
    DefaultValueFactory = _ => 24,
};
var sampleRateOpt = new Option<int?>("--sample-rate")
{
    Description = "How many times/sec the source is sampled, decoupling the data rate from --fps (feed = 1000/HZ ms).",
};
var bufferOpt = new Option<int>("--buffer")
{
    Description = "Samples per channel decoded per frame. Sets the oscilloscope window and the FFT size.",
    DefaultValueFactory = _ => 2048,
};
var scatterOpt = new Option<bool>("--scatter")
{
    Description = "Start in scatter mode (draw points) instead of connected lines. Toggle at runtime with 's'.",
};
var deviceOpt = new Option<string?>("--device")
{
    Description = "Live capture endpoint: an index or partial name from --list-devices, or a raw endpoint ID / ALSA PCM name.",
};
var loopbackOpt = new Option<bool>("--loopback")
{
    Description = "Scope what is PLAYING rather than what is recording (WASAPI loopback / PulseAudio monitor).",
};
var ticksOpt = new Option<int>("--ticks")
{
    Description = "Cells between horizontal ticks/grid lines on the osc + spectro panes; bigger is sparser, 0 removes the chrome.",
    DefaultValueFactory = _ => 11,
};
var followOpt = new Option<bool>("--follow")
{
    Description = "Advance one buffer per tick (feed = buffer/sample-rate), so each frame is fresh audio. --fps still caps painting.",
};
var monoOpt = new Option<bool>("--mono")
{
    Description = "Scope a single channel: opens a 1-channel stream where the backend allows it, else averages the device's channels.",
};
var listDevicesOpt = new Option<bool>("--list-devices")
{
    Description = "Print the live capture endpoints --device can select (honours --loopback) and exit.",
};

var root = new RootCommand("AudioScope -- view an oscilloscope, spectroscope, and vectorscope from audio input.")
{
    inputArg, pathOpt, fpsOpt, sampleRateOpt, bufferOpt, scatterOpt, deviceOpt, loopbackOpt, followOpt, monoOpt,
    ticksOpt, listDevicesOpt,
};

root.SetAction(async (parse, ct) =>
{
    var input = parse.GetValue(inputArg)!;
    var filePath = parse.GetValue(pathOpt) ?? @"C:\Projects\Jumbee.Console\reference\media\02 - Girlfriend.mp3";
    var fps = Math.Clamp(parse.GetValue(fpsOpt), 1, 240);
    var sampleRate = parse.GetValue(sampleRateOpt);
    var bufferSamples = Math.Clamp(parse.GetValue(bufferOpt), 64, 1 << 16);
    var startScatter = parse.GetValue(scatterOpt);
    var deviceSpec = parse.GetValue(deviceOpt);
    var loopback = parse.GetValue(loopbackOpt);
    var mono = parse.GetValue(monoOpt);
    var follow = parse.GetValue(followOpt);
    // Upper bound is a usability limit, not a safety one: past ~40 cells a pane has so few ticks left that the axis
    // stops being readable, and ScopeView clamps again to the pane's own width.
    var ticks = Math.Clamp(parse.GetValue(ticksOpt), 0, 40);
    var live = input.Equals("live", StringComparison.OrdinalIgnoreCase);

    // --list-devices is a query, not a run: print the endpoints --device selects and exit before any UI starts.
    if (parse.GetValue(listDevicesOpt))
    {
        try
        {
            var devices = RecordingAudioSource.ListDevices(loopback);
            Console.WriteLine(loopback ? "Playback endpoints (--loopback):" : "Recording endpoints:");
            foreach (var d in devices) Console.WriteLine($"  [{d.Index}] {d.Name}{(d.IsDefault ? "  (default)" : "")}\n        {d.Id}");
            if (devices.Count == 0) Console.WriteLine("  (none)");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Could not enumerate audio devices: {ex.Message}");
            return 1;
        }
        return 0;
    }

    if (!live && (deviceSpec is not null || loopback || mono))
    {
        Console.Error.WriteLine("--device, --loopback and --mono apply to 'live' input only.");
        return 1;
    }

    if (follow && sampleRate is not null)
    {
        Console.Error.WriteLine("--follow and --sample-rate both set the feed rate; pass only one.");
        return 1;
    }

    // Fixed calibration gain on the decoded samples (see GraphConfig.Gain / Oscilloscope.Process): NAudio's floats
    // are normalized to [-1,1], so a typical passage only fills a small slice; this makes the default view fill the
    // axis the way scope-tui's raw-sample-space plot does, without touching the interactive Scale knob.
    const double AmplitudeGain = 5.0;
    IAudioSource audio;
    try
    {
        audio = live
            ? new RecordingAudioSource(bufferSamples, RecordingAudioSource.ResolveDevice(deviceSpec, loopback), loopback, mono)
            : new FileAudioSource(filePath, bufferSamples);     // decode the mp3 (default)
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Could not open audio input '{input}': {ex.Message}");
        return 1;
    }

    // Feed period, in precedence order: --follow, then --sample-rate (1000/rate), else --fps (1000/fps).
    //
    // --follow ties the feed to the AUDIO instead of the clock: one buffer's worth of samples per tick, which is the
    // cadence scope-tui gets for free by blocking on its buffer queue (one frame per capture callback, contiguous and
    // non-overlapping). Without it the feed runs at its own rate against a rolling latest-window, so consecutive
    // frames overlap -- at 180fps against a 2048-sample window only ~260 samples per frame are new and the trace
    // barely moves. A window can only be fully replaced sampleRate/buffer times a second (~23Hz for 2048 @ 48kHz),
    // so any faster feed MUST re-show samples; --follow just sits exactly on that limit.
    //
    // The cadence matches; contiguity is still best-effort. A live device hands us a rolling latest-window rather
    // than a queue, so timer jitter can re-show or skip a few samples at the seam -- scope-tui cannot, since it
    // consumes every buffer in order (and drifts behind realtime instead, if the UI stalls).
    var feedMs = follow
        ? Math.Max(1, (int)Math.Round(1000.0 * bufferSamples / audio.SampleRate))
        : sampleRate is { } rate
            ? Math.Max(1, (int)Math.Round(1000.0 / Math.Clamp(rate, 1, 1000)))
            : Math.Max(1, (int)Math.Round(1000.0 / fps));
    var feedInterval = TimeSpan.FromMilliseconds(feedMs);

    // The single fan-out point and the single decoder that fills it (see ChannelBus / AudioPump).
    var bus = new ChannelBus();
    var pump = new AudioPump(audio, bus);

    // ONE GraphConfig per pane, so scale / samples / scatter / pause / references / hide-UI adjust ONLY the focused
    // pane -- each scope is independently controllable. Each starts identical.
    GraphConfig NewConfig() => new()
    {
        Samples = bufferSamples,
        Width = bufferSamples,
        Gain = AmplitudeGain,
        SampleRate = audio.SampleRate,
        Scatter = startScatter,
    };
    var oscCfg = NewConfig();
    var spectroCfg = NewConfig();
    var vectorCfg = NewConfig();
    GraphConfig[] configs = [oscCfg, spectroCfg, vectorCfg];
    foreach (var c in configs) c.Publish(); // publish now the object-initializer has set the real field values

    // One mode instance and one ScopeView per pane -- each pane is FIXED to its mode. Osc/spectro own their hotkey
    // knobs (trigger, FFT window/averaging); vector has none.
    var osc = new Oscilloscope();
    var spectro = new Spectroscope(audio.SampleRate, bufferSamples);
    var vec = new Vectorscope();

    // --ticks applies to the two wide, full-width panes only. The vectorscope keeps the default: it is a ~square
    // Lissajous pane about a fifth of the width, and both its axes are amplitude -- not the time/frequency axis whose
    // grid density is the thing worth thinning.
    var oscPane = new ScopeView(xTickStep: ticks);
    var spectroPane = new ScopeView(xTickStep: ticks);
    var vectorPane = new ScopeView();

    // Panes and their modes/configs in Tab-focus order (osc -> spectro -> vector), index-aligned.
    ScopeView[] panes = [oscPane, spectroPane, vectorPane];
    IDisplayMode[] paneModes = [osc, spectro, vec];

    // Each pane gets a border frame -- to separate the panes and to carry the focus cue: a frame recolours its border
    // to the theme's focused colour (BorderFocusedText) while its control is focused, and the at-rest colour
    // (BorderText) otherwise, automatically. So the focused scope's border stands out with zero manual bookkeeping.
    _ = new ControlFrame(oscPane, borderStyle: BorderStyle.Rounded);
    _ = new ControlFrame(spectroPane, borderStyle: BorderStyle.Rounded);
    _ = new ControlFrame(vectorPane, borderStyle: BorderStyle.Rounded);

    // Layout: oscilloscope full-width on top; spectroscope (wide, left) + vectorscope (square, right) share the row
    // below. Both split positions are recomputed on resize (below) so the top stays ~half and the vector stays ~square.
    var bottomSplit = new SplitPanel(SplitOrientation.Horizontal, spectroPane, vectorPane, splitPosition: 70);
    var outerSplit = new SplitPanel(SplitOrientation.Vertical, oscPane, bottomSplit, splitPosition: 12);

    // Which pane currently has keyboard focus (set by a click -- ScopeView opts into the mouse for click-to-focus --
    // or Tab). Hotkeys route to this pane; defaults to the top pane before the first focus lands.
    int FocusedIndex()
    {
        for (var i = 0; i < panes.Length; i++) if (panes[i].IsFocused) return i;
        return 0;
    }
    GraphConfig FocusedConfig() => configs[FocusedIndex()];

    // --- FPS counter + resize-driven split recompute, both on the UI thread via the per-frame Paint hook. -----------
    var framerate = 0;
    var paintCount = 0;
    var lastPoll = DateTime.UtcNow;
    (int W, int H) lastSize = (0, 0);
    UI.Paint += (_, _) =>
    {
        paintCount++;
        var now = DateTime.UtcNow;
        if ((now - lastPoll).TotalSeconds >= 1) { framerate = paintCount; paintCount = 0; lastPoll = now; }

        // Only reproportion on an ACTUAL terminal resize -- not every frame -- so a manual divider drag survives
        // between resizes (setting SplitPosition every frame would fight the drag and snap it back).
        var w = outerSplit.Size.Width;
        var h = outerSplit.Size.Height;
        if (w > 0 && h > 0 && (w, h) != lastSize)
        {
            lastSize = (w, h);
            outerSplit.SplitPosition = h / 2;                          // osc = top half
            var bottomH = h - h / 2 - 1;                               // bottom row height (minus the divider)
            var vectorW = Math.Min(w - 12, Math.Max(12, 2 * bottomH)); // ~square: console cells are ~2:1 tall:wide
            bottomSplit.SplitPosition = Math.Max(12, w - vectorW - 1); // spectro (first/left) gets the remainder
        }
    };

    // Prime the bus with one frame so the panes have data to draw on their very first tick (decoded here on the main
    // thread, before the UI loop starts -- fine, it's the same single-threaded reader the pump will continue from).
    bus.Publish(audio.NextFrame());

    // --- The fan-out: each pane runs its OWN self-driving compute feed off the one shared bus. -------------------
    FeedHandle[] paneFeeds =
    [
        oscPane.StartComputeFeed(bus, osc, oscCfg, () => framerate, feedInterval, ex => oscPane.SetError($"render failed: {ex.Message}")),
        spectroPane.StartComputeFeed(bus, spectro, spectroCfg, () => framerate, feedInterval, ex => spectroPane.SetError($"render failed: {ex.Message}")),
        vectorPane.StartComputeFeed(bus, vec, vectorCfg, () => framerate, feedInterval, ex => vectorPane.SetError($"render failed: {ex.Message}")),
    ];

    // The single decode pump (it keeps its own feed handle for teardown). A decode failure is surfaced on all three
    // panes, since it affects them all.
    pump.Start(feedInterval, onError: ex => { foreach (var p in panes) p.SetError($"decode failed: {ex.Message}"); });

    // --- Scope-adjusting hotkeys apply to the FOCUSED pane's own GraphConfig, then Publish so that pane picks up the
    // change (the others are untouched). ---
    (Func<ConsoleKey, ConsoleKeyInfo> Build, double Magnitude)[] tiers =
    [
        (key => new ConsoleKeyInfo('\0', key, false, false, false), 1.0),
        (UI.HotKeys.Shift, 10.0),
        (UI.HotKeys.Ctrl, 5.0),
        (UI.HotKeys.Alt, 0.2),
    ];

    foreach (var (build, m) in tiers)
    {
        UI.RegisterHotKey(build(ConsoleKey.UpArrow), () => { var c = FocusedConfig(); GraphConfig.UpdateF(ref c.Scale, 0.01, m, 0.0, 10.0); c.Publish(); });
        UI.RegisterHotKey(build(ConsoleKey.DownArrow), () => { var c = FocusedConfig(); GraphConfig.UpdateF(ref c.Scale, -0.01, m, 0.0, 10.0); c.Publish(); });
        UI.RegisterHotKey(build(ConsoleKey.RightArrow), () => { var c = FocusedConfig(); GraphConfig.UpdateI(ref c.Samples, true, 25, m, 0, c.Width * 2); c.Publish(); });
        UI.RegisterHotKey(build(ConsoleKey.LeftArrow), () => { var c = FocusedConfig(); GraphConfig.UpdateI(ref c.Samples, false, 25, m, 0, c.Width * 2); c.Publish(); });
    }

    var quitting = false;
    async void Quit()
    {
        if (quitting) return;
        quitting = true;
        await pump.StopAsync();                     // stop decoding and join the in-flight read...
        pump.DisposeAudio();                         // ...only THEN is it safe to dispose the reader
        foreach (var f in paneFeeds) f.Cancel();     // panes read only immutable bus frames, so no join is needed
        UI.Stop();
    }
    // Quit on q, Ctrl+C, Ctrl+Q, Ctrl+W (scope-tui's four escape hatches).
    UI.RegisterHotKey(UI.HotKeys.Char('q'), Quit);
    UI.RegisterHotKey(UI.HotKeys.Ctrl(ConsoleKey.C), Quit);
    UI.RegisterHotKey(UI.HotKeys.Ctrl(ConsoleKey.Q), Quit);
    UI.RegisterHotKey(UI.HotKeys.Ctrl(ConsoleKey.W), Quit);

    UI.RegisterHotKey(UI.HotKeys.Char(' '), () => { var c = FocusedConfig(); c.Pause = !c.Pause; c.Publish(); });
    UI.RegisterHotKey(UI.HotKeys.Char('s'), () => { var c = FocusedConfig(); c.Scatter = !c.Scatter; c.Publish(); });
    UI.RegisterHotKey(UI.HotKeys.Char('h'), () => { var c = FocusedConfig(); c.ShowUi = !c.ShowUi; c.Publish(); });
    UI.RegisterHotKey(UI.HotKeys.Char('r'), () => { var c = FocusedConfig(); c.References = !c.References; c.Publish(); });
    UI.RegisterHotKey(UI.HotKeys.Escape, () => { var c = FocusedConfig(); c.Samples = c.Width; c.Scale = 1.0; c.Publish(); });

    // Tab / Shift+Tab move keyboard focus between the three panes (clicking a pane also focuses it).
    UI.RegisterHotKey(UI.HotKeys.Tab, () => UI.SetFocus(panes[(FocusedIndex() + 1) % panes.Length]));
    UI.RegisterHotKey(UI.HotKeys.Shift(ConsoleKey.Tab), () => UI.SetFocus(panes[(FocusedIndex() - 1 + panes.Length) % panes.Length]));

    // --- Mode-specific hotkeys route to the FOCUSED pane's mode. A mode that doesn't recognize the key returns false
    // and nothing happens; the pane's own compute feed notices its mode snapshot changed and recomputes next tick. ---
    void ModeKey(ConsoleKeyInfo key, double magnitude = 1.0) => paneModes[FocusedIndex()].HandleKey(key, magnitude);

    foreach (var (build, m) in tiers)
    {
        UI.RegisterHotKey(build(ConsoleKey.PageUp), () => ModeKey(build(ConsoleKey.PageUp), m));
        UI.RegisterHotKey(build(ConsoleKey.PageDown), () => ModeKey(build(ConsoleKey.PageDown), m));
    }
    UI.RegisterHotKey(UI.HotKeys.Char('t'), () => ModeKey(UI.HotKeys.Char('t')));
    UI.RegisterHotKey(UI.HotKeys.Char('e'), () => ModeKey(UI.HotKeys.Char('e')));
    UI.RegisterHotKey(UI.HotKeys.Char('p'), () => ModeKey(UI.HotKeys.Char('p')));
    UI.RegisterHotKey(UI.HotKeys.Char('w'), () => ModeKey(UI.HotKeys.Char('w')));
    UI.RegisterHotKey(UI.HotKeys.Char('l'), () => ModeKey(UI.HotKeys.Char('l')));
    UI.RegisterHotKey(UI.HotKeys.Char('='), () => ModeKey(UI.HotKeys.Char('=')));
    UI.RegisterHotKey(UI.HotKeys.Char('-'), () => ModeKey(UI.HotKeys.Char('-')));
    UI.RegisterHotKey(UI.HotKeys.Char('+'), () => ModeKey(UI.HotKeys.Char('+')));
    UI.RegisterHotKey(UI.HotKeys.Char('_'), () => ModeKey(UI.HotKeys.Char('_')));

    // --- Help (F1): each pane contributes its own mode-specific keys on top of ScopeView's shared keys; F1 shows the
    // focused pane's help (SetActive focuses it). ---
    oscPane.OnHelp += info =>
        info.WithKey("t", "Toggle trigger sync (freeze the waveform to a rising/falling edge crossing)")
            .WithKey("e", "Flip trigger edge polarity (rising <-> falling)")
            .WithKey("p", "Toggle peak markers on the waveform")
            .WithKey("PageUp/PageDown", "Raise/lower the trigger threshold")
            .WithKey("+ / - / = / _", "Raise/lower how many samples ahead the trigger searches for a crossing");
    spectroPane.OnHelp += info =>
        info.WithKey("w", "Toggle Hann window before the FFT")
            .WithKey("l", "Toggle log-Y (level vs raw amplitude)")
            .WithKey("PageUp/PageDown", "Raise/lower FFT frame averaging (N)");

    // Focus the top pane once the loop is running (controls are registered by then); its frame border highlights.
    UI.Post(() => UI.SetFocus(oscPane));

    var uiTask = UI.Start(outerSplit, width: 110, height: 25, fps: fps);
    await uiTask;

    // Safety net: reachable if the UI stopped via some path other than our quit handlers. Guarded by `quitting` so a
    // normal quit (which already tore these down) doesn't double-dispose.
    if (!quitting)
    {
        quitting = true;
        await pump.StopAsync();
        pump.DisposeAudio();
        foreach (var f in paneFeeds) f.Cancel();
    }

    return 0;
});

// System.CommandLine drives parsing; InvokeAsync handles --help/--version and prints parse errors, running the
// action above only on a valid parse.
return await root.Parse(args).InvokeAsync();
