namespace Jumbee.Console;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Spectre.Console;
using Spectre.Console.Rendering;
using Spectre.Console.Interop;
using ConsoleGUI.Input;
using ConsoleGUI.Space;

/// <summary>
/// Displays a flat list of items and allows user input navigation and selection.
/// </summary>
public partial class ListBox : RenderableControl
{
    #region Constructors
    /// <summary>Initializes an empty <see cref="ListBox"/>.</summary>
    public ListBox() => ApplyTheme();

    /// <summary>Initializes a <see cref="ListBox"/> populated with the given renderable items.</summary>
    public ListBox(params IRenderable[] items) : this() => AddItems(items);

    /// <summary>Initializes a <see cref="ListBox"/> populated with the given text items (an array, a
    /// <see cref="List{T}"/>, a LINQ query, or individual strings).</summary>
    public ListBox(params IEnumerable<string> items) : this() => AddItems(items);

    #endregion

    #region Properties
    /// <summary>The items currently in the list.</summary>
    public ICollection<ListBoxItem> Items => _items.Values;

    /// <summary>Foreground of the selected row. Defaults to the theme's <see cref="IStyleTheme.Selection"/>.</summary>
    public Color? SelectedForegroundColor
    {
        get => _selectedForegroundColor;
        set => SetAtomicProperty(ref _selectedForegroundColor, value, themeOverride: true);
    }

    /// <summary>Background of the selected row. Defaults to the theme's <see cref="IStyleTheme.Selection"/>.</summary>
    public Color? SelectedBackgroundColor
    {
        get => _selectedBackgroundColor;
        set => SetAtomicProperty(ref _selectedBackgroundColor, value, themeOverride: true);
    }

    /// <summary>How the selected row is indicated — highlight / underline / caret. Defaults to the theme's
    /// <see cref="IStyleTheme.SelectionStyle"/>.</summary>
    public SelectionStyle SelectionStyle
    {
        get => _selectionStyle;
        set => SetAtomicProperty(ref _selectionStyle, value, themeOverride: true);
    }

    /// <summary>When <see langword="true"/>, the selection style fills the entire row width, not just the item's own
    /// width — so a selected row reads as a full-width bar. Defaults to <see langword="false"/>.</summary>
    /// <remarks>Most visible in <see cref="SelectionStyle.Highlight"/> mode (the selection background spans the
    /// row).</remarks>
    public bool HighlightFullWidth
    {
        get => _highlightFullWidth;
        set => SetAtomicProperty(ref _highlightFullWidth, value);
    }

    /// <summary>The index of the highlighted item (in item order), clamped to the item range.</summary>
    public int SelectedIndex
    {
        get => _selectionIndex;
        set
        {
            var count = _items.Count;
            var clamped = count == 0 ? 0 : Math.Clamp(value, 0, count - 1);
            if (clamped == _selectionIndex) return;
            _selectionIndex = clamped;
            AutoScroll();
            Invalidate();
            SelectionChanged?.Invoke(this, _selectionIndex);
        }
    }

    /// <summary>The highlighted item, or <see langword="null"/> when empty.</summary>
    public ListBoxItem? SelectedItem
    {
        get
        {
            var ordered = OrderedItems();
            return _selectionIndex >= 0 && _selectionIndex < ordered.Length ? ordered[_selectionIndex] : null;
        }
    }

    /// <summary>An optional menu shown when a row is right-clicked. Left <see langword="null"/> = no menu.</summary>
    /// <remarks>The right-click first selects that row and raises <see cref="ContextMenuOpening"/> (with the item),
    /// then the menu floats at the pointer. Item handlers can read <see cref="SelectedItem"/> to act on the
    /// right-clicked row.</remarks>
    public ContextMenu? ContextMenu { get; set; }

    /// <summary>Always <see langword="true"/>: the list handles its own selection keys. No opt-in needed — unlike
    /// the base default, this is on.</summary>
    public override bool HandlesInput => true;
    #endregion

