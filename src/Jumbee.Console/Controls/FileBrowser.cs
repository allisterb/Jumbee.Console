namespace Jumbee.Console;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>What a <see cref="FileBrowser"/> is asking the user to choose.</summary>
public enum FileBrowserMode
{
    /// <summary>A file. Directories are navigated into rather than chosen.</summary>
    OpenFile,

    /// <summary>A directory. Files are listed for context but cannot be chosen.</summary>
    OpenDirectory,
}

/// <summary>
/// A two-pane file chooser: a lazily-populated directory <see cref="Tree"/> on the left, the current directory's
/// contents in a <see cref="ListBox"/> on the right, a path field above and a filter drop-down below.
/// </summary>
/// <remarks>
/// <para>
/// Usually shown as a modal through <see cref="OpenFile"/> or <see cref="OpenDirectory"/>, which wrap it in a
/// <see cref="Dialog"/> and report the chosen path (or <see langword="null"/> if cancelled). Place it directly
/// when you want a browser embedded in a pane rather than floating over one.
/// </para>
/// <para>
/// Directories are read on demand — a tree node's children when it is first expanded, the listing when the
/// directory changes — so a folder with thousands of entries under it costs nothing until you look. Every
/// enumeration is guarded: an unreadable directory shows a message in the list instead of throwing out of a paint,
/// which matters because the first thing a Windows user meets under <c>C:\</c> is
/// <c>System Volume Information</c>.
/// </para>
/// <para>
/// The tree shows the directory being listed and what is under it, and is re-rooted whenever the listing moves.
/// Going up or elsewhere is what the <c>..</c> row and the path field are for. See <c>RerootTree</c> for why it
/// works this way rather than showing the whole machine.
/// </para>
/// </remarks>
public class FileBrowser : CompositeControl
{
    #region Constructors
    /// <summary>Creates a browser rooted at <paramref name="startPath"/> (a directory, or a file whose directory is
    /// opened with it selected), choosing whatever <paramref name="mode"/> asks for and narrowing the file list to
    /// <paramref name="filters"/>.</summary>
    /// <remarks>The filters are fixed for the browser's lifetime because they populate the filter drop-down; build
    /// a new browser to change them.</remarks>
    public FileBrowser(string? startPath = null, FileBrowserMode mode = FileBrowserMode.OpenFile,
                       IReadOnlyList<string>? filters = null)
    {
        Mode = mode;
        Width = DefaultWidth;
        Height = DefaultHeight;

        // "everything" is always offered, and is the only option when no filter was asked for — a chooser that can
        // only ever show one kind of file gives a user no way to see they are in the wrong directory.
        this.filters = filters is { Count: > 0 }
            ? [.. filters.Append(AllFiles).Distinct(StringComparer.OrdinalIgnoreCase)]
            : [AllFiles];
        filter = new Select([.. this.filters]) { SelectedIndex = 0 };

        path.Submitted += (_, _) => NavigateTo(path.Text);
        filter.SelectionChanged += (_, _) => Populate();
        entries.SelectionChanged += (_, _) => OnEntrySelected();
        entries.Committed += (_, item) => Activate(item.Tag as string);
        tree.NodeExpanding += (_, node) => Expand(node);
        // The guard matters: re-rooting the tree replaces its nodes, and that raises a selection change of its own.
        tree.NodeActivated += (_, node) => { if (!rerooting && PathOf(node) is { } p) NavigateTo(p); };

        SetContent(new DockPanel(DockedControlPlacement.Top, path,
            new DockPanel(DockedControlPlacement.Bottom, filter,
                new SplitPanel(SplitOrientation.Horizontal, Framed(tree, "Folders"), Framed(entries, "Contents"),
                    splitPosition: TreeColumns))));

        var (directory, preselect) = Resolve(startPath);
        NavigateTo(directory);
        if (preselect is not null) Select(preselect);
    }
    #endregion

    #region Events
    /// <summary>Raised when the highlighted entry changes, with the full path or <see langword="null"/>.</summary>
    public event EventHandler<string?>? SelectionChanged;

    /// <summary>Raised when an entry is committed (Enter, or a double-click) and it is a valid choice for the
    /// current <see cref="Mode"/>. Committing a directory in <see cref="FileBrowserMode.OpenFile"/> navigates into
    /// it instead and raises nothing.</summary>
    public event EventHandler<string>? PathActivated;
    #endregion

    #region Properties
    /// <summary>What the browser is choosing.</summary>
    public FileBrowserMode Mode { get; }

