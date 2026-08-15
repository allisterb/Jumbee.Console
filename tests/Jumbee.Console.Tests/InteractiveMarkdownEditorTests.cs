namespace Jumbee.Console.Tests;

using ConsoleGUI.Input;

using Jumbee.Console;
using Jumbee.Console.Snapshot;

using Xunit;

// The editor composes core controls; these tests exercise the live-sync behaviour that
// --verify (a one-frame render check) can't: an edit in the code pane must flow to the preview pane, and caret-only
// movement must not. Headless (UI not running), so the coalesced sync applies inline (see ScheduleSync).
public class InteractiveMarkdownEditorTests
{
    private static ConsoleKeyInfo K(char ch, ConsoleKey key) => new(ch, key, false, false, false);

    [Fact]
    public void Construction_StartsBothPanesInSync()
    {
        var ed = new InteractiveMarkdownEditor("# Title\n\nbody");

        Assert.Equal("# Title\n\nbody", ed.Editor.Text);
        Assert.Equal("# Title\n\nbody", ed.Preview.Markdown);
    }

    [Fact]
    public void Editing_TheCodePane_UpdatesThePreview_AndRaisesTextChanged()
    {
        var ed = new InteractiveMarkdownEditor("# Doc\n");
        ConsoleSnapshot.Render(ed, 80, 20);

        string? fired = null;
        ed.TextChanged += (_, t) => fired = t;

        // Paste is the simplest deterministic edit; it raises the editor's Changed just like typing.
        ed.Editor.Editor.OnPaste("more");

        Assert.Equal(ed.Editor.Text, ed.Preview.Markdown);   // preview mirrors the editor
        Assert.Contains("more", ed.Preview.Markdown);
        Assert.Equal(ed.Editor.Text, fired);                 // TextChanged carried the new text
    }

    [Fact]
    public void SettingText_LoadsTheEditor_AndRefreshesThePreview()
    {
        var ed = new InteractiveMarkdownEditor();

        ed.Text = "| a | b |\n|---|---|\n| 1 | 2 |";

        Assert.Equal(ed.Text, ed.Preview.Markdown);
        Assert.Equal(ed.Editor.Text, ed.Preview.Markdown);
    }

    [Fact]
    public void FramedEditor_AsFillUnderBoundaryToolbar_RendersCaptionAndBothPanes()
    {
        // Reproduces the "Interactive Markdown" example layout: a one-row caption + dropdown toolbar docked on top of
        // the editor in the DockPanel fill slot. Two things this guards: (1) the editor is sized to its frame's
        // viewport, so it needs a (borderless) frame to size to the fill area; (2) the toolbar would expand to the
        // full height and collapse the fill region, so a Boundary(height: 1) pins it to one row.
        var editor = new InteractiveMarkdownEditor("# Hello\n\nsome body text");
        editor.WithFrame(borderStyle: BorderStyle.None);
        const string captionText = "Layout: ";
        var caption = new Boundary(new TextLabel(TextLabelOrientation.Horizontal, captionText) { Focusable = false }, width: captionText.Length);
        var select = new Select("Editor 30%", "Even 50%", "Editor 70%") { Placeholder = "Even 50%" };
        var toolbar = new Boundary(new HorizontalStackPanel(caption, new Boundary(select, width: select.Width)), height: 1);
        var dock = new DockPanel(DockedControlPlacement.Top, toolbar, editor);

        var text = ConsoleSnapshot.ToText(dock, 100, 24);
        var firstRow = text.Split('\n')[0];

        Assert.Contains("Layout:", firstRow);     // caption drew on the toolbar row
        Assert.Contains("Even 50%", firstRow);    // dropdown drew next to it (not squeezed to zero width)
        Assert.Contains("Markdown", text);        // left pane frame title
        Assert.Contains("Preview", text);         // right pane frame title
        Assert.Contains("Hello", text);           // editor content actually rendered into the fill area
    }

    [Fact]
    public void VerticalOrientation_StacksBothPanes()
    {
        // The editor takes orientation at construction; Vertical stacks the panes (editor on top, preview below) with a
        // horizontal divider between. Both panes must still render their content.
        var editor = new InteractiveMarkdownEditor("# Hello\n\nsome body text", SplitOrientation.Vertical, splitPosition: 8);
        editor.WithFrame(borderStyle: BorderStyle.None);

        var lines = ConsoleSnapshot.ToText(editor, 60, 24).Split('\n');
        var markdownRow = System.Array.FindIndex(lines, l => l.Contains("Markdown"));
        var previewRow = System.Array.FindIndex(lines, l => l.Contains("Preview"));

        Assert.True(markdownRow >= 0, "Markdown pane title rendered");
        Assert.True(previewRow > markdownRow, "Preview pane is stacked below the editor pane");
        Assert.Equal(SplitOrientation.Vertical, editor.Split.Orientation);
    }

    [Fact]
    public void SwappingDockFillControl_SwitchesOrientation_LikeTheExampleToggle()
    {
        // Mirrors the example's orientation toggle: a bounded toolbar docked over the editor in a DockPanel's fill
        // slot; switching orientation rebuilds the editor and reassigns DockPanel.FillControl. Both renders must show
        // the panes, and the layout must actually change (side-by-side -> stacked).
        var toolbar = new Boundary(new TextLabel(TextLabelOrientation.Horizontal, "Layout:") { Focusable = false }, height: 1);
        var horizontal = new InteractiveMarkdownEditor("# Hi\n\nbody", SplitOrientation.Horizontal);
        horizontal.WithFrame(borderStyle: BorderStyle.None);
        var dock = new DockPanel(DockedControlPlacement.Top, toolbar, horizontal);

        var before = ConsoleSnapshot.ToText(dock, 70, 20);
        var mdRowH = FirstRowContaining(before, "Markdown");
        var pvRowH = FirstRowContaining(before, "Preview");
        Assert.True(mdRowH >= 0 && pvRowH >= 0);
        Assert.Equal(mdRowH, pvRowH);   // side by side: both titles on the same row

        var vertical = new InteractiveMarkdownEditor(horizontal.Text, SplitOrientation.Vertical, splitPosition: 7);
        vertical.WithFrame(borderStyle: BorderStyle.None);
        dock.FillControl = vertical;    // swap just the fill; the toolbar stays

        var after = ConsoleSnapshot.ToText(dock, 70, 20);
        var mdRowV = FirstRowContaining(after, "Markdown");
        var pvRowV = FirstRowContaining(after, "Preview");
        Assert.True(mdRowV >= 0 && pvRowV >= 0, "panes rendered after the fill swap");
        Assert.True(pvRowV > mdRowV, "stacked: preview title is below the editor title");
    }

    private static int FirstRowContaining(string text, string needle) =>
        System.Array.FindIndex(text.Split('\n'), l => l.Contains(needle));

    [Fact]
    public void CaretOnlyMovement_DoesNotRaiseTextChanged()
    {
        // The wrapped TextEditor raises Changed on caret moves too; the editor must suppress those from the preview /
        // TextChanged path (only real text changes propagate).
        var ed = new InteractiveMarkdownEditor("abc\ndef");
        ConsoleSnapshot.Render(ed, 80, 20);
        UI.SetFocus(ed.Editor.Editor);

        var count = 0;
        ed.TextChanged += (_, _) => count++;

        UI.SendInput(ed.Editor.Editor, K('\0', ConsoleKey.RightArrow));
        UI.SendInput(ed.Editor.Editor, K('\0', ConsoleKey.DownArrow));

        Assert.Equal(0, count);
        Assert.Equal(ed.Editor.Text, ed.Preview.Markdown);   // still in sync, unchanged
    }
}
