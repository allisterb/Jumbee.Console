namespace Jumbee.Console.SandboxDemo;

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
    /// <exception cref="InvalidDataException">The file holds no usable geometry.</exception>
    /// <remarks>An unrecognised extension is read as OBJ rather than rejected: that was the behaviour before there
    /// was a second format, and a text mesh under some other suffix is a likelier thing to meet than a file that
    /// wants refusing.</remarks>
    public static Mesh Load(string path, float radius = 0.5f) => Extension(path) switch
    {
        ".stl" => StlLoader.Load(path, radius),
        ".ply" => PlyLoader.Load(path, radius),
        _ => ObjLoader.Load(path, radius),
    };

    /// <summary>Whether this path names a model this app can read.</summary>
    public static bool IsModel(string path) =>
        Extensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase);
    #endregion

    #region Private methods
    private static string Extension(string path) => Path.GetExtension(path).ToLowerInvariant();
    #endregion

    #region Fields
    /// <summary>The file extensions the app reads, lowercase and dotted.</summary>
    public static readonly string[] Extensions = [".obj", ".stl", ".ply"];

    /// <summary>The same set as file-browser glob patterns.</summary>
    public static readonly string[] Patterns = [.. Extensions.Select(e => "*" + e)];
    #endregion
}
