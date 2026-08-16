namespace Jumbee.Console;

using System;

using ColorCode;

/// <summary>
/// A composite control pairing a <see cref="TextEditor"/> with a <see cref="LineNumberGutter"/> docked to its left.
/// </summary>
/// <remarks>
/// The gutter is kept in sync with the editor (line count + active line) by listening to the editor's
/// <see cref="TextEditor.Changed"/> event — the canonical "child controls react to each other" pattern that
/// <see cref="CompositeControl"/> exists to support.
/// <para>
/// The composite sizes to its content (the editor's wrapped row count), so wrapping it in a <see cref="ControlFrame"/>
/// (e.g. <c>codeEditor.WithFrame()</c>) scrolls the gutter and text together with an accurate scrollbar; the
/// editor's caret is kept in view by <see cref="AutoScroll"/> driving that frame.
/// </para>
/// </remarks>
public class CodeEditor : CompositeControl, IScrollable
{
    #region Constructors
    /// <summary>Creates a code editor highlighted for the given built-in <see cref="Language"/>.</summary>
    public CodeEditor(Language language = Language.None) : this(new TextEditor(language)) { }

    /// <summary>Creates an editor highlighted by a custom ColorCode grammar — for languages outside the built-in
    /// <see cref="Language"/> enum (e.g. a Mermaid grammar defined by another project).</summary>
    public CodeEditor(ILanguage customLanguage) : this(new TextEditor(customLanguage)) { }

    private CodeEditor(TextEditor editor)
    {
        _editor = editor;
        _gutter = new LineNumberGutter
        {
            // Pulled each render: wrap-aware labels (0 = a soft-wrapped continuation row) + the caret's visual row.
            // The gutter is content-tall like the editor, so the surrounding frame scrolls them together aligned.
            RowsProvider = () => (_editor.VisualLineNumbers(), _editor.CaretVisualRow),
        };

        // Inter-child wiring: the gutter follows the editor, and the composite re-measures + scrolls to the caret.
        _editor.Changed += (_, _) => OnEditorChanged();

        // A wheel notch over the editor scrolls OUR frame — the same scroll target AutoScroll drives. The editor
        // isn't framed itself (its own OnMouseWheel is a no-op), so we bubble its MouseWheeled event up to the
        // composite. This is the "composite owns its children and wires their events itself" pattern.
        _editor.MouseWheeled += (_, delta) => Frame?.Scroll(delta);

        SetContent(new DockPanel(DockedControlPlacement.Left, _gutter, _editor));
        _gutter.LineCount = _editor.LineCount;   // initial sync; thereafter OnEditorChanged keeps it in step
    }
    #endregion

    #region Properties
    /// <summary>The wrapped text editor (focus this to type; e.g. <c>UI.SetFocus(codeEditor.Editor)</c>).</summary>
    public TextEditor Editor => _editor;

    /// <summary>The line-number gutter.</summary>
    public LineNumberGutter Gutter => _gutter;

    /// <summary>When <see langword="true"/>, the editor ignores edits (typing/Backspace/Delete/Enter/Tab/paste) but
    /// still navigates and scrolls — a read-only code viewer. Passthrough to <see cref="TextEditor.ReadOnly"/>.</summary>
    public bool ReadOnly
    {
        get => _editor.ReadOnly;
        set => _editor.ReadOnly = value;
    }

    /// <summary>The editor's text. Setting it loads the document with the caret at the start (top of the file).</summary>
    public string Text
    {
        get => _editor.Text;
        set
        {
            _editor.Text = value;       // raises Changed -> OnEditorChanged
            _editor.CaretIndex = 0;     // open at the top, like a file editor, not at the end of the text
        }
    }
    #endregion

    #region Methods
    // Our content height is the editor's wrapped row count at the editor's width (our width minus the gutter), so a
    // surrounding frame sizes us to content and its scrollbar/scroll-range are accurate.
    /// <inheritdoc/>
    public virtual int MeasureHeight(int width) =>
        Math.Max(1, _editor.VisualRowCount(Math.Max(1, width - _gutter.Width)));

    // Named "Editor" so it shares the editor tab (and the focused composite opens it). Describes the code editor.
    /// <inheritdoc/>
    protected internal override HelpInfo? GetHelpInfo() => new HelpInfo("Editor", "Editor",
        "A code editor with line numbers and syntax highlighting.")
        .WithKey("Arrows", "Move the caret")
        .WithKey("Tab", "Indent")
        .WithKey("Enter", "New line")
        .WithKey("Alt+↑ / Alt+↓", "Scroll");

    // Re-baseline the cached "what the gutter currently shows" on every (re)layout, so the first edit after a
    // resize compares against the just-laid-out width rather than a stale measurement.
    /// <inheritdoc/>
    protected override void Control_OnInitialization()
    {
        base.Control_OnInitialization();
        _lastRowCount = EditorRowCount();
        _lastActiveRow = _editor.CaretVisualRow;
    }

    private void OnEditorChanged()
    {
        // Sync the gutter's logical line count (its own setter is already a no-op when the count is unchanged).
        _gutter.LineCount = _editor.LineCount;

        var rows = EditorRowCount();
        var activeRow = _editor.CaretVisualRow;

        // Only repaint the gutter when something it actually draws moved: the active-row highlight, or the row
        // labels (which track the wrapped row count). Typing within a line changes neither, so the gutter stays
        // valid and is not re-rendered — the per-control invalidation is preserved instead of being defeated by a
        // blanket refresh on every keystroke.
        if (activeRow != _lastActiveRow || rows != _lastRowCount) _gutter.Refresh();

        // Re-measure our content height for the frame's scroll range only when the wrapped row count changed; an
        // in-line edit that adds/removes no row needs no re-layout (Initialize would otherwise run every keystroke).
        if (rows != _lastRowCount) Initialize();

        AutoScroll();   // keep the caret in view by scrolling our frame (no-op when the caret's row is unchanged)
        (_lastActiveRow, _lastRowCount) = (activeRow, rows);
    }

    // The editor's wrapped row count at its current width — our content height, and the value that decides whether a
    // re-layout / gutter relabel is needed.
    private int EditorRowCount() => _editor.VisualRowCount(Math.Max(1, _editor.ActualWidth));

    // The editor itself isn't framed, so scroll OUR ControlFrame to keep the editor's caret row within the viewport.
    private void AutoScroll() => ScrollIntoView(_editor.CaretVisualRow);
    #endregion

    #region Fields
    private readonly TextEditor _editor;
    private readonly LineNumberGutter _gutter;
    // Cached snapshot of what the gutter currently reflects (wrapped row count + caret's visual row), so
    // OnEditorChanged can skip the gutter repaint and the content re-measure when an edit changes neither — e.g.
    // typing within a line. -1 = nothing measured yet (forces the first sync). Re-baselined on each (re)layout.
    private int _lastActiveRow = -1;
    private int _lastRowCount = -1;
    #endregion
}
