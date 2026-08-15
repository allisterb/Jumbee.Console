namespace Jumbee.Console;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using RazorConsole.Core;

using ConsoleGUI.Input;
using NTokenizers.Extensions.Spectre.Console;
using Spectre.Console;
using Spectre.Console.Rendering;
using RazorConsole.Core.Rendering.Syntax;
using ColorCode;

/// <summary>
/// A source language for syntax highlighting, shared by the text/code controls (e.g. <see cref="TextEditor"/>,
/// <see cref="CodeEditor"/>). <see cref="None"/> renders plain, unhighlighted text.
/// </summary>
public enum Language
{
    /// <summary>No highlighting; plain text.</summary>
    None,
    /// <summary>Markdown.</summary>
    Markdown,
    /// <summary>JSON.</summary>
    Json,
    /// <summary>HTML.</summary>
    Html,
    /// <summary>CSS.</summary>
    Css,
    /// <summary>C#.</summary>
    CSharp,
    /// <summary>SQL.</summary>
    Sql,
    /// <summary>TypeScript.</summary>
    TypeScript,
    /// <summary>XML.</summary>
    Xml,
    /// <summary>YAML.</summary>
    Yaml,
    /// <summary>TOML.</summary>
    Toml,
    /// <summary>C.</summary>
    C,
    /// <summary>C++.</summary>
    Cpp,
    /// <summary>Go.</summary>
    Go,
    /// <summary>Java.</summary>
    Java,
    /// <summary>Kotlin.</summary>
    Kotlin,
    /// <summary>Python.</summary>
    Python,
    /// <summary>Rust.</summary>
    Rust,
    /// <summary>Swift.</summary>
    Swift
}

/// <summary>
/// A text editor control with syntax highlighting for supported languages.
/// </summary>
public class TextEditor : Control, IScrollable
{
    #region Constructors
    /// <summary>Initializes a new <see cref="TextEditor"/> highlighted for the given <paramref name="language"/>, with optional caret display and blink.</summary>
    public TextEditor(Language language = Language.None, bool showCursor = true, bool blinkCursor = false) : base()
    {
        this._language = language;
        this._showCursor = showCursor;
        this._blinkCursor = blinkCursor;
        this._ccLang = ColorCodeLanguage(language);
        ansiConsole.wrap = true;   // character-level soft wrap at the buffer's right edge
        ApplyTheme();
    }

    /// <summary>Creates an editor highlighted by a custom ColorCode grammar — for languages outside the built-in
    /// <see cref="Language"/> enum (e.g. a Mermaid grammar defined by another project).</summary>
    /// <remarks>The grammar is applied by the same segment formatter/cache as the built-in languages.</remarks>
    public TextEditor(ILanguage customLanguage, bool showCursor = true, bool blinkCursor = false)
        : this(Language.None, showCursor, blinkCursor)
        => _ccLang = customLanguage ?? throw new ArgumentNullException(nameof(customLanguage));
    #endregion

    #region Properties
    /// <summary>Reports <see langword="true"/> so input routing delivers keys to the control.</summary>
    public override bool HandlesInput => true;
    /// <summary>Reports <see langword="true"/>: the caret indicates focus, so no default focus tint is drawn.</summary>
    protected override bool RendersOwnFocus => true;   // the caret/cursor shows focus

    /// <summary>The number of spaces inserted when the Tab key is pressed. Defaults to 4.</summary>
    public int TabWidth { get; set; } = 4;

    /// <summary>Whether the caret is drawn.</summary>
    public bool ShowCursor
    {
        get => _showCursor;
        set => SetAtomicProperty(ref _showCursor, value);
    }
    /// <summary>Whether the caret blinks.</summary>
    public bool BlinkCursor
    {
        get => _blinkCursor;
        set => SetAtomicProperty(ref _blinkCursor, value);
    }

    /// <summary>The caret's column within the viewport.</summary>
    public int CursorX
    {
        get => ansiConsole.CursorX;
        set
        {
            var x = Math.Clamp(value, 0, ActualWidth);
            var dx = value - ansiConsole.CursorX;
            ansiConsole.Cursor.MoveRight(dx);
            RenderCursor();
        }
    }

