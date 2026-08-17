# Jumbee.Console 3D sandbox

A real-time **3D rigid-body sandbox** in the terminal, and an **OBJ model viewer**, built on `Jumbee.Console`.
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

# The model viewer: one asset filling the viewport, on a turntable.
dotnet run --project examples/Jumbee.Console.3DSandboxDemo -c Release -- obj path/to/models

# Make models spawnable in the sandbox instead, so you can throw them at things.
dotnet run --project examples/Jumbee.Console.3DSandboxDemo -c Release -- --model path/to/teapot.obj
```

`obj` takes **one** path, a file *or* a directory. Either way the whole directory is loaded and `[` / `]` cycle
through it — naming a file only decides which one opens first.

With **no path** it looks for a `models` folder in the current directory and loads that; with no such folder it opens
on the generated torus knot. It never loads the working directory itself — picking up whatever `.obj` files happen to
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

## Colour

The viewer's **Colour** drop-down recolours the model, and it is one setting for all three renderers: every one
of them tints through the same palette, so the wireframe's edges and the two shaded renderers' surfaces follow it
together.

Its rows are a swatch in the colour itself beside the name, which is what `Select`'s renderable options exist for —
a text option gets one style for the whole row, so it could not colour the block differently from the label.

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
| drag / arrows | orbit the camera (Shift for fine steps) |
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
