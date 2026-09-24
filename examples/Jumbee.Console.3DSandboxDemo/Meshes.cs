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

    /// <summary>Texture coordinates, or <see langword="null"/> when the mesh carries none.</summary>
    public Vector2[]? Uvs { get; init; }

    /// <summary>
    /// Per-corner indices into <see cref="Uvs"/>, three per triangle and parallel to <see cref="Indices"/>, or
    /// <see langword="null"/> when <see cref="Uvs"/> runs parallel to <see cref="Vertices"/> and
    /// <see cref="Indices"/> addresses both.
    /// </summary>
    /// <remarks>
    /// A separate index buffer because OBJ genuinely needs one — measured on the sample models, 723 of 800 face
    /// corners in <c>plane.obj</c> have <c>vt != v</c>, and <c>LP_Sneaker3.obj</c> carries 1,461,581 vertices
    /// against 1,557,366 UVs. A generated mesh has no such split and leaves this null.
    /// </remarks>
    public int[]? UvIndices { get; init; }

    /// <summary>Whether this mesh can be textured.</summary>
    public bool HasUvs => Uvs is { Length: > 0 };

    /// <summary>The image map this mesh's <see cref="Uvs"/> address, or <see langword="null"/> when it has none.
    /// Drawn only when the renderer's <see cref="MeshRenderer.Texture"/> is <see cref="TextureMode.Image"/>.</summary>
    public Texture? Texture { get; init; }

    /// <summary>Number of triangles.</summary>
    public int TriangleCount => Indices.Length / 3;

    /// <summary>
    /// One colour per triangle, or <see langword="null"/> when the mesh carries no colour of its own and takes the
    /// body's palette tint instead.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Length is <see cref="TriangleCount"/>. Set by <c>PlyLoader</c>, the only format here that carries colour
    /// without a side-car material file — OBJ needs an <c>.mtl</c> (and usually a texture with it) and STL carries
    /// nothing at all.
    /// </para>
    /// <para>
    /// <b>Per triangle, not per vertex, even for files that store it per vertex.</b> The renderers shade a whole
    /// face at once (see <c>MeshRenderer.Triangle</c>), so a corner-varying colour has nowhere to go without moving
    /// to the per-pixel path and paying a barycentric blend for it. At a shade ramp of a couple of dozen levels
    /// across a face a few sub-pixels wide, the average of the three corners lands on the same quantised colour the
    /// blend would have produced nearly everywhere. <c>PlyLoader</c> therefore averages corner colours at load and
    /// the renderers stay simple.
    /// </para>
    /// <para>
    /// <b><see cref="Uvs"/> does not follow that reasoning, and the difference is the point.</b> A corner colour is
    /// an approximation of one value per face, so averaging it loses almost nothing; a texture coordinate is an
    /// <em>address</em>, and the variation it fetches lives <em>inside</em> the face rather than across its corners.
    /// Averaging it would sample one texel per triangle and discard the whole texture. So UVs take the per-pixel
    /// path and pay the barycentric interpolation this paragraph declines to pay for colour.
    /// </para>
    /// </remarks>
    public Color[]? FaceColors { get; init; }
    #endregion

    #region Methods
    /// <summary>A copy of this mesh carrying <paramref name="texture"/>. Geometry and UVs are shared, not copied.</summary>
    /// <remarks>The seam a material loader attaches a map through, since the geometry loaders know nothing of
    /// images. <b>Every init property must be carried here</b> — a new one left out is silently dropped from every
    /// textured mesh, and the harness checks each one survives.</remarks>
    public Mesh WithTexture(Texture? texture) => new(Vertices, Indices)
    {
        AuthoredUpAxis = AuthoredUpAxis,
        FaceColors = FaceColors,
        Uvs = Uvs,
        UvIndices = UvIndices,
        Texture = texture,
    };
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
        // One extra ring around the curve and one extra around the tube, each an exact duplicate of the seam it
        // closes against. The geometry is unchanged — the duplicated vertices coincide — but it lets the UVs run
        // 0..1 without the last column of quads having to wrap u backwards from 1 to 0, which is the classic seam
        // artefact and reads as a defect in a render rather than as a texture.
        var cols = segments + 1;
        var rows = sides + 1;
        var vertices = new Vector3[cols * rows];
        var uvs = new Vector2[cols * rows];
        var indices = new int[segments * sides * 6];

        for (var i = 0; i < cols; i++)
        {
            var u = MathF.Tau * (i % segments) / segments;
            var centre = KnotPoint(u, p, q);
            // A Frenet-ish frame from the curve's tangent, so the tube keeps a consistent cross-section.
            var tangent = Vector3.Normalize(KnotPoint(u + 0.01f, p, q) - centre);
            var normal = Vector3.Normalize(Vector3.Cross(tangent, Vector3.UnitY));
            if (!float.IsFinite(normal.X)) normal = Vector3.UnitX;
            var binormal = Vector3.Cross(tangent, normal);

            for (var j = 0; j < rows; j++)
            {
                var v = MathF.Tau * (j % sides) / sides;
                var offset = (normal * MathF.Cos(v)) + (binormal * MathF.Sin(v));
                vertices[(i * rows) + j] = centre + (offset * 0.55f);
                // u runs along the curve, v around the tube. The natural parameterisation, and it is strongly
                // ANISOTROPIC: measured, u spans 31.90 units of arc against v's 3.46, a ratio of 9.23:1. So a
                // checker at one scale is 9:1 rectangles, and what it looks like is stripes running lengthwise
                // along the tube that swap phase wherever a u boundary crosses — not a square checkerboard.
                //
                // Left alone deliberately. Separate u/v scales would make the stand-in prettier and would be
                // tuning the instrument to flatter itself; a real .obj carries UVs an authoring tool laid out at
                // roughly uniform texel density, so the stretch is a property of this generated mesh and not of
                // the feature being measured.
                uvs[(i * rows) + j] = new Vector2((float)i / segments, (float)j / sides);
            }
        }

        var k = 0;
        for (var i = 0; i < segments; i++)
        {
            for (var j = 0; j < sides; j++)
            {
                int a = (i * rows) + j;
                int b = ((i + 1) * rows) + j;
                int c = ((i + 1) * rows) + j + 1;
                int d = (i * rows) + j + 1;
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

        return new Mesh(vertices, indices) { Uvs = uvs };
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
