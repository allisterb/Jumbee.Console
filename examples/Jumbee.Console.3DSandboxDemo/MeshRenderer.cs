namespace Jumbee.Console.SandboxDemo;

using System.Numerics;

using CColor = ConsoleGUI.Data.Color;

/// <summary>
/// The shared triangle rasteriser: transforms the scene, culls, z-tests and fills onto a
/// <see cref="HalfBlockSurface"/>. Subclasses supply only the shading and any post-process.
/// </summary>
/// <remarks>
/// <para>
/// Depth is <b>per sub-pixel</b>, not a painter's sort of whole bodies, so bodies interpenetrate correctly and a
/// box in front genuinely occludes the one behind it.
/// </para>
/// <para>
/// Two shading paths, chosen by <see cref="ShadesPerPixel"/>: evaluate once per triangle (cheapest, and all a
/// directional light can offer — see <see cref="SolidRenderer"/>) or once per covered sub-pixel (needed by
/// anything positional, see <see cref="ShadedRenderer"/>). The per-pixel path costs a perspective-correct
/// interpolation of world position and defers the shade until after the depth test.
/// </para>
/// </remarks>
public abstract class MeshRenderer : ISceneRenderer
{
    #region Constructors
    /// <summary>Initialises the shared rasteriser with the shade-ramp resolution its subclass wants by default.</summary>
    protected MeshRenderer(float shadeLevels) => ShadeLevels = shadeLevels;
    #endregion

    #region Properties
    /// <inheritdoc/>
    public abstract string Name { get; }

    /// <inheritdoc/>
    public Control Surface => surface;

    /// <inheritdoc/>
    public Projection Projection { get; } = new(60f);

    /// <inheritdoc/>
    public Viewport Viewport { get; private set; }

    /// <inheritdoc/>
    public int? Selected { get; set; }

    /// <summary>Half-width of the checkerboard ground, in world units.</summary>
    public int GroundHalfExtent { get; set; } = 12;

    /// <summary>Size of one checkerboard square, in world units.</summary>
    public float GroundStep { get; set; } = 2f;

    /// <summary>
    /// How many distinct brightness levels the shade ramp is quantised to. Rounded and clamped to
    /// [<see cref="MinShadeLevels"/>, <see cref="MaxShadeLevels"/>].
    /// </summary>
    /// <remarks>
    /// <para>
    /// The quality/cost dial, and the one that actually moves the picture at this resolution — a curved surface shows
    /// this many bands across it, so a sphere at 12 sub-pixels across is showing about four. Raising it smooths the
    /// banding; lowering it flattens the scene into poster shades.
    /// </para>
    /// <para>
    /// It is <b>also the largest performance lever the renderer has</b>, which is why the defaults are low. Coarse
    /// levels mean neighbouring cells share a colour and the emitter coalesces them into long runs; fine levels break
    /// those runs and the ANSI byte count climbs. Cheap to raise for a recording, where nothing is waiting on the
    /// terminal; think before raising it for interactive use on a slow one.
    /// </para>
    /// </remarks>
    public float ShadeLevels
    {
        get => shadeLevels;
        set => shadeLevels = Math.Clamp(MathF.Round(value), MinShadeLevels, MaxShadeLevels);
    }

    /// <summary>
    /// Doubles the horizontal sampling rate and composites each 2×2 block into a quadrant glyph, so a silhouette
    /// lands on a half-cell boundary instead of a whole-cell one. Off by default; costs twice the fill.
    /// </summary>
    /// <remarks>
    /// The <em>other</em> resolution dial, and the counterpart to <see cref="ShadeLevels"/>: that one buys colour
    /// precision, this one buys spatial precision, and both are paid for in ANSI runs — plus, here, twice the fill.
    /// Measured cost and the reason it emits no colour the renderer did not already produce are in
    /// <see cref="HalfBlockSurface.QuadrantSampling"/>. Both solid renderers have it: the surface composites, so
    /// neither's shading needs to know.
    /// </remarks>
    public bool QuadrantSampling
    {
        get => surface.QuadrantSampling;
        set => surface.QuadrantSampling = value;
    }

