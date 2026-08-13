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
- **The launch muzzle distance is derived from the body's size, not a constant.** Inertia spawns the projectile at
  `eye + dir * 1.0`, which in a terminal viewport puts it on the lens: measured, a default sphere projects to an NDC
  radius of 0.866 on its first frame — **141% of the viewport height, a circle 42 rows tall on a 30-row view** — and
  because NDC X spans ±1 while Y spans ±0.61 it is clipped top and bottom, so it reads as a full-width *ellipse*
  collapsing to a dot rather than as an object being thrown. Solving `ndcRadius = focal · r / distance` for a target
  first-frame size (0.2 NDC) instead gives ~4.3 units for a sphere and ~7.5 for a box, and stays right when `+`/`-`
  changes the spawn scale. A spawn or launch also selects what it produced, so the footer names it — the id only
  comes back through the snapshot, since the command that created it ran on the physics thread.

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

### M2 result — the solid renderer (2026-08-09)

`HalfBlockSurface.cs` (a `W × 2H` sub-pixel colour + depth buffer presented as `▀` cells, fg = upper sub-pixel,
bg = lower), `Meshes.cs` (indexed unit cube and UV sphere, built once), `SolidRenderer.cs` (perspective-correct
z-tested triangle fill, backface culling, one directional light, quantised shading, checkerboard ground). `v`
switches renderers live; `SceneView` holds a list and swaps the layout child.

**The headline number: the solid renderer emits FEWER ANSI bytes than the wireframe, at every size** — despite
covering every cell rather than a scatter of them. Median of 120 frames, 7 bodies, camera orbiting every frame:

| | scene | paint | emit | total | % of a 60 fps frame | ANSI |
|---|---|---|---|---|---|---|
| solid, 100×34 | 377 µs | 98 µs | 892 µs | 1.37 ms | 8% | 4.3 KB |
| wireframe, 100×34 | 7 µs | 329 µs | 255 µs | 0.59 ms | 4% | 5.0 KB |
| solid, 200×50 | 291 µs | 191 µs | 1141 µs | 1.62 ms | 10% | **11.8 KB** |
| wireframe, 200×50 | 10 µs | 648 µs | 831 µs | 1.49 ms | 9% | 12.7 KB |

This is the M0.1 lever paying off, and it is worth stating plainly because it inverts the intuition: a wireframe
lights *fewer* cells but they are scattered singletons, each needing its own cursor move and SGR change, while
quantised flat shading produces long runs the renderer coalesces. Against M0.1's synthetic bounds at 200×50 — 51 KB
for coherent content, 369 KB when every neighbour differs — the real solid renderer lands at **11.8 KB**, better
than even the coherent synthetic case, because a real scene has large uniform regions the synthetic one never had.

Both renderers sit at ~10% of a 60 fps frame at 200×50. There is plenty of budget left for M3–M4.

**A sign bug worth recording, because the reflex answer is wrong.** Meshes are wound counter-clockwise seen from
outside in world space, but the screen mapping inverts Y (NDC +y is up, rows count downward) and that flip
*reverses handedness* — so an outward-facing triangle arrives at the culling test with a **negative** signed area.
Culling `area <= 0` therefore discards every visible face and keeps only hidden ones: bodies render as their own
far side, and single-sided geometry like the ground vanishes completely. The failure looks like "almost nothing is
drawn", not like "the culling is backwards".

#### Half-lambert, from `voxcii` (2026-08-10)

`reference/projects/voxcii-main` is a triangle rasteriser with a z-buffer — architecturally a **subset** of what
`MeshRenderer` already does (orthographic rather than perspective, one sample per cell rather than two sub-pixels,
ANSI-256 by material index rather than true colour, a hardcoded `1.8f` cell-aspect fudge rather than a derived one).
Nothing in its rasteriser is worth taking. One line of its shading is:

```
sim = norm.dot(light) * 0.5 + 0.5      // instead of max(0, N·L)
```

**Half-lambert wrapping, and it earns its place *because* a terminal has so few shade levels.** Clamping sends every
face turned past perpendicular to the same flat black, so the entire unlit half of an object collapses to one value.
Now `ShadedRenderer.WrapLighting` (default on). Note the specular must gate on the **raw** dot, not the wrapped one —
wrapped never returns zero for a merely-turned-away face, so gating on it puts highlights on surfaces facing into
shadow.

