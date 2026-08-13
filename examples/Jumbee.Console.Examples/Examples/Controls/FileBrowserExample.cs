namespace Jumbee.Console.Examples;

using System.Collections.Generic;
using System.IO;

/// <summary>
/// The modal file chooser: pick a file or a folder, or watch the embedded browser report what it is looking at.
/// Enter or a double-click opens a folder and chooses a file.
/// </summary>
public sealed class FileBrowserExample : CompositeControl, IExample
{
    public FileBrowserExample()
    {
        openFile.Activated += (_, _) => FileBrowser.OpenFile("Choose a source file", start, Filters, Report);
        openFolder.Activated += (_, _) => FileBrowser.OpenDirectory("Choose a folder", start, Report);

        // The same control without the dialog around it — SelectionChanged is what an embedded browser drives a
        // preview pane from.
        embedded.SelectionChanged += (_, path) => where.Text = path is null
            ? $"▸ looking at {embedded.CurrentDirectory}"
            : $"▸ {Path.GetFileName(path)}";

        SetContent(new VerticalStackPanel(
            Header("Modal — one call each, and the result comes back or null if cancelled"),
            new Grid([1], [22, 22], [[openFile, openFolder]]),
            result,

            Header("Embedded — the same control placed in a pane instead of floating over one"),
            embedded,
            where));
    }

    protected override bool TabNavigatesChildren => true;

    private void Report(string? path) =>
        result.Text = path is null ? "▸ cancelled" : "▸ " + path;

    private static TextLabel Header(string text) =>
        new TextLabel(TextLabelOrientation.Horizontal, text, HeaderColor) { Focusable = false };

    #region IExample
    string IExample.Category => "Controls";
    string IExample.Title => "File Browser";
    string IExample.Description =>
        "A two-pane file chooser: folders on the left, contents on the right, a path field and a filter. Shown as a modal, or placed in a pane.";
    IReadOnlyList<string> IExample.SourceFiles => ["FileBrowserExample.cs", "FileBrowser.cs"];
    #endregion

    #region Fields
    private static readonly string[] Filters = ["*.cs", "*.md;*.txt"];
    private static readonly string start = Directory.GetCurrentDirectory();

    private readonly Button openFile = Button.Primary("Open a file…");
    private readonly Button openFolder = Button.Secondary("Open a folder…");
    private readonly TextLabel result = new TextLabel(TextLabelOrientation.Horizontal, "▸ nothing chosen yet", StatusColor);

    private readonly FileBrowser embedded = new FileBrowser(Directory.GetCurrentDirectory(), FileBrowserMode.OpenFile, Filters)
    {
        Height = 14,
    };

    private readonly TextLabel where = new TextLabel(TextLabelOrientation.Horizontal, "▸ ready", StatusColor);

    private static readonly Color HeaderColor = new(0x9a, 0xc8, 0xff);
    private static readonly Color StatusColor = new(0x8f, 0xd0, 0x66);
    #endregion
}
