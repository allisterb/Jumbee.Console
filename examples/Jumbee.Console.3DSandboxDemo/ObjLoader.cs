namespace Jumbee.Console.SandboxDemo;

using System.Globalization;
using System.Numerics;

/// <summary>
/// A minimal Wavefront OBJ reader: enough to turn a downloaded model into something the sandbox can draw and
/// simulate. Geometry only — no materials, no texture coordinates.
/// </summary>
/// <remarks>
/// <para>
/// Two details do the real work. <b>Faces are fan-triangulated</b>, because plenty of real models (the Utah teapot
/// among them) store quads or larger n-gons and a rasteriser wants triangles. And the result is <b>centred and
/// normalised</b> to a unit size, so a model authored at any scale drops into the sandbox next to the unit cubes
/// instead of a hundred metres away or invisibly small.
/// </para>
/// <para>
/// Vertex normals (<c>vn</c>) are parsed past but not kept: the renderers derive a face normal from the winding, so
/// a loaded model is flat-shaded like everything else. Smooth normals would mean interpolating them per pixel,
/// which works against the quantised shading the emission budget depends on.
/// </para>
/// </remarks>
public static class ObjLoader
{
    #region Methods
    /// <summary>Loads an OBJ file, centred on its bounding-box midpoint and scaled so its largest half-extent is
    /// <paramref name="radius"/>.</summary>
    /// <exception cref="InvalidDataException">The file holds no usable triangles.</exception>
    public static Mesh Load(string path, float radius = 0.5f) =>
        Parse(File.ReadLines(path), radius);

    /// <summary>Parses OBJ text. Split out from <see cref="Load"/> so it can be tested without a file.</summary>
    public static Mesh Parse(IEnumerable<string> lines, float radius = 0.5f)
    {
        var vertices = new List<Vector3>();
        var indices = new List<int>();
        ModelUpAxis? authoredUpAxis = null;

        foreach (var line in lines)
        {
            var span = line.AsSpan().Trim();
            if (span.Length < 2) continue;

            // The exporter banner, which OBJ writers put in the first few comment lines. Only comments before any
            // geometry are considered, so a stray mention deeper in the file cannot flip a model over.
            if (span[0] == '#')
            {
                if (vertices.Count == 0 && authoredUpAxis is null && IsZUpExporter(span)) authoredUpAxis = ModelUpAxis.Z;
                continue;
            }

            if (span[0] == 'v' && span[1] == ' ')
            {
                var parts = Split(span[2..]);
                if (parts.Count >= 3) vertices.Add(new Vector3(Number(parts[0]), Number(parts[1]), Number(parts[2])));
            }
            else if (span[0] == 'f' && span[1] == ' ')
            {
                var parts = Split(span[2..]);
                if (parts.Count < 3) continue;

                // Fan-triangulate: (0,1,2), (0,2,3), (0,3,4)... Correct for the convex polygons OBJ faces are
                // supposed to be, and the same thing both reference loaders do.
                for (var i = 1; i < parts.Count - 1; i++)
                {
                    indices.Add(VertexIndex(parts[0], vertices.Count));
                    indices.Add(VertexIndex(parts[i], vertices.Count));
                    indices.Add(VertexIndex(parts[i + 1], vertices.Count));
                }
            }
        }

        if (vertices.Count == 0 || indices.Count < 3)
            throw new InvalidDataException("no usable geometry in the OBJ (needs at least one triangular face)");

        // Drop triangles that reference a vertex the file never defined, rather than throwing: a malformed line in
        // an otherwise fine model should cost that face, not the model.
        var clean = new List<int>(indices.Count);
        for (var i = 0; i + 2 < indices.Count; i += 3)
        {
            if (indices[i] < 0 || indices[i + 1] < 0 || indices[i + 2] < 0) continue;
            if (indices[i] >= vertices.Count || indices[i + 1] >= vertices.Count || indices[i + 2] >= vertices.Count) continue;
            clean.Add(indices[i]);
            clean.Add(indices[i + 1]);
            clean.Add(indices[i + 2]);
        }

        if (clean.Count < 3) throw new InvalidDataException("every face in the OBJ referenced a missing vertex");

        Normalise(vertices, radius);
        return new Mesh([.. vertices], [.. clean]) { AuthoredUpAxis = authoredUpAxis };
    }
    #endregion

    #region Private methods
    // Exporters whose OBJ output is conventionally Z-up, recognised by the banner they write into the first
    // comment lines.
    //
    // A DEFAULT, not a fact, and worth being honest about the difference: the banner names the exporter, and both
    // of these have a Y/Z-up option that writes the same header either way. It is right often enough to be worth
    // applying, and the viewer applies it somewhere VISIBLE — the sidebar's Z-up switch — so a wrong guess reads as
    // a setting to flip rather than as a broken loader. A silent rotation would not be worth the risk.
    //
    // The geometric alternative is worse, not better. "The up axis is the smallest extent" misfires on the
    // reference cow, whose extents are (0.500, 0.306, 0.163): Z is smallest and the model is Y-up.
    private static bool IsZUpExporter(ReadOnlySpan<char> comment)
    {
        foreach (var banner in ZUpExporters)
            if (comment.Contains(banner, StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }
    // Centre on the bounding box and scale so the largest half-extent matches `radius`. Uniform, so the model is
    // not distorted; using the box rather than the centroid keeps a lopsided model inside its own bounds.
    private static void Normalise(List<Vector3> vertices, float radius)
    {
        var min = new Vector3(float.MaxValue);
        var max = new Vector3(float.MinValue);
        foreach (var v in vertices)
        {
            min = Vector3.Min(min, v);
            max = Vector3.Max(max, v);
        }

        var centre = (min + max) * 0.5f;
        var extent = (max - min) * 0.5f;
        var largest = MathF.Max(extent.X, MathF.Max(extent.Y, extent.Z));
        var scale = largest > 1e-6f ? radius / largest : 1f;

        for (var i = 0; i < vertices.Count; i++) vertices[i] = (vertices[i] - centre) * scale;
    }

    // An OBJ face vertex is `v`, `v/vt`, `v//vn` or `v/vt/vn`; only the first field is geometry. Indices are
    // 1-based, and NEGATIVE indices count back from the most recent vertex.
    private static int VertexIndex(string token, int vertexCount)
    {
        var slash = token.IndexOf('/');
        var text = slash < 0 ? token : token[..slash];
        if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var index)) return -1;
        return index > 0 ? index - 1 : vertexCount + index;
    }

    private static List<string> Split(ReadOnlySpan<char> span)
    {
        var parts = new List<string>(4);
        foreach (var range in span.Split(' '))
        {
            var part = span[range].Trim();
            if (!part.IsEmpty) parts.Add(part.ToString());
        }

        return parts;
    }

    private static float Number(string text) =>
        float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) ? value : 0f;
    #endregion

    #region Fields
    private static readonly string[] ZUpExporters = ["3ds Max Wavefront OBJ Exporter", "SolidWorks", "AutoCAD"];
    #endregion
}