    #region Events
    /// <summary>Raised when the highlighted index changes (navigation or selection).</summary>
    public event EventHandler<int>? SelectionChanged;
    /// <summary>Raised when an item is committed (Enter or click).</summary>
    public event EventHandler<ListBoxItem>? Committed;
    /// <summary>Raised when the list is cancelled (Escape).</summary>
    public event EventHandler? Cancelled;
    /// <summary>Raised just before <see cref="ContextMenu"/> is shown for a right-clicked row, with that item (now
    /// the selected one).</summary>
    /// <remarks>Use it to tailor the menu, or read <see cref="SelectedItem"/> from the menu's own item handlers.
    /// Only fires when <see cref="ContextMenu"/> is set.</remarks>
    public event EventHandler<ListBoxItem>? ContextMenuOpening;
    #endregion

    #region Methods        
    // Item creation (and the atomic index) happens on the calling thread so AddItem can return immediately;
    // the dictionary mutation is marshaled to the UI thread (inline when already there) so reads during
    // rendering never see a concurrent write.
    /// <summary>Appends the given renderable items to the list.</summary>
    public void AddItems(params IEnumerable<IRenderable> items)
    {
        var added = items.Select(i => new ListBoxItem(this, Interlocked.Increment(ref _itemIndex), i)).ToArray();
        UI.Invoke(() =>
        {
            foreach (var item in added) _items[item.Index] = item;
            InvalidateLayout();   // re-measure: item set changed, so heights may differ
        });
    }

    /// <summary>Appends the given text items to the list.</summary>
    public void AddItems(params IEnumerable<string> items)
    {
        var added = items.Select(s => new ListBoxItem(this, Interlocked.Increment(ref _itemIndex), s)).ToArray();
        UI.Invoke(() =>
        {
            foreach (var item in added) _items[item.Index] = item;
            InvalidateLayout();   // re-measure: item set changed, so heights may differ
        });
    }

    /// <summary>Appends the given text items with per-item foreground/background colours to the list.</summary>
    public void AddItems(params (string text, Color? fgColor, Color? bgColor)[] items)
    {
        var added = items.Select(t => new ListBoxItem(this, Interlocked.Increment(ref _itemIndex), t.text, t.fgColor, t.bgColor)).ToArray();
        UI.Invoke(() =>
        {
            foreach (var item in added) _items[item.Index] = item;
            InvalidateLayout();   // re-measure: item set changed, so heights may differ
        });
    }

    /// <summary>Appends a single renderable item and returns it.</summary>
    public ListBoxItem AddItem(IRenderable item)
    {
        var listItem = new ListBoxItem(this, Interlocked.Increment(ref _itemIndex), item);
        UI.Invoke(() =>
        {
            _items[listItem.Index] = listItem;
            InvalidateLayout();
        });
        return listItem;
    }

    /// <summary>Appends a single text item with optional foreground/background colours and returns it.</summary>
    public ListBoxItem AddItem(string text, Color? foreground = null, Color? background = null)
    {
        var item = new ListBoxItem(this, Interlocked.Increment(ref _itemIndex), text, foreground, background);
        UI.Invoke(() =>
        {
            _items[item.Index] = item;
            InvalidateLayout();
        });
        return item;
    }

    /// <summary>Removes an item.</summary>
    /// <remarks>
    /// The result is reliable only when called on the UI thread; off-thread callers should not rely on it (the
    /// removal is marshaled and applied on the next pump).
    /// </remarks>
    public bool RemoveItem(ListBoxItem item)
    {
        var removed = false;
        UI.Invoke(() =>
        {
            if (_items.Remove(item.Index, out var r))
            {
                r.Detach();
                removed = true;
                InvalidateLayout();
            }
        });
        return removed;
    }

    /// <summary>Removes all items from the list.</summary>
    public void Clear()
    {
        UI.Invoke(() =>
        {
            foreach (var item in _items.Values) item.Detach();
            _items.Clear();
            InvalidateLayout();
        });
    }

