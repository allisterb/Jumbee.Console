namespace Jumbee.Console;

using System;
using System.Collections.Generic;
using System.Linq;

using ConsoleGUI.Data;
using ConsoleGUI.Input;
using ConsoleGUI.Space;

/// <summary>
/// One entry in a <see cref="ContextMenu"/>: a label, an optional right-aligned shortcut hint, an action invoked
/// when chosen, an enabled flag, and an optional <see cref="Submenu"/>. Use <see cref="Separator"/> for a
/// non-selectable divider line.
/// </summary>
public sealed class MenuItem
{
    /// <summary>The item's label text.</summary>
    public string Text { get; init; } = string.Empty;
    /// <summary>An optional right-aligned shortcut hint (display only).</summary>
    public string? Shortcut { get; init; }
    /// <summary>The action invoked when the item is chosen.</summary>
    public Action? Action { get; init; }
    /// <summary>Whether the item is selectable; disabled items are shown muted and skipped.</summary>
    public bool Enabled { get; init; } = true;
    /// <summary>Whether the item is a non-selectable divider row.</summary>
    public bool IsSeparator { get; init; }

    /// <summary>Child items shown as a submenu to the right when this item is highlighted or chosen.</summary>
    /// <remarks>When set, the item opens the submenu instead of running its <see cref="Action"/>. Nest to any
    /// depth.</remarks>
    public IReadOnlyList<MenuItem>? Submenu { get; init; }

    /// <summary>Initializes an empty <see cref="MenuItem"/>.</summary>
    public MenuItem() { }
    /// <summary>Initializes a <see cref="MenuItem"/> with the given label and optional action.</summary>
    public MenuItem(string text, Action? action = null) { Text = text; Action = action; }

    /// <summary>A parent item that opens <paramref name="submenu"/> when chosen (Right/Enter, or hover).</summary>
    public MenuItem(string text, IEnumerable<MenuItem> submenu) { Text = text; Submenu = submenu.ToList(); }

    /// <summary>A non-selectable divider row.</summary>
    public static readonly MenuItem Separator = new() { IsSeparator = true, Enabled = false };

    internal bool Selectable => !IsSeparator && Enabled;
    internal bool HasSubmenu => Submenu is { Count: > 0 };
}

/// <summary>
/// A floating, keyboard-navigable menu of <see cref="MenuItem"/>s, shown anchored in the ambient <see cref="UI.Overlay"/>.
/// The shared primitive behind drop-downs / context menus / a <see cref="MenuBar"/>'s menus.
/// </summary>
/// <remarks>
/// <para>
/// Up/Down move the highlight (skipping separators and disabled items), Enter/Space choose, Escape or a click outside
/// dismiss. An item with a <see cref="MenuItem.Submenu"/> opens a child menu to its right (Right/Enter or hover to
/// open, Left to go back); the whole open chain is drawn as this one popup, so submenus nest to any depth. Choosing a
/// leaf item runs its <see cref="MenuItem.Action"/> and raises <see cref="ItemActivated"/>; <see cref="Closed"/> fires
/// whenever the menu closes.
/// </para>
/// <para>
/// Because the <see cref="Overlay"/> hosts a single popup, the menu draws the entire open submenu chain itself: it is
/// a plain <see cref="Control"/> that writes each level's bordered box directly into its buffer (leaving the gaps
/// between boxes transparent) and routes input to the deepest open level.
/// </para>
/// </remarks>
public class ContextMenu : Control
{
    #region Constructors
    /// <summary>Initializes a <see cref="ContextMenu"/> from the given top-level items.</summary>
    public ContextMenu(IEnumerable<MenuItem> items)
    {
        _root = items?.ToList() ?? throw new ArgumentNullException(nameof(items));
        ApplyTheme();
        ResetToRoot();
        OnLostFocus += () => Closed?.Invoke(this, EventArgs.Empty);   // dismissed (Escape / click-outside) or chosen
    }
    #endregion

    #region Events
    /// <summary>Raised when a leaf item is chosen (after its <see cref="MenuItem.Action"/> runs).</summary>
    public event EventHandler<MenuItem>? ItemActivated;

    /// <summary>Raised whenever the menu closes — whether an item was chosen or it was dismissed.</summary>
    public event EventHandler? Closed;
    #endregion

    #region Properties
    /// <summary>Always <see langword="true"/>: the menu handles its own navigation and activation keys. No opt-in
    /// needed — unlike the base default, this is on.</summary>
    public override bool HandlesInput => true;
    /// <summary>Always <see langword="true"/>: items receive hover and click, so hovering highlights an item and
    /// opens its submenu. No opt-in needed — unlike the base default, this is on.</summary>
    protected override bool WantsMouse => true;
    /// <summary>The menu's top-level items.</summary>
    public IReadOnlyList<MenuItem> Items => _root;
    #endregion

