namespace Jumbee.Console.SandboxDemo;

using System.Numerics;

/// <summary>A triangle mesh in unit local space, scaled and rotated onto each body as it is drawn.</summary>
/// <remarks>
/// Parallel arrays again, and indexed triangles rather than loose ones: a cube's 8 corners are transformed once and
/// referenced by its 12 triangles, so per-body work is 8 transforms rather than 36.
/// </remarks>
public sealed class Mesh
{
    #region Constructors
    /// <summary>Creates a mesh from vertices and triangle indices (three per triangle, counter-clockwise seen from
    /// outside).</summary>
    public Mesh(Vector3[] vertices, int[] indices)
    {
        Vertices = vertices;
        Indices = indices;

        var min = new Vector3(float.MaxValue);
        var max = new Vector3(float.MinValue);
        foreach (var v in vertices)
        {
            min = Vector3.Min(min, v);
            max = Vector3.Max(max, v);
        }

        Min = vertices.Length == 0 ? Vector3.Zero : min;
        Extents = vertices.Length == 0 ? Vector3.Zero : (max - min) * 0.5f;
    }
    #endregion

    #region Properties
    /// <summary>Vertex positions, in a unit-sized local space.</summary>
    public Vector3[] Vertices { get; }

    /// <summary>
    /// The low corner of the mesh's bounding box, in local space.
    /// </summary>
    /// <remarks>
    /// Negative on every axis, because a loaded mesh is centred on its bounding box (see <c>ObjLoader</c>). Negate
    /// the up axis' component to stand a model on a surface rather than burying half of it — which axis that is
    /// depends on how the file was authored, so it is taken per component rather than assumed to be Y.
    /// </remarks>
    public Vector3 Min { get; }

    /// <summary>Half-extents of the mesh's bounding box, in local space.</summary>
    /// <remarks>The loader normalises the <em>largest</em> of the three to a fixed value, so these say how far from
    /// a cube a model is — which is what decides how far back a camera has to sit to frame it.</remarks>
    public Vector3 Extents { get; }

    /// <summary>The up axis the file appears to have been authored with, or <see langword="null"/> when nothing in
    /// it suggested one.</summary>
    /// <remarks>A default for the viewer's Z-up switch, not a fact — see <c>ObjLoader.IsZUpExporter</c> for why the
    /// distinction matters and why the guess is applied somewhere the user can see and undo it.</remarks>
    public ModelUpAxis? AuthoredUpAxis { get; init; }

    /// <summary>Triangle corner indices, three per triangle.</summary>
    public int[] Indices { get; }

    /// <summary>Number of triangles.</summary>
    public int TriangleCount => Indices.Length / 3;

    /// <summary>
    /// A reduced edge list for the wireframe renderer: unique triangle edges, thinned to at most
    /// <see cref="MaxWireEdges"/>.
    /// </summary>
    /// <remarks>
    /// Drawing every edge of a loaded model is not an option — a 6,300-face teapot has ~9,500 unique edges, and the
    /// wireframe renderer exists precisely because it is the cheap one that scales to many bodies. Thinning keeps
    /// the shape recognisable as a wire cloud at a bounded cost. Built once, on first use.
    /// </remarks>
    public (int A, int B)[] WireEdges => wireEdges ??= BuildWireEdges();
    #endregion

    #region Private methods
    private (int A, int B)[] BuildWireEdges()
    {
        var seen = new HashSet<(int, int)>();
        var edges = new List<(int A, int B)>();
        for (var t = 0; t + 2 < Indices.Length; t += 3)
        {
            Add(Indices[t], Indices[t + 1]);
            Add(Indices[t + 1], Indices[t + 2]);
            Add(Indices[t + 2], Indices[t]);
        }

        void Add(int a, int b)
        {
            var key = a < b ? (a, b) : (b, a);
            if (seen.Add(key)) edges.Add((key.Item1, key.Item2));
        }

        if (edges.Count <= MaxWireEdges) return [.. edges];

        // Stride rather than truncate: taking the first N would draw one dense corner of the model and nothing
        // else, where an even sample across the whole edge list still reads as the shape.
        var stride = (edges.Count + MaxWireEdges - 1) / MaxWireEdges;
        var thinned = new List<(int A, int B)>(MaxWireEdges);
        for (var i = 0; i < edges.Count; i += stride) thinned.Add(edges[i]);
        return [.. thinned];
    }
    #endregion

