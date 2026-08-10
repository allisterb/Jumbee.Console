# 3D Physics Sandbox — implementation plan

Plan for `examples/Jumbee.Console.3DSandboxDemo` (root namespace `Jumbee.Console.SandboxDemo`): a real-time 3D
rigid-body sandbox rendered in the terminal, modelled on [inertia](https://github.com/aclfe/inertia) but with a
solid-shaded renderer inertia doesn't have.

Two goals, in order:

1. A demo that shows Jumbee doing something no other .NET TUI library can. The AudioScope and Globe demos landed
   well for exactly this reason; a 3D physics sandbox is the same shape of argument.
2. Surface real gaps in the library. Every previous demo has paid for itself in fixes, and one blocking gap is
   already confirmed before a line is written (see "Canvas batch API" below).

Everything in this document was verified against the sources on 2026-08-02, not recalled. File and line references
are to the state at that commit.

## Scope

**In:** rigid bodies (boxes, spheres, capsules), an orbit camera, spawn/launch/grab, scene presets, two renderers,
the sidebar UI, help, snapshot tests, a Docker target.

**Out of v1:** inertia's cloth, fluid and n-body modes. Those are ~2,300 lines of hand-written solvers
(`src/cloth.rs`, `src/fluid.rs`, `src/nbody.rs`), not physics-engine features — Box3D gives us none of them. They
are a plausible v2 and the architecture below shouldn't preclude them, but they are not what this demo is for.

## What inertia actually does

Worth stating plainly because the gif oversells the complexity. Reference sources are under
`reference/projects/inertia-main/`.

**The 3D is pure wireframe.** No fill, no z-buffer, no lighting:

- A cuboid is its 8 corners projected and joined by 12 edges (`src/render/shapes.rs`, `draw_cuboid`).
- A sphere is *one* screen-space circle. The radius is found by projecting `center + camera.right * radius` and
  measuring the screen distance from the projected centre (`draw_sphere`). Cheap and surprisingly convincing.
- Depth is a back-to-front sort of whole bodies (painter's algorithm). There is no per-pixel depth anywhere.

**Camera** (`src/render/camera.rs`, 98 lines) is a standard orbit rig: `theta`, `phi`, `distance`, `target`, with
`phi` clamped to `[0.05, π-0.05]` and distance to `[2, 60]`. The view basis is
`forward = normalize(target - eye)`, `right = forward × worldUp`, `up = right × forward`, and `View.transform`
projects a world point onto that basis. Eye position is spherical around the target.

**Projection** (`src/render/projection.rs`, 26 lines) is the whole of it:

```
focal = 1 / tan(fovY / 2)
reject if view.z <= near (0.1)
ndc = (focal * view.x / view.z, focal * view.y / view.z)
```

**The simulation loop is the part worth copying exactly.** Fixed 1/60 s steps driven by an accumulator with a
wall-clock catch-up budget: when a step gets too expensive to keep up, the sim eases into slow motion instead of
stalling input and rendering. Physics is decoupled from frame rate.

**The UI** is a viewport with a right-hand sidebar of stacked panels — mode, params, spawn, inspector — plus a help
overlay (`src/render/mod.rs`, `draw_sidebar` onward). We should mirror this; it reads well and it maps cleanly onto
`DockPanel` + `Boundary`.

## Box3D.NET

`Box3D.NET` 0.3.0 is an **idiomatic C# binding over Erin Catto's Box3D C engine**, not a managed port. It uses
`System.Numerics` types directly, and states no managed allocations on the simulation hot path.

Verified API (from `Box3D.NET.xml` in the package):

```csharp
using var world = new PhysicsWorld();
Body ground = world.CreateStaticBody(new Vector3(0, -0.5f, 0));
ground.AddBox(new Box(new Vector3(50, 0.5f, 50)));
Body ball = world.CreateDynamicBody(new Vector3(0, 10, 0));
ball.AddSphere(new Sphere(0.5f));
world.Step(1f / 60f);
```

`Body` exposes what the renderer and inspector need: `Position`, `Rotation` (quaternion), `LinearVelocity`,
`AngularVelocity`, `Mass`, `Bounds`, `IsAwake`, `GetShapes`, `ShapeCount`, plus `ApplyImpulse` /
`ApplyImpulseToCenter` / `ApplyForce` / `SetTransform` / `Destroy`. Shape types include `Box`, `Sphere`,
`Capsule`, `ConvexHull`, `CollisionMesh`, `HeightField`. Nine joint types exist (`JointType`).

There is also an `IDebugDrawer` / `DebugShape` / `IDebugShapeFactory` API. **Worth a look in M0** — if it yields a
usable draw list, it may be a cleaner source of geometry than walking bodies ourselves, and it would demo well.
Don't commit to it before seeing what it actually emits.

**Native dependency is a non-issue.** `Box3D.NET` pulls `Box3D.NET.Native`, which ships
`runtimes/{win,linux,osx}-{x64,arm64}/native/` — six RIDs including `linux-x64` and `linux-arm64`. P/Invoke is the
supported NativeAOT path, so this is the same situation as NAudio in AudioScopeDemo, which already publishes and
trims clean in the AOT image. The only real constraint is that AOT publishing is per-RID, which it already is.
Watch for the known `PublishAot`-per-project gotcha when wiring the Docker target.

## Rendering: two renderers, one interface

This is the design decision that makes the demo worth building rather than porting.

Inertia is wireframe because ratatui's canvas offers braille and nothing else. We are not limited to that.
`Globe` already proves the alternative in this codebase (`src/Jumbee.Console/Controls/Globe.cs:312`): two rays per
character cell, drawn with `▀`/`▄` half-blocks carrying independent foreground and background RGB — 2× vertical
resolution with true colour per sub-pixel. Add a z-buffer and a flat-shaded triangle rasteriser and the result is
solid, lit, depth-correct 3D in a terminal.

| | Wireframe | Solid |
|---|---|---|
| Surface | `Canvas`, braille 2×4 sub-cell | custom `Control`, half-block 2×1 |
| Colour | one per shape | per sub-pixel, lit |
| Depth | painter's sort, whole bodies | per-sub-pixel z-buffer |
| Cost | low; scales to high body counts | higher; the headline screenshot |

Both live behind `ISceneRenderer` and are toggleable at runtime. Showing the same scene both ways, live, is the
demo's best single moment.

Prior art for the solid path, both under `reference/projects/`: `glyphion-main` does z-buffered `SetPixel` with
per-cell chars and RGB (`Glyphion/Core/Renderer/TerminalRenderer.cs:300`), and `3d-engine-on-terminal-main` does a
NumPy z-buffered triangle rasteriser (`src/graphicspipe/renderer.py:106`). Neither combines depth with half-block
sub-cell colour, which is the combination available to us.

## Math, and where the frame time actually goes

Write the camera and projection ourselves — it is ~120 lines and a dependency would be absurd. Use
`System.Numerics` (`Vector3`, `Quaternion`, `Matrix4x4`): Box3D.NET already speaks those types so there is no
conversion at the boundary, and the BCL's matrix and quaternion operations are already hardware-accelerated.

**Do not hand-roll SIMD up front.** For reference, inertia doesn't either — its `Cargo.toml` takes rapier3d and
nalgebra at defaults and never enables `simd-stable`/`simd-nightly`. More importantly, the transform math is not
where the time goes. A rough budget for a 200×50 terminal at 60 fps (16.6 ms/frame):

| | Work per frame | Realistic cost |
|---|---|---|
| Physics | ~50 bodies, native Box3D | sub-ms, and not ours to optimise |
| Projection | ~400 points (50 boxes × 8 corners) | microseconds |
| Solid rasterisation | ~20k sub-pixels, ~600 triangles | well under 1 ms scalar |
| **Composite + ANSI emission** | **~10,000 cells, every one changed, fg+bg colour** | **likely dominant** |

A 3D viewport changes every cell every frame, which defeats the dirty-rect renderer — the optimisation that took
the dashboard from 26 ms to 1.3 ms is precisely the one this workload cannot use.

**So the first measurement in M0 is not the physics or the renderer.** Measure an empty full-screen control
repainting every frame, at a few terminal sizes. That bounds everything else: if the library's full-screen repaint
costs 8 ms there is no budget to spend on anything clever, and if it costs 1 ms then scalar C# will fit
comfortably. If the ceiling turns out to be the library's render path, that is the most valuable thing this demo
can find — it limits every full-screen animated control, not just this one.

If something later does need vectorising it will be the solid rasteriser's inner loop (edge functions and z-test
across 8 sub-pixels via `Vector256<float>`, with `Vector<T>` as the portable fallback for linux-arm64), not the
transforms. One cheap hedge to take now: shape the physics snapshot as **parallel arrays rather than an array of
structs**. It costs nothing today and it is the layout SIMD would need later; retrofitting it is the expensive
part.

## Architecture

**`PhysicsRunner`** — owns the `PhysicsWorld`, steps on a **background thread** at fixed 1/60 with inertia's
catch-up accumulator, and publishes one **immutable snapshot per tick** (per body: position, rotation, half
extents, shape kind, velocity, awake, colour key). This is precisely the snapshot-per-tick pattern from
`docs/controls/Live Data.md` — the demo should dogfood our own guidance, and any friction here is a finding.

**`Scene`** — camera, projection, colour mode, selection, spawn settings. UI-thread owned.

**`ISceneRenderer`** — `WireframeRenderer` (drives `Canvas`) and `SolidRenderer` (writes cells directly).

**`SceneView : Control`** — reads the newest snapshot and draws it. Reads `ActualWidth`/`ActualHeight` **inside
`Render()`**, never in a constructor or setter; that trap is documented in
`docs/controls/What Happens When.md` and has already bitten one cold-start port.

**Shell** — `DockPanel`: footer docked bottom, sidebar docked right inside a `Boundary`, viewport filling. Never a
`Grid` at the root — four independent ports have made that mistake.

## Canvas batch API (approved — worth doing, but not a prerequisite)

`Canvas.Add` wraps each shape in `UI.Invoke` and calls `Rebuild()`
(`src/Jumbee.Console/Controls/Canvas/Canvas.cs:153`). **`Rebuild()` is only `_dirty = true; Invalidate();`**
(`Canvas.cs:294`) — the actual rasterisation, `BuildLayers`, runs **once per frame** from `Render()`, gated on
that flag. So the per-frame cost of an N-shape scene is:

- N × `UI.Invoke` — which runs *inline* when already on the UI thread, so not a marshal;
- N × closure allocation (the lambda captures the shape), N × `_ops.Add`, N × redundant `Invalidate()`;
- **1 ×** `BuildLayers` rasterising all N shapes, which is the work we actually want.

At ~200 shapes and 60 fps that is ~12,000 closure allocations and redundant invalidations per second. Real GC
churn on the frame path, linear rather than quadratic, and **not blocking** — the demo can ship without this.

Still worth fixing properly, for three reasons: it removes the churn, it gives a clean one-call-per-frame
semantic, and it removes the "did I remember to `Clear()` first" footgun that per-frame use of a retained-shape
API otherwise has. Shape to aim for: one call that replaces the whole shape set — e.g.
`canvas.Frame(c => { c.Add(...); ... })` or `SetShapes(IEnumerable<IShape>)`. Requirements:

- one `UI.Invoke`, one dirty/invalidate, one `BuildLayers`, for the whole batch;
- no allocation per shape on the frame path;
- must not regress the existing retained/`DamageTracking` path (the outage-map case is 3× off damage tracking);
- benchmark before and after with `PerfHud` at a realistic body count, and record the numbers.

**Do not turn `DamageTracking` on for the 3D view.** It narrows what the compositor *scans* and does nothing for
`Add` or `BuildLayers` cost, and break-even needs the changed region to be a small fraction of the control. An
orbiting camera changes nearly every cell every frame, so it is all bookkeeping and no saving — the case both
`Canvas` and `Plot` default it off for.

## Milestones

**M0 — spike.** In this order: (1) measure a full-screen control repainting every frame at a few terminal sizes —
that is the ceiling, see the section above; (2) publish AOT for `win-x64` *and* `linux-x64` to confirm the native
dep flows, before anything depends on it; (3) Box3D world with falling boxes on a static plane, orbit camera,
wireframe. Look at `IDebugDrawer` here and decide whether it beats walking bodies ourselves. Record the numbers
from (1) in this file.

### M0.1 result — the full-screen repaint ceiling (measured 2026-08-09)

`tests/Jumbee.Console.Benchmarks -- --fullscreen` (`FullScreenBenchmarks.cs`) drives a `Control` that rewrites its
whole area every frame with `▀`/`▄` half-blocks carrying independent fg/bg RGB — the `Globe` technique the solid
renderer will use — through the real `ConsoleManager` headlessly. Four fills bracket where the time goes: `blank`
paints nothing, `static` writes every cell but always the same value (full paint, nothing emitted), `animated`
changes every cell *and* leaves no two neighbours agreeing, `shaded` changes every cell in moving 8-cell bands.

Per frame at 200×50 (10,000 cells), and at 240×67 (16,080 — a maximised window):

| Fill | frame, 200×50 | ANSI bytes/frame | frame, 240×67 | ANSI bytes/frame |
|---|---|---|---|---|
| blank | ~0.6 ms | 4 B | ~0.9 ms | 4 B |
| static | ~1.0 ms | 4 B | ~1.3 ms | 4 B |
| **shaded** | **~1.0–1.6 ms** | **51 KB** (3.0 MB/s at 60 fps) | **~2.0 ms** | **83 KB** (4.9 MB/s) |
| animated | ~3.1–5.3 ms | 369 KB (21.6 MB/s) | ~5.0–8.7 ms | 596 KB (34.9 MB/s) |

**There is budget.** A flat-shaded viewport at 200×50 costs ~1.6 ms of a 16.6 ms frame — under 10%, with the
scalar rasteriser still to be paid for out of the remaining 15 ms. The plan's bad case ("if full-screen repaint
costs 8 ms there is no budget for anything clever") did not happen.

**Three findings that change the renderer's design:**

1. **Emission dominates, not painting.** Writing 16,080 cells costs ~0.3 ms; compositing and emitting them costs
   1.7–8.3 ms. Optimising our shading loop is optimising the wrong 5%.
2. **Cost tracks how much adjacent cells differ, far more than cell count.** Same cells, same writes, same paint
   time — `shaded` emits **7× fewer bytes** than `animated` and runs **~3× faster** end to end, purely because
   neighbours share a style and the renderer coalesces the run. So **quantise the shade ramp**: flat facets with
   few distinct colours are not just stylistically right for this demo, they are the single largest perf lever
   available. A smooth or dithered gradient would put us in the `animated` column.
3. **`ConsoleManager.LastFrameDirtyCells` counts cells *re-composited*, not cells emitted** — a whole-area
   repainter always reports 100%, in every fill including the ones that emit 4 bytes. It is not a usable signal
   here; ANSI bytes is. (Consistent with the documented `Damage` semantics: damage narrows what is scanned, never
   what is sent.)

**On trusting these numbers:** the byte counts are exact — identical to the digit across three runs (369,090 /
596,188 / 51,277 / 82,793). The times are not: the same configuration varied up to 2.3× between runs on this
desktop (240×67 `blank` draw: 0.6 / 1.4 / 0.6 ms). Quote the bytes; treat the times as order-of-magnitude. This is
`perf-measurement-methodology` playing out exactly as recorded — deterministic counters over microseconds.

### M0.2 result — Box3D under NativeAOT (2026-08-09)

The API in 0.3.0 lives in namespace **`Box3D`** (not `Box3D.NET`), and differs from what this document recorded:
both `world.CreateDynamicBody(pos)` and `world.CreateBody(BodyDefinition.Dynamic(pos))` exist, and shapes are built
with factories — `Box.Cube(0.5f)`, `Box.FromSize(v)`, `new Sphere(r)` — not `new Box(halfExtents)`. Also
`PhysicsWorld.Count` is **static** (worlds, not bodies); the per-world body figure is `AwakeBodyCount`. One thing
absent from the surface: there is no way to read a box shape's half extents back off a `Shape` — `Shape.Bounds`
gives a world AABB, which for a rotated box is not the same thing. The renderer must therefore **keep the extents
it spawned with**, which the snapshot was going to carry anyway.

The smoke check (`Program.cs` at this milestone) drops eight unit cubes onto a static plane and steps 600 × 1/60 s.
They settle at y = 0.499, 1.496, 2.492 … and all eight go to sleep — the engine loads, simulates and sleeps.

- **win-x64 NativeAOT: works, and trims clean.** 1.12 MB binary (a real AOT image — the apphost-stub tell is
  ~76 KB), plus `box3d.dll` 1.10 MB alongside; runs and exits 0. **Zero `IL2xxx`/`IL3xxx` warnings.** As with
  NAudio, P/Invoke is the supported AOT path and nothing here needs a rooting descriptor.
  Needs the host-toolchain workaround: `$env:PATH = 'C:\Program Files (x86)\Microsoft Visual Studio\Installer;' + $env:PATH`.
- **linux-x64: the native asset flows.** `dotnet publish -r linux-x64` stages `libbox3d.so` (1.13 MB) beside the
  managed assemblies, so the RID-specific resolution is correct.
- **The linux NativeAOT *link* is deferred to M5's Docker target.** NativeAOT cannot cross-compile OS boundaries —
  it needs clang in a linux image, which is exactly what `Dockerfile.aot` already does for AudioScope. Doing it
  now would mean pulling `dotnet/sdk:10.0` + clang (~3–4 GB) onto a host disk at 98% (23 GB free), which is the
  condition that corrupted the Docker WSL disk once already. The residual risk is low: the managed side has zero
  AOT warnings and the per-RID native asset resolves.

### M0.3 result — the wireframe spike (2026-08-09)

Shipped: `Camera.cs` (orbit rig, view basis, projection — 130 lines, `System.Numerics`, faithful to inertia's maths
including the φ clamp and the project-a-point-one-radius-right sphere trick), `SceneSnapshot.cs` (parallel arrays),
`PhysicsRunner.cs` (background thread, fixed 1/60 with the catch-up budget, `PhysicsScene` command queue),
`ISceneRenderer.cs`, `WireframeRenderer.cs`, `SceneView.cs`, `SceneFooter.cs`, `Program.cs`.

Validated headlessly — a scratch harness compiling the demo's sources against `Jumbee.Console.Snapshot`, letting the
physics settle 180 steps, then painting one frame: the floor grid recedes correctly to a horizon with the box stack
standing on it. Orientation was checked numerically rather than by eye (tower top projects to ndc.y = +0.54, world
origin to −0.07 — up is up).

**Frame cost of the real scene**, 100×34, camera orbiting every frame, median over 150 frames:

| Bodies | project + `Canvas.Add` | Canvas rasterise | composite + emit | total | ANSI |
|---|---|---|---|---|---|
| 11 | 36 µs | 420 µs | 1345 µs | 1.80 ms (11% of a 60 fps frame) | 5.7 KB |
| 50 | 128 µs | 1089 µs | 1443 µs | 2.66 ms (16%) | 8.2 KB |
| 200 | 418 µs | 2783 µs | 1468 µs | 4.67 ms (28%) | 11.8 KB |

**This inverts M0.1's conclusion for the wireframe, and it is worth being precise about why.** M0.1 measured a
control that rewrites *every cell*; a wireframe lights a sparse scatter of braille cells and leaves the rest blank,
so it emits 6–12 KB/frame rather than 51–369 KB — **30× less** — and emission stops being the bottleneck. What
dominates instead is `Canvas`'s own rasterisation, and it scales with body count (2.8 ms at 200 bodies) while
emission stays flat. So the two renderers have genuinely different bottlenecks: **wireframe is paint-bound and
scales with bodies; solid will be emission-bound and scale with screen area.** M0.1's "quantise the shade ramp"
lever applies to the solid renderer only.

Note the `Canvas.Add` column is the closure-churn the batch API would remove — 418 µs at 200 bodies, ~13% of that
frame. Real, worth fixing, still not blocking.

**Two library findings, both confirmed by being bitten:**

1. **`FillsFrameViewport` is a trap with a silent failure mode.** A framed `SceneView` came out **97×1000**, not
   97×31: a wrapping `ControlFrame` offers an unbounded height so a scrollable child can grow, and a control with
   no intrinsic height falls through `CalculateSize` to the 1000-row clamp. The camera's whole picture then lands
   off-screen and the viewport renders **completely empty** — which reads as "my projection maths is wrong", not as
   a layout problem. This is the third demo to meet it (see the AudioScope notes). Any control that is a *window*
   rather than a *document* needs `FillsFrameViewport => true`, and nothing in the failure points at it.
2. **`protected internal` virtuals are awkward to override from another assembly.** Both `FillsFrameViewport` and
   `GetHelpInfo` must be overridden as plain `protected` outside the library, and writing the modifier the base
   declares gives `CS0507: cannot change access modifiers` — an error that reads as nonsense when you have copied
   the signature verbatim. Cheap fix: document the `protected override` form on both members.

**M1 — sandbox.** Grid floor, spawn box/sphere at the camera target, launch impulse, grab and drag, delete,
reset, pause and single-step.

### M1 result — the sandbox interactions (2026-08-09)

All of M1 is in, faithful to inertia's approach: screen-space picking (project each body's centre, nearest within
0.08 NDC), ray/plane dragging, and a kinematic hold. New: `SpawnSettings.cs`, plus `Viewport` in `Camera.cs`,
selection/pick on `ISceneRenderer`, and spawn/launch/grab/delete on `SceneView` and `PhysicsScene`.

