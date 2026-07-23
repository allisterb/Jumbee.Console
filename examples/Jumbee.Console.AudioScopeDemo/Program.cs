using Jumbee.Console;
using ScopeTui;

var mp3Path = args.Length > 0 ? args[0] : @"C:\Projects\Jumbee.Console\reference\media\02 - Girlfriend.mp3";

const int bufferSamples = 2048;

// --- Round-6 (item 2): AudioSource decodes via NAudio's AudioFileReader, whose samples are IEEE-float normalized
// to [-1,1] -- so a typical music passage (rarely hitting full scale) only occupies a small slice of that range.
// On a real terminal this made the DEFAULT (unzoomed) view look ~5x smaller than the original's screenshots; the
// naive fix -- shrinking GraphConfig.Scale's default instead -- would shrink the Y-AXIS BOUNDS too, so any sample
// that still spikes past the new narrow bounds overshoots by 5x and every such line segment spans the full plot
// height, which is the exact dense-render slowdown the round noted. So the fix is a fixed CALIBRATION gain
// applied to the decoded samples themselves (not the interactive Scale knob), leaving the default axis bounds
// at the original +/-1.0 (no overshoot regime) while making a typical passage actually fill them the way the
// original's raw-sample-space plot does.
//
// Round-8 (item 3): this gain used to be applied unconditionally to the SHARED `channels` buffer in HandleFrame
// below, before EVERY mode read it -- so the vectorscope's Lissajous figure clipped against its +/-1 square and
// the spectroscope's level sat 5x too high. Round-10 (item 3) moves it onto `cfg.Gain`, which rides the shared
// GraphSnapshot every mode receives -- only Oscilloscope.Process reads it, so the shared `channels` buffer stays
// raw and Vectorscope/Spectroscope are unaffected, with no concrete-type check anywhere in ComputeFrame.
const double AmplitudeGain = 5.0;
var audio = new AudioSource(mp3Path, bufferSamples);

var cfg = new GraphConfig
{
    Samples = bufferSamples,
    Width = bufferSamples,
    Gain = AmplitudeGain,
    SampleRate = audio.SampleRate,
};


// --- Round-4: three display modes, Tab cycles between them. All three implement IDisplayMode; `modes[activeMode]`
// is the single source of truth for "which mode is on screen right now". Round-6 adds Spectroscope. ---
var osc = new Oscilloscope();
var vec = new Vectorscope();
var spectro = new Spectroscope(audio.SampleRate, bufferSamples);
IDisplayMode[] modes = [osc, vec, spectro];
var activeMode = 0;

// Round-6 (item a, state OUT): one cross-frame accumulator slot per mode, so Tab-ing away from the spectroscope
// and back doesn't discard its FFT averaging history. Only Program.cs (the one place threads outlive a single
// ComputeFrame call) touches this array, and only ever on the UI thread (see RebuildNow/RequestRebuild below).
var modeAccumulators = new object?[modes.Length];

var view = new ScopeView(width: 110, plotHeight: 24);

// --- Round-5 (item 4): Grid.md documents Grid sizing as FIXED-cell only ("no proportional/star sizing and no
// auto-fill"), so round-4's `Grid(rowHeights:[25], columnWidths:[110], controls:[[view]])` root could never be
// more than 110x25 cells -- on a wider real terminal the waveform only filled part of the width. Boundary.md
// documents `Boundary(content, width: null, height: null)` as a single-child layout that, with both dimensions
// left null, lets its child "size freely within the slot" -- i.e. a pure pass-through fill wrapper, which is all
// the root needs since `view`'s own Width/Height default to 0 ("fills the space the parent offers", Control.md).
// UI.Start reads the real terminal size on an ANSI terminal and the framework re-lays-out on resize automatically
// (UI.md/Control.md), so this is the whole fix: pick a layout that fills instead of one that sums fixed cells.
var root = new Boundary(view);

double[][] channels = [[], []];

