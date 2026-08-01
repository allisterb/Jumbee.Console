# Spectre.Console Interop

Jumbee renders through Spectre.Console, so Spectre widgets and styling work here — but the ownership model is
inverted. This page covers bringing existing Spectre code in.

## The model, briefly

Spectre.Console is **immediate mode**: you call `AnsiConsole.Write(renderable)` and it goes to the terminal now.
Jumbee is **retained mode**: controls persist and the framework repaints them. The bridge is `AnsiConsoleBuffer`,
an `IAnsiConsole` implementation that captures Spectre's output into a cell buffer instead of writing it out — so a
Spectre widget renders into a control rather than onto the screen.

See [Writing Applications](Writing%20Applications.md) if that distinction is new.

## Choosing

| I have | Use |
|---|---|
| A Spectre `IRenderable` (a `Table`, `Panel`, `Rows`, custom widget) | `SpectreControl<T>` |
| Spectre's `LiveDisplay` | `SpectreLiveDisplay` |
| Spectre's `Progress` | `SpectreTaskProgress` |
| A new control I want to build with Spectre styling | subclass `RenderableControl` |
| Markup in a plain block of text | [`TextPanel`](Lists%20and%20Data.md#textpanel) |

Check the [decision table](README.md) before wrapping: many Spectre widgets have a native equivalent that takes
input and themes properly. `BarChart`, `Tree`, `Table` → `DataTable`, and the progress widgets all have native
counterparts. Wrapping is for what doesn't.

## `SpectreControl<T>`

Wraps any `IRenderable` as a control.

```csharp
using Spectre.Console;

var panel = new Panel("[green]all systems normal[/]").Header("Status");
var control = new SpectreControl<Panel>(panel).WithFrame();   // frame it like any other control

var root = new DockPanel(DockedControlPlacement.Top, control, body);   // place it like any other control
```

`Content` reads the wrapped renderable back. The control measures and renders through Spectre, so the widget lays
itself out exactly as it would on the console.

**Mutate the wrapped widget through `UpdateContent`, not directly:**

```csharp
control.UpdateContent(t => t.AddRow("node", "11.4"));
```

`UpdateContent` applies the mutation on the UI thread — inline if you're already there, marshaled otherwise — so a
non-atomic change can never race with rendering. Mutating `Content` directly from a background thread is a data
race against the render path: Spectre widgets hold plain collections, and the renderer enumerates them.

Wrapped widgets are display-only. They don't take focus or input, because Spectre widgets have no concept of
either.

## `SpectreLiveDisplay`

Spectre's `LiveDisplay` — a renderable refreshed repeatedly from your own loop.

```csharp
var live = new SpectreLiveDisplay(table);

live.StartAsync(async ctx =>
{
    while (UI.IsRunning)
    {
        table.AddRow(NextRow());
        ctx.Refresh();
        await Task.Delay(500);
    }
});
```

`Start` takes a synchronous callback, `StartAsync` an async one. The callback runs on its own thread and drives
refreshes through the `LiveDisplayContext`, exactly as it would in a console app.

Observe `UI.IsRunning` (or `UI.CancellationToken`) in the loop so it exits when the app stops.

## `SpectreTaskProgress`

Spectre's multi-task `Progress` widget.

```csharp
using Spectre.Console;

var progress = new SpectreTaskProgress();
progress.AddColumns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn());

progress.StartAsync(async ctx =>
{
    var task = ctx.AddTask("downloading");
    while (!task.IsFinished) { task.Increment(2); await Task.Delay(50); }
});
```

For a **single** task that you place and theme like a normal control, use
[`ProgressBar`](Display%20Widgets.md) instead — it's a plain composable control you drive with `Value`, rather than
a widget that owns a callback. `SpectreTaskProgress` earns its place when you genuinely have several concurrent
tasks and want Spectre's column layout.

## Building a control on Spectre rendering

`RenderableControl` is the base for a new control whose appearance is text and styling. You implement one method:

```csharp
using Spectre.Console.Rendering;

public sealed class StatusLine : RenderableControl
{
    private static readonly Style Green = Color.Green;   // Color → Jumbee Style, implicitly
    private string _text = "";

    protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth) =>
        [new Segment(_text, Green)];                     // Jumbee Style → Spectre Style, implicitly
}
```

The two-step conversion above is deliberate. `Jumbee.Console.Style`'s two-colour constructor requires **both** a
foreground and a background, but a `Color` converts implicitly to a `Style`, and a Jumbee `Style` converts
implicitly to the `Spectre.Console.Style` that `Segment` wants. What you can't do is write
`new Segment(text, Color.Green)` — **C# won't chain two user-defined conversions**, so the colour has to become a
`Style` in its own expression first.

You get Spectre's measurement and styling, and the framework handles buffering and compositing. Override `Measure`
when the default (take the full width) is wrong.

Most built-in controls are built this way — `Button`, `Badge`, `Gauge`, `ListBox`, `Tree`. Choose it over raw
`Control` for anything expressible as styled text; see [Writing Applications](Writing%20Applications.md) for the
full base-class comparison.

## `AnsiConsoleBuffer`

The bridge itself, if you need to render Spectre output into a buffer directly. Three flags matter:

| Flag | Effect |
|---|---|
| `marshal` | routes `Write`/`Clear` through `UI.Invoke`, for widgets that refresh from their own thread |
| `wrap` | wraps glyphs to the next row at the right edge instead of clipping |
| `wrapWords` | wraps at word boundaries, falling back to character wrap for an over-long word |

One behaviour worth knowing because it's deliberate and non-obvious: the buffer reports `Ansi` and `Interactive`
as **intrinsically true**, rather than detecting them from the process's stdout. A buffer always accepts styled
segments and Jumbee re-composites it every frame, so live widgets work regardless of the host's stdout. Inheriting
the ambient detection meant that whenever output was redirected — a pipe, CI, a debugger, any headless test —
Spectre saw a non-interactive console and silently swapped `Progress`'s live renderer for one that drew nothing.

Colour depth and glyph coverage *are* detected, since those describe the real output device.

## See also

- [Writing Applications](Writing%20Applications.md) — retained versus immediate mode, and choosing a base class.
- [Live Data](Live%20Data.md) — the threading rules that `UpdateContent` exists to satisfy.
- [Control Model](Control%20Model.md) — how a control is sized, framed and focused.
