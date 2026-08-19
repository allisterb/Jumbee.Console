namespace Jumbee.Console.SandboxDemo;

using System.Buffers.Binary;
using System.Globalization;
using System.Numerics;

/// <summary>
/// An STL reader, binary and ASCII. Geometry only, which is all the format carries.
/// </summary>
/// <remarks>
/// <para>
/// STL is a <b>triangle soup</b>: each facet repeats its three corner positions in full, with no vertex sharing, no
/// materials and no texture coordinates. It is the CAD and 3D-printing interchange format, which is the reason to
/// support it — those files exist in enormous numbers and nothing else reads them here.
/// </para>
/// <para>
/// Corners are <b>welded on exact equality</b> as they are read, because the rest of the renderer assumes indexed
/// geometry — a body's vertices are transformed once per frame and referenced by its triangles, so a soup pays for
/// every corner over again. Measured on the reference bee: 7,653 corners down to <b>1,286</b> vertices, a factor of
/// six, and the same 2,551 triangles. Exact rather than tolerant matching on purpose: an exporter writes the same
/// bits for a shared corner, and a tolerance wide enough to catch the rest would also merge corners a model meant
/// to keep apart.
/// </para>
/// <para>
/// The degeneracy guard is <b>absolute, and runs in the file's own units</b>, which is worth knowing rather than
/// fixing. It catches the exactly-collapsed facets every scanned model carries (17 of the bee's 2,568), but a facet
/// can still be a sliver: a dozen of the bee's survive at an area six to nine orders of magnitude below the median,
/// and one underflows to zero once the model is scaled down. Their winding direction is numerically meaningless, so
/// whether the cull keeps them is arbitrary — and irrelevant, since they cover no pixels either way. A threshold
/// relative to the model would classify them, at the price of a heuristic on the load path for no visible gain.
/// </para>
/// <para>
/// <b>Z-up by default.</b> The format has no axis convention at all, but it comes overwhelmingly from tools where Z
/// is up. Same treatment as the OBJ exporter banner, and for the same reason: a default rather than a fact, applied
/// where it is visible and undoable (the viewer's Z-up switch, the <c>a</c> key) rather than silently.
/// </para>
/// </remarks>
public static class StlLoader
{
    #region Methods
    /// <summary>Loads an STL file, binary or ASCII, centred and scaled like <see cref="ObjLoader.Load"/>.</summary>
    /// <exception cref="InvalidDataException">The file holds no usable triangles.</exception>
    public static Mesh Load(string path, float radius = 0.5f)
    {
        using var stream = File.OpenRead(path);
        Span<byte> prefix = stackalloc byte[HeaderBytes + 4];
        var read = stream.ReadAtLeast(prefix, prefix.Length, throwOnEndOfStream: false);

        if (read == prefix.Length && IsBinary(stream.Length, prefix))
            return ParseBinary(stream, BinaryPrimitives.ReadUInt32LittleEndian(prefix[HeaderBytes..]), radius);

        // Not binary: re-read as text. Streamed rather than loaded whole, as the OBJ path is — an ASCII STL of the
        // same model is roughly thirty times the size of its binary form.
        return ParseAscii(File.ReadLines(path), radius);
    }

    /// <summary>
    /// Whether a file of this length, starting with these bytes, is binary STL.
    /// </summary>
    /// <remarks>
    /// <b>Arithmetic, not the <c>solid</c> prefix.</b> The obvious test — ASCII files begin with <c>solid</c> — is
    /// what most readers use and it is wrong: a binary STL's 80-byte header is free-form, and several exporters
    /// write a description into it that begins with the same word. A binary file's length is fully determined
    /// (<c>84 + n × 50</c>), so asking whether the length matches the count it declares identifies the format
    /// exactly. It also makes the count itself trustworthy, which is what lets the reader below size a buffer from
    /// it without a sanity cap.
    /// </remarks>
    public static bool IsBinary(long length, ReadOnlySpan<byte> prefix) =>
        prefix.Length >= HeaderBytes + 4 &&
        length == HeaderBytes + 4 + ((long)BinaryPrimitives.ReadUInt32LittleEndian(prefix[HeaderBytes..]) * FacetBytes);

