# Lists and Data

Five controls present rows of things: `ListBox`, `DataTable`, `Tree`, `Log` and `TextPanel`. They look
interchangeable from the outside and are not, so this guide leads with the choice.

## Choosing

| Control | Shape | Interactive | Owns its scrolling |
|---|---|---|---|
| `ListBox` | flat list, one item per row | select, activate, right-click menu | no — a `ControlFrame` scrolls it |
| `DataTable` | columns and rows | select a row, activate it | **yes** — fixed header, own viewport and scrollbar |
| `Tree` | hierarchy, expand/collapse | select, activate, expand | no — a `ControlFrame` scrolls it |
| `Log` | append-only stream | scroll only | **yes** — virtualized, tails automatically |
| `TextPanel` | a block of static markup | none | no |

The distinctions that actually decide it:

- **One column or several?** `ListBox` is one item per row (though the item can be any Spectre `IRenderable`, so a
  "row" can be rich). `DataTable` is real columns with a fixed header.
- **Is it a stream or a list?** If entries arrive continuously and the user mostly wants the newest, that's `Log` —
  it's the only one virtualized for the purpose. Don't build a growing `ListBox` for this; see below.
- **Does the user pick from it?** `TextPanel` is display-only. Everything else takes focus and selection.
- **Who scrolls it?** `DataTable` and `Log` own their scrolling. `ListBox` and `Tree` size to their content and
  expect a frame to scroll them — which is why they usually want `.WithFrame()`.

## `ListBox`

A flat, focusable list. Items are strings or `IRenderable`s, so rows can carry their own styling.

```csharp
var list = new ListBox("Overview", "Processes", "Network", "Disks");
list.SelectionChanged += (_, i) => detail.Markup = Describe(i);
list.WithFrame();   // ListBox is content-tall; the frame gives it a border and a scrollbar
```

Selection is `SelectedIndex` / `SelectedItem`, with `SelectionChanged` as you move and `Committed` on Enter or
double-click (`Cancelled` on Escape). `AddItem` / `AddItems` / `RemoveItem` / `Clear` maintain the contents, and
`Items` exposes them.

Two properties worth knowing: `HighlightFullWidth` extends the selection bar across the control rather than fitting
the text, and `SelectionStyle` switches between highlight, underline and caret cues — see
[Selection Controls](Selection%20Controls.md) for what those look like and when a caret gutter matters.

Set `ContextMenu` to get a right-click menu; `ContextMenuOpening` fires first so you can rebuild the items for the
row under the pointer.

## `DataTable`

Columns and rows, a header that stays put while rows scroll, and a selection bar you navigate with the keyboard or
click.

```csharp
var table = new DataTable("Command", "CPU %", "Count", "Memory %");
table.AddRow("node", "11.4", "1", "2.2");
table.AddRow("firefox", "9.3", "1", "6.6");

table.SelectionChanged += (_, i) => status.Text = $"Row {i}";
table.RowActivated   += (_, i) => OpenDetail(table.SelectedRow);
```

Cells are text — you format numbers before handing them over. `SelectedRow` gives you the selected row's cells back
as a `string[]`, which is what you use to identify it (see the rebuild pattern below).

**Two limits to design around, both deliberate:**

**It doesn't sort.** There is no sort API and no clickable header. Sort your own data and rebuild the table. The
control shows what you give it, in the order you give it.

**It has no in-place row update.** The row API is `AddRow` / `RemoveRow(int)` / `Clear()`, so a table whose values
change every tick must be rebuilt — and rebuilding drops the selection, which has to be restored by key rather than
by index because rows reorder between ticks. [Live Data](Live%20Data.md) has the worked recipe, under "Tearing
down views"; do the whole sequence inside one `UI.Invoke` so no frame paints a half-rebuilt table.

Its colours come from the active theme rather than from properties on the control: the selection bar and the
surface follow `IStyleTheme`, so restyle it by supplying a theme rather than looking for a `SelectedBackground` on
`DataTable`. `IStyleTheme`'s members are default-implemented, so a custom theme means overriding the few you care
about, not all of them — see [Theming](../internal/Theming.md).

**When it gets narrow, columns are dropped rather than wrapped.** `DropNarrowColumns` defaults to `true`: the table
drops columns from the right and always keeps the leftmost, because a table whose headers have broken mid-word is
unreadable. Widths are measured from the header and the rows currently on screen, so one enormous value scrolled
far away can't collapse the layout. Set it to `false` to keep every column and accept the wrapping.

## `Tree`

Hierarchy, with expand/collapse and the same selection model as `ListBox`.

```csharp
var tree = new Tree("Solution");
var src  = tree.AddNode("src");     // Tree.AddNode adds at the root…
src.AddChild("Program.cs");         // …and TreeNode.AddChild adds beneath a node
src.AddChild("Control.cs");

tree.NodeActivated += (_, node) => Open(node);
tree.WithFrame();
```

`AddNode` adds at the root and returns the node; you build depth with `TreeNode.AddChild` on what you got back.
`Root` and the `this[uint]` indexer reach existing nodes, and `RemoveNode` prunes. `SelectedNode` is the current one, with `SelectionChanged` and
`NodeActivated` (Enter or double-click). Arrow keys move and expand: →/← open and close, Enter activates.

Appearance is `Guide` (the connector glyphs), `LeafGlyph` / `LeafGlyphColor` for the icon on childless nodes, and
`HoverHighlighting` / `HoverStyle` if you want rows to light up under the pointer.

## `Log`

An append-only stream, and the only control here built for volume.

```csharp
var log = new Log { MaxEntries = 5000 };
log.Write("[green]ready[/]");
log.Write(someRenderable);
```

It's **viewport-virtualized**: each entry is rendered to visual lines once, on write, and only the visible window is
blitted per paint — so writing and painting cost O(viewport), not O(total entries). It fills its framing viewport
rather than growing content-tall, and draws its own scrollbar in the rightmost column.

Tailing is automatic and does the polite thing: writing while the user has scrolled up keeps the view where they
put it, and new lines accumulate below. Scrolling back to the bottom re-engages tailing. `ScrollToBottom()` forces
it. The wheel scrolls it, and when focused so do Up/Down/PageUp/PageDown/Home/End.

`MaxEntries` caps retention, `Clear()` empties it.

> **Don't use a `ListBox` as a log.** It isn't virtualized, it's content-tall so the frame has to measure all of it,
> and it has no tailing behaviour. It gets slower as the stream grows; `Log` doesn't.

## `TextPanel`

A block of multi-line Spectre markup — the multi-line counterpart to `TextLabel`. Display-only: no focus, no
selection.

```csharp
var panel = new TextPanel("[bold]Weather[/]\n15 °C, overcast\nWind 12 km/h");
panel.Markup = TextPanel.Escape(untrustedText);   // when the content isn't yours
```

For static readouts, ASCII art, key/value summaries — the little boxes in a dashboard that describe rather than
plot. `Escape` neutralises markup in text you didn't author, which matters for anything coming from a file, a
process or the network.

For a scrolling document rather than a fixed block, use [`MarkdownViewer`](Documents.md) instead.

## See also

- [Selection Controls](Selection%20Controls.md) — selection cues, and the toggle/multi-select controls.
- [Live Data](Live%20Data.md) — feeding any of these from a background thread.
- [Layouts](Layouts.md) — frames, and getting a scrollbar around the content-tall ones.