    #region Fields
    /// <summary>
    /// Ceiling on <see cref="WireEdges"/>, and it is set by LEGIBILITY rather than by cost.
    /// </summary>
    /// <remarks>
    /// A body is perhaps 30 cells across on screen, so 60 braille sub-pixels. Several hundred edges over that area
    /// is more line than there is space, and the model renders as a solid scribble -- the first attempt used 400 and
    /// looked like a filled blob. Roughly a box's worth of edges reads as a shape; the Canvas could afford far more.
    /// </remarks>
    public const int MaxWireEdges = 64;

    private (int A, int B)[]? wireEdges;
    #endregion
}

/// <summary>The unit meshes the solid renderer draws bodies with, built once at startup.</summary>
public static class Meshes
{
    #region Properties
    /// <summary>A cube spanning ±1 on each axis, so scaling by a body's half extents gives its box exactly.</summary>
    public static Mesh Cube { get; } = BuildCube();

    /// <summary>A unit-radius sphere. Flat shading leaves it visibly faceted, which is the intent — it reads as a
    /// solid object at terminal resolution, and smooth normals would cost the shade quantisation that makes the
    /// whole thing cheap to emit.</summary>
    public static Mesh Sphere { get; } = BuildSphere(rings: 6, segments: 10);
    #endregion

    #region Methods
    /// <summary>Registers a loaded mesh and returns the id a <see cref="SceneSnapshot"/> refers to it by.</summary>
    /// <remarks>
    /// A registry rather than a reference on the snapshot: the snapshot crosses threads every tick, and an id keeps
    /// it a plain value type all the way through. Meshes are added at startup and never removed, so a bare list
    /// needs no synchronisation of its own.
    /// </remarks>
    public static int Register(Mesh mesh, string name)
    {
        loaded.Add((mesh, name));
        return loaded.Count - 1;
    }

    /// <summary>A registered mesh by id.</summary>
    public static Mesh Get(int id) => loaded[id].Mesh;

    /// <summary>The display name a mesh was registered under.</summary>
    public static string NameOf(int id) => loaded[id].Name;

    /// <summary>How many meshes have been registered.</summary>
    public static int RegisteredCount => loaded.Count;

    /// <summary>
    /// A torus knot: complex, non-convex geometry available without shipping anyone's model file.
    /// </summary>
    /// <remarks>
    /// Worth having built in. It gives the renderers something with real curvature and self-occlusion to be
    /// compared on — uniform boxes and spheres flatter every renderer equally — and it means the mesh path is
    /// exercisable with no third-party asset and no licensing question attached.
    /// </remarks>
    public static Mesh TorusKnot(int p = 2, int q = 3, int segments = 120, int sides = 10, float radius = 0.5f)
    {
        var vertices = new Vector3[segments * sides];
        var indices = new int[segments * sides * 6];

        for (var i = 0; i < segments; i++)
        {
            var u = MathF.Tau * i / segments;
            var centre = KnotPoint(u, p, q);
            // A Frenet-ish frame from the curve's tangent, so the tube keeps a consistent cross-section.
            var tangent = Vector3.Normalize(KnotPoint(u + 0.01f, p, q) - centre);
            var normal = Vector3.Normalize(Vector3.Cross(tangent, Vector3.UnitY));
            if (!float.IsFinite(normal.X)) normal = Vector3.UnitX;
            var binormal = Vector3.Cross(tangent, normal);

            for (var j = 0; j < sides; j++)
            {
                var v = MathF.Tau * j / sides;
                var offset = (normal * MathF.Cos(v)) + (binormal * MathF.Sin(v));
                vertices[(i * sides) + j] = centre + (offset * 0.55f);
            }
        }

        var k = 0;
        for (var i = 0; i < segments; i++)
        {
            for (var j = 0; j < sides; j++)
            {
                int a = (i * sides) + j;
                int b = (((i + 1) % segments) * sides) + j;
                int c = (((i + 1) % segments) * sides) + ((j + 1) % sides);
                int d = (i * sides) + ((j + 1) % sides);
                indices[k++] = a; indices[k++] = b; indices[k++] = c;
                indices[k++] = a; indices[k++] = c; indices[k++] = d;
            }
        }

        // Scale to the requested radius the same way a loaded model is normalised, so every mesh body is sized
        // comparably however it was produced.
        var max = 0f;
        foreach (var v in vertices) max = MathF.Max(max, v.Length());
        if (max > 1e-6f)
        {
            var s = radius / max;
            for (var i = 0; i < vertices.Length; i++) vertices[i] *= s;
        }

        return new Mesh(vertices, indices);
    }
    #endregion