    #region Indexers
    // Guard cell reads against the real buffer bounds: while a submenu open/close resizes the control, the base
    // indexer (which guards on Size, not the buffer) can be read re-entrantly by the overlay's re-layout in the
    // window between Size growing and the buffer growing — reading past the buffer and throwing. Returning an empty
    // cell for out-of-buffer positions keeps that transient read safe.
    /// <inheritdoc/>
    public override Cell this[Position position]
    {
        get
        {
            var size = consoleBuffer.Size;
            if (position.X < 0 || position.Y < 0 || position.X >= size.Width || position.Y >= size.Height)
                return emptyCell;
            return base[position];
        }
    }
    #endregion

    #region Methods
    /// <summary>Shows the menu (reset to its root level) in the ambient <see cref="UI.Overlay"/> with its top-left at
    /// (<paramref name="x"/>, <paramref name="y"/>), then focuses it (no-op before <see cref="UI.Start"/>).</summary>
    public void Show(int x, int y)
    {
        _anchorX = x;
        _anchorY = y;
        ResetToRoot();
        UI.Overlay?.Show(this, x, y);
    }

    /// <inheritdoc/>
    protected override void ApplyTheme()
    {
        _textStyle = UI.StyleTheme.Text;
        _mutedStyle = UI.StyleTheme.TextMuted;
        _selectedStyle = UI.StyleTheme.Selection;
        _borderStyle = UI.StyleTheme.BorderText;
    }

    /// <inheritdoc/>
    protected internal override HelpInfo? GetHelpInfo() => new HelpInfo("Menu", "Menu", "A pop-up menu.")
        .WithKey("Up / Down", "Move")
        .WithKey("Right / Left", "Open / close submenu")
        .WithKey("Enter", "Choose")
        .WithKey("Esc", "Close");

    /// <inheritdoc/>
    protected override void OnInput(InputEvent inputEvent)
    {
        var active = _levels[^1];   // keyboard always drives the deepest open level
        switch (inputEvent.Key.Key)
        {
            case ConsoleKey.UpArrow: SetHighlight(active, NextSelectable(active.Items, active.Highlighted, -1)); break;
            case ConsoleKey.DownArrow: SetHighlight(active, NextSelectable(active.Items, active.Highlighted, +1)); break;
            case ConsoleKey.Home: SetHighlight(active, NextSelectable(active.Items, -1, +1)); break;
            case ConsoleKey.End: SetHighlight(active, NextSelectable(active.Items, active.Items.Count, -1)); break;
            case ConsoleKey.RightArrow: OpenSubmenu(); break;
            case ConsoleKey.LeftArrow: CloseDeepest(); break;
            case ConsoleKey.Enter:
            case ConsoleKey.Spacebar: ActivateHighlighted(); break;
            default: return;   // let other keys bubble (e.g. Escape is handled by the Overlay)
        }
        inputEvent.Handled = true;
    }

    // Hover highlights the item under the pointer and opens its submenu; moving to a shallower level trims the deeper
    // ones. Guarded on the last-hovered cell so jitter within one item doesn't rebuild the chain.
    /// <inheritdoc/>
    protected override void OnMouseMove(Position position)
    {
        var (level, item) = HitTest(position);
        if (level < 0) { _lastHoverLevel = _lastHoverItem = -1; return; }
        if (level == _lastHoverLevel && item == _lastHoverItem) return;
        _lastHoverLevel = level;
        _lastHoverItem = item;

        TrimTo(level);
        var lvl = _levels[level];
        if (item >= 0 && lvl.Items[item].Selectable)
        {
            lvl.Highlighted = item;
            if (lvl.Items[item].HasSubmenu) PushSubmenu(lvl);
        }
        Relayout();
    }

    /// <inheritdoc/>
    protected override void OnMouseLeave() { _lastHoverLevel = _lastHoverItem = -1; }

    // Click a leaf to choose it; click a submenu parent to open it.
    /// <inheritdoc/>
    protected override void OnClick(Position position)
    {
        var (level, item) = HitTest(position);
        if (level < 0 || item < 0) return;
        var menuItem = _levels[level].Items[item];
        if (!menuItem.Selectable) return;

        TrimTo(level);
        _levels[level].Highlighted = item;
        _lastHoverLevel = level;
        _lastHoverItem = item;
        if (menuItem.HasSubmenu) { PushSubmenu(_levels[level]); Relayout(); }
        else Activate(menuItem);
    }

    private void ResetToRoot()
    {
        _levels.Clear();
        _levels.Add(new Level(_root) { Highlighted = NextSelectable(_root, -1, +1) });
        _lastHoverLevel = _lastHoverItem = -1;
        Relayout();
    }

