namespace Jumbee.Console.SandboxDemo;

using System.Numerics;

using Jumbee.Console.Drawing;

/// <summary>
/// Wireframe: every body reduced to projected edges on a <see cref="Canvas"/> at braille resolution, sorted
/// back-to-front. Cheap, and it scales to body counts the solid renderer would not enjoy.
/// </summary>
/// <remarks>
/// <para>
/// A box is its 8 corners joined by 12 edges. A sphere is <em>one</em> screen-space circle, sized by projecting
/// <c>centre + right * radius</c> and measuring how far that landed from the projected centre — cheap, and
/// convincing because a sphere's silhouette really is a circle from any angle.
/// </para>
/// <para>
/// A loaded mesh is too big to draw whole, so it is culled to the faces pointing at the camera and thinned to a
/// budget that follows its size on screen — see <see cref="DrawMesh"/>, where the reasoning is.
/// </para>
/// <para>
/// Depth is a painter's sort of whole bodies, not per pixel: draw far ones first and let near ones overwrite them.
/// It is wrong for interpenetrating bodies and right almost everywhere else, which is the trade a wireframe makes.
/// </para>
/// <para>
/// The canvas keeps X in [-1, 1] and stretches Y to the <em>cell</em> aspect, so the sub-pixel grid comes out
/// isotropic and a circle stays a circle: braille packs 2×4 sub-cells into a character that is itself about twice
/// as tall as it is wide, so at <c>YBounds = ±2·rows/cols</c> both axes land on the same sub-pixels per world unit.
/// </para>
/// </remarks>
public sealed class WireframeRenderer : ISceneRenderer
{
    #region Constructors
    /// <summary>Creates the renderer and the canvas it draws into.</summary>
    public WireframeRenderer()
    {
        canvas = new Canvas { Marker = CanvasMarker.Braille };
        // Damage tracking stays OFF. It narrows what the compositor scans, and an orbiting camera changes nearly
        // every cell every frame — all bookkeeping, no saving. See the M0.1 numbers in the plan.
        canvas.DamageTracking = false;
    }
    #endregion

    #region Properties
    /// <inheritdoc/>
    public string Name => "wireframe";

    /// <inheritdoc/>
    public Control Surface => canvas;

    /// <inheritdoc/>
    public Projection Projection { get; } = new(60f);

    /// <inheritdoc/>
    public Viewport Viewport { get; private set; }

    /// <inheritdoc/>
    public int? Selected { get; set; }

    /// <summary>Half-width of the floor grid, in world units.</summary>
    public int GridHalfExtent { get; set; } = 12;

    /// <summary>Spacing between floor grid lines, in world units.</summary>
    public int GridStep { get; set; } = 2;

    /// <summary>
    /// Braille sub-pixels of screen area per drawn triangle — the density/cost dial for mesh bodies. Lower draws
    /// more.
    /// </summary>
    /// <remarks>
    /// The budget for a mesh is its projected area divided by this, so halving it roughly doubles both the ink and
    /// the line-drawing cost. Tuned on the bunny filling the model viewer at 200x50: at 25 the body fills in and
    /// reads as a silhouette rather than a wireframe, at 120 it opens back up into a cloud, and 40 is where the
    /// surface is solid enough to read and still see-through.
    /// </remarks>
    public float SubPixelsPerTriangle { get; set; } = DefaultSubPixelsPerTriangle;

    /// <summary>
    /// Roughly how many triangles are examined per mesh body per frame, whatever the mesh's size — the ceiling on
    /// what drawing one costs.
    /// </summary>
    /// <remarks>
    /// Only meshes with more triangles than this are sampled at all, so it does nothing for models below it and
    /// everything for models far above it. Raising it lets the sampler find geometry in parts of a model that hold
    /// few triangles but cover a lot of screen — 40,000 is where the reference plane stops leaving empty regions,
    /// while the dragon is complete by 20,000 and the smaller models at any value.
    /// </remarks>
    public int ScanCap { get; set; } = DefaultScanCap;

