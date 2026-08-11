namespace Jumbee.Console.SandboxDemo;

/// <summary>What the viewer's single path argument resolved to.</summary>
/// <param name="Files">Every <c>.obj</c> in the directory, in name order. Empty when there are none.</param>
/// <param name="StartIndex">Which of them to open on — the named file, or the first.</param>
/// <param name="Error">A message to print and exit on, or <see langword="null"/> when the path was usable.</param>
public readonly record struct ModelSet(string[] Files, int StartIndex, string? Error);

/// <summary>Turns the viewer's path argument into a list of models to load.</summary>
/// <remarks>
/// Its own type rather than a helper inside <c>Program</c> so the resolution rules — which are all edge cases —
/// can be tested without launching a UI.
/// </remarks>
public static class ModelLibrary
{
    #region Methods
    /// <summary>
    /// Resolves a file, a directory, or nothing into the set of models to load and the one to show first.
    /// </summary>
    /// <remarks>
    /// A named file does <b>not</b> narrow the set — the whole directory is loaded and the named file merely decides
    /// where cycling starts. The useful unit for a viewer is the directory: one you have to restart to see the next
    /// asset is a worse tool than one you step through with a keypress.
    /// </remarks>
    public static ModelSet Resolve(string? path, string? currentDirectory = null)
    {
        string directory;
        string? named = null;

        if (string.IsNullOrWhiteSpace(path))
        {
            directory = currentDirectory ?? Directory.GetCurrentDirectory();
        }
        else if (Directory.Exists(path))
        {
            directory = path;
        }
        else if (File.Exists(path))
        {
            directory = Path.GetDirectoryName(Path.GetFullPath(path)) ?? ".";
            named = Path.GetFullPath(path);
        }
        else
        {
            return new ModelSet([], 0, $"'{path}' is neither a file nor a directory.");
        }

        var files = Directory.GetFiles(directory, "*.obj").OrderBy(f => f, StringComparer.OrdinalIgnoreCase).ToArray();
        if (files.Length == 0)
        {
            // Only a failure when a path was actually named. With no argument the viewer still has its generated
            // mesh to fall back on, so an empty working directory is not an error.
            return new ModelSet([], 0, path is null ? null : $"no .obj files in '{directory}'.");
        }

        var start = 0;
        if (named is not null)
        {
            for (var i = 0; i < files.Length; i++)
            {
                if (string.Equals(Path.GetFullPath(files[i]), named, StringComparison.OrdinalIgnoreCase))
                {
                    start = i;
                    break;
                }
            }
        }

        return new ModelSet(files, start, null);
    }
    #endregion
}