Design notes worth keeping:

- **Selection is by id, not index.** `SceneSnapshot.Ids` carries a stable per-body id assigned at spawn; deleting a
  body shifts every index after it, so an index-keyed selection would silently retarget.
- **A held body goes kinematic**, is steered with `Body.MoveTowards` (a real velocity, so it shoves the rest of the
  scene rather than teleporting through it), and is handed back to the solver with a throw velocity on release. The
  drag target is re-applied *every step* rather than once per pointer move — applying it once lets the body arrive
  and then overshoot on the following steps.
- **Throw velocity is measured from where the grab point went**, over the last 5 samples, capped at 40 m/s. A flick
  across a coarse terminal grid otherwise implies an absurd speed, and releasing after a pause should drop a body,
  not fling it.
- **`Pick` is a default interface method on `ISceneRenderer`** — it needs only `Viewport` and `Projection`, so both
  renderers get it without duplication. Note it is then callable only through the interface, not the concrete type.

**The finding that matters, because it is a library trap and it bit a user, not a test.** The arrow keys did nothing
in the shipped M0 build. The cause:

> `Layout.OnInput` treats a `CompositeControl` as a *container*: it calls `RouteInterceptInput` and then dispatches
> into the composite's **content layout**, so the key lands on whichever child has focus. **`CompositeControl.OnInput`
> is never called on this path.** A composite whose children are display-only — here a `Canvas`, which does not
> handle input — therefore drops every key silently.

