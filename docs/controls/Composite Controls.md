# Composite Controls

A **composite control** is a single `Control` built out of several child controls laid out together — for
example a `CodeEditor` (a `TextEditor` with a line-number gutter docked to its left). Because it *is* a
`Control`, it drops into any layout cell, can be framed, and participates in theming and painting like any leaf
control.

This is what `Layout` cannot give you: a `Layout` arranges independent top-level controls, but it is not a
`Control`, so you can't subclass it into a reusable widget. `CompositeControl` fills that gap.

## Using one: `CodeEditor`

```csharp
var editor = new CodeEditor(Language.CSharp) { Text = File.ReadAllText("Program.cs") };
editor.WithRoundedBorder(Color.Cyan1).WithTitle("Program.cs");

var grid = new Grid([20], [80], [[editor]]);
var run = UI.Start(grid, width: 84, height: 22, input: new VtInputSource(anyMotion: true));
UI.SetFocus(editor.Editor);   // focus the inner editor to type
run.Wait();
```

The gutter automatically tracks the editor's line count and highlights the caret's line, and long lines soft-wrap.
To scroll content taller than the viewport, **wrap the `CodeEditor` in a frame** (e.g.
`codeEditor.WithRoundedBorder()` or `.WithFrame()`); then arrows / PageUp-Down scroll, `AutoScroll` keeps the
caret visible, the scrollbar tracks position, and the gutter stays aligned with the scrolled text. `editor.Editor`
and `editor.Gutter` expose the children.

> **To make your own composite scroll, override `MeasureHeight`.** `CompositeControl` doesn't do it for you, and
> the default reports no height at all — you get a 1000-row scrollbar over a mostly empty region, silently.
> `CodeEditor` returns the editor's wrapped row count, which is what lets an enclosing frame scroll gutter and text
> together; it also scrolls that frame itself (`AutoScroll`) to keep the caret in view, because focus doesn't scroll
> into view on its own. Full rules, including the traps, are in
> [Control Model → Scrolling](Control%20Model.md#scrolling).

## Authoring a composite: subclass `CompositeControl`

Build the children, arrange them with any `Layout` (here a `DockPanel`), wire their interactions, then call
`SetContent`:

```csharp
public class LabeledInput : CompositeControl
{
    private readonly TextLabel _label;
    private readonly TextEditor _input;

    public LabeledInput(string caption)
    {
        _label = new TextLabel(TextLabelOrientation.Horizontal, caption, Color.Grey) { Focusable = false };
        _input = new TextEditor();

        // Inter-child behaviour goes here — just subscribe to the children's events:
        _input.Changed += (_, _) => { /* e.g. validate, update the label … */ };

        SetContent(new DockPanel(DockedControlPlacement.Left, _label, _input));
    }

    public TextEditor Input => _input;
}
```

`SetContent` takes an `ILayout`. For a composite that wraps a **single** child (e.g. one `ListBox` or
`MarkdownViewer` with your own logic around it), wrap that child in a one-child layout — `Boundary` — so it fills
the composite: `SetContent(new Boundary(child));` (pass `Boundary(child, width, height)` to pin a fixed size
instead). For two-or-more children, use `Grid`/`DockPanel`/a stack panel as above.

What you get for free:

- **Layout** — reuse `Grid`, `DockPanel`, `HorizontalStackPanel`, etc. as the internal arrangement.
- **Rendering & mouse** — each child renders itself; the composite composites their cells (with the child's own
  mouse listener intact), so hover/click and click-to-focus reach the children automatically.
- **Keyboard** — `CompositeControl` overrides `FocusedControl` to return the focused descendant, so keystrokes
  from the parent layout route to the right child.
- **Inter-child communication** — the composite owns the children as fields, so you wire their events directly in
  the constructor. No event bus or mediator is needed.

Notes:

- Mark pure adornments (labels, gutters) `Focusable = false` so they stay out of the tab order and don't take
  clicks.
- The composite paints nothing of its own by default. Override `Render()` to draw a background or separators into
  the buffer *behind* the children.
- Expose the children you want callers to drive (e.g. `Editor`, `Input`) as properties.

See `CodeEditorDemo` in the TestDemo project for a runnable example.
