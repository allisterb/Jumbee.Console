namespace Jumbee.Console.IdeDemo;

using Jumbee.Console;

/// <summary>
/// A minimal VS Code–style IDE composed from Jumbee.Console controls: a file explorer (<see cref="Tree"/>), a tabbed
/// C# editor (<see cref="MultiTabCodeEditor"/>), and an embedded <see cref="TerminalEmulator"/> launched in the
/// project directory so <c>dotnet build</c>/<c>dotnet run</c> resolve there. C#-file focused; not a catalog example.
/// </summary>
internal sealed class IdeApp
{
    #region Constructors
    // headless: skip spawning a real shell (used by the --verify offscreen render) — the terminal is left in
    // manual-drive mode so no child process is launched.
    public IdeApp(string projectDir, bool headless = false)
    {
        _projectDir = projectDir;

        _tree = BuildExplorer(projectDir);
        _tree.NodeActivated += (_, node) => OpenFromNode(node);

        _editor = new MultiTabCodeEditor(Language.CSharp) { ConfirmOnClose = true };
        _editor.DocumentClosed += (_, ed) =>
        {
            if (_pathByEditor.Remove(ed, out var path)) _openByPath.Remove(path);
            UpdateStatus();
        };
        // Keep the status bar's caret readout hooked to whichever editor is active.
        _editor.ActiveDocumentChanged += (_, _) => { RehookActiveEditor(); UpdateStatus(); };

        // A real shell rooted in the project directory (the workingDirectory support added for this demo), so the
        // user can type `dotnet build`, `dotnet run`, etc. and have them resolve against the open project.
        _terminal = new TerminalEmulator(headless ? null : Pty.DefaultShell, workingDirectory: projectDir);

        _menu = BuildMenu();
        _status = new TextLabel(TextLabelOrientation.Horizontal, " Ready", StatusColor);
        _footer = new Footer(
            new FooterHint("^E", "Explorer"),
            new FooterHint("^L", "Editor"),
            new FooterHint("^T", "Terminal"),
            new FooterHint("^S", "Save"),
            new FooterHint("^B", "Build"),
            new FooterHint("^Q", "Quit"));

        _root = BuildLayout();
        UpdateStatus();
    }
    #endregion

    #region Properties
    /// <summary>The composed root layout — exposed for the headless <c>--verify</c> render.</summary>
    internal ILayout Root => _root;
    #endregion

    #region Methods
    public void Run()
    {
        UI.RegisterHotKey(UI.HotKeys.Ctrl(ConsoleKey.S), SaveActive);
        UI.RegisterHotKey(UI.HotKeys.Ctrl(ConsoleKey.B), Build);
        UI.RegisterHotKey(UI.HotKeys.Ctrl(ConsoleKey.E), FocusExplorer);
        UI.RegisterHotKey(UI.HotKeys.Ctrl(ConsoleKey.L), FocusEditor);
        UI.RegisterHotKey(UI.HotKeys.Ctrl(ConsoleKey.T), FocusTerminal);

        var run = UI.Start(_root, width: 150, height: 44, isAnsiTerminal: true,
            input: new VtInputSource(anyMotion: true));
        UI.SetFocus(_tree);
        run.Wait();
    }

    private ILayout BuildLayout()
    {
        var explorer = _tree.WithFrame(BorderStyle.Rounded, title: "Explorer");
        var editors = _editor.WithFrame(BorderStyle.Rounded, title: "Editor");
        var terminal = _terminal.WithFrame(BorderStyle.Rounded, title: "Terminal");

        // Editor over terminal on the right; explorer sidebar on the left; menu on top; status line + key hints
        // stacked at the bottom.
        var editorAndTerminal = new SplitPanel(SplitOrientation.Vertical, editors, terminal, splitPosition: 26);
        var main = new SplitPanel(SplitOrientation.Horizontal, explorer, editorAndTerminal, splitPosition: 34);

        return new DockPanel(DockedControlPlacement.Bottom, _footer,
                   new DockPanel(DockedControlPlacement.Bottom, _status,
                       new DockPanel(DockedControlPlacement.Top, _menu, main)));
    }