    /// <summary>
    /// Whether a mesh's budget is spread evenly over the SCREEN (on) or evenly over its triangle list (off).
    /// </summary>
    /// <remarks>
    /// Off is cheaper — it skips a projection per candidate and the sort — and identical on a model whose triangles
    /// are spread evenly over its surface, which every generated mesh and most scanned ones are. It differs on an
    /// authored asset whose detail holds most of the triangles while its flat panels hold most of the area: the
    /// reference plane draws its engines as a dense speckle and leaves its wings nearly bare with this off.
    /// </remarks>
    public bool Stratify { get; set; } = true;
    #endregion

    #region Methods
    /// <inheritdoc/>
    public void Draw(SceneSnapshot snapshot, OrbitCamera camera)
    {
        // ActualWidth/Height are read HERE, in the draw, never cached from a constructor or a setter -- they are
        // only real once the control has been laid out, and they change on every terminal resize.
        var viewport = new Viewport(canvas.ActualWidth, canvas.ActualHeight);
        Viewport = viewport;
        if (!viewport.IsValid) return;

        canvas.XBounds = (-1, 1);
        canvas.YBounds = (-viewport.CellAspect, viewport.CellAspect);
        canvas.Clear();

        var view = camera.GetView();
        DrawGrid(view);

        // Painter's algorithm: sort indices by camera-space depth, far first, so near bodies overwrite them. Sorting
        // the index array rather than the snapshot keeps the snapshot immutable and the arrays parallel.
        EnsureOrderCapacity(snapshot.Count);
        for (var i = 0; i < snapshot.Count; i++)
        {
            order[i] = i;
            depths[i] = view.Transform(snapshot.Positions[i]).Z;
        }

        Array.Sort(depths, order, 0, snapshot.Count);
        for (var i = snapshot.Count - 1; i >= 0; i--)
        {
            var b = order[i];
            var selected = Selected == snapshot.Ids[b];
            var color = selected ? Palette.Selection : Palette.For(snapshot.ColorKeys[b], snapshot.Awake[b]);
            switch (snapshot.Shapes[b])
            {
                case BodyShape.Sphere:
                    DrawSphere(view, snapshot.Positions[b], snapshot.HalfExtents[b].X, color);
                    break;
                case BodyShape.Mesh:
                    DrawMesh(view, Meshes.Get(snapshot.MeshIds[b]), snapshot.Positions[b],
                        snapshot.LocalTransforms is { } t
                            ? t[b]
                            : Matrix4x4.CreateScale(snapshot.HalfExtents[b].X / 0.5f)
                              * Matrix4x4.CreateFromQuaternion(snapshot.Rotations[b]),
                        color);
                    break;
                default:
                    DrawBox(view, snapshot.Positions[b], snapshot.Rotations[b], snapshot.HalfExtents[b], color);
                    break;
            }

            // A selected body also gets a crosshair through its centre -- the recolour alone is easy to lose in a
            // busy scene, and at this resolution a small box is only a handful of lit sub-cells.
            if (selected) DrawMarker(view, snapshot.Positions[b]);
        }
    }

    #endregion

    #region Private methods
    private void DrawGrid(in CameraView view)
    {
        var half = (float)GridHalfExtent;
        for (var i = -GridHalfExtent; i <= GridHalfExtent; i += GridStep)
        {
            var f = (float)i;
            var axis = i == 0;
            DrawWorldLine(view, new Vector3(-half, 0, f), new Vector3(half, 0, f), axis ? Palette.AxisZ : Palette.Grid);
            DrawWorldLine(view, new Vector3(f, 0, -half), new Vector3(f, 0, half), axis ? Palette.AxisX : Palette.Grid);
        }
    }

    private void DrawBox(in CameraView view, Vector3 center, Quaternion rotation, Vector3 halfExtents, Color color)
    {
        // The body's own axes, rotated into world space — the three columns of the rotation matrix.
        var bx = Vector3.Transform(Vector3.UnitX, rotation) * halfExtents.X;
        var by = Vector3.Transform(Vector3.UnitY, rotation) * halfExtents.Y;
        var bz = Vector3.Transform(Vector3.UnitZ, rotation) * halfExtents.Z;

        for (var bits = 0; bits < 8; bits++)
        {
            corners[bits] = center
                + ((bits & 1) == 0 ? -bx : bx)
                + ((bits & 2) == 0 ? -by : by)
                + ((bits & 4) == 0 ? -bz : bz);
        }

        foreach (var (a, b) in BoxEdges) DrawWorldLine(view, corners[a], corners[b], color);
    }

