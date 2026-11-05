namespace Jumbee.Console.SandboxDemo;

using System.Numerics;

/// <summary>Loads a model file of any supported format, chosen by extension.</summary>
/// <remarks>
/// One place that knows which formats exist, so adding another means touching this file rather than hunting for
/// every <c>*.obj</c> in the app — the file browser's filter, the viewer's directory scan and its "nothing here"
/// message all read <see cref="Extensions"/> from here. Adding PLY was two lines here and one new loader, which is
/// the arrangement working.
/// </remarks>
public static class ModelLoader
{
    #region Methods
    /// <summary>Loads a model, centred on its bounding box and scaled so its largest half-extent is
    /// <paramref name="radius"/>.</summary>
    /// <param name="path">The file to read.</param>
    /// <param name="radius">The half-extent the model's largest axis is scaled to.</param>
    /// <param name="withUvs">Read texture coordinates even when no material needs them — for the harness, which maps
    /// procedural sources through a model's UVs. An OBJ whose materials have a map gets them regardless. Only OBJ
    /// has UVs here: STL has no such concept and PLY's multi-texture scheme is out of scope.</param>
    /// <param name="withMaterials">Resolve an OBJ's <c>mtllib</c>/<c>usemtl</c> into <see cref="Mesh.Materials"/>,
    /// loading any maps. On by default.</param>
    /// <exception cref="InvalidDataException">The file holds no usable geometry, or a map its materials name exists
    /// but cannot be decoded.</exception>
    /// <remarks>
    /// <para>
    /// An unrecognised extension is read as OBJ rather than rejected: that was the behaviour before there was a second
    /// format, and a text mesh under some other suffix is a likelier thing to meet than a file that wants refusing.
    /// </para>
    /// <para>
    /// <b>Absent and broken are treated differently, on purpose.</b> A missing <c>.mtl</c> or map file is common —
    /// models are downloaded without them — and the model loads as it always did: no <c>.mtl</c> means no materials,
    /// a missing map means that material shows its <c>Kd</c>. A map that <em>exists and will not decode</em> is a
    /// parse failure, and fails the load with the file named, through the same channels any unreadable model uses.
    /// Drawing it untextured instead would be a parse failure read as no data.
    /// </para>
    /// </remarks>
    public static Mesh Load(string path, float radius = 0.5f, bool withUvs = false, bool withMaterials = true) =>
        Extension(path) switch
        {
            ".stl" => StlLoader.Load(path, radius),
            ".ply" => PlyLoader.Load(path, radius),
            _ => LoadObj(path, radius, withUvs, withMaterials),
        };

    /// <summary>Whether this path names a model this app can read.</summary>
    public static bool IsModel(string path) =>
        Extensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase);
    #endregion

    #region Private methods
    private static string Extension(string path) => Path.GetExtension(path).ToLowerInvariant();

    private static Mesh LoadObj(string path, float radius, bool withUvs, bool withMaterials)
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(path)) ?? ".";
        var library = withMaterials ? ReadMaterialLibraries(path, directory) : [];

        // UVs cost ~40% more parse time on a large model, so they are read only if a map will actually be drawn
        // through them -- a material whose map file is missing does not count.
        var mesh = ObjLoader.Load(path, radius, withUvs || library.Values.Any(m => m.Map is not null));
        if (mesh.MaterialNames is not { } names || library.Count == 0) return mesh;

        // A usemtl naming nothing in the library gets MTL's default: white, no map. If NO name resolved, the
        // library was the wrong one, and the materials are dropped rather than drawn as a model's worth of white.
        if (!names.Any(library.ContainsKey)) return mesh;
        return mesh.WithMaterials([.. names.Select(n => library.TryGetValue(n, out var m) ? m : new Material(n, White, null))]);
    }

    // Every material in the file's mtllib(s), by name, each with its map loaded -- or with none if the map file is
    // absent. Reads only the file's opening lines: mtllib comes before any geometry in every OBJ measured, and
    // scanning a 219 MB model for it would cost more than the parse it precedes.
    private static Dictionary<string, Material> ReadMaterialLibraries(string objPath, string directory)
    {
        var result = new Dictionary<string, Material>(StringComparer.Ordinal);
        foreach (var library in MaterialLibraryNames(objPath))
        {
            var mtlPath = Path.Combine(directory, library);
            if (!File.Exists(mtlPath)) continue;
            var mtlDirectory = Path.GetDirectoryName(mtlPath) ?? directory;

            foreach (var d in MtlLoader.Load(mtlPath))
            {
                Texture? map = null;
                if (d.MapKd is { } mapName)
                {
                    // Exporters write Windows separators; resolve them on any platform.
                    var mapPath = Path.Combine(mtlDirectory, mapName.Replace('\\', Path.DirectorySeparatorChar));
                    if (File.Exists(mapPath)) map = Texture.Load(mapPath);   // a broken one throws, by design
                }

                // MTL's rule: Kd multiplies the map. So the flat colour is Kd times the map's average -- which is why
                // capsule.mtl's Kd 1 1 1 does not draw it white.
                var flat = map is null ? d.Kd : d.Kd * new Vector3(map.Average.R, map.Average.G, map.Average.B) / 255f;
                var colour = new Color((byte)MathF.Round(flat.X * 255), (byte)MathF.Round(flat.Y * 255), (byte)MathF.Round(flat.Z * 255));
                result[d.Name] = new Material(d.Name, colour, map);
            }
        }

        return result;
    }

    private static IEnumerable<string> MaterialLibraryNames(string objPath)
    {
        foreach (var raw in File.ReadLines(objPath).Take(MaxHeaderLines))
        {
            var line = raw.TrimStart();
            if (line.StartsWith("v ") || line.StartsWith("f ")) yield break;
            if (!line.StartsWith("mtllib ")) continue;
            yield return line[7..].Trim();
        }
    }
    #endregion

    #region Fields
    /// <summary>The file extensions the app reads, lowercase and dotted.</summary>
    public static readonly string[] Extensions = [".obj", ".stl", ".ply"];

    /// <summary>The same set as file-browser glob patterns.</summary>
    public static readonly string[] Patterns = [.. Extensions.Select(e => "*" + e)];

    // How far into an OBJ to look for mtllib before giving up; the surveyed files declare it within 8 lines.
    private const int MaxHeaderLines = 200;

    private static readonly Color White = new(255, 255, 255);
    #endregion
}
