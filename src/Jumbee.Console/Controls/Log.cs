namespace Jumbee.Console;

using System;
using System.Collections.Generic;

using ConsoleGUI.Data;
using ConsoleGUI.Input;
using ConsoleGUI.Space;

using Spectre.Console;
using Spectre.Console.Rendering;

/// <summary>
/// An append-only, scrolling log of Spectre <see cref="IRenderable"/> entries — markup strings, tables, rules,
/// etc. — that always shows the most recent entries that fit (a "tail" view). Each entry is a renderable, so log
/// lines can be styled/coloured. <see cref="Write(string)"/>/<see cref="Write(IRenderable)"/> are safe to call
/// from any thread.
/// </summary>
/// <remarks>
/// The log is <em>viewport-virtualized</em> and owns its own scrolling (like a terminal): each entry is rendered to
/// visual lines <em>once</em>, on write, and only the visible window is blitted each paint — so writing and painting
/// cost O(viewport), not O(total entries). It is not <see cref="IScrollable"/> — it is sized to its framing viewport
/// rather than grown content-tall for the frame to scroll — and draws its own scrollbar in the rightmost column.
/// The mouse wheel scrolls it; when focused, Up/Down/PageUp/PageDown/Home/End do too. Writing while scrolled up keeps
/// the view put (new lines accumulate below); scrolling back to the bottom re-engages tailing.
/// </remarks>
public class Log : Control
{
    #region Properties
    /// <summary>The maximum number of entries retained; older entries are discarded. Defaults to 1000.</summary>
    public int MaxEntries
    {
        get => _maxEntries;
        set => UI.Invoke(() => { _maxEntries = Math.Max(1, value); Trim(); Invalidate(); });
    }
    #endregion

    #region Methods
    /// <summary>Appends a markup string as a styled entry. Safe to call from any thread.</summary>
    public void Write(string markup) => Write(new Markup(markup ?? ""));

    /// <summary>Appends a renderable entry. Safe to call from any thread.</summary>
    public void Write(IRenderable renderable)
    {
        UI.Invoke(() =>
        {
            _entries.Add(renderable);
            // Render the new entry to its visual lines immediately (O(entry), not O(all entries)). If the control
            // isn't laid out yet (width unknown, e.g. seeded in a ctor) defer to the next Render's full rebuild.
            if (ContentWidth > 0 && ContentWidth == _renderedWidth)
                AppendLines(renderable, ContentWidth);
            else
                _needsRebuild = true;
            Trim();
            Invalidate();   // content-only repaint; no Initialize/relayout — the log fills a fixed viewport
        });
    }

    /// <summary>Removes all entries.</summary>
    public void Clear() => UI.Invoke(() =>
    {
        _entries.Clear();
        _lines.Clear();
        _entryLineCounts.Clear();
        _viewTop = 0;
        _follow = true;
        Invalidate();
    });

    /// <summary>Scrolls the view to the newest entry and re-engages tailing.</summary>
    public void ScrollToBottom() => UI.Invoke(() => { if (!_follow) { _follow = true; Invalidate(); } });

    // Deliberately NOT IScrollable: the log manages its own scrolling and is sized to its frame's viewport, which is
    // what keeps writing and painting O(viewport) instead of O(entries). Making it scrollable would grow it
    // content-tall for the frame to window over, and lose that.

    /// <summary>Reports <see langword="true"/> so the log receives mouse-wheel events for scrolling.</summary>
    // Output never depends on focus/hover, so a focus/mouse change shouldn't re-render (there's nothing to re-render
    // here anyway — Render blits pre-built lines — but keep the default focus tint from repainting needlessly).
    protected override bool WantsMouse => true;

    /// <inheritdoc/>
    // Re-render the retained entries whenever the usable width changes (wrapping is width-dependent). Cheap on a
    // steady-state log (only on resize); the per-write path stays incremental.
    protected override void Control_OnInitialization()
    {
        base.Control_OnInitialization();
        if (ContentWidth != _renderedWidth) _needsRebuild = true;
    }

    /// <summary>Blits the visible page of pre-built log lines and the scrollbar into the viewport.</summary>
    protected override void Render()
    {
        int width = ActualWidth, height = ActualHeight;
        if (width <= 0 || height <= 0) return;
        int cw = ContentWidth;

        if (_needsRebuild || cw != _renderedWidth) RebuildLines(cw);

        consoleBuffer.Initialize();   // blank the viewport (lines may be shorter than it / fewer than its rows)
        if (cw <= 0) return;

        int total = _lines.Count;
        int top = ViewTop(total, height);

        for (int row = 0; row < height && top + row < total; row++)
        {
            ansiConsole.SetCursorPosition(0, row);
            ansiConsole.Write(_lines[top + row]);
        }

        DrawScrollBar(width - ScrollbarWidth, height, top, total);
    }

    // The absolute top line to show: pinned to the last page when following, else the clamped saved position.
    private int ViewTop(int total, int height)
    {
        int max = Math.Max(0, total - height);
        return _follow ? max : Math.Clamp(_viewTop, 0, max);
    }

