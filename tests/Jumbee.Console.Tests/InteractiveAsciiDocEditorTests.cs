namespace Jumbee.Console.Tests;

using System;

using Jumbee.Console;
using Jumbee.Console.Documents;
using Jumbee.Console.Snapshot;

using Xunit;

// The AsciiDoc editor pairs an AsciiDoc-highlighted CodeEditor with a live AsciiDocViewer via the shared
// InteractiveSourceEditor base. These exercise the sync path headlessly (the coalesced sync applies inline when the
// UI loop isn't running).
public class InteractiveAsciiDocEditorTests
{
    private static ConsoleKeyInfo K(ConsoleKey key) => new('\0', key, false, false, false);

    [Fact]
    public void Construction_StartsBothPanesInSync()
    {
        var ed = new InteractiveAsciiDocEditor("= Title\n\nBody text.");

        Assert.Equal("= Title\n\nBody text.", ed.Editor.Text);
        Assert.Equal("= Title\n\nBody text.", ed.Preview.AsciiDoc);
    }

    [Fact]
    public void Editing_TheSource_UpdatesThePreview_AndRaisesTextChanged()
    {
        var ed = new InteractiveAsciiDocEditor("= Title\n");
        ConsoleSnapshot.Render(ed, 90, 20);

        string? fired = null;
        ed.TextChanged += (_, t) => fired = t;

        ed.Editor.Editor.OnPaste("\nNOTE: added");

        Assert.Equal(ed.Editor.Text, ed.Preview.AsciiDoc);
        Assert.Contains("NOTE: added", ed.Preview.AsciiDoc);
        Assert.Equal(ed.Editor.Text, fired);
    }

    [Fact]
    public void RendersTheEditorPane_WithAsciiDocSource()
    {
        var ed = new InteractiveAsciiDocEditor("= My Title\n\nSome text.");
        ed.WithFrame(borderStyle: BorderStyle.None);

        var text = ConsoleSnapshot.ToText(ed, 90, 20);

        Assert.Contains("AsciiDoc", text);   // editor pane frame title
        Assert.Contains("Preview", text);     // preview pane frame title
        Assert.Contains("My Title", text);    // source renders in the editor pane
    }

    [Fact]
    public void CaretOnlyMovement_DoesNotRaiseTextChanged()
    {
        var ed = new InteractiveAsciiDocEditor("= Title\n\nBody.");
        ConsoleSnapshot.Render(ed, 90, 20);
        UI.SetFocus(ed.Editor.Editor);

        var count = 0;
        ed.TextChanged += (_, _) => count++;

        UI.SendInput(ed.Editor.Editor, K(ConsoleKey.RightArrow));
        UI.SendInput(ed.Editor.Editor, K(ConsoleKey.DownArrow));

        Assert.Equal(0, count);
    }
}