    private void SetHighlight(Level level, int index)
    {
        if (index < 0 || index == level.Highlighted) return;
        level.Highlighted = index;
        Relayout();
    }

    private void OpenSubmenu()
    {
        var active = _levels[^1];
        if (active.Highlighted >= 0 && active.Items[active.Highlighted].HasSubmenu)
        {
            PushSubmenu(active);
            Relayout();
        }
    }

    // Push the highlighted item's submenu of <paramref name="parent"/> (which must be the deepest open level).
    private void PushSubmenu(Level parent)
    {
        var sub = parent.Items[parent.Highlighted].Submenu!;
        _levels.Add(new Level(sub) { Highlighted = NextSelectable(sub, -1, +1) });
    }

    private void CloseDeepest()
    {
        if (_levels.Count <= 1) return;   // the root level always stays
        _levels.RemoveAt(_levels.Count - 1);
        Relayout();
    }

    // Drop every level deeper than <paramref name="level"/>.
    private void TrimTo(int level)
    {
        if (_levels.Count > level + 1)
            _levels.RemoveRange(level + 1, _levels.Count - level - 1);
    }

    private void ActivateHighlighted()
    {
        var active = _levels[^1];
        if (active.Highlighted < 0) return;
        var item = active.Items[active.Highlighted];
        if (item.HasSubmenu) OpenSubmenu();
        else Activate(item);
    }

    private void Activate(MenuItem item)
    {
        if (!item.Selectable) return;
        UI.Overlay?.Hide();   // close first (restores focus), then run the effect
        item.Action?.Invoke();
        ItemActivated?.Invoke(this, item);
    }

    // Recompute each level's box (size + position: level 0 at the origin, each child to the right of its parent and
    // aligned to the parent's highlighted row), size the control to the whole chain's bounding box, and repaint.
    private void Relayout()
    {
        for (var i = 0; i < _levels.Count; i++)
        {
            var lvl = _levels[i];
            lvl.Width = ContentWidth(lvl.Items) + 2;   // + left/right border
            lvl.Height = lvl.Items.Count + 2;          // + top/bottom border
            if (i == 0)
            {
                lvl.OriginX = lvl.OriginY = 0;
            }
            else
            {
                var parent = _levels[i - 1];
                lvl.OriginX = parent.OriginX + parent.Width;
                lvl.OriginY = parent.OriginY + Math.Max(0, parent.Highlighted);
            }
        }

        var w = _levels.Max(l => l.OriginX + l.Width);
        var h = _levels.Max(l => l.OriginY + l.Height);
        var sizeChanged = w != _chainWidth || h != _chainHeight;
        _chainWidth = w;
        _chainHeight = h;
        Initialize();   // re-measure via IntrinsicWidth/Height -> Resize, so Size (and the buffer) track the chain
        // When shown, a size change (opening/closing a submenu) must re-anchor so the overlay re-lays-out the popup
        // at its new size — the persistent live layout won't otherwise re-measure a control that resized itself.
        if (sizeChanged && ReferenceEquals(UI.Overlay?.Top, this)) UI.Overlay!.Reanchor(_anchorX, _anchorY);
        Invalidate();
    }

    // The menu sizes itself to the whole open chain's bounding box (an intrinsic extent honored even under a finite
    // parent), so the overlay anchors the popup to it and Size stays in step with the buffer.
    /// <inheritdoc/>
    protected override int IntrinsicWidth() => _chainWidth;
    /// <inheritdoc/>
    protected override int IntrinsicHeight() => _chainHeight;

    // The (level, item) under a content-space position, searching the deepest box first, or (-1, -1) if over a gap.
    private (int level, int item) HitTest(Position p)
    {
        for (var l = _levels.Count - 1; l >= 0; l--)
        {
            var lvl = _levels[l];
            if (p.X > lvl.OriginX && p.X < lvl.OriginX + lvl.Width - 1 &&
                p.Y > lvl.OriginY && p.Y < lvl.OriginY + lvl.Height - 1)
            {
                return (l, p.Y - lvl.OriginY - 1);
            }
        }
        return (-1, -1);
    }

    /// <inheritdoc/>
    protected override void Render()
    {
        ansiConsole.Clear(true);   // clear to transparent so the gaps between boxes show the layer beneath
        if (ActualWidth <= 0 || ActualHeight <= 0) return;
        foreach (var lvl in _levels) DrawLevel(lvl);
    }

