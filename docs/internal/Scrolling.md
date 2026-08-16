# Scrolling: the frame/control contract, and `IScrollable`

This note records why "wrap a control in a `ControlFrame` and get a scrollbar" did not work by default, what the
contract used to be, and the interface that replaced it. It was written after the 3D sandbox demo's model sidebar
outgrew its terminal and the obvious fix — frame it, let it scroll — turned out to need three pieces of knowledge
that live in three different places.

> **Status: shipped.** `IScrollable` carries `MeasureHeight` plus a defaulted `FocusRowChanged` event;
> `ControlFrame` keys off the interface, subscribes to the event, owns the one `ScrollIntoView` implementation, and
> reveals a newly focused descendant on its own; `FillsFrameViewport` is gone. The polled `int? FocusRow` proposed
> below was **rejected** in favour of the event — see *Why the focus row is an event, not a polled property*.
> Everything from "The default is the broken one" through "Blast radius" describes the world *before* the change and
> is kept as the rationale; the sections after the rule describe what shipped.

## What the frame does today

Scrolling is the frame's job, not the control's. `ControlFrame` owns the viewport, the scrollbar, and the offset:

- It hands its child `int.MaxValue` for height so the child can grow past the visible area
  ([ControlFrame.cs:631](../../src/Jumbee.Console/ControlFrame.cs:631)) — unless the child sets
  `FillsFrameViewport`, which gets the bounded viewport height instead.
- It reserves a column for the scrollbar ([ControlFrame.cs:626](../../src/Jumbee.Console/ControlFrame.cs:626)).
- It owns the offset (`Top`) and moves it: `Scroll(n) => Top += n`
  ([ControlFrame.cs:592](../../src/Jumbee.Console/ControlFrame.cs:592)), driven by the wheel
  ([ControlFrame.cs:1034](../../src/Jumbee.Console/ControlFrame.cs:1034)) and by thumb drag.
- It sizes the thumb from the child's height against the viewport height.

So the machinery genuinely is free. The child's only job is to answer one question honestly: **how tall is your
content at this width?** That answer is `MeasureHeight(width)`.

## The default is the broken one

`MeasureHeight` defaults to `0` ([Control.cs:770](../../src/Jumbee.Console/Control.cs:770)), meaning "no opinion,
fill what you're given." Under a frame, what you're given is `int.MaxValue`, and `CalculateSize` clamps that to 1000
([Control.cs:723](../../src/Jumbee.Console/Control.cs:723)). `CompositeControl` overrides neither `MeasureHeight`
nor `IntrinsicHeight`, so a composite that hasn't thought about scrolling resolves to exactly 1000 rows.

Measured, not inferred — a two-row composite under the limits a frame actually passes:

```
plain composite (no MeasureHeight): 1000
composite with MeasureHeight => 2: 2
```

The result is a scrollbar with a sliver of a thumb over 998 blank rows. Nothing throws, nothing logs, the control
renders correctly at the top of a vast empty region, and a snapshot test of the visible area passes. It is the same
shape as the other entries in *Layout silent failure modes*: the wrong answer is the one you get by doing nothing.

That is the whole problem. The mechanism is fine; the **default** is inverted.

## The contract, as it currently stands

Three unrelated pieces, none of which references the others:

| Piece | Kind | Default | Meaning |
|---|---|---|---|
| `MeasureHeight(width)` | `protected virtual` | `0` | content height → the scroll range |
| `FillsFrameViewport` | `protected internal virtual` | `false` | "I own my own viewport, never scroll me" |
| `ControlFrame.Top` / `Scroll(n)` | public on the frame | — | the offset, owned by the frame |

Seven controls override `MeasureHeight` (`ListBox`, `TextEditor`, `TextPanel`, `MarkdownViewer`, `CodeEditor`,
`ChatPrompt`, `Dialog`); nine override `FillsFrameViewport` (`Canvas`, `Dialog`, `Globe`, `InteractiveSourceEditor`,
`Log`, `MultiTabCodeEditor`, `Plot`, `RunChart`, `TerminalEmulator`). Every one of those is a decision an author had
to know to make. Nothing in the type system asks the question.

## What was missing entirely

*(All three were true before the change. The first is now solved for controls but not for composites; the second and
third stand.)*

**Scroll-into-view.** There was no `ScrollTo` / `ScrollIntoView` anywhere in `ControlFrame` or `CompositeControl`.
`CodeEditor` keeps its caret visible by driving the enclosing frame itself (`AutoScroll`), and every other control
that needs it would have to reinvent that. The consequence for a keyboard-navigable composite is sharp: with
`TabNavigatesChildren` set, Tab can move focus to a child that is scrolled out of sight, and the focus cue is drawn
somewhere the user cannot see.