**The first comparison suggested it was a slight loss**, because the camera and lamp were on the same side: with no
unlit faces visible, wrapping only compresses the lit range and costs contrast. Re-shot from the opposite side, the
result is unambiguous — clamped renders the box tower as a near-black column with its colours gone, wrapped keeps
all seven bodies legible and individually coloured. For a sandbox where bodies tumble and the camera orbits freely,
the dark side is a constant case, so it stays on.

Unexpected secondary benefit, measured: wrapping emits **16% fewer ANSI bytes** (15,584 vs 18,591 at 200×50).
Compressing the lit side into the upper half of the range means adjacent cells land on the same quantised level more
often, so runs coalesce better — the M0.1 model predicting its own consequence.

**Also worth taking from that repo, but not a rendering technique:** OBJ/STL loading. `models/` has bunny (4,968
faces), cow (5,804) and teapot (6,320) — all within budget — plus a 249,882-face dragon that is not. The value is
that Box3D's `ConvexHull.FromPoints`/`Body.AddHull` and `CollisionMesh.FromTriangles`/`Body.AddMesh` would make a
loaded model an actual rigid body to grab and throw, not scenery. Deferred as its own milestone.

#### Three renderers, and what looking at a PNG changed (2026-08-10)

Point lighting, silhouettes and contact darkening were first bolted onto `SolidRenderer` as modes. That was wrong —
it turned one renderer into a mode matrix and made "solid" mean two different things. Split into three renderers
cycled by `v`, sharing a rasteriser:

| | `MeshRenderer` (abstract) | shading | post-process |
|---|---|---|---|
| `WireframeRenderer` | — (drives a `Canvas`) | one colour per body | — |
| `SolidRenderer` | ✓ | flat, per **triangle**, directional | — |
| `ShadedRenderer` | ✓ | point light + specular, per **pixel** | silhouettes + contact darkening |

**The methodology lesson is the bigger one.** Every visual judgement up to here was made from an ASCII luminance
dump piped through bash, which mangles `▀`, braille and `◆◇◈◊` into `?`. `ConsoleSnapshot.SavePng` was available
the whole time and needs only `SnapshotImageOptions.FontFamily = "Cascadia Mono"` for glyph coverage. The first
actual PNG immediately exposed three defects that every passing test had missed:

1. **The shaded renderer was much darker than the flat one.** `attenuation = 1/(1 + d²/R²)` at `R = 14` collapsed
   the far checkerboard to near-black, destroying the recession cue that makes the flat renderer read as 3D. The
   point light exists for the *gradient across a face*, not for dramatic falloff — `R = 40` fixed it.
2. **Edge glyphs were outlining the ground plane's own outer boundary**, which reads as speckle along the horizon
   rather than as shape. Sub-pixels now carry a `group` byte (scenery vs body) and only bodies are outlined.
3. **The outline was invisible on sleeping bodies.** A sleeping body is drawn at a third brightness, so an outline
   that inherits its surface colour came out as the faintest glyph in a dark colour on a dark background — present
   in the buffer, and invisible on screen. Edge cells now *boost* brightness rather than inherit it.

None of these were detectable from the tests, all three were obvious in one image. Two of them were things I had
explicitly reasoned about and got wrong.

Also flushed out: the harness's selection-highlight check asserted an exact `(255,255,255)`, which only held while
shading was flat enough to reach full intensity. Now that the tint is lit and quantised like any other surface, the
check tests the real signature — achromatic and bright, which no body colour or ground shade can produce.

**Cost**, 200×50, 7 bodies, orbiting camera (bytes exact, times noisy):

| | scene | total | ANSI |
|---|---|---|---|
| wireframe | 22 µs | 2.36 ms (14%) | 12,833 B |
| solid | 253 µs | 1.71 ms (10%) | 11,748 B |
| shaded | 1292 µs | 2.28 ms (14%) | 16,877 B |
| shaded + AO | 1390 µs | 2.36 ms (14%) | 16,379 B |
| shaded + AO + glyph edges | 1431 µs | 2.45 ms (15%) | 16,740 B |

`solid` remains the cheapest by both measures. The AO and edge passes are each a single linear scan of the depth
buffer and cost nothing measurable; the whole difference is per-pixel shading. All three fit comfortably.

#### Lighting: what `c_ascii_render` actually does differently (2026-08-10)