// --- Round-5 (item 2): FPS must reflect the framework's REAL per-frame paint cadence, not our own feed-loop
// iteration count. UI.Paint ("Raised each frame so subscribed controls render their state", UI.md) is the
// documented per-frame hook; subscribing here counts actual paints and rolls them up into a 1-second rate,
// independent of whether a key was ever pressed.
var framerate = 0;
var paintCount = 0;
var lastPoll = DateTime.UtcNow;
UI.Paint += (_, _) =>
{
    paintCount++;
    var now = DateTime.UtcNow;
    if ((now - lastPoll).TotalSeconds >= 1)
    {
        framerate = paintCount;
        paintCount = 0;
        lastPoll = now;
    }
};

// --- ONE snapshot path for EVERYTHING the heavy per-frame transform reads, now also capturing which mode index
// is active and that mode's prior cross-frame accumulator (round-6), all read together on the UI thread only. ---
(GraphSnapshot g, IDisplayMode mode, int modeIndex, object? modeState, object? priorState, double[][] channels, int framerate) TakeSnapshot() =>
    (cfg.Snapshot(), modes[activeMode], activeMode, modes[activeMode].Snapshot(), modeAccumulators[activeMode], channels, framerate);

void RebuildNow()
{
    var (g, mode, idx, modeState, prior, ch, fr) = TakeSnapshot();
    var frame = ScopeView.ComputeFrame(g, mode, modeState, prior, ch, fr);
    view.Apply(frame);
    modeAccumulators[idx] = frame.NextModeState; // state OUT, stored back on the UI thread
}

// --- Round-7 (accumulator single-flight fix): RequestRebuild fires on EVERY ~20Hz audio tick AND every hotkey,
// so without a guard two calls can be in flight at once -- both TakeSnapshot() the same modeAccumulators[idx] as
// `prior` before either has stored its result back, so the second call's computed NextModeState is built from a
// prior that's already one frame stale by the time it lands; whichever UI.Invoke callback runs LAST simply
// overwrites the other's store, so the FFT averaging history silently skips a frame. `rebuildInFlight` makes
// rebuilds single-flight PER MODE: a tick that arrives while the previous one for this mode is still computing
// is dropped rather than racing it. Nothing is lost, only delayed -- TakeSnapshot() re-reads `cfg`/`channels`/the
// mode's live fields fresh on the NEXT tick (~50ms later), so a dropped rebuild just means one fewer render this
// tick, not a missed input; the accumulator chain itself can never see two writers for the same prior again.
var rebuildInFlight = new bool[modes.Length];

void RequestRebuild()
{
    var (g, mode, idx, modeState, prior, ch, fr) = TakeSnapshot();
    if (rebuildInFlight[idx]) return; // a rebuild for this mode is already computing off-thread; let it finish
    rebuildInFlight[idx] = true;
    Task.Run(() =>
    {
        var frame = ScopeView.ComputeFrame(g, mode, modeState, prior, ch, fr);
        UI.Invoke(() =>
        {
            view.Apply(frame);
            modeAccumulators[idx] = frame.NextModeState; // state OUT, marshaled onto the UI thread with the rest of Apply
            rebuildInFlight[idx] = false;
        });
    });
}