    private MenuBar BuildMenu() => new MenuBar()
        .Add("File",
            new MenuItem("Save", SaveActive) { Shortcut = "Ctrl+S" },
            MenuItem.Separator,
            new MenuItem("Exit", UI.Stop) { Shortcut = "Ctrl+Q" })
        .Add("Build",
            new MenuItem("Build", Build) { Shortcut = "Ctrl+B" },
            new MenuItem("Run", () => RunInTerminal("dotnet run")),
            new MenuItem("Clean", () => RunInTerminal("dotnet clean")))
        .Add("View",
            new MenuItem("Focus Explorer", FocusExplorer) { Shortcut = "Ctrl+E" },
            new MenuItem("Focus Editor", FocusEditor) { Shortcut = "Ctrl+L" },
            new MenuItem("Focus Terminal", FocusTerminal) { Shortcut = "Ctrl+T" });

    // ── Explorer ────────────────────────────────────────────────────────────────────────────────────────────────

    // Builds the file tree for `dir`: folders (recursively) then C#/project files, skipping build output. Folders keep
    // the default disclosure glyph tinted blue; source/project files get per-node leaf glyphs + colours (the per-node
    // glyph API). Leaf file nodes are mapped to their path in _filePaths for open-on-activate.
    private Tree BuildExplorer(string dir)
    {
        var tree = new Tree(Path.GetFileName(dir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)));
        tree.Root.DisclosureGlyphColor = FolderColor;
        Populate(tree.Root, dir);
        return tree;
    }

    private void Populate(Tree.TreeNode parent, string dir)
    {
        foreach (var sub in Directory.GetDirectories(dir).OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase))
        {
            var name = Path.GetFileName(sub);
            if (name is "bin" or "obj" or ".git" or ".vs") continue;
            var node = parent.AddChild(name);
            node.DisclosureGlyphColor = FolderColor;
            Populate(node, sub);
        }
        foreach (var file in Directory.GetFiles(dir).OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase))
        {
            var name = Path.GetFileName(file);
            if (!IsShown(name)) continue;
            var node = parent.AddChild(name);
            var isProject = name.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase);
            node.LeafGlyph = "◆ ";
            node.LeafGlyphColor = isProject ? ProjectColor : SourceColor;
            _filePaths[node] = file;
        }
    }

    private static bool IsShown(string name) =>
        name.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) ||
        name.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase);

    private void OpenFromNode(Tree.TreeNode node)
    {
        if (_filePaths.TryGetValue(node, out var path)) OpenFile(path);
    }

    // ── Editor ──────────────────────────────────────────────────────────────────────────────────────────────────

    private void OpenFile(string path)
    {
        if (_openByPath.TryGetValue(path, out var existing))
        {
            SelectTabFor(existing);
            UI.SetFocus(existing.Editor);
            return;
        }

        string text;
        try { text = File.ReadAllText(path); }
        catch { return; }

        var editor = _editor.OpenDocument(Path.GetFileName(path), text, LanguageFor(path));
        _openByPath[path] = editor;
        _pathByEditor[editor] = path;
        UI.SetFocus(editor.Editor);
    }

    private void SelectTabFor(CodeEditor editor)
    {
        var editors = _editor.Editors;
        for (var i = 0; i < editors.Count; i++)
            if (ReferenceEquals(editors[i], editor)) { _editor.Tabs.SelectTab(i); return; }
    }

    private void SaveActive()
    {
        if (_editor.ActiveEditor is not { } editor) return;
        if (!_pathByEditor.TryGetValue(editor, out var path)) return;   // untitled / not file-backed
        try
        {
            File.WriteAllText(path, editor.Text);
            _editor.SetDirty(editor, false);
            UpdateStatus();
        }
        catch { /* a real IDE would surface this; the demo leaves the file dirty */ }
    }

    // ── Status bar ──────────────────────────────────────────────────────────────────────────────────────────────

    // Re-point the caret readout at the active editor: unhook the previous editor's Changed and hook the new one
    // (TextEditor.Changed fires on caret moves too, so the Ln/Col updates as you navigate).
    private void RehookActiveEditor()
    {
        if (_hookedEditor is not null) _hookedEditor.Editor.Changed -= OnActiveEditorChanged;
        _hookedEditor = _editor.ActiveEditor;
        if (_hookedEditor is not null) _hookedEditor.Editor.Changed += OnActiveEditorChanged;
    }

    private void OnActiveEditorChanged(object? sender, EventArgs e) => UpdateStatus();

    private void UpdateStatus() => _status.Text = StatusText();

    private string StatusText()
    {
        if (_editor.ActiveEditor is not { } editor)
            return $"  {Path.GetFileName(_projectDir)}   —   no file open";
        var name = _pathByEditor.TryGetValue(editor, out var path)
            ? Path.GetFileName(path)
            : _editor.ActiveDocumentName ?? "untitled";
        var dirty = _editor.IsDirty(editor) ? " ●" : "";
        var (line, column) = CaretLineCol(editor);
        // Ln/Col fixed-width so a caret move within one file doesn't change the label length (avoids a relayout).
        return $"  {name}{dirty}    Ln {line,-4}Col {column,-4}{_projectDir}";
    }

    // 1-based (line, column) of the caret: line from CaretLine (0-based), column by scanning back to the newline.
    private static (int line, int column) CaretLineCol(CodeEditor editor)
    {
        var text = editor.Text;
        var index = Math.Clamp(editor.Editor.CaretIndex, 0, text.Length);
        var column = 1;
        for (var i = index - 1; i >= 0 && text[i] != '\n'; i--) column++;
        return (editor.Editor.CaretLine + 1, column);
    }

    private static Language LanguageFor(string path) => Path.GetExtension(path).ToLowerInvariant() switch
    {
        ".cs" => Language.CSharp,
        ".csproj" or ".xml" => Language.Xml,
        ".json" => Language.Json,
        ".md" => Language.Markdown,
        _ => Language.None,
    };

    // ── Terminal / build ────────────────────────────────────────────────────────────────────────────────────────

    private void Build() => RunInTerminal("dotnet build");

    private void RunInTerminal(string command)
    {
        _terminal.SendText(command + "\r");
        FocusTerminal();
    }

    // ── Focus ───────────────────────────────────────────────────────────────────────────────────────────────────

    private void FocusExplorer() => UI.SetFocus(_tree);
    private void FocusTerminal() => UI.SetFocus(_terminal);
    private void FocusEditor()
    {
        if (_editor.ActiveEditor is { } editor) UI.SetFocus(editor.Editor);
    }
    #endregion

    #region Fields
    private static readonly Color FolderColor = new(0x8f, 0xd0, 0xff);   // soft blue
    private static readonly Color SourceColor = new(0x8f, 0xd0, 0x66);   // C# green
    private static readonly Color ProjectColor = new(0xe0, 0xa0, 0x50);  // project orange
    private static readonly Color StatusColor = new(0x9a, 0xa6, 0xc0);   // muted status text

    private readonly string _projectDir;
    private readonly Tree _tree;
    private readonly MultiTabCodeEditor _editor;
    private readonly TerminalEmulator _terminal;
    private readonly MenuBar _menu;
    private readonly TextLabel _status;
    private readonly Footer _footer;
    private readonly ILayout _root;
    private CodeEditor? _hookedEditor;

    private readonly Dictionary<Tree.TreeNode, string> _filePaths = new();                       // leaf node -> path
    private readonly Dictionary<string, CodeEditor> _openByPath = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<CodeEditor, string> _pathByEditor = new();
    #endregion
}
