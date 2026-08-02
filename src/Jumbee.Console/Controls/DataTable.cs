namespace Jumbee.Console;

using System;
using System.Collections.Generic;
using System.Linq;

using ConsoleGUI.Data;
using ConsoleGUI.Input;
using ConsoleGUI.Space;

using Spectre.Console;
using Spectre.Console.Rendering;

using CColor = ConsoleGUI.Data.Color;

/// <summary>
/// An interactive data grid.
/// </summary>
/// <remarks>
/// Columns and rows are supplied as text; the visible row window is laid out by Spectre.Console's
/// <see cref="Table"/> (column sizing, borders, wrapping), and this control adds the interactivity Spectre lacks: a
/// highlighted selected row, keyboard navigation, a scroll viewport over many rows with its own scrollbar,
/// click-to-select, and <see cref="SelectionChanged"/>/<see cref="RowActivated"/> events. The header stays fixed
/// while the rows scroll. (Inline cell editing is not supported yet.)
/// </remarks>
public class DataTable : Control
{
    #region Constructors
    /// <summary>Initializes a new <see cref="DataTable"/> with the given column headers.</summary>
    public DataTable(params string[] columns)
    {
        _columns = columns?.ToList() ?? new List<string>();
    }
    #endregion

    #region Events
    /// <summary>Raised when the selected row changes; the argument is the new row index (or -1 when empty).</summary>
    public event EventHandler<int>? SelectionChanged;

    /// <summary>Raised when the selected row is activated (Enter / double-click); the argument is the row index.</summary>
    public event EventHandler<int>? RowActivated;
    #endregion

    #region Properties
    /// <summary>Always <see langword="true"/>: the table handles its own selection keys (arrows, Home/End, Page
    /// Up/Down, Enter). No opt-in needed — unlike the base default, this is on.</summary>
    public override bool HandlesInput => true;
    /// <summary>Always <see langword="true"/>: rows receive hover and click, so a click selects a row and a
    /// double-click activates it. No opt-in needed — unlike the base default, this is on.</summary>
    protected override bool WantsMouse => true;

    /// <summary>The column headers.</summary>
    public IReadOnlyList<string> Columns => _columns;

    /// <summary>
    /// When <see langword="true"/> (the default), columns are dropped from the right rather than letting text wrap
    /// once the table is too narrow to show them all.
    /// </summary>
    /// <remarks>
    /// A table squeezed below the width its content needs otherwise wraps: headers break mid-word and values split
    /// across lines, which is unreadable and makes rows taller than one line. Dropping whole columns — keeping the
    /// leftmost, which is normally the identifier — is what terminal process monitors do. Set this to
    /// <see langword="false"/> to keep every column and accept the wrapping.
    /// </remarks>
    public bool DropNarrowColumns
    {
        get => _dropNarrowColumns;
        set
        {
            if (_dropNarrowColumns == value) return;
            _dropNarrowColumns = value;
            _chromeTotal = -1;   // chrome depends on the column set
            Invalidate();
        }
    }
    /// <summary>The number of data rows.</summary>
    public int RowCount => _rows.Count;

    /// <summary>The selected row index, or -1 when there are no rows.</summary>
    public int SelectedIndex
    {
        get => _rows.Count == 0 ? -1 : _selected;
        set => Select(value);
    }

    /// <summary>The selected row's cells, or <see langword="null"/> when there are no rows.</summary>
    public string[]? SelectedRow => _rows.Count == 0 ? null : _rows[Math.Clamp(_selected, 0, _rows.Count - 1)];
    #endregion

    #region Methods
    /// <summary>Appends a column with the given header.</summary>
    public void AddColumn(string header)
    {
        _columns.Add(header ?? string.Empty);
        _chromeTotal = -1;   // column set changed -> re-measure header/border height
        Invalidate();
    }

    /// <summary>Appends a row and returns its index.</summary>
    public int AddRow(params string[] cells)
    {
        _rows.Add(cells ?? []);
        Invalidate();
        return _rows.Count - 1;
    }

    /// <summary>Removes the row at <paramref name="index"/> (no-op if out of range).</summary>
    public void RemoveRow(int index)
    {
        if (index < 0 || index >= _rows.Count) return;
        _rows.RemoveAt(index);
        if (_selected >= _rows.Count) _selected = Math.Max(0, _rows.Count - 1);
        Invalidate();
    }