    /// <summary>Requests a redraw of the list.</summary>
    public void Update() => Invalidate();

    /// <inheritdoc/>
    // The selected row is the focus indicator, so don't also tint the whole control when it's focused (a borderless
    // list would otherwise light up entirely on focus). A framed list already shows focus through its border colour.
    protected override bool RendersOwnFocus => true;

    /// <inheritdoc/>
    // Default the selected-row colours from the theme so a bare ListBox shows a visible selection (re-applied on a
    // runtime theme switch; explicit SelectedForeground/BackgroundColor overrides are left alone).
    protected override void ApplyTheme()
    {
        if (!IsThemeOverridden(nameof(SelectedForegroundColor))) _selectedForegroundColor = UI.StyleTheme.Selection.ForegroundColor;
        if (!IsThemeOverridden(nameof(SelectedBackgroundColor))) _selectedBackgroundColor = UI.StyleTheme.Selection.BackgroundColor;
        if (!IsThemeOverridden(nameof(SelectionStyle))) _selectionStyle = UI.StyleTheme.SelectionStyle;
        _selectionCaret = UI.GlyphTheme.SelectionCaret;
    }

    // Items may be multi-line (an IRenderable carrying newlines), so our content height is the sum of the item
    // heights — not the item count. Reporting it lets a surrounding ControlFrame size us to our content and scroll
    // accurately, instead of filling to the 1000-row clamp.
    //
    // Item height is measured (and items are rendered — see Render) at a fixed wide width, so a row is the item's
    // EXPLICIT line count and is INDEPENDENT of the control's width: long lines are clipped to the viewport, not
    // wrapped. This is deliberate — a width-dependent height feeds the layout's content-height↔width convergence in a
    // scrolling frame and can fail to settle (an infinite layout loop); a width-independent height converges exactly
    // as the single-line case always did.
    /// <inheritdoc/>
    protected override int MeasureHeight(int width) => Math.Max(1, EnsureLayout()[^1]);

    // Per-item vertical layout: cumulative row offsets, length = itemCount + 1, so _offsets[k] is the first row of
    // item k and _offsets[itemCount] is the total content height. Recomputed when the item set/content changes
    // (tracked by _itemsVersion). Maps between item indices and rows for scrolling and click hit-testing, since a row
    // is no longer an item index once items span multiple lines.
    private int[] EnsureLayout()
    {
        if (_layoutVersion == _itemsVersion && _offsets.Length == _items.Count + 1)
            return _offsets;

        var items = OrderedItems();
        var options = new RenderOptions(ansiConsole.Profile.Capabilities, new Spectre.Console.Size(LayoutWidth, 1));
        var gutter = _selectionStyle == SelectionStyle.Caret ? _selectionCaret.GetCellWidth() : 0;
        var contentWidth = Math.Max(1, LayoutWidth - gutter);

        var offsets = new int[items.Length + 1];
        var y = 0;
        for (var i = 0; i < items.Length; i++)
        {
            offsets[i] = y;
            y += Math.Max(1, Segment.SplitLines(items[i].Content.Render(options, contentWidth)).Count);
        }
        offsets[items.Length] = y;

        _offsets = offsets;
        _layoutVersion = _itemsVersion;
        return _offsets;
    }

    // Bump the layout version (and re-measure) after a change that can alter item heights — an add/remove or an
    // item's content changing. A height change must re-lay-out (Initialize), not merely Invalidate, so a surrounding
    // frame re-measures our content height (see Control.MeasureHeight).
    internal void InvalidateLayout() => UI.Invoke(() => { _itemsVersion++; Initialize(); });

    // The item whose rows contain content-row <paramref name="row"/>, or -1 if past the last item.
    private int ItemIndexAtRow(int row)
    {
        if (row < 0) return -1;
        var offsets = EnsureLayout();
        for (var i = 0; i + 1 < offsets.Length; i++)
            if (row >= offsets[i] && row < offsets[i + 1]) return i;
        return -1;
    }

