namespace Jumbee.Console;

using System;
using System.Collections.Generic;

/// <summary>Which edge a <see cref="TabPanel"/> docks its tab bar on.</summary>
public enum TabBarDock
{
    /// <summary>Tab bar along the top edge.</summary>
    Top,
    /// <summary>Tab bar along the left edge.</summary>
    Left,
    /// <summary>Tab bar along the right edge.</summary>
    Right,
    /// <summary>Tab bar along the bottom edge.</summary>
    Bottom
}

/// <summary>
/// A tabbed container: a bar of selectable <see cref="TabHeader"/> labels docked on one edge, with the selected
/// tab's content filling the rest.
/// </summary>
/// <remarks>
/// Select a tab by clicking its label, by the arrow keys while the bar is focused (Left/Right for a top/bottom bar,
/// Up/Down for a left/right bar), or programmatically via <see cref="SelectedIndex"/>. Tabs can be added, removed,
/// hidden, disabled, and relabelled at runtime — via <see cref="AddTab"/> / <see cref="RemoveTab(TabItem)"/> and the
/// returned <see cref="TabItem"/> handle (whose identity is stable across structural changes, unlike an index).
/// </remarks>
public class TabPanel : Layout<TabPanelDockPanel>
{
    #region Constructors
    /// <summary>Initializes a new <see cref="TabPanel"/> with its bar docked at <paramref name="tabBarDock"/> and the given <paramref name="tabs"/> (first selectable tab auto-selects).</summary>
    public TabPanel(TabBarDock tabBarDock, params (string Name, IFocusable Content)[] tabs)
        : base(new TabPanelDockPanel(tabBarDock, BarThickness(tabBarDock, tabs)))
    {
        _horizontal = control.IsHorizontalTabBar;
        _selectionStyle = UI.StyleTheme.SelectionStyle;
        foreach (var (name, content) in tabs) AddTabCore(name, content, -1);   // first selectable auto-selects
    }

    // The bar's cross-axis size, from the label texts (see TabPanelDockPanel): one row tall for a top/bottom bar,
    // or as wide as the widest label for a left/right bar. The +2 matches TabHeader's one-space-each-side padding.
    private static int BarThickness(TabBarDock dock, (string Name, IFocusable Content)[] tabs)
    {
        if (dock is TabBarDock.Top or TabBarDock.Bottom) return 1;
        var width = 1;
        foreach (var (name, _) in tabs) width = Math.Max(width, name.Length + 2);
        return width;
    }
    #endregion

    #region Events
    /// <summary>Raised after the selected tab changes, with the new index (-1 when no tab is selectable).</summary>
    public event EventHandler<int>? SelectionChanged;

    /// <summary>Raised when a closable tab's ✕ is clicked (see <see cref="ClosableTabs"/>).</summary>
    /// <remarks>Set <see cref="TabCloseEventArgs.Cancel"/> to keep the tab (e.g. after prompting about unsaved
    /// changes); otherwise the panel removes it.</remarks>
    public event EventHandler<TabCloseEventArgs>? TabCloseRequested;

    /// <summary>Raised when the "+" new-tab button is clicked (see <see cref="ShowAddButton"/>). The handler
    /// typically opens a new tab.</summary>
    public event EventHandler? NewTabRequested;

    /// <summary>Raised after a tab has been removed (via ✕, <see cref="RemoveTab(TabItem)"/>, etc.), with its handle.</summary>
    public event EventHandler<TabItem>? TabRemoved;
    #endregion

    #region Properties
    /// <summary>The number of tabs (including hidden and disabled ones).</summary>
    public int TabCount => _tabs.Count;

    /// <summary>The tab handles, in order (including hidden and disabled tabs).</summary>
    public IReadOnlyList<TabItem> Tabs => _tabs;

    /// <summary>The tab header labels, in order (for inspection or per-tab styling).</summary>
    public IReadOnlyList<TabHeader> Headers => _tabs.ConvertAll(t => t.Header);

    /// <summary>The selected tab handle, or <see langword="null"/> when no tab is selectable.</summary>
    public TabItem? ActiveTab => _selected;