    /// <summary>Removes all rows and resets the selection and scroll.</summary>
    public void Clear()
    {
        _rows.Clear();
        _selected = 0;
        _scroll = 0;
        Invalidate();
    }

    /// <inheritdoc/>
    protected internal override HelpInfo? GetHelpInfo() => new HelpInfo("Table", "Data table", "A scrollable data grid.")
        .WithKey("Up / Down", "Select a row")
        .WithKey("PgUp / PgDn", "Page")
        .WithKey("Enter", "Activate the row");

    /// <inheritdoc/>
    protected override void OnInput(InputEvent inputEvent)
    {
        var visible = Math.Max(1, VisibleRows());
        switch (inputEvent.Key.Key)
        {
            case ConsoleKey.UpArrow: Select(_selected - 1); break;
            case ConsoleKey.DownArrow: Select(_selected + 1); break;
            case ConsoleKey.Home: Select(0); break;
            case ConsoleKey.End: Select(_rows.Count - 1); break;
            case ConsoleKey.PageUp: Select(_selected - visible); break;
            case ConsoleKey.PageDown: Select(_selected + visible); break;
            case ConsoleKey.Enter:
                if (_rows.Count > 0) RowActivated?.Invoke(this, _selected);
                break;
            default: return;
        }
        inputEvent.Handled = true;
    }

    /// <inheritdoc/>
    protected override void OnClick(Position position)
    {
        // A press that landed on the scrollbar (page/drag) isn't a row click; nor is the reserved last column.
        if (_pressedScrollbar) { _pressedScrollbar = false; return; }
        if (position.X >= ContentWidth) return;

        // Map the clicked screen row back to a data row (the rows start below the header chrome).
        var dataRow = _scroll + (position.Y - RowTop());
        if (dataRow >= 0 && dataRow < _rows.Count) Select(dataRow);
    }

    /// <inheritdoc/>
    // Double-click activates the row under the pointer, the mouse equivalent of Enter. Selecting first means
    // RowActivated always reports the row that was double-clicked, even if the first click of the pair was
    // swallowed (e.g. it landed on the scrollbar).
    protected override void OnDoubleClick(Position position)
    {
        if (_pressedScrollbar) { _pressedScrollbar = false; return; }
        if (position.X >= ContentWidth) return;

        var dataRow = _scroll + (position.Y - RowTop());
        if (dataRow < 0 || dataRow >= _rows.Count) return;

        Select(dataRow);
        RowActivated?.Invoke(this, dataRow);
    }

    // True when `position` is on the active scrollbar column, yielding the thumb/track metrics for the current view
    // (mirrors DrawScrollBar). `i` is the row within the bar (0 at the first data row).
    private bool OnScrollbar(Position position, out int i, out int visible, out int thumb, out int thumbPos)
    {
        visible = VisibleRows();
        i = thumb = thumbPos = 0;
        if (position.X != ActualWidth - 1 || visible <= 0 || _rows.Count <= visible) return false;
        i = position.Y - RowTop();
        if (i < 0 || i >= visible) return false;
        thumb = Math.Clamp((int)((long)visible * visible / _rows.Count), 1, visible);
        var maxScroll = _rows.Count - visible;
        thumbPos = maxScroll <= 0 ? 0 : (int)((long)(visible - thumb) * _scroll / maxScroll);
        return true;
    }

    /// <inheritdoc/>
    protected override void OnMousePress(Position position)
    {
        _pressedScrollbar = OnScrollbar(position, out var i, out var visible, out var thumb, out var thumbPos);
        if (!_pressedScrollbar) return;

        if (i >= thumbPos && i < thumbPos + thumb)
        {
            _scrollDragging = true;
            _scrollGrabOffset = i - thumbPos;   // grab point within the thumb, so the drag doesn't jump
            CaptureMouse();                     // keep the drag alive off the 1-col bar
        }
        else
        {
            SetScroll(_scroll + (i < thumbPos ? -visible : visible));   // click track above/below thumb -> page
        }
    }

    /// <inheritdoc/>
    protected override void OnMouseMove(Position position)
    {
        if (!_scrollDragging) return;
        var visible = VisibleRows();
        if (visible <= 0 || _rows.Count <= visible) return;
        var thumb = Math.Clamp((int)((long)visible * visible / _rows.Count), 1, visible);
        var available = visible - thumb;
        if (available <= 0) return;
        var desiredThumbPos = (position.Y - RowTop()) - _scrollGrabOffset;
        SetScroll((int)Math.Round((double)desiredThumbPos / available * (_rows.Count - visible)));
    }