    /// <summary>Parses the facets of a binary STL, from a stream positioned just past the count.</summary>
    public static Mesh ParseBinary(Stream stream, uint facets, float radius = 0.5f)
    {
        var builder = new Builder((int)facets);
        Span<byte> facet = stackalloc byte[FacetBytes];
        for (var i = 0u; i < facets; i++)
        {
            stream.ReadExactly(facet);
            builder.Add(Vector(facet), Vector(facet[12..]), Vector(facet[24..]), Vector(facet[36..]));
        }

        return builder.Build(radius);
    }

    /// <summary>Parses ASCII STL text. Split out so it can be tested without a file.</summary>
    public static Mesh ParseAscii(IEnumerable<string> lines, float radius = 0.5f)
    {
        var builder = new Builder(0);
        var normal = Vector3.Zero;
        Vector3 a = default, b = default;
        var corner = 0;

        foreach (var line in lines)
        {
            var span = line.AsSpan().Trim();
            // Only two keywords matter. `outer loop`, `endloop`, `endfacet` and `endsolid` carry no data, and a
            // reader that ignores them handles the malformed-but-common files that omit one.
            if (span.StartsWith("facet", StringComparison.OrdinalIgnoreCase))
            {
                normal = Triple(span, skip: 2);   // "facet normal ni nj nk"
                corner = 0;
            }
            else if (span.StartsWith("vertex", StringComparison.OrdinalIgnoreCase))
            {
                var v = Triple(span, skip: 1);
                if (corner == 0) a = v;
                else if (corner == 1) b = v;
                else if (corner == 2) builder.Add(normal, a, b, v);
                corner++;
            }
        }

        return builder.Build(radius);
    }
    #endregion

    #region Private methods
    private static Vector3 Vector(ReadOnlySpan<byte> at) => new(
        BinaryPrimitives.ReadSingleLittleEndian(at),
        BinaryPrimitives.ReadSingleLittleEndian(at[4..]),
        BinaryPrimitives.ReadSingleLittleEndian(at[8..]));

    // The three numbers after `skip` words. Tolerant of the extra spacing STL writers indent with.
    private static Vector3 Triple(ReadOnlySpan<char> span, int skip)
    {
        Span<float> values = [0f, 0f, 0f];
        var found = 0;
        foreach (var range in span.Split(' '))
        {
            var word = span[range].Trim();
            if (word.IsEmpty) continue;
            if (skip > 0) { skip--; continue; }
            if (found < 3 && float.TryParse(word, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                values[found++] = value;
        }

        return new Vector3(values[0], values[1], values[2]);
    }
    #endregion

    #region Fields
    private const int HeaderBytes = 80;
    private const int FacetBytes = 50;   // 12 floats + a 2-byte attribute word
    #endregion

    #region Child types
    // Accumulates welded corners and triangles for either reader.
    private sealed class Builder(int facets)
    {
        public void Add(Vector3 normal, Vector3 a, Vector3 b, Vector3 c)
        {
            // Drop a degenerate facet rather than emitting a triangle the rasteriser will reject anyway.
            var winding = Vector3.Cross(b - a, c - a);
            if (winding.LengthSquared() < 1e-20f) return;

            // THE STORED NORMAL IS USED FOR EXACTLY ONE THING, and it is not lighting: the renderers derive their
            // own normal from the winding, and back-face culling keys off the SIGN of that. STL files in the wild
            // routinely have facets wound the wrong way round — the normal is what their authoring tool trusted —
            // so a mesh taken purely at its winding loses scattered facets to the cull and reads as full of holes.
            // Where the file's own normal disagrees, swap two corners so the winding agrees with it. Facets whose
            // normal is absent or zero (also common) keep their winding, which is the only information left.
            if (Vector3.Dot(winding, normal) < 0f) (b, c) = (c, b);

            indices.Add(Index(a));
            indices.Add(Index(b));
            indices.Add(Index(c));
        }

        public Mesh Build(float radius)
        {
            if (indices.Count < 3)
                throw new InvalidDataException("no usable geometry in the STL (needs at least one facet)");

            ObjLoader.Normalise(vertices, radius);
            return new Mesh([.. vertices], [.. indices]) { AuthoredUpAxis = ModelUpAxis.Z };
        }

        private int Index(Vector3 v)
        {
            if (welded.TryGetValue(v, out var existing)) return existing;
            var index = vertices.Count;
            vertices.Add(v);
            welded[v] = index;
            return index;
        }

        private readonly List<Vector3> vertices = new(facets);
        private readonly List<int> indices = new(facets * 3);
        private readonly Dictionary<Vector3, int> welded = new(facets);
    }
    #endregion
}