    private void DrawSphere(in CameraView view, Vector3 center, float radius, Color color)
    {
        if (!Projection.TryProject(view.Transform(center), out var x, out var y)) return;
        // The radius on screen: project a point one radius to the camera's right and measure the gap. Doing it this
        // way rather than by trigonometry means perspective foreshortening comes out right for free.
        if (!Projection.TryProject(view.Transform(center + (view.Right * radius)), out var ex, out var ey)) return;

        var screenRadius = MathF.Sqrt(((ex - x) * (ex - x)) + ((ey - y) * (ey - y)));
        canvas.Add(new Circle(x, y, screenRadius, color));
    }

    // A loaded model drawn edge-for-edge would swamp this renderer -- a 6,300-face teapot has ~9,500 unique edges,
    // against 12 for a box -- and cheapness is the whole reason the wireframe exists. So a model is thinned to a
    // budget; the three decisions that make the thinned sample READ as the model rather than as a dot cloud are:
    //
    //   1. The budget comes from how big the body is ON SCREEN, not from a constant. The sandbox draws many bodies
    //      a few dozen sub-pixels across, where a hundred edges is more line than there is room for; the model
    //      viewer draws ONE body filling the viewport, where the same hundred edges is 1% of the ink available. A
    //      single cap cannot serve both, and the one that was here served the sandbox -- a 4,968-face bunny came out
    //      as 64 edges out of 7,473, a strided sample so sparse it read as scattered dashes.
    //   2. Thinning is by TRIANGLE, keeping its three edges together. Striding an edge list picks unrelated edges
    //      from all over the surface, and each lands as a floating segment; whole triangles land as surface.
    //   3. Back faces are culled first, so the sample is spread over the half of the model you can actually see and
    //      the silhouette survives. Without it the far side is interleaved with the near one and neither reads.
    private void DrawMesh(in CameraView view, Mesh mesh, Vector3 center, Matrix4x4 transform, Color color)
    {
        var vertices = mesh.Vertices;
        EnsureMeshCapacity(vertices.Length);

        // The bounding radius falls out of the transform loop for one LengthSquared per vertex, and it is exact
        // under shear and non-uniform scale where transforming the mesh's extents would only approximate it.
        var farthest = 0f;
        for (var i = 0; i < vertices.Length; i++)
        {
            var offset = Vector3.Transform(vertices[i], transform);
            meshWorld[i] = center + offset;
            farthest = MathF.Max(farthest, offset.LengthSquared());
        }

        var budget = TriangleBudget(view, center, MathF.Sqrt(farthest));

        // Two passes, and they are separate for a reason: finding the visible faces and choosing which of them to
        // draw must EACH walk the whole mesh. Doing it in one pass -- stride forward, draw what survives the cull,
        // stop at the budget -- ends the walk partway down the triangle list, and OBJ index order is spatially
        // coherent, so the tail of the model is never CONSIDERED rather than merely thinned. That drew the bunny
        // with its back half missing, and moved the missing part as the camera orbited (where the walk stops
        // depends on how many faces the cull happens to keep, which depends on the angle).
        var triangles = mesh.TriangleCount;
        var indices = mesh.Indices;

        // Pass 1 -- which faces point at us, and WHERE ON SCREEN each one lands.
        //
        // The stride here is bounded by an absolute count, not by a multiple of the budget, because what it has to
        // guarantee is that no REGION of the model goes uncandidated -- a screen bucket with no candidate is a
        // hole however cleverly pass 2 then spends the budget. A budget-relative stride does not guarantee that:
        // on the reference plane it worked out at 26, which found candidates in only 91 of the 116 buckets the
        // model covers. Measured coverage against scan cap: bunny, teapot and cow reach every bucket at any cap;
        // the dragon needs ~20k; the plane needs 40k, because the triangles covering its wings are a tiny
        // fraction of an 81k-triangle list.
        var scanStride = Math.Max(1, triangles / Math.Max(1, ScanCap));
        var facing = 0;
        EnsureFacingCapacity((triangles + scanStride - 1) / scanStride);
        if (Stratify) Array.Clear(bucketCounts);

        for (var t = 0; t < triangles; t += scanStride)
        {
            var i = t * 3;
            var a = meshWorld[indices[i]];
            var b = meshWorld[indices[i + 1]];
            var c = meshWorld[indices[i + 2]];

            // Backface culling against the world-space normal rather than the projected winding: meshes are wound
            // counter-clockwise seen from outside, so cross(b-a, c-a) points out of the surface, and a face is
            // visible when that agrees with the direction back to the eye. Correct under any affine transform --
            // which matters here, because the model viewer's bodies carry shear and non-uniform scale.
            if (Vector3.Dot(Vector3.Cross(b - a, c - a), view.Eye - a) <= 0) continue;

            // The centroid projection is only needed to bucket the triangle, so it is skipped entirely when
            // stratification is off -- which is most of what makes the cheap mode cheap.
            if (Stratify)
            {
                if (!Projection.TryProject(view.Transform((a + b + c) / 3f), out var nx, out var ny)) continue;

                var bx = Math.Clamp((int)((nx + 1f) * 0.5f * BucketsX), 0, BucketsX - 1);
                var by = Math.Clamp(
                    (int)((float)((Viewport.CellAspect - ny) / (2.0 * Viewport.CellAspect)) * BucketsY),
                    0, BucketsY - 1);
                var bucket = (by * BucketsX) + bx;
                facingBuckets[facing] = bucket;
                bucketCounts[bucket]++;
            }

            facingTriangles[facing] = i;
            facing++;
        }

        // Pass 2 -- spend the budget EVENLY OVER THE SCREEN, not evenly over the triangle list.
        //
        // Picking one triangle per so many triangles assumes a model's triangles are spread evenly over its
        // surface, and a modelled asset's are not. The reference plane is 81k triangles, but its detail (engines,
        // gear, panel lines) holds most of the TRIANGLES while its wings and fuselage hold most of the AREA -- so
        // an even-by-count budget went almost entirely on sub-pixel dots in the detail and left the wings bare.
        // Its median drawn triangle spanned 0.13 cells against the bunny's 2.00.
        //
        // Giving each occupied screen bucket an equal share makes the ink uniform whatever the tessellation, and it
        // does so without caring WHY a region is dense. Weighting by projected area instead was tried and is
        // worse: it fixes the plane (87% -> 91% of occupied buckets inked) but regresses the dragon (95% -> 86%),
        // because the carry left over from a large triangle draws its list-neighbours in a run.
        //
        // It has to be ROUND-ROBIN rather than a fixed quota of budget/occupied, and that is not a detail. A fixed
        // quota is spent by buckets that HAVE that many candidates, and every bucket holding fewer leaves its share
        // unspent -- so the model gets uniform but thin, drawing 57% of its budget on a zoomed-in teapot where the
        // even-by-count pick drew all of it. Going round again with what the sparse buckets did not use keeps the
        // uniformity and spends the lot.
        var drawn = 0;
        if (Stratify)
        {
            // Counting sort into per-bucket runs, so each round is a walk of the occupied buckets rather than a
            // rescan of every candidate.
            var occupied = 0;
            var offset = 0;
            for (var b = 0; b < bucketCounts.Length; b++)
            {
                if (bucketCounts[b] == 0) continue;
                bucketStart[b] = offset;
                offset += bucketCounts[b];
                occupiedBuckets[occupied++] = b;
            }

            EnsureOrderedCapacity(facing);
            Array.Copy(bucketStart, bucketCursor, bucketStart.Length);
            for (var k = 0; k < facing; k++) bucketOrdered[bucketCursor[facingBuckets[k]]++] = facingTriangles[k];

            // Round r takes the r-th candidate of every bucket that still has one. Exhausted buckets are swapped
            // off the end of the live list, so the total work is O(candidates drawn + buckets) rather than
            // O(rounds x buckets).
            var live = occupied;
            for (var round = 0; live > 0 && drawn < budget; round++)
            {
                for (var s = 0; s < live && drawn < budget; s++)
                {
                    var b = occupiedBuckets[s];
                    if (round >= bucketCounts[b])
                    {
                        occupiedBuckets[s] = occupiedBuckets[--live];
                        s--;
                        continue;
                    }

                    DrawTriangle(view, indices, bucketOrdered[bucketStart[b] + round], color);
                    drawn++;
                }
            }
        }
        else
        {
            // Un-stratified: an even pick across the candidate list. Cheaper -- no centroid projection, no sort --
            // and identical on an evenly tessellated model. See the Stratify property for when it is not.
            var carry = 0;
            for (var k = 0; k < facing && drawn < budget; k++)
            {
                carry += budget;
                if (carry < facing) continue;
                carry -= facing;
                DrawTriangle(view, indices, facingTriangles[k], color);
                drawn++;
            }
        }
    }

