namespace Jumbee.Console.Tests;

using System;

using Jumbee.Console;
using Jumbee.Console.Documents;
using Jumbee.Console.Snapshot;

using Xunit;

// The extended Markdown editor pairs a Markdown+Mermaid-highlighted CodeEditor with a live MarkdownExtendedViewer
// (which renders embedded ```mermaid blocks as diagrams) via the shared InteractiveSourceEditor base.
public class InteractiveMarkdownExtendedEditorTests
{
    private static ConsoleKeyInfo K(ConsoleKey key) => new('\0', key, false, false, false);

    private const string Doc = "# Title\n\n```mermaid\ngraph TD\n    A --> B\n```\n";

    [Fact]
    public void Construction_StartsBothPanesInSync()
    {
        var ed = new InteractiveMarkdownExtendedEditor(Doc);

        Assert.Equal(Doc, ed.Editor.Text);
        Assert.Equal(Doc, ed.Preview.Markdown);
        Assert.IsType<MarkdownExtendedViewer>(ed.Preview);
    }

    [Fact]
    public void Editing_TheSource_UpdatesThePreview_AndRaisesTextChanged()
    {
        var ed = new InteractiveMarkdownExtendedEditor("# Title\n");
        ConsoleSnapshot.Render(ed, 100, 24);

        string? fired = null;
        ed.TextChanged += (_, t) => fired = t;

        ed.Editor.Editor.OnPaste("\nmore **text**");

        Assert.Equal(ed.Editor.Text, ed.Preview.Markdown);
        Assert.Contains("more **text**", ed.Preview.Markdown);
        Assert.Equal(ed.Editor.Text, fired);
    }

    [Fact]
    public void Preview_RendersAnEmbeddedMermaidDiagram()
    {
        // The extended preview draws the ```mermaid block as a box-drawing diagram (node boxes), not as source text.
        var ed = new InteractiveMarkdownExtendedEditor(Doc, SplitOrientation.Vertical, splitPosition: 12);
        ed.WithFrame(borderStyle: BorderStyle.None);

        var text = ConsoleSnapshot.ToText(ed, 90, 40);

        Assert.Contains("Markdown", text);   // editor pane title
        Assert.Contains("┌", text);          // a node box drawn in the preview (the rendered diagram)
    }

    [Fact]
    public void CaretOnlyMovement_DoesNotRaiseTextChanged()
    {
        var ed = new InteractiveMarkdownExtendedEditor(Doc);
        ConsoleSnapshot.Render(ed, 100, 24);
        UI.SetFocus(ed.Editor.Editor);

        var count = 0;
        ed.TextChanged += (_, _) => count++;

        UI.SendInput(ed.Editor.Editor, K(ConsoleKey.RightArrow));
        UI.SendInput(ed.Editor.Editor, K(ConsoleKey.DownArrow));

        Assert.Equal(0, count);
    }
}
