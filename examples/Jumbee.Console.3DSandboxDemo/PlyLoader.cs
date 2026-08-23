namespace Jumbee.Console.SandboxDemo;

using System.Numerics;

using Ply.Net;

/// <summary>
/// A PLY reader, ASCII and binary little-endian. The only format here that carries colour in the file itself.
/// </summary>
/// <remarks>
/// <para>
/// PLY is the reason the renderers grew <see cref="Mesh.FaceColors"/>. OBJ needs a side-car <c>.mtl</c> (and in
/// practice a texture with it, which nothing here can sample) and STL carries no colour at all, so before this
/// every model took a flat palette tint. A PLY file states its own colours inline, per vertex or per face, with no
/// second file to find.
/// </para>
/// <para>
/// <b>Geometry is already indexed</b>, unlike STL's triangle soup, so there is no welding pass — the file's own
/// vertex list is the mesh's. What the format does need instead is <b>fan triangulation</b>: a face is a list of
/// any length, and quads are common. A face of n corners becomes n-2 triangles, all sharing corner 0, and all
/// sharing that face's colour.
/// </para>
/// <para>
/// <b>Every property arrives loosely typed and has to be coerced.</b> The parser hands back whatever the file
/// declared — positions as <c>float[]</c> or <c>double[]</c>, indices as <c>int[][]</c> or <c>uint[][]</c>,
/// colours as <c>byte[]</c> (0..255) or <c>float[]</c> (0..1) — because all of those are legal and all of them
/// occur. The coercion helpers below are most of this file, and they are the reason a reader that only handles
/// <c>float</c>/<c>int</c> appears to work until it meets a file from a different exporter.
/// </para>
/// <para>
/// <b>binary_big_endian is rejected, not read.</b> The spec allows it; essentially no current exporter emits it.
/// The parser refuses it on a little-endian machine and this loader turns that into a message naming the format,
/// which is a better outcome than the byte-swapped garbage a half-hearted attempt would produce.
/// </para>
/// <para>
/// <b>A truncated file is an error here even though the parser tolerates one.</b> A PLY header declares its element
/// counts up front, and the parser sizes its arrays from that declaration and fills what it finds — so a file
/// holding two of the two thousand vertices it promised yields 1,998 vertices at the origin, mentioned only through
/// an optional log callback. That reads as a modelling artefact rather than a broken file, which is exactly the
/// failure worth refusing: the callback is wired up below and anything it reports becomes an exception.
/// </para>
/// </remarks>
public static class PlyLoader
{
    #region Methods
    /// <summary>Loads a PLY file, centred and scaled like <see cref="ObjLoader.Load"/>, with per-face colour when
    /// the file carries any.</summary>
    /// <exception cref="InvalidDataException">The file holds no usable geometry, is truncated, or is in a form this
    /// reader does not support.</exception>
    public static Mesh Load(string path, float radius = 0.5f)
    {
        using var stream = File.OpenRead(path);
        return Parse(stream, radius);
    }

    /// <summary>Parses PLY from a stream. Split out so it can be tested without a file.</summary>
    /// <exception cref="InvalidDataException">As <see cref="Load"/>.</exception>
    public static Mesh Parse(Stream stream, float radius = 0.5f)
    {
        // The parser reports a short read through this callback rather than by throwing -- see the truncation note
        // above. Capture the first complaint and fail on it once the parse is done.
        string? complaint = null;
        void Log(string message)
        {
            if (complaint is null && (message.Contains("Premature", StringComparison.Ordinal) ||
                                      message.Contains("Failed to read", StringComparison.Ordinal)))
                complaint = message;
        }

        PlyParser.Dataset dataset;
        try
        {
            dataset = PlyParser.Parse(stream, ChunkBytes, Log);
        }
        catch (Exception e) when (e is not InvalidDataException)
        {
            // Everything the parser rejects arrives as a bare Exception: not a ply file, an unsupported format, a
            // big-endian body. Restate it as the type the rest of the app already handles from the other loaders.
            throw new InvalidDataException(Explain(e.Message), e);
        }

        var vertices = new List<Vector3>();
        var vertexColors = new List<Color>();
        var faces = new List<int[]>();
        var faceColors = new List<Color>();

        // Binary elements arrive in CHUNKS -- one ElementData per chunk, repeating the same Element -- so this
        // accumulates across them rather than assuming a single block per element. ASCII yields one chunk, which is
        // the same loop with one iteration.
        try
        {
            foreach (var element in dataset.Data)
            {
                switch (element.Element.Type)
                {
                    case PlyParser.ElementType.Vertex:
                        ReadVertices(element, vertices, vertexColors);
                        break;
                    case PlyParser.ElementType.Face:
                        ReadFaces(element, faces, faceColors);
                        break;
                }
            }
        }
        catch (Exception e) when (e is not InvalidDataException)
        {
            throw new InvalidDataException(Explain(e.Message), e);
        }

        if (complaint is not null) throw new InvalidDataException(Truncated(complaint));

        if (vertices.Count == 0)
            throw new InvalidDataException("no usable geometry in the PLY (it declares no vertices)");

        var (indices, colors) = Triangulate(faces, faceColors, vertexColors, vertices.Count);
        if (indices.Count < 3)
            throw new InvalidDataException(
                faces.Count == 0
                    ? "no usable geometry in the PLY (it declares vertices but no faces)"
                    : $"no usable geometry in the PLY (all {faces.Count} faces were degenerate or out of range)");

        ObjLoader.Normalise(vertices, radius);
        return new Mesh([.. vertices], [.. indices]) { FaceColors = colors };
    }
    #endregion