    private void DrawTriangle(in CameraView view, int[] indices, int i, Color color)
    {
        var a = meshWorld[indices[i]];
        var b = meshWorld[indices[i + 1]];
        var c = meshWorld[indices[i + 2]];
        DrawWorldLine(view, a, b, color);
        DrawWorldLine(view, b, c, color);
        DrawWorldLine(view, c, a, color);
    }

    private void EnsureFacingCapacity(int count)
    {
        if (facingTriangles.Length >= count) return;
        var size = Math.Max(count, facingTriangles.Length * 2);
        facingTriangles = new int[size];
        facingBuckets = new int[size];
    }

    private void EnsureOrderedCapacity(int count)
    {
        if (bucketOrdered.Length < count) bucketOrdered = new int[Math.Max(count, bucketOrdered.Length * 2)];
    }

    /// <summary>How many triangles of a mesh body are worth drawing, from the area its bounding sphere covers on
    /// screen.</summary>
    /// <remarks>
    /// The canvas keeps X in [-1, 1] across <c>Width x 2</c> braille sub-pixels and stretches Y to the cell aspect
    /// to match, so one NDC unit is <see cref="Viewport.Width"/> sub-pixels on <em>both</em> axes — which is what
    /// lets a screen-space radius in NDC become a sub-pixel area with one multiply.
    /// </remarks>
    private int TriangleBudget(in CameraView view, Vector3 center, float radius)
    {
        if (!Projection.TryProject(view.Transform(center), out var x, out var y) ||
            !Projection.TryProject(view.Transform(center + (view.Right * radius)), out var ex, out var ey))
        {
            return MinTriangles;
        }

        var screenRadius = MathF.Sqrt(((ex - x) * (ex - x)) + ((ey - y) * (ey - y))) * Viewport.Width;
        var area = 4f * screenRadius * screenRadius;
        return Math.Clamp((int)(area / SubPixelsPerTriangle), MinTriangles, MaxTriangles);
    }

