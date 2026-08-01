namespace Jumbee.Console;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// A tabbed group of <see cref="CodeEditor"/>s — a VS-Code-style editor area. Each open document is a closable tab
/// (click the ✕ on the active/hovered tab), and a "+" button at the end of the bar opens a new document.
/// </summary>
/// <remarks>
/// Built on a top-docked <see cref="TabPanel"/>; each editor is wrapped in its own frame so it scrolls independently.
/// Switch tabs with Alt+←/→ or by clicking a tab.
/// </remarks>
public class MultiTabCodeEditor : CompositeControl
{
    #region Constructors
    /// <summary>Initializes an empty editor group whose new documents default to <paramref name="defaultLanguage"/>.</summary>
    public MultiTabCodeEditor(Language defaultLanguage = Language.None)
    {
        _defaultLanguage = defaultLanguage;
        _panel = new TabPanel(TabBarDock.Top) { ClosableTabs = true, ShowAddButton = true };
        _panel.NewTabRequested += (_, _) => NewDocument();
        _panel.TabCloseRequested += OnTabCloseRequested;   // route the ✕ through the cancelable DocumentClosing
        _panel.TabRemoved += OnTabRemoved;
        _panel.SelectionChanged += (_, _) => ActiveDocumentChanged?.Invoke(this, ActiveEditor);
        SetContent(_panel);
    }
    #endregion

    #region Events
    /// <summary>Raised after a document is opened (its editor + tab exist and it is selected).</summary>
    public event EventHandler<CodeEditor>? DocumentOpened;

    /// <summary>Raised before a document closes (via ✕ or <see cref="CloseDocument"/>). Set
    /// <see cref="DocumentClosingEventArgs.Cancel"/> to keep it open — e.g. after prompting about unsaved changes.</summary>
    public event EventHandler<DocumentClosingEventArgs>? DocumentClosing;

    /// <summary>Raised after a document's tab has been removed.</summary>
    public event EventHandler<CodeEditor>? DocumentClosed;

    /// <summary>Raised after the active document changes (its editor, or <see langword="null"/> when none remain).</summary>
    public event EventHandler<CodeEditor?>? ActiveDocumentChanged;
    #endregion

    #region Properties
    /// <summary>The underlying tab panel (for styling or advanced tab operations).</summary>
    public TabPanel Tabs => _panel;

    /// <summary>The selected document's editor, or <see langword="null"/> when none are open.</summary>
    public CodeEditor? ActiveEditor => _panel.ActiveTab?.Content as CodeEditor;

    /// <summary>The selected document's name (tab label), or <see langword="null"/> when none are open.</summary>
    public string? ActiveDocumentName => _panel.ActiveTabName;

    /// <summary>All open editors, in tab order.</summary>
    public IReadOnlyList<CodeEditor> Editors => _panel.Tabs.Select(t => (CodeEditor)t.Content).ToList();

    /// <summary>The number of open documents.</summary>
    public int DocumentCount => _panel.TabCount;

    /// <summary>When <see langword="true"/>, closing a document with unsaved changes (see <see cref="IsDirty"/>)
    /// first shows a modal "Discard changes?" confirmation and only closes on confirm. Default
    /// <see langword="false"/>.</summary>
    /// <remarks>Requires an ambient <see cref="UI.Overlay"/> (present after <see cref="UI.Start"/>); without one, the
    /// close proceeds.</remarks>
    public bool ConfirmOnClose { get; set; }

    /// <summary>Overlay to host the confirm-on-close dialog on. Defaults to the ambient <see cref="UI.Overlay"/>;
    /// set explicitly to host it elsewhere (also used by tests to avoid mutating the global overlay).</summary>
    internal Overlay? DialogOverlay { get; set; }
    #endregion

    #region Methods
    /// <summary>Opens a document in a new tab and selects it. Returns the created editor. Set
    /// <paramref name="closable"/> to <see langword="false"/> to pin it (no ✕).</summary>
    public CodeEditor OpenDocument(string name, string text = "", Language? language = null, bool closable = true)
    {
        CodeEditor editor = null!;
        UI.Invoke(() =>
        {
            editor = new CodeEditor(language ?? _defaultLanguage) { Text = text };
            editor.WithFrame(borderStyle: BorderStyle.None);   // each editor gets its own scroll viewport
            var tab = _panel.AddTab(name, editor);
            if (!closable) tab.Closable = false;
            _baseline[editor] = editor.Text;                          // the "saved" baseline for dirty tracking
            editor.Editor.Changed += (_, _) => OnDocEdited(editor);   // auto-dirty once the text diverges
            _panel.SelectTab(tab);
        });
        DocumentOpened?.Invoke(this, editor);
        return editor;
    }

    /// <summary>Opens a new empty document with a generated "untitled-N" name.</summary>
    public CodeEditor NewDocument() => OpenDocument($"untitled-{++_untitled}", "", _defaultLanguage);

    /// <summary>Closes the given document, honoring <see cref="DocumentClosing"/> and (when
    /// <see cref="ConfirmOnClose"/> is set) the unsaved-changes prompt.</summary>
    public void CloseDocument(CodeEditor editor)
    {
        if (TabOf(editor) is { } tab) BeginClose(tab, editor);
    }

    /// <summary>Closes the active document (if any).</summary>
    public void CloseActiveDocument() { if (ActiveEditor is { } e) CloseDocument(e); }

    /// <summary>Closes every document immediately, without the <see cref="DocumentClosing"/> veto or confirm prompt
    /// (each still raises <see cref="DocumentClosed"/>).</summary>
    /// <remarks>For resetting the group — e.g. reloading a different set of files.</remarks>
    public void Clear() => UI.Invoke(() =>
    {
        foreach (var tab in _panel.Tabs.ToList()) _panel.RemoveTab(tab);
    });

