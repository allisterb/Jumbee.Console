# Porting scope-tui to .NET with Jumbee.Console (AI-generated)

*A build-along on writing a real-time audio oscilloscope TUI — braille plots, off-thread audio decode,
three display modes, and a headless test suite that actually watches the pixels.*

I'd already ported one app to Jumbee.Console (an RSS reader, mostly static layout and list widgets), and I
wanted to see what happened when I pointed the library at something that actually moves: [scope-tui](https://github.com/alemidev/scope-tui),
alemidev's Rust/ratatui oscilloscope-in-your-terminal. scope-tui reads an audio stream, decodes it into
sample buffers, and redraws a waveform, a Lissajous figure, or an FFT spectrum tens of times a second — a
genuinely different kind of TUI from a feed reader. If Jumbee.Console could carry this, it could carry
most things.

This article is the how, in the order I actually built it: the shape of the app, the plotting, getting
audio off the UI thread without wedging the dispatcher, the three-mode strategy pattern, and — because a
picture doesn't prove a waveform actually reached the screen — the headless snapshot tests that do.

![oscilloscope](https://imgur.com/zanUBFx.png)

That's `ScopeView` rendering a real decoded MP3: a bold mode name and status header on top, a braille
waveform filling the rest of the terminal underneath. Let's get there.

## The shape of the app

scope-tui's UI is almost entirely one thing: a plot. A one-line header on top (mode name, trigger status,
scale, samples-per-frame, fps, scatter toggle, pause glyph), and everything else is the waveform/vectorscope/
spectrum. I built a single reusable control, `ScopeView : CompositeControl`, that owns both:

```csharp
public ScopeView(int width = 110, int plotHeight = 24)
{
    modeLabel = new TextLabel(TextLabelOrientation.Horizontal, "", Color.Red1, decoration: Spectre.Console.Decoration.Bold);
    // ...six more TextLabels for module/scale/spf/fps/scatter/pause...

    plot = new Plot();
    plot.ConfigureGrid(g => g.IsVisible = false);
    plot.ConfigureTicks(t => { t.IsVisible = false; t.Labels.IsVisible = false; });

    var headerRow = new Grid(rowHeights: [1], columnWidths: HeaderColumnWidths(width),
        controls: [[modeLabel, moduleLabel, scaleLabel, spfLabel, fpsLabel, scatterLabel, pauseLabel]]);
    dock = new DockPanel(DockedControlPlacement.Top, headerRow, plot);
    SetContent(dock);
}
```

Two things worth calling out here, because both came from a mistake I made and then had to unlearn.

**A `Grid`'s size is a fixed sum of its cells.** My first root layout was
`new Grid(rowHeights: [25], columnWidths: [110], controls: [[view]])` — which is exactly what you'd reach
for coming from a "rows and columns" mental model. It worked at 110x25 and nowhere else: on a wider real
terminal the waveform just stopped at column 110 and left a blank margin. Jumbee's own docs are explicit
about why — `Grid` has "no proportional/star sizing and no auto-fill," it's a fixed-cell layout, full stop.
The fill primitive is different: `Boundary(content, width: null, height: null)` is a single-child layout
that, with both dimensions left null, lets its child "size freely within the slot." Combine that with a
`Control`'s own `Width`/`Height` defaulting to 0 ("fills the space the parent offers"), and the fix was just
picking the right layout, not writing more code:

```csharp
var root = new Boundary(view);
// ...
var uiTask = UI.Start(root, width: 110, height: 25, fps: 24);
```

Inside `ScopeView` itself I made the same swap: the header is pinned to the top edge of a `DockPanel`, and
the `Plot` — Width/Height still 0 — gets whatever's left. Resize the terminal and the plot resizes with it,
automatically, no event handler required on my end.

**The header row's own column widths still don't reflow.** `Grid` has no settable `ColumnWidths` once
built — there's no "give me the same grid, new widths" API. My workaround: `ScopeView.Apply` checks
`ActualWidth` every tick, and if it changed since the header was built, it throws away the old 7-column
`Grid` and builds a new one at the new width, then reassigns it via `DockPanel.DockedControl` (which the
docs do call out as swappable at runtime, specifically for this — "to swap the docked pane in place"). It's
cheap (seven cells) and it works, but it's a workaround for a real gap, not a documented feature — there's
no "resize a Grid's columns" primitive, so I had to rebuild instead of reflow.

## Live plotting without rebuilding every frame

The whole reason this app exists is to prove out something an RSS reader never touches: pushing new data to
a widget many times a second without the widget's setup cost riding along every time.

My first pass did what felt natural coming from an immediate-mode background: `plot.Clear()`, then
`plot.AddSeries(xs, ys)` for the waveform, every single tick. It worked, and it was wasteful — `Plot`'s own
docs told me so once I went looking: a `Clear()` "replays all configuration on the next rebuild." Every
audio tick I was paying for the plot's *entire* setup cost (grid, ticks, axis titles) to draw a *waveform*
that mostly needed new numbers, not new configuration.

The fix is `AddLiveSeries`/`AddLiveScatter` (Braille markers by default) plus `PlotSeries.SetData`: allocate
the series handle *once*, then just feed it new numbers.

```csharp
foreach (var s in frame.Dynamic)
{
    dynamicPool.Add(s.Scatter ? plot.AddLiveScatter(s.Color) : plot.AddLiveSeries(s.Color));
    dynamicShape.Add((s.Color, s.Scatter));
}
```

That's the *rare* path — it only runs when the *shape* of what's being drawn changes: switching display
mode (Tab), toggling scatter ('s'), or a colour change. An ordinary ~20Hz audio tick, which is 95%+ of what
happens while the app is running, reduces to:

```csharp
for (var i = 0; i < frame.Dynamic.Count; i++) dynamicPool[i].SetData(frame.Dynamic[i].Xs, frame.Dynamic[i].Ys);
```

No `Clear()`, no re-`Add*`, no chrome replay — just new sample data landing in an already-configured series.
I split the frame into `References` (the static zero-line / crosshair / ~29 decade-marker gridlines, which
never change tick-to-tick) and `Dynamic` (the actual waveform/spectrum/Lissajous data), each with its own
"has this actually changed" gate, so the static geometry gets `SetData` only on a mode switch or an
axis-bounds change — never on an ordinary tick.

I proved this wasn't just a good idea in theory with test-only counters wired straight into `Apply()`:

```csharp
public int LiveShapeRebuilds { get; private set; }
public int ReferencesRefreshCount { get; private set; }
```

Ten simulated audio ticks through the real `Apply()` path, and both counters stay at 1 (the initial build)
— only a Tab (mode switch) bumps them to 2. That's the difference between "I think this doesn't rebuild"
and "I ran ten ticks and it rebuilt exactly once."

One gap I hit along the way, worth flagging honestly: for a while, `AddLiveSeries` had a scatter/marker
counterpart missing — `AddLiveScatter` didn't exist yet, so turning scatter mode on had to fall back to the
slow `Clear()`+`AddScatter()`-per-frame path. It landed later, and the fix on my side was genuinely small —
pick `AddLiveScatter` vs `AddLiveSeries` *per series*, from that series' own `Scatter` flag, since a single
frame legitimately mixes both (the oscilloscope's threshold/peak markers are always scatter, the waveform
itself follows the user's 's' toggle):

```csharp
dynamicPool.Add(s.Scatter ? plot.AddLiveScatter(s.Color) : plot.AddLiveSeries(s.Color));
```

Once that existed, the whole `Clear()`-fallback path (and its "ScatterFallbackFrames" counter) just got
deleted. There's one code path now, for every mode and every toggle.

## Axis pinning, minimal chrome, and true-RGB colour

scope-tui's look is deliberately spare — no grid, no tick labels, just two axis lines and two captions
("time -" / "| amplitude"). `Plot.ConfigureGrid`/`ConfigureTicks` turn that off (note that visibility of the
grid lines and visibility of tick *labels* are two separate flags — I set both explicitly rather than assume
one implies the other). The axis window itself is pinned per mode via `SetXRange`/`SetYRange`, computed from
each `IDisplayMode.AxisBounds` (the oscilloscope's `[0, samples] x [-scale, scale]`, the vectorscope's
symmetric `[-scale, scale]^2` square).

The chrome colour turned out to have a sharper edge than I expected. `Plot.SetAxisColor`/`SetGridColor`/
`SetTickColor`/`SetAxisTitles` all take a `Jumbee.Console.Color` directly — full RGB, not a boxed
`ConsolePlot.Drawing.Tools.LinePen` from the wrapped plotting library. I used that to give the scope a
proper dim-blue-grey axis (`new Color(70, 100, 140)`) and a muted teal-cyan caption colour
(`new Color(90, 220, 200)`) instead of settling for the 16 named console colours. I didn't just eyeball
that it looked right — I asserted the *exact* RGB round-tripped through rendering:

```csharp
var fg = ConsoleSnapshot.ForegroundAt(buffer, cx, capRow)!.Value;
Check("axis caption colour reads back EXACTLY (90,220,200) -- not snapped to the 16-colour Cyan1",
    fg == cfg.LabelsColor && fg == new Color(90, 220, 200) && fg != Color.Cyan1);
```

That passed, byte for byte — the framework isn't downsampling plot chrome to the nearest of the 16 console
colours, which mattered a lot for matching scope-tui's understated look rather than something that reads as
"basic terminal colours."

The one line in the docs I misread on the first pass: `SetAxisTitles`'s remarks say *"a null title is left
unchanged."* Hiding the UI ('h') needs an explicit empty string, not `null` — pass `null` and the last-shown
caption just sits there forever, which is exactly the bug I shipped in an earlier round before reading that
line properly.

And because these setters are documented as *"retained across `Clear()`... set it once at setup rather than
per frame,"* I stopped calling `ConfigureAxis(a => a.Pen = new LinePen(...))` every tick (a closure and an
immutable pen allocated 20 times a second for a colour that never moved) and instead set the four colour
setters once, gated behind the same "did anything actually change" check that already guarded the header
text.

## Getting audio off the UI thread

Jumbee.Console is emphatic about this in its docs: one UI thread, no locks, mutate from elsewhere via
`UI.Invoke`/`UI.Post`/`UI.InvokeAsync`. For a feed reader that's easy to respect by accident. For a scope
decoding MP3 frames 20 times a second while also fielding six kinds of hotkeys, it's the whole ballgame.

The shape I landed on is a strict split: a pure `ComputeFrame` that touches no `Control` at all, and a thin
`Apply` that only ever runs on the UI thread.

```csharp
public static ScopeFrame ComputeFrame(GraphSnapshot g, IDisplayMode mode, object? modeState,
    object? priorState, double[][] channels, int framerate)
{
    var references = g.References ? mode.References(g) : [];
    var (processed, nextState) = mode.Process(g, modeState, priorState, channels);
    // ...builds an immutable ScopeFrame record struct, no Control touched...
}

public void Apply(ScopeFrame frame) { /* Plot/TextLabel mutation only */ }
```

`ComputeFrame` is safe to run on a background thread precisely because everything it reads is a value-type
snapshot taken *before* crossing threads — `GraphSnapshot` is a `readonly record struct` copy of the mutable
`GraphConfig` fields, taken on the UI thread; each `IDisplayMode`'s own hotkey-driven knobs (trigger state,
FFT averaging count) get the same treatment via their own `Snapshot()`. Nothing on the background thread
ever reaches back into a field a hotkey handler might be mutating concurrently.