    /// <summary>
    /// A procedural texture applied to mesh bodies that carry UVs, modulating the body's tint per sub-pixel before
    /// it is shaded. <see cref="TextureMode.None"/> by default.
    /// </summary>
    /// <remarks>
    /// The procedural modes are a <b>measuring instrument</b>: they price what a per-sub-pixel tint does to the
    /// emission budget, and ANSI runs — not compute — are what this renderer is bound by. <see cref="TextureMode.Image"/>
    /// draws a mesh's real map. Not yet exposed in the UI: it is driven from the harness's <c>--texture</c> mode,
    /// and only <see cref="ShadedRenderer"/> should offer it — texturing costs <see cref="SolidRenderer"/> more than
    /// the shaded renderer costs untextured. See <c>docs/internal/agent/3D Textures Plan.md</c>.
    /// </remarks>
    public TextureMode Texture { get; set; }

    /// <summary>How many times <see cref="Texture"/> repeats across the UV range. The frequency dial, and the whole
    /// point of the experiment — detail finer than a sub-pixel cannot be shown and is paid for regardless.</summary>
    public float TextureScale { get; set; } = 8f;

    /// <summary>The coarsest shade ramp on offer — two levels is lit and unlit, and little else.</summary>
    public const float MinShadeLevels = 2f;

    /// <summary>The finest shade ramp on offer. Past this the bands are below what the palette and the eye resolve,
    /// and only the byte count keeps climbing.</summary>
    public const float MaxShadeLevels = 24f;
    #endregion
    #region Methods
    /// <inheritdoc/>
    public bool DrawsOffThread => true;

    /// <inheritdoc/>
    public object? Draw(in FrameRequest request)
    {
        // Everything geometric arrives in the request, captured on the UI thread -- see FrameRequest for why this
        // reads nothing off the control. Viewport is assigned rather than measured for the same reason.
        Viewport = new Viewport(request.Cells, request.Rows);
        if (!Viewport.IsValid || !surface.BeginFrame(request.Cells, request.Rows)) return null;

        View = request.View;
        // Sub-pixel grid: W wide, 2H tall, isotropic (see HalfBlockSurface). NDC x spans [-1,1] over the width and
        // y spans the cell aspect over the height, exactly as the wireframe canvas does -- so every renderer agrees
        // and picking works identically under any of them.
        halfW = surface.PixelWidth / 2f;
        halfH = surface.PixelHeight / 2f;
        scaleY = (float)(surface.PixelHeight / (2.0 * Viewport.CellAspect));

        var snapshot = request.Snapshot;
        DrawGround();
        for (var i = 0; i < snapshot.Count; i++) DrawBody(snapshot, i);

        // Any depth post-process runs here, once the buffer is complete: silhouettes and ambient occlusion can only
        // be found after every surface has been resolved against every other one.
        PostProcess();
        return surface.EndFrame();
    }

    /// <inheritdoc/>
    public void Publish(object? frame) => surface.Publish(frame as SurfaceFrame);
    #endregion

    #region Protected members
    /// <summary>The surface this renderer draws into.</summary>
    protected HalfBlockSurface HalfBlocks => surface;

    /// <summary>This frame's camera basis.</summary>
    protected CameraView View { get; private set; }

    /// <summary>When <see langword="true"/>, <see cref="ShadePixel"/> is called for every covered sub-pixel;
    /// otherwise <see cref="ShadeFace"/> is called once per triangle.</summary>
    protected abstract bool ShadesPerPixel { get; }

    /// <summary>Colour for a whole triangle, given its world-space face normal.</summary>
    protected abstract CColor ShadeFace(Vector3 normal, Color tint);

    /// <summary>Colour for one sub-pixel. Only called when <see cref="ShadesPerPixel"/> is set.</summary>
    protected virtual CColor ShadePixel(Vector3 world, Vector3 normal, Color tint) => ShadeFace(normal, tint);

    /// <summary>Runs after the whole scene is rasterised. The default does nothing.</summary>
    protected virtual void PostProcess() { }

    /// <summary>Quantise, then apply. Faces land on one of a handful of levels, so neighbouring cells share a colour
    /// and the renderer's run-coalescing does its job — the single largest emission lever.</summary>
    /// <remarks>
    /// Measured at M0.1: content in flat bands emits ~7× fewer ANSI bytes and runs ~3× faster end to end than
    /// content where every neighbour differs. A smooth gradient across each face would buy nothing visible at this
    /// resolution and would put the renderer in the expensive column.
    /// </remarks>
    protected static CColor Quantise(Color tint, float intensity, float levels)
    {
        intensity = MathF.Round(Math.Clamp(intensity, 0f, 1f) * levels) / levels;
        return new CColor((byte)(tint.R * intensity), (byte)(tint.G * intensity), (byte)(tint.B * intensity));
    }
    #endregion

