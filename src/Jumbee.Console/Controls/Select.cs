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

    /// <summary>
    /// Whether the collapsed control is drawn only as wide as its widest option — the same width as the dropdown —
    /// instead of filling whatever width its layout offers. Default <see langword="false"/>.
    /// </summary>
    /// <remarks>
    /// The default suits a form, where a field filling its column is the convention and the <c>▼</c> sits at the
    /// right edge under the ones above it. Set this in a narrow panel of mixed controls, where a full-width block
    /// of colour for a three-word choice reads as far heavier than the choice is — and where the collapsed control
    /// not matching the list it opens is itself a little jarring.
    /// </remarks>
    public bool FitContent { get => _fitContent; set => SetAtomicProperty(ref _fitContent, value); }

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

    // Only while fitting: the themed cue fills the control's unpainted cells, which is right for a Select that
    // fills its column and wrong for one that doesn't. See Render.
    /// <inheritdoc/>
    protected override bool RendersOwnFocus => _fitContent;
    #endregion

    #region Methods
    /// <summary>Replaces the options, keeping the current value selected if it is still among them (otherwise the
    /// first, or nothing when the list is empty). Re-sizes the control to the new widest option.</summary>
    /// <remarks>For a drop-down over a list that changes at runtime — files that have been loaded, devices that
    /// have appeared. Raises <see cref="SelectionChanged"/> only if the selected <em>value</em> actually changes,
    /// so a rebuild that keeps the current choice is silent.</remarks>
    public void SetOptions(params IEnumerable<string> options)
    {
        var previous = SelectedValue;
        _options = [.. options];

        var index = previous is null ? -1 : _options.FindIndex(o => o == previous);
        if (index < 0) index = _options.Count > 0 ? 0 : -1;

        // Assigned directly rather than through SelectedIndex: the property short-circuits when the index is
        // unchanged, which is exactly the case where the value underneath it may have changed.
        _selectedIndex = index;
        Width = PreferredWidth();
        Invalidate();

        if (SelectedValue is { } current && current != previous) SelectionChanged?.Invoke(this, current);
    }

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
        var label = SelectedValue ?? Placeholder;

        // Fitting leaves the rest of the row untouched (transparent), so the control ends where the dropdown it
        // opens would — never wider than the room actually offered.
        var width = _fitContent ? Math.Min(maxWidth, PreferredWidth()) : maxWidth;

        // Focus is drawn into the box itself when fitting. The themed default cue fills every UNPAINTED cell of the
        // control, which for a fitted Select is the whole rest of the row — so choosing an option, which hands
        // focus back, made the control look like it had sprung back to full width.
        var background = _fitContent && IsFocused ? Background.Mix(Color.White, FocusLift) : Background;
        var style = new Spectre.Console.Style(Foreground, background);

        var inner = $" {label}";
        if (inner.Length > width - 2) inner = inner[..Math.Max(0, width - 2)];
        var text = inner.PadRight(Math.Max(0, width - 1)) + "▼";   // value left, arrow at the right edge

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

    // How far the focused fill is lifted toward white. Derived from the control's own Background rather than taken
    // from the theme so it stays visible whatever colour the caller set.
    private const double FocusLift = 0.22;

    private bool _fitContent;
    private List<string> _options;
    private int _selectedIndex = -1;
    private int _controlLeft = -1;
    private int _controlTop = -1;
    #endregion
}