    #region Private methods
    // Fan triangulation, dropping the faces that cannot be drawn. A face of n corners contributes n-2 triangles
    // around corner 0, which is correct for the convex faces PLY writers emit; a concave quad would fold, and that
    // is a trade worth taking over carrying an ear-clipper for a case the format barely sees.
    //
    // Two kinds of face are dropped rather than emitted, both silently, because a handful in a scanned model is
    // normal and none of them can produce a pixel: a corner index outside the vertex list (which would fault the
    // renderer's array read), and a repeated corner (zero area, so the rasteriser rejects it anyway). Dropping
    // rather than throwing is safe because the caller checks that SOMETHING survived -- a file where every face
    // goes is a corrupt file, and it is reported as one.
    private static (List<int> Indices, Color[]? Colors) Triangulate(
        List<int[]> faces, List<Color> faceColors, List<Color> vertexColors, int vertexCount)
    {
        var perFace = faceColors.Count == faces.Count && faces.Count > 0;
        var perVertex = !perFace && vertexColors.Count == vertexCount && vertexCount > 0;

        var indices = new List<int>(faces.Count * 3);
        var colors = perFace || perVertex ? new List<Color>(faces.Count) : null;

        for (var f = 0; f < faces.Count; f++)
        {
            var corners = faces[f];
            if (corners.Length < 3) continue;

            var usable = true;
            foreach (var c in corners)
                if (c < 0 || c >= vertexCount) { usable = false; break; }
            if (!usable) continue;

            for (var k = 2; k < corners.Length; k++)
            {
                int a = corners[0], b = corners[k - 1], c = corners[k];
                if (a == b || b == c || a == c) continue;

                indices.Add(a);
                indices.Add(b);
                indices.Add(c);
                // Every triangle of a fan takes the face's colour; a per-vertex file averages the three corners.
                // See Mesh.FaceColors for why the average is enough at this resolution.
                colors?.Add(perFace ? faceColors[f] : Average(vertexColors[a], vertexColors[b], vertexColors[c]));
            }
        }

        return (indices, colors?.ToArray());
    }

    private static Color Average(Color a, Color b, Color c) => new(
        (byte)((a.R + b.R + c.R) / 3),
        (byte)((a.G + b.G + c.G) / 3),
        (byte)((a.B + b.B + c.B) / 3));

    private static void ReadVertices(PlyParser.ElementData element, List<Vector3> vertices, List<Color> colors)
    {
        var xs = Scalars(element, "x");
        var ys = Scalars(element, "y");
        var zs = Scalars(element, "z");
        if (xs is null || ys is null || zs is null)
            throw new InvalidDataException("the PLY's vertex element is missing an x, y or z property");

        var count = Math.Min(xs.Length, Math.Min(ys.Length, zs.Length));
        for (var i = 0; i < count; i++) vertices.Add(new Vector3(xs[i], ys[i], zs[i]));

        AppendColors(element, count, colors);
    }

    private static void ReadFaces(PlyParser.ElementData element, List<int[]> faces, List<Color> colors)
    {
        // `vertex_indices` is the spec's name and what almost everything writes; `vertex_index` is the long-standing
        // variant that a few tools emit, so both are accepted rather than one of them reading as a faceless file.
        var data = element["vertex_indices"]?.Data ?? element["vertex_index"]?.Data;
        if (data is null) return;

        if (data is int[][] fast)
        {
            faces.AddRange(fast);
        }
        else
        {
            for (var i = 0; i < data.Length; i++)
                faces.Add(Integers((Array)data.GetValue(i)!));
        }

        AppendColors(element, data.Length, colors);
    }