    private void EnsureMeshCapacity(int count)
    {
        if (meshWorld.Length < count) meshWorld = new Vector3[Math.Max(count, meshWorld.Length * 2)];
    }

    private void DrawMarker(in CameraView view, Vector3 center)
    {
        if (!Projection.TryProject(view.Transform(center), out var x, out var y)) return;
        const float Arm = 0.05f;
        canvas.Add(new Line(x - Arm, y, x + Arm, y, Palette.Selection));
        canvas.Add(new Line(x, y - Arm, x, y + Arm, Palette.Selection));
    }

    private void DrawWorldLine(in CameraView view, Vector3 a, Vector3 b, Color color)
    {
        // Per-endpoint reject, no near-plane clipping: an edge with one end behind the camera is dropped rather than
        // drawn wrong. Cheap, and at the distances an orbit camera holds it is rarely visible.
        if (Projection.TryProject(view.Transform(a), out var x1, out var y1) &&
            Projection.TryProject(view.Transform(b), out var x2, out var y2))
        {
            canvas.Add(new Line(x1, y1, x2, y2, color));
        }
    }

    private void EnsureOrderCapacity(int count)
    {
        if (order.Length >= count) return;
        var size = Math.Max(count, order.Length * 2);
        order = new int[size];
        depths = new float[size];
    }
    #endregion

