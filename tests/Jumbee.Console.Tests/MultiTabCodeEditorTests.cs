namespace Jumbee.Console.Tests;

using ConsoleGUI.Input;

using Jumbee.Console;
using Jumbee.Console.Snapshot;

using Xunit;

public class MultiTabCodeEditorTests
{
    private static ConsoleKeyInfo K(ConsoleKey k) => new('\0', k, false, false, false);

    // A laid-out overlay for hosting the confirm-on-close dialog, kept off the global UI.Overlay to stay isolated.
    private static Overlay Overlay()
    {
        var overlay = new Overlay(new Grid([1], [40], [[new TextInput(placeholder: "bg")]]));
        ConsoleSnapshot.Render(overlay, 60, 20);
        return overlay;
    }

    private static void Send(Overlay overlay, ConsoleKey key)
    {
        overlay.OnInput(new UI.InputEventArgs(new InputEvent(K(key))));
        ConsoleSnapshot.Render(overlay, 60, 20);
    }


    [Fact]
    public void OpenDocument_AddsTab_SelectsIt_AndReturnsTheEditor()
    {
        var group = new MultiTabCodeEditor(Language.CSharp);

        var editor = group.OpenDocument("main.cs", "class C { }");

        Assert.Equal(1, group.DocumentCount);
        Assert.Same(editor, group.ActiveEditor);
        Assert.Equal("main.cs", group.ActiveDocumentName);
        Assert.Equal("class C { }", editor.Text);
    }

    [Fact]
    public void ActiveEditor_TracksSelection()
    {
        var group = new MultiTabCodeEditor();
        var a = group.OpenDocument("a.txt", "aaa");
        var b = group.OpenDocument("b.txt", "bbb");   // opening selects the latest

        Assert.Same(b, group.ActiveEditor);

        group.Tabs.SelectedIndex = 0;
        Assert.Same(a, group.ActiveEditor);
    }

    [Fact]
    public void Framed_MouseWheelOverEditor_ScrollsThePerTabFrame_NotTheOuterFrame()
    {
        // Reproduces the source-pane arrangement: a tall document in a MultiTabCodeEditor wrapped in an outer
        // container frame. Because the group fills the outer frame's viewport (it never scrolls itself), the wheel
        // must scroll the active editor's own per-tab frame — not the outer frame, which must stay put.
        var group = new MultiTabCodeEditor(Language.None);
        var lines = new string[30];
        for (var i = 0; i < 30; i++) lines[i] = $"line {i + 1:00}";
        var editor = group.OpenDocument("big.txt", string.Join("\n", lines));
        group.WithFrame(title: "Source");

        ConsoleSnapshot.Render(group, 24, 10);   // viewport far shorter than the 30 content rows

        var outerFrame = group.Frame!;
        var innerFrame = editor.Frame!;          // the per-tab scroll frame
        Assert.NotSame(outerFrame, innerFrame);
        Assert.Equal(0, innerFrame.Top);

        // A wheel notch over the editor: TextEditor.MouseWheeled bubbles to CodeEditor.Frame.Scroll (the per-tab frame).
        ((IMouseWheelListener)editor.Editor).OnMouseWheel(new ConsoleGUI.Space.Position(0, 0), 3);

        Assert.True(innerFrame.Top > 0);         // the per-tab frame scrolled
        Assert.Equal(0, outerFrame.Top);         // the outer container frame did not (it fills its viewport)
    }

    [Fact]
    public void NewDocument_OpensAnUntitledEditor()
    {
        var group = new MultiTabCodeEditor();

        group.NewDocument();
        group.NewDocument();

        Assert.Equal(2, group.DocumentCount);
        Assert.Equal("untitled-2", group.ActiveDocumentName);
    }

    [Fact]
    public void CloseDocument_RemovesTab_AndRaisesClosed()
    {
        var group = new MultiTabCodeEditor();
        var a = group.OpenDocument("a", "1");
        var b = group.OpenDocument("b", "2");
        CodeEditor? closed = null;
        group.DocumentClosed += (_, e) => closed = e;

        group.CloseDocument(b);

        Assert.Equal(1, group.DocumentCount);
        Assert.Same(b, closed);
        Assert.Same(a, group.ActiveEditor);
    }

