# Rendering Model: pull-tree vs push-buffer (and why we keep the tree)

This note records a deliberate architectural decision: Jumbee.Console composites the screen by **pulling cells
through the control tree**, not by pushing widget output into one flat frame buffer. It exists so the "why" is on
record when the per-cell composite cost shows up in a profiler (it will) and someone wonders whether to rewrite it.

## The two models

**Push / flat-buffer (e.g. ratatui).** The renderer owns a single flat `Cell` buffer. Each widget writes its cells
*directly* into that buffer at its area during render (last-writer-wins by paint order — overlaps just overwrite).
At the end of the pass the renderer diffs the flat buffer against the previous one and sends only changed cells to
the terminal. Compositing is free: by diff time everything is already one buffer. There is no per-cell tree walk,
and no dirty-rect concept — every widget re-renders into the buffer every frame, and the *write*-diff is what spares
the terminal.

**Pull / tree (ours).** The control tree *is* the source of truth. `ConsoleManager.Update` iterates the cells of
the damaged rect and, for each, calls `ContentContext[position]`, which recursively descends the
`DrawingContext`/`Control` tree (`Overlay.get_Item` → `DrawingContext[...]` → `Child[...]` → …) to fetch that cell.
A control draws its own content into its own buffer in its own coordinate space (drawing); the tree assembles the
final screen by pulling (painting). The renderer's own double-buffer diff + contiguous-run cursor batching then
minimizes terminal writes, same as ratatui's backend.

## The cost, honestly

The per-cell descent is real and it dominates the composite half of a frame: in the AudioScope profile
`Overlay.get_Item` and the surrounding `ConsoleManager.Update` loop are ~40–15% of the frame. Its cost scales with
`screen_cells × tree_depth`, so it grows with terminal size and nesting faster than a flat-buffer diff would. The
push model structurally has no equivalent line.

## Why we keep the tree anyway

The library's value proposition is *composability* — nest layouts arbitrarily, drop a control anywhere, get
transparent overlays/scrims/z-order "for free" — and that DX rests on the tree:

- A control renders its own content and never reasons about placement.
- Layouts nest without any control knowing it's nested.
- Overlays composite per cell (top-if-non-empty-else-bottom), so modals, dim-scrims and popups need no paint-order
  coordination from the app. In the push model these are the app's responsibility (render order into a shared buffer).

`ScopeView` is the proof: an oscilloscope built from `DockPanel` + `Grid` + `TextLabel`s + `Plot`, handling input
and drawing, with the author writing essentially only app logic. That intuitiveness is the thing being protected.

## The key insight that makes the decision safe

**The composability comes from the tree + the drawing/painting separation — NOT from the pull-per-cell mechanism.**
Pull-per-cell is one *implementation* of "composite this tree." A buffer-blend compositor (each control renders into
its own buffer; parents blend child buffers upward) composites the *same* tree, preserves the *same* public API, and
keeps the *same* draw/paint split — it only changes the internal assembly step. So the real fork is not "tree vs
push"; it is "pull-per-cell vs blend-buffers, given the same tree." **Even a future perf ceiling is not a reason to
abandon the model** — the internal compositor can be swapped without touching a line of user-facing API.

## Model-preserving perf levers (the incremental path)

1. **Per-control damage tracking** (`Control.TracksDamage` + `Damage()`). A control that changes only a sub-region
   reports just those rects; the compositor skips the rest — no per-cell descent over unchanged area. `Canvas`
   (the world/outage map, ~3×) and `Globe` already opt in. **`Plot` does not yet** — deferred, because the obvious
   consumer (the dashboard's live *rolling* plot) also **scrolls its horizontal axis**, so "what changed" is nearly
   everything every tick unless damage is computed against the scrolled frame. Needs care; not needed for the scope
   (its waveform is full-dirty every frame anyway, so damage would report ~the whole plot and save nothing).
2. **The write-diff is already there.** `ConsoleManager`'s double buffer + contiguous-run cursor batching already
   emit only changed cells — the same terminal-I/O saving ratatui gets. What the tree pays extra for is the *read*
   (the descent), not the *write*.

## Where this nets out

At 220×53 the pull compositor keeps the scope comfortably close to native scope-tui's free-running frame rate — and
that rate is already many times what a human perceives. The tree model is not the bottleneck at any realistic size,
and the levers above address the tail without spending the composability the model exists to provide. **Decision:
keep the pull-tree model.** Revisit the internal compositor (blend-buffers) only if a genuinely larger/deeper
workload makes the per-cell descent the measured wall, and even then keep the public model intact.