The fix is to handle keys in **`InterceptInput`**, which is consulted on *both* routes (`Layout.OnInput` before it
descends, and `ControlFrame.OnInput` before it forwards). A composite that owns its keys rather than delegating them
to children belongs there.

**And the reason this shipped: the obvious test passes.** `UI.SendInput(control, key)` dispatches to
`control.FocusableControl` — the `ControlFrame` — which *does* reach `OnInput` via its
`FocusedControl ?? _control` fallback. The live loop instead hands the event to the **root layout**
(`UI.OnInput` → `layout.OnInput`), which takes the branch above. So the two paths genuinely differ, and a test built
on `UI.SendInput` is green while the app is dead. **Route keyboard tests through the root layout**
(`root.OnInput(new UI.InputEventArgs(new InputEvent(key)))`) when what is under test is a composite.

Mouse routing does *not* have this problem: `CompositeControl`'s indexer attaches the composite's own listener to any
cell whose child carries none, provided the composite sets `WantsMouse`. Click-to-select was verified end to end
through `ConsoleSnapshot.ToTextAfterClick`.

**Verification.** A headless harness (scratchpad `render3d`) exercises 28 checks against the real physics thread and
compositor: keyboard through the root layout, click-to-select through the real mouse routing, spawn, launch,
pick round-trip (project a body, pick that cell back, expect the same body — this catches the renderer's bounds and
the viewport's un-projection drifting apart), grab/drag/throw/fall, delete, clear, and the selection highlight read
back as emitted colour via `AnsiConsoleSnapshot`. All pass. **M5 should promote this harness into a real test
project** — it has already caught one shipped bug.

**M2 — solid renderer.** Z-buffer plus flat-shaded triangles at half-block resolution, one directional light,
runtime toggle against wireframe.

**M3 — UI.** Sidebar panels (mode, params, spawn, inspector showing the selected body's mass/velocity/sleep
state), footer key hints, F1 help via `HelpInfo`.

**M4 — polish.** Colour modes (velocity / mass / sleep), motion trails, scene presets: stack, tower, pyramid,
domino run, wrecking ball.

**M5 — ship.** Snapshot tests, README, gallery entry, Docker target beside `audio-scope`.

## Expected library gaps

Predicted, so they can be recognised rather than worked around silently:

1. **`Canvas` batch API** — confirmed and approved for a proper fix, but an optimisation rather than a blocker;
   see above for the corrected cost model.
2. **No reusable half-block surface.** `Globe` has the technique inline and private. A shared cell/half-block
   drawing primitive would serve any 3D, image or heatmap control.
3. **No layout-changed event** (finding V-21). The viewport needs the aspect ratio; workable from `Render()`, but
   this would be the third demo to want it.
4. **Aspect correction for 2:1 cells.** Confirm `Canvas`'s `XBounds`/`YBounds` don't distort circles in a
   non-square viewport — inertia corrects for this explicitly.
5. **No depth or triangle rasterisation support** anywhere in the library. App-level for now; a candidate to
   graduate if it comes out clean.

## Verification

- Frame cost measured with `PerfHud`, not estimated. Record numbers at M0 and again at M2.
- Snapshot tests via `Jumbee.Console.Snapshot`. **Set `SnapshotImageOptions.FontFamily = "Cascadia Mono"` for any
  PNG containing braille** — the default font has no coverage at U+2800–U+28FF and cells rasterise as boxes.
- Physics runs on a background thread, so assert on a *snapshot*, not on live `Body` state.
- The doc-snippet test (`tests/Jumbee.Console.DocSnippets`) covers `docs/` and the READMEs; any code in this
  demo's README is not covered by it unless the scan set is extended.

## Key references

| What | Where |
|---|---|
| inertia camera / projection / shapes | `reference/projects/inertia-main/src/render/` |
| inertia sim loop and UI | `src/app.rs`, `src/render/mod.rs` |
| Demo target (gif) | `reference/screenshots/inertia/demo.gif` |
| C# terminal 3D engine (z-buffer) | `reference/projects/glyphion-main/Glyphion/Core/Renderer/` |
| Python z-buffered rasteriser | `reference/projects/3d-engine-on-terminal-main/src/graphicspipe/renderer.py` |
| Half-block technique in this repo | `src/Jumbee.Console/Controls/Globe.cs:312` |
| Canvas (and the batch gap) | `src/Jumbee.Console/Controls/Canvas/Canvas.cs:153` |
| Threading pattern to follow | `docs/controls/Live Data.md` |
| Layout traps to avoid | `docs/controls/Layouts.md`, `docs/controls/What Happens When.md` |