    #region Fields
    /// <summary>
    /// The default for <see cref="SubPixelsPerTriangle"/>, set by LEGIBILITY rather than by cost.
    /// </summary>
    /// <remarks>
    /// The Canvas could afford several times the resulting edge count — 40 is what the eye can separate, not what
    /// the renderer can pay for. Exposed so a UI offering the dial can label this value as the default.
    /// </remarks>
    public const float DefaultSubPixelsPerTriangle = 40f;

    /// <summary>The floor on <see cref="TriangleBudget"/>, so a body too small or too distant to earn the density
    /// still reads as a shape rather than a handful of strokes.</summary>
    /// <remarks>The cap this replaces was 64 <em>edges</em> for every mesh body whatever its size. As a floor on
    /// triangles it leaves sandbox-sized bodies denser than they were, which they can afford now that culling
    /// spends the whole budget on the side facing the camera.</remarks>
    public const int MinTriangles = 64;

    /// <summary>The ceiling on <see cref="TriangleBudget"/>. Reached only by a body filling most of the viewport,
    /// and there to bound the per-frame cost rather than because the picture needs it.</summary>
    /// <remarks>
    /// Known limitation, deferred: this is a constant, and it bites before the top half of
    /// <see cref="SubPixelsPerTriangle"/> does for any model that fills the frame — the bunny wants 1,689 triangles
    /// at "detail 4" and 3,379 at "detail 8", and gets 1,200 either way, so those two settings draw the same
    /// picture. It was written when the density was fixed; as a user-facing dial, a ceiling derived from that dial
    /// would let the top half mean something. See open question 6 in <c>docs/internal/handoff.md</c>.
    /// </remarks>
    public const int MaxTriangles = 1200;

    /// <summary>
    /// The screen grid pass 2 spreads a mesh's budget over, in NDC — so it covers the viewport, not the body.
    /// </summary>
    /// <remarks>
    /// Sized so that a body filling the frame lands in a few hundred buckets and each gets a handful of triangles
    /// at a typical budget. Much finer and every bucket holds one triangle, which is just the un-stratified pick
    /// again; much coarser and a bucket spans enough screen for the density bias to reappear inside it.
    /// </remarks>
    private const int BucketsX = 48;

    /// <summary>Rows of the screen grid. See <see cref="BucketsX"/>.</summary>
    private const int BucketsY = 24;

    /// <summary>
    /// The default for <see cref="ScanCap"/>: set by coverage, then checked against cost.
    /// </summary>
    /// <remarks>
    /// 40,000 is where the last of the reference models (the plane) stops leaving empty screen regions; below it
    /// the plane sits at 96%. It is an empirical "big enough", not a derived bound — a model with even less of its
    /// geometry on the parts covering the most screen would still want more, which is why it is a dial.
    /// </remarks>
    public const int DefaultScanCap = 40_000;

    /// <summary>The lowest and highest "detail" a UI should offer. See <see cref="DetailFromSubPixels"/>.</summary>
    public const float MinDetail = 1f;

    /// <summary>The upper end of the detail range. See <see cref="DetailFromSubPixels"/>.</summary>
    public const float MaxDetail = 8f;

    /// <summary>
    /// <see cref="SubPixelsPerTriangle"/> expressed as a "detail" figure, where HIGHER draws more.
    /// </summary>
    /// <remarks>
    /// The renderer's own dial is an area per triangle, so smaller means denser — and a slider that draws less as
    /// you push it right is a small cruelty. UIs show this instead, and the inversion happens here so both sidebars
    /// agree on what a given number means. Detail 2 is the default; the range spans a sparse cloud to as dense as
    /// the braille grid can separate.
    /// </remarks>
    public static float DetailFromSubPixels(float subPixelsPerTriangle) =>
        DetailReference / Math.Max(1f, subPixelsPerTriangle);