    // Colour lives on either element, under either spelling. Absent or partial colour appends nothing, which is
    // what makes the count check in Triangulate the test for "did this file colour every one of them".
    private static void AppendColors(PlyParser.ElementData element, int count, List<Color> colors)
    {
        var r = Channel(element, "red", "r");
        var g = Channel(element, "green", "g");
        var b = Channel(element, "blue", "b");
        if (r is null || g is null || b is null) return;

        var n = Math.Min(count, Math.Min(r.Length, Math.Min(g.Length, b.Length)));
        for (var i = 0; i < n; i++) colors.Add(new Color(r[i], g[i], b[i]));
    }

    private static byte[]? Channel(PlyParser.ElementData element, string name, string shortName) =>
        (element[name] ?? element[shortName])?.Data switch
        {
            null => null,
            // 0..255 as written. `char` is signed in PLY terms but tools that use it still mean an unsigned byte.
            byte[] v => v,
            sbyte[] v => Array.ConvertAll(v, x => (byte)x),
            // 0..1, the other common convention -- exporters that treat colour as a float attribute.
            float[] v => Array.ConvertAll(v, x => Clamp(x * 255f)),
            double[] v => Array.ConvertAll(v, x => Clamp((float)x * 255f)),
            // 16-bit channels, down-shifted rather than divided.
            ushort[] v => Array.ConvertAll(v, x => (byte)(x >> 8)),
            short[] v => Array.ConvertAll(v, x => (byte)((ushort)x >> 8)),
            _ => null,
        };

    private static byte Clamp(float v) => (byte)Math.Clamp(v, 0f, 255f);

    private static float[]? Scalars(PlyParser.ElementData element, string name) => element[name]?.Data switch
    {
        null => null,
        float[] v => v,
        double[] v => Array.ConvertAll(v, x => (float)x),
        // Integer positions are legal and appear in voxelised and fixed-point exports.
        int[] v => Array.ConvertAll(v, x => (float)x),
        uint[] v => Array.ConvertAll(v, x => (float)x),
        short[] v => Array.ConvertAll(v, x => (float)x),
        ushort[] v => Array.ConvertAll(v, x => (float)x),
        sbyte[] v => Array.ConvertAll(v, x => (float)x),
        byte[] v => Array.ConvertAll(v, x => (float)x),
        long[] v => Array.ConvertAll(v, x => (float)x),
        ulong[] v => Array.ConvertAll(v, x => (float)x),
        _ => null,
    };

    private static int[] Integers(Array a) => a switch
    {
        int[] v => v,
        uint[] v => Array.ConvertAll(v, x => (int)x),
        short[] v => Array.ConvertAll(v, x => (int)x),
        ushort[] v => Array.ConvertAll(v, x => (int)x),
        sbyte[] v => Array.ConvertAll(v, x => (int)x),
        byte[] v => Array.ConvertAll(v, x => (int)x),
        // A vertex list long enough to need 64 bits is past anything this renderer will draw, so an index that does
        // not fit is a corrupt file rather than a big one: -1 fails Triangulate's range check and the face is dropped.
        long[] v => Array.ConvertAll(v, x => x is >= 0 and <= int.MaxValue ? (int)x : -1),
        ulong[] v => Array.ConvertAll(v, x => x <= int.MaxValue ? (int)x : -1),
        _ => [],
    };

    // The parser's short-read message embeds the whole Element record -- name, count, and the ToString of its
    // property list -- which is most of a screen of noise around the two numbers that matter. Keep the counts.
    private static string Truncated(string complaint)
    {
        var name = Between(complaint, "Name = ", ",");
        var counts = Between(complaint, " after ", " lines");
        return name is null || counts is null
            ? $"truncated PLY: {complaint.Replace("[PlyParser] ", "")}"
            : $"truncated PLY: the file declares more '{name}' rows than it contains (found {counts})";
    }

    private static string? Between(string s, string open, string close)
    {
        var a = s.IndexOf(open, StringComparison.Ordinal);
        if (a < 0) return null;
        a += open.Length;
        var b = s.IndexOf(close, a, StringComparison.Ordinal);
        return b < 0 ? null : s[a..b];
    }

    // The parser's messages are accurate but internal-sounding, and two of them are things a user can act on.
    private static string Explain(string message) =>
        message.Contains("big endian", StringComparison.OrdinalIgnoreCase)
            ? "this PLY is binary_big_endian, which is not supported (re-save it as ascii or binary_little_endian)"
        : message.Contains("Not a ply file", StringComparison.OrdinalIgnoreCase)
            ? "not a PLY file (it does not start with a ply header)"
            : $"could not read the PLY: {message}";
    #endregion

    #region Fields
    // Big enough that every model here parses as one chunk, small enough that a huge point cloud does not size a
    // single allocation from the file's own declared count.
    private const int ChunkBytes = 16 * 1024 * 1024;
    #endregion
}
