# Layouts

A **layout** (`ILayout`) arranges independent top-level controls. `UI.Start` takes one as the root of the app, and
layouts nest freely inside each other.

Most of the choice between them is obvious from what they arrange — rows and columns, two panes, a stack. The part
that isn't obvious, and the part that decides whether your app looks right in someone else's terminal, is **how each
one sizes**. That's what this guide is mostly about.

## The one rule that matters: which layouts fill the terminal

Jumbee re-lays-out the whole app whenever the terminal is resized. But a layout only *uses* the new size if its
sizing model is proportional — and one of them, `Grid`, is deliberately absolute:

| Layout | Sizing model | Fills the terminal? |
|---|---|---|
| `DockPanel` | docked child fixed on one axis, other child takes the rest | **Yes**, both axes |
| `SplitPanel` | first pane = `SplitPosition` (absolute cells), second takes the rest | **Yes**, both axes — see the caveat below |
| `VerticalStackPanel` / `HorizontalStackPanel` | fills across the stack axis, sums along it | **Width yes**, height = content |
| `Grid` | every row height and column width is an absolute cell count | **No** — stays the size you gave it |
| `Boundary` | pins one or both of its child's extents | No, by design |
| `TabPanel` | follows the active page | — |
| `Overlay` | follows its bottom layer | — |

Measured with framed content, so the border shows each layout's actual allocation:

| Layout | in a 40×10 terminal | in a 100×30 terminal |
|---|---|---|
| `Grid([5], [30], …)` | 30×5 | **30×5** |
| `DockPanel` | 40×10 | **100×30** |
| `SplitPanel` | 40×10 | **100×30** |
| `VerticalStackPanel` | 40×6 | **100×6** |

> **`Grid` does not grow, and nesting doesn't rescue it.** A `Grid` inside a `DockPanel`'s fill slot still renders at
> its declared size — the *region* grows, the grid doesn't. If your app renders correctly at one terminal size and
> squashed or letterboxed at every other, a `Grid` at the root is almost always why.

**Filling is not the same as staying proportional.** Both `DockPanel` and `SplitPanel` fill the space they're given,
but the size of the *docked* pane is an absolute cell count in both cases. A `SplitPanel` whose first pane is 20 cells
stays 20 cells when the terminal grows — the second pane absorbs all of the new space. So a layout expressed as "this
pane is a fixed sidebar" holds at every size, while one expressed as "this pane is half the screen" does not: it is
half only at the size you tuned it for.

There is no fractional/percentage split and no layout-changed event to recompute one from. If you need a pane to
track a proportion, drive `SplitPosition` yourself from the container's measured extent — and be aware that's app
code you have to run whenever the size changes, not something the layout maintains for you.

## Choosing

- **Building the app shell?** `DockPanel` and `SplitPanel`. Bars docked top and bottom, a sidebar docked left,
  resizable panes in the middle. This is what a full-screen app is made of.
- **Arranging a fixed region?** `Grid`. A form, a panel of labelled fields, a dashboard tile, a dialog's interior —
  anywhere you are deliberately choosing the geometry rather than inheriting it.
- **A row or column of controls?** `VerticalStackPanel` / `HorizontalStackPanel` — a toolbar, a button row, a
  settings list. They size to their content along the stack axis, so wrap one in a `Boundary` before docking it.
- **Alternate whole pages?** `TabPanel`.
- **Pop-ups, menus, modals?** `Overlay` — see below.

A typical app is a few nested `DockPanel`s with `Grid`s inside the regions that genuinely want fixed geometry.

## Giving a docked child a size: `Boundary`

Controls like `TextInput` have their own `Width`/`Height`. Many things don't — a `ControlFrame` (anything you called
`.WithFrame()`/`.WithBorder()` on), a nested layout, a stack panel. Those expand to fill whatever they're given, so
docking one directly means it takes the entire panel and starves the fill child. `Boundary` pins the extent:

```csharp
new Boundary(sidebar, width: 24)     // 24 columns wide, full height
new Boundary(toolbar, height: 1)     // one row tall, full width
```

**Constrain only the docked axis and leave the other unset** (`null`, the default) so it stretches. Setting both
pins the child to a fixed box, which is what you want for a tile and not what you want for a sidebar.

## Recipes

### An app shell — header, status bar, sidebar, content

Three nested `DockPanel`s. Each docked child is bounded on its docked axis; everything else stretches.