    /// <summary>How the active tab is indicated — highlight / underline / caret.</summary>
    /// <remarks>Defaults to the theme's <see cref="IStyleTheme.SelectionStyle"/>; setting it applies to every tab
    /// header (and future ones).</remarks>
    public SelectionStyle SelectionStyle
    {
        get => _selectionStyle;
        set { _selectionStyle = value; foreach (var t in _tabs) t.Header.SelectionStyle = value; }
    }

    /// <summary>When <see langword="true"/>, every tab shows a clickable close (✕) glyph (on the active/hovered
    /// tab) and clicking it raises the cancelable <see cref="TabCloseRequested"/>. Default <see langword="false"/>.</summary>
    /// <remarks>Applies to existing and future tabs.</remarks>
    public bool ClosableTabs
    {
        get => _closableTabs;
        set { _closableTabs = value; foreach (var t in _tabs) t.Header.Closable = value; }
    }

    /// <summary>When <see langword="true"/>, a "+" button is shown at the end of the bar; clicking it raises
    /// <see cref="NewTabRequested"/>. Default <see langword="false"/>.</summary>
    /// <remarks>The button is mouse-only (not part of keyboard tab traversal).</remarks>
    public bool ShowAddButton
    {
        get => _showAddButton;
        set { if (_showAddButton == value) return; _showAddButton = value; UI.Invoke(RebuildBar); }
    }

    /// <summary>The "+" button instance once <see cref="ShowAddButton"/> has built it, else <see langword="null"/>.
    /// Exposed for testing (click it to exercise <see cref="NewTabRequested"/>).</summary>
    internal TabAddButton? AddButton => _addButton;

    /// <summary>The zero-based selected tab, or -1 when no tab is selected.</summary>
    /// <remarks>Setting it activates that tab (clamped to range) if it is selectable (not hidden or disabled); raises
    /// <see cref="SelectionChanged"/> when it actually changes.</remarks>
    public int SelectedIndex
    {
        get => _selected is null ? -1 : _tabs.IndexOf(_selected);
        set
        {
            if (_tabs.Count == 0) return;
            var item = _tabs[Math.Clamp(value, 0, _tabs.Count - 1)];
            if (!IsSelectable(item)) return;   // can't select a hidden or disabled tab

            // If focus is already inside the panel, follow the selection into the new tab so its content is usable
            // straight away; if focus is elsewhere, don't steal it on a (possibly programmatic) selection change.
            SelectItemCore(item, followFocus: FocusedControl is not null);
        }
    }

    /// <summary>The selected tab's content, or <see langword="null"/> when no tab is selected.</summary>
    public IFocusable? ActiveContent => _selected?.Content;

    /// <summary>The selected tab's name, or <see langword="null"/> when no tab is selected.</summary>
    public string? ActiveTabName => _selected?.Name;

    // Logical children, flattened so input routing reaches them whether this panel is the root layout (its own
    // OnInput walks Controls) or nested in another layout (the parent routes through FocusedControl below): the
    // selectable (visible, enabled) tab headers, then the active tab's content. The visual arrangement is handled
    // separately by the wrapped DockPanel.
    /// <summary>Number of logical rows for input routing: the selectable tab headers plus one for the active content.</summary>
    public override int Rows
    {
        get
        {
            var n = 0;
            foreach (var t in _tabs) if (IsSelectable(t)) n++;
            return n + (_selected is not null ? 1 : 0);
        }
    }

    /// <summary>Number of columns in the layout grid (always 1).</summary>
    public override int Columns => 1;

    /// <summary>Gets the logical child at <paramref name="row"/>: a selectable tab header, or the active content in the last row.</summary>
    public override IFocusable this[int row, int column]
    {
        get
        {
            if (column != 0) throw new ArgumentOutOfRangeException(nameof(column));
            if (row < 0 || row >= Rows) throw new ArgumentOutOfRangeException(nameof(row));
            var i = 0;
            foreach (var t in _tabs)
            {
                if (!IsSelectable(t)) continue;
                if (i == row) return t.Header;
                i++;
            }

            return _selected!.Content;   // row == number of headers -> the active content
        }
    }

    // Return the focused descendant — a focused tab header, else whatever is focused inside the active content — so
    // a parent layout routing input through this single IFocusable reaches it (the CompositeControl pattern).
    /// <inheritdoc/>
    public override IFocusable? FocusedControl
    {
        get
        {
            foreach (var t in _tabs)
                if (t.Header.FocusedControl is { } focusedHeader) return focusedHeader;
            if (_addButton?.FocusedControl is { } addBtn) return addBtn;
            return _selected?.Content.FocusedControl;
        }
    }
    #endregion