    /// <inheritdoc/>
    protected override void OnMouseRelease(Position position)
    {
        if (!_scrollDragging) return;
        _scrollDragging = false;
        ReleaseMouse();
    }

    /// <inheritdoc/>
    protected override void OnMouseWheel(Position position, int delta) => SetScroll(_scroll + delta);

    private void Select(int index)
    {
        if (_rows.Count == 0) return;
        index = Math.Clamp(index, 0, _rows.Count - 1);
        if (index == _selected) return;
        _selected = index;
        ScrollToSelected();   // moving the selection keeps it in view (manual scroll doesn't)
        Invalidate();
        SelectionChanged?.Invoke(this, _selected);
    }

    /// <inheritdoc/>
    protected override void Render()
    {
        ansiConsole.Clear(true);
        var width = ActualWidth;
        var height = ActualHeight;
        if (width <= 0 || height <= 0 || _columns.Count == 0) return;

        // A selection made before the control had a size deferred its scroll until now (see ScrollToSelected).
        if (_scrollToSelectionPending) { _scrollToSelectionPending = false; ScrollToSelected(); }

        var visible = VisibleRows();
        ClampScroll(visible);

        // Lay out the visible window via a Spectre Table sized to leave the last column for the scrollbar.
        var shown = Math.Min(visible, Math.Max(0, _rows.Count - _scroll));

        // Render to segments once, then measure THIS table's chrome from them before writing. Measure()'s probe
        // can't be trusted for the highlight/scrollbar offsets: Spectre allocates column widths from cell content,
        // so a probe filled with placeholder cells lays its columns out differently from the real table and its
        // header wraps to a different number of lines. The difference grows as the control narrows, and it used to
        // put the selection bar one row too high per extra header line — highlighting the wrong process, and
        // mapping clicks to the wrong row. Counting the real segments costs nothing extra: Write would enumerate
        // them anyway.
        //
        // This derives the header height by subtracting the data lines, so it assumes one line per row. That holds
        // because BuildTable drops columns rather than letting content wrap (see FittingColumnCount) — with
        // DropNarrowColumns turned off, a hard-squeezed table wraps its cells, rows become taller than one line,
        // and these offsets drift again.
        var table = BuildTable(_scroll, shown);
        var segments = table.GetSegments(ansiConsole).ToList();
        var lines = 0;
        foreach (var s in segments) lines += s.TextSpan.Count('\n');
        _renderedChromeTop = shown > 0 ? Math.Max(0, lines - shown - 1) : ChromeTop();
        ansiConsole.Write(segments);

        HighlightSelectedRow(visible);
        DrawScrollBar(visible);
    }

    // Fills the buffer's selected-row line with the Selection style (full width, including cell padding) — Spectre
    // can't background a whole row, so we recolour the rendered cells in place, keeping each glyph.
    private void HighlightSelectedRow(int visible)
    {
        if (_rows.Count == 0) return;
        var offset = _selected - _scroll;
        if (offset < 0 || offset >= visible) return;
        var line = RowTop() + offset;
        if (line < 0 || line >= ActualHeight) return;

        var sel = UI.StyleTheme.Selection;
        var fg = sel.ForegroundColor?.ToConsoleGUIColor();
        var bg = sel.BackgroundColor?.ToConsoleGUIColor();
        for (var x = 1; x < ContentWidth - 1; x++)   // inside the left/right borders
        {
            var ch = consoleBuffer[x, line].Character;
            consoleBuffer.Write(new Position(x, line), new Character(ch.Content ?? ' ', fg, bg, ch.Decoration));
        }
    }

    private void DrawScrollBar(int visible)
    {
        var col = ActualWidth - 1;
        if (col < 0 || _rows.Count <= visible || visible <= 0) return;   // nothing scrolled off

        var top = RowTop();
        var thumb = Math.Clamp((int)((long)visible * visible / _rows.Count), 1, visible);
        var maxScroll = _rows.Count - visible;
        var thumbPos = maxScroll <= 0 ? 0 : (int)((long)(visible - thumb) * _scroll / maxScroll);
        for (var i = 0; i < visible; i++)
        {
            var y = top + i;
            if (y < 0 || y >= ActualHeight) continue;
            var isThumb = i >= thumbPos && i < thumbPos + thumb;
            consoleBuffer.Write(new Position(col, y), isThumb ? ScrollThumb : ScrollTrack);
        }
    }