**The stolen column.** Framing a control narrows its interior by one, silently. For a width-tuned control — the
sandbox sidebar sizes its slider label gutter to the exact interior width — that shifts the layout by a cell.

**Horizontal scrolling.** The frame has `Top` and no `Left`. Out of scope for this note, but worth stating so nobody
assumes symmetry.

## Proposal: `IScrollable`

> **As proposed, not as shipped.** `MeasureHeight` landed as-is; the polled `FocusRow` property became a
> `FocusRowChanged` event. Kept here because the reasoning below is what the rejection argues against.

```csharp
public interface IScrollable
{
    int MeasureHeight(int width);   // content height — the scroll range
    int? FocusRow { get; }          // row to keep in view, or null for "don't care"  (NOT shipped)
}
```

`ControlFrame` tests `if (control is IScrollable s)`. **No interface means no scrolling**: the child gets the
bounded viewport height, no scrollbar, no reserved column — today's `FillsFrameViewport` behaviour becomes the
default, and the interface becomes the opt-in.

That inversion is the point of the whole exercise. A control whose author never considered scrolling gets the
outcome that looks right. A control that wants scrolling gets a compiler-enforced list of what it owes. The 1000-row
failure stops being reachable by omission.

**`FocusRow` rather than a `ScrollTo` method** because the frame owns the offset. A control calling `ScrollTo` on
its parent is the `CodeEditor.AutoScroll` pattern, and it inverts responsibility — the child reaches up and mutates
the parent's state. Having the child *report* its interesting row and letting the frame clamp `Top` around it keeps
the offset in one place, and turns scroll-into-view from something each control writes into a slot each control
fills. `CodeEditor` would reduce to `int? FocusRow => CaretRow`.

**Explicit implementation keeps the surface clean.** `int IScrollable.MeasureHeight(int w) => …` puts nothing new on
the public API. The codebase already uses that idiom for capability-not-API — see
[Control.cs:329](../../src/Jumbee.Console/Control.cs:329).

`FillsFrameViewport` disappears: it becomes the absence of `IScrollable`.

## Blast radius

Sixteen in-repo controls (seven `MeasureHeight` + nine `FillsFrameViewport`, `Dialog` in both) need touching, and
all of it is mechanical — the nine `FillsFrameViewport` overriders just lose the override.

The break is for third-party controls relying on today's implicit default. They would stop scrolling rather than
start scrolling wrongly, which is the right direction to fail, but it is a real break and belongs in a minor version
with a note. Worth checking against the `jc-curious` ports before committing, since those are the closest thing to
outside consumers we have.

## Open questions

- **Push or poll for `FocusRow`?** *(Resolved: push. See the section below.)*
- **Does the frame reserve the scrollbar column when content is shorter than the viewport?** Today it always does.
  Reserving only when needed is nicer, but the column appearing on the row that overflows would reflow the content.
- **Height changes still need `Initialize()`, not `Invalidate()`**, so the frame re-measures. An interface does not
  fix that, and it is the second-most-common way to get this wrong.

## Documentation, which needs fixing regardless

*(All three fixed. `Control Model.md` now carries the canonical recipe under its own **Scrolling** heading, names
the 1000-row symptom outright, and documents both caveats; the composite page defers to it instead of restating
it.)*

The recipe exists and is good — [Composite Controls.md:29](../controls/Composite%20Controls.md) covers the unbounded
height, the `MeasureHeight` requirement, `Initialize()`-not-`Invalidate()`, and the don't-nest-two-scrollers rule.
Three problems with it:

1. **It is filed under composites.** Someone framing a plain control to get a scrollbar has no reason to open that
   page. [Control Model.md:81](../controls/Control%20Model.md) — where they would look — gives `MeasureHeight` one
   sentence and never says you must override it.
2. **The failure mode is not named.** The text says the scrollbar will not be "accurate" without `MeasureHeight`,
   which reads as cosmetic. "You get a 1000-row scroll range" is the sentence that would actually stop someone.
3. **The two caveats are absent** — the reserved column, and the missing scroll-into-view.

These fixes are cheap and independent of the interface. If `IScrollable` lands, about half the prose becomes
unnecessary, because the interface says it.

## Recommendation

