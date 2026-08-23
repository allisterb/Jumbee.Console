# Jumbee.Console 3D sandbox

A real-time **3D rigid-body sandbox** in the terminal, and a **model viewer** for `.obj`, `.stl` and `.ply`, built on `Jumbee.Console`.
Physics comes from [Box3D](https://github.com/erincatto/box3d) (Erin Catto's engine, via the `Box3D.NET` binding);
everything else — camera, projection, rasteriser — is about 900 lines using `System.Numerics` entirely. See [3D Rendering in a Terminal](../../docs/3D%20Rendering%20in%20a%20Terminal.md) for a deep dive into how it works.

Three renderers draw the same scene, cycled live with `v`:

| | how it draws | depth | shading |
|---|---|---|---|
| **wireframe** | projected edges on a `Canvas`, braille 2×4 sub-cells | painter's sort, whole bodies | one colour per body |
| **solid** | z-buffered triangles on half-block `▀` cells | per sub-pixel | flat, per triangle, directional light |
| **shaded** | as solid | per sub-pixel | per **pixel**: point light, specular, silhouettes, ambient occlusion |

The half-block surface gives **twice the terminal's vertical resolution** — each cell carries two independently
coloured sub-pixels — so the solid renderers draw at `width × 2·height` with true colour throughout.

## Run it

```bash
# The sandbox: a leaning tower, some spheres, and a floor.
dotnet run --project examples/Jumbee.Console.3DSandboxDemo -c Release

# The model viewer: one asset filling the viewport, on a turntable. Reads .obj, .stl and .ply.
dotnet run --project examples/Jumbee.Console.3DSandboxDemo -c Release -- obj path/to/models

# Make models spawnable in the sandbox instead, so you can throw them at things.
dotnet run --project examples/Jumbee.Console.3DSandboxDemo -c Release -- --model path/to/part.stl
```

`obj` takes **one** path, a file *or* a directory. Either way the whole directory is loaded and `[` / `]` cycle
through it — naming a file only decides which one opens first.

With **no path** it looks for a `models` folder in the current directory and loads that; with no such folder it opens
on the generated torus knot. It never loads the working directory itself — picking up whatever model files happen to
be sitting next to you is a surprise, and there is always a mesh to fall back on.

Whichever you launch, **Scene ▸ Switch to model viewer** (and **Model ▸ Switch to sandbox** coming back) moves
between the two without leaving the process. Loaded models are kept, so nothing is re-parsed, and the viewer opens on
whichever model you were last looking at — or, coming from the sandbox, on whatever the spawn drop-down was set to.

> **Loading a directory can pause at startup.** Models are parsed eagerly, before the UI appears, and a large one
> is not fast: the 250,000-triangle Stanford dragon takes ~600 ms on its own, where a 6,000-triangle teapot takes
> ~4 ms. A directory of big models will sit for a moment before anything is drawn. This is deliberate — parsing on
> first display would move that pause into the middle of cycling, and a stall mid-interaction reads as a hang where
> a stall at startup reads as loading. Point `obj` at a single file's directory, or a directory of small models, if
> you would rather not wait.

## File formats

| | |
|---|---|
| **`.obj`** | Wavefront. Geometry only — `v` and `f`, with n-gons fan-triangulated. `vt`/`vn` are parsed past, and materials are not read at all. |
| **`.stl`** | Binary and ASCII, auto-detected. The CAD and 3D-printing interchange format, which is the reason it is here. |
| **`.ply`** | ASCII and binary little-endian. **The only one that carries colour**, per vertex or per face, with no side-car file. |

A directory holding several formats is loaded as one list in name order, so `[` and `]` walk the directory rather
than all the OBJs and then all the STLs.

**STL is a triangle soup** — every facet repeats its three corners in full, with no vertex sharing, no materials and
no texture coordinates. The loader welds identical corners as it reads, because the renderer transforms a body's
vertices once per frame and references them from its triangles: the sample bee goes from 7,653 corners to 1,286
vertices for the same 2,551 triangles.

Two details are worth knowing, because they are what most STL readers get wrong:

- **The format is detected by arithmetic, not by the word `solid`.** A binary file's 80-byte header is free-form and
  exporters do write descriptions into it beginning with that word — the usual sniff then reads a perfectly good
  binary file as ASCII and yields nothing. A binary file's length is fully determined by its facet count
  (`84 + n × 50`), so checking that identifies the format exactly.
- **A facet's stored normal decides its winding.** The renderers derive their own normal from the vertex order and
  cull back faces on its sign, and STL files in the wild routinely wind facets against the normal their authoring
  tool recorded. Taken at their winding alone those facets vanish into the cull and the model reads as full of
  holes, so where the two disagree the loader swaps two corners to follow the file's normal.

**STLs are treated as Z-up**, since they come overwhelmingly from tools where Z is up. A default rather than a fact,
and like the OBJ exporter banner it is applied somewhere visible — the viewer's **Z-up file** switch and the `a`
key — so a wrong guess is a setting to flip rather than a broken-looking model.

**PLY is the one format here that states its own colours.** OBJ needs a side-car `.mtl` (and usually a texture with
it, which nothing here can sample) and STL carries nothing at all, so every other model takes a flat palette tint.
A PLY file can colour every facet inline, and the loader reads it whether the file stores colour per vertex or per
face. Per-vertex colours are averaged onto the face, because the renderers shade a whole triangle at once — at a
shade ramp of seven levels across a face a few sub-pixels wide, the average lands on the same quantised colour a
corner-to-corner blend would have produced nearly everywhere.

Three things about the reader are deliberate:

- **Every property is coerced, not assumed.** Positions arrive as `float` or `double`, indices as `int` or `uint`,
  colours as `uchar` 0–255 or `float` 0–1, under `red`/`green`/`blue` or `r`/`g`/`b`. All of those are legal and all
  of them occur, so a reader that handles only `float`/`int` works until it meets a file from another exporter.
- **A truncated file is an error.** A PLY header declares its element counts up front, so a file holding two of the
  two thousand vertices it promised would otherwise yield 1,998 vertices at the origin — a modelling artefact rather
  than a broken file. The loader turns the parser's short-read warning into a failure that names the counts.
- **`binary_big_endian` is refused with a message naming the format**, rather than read as byte-swapped garbage. The
  spec allows it; essentially no current exporter emits it.

**PLY models get no up-axis guess at all**, unlike OBJ and STL — and the absence is a finding rather than a gap.

The obvious move is to copy the OBJ approach and recognise the exporter's signature. It was tried and reverted,
because PLY's volume comes from **3D scanners**, and a scan is stored in the *scanner's* frame at capture time —
a fact about how the operator held the thing, not about the tool. The two reference Artec models settle it: the
Christmas bear and the bus carry the identical `File exported by Artec Group` comment, and the bear is Z-up while
the bus is Y-up. A rule keyed on that banner is right about half the time, which is worse than no rule — an absent
guess is predictable and documented, a coin-flip guess is neither.

Geometry does no better. "The up axis is the longest extent" places the bear correctly and stands the reference cow
on its nose, because a cow is longest along its length; the mirror rule fails the other way. On the bus it cannot
even choose — its Y and Z half-extents are 0.216 and 0.213.

So a PLY opens Y-up and the **`a` key** (the sidebar's **Z-up file** switch) fixes the ones that are wrong. That is
the same "visible and undoable" position the OBJ banner and the STL default take, arrived at from the other side.

## Colour

The viewer's **Colour** drop-down recolours the model, and it is one setting for all three renderers: every one
of them tints through the same palette, so the wireframe's edges and the two shaded renderers' surfaces follow it
together.

Its rows are a swatch in the colour itself beside the name, which is what `Select`'s renderable options exist for —
a text option gets one style for the whole row, so it could not colour the block differently from the label.

**A PLY that carries its own colours ignores the drop-down**, since the file is more specific than the palette.
Selecting the body still overrides both — the selection tint has to win over the whole model or there is no way to
see which one is selected.

## Shaded detail

Everything belonging to the shaded renderer — edge style and the two lighting dials — greyed out under the other
two, which draw none of it.

**Half-Lambert light** (off by default) spreads the unlit half of an object across the low shade levels instead of
collapsing it to black, and pays for that with contrast on the lit side. With only seven quantised levels that is
an expensive trade, and the lit surface is usually what you are looking at. Mean contrast between neighbouring lit
cells on the bunny:

| | contrast |
|---|---:|
| solid | 11.5 |
| shaded, wrap **on** | 9.0 |
| shaded, wrap **off** (default) | 14.3 |

Turn it **on** for a scene where the dark side matters more than the lit one — a sandbox of tumbling bodies under
an orbiting camera, which is the case it was written for. It also emits ~12% fewer ANSI bytes a frame, because
compressing the range makes neighbouring cells land on the same level more often.

**Occlusion** is screen-space ambient occlusion. Per sub-pixel it takes the depth gradient, samples a ring of eight
neighbours, and counts how many sit nearer than a flat surface through that point would predict — so it darkens
creases and where surfaces meet, and leaves a steeply receding flat surface alone. The slider is the strength
multiplier: `1 − strength × occluded/8`, so 1.0 sends a fully-enclosed sub-pixel to black and 0 skips the pass.

It does the opposite of what you might expect: turning it *down* flattens the picture (contrast 9.0 → 7.7 at zero,
and 26 distinct shade levels collapse to 5), because it multiplies the quantised levels by a continuous per-cell
factor and hands back gradation the quantiser threw away.

It is not a shadow — it only knows what is in the depth buffer, so an occluder off-screen or behind the camera
contributes nothing. Real shadows would need a second depth pass from the light.

## The two resolutions, and the two ways to spend on them

A half-block cell carries two colours and two sub-pixels, so there are exactly two things to buy: finer **colour**
and finer **space**. Both dials are live under the solid renderer as well as the shaded one, and both grey out
under the wireframe, which composites no cells.

**Shade Levels** is the colour axis — how many brightness levels the ramp quantises to (7 shaded, 5 solid by default).
Raising it smooths the banding on a curved surface and costs ANSI bytes, because neighbouring cells stop sharing a
colour and the emitter's runs break up.

**Quadrant glyphs** is the space axis. The surface samples twice per column and each 2×2 block is composited
into whichever of the sixteen quadrant glyphs (`▘▝▖▗▌▐▞▚▛▜▙▟▀▄█`) best fits its four colours — so a silhouette
can land *between* two columns instead of only on the boundary between them. `▀` is one of those sixteen and
wins whenever the block's structure really is horizontal, so this only ever adds resolution; it never trades the
vertical resolution away.
The two colours it emits are members of the block, never a blend of them, so the palette is untouched — everything
stays on the quantised ramp.

Measured on a sphere against the sky, the silhouette's placement error halves (0.65 → 0.32 half-cells RMS) and
about half the silhouette rows move to a half-cell boundary, where before none could. It costs roughly half again
as much per frame — at 200×50, shaded 2.9 → 4.4 ms and 18.1 → 25.7 KB of ANSI, solid 1.7 → 2.4 ms and
12.0 → 17.4 KB — because there are twice as many sub-pixels to shade and a boundary inside a cell breaks a run
that used to coalesce. Off by default for that reason.

### The blend that came first, and why it is gone

The switch was called **Anti-Aliasing** for a while, and the name was wrong: nothing here is blended. It buys its
smoothness by subdividing the sample grid and quantising each block better, and every colour it emits is one that
was already in the block. It is named for that now.

There was a second control here that genuinely was antialiasing, a **Smooth** slider. It blended each detected
edge sub-pixel toward its neighbours: no extra sampling, and a cost tracking silhouette *perimeter* rather than
screen *area*, which made it much the cheaper idea. Two things were wrong with it, and both are worth knowing
before reaching for the same trick elsewhere.

It blends **whole** sub-pixels where real antialiasing blends *within* one. On a body a dozen sub-pixels across a
one-sub-pixel ring is ~8% of the diameter per side, so full strength reads as erosion — the body visibly shrinks —
and a low setting merely softens the staircase without moving it. Measured, it makes the placement error slightly
*worse* (0.65 → 0.87 half-cells). And being driven by a depth-based detector, it cannot see a boundary that is only
a change of **colour**: the checkerboard and the shade-band contours, arguably the blockiest things on screen,
stayed exactly as they were.

Run together the two are independent — one rewrites sub-pixels, the other decides how a 2×2 block becomes a cell —
so the honest test was to measure all four combinations. Adding the blend on top of quadrant sampling cost 43% more
distinct fg/bg pairs (277 → 395) and bought nothing measurable (placement 0.32 → 0.35). It was removed rather than
left at a default of zero: two controls where one of them does nothing you can see is worse than one.

## Mesh detail, under the wireframe

The wireframe cannot draw every edge of a loaded model — a 6,300-triangle teapot has ~9,500 — so it draws a sample,
and three dials decide which sample. The viewer puts them in a **Mesh detail** panel; the sandbox folds them into
**Render**.

They configure the wireframe, so they keep their values — and keep showing them — while the solid or shaded
renderer is drawing; those rasterise every triangle and are unaffected. Both the sidebar controls and the Render
menu entries grey out while that is the case.

| control | | cost |
|---|---|---|
| **Even over screen** | Spreads the budget evenly over the *screen* rather than over the model's triangle list. Matters for assets whose detail holds most of the triangles while their flat panels hold most of the area — the reference plane draws its engines as a dense speckle and its wings nearly bare with this off. Makes no difference to an evenly tessellated model. | roughly free at typical sizes; ~25% on a 250k-triangle model |
| **Scan** | How many triangles are examined per frame. Lower is faster and starts leaving regions of a big model undrawn; models smaller than the cap are unaffected by it entirely. | the main lever — 5k roughly halves the frame cost of a large model |
| **Detail** | How much gets drawn per unit of screen area. Higher is denser. | scales the line-drawing work, but not the scan, so it moves less than you would expect on a big model |

Defaults are tuned for the models in `media/models`. Drop **Scan** to `5k` first if you want speed; that is where
the time goes on a dense mesh.

## Keys

**Both scenes**

| key | |
|---|---|
| drag / arrows | orbit the camera (Shift for fine steps) — in the sandbox, a drag that starts *on a body* grabs it instead |
| wheel, PgUp/PgDn | zoom |
| Home | reset the camera |
| `v` | cycle renderer: wireframe → solid → shaded |
| `e` | shaded only: cycle silhouettes — off, ink outline, edge glyphs |
| F1 / `q` | help / quit |

**Sandbox**

| key | |
|---|---|
| click, Tab | select a body; drag a body to grab and throw it |
| `n` / `f` | drop one above the camera target / fire one out of the camera |
| `b` / `m` | cycle spawn shape (box, sphere, mesh) / next loaded mesh |
| `+` `-` / `[` `]` | spawn size / launch speed |
| `x`, Del / `c` | delete the selected body / clear them all |
| space / `.` / `r` | pause / single-step / reset the scene |

**Model viewer**

| key | |
|---|---|
| `[` `]` | previous / next model |
| `x` `y` `z` / `X` `Y` `Z` | shrink / stretch that axis |
| `,` `.` and `;` `'` | shear |
| `0` / `p` | reset the transform / stop the turntable |

## Where the rasteriser runs

**The solid renderers rasterise off the UI thread.** Each tick captures a `FrameRequest` — the published snapshot,
the camera resolved to a value, and the laid-out viewport size — and hands it to a `Control.Job`, which produces the
frame on a background thread and publishes it on the UI thread. `HalfBlockSurface` hands frames over by value for
exactly this reason: the rasteriser fills a buffer the paint path is not reading, and the swap happens on the UI
thread in `Publish`.

It buys **responsiveness, not frame rate**. A dense model costs what it costs; what changes is that the cost lands
somewhere the app is not trying to answer a keypress. Measured on the 800k-triangle reference scan, timing a
round-trip through the UI thread while the viewer runs:

| | UI round-trip (median / worst) |
|---|---:|
| rasterising on the UI thread | **> 5,000 ms** (timed out) |
| rasterising off it | **0.0 ms / 3.8 ms** |

The old number is not merely "one frame of 90 ms". The redraw feed ticks every 16 ms and each tick posted a 90 ms
draw, so work arrived about six times faster than it could be served and the queue grew without bound — the app
stopped answering at all rather than animating slowly. Coalescing is what fixes that: however many requests pile up
during a run, exactly one more run follows, so falling behind costs frames and never responsiveness.

The **wireframe renderer stays on the UI thread** (`ISceneRenderer.DrawsOffThread` is false for it). It draws
straight into a shared `Canvas` rather than handing over a buffer, and at ~6 ms for the same model — the triangle
budget sees to that — it has the least to gain.

## Notes on the physics

A mesh body **renders as its triangles but collides as its convex hull**. That is not a shortcut: Box3D's triangle
mesh shape requires a *static* body, so a triangle mesh cannot be a dynamic rigid body at all. The visible
consequence is that concavities are solid to the solver — nothing drops through the torus knot's hole, and a
teapot's handle will not catch on anything.

Shear and non-uniform scale exist only in the **viewer**, for the same reason in reverse: Box3D has no sheared
collision shape, so a sheared rigid body would render one way and collide another.

Physics runs on its own thread at a fixed 1/60 s with a catch-up budget, publishing one immutable snapshot per
tick; the UI thread only ever reads the newest snapshot and posts changes back. See `docs/controls/Live Data.md`
for the pattern.