Prompted by `reference/projects/c_ascii_render-main`, a C ray-marching ASCII renderer whose lighting looks markedly
richer. Worth recording what the difference really is, because the obvious answer is wrong.

**It is not ray marching.** That project marches exactly one SDF (`sdf_cube` is the only one in `sdf.c`); the busy
scene around it — pyramids, rain, equaliser bars — is 2D procedural glyph work in `render_environment_background`
with no geometry behind it. What makes its *cube* look better than our facets is two things, neither tied to SDFs:

1. **A point light, not a directional one** (`to_light = light.position - hit_point`). The direction to a nearby
   lamp changes across a surface, so a flat face picks up a genuine gradient. Ours was directional.
2. **Shading evaluated per pixel, not per triangle.** Together with (1) this is decisive, and the reason is worth
   stating plainly: with a face normal and a light at infinity, `N·L` is **constant across a face**, so a box face
   is mathematically one colour — no number of shade levels can produce a gradient. It was never a resolution
   problem.

Both are now implemented as `LightingMode.Point` (the default), toggleable against `LightingMode.Flat` with `l`.
Per-pixel world position comes from perspective-correct barycentric interpolation (interpolate `world/z`, divide by
the interpolated `1/z`), and the depth test moved *before* shading so hidden pixels cost only the compare.

**Measured cost**, 200×50, 7 bodies, orbiting camera:

| | scene draw | total | ANSI |
|---|---|---|---|
| flat (per-face, directional) | 319 µs | 1.74 ms (10%) | 11,759 B |
| point (per-pixel, + specular) | 1186 µs | 2.24 ms (13%) | 14,955 B |

Shading per pixel costs **3.7× the scene-draw time** and **+27% ANSI bytes** — the byte increase being exactly the
predicted trade, since a gradient makes more cells differ from frame to frame. Quantising to 5 levels is what keeps
it to +27% rather than several times; distinct fg/bg pairs went 22 → 44. At 13% of a 60 fps frame it is affordable,
and the times here are noisy while the byte counts are exact (they reproduce to within one byte across runs).

**Still absent versus theirs, and these ones genuinely do want the SDF:** soft shadows (march toward the light,
accumulate `min(shadow, k·dist/t)`) and ambient occlusion (a few samples along the normal). A rasteriser needs a
shadow map and an SSAO-style pass for the equivalent.

#### Silhouettes and creases (2026-08-10)

Their `detect_edge` asks whether a hit lies near two or more of a **box's** local boundaries — an SDF test tied to
one primitive, which does not generalise to arbitrary meshes. The depth buffer offers something better.

**The detector is the second difference of the inverse-depth field**, and it is exact rather than approximate:
`1/z` is *linear in screen space* across any planar surface — the very property that lets the rasteriser
interpolate it with barycentrics. So on a plane the second difference is **identically zero**, however steeply that
plane recedes, and it goes non-zero in exactly two places: a **crease** where two differently-oriented planes meet
(a box edge), and a **silhouette** where depth jumps to whatever lies behind. That is the same set their
box-specific test finds, with no knowledge of what is being drawn.

This matters because the obvious detector cannot work here: a plain "do neighbouring depths differ" test fails on
ground seen near the horizon, where adjacent rows legitimately differ enormously, so any threshold either paints
the whole far plane or misses close-up edges. Measured on the real scene: **0 of 3,556 wholly-planar sub-pixels**
register as edges, while 57 sub-pixels of body creases and silhouettes do.

Two presentations, cycled with `e` (`SilhouetteStyle`):

- **`Glyph`** — the `◆◇◈◊◌` ramp, as theirs. Note the trade this carries *here* and not there: a glyph has one
  foreground and one background, so an edge cell **gives up its two independent sub-pixels** and the outline lands
  on a full-cell boundary instead of a half-cell one. Free for a renderer sampling once per cell; a real cost at
  double vertical resolution.
- **`Ink`** — darken the edge sub-pixels instead, keeping full resolution.

**Measured cost**, 200×50, point lighting, orbiting camera — the detection pass is a single linear scan of the
depth buffer and does not move the scene-draw time at all (1176 → 1179 µs):

| | ANSI | vs no edges |
|---|---|---|
| point, no edges | 14,955 B | — |
| point + glyph | 14,810 B | **−1%** |
| point + ink | 15,937 B | +7% |