Do the doc pass now; it costs little and helps immediately. Treat `IScrollable` as the durable fix and schedule it
against a minor version, because the value is entirely in flipping the default and that is exactly the part that
breaks existing controls. Prototyping it behind the current behaviour is possible — `is IScrollable` first, fall
back to `MeasureHeight`/`FillsFrameViewport` — but a compatibility shim preserves the silent failure it is meant to
remove, so it is only worth it as a migration step with an end date.

---

## What actually landed

Phase 1 shipped as designed — interface carries `MeasureHeight`, frame keys off `is IScrollable`,
`FillsFrameViewport` deleted, no shim. Four things the plan above got wrong or did not anticipate.

**The blast radius was 39 sites, not 16.** The estimate counted only `src/`. `examples/` held 20 more, and those are
doc surface too — example source is displayed beside each example in the browser. All of it was mechanical, and
every break was a compile error at the exact line.

**`MeasureHeight` is public, not explicitly implemented.** The plan wanted `int IScrollable.MeasureHeight(int w)` to
keep the surface clean. Explicit implementations are not virtual, so the first subclass wanting a different
measurement would have had to re-declare the interface — a footgun of exactly the kind being removed. It is
`public virtual` instead. Nothing derives-and-overrides today, but the trap was not worth setting.

**`Dialog` overrode both, and needed neither.** It had `MeasureHeight` *and* `FillsFrameViewport => true`, which
contradict each other under the new model. The measurement turned out to be dead code: `Dialog` always sets an
explicit `Height` in its constructor, and `CalculateSize` honours that before any content height. It now implements
nothing and behaves identically.

**`Tree` was scrolling by accident, and the change caught it.** `Tree` overrode nothing at all — no `MeasureHeight`,
no `IntrinsicHeight` — so the old default ballooned it to the 1000-row clamp and the frame windowed *that*. It
scrolled, so nobody noticed; the snapshot test asserting "scrolled after navigating down" passed for the wrong
reason. Flipping the default broke that test, which is how the latent defect surfaced. `Tree` now implements
`IScrollable` and reports its real row count, measured at a fixed wide width like `ListBox` (a width-dependent
height feeds the content-height↔width convergence loop and can fail to settle).

That last one is the argument for the whole exercise, made concretely: the old default did not merely permit a
silent failure, it was actively hiding one in a shipped control.

Guarded by `tests/Jumbee.Console.Tests/ScrollableTests.cs` — the negative case (no interface → sized to viewport,
never 1000) is the one that matters.

## Why the focus row is an event, not a polled property

Scroll-into-view shipped as `event EventHandler<RowSpan>? FocusRowChanged` on `IScrollable`, not the polled
`int? FocusRow` property proposed above. Two reasons the polling form was rejected, the first fatal on its own.

**Polling would fight the user.** The frame lays out on resize and on content change. If it re-asserted a focus row
every layout, then after you wheeled away from the selection, the next live-log append or terminal resize would yank
you back. A scroll offset the user set must not be overridden by a control's standing preference — only by an event
that genuinely moves the selection.

**And it would mostly not fire.** A focus change calls `Invalidate` (repaint), not `Initialize` (layout). Polling in
`Initialize` would miss most focus moves entirely, so the feature would be both intrusive and unreliable.

Push is also what the three existing implementations already did — `ListBox`, `Tree` and `CodeEditor` each called
their private `AutoScroll` at exactly the moment the selection or caret moved. That trigger was always right; only
the arithmetic was duplicated, and unifying it fixed a real bug: `Tree`'s copy lacked the guard `ListBox` had for a
span taller than the viewport, so navigating to a tall node scrolled past its own first row and pushed the
just-selected node off the top.

### Why an event rather than the control calling the frame

The first cut had the control call `Frame.ScrollIntoView(...)` directly. That works, but it is the inverted
responsibility the original `FocusRow` argument objected to: the child reaches up and mutates its parent's state.
The event recovers the layering without reintroducing polling — the control announces "my selection is now at these
rows" and knows nothing about frames; the frame subscribes and decides. `ControlFrame` attaches and detaches in
`BindControl`, which is the single path used by both the constructor and the settable `Control` property, so
replacing a frame's control stops it following the old one. (That detach is load-bearing and tested: removing it
makes `ReplacingTheWrappedControl_StopsFollowingTheOldOne` fail.)

`ControlFrame.ScrollIntoView` stays public, because not every scroll is a selection move — following new output to
the bottom, or restoring a saved position, has no business being an event.

### What the event does and does not enforce

