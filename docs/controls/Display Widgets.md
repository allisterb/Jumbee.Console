# Display Widgets

Widgets for presenting information — you place them in a layout and update them from your code. `Sparkline` and
`Digits` take no focus or input; `Log` is the exception — it scrolls (mouse wheel, or the arrow/Page/Home/End
keys when focused). All live in the `Jumbee.Console` namespace.

| Control | Shows | Size |
|---------|-------|------|
| `Sparkline` | a series of numbers as inline block bars | one cell per value, 1 row |
| `Digits` | text in large seven-segment glyphs (clocks, counters) | 3 cells wide per char, 3 rows |
| `Log` | an append-only tail of styled/renderable entries | fills its cell |

They are appearance-themed where it makes sense (bar/text styles come from the active
[theme](../internal/Theming.md) and can be overridden per instance).

> **Looking for a bigger chart?** `Sparkline` is one row of block bars — deliberately small. For anything taller
> or denser:
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

See [Snapshot Testing](../internal/Snapshot%20Testing.md) for the per-font glyph-coverage details.

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
