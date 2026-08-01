namespace Jumbee.Console.Tests;

using System;

using Jumbee.Console;
using Jumbee.Console.Documents;
using Jumbee.Console.Snapshot;

using Xunit;

// The Mermaid editor pairs a Mermaid-highlighted CodeEditor with a live MermaidViewer via the shared
// InteractiveSourceEditor base. These exercise the sync path headlessly (the coalesced sync applies inline when the
// UI loop isn't running).
public class InteractiveMermaidEditorTests
{
    private static ConsoleKeyInfo K(ConsoleKey key) => new('\0', key, false, false, false);

    [Fact]
    public void Construction_StartsBothPanesInSync()
    {
        var ed = new InteractiveMermaidEditor("graph TD\n    A --> B");

        Assert.Equal("graph TD\n    A --> B", ed.Editor.Text);
        Assert.Equal("graph TD\n    A --> B", ed.Preview.Mermaid);
    }

    [Fact]
    public void Editing_TheSource_UpdatesThePreview_AndRaisesTextChanged()
    {
        var ed = new InteractiveMermaidEditor("graph TD\n");
        ConsoleSnapshot.Render(ed, 90, 20);

        string? fired = null;
        ed.TextChanged += (_, t) => fired = t;

        ed.Editor.Editor.OnPaste("    A --> B");

        Assert.Equal(ed.Editor.Text, ed.Preview.Mermaid);   // preview source mirrors the editor
        Assert.Contains("A --> B", ed.Preview.Mermaid);
        Assert.Equal(ed.Editor.Text, fired);                // TextChanged carried the new text
    }

    [Fact]
    public void RendersTheEditorPane_WithMermaidSource()
    {
        var ed = new InteractiveMermaidEditor("graph TD\n    A[Start] --> B");
        ed.WithFrame(borderStyle: BorderStyle.None);

        var text = ConsoleSnapshot.ToText(ed, 90, 20);

        Assert.Contains("Mermaid", text);   // editor pane frame title
        Assert.Contains("Diagram", text);   // preview pane frame title
        Assert.Contains("graph", text);     // the source renders in the editor pane
    }

    [Fact]
    public void CaretOnlyMovement_DoesNotRaiseTextChanged()
    {
        var ed = new InteractiveMermaidEditor("graph TD\n    A --> B");
        ConsoleSnapshot.Render(ed, 90, 20);
        UI.SetFocus(ed.Editor.Editor);

        var count = 0;
        ed.TextChanged += (_, _) => count++;

        UI.SendInput(ed.Editor.Editor, K(ConsoleKey.RightArrow));
        UI.SendInput(ed.Editor.Editor, K(ConsoleKey.DownArrow));

        Assert.Equal(0, count);
    }

    [Fact]
    public void SettingText_LoadsTheEditor_AndRefreshesThePreview()
    {
        var ed = new InteractiveMermaidEditor();

        ed.Text = "stateDiagram\n    [*] --> Idle";

        Assert.Equal(ed.Text, ed.Preview.Mermaid);
    }
}
