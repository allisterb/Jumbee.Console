# 3D Rendering in a Terminal

This is an AI-generated doc about the 3D graphics implementation in Jumbee.Console. It doesn't assume any knowledge' 
about 3D concepts or things like rasterisation or shading or physics engines.

Everything here is implemented in [`examples/Jumbee.Console.3DSandboxDemo`](../examples/Jumbee.Console.3DSandboxDemo)
— a real-time rigid-body sandbox and a model viewer, both drawn in a terminal at 60 fps. The code is about 3,500
lines in total.


## Preamble
Two claims are worth setting up front, because they are the surprising ones:

1. **A terminal cell can hold two independently coloured pixels**, not one — so the effective resolution is twice
   what the character grid suggests. See [The surface: two pixels per character](#the-surface-two-pixels-per-character) section for how that works.
2. **What costs you in a terminal is not what costs you on a GPU.** Filling pixels is nearly free; *describing* the
   result to the terminal is the expensive part, and that cost depends on how much neighbouring cells differ. This
   inverts several pieces of standard graphics advice, and it shows up in almost every design decision below.

---

## Part 1 — The vocabulary

If you already know what a rasteriser and a z-buffer are, skip to [Part 2](#part-2--the-pipeline-end-to-end).

### Renderer vs rasteriser

These get used interchangeably and they are not the same thing.

A **renderer** is the whole subsystem that turns "here is a scene" into "here are the pixels". It owns the camera,
decides what is visible, works out what colour things are, and writes the output.

A **rasteriser** is one *strategy* inside a renderer: the step that takes a triangle described by three 2D points
and figures out which pixels it covers. "Rasterise" literally means "convert to a raster" — to a grid.

The distinction matters because rasterisation is not the only strategy. The main alternative is **ray tracing** (or
ray marching), which works backwards: for each pixel, fire a ray into the scene and find what it hits. Ray tracing
handles shadows, reflections and transparency naturally, because you can just fire more rays. Rasterisation handles
none of those naturally but is enormously faster for ordinary opaque geometry, because it touches each triangle
once instead of testing every ray against everything.

This project rasterises. Two of its three renderers share one rasteriser and differ only in what colour they decide
each pixel should be.

> **An aside on a wrong turn.** The shaded renderer here was prompted by a ray-marching ASCII renderer whose
> lighting looked visibly richer, and the obvious conclusion was "ray marching looks better". That was wrong. The
> difference was two things a rasteriser can have just as cheaply — a *point* light instead of a distant one, and
> shading evaluated per pixel instead of per triangle. Both are explained below. What genuinely does belong to ray
> marching is soft shadows and true ambient occlusion, because those fall out of already having a distance
> function.

### Scene, geometry, mesh

The **scene** is what exists: a list of objects, each with a position, an orientation, a size and a colour. It says
nothing about how any of it looks on screen.

**Geometry** is the shape of one object. In practice, almost always a **mesh**: a list of vertices (points in 3D)
and a list of triangles (triples of indices into that vertex list). Triangles, specifically, because a triangle is
the only polygon guaranteed to be flat and convex — which makes every operation below simple and total.

```csharp
public sealed class Mesh
{
    public Vector3[] Vertices { get; }   // points, in the model's own local space
    public int[] Indices { get; }        // three per triangle, into Vertices
}
```

A mesh lives in **local space**, centred on its own origin — a cube's vertices are at ±0.5 regardless of where the
cube is in the world. Placing it is a separate step.

### Spaces, and the journey between them

A vertex passes through four coordinate systems on its way to the screen. This is the backbone of the whole
pipeline, and every renderer does it in the same order:

| Space | Origin | What it is |
|---|---|---|
| **Local** | the model's own centre | the mesh as authored |
| **World** | the scene's origin | after placing, rotating and scaling the object |
| **Camera** (view) | the eye | rotated so the camera looks down +Z |
| **Screen** (NDC, then cells) | the middle of the viewport | flattened to 2D by the projection |

Local → world is a transform per object. World → camera is a transform per frame. Camera → screen is the
projection. That is the entire vertex path.

### Projection: how 3D becomes 2D

The trick that makes perspective work is embarrassingly simple: **divide by depth**. Things twice as far away
appear half as big.

```csharp
x_screen = focal * x_camera / z_camera;
y_screen = focal * y_camera / z_camera;
```

`focal` comes from the field of view — how wide an angle the camera takes in. A narrow field of view is a telephoto
lens (things look flat and compressed); a wide one is a fish-eye. Here:

```csharp
Focal = 1f / MathF.Tan(fovYDegrees * (MathF.PI / 180f) / 2f);   // 60° → ≈1.73
```

There is one guard. Points at or behind the eye have `z ≤ 0`, and dividing by that produces garbage — a point just
behind the camera projects to somewhere wildly off-screen *in the wrong direction*. So anything nearer than a
**near plane** is rejected outright:

```csharp
if (view.Z <= Near) return false;   // Near = 0.1
```

The result is **normalized device coordinates** (NDC): x in [-1, 1], y in a similar range, centre of screen at
(0, 0). Mapping NDC to actual cells is the last step, and in a terminal it has a wrinkle — see
[cell aspect](#the-cell-aspect-problem).

### Depth: the z-buffer, and the alternative

Two objects project onto the same pixel. Which one do you draw?

**Painter's algorithm** — sort objects far-to-near and draw in that order, letting near ones paint over far ones.
It is cheap (one sort per frame) and it is *wrong* whenever objects interpenetrate or overlap cyclically, because
"which object is in front" is not a property of the object; it is a property of the pixel.

**Z-buffer** — keep a depth value per pixel alongside the colour. Before writing a pixel, compare: if what you are
about to draw is further than what is already there, skip it.

```csharp
public void TestAndSet(int x, int y, float inverseDepth, CColor c, byte group = 0)
{
    if ((uint)x >= (uint)PixelWidth || (uint)y >= (uint)PixelHeight) return;
    var i = (y * PixelWidth) + x;
    if (inverseDepth <= depth[i]) return;   // something nearer is already here
    depth[i] = inverseDepth;
    color[i] = c;
    this.group[i] = group;                  // scenery or body — the edge pass only outlines bodies
}
```

This is per-pixel and therefore always correct, at the cost of one float per pixel and one compare per write. It
also means triangles can be drawn in *any* order, which removes the sort entirely.

Note `inverseDepth` — the buffer stores **1/z**, not z. That is not a micro-optimisation; it is required, and
[Part 2](#interpolation-and-why-1z) explains why.

This project uses both: the wireframe renderer sorts (it is drawing lines, which have no interior to test), the two
solid renderers use a z-buffer.

### Shading and lighting

**Shading** is deciding what colour a surface is. **Lighting** is the part of that which depends on where the
lights are.

The workhorse is **Lambert's cosine law**: a surface looks brightest when it faces the light head-on, and fades to
nothing as it turns away, following the cosine of the angle between them. Since both are unit vectors, the cosine
is just a dot product:

```csharp
var lambert = MathF.Max(0f, Vector3.Dot(normal, lightDirection));
```

A **normal** is the unit vector pointing straight out of a surface. For a triangle, it is the normalised cross
product of two of its edges. Which way it points depends on the order the vertices are listed in — the **winding**
— which is also how you tell the front of a surface from the back.

Two kinds of light:

- A **directional light** is infinitely far away, so its direction is the same everywhere (the sun). One vector,
  constant for the whole scene.
- A **point light** is at a position, so the direction to it *changes across a surface*, and it gets dimmer with
  distance (**attenuation**).

That difference has a consequence worth pausing on, because it is the single most important shading fact in this
project:

> With a **flat face normal** and a **directional light**, `N·L` is the same at every point on that face. The face
> is mathematically one colour. No number of shade levels, no amount of resolution, and no tuning changes that.

So a flat-shaded object lit by a distant light looks like folded paper — which is often exactly what you want, and
sometimes is not. Getting a gradient *across* a face requires the light direction to vary, which means a point
light, which is only visible if you evaluate the shading per *pixel* rather than per *triangle*. Both changes are
needed; either alone buys nothing.

**Specular** is the other half of a lighting model: the tight bright spot where a surface reflects the light more
or less straight at your eye. It is what makes something look glossy rather than chalky. Cheap version:

```csharp
var half = Vector3.Normalize(lightDir + toEye);           // halfway between light and viewer
var spec = MathF.Pow(Math.Max(0, Vector3.Dot(normal, half)), power) * strength;
```

**Ambient** is the fudge that stops unlit faces going pure black. Real indirect light bounces off everything; a
simple renderer approximates all of that with a constant floor.

### Edges: silhouettes and creases

An **edge** in a rendered image is one of two things:

- a **silhouette**, where an object ends and something much further away begins;
- a **crease**, where two differently-angled flat surfaces meet — a box corner.

Drawing them explicitly (a technique broadly called *toon* or *edge* rendering) makes shapes far more legible,
which matters enormously at terminal resolution where an object might be thirty cells across.

The naive detector — "do neighbouring pixels differ a lot in depth?" — does not work. Look at a floor stretching to
the horizon: adjacent pixel rows there differ *enormously* in depth, so any threshold either paints the whole far
plane as edges or misses real edges close up. [Part 3](#detecting-edges-exactly) shows the detector that does work,
and why it is exact rather than approximate.

### Ambient occlusion

**Ambient occlusion (AO)** darkens creases and crevices — the inside corner where a wall meets a floor, the contact
point where a ball rests on a table. Physically it approximates "how much of the sky can this point actually see":
a point in a corner is shadowed by its own surroundings.

It is disproportionately effective. A scene without it looks like objects floating slightly above the ground; a
little darkening where they touch makes them *sit* there. The version here is a screen-space approximation
(sometimes SSAO) that works from the depth buffer alone — see [Part 3](#contact-shading-ao-on-the-cheap).

---

## Part 2 — The pipeline, end to end

### The surface: two pixels per character

Terminal cells are the fundamental unit, and a cell can carry one glyph, one foreground colour and one background
colour. The trick is to choose the glyph `▀` — UPPER HALF BLOCK. It fills the top half of the cell with the
foreground colour and leaves the bottom half showing the background:

```
┌────────┐
│▀▀▀▀▀▀▀▀│  ← foreground colour
│        │  ← background colour
└────────┘
```

One character, **two independently coloured pixels**. So a viewport of *W × H* cells is a colour buffer of
*W × 2H* pixels.

```csharp
public bool BeginFrame()
{
    var w = ActualWidth;
    var h = ActualHeight * 2;            // two sub-pixels per cell row
    if (w <= 0 || h <= 0) return false;

    if (w != PixelWidth || h != PixelHeight)
    {
        PixelWidth = w;
        PixelHeight = h;
        color = new CColor[w * h];
        depth = new float[w * h];
        group = new byte[w * h];         // scenery (0) vs body (1) — used by the edge pass
    }

    Array.Fill(color, Background);
    Array.Fill(depth, 0f);               // 0 = infinitely far, since depth is a reciprocal
    Array.Clear(group);
    return true;
}
```

Note the size is read **here, in the draw** — never cached from a constructor. Sizes only exist after layout has
run, and they change on every terminal resize.

There is a bonus. A character cell is roughly twice as tall as it is wide, so a half-height sub-pixel comes out
**square**. The grid is isotropic: a circle drawn on it is a circle, not an ellipse. That saves a correction factor
in every piece of geometry code downstream.

Compositing back to characters at the end of the frame is one pass:

```csharp
for (var row = 0; row < rows; row++)
{
    var top = row * 2 * PixelWidth;
    var bottom = top + PixelWidth;
    for (var x = 0; x < PixelWidth; x++)
        consoleBuffer.Write(new Position(x, row),
            new CCharacter('▀', color[top + x], color[bottom + x]));
}
```

### The cell aspect problem

NDC x spans [-1, 1] across the viewport's width. What should y span?

If y also spanned [-1, 1], world units would be squashed or stretched depending on the terminal's shape — a
circle would come out as an ellipse in almost every window. Instead y spans ±*cellAspect*:

```csharp
CellAspect = 2.0 * height / width;   // ×2 because a cell is ~2:1 tall
```

This keeps world units square on screen and letterboxes a wide terminal instead of distorting it.

The important engineering point is that **there is exactly one place this constant lives** (`Viewport`), used by
both the renderer that draws the scene and the code that converts a mouse click back into a world ray. If those two
ever disagreed, clicking would select the object *next to* the one you clicked, and nothing in the code would say
why.

### The camera: an orbit rig

The camera is described by where it looks and where it sits relative to that — spherical coordinates around a
target:

```csharp
public Vector3 Eye => Target + (Distance * new Vector3(
    MathF.Sin(Phi) * MathF.Cos(Theta),
    MathF.Cos(Phi),
    MathF.Sin(Phi) * MathF.Sin(Theta)));
```

`Theta` is the azimuth (drag left/right), `Phi` the elevation from straight up (drag up/down), `Distance` the zoom.
Every 3D editor you have used works this way, because it makes "orbit around the thing I am looking at" the
default gesture.

Turning that into a **view basis** — three perpendicular unit vectors — is four lines:

```csharp
var forward = Vector3.Normalize(Target - eye);
var right   = Vector3.Normalize(Vector3.Cross(forward, WorldUp));
var up      = Vector3.Cross(right, forward);
```

And then world → camera is three dot products, no matrix required:

```csharp
public Vector3 Transform(Vector3 world)
{
    var rel = world - Eye;
    return new Vector3(Vector3.Dot(rel, Right), Vector3.Dot(rel, Up), Vector3.Dot(rel, Forward));
}
```

`Phi` is clamped just short of 0 and π. At exactly those poles, `forward` is parallel to world-up, the cross
product collapses to a zero vector, and the basis becomes degenerate — the view flips or vanishes. Clamping is the
standard fix and it is why every orbit camera you have used refuses to tip quite all the way over.

### Rasterising a triangle

Given three screen-space points, which pixels are inside? The classic answer is **edge functions**.

For each edge of the triangle, an expression that is positive on one side and negative on the other:

```csharp
var w0 = ((b.X - a.X) * (py - a.Y)) - ((b.Y - a.Y) * (px - a.X));
```

If all three are non-negative, the point is inside. Loop over the triangle's bounding box, test each pixel, done.

The three edge functions are more than a test: they *are* the **barycentric coordinates** of the point — the
weights that express it as a blend of the three corners. Which means the same three numbers that told you the pixel
is inside also tell you how to interpolate anything you like across the triangle: depth, colour, world position.

### Backface culling, and a sign that bites

Roughly half the triangles of a closed object face away from you and are hidden by the front half. Skipping them is
free performance and it is also *necessary* for correctness with single-sided geometry.

The test: compute the triangle's signed area in screen space. Its sign tells you the winding, which tells you which
way the face points.

```csharp
var area = ((pb.X - pa.X) * (pc.Y - pa.Y)) - ((pb.Y - pa.Y) * (pc.X - pa.X));
if (area >= 0) return;   // cull
```

> **The trap, recorded because the reflex answer is wrong.** Meshes here are wound counter-clockwise seen from
> outside in world space. But mapping to screen inverts Y — NDC +y is up, while rows count downward — and that flip
> **reverses handedness**. So a *visible* triangle arrives at this test with a **negative** signed area. Culling
> `<= 0`, which is what everybody writes first, discards every visible face and keeps only the hidden ones. Bodies
> then render as their own far side, and single-sided geometry like the ground plane disappears entirely. The
> failure looks like "almost nothing is drawn", not like "the culling is backwards", which is what makes it cost an
> afternoon.

### Interpolation, and why 1/z

Here is the piece that trips up everyone writing their first rasteriser.

Under perspective, **z is not linear in screen space**. Walk across a receding floor one pixel at a time and the
depth does not change by a constant amount per pixel — it changes slowly near you and rapidly toward the horizon.
So interpolating z with barycentric weights is simply wrong, and the error is largest on exactly the big
ground-plane polygons where it is most visible.

But **1/z *is* linear in screen space**. That is a property of the perspective divide, and it is why every hardware
rasteriser stores reciprocal depth.

```csharp
// Exact — no affine-depth warping across large faces.
var inverseDepth = ((w1 * a.Z) + (w2 * b.Z) + (w0 * c.Z)) * inverseArea;
```

Anything else you want to interpolate — world position, for per-pixel lighting — has to be corrected the same way:
divide the attribute by z first, interpolate *that* linearly, then divide by the interpolated 1/z at the end.

```csharp
var wan = wa * a.Z;   // premultiply by 1/z, once per triangle
// ...
var point = (((w1 * wan) + (w2 * wbn) + (w0 * wcn)) * inverseArea) / inverseDepth;
```

This same linearity property is what the [edge detector](#detecting-edges-exactly) exploits, which is a
satisfying place for it to turn up twice.

### Normals under a transform, and the one you get for free

Textbook advice: when you transform an object, you cannot transform its normals with the same matrix. Non-uniform
scale and shear break them, and you need the **inverse transpose** instead.

That advice is correct — and it is entirely avoidable. Instead of transforming stored normals, derive the normal
from the **world-space winding of the already-transformed triangle**:

```csharp
var normal = Vector3.Cross(b - a, c - a);   // a, b, c are already in world space
normal /= normal.Length();
```

This is correct under *any* affine transform, by construction. In the model viewer you can shear a teapot and
stretch it 4× on one axis and it stays correctly lit, with no inverse-transpose anywhere in the codebase.

The trade is that you get **flat** normals — one per face, not smoothly varying across it. For this project that is
not a cost at all, because [quantised flat shading is the thing that makes the output cheap to
emit](#the-cost-model-that-drives-everything).

---

## Part 3 — The three renderers

All three implement one interface and can be swapped live with `v`. The comparison is the point of the demo:
the same scene, drawn three ways, at the same instant.

```csharp
public interface ISceneRenderer
{
    string Name { get; }
    Control Surface { get; }        // the control it draws into
    Projection Projection { get; }
    Viewport Viewport { get; }
    void Draw(SceneSnapshot snapshot, OrbitCamera camera);
}
```

### Wireframe — edges only

Projects each object's edges and draws them as lines on a `Canvas` using **braille** characters, which pack a 2×4
grid of dots into a cell — even finer than the half-block grid, though with one colour per cell rather than two.

Its representation of a primitive is *exact* and almost free:

- A box is 8 corners joined by 12 edges.
- A sphere is **one screen-space circle**, sized by projecting `centre + right * radius` and measuring how far
  that landed from the projected centre. Cheap, and convincing, because a sphere's silhouette genuinely is a circle
  from every angle.

Depth is a painter's sort of whole bodies. Wrong for interpenetrating objects, right almost everywhere else.

**Where it falls down** is dense meshes, and this is a real limitation rather than a tuning problem. A 6,320-triangle
teapot has ~9,500 unique edges, and a body on screen is perhaps 30 cells across. Drawing all of them is a solid
scribble; the current cap of 64 reads as a sparse wire cloud. The principled fix is to draw the *convex hull's*
edges — a genuine shape at 30–60 edges — which needs a hull implementation the physics engine does not expose.

### Solid — flat shaded, one directional light

The cheapest correct renderer. One `N·L` per triangle, quantised, z-buffered.

```csharp
protected override CColor ShadeFace(Vector3 normal, Color tint)
{
    var lambert = MathF.Max(0f, Vector3.Dot(normal, -LightDirection));
    return Quantise(tint, Ambient + ((1f - Ambient) * lambert), ShadeLevels);   // 5 levels
}
```

As established above, each face is necessarily one flat colour. That reads as faceted, which suits a terminal well
— and it produces large uniform regions, which turns out to matter more than it sounds.

### Shaded — point light, per pixel, plus post-processing

Everything the solid renderer will not do:

```csharp
protected override CColor ShadePixel(Vector3 world, Vector3 normal, Color tint)
{
    var toLight  = LightPosition - world;          // a POSITION, so this varies across a face
    var distance = toLight.Length();
    var lightDir = toLight / distance;

    var facing  = Vector3.Dot(normal, lightDir);
    var lambert = WrapLighting ? (facing * 0.5f) + 0.5f : MathF.Max(0f, facing);

    var attenuation = 1f / (1f + (distance * distance / (LightRadius * LightRadius)));

    var specular = 0f;
    if (facing > 0f)   // NB: the raw dot, not the wrapped one
    {
        var half = Vector3.Normalize(lightDir + Vector3.Normalize(View.Eye - world));
        specular = MathF.Pow(MathF.Max(0f, Vector3.Dot(normal, half)), SpecularPower) * SpecularStrength;
    }

    return Quantise(tint, Ambient + ((1f - Ambient) * lambert * attenuation) + (specular * attenuation), 7f);
}
```

Three details are worth extracting.

**Half-lambert wrapping.** `N·L * 0.5 + 0.5` instead of `max(0, N·L)`. Instead of clamping the unlit half to zero,
it maps the full -1..1 range into 0..1. This is a hack with no physical justification, and it earns its place
*specifically because a terminal has so few shade levels*: clamping sends every face turned past perpendicular to
the same flat black, so the entire unlit half of an object collapses into one value and loses its shape. Wrapping
spreads that half across the lower levels. The cost is contrast on the lit side.

**The specular gate.** It tests `facing > 0` — the *raw* dot — not the wrapped value. Wrapping never returns zero
for a merely-turned-away face, so gating on it would put highlights on surfaces pointing into shadow.

**Attenuation radius.** Set to 40 in a scene about 24 units across, which sounds far too generous. It is
deliberate: the point light exists for the *gradient across a face*, not for dramatic falloff. At a tighter radius
the far checkerboard collapsed to near-black and took with it the recession cue that makes the whole thing read as
3D in the first place.

#### Detecting edges exactly

The insight: **1/z is linear in screen space across any planar surface** — the same property the rasteriser relies
on for depth interpolation. So on a plane, the *second difference* of the inverse-depth field is **identically
zero**, however steeply that plane recedes.

It goes non-zero in exactly two places: a crease, where two differently-oriented planes meet, and a silhouette,
where depth jumps to whatever is behind. Which is precisely the set we want.

```csharp
var bendX = MathF.Abs((2f * d) - depth[i - 1] - depth[i + 1]);
var bendY = MathF.Abs((2f * d) - depth[i - PixelWidth] - depth[i + PixelWidth]);
if (MathF.Max(bendX, bendY) <= threshold * d) continue;   // not an edge
```

This is not a heuristic that happens to work — it is exact for planar geometry, which is why it succeeds on the
receding floor where a depth-difference test cannot. Measured on a real scene: **0 of 3,556** wholly-planar
sub-pixels register as edges, while 57 sub-pixels of genuine creases and silhouettes do. (Curved surfaces have a
genuinely non-zero second difference, so a sphere's interior carries a small signal — that is what the threshold
is for.)

Two presentations, and the difference between them is a resolution trade specific to this medium:

- **`Line`** brightens the edge sub-pixels in place, keeping the doubled vertical resolution.
- **`Glyph`** substitutes a `◆◇◈◊◌` glyph. A glyph carries one foreground and one background, so the cell **gives
  up its two independent sub-pixels** and the outline lands on a full-cell boundary. Free for a renderer that
  samples once per cell; a genuine cost at double resolution.

Both *brighten*. That is not arbitrary: a sleeping body is drawn at a third brightness, so an outline that merely
inherited its surface colour came out as the faintest possible mark on a dark background — present in the buffer,
invisible on screen.

#### Contact shading (AO on the cheap)

Real AO asks "how much of the sky can this point see", which needs to sample the surrounding geometry. The
screen-space approximation asks a cheaper question of the depth buffer: **is anything sticking out in front of
where this surface should be?**

The gradient of 1/z is *exact* on a plane (that property again), so extrapolating it predicts precisely where the
surface ought to be at each neighbouring sample. A sample nearer than predicted is something genuinely intruding —
not just the surface receding.

```csharp
var gx = (depth[i + 1] - depth[i - 1]) * 0.5f;   // exact screen-space gradient on a plane
var gy = (depth[i + PixelWidth] - depth[i - PixelWidth]) * 0.5f;

foreach (var (ox, oy) in ContactRing)
{
    var predicted = d + (gx * ox) + (gy * oy);
    if (sample > predicted + (ContactBias * d)) occluded++;
}
```

That distinction — extrapolate, then compare — is what separates a corner from a floor viewed at a grazing angle,
which a naive depth comparison cannot do.

Order matters in the post-process: **darken first, outline second**. Outlining last keeps edges at full brightness
instead of having the contact pass mute the very lines that define the shape.

---

## Part 4 — The cost model that drives everything

This is where terminal rendering stops resembling GPU rendering.

The output is not a framebuffer handed to a display. It is a **stream of ANSI escape sequences** written to a pipe:
"move the cursor here, set the foreground to this RGB, set the background to that RGB, emit these characters". A
run of identical cells costs one colour change and then one byte each. A run where every cell differs costs a full
colour change *per cell* — around 40 bytes.

So the cost is dominated by **how much adjacent cells differ**, not by how many pixels you filled.

Measured at 200×50 (10,000 cells), a synthetic control rewriting its whole area every frame:

| Fill | Frame time | ANSI bytes/frame |
|---|---|---|
| blank | ~0.6 ms | 4 B |
| same value everywhere | ~1.0 ms | 4 B |
| **moving 8-cell bands** | **~1.0–1.6 ms** | **51 KB** |
| every neighbour differs | ~3.1–5.3 ms | 369 KB |

Same cell count, same writes, same paint time — and a **7× difference in bytes**, with a ~3× difference in total
frame time. The split is stark at a maximised window (240×67, 16,080 cells): *writing* every cell costs ~0.3 ms,
while compositing and *emitting* them costs 1.7–8.3 ms depending only on how similar neighbours are. Optimising the
shading loop would be optimising the wrong 5%.

The 369 KB row works out at ~37 bytes per cell — that is one cursor move plus two full RGB colour changes, per
cell, sixty times a second.

Three consequences run through the whole design:

**1. Quantise the shade ramp.** Faces land on one of 5 (solid) or 7 (shaded) brightness levels, so neighbouring
cells share a colour and runs coalesce. This is not a stylistic choice that happens to be fast; it is the single
largest performance lever available, and a smooth gradient would buy nothing visible at this resolution while
moving the renderer into the expensive column.

**2. Flat shading is a feature.** Large uniform regions are exactly what the emitter wants.

**3. The intuition about wireframes is backwards.** A wireframe lights *fewer* cells — but they are scattered
singletons, each needing its own cursor move and colour change. Flat shading covers *every* cell but in long
uniform runs.

Measured on the real scene — 200×50, 7 bodies, camera orbiting every frame, median of 120 frames, all three in one
run so the rows are comparable:

| Renderer | scene | total | ANSI |
|---|---|---|---|
| wireframe | 22 µs | 2.36 ms (14%) | 12,833 B |
| **solid** | 253 µs | **1.71 ms (10%)** | **11,748 B** |
| shaded | 1292 µs | 2.28 ms (14%) | 16,877 B |
| shaded + AO | 1390 µs | 2.36 ms (14%) | 16,379 B |
| shaded + AO + glyph edges | 1431 µs | 2.45 ms (15%) | 16,740 B |

**The solid renderer emits fewer bytes than the wireframe**, despite covering every cell rather than a scatter of
them. All three sit under a fifth of a 60 fps frame, and the AO and edge passes are each a single linear scan of
the depth buffer — they cost essentially nothing.

The two also have genuinely *different* bottlenecks, which is worth knowing when choosing. Measured separately, at
rising body counts, the wireframe's canvas rasterisation goes 420 µs → 1089 µs → 2783 µs for 11 → 50 → 200 bodies
while its emission stays flat around 1.4 ms. So **wireframe is paint-bound and scales with body count**, and
**solid is emission-bound and scales with screen area**.

> **On trusting these numbers.** The byte counts are exact and reproduce to the digit across runs. The times are
> not — the same configuration varied up to 2.3× between runs on the development machine. Quote the bytes; treat
> the microseconds as order-of-magnitude. Deterministic counters beat wall-clock on a noisy desktop.

---

## Part 5 — Physics

Rendering answers "what does the scene look like". Physics answers "where is everything now".

### What a rigid-body engine does

The sandbox uses [Box3D.NET](https://www.nuget.org/packages/Box3D.NET), a C# binding over Erin Catto's Box3D — the
3D successor to Box2D. It speaks `System.Numerics` natively, so there is **no type conversion anywhere** between
the engine and the renderer: a `Vector3` from a physics body goes straight into the vertex transform.

A **rigid body** is an object that does not deform: a position, an orientation, a linear and angular velocity, a
mass, and a **collision shape**. Each step the engine integrates velocities into positions, finds pairs that
overlap, and solves the constraints that push them apart — plus friction, restitution (bounciness) and resting
contacts.

Bodies come in three flavours, and the distinction matters for the grab-and-throw interaction:

| Type | Moved by | Pushed by |
|---|---|---|
| **Static** | nothing | nothing — the ground |
| **Dynamic** | forces and collisions | everything |
| **Kinematic** | you, directly | nothing — but it pushes dynamics aside |

You do not write any of the solver. What you write is the *plumbing*: how the world is stepped, and how its results
reach the screen.

### The fixed timestep

Physics is stepped at a **fixed** 1/60 s regardless of frame rate. This is not laziness — a variable timestep makes
simulation non-deterministic and can make a solver explode when a frame runs long.

The bookkeeping is an **accumulator**: bank the real elapsed time, spend it in fixed-size steps.

```csharp
accumulator += elapsed * TimeScale;
if (accumulator > MaxBacklog) accumulator = MaxBacklog;   // cap BEFORE stepping

while (accumulator >= FixedStepSpan)
{
    scene.Step(FixedStep);
    accumulator -= FixedStepSpan;
    if (stepBatch.Elapsed.TotalMilliseconds >= StepBudgetMs) { accumulator = 0; break; }
}
```

Two guards, both load-bearing:

- **Cap the backlog before stepping.** Without it, one long stall — a debugger break, a laptop resuming from sleep
  — banks minutes of simulation time and the engine tries to spend it all in one frame.
- **Budget the batch.** If a batch overruns, abandon what is left rather than falling further behind. The effect is
  that a scene too heavy to simulate in real time **eases into slow motion** instead of freezing the app.

### Crossing the thread boundary

The physics world lives on its own thread. Every `Body` handle, the body list, and the world itself belong to that
thread and are never touched from outside it. So how does the renderer, on the UI thread, draw them?

**One immutable snapshot per tick**, published by a single reference swap:

```csharp
private void Publish(PhysicsScene scene, double stepMs)
{
    var next = new SceneSnapshot(bodies.Count) { Count = bodies.Count };
    for (var i = 0; i < bodies.Count; i++)
    {
        next.Positions[i]  = bodies[i].Handle.Position;
        next.Rotations[i]  = bodies[i].Handle.Rotation;
        next.Velocities[i] = bodies[i].Handle.LinearVelocity;
        // ...
    }
    Volatile.Write(ref snapshot, next);
}
```

The UI thread reads `Snapshot` whenever it likes and always gets a whole, self-consistent tick — never a half-updated
one. There is **no lock anywhere**.

Mutations travel the other way, as commands queued and drained at the top of a step:

```csharp
public void Post(Action<PhysicsScene> action) => commands.Enqueue(action);
// ...on the physics thread, before stepping:
while (commands.TryDequeue(out var command)) command(scene);
```

So "spawn a box here" is a lambda that runs *on the physics thread with the world in a consistent state*, not a
cross-thread poke at a native handle mid-solve.

The snapshot is a **fresh allocation** each tick rather than a recycled buffer. About 12 KB for 200 bodies — ~0.7
MB/s of gen-0 garbage at 60 Hz, which the collector barely notices — and recycling would hand the renderer an array
being overwritten underneath it.

The snapshot is also **parallel arrays** (`Positions[]`, `Rotations[]`, …) rather than an array of structs. It costs
nothing today and it is the layout SIMD would need later; retrofitting that is the expensive part.

### Selection is by id, not index

`SceneSnapshot.Ids` carries a stable per-body id assigned at spawn. Deleting a body shifts every index after it, so
an index-keyed selection would silently retarget to a different object. This is the kind of bug that survives
testing and confuses users, and the fix costs one array.

### Picking: screen-space, not a raycast

Clicking a body could be done with a physics raycast. It is not. Instead, each body's centre is projected to the
screen and the nearest within a threshold wins:

```csharp
if (!Projection.TryProject(view.Transform(snapshot.Positions[i]), out var x, out var y)) continue;
var d = ((x - nx) * (x - nx)) + ((y - ny) * (y - ny));
```

Deliberately, for one reason: it reads **only the snapshot**, so picking works on the UI thread while the physics
thread is mid-step, with no round trip through the command queue. The cost is that it picks by centre, so clicking
the far corner of a large box can select a small body behind it.

Worth noting the test that guards it: **project a body to a cell, then pick that cell back and assert you get the
same body**. That round trip catches the renderer's bounds and the viewport's un-projection drifting apart, which
is otherwise a silent, maddening off-by-a-bit.

### Dragging: ray, plane, kinematic

Grabbing and throwing a body composes three things already built:

1. **Un-project** the mouse cell into a world-space ray (`Viewport.TryRay` — the exact inverse of the projection).
2. **Intersect** that ray with a plane through the body, facing the camera (`Projection.TryPlaneHit`). A ray has
   infinite depth, so something must pin down *how far along it* the body should sit; a camera-facing plane is the
   convention every 3D editor uses.
3. **Make the body kinematic** and steer it with `MoveTowards`, which gives it a real velocity rather than
   teleporting it — so it shoves the rest of the scene around properly on the way.

The drag target is re-applied **every step**, not once per mouse event: applying it once lets the body arrive and
then overshoot on the following steps.

On release, throw velocity is measured from **where the grab point actually went** over the last 5 samples, capped
at 40 m/s. Using the last mouse delta instead means a flick across a coarse terminal grid implies an absurd speed,
and releasing after a pause should *drop* a body, not fling it.

### A constraint worth knowing

`Body.AddMesh` requires a **static** body — a triangle mesh cannot be a dynamic rigid body in Box3D at all. So a
spawned model **renders as its triangles and collides as its convex hull** (`ConvexHull.FromPoints`, capped at 32
vertices).

That is the standard games arrangement rather than a shortcut, and the visible consequence is that concavities are
solid to the solver: nothing falls through a torus knot's hole and a teapot's handle will not catch on anything.
Approximating a concave shape properly needs a compound of several hulls.

---

## Part 6 — What the trade-offs actually are

| | Wireframe | Solid | Shaded |
|---|---|---|---|
| Surface | `Canvas`, braille 2×4 | half-block 2×1 | half-block 2×1 |
| Colour | one per body | per **triangle** | per **pixel** |
| Light | none | directional | point + specular |
| Depth | painter's sort | z-buffer | z-buffer |
| Post | — | — | AO + silhouettes |
| Bottleneck | **paint**, scales with bodies | **emission**, scales with area | shading |
| Dense meshes | poor | good | good |
| Cost (200×50, 7 bodies) | 1.49 ms / 12.8 KB | **1.62 ms / 11.7 KB** | 2.45 ms / 16.7 KB |

Choosing between them:

- **Many bodies, shape matters more than surface** → wireframe. It scales with body count in a way the solid
  renderers do not, and its representation of primitives is exact.
- **General use** → solid. Cheapest by both measures, reads unambiguously as 3D, and its faceting suits the medium.
- **A single subject you want to look good** → shaded. The gradient across a face and the contact darkening are
  what make an object look like it is *there*, and at 15% of a frame budget you can afford it for one model.

## Things that are absent, and what they would cost

- **Real shadows.** Would need a second depth pass rendered from the light's point of view (a shadow map), then a
  lookup per pixel. Entirely doable; roughly doubles the rasterisation work.
- **Texturing.** Interpolate UV coordinates (perspective-correctly, exactly as world position is handled here) and
  sample an image. The blocker is not the maths — it is that a terminal cell has one colour, so texture detail
  finer than a sub-pixel simply cannot be shown.
- **Smooth (Phong) normals.** Store per-vertex normals and interpolate them. Would remove the faceting — and would
  also destroy the shade quantisation that makes the output cheap to emit. A deliberate omission, not an oversight.
- **Anti-aliasing.** The half-block trick is already a form of vertical supersampling. Going further means blending
  colours between cells, which increases how much neighbours differ — directly the expensive direction.
- **Near-plane clipping.** Triangles with any corner behind the camera are dropped rather than split. Cheap, and at
  the distances an orbit camera holds it costs a sliver of geometry at the very edge of the view.

## Where to look in the code

| File | What it holds |
|---|---|
| `Camera.cs` | `OrbitCamera`, `CameraView`, `Viewport`, `Projection` — all the spaces and transforms |
| `HalfBlockSurface.cs` | the sub-pixel colour + depth buffer, edge detection, contact shading, compositing |
| `MeshRenderer.cs` | the shared rasteriser: transform, cull, edge functions, interpolation |
| `SolidRenderer.cs` / `ShadedRenderer.cs` | the two shading models, and nothing else |
| `WireframeRenderer.cs` | projection to lines on a braille `Canvas` |
| `Meshes.cs` / `ObjLoader.cs` | primitives, a generated torus knot, and a Wavefront OBJ reader |
| `PhysicsRunner.cs` | the physics thread, the fixed-step loop, snapshot publication |
| `SceneSnapshot.cs` | the immutable parallel-array tick handed to the renderer |

Every measurement quoted here, plus the reasoning behind each decision and the bugs met on the way, is recorded
milestone by milestone in [`docs/internal/3D Sandbox Plan.md`](internal/3D%20Sandbox%20Plan.md).

## See also

- [Charts](controls/Charts.md) — `Canvas`, the braille drawing surface the wireframe renderer uses, and `Globe`,
  which uses the same half-block technique for a ray-traced earth.
- [Live Data](controls/Live%20Data.md) — the snapshot-per-tick threading pattern in its general form.
- [Rendering Model](internal/Rendering%20Model.md) — how the library composites and emits, one layer below all of
  the above.