    private void DrawLevel(Level lvl)
    {
        int ox = lvl.OriginX, oy = lvl.OriginY, w = lvl.Width, h = lvl.Height;

        // Rounded border around the box.
        Put(ox, oy, '╭', _borderStyle);
        Put(ox + w - 1, oy, '╮', _borderStyle);
        Put(ox, oy + h - 1, '╰', _borderStyle);
        Put(ox + w - 1, oy + h - 1, '╯', _borderStyle);
        for (var x = ox + 1; x < ox + w - 1; x++) { Put(x, oy, '─', _borderStyle); Put(x, oy + h - 1, '─', _borderStyle); }
        for (var y = oy + 1; y < oy + h - 1; y++) { Put(ox, y, '│', _borderStyle); Put(ox + w - 1, y, '│', _borderStyle); }

        var cw = w - 2;
        for (var i = 0; i < lvl.Items.Count; i++)
        {
            var ry = oy + 1 + i;
            var item = lvl.Items[i];
            if (item.IsSeparator)
            {
                for (var x = 0; x < cw; x++) Put(ox + 1 + x, ry, '─', _borderStyle);
                continue;
            }
            var style = !item.Enabled ? _mutedStyle : (i == lvl.Highlighted ? _selectedStyle : _textStyle);
            var row = FormatRow(item, cw);
            for (var x = 0; x < cw; x++) Put(ox + 1 + x, ry, row[x], style);
        }
    }

    // The interior text of a row, exactly <paramref name="cw"/> cells: " label" left, shortcut or a "►" submenu
    // marker right. Too narrow -> the label is truncated and the right part dropped.
    private static string FormatRow(MenuItem item, int cw)
    {
        var left = " " + item.Text;
        var right = item.HasSubmenu ? "► " : (string.IsNullOrEmpty(item.Shortcut) ? "" : item.Shortcut + " ");
        if (left.Length + right.Length > cw)
            return left.Length > cw ? left[..cw] : left.PadRight(cw);
        return left + new string(' ', cw - left.Length - right.Length) + right;
    }

    private void Put(int x, int y, char c, Style style)
    {
        // Guard on the real buffer size (not ActualWidth/Height): a layout can clamp the buffer smaller than the
        // requested chain size near a screen edge, so the chain is clipped rather than overrunning the buffer.
        var size = consoleBuffer.Size;
        if (x < 0 || y < 0 || x >= size.Width || y >= size.Height) return;
        consoleBuffer.Write(new Position(x, y), Glyph(c, style));
    }

    private static Character Glyph(char content, Style style)
    {
        var fg = style.ForegroundColor is { } f ? f.ToConsoleGUIColor() : (ConsoleGUI.Data.Color?)null;
        var bg = style.BackgroundColor is { } b ? b.ToConsoleGUIColor() : (ConsoleGUI.Data.Color?)null;
        var deco = style.SpectreConsoleStyle?.Decoration ?? Spectre.Console.Decoration.None;
        ConsoleGUI.Data.Decoration? decoration = deco == Spectre.Console.Decoration.None ? null : (ConsoleGUI.Data.Decoration)deco;
        return new Character(content, fg, bg, decoration);
    }

    private static int ContentWidth(IReadOnlyList<MenuItem> items)
    {
        var w = 0;
        foreach (var item in items)
        {
            if (item.IsSeparator) continue;
            var len = item.Text.Length
                + (item.HasSubmenu ? 2 : (string.IsNullOrEmpty(item.Shortcut) ? 0 : item.Shortcut!.Length + 2));
            w = Math.Max(w, len);
        }
        return w + 2;   // a leading space + a trailing space
    }

    // The next selectable index from <paramref name="from"/> in <paramref name="dir"/> (±1); if none, keeps a valid
    // current highlight, else falls back to the first selectable, else -1 (nothing selectable).
    private static int NextSelectable(IReadOnlyList<MenuItem> items, int from, int dir)
    {
        for (var i = from + dir; i >= 0 && i < items.Count; i += dir)
            if (items[i].Selectable) return i;
        if (from >= 0 && from < items.Count && items[from].Selectable) return from;
        for (var i = 0; i < items.Count; i++) if (items[i].Selectable) return i;
        return -1;
    }
    #endregion

    #region Child types
    // One open level of the menu chain: its items, the highlighted index, and its box rect in the control's buffer.
    private sealed class Level
    {
        public readonly IReadOnlyList<MenuItem> Items;
        public int Highlighted;
        public int OriginX, OriginY, Width, Height;
        public Level(IReadOnlyList<MenuItem> items) => Items = items;
    }
    #endregion

    #region Fields
    private readonly List<MenuItem> _root;
    private readonly List<Level> _levels = new();   // _levels[0] = root; deeper entries = open submenus
    private int _lastHoverLevel = -1;
    private int _lastHoverItem = -1;
    private int _chainWidth;
    private int _chainHeight;
    private int _anchorX;
    private int _anchorY;
    private Style _textStyle;
    private Style _mutedStyle;
    private Style _selectedStyle;
    private Style _borderStyle;
    #endregion
}