    /// <inheritdoc/>
    protected internal override HelpInfo? GetHelpInfo() => new HelpInfo("List", "List", "A scrollable list of items; the selected item is highlighted.")
        .WithKey("↑ / ↓", "Move the selection")
        .WithKey("Home / End", "First / last item")
        .WithKey("PgUp / PgDn", "Move by a page")
        .WithKey("Enter", "Choose the selected item")
        .WithKey("Click", "Select an item");

    /// <inheritdoc/>
    protected override void OnInput(InputEvent inputEvent)
    {
        var count = _items.Count;
        switch (inputEvent.Key.Key)
        {
            case ConsoleKey.UpArrow when count > 0:
                SelectedIndex = (_selectionIndex - 1 + count) % count;
                inputEvent.Handled = true;
                break;
            case ConsoleKey.DownArrow when count > 0:
                SelectedIndex = (_selectionIndex + 1) % count;
                inputEvent.Handled = true;
                break;
            case ConsoleKey.Home when count > 0:
                SelectedIndex = 0;
                inputEvent.Handled = true;
                break;
            case ConsoleKey.End when count > 0:
                SelectedIndex = count - 1;
                inputEvent.Handled = true;
                break;
            case ConsoleKey.PageUp when count > 0:
                SelectedIndex = _selectionIndex - Page();
                inputEvent.Handled = true;
                break;
            case ConsoleKey.PageDown when count > 0:
                SelectedIndex = _selectionIndex + Page();
                inputEvent.Handled = true;
                break;
            case ConsoleKey.Enter when count > 0:
                Commit();
                inputEvent.Handled = true;
                break;
            case ConsoleKey.Escape:
                Cancelled?.Invoke(this, EventArgs.Empty);
                inputEvent.Handled = true;
                break;
        }
    }

    // A page for PageUp/PageDown, in items: roughly how many average-height items fit in the viewport, so paging
    // advances by a viewport regardless of item height. SelectedIndex clamps.
    private int Page()
    {
        var viewport = Math.Max(1, Frame?.ViewportSize.Height ?? 10);
        var count = _items.Count;
        var total = EnsureLayout()[^1];
        if (count == 0 || total <= 0) return 1;
        var avgHeight = (double)total / count;
        return Math.Max(1, (int)(viewport / avgHeight));
    }

    // A left click selects the item under the pointer and commits it; a right click selects it and opens the context
    // menu instead. The listener position is in content coordinates; an item may span several rows, so map the row.
    /// <inheritdoc/>
    protected override void OnClick(Position position)
    {
        if (UI.MouseButton == TerminalMouseButton.Right) { OpenContextMenu(position.Y); return; }
        var index = ItemIndexAtRow(position.Y);
        if (index < 0) return;
        SelectedIndex = index;
        Commit();
    }

    // A fast double right-click still just (re)opens the menu rather than committing underneath it.
    /// <inheritdoc/>
    protected override void OnDoubleClick(Position position)
    {
        if (UI.MouseButton == TerminalMouseButton.Right) OpenContextMenu(position.Y);
    }

    private void Commit()
    {
        if (SelectedItem is { } item) Committed?.Invoke(this, item);
    }

    // Right-click: select the row, announce it, then float ContextMenu at the pointer (in the ambient UI.Overlay).
    // Item handlers can read SelectedItem. No-op if no menu is set or the click missed every row.
    private void OpenContextMenu(int row)
    {
        var index = ItemIndexAtRow(row);
        if (ContextMenu is null || index < 0) return;
        SelectedIndex = index;
        if (SelectedItem is { } item) ContextMenuOpening?.Invoke(this, item);
        if (ConsoleGUI.ConsoleManager.MousePosition is { } m) ContextMenu.Show(m.X, m.Y);
        else ContextMenu.Show(0, row);
    }

    private ListBoxItem[] OrderedItems() => _items.Values.OrderBy(i => i.Index).ToArray();

