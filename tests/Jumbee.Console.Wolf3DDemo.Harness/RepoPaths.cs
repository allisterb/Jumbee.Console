/// <summary>
/// The repository root, found by walking up from the harness binary rather than hard-coded.
/// </summary>
/// <remarks>
/// <para>
/// Every asset path in this harness used to be an absolute <c>C:\Projects\Jumbee.Console\...</c> literal, which
/// worked on exactly one machine. Now that the harness is in the solution and builds everywhere, so do the paths.
/// </para>
/// <para>
/// It anchors on <c>src/Jumbee.Console.sln</c> rather than on <c>.git</c>, because a checkout used as a submodule
/// or exported without history still has the solution. The failure is a <b>throw</b>, deliberately: a harness that
/// silently fell back to the working directory would report missing models as check failures and send the next
/// session hunting a rendering bug that does not exist.
/// </para>
/// </remarks>
internal static class RepoPaths
{
    #region Properties
    /// <summary>Absolute path of the repository root.</summary>
    public static string Root { get; } = Find();
    #endregion

    #region Methods
    /// <summary>A path under the repository root, from its segments.</summary>
    public static string At(params string[] parts) => Path.Combine([Root, .. parts]);

    /// <summary>A path under the repository root, or <see langword="null"/> when nothing is there — for the
    /// optional assets (<c>media/</c> is gitignored, <c>reference/</c> may not be present).</summary>
    public static string? Optional(params string[] parts)
    {
        var path = At(parts);
        return File.Exists(path) || Directory.Exists(path) ? path : null;
    }
    #endregion

    #region Private methods
    private static string Find()
    {
        for (var d = new DirectoryInfo(AppContext.BaseDirectory); d is not null; d = d.Parent)
            if (File.Exists(Path.Combine(d.FullName, "src", "Jumbee.Console.sln")))
                return d.FullName;

        throw new InvalidOperationException(
            $"repository root (the directory holding src/Jumbee.Console.sln) not found above {AppContext.BaseDirectory}");
    }
    #endregion
}
