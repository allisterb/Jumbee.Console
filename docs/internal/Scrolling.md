# Scrolling: the frame/control contract, and a proposed `IScrollable`

This note records why "wrap a control in a `ControlFrame` and get a scrollbar" does not work by default, what the
contract actually is today, and a proposal to make it explicit. It was written after the 3D sandbox demo's model
sidebar outgrew its terminal and the obvious fix — frame it, let it scroll — turned out to need three pieces of
knowledge that live in three different places.

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

## What is missing entirely

**Scroll-into-view.** There is no `ScrollTo` / `ScrollIntoView` anywhere in `ControlFrame` or `CompositeControl`.
`CodeEditor` keeps its caret visible by driving the enclosing frame itself (`AutoScroll`), and every other control
that needs it would have to reinvent that. The consequence for a keyboard-navigable composite is sharp: with
`TabNavigatesChildren` set, Tab can move focus to a child that is scrolled out of sight, and the focus cue is drawn
somewhere the user cannot see.

**The stolen column.** Framing a control narrows its interior by one, silently. For a width-tuned control — the
sandbox sidebar sizes its slider label gutter to the exact interior width — that shifts the layout by a cell.

**Horizontal scrolling.** The frame has `Top` and no `Left`. Out of scope for this note, but worth stating so nobody
assumes symmetry.

## Proposal: `IScrollable`

```csharp
public interface IScrollable
{
    int MeasureHeight(int width);   // content height — the scroll range
    int? FocusRow { get; }          // row to keep in view, or null for "don't care"
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

- **Push or poll for `FocusRow`?** Polling an `int?` each layout pass is cheap and needs no notification plumbing,
  but the frame only re-lays-out when something invalidates. A focus move that changes nothing else might not
  trigger one.
- **Does the frame reserve the scrollbar column when content is shorter than the viewport?** Today it always does.
  Reserving only when needed is nicer, but the column appearing on the row that overflows would reflow the content.
- **Height changes still need `Initialize()`, not `Invalidate()`**, so the frame re-measures. An interface does not
  fix that, and it is the second-most-common way to get this wrong.

## Documentation, which needs fixing regardless

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