    /// <summary>
    /// Scrolls the containing <see cref="ControlFrame"/> (if any) so the selected item stays within the viewport.
    /// Items may span several rows, so the selected item's row range is [offset, offset + height).
    /// </summary>
    private void AutoScroll()
    {
        if (Frame == null) return;

        var offsets = EnsureLayout();
        if (_selectionIndex < 0 || _selectionIndex + 1 >= offsets.Length) return;
        var itemTop = offsets[_selectionIndex];
        var itemBottom = offsets[_selectionIndex + 1];   // exclusive

        var top = Frame.Top;
        var viewportHeight = Frame.ViewportSize.Height;
        if (viewportHeight <= 0) return;

        if (itemTop < top)
            Frame.Top = itemTop;                                 // scrolled above: bring its top into view
        else if (itemBottom > top + viewportHeight)
            Frame.Top = Math.Min(itemTop, itemBottom - viewportHeight);   // below: reveal its bottom (top wins if taller than the viewport)
    }

    /// <inheritdoc/>
    protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        var items = OrderedItems();

        // Ensure selection index is valid
        if (_selectionIndex >= items.Length) _selectionIndex = Math.Max(0, items.Length - 1);
        if (_selectionIndex < 0 && items.Length > 0) _selectionIndex = 0;

        var renderables = new IRenderable[items.Length];

        // In Caret mode reserve a caret-width gutter on every row (the selected row shows the caret, the rest a
        // blank) so the item text stays put as the selection moves instead of jumping.
        var caret = _selectionStyle == SelectionStyle.Caret;
        var gutter = caret ? new string(' ', _selectionCaret.GetCellWidth()) : "";
        var selStyle = _selectionStyle.TextStyle(_selectedForegroundColor, _selectedBackgroundColor);

        for (int i = 0; i < items.Length; i++)
        {
            var item = items[i];
            var selected = i == _selectionIndex;
            if (item.Text != null)
            {
                if (selected)
                {
                    // Indicate the selected row per SelectionStyle: a highlight, an underline, or a caret prefix.
                    renderables[i] = new Markup(_selectionStyle.Prefix(_selectionCaret) + item.Text, selStyle);
                }
                else if (caret)
                {
                    // Blank gutter, keeping the item's own colours, so unselected rows line up with the selected one.
                    renderables[i] = new Markup(gutter + item.Text, new Spectre.Console.Style(item.ForegroundColor, item.BackgroundColor));
                }
                else
                {
                    renderables[i] = item.Content;
                }
            }
            else
            {
                // An IRenderable item can't be folded into markup, so a selected one is highlighted by RESTYLING its
                // rendered segments — the selection style is overlaid on each, keeping the segment's own colours so a
                // colourful label stays colourful under the highlight (the same approach Tree uses for IRenderable
                // nodes). The caret gutter (Caret mode) is reserved on every row so labels stay aligned as the
                // selection moves.
                var overlay = selected ? selStyle : (Spectre.Console.Style?)null;
                renderables[i] = new HighlightedRenderable(item.Content, overlay, caret ? _selectionCaret : null, selected);
            }

            // Extend the selection across the whole row: pad the selected row to the full width in the selection
            // style so it reads as a full-width bar rather than stopping at the item's own width.
            if (selected && _highlightFullWidth)
                renderables[i] = new FullWidthRow(renderables[i], selStyle);
        }