    /// <summary>The directory being listed. Setting it navigates.</summary>
    public string CurrentDirectory
    {
        get => current;
        set => NavigateTo(value);
    }

    /// <summary>The full path of the highlighted entry, or <see langword="null"/> when nothing valid is highlighted.</summary>
    /// <remarks>In <see cref="FileBrowserMode.OpenDirectory"/> this is the listed directory itself while no
    /// subdirectory is highlighted, so the dialog's OK button always has something to return.</remarks>
    public string? SelectedPath
    {
        get
        {
            if (selected is { } path) return path;
            return Mode == FileBrowserMode.OpenDirectory ? current : null;
        }
    }

    /// <summary>The glob patterns the file list can be narrowed to (<c>*.obj</c>, <c>*.png</c>, …), as offered by
    /// the filter drop-down. <see cref="AllFiles"/> shows everything and is always the last option.</summary>
    public IReadOnlyList<string> Filters => filters;

    /// <summary>Whether hidden and system entries are listed. Default <see langword="false"/>.</summary>
    public bool ShowHidden
    {
        get => showHidden;
        set { showHidden = value; Populate(); }
    }
    #endregion

    #region Methods
    /// <summary>Shows a modal file chooser and reports the chosen path, or <see langword="null"/> if cancelled.</summary>
    /// <param name="title">The dialog's title bar.</param>
    /// <param name="start">Where to open — a directory, or a file to preselect. Null starts in the current directory.</param>
    /// <param name="filters">Glob patterns for the filter drop-down; the first is applied. Null lists everything.</param>
    /// <param name="onResult">Invoked with the chosen path, or <see langword="null"/> on cancel.</param>
    public static Dialog OpenFile(string title, string? start, string[]? filters, Action<string?> onResult) =>
        Show(title, new FileBrowser(start, FileBrowserMode.OpenFile, filters), onResult);

    /// <summary>Shows a modal directory chooser and reports the chosen directory, or <see langword="null"/> if
    /// cancelled.</summary>
    public static Dialog OpenDirectory(string title, string? start, Action<string?> onResult) =>
        Show(title, new FileBrowser(start, FileBrowserMode.OpenDirectory), onResult);

    /// <summary>Lists <paramref name="directory"/>, if it can be read.</summary>
    public void NavigateTo(string? directory) => NavigateTo(directory, syncTree: true);

    /// <summary>Highlights <paramref name="fullPath"/> in the list, if it is in the directory being listed.</summary>
    public void Select(string fullPath)
    {
        for (var i = 0; i < entries.Items.Count; i++)
        {
            if (entries.Items.ElementAt(i).Tag as string == fullPath)
            {
                entries.SelectedIndex = i;
                return;
            }
        }
    }

    /// <inheritdoc/>
    protected override bool TabNavigatesChildren => true;

    /// <inheritdoc/>
    protected override void ApplyTheme()
    {
        directoryStyle = UI.StyleTheme.TextAccent;
        fileStyle = UI.StyleTheme.Text;
        messageStyle = UI.StyleTheme.TextMuted;
        folderGlyph = UI.GlyphTheme.FolderClosed;
        fileGlyph = UI.GlyphTheme.File;
        Populate();
    }

    /// <inheritdoc/>
    protected internal override HelpInfo? GetHelpInfo() =>
        new HelpInfo("Files", "File browser", "Choose a file or folder.")
            .WithKey("Up / Down", "Move through the listing")
            .WithKey("Enter", "Open a folder, or choose the highlighted entry")
            .WithKey("Backspace", "Go up one level")
            .WithKey("Tab", "Move between the path field, the folder tree and the listing")
            .WithKey("Right / Left", "In the tree: expand or collapse a folder");
    #endregion

    #region Private methods
    private static Dialog Show(string title, FileBrowser browser, Action<string?> onResult)
    {
        var dialog = new Dialog(title, browser, DialogButtons.OkCancel);
        // A double-click is a commit, and a commit on a valid choice should close the dialog — otherwise the user
        // has to double-click and then find OK, which nobody does.
        browser.PathActivated += (_, _) => dialog.Close(DialogResult.Ok);
        dialog.Completed += (_, result) => onResult(result == DialogResult.Ok ? browser.SelectedPath : null);
        dialog.Show();
        return dialog;
    }

