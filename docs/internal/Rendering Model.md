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
   (the world/outage map, ~3×), `Globe`, and `Plot` (opt-in via `Plot.DamageTracking`, off by default) use it.
   An earlier revision of this note predicted damage would "save nothing" for the scope because its waveform is
   full-dirty every frame. Measurement disproved that for every signal short of full scale — see **When damage
   tracking pays** below, which also covers what it explicitly does *not* buy.
2. **The write-diff is already there.** `ConsoleManager`'s double buffer + contiguous-run cursor batching already
   emit only changed cells — the same terminal-I/O saving ratatui gets. What the tree pays extra for is the *read*
   (the descent), not the *write*.

### When damage tracking pays

Damage narrows what the compositor **scans**. It does not change what the terminal **receives** — lever 2 is why:
the per-cell write-diff already gates emission, so a frame's escape sequences are decided by which cells actually
differ, never by which rects were declared dirty. Measured on the live scope (220×53, two 2048-point braille
series, `Benchmarks -- --damage`):

| trace fill | tracking | dirty cells | composite | ANSI bytes/frame |
|---|---|---|---|---|
| 15% of Y range | off | 11660 / 11660 | 1336 µs | 15620 |
| 15% of Y range | on  | 1966 / 11660  | 271 µs  | 15739 |
| 95% of Y range | off | 11660 / 11660 | 1772 µs | 97562 |
| 95% of Y range | on  | 11660 / 11660 | 1560 µs | 97562 |

Cell and byte counts are deterministic; the microsecond figures drift with machine load (the two dense rows do
identical work — identical dirty cells — so their composite difference is drift, not signal).

**Bytes are flat.** The 0.8% rise at 15% fill is extra cursor positioning between disjoint rects. So damage
tracking is never the lever for terminal load; that is set by how many cells genuinely change, i.e. data density
(`--buffer`, trace thickness), and it is the real ceiling at high frame rates.

On the read side the saving is real and large when the figure is sparse — ~5× off the composite, roughly halving
the frame. But the break-even is worse than a cell-for-cell comparison suggests, for two reasons:

- **The scan it replaces is sequential.** The double-buffer diff walks memory linearly — the friendliest possible
  access pattern. Damage bookkeeping is scattered by nature: save a previous value on first touch, then flush over
  a list of touched indices. A cell *avoided* is therefore cheaper than a cell *added*, so swapping N scanned cells
  for N tracked ones is a net loss. The win only arrives at a large ratio.
- **The recorder sits in the write path.** `DamageBuffer` wraps the plot's render target, so every cell write pays
  an extra indirection whether or not tracking achieves anything that frame. On the dense workload that residual
  accounted for essentially all of the ~370 µs paint overhead (~20k writes/frame), which is why the guards inside
  it — bail past half the surface, then skip the next 60 frames before re-probing — bound the waste but cannot
  remove it.

**Rule of thumb:** on when the changing part of the figure is a small, bounded fraction of the control's area; off
when the figure fills its area. A live scope trace, a moving sprite, a marker set: yes. A dense heatmap, a
full-scale waveform: no. `Plot.DamageTracking` defaults to off for that reason; the AudioScope demo turns it on
because a scope signal spends most of its time well away from the rails, and the dense case costs ~4%.

Measure rather than assume: `Benchmarks -- --damage` reports dirty cells, ANSI bytes and the paint/composite split
with tracking on and off, over the live `SetData` path. Note that the *rebuild* path (`Clear` + `AddSeries` each
frame) reports `DamageAll` by construction, so tracking cannot help there at all — benchmarking a rebuild workload
prices the bookkeeping and none of the benefit.

## Where this nets out

At 220×53 the pull compositor keeps the scope comfortably close to native scope-tui's free-running frame rate — and
that rate is already many times what a human perceives. The tree model is not the bottleneck at any realistic size,
and the levers above address the tail without spending the composability the model exists to provide. **Decision:
keep the pull-tree model.** Revisit the internal compositor (blend-buffers) only if a genuinely larger/deeper
workload makes the per-cell descent the measured wall, and even then keep the public model intact.