    /// <summary>Whether a document has unsaved changes (its text differs from when it was opened or last marked
    /// saved via <see cref="SetDirty"/>).</summary>
    public bool IsDirty(CodeEditor editor) => _dirty.Contains(editor);

    /// <summary>Marks a document dirty, or clean (<paramref name="dirty"/> = <see langword="false"/> also records the
    /// current text as the new saved baseline). Toggles a leading "● " marker on the tab label.</summary>
    public void SetDirty(CodeEditor editor, bool dirty)
    {
        if (!dirty && _baseline.ContainsKey(editor)) _baseline[editor] = editor.Text;   // "saved": reset baseline
        ApplyDirty(editor, dirty);
    }

    // Auto-dirty: an edit fires the editor's Changed; the doc is dirty exactly when its text differs from the
    // baseline (so an undo back to the baseline clears it).
    private void OnDocEdited(CodeEditor editor)
    {
        if (_baseline.TryGetValue(editor, out var b)) ApplyDirty(editor, editor.Text != b);
    }

    private void ApplyDirty(CodeEditor editor, bool dirty)
    {
        if (dirty == _dirty.Contains(editor)) return;
        if (dirty) _dirty.Add(editor); else _dirty.Remove(editor);
        if (TabOf(editor) is not { } tab) return;
        var marked = tab.Name.StartsWith(DirtyMark, StringComparison.Ordinal);
        if (dirty && !marked) tab.Name = DirtyMark + tab.Name;
        else if (!dirty && marked) tab.Name = tab.Name[DirtyMark.Length..];
    }

    // The single close funnel (both the ✕ and CloseDocument route here): apply the app veto, then the optional
    // unsaved-changes prompt, then remove. The prompt is modal/async, so it removes the tab from its callback.
    private void BeginClose(TabItem tab, CodeEditor editor)
    {
        if (RaiseClosing(editor)) return;   // app veto via DocumentClosing
        if (ConfirmOnClose && IsDirty(editor) && (DialogOverlay ?? UI.Overlay) is { } overlay)
        {
            var dialog = new Dialog("Unsaved changes", $"Discard changes to {NameWithoutMarker(tab)}?", DialogButtons.YesNo);
            dialog.Completed += (_, r) => { if (r == DialogResult.Yes) _panel.RemoveTab(tab); };
            dialog.Show(overlay);
            return;
        }
        _panel.RemoveTab(tab);
    }

    // The panel's ✕ asks to close: always cancel its built-in removal and route through BeginClose (which removes
    // the tab itself once the close is allowed to proceed).
    private void OnTabCloseRequested(object? sender, TabCloseEventArgs e)
    {
        e.Cancel = true;
        if (e.Tab.Content is CodeEditor editor) BeginClose(e.Tab, editor);
    }

    private void OnTabRemoved(object? sender, TabItem tab)
    {
        if (tab.Content is not CodeEditor editor) return;
        _dirty.Remove(editor);
        _baseline.Remove(editor);
        DocumentClosed?.Invoke(this, editor);
    }

    private static string NameWithoutMarker(TabItem tab) =>
        tab.Name.StartsWith(DirtyMark, StringComparison.Ordinal) ? tab.Name[DirtyMark.Length..] : tab.Name;

    // Raises DocumentClosing; returns true when a handler canceled the close.
    private bool RaiseClosing(CodeEditor editor)
    {
        var args = new DocumentClosingEventArgs(editor);
        DocumentClosing?.Invoke(this, args);
        return args.Cancel;
    }

    private TabItem? TabOf(CodeEditor editor)
    {
        foreach (var t in _panel.Tabs) if (ReferenceEquals(t.Content, editor)) return t;
        return null;
    }

    // Each editor scrolls inside its own per-tab frame (see OpenDocument), so this group must fill a surrounding
    // ControlFrame's viewport rather than balloon to content height — otherwise that outer frame becomes a second
    // scroller and the mouse wheel (routed to the per-tab frame) targets the wrong one and no-ops.
    /// <inheritdoc/>
    protected internal override bool FillsFrameViewport => true;

    /// <inheritdoc/>
    protected internal override HelpInfo? GetHelpInfo() => new HelpInfo("Editors", "Editor Group",
        "A tabbed group of code editors.")
        .WithKey("Alt+← / Alt+→", "Switch tab")
        .WithKey("Click ✕", "Close tab")
        .WithKey("Click +", "New tab");
    #endregion

    #region Fields
    private readonly TabPanel _panel;
    private readonly Language _defaultLanguage;
    private int _untitled;
    // Dirty tracking: the set of editors whose text differs from their baseline, and the baseline ("saved") text
    // per editor. Auto-updated on edit; cleared when a tab is removed.
    private readonly HashSet<CodeEditor> _dirty = new();
    private readonly Dictionary<CodeEditor, string> _baseline = new();
    private const string DirtyMark = "● ";
    #endregion
}

/// <summary>Arguments for <see cref="MultiTabCodeEditor.DocumentClosing"/>. Set <see cref="Cancel"/> to keep the
/// document open (e.g. after confirming unsaved changes).</summary>
public sealed class DocumentClosingEventArgs : EventArgs
{
    internal DocumentClosingEventArgs(CodeEditor editor) => Editor = editor;

    /// <summary>The document's editor.</summary>
    public CodeEditor Editor { get; }

    /// <summary>Set to <see langword="true"/> to cancel the close and keep the document open.</summary>
    public bool Cancel { get; set; }
}