    // Draws the scrollbar thumb/track in the reserved rightmost column, only when there's more content than fits.
    private void DrawScrollBar(int col, int height, int top, int total)
    {
        if (col < 0 || total <= height) return;   // everything fits → no scrollbar

        int thumbSize = Math.Clamp((int)((long)height * height / total), 1, height);
        int maxTop = total - height;
        int thumbStart = maxTop > 0 ? (int)((long)(height - thumbSize) * top / maxTop) : 0;
        for (int y = 0; y < height; y++)
        {
            bool isThumb = y >= thumbStart && y < thumbStart + thumbSize;
            consoleBuffer.Write(new Position(col, y), isThumb ? ScrollThumb : ScrollTrack);
        }
    }

    /// <inheritdoc/>
    protected override void OnMouseWheel(Position position, int delta) => ScrollByLines(delta);

    /// <inheritdoc/>
    protected override void OnInput(InputEvent inputEvent)
    {
        int height = Math.Max(1, ActualHeight);
        switch (inputEvent.Key.Key)
        {
            case ConsoleKey.UpArrow: ScrollByLines(-1); break;
            case ConsoleKey.DownArrow: ScrollByLines(1); break;
            case ConsoleKey.PageUp: ScrollByLines(-(height - 1)); break;
            case ConsoleKey.PageDown: ScrollByLines(height - 1); break;
            case ConsoleKey.Home: ScrollToTop(); break;
            case ConsoleKey.End: ScrollToBottom(); break;
            default: return;
        }
        inputEvent.Handled = true;
    }

    /// <summary>Reports <see langword="true"/> so input routing delivers scroll keys to the log.</summary>
    // Log is a keyboard target only for scrolling; report that so input routing delivers keys to it.
    public override bool HandlesInput => true;

    private void ScrollToTop() => UI.Invoke(() => { _follow = false; _viewTop = 0; Invalidate(); });

    // Scrolls by a signed line count; reaching the bottom re-engages tailing, scrolling up pins the absolute line.
    private void ScrollByLines(int lines) => UI.Invoke(() =>
    {
        int height = Math.Max(1, ActualHeight);
        int total = _lines.Count;
        int max = Math.Max(0, total - height);
        int current = _follow ? max : Math.Clamp(_viewTop, 0, max);
        int next = Math.Clamp(current + lines, 0, max);
        _follow = next >= max;   // back at the bottom → follow again
        _viewTop = next;
        Invalidate();
    });

    // Renders one entry to visual lines at the given width and appends them (with a per-entry count for trimming).
    private void AppendLines(IRenderable entry, int width)
    {
        var lines = RenderEntry(entry, width);
        foreach (var line in lines) _lines.Add(line);
        _entryLineCounts.Add(lines.Count);
    }

    // Re-renders every retained entry at the current width. Runs on a width change (or a deferred first layout),
    // not per write.
    private void RebuildLines(int width)
    {
        _lines.Clear();
        _entryLineCounts.Clear();
        _renderedWidth = width;
        _needsRebuild = false;
        if (width <= 0) return;
        foreach (var entry in _entries) AppendLines(entry, width);
        // A rebuild changed line indices; keep the saved top in range.
        _viewTop = Math.Clamp(_viewTop, 0, Math.Max(0, _lines.Count - Math.Max(1, ActualHeight)));
    }

    private List<SegmentLine> RenderEntry(IRenderable entry, int width)
    {
        var options = new RenderOptions(ansiConsole.Profile.Capabilities, new Spectre.Console.Size(width, 1));
        return Segment.SplitLines(entry.Render(options, width));
    }

    // Discards the oldest entries (and their lines) once past MaxEntries, keeping the saved scroll position aligned
    // with the content that shifted up under it.
    private void Trim()
    {
        while (_entries.Count > _maxEntries)
        {
            _entries.RemoveAt(0);
            if (_entryLineCounts.Count > 0)
            {
                int removed = _entryLineCounts[0];
                _entryLineCounts.RemoveAt(0);
                if (removed > 0 && _lines.Count >= removed) _lines.RemoveRange(0, removed);
                if (!_follow) _viewTop = Math.Max(0, _viewTop - removed);
            }
        }
    }
    #endregion

    #region Fields
    private const int ScrollbarWidth = 1;
    private static readonly Character ScrollThumb = new('█', new Color(0x9e, 0x9e, 0x9e), null, ConsoleGUI.Data.Decoration.None);
    private static readonly Character ScrollTrack = new('░', new Color(0x44, 0x44, 0x44), null, ConsoleGUI.Data.Decoration.None);

    private readonly List<IRenderable> _entries = new();
    // Pre-rendered visual lines (flat) at _renderedWidth, and the line count each entry contributed (for trimming).
    private readonly List<SegmentLine> _lines = new();
    private readonly List<int> _entryLineCounts = new();
    private int _renderedWidth = -1;
    private bool _needsRebuild;
    private int _maxEntries = 1000;
    // Scroll view state: _follow pins the view to the newest line; when false, _viewTop is the absolute top line.
    private bool _follow = true;
    private int _viewTop;

    private int ContentWidth => Math.Max(0, ActualWidth - ScrollbarWidth);
    #endregion
}