    #region Private methods
    private void DrawGround()
    {
        // A checkerboard rather than a grid of lines: solid ground gives the depth cue the whole renderer exists to
        // show, and alternating squares read as distance far better than a flat plane would.
        var half = GroundHalfExtent;
        var n = (int)(half * 2 / GroundStep);
        for (var iz = 0; iz < n; iz++)
        {
            for (var ix = 0; ix < n; ix++)
            {
                var x0 = -half + (ix * GroundStep);
                var z0 = -half + (iz * GroundStep);
                var x1 = x0 + GroundStep;
                var z1 = z0 + GroundStep;
                var shade = ((ix + iz) & 1) == 0 ? Palette.GroundLight : Palette.GroundDark;

                // Wound so the normal points up (+Y); the ground is lit like everything else, so it picks up the
                // same term and does not read as a flat cut-out.
                var a = new Vector3(x0, 0, z0);
                var b = new Vector3(x0, 0, z1);
                var c = new Vector3(x1, 0, z1);
                var d = new Vector3(x1, 0, z0);
                Triangle(a, b, c, shade, GroundGroup);
                Triangle(a, c, d, shade, GroundGroup);
            }
        }
    }

    private void DrawBody(SceneSnapshot snapshot, int i)
    {
        var shape = snapshot.Shapes[i];
        var half = snapshot.HalfExtents[i];
        // A loaded mesh is already normalised to a half-extent of 0.5, so it scales like the unit sphere does.
        var (mesh, scale) = shape switch
        {
            BodyShape.Sphere => (Meshes.Sphere, new Vector3(half.X)),
            BodyShape.Mesh => (Meshes.Get(snapshot.MeshIds[i]), new Vector3(half.X / 0.5f)),
            _ => (Meshes.Cube, half),
        };
        var rotation = snapshot.Rotations[i];
        var center = snapshot.Positions[i];

        var tint = Selected == snapshot.Ids[i]
            ? Palette.Selection
            : Palette.For(snapshot.ColorKeys[i]);

        EnsureVertexCapacity(mesh.Vertices.Length);
        // A full affine transform when the scene supplies one -- shear and non-uniform scale cannot go through the
        // scale-then-quaternion path. Lighting needs no special handling either way: Triangle() derives the face
        // normal from the WORLD-space winding of the transformed triangle, which is correct under any affine map,
        // so there is no inverse-transpose to apply.
        if (snapshot.LocalTransforms is { } transforms)
        {
            var m = transforms[i];
            for (var v = 0; v < mesh.Vertices.Length; v++)
                world[v] = center + Vector3.Transform(mesh.Vertices[v], m);
        }
        else
        {
            for (var v = 0; v < mesh.Vertices.Length; v++)
                world[v] = center + Vector3.Transform(mesh.Vertices[v] * scale, rotation);
        }

        // A mesh that brought its own colours (PLY) shades each face with its own, EXCEPT while selected: the
        // selection tint has to win over the whole body or there is no way to see which one is selected.
        var selected = Selected == snapshot.Ids[i];
        var faceColors = selected ? null : mesh.FaceColors;
        // Selection suppresses materials and textures for the reason it suppresses FaceColors: the selection tint has
        // to win over the whole body, or there is no way to see which one is selected.
        var mode = selected ? TextureMode.None : Texture;

        var idx = mesh.Indices;
        // The procedural modes are measuring instruments: they map through the mesh's UVs whatever its materials. Auto
        // and Image draw the MATERIALS instead -- each triangle its own material's map or flat colour. None draws
        // neither, which is the look from before materials existed, and the one where the body colour applies.
        var procedural = mode is TextureMode.Checker or TextureMode.Gradient or TextureMode.Noise;
        var materials = mode is TextureMode.Auto or TextureMode.Image ? mesh.Materials : null;
        var ids = mesh.MaterialIds;
        var uvs = mesh.Uvs;
        var uvIdx = mesh.UvIndices ?? idx;
        for (var t = 0; t < idx.Length; t += 3)
        {
            var color = faceColors is null ? tint : faceColors[t / 3];
            bodyTexture = null;
            if (materials is not null && ids is not null && ids[t / 3] is >= 0 and var id)
            {
                var material = materials[id];
                color = material.Colour;
                // THE GATE. Auto draws a map only when it reads as an image rather than as noise; otherwise the face
                // keeps its material's flat colour -- the plan's Tier 0. Image draws every map regardless.
                if (material.Map is { } map && (mode == TextureMode.Image || map.Legible)) bodyTexture = map;
            }

            var uv = uvs is not null && (procedural || bodyTexture is not null)
                ? new UvTriple(uvs[uvIdx[t]], uvs[uvIdx[t + 1]], uvs[uvIdx[t + 2]], true)
                : default;
            Triangle(world[idx[t]], world[idx[t + 1]], world[idx[t + 2]], color, BodyGroup, uv);
        }
    }

