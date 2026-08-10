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

**M1 — sandbox.** Grid floor, spawn box/sphere at the camera target, launch impulse, grab and drag, delete,
reset, pause and single-step.

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