The actual off-thread pump is `Control.Feed`, the framework's own repeating-timer helper:

```csharp
public FeedHandle StartAudioFeed(Func<double[][]> produce, Action<double[][]> apply, TimeSpan interval,
    Action<Exception>? onError = null) =>
    Feed(produce, apply, interval, onError);
```

`produce` (the NAudio decode) runs on a background thread; `apply` is marshaled onto the UI thread for you.
Both legs are delivered via `UI.Post` — documented as "never blocks, never runs inline" — which turned out
to matter more than it sounds. My first attempt at this loop was hand-rolled: a `Task.Run` with
`await UI.InvokeAsync(...)` inside it, and a shutdown path that did `cts.Cancel(); await feedTask;` *after*
`UI.Stop()` had already run. `UI.InvokeAsync` is documented as returning "a task that completes when the
marshaled action finishes" — but if the dispatcher has already stopped draining its queue by the time that
call is in flight, that task *never* completes, and `await feedTask` hangs forever. `Control.Feed`'s
fire-and-forget delivery has no such promise to break.

Shutdown itself needed one more piece: `FeedHandle.StopAsync()` requests the stop and lets you *await* the
in-flight tick finishing, specifically so you don't dispose a resource the producer is still mid-read on:

```csharp
async void Quit()
{
    if (quitting) return;
    quitting = true;
    await feedHandle.StopAsync(); // waits for any in-flight audio.NextFrame() read to finish
    audio.Dispose();              // only THEN is it safe to dispose the reader it reads
    UI.Stop();
}
```