    // PROJECTING PER VERTEX INSTEAD OF PER CORNER WAS TRIED AND DOES NOT PAY, which is worth recording because it
    // is the obvious optimisation to reach for. A closed mesh shares each vertex between about six triangles, so
    // the camera transform and perspective divide below run ~6x per vertex -- 2.4M times for a 400k-vertex scan.
    // Hoisting them into two cached arrays measured 9.6% FASTER on that scan and 10% SLOWER on a 5.8k-triangle
    // model, interleaved A/B, 21 samples. The rasteriser is bound by the scattered `world[idx[t]]` reads, not by
    // the arithmetic: adding two more 4.8 MB arrays to walk randomly costs more bandwidth than the saved maths.
    private void Triangle(Vector3 a, Vector3 b, Vector3 c, Color tint, byte group, UvTriple uv = default)
    {
        var va = View.Transform(a);
        var vb = View.Transform(b);
        var vc = View.Transform(c);

        // No near-plane clipping: drop a triangle with any corner behind the camera rather than splitting it. Cheap,
        // and at the distances an orbit camera holds it costs a sliver of geometry at the very edge of the view.
        if (va.Z <= Projection.Near || vb.Z <= Projection.Near || vc.Z <= Projection.Near) return;

        // The face normal, from the WORLD-space winding, so it does not depend on where the camera is.
        var normal = Vector3.Cross(b - a, c - a);
        var lengthSquared = normal.LengthSquared();
        if (lengthSquared < 1e-12f) return;
        normal /= MathF.Sqrt(lengthSquared);

        // A texture forces the per-pixel path even under SolidRenderer: a colour evaluated once per triangle cannot
        // vary across it, which is the entire thing a texture is. That is also the honest way to price texturing
        // for the flat renderer -- it loses the one-uniform-run-per-triangle property that makes it cheap.
        var flat = ShadesPerPixel || uv.Textured ? default : ShadeFace(normal, tint);

        var pa = ToScreen(va);
        var pb = ToScreen(vb);
        var pc = ToScreen(vc);

        // Backface culling by screen-space winding. Note the sign: meshes are wound counter-clockwise seen from
        // OUTSIDE in world space, but ToScreen inverts Y (NDC +y is up, rows count downward), and that flip reverses
        // handedness -- so an outward-facing triangle arrives here with a NEGATIVE signed area. Culling `<= 0` (the
        // reflex answer) therefore discards every visible face and keeps only the hidden ones: bodies render as
        // their own far side and single-sided geometry like the ground disappears completely.
        var area = ((pb.X - pa.X) * (pc.Y - pa.Y)) - ((pb.Y - pa.Y) * (pc.X - pa.X));
        if (area >= 0) return;

        // Swap two corners to restore a positive winding for the fill, which wants all three edge functions to agree
        // in sign. Swapping b and c swaps their barycentric weights too, so depth interpolation stays correct.
        // World positions ride along in the same swapped order for per-pixel shading, and so must the UVs — an
        // attribute interpolated against the wrong corner's weight is the classic way to get a texture that looks
        // almost right and is mirrored along one edge.
        Fill(pa, pc, pb, a, c, b, -area, normal, tint, flat, group, uv.Swapped);
    }

    // Camera space -> sub-pixel coordinates, carrying reciprocal depth as Z. Screen y is inverted because NDC +y is
    // up and rows count downward.
    private Vector3 ToScreen(Vector3 cameraSpace)
    {
        var inverseZ = 1f / cameraSpace.Z;
        var ndcX = Projection.Focal * cameraSpace.X * inverseZ;
        var ndcY = Projection.Focal * cameraSpace.Y * inverseZ;
        return new Vector3(halfW + (ndcX * halfW), halfH - (ndcY * scaleY), inverseZ);
    }

