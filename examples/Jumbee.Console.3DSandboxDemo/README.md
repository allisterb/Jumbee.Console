# Jumbee.Console 3D sandbox

A real-time **3D rigid-body sandbox** in the terminal, and an **OBJ model viewer**, built on `Jumbee.Console`.
Physics comes from [Box3D](https://github.com/erincatto/box3d) (Erin Catto's engine, via the `Box3D.NET` binding);
everything else — camera, projection, rasteriser — is about 900 lines of `System.Numerics` in this project. See [3D Rendering in a Terminal](../../docs/3D%20Rendering%20in%20a%20Terminal.md) for a deep dive into how it works.

Three renderers draw the same scene, cycled live with `v`:

| | how it draws | depth | shading |
|---|---|---|---|
| **wireframe** | projected edges on a `Canvas`, braille 2×4 sub-cells | painter's sort, whole bodies | one colour per body |
| **solid** | z-buffered triangles on half-block `▀` cells | per sub-pixel | flat, per triangle, directional light |
| **shaded** | as solid | per sub-pixel | per **pixel**: point light, specular, silhouettes, contact darkening |

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

> **Loading a directory can pause at startup.** Models are parsed eagerly, before the UI appears, and a large one
> is not fast: the 250,000-triangle Stanford dragon takes ~600 ms on its own, where a 6,000-triangle teapot takes
> ~4 ms. A directory of big models will sit for a moment before anything is drawn. This is deliberate — parsing on
> first display would move that pause into the middle of cycling, and a stall mid-interaction reads as a hang where
> a stall at startup reads as loading. Point `obj` at a single file's directory, or a directory of small models, if
> you would rather not wait.

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