    #region Methods
    /// <summary>Selects the tab at <paramref name="index"/> (clamped). Equivalent to setting <see cref="SelectedIndex"/>.</summary>
    public void SelectTab(int index) => SelectedIndex = index;

    /// <summary>Selects the given tab if it is selectable.</summary>
    public void SelectTab(TabItem tab)
    {
        if (_tabs.Contains(tab) && IsSelectable(tab)) SelectItemCore(tab, followFocus: FocusedControl is not null);
    }

    /// <summary>Adds a tab, optionally at <paramref name="index"/> (default appends). Returns its handle.</summary>
    /// <remarks>If nothing is selected yet and the new tab is selectable, it becomes selected.</remarks>
    public TabItem AddTab(string name, IFocusable content, int index = -1)
    {
        TabItem item = null!;
        UI.Invoke(() => item = AddTabCore(name, content, index));
        return item;
    }

    /// <summary>Removes the tab. If it was selected, selection moves to the nearest selectable tab (or clears).</summary>
    public void RemoveTab(TabItem tab) => UI.Invoke(() => RemoveTabCore(tab));

    /// <summary>Removes the tab at <paramref name="index"/>.</summary>
    public void RemoveTab(int index)
    {
        if (index >= 0 && index < _tabs.Count) RemoveTab(_tabs[index]);
    }

    // Layout-tier navigation (the "Alt tier"): Alt+Left/Right on a top/bottom bar — or Alt+Up/Down on a left/right
    // bar — switch tabs. Handled here (the tunnel) rather than on the header so it works from anywhere inside the
    // panel, including while the active tab's content is focused. The base Layout.OnInput calls this for the panel
    // whenever it is on the focus path (even nested), and marks the key handled when we return true.
    /// <summary>Intercepts Alt+arrow tab switching, "+"-button keys, and header arrow navigation before input routes to the focused control.</summary>
    protected override bool InterceptInput(UI.InputEventArgs inputEventArgs)
    {
        if (inputEventArgs.InputEvent is not { } e) return false;

        // Alt+arrows switch tabs from anywhere in the panel — even while the active tab's content is focused.
        var altDelta = _horizontal
            ? (e.Key == UI.HotKeys.AltLeft ? -1 : e.Key == UI.HotKeys.AltRight ? +1 : 0)
            : (e.Key == UI.HotKeys.AltUp ? -1 : e.Key == UI.HotKeys.AltDown ? +1 : 0);
        if (altDelta != 0) { MoveSelection(altDelta, followFocus: true); return true; }

        // The "+" button isn't in the panel's logical rows, so Layout routing never dispatches keys to it — the
        // tunnel drives it while it's focused: Enter/Space activates, and the "back" arrow returns to the last tab.
        if (_showAddButton && _addButton is { IsFocused: true } add)
        {
            if (e.Key.Key is ConsoleKey.Enter or ConsoleKey.Spacebar) { add.Activate(); return true; }
            var back = _horizontal ? ConsoleKey.LeftArrow : ConsoleKey.UpArrow;
            if (e.Key.Key == back)
            {
                var last = LastSelectableIndex();
                if (last >= 0) UI.SetFocus(_tabs[last].Header);
                return true;
            }
            return false;
        }

        // Plain arrows move between tabs while a header has focus (the standard tab-strip behaviour), keeping focus on
        // the header so the user can keep arrowing. Arrowing forward past the last tab lands on the "+" button (when
        // shown). When the content is focused instead, plain arrows belong to it.
        if (AnyHeaderFocused())
        {
            var delta = _horizontal
                ? (e.Key.Key == ConsoleKey.LeftArrow ? -1 : e.Key.Key == ConsoleKey.RightArrow ? +1 : 0)
                : (e.Key.Key == ConsoleKey.UpArrow ? -1 : e.Key.Key == ConsoleKey.DownArrow ? +1 : 0);
            if (delta != 0)
            {
                var from = _selected is null ? -1 : _tabs.IndexOf(_selected);
                var target = NextSelectableIndex(from, delta);
                if (target >= 0) { SelectItemCore(_tabs[target], followFocus: false); UI.SetFocus(_tabs[target].Header); }
                else if (delta > 0 && _showAddButton && _addButton is not null) UI.SetFocus(_addButton);
                return true;
            }
        }
        return false;
    }