Edge glyphs are effectively free, and marginally *cheaper*: substituting a glyph collapses an edge cell's two
colours into a canonically ordered (brighter, darker) pair, which is more stable frame to frame than two sub-pixels
that can swap. Ink costs a little because darkening creates additional distinct colour values.

**Verification.** The ASCII luminance dump caught the winding bug, but the proof is numeric: for sub-pixels showing
bare ground, cast that pixel's ray, intersect `y = 0` analytically, and compare against the z-buffer —
**0.00% error over 10 pixels**, which exercises the projection, the screen mapping and the perspective-correct
reciprocal-depth interpolation together. The harness also asserts the shade ramp stays quantised (22 distinct fg/bg
pairs, not hundreds); if that ever regressed, the byte count would follow it into the expensive column.

**M2.5 — loaded meshes.** OBJ loading, a generated torus knot, and mesh bodies you can spawn, grab and throw.

### M2.5 result — grabbable teapots (2026-08-10)

`ObjLoader.cs` (geometry-only Wavefront reader), `Meshes.TorusKnot()`, a mesh registry, `BodyShape.Mesh` +
`SceneSnapshot.MeshIds`, `PhysicsScene.AddMeshBody`, and mesh drawing in all three renderers. `m` cycles the loaded
meshes, `b` now cycles box → sphere → mesh. Any `.obj` on the command line is registered at startup.

**The constraint that shaped the design: `Body.AddMesh` requires a *static* body.** A triangle mesh cannot be a
dynamic rigid body in Box3D at all, so a spawned model **renders as its triangles and collides as its convex hull**
(`ConvexHull.FromPoints` with a 32-vertex budget). That is the standard games arrangement rather than a shortcut,
and the visible consequence is that concavities are solid to the solver — nothing falls through the torus knot's
hole and a teapot's handle will not catch. Approximating a concave shape properly needs a compound of hulls
(`CompoundBuilder.AddHull`), which is a separate piece of work.

**A generated torus knot ships alongside the loader**, deliberately. It gives the renderers geometry with real
curvature and self-occlusion to be compared on — uniform boxes and spheres flatter every renderer equally — and it
means the mesh path is exercisable with no third-party asset and no licensing question attached. Model *files*
are a separate question from voxcii's MIT code licence, so nothing is vendored.

**Two wrong assumptions, both caught by testing rather than by reading:**

1. **The reference models are already triangulated.** All of teapot/bunny/cow are pure 3-gons, so loading one
   exercises the n-gon path not at all — the test asserting "more triangles than faces" failed because its premise
   was wrong, not the loader. Fan triangulation now has a synthetic case (a quad and a pentagon → 2 and 3
   triangles), as do the `v`, `v/vt`, `v//vn`, `v/vt/vn` and negative-index face forms.
2. **"The mesh body never falls"** was the test dropping it at the origin, straight into the box tower, which held
   it up. Spawning it clear of the stack, it falls from y=7.44 to y=0.35 and sleeps.

**Wireframe is the weak renderer for meshes, and this is a real limitation rather than a tuning problem.** A box is
exactly 12 edges and a sphere exactly one circle, so the wireframe's representation of a primitive is *exact*. A
dense mesh has no cheap exact wireframe: a 6,320-triangle teapot has ~9,500 unique edges, and a body is only ~30
cells (60 braille sub-pixels) across. The first cap of 400 rendered as a solid scribble; 64 reads as a sparse wire
cloud — better, but still a subset of edges rather than a silhouette, so it never quite reads as the object. The
principled fix is to draw the **convex hull's** edges, which is a genuine shape at ~30–60 edges, but Box3D's
`ConvexHull` does not expose its geometry so that means writing a hull ourselves. Left as a known limitation; the
solid and shaded renderers handle meshes well, and they are the ones meshes are for.

**Cost** with two meshes plus a sphere (200×50, orbiting camera):

| | scene | total | ANSI |
|---|---|---|---|
| wireframe | 81 µs | 2.20 ms (13%) | 12,929 B |
| solid | 769 µs | 2.38 ms (14%) | 12,028 B |
| shaded + AO + glyph edges | 1797 µs | 2.93 ms (18%) | 17,452 B |

A 6,320-triangle teapot and a 2,400-triangle knot cost the solid renderer ~3× what the primitive scene did
(253 → 769 µs) and still leave everything under a fifth of a 60 fps frame.

