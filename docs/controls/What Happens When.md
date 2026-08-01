# What happens when…

Behaviour that surprises people, with the answer first. Read this when the app compiles and runs but does something
you didn't expect — the screen doesn't fill, an update never appears, a control silently stops drawing.

Every answer here is checked against the source; the interesting ones cite it.

## Resizing and layout

### …I resize the terminal?

**The whole app is re-laid-out, every frame.** `ConsoleManager` compares the terminal size each frame and pushes any
change into the root layout, which propagates down. There is nothing you have to call.

Whether a given control *uses* the new space is a separate question, decided entirely by the sizing model of the
layout holding it. See the next two answers, and the sizing table in [Layouts](Layouts.md).

### …a `Grid`'s parent grows?

**Nothing. The grid stays exactly the size you declared.** Every `Grid` row height and column width is an absolute
cell count, so the *region* grows and the grid doesn't.

Nesting does not rescue it — a `Grid` inside a `DockPanel`'s fill slot still renders at its declared size. If your
app looks right at one terminal size and squashed or letterboxed at every other, a `Grid` at the root is almost
always why. Build the shell from `DockPanel` and `SplitPanel`; use `Grid` for regions where you genuinely want fixed
geometry (a form, a dialog interior, a dashboard tile).

This is the single most common structural mistake with this library — four independent cold-start ports all made it.

### …a `SplitPanel`'s container grows — do the panes stay proportional?

**No. The first pane stays put and the second absorbs all of the new space.** `SplitPosition` is an absolute cell
count, not a fraction.

So "a fixed 20-cell sidebar" holds at every terminal size, while "half the screen" is half only at the size you
tuned it for. Filling and staying proportional are different properties, and only the first is automatic.

### …I want to recompute a proportional split on resize — what event do I hook?

**There isn't one.** There is no resize or layout-changed event on `Control` or on `UI`. If you need a pane to track
a proportion, drive `SplitPosition` yourself from the container's measured extent, on your own cadence — that's app
code you own, not something the layout maintains.

Stated plainly because it's a real limitation, not an oversight you should keep hunting for.

### …I set `Width = 0`?

**It fills, rather than collapsing to nothing.** `0` is the *unset* sentinel: sizing falls through `Width` → intrinsic
width → the allocated `Size` (`Control.cs:733`). A docked control given `Width = 0` can therefore take the whole
region and blank what you expected to see beside it.

Beware the neighbouring trap: a `0` passed to a `Grid` row/column means an actual zero extent, so the same literal
means opposite things one line apart. To leave a `Boundary` axis unconstrained, omit it (`null` = size freely)
rather than passing `0`.

### …I read `ActualWidth` before the first layout pass?

**You get `0` or a stale value.** Sizes exist only after layout has run, so reading `ActualWidth`/`ActualHeight` in a
constructor or a property setter gives you nothing useful — geometry built there collapses to a single invisible
column.

Build size-dependent geometry in your `Render()` override instead, which runs after layout. `Control.HasLayout`
(`ActualWidth > 0 && ActualHeight > 0`) is the guard if you need to check.

### …I put a `VerticalStackPanel` inside a `DockPanel`?

**It fills across the stack axis and sizes to content along it** — full width, content height. That's usually what
you want for a toolbar or button row, and usually *not* what you want for a docked region, so wrap it in a
`Boundary` to pin the axis you care about.

## Threading and updates

### …I mutate a control from a background thread?

**It depends on what you mutate, and the dangerous case is silent.**

Scalar properties written through `SetAtomicProperty` marshal themselves, so those are safe. Collections do not —
and collections are what a live app actually updates. Writing a `List<double>` from a sampler thread while the
render path enumerates it is a plain data race.

The reason this bites: **an unsynchronized write produces zero lock contention**, so `PerfHud`'s `locks` counter
stays at zero while your state corrupts. Read that counter to confirm you haven't *introduced* locking, never to
prove your threading is right. Full pattern in [Live Data](Live%20Data.md).

### …the action I pass to `UI.Invoke` throws?

**It depends which thread you called it from, and neither case is what a WPF developer expects.**

`UI.Invoke` is not `Dispatcher.Invoke`. It runs the action inline when you're already on the UI thread, and
otherwise posts it fire-and-forget (`UI.cs:627`):

| Called from | Behaviour | An exception… |
|---|---|---|
| the UI thread | runs inline, immediately | propagates to your caller |
| a background thread | posted, runs on a later frame | is **swallowed** (`Dispatcher.cs:266`) |

So the same call site changes semantics with the calling thread, and the background case — the one you wrote it for
— discards the error. Use `UI.InvokeAsync` when you need to wait for completion or observe the fault; it returns a
task that carries the exception.

### …I never `await` the task from `UI.InvokeAsync`?

**The exception is lost.** It's stored on the task, and nothing observes it for you. Faults from fire-and-forget
sampling loops are the usual way a live app goes quiet without a visible error.

## Rendering and cost

### …I do expensive work in a control's `Render()`?