    private Table BuildTable(int from, int count)
    {
        var columns = FittingColumnCount(from, count);
        var table = new Table { Border = TableBorder.Rounded, Width = Math.Max(1, ContentWidth), Expand = true };
        for (var c = 0; c < columns; c++)
            table.AddColumn(new TableColumn(new Markup(Markup.Escape(_columns[c]))) { NoWrap = true });   // 1 line per row
        for (var i = from; i < from + count && i < _rows.Count; i++)
        {
            var row = _rows[i];
            table.AddRow(Enumerable.Range(0, columns)
                .Select(c => (IRenderable)new Markup(Markup.Escape(c < row.Length ? row[c] ?? string.Empty : string.Empty)))
                .ToArray());
        }
        return table;
    }

    // How many columns fit at the current width without anything having to wrap, counted from the left.
    //
    // Squeezed below the width its content wants, Spectre shrinks columns and the text wraps — headers break
    // mid-word ("Memory %" over two lines) and values split ("11.4" -> "11" / ".4"), which also makes rows taller
    // than one line and throws off every row offset. Dropping whole columns from the right instead is what `top`
    // and vtop do, and it keeps the leftmost column — the identifier you actually navigate by — readable.
    //
    // A rounded-border table costs `3n + 1` cells of chrome for n columns (n+1 border glyphs plus two padding
    // cells per column), so the natural widths must fit in what's left. Measured against the rows on screen, not
    // the whole set, so one enormous value scrolled far away can't collapse the layout.
    private int FittingColumnCount(int from, int count)
    {
        var n = _columns.Count;
        if (!_dropNarrowColumns || n <= 1) return n;

        var widths = new int[n];
        for (var c = 0; c < n; c++) widths[c] = _columns[c].GetCellWidth();
        for (var i = from; i < from + count && i < _rows.Count; i++)
        {
            var row = _rows[i];
            for (var c = 0; c < n && c < row.Length; c++)
                widths[c] = Math.Max(widths[c], (row[c] ?? string.Empty).GetCellWidth());
        }

        var total = 0;
        for (var c = 0; c < n; c++) total += widths[c];
        while (n > 1 && 3 * n + 1 + total > ContentWidth) total -= widths[--n];
        return n;
    }


    // Keeps the scroll offset in range for the current row count / viewport (run every render). Does NOT force the
    // selected row into view — manual scrolling (wheel / scrollbar drag) is free to move the view off the selection.
    private void ClampScroll(int visible)
    {
        if (_rows.Count == 0) { _scroll = 0; return; }
        _scroll = Math.Clamp(_scroll, 0, Math.Max(0, _rows.Count - visible));
    }

    // Auto-scrolls so the selected row is visible — run only when the selection moves (keys / click / programmatic),
    // not every render, so a manual scroll persists.
    private void ScrollToSelected()
    {
        if (_rows.Count == 0) { _scroll = 0; return; }
        // Before the first layout there is no geometry to scroll against: ActualWidth/Height are 0, so VisibleRows
        // would measure against a one-cell-wide table. Defer to the first Render, which runs with a real size —
        // otherwise a selection set at construction time (a common thing to do) is either dropped or resolved
        // against a table that doesn't exist yet.
        if (!HasLayout) { _scrollToSelectionPending = true; return; }
        var visible = VisibleRows();
        _selected = Math.Clamp(_selected, 0, _rows.Count - 1);
        if (visible <= 0) { _scroll = _selected; return; }
        if (_selected < _scroll) _scroll = _selected;
        else if (_selected >= _scroll + visible) _scroll = _selected - visible + 1;
        ClampScroll(visible);
    }

    // Sets the scroll offset directly (mouse wheel / scrollbar), clamped, without touching the selection.
    private void SetScroll(int value)
    {
        if (_rows.Count == 0) return;
        var visible = VisibleRows();
        var clamped = Math.Clamp(value, 0, Math.Max(0, _rows.Count - visible));
        if (clamped == _scroll) return;
        _scroll = clamped;
        Invalidate();
    }