    // Where to open, and what to preselect. A path that is neither a file nor a directory falls back to the current
    // directory rather than failing: a chooser that refuses to appear is worse than one that opens somewhere else.
    private static (string Directory, string? Select) Resolve(string? startPath)
    {
        if (string.IsNullOrWhiteSpace(startPath)) return (Directory.GetCurrentDirectory(), null);
        try
        {
            if (Directory.Exists(startPath)) return (Path.GetFullPath(startPath), null);
            if (File.Exists(startPath))
            {
                var full = Path.GetFullPath(startPath);
                return (Path.GetDirectoryName(full) ?? Directory.GetCurrentDirectory(), full);
            }
        }
        catch (Exception e) when (IsFileSystemFailure(e))
        {
            // Fall through to the current directory.
        }

        return (Directory.GetCurrentDirectory(), null);
    }

    private void NavigateTo(string? directory, bool syncTree)
    {
        if (string.IsNullOrWhiteSpace(directory)) return;

        string full;
        try
        {
            if (!Directory.Exists(directory)) return;
            full = Path.GetFullPath(directory);
        }
        catch (Exception e) when (IsFileSystemFailure(e))
        {
            return;
        }

        if (string.Equals(full, current, StringComparison.OrdinalIgnoreCase)) return;

        current = full;
        path.Text = full;
        selected = null;
        Populate();
        if (syncTree) RerootTree(full);
        SelectionChanged?.Invoke(this, SelectedPath);
    }

    // Fills the listing: an ".." row unless we are at a root, then subdirectories, then whatever the filter admits.
    // Directories first and each group sorted, which is what every file chooser does and what makes a long listing
    // scannable.
    private void Populate()
    {
        if (entries is null || current.Length == 0) return;

        entries.Clear();
        if (Directory.GetParent(current) is { } parent)
        {
            entries.AddItem($"{folderGlyph} ..", directoryStyle.ForegroundColor).Tag = parent.FullName;
        }

        if (!TryList(current, out var directories, out var files, out var error))
        {
            entries.AddItem($"  {error}", messageStyle.ForegroundColor);
            return;
        }

        foreach (var directory in directories)
            entries.AddItem($"{folderGlyph} {Path.GetFileName(directory)}", directoryStyle.ForegroundColor).Tag = directory;

        foreach (var file in files)
            entries.AddItem($"{fileGlyph} {Path.GetFileName(file)}", fileStyle.ForegroundColor).Tag = file;

        if (entries.Items.Count == 0)
            entries.AddItem("  (empty)", messageStyle.ForegroundColor);
    }