Cancelling and disposing on the same line, without awaiting `StopAsync`/`Completion` first, is exactly how
you get an `ObjectDisposedException` racing a background read — `FeedHandle`'s own docs call this out
directly ("await Completion... before disposing anything the producer touches").

And decode failures (a corrupt frame, an I/O error) needed somewhere to go instead of silently ending the
feed — the `onError` callback marshals the exception to the UI thread, where `ScopeView.SetError` turns it
into a visible red header instead of a feed that just quietly stops updating:

```csharp
var feedHandle = view.StartAudioFeed(audio.NextFrame, HandleFrame, TimeSpan.FromMilliseconds(50),
    onError: ex => view.SetError($"decode failed: {ex.Message}"));
```

## Three display modes, one interface

scope-tui's oscilloscope, vectorscope, and spectroscope aren't three apps glued together — they're three
implementations of the same shape, Tab-cycled at runtime. I mirrored that with one interface,
`IDisplayMode`, and a small array:

```csharp
IDisplayMode[] modes = [osc, vec, spectro];
var activeMode = 0;
UI.RegisterHotKey(UI.HotKeys.Tab, () => { activeMode = (activeMode + 1) % modes.Length; RequestRebuild(); });
```

Each mode owns its own per-frame transform (`Process`), its static reference geometry (`References` — the
oscilloscope's zero-line, the vectorscope's crosshair, the spectroscope's ~29 decade-marker gridlines), its
axis bounds and captions, and — the part that took a round to get right — its *own* mode-specific hotkeys,
via `HandleKey`:

```csharp
void ModeKey(ConsoleKeyInfo key, double magnitude = 1.0)
{
    if (modes[activeMode].HandleKey(key, magnitude)) RequestRebuild();
}
UI.RegisterHotKey(UI.HotKeys.Char('t'), () => ModeKey(UI.HotKeys.Char('t')));
```

Every physical key is registered globally, exactly once, through `UI.RegisterHotKey`. What happens next is
decided entirely by which mode is active — the oscilloscope's `t`/`e`/`p` (trigger toggle, edge polarity,
peak markers), the spectroscope's `w`/`l` (Hann window, log-Y), PageUp/PageDown meaning "raise the trigger
threshold" in one mode and "raise FFT averaging" in another. A mode that doesn't recognize a key just
returns `false` and nothing happens — no concrete-type checks scattered through `Program.cs`.

