using System.CommandLine;

using Jumbee.Console;
using ScopeTui;

// --- Jumbee.Console AudioScope  demo ---------------------------------------
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
// CLI (System.CommandLine): file|live [--path FILE] [--fps N] [--buffer N] [--overlap F] [--scatter] [--device D]
// [--loopback] [--mono] [--tick N] [--scheme NAME] [--list-devices].
//
// The three scopes sit under a shared one-line ScopeStatusBar carrying the values that belong to the RUN -- input
// device, channels, overlap, paint rate. Each pane's own header carries only what its hotkeys can change.
//
// Two independent clocks drive the scope, and only one of them is an option:
//
//   The FEED period -- how often the source is sampled and the waveforms recompute -- is DERIVED, not set directly:
//   it is the wall-clock duration of the new audio in each frame, i.e. --buffer / the PCM rate, scaled down by
//   --overlap. At the default (no overlap) every frame carries one whole buffer of fresh audio, which is the only
//   rate that neither re-shows nor drops anything. --overlap deliberately re-uses part of each frame to refresh
//   faster without shrinking the FFT -- see the feed-period note below.
//
//   --fps is purely the UI paint/input loop cap. It does NOT affect what data is produced -- painting more often
//   than the data changes costs almost nothing (the loop skips compositing when nothing is dirty), and the loop is
//   also where input is drained, so a healthy rate keeps the app responsive no matter how big --buffer is.
//
// --buffer sets three things at once: the oscilloscope window, the FFT size (and so the frequency-bin resolution),
// and -- via the derived feed -- the refresh rate. A bigger buffer buys resolution and costs refresh; --overlap is
// how you buy the refresh back.

