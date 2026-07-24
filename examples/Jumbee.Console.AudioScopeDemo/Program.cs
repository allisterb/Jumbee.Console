using Jumbee.Console;
using ScopeTui;

// --- scope-tui as THREE simultaneous panes, all fed from ONE audio source ---------------------------------------
// Milestone: the earlier version showed one scope and Tab-cycled the mode. This shows all three at once -- an
// oscilloscope across the top, a spectroscope (bottom-left) and a vectorscope (bottom-right) below -- each its own
// ScopeView control, each driven by its OWN Control.Feed, all reading the SAME decoded audio from one ChannelBus.
// AudioSource is single-threaded (one reader, one stream position), so exactly ONE pump decodes and publishes; the
// three panes fan out from the bus, computing their (different) transforms off the UI thread in parallel. This is a
// deliberate stress test of Control.Feed: four concurrent feeds, three consumers marshalling onto the one UI thread.
//
// CLI: [mp3path] [--fps N] [--interval MS] [--sample-rate HZ]. Two independent clocks drive the scope: the FEED
// period (how often the source is sampled and the waveforms recompute) and the UI paint FPS cap. `--interval MS`
// sets the feed period directly; `--sample-rate HZ` sets it as 1000/HZ; otherwise `--fps N` (when given) tightens
// it to 1000/N so the DATA refreshes N times/sec, not just repaints; the default is 50ms (20Hz). All are distinct
// from NAudio's 44.1kHz PCM rate, which only sets how fast the waveform scrolls.
string? mp3Path = null;
int fps = 24;
bool fpsSet = false;
int? sampleRate = null;   // feed period as 1000/rate ms
int? intervalMs = null;   // explicit feed period in ms (wins over the others)
for (var i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--fps" when i + 1 < args.Length && int.TryParse(args[i + 1], out var f):
            fps = Math.Clamp(f, 1, 240);
            fpsSet = true;
            i++;
            break;
        case "--interval" when i + 1 < args.Length && int.TryParse(args[i + 1], out var ms):
            intervalMs = Math.Clamp(ms, 1, 1000);
            i++;
            break;
        case "--sample-rate" when i + 1 < args.Length && int.TryParse(args[i + 1], out var sr):
            sampleRate = Math.Clamp(sr, 1, 1000);
            i++;
            break;
        default:
            mp3Path ??= args[i]; // first non-flag positional argument is the mp3 path
            break;
    }
}
mp3Path ??= @"C:\Projects\Jumbee.Console\reference\media\02 - Girlfriend.mp3";
// Feed period: an explicit --interval wins, else --sample-rate (1000/rate), else --fps (1000/fps) when set, else 50ms.
var feedMs = intervalMs
    ?? (sampleRate is { } rate ? Math.Max(1, (int)Math.Round(1000.0 / rate))
        : fpsSet ? Math.Max(1, (int)Math.Round(1000.0 / fps)) : 50);
var feedInterval = TimeSpan.FromMilliseconds(feedMs);

const int bufferSamples = 2048;
// Fixed calibration gain on the decoded samples (see GraphConfig.Gain / Oscilloscope.Process): NAudio's floats are
// normalized to [-1,1], so a typical passage only fills a small slice; this makes the default view fill the axis the
// way scope-tui's raw-sample-space plot does, without touching the interactive Scale knob.
const double AmplitudeGain = 5.0;
var audio = new AudioSource(mp3Path, bufferSamples);

var cfg = new GraphConfig
{
    Samples = bufferSamples,
    Width = bufferSamples,
    Gain = AmplitudeGain,
    SampleRate = audio.SampleRate,
};
cfg.Publish(); // refresh the published snapshot now the object-initializer has set the real field values

// The single fan-out point and the single decoder that fills it (see ChannelBus / AudioPump).
var bus = new ChannelBus();
var pump = new AudioPump(audio, bus, cfg);

// One mode instance and one ScopeView per pane -- each pane is FIXED to its mode (no Tab-cycling a single view any
// more). Osc/spectro own their hotkey knobs (trigger, FFT window/averaging); vector has none.
var osc = new Oscilloscope();
var spectro = new Spectroscope(audio.SampleRate, bufferSamples);
var vec = new Vectorscope();

var oscPane = new ScopeView();
var spectroPane = new ScopeView();
var vectorPane = new ScopeView();

// Panes and their modes in Tab-focus order (osc -> spectro -> vector). `focusedPane` indexes this for routing the
// mode-specific hotkeys to whichever pane is active.
ScopeView[] panes = [oscPane, spectroPane, vectorPane];
IDisplayMode[] paneModes = [osc, spectro, vec];
var focusedPane = 0;

// Each pane gets a border frame -- both to separate the three panes and to host the active-pane cue: the focused
// pane's border is bright, the others dim. The first pane starts active.
var activeBorder = new Color(120, 210, 255);
var dimBorder = new Color(70, 80, 100);
var oscFrame = new ControlFrame(oscPane, borderStyle: BorderStyle.Rounded, borderFgColor: activeBorder);
var spectroFrame = new ControlFrame(spectroPane, borderStyle: BorderStyle.Rounded, borderFgColor: dimBorder);
var vectorFrame = new ControlFrame(vectorPane, borderStyle: BorderStyle.Rounded, borderFgColor: dimBorder);
ControlFrame[] frames = [oscFrame, spectroFrame, vectorFrame];