Declared field-like, an event that is never raised gets **CS0067** in the implementer's own build, which is exactly
the mistake worth catching. Three limits, all verified rather than assumed:

- **Explicit implementation escapes it.** An explicit implementation cannot be field-like at all (`CS0071`), so it
  must use `add`/`remove` accessors — and with no compiler-generated backing field there is nothing for CS0067 to
  notice. The style that keeps members off the public surface is precisely the style that silences the check.
- **A raise-method pattern also escapes it.** Adding `RaiseFocusRowChanged(...)` that invokes the event makes the
  event "used" whether or not anything ever calls the raise method, so the lazy implementer compiles clean. It also
  cannot live on the interface: a default interface method may not raise the interface's own event (`CS0079`),
  because an interface has no backing field. So that pattern buys per-class boilerplate and costs the warning.
- **Opting out is silent.** The event's do-nothing default is what keeps the seven selectionless implementers
  (`MarkdownViewer`, `AsciiDocViewer`, `MermaidViewer`, `TextPanel`, `TranscriptView`, `TaskListView`,
  `ChatPrompt`) from each carrying a CS0067 they cannot fix. The cost is that a control which *should* report a
  selection can simply not declare the event, and nothing complains.

So CS0067 is a nudge for the careless, not a guarantee. A real guarantee would be a Roslyn analyzer; the type
system cannot require that a method be *called*.

**Composites are automatic after all.** An earlier draft of this note claimed a composite would have to work out its
focused child's row itself, because a control has no position information. That was too pessimistic — see
*Revealing a focused descendant* below. The underlying facts are still worth recording, because they are what shapes
the solution:

- A control *does* hold its context — `ConsoleGUI.Common.Control` has `private IDrawingContext _context`, surfaced
  as an explicit `IDrawingContext IControl.Context`, so it is not visible as `this.Context` in a derived control.
- But `IDrawingContext` exposes only `MinSize`, `MaxSize`, `Redraw`, `Update`, `SizeLimitsChanged` — **no offset**.
  The concrete `DrawingContext` has `public Vector Offset`; the control only ever sees the interface. That is the
  pull model's discipline: a control draws in its own coordinate space and never reasons about placement.
- Even downcasting would give the wrong offset. A `DrawingContext` is created by the parent, so its `Offset`
  describes one parent→child hop; scroll-into-view needs a row within a distant ancestor. And `DrawingContext.Parent`
  is an `IDrawingContextListener`, not another `DrawingContext`, so there is no generic upward walk to accumulate
  offsets.

### Revealing a focused descendant

The missing link in the list above is that `DrawingContext.Parent`, though typed `IDrawingContextListener`, is in
practice also an `IControl` with a context of its own — so the chain *can* be walked after all, without extending
the layout contract:

```csharp
var ctx = ((IControl)focused).Context as DrawingContext;
while (ctx is not null) { if (ctx.Parent is ControlFrame f) { /* reveal at row */ }
                          row += ctx.Offset.Y; ctx = (ctx.Parent as IControl)?.Context as DrawingContext; }
```

`ControlFrame.RevealFocused` does this, called from `Control.IsFocused` on gain. Two properties make it work:

- **It reads the layout's real positions**, so it cannot drift from the layout the way arithmetic in a composite
  would. That matters here: the sandbox sidebar's Scale section changed height twice in one afternoon.
- **The walk stops at the context a `ControlFrame` owns**, and that context's offset — the frame's own border and
  scroll offset — is deliberately not counted, so the result is a row in content coordinates.

The rule that keeps it from fighting the event: **only a descendant deeper than the frame's direct child is
revealed.** A framed `ListBox` reveals the selected *item* through the event; if the frame also revealed the list
itself on focus, it would drag `Top` back to 0 every time the list was focused.

Verified against nested framed composites, which is the shape that matters — a walk through a stack, through each
`Section`'s frame, into its inner content, resolved every row exactly.

**A trap this exposed, worth knowing on its own.** While validating the walk, a probe reported an inner control at a
row 3 lower than the layout implied. The walk was right; the *probe* was wrong — its composite under-reported
`MeasureHeight` (25 rows for content needing 35). Under-reporting does not clip the tail of a stack, it **collapses
it to zero height**: the last sections came back with `frameH=0`, `innerH=0`, and the walk faithfully reported where
those flattened controls now were. Over-reporting is harmless (a stack sizes to its content). So for a hand-written
composite `MeasureHeight`, under-reporting is the dangerous direction, and it fails by silently flattening your last
children rather than by looking obviously wrong.