### M2.5b — the `obj` model viewer and affine transforms (2026-08-10)

A loaded model at sandbox scale is a few dozen cells across, where a teapot and a rock look identical. The `obj`
verb opens a second scene — one asset filling the viewport on a turntable — which is the only size at which the
loader and the renderers can be judged. `System.CommandLine` provides the verb; `[`/`]` step models, `xyz`/`XYZ`
scale per axis, `,.` and `;'` shear, `0` resets, `p` stops the spin.

`ModelScene : ISceneSource` is a static scene with no physics. `SceneView` now takes an `ISceneSource` for reading
and treats the simulation as optional (`source as PhysicsRunner`), so spawn/grab/delete quietly do nothing when
there is nothing to act on — one view, two scenes, no duplicated camera or input code.

**Affine transforms cost almost nothing to render, and that is a property of a decision made back in M2.** The
rasteriser derives each face normal from the **world-space winding of the already-transformed triangle**, not by
transforming a stored normal — so shear and non-uniform scale come out correctly lit with **no inverse-transpose**,
which the usual arrangement would need. Verified in the render: a sheared teapot has no black facets and no
inverted shading. `SceneSnapshot.LocalTransforms` (a nullable `Matrix4x4[]`) carries the map, since a quaternion
cannot express a shear; it is null for the physics scene, which pays nothing for it.

**Transforms stay out of the sandbox on purpose.** Box3D has no sheared collision shape, so a sheared rigid body
would render one way and collide another — the same render/collide divergence that mesh bodies already carry for
an unavoidable reason, which would be gratuitous here. Non-uniform *scale* would be supportable for boxes (Box3D
boxes already take half-extents) but not for spheres.

Two CLI details worth keeping. The sandbox's model list is an **option** (`--model`), not a positional argument: a
positional on the root command is inherited by the `obj` subcommand — its help listed `<models>` twice — and makes
`app foo.obj` versus `app obj` ambiguous to parse.

And `obj` takes **one** path, which may be a file or a directory; either way the whole directory is loaded and
`[`/`]` cycle it, with a named file only deciding where cycling starts. The useful unit for a viewer is the
directory — one you have to restart to see the next asset is a worse tool than one you step through with a keypress.
The rules live in `ModelLibrary.Resolve` rather than in `Program` so they can be tested without launching a UI;
every branch is an edge case (file vs directory vs neither, empty directory, no argument at all, and the
case-insensitive match that finds the start index).

Loading is **eager**, measured rather than assumed: the four reference models parse in ~750 ms total, of which the
250k-triangle dragon is 608 ms. Parsing on first display would move that cost into the middle of cycling, which is
the worse place for it — a startup pause reads as loading, a mid-interaction stall reads as a hang.

**M3 — UI.** Sidebar panels (mode, params, spawn, inspector showing the selected body's mass/velocity/sleep
state), footer key hints, F1 help via `HelpInfo`.

### M3 result — the UI, and what it demanded from the library (2026-08-10)

**The scope grew, deliberately, and the reason is the point of the milestone.** Inertia's sidebar is a readout;
this one is a control surface. Every action that had a key binding now also has a mouse route, because the argument
the demo is making is not "a terminal can draw 3D" — it is that a 60 fps z-buffered viewport is **not a special
case**: ordinary controls sit beside it, on the same UI thread, in the same layout, and neither gives anything up.
A sidebar you can only read does not make that argument.

Shipped: `SandboxShell` (assembles both scenes), `SidebarPanel` and `ModelSidebarPanel`, `SceneMenu`,
`SandboxParameters`, plus `PhysicsScene.ApplyParameters`. The shell is
`DockPanel(Top, menu, DockPanel(Bottom, footer, DockPanel(Right, sidebar, view)))` — nested docks, never a `Grid`.

**Cost, and the result that matters** (200×50, 11 bodies, orbiting camera; bytes exact, times median of 120):

| | scene | paint | emit | total | ANSI, with shell | ANSI, viewport alone (M2.5) |
|---|---|---|---|---|---|---|
| wireframe | 18 µs | 363 µs | 898 µs | 1.28 ms (8%) | **11,598 B** | 12,929 B |
| solid | 305 µs | 117 µs | 846 µs | 1.27 ms (8%) | **11,818 B** | 12,028 B |
| shaded | 1199 µs | 132 µs | 957 µs | 2.29 ms (14%) | **14,738 B** | 17,452 B |

