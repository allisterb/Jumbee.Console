# Control Model

What the pieces are, what nests inside what, and who owns sizing, focus and input. Read this once and most of the
library's shape stops being surprising.

## Four kinds of thing

| | What it is | How many children |
|---|---|---|
| `Control` | draws itself and takes input — the leaf | none |
| `ControlFrame` | adornment *around* a control: border, title, margin, scrollbar | exactly one |
| `ILayout` | arranges independent controls in space | many |
| `CompositeControl` | a single `Control` built out of several others | many, privately |

The distinction people trip over is the last two. **A `CompositeControl` is not a layout.** It uses one internally,
but from the outside it's a single control: it drops into a layout cell, gets framed, holds focus as a unit, and
its children are its own business. `CodeEditor` is a composite — an editor plus a gutter — and you place it exactly
as you'd place a `Button`.

If you want to *arrange* controls, you want an [`ILayout`](Layouts.md). If you want to *build* one control out of
several, you want a `CompositeControl`.

## Framing is a property, not a wrapper

This is the one that surprises people coming from other toolkits. A frame is not a node you wrap a control in — it's
a property *on* the control:

```csharp
var list = new ListBox("one", "two");
list.WithFrame(title: "Items");            // sets list.Frame and returns list

var root = new VerticalStackPanel(list);   // you still place the ListBox, not a wrapper
```

`WithFrame` and the `WithBorder` / `WithTitle` / `WithMargin` family all set `Frame` and return the control itself,
so they chain and never change what you're holding. `HasFrame` and `Frame` read it back.

The consequence: **you place the control, and the frame comes with it.** There's no wrapper object to keep track of,
and no risk of adding the control while its frame stays behind.

When a control is framed, the frame owns the border, the title, the margin, the scrollbar and — for scrollable
content — the viewport. That's why `ListBox` and `Tree` need a frame to scroll: they size to their content and let
the frame do the scrolling. `Log` and `DataTable` don't, because they own their viewports (`FillsFrameViewport`).

## What nests inside what

- A **layout** holds controls and other layouts, freely. Layouts nest without limit. **You pass the children to the
  layout's constructor** — there's no `Add` method to call afterwards.
- A **control** goes in a layout cell, or inside a composite, or directly as `UI.Start`'s root.
- A **frame** wraps one control, and travels with it.
- A **composite** holds its own children privately and appears as one control.

`UI.Start` takes an `ILayout` as the app root. A bare control needs a layout around it.

One practical rule from the layout guide, repeated because it bites: a **stack panel** sizes to its content along
the stack axis, so wrap it in a `Boundary` before docking it. And to leave a `Boundary` axis unconstrained, omit it
— `null` means "size freely"; `0` does not.

## Sizing

Every control resolves its size in the same order:

1. **`Width` / `Height`** if you set them explicitly.
2. **Intrinsic size** — `IntrinsicWidth()` / `IntrinsicHeight()`, what the content needs. A `Badge` is as wide as
   its text; a `TextLabel` as wide as its string.
3. **The allocated size** the layout gave it.

> **`0` means "unset", not "zero".** Sizing falls through to the next step, so a control with `Width = 0` fills
> rather than disappearing. A docked control given `Width = 0` can take the whole region. This is the opposite of
> what `0` means for a `Grid` row or column, where it's a real zero extent.

`ActualWidth` / `ActualHeight` report what the control actually got — **but only after layout has run.** They're `0`
before that, so anything size-dependent belongs in `Render()`, not a constructor or property setter. `HasLayout`
is the guard.

`MeasureHeight(width)` is how a frame asks a control how tall it wants to be at a given width — which is what makes
scrolling work for content-tall controls.

## Focus

Focus is exclusive: `Focus()` takes it from whoever had it, `UnFocus()` releases, `IsFocused` reads it. Only
controls with `Focusable` set participate, and only the focused control draws the native terminal cursor.

Containers delegate. `FocusedControl` on a composite or layout names which child currently holds focus, and that's
what keyboard input is routed to. A composite that wants Tab to move between its own children opts in with
`TabNavigatesChildren`.

By default a focused control gets a background tint, and a framed one gets its border recoloured to
`IStyleTheme.FocusedFrameBorder`. `RendersOwnFocus` opts out when a control draws its own cue — or when your design
has no focus cue at all and you want every panel to match.

## Input

Input reaches a control through `OnInput`, and tunnels outward: the focused control sees an event first, then its
frame, then the layouts above it. Anything unhandled falls through to the global hotkeys registered with
`UI.RegisterHotKey`.

Mouse events are separate and hit-tested by position — `OnClick`, `OnDoubleClick`, `OnMousePress` / `Release` /
`Move` / `Wheel`, plus `CaptureMouse()` / `ReleaseMouse()` for drags. A control opts in with `WantsMouse`.

> **If you override `OnClick`, override `OnDoubleClick` too.** The second click of a rapid pair routes to
> `OnDoubleClick`, so without it every other click is silently swallowed:
> `protected override void OnDoubleClick(Position position) => OnClick(position);`

Full routing detail, including composites, is in [Input](../internal/Input.md).

## Redraw

Controls don't draw on demand — they mark themselves dirty and the frame loop repaints them.

- `SetAtomicProperty` is the normal path for a scalar property: equality check, assign, invalidate, in one call.
- `Invalidate()` marks the control dirty directly, for anything more complicated.
- `Damage(rect)` reports a changed sub-region when the control opts into `TracksDamage`, so the compositor can skip
  the rest.

A property that changes what's on screen without invalidating simply won't appear. That's the most common bug when
writing a control.

Batch related changes into one setter with one invalidation where you can — and remember there's exactly one UI
thread and no lock, so mutations from elsewhere go through `UI.Invoke`. See [Live Data](Live%20Data.md).

## See also

- [Writing Applications](Writing%20Applications.md) — choosing a base class, and your responsibilities.
- [Layouts](Layouts.md) — the layouts themselves, and which ones fill the terminal.
- [Composite Controls](Composite%20Controls.md) — authoring a control built from other controls.
- [What happens when…](What%20Happens%20When.md) — the surprising consequences of the rules above.