    private void Fill(Vector3 a, Vector3 b, Vector3 c, Vector3 wa, Vector3 wb, Vector3 wc,
                      float area, Vector3 normal, Color tint, CColor flatColor, byte group, UvTriple uv = default)
    {
        var minX = Math.Max(0, (int)MathF.Floor(MathF.Min(a.X, MathF.Min(b.X, c.X))));
        var maxX = Math.Min(surface.PixelWidth - 1, (int)MathF.Ceiling(MathF.Max(a.X, MathF.Max(b.X, c.X))));
        var minY = Math.Max(0, (int)MathF.Floor(MathF.Min(a.Y, MathF.Min(b.Y, c.Y))));
        var maxY = Math.Min(surface.PixelHeight - 1, (int)MathF.Ceiling(MathF.Max(a.Y, MathF.Max(b.Y, c.Y))));
        if (minX > maxX || minY > maxY) return;

        var inverseArea = 1f / area;
        var textured = uv.Textured;
        var perPixel = ShadesPerPixel || textured;
        // Perspective-correct interpolation needs the attribute divided by depth: (world/z) varies linearly in
        // screen space where world alone does not. Premultiply once per triangle, divide back per pixel.
        var wan = wa * a.Z;
        var wbn = wb * b.Z;
        var wcn = wc * c.Z;
        // UVs are the same kind of attribute and take exactly the same treatment. Affine UVs on a face seen at a
        // glancing angle -- the ground, or the far side of the knot -- swim visibly as the camera orbits.
        var uan = uv.A * a.Z;
        var ubn = uv.B * b.Z;
        var ucn = uv.C * c.Z;

        for (var y = minY; y <= maxY; y++)
        {
            var py = y + 0.5f;
            for (var x = minX; x <= maxX; x++)
            {
                var px = x + 0.5f;
                // Edge functions: the signed areas of the three sub-triangles are the barycentric weights, and all
                // three non-negative means the point is inside.
                var w0 = ((b.X - a.X) * (py - a.Y)) - ((b.Y - a.Y) * (px - a.X));
                if (w0 < 0) continue;
                var w1 = ((c.X - b.X) * (py - b.Y)) - ((c.Y - b.Y) * (px - b.X));
                if (w1 < 0) continue;
                var w2 = ((a.X - c.X) * (py - c.Y)) - ((a.Y - c.Y) * (px - c.X));
                if (w2 < 0) continue;

                // Reciprocal depth is the one quantity that IS linear in screen space under perspective, so
                // interpolating it with the barycentrics is exact -- no affine-depth warping across large faces
                // like the ground squares.
                var inverseDepth = ((w1 * a.Z) + (w2 * b.Z) + (w0 * c.Z)) * inverseArea;
                if (!perPixel)
                {
                    surface.TestAndSet(x, y, inverseDepth, flatColor, group);
                    continue;
                }

                // Depth-test BEFORE shading: a hidden pixel costs nothing but the compare, which matters when the
                // shading is this much more expensive than a colour copy.
                if (inverseDepth <= surface.DepthAt(x, y)) continue;

                var point = (((w1 * wan) + (w2 * wbn) + (w0 * wcn)) * inverseArea) / inverseDepth;
                // Same weights, same order, same divide -- copied structurally from the line above rather than
                // re-derived, because the weight-to-corner mapping is rotated (w1 is a, w2 is b, w0 is c) and
                // reasoning it out a second time is how the two fall out of step.
                var texel = textured
                    ? SampleTexture((((w1 * uan) + (w2 * ubn) + (w0 * ucn)) * inverseArea) / inverseDepth, tint)
                    : tint;
                surface.TestAndSet(x, y, inverseDepth, ShadePixel(point, normal, texel), group);
            }
        }
    }