// --- Round-6 (item 1): the round-5 quit fix, completed. Round-5's Feed<T> already fixed the QUIT HANG (tick/apply
// delivered via fire-and-forget UI.Post, so nothing awaits a promise a stopped dispatcher would never fulfill),
// but it introduced two NEW gaps the docs now give a documented fix for:
//
//  (a) a thrown decode exception (corrupt MP3 frame, I/O error) used to just end the feed silently -- nothing
//      observed the failure. Control.md's Feed<T> overload now takes an `onError` callback: "the exception is
//      marshaled to it on the UI thread ... without onError the throw silently ends the feed." Wiring one in
//      below surfaces it as visible state via ScopeView.SetError instead of a dead, unexplained UI.
//
//  (b) Quit() used to Cancel() the feed and IMMEDIATELY Dispose() the NAudio reader on the same call -- but
//      Cancel() only REQUESTS the stop; the feed's background thread could still be mid-`audio.NextFrame()`
//      read, racing the dispose (ObjectDisposedException). FeedHandle.md documents exactly this: "await
//      Completion (or StopAsync) ... before disposing anything the producer touches, so the resource is never
//      torn down under a live tick." Quit is `async void` (the standard C# pattern for a fire-and-forget event
//      handler) so it can await StopAsync() before the dispose.
//
// Round-8 (item 1, skip-while-paused): RequestRebuild() used to be called UNCONDITIONALLY on every audio tick
// even while paused, just to redraw identical data -- moved inside the `!cfg.Pause` branch so a paused scope does
// zero work on the ~20Hz feed cadence (Space/arrow-key hotkeys still call RequestRebuild() directly when the user
// actually changes something while paused).
void HandleFrame(double[][] frame)
{
    if (!cfg.Pause)
    {
        channels = frame;
        RequestRebuild();
    }
}

var feedHandle = view.StartAudioFeed(audio.NextFrame, HandleFrame, TimeSpan.FromMilliseconds(50),
    onError: ex => view.SetError($"decode failed: {ex.Message}"));

// --- Mode-agnostic global hotkeys: scale, samples, pause, scatter, quit, Tab. ---
(Func<ConsoleKey, ConsoleKeyInfo> Build, double Magnitude)[] tiers =
[
    (key => new ConsoleKeyInfo('\0', key, false, false, false), 1.0),
    (UI.HotKeys.Shift, 10.0),
    (UI.HotKeys.Ctrl, 5.0),
    (UI.HotKeys.Alt, 0.2),
];

foreach (var (build, m) in tiers)
{
    UI.RegisterHotKey(build(ConsoleKey.UpArrow), () => { GraphConfig.UpdateF(ref cfg.Scale, 0.01, m, 0.0, 10.0); RequestRebuild(); });
    UI.RegisterHotKey(build(ConsoleKey.DownArrow), () => { GraphConfig.UpdateF(ref cfg.Scale, -0.01, m, 0.0, 10.0); RequestRebuild(); });
    UI.RegisterHotKey(build(ConsoleKey.RightArrow), () => { GraphConfig.UpdateI(ref cfg.Samples, true, 25, m, 0, cfg.Width * 2); RequestRebuild(); });
    UI.RegisterHotKey(build(ConsoleKey.LeftArrow), () => { GraphConfig.UpdateI(ref cfg.Samples, false, 25, m, 0, cfg.Width * 2); RequestRebuild(); });
}

var quitting = false;
async void Quit()
{
    if (quitting) return;
    quitting = true;
    await feedHandle.StopAsync(); // waits for any in-flight audio.NextFrame() read to finish
    audio.Dispose();              // ...only THEN is it safe to dispose the reader it reads
    UI.Stop();
}
// Round-6 (item 4): the original quits on q, Ctrl+C, Ctrl+Q, AND Ctrl+W (app.rs: "mimic other programs shortcuts
// to quit, for user friendlyness") -- round-5 only had q/Ctrl+C.
UI.RegisterHotKey(UI.HotKeys.Char('q'), Quit);
UI.RegisterHotKey(UI.HotKeys.Ctrl(ConsoleKey.C), Quit);
UI.RegisterHotKey(UI.HotKeys.Ctrl(ConsoleKey.Q), Quit);
UI.RegisterHotKey(UI.HotKeys.Ctrl(ConsoleKey.W), Quit);
UI.RegisterHotKey(UI.HotKeys.Char(' '), () => { cfg.Pause = !cfg.Pause; RequestRebuild(); });
UI.RegisterHotKey(UI.HotKeys.Char('s'), () => { cfg.Scatter = !cfg.Scatter; RequestRebuild(); });
UI.RegisterHotKey(UI.HotKeys.Char('h'), () => { cfg.ShowUi = !cfg.ShowUi; RequestRebuild(); });
UI.RegisterHotKey(UI.HotKeys.Char('r'), () => { cfg.References = !cfg.References; RequestRebuild(); });
UI.RegisterHotKey(UI.HotKeys.Escape, () => { cfg.Samples = cfg.Width; cfg.Scale = 1.0; RequestRebuild(); });