```csharp
var body       = new DockPanel(DockedControlPlacement.Left,   new Boundary(sidebar,   width: 24), content);
var withHeader = new DockPanel(DockedControlPlacement.Top,    new Boundary(titleBar,  height: 1), body);
var root       = new DockPanel(DockedControlPlacement.Bottom, new Boundary(statusBar, height: 1), withHeader);

await UI.Start(root, width: 100, height: 30, input: new VtInputSource(anyMotion: true));
```

The `width`/`height` passed to `UI.Start` are the *initial* terminal size; because the shell is built from
`DockPanel`s, it reflows from there rather than staying pinned to it.

### Master–detail with a draggable divider

```csharp
var split = new SplitPanel(SplitOrientation.Horizontal, headlines, article, splitPosition: 32);
```

`SplitOrientation` describes the *panes*, not the divider: `Horizontal` puts them side by side (first = left),
`Vertical` stacks them (first = top). The divider is draggable with the mouse, and focusable for arrow-key resizing.

Because the first pane's width is just a property, a "zen mode" toggle is free — collapse it to a sliver and back:

```csharp
int expanded = split.SplitPosition;
split.MinFirst = 1;
UI.RegisterHotKey(UI.HotKeys.Char('z'), () =>
    split.SplitPosition = split.SplitPosition > split.MinFirst ? split.MinFirst : expanded);
```

`SplitPosition` can't reach 0 — `MinFirst` clamps to at least 1 — so a hair of the first pane always remains.

### A dashboard of fixed tiles

This is `Grid` used correctly: the tiles are a deliberate size, and the grid sits inside a shell that fills.

```csharp
var tiles = new Grid(
    rowHeights:   [8, 8],
    columnWidths: [30, 30],
    controls: [[cpuTile, memTile], [diskTile, netTile]]);

var root = new DockPanel(DockedControlPlacement.Top, new Boundary(titleBar, height: 1), tiles);
```

Note what this does and doesn't get you: the shell reflows, the tile block does not. That's the right trade for
tiles with a designed size. If you want the tiles themselves to grow, build the rows and columns from nested
`DockPanel`s or `SplitPanel`s instead.

## Overlays: pop-ups, menus and modals

An `Overlay` layers content above the main view. `UI.Start` creates one for you and publishes it as `UI.Overlay`,
which is where `Dialog`, `ContextMenu`, `Select`'s dropdown and `Autocomplete`'s suggestion list attach themselves —
so most of the time you never touch it directly.

You do need it explicitly in two places:

```csharp
var overlay = new Overlay(root);   // build one yourself
UI.Overlay  = overlay;             // ...when there's no UI.Start to do it (e.g. a snapshot test)
dialog.Show(overlay);              // ...or show a modal on a specific overlay
```

Snapshot tests are the common case — see
[Testing a modal dialog](../../GETTING-STARTED.md#testing-a-modal-dialog). The rule there is that the modal lives in
the overlay, not in your root layout, so a snapshot of the root shows no dialog at all.

## Gotchas

- **A layout is not a `Control`.** You can't subclass one into a reusable widget, and you can't put a bare layout
  where a control is expected. To package an arrangement as a reusable component, subclass `CompositeControl` — see
  [Composite Controls](Composite%20Controls.md).
- **`Grid`'s `0` is not `DockPanel`'s `0`.** In a `Grid`, a `0` row height or column width means a collapsed
  0-cell track. In a control's `Width`/`Height`, `0` means "fill the parent". The same literal means opposite
  things, and it's the most common source of an app that renders blank or takes the whole screen.
- **Stack panels expand across their axis.** A `HorizontalStackPanel` is full-height by default, so docking one as
  a toolbar without a `Boundary(height: 1)` collapses the region below it.
- **Nesting a scroll inside a scroll.** A control that scrolls itself shouldn't sit inside a scrolling
  `ControlFrame` — both will try.

## See also

- [Getting started §3 — Layouts](../../GETTING-STARTED.md#3-layouts) — the short version, in context.
- [Composite Controls](Composite%20Controls.md) — packaging an arrangement as a reusable control.
- API reference: [`DockPanel`](../api/Jumbee.Console.DockPanel.md) · [`SplitPanel`](../api/Jumbee.Console.SplitPanel.md) ·
  [`Grid`](../api/Jumbee.Console.Grid.md) · [`Boundary`](../api/Jumbee.Console.Boundary.md) ·
  [`Overlay`](../api/Jumbee.Console.Overlay.md)