**A menu bar, a 32-column sidebar of live widgets and a two-line footer make the frame *cheaper*, not dearer.**
Every renderer emits fewer bytes than the bare viewport did. The reason is M0.1's model playing out again: the
sidebar replaces 32 columns of continuously-changing 3D with static, high-coherence text that mostly does not
change between frames, and the compositor emits nothing for it. The widgets cost real paint time only when a value
moves. Whatever the objection to putting UI beside a 3D viewport is, it is not the frame budget.

**Keys and widgets agree because neither knows about the other.** The state objects own the truth and raise a
change event; a widget writes to the state, and the panel reads it back. A `syncing` flag makes a widget's own
change handler inert while the panel is writing into it, which is what stops the round trip looping. Both
directions are asserted headlessly, and the second one against *what is on screen* rather than the object: set a
parameter, then parse the value back out of the slider's rendered readout.

**Four library gaps, all closed, all found by building this:**

1. **No draggable value control.** `Gauge` and `ProgressBar` are display-only. Added `Slider` (+ `SliderStyle`,
   `IStyleTheme.Slider`, `IGlyphTheme.SliderThumb`), with docs, an example and 16 tests.
2. **`MenuBar` items were fixed at `Add`.** `MenuItem` is immutable, so a checkmark against the active renderer
   could never update. Added `MenuBar.Add(title, Func<MenuItem[]>)`, rebuilt at open time, and
   `MenuItem.Checked` (a `bool?` — `null` means "not that kind of item", which is what lets a level reserve the
   marker column only when it has one).
3. **`Tree` could not be populated lazily** — no expand event, so `IdeDemo` walks whole directories eagerly and a
   drive-rooted tree was not viable. Added `Tree.NodeExpanding` and made `Tree.SelectNode` public.
4. **`Select` options were fixed for life.** A drop-down over a list that changes at runtime (models loaded from
   the new file browser) had no way to update. Added `Select.SetOptions`.

And one addition to the test surface: **`ConsoleSnapshot.Drag`**, which honours mouse capture the way the live path
does. Without it a drag test silently retargets whatever cell it passes over and passes for the wrong reason.

**Two traps worth recording.**

*Judging from a PNG changed the Slider's design.* The first implementation drew the thumb with the eighth-block
sub-cell ramp, exactly as `ProgressBar` draws its fill edge, and every test passed. The image showed why that is
wrong: a sub-cell marker is one eighth of a cell wide at some values and a full cell at others, so a stack of
sliders reads as a ragged bar chart rather than a row of controls you can grab. The thumb is now a whole cell.
Nothing is lost — only the *drawing* quantises; the value stays continuous.