    // Data rows that fit below the header chrome.
    private int VisibleRows() => Math.Max(0, ActualHeight - ChromeTotal());

    // Non-data chrome rows (top border + header + header separator + bottom border). Measured from a ONE-row probe
    // — a header-only table omits the header separator that appears once there is data, so it would mislead.
    // Clamped because Measure() leaves the cache at -1 until the control has a layout; every drawing-time caller
    // runs after that, so the 0 is only ever seen by a pre-layout query.
    private int ChromeTotal() { Measure(); return Math.Max(0, _chromeTotal); }

    // Rows drawn above the first data row (everything except the bottom border).
    //
    // Two sources, deliberately: RowTop() is the exact count measured from the table actually rendered and is what
    // anything positioning against drawn rows must use (the highlight, the scrollbar, click hit-testing). ChromeTop()
    // is the pre-render estimate from Measure()'s probe, used only to decide how many rows to ask for before there
    // is a real table to measure.
    private int ChromeTop() { Measure(); return Math.Max(0, _chromeTop); }

    // Chrome rows above the first data row in the most recently rendered table. Falls back to the probe estimate
    // until the first render has happened.
    private int RowTop() => _renderedChromeTop >= 0 ? _renderedChromeTop : ChromeTop();

    private void Measure()
    {
        // Re-measure when the columns change OR the width changes (an early measure at a transient tiny width would
        // render a degenerate table and cache the wrong chrome).
        if (_chromeTotal >= 0 && _measuredWidth == ContentWidth) return;
        // Never probe before there is a layout. ContentWidth clamps to 1 when ActualWidth is 0, and a multi-column
        // table one cell wide has no width to give any column: every column minimum comes out 0, so Spectre's
        // Ratio.Distribute is asked to share space between ratios summing to zero (Debug.Assert "Sum or ratios must
        // be > 0"). Release builds compile that assert out and carry on, which is why this stayed invisible.
        // Leaving the cache unset means the first real measurement happens once a width exists.
        if (!HasLayout) return;

        var probe = new Table { Border = TableBorder.Rounded, Width = Math.Max(1, ContentWidth), Expand = true };
        foreach (var column in _columns)
            probe.AddColumn(new TableColumn(new Markup(Markup.Escape(column))) { NoWrap = true });
        probe.AddRow(_columns.Select(_ => (IRenderable)new Markup(" ")).ToArray());   // one (no-wrap) data row
        // Count rendered rows by newlines (how the buffer writer advances), not SplitLines. The table emits a
        // trailing newline after its bottom border, so the newline count equals the visible line count:
        // top + header + separator + row + bottom = 5 for this one-row probe.
        var newlines = 0;
        foreach (var s in probe.GetSegments(ansiConsole))
        {
            newlines += s.TextSpan.Count('\n');
        }

        var lines = Math.Max(1, newlines);
        _chromeTotal = Math.Max(0, lines - 1);   // everything that isn't the data row
        _chromeTop = Math.Max(0, lines - 2);     // everything above the data row
        _measuredWidth = ContentWidth;
    }

    // The columns the table draws into: the control width minus the reserved scrollbar column.
    private int ContentWidth => Math.Max(1, ActualWidth - ScrollbarWidth);
    #endregion

    #region Fields
    private const int ScrollbarWidth = 1;
    private static readonly Character ScrollThumb = new('█', new CColor(0x9e, 0x9e, 0x9e), null, ConsoleGUI.Data.Decoration.None);
    private static readonly Character ScrollTrack = new('░', new CColor(0x44, 0x44, 0x44), null, ConsoleGUI.Data.Decoration.None);
    private readonly List<string> _columns;
    private readonly List<string[]> _rows = new();
    private int _selected;
    private int _scroll;
    private bool _scrollDragging;
    private bool _pressedScrollbar;
    private int _scrollGrabOffset;
    private bool _dropNarrowColumns = true;
    private int _chromeTotal = -1;   // -1 = not yet measured (re-measured when columns or width change)
    private int _chromeTop = -1;
    // Chrome rows above the first data row in the table last actually rendered; -1 until the first render. Exact,
    // unlike _chromeTop's probe-based estimate, so everything that positions against drawn rows uses it.
    private int _renderedChromeTop = -1;
    private int _measuredWidth = -1;
    private bool _scrollToSelectionPending;   // selection moved before the control had a size; resolve on first Render
    #endregion
}