        // Render at the fixed wide layout width (not maxWidth) so items are laid out by their explicit lines and don't
        // wrap — matching EnsureLayout's height measurement. Content wider than the control is clipped when written to
        // the (viewport-width) buffer. The full-width highlight likewise pads to this width and clips to the viewport.
        var rows = new Rows(renderables);
        return ((IRenderable)rows).Render(options, LayoutWidth);
    }
    #endregion

    #region Types
    /// <summary>
    /// Wraps a <see cref="ListBoxItem"/>'s <see cref="IRenderable"/> content so a selected row is highlighted the way
    /// <see cref="Tree"/> highlights a selected <see cref="IRenderable"/> node: the selection style is overlaid on
    /// each rendered segment — keeping the segment's own foreground/background where it set one, so a colourful label
    /// stays colourful under the highlight. In <see cref="SelectionStyle.Caret"/> mode a caret glyph (selected) or a
    /// blank spacer (others) is reserved on the left so labels stay aligned as the selection moves.
    /// </summary>
    /// <param name="inner">The item's content.</param>
    /// <param name="overlay">The selection style to overlay when the row is selected; <see langword="null"/> otherwise.</param>
    /// <param name="caret">The caret glyph to reserve a gutter for in Caret mode; <see langword="null"/> in other modes.</param>
    /// <param name="selected">Whether this row is the selected one.</param>
    private sealed class HighlightedRenderable(IRenderable inner, Spectre.Console.Style? overlay, string? caret, bool selected) : IRenderable
    {
        private int GutterWidth => caret?.GetCellWidth() ?? 0;

        public Measurement Measure(RenderOptions options, int maxWidth)
        {
            var g = GutterWidth;
            var m = inner.Measure(options, Math.Max(0, maxWidth - g));
            return new Measurement(m.Min + g, m.Max + g);
        }

        public IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
        {
            var g = GutterWidth;
            var lines = Segment.SplitLines(inner.Render(options, Math.Max(0, maxWidth - g)));
            var result = new List<Segment>();
            for (var i = 0; i < lines.Count; i++)
            {
                if (g > 0)
                {
                    // Caret glyph on the selected row's first line (in the selection style), else a blank spacer.
                    result.Add(selected && i == 0
                        ? new Segment(caret!, overlay ?? Spectre.Console.Style.Plain)
                        : new Segment(new string(' ', g)));
                }

                // Overlay the selection style on a selected row's segments (each keeps its own colours where set).
                if (overlay is { } o)
                    foreach (var seg in lines[i]) result.Add(seg.WithStyle(o.Combine(seg.Style)));
                else
                    result.AddRange(lines[i]);

                result.Add(Segment.LineBreak);
            }
            return result;
        }
    }

    /// <summary>
    /// Pads each rendered line of the wrapped row out to the full viewport width with a fill style, so a selected
    /// row's selection style spans the whole ListBox width instead of stopping at the item's own width. Opt in via
    /// <see cref="HighlightFullWidth"/>.
    /// </summary>
    /// <param name="inner">The already-styled selected row.</param>
    /// <param name="fill">The style (the selection style) painted over the trailing padding.</param>
    private sealed class FullWidthRow(IRenderable inner, Spectre.Console.Style fill) : IRenderable
    {
        public Measurement Measure(RenderOptions options, int maxWidth) => new(maxWidth, maxWidth);

        public IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
        {
            var lines = Segment.SplitLines(inner.Render(options, maxWidth));
            var result = new List<Segment>();
            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                result.AddRange(line);
                var pad = maxWidth - Segment.CellCount(line);
                if (pad > 0) result.Add(new Segment(new string(' ', pad), fill));
                result.Add(Segment.LineBreak);
            }
            return result;
        }
    }
    #endregion

    #region Fields
    private readonly Dictionary<int, ListBoxItem> _items = new();
    private int _itemIndex = 0;
    private int _selectionIndex = 0;
    private Color? _selectedBackgroundColor;
    private Color? _selectedForegroundColor;
    private SelectionStyle _selectionStyle;
    private string _selectionCaret = "";
    private bool _highlightFullWidth;
    // Cached per-item row layout (see EnsureLayout): cumulative row offsets, recomputed when the item set/content
    // (_itemsVersion) changes. Item heights are width-independent, so the layout doesn't depend on the control width.
    private int[] _offsets = [0];
    private int _layoutVersion = -1;
    private int _itemsVersion;
    // The fixed wide width items are measured and rendered at, so heights are width-independent (see MeasureHeight).
    // Matches Control.CalculateSize's 1000-cell clamp — the widest a control is ever laid out — so nothing visible is
    // ever lost to the cap; wider content simply clips to the viewport.
    private const int LayoutWidth = 1000;
    #endregion
}