// Layout: oscilloscope full-width on top; spectroscope (wide, left) + vectorscope (square, right) share the row
// below. Both split positions are recomputed on resize (below) so the top stays ~half and the vector stays ~square.
var bottomSplit = new SplitPanel(SplitOrientation.Horizontal, spectroPane, vectorPane, splitPosition: 70);
var outerSplit = new SplitPanel(SplitOrientation.Vertical, oscPane, bottomSplit, splitPosition: 12);
var root = outerSplit;

void SetActive(int i)
{
    focusedPane = i;
    for (var j = 0; j < frames.Length; j++) frames[j].BorderFgColor = j == i ? activeBorder : dimBorder;
    UI.SetFocus(panes[i]); // best-effort: routes the F1 help overlay to the active pane
}

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

    // Only reproportion on an ACTUAL terminal resize -- not every frame -- so a manual divider drag survives between
    // resizes (setting SplitPosition every frame would fight the drag and snap it back).
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

// --- The fan-out: each pane runs its OWN self-driving compute feed off the one shared bus. -----------------------
FeedHandle[] paneFeeds =
[
    oscPane.StartComputeFeed(bus, osc, cfg, () => framerate, feedInterval, ex => oscPane.SetError($"render failed: {ex.Message}")),
    spectroPane.StartComputeFeed(bus, spectro, cfg, () => framerate, feedInterval, ex => spectroPane.SetError($"render failed: {ex.Message}")),
    vectorPane.StartComputeFeed(bus, vec, cfg, () => framerate, feedInterval, ex => vectorPane.SetError($"render failed: {ex.Message}")),
];

// The single decode pump (it keeps its own feed handle for teardown). A decode failure is surfaced on all three
// panes, since it affects them all.
pump.Start(feedInterval, onError: ex => { foreach (var p in panes) p.SetError($"decode failed: {ex.Message}"); });

// --- Shared hotkeys: apply to ALL panes via the one GraphConfig, then Publish so the panes pick up the change. ---
void ConfigChanged() => cfg.Publish();

(Func<ConsoleKey, ConsoleKeyInfo> Build, double Magnitude)[] tiers =
[
    (key => new ConsoleKeyInfo('\0', key, false, false, false), 1.0),
    (UI.HotKeys.Shift, 10.0),
    (UI.HotKeys.Ctrl, 5.0),
    (UI.HotKeys.Alt, 0.2),
];

foreach (var (build, m) in tiers)
{
    UI.RegisterHotKey(build(ConsoleKey.UpArrow), () => { GraphConfig.UpdateF(ref cfg.Scale, 0.01, m, 0.0, 10.0); ConfigChanged(); });
    UI.RegisterHotKey(build(ConsoleKey.DownArrow), () => { GraphConfig.UpdateF(ref cfg.Scale, -0.01, m, 0.0, 10.0); ConfigChanged(); });
    UI.RegisterHotKey(build(ConsoleKey.RightArrow), () => { GraphConfig.UpdateI(ref cfg.Samples, true, 25, m, 0, cfg.Width * 2); ConfigChanged(); });
    UI.RegisterHotKey(build(ConsoleKey.LeftArrow), () => { GraphConfig.UpdateI(ref cfg.Samples, false, 25, m, 0, cfg.Width * 2); ConfigChanged(); });
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

UI.RegisterHotKey(UI.HotKeys.Char(' '), () => { cfg.Pause = !cfg.Pause; ConfigChanged(); });
UI.RegisterHotKey(UI.HotKeys.Char('s'), () => { cfg.Scatter = !cfg.Scatter; ConfigChanged(); });
UI.RegisterHotKey(UI.HotKeys.Char('h'), () => { cfg.ShowUi = !cfg.ShowUi; ConfigChanged(); });
UI.RegisterHotKey(UI.HotKeys.Char('r'), () => { cfg.References = !cfg.References; ConfigChanged(); });
UI.RegisterHotKey(UI.HotKeys.Escape, () => { cfg.Samples = cfg.Width; cfg.Scale = 1.0; ConfigChanged(); });

// Tab / Shift+Tab move focus between the three panes (mode-specific keys route to the focused one).
UI.RegisterHotKey(UI.HotKeys.Tab, () => SetActive((focusedPane + 1) % panes.Length));
UI.RegisterHotKey(UI.HotKeys.Shift(ConsoleKey.Tab), () => SetActive((focusedPane - 1 + panes.Length) % panes.Length));

// --- Mode-specific hotkeys route to the FOCUSED pane's mode. A mode that doesn't recognize the key returns false
// and nothing happens; the pane's own compute feed notices its mode snapshot changed and recomputes next tick. ---
void ModeKey(ConsoleKeyInfo key, double magnitude = 1.0) => paneModes[focusedPane].HandleKey(key, magnitude);

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

// Prime the active-pane cue + focus on the UI thread once the loop is running (controls are registered by then).
UI.Post(() => SetActive(0));

var uiTask = UI.Start(root, width: 110, height: 25, fps: fps);
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
