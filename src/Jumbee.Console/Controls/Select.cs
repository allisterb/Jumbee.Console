namespace Jumbee.Console;

using System;
using System.Collections.Generic;
using System.Linq;

using ConsoleGUI;
using ConsoleGUI.Input;
using ConsoleGUI.Space;

using Spectre.Console.Rendering;

/// <summary>Where a <see cref="Select"/>'s dropdown opens relative to the control.</summary>
public enum SelectPopupPosition
{
    /// <summary>Open below when the dropdown fits under the control, otherwise above — so a Select near the bottom of
    /// the screen still shows all its options.</summary>
    Auto,
    /// <summary>Always open below the control.</summary>
    Below,
    /// <summary>Always open above the control.</summary>
    Above,
}

/// <summary>
/// A drop-down selector.
/// </summary>
/// <remarks>
/// Closed, it shows the current value with a ▼ marker; clicking it (or Enter/Space while
/// focused) opens its options in the ambient <see cref="UI.Overlay"/>. By default the list opens below the control,
/// flipping above when there isn't room (see <see cref="PopupPosition"/>). Choosing an option (click or Enter)
/// commits it; Escape or a click outside cancels.
/// </remarks>
public class Select : RenderableControl
{
    #region Constructors
    /// <summary>Initializes a new <see cref="Select"/> with the given <paramref name="options"/>.</summary>
    public Select(params string[] options)
    {
        _options = options.ToList();
        Height = 1;
        Width = PreferredWidth();
    }
    #endregion

    #region Events
    /// <summary>Raised when a different value is committed.</summary>
    public event EventHandler<string>? SelectionChanged;
    #endregion

    #region Properties
    /// <summary>The selectable options.</summary>
    public IReadOnlyList<string> Options => _options;

    /// <summary>Text shown when no option is selected.</summary>
    public string Placeholder { get; set; } = "Select…";

    /// <summary>The text colour of the collapsed control.</summary>
    public Color Foreground { get; set; } = Color.White;
    /// <summary>The background colour of the collapsed control.</summary>
    public Color Background { get; set; } = new(50, 50, 70);

    /// <summary>Whether the dropdown opens below or above the control. Defaults to <see cref="SelectPopupPosition.Auto"/>.</summary>
    public SelectPopupPosition PopupPosition { get; set; } = SelectPopupPosition.Auto;

    /// <summary>The index of the selected option, or -1 when none is selected. Setting it raises <see cref="SelectionChanged"/>.</summary>
    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            var clamped = _options.Count == 0 ? -1 : Math.Clamp(value, 0, _options.Count - 1);
            if (clamped == _selectedIndex) return;
            _selectedIndex = clamped;
            Invalidate();
            if (SelectedValue is { } v) SelectionChanged?.Invoke(this, v);
        }
    }

    /// <summary>The selected option text, or <see langword="null"/> when nothing is selected.</summary>
    public string? SelectedValue => _selectedIndex >= 0 && _selectedIndex < _options.Count ? _options[_selectedIndex] : null;

    /// <summary>Reports <see langword="true"/> so input routing delivers keys to the control.</summary>
    public override bool HandlesInput => true;
    #endregion

    #region Methods
    /// <summary>Opens the dropdown into the ambient <see cref="UI.Overlay"/> (no-op before <see cref="UI.Start"/>
    /// or with no options).</summary>
    public void Open()
    {
        if (UI.Overlay is not { } host || _options.Count == 0) return;

        var rows = Math.Min(_options.Count, MaxDropdownRows);
        var list = new ListBox(_options.ToArray())
        {
            SelectedForegroundColor = Color.White,
            SelectedBackgroundColor = new Color(40, 90, 160),
            Width = PreferredWidth(),
            Height = rows,
        };
        list.SelectedIndex = Math.Max(0, _selectedIndex);
        list.WithRoundedBorder(Color.Grey);

        list.Committed += (_, item) =>
        {
            var index = _options.IndexOf(item.Text ?? string.Empty);
            if (index >= 0) SelectedIndex = index;
            Close();
        };
        list.Cancelled += (_, _) => Close();

        if (_controlLeft >= 0) host.Show(list, _controlLeft, ResolveTop(rows + 2));   // + rounded border rows
        else host.Show(list);
    }

    // The y at which the dropdown opens, honouring PopupPosition. Auto opens below when the popup fits under the
    // control, else above when there's room there — so a Select near the bottom edge still shows every option.
    private int ResolveTop(int popupHeight)
    {
        var below = _controlTop + 1;                 // just under the one-row control
        var above = _controlTop - popupHeight;       // directly above it
        var screenHeight = ConsoleManager.WindowSize.Height;
        return PopupPosition switch
        {
            SelectPopupPosition.Below => below,
            SelectPopupPosition.Above => Math.Max(0, above),
            _ => below + popupHeight <= screenHeight ? below : above >= 0 ? above : below,
        };
    }

    /// <inheritdoc/>
    protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        var style = new Spectre.Console.Style(Foreground, Background);
        var label = SelectedValue ?? Placeholder;

        var inner = $" {label}";
        if (inner.Length > maxWidth - 2) inner = inner[..Math.Max(0, maxWidth - 2)];
        var text = inner.PadRight(Math.Max(0, maxWidth - 1)) + "▼";   // value left, arrow at the right edge

        yield return new Segment(text, style);
    }

    /// <inheritdoc/>
    protected override void OnClick(Position position)
    {
        // Record this control's top-left on screen: the click's absolute position minus its position relative to us.
        // Open() turns that into the dropdown anchor per PopupPosition.
        if (ConsoleManager.MousePosition is { } m)
        {
            _controlLeft = m.X - position.X;
            _controlTop = m.Y - position.Y;
        }
        Open();
    }

    /// <inheritdoc/>
    // The second click of a rapid pair arrives here, not at OnClick — without this a double-click on the closed
    // control would be swallowed instead of toggling the dropdown.
    protected override void OnDoubleClick(Position position) => OnClick(position);

    /// <inheritdoc/>
    protected override void OnInput(InputEvent inputEvent)
    {
        // Enter/Space or Down/Up open the dropdown (the standard combobox keys); the open list then navigates.
        if (inputEvent.Key.Key is ConsoleKey.Enter or ConsoleKey.Spacebar or ConsoleKey.DownArrow or ConsoleKey.UpArrow)
        {
            Open();
            inputEvent.Handled = true;
        }
    }

    private void Close()
    {
        UI.Overlay?.Hide();
        UI.SetFocus(this);
    }

    private int PreferredWidth()
    {
        var longest = _options.Count == 0 ? 0 : _options.Max(o => o.Length);
        return Math.Max(longest, Placeholder.Length) + 3;   // leading space + arrow + a little padding
    }
    #endregion

    #region Fields
    private const int MaxDropdownRows = 8;
    private readonly List<string> _options;
    private int _selectedIndex = -1;
    private int _controlLeft = -1;
    private int _controlTop = -1;
    #endregion
}
