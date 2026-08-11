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
    // against 12 for a box -- and cheapness is the whole reason the wireframe exists. Mesh.WireEdges is a thinned,
    // capped sample of them, so a model costs about what a dozen boxes do and still reads as its shape.
    private void DrawMesh(in CameraView view, Mesh mesh, Vector3 center, Matrix4x4 transform, Color color)
    {
        EnsureMeshCapacity(mesh.Vertices.Length);
        for (var i = 0; i < mesh.Vertices.Length; i++)
            meshWorld[i] = center + Vector3.Transform(mesh.Vertices[i], transform);

        foreach (var (a, b) in mesh.WireEdges) DrawWorldLine(view, meshWorld[a], meshWorld[b], color);
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
    private static readonly (int A, int B)[] BoxEdges =
    [
        (0, 1), (2, 3), (4, 5), (6, 7),
        (0, 2), (1, 3), (4, 6), (5, 7),
        (0, 4), (1, 5), (2, 6), (3, 7),
    ];

    private readonly Canvas canvas;
    private readonly Vector3[] corners = new Vector3[8];
    private Vector3[] meshWorld = new Vector3[64];

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

    private static readonly Color[] Bodies =
    [
        new(235, 180, 90),
        new(120, 200, 160),
        new(200, 120, 190),
        new(130, 175, 235),
        new(225, 130, 110),
        new(170, 210, 120),
    ];
    #endregion
}