    private bool AnyHeaderFocused()
    {
        foreach (var t in _tabs) if (t.Header.IsFocused) return true;
        return false;
    }

    private static bool IsSelectable(TabItem t) => !t.IsHidden && !t.IsDisabled;

    // First selectable tab reached by stepping from `from` in direction `dir` (+1/-1); -1 if none before the edge.
    private int NextSelectableIndex(int from, int dir)
    {
        for (var i = from + dir; i >= 0 && i < _tabs.Count; i += dir)
            if (IsSelectable(_tabs[i])) return i;
        return -1;
    }

    // Index of the last selectable tab, or -1 if none.
    private int LastSelectableIndex()
    {
        for (var i = _tabs.Count - 1; i >= 0; i--) if (IsSelectable(_tabs[i])) return i;
        return -1;
    }

    // The selectable tab nearest to `around` (prefers forward), or null if none exists.
    private TabItem? NearestSelectable(int around)
    {
        for (var d = 0; d < _tabs.Count; d++)
        {
            var f = around + d;
            if (f < _tabs.Count && IsSelectable(_tabs[f])) return _tabs[f];
            if (d > 0)
            {
                var b = around - d;
                if (b >= 0 && IsSelectable(_tabs[b])) return _tabs[b];
            }
        }
        return null;
    }

    private TabItem AddTabCore(string name, IFocusable content, int index)
    {
        var header = new TabHeader(_tabs.Count, name) { SelectionStyle = _selectionStyle, Closable = _closableTabs };
        var item = new TabItem(this, name, content, header);
        header.Activated += (_, _) => { if (IsSelectable(item)) SelectItemCore(item, followFocus: FocusedControl is not null); };
        header.CloseRequested += (_, _) => RequestClose(item);

        var at = index < 0 || index > _tabs.Count ? _tabs.Count : index;
        _tabs.Insert(at, item);
        RebuildBar();
        if (_selected is null && IsSelectable(item)) SelectItemCore(item, followFocus: false);
        return item;
    }

    // A ✕ click asks to close: raise the cancelable event, then remove unless a handler canceled. Runs on the UI
    // thread (mouse dispatch), so it calls the core directly like the other internal mutators.
    private void RequestClose(TabItem item)
    {
        if (!_tabs.Contains(item)) return;
        var args = new TabCloseEventArgs(item);
        TabCloseRequested?.Invoke(this, args);
        if (!args.Cancel) RemoveTabCore(item);
    }

    private void RemoveTabCore(TabItem item)
    {
        var idx = _tabs.IndexOf(item);
        if (idx < 0) return;
        var wasSelected = ReferenceEquals(item, _selected);
        _tabs.RemoveAt(idx);
        RebuildBar();
        if (wasSelected)
        {
            // _selected still points at the removed item; SelectItemCore transitions off it (or clears).
            var next = _tabs.Count == 0 ? null : NearestSelectable(Math.Clamp(idx, 0, _tabs.Count - 1));
            SelectItemCore(next, followFocus: false);
        }
        TabRemoved?.Invoke(this, item);
    }

    // Per-tab closable override (from TabItem.Closable): flip the header's close slot and reflow the bar for the
    // changed header width.
    internal void SetTabClosable(TabItem item, bool value) => UI.Invoke(() =>
    {
        item.Header.Closable = value;
        RebuildBar();
    });

    internal void RelabelTab(TabItem item)
    {
        UI.Invoke(() =>
        {
            item.Header.Text = item.Name;
            RebuildBar();   // reflow the bar (and a vertical bar's width) for the new label length
        });
    }

    internal void OnTabVisibilityChanged(TabItem item)
    {
        UI.Invoke(() =>
        {
            RebuildBar();   // the bar shows only non-hidden headers
            if (item.IsHidden)
            {
                if (ReferenceEquals(item, _selected))
                    SelectItemCore(NearestSelectable(_tabs.IndexOf(item)), followFocus: false);
            }
            else if (_selected is null && IsSelectable(item))
            {
                SelectItemCore(item, followFocus: false);   // un-hiding into an empty panel selects it
            }
        });
    }