The spectroscope is the one mode with real cross-frame state (its N-frame FFT averaging needs the *last* N
raw sample buffers, not just this tick's), and that's threaded through explicitly rather than hidden in an
instance field: `Process` returns `(series, nextState)`, and `Program.cs` — the one place that lives across
multiple `ComputeFrame` calls — stores `nextState` back, one slot per mode, so Tab-ing away from the
spectroscope and back doesn't discard its averaging history.

```csharp
(IReadOnlyList<Series> Series, object? NextState) Process(GraphSnapshot g, object? modeState,
    object? priorState, double[][] channels);
```

Here's the vectorscope's Lissajous figure, `L`/`R` paired into `(x, y)` points and split into two half-arcs
for two-colour rendering, and its reference geometry — a plain crosshair through the origin:

```csharp
public static List<Series> References(GraphSnapshot g) =>
[
    new(null, [-g.Scale, g.Scale], [0.0, 0.0], Scatter: false, g.AxisColor),
    new(null, [0.0, 0.0], [-g.Scale, g.Scale], Scatter: false, g.AxisColor),
];
```

![vectorscope](https://imgur.com/mKkFgeA.png)

And the spectroscope — FFT magnitude spectrum, log-Y by default, decade-marker gridlines ported from the
Rust source:

![spectroscope](https://imgur.com/el25rjy.png)

The FFT deserves one honest note, because it's the closest I came to shipping a genuinely broken feature
without a test catching it. NAudio's `FastFourierTransform.FFT` normalizes its forward transform by `1/N` —
undocumented on NAudio's side, found only by comparing output magnitudes against the Rust reference's
`rustfft`, which does *not* normalize. Every downstream constant in this port (the log-Y axis bounds, the
decade-marker heights) assumed *unnormalized* magnitudes, carried straight over from the Rust source. The
result: `ln(magnitude)` for a perfectly normal tone landed comfortably below the plot's own y-axis minimum,
and the entire spectrum curve clipped silently off the bottom of the screen — invisible, every time, while
the decade-marker reference grid (unaffected by the bug) rendered just fine on top of nothing. A test that
only checked "some braille glyphs exist somewhere" passed anyway, because the reference grid *is* braille.
The fix was one multiply-back-by-N at the point the magnitude comes out of the FFT; the test that actually
catches a regression here checks the *value*, not just glyph presence — the peak `ln`-magnitude has to fall
inside the plot's own `[yMin, yMax]` window, which fails hard (and did, when I reverted the fix to confirm)
on the unnormalized version.

## Headless verification with Jumbee.Console.Snapshot

I can't drive a real terminal from this sandbox, so every claim above needed to be provable without one.
`Jumbee.Console.Snapshot` is a genuinely separate NuGet package built for exactly this — render a control
tree to an in-memory buffer and inspect it like a screen, no terminal attached.

The basic shape, reused by every check in `Tests/Program.cs`:

```csharp
var (root, view, cfg, modes, activeMode, rebuild) = Build(SineFrame(3, 5));
var text = ConsoleSnapshot.ToText(root, 110, 25);
Check("header shows mode::scope-tui", text.Contains("oscillo::scope-tui"));

var buffer = ConsoleSnapshot.Render(root, 110, 25);
var plotArea = PlotArea(buffer); // header row excluded by ROW index, not a magic character offset
Check("plot area has drawn glyphs (braille/line/axis chars)",
    plotArea.Any(c => c is '│' or '╴' or '╎' or '─' || (c >= '⠀' && c <= '⣿')));
```

Three techniques carried most of the weight:

**Reference-pixel colour assertions.** `ConsoleSnapshot.ForegroundAt`/`BackgroundAt` return a plain
`Jumbee.Console.Color?` for a given cell — no reaching into `ConsoleGUI.Data`'s internal `Cell`/`Character`
types. I used this to prove the true-RGB chrome claim above, and separately to prove a *static reference*
genuinely wasn't being touched by an ordinary tick: pick a screen coordinate only the vectorscope's
crosshair reference can ever reach (never the moving Lissajous data, by geometric construction — I picked
a circle radius that provably keeps the moving figure away from that exact cell), sample its colour across
several ticks, and assert it's byte-identical every time:

```csharp
Check("that reference pixel's colour is BYTE-IDENTICAL across ticks " +
      "(the static reference really was left alone, not corrupted by a stray SetData/rebuild)",
      fgA == cfg.AxisColor && fgB == cfg.AxisColor && fgA == fgB);
```

**Rendered-output assertions, not just internal counters.** My first pass at the no-rebuild perf check only
asserted `LiveShapeRebuilds` stayed flat — which proves the *code path* didn't fire, but not that `SetData`
actually reached the screen. The stronger version renders the *same* control twice with genuinely different
data (a 0.9-radius circle, then a 0.3-radius circle) and diffs the plot area text between them:

```csharp
Check("PRIMARY: rendered plot area actually CHANGED between the two circles " +
      "(SetData really pushed new samples to the screen, not a no-op)",
      plotAreaA != plotAreaB);
```

That's the difference between "I believe SetData works" and "I rendered two different frames and the pixels
actually differ." I keep the counters too, but as a secondary signal now, not the primary one.

**PNGs for the reviewer, not just for me.** `ConsoleSnapshot.SavePng(root, width, height, path, imageOptions)`
renders the exact same buffer to a real image — every screenshot embedded in this article came out of the
test suite itself, not a manually-run app:

```csharp
var imageOptions = new SnapshotImageOptions { FontFamily = "Cascadia Mono" };
ConsoleSnapshot.SavePng(root, 110, 25, Path.Combine(snapshotDir, "01-oscilloscope.png"), imageOptions);
```

In the screenshot below the pause glyph flips from `|>` to `||` in the header. Space toggles it and freezes
`RequestRebuild` from firing on the next audio tick — no CPU spent redrawing identical data while paused).
![paused](https://imgur.com/yLUXWFM.png)

The triggering shot shows the `T` threshold marker and the module field switching from
`live` to a trigger description once `t` is pressed — the waveform holds steady at a zero-crossing instead
of scrolling continuously.
![triggering](https://imgur.com/dS2pZKx.png)

This screenshot is the resize proof: the *same* `ScopeView` instance, rendered once at 110 columns and
once at 160, with the waveform's braille reaching close to the right edge at both — proving the `Boundary`
fill actually fills, rather than staying capped at its construction-time width.
![wide resize](https://imgur.com/lLUOVo1.png))

And scatter mode on, rendered through the `AddLiveScatter`-backed no-rebuild path,
real marker glyphs on screen (not just a rebuild counter saying "I switched shape"):

![scatter mode](https://imgur.com/dPoQAOC.png)


The full run, 119 checks (a handful of hotkey/geometry/FFT-correctness unit tests alongside the render
assertions above):

```
PASS - PRIMARY: rendered plot area actually CHANGED between the 0.9-radius and 0.3-radius circle...
PASS - PRIMARY: that reference pixel's colour is BYTE-IDENTICAL across ticks...
PASS - PRIMARY (true-colour proof): axis caption colour reads back EXACTLY (90,220,200)...
PASS - FFT peak (~1000Hz) is near the true tone frequency (1000Hz)
...
ALL PASS
```

One honest gap in the harness, recorded rather than hidden: I could not get the `onError` callback to fire
inside this headless test runner — there's no running `UI.Start` loop draining `UI.Post` deliveries the way
a live app has, so an exception injected into `produce` never reaches `SetError` in the test process. The
live-app wiring is correct by the documented contract (I read the exact text on `Feed<T>`'s `onError`
parameter and matched it), but I couldn't close that particular loop end-to-end without a real running
dispatcher — a genuine boundary of what the headless harness can currently prove, not a bug in the app.

## Hotkeys and the overall feel

Every hotkey in the app, mode-agnostic and mode-specific, goes through `UI.RegisterHotKey`, including
Shift/Ctrl/Alt tiers for coarse/fine adjustment on the arrow keys (scale, samples-per-frame):

```csharp
(Func<ConsoleKey, ConsoleKeyInfo> Build, double Magnitude)[] tiers =
[
    (key => new ConsoleKeyInfo('\0', key, false, false, false), 1.0),
    (UI.HotKeys.Shift, 10.0),
    (UI.HotKeys.Ctrl, 5.0),
    (UI.HotKeys.Alt, 0.2),
];
foreach (var (build, m) in tiers)
    UI.RegisterHotKey(build(ConsoleKey.UpArrow), () => { GraphConfig.UpdateF(ref cfg.Scale, 0.01, m, 0.0, 10.0); RequestRebuild(); });
```

Quit mirrors the original's four escape hatches — `q`, Ctrl+C, Ctrl+Q, Ctrl+W — all routed to the same
async `Quit()` that awaits the feed's teardown before calling `UI.Stop()`. F1 opens the framework's built-in
help overlay; `ScopeView.GetHelpInfo()` supplies the mode-agnostic keys, and `Control.OnHelp` is the
documented extension point `Program.cs` uses to append whichever mode's keys are *currently* active:

```csharp
view.OnHelp += info =>
{
    if (modes[activeMode] == osc)
        info.WithKey("t", "Toggle trigger sync (freeze the waveform to a rising/falling edge crossing)")
            .WithKey("PageUp/PageDown", "Raise/lower the trigger threshold");
    // ...
};
```

## What this taught me about Jumbee.Console for real-time, graphics-heavy TUIs

The plotting side held up well under real pressure: `AddLiveSeries`/`SetData` is the right lever for
streaming data and the docs actively steer you to it (the `Clear()`-cost warning is right there on `Plot`'s
own remarks, not something I had to discover the hard way). The colour setters taking full RGB directly,
and being explicitly documented as retained-across-`Clear()`, meant true-colour chrome was just "call the
setter once," not a fight. The threading story — value-snapshot in, pure compute, `Control.Feed` /
`FeedHandle.StopAsync` for teardown — is opinionated in a way that, once I stopped fighting it with a
hand-rolled `Task.Run`/`await UI.InvokeAsync` loop, made a background audio decoder genuinely simple to get
right, including clean shutdown.

The two rough edges were both small and both fixable by reading further: a `Grid`'s fixed-cell sizing
looks like it should reflow and doesn't (reach for `Boundary` + Width/Height 0 instead, and expect to
rebuild-not-resize a `Grid` whose column widths need to track a resize), and a live series' scatter/marker
mode wasn't available on day one, so the fast no-rebuild path and the "cheap markers for dense data" path
briefly didn't compose. Neither one blocked the app — they cost a workaround and, eventually, a clean fix —
but both are exactly the kind of thing you only find by building something that actually streams data at a
plot instead of drawing it once and leaving it alone.

If you're building something in this category — anything that redraws continuously off a live data source
— the two habits worth adopting early are: keep your per-frame transform a pure function of value-type
snapshots so it can run off the UI thread with nothing to race, and reach for the *Live*/`SetData` variant
of whatever widget you're feeding before you reach for `Clear()` — the second one will work, but the first
one is what makes 20Hz feel free.