*A control cannot scroll to a row it has just made reachable.* The file browser first rooted its tree at the
machine's drives and revealed the current path. Expanding the chain is what makes the tree tall enough to scroll,
but a frame clamps its scroll offset against the content height it last **measured** — so a scroll issued in the
same layout pass is dropped to zero, and the next pass computes the row from the layout before the expansion. Two
wrong answers, both silent. (A self-inflicted variant came first: an explicit `Height` on the tree pins the control
to that many rows, so the frame's content can never exceed its viewport and the pane will not scroll at all.) The
fix was to stop needing it — see below.

### M3.5 — `FileBrowser`, and the tree that roots itself where you are

The demo chose its models with a command-line argument and could not change them once running, which is the wrong
shape for a browser. `FileBrowser` (library) is a two-pane modal chooser: a lazily-populated directory `Tree`, the
listing in a `ListBox`, a path field and a filter drop-down, with `OpenFile`/`OpenDirectory` helpers that wrap it in
a `Dialog`. `Scene ▸ Load model…` registers an `.obj` at runtime and points the spawn keys at it; `Model ▸ Open…`
re-resolves a whole directory through the existing `ModelLibrary.Resolve`.

**The tree is rooted at the directory being listed, not at the machine's drives**, and that is a design decision
rather than a retreat from the scrolling problem above. A drive-rooted tree spends its 26 columns on forty sibling
folders you did not ask about, needs deep-path reveal machinery to be useful at all, and puts unreadable truncated
names in a narrow pane. Rooted at the current directory it is a drill-down of where you are, always in view, and
the `..` row and the path field are what "somewhere else" means. Re-rooting on every navigation also removes the
reveal, the scroll-to and the layout race in one go.

Enumeration is guarded everywhere: `Directory.GetDirectories(@"C:\")` throws on `System Volume Information`, so an
unreadable directory shows a message in the pane instead of throwing out of a paint.

#### The bug the first real run found, which was in `Dialog` all along

Reported as "clicking OK does nothing, and neither does Cancel". It was **two** faults stacked, and the first one
hid the second.

1. **`Dialog` treated any loss of focus as a dismissal.** Clicking a field inside a dialog moves focus to that
   child — a nested composite is its own focus unit, so the dialog itself stops being the focused control — and the
   lost-focus handler completed the dialog with its cancel result on the **first click inside it**. `_completed`
   then swallowed every button, so the dialog sat on screen with OK and Cancel both dead. It had never shown up
   because `Dialog.Message`/`Confirm` have no focusable content, and the existing tests drive the buttons by
   keyboard. The handler now asks the overlay whether it still holds this dialog, which is what it actually wanted
   to know.
2. **`ListBox` commits on a single click.** With (1) fixed, clicking a file in the browser immediately closed the
   dialog and loaded it — you could not look at a listing without opening something. That is right for a list of
   actions and wrong for a chooser, so `ListBox.CommitOnClick` (default `true`, preserving every existing consumer)
   opts out and the browser sets it: a click selects, a double-click or Enter commits.

Worth noting the shape of this: **every test passed, and a headless click test passed too.** What found it was
running the app. The regression tests now cover both — a click inside dialog content, and select-versus-activate in
the browser.

#### The sidebar spaces itself, or doesn't

Also from running it: the panels stacked flush read as one undifferentiated slab, and it is genuinely hard to tell
which slider track belongs to which label. Every interactive control now has a blank row under it (readout text
stays flush — it is one block, not a set of things to aim at), which costs 10 rows and takes the sidebar to 40.

That does not fit a 40-row terminal, and the failure mode is bad: the Inspector, with Delete and Clear, is clipped
off the bottom and reads as not existing. So `SidebarPanel` switches layout on its own — spaced at
`ActualHeight >= 40`, compact below it — rebuilt from `Control_OnInitialization` and **guarded on the mode
changing, not on the height**, since that override runs on every re-layout and calling `SetContent` from inside one
that changed nothing is how a layout starts chasing its own tail. The demo now asks for 44 rows so a fresh window
gets the spaced form. Verified both ways from PNGs at 120×44 and 100×36.

**A Camera pad** closes the last thing the mouse could not reach: a four-way orbit pad, zoom, and reset, in a panel
under the others in both sidebars. One click is ~15° rather than the arrow keys' 4.6° — a key is held or tapped
repeatedly, a button is clicked, and eighty clicks for a turn is not a control. **Each button hands focus back to
the viewport**, which is the part that matters: leaving focus on the button means the arrow keys stop orbiting the
moment you nudge it once with the mouse, a dead end the harness now checks for explicitly.

**A fifth library gap, from the same look:** `Select` computed a preferred width from its options and used it for
the **pop-up**, while the closed control padded to whatever width the layout offered — so the two never matched,
and a three-word choice was a full-width block of colour next to a slider. Filling the column is right for a form
and wrong for a narrow panel of mixed controls, so it is now `Select.FitContent` (default `false`, so no existing
consumer moves) and the sidebars opt in.

That immediately surfaced a second, more general trap, again only visible by using it: after choosing an option the
control sprang back to full width. Not a width bug — **the themed focus cue fills a control's *unpainted* cells**,
and for a fitted `Select` that is the whole rest of the row, so handing focus back on a choice repainted it edge to
edge. Any control that deliberately paints less than the width it is offered has to set `RendersOwnFocus` and draw
its own cue. Measured before and after: 30 painted cells of a 30-wide row when focused, 12 either way now.

**M4 — polish.** Colour modes (velocity / mass / sleep), motion trails, scene presets: stack, tower, pyramid,
domino run, wrecking ball.

**M5 — ship.** Snapshot tests, README, gallery entry, Docker target beside `audio-scope`.

## Expected library gaps

Predicted, so they can be recognised rather than worked around silently. **M3 found four the list did not
predict** — no draggable value control, fixed `MenuBar` items, no `Tree` expand event, fixed `Select` options —
all now closed; see the M3 result above.

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