    internal void OnTabEnabledChanged(TabItem item)
    {
        UI.Invoke(() =>
        {
            item.Header.IsEnabled = !item.IsDisabled;   // dims the header and drops it from focus traversal
            if (item.IsDisabled && ReferenceEquals(item, _selected))
                SelectItemCore(NearestSelectable(_tabs.IndexOf(item)), followFocus: false);
        });
    }

    // Move the selection by one selectable step; the setter follows focus into the new tab. Reached from the
    // Alt-arrow tunnel, so the panel always has focus here.
    private void MoveSelection(int delta, bool followFocus)
    {
        if (_tabs.Count == 0) return;
        if (_selected is null) { var first = _tabs.Find(IsSelectable); if (first is not null) SelectItemCore(first, followFocus); return; }
        var target = NextSelectableIndex(_tabs.IndexOf(_selected), delta);
        if (target >= 0) SelectItemCore(_tabs[target], followFocus);
    }

    // The single point that changes the selection. `item` == null clears it (the empty state). No-ops when already
    // selected. Deactivates the old header, activates the new, swaps the filled content, optionally follows focus,
    // and raises SelectionChanged with the new index.
    private void SelectItemCore(TabItem? item, bool followFocus)
    {
        if (ReferenceEquals(item, _selected)) return;

        if (_selected is not null) _selected.Header.IsActive = false;
        _selected = item;

        if (item is not null)
        {
            item.Header.IsActive = true;
            control.SetFill(item.Content.RenderNode());
            if (followFocus) FocusActiveTab();
        }
        else
        {
            control.SetFill(null);
        }

        SelectionChanged?.Invoke(this, SelectedIndex);
    }

    // Rebuild the tab bar from the current model: only non-hidden headers, in order; keep each header's Index in
    // sync; resize a vertical bar to the widest visible label.
    private void RebuildBar()
    {
        var visible = new List<ConsoleGUI.IControl>(_tabs.Count + 1);
        var thickness = 1;
        for (var i = 0; i < _tabs.Count; i++)
        {
            _tabs[i].Header.SetIndex(i);
            if (_tabs[i].IsHidden) continue;
            visible.Add(_tabs[i].Header);
            thickness = Math.Max(thickness, _tabs[i].Header.Width);
        }

        if (_showAddButton)
        {
            _addButton ??= CreateAddButton();
            visible.Add(_addButton);
            thickness = Math.Max(thickness, _addButton.Width);
        }

        control.SetHeaders(visible);
        control.SetBarThickness(thickness);
    }

    private TabAddButton CreateAddButton()
    {
        var button = new TabAddButton();
        button.Activated += (_, _) => NewTabRequested?.Invoke(this, EventArgs.Empty);
        return button;
    }

    // Put focus on the active tab's content when it can take keyboard input; otherwise on its header (e.g. a tab
    // whose content is a plain label). NB: content that is a layout/composite focuses the header — navigate in with
    // Ctrl+N — since "first interactive descendant" isn't resolved here.
    private void FocusActiveTab()
    {
        if (_selected is null) return;
        var content = _selected.Content;
        if (content is Control { Focusable: true, HandlesInput: true } interactive) UI.SetFocus(interactive);
        else UI.SetFocus(_selected.Header);
    }
    #endregion

    #region Fields
    private readonly bool _horizontal;
    private readonly List<TabItem> _tabs = new();
    private TabItem? _selected;
    private SelectionStyle _selectionStyle;
    private bool _closableTabs;
    private bool _showAddButton;
    private TabAddButton? _addButton;
    #endregion
}

/// <summary>Arguments for <see cref="TabPanel.TabCloseRequested"/>.</summary>
/// <remarks>Set <see cref="Cancel"/> to keep the tab open (e.g. after confirming unsaved changes); otherwise the
/// panel removes it.</remarks>
public sealed class TabCloseEventArgs : EventArgs
{
    internal TabCloseEventArgs(TabItem tab) => Tab = tab;

    /// <summary>The tab whose ✕ was clicked.</summary>
    public TabItem Tab { get; }

    /// <summary>Set to <see langword="true"/> to cancel the close and keep the tab.</summary>
    public bool Cancel { get; set; }
}