    // The three procedural sources, chosen to bracket the real question rather than to look good. A checker is the
    // BEST case: two tints, large patches, runs survive. The gradient turned out to be the WORST, not the middle --
    // Quantise rounds only the LIGHTING intensity, never the tint, so a continuous tint reaches the screen
    // unquantised and every sub-pixel gets a colour of its own. Measured on the capsule: 4.4x the untextured bytes,
    // falling to 1.77x once the tint is snapped to 7 levels. Noise is blocky at 4x the nominal scale and sits
    // between the two until its frequency rises to match. Left unquantised on purpose: it is the stand-in for a
    // real image, whose texels are arbitrary 24-bit colours, and it is what shows the texture type must quantise.
    private Color SampleTexture(Vector2 uv, Color tint) => Texture switch
    {
        TextureMode.Checker =>
            (((int)MathF.Floor(uv.X * TextureScale) + (int)MathF.Floor(uv.Y * TextureScale)) & 1) == 0
                ? tint
                : Modulate(tint, 0.35f),
        TextureMode.Gradient => Modulate(tint, 0.25f + (0.75f * Fraction(uv.X * TextureScale))),
        TextureMode.Noise => Modulate(tint, 0.2f + (0.8f * Hash(uv, TextureScale))),
        // The map's colour REPLACES the body's palette tint rather than modulating it: a diffuse map is the albedo.
        // Already reduced and quantised at load, so this is one clamped index and nothing else.
        TextureMode.Image or TextureMode.Auto => bodyTexture!.Sample(uv),
        _ => tint,
    };

    private static Color Modulate(Color tint, float factor) =>
        new((byte)(tint.R * factor), (byte)(tint.G * factor), (byte)(tint.B * factor));

    private static float Fraction(float value) => value - MathF.Floor(value);

    // A cheap integer hash rather than value noise: the point is uncorrelated neighbours, which is the pathological
    // case for run coalescing. Sampled at 4x the nominal scale so it is genuinely sub-pixel at useful scales.
    private static float Hash(Vector2 uv, float scale)
    {
        var x = (int)MathF.Floor(uv.X * scale * 4f);
        var y = (int)MathF.Floor(uv.Y * scale * 4f);
        var h = (uint)((x * 374761393) + (y * 668265263));
        h = (h ^ (h >> 13)) * 1274126177;
        return ((h ^ (h >> 16)) & 0xFFFF) / 65535f;
    }

    private void EnsureVertexCapacity(int count)
    {
        if (world.Length < count) world = new Vector3[Math.Max(count, world.Length * 2)];
    }
    #endregion

    #region Child types
    // A triangle's three UVs, or default when it carries none. A struct rather than three optional Vector2
    // parameters so the "is this textured at all" flag cannot drift from the coordinates it guards.
    private readonly record struct UvTriple(Vector2 A, Vector2 B, Vector2 C, bool Textured)
    {
        // Mirrors the b/c swap Triangle applies to restore positive winding.
        public UvTriple Swapped => new(A, C, B, Textured);
    }
    #endregion

    #region Fields
    private float shadeLevels;
    // Scenery versus bodies: only bodies are outlined (see HalfBlockSurface.TestAndSet).
    private const byte GroundGroup = 0;
    private const byte BodyGroup = 1;

    private readonly HalfBlockSurface surface = new();

    private Vector3[] world = new Vector3[64];
    // The map the triangle being drawn samples, when its material has one the mode draws. Per-draw scratch, like
    // `world`: set per triangle in DrawBody and read by SampleTexture down the same call.
    private Texture? bodyTexture;
    private float halfW;
    private float halfH;
    private float scaleY;
    #endregion
}

/// <summary>Which procedural texture <see cref="MeshRenderer.Texture"/> applies, if any.</summary>
/// <remarks>Stand-ins for a real image map, used to price texturing before an image decoder is paid for.
/// <see cref="Checker"/> is the cheapest a texture can be; <see cref="Gradient"/>, whose tint is never quantised,
/// the most expensive.</remarks>
public enum TextureMode
{
    /// <summary>No texture and no materials; the body takes its palette tint, as before materials existed.</summary>
    None,

    /// <summary>Two-tone squares in UV space. Large flat patches, so colour runs mostly survive.</summary>
    Checker,

    /// <summary>A repeating ramp along u. The most expensive source: the shade quantiser rounds lighting, not tint,
    /// so a continuous tint reaches the screen as a distinct colour per sub-pixel.</summary>
    Gradient,

    /// <summary>Uncorrelated values in texel blocks at 4× the nominal scale — what a detailed photograph becomes once
    /// it is minified to a hundred-odd sub-pixels across. Converges on <see cref="Gradient"/>'s cost as the
    /// frequency rises.</summary>
    Noise,

    /// <summary>The mesh's materials, every map drawn. A face without a mapped material shows its material's flat
    /// colour; a mesh without materials is drawn as under <see cref="None"/>.</summary>
    Image,

    /// <summary>The mesh's materials, each map drawn only if <see cref="Texture.Legible"/> — otherwise that material
    /// shows its flat colour. The one to use by default: it declines to texture where texturing would read as noise.</summary>
    Auto,
}
