namespace Jumbee.Console;

using System;
using System.Collections.Generic;

using ConsoleGUI;
using ConsoleGUI.Input;
using ConsoleGUI.Space;

using Spectre.Console.Rendering;

/// <summary>
/// A horizontal bar of top-level menu titles (e.g. <c>File  Edit  View</c>).
/// </summary>
/// <remarks>
/// Clicking a title — or focusing the bar and pressing Enter / Down — opens that title's items as a non-modal
/// <see cref="ContextMenu"/> anchored just below it (so the rest of the app stays live; clicking elsewhere dismisses
/// the menu). Left/Right move between titles. The drop-downs float in the ambient <see cref="UI.Overlay"/>. Choosing
/// an item runs its <see cref="MenuItem.Action"/> and raises <see cref="ItemActivated"/>.
/// </remarks>
public class MenuBar : RenderableControl
{
    #region Constructors
    /// <summary>Initializes an empty <see cref="MenuBar"/>.</summary>
    public MenuBar() { }
    #endregion

    #region Events
    /// <summary>Raised when an item in any of the bar's menus is chosen.</summary>
    public event EventHandler<MenuItem>? ItemActivated;
    #endregion

    #region Properties
    /// <summary>Always <see langword="true"/>: the bar handles its own navigation and activation keys. No opt-in
    /// needed — unlike the base default, this is on.</summary>
    public override bool HandlesInput => true;
    /// <summary>Always <see langword="true"/>: menu titles receive hover and click. No opt-in needed — unlike the
    /// base default, this is on.</summary>
    protected override bool WantsMouse => true;
    /// <inheritdoc/>
    protected override bool RendersOwnFocus => true;   // highlights the active menu item
    #endregion

    #region Methods
    /// <summary>Adds a top-level menu with the given title and items. Returns this for chaining.</summary>
    public MenuBar Add(string title, params MenuItem[] items)
    {
        _menus.Add((title, items));
        Invalidate();
        return this;
    }

    /// <inheritdoc/>
    protected override int IntrinsicHeight() => 1;
    /// <inheritdoc/>
    protected override int IntrinsicWidth() => 0;   // fill the width; titles sit at the left

    /// <inheritdoc/>
    protected override void OnInput(InputEvent inputEvent)
    {
        switch (inputEvent.Key.Key)
        {
            case ConsoleKey.LeftArrow: MoveActive(-1); break;
            case ConsoleKey.RightArrow: MoveActive(+1); break;
            case ConsoleKey.Home: SetActive(0); break;
            case ConsoleKey.End: SetActive(_menus.Count - 1); break;
            case ConsoleKey.Enter:
            case ConsoleKey.Spacebar:
            case ConsoleKey.DownArrow: OpenActive(); break;
            default: return;
        }
        inputEvent.Handled = true;
    }

    /// <inheritdoc/>
    protected override void OnMouseMove(Position position) => RecordOrigin(position);

    /// <inheritdoc/>
    protected override void OnClick(Position position)
    {
        RecordOrigin(position);
        var index = TitleAt(position.X);
        if (index < 0) return;
        _active = index;
        OpenActive();
    }

    /// <inheritdoc/>
    protected internal override HelpInfo? GetHelpInfo() => new HelpInfo("MenuBar", "Menu bar", "Application menus.")
        .WithKey("Left / Right", "Move between menus")
        .WithKey("Enter / Down", "Open a menu");

    /// <summary>Opens the currently-active title's menu in the ambient <see cref="UI.Overlay"/> (no-op before
    /// <see cref="UI.Start"/> or with no menus).</summary>
    public void OpenActive()
    {
        if (UI.Overlay is null || _menus.Count == 0) return;
        var (_, items) = _menus[_active];

        var menu = new ContextMenu(items);
        menu.ItemActivated += (_, item) => ItemActivated?.Invoke(this, item);
        menu.Closed += (_, _) => { _openIndex = -1; Invalidate(); };

        _openIndex = _active;
        Invalidate();
        menu.Show(_originX + TitleOffset(_active), _originY + 1);   // just below the bar
    }

    private void MoveActive(int dir) => SetActive(Math.Clamp(_active + dir, 0, Math.Max(0, _menus.Count - 1)));

    private void SetActive(int index)
    {
        if (_menus.Count == 0) return;
        index = Math.Clamp(index, 0, _menus.Count - 1);
        if (index == _active) return;
        _active = index;
        Invalidate();
    }

    // Record our screen origin from the latest mouse event (absolute pointer minus its position relative to us), so
    // a keyboard-opened menu can still anchor under the right title. Defaults to (0,0) until the bar is first hovered.
    private void RecordOrigin(Position relative)
    {
        if (ConsoleManager.MousePosition is { } m)
        {
            _originX = m.X - relative.X;
            _originY = m.Y - relative.Y;
        }
    }

    private int TitleWidth(int i) => _menus[i].Title.Length + 2;   // a space on each side

    private int TitleOffset(int index)
    {
        var x = 0;
        for (var i = 0; i < index && i < _menus.Count; i++) x += TitleWidth(i);
        return x;
    }

    private int TitleAt(int x)
    {
        var start = 0;
        for (var i = 0; i < _menus.Count; i++)
        {
            var w = TitleWidth(i);
            if (x >= start && x < start + w) return i;
            start += w;
        }
        return -1;
    }

    /// <inheritdoc/>
    protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        if (maxWidth <= 0) yield break;
        var used = 0;
        for (var i = 0; i < _menus.Count; i++)
        {
            var label = $" {_menus[i].Title} ";
            if (used + label.Length > maxWidth) break;
            var hot = i == _openIndex || (IsFocused && i == _active);
            var style = hot ? UI.StyleTheme.Selection : UI.StyleTheme.Text;
            yield return new Segment(label, style.SpectreConsoleStyle);
            used += label.Length;
        }
    }
    #endregion

    #region Fields
    private readonly List<(string Title, MenuItem[] Items)> _menus = new();
    private int _active;
    private int _openIndex = -1;
    private int _originX;
    private int _originY;
    #endregion
}