    /// <summary>The caret's row within the viewport.</summary>
    public int CursorY
    {
        get => ansiConsole.CursorY;
        set
        {
            var y = Math.Clamp(value, 0, ActualHeight);
            var dy = y - ansiConsole.CursorY;
            ansiConsole.Cursor.MoveDown(dy);
            RenderCursor();
        }
    }

    /// <summary>The full text content. Setting it replaces the buffer, moves the caret to the end, and raises
    /// <see cref="Changed"/>.</summary>
    public string Text
    {
        get => input;
        set
        {
            input = NormalizeNewlines(value);
            caretPosition = input.Length;
            _selAnchor = -1;
            _desiredColumn = -1;
            newInput = true;
            Initialize();   // new content -> re-measure the row count (for a framed editor's scroll range)
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>When <see langword="true"/>, edit keys (typing, Backspace/Delete/Enter/Tab) and paste are ignored;
    /// navigation (arrows/Home/End/PgUp/PgDn) and the caret still work.</summary>
    /// <remarks>Use for read-only viewers (e.g. a response body). Does not change appearance.</remarks>
    public bool ReadOnly { get; set; }

    /// <summary>The number of lines (newline count + 1).</summary>
    public int LineCount
    {
        get
        {
            var count = 1;
            foreach (var c in input) if (c == '\n') count++;
            return count;
        }
    }

    /// <summary>The caret's index into the text, clamped to <c>0..Text.Length</c>. Setting it moves the caret and
    /// raises <see cref="Changed"/> (so adornments/auto-scroll follow); e.g. set to 0 to move to the start.</summary>
    public int CaretIndex
    {
        get => caretPosition;
        set
        {
            caretPosition = Math.Clamp(value, 0, input.Length);
            _selAnchor = -1;
            _selectionDirty = true;
            _desiredColumn = -1;
            Invalidate();
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>The selected text, or empty when there is no selection.</summary>
    public string SelectedText => HasSelection ? input.Substring(SelStart, SelEnd - SelStart) : string.Empty;

    /// <summary>Selects the whole document (the same as Ctrl+A).</summary>
    public void SelectAll()
    {
        _selAnchor = 0;
        caretPosition = input.Length;
        _selectionDirty = true;
        Invalidate();
        Changed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>The zero-based logical line the caret is on (newline count before it).</summary>
    public int CaretLine
    {
        get
        {
            int line = 0;
            for (int i = 0; i < caretPosition && i < input.Length; i++)
                if (input[i] == '\n') line++;
            return line;
        }
    }

    /// <summary>The zero-based visual (wrapped) row the caret is on.</summary>
    public int CaretVisualRow => GetCursorPositionFromCaret(caretPosition).y;

    /// <summary>
    /// For each visual (wrapped) row, the 1-based logical line number when the row starts a logical line, or 0
    /// for a wrapped continuation row.
    /// </summary>
    /// <remarks>A line-number gutter uses this to stay aligned with soft-wrapped text.</remarks>
    public IReadOnlyList<int> VisualLineNumbers()
    {
        var rows = BuildVisualRows();
        var labels = new List<int>(rows.Count);
        int logicalLine = 1;
        for (int r = 0; r < rows.Count; r++)
        {
            var start = rows[r].start;
            bool firstOfLine = start == 0 || input[start - 1] == '\n';
            if (r > 0 && firstOfLine) logicalLine++;
            labels.Add(firstOfLine ? logicalLine : 0);
        }
        return labels;
    }
    #endregion

    #region Events
    /// <summary>Raised after the text or caret position changes (typing, paste, delete, navigation, or setting
    /// <see cref="Text"/>).</summary>
    /// <remarks>Composites use it to keep adornments — e.g. a line-number gutter — in sync.</remarks>
    public event EventHandler? Changed;
    #endregion

    #region Methods
    /// <inheritdoc/>
    protected override void ApplyTheme() =>
        _selectionBg = UI.StyleTheme.Selection.BackgroundColor?.ToConsoleGUIColor();

    /// <inheritdoc/>
    protected override void Control_OnInitialization()
    {
        if (!string.IsNullOrEmpty(input))
        {
            ansiConsole.Clear(true);
            WriteText(_language, input);
            HighlightSelection();
        }
        // Draw the cursor as part of initialization too, so a focused editor shows it on the very first paint
        // (Render is otherwise skipped once Validate clears the paint request).
        RenderCursor();
        Validate();
        _selectionDirty = false;
    }

    /// <summary>Re-highlights the text when it or the selection changed, then draws the caret.</summary>
    protected override void Render()
    {
        // Rebuild the buffer when the text changed (newInput) OR the selection changed (so the old highlight is
        // cleared and the new range painted); a pure caret move needs only the cursor redrawn.
        if (newInput || _selectionDirty)
        {
            ansiConsole.Clear(true);
            WriteText(_language, input);
            HighlightSelection();
            newInput = false;
            _selectionDirty = false;
        }
        RenderCursor();
    }

    // Paints the selection background over the (already syntax-coloured) buffer cells, keeping each glyph's
    // foreground so highlighting doesn't lose the syntax colours. Maps the selected [start, end) index range to
    // per-visual-row column spans via the same wrap the renderer uses.
    private void HighlightSelection()
    {
        if (_selectionBg is not { } bg || !HasSelection) return;
        int s = SelStart, e = SelEnd;
        var rows = BuildVisualRows();
        int h = consoleBuffer.Size.Height, w = consoleBuffer.Size.Width;
        for (int r = 0; r < rows.Count && r < h; r++)
        {
            var (start, end) = rows[r];
            int a = Math.Max(s, start);
            int b = Math.Min(e, end);
            // A selected line break extends the highlight one cell past the row's last glyph (a visual cue that the
            // newline is included), so an empty selected line still shows a mark.
            bool newlineSelected = end < input.Length && input[end] == '\n' && s <= end && e > end;
            if (a >= b && !newlineSelected) continue;
            int colStart = ColumnOf(start, a);
            int colEnd = ColumnOf(start, Math.Max(a, b));
            if (newlineSelected) colEnd = Math.Min(w, colEnd + 1);
            for (int x = colStart; x < colEnd && x < w; x++)
            {
                var ch = consoleBuffer[x, r].Character;
                consoleBuffer.Write(new ConsoleGUI.Space.Position(x, r), ch.WithBackground(bg));
            }
        }
    }

    /// <summary>Inserts pasted <paramref name="text"/> at the caret in one shot, replacing any selection.</summary>
    // Insert a whole paste at the caret in one shot (no per-key re-interpretation; newlines kept verbatim).
    public override void OnPaste(string text)
    {
        if (ReadOnly || string.IsNullOrEmpty(text)) return;
        text = NormalizeNewlines(text);
        if (HasSelection) DeleteSelection();   // a paste replaces the selection
        input = input.Insert(caretPosition, text);
        caretPosition += text.Length;
        _desiredColumn = -1;
        newInput = true;
        AutoScroll();
        Initialize();   // a paste changes the row count -> re-measure (for a framed editor's scroll range)
        Changed?.Invoke(this, EventArgs.Empty);
    }

    // Editor state uses '\n' as the only line separator. Collapse CRLF/CR (e.g. from a Windows clipboard paste)
    // so a stray '\r' can't sit invisibly in the buffer: both the renderer and the caret-position math skip '\r',
    // which makes it a zero-width character the caret can land on — pressing an arrow key then appears to do
    // nothing and the drawn cursor falls out of sync with the logical caret.
    private static string NormalizeNewlines(string? text) =>
        string.IsNullOrEmpty(text) ? string.Empty : text.Replace("\r\n", "\n").Replace('\r', '\n');

    /// <inheritdoc/>
    protected override void OnInput(InputEvent inputEvent)
    {
        var key = inputEvent.Key;
        var shift = (key.Modifiers & ConsoleModifiers.Shift) != 0;
        var ctrl = (key.Modifiers & ConsoleModifiers.Control) != 0;
        var vertical = key.Key is ConsoleKey.UpArrow or ConsoleKey.DownArrow
            or ConsoleKey.PageUp or ConsoleKey.PageDown;
        var hadSelection = HasSelection;
        var selStart = SelStart;   // valid only when hadSelection; captured before the anchor is cleared below
        var selEnd = SelEnd;

        switch (key.Key)
        {
            case ConsoleKey.A when ctrl:
                _selAnchor = 0;
                caretPosition = input.Length;
                _selectionDirty = true;
                inputEvent.Handled = true;
                break;

            case ConsoleKey.LeftArrow:
                // Without Shift, an existing selection collapses to its start rather than moving the caret.
                if (!shift && hadSelection) { caretPosition = selStart; ClearSelection(); }
                else { ExtendOrClear(shift); if (caretPosition > 0) caretPosition--; }
                inputEvent.Handled = true;
                break;
            case ConsoleKey.RightArrow:
                if (!shift && hadSelection) { caretPosition = selEnd; ClearSelection(); }
                else { ExtendOrClear(shift); if (caretPosition < input.Length) caretPosition++; }
                inputEvent.Handled = true;
                break;
            case ConsoleKey.UpArrow:
                ExtendOrClear(shift);
                MoveCaretVertically(-1);
                inputEvent.Handled = true;
                break;
            case ConsoleKey.DownArrow:
                ExtendOrClear(shift);
                MoveCaretVertically(1);
                inputEvent.Handled = true;
                break;
            case ConsoleKey.PageUp:
                ExtendOrClear(shift);
                MoveCaretVertically(-(Frame?.ViewportSize.Height ?? 10));
                inputEvent.Handled = true;
                break;
            case ConsoleKey.PageDown:
                ExtendOrClear(shift);
                MoveCaretVertically(Frame?.ViewportSize.Height ?? 10);
                inputEvent.Handled = true;
                break;
            case ConsoleKey.Home:
                ExtendOrClear(shift);
                MoveCaretHome();
                inputEvent.Handled = true;
                break;
            case ConsoleKey.End:
                ExtendOrClear(shift);
                MoveCaretEnd();
                inputEvent.Handled = true;
                break;

            case ConsoleKey.Backspace:
                if (ReadOnly) break;
                if (HasSelection) DeleteSelection();
                else if (caretPosition > 0) { input = input.Remove(--caretPosition, 1); newInput = true; }
                else break;
                inputEvent.Handled = true;
                break;
            case ConsoleKey.Delete:
                if (ReadOnly) break;
                if (HasSelection) DeleteSelection();
                else if (caretPosition < input.Length) { input = input.Remove(caretPosition, 1); newInput = true; }
                else break;
                inputEvent.Handled = true;
                break;
            case ConsoleKey.Enter:
                if (ReadOnly) break;
                if (HasSelection) DeleteSelection();
                input = input.Insert(caretPosition++, "\n");
                newInput = true;
                inputEvent.Handled = true;
                break;
            case ConsoleKey.Tab:
                // Plain Tab now reaches the editor (focus traversal moved to Ctrl+arrows), so indent with it.
                // Insert spaces rather than a literal '\t' so the wrap/caret column math stays simple and exact.
                if (ReadOnly) break;
                if (HasSelection) DeleteSelection();
                var indent = new string(' ', Math.Max(0, TabWidth));
                input = input.Insert(caretPosition, indent);
                caretPosition += indent.Length;
                newInput = true;
                inputEvent.Handled = true;
                break;
            default:
                if (!ReadOnly && !char.IsControl(key.KeyChar))
                {
                    if (HasSelection) DeleteSelection();   // typing replaces the selection
                    input = input.Insert(caretPosition++, key.KeyChar.ToString());
                    newInput = true;
                    inputEvent.Handled = true;
                }
                break;
        }
        if (!vertical) _desiredColumn = -1;   // a horizontal move or an edit drops the remembered column
        AutoScroll();
        // An edit (newInput) can change the wrapped row count, so re-lay-out for a framed editor's scroll range;
        // navigation only needs a repaint.
        if (newInput) Initialize(); else Invalidate();
        Changed?.Invoke(this, EventArgs.Empty);
    }

    // Shift+navigation extends the selection (starting an anchor if none); an unmodified move drops it. Marks the
    // selection dirty so the highlight is re-rendered even when the text itself is unchanged.
    private void ExtendOrClear(bool shift)
    {
        if (shift) { if (_selAnchor < 0) _selAnchor = caretPosition; _selectionDirty = true; }
        else if (_selAnchor >= 0) ClearSelection();
    }

    private void ClearSelection()
    {
        if (_selAnchor < 0) return;
        _selAnchor = -1;
        _selectionDirty = true;
    }

    // Removes the selected span, leaves the caret at its start, and drops the selection. Marks newInput (the text
    // changed) so the buffer is rebuilt and the row count re-measured.
    private void DeleteSelection()
    {
        int s = SelStart, e = SelEnd;
        input = input.Remove(s, e - s);
        caretPosition = s;
        _selAnchor = -1;
        newInput = true;
    }

    private bool HasSelection => _selAnchor >= 0 && _selAnchor != caretPosition;
    private int SelStart => Math.Min(_selAnchor < 0 ? caretPosition : _selAnchor, caretPosition);
    private int SelEnd => Math.Max(_selAnchor < 0 ? caretPosition : _selAnchor, caretPosition);

    /// <summary>Positions and shows or hides the terminal caret according to the current focus and cursor settings.</summary>
    protected void RenderCursor()
    {
        var pos = GetCursorPositionFromCaret(caretPosition);
        ansiConsole.SetCursorPosition(pos.x, pos.y);
        if (IsFocused && _showCursor)
        {
            ansiConsole.BufferCursor.Style = _blinkCursor ? CursorStyle.BlinkingBlock : CursorStyle.SteadyBlock;
            ansiConsole.Cursor.Show();
        }
        else
        {
            ansiConsole.Cursor.Hide();
        }
    }

    /// <summary>The column the text wraps at — the rendered buffer width.</summary>
    private int WrapWidth => Math.Max(1, ActualWidth);

    /// <inheritdoc/>
    // Report the wrapped row count as the content height so a surrounding ControlFrame sizes the editor to its
    // content and scrolls accurately (measured at the layout width, which may differ from the current ActualWidth).
    public virtual int MeasureHeight(int width) => BuildVisualRows(width).Count;

    /// <inheritdoc/>
    protected internal override HelpInfo? GetHelpInfo() => new HelpInfo("Editor", "Editor", "A multi-line text editor.")
        .WithKey("Arrows", "Move the caret")
        .WithKey("Shift+Arrows", "Select text")
        .WithKey("Ctrl+A", "Select all")
        .WithKey("Tab", "Indent")
        .WithKey("Enter", "New line");

    /// <summary>The number of visual (wrapped) rows the text occupies at the given width — the editor's content
    /// height.</summary>
    /// <remarks>A composite (e.g. <see cref="CodeEditor"/>) uses it to size/scroll itself around the editor.</remarks>
    public int VisualRowCount(int width) => BuildVisualRows(Math.Max(1, width)).Count;

    // Splits the document into visual rows (as the renderer does) using the same character wrap. Each row is a
    // half-open [start, end) range of indices into `input`; a '\n' ends a row and is not part of it. The wrap
    // width defaults to the current buffer width, but can be supplied (e.g. while measuring at a new layout width).
    private List<(int start, int end)> BuildVisualRows(int? wrapWidth = null)
    {
        int width = Math.Max(1, wrapWidth ?? WrapWidth);
        // Memoize on (text, width): the wrap is a pure function of those. A framed control's content height depends on
        // its width, which drives the surrounding layout to converge — re-measuring the whole document many times for
        // one layout pass. Caching collapses those repeats to O(1). All callers read the returned list, never mutate
        // it, so sharing the cached instance is safe. Invalidated implicitly: an edit reassigns `input` (new string
        // reference) and a width change misses on the key.
        if (_cachedRows is not null && ReferenceEquals(input, _cachedRowsText) && width == _cachedRowsWidth)
            return _cachedRows;

        visualRowsBuilt++;   // instrumentation: counts actual (uncached) wrap computations
        var rows = new List<(int, int)>();
        int n = input.Length;
        int rowStart = 0, col = 0, i = 0;
        while (i < n)
        {
            char c = input[i];
            if (c == '\n')
            {
                rows.Add((rowStart, i));
                i++;
                rowStart = i;
                col = 0;
            }
            else
            {
                int w = c.GetCellWidth();
                if (w <= 0) { i++; continue; }                 // zero-width: renderer skips it too
                if (col > 0 && col + w > width)                // glyph won't fit -> wrap before it
                {
                    rows.Add((rowStart, i));
                    rowStart = i;
                    col = 0;
                }
                col += w;
                i++;
            }
        }
        rows.Add((rowStart, n));

        _cachedRows = rows;
        _cachedRowsText = input;
        _cachedRowsWidth = width;
        return rows;
    }

    // Visual column of an index within its row (sum of cell widths from the row start).
    private int ColumnOf(int start, int caret)
    {
        int col = 0;
        for (int i = start; i < caret && i < input.Length; i++)
        {
            char c = input[i];
            if (c == '\n' || c == '\r') continue;
            int w = c.GetCellWidth();
            if (w > 0) col += w;
        }
        return col;
    }

    private (int x, int y) GetCursorPositionFromCaret(int caret)
    {
        caret = Math.Clamp(caret, 0, input.Length);
        var rows = BuildVisualRows();
        for (int r = 0; r < rows.Count; r++)
        {
            var (start, end) = rows[r];
            // The caret is on row r when strictly inside it, or at its end only if that end is a hard break (a
            // newline or the document end). At a wrap point the caret belongs to the next row's start, so we fall
            // through to the next iteration (whose start == caret).
            bool atHardEnd = caret == end && (r == rows.Count - 1 || input[end] == '\n');
            if (caret < end || atHardEnd)
            {
                int x = ColumnOf(start, caret);
                // A caret past the last cell of a completely full row shows at the start of the next visual row.
                if (x >= WrapWidth) return (x - WrapWidth, r + 1);
                return (x, r);
            }
        }
        var last = rows[^1];
        return (ColumnOf(last.start, input.Length), rows.Count - 1);
    }

    // The index on visual row `visualRow` nearest (rounded down to) column `targetColumn`.
    private int CaretIndexAt(int visualRow, int targetColumn)
    {
        var rows = BuildVisualRows();
        visualRow = Math.Clamp(visualRow, 0, rows.Count - 1);
        var (start, end) = rows[visualRow];
        int i = start, col = 0;
        while (i < end && col < targetColumn)
        {
            int w = input[i].GetCellWidth();
            if (w <= 0) { i++; continue; }
            if (col + w > targetColumn) break;
            col += w;
            i++;
        }
        return i;
    }

    private void MoveCaretVertically(int direction)
    {
        var (currentX, currentY) = GetCursorPositionFromCaret(caretPosition);
        // Remember the column the user is aiming for so a run of Up/Down keeps it even across short lines.
        if (_desiredColumn < 0) _desiredColumn = currentX;
        caretPosition = CaretIndexAt(currentY + direction, _desiredColumn);
    }
    
    private void MoveCaretHome()
    {
        int i = caretPosition;
        while (i > 0 && input[i-1] != '\n')
        {
            i--;
        }
        caretPosition = i;
    }

    private void MoveCaretEnd()
    {
         while (caretPosition < input.Length && input[caretPosition] != '\n')
         {
             caretPosition++;
         }
    }

    private void AutoScroll()
    {
        if (Frame != null)
        {
            var (x, y) = GetCursorPositionFromCaret(caretPosition);

            int top = Frame.Top;
            int viewportHeight = Frame.ViewportSize.Height;

            if (y < top)
            {
                Frame.Top = y;
            }
            else if (y >= top + viewportHeight)
            {
                Frame.Top = y - viewportHeight + 1;
            }
        }
    }

    private void WriteText(Language language, string text)
    {
        // Render each logical line unwrapped (a large profile width) so Spectre never word-wraps; the buffer then
        // applies one deterministic character wrap at its real width (ansiConsole.wrap), which BuildVisualRows /
        // the caret math mirror exactly. The editor owns this profile and always wants the inflated render width,
        // so it stays set between calls (re-set here every render; always >= 2, never the buffer's possibly-0 width).
        ansiConsole.Profile.Width = LongestLineWidth(text) + 1;
        WriteHighlighted(language, text);
    }

    private static int LongestLineWidth(string text)
    {
        int max = 1, col = 0;
        foreach (var c in text)
        {
            if (c == '\n') { if (col > max) max = col; col = 0; }
            else if (c != '\r') col += c.GetCellWidth();
        }
        return Math.Max(max, col);
    }

    private void WriteHighlighted(Language language, string text)
    {
        // ColorCode-highlighted languages (built-in or a custom grammar) go through the segment cache (a full regex
        // parse is the single most expensive operation here); the remaining writers highlight cheaply/directly.
        if (_ccLang is { } ccLang)
        {
            ansiConsole.Write(HighlightSegments(text, ccLang));
            return;
        }

        switch (language)
        {
            case Language.None: ansiConsole.Write(text); break;
            case Language.Json: ansiConsole.WriteJson(text); break;
            case Language.Yaml: ansiConsole.WriteYaml(text); break;
            case Language.Toml: ansiConsole.WriteToml(text); break;
            case Language.C: ansiConsole.WriteC(text); break;
            case Language.Go: ansiConsole.WriteGo(text); break;
            case Language.Kotlin: ansiConsole.WriteKotlin(text); break;
            case Language.Rust: ansiConsole.WriteRust(text); break;
            case Language.Swift: ansiConsole.WriteSwift(text); break;
        }
    }

    // The ColorCode grammar for languages highlighted through SpectreSegmentFormatter, or null for languages that use
    // a different writer (or render plain).
    private static ILanguage? ColorCodeLanguage(Language language) => language switch
    {
        Language.Markdown => Languages.Markdown,
        Language.CSharp => Languages.CSharp,
        Language.TypeScript => Languages.Typescript,
        Language.Sql => Languages.Sql,
        Language.Html => Languages.Html,
        Language.Css => Languages.Css,
        Language.Xml => Languages.Xml,
        Language.Cpp => Languages.Cpp,
        Language.Java => Languages.Java,
        Language.Python => Languages.Python,
        _ => null,
    };

    // Highlighting a document is a full regex parse — the single most expensive per-render operation — yet it depends
    // only on (language, text), not on the control's size. The buffer is rebuilt on every layout-driven
    // re-initialization (resize, split-drag, dock-panel convergence), which previously re-parsed the whole document
    // each time. Cache the parsed segments and reuse them while the text reference and language are unchanged, so a
    // re-layout just re-blits the cached highlight and only a real edit re-parses. The formatter reuses its own list
    // between calls, so the segments are copied out to own them.
    private IReadOnlyList<Segment> HighlightSegments(string text, ILanguage ccLang)
    {
        // The grammar (_ccLang) is fixed for the editor's lifetime, so the cache key is just the text reference: a
        // real edit reassigns `input` to a new string, invalidating it; a re-layout reuses the same reference and hits.
        if (_cachedSegments is not null && ReferenceEquals(text, _cachedText))
            return _cachedSegments;

        _cachedSegments = new List<Segment>(ccFormatter.Format(text, ccLang, ccSyntaxTheme, ccSyntaxOptions));
        _cachedText = text;
        return _cachedSegments;
    }
    #endregion

    #region Fields
    private Language _language;
    private bool _showCursor;
    private bool _blinkCursor;
    private string input = string.Empty;
    private bool newInput;
    private int caretPosition = 0;
    // The visual column a vertical (Up/Down) move aims for, preserved across a run of them; -1 = recompute from
    // the caret's current column on the next vertical move. Reset by any horizontal move or edit.
    private int _desiredColumn = -1;
    // Selection anchor index (-1 = no selection); the selection is [min(anchor,caret), max(anchor,caret)).
    private int _selAnchor = -1;
    // True when the selection range changed without a text change (a Shift-move, or a cleared selection), so Render
    // rebuilds the buffer to repaint/clear the highlight even though newInput is false.
    private bool _selectionDirty;
    // The selection highlight background (captured from the theme in ApplyTheme), or null if the theme sets none.
    private ConsoleGUI.Data.Color? _selectionBg;

    // Test instrumentation: number of actual BuildVisualRows computations (cache misses). Used by tests to guard the
    // wrap-memoization against layout-convergence re-measurement blowups.
    internal int visualRowsBuilt;
    // Memoized wrap layout, keyed by (text reference, wrap width) — see BuildVisualRows.
    private List<(int start, int end)>? _cachedRows;
    private string? _cachedRowsText;
    private int _cachedRowsWidth = -1;

    SpectreSegmentFormatter ccFormatter = new SpectreSegmentFormatter();
    SyntaxTheme ccSyntaxTheme = SyntaxTheme.CreateDefault();
    SyntaxOptions ccSyntaxOptions = new SyntaxOptions() { TabWidth = 0,   };

    // The resolved ColorCode grammar for this editor (a built-in mapped from _language, or a custom ILanguage passed
    // to the constructor), or null for languages that use a non-ColorCode writer or render plain. Fixed at construction.
    private readonly ILanguage? _ccLang;

    // Cached syntax-highlight segments, reused across size-only re-renders (see HighlightSegments). Invalidated
    // implicitly: a real edit reassigns `input` (a new string reference), so it misses this cache and re-parses. The
    // grammar and syntax theme are fixed at construction, so colours can't go stale.
    private List<Segment>? _cachedSegments;
    private string? _cachedText;
    #endregion
}
