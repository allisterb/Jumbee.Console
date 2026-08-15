namespace Jumbee.Console.Examples;

using System.Collections.Generic;

/// <summary>
/// A live split-pane Markdown editor with a "Layout" dropdown (a <see cref="Select"/>) that switches the panes between
/// side-by-side and stacked; edits render live in the preview pane. Drag the divider too, or focus it and use the arrows.
/// </summary>
public sealed class InteractiveMarkdownEditorExample : CompositeControl, IExample
{
    public InteractiveMarkdownEditorExample()
    {
        editor = BuildEditor(SourceLoader.Read("interactive-md.md"), SplitOrientation.Horizontal);
        layout = new Select("Side by side", "Stacked");
        layout.SelectedIndex = 0;
        layout.SelectionChanged += (_, _) =>
            SetOrientation(layout.SelectedIndex == 1 ? SplitOrientation.Vertical : SplitOrientation.Horizontal);

        const string captionText = "Layout: ";
        var caption = new Boundary(new TextLabel(TextLabelOrientation.Horizontal, captionText) { Focusable = false },
            width: captionText.Length);
        var toolbar = new Boundary(new HorizontalStackPanel(caption, new Boundary(layout, width: layout.Width)), height: 1);
        dock = new DockPanel(DockedControlPlacement.Top, toolbar, editor);
        SetContent(dock);
        this.WithFrame(borderStyle: BorderStyle.None, borderPlacement: BorderPlacement.None);
    }

    // The editor is sized to its frame's viewport, so it needs a borderless frame — as a bare fill
    // control its sizing collapses and the panes don't render.
    private static InteractiveMarkdownEditor BuildEditor(string text, SplitOrientation orientation) =>
        new InteractiveMarkdownEditor(text, orientation).WithFrame(borderStyle: BorderStyle.None, borderPlacement: BorderPlacement.None);
        
           
    // Orientation is fixed at a SplitPanel's construction, so switching it rebuilds the editor in the new orientation
    // (carrying its text over) and swaps it into the dock's fill slot; the toolbar stays put. A no-op if unchanged.
    private void SetOrientation(SplitOrientation orientation)
    {
        if (editor.Split.Orientation == orientation) return;
        editor = BuildEditor(editor.Text, orientation);
        dock.FillControl = editor;
    }

    // Keyboard focus lands in the (current) editor, not the toolbar dropdown.
    protected override Control? FocusChild => editor;

    #region Fields
    private readonly DockPanel dock;
    private readonly Select layout;
    private InteractiveMarkdownEditor editor;
    #endregion

    #region IExample
    string IExample.Category => "Editors";
    string IExample.Title => "Interactive Markdown";
    string IExample.Description =>
        "Edit Markdown live; the Layout dropdown switches the panes between side-by-side and stacked.";
    IReadOnlyList<string> IExample.SourceFiles =>
        ["InteractiveMarkdownEditorExample.cs", "InteractiveMarkdownEditor.cs", "interactive-md.md"];
    #endregion
}
