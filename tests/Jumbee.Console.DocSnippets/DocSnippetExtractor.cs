namespace Jumbee.Console.DocSnippets;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>A single ```csharp fence lifted out of a Markdown doc, with enough provenance to report a failure
/// back at the line the reader would be looking at.</summary>
public sealed record DocSnippet(string DocPath, string DocRelativePath, int StartLine, string Body, string GeneratedSource)
{
    /// <summary>The name the generated syntax tree is filed under; diagnostics are mapped back through this.</summary>
    public string SourceName => $"{DocRelativePath}:{StartLine}";
}

/// <summary>
/// Pulls every C# code fence out of the consumer-facing docs and wraps each one in enough scaffolding to compile.
/// </summary>
/// <remarks>
/// Doc snippets are fragments, not programs, so three things have to be reconciled before the compiler sees them:
/// <list type="bullet">
/// <item>a fence may be statements, a class member, or a whole type — each needs a different wrapper;</item>
/// <item><c>using</c> directives are pooled per <em>document</em>, because a reader works down a page and an import
/// established in one fence is in scope for the fences below it;</item>
/// <item>docs elide with <c>(...)</c> and sometimes show a bare <c>new Thing(...)</c> expression, neither of which is
/// valid C# — both are rewritten in the generated copy only, never in the doc.</item>
/// </list>
/// </remarks>
public static class DocSnippetExtractor
{
    #region Methods
    /// <summary>Walks up from the test binaries to the repository root (identified by GETTING-STARTED.md).</summary>
    public static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "GETTING-STARTED.md")))
            dir = dir.Parent;
        return dir?.FullName ?? throw new InvalidOperationException(
            "Could not locate the repository root (no GETTING-STARTED.md found walking up from the test binaries).");
    }

    /// <summary>The docs whose snippets are checked: the control guides plus the three onboarding pages.</summary>
    public static IReadOnlyList<string> DocFiles(string repoRoot)
    {
        var files = new List<string>();
        var controls = Path.Combine(repoRoot, "docs", "controls");
        if (Directory.Exists(controls)) files.AddRange(Directory.GetFiles(controls, "*.md").OrderBy(f => f));
        foreach (var rel in new[] { "README.md", Path.Combine("package", "README.md"), "GETTING-STARTED.md" })
        {
            var full = Path.Combine(repoRoot, rel);
            if (File.Exists(full)) files.Add(full);
        }
        return files;
    }

    /// <summary>Extracts and wraps every snippet in every doc.</summary>
    public static IReadOnlyList<DocSnippet> Extract(string repoRoot)
    {
        var snippets = new List<DocSnippet>();
        foreach (var doc in DocFiles(repoRoot))
        {
            var rel = Path.GetRelativePath(repoRoot, doc).Replace('\\', '/');
            var fences = Fences(File.ReadAllLines(doc));

            // Pooled per document — see the class remarks.
            var pooled = fences
                .SelectMany(f => f.Body)
                .Select(UsingDirective)
                .Where(u => u is not null)
                .Distinct()
                .OrderBy(l => l, StringComparer.Ordinal)
                .ToArray();

            foreach (var fence in fences)
            {
                var body = string.Join(Environment.NewLine,
                    fence.Body.Where(l => !IsUsingDirective(l)).Select(Rewrite));
                snippets.Add(new DocSnippet(doc, rel, fence.StartLine, body, Wrap(body, pooled, snippets.Count)));
            }
        }

        return snippets;
    }
    #endregion

    #region Private methods
    private static List<(int StartLine, List<string> Body)> Fences(string[] lines)
    {
        var fences = new List<(int, List<string>)>();
        List<string> current = null;
        var start = 0;

        for (var i = 0; i < lines.Length; i++)
        {
            var trimmed = lines[i].Trim();
            if (current is null)
            {
                if (trimmed == "```csharp") { current = []; start = i + 1; }
            }
            else if (trimmed == "```")
            {
                fences.Add((start, current));
                current = null;
            }
            else
            {
                current.Add(lines[i]);
            }
        }

        return fences;
    }

    // `using X;`, `using static X;` and `using X = Y;` are directives to hoist. `using var x = ...;` is a statement
    // and must stay where it is. Docs routinely annotate a directive with a trailing comment, so compare against the
    // code part only — and hoist that part, since the comment would be meaningless at the top of the file.
    private static bool IsUsingDirective(string line) => UsingDirective(line) is not null;

    private static string UsingDirective(string line)
    {
        var t = line.Trim();
        if (!t.StartsWith("using ", StringComparison.Ordinal)) return null;
        if (t.StartsWith("using var ", StringComparison.Ordinal)) return null;

        var comment = t.IndexOf("//", StringComparison.Ordinal);
        if (comment >= 0) t = t[..comment].TrimEnd();
        return t.EndsWith(";", StringComparison.Ordinal) ? t : null;
    }

    private static string Rewrite(string line)
    {
        // "(...)" is the docs' elision marker for arguments the reader supplies.
        var l = line.Replace("(...)", "()");

        // A fence sometimes shows a bare constructor expression to name a shape, e.g. `new Boundary(x, width: 24)`.
        // Make it a statement so it parses; keep any trailing comment.
        var bare = Regex.Match(l, @"^(?<expr>new [A-Za-z][^/]*?)\s*(?<comment>//.*)?$");
        if (bare.Success && !l.TrimEnd().EndsWith(";", StringComparison.Ordinal))
            l = $"_ = {bare.Groups["expr"].Value.TrimEnd()}; {bare.Groups["comment"].Value}".TrimEnd();

        return l;
    }

    // A fence that declares a member or a type cannot be nested inside a method body.
    private static bool IsMemberScope(string body) =>
        Regex.IsMatch(body, @"^\s*(public|internal|private|protected|static|abstract|sealed|partial|override|async Task [A-Za-z_]+\()",
            RegexOptions.Multiline);

    private static string Wrap(string body, IReadOnlyList<string> pooledUsings, int index)
    {
        var sb = new StringBuilder();
        // The SDK's implicit usings, which a consumer's project has by default and this compilation does not.
        sb.AppendLine("using System; using System.Collections.Generic; using System.IO; using System.Linq;");
        sb.AppendLine("using System.Net.Http; using System.Threading; using System.Threading.Tasks; using System.Globalization;");
        sb.AppendLine("using Jumbee.Console; using Spectre.Console.Rendering;");
        foreach (var u in pooledUsings) sb.AppendLine(u);
        sb.AppendLine($"namespace DocSnippet{index} {{ public partial class Snippet : DocSnippetContext {{");
        if (IsMemberScope(body))
        {
            sb.AppendLine(body);
        }
        else
        {
            // async so a fence containing `await` parses.
            sb.AppendLine("public async Task Exec() {");
            sb.AppendLine(body);
            sb.AppendLine("}");
        }
        sb.AppendLine("} }");
        return sb.ToString();
    }
    #endregion
}