    [Fact]
    public void DocumentClosing_CanCancel_KeepingTheDocument()
    {
        var group = new MultiTabCodeEditor();
        var a = group.OpenDocument("a", "1");
        group.DocumentClosing += (_, e) => e.Cancel = true;

        group.CloseDocument(a);

        Assert.Equal(1, group.DocumentCount);
        Assert.Same(a, group.ActiveEditor);
    }

    [Fact]
    public void SetDirty_TogglesAMarkerOnTheTabLabel()
    {
        var group = new MultiTabCodeEditor();
        var a = group.OpenDocument("a.cs", "x");

        group.SetDirty(a, true);
        Assert.StartsWith("● ", group.ActiveDocumentName);

        group.SetDirty(a, false);
        Assert.Equal("a.cs", group.ActiveDocumentName);
    }

    [Fact]
    public void RendersActiveEditorContent_AndTabBar()
    {
        var group = new MultiTabCodeEditor();
        group.OpenDocument("readme", "hello world");

        var text = ConsoleSnapshot.ToText(group, 40, 8);

        Assert.Contains("readme", text);        // tab label in the bar
        Assert.Contains("hello world", text);   // active editor content
    }

    [Fact]
    public void Clear_ClosesAllDocuments_AndRaisesClosedForEach()
    {
        var group = new MultiTabCodeEditor();
        var a = group.OpenDocument("a", "1");
        var b = group.OpenDocument("b", "2");
        var closed = new System.Collections.Generic.List<CodeEditor>();
        group.DocumentClosed += (_, e) => closed.Add(e);

        group.Clear();

        Assert.Equal(0, group.DocumentCount);
        Assert.Null(group.ActiveEditor);
        Assert.Equal(2, closed.Count);
        Assert.Contains(a, closed);
        Assert.Contains(b, closed);
    }

    [Fact]
    public void OpenDocument_NonClosable_PinsTheTab()
    {
        var group = new MultiTabCodeEditor();

        group.OpenDocument("pinned", "x", closable: false);

        Assert.False(group.Tabs.Tabs[0].Closable);
    }

    [Fact]
    public void EditingADocument_MarksItDirty_AndRevertingClearsIt()
    {
        var group = new MultiTabCodeEditor();
        var editor = group.OpenDocument("a.cs", "abc");
        Assert.False(group.IsDirty(editor));

        UI.SendInput(editor.Editor, new ConsoleKeyInfo('z', ConsoleKey.Z, false, false, false));   // "zabc"
        Assert.True(group.IsDirty(editor));
        Assert.StartsWith("● ", group.ActiveDocumentName);

        UI.SendInput(editor.Editor, K(ConsoleKey.Backspace));   // back to "abc" -> matches baseline
        Assert.False(group.IsDirty(editor));
        Assert.Equal("a.cs", group.ActiveDocumentName);
    }

    [Fact]
    public void ConfirmOnClose_CleanDocument_ClosesWithoutPrompt()
    {
        var group = new MultiTabCodeEditor { ConfirmOnClose = true };
        var editor = group.OpenDocument("a.cs", "abc");   // clean

        group.CloseDocument(editor);

        Assert.Equal(0, group.DocumentCount);
    }

    [Fact]
    public void ConfirmOnClose_DirtyDocument_ShowsDialog_ClosesOnYes()
    {
        var overlay = Overlay();
        var group = new MultiTabCodeEditor { ConfirmOnClose = true, DialogOverlay = overlay };
        var editor = group.OpenDocument("a.cs", "abc");
        group.SetDirty(editor, true);

        group.CloseDocument(editor);
        ConsoleSnapshot.Render(overlay, 60, 20);   // lay out the modal
        Assert.Equal(1, group.DocumentCount);      // deferred behind the dialog

        Send(overlay, ConsoleKey.Enter);           // the default-focused "Yes"
        Assert.Equal(0, group.DocumentCount);      // closed after confirming
    }

    [Fact]
    public void ConfirmOnClose_DirtyDocument_Escape_KeepsIt()
    {
        var overlay = Overlay();
        var group = new MultiTabCodeEditor { ConfirmOnClose = true, DialogOverlay = overlay };
        var editor = group.OpenDocument("a.cs", "abc");
        group.SetDirty(editor, true);

        group.CloseDocument(editor);
        ConsoleSnapshot.Render(overlay, 60, 20);

        Send(overlay, ConsoleKey.Escape);          // cancel
        Assert.Equal(1, group.DocumentCount);      // still open
    }
}