**You stall the entire app for that long, every frame the control is dirty.** There is one UI thread; rendering,
input dispatch and every other control's paint are all on it. A slow `Render()` is not a slow control, it's a slow
application.

Do the expensive part elsewhere and let `Render()` blit a prepared result. That's what the document viewers do:
`MarkdownViewer`, `AsciiDocViewer` and `MermaidViewer` all run their parse and layout on a background thread, so
setting the content or resizing never blocks the UI thread and the view fills in when the render completes.

Guidance for authoring a control this way is in [Composite Controls](Composite%20Controls.md); the frame-path
section of [Live Data](Live%20Data.md) covers keeping a streaming control cheap.

### …my control throws during a frame?

**The exception is swallowed and the UI thread survives.** The frame loop wraps the whole frame in
`try { … } catch { }` with the comment *"a frame error must not kill the UI thread"* (`Dispatcher.cs:250`); posted
actions get the same treatment (`Dispatcher.cs:266`).

That's deliberate — one broken control shouldn't take down the app — but it means **a control that throws every
frame fails invisibly**. There is no log, no crash, no error indicator; you get a stale or partially-drawn screen
and no clue why. If a control has simply stopped updating and nothing looks wrong, put a breakpoint or a try/catch
inside its `Render()` before looking anywhere else.

### …I call `Invalidate()` on every incoming data point?

**You get one redraw per frame, not one per point.** Invalidation marks the control dirty; the frame loop coalesces.
Pushing 10 000 points between frames costs 10 000 pushes and one render.

What that does *not* protect you from is the cost of the pushes themselves, or a `Render()` that rebuilds
everything from scratch. Cap history, update in place, and rebuild on push rather than per frame.

## Input

### …the user double-clicks?

**The second click is routed to `OnDoubleClick`, not to `OnClick` again.** For the built-in controls this is handled
— all nine affected ones were fixed in 0.1.9, and rapid clicks now produce one activation each.

It matters when you author a control: **override `OnClick` without `OnDoubleClick` and you silently swallow every
second rapid click.** An impatient double-click on your button does nothing the second time. The one-liner is
`protected override void OnDoubleClick(Position position) => OnClick(position);`.

### …two controls both want the same key?

The focused control gets first refusal, then the key tunnels outward through frames and layouts, and global hotkeys
registered with `UI.RegisterHotKey` are handled separately. Focus is exclusive — `Focus()` takes it from whoever had
it. The details, including composite routing, are in [Input](../internal/Input.md).

### …I want to turn a control's built-in mouse handling off?

`WantsMouse` is `protected virtual`, so a subclass can suppress it. There is no per-instance property to switch it
off on a stock control.

## Styling and theming

### …I set a themed property explicitly, then switch themes?

**That property stops following the theme, permanently.** Each control records explicitly-set properties in
`ThemeOverrides`, and a later `UI.SetTheme` re-applies only to properties you have *not* overridden.

This is intentional — it's what makes deliberate per-control styling survive a theme switch — but it reads as a bug
from the other direction: you set one colour early on during development, and months later `SetTheme` appears to
skip that control. If a theme switch isn't taking, look for an explicit assignment first.

Re-capture happens on assignment only, never on the render path, so themes cost nothing per frame.

### …a framed control takes focus?

**Its border switches to `IStyleTheme.FocusedFrameBorder`**, so a focused panel looks different from its neighbours.
If your design has no focus cue and you want all panels to match, `RendersOwnFocus` is the opt-out.

### …a `DataTable` is too narrow for its columns?

**Columns are dropped from the right, keeping the leftmost.** `DropNarrowColumns` defaults to `true`, because a
table whose headers have wrapped mid-word is unreadable and its rows stop being one line tall.

Set it to `false` to keep every column and accept the wrapping.

## Testing and capture

### …I capture a braille `Canvas` to a PNG and get empty boxes?

**The font has no braille coverage.** Live rendering and `ToText` are fine — only the image is wrong, because
rasterising needs actual glyphs at U+2800–U+28FF and the default (Consolas) has none.

Fallback fonts are now applied per glyph, so this should resolve itself. If it doesn't, set
`SnapshotImageOptions.FontFamily` to `"Cascadia Mono"` or another braille-covering font.

### …a snapshot test passes but the UI is visibly wrong?

**You probably asserted state rather than what was drawn.** A test checking `SelectedIndex` passes happily while the
highlight bar sits on the wrong row — that exact bug shipped, guarded by exactly that kind of test.

Assert the rendered effect: find the highlight in the rendered text, count the backgrounded cells, compare the
glyph. Similarly, a call that succeeds is not evidence the thing happened — routing a hotkey that was never
registered returns success and does nothing. See [Snapshot Testing](../internal/Snapshot%20Testing.md).

## See also

- [Controls](README.md) — the decision table and category guides.
- [Live Data](Live%20Data.md) — the threading and streaming answers above, in full.
- [Layouts](Layouts.md) — the sizing answers above, with measured tables.