// Tab cycles display modes.
UI.RegisterHotKey(UI.HotKeys.Tab, () => { activeMode = (activeMode + 1) % modes.Length; RequestRebuild(); });

// --- Round-6 (item b): mode-specific keys are now routed through IDisplayMode.HandleKey instead of concrete-type
// checks. Every physical key is registered EXACTLY ONCE (same as before) and unconditionally forwards to
// `modes[activeMode].HandleKey(...)`; a mode that doesn't recognize the key returns false and nothing happens --
// no more `if (modes[activeMode] != osc) return;` anywhere in this file. This is what let Spectroscope's w/l/
// PageUp/PageDown slot in without touching a single existing hotkey registration's body.
void ModeKey(ConsoleKeyInfo key, double magnitude = 1.0)
{
    if (modes[activeMode].HandleKey(key, magnitude)) RequestRebuild();
}

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

// --- Help screen. UI.ShowHelp() (bound to F1 by default, UI.md) opens a built-in modal compiled from every
// registered control's Control.GetHelpInfo -- ScopeView.GetHelpInfo supplies the mode-agnostic keys.
// Control.OnHelp is the documented extension point for appending the CURRENTLY-ACTIVE mode's keys, since only
// Program.cs (not the reusable ScopeView control) knows which mode is active. Text authorship still switches on
// concrete mode type here -- that's fine, it's describing content, not routing input (see IDisplayMode.HandleKey
// for the actual input-routing seam).
view.OnHelp += info =>
{
    if (modes[activeMode] == osc)
    {
        info.WithKey("t", "Toggle trigger sync (freeze the waveform to a rising/falling edge crossing)")
            .WithKey("e", "Flip trigger edge polarity (rising <-> falling)")
            .WithKey("p", "Toggle peak markers on the waveform")
            .WithKey("PageUp/PageDown", "Raise/lower the trigger threshold")
            .WithKey("+ / - / = / _", "Raise/lower how many samples ahead the trigger searches for a crossing");
    }
    else if (modes[activeMode] == spectro)
    {
        info.WithKey("w", "Toggle Hann window before the FFT")
            .WithKey("l", "Toggle log-Y (level vs raw amplitude)")
            .WithKey("PageUp/PageDown", "Raise/lower FFT frame averaging (N)");
    }
    else
    {
        info.Text += "\n\n[grey](Vectorscope has no mode-specific keys.)[/]";
    }
};

RebuildNow();
// --- Round-8 (item 1): fps dropped from 60 to 24 -- UI.Start's fps doc says "a frame is skipped when nothing is
// dirty, so a higher fps only costs CPU while the UI is actually changing", but our own feed already drives every
// visible change (RequestRebuild) at the audio tick's ~20Hz cadence (50ms interval); there is nothing this app
// ever invalidates faster than that. 60fps bought no extra smoothness (data itself only refreshes 20x/sec) but
// still had the UI thread waking 3x more often than necessary to check for dirty regions. 24fps sits just above
// the 20Hz data rate (comfortable headroom for a tick landing slightly early) instead of a free-running clock.
var uiTask = UI.Start(root, width: 110, height: 25, fps: 24);
await uiTask;

// Safety net: reachable even if the UI stopped via a path other than our own quit handlers. Guarded by `quitting`
// so a normal `q`/Ctrl+C/Ctrl+Q/Ctrl+W quit (which already awaited StopAsync + disposed above) doesn't try to
// tear the same resources down twice.
if (!quitting)
{
    quitting = true;
    await feedHandle.StopAsync();
    audio.Dispose();
}