    /// <summary>The inverse of <see cref="DetailFromSubPixels"/>.</summary>
    public static float SubPixelsFromDetail(float detail) => DetailReference / Math.Max(0.1f, detail);

    private const float DetailReference = DefaultSubPixelsPerTriangle * 2f;

    /// <summary>Values a UI should offer for <see cref="ScanCap"/>, with the cost/coverage trade each represents.</summary>
    public static readonly (string Label, int Value)[] ScanCapChoices =
    [
        ("5k (fastest)", 5_000),
        ("10k", 10_000),
        ("20k (dragon complete)", 20_000),
        ("40k (default)", DefaultScanCap),
        ("80k", 80_000),
        ("unlimited", int.MaxValue),
    ];

    private static readonly (int A, int B)[] BoxEdges =
    [
        (0, 1), (2, 3), (4, 5), (6, 7),
        (0, 2), (1, 3), (4, 6), (5, 7),
        (0, 4), (1, 5), (2, 6), (3, 7),
    ];

    private readonly Canvas canvas;
    private readonly Vector3[] corners = new Vector3[8];
    private Vector3[] meshWorld = new Vector3[64];

    /// <summary>Pass 1's output: the index-array offset of each front-facing triangle found this frame, and which
    /// screen bucket it landed in. Reused across frames and bodies so the two-pass draw allocates nothing.</summary>
    private int[] facingTriangles = new int[256];
    private int[] facingBuckets = new int[256];
    private int[] bucketOrdered = new int[256];
    private readonly int[] bucketCounts = new int[BucketsX * BucketsY];
    private readonly int[] bucketStart = new int[BucketsX * BucketsY];
    private readonly int[] bucketCursor = new int[BucketsX * BucketsY];
    private readonly int[] occupiedBuckets = new int[BucketsX * BucketsY];

    private int[] order = new int[64];
    private float[] depths = new float[64];
    #endregion
}

/// <summary>Scene colours: one hue per body colour key, dimmed when the body is asleep.</summary>
public static class Palette
{
    #region Methods
    /// <summary>The colour for a body, dimmed if it has gone to sleep — which makes the engine's sleep behaviour
    /// visible rather than something you have to read off the inspector.</summary>
    public static Color For(int colorKey, bool awake)
    {
        var c = Bodies[((colorKey % Bodies.Length) + Bodies.Length) % Bodies.Length];
        return awake ? c : new Color((byte)(c.R / 3), (byte)(c.G / 3), (byte)(c.B / 3));
    }
    #endregion

    #region Fields
    /// <summary>The floor grid.</summary>
    public static readonly Color Grid = new(60, 60, 70);

    /// <summary>The grid line along world X.</summary>
    public static readonly Color AxisX = new(150, 70, 70);

    /// <summary>The grid line along world Z.</summary>
    public static readonly Color AxisZ = new(70, 90, 150);

    /// <summary>The selected body — deliberately outside the body palette so it can never be mistaken for one.</summary>
    public static readonly Color Selection = new(255, 255, 255);

    /// <summary>Light squares of the solid renderer's checkerboard ground.</summary>
    public static readonly Color GroundLight = new(96, 100, 112);

    /// <summary>Dark squares of the solid renderer's checkerboard ground.</summary>
    public static readonly Color GroundDark = new(58, 62, 72);

    /// <summary>
    /// The body colours, in the order a colour key indexes them, with a name each so a UI can offer them.
    /// </summary>
    /// <remarks>
    /// The palette is the only place a body's colour comes from — every renderer tints through
    /// <see cref="For"/> — so picking one of these is all "choose the model's colour" needs to mean.
    /// </remarks>
    public static readonly (string Name, Color Color)[] Named =
    [
        ("Amber", new(235, 180, 90)),
        ("Mint", new(120, 200, 160)),
        ("Orchid", new(200, 120, 190)),
        ("Sky", new(130, 175, 235)),
        ("Coral", new(225, 130, 110)),
        ("Lime", new(170, 210, 120)),
    ];

    private static readonly Color[] Bodies = [.. Named.Select(n => n.Color)];
    #endregion
}