    #region Private methods
    private static Vector3 KnotPoint(float u, int p, int q)
    {
        var r = MathF.Cos(q * u) + 2f;
        return new Vector3(r * MathF.Cos(p * u), -MathF.Sin(q * u), r * MathF.Sin(p * u));
    }

    private static Mesh BuildCube()
    {
        Vector3[] v =
        [
            new(-1, -1, -1), new(1, -1, -1), new(1, 1, -1), new(-1, 1, -1),
            new(-1, -1, 1), new(1, -1, 1), new(1, 1, 1), new(-1, 1, 1),
        ];

        // Two triangles per face, wound counter-clockwise seen from outside so backface culling keeps the near faces.
        int[] i =
        [
            0, 2, 1, 0, 3, 2,   // -Z
            4, 5, 6, 4, 6, 7,   // +Z
            0, 4, 7, 0, 7, 3,   // -X
            1, 2, 6, 1, 6, 5,   // +X
            0, 1, 5, 0, 5, 4,   // -Y
            3, 7, 6, 3, 6, 2,   // +Y
        ];

        return new Mesh(v, i);
    }

    private static readonly List<(Mesh Mesh, string Name)> loaded = [];

    private static Mesh BuildSphere(int rings, int segments)
    {
        // A UV sphere: `rings` latitude bands between the poles, `segments` around. Poles are single vertices, so the
        // top and bottom bands are triangles and the rest are quads split in two.
        var vertices = new List<Vector3>((rings * segments) + 2);
        var indices = new List<int>(rings * segments * 6);

        vertices.Add(new Vector3(0, 1, 0));
        for (var r = 1; r < rings; r++)
        {
            var phi = MathF.PI * r / rings;
            var (sinPhi, cosPhi) = (MathF.Sin(phi), MathF.Cos(phi));
            for (var s = 0; s < segments; s++)
            {
                var theta = MathF.Tau * s / segments;
                vertices.Add(new Vector3(sinPhi * MathF.Cos(theta), cosPhi, sinPhi * MathF.Sin(theta)));
            }
        }

        vertices.Add(new Vector3(0, -1, 0));

        var south = vertices.Count - 1;
        int Ring(int r, int s) => 1 + ((r - 1) * segments) + (s % segments);

        for (var s = 0; s < segments; s++) indices.AddRange([0, Ring(1, s), Ring(1, s + 1)]);
        for (var r = 1; r < rings - 1; r++)
        {
            for (var s = 0; s < segments; s++)
            {
                int a = Ring(r, s), b = Ring(r, s + 1), c = Ring(r + 1, s + 1), d = Ring(r + 1, s);
                indices.AddRange([a, d, c, a, c, b]);
            }
        }

        for (var s = 0; s < segments; s++) indices.AddRange([south, Ring(rings - 1, s + 1), Ring(rings - 1, s)]);
        return new Mesh([.. vertices], [.. indices]);
    }
    #endregion
}
