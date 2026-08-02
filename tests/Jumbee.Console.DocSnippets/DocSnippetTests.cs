namespace Jumbee.Console.DocSnippets;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Xunit;

/// <summary>
/// Compiles every C# code fence in the consumer-facing docs against the real assemblies, so a snippet cannot
/// silently rot when the API moves.
/// </summary>
/// <remarks>
/// <para>
/// Snippets are illustrative fragments: they refer to variables and methods the reader supplies (<c>plot</c>,
/// <c>status</c>, your own <c>Save()</c>). Those surface as <c>CS0103</c> and are the one diagnostic ignored here.
/// Everything else — a member that does not exist, a wrong delegate shape, a constructor with the wrong arity, a
/// missing <c>using</c> — fails the test with the doc file and line to look at.
/// </para>
/// <para>
/// This has earned its keep: it has caught a missing semicolon in the repository README's first-app example, four
/// invented API members, five handlers written against the wrong delegate type, and an ambiguous <c>Style</c>
/// reference that only appears once a reader imports both <c>Jumbee.Console</c> and <c>Spectre.Console</c>.
/// </para>
/// </remarks>
public class DocSnippetTests
{
    #region Constants
    /// <summary>Identifiers the reader is expected to supply. See the class remarks.</summary>
    private const string ReaderSuppliedIdentifier = "CS0103";

    /// <summary>A floor, not a target: it exists so a broken doc path cannot make this suite vacuously green.</summary>
    private const int MinimumExpectedSnippets = 80;
    #endregion

    #region Tests
    [Fact]
    public void EveryDocumentedSnippetCompiles()
    {
        var snippets = DocSnippetExtractor.Extract(DocSnippetExtractor.FindRepoRoot());
        var trees = snippets
            .Select(s => CSharpSyntaxTree.ParseText(
                s.GeneratedSource,
                new CSharpParseOptions(LanguageVersion.Preview),
                path: s.SourceName))
            .Append(CSharpSyntaxTree.ParseText(SupportSource, new CSharpParseOptions(LanguageVersion.Preview), path: "<support>"))
            .ToArray();

        var compilation = CSharpCompilation.Create(
            "DocSnippets",
            trees,
            ReferencedAssemblies(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: false));

        var failures = compilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error && d.Id != ReaderSuppliedIdentifier)
            .ToArray();

        Assert.True(failures.Length == 0, Describe(failures, snippets));
    }

    [Fact]
    public void TheExpectedDocsAreBeingScanned()
    {
        var root = DocSnippetExtractor.FindRepoRoot();
        var docs = DocSnippetExtractor.DocFiles(root).Select(d => Path.GetRelativePath(root, d).Replace('\\', '/')).ToArray();

        // The onboarding pages are the ones most likely to be silently dropped by a path change, and the ones whose
        // snippets people actually copy first.
        Assert.Contains("README.md", docs);
        Assert.Contains("package/README.md", docs);
        Assert.Contains("GETTING-STARTED.md", docs);
        Assert.Contains(docs, d => d.StartsWith("docs/controls/", StringComparison.Ordinal));

        var snippets = DocSnippetExtractor.Extract(root);
        Assert.True(snippets.Count >= MinimumExpectedSnippets,
            $"Only {snippets.Count} snippets were extracted from {docs.Length} docs; expected at least " +
            $"{MinimumExpectedSnippets}. Extraction is probably broken rather than the docs being empty.");
    }
    #endregion

    #region Private methods
    // Everything on the trusted-platform-assemblies list: the framework plus every DLL copied next to the test
    // binaries, which is where Jumbee.Console and the forked assemblies land.
    private static IReadOnlyList<MetadataReference> ReferencedAssemblies() =>
        [.. ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Where(p => p.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) && File.Exists(p))
            .Select(p => (MetadataReference)MetadataReference.CreateFromFile(p))];

    private static string Describe(IReadOnlyList<Diagnostic> failures, IReadOnlyList<DocSnippet> snippets)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{failures.Count} doc snippet error(s) — each is a real defect in the documentation:");
        sb.AppendLine();

        foreach (var group in failures.GroupBy(d => d.Location.SourceTree?.FilePath ?? "<unknown>").OrderBy(g => g.Key))
        {
            // The tree path is "<doc>:<fence start line>"; the offset within the generated wrapper is not the
            // reader's line number, so point at the fence and let the message do the rest.
            sb.AppendLine($"  {group.Key}");
            foreach (var d in group.OrderBy(d => d.Location.SourceSpan.Start))
                sb.AppendLine($"    {d.Id}: {d.GetMessage()}");
            sb.AppendLine();
        }

        sb.AppendLine($"({snippets.Count} snippets checked. CS0103 is ignored — those are identifiers the reader supplies.)");
        return sb.ToString();
    }
    #endregion

    #region Fields
    // Compiled alongside the snippets. Stand-ins for the app-domain types and the test framework the docs refer to,
    // so those references resolve and the Jumbee API usage around them is actually checked.
    private const string SupportSource = """
        using System;
        using Jumbee.Console;

        public sealed record ProcessGroup(string Name, double Cpu);

        // Minimal stand-in for xunit's Assert, used by the "testing without a terminal" snippets. Nothing runs.
        public static class Assert
        {
            public static void Contains(string expected, string actual) { }
            public static void DoesNotContain(string expected, string actual) { }
            public static void Equal<T>(T expected, T actual) { }
            public static void True(bool condition, string message = null) { }
            public static void False(bool condition, string message = null) { }
            public static void NotNull(object o) { }
        }

        // Snippets are written as if inside a control or an app class, and refer to a root layout.
        public class DocSnippetContext : Control
        {
            protected ILayout root, body;
            protected override void Render() { }
        }
        """;
    #endregion
}
