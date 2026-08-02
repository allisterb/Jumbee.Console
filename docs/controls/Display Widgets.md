# Display Widgets

Small, self-contained readouts and status indicators — you place them in a layout and update them from your code.
Most take no focus or input; `Log` is the exception, since it scrolls. All live in the `Jumbee.Console` namespace.

| Control | Shows | Size |
|---------|-------|------|
| `Sparkline` | a series of numbers as inline block bars | one cell per value, 1 row |
| `Digits` | text in large seven-segment glyphs (clocks, counters) | 3 cells wide per char, 3 rows |
| `Gauge` | one proportion as a filled meter | 1 row |
| `ProgressBar` | one task's progress, with optional spinner and timing | 1 row |
| `Spinner` | indeterminate activity, animated | 1 row |
| `Badge` | a short status pill on a filled background | text + padding, 1 row |
| `Footer` | a key-hints bar | full width, 1 row |
| `Log` | an append-only tail of styled/renderable entries | fills its cell |
| `PerfHud` | frame timings and allocation, while developing | floats over the app |

They are appearance-themed where it makes sense (bar/text styles come from the active
[theme](../../GETTING-STARTED.md#7-styling-and-theming) and can be overridden per instance).

**Picking between the progress-ish ones**, since there are four and they overlap:

| You want | Use |
|---|---|
| A proportion — capacity, disk used, percent of a budget | `Gauge` |
| One task advancing toward completion | `ProgressBar` |
| Activity with no known duration | `Spinner`, or `ProgressBar.IsIndeterminate` |
| Several concurrent tasks in Spectre's column layout | [`SpectreTaskProgress`](Spectre%20Interop.md) |

> **Looking for a bigger chart?** `Sparkline` is one row of block bars — deliberately small. For anything taller
> or denser ([Charts](Charts.md) covers all of these in full):
>
> | You want | Use |
> |---|---|
> | A one-row inline trend next to a label | `Sparkline` (below) |
> | Axes, ticks, a legend, multiple series | [`Plot`](../api/Jumbee.Console.Plot.md) |
> | A **filled/area chart at sub-cell resolution** — the dense braille look of `htop`/`vtop`-style monitors | [`Canvas`](../api/Jumbee.Console.Canvas.md) with `CanvasMarker.Braille` (the default) and one [`Drawing.FilledLine`](../api/Jumbee.Console.Drawing.FilledLine.md) per column |
>
> The last one isn't obvious from the type names: `Canvas` reads as a general drawing surface, but a filled
> braille column chart is a few lines on top of it, and `Plot`'s bar methods can't produce that look (they take no
> braille brush). See [Live Data](Live%20Data.md) for wiring any of them to a running data source.

```csharp
// A filled braille column chart — one FilledLine per sample, baseline to value.
using Jumbee.Console.Drawing;

var chart = new Canvas().WithYBounds(0, 100).WithXBounds(0, samples.Length - 1);
for (var x = 0; x < samples.Length; x++)
    chart.Add(new FilledLine(x, 0, x, samples[x], fillToY: 0, color));
```

> **If a braille chart comes out as empty boxes in a PNG**, it's the snapshot font, not your chart — the image
> renderer defaults to Consolas, which has no braille glyphs. The terminal and `ConsoleSnapshot.ToText` are
> unaffected, which is why this only shows up in saved images. Pass
> `new SnapshotImageOptions { FontFamily = "Cascadia Mono" }` (or `"DejaVu Sans Mono"`) to `SavePng`/`ToImage`.

## `Sparkline`

Draws a list of values as block bars (`▁▂▃▄▅▆▇█`), one cell per value, scaled against the series maximum.

```csharp
var spark = new Sparkline(3, 5, 2, 8, 6, 7, 4) { BarStyle = Style.Cyan1 };

// Update the series later (the control re-sizes to the new count):
spark.Values = [.. latestSamples];

// Pin the top of the scale so bars don't re-normalise every update:
spark.Max = 100;
```

The default bar glyphs are the eighth-block elements `▁▂▃▄▅▆▇█`, which need a font with block-element coverage
(Windows Terminal / Cascadia Mono render them fine). A **legacy console** — `cmd.exe` with a raster or Lucida
Console font — has the full block `█` and box-drawing characters but **not** the partial blocks, so they show as
missing-glyph boxes. For those terminals switch to the ASCII ramp:

```csharp
var spark = new Sparkline(samples) { Bars = Sparkline.AsciiBars };   // ".:-=+*#@"
// or supply your own ramp, ordered shortest -> tallest:
spark.Bars = " ▄█";
```

The same applies to PNG snapshots, which rasterise with their own font — see
[`SnapshotImageOptions.FontFamily`](../api/Jumbee.Console.Snapshot.SnapshotImageOptions.md) for choosing one with
the coverage your glyphs need.

## `Digits`

Renders text in large three-row glyphs — handy for clocks and counters. Supported characters are `0-9` and
`. , : - + space`; anything else renders blank.

```csharp
var clock = new Digits(DateTime.Now.ToString("HH:mm:ss")) { DigitStyle = Style.Green1 };
// later, from any thread:
clock.Text = DateTime.Now.ToString("HH:mm:ss");
```

The control is `text.Length * 3` cells wide and 3 rows tall. Its glyphs are plain `_`/`|`, so it renders in any
font.

## `Log`

An append-only "tail" view: it always shows the **most recent** entries that fit in its height, like a live
console. Entries are Spectre renderables, so log lines can be coloured/styled (pass a markup string, or any
`IRenderable`). `Write` is safe to call from any thread (it marshals onto the UI thread for you).

```csharp
var log = new Log { MaxEntries = 500 };
log.Write("[green]OK[/]   server started");        // a markup string …
log.Write("[yellow]WARN[/] disk almost full");
log.Write(new Spectre.Console.Rule("section"));    // … or any Spectre IRenderable
```

Place it in a fixed-size layout cell (e.g. a `Grid` row) so the visible window matches the cell height; the
newest lines stay pinned to the bottom. The log is viewport-virtualized and owns its own scrolling — the mouse
wheel scrolls it, and when focused so do Up/Down/PageUp/PageDown/Home/End; it draws its own scrollbar. Writing
while scrolled up leaves the view put (new lines accumulate below); scrolling back to the bottom (or `End`)
re-engages tailing.

`Log` is covered in more depth, alongside the other row-oriented controls, in
[Lists and Data](Lists%20and%20Data.md#log).

## `Gauge`

A single-row meter: the track fills in proportion to `Value` / `Max`, optionally followed by the percentage and
the raw value — `████████░░░░  34.5% (126)`.

```csharp
var disk = new Gauge(126, max: 512) { Label = "disk", ShowPercent = true, ShowValue = true };
disk.WithFill(Color.Green);

disk.Value = used;    // setting it redraws
```

The bar is a solid colour band with an eighth-block sub-cell edge, so it animates smoothly and stays seam-free in
any font. Use it for capacity and progress-through-a-budget readouts — year/day progress, a deployment percentage,
disk used.

For a task advancing to completion, `ProgressBar` is the better fit: it carries a description, timing and a
spinner.

## `ProgressBar`

One row of task progress, modelled on a row of Spectre's `Progress` — but a plain composable control you place,
theme and drive yourself.

```csharp
var bar = new ProgressBar("downloading", value: 0, max: 100)
{
    ShowPercentage = true,
    ShowSpinner = true,
    TimeDisplay = ProgressTimeDisplay.Elapsed,   // or .Remaining, or .None
};
bar.WithGradient(Color.Blue, Color.Cyan1);
bar.Start();

// as work advances:
bar.Value = percentComplete;
```

`Start()` begins the spinner and any animation; `Stop()` freezes both. `IsIndeterminate` switches to a pulse when
you don't know the total. `Description` labels it, and `TimeDisplay` takes a `ProgressTimeDisplay` —
`None`, `Elapsed` or `Remaining` — rather than a bool.

The bar is a smooth sub-cell band by default; `WithGlyphs` switches it to a per-cell glyph fill (hatch, segments,
ASCII) for fonts or terminals where that reads better. `WithFill`, `WithGradient` and `WithPadding` cover the rest
of the appearance.

## `Spinner`

Indeterminate activity, animated on its own timer.

```csharp
var spinner = new Spinner { SpinnerType = Spectre.Console.Spinner.Known.Dots, Text = "thinking" };
spinner.Start();
// …
spinner.Stop();
```

`SpinnerType` selects the animation and `Text` puts a label beside it. It's an `AnimatedControl`, so it drives its
own frames — you don't tick it.

`ChatPrompt` has one built into its gutter, driven by `Busy`; see [Text and Input](Text%20and%20Input.md).

## `Badge`

A small inline status pill — short text on a filled background with a little horizontal padding.

```csharp
var status = new Badge("LIVE", BadgeVariant.Success);
status.Text = "STALE";
status.Variant = BadgeVariant.Warning;
```

`BadgeVariant` picks a themed colour scheme; pass an explicit `Style` instead when you want an exact colour.
`Padding` adjusts the breathing room. Non-interactive, and fixed-width — it sizes to its text.

## `Footer`

A one-row key-hints bar, filling the available width — `^j Send  ^t Method  ^c Quit  f1 Help`.

```csharp
var footer = new Footer(
    new FooterHint("^s", "Save"),
    new FooterHint("^q", "Quit"),
    new FooterHint("f1", "Help"));

footer.SetHints(ContextualHints());   // swap the whole set as context changes
```

The key chord is drawn in an accent style and the label in normal text (`KeyStyle` / `LabelStyle`, `Gap` for the
spacing). Non-interactive: the hints are yours to keep in step with your actual hotkeys — nothing verifies that
they match.

Dock it to the bottom of the shell. For the fuller, on-demand version, use the F1 help system described in
[Navigation](Navigation.md).

## `GlassPanel`

A translucent panel that floats over the app — a HUD, a tooltip, a heads-up readout that shouldn't fully hide
what's behind it.

```csharp
var hud = new GlassPanel(width: 30, height: 8, tint: Color.Black, factor: 0.6f);
hud.Content = someControl;
hud.Show(x: 4, y: 2);       // floats in UI.Overlay; Hide() and Toggle() do the obvious
```

`tint` is the glass colour the layer beneath is blended toward, and `factor` the blend strength — `0` is fully
see-through, `1` a solid fill. `frosted` (on by default) collapses each cell beneath to a single perceived colour
so content behind becomes a faithful blur; turn it off and the glyphs underneath show through, tinted.
`gammaCorrect` blends in linear light.

A terminal cell has no alpha channel — ANSI has no way to express one — so this is a software flatten of the
overlapping layers to opaque colours, done lazily as cells are drawn. Only the panel's own cells are blended, and
only when a frame redraws them.

`PerfHud` below is a `GlassPanel` subclass, which is what it looks like in practice.

## `PerfHud`

A development overlay showing frame timings, allocation rate and lock contention, floating over the app.

```csharp
var hud = new PerfHud();
hud.ShowTopRight();
hud.RegisterToggle(UI.HotKeys.CtrlF12);    // toggle it with a key
```

Counters are read directly from `Process`/`GC`/`Monitor` and differenced across refreshes, so nothing has to be
sampling for it to work, and it refreshes itself a few times a second while shown.

> **The `locks` counter measures contention, not correctness.** It's cumulative, not a rate. The dangerous
> threading bug — unsynchronized writes from a background thread — produces **zero** contention and still corrupts
> your state. Read it to confirm you haven't introduced locking, never to prove your threading is right. See
> [Live Data](Live%20Data.md).

## Putting it together

```csharp
var clock = new Digits("00:00:00");
var spark = new Sparkline(1, 2, 3, 4, 5);
var log   = new Log();

var grid = new Grid(
    rowHeights:    [3, 1, 10],
    columnWidths:  [54],
    controls:
    [
        [clock],
        [spark],
        [log],
    ]);

var run = UI.Start(grid, width: 58, height: 16);
run.Wait();
```

See `WidgetGalleryDemo` in the TestDemo project for a live version that ticks the clock, shifts the sparkline,
and appends log lines from a background loop.