    // One guarded enumeration for both panes. Reports the failure as a message rather than letting it escape:
    // GetDirectories on a drive root throws UnauthorizedAccessException on the first protected folder it meets, and
    // a removable drive throws IOException when nothing is in it.
    private bool TryList(string directory, out string[] directories, out string[] files, out string? error)
    {
        directories = [];
        files = [];
        error = null;
        try
        {
            directories = [.. Directory.EnumerateDirectories(directory).Where(Visible).OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase)];
            files = [.. MatchingFiles(directory).Where(Visible).OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase)];
            return true;
        }
        catch (Exception e) when (IsFileSystemFailure(e))
        {
            error = e is UnauthorizedAccessException ? "(no permission)" : "(unreadable)";
            return false;
        }
    }

    private IEnumerable<string> MatchingFiles(string directory)
    {
        var pattern = filter.SelectedValue ?? filters[0];
        if (pattern == AllFiles) return Directory.EnumerateFiles(directory);
        // Several patterns in one option ("*.jpg;*.png") list as one group, which is how a user thinks of "images".
        return pattern.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .SelectMany(p => Directory.EnumerateFiles(directory, p))
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }

    private bool Visible(string entry)
    {
        if (showHidden) return true;
        try
        {
            var attributes = File.GetAttributes(entry);
            return (attributes & (FileAttributes.Hidden | FileAttributes.System)) == 0;
        }
        catch (Exception e) when (IsFileSystemFailure(e))
        {
            return false;
        }
    }

    /// <summary>
    /// Re-roots the tree at the directory being listed, so it shows that folder and what is under it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A deliberate choice, and the one that makes the left pane work at 26 columns. The obvious alternative — root
    /// at the machine's drives and reveal the current path — needs the tree to scroll to a row it has just made
    /// reachable, and a frame clamps its scroll offset against the content height it last <em>measured</em>: the
    /// expansion and the scroll cannot happen in the same layout pass, so the tree lands on the wrong row or none at
    /// all. Chasing that across passes is a lot of machinery for a pane that would then be showing forty sibling
    /// folders you did not ask about.
    /// </para>
    /// <para>
    /// So the tree is a drill-down of where you are. Going <em>up</em> or elsewhere is what the <c>..</c> row and
    /// the path field are for, and both re-root this.
    /// </para>
    /// </remarks>
    private void RerootTree(string directory)
    {
        rerooting = true;
        try
        {
            foreach (var child in tree.Root.Nodes.ToArray()) tree.Root.RemoveChild(child.Index);
            paths.Clear();
            expanded.Clear();

            tree.Root.Label = new Spectre.Console.Markup(Spectre.Console.Markup.Escape(Label(directory)));
            paths[tree.Root] = directory;
            expanded.Add(tree.Root);   // its children are filled right here, so it must not be filled again on expand
            Fill(tree.Root, directory);
            tree.Root.Expanded = true;
        }
        finally
        {
            rerooting = false;
        }
    }

    // A drive root has no file name, so fall back to the path itself ("C:\" rather than "").
    private static string Label(string directory) =>
        Path.GetFileName(directory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)) is { Length: > 0 } name
            ? name
            : directory;

    // Replace the placeholder with the real subdirectories, once, when the node is first opened. Raised by
    // Tree.NodeExpanding before the expansion takes effect, so the children are in place by the time it draws.
    private void Expand(Tree.TreeNode node)
    {
        if (!expanded.Add(node) || PathOf(node) is not { } directory) return;
        foreach (var child in node.Nodes.ToArray()) node.RemoveChild(child.Index);
        Fill(node, directory);
    }

    // The subfolders of one node, each carrying a placeholder child so it renders as a folder and can be opened —
    // a node with no children is a leaf, and nothing would ever ask it to expand.
    private void Fill(Tree.TreeNode node, string directory)
    {
        if (!TryList(directory, out var directories, out _, out var error))
        {
            node.AddChild(error ?? "(unreadable)");
            return;
        }

        foreach (var sub in directories)
        {
            var child = node.AddChild(Path.GetFileName(sub));
            paths[child] = sub;
            child.AddChild(Placeholder);
            child.Expanded = false;
        }

        if (directories.Length == 0) node.AddChild("(no folders)");
    }

    private string? PathOf(Tree.TreeNode node) => paths.GetValueOrDefault(node);

    private void OnEntrySelected()
    {
        var item = entries.SelectedItem;
        var target = item?.Tag as string;
        selected = target is null ? null : Choosable(target) ? target : null;
        SelectionChanged?.Invoke(this, SelectedPath);
    }

    // Enter or a double-click. A directory in file mode is a navigation, not a choice — which is the behaviour that
    // makes the same gesture do the obvious thing on both kinds of row.
    private void Activate(string? target)
    {
        if (target is null) return;
        if (Directory.Exists(target) && Mode == FileBrowserMode.OpenFile)
        {
            NavigateTo(target);
            return;
        }

        if (Choosable(target)) PathActivated?.Invoke(this, target);
    }

    private bool Choosable(string target)
    {
        try
        {
            return Mode == FileBrowserMode.OpenDirectory ? Directory.Exists(target) : File.Exists(target);
        }
        catch (Exception e) when (IsFileSystemFailure(e))
        {
            return false;
        }
    }

    private static bool IsFileSystemFailure(Exception e) =>
        e is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException;

    private static Control Framed(Control control, string title) =>
        control.WithFrame(borderStyle: BorderStyle.Rounded)
               .WithTitle(title, new TitleStyle(TitlePos.TopLeft, TitleBorderStyle.Inline));
    #endregion

    #region Fields
    /// <summary>The pattern that lists every file, and the default when none is given.</summary>
    public const string AllFiles = "*.*";

    private const int DefaultWidth = 78;
    private const int DefaultHeight = 20;
    private const int TreeColumns = 26;

    // The stand-in child an unexpanded folder carries so the tree draws it as a folder rather than a leaf.
    private const string Placeholder = "…";

    private readonly TextInput path = new TextInput(placeholder: "type a path and press Enter");
    // No explicit Height: an explicit one pins the control to that many rows, so the frame's content can never be
    // taller than its viewport and the pane will not scroll however deep the tree gets.
    private readonly Tree tree = new Tree("Computer");
    // A single click only SELECTS here: a chooser is a list you look through before acting on it, and the default
    // (click commits) would open whatever file you glanced at.
    private readonly ListBox entries = new ListBox { CommitOnClick = false };
    private readonly Select filter;

    private readonly Dictionary<Tree.TreeNode, string> paths = [];
    private readonly HashSet<Tree.TreeNode> expanded = [];

    private readonly string[] filters;
    private string current = "";
    private string? selected;
    private bool showHidden;
    private bool rerooting;

    private Style directoryStyle;
    private Style fileStyle;
    private Style messageStyle;
    private string folderGlyph = "▸";
    private string fileGlyph = " ";
    #endregion
}