var inputArg = new Argument<string?>("input")
{
    Arity = ArgumentArity.ExactlyOne,
    Description = "Selects the audio input: 'file' decodes the mp3 argument; 'live' captures the default recording device.",
}
.AcceptOnlyFromAmong("file", "live"); ;
var pathOpt = new Option<string?>("--path")
{
    Arity = ArgumentArity.ZeroOrOne,
    Description = "Path to an MP3 file to decode. Defaults to the bundled sample track.",
}.AcceptLegalFilePathsOnly();
var fpsOpt = new Option<int>("--fps")
{
    Description = "UI paint-rate cap in frames/sec. Purely the paint/input loop -- the data rate follows --buffer.",
    DefaultValueFactory = _ => 60,
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
var overlapOpt = new Option<double>("--overlap")
{
    Description = "Fraction of each frame the next one re-uses (0-0.95). Refreshes faster than one buffer at a time, at no cost in FFT size.",
    // Defaulted ON. Measured over 10s wall at the default buffer: 17% of a core at 0, 23% at 0.75, and 23% at 0.95 --
    // the paint loop dominates and the extra feed work barely registers, so the smoother display is nearly free.
    // 0.75 rather than higher because it is the conventional analyser value, every paint still gets a fresh frame
    // (86 feeds/sec against a 60fps cap), and it stays clear of the MinFeedMs floor that makes ~0.95 quietly
    // deliver ~0.91.
    DefaultValueFactory = _ => 0.75,
};
var tickOpt = new Option<int>("--tick")
{
    Description = "Samples between the oscilloscope's vertical grid lines (0 removes its grid/labels). Spectroscope ticks are fixed frequencies.",
    DefaultValueFactory = _ => 200,
};
var monoOpt = new Option<bool>("--mono")
{
    Description = "Scope a single channel: opens a 1-channel stream where the backend allows it, else averages the device's channels.",
};
var schemeOpt = new Option<string>("--scheme")
{
    Description = $"Colour scheme for the scope panes: {string.Join(", ", ScopeTheme.Names)}.",
    DefaultValueFactory = _ => ScopeTheme.ScopeTui.Name,
}.AcceptOnlyFromAmong(ScopeTheme.Names);
var listDevicesOpt = new Option<bool>("--list-devices")
{
    Description = "Print the live capture endpoints --device can select (honours --loopback) and exit.",
};

var root = new RootCommand("AudioScope -- view an oscilloscope, spectroscope, and vectorscope from audio input.")
{
    inputArg, pathOpt, fpsOpt, bufferOpt, scatterOpt, deviceOpt, loopbackOpt, monoOpt,
    overlapOpt, tickOpt, schemeOpt, listDevicesOpt,
};

root.SetAction(async (parse, ct) =>
{
    var input = parse.GetValue(inputArg)!;
    var filePath = parse.GetValue(pathOpt) ?? @"C:\Projects\Jumbee.Console\reference\media\02 - Girlfriend.mp3";
    var fps = Math.Clamp(parse.GetValue(fpsOpt), 1, 240);
    var bufferSamples = Math.Clamp(parse.GetValue(bufferOpt), 64, 1 << 16);
    var startScatter = parse.GetValue(scatterOpt);
    var deviceSpec = parse.GetValue(deviceOpt);
    var loopback = parse.GetValue(loopbackOpt);
    var mono = parse.GetValue(monoOpt);
    // In SAMPLES, so it is bounded by the window rather than by the pane: one gridline per sample is meaningless,
    // and a step wider than the window would draw none at all. XTickBuilders thins further if the pane is too narrow
    // to draw them all.
    var tick = Math.Clamp(parse.GetValue(tickOpt), 0, bufferSamples);
    // Capped below 1: at 1.0 a frame would be entirely re-used audio, i.e. an infinitely fast feed showing nothing new.
    var overlap = Math.Clamp(parse.GetValue(overlapOpt), 0.0, 0.95);
    // --scheme is a THEME: applying it here, before any control is built, means the panes' plots pick up their
    // axis/grid/tick/surface colours and series palette from it on construction, and each pane's frame picks up its
    // at-rest border. Nothing downstream takes a colour argument. (UI.SetTheme also raises ThemeChanged, so already-
    // built controls would re-capture too -- the ordering here is for clarity, not correctness.)
    var scheme = ScopeTheme.FromName(parse.GetValue(schemeOpt));
    UI.SetTheme(scheme, UI.GlyphTheme);
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

    // Fixed calibration gain on the decoded samples (see GraphConfig.Gain / Oscilloscope.Process): NAudio's floats
    // are normalized to [-1,1], so a typical passage only fills a small slice; this makes the default view fill the
    // axis the way scope-tui's raw-sample-space plot does, without touching the interactive Scale knob.
    const double AmplitudeGain = 5.0;
    IAudioSource audio;
    try
    {
        audio = live
            // A live device already hands out a rolling latest-window, so overlapping is purely a matter of sampling
            // it more often -- nothing to configure on the source. A file has to retain the previous frame's tail.
            ? new RecordingAudioSource(bufferSamples, RecordingAudioSource.ResolveDevice(deviceSpec, loopback), loopback, mono)
            : new FileAudioSource(filePath, bufferSamples, overlap);
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Could not open audio input '{input}': {ex.Message}");
        return 1;
    }

    // The feed period is the wall-clock duration of the NEW audio in each frame: a whole buffer by default, or the
    // non-overlapping part of one when --overlap is given. Without overlap this is the only rate that is neither
    // wasteful nor lossy -- a window can be fully replaced exactly sampleRate/buffer times a second (~23Hz for 2048
    // @ 48kHz), so a faster feed could only re-show samples already on screen and a slower one would drop audio
    // outright. It is also the cadence scope-tui gets for free by blocking on its capture queue.
    //
    // --overlap buys refresh rate back at no cost in resolution, which is the trade a spectrum analyser makes: at
    // --buffer 8192 the bins are 5.9Hz wide but a full window only arrives 5.9 times a second, and --overlap 0.75
    // puts the display back at ~23Hz by re-using three quarters of each frame. Re-showing samples is the POINT
    // there, rather than the accident it would be at a mismatched fixed rate.
    //
    // The cadence matches; contiguity is best-effort. A live device hands us a rolling latest-window rather than a
    // queue, so timer jitter can re-show or skip a few samples at the seam -- scope-tui cannot, since it consumes
    // every buffer in order (and drifts behind realtime instead, if the UI stalls).
    //
    // The 4ms floor (250 ticks/sec) guards the small-buffer end: --buffer 64 at 48kHz is a 1.3ms window, which would
    // otherwise run the pump ~750 times a second and fan out to three pane computes on each. Below that floor the
    // feed no longer keeps up with the audio, so windows have gaps -- acceptable, since 64 samples is a degenerate
    // display anyway, and the alternative is a self-inflicted busy loop.
    const int MinFeedMs = 4;
    TimeSpan FeedIntervalFor(double ov) => TimeSpan.FromMilliseconds(
        Math.Max(MinFeedMs, (int)Math.Round(1000.0 * bufferSamples * (1.0 - ov) / audio.SampleRate)));
    var feedInterval = FeedIntervalFor(overlap);

    // What the feed ACHIEVES, which the floor above (and the ms rounding) can hold below what was asked for -- at the
    // default buffer, --overlap 0.95 really runs at 0.91. Report the achieved value so the readout never overstates.
    double AchievedOverlap(double ov) =>
        Math.Max(0.0, 1.0 - (FeedIntervalFor(ov).TotalMilliseconds * audio.SampleRate / 1000.0 / bufferSamples));

    // The floor also caps how much overlap is REACHABLE: once the tick is at MinFeedMs it cannot get shorter, so every
    // higher request lands on the same feed and changes nothing. Clamp to that ceiling rather than accepting values
    // that do nothing -- at --buffer 384 / 48kHz one window is 8ms, so the ceiling is 0.50 and without this the
    // readout sat at 50 while [ ] silently rebuilt all four feeds on every press. Small buffers are already fast
    // enough that they need little overlap: 384 samples refreshes 125 times a second before any is applied.
    var maxOverlap = Math.Clamp(1.0 - (MinFeedMs * audio.SampleRate / 1000.0 / bufferSamples), 0.0, 0.95);
    overlap = Math.Min(overlap, maxOverlap);
    feedInterval = FeedIntervalFor(overlap);

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
    // A Series carries its own colour (it is built off the UI thread, from a GraphSnapshot), so the display modes
    // need the trace colours as DATA as well as the Plot needing them as theme. Source both from the one theme
    // rather than keeping a second copy in the config's initializers.
    // Applying a scheme has TWO halves, and both are needed because the colours are consumed two different ways.
    // UI.SetTheme covers everything a control reads for itself -- each plot's axis/grid/tick/surface and series
    // palette, each pane frame's at-rest border, the header strip's background -- via the ThemeChanged re-capture.
    // But a Series carries its OWN colour (it is built off the UI thread from a GraphSnapshot), so the trace colours
    // must also be pushed into the configs as data and republished. Used at startup and by the Ctrl+T cycle below.
    var activeScheme = scheme;
    void ApplyScheme(ScopeTheme theme)
    {
        activeScheme = theme;
        UI.SetTheme(theme, UI.GlyphTheme);
        foreach (var c in configs)
        {
            c.Palette = [.. Enumerable.Range(0, theme.Palette.Count).Select(i => theme.Palette[i])];
            c.LabelsColor = theme.LabelsColor;
            c.AxisColor = theme.AxisColor;
            c.Publish();
        }
    }

    ApplyScheme(scheme);   // also publishes, now the object initializers have set the real field values

    // One mode instance and one ScopeView per pane -- each pane is FIXED to its mode. Osc/spectro own their hotkey
    // knobs (trigger, FFT window/averaging); vector has none.
    var osc = new Oscilloscope();
    var spectro = new Spectroscope(audio.SampleRate, bufferSamples);
    var vec = new Vectorscope();

    // Header source field -- what this run is actually scoping. 'loop' rather than 'live' when tapping playback,
    // since the endpoint name alone ("Speakers") wouldn't say which direction it is being read in. The device name is
    // the FRIENDLY name, not RecordingAudioSource.DeviceId, which on Windows is an unreadable endpoint GUID. 16 is
    // what the 5-char prefix leaves of the source column's 22 cells on a 100-cell header (see headerPercents);
    // "Stereo Mix (Realtek(R) Audio)" and the like really are that long, and a column overrun collides with the
    // next field rather than being cut off.
    static string Trim(string s, int max) => s.Length <= max ? s : string.Concat(s.AsSpan(0, max - 2), "..");
    // Generous limits now that this sits on its own full-width row rather than in a ninth of a pane header:
    // "Microphone Array (Realtek(R) Audio)" is 35 and "Speakers (Steam Streaming Speakers)" 35, so 44 shows the
    // real endpoint names whole and only trims the pathological ones.
    var sourceText = audio is RecordingAudioSource rec
        ? $"device:{(loopback ? "loop" : "live")}/{Trim(rec.DeviceName, 44)}"
        : $"file:{Trim(Path.GetFileName(filePath), 44)}";

    // Each pane gets the tick scheme its x axis actually calls for, which is why --tick governs only the first:
    //   oscillo  x = sample index      -> a gridline every --tick samples, labelled with the sample number
    //   spectro  x = ln(frequency)     -> fixed frequencies, labelled in Hz ("100", "1k") instead of the raw ln
    //                                     values the axis would otherwise show ("4.6", "6.9")
    //   vector   x = amplitude         -> the library's spacing heuristic; neither scheme means anything here
    // Passing --tick to the spectroscope would be nonsense: 200 ln-units spans the whole axis several times over.
    var oscPane = new ScopeView(xTickStep: tick, source: sourceText, channels: audio.Channels,
        xTicks: tick > 0 ? XTickBuilders.Every(tick) : null, tickInfo: tick > 0 ? $"tick:{tick}" : "tick:off");
    var spectroPane = new ScopeView(source: sourceText, channels: audio.Channels,
        xTicks: XTickBuilders.Frequencies());
    var vectorPane = new ScopeView(source: sourceText, channels: audio.Channels);

    // Panes and their modes/configs in Tab-focus order (osc -> spectro -> vector), index-aligned.
    ScopeView[] panes = [oscPane, spectroPane, vectorPane];
    IDisplayMode[] paneModes = [osc, spectro, vec];

    // Each pane gets a border frame -- to separate the panes and to carry the focus cue: a frame recolours its border
    // to the theme's focused colour (BorderFocusedText) while its control is focused, and the at-rest colour
    // (BorderText) otherwise, automatically. So the focused scope's border stands out with zero manual bookkeeping.
    //
    // The theme supplies the at-rest colour through BorderText; ControlFrame resolves the FOCUSED colour first
    // (`(focusCueVisible ? focusedBorderFgColor : null) ?? borderFgColor`) and ScopeTheme deliberately leaves
    // BorderFocusedText at the default, so recolouring a scheme's borders never swallows the focus cue.
    foreach (var pane in panes) _ = new ControlFrame(pane, borderStyle: BorderStyle.Rounded);

    // Layout: oscilloscope full-width on top; spectroscope (wide, left) + vectorscope (square, right) share the row
    // below. Both split positions are recomputed on resize (below) so the top stays ~half and the vector stays ~square.
    var bottomSplit = new SplitPanel(SplitOrientation.Horizontal, spectroPane, vectorPane, splitPosition: 70);
    var outerSplit = new SplitPanel(SplitOrientation.Vertical, oscPane, bottomSplit, splitPosition: 12);

    // One shared line above the scopes for the run-wide values (device, channels, overlap, fps). Docked Top so the
    // splits below keep the rest -- see the remarks on ScopeStatusBar for why these left the pane headers.
    var statusBar = new ScopeStatusBar(sourceText, audio.Channels);
    var rootLayout = new DockPanel(DockedControlPlacement.Bottom, statusBar, outerSplit);

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
        if ((now - lastPoll).TotalSeconds >= 1) { framerate = paintCount; paintCount = 0; lastPoll = now; statusBar.Framerate = framerate; }

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
    // Factored into a function because [ / ] retune the feed at runtime, and a FeedHandle's interval is fixed when it
    // is created -- there is no way to re-time a running feed, so changing the rate means stopping and re-starting all
    // four of them. The pump is started LAST so no decode is in flight while the source is being reconfigured.
    var paneFeeds = new FeedHandle[panes.Length];
    void StartFeeds(TimeSpan interval)
    {
        for (var i = 0; i < panes.Length; i++)
        {
            var pane = panes[i];   // captured per iteration, so each error handler targets its own pane
            paneFeeds[i] = pane.StartComputeFeed(bus, paneModes[i], configs[i], () => framerate, interval,
                ex => pane.SetError($"render failed: {ex.Message}"));
        }

        // The single decode pump. A decode failure is surfaced on all three panes, since it affects them all.
        pump.Start(interval, onError: ex => { foreach (var p in panes) p.SetError($"decode failed: {ex.Message}"); });
    }

    StartFeeds(feedInterval);
    // Prime the header readout with the startup --overlap. Without this only the [ / ] handler ever told the panes,
    // so `--overlap 0.75` ran at the right rate but reported nothing.
    statusBar.Overlap = AchievedOverlap(overlap);

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

    // Ctrl+T cycles the colour scheme across ALL THREE panes at once -- unlike the other hotkeys, which act on the
    // focused pane. It is a theme switch, and a theme is global by definition.
    UI.RegisterHotKey(UI.HotKeys.Ctrl(ConsoleKey.T), () => ApplyScheme(activeScheme.Next()));

    // [ and ] adjust the feed overlap. Deliberately NOT pane-routed and deliberately not PageUp/PageDown: overlap
    // belongs to the one audio source every pane reads, so it cannot be per-pane the way scale/samples/trigger/
    // averaging are, and PageUp/PageDown already mean the trigger threshold on the oscilloscope and the FFT
    // averaging depth on the spectroscope. Dedicated global keys keep all three meanings distinct.
    var retuning = false;
    async void SetOverlap(double next)
    {
        next = Math.Clamp(next, 0.0, maxOverlap);
        // Re-entrancy guard: each change tears down and rebuilds four feeds asynchronously, so a held key would
        // otherwise interleave two rebuilds and leak a feed.
        if (retuning || quitting || Math.Abs(next - overlap) < 1e-9) return;
        retuning = true;
        try
        {
            overlap = next;
            await pump.StopAsync();                      // join the in-flight decode BEFORE touching the source...
            foreach (var f in paneFeeds) f.Cancel();     // ...panes read only immutable bus frames, so no join needed
            audio.SetOverlap(overlap);                   // ...and only then is it safe to reconfigure
            if (quitting) return;                        // quit may have landed while we were awaiting
            StartFeeds(FeedIntervalFor(overlap));
            statusBar.Overlap = AchievedOverlap(overlap);
        }
        finally { retuning = false; }
    }

    // Step until the FEED actually differs, rather than by a fixed 0.05. The feed period is a whole number of
    // milliseconds, so a short window quantises coarsely -- at --buffer 384 (an 8.7ms window) there are only about
    // nine distinct periods, and a 0.05 step lands on the same one two presses running, making the key look dead.
    // Skipping to the next distinct period means every press changes something at any buffer size.
    void StepOverlap(int direction)
    {
        var currentMs = FeedIntervalFor(overlap).TotalMilliseconds;
        var next = overlap;
        for (var i = 0; i < 40; i++)   // bounded: 0..maxOverlap can hold at most 20 steps of 0.05 either way
        {
            next = Math.Clamp(next + (direction * 0.05), 0.0, maxOverlap);
            if (FeedIntervalFor(next).TotalMilliseconds != currentMs) break;
            if (next <= 0.0 || next >= maxOverlap) break;   // at an end: nothing further to reach
        }
        SetOverlap(next);
    }

    UI.RegisterHotKey(UI.HotKeys.Char(']'), () => StepOverlap(+1));
    UI.RegisterHotKey(UI.HotKeys.Char('['), () => StepOverlap(-1));

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

    var uiTask = UI.Start(rootLayout, width: 110, height: 25, fps: fps);
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
