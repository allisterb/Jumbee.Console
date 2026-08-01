# Charts

`Plot`, `Canvas`, `BarChart`, `RunChart` and `Globe` — plus [`Sparkline`](Display%20Widgets.md#sparkline), which
lives with the small widgets. Choosing between them is most of the work, because two of them overlap in a way that
isn't obvious from their names.

## Choosing

| I want | Use |
|---|---|
| Axes, ticks, several series, a real chart | `Plot` |
| A streaming chart with a legend and current/delta/max/min | `RunChart` |
| A one-row inline trend | [`Sparkline`](Display%20Widgets.md#sparkline) |
| **A dense filled/area chart — the braille look of a system monitor** | **`Canvas` with one `Drawing.FilledLine` per column** |
| Labelled categories with value text | `BarChart` |
| Arbitrary drawing, or geography | `Canvas`, plus `Drawing.WorldMap` |
| A rotating earth | `Globe` |

Two pairs get confused:

**`Plot` versus `BarChart`.** `Plot` has `AddBars`, so both draw bars. `Plot`'s bars are numeric — plotted at x/y
positions against axes, for a histogram or a distribution. `BarChart`'s are categorical — a label, a value and a
colour per bar, with the value printed alongside. If your bars have names rather than coordinates, you want
`BarChart`.

**`Plot` versus `Canvas` for a filled chart.** This one costs people an afternoon, so it's stated plainly:

> A dense, sub-cell **filled area chart** — the braille texture that system monitors use — is a `Canvas` with one
> `Drawing.FilledLine` per column, **not** a `Plot`. `Canvas` reads as a general drawing surface so people don't
> think to look there, and `Plot`'s bar methods take no braille brush, so there's no chart control to switch on.

## `Plot`

A full chart: axes, grid, ticks, labels and a dozen series types, rendered into the control's buffer. It fills its
container and re-draws to fit on resize, replaying all configuration — so your styling survives a resize.

```csharp
var plot = new Plot();
plot.AddSeries(xs, ys, PlotBrush.Braille, Color.Cyan1);
plot.SetAxisTitles("time (s)", "load");
plot.SetYRange(0, 100);                       // pin the range, or idle and saturated look identical
plot.ConfigureGrid(g => g.IsVisible = false);
```

`PlotBrush` picks the plotting glyph: `Braille` (default, finest), `Quadrant`, `Block`, `Dot`, `Star`.

**Series types**, all `Add*` methods: `AddSeries` (line), `AddScatter`, `AddBars` / `AddHBars` / `AddGroupedBars` /
`AddStackedBars`, `AddHistogram`, `AddStem`, `AddCandles`, `AddErrorBars`, `AddBox` / `AddBoxes`, `AddHeatmap`,
`AddConfusionMatrix`, and `AddLabel` for annotations.

**Ranges and ticks.** `SetXRange` / `SetYRange` pin the axes; `AutoRangeX` / `AutoRangeY` fit them to the data;
`SetXWindow` keeps a moving window. `SetXTicks` / `SetYTicks` take explicit `(value, label)` pairs when you want
dates or categories rather than numbers.

**Pin your Y range for anything measured as a percentage.** With auto-ranging, a series that never exceeds 34 %
draws at full height, so an idle machine and a saturated one look identical.

### Live data

Don't rebuild the plot each tick. Add a live series once and push into the handle it returns:

```csharp
var series = plot.AddLiveSeries(Color.Green, PlotBrush.Braille);
plot.SetXWindow(60);                          // keep the last 60 units on screen

// later, on the UI thread:
series.Push(t, value);
```

The live path mutates the data in place without re-allocating the plot, and it keeps your `Configure*` styling —
which `Clear()` would drop along with the series. `PlotSeries` also has `SetData`, `SetValues`, `Scroll` and
`Clear`. `AddLiveScatter` and `AddLiveBars` are the equivalents for the other shapes.

See [Live Data](Live%20Data.md) for getting values from a sampler thread onto the UI thread safely.

## `Canvas`

A drawing surface in data coordinates. **The origin is the bottom-left corner**, and `XBounds` / `YBounds` define
the coordinate space you draw in — so you work in your data's units, not cells.

```csharp
using Jumbee.Console.Drawing;                 // the shapes live in their own namespace

var canvas = new Canvas()
    .WithXBounds(0, 100)
    .WithYBounds(0, 100)
    .WithMarker(CanvasMarker.Braille);        // Braille is the default and the finest

canvas.Add(new Circle(50, 50, 20, Color.Yellow));
canvas.Print(10, 90, "label", Color.Grey);
```

Shapes live in `Jumbee.Console.Drawing`: `Line`, `FilledLine`, `Circle`, `Rectangle`, `Points` and `WorldMap`.
`Add` retains a shape, `Clear()` drops them all, `ClearLabels()` drops just the printed text. `Layer(marker)` starts
a new layer with its own marker, so you can put a coarse `Block` backdrop behind a fine `Braille` foreground.

Set `Interactive` to allow drag-to-pan and wheel-zoom.

### The filled area chart

The system-monitor look, in full — one `FilledLine` per column, filled down to the baseline:

```csharp
using Jumbee.Console.Drawing;

var canvas = new Canvas().WithXBounds(0, history.Length).WithYBounds(0, 100);
canvas.Clear();
for (var i = 0; i < history.Length; i++)
    canvas.Add(new FilledLine(i, history[i], i, history[i], fillToY: 0, Color.Green));
```

`FilledLine(x1, y1, x2, y2, fillToY, color)` draws the segment and fills between it and `fillToY`. A degenerate
segment — the same point twice, as above — gives you a single filled column, which at braille resolution is two
lit dot-columns per character cell.

> **Braille and PNG snapshots:** rasterising braille needs a font with glyphs at U+2800–U+28FF, and the default
> (Consolas) has none. Fallbacks are applied per glyph now, but if your captures come out as empty boxes, set
> `SnapshotImageOptions.FontFamily` to `"Cascadia Mono"`. Live rendering and `ToText` are never affected.

`DamageTracking` opts into partial redraw — worthwhile when a small region of a large surface changes, and
measurably *not* worthwhile when the whole picture changes every frame.

## `RunChart`

A streaming multi-series time chart with a legend — a `Plot` on the left, and a per-series readout (name, current,
delta, max, min) on the right. Pure composition over `Plot`: it adds a legend, not a new rendering path.

```csharp
var chart = new RunChart();
var cpu = chart.AddSeries("cpu", Color.Cyan1);
var mem = chart.AddSeries("mem", Color.Magenta1);
chart.SetXWindow(120);
chart.SetYRange(0, 100);

// on the UI thread, per tick:
cpu.Push(cpuPercent);
mem.Push(memPercent);
```

`AddSeries` returns a `RunSeries` whose only member is `Push(double)` — you feed values, and the data flows through
a stationary X window so the frame stays put and the values scroll through it. `ValueFormat` controls how the
readout formats numbers; `AutoRangeY` if a fixed range doesn't suit.

Reach for this over a bare `Plot` whenever you'd otherwise hand-build a legend beside a chart.

## `BarChart`

Labelled categories with their values printed alongside.

```csharp
var chart = new BarChart(ChartOrientation.Horizontal,
    ("nginx",  42.0, Color.Green),
    ("redis",  17.5, Color.Yellow),
    ("worker",  9.1, Color.Red));

chart.ShowValues = true;
chart.ValueFormatter = (v, culture) => $"{v.ToString("F1", culture)}%";   // Func<double, CultureInfo, string>
```

`Orientation` switches between horizontal and vertical, `MaxValue` pins the scale (otherwise it fits the largest
bar), and `Label` / `LabelAlignment` / `CenterLabel` position the chart title. `AddItem` / `AddItems` /
`RemoveItem` maintain the bars, and the `this[string[]]` indexer updates several at once.

The bar glyphs are configurable — `AsciiBar`, `HorizontalUnicodeBar`, `VerticalUnicodeBar` — for terminals or fonts
where the default blocks don't render cleanly.

## `Globe`

A ray-traced earth in coloured half-blocks: each ray that hits the sphere maps to a lat/long and is coloured from an
ocean-depth → land-elevation → polar-ice ramp. With `DisplayNight` it's shaded by a fixed light, so the day/night
terminator sweeps across as it turns.

```csharp
var globe = new Globe { DisplayNight = true, Interactive = true };

// Spin it from your own loop, marshaling each step onto the UI thread.
_ = Task.Run(async () =>
{
    using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(50));
    while (await timer.WaitForNextTickAsync(UI.CancellationToken))
        UI.Invoke(() => globe.RotationAngle += 0.02);
});
```

Spin it by advancing `RotationAngle`. (`Control.Feed` is `protected` — it's for controls driving their own tick,
not for app code. See [Live Data](Live%20Data.md).) `Interactive` enables drag-to-rotate and wheel-zoom. The land map is
generated at runtime from public-domain Natural Earth polygons.

## See also

- [Display Widgets](Display%20Widgets.md) — `Sparkline`, `Gauge` and the other small readouts.
- [Live Data](Live%20Data.md) — sampling off the UI thread, cadence, and keeping the frame path cheap.
- [Layouts](Layouts.md) — charts fill their container, so the layout decides how big they get.
