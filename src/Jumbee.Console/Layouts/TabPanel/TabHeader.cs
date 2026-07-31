namespace Jumbee.Console;

using System;
using System.Collections.Generic;

using ConsoleGUI.Input;
using ConsoleGUI.Space;

using Spectre.Console;
using Spectre.Console.Rendering;

/// <summary>
/// A single clickable tab label in a <see cref="TabPanel"/>'s tab bar.
/// </summary>
/// <remarks>
/// Focusable and listener-tagging (so a click selects it and keyboard focus can land on it); renders its name styled
/// by active / hover / inactive state. Selecting raises <see cref="Activated"/> and closing raises
/// <see cref="CloseRequested"/>; tab-to-tab navigation (Alt+arrows) is handled by the owning <see cref="TabPanel"/>.
/// </remarks>
public class TabHeader : RenderableControl
{
    #region Constructors
    internal TabHeader(int index, string text)
    {
        _index = index;
        _text = text;
        ApplyTheme();        // capture SelectionStyle + caret first: a caret style reserves a gutter in the width
        Height = 1;
        Width = LabelWidth();
    }
    #endregion

    #region Events
    /// <summary>Raised when this tab is chosen — by a click or by Enter/Space while focused.</summary>
    public event EventHandler? Activated;

    /// <summary>Raised when the tab's close (✕) glyph is clicked (only fires when <see cref="Closable"/> and the
    /// glyph is shown — i.e. the tab is active or hovered).</summary>
    /// <remarks>The owning <see cref="TabPanel"/> turns this into a cancelable
    /// <see cref="TabPanel.TabCloseRequested"/>.</remarks>
    public event EventHandler? CloseRequested;
    #endregion

    #region Properties
    /// <summary><see langword="true"/> unless the tab is disabled: an enabled header handles Enter/Space to select.</summary>
    public override bool HandlesInput => _isEnabled;
    /// <summary><see langword="true"/>: the header shows keyboard focus itself by underlining an inactive tab.</summary>
    protected override bool RendersOwnFocus => true;   // underlines an inactive header on focus

    /// <summary>This tab's position among all tabs. Kept in sync by the owning <see cref="TabPanel"/>.</summary>
    public int Index => _index;

    internal void SetIndex(int index) => _index = index;

    /// <summary><see langword="false"/> for a disabled tab: drawn dimmed, not focusable, ignores clicks/keys.</summary>
    public bool IsEnabled
    {
        get => _isEnabled;
        set { Focusable = value; SetAtomicProperty(ref _isEnabled, value); }
    }

    /// <summary>The tab label.</summary>
    public string Text
    {
        get => _text;
        set => SetAtomicProperty(ref _text, value, updatesLayout: true, watch: (_, _) => Width = LabelWidth());
    }

    /// <summary><see langword="true"/> for the currently selected tab (drawn with <see cref="ActiveStyle"/>).</summary>
    public bool IsActive { get => _isActive; set => SetAtomicProperty(ref _isActive, value); }

    /// <summary>When <see langword="true"/> the tab reserves a close (✕) slot: the glyph is drawn on the active or
    /// hovered tab (a same-width blank otherwise, so labels don't shift), and clicking it raises
    /// <see cref="CloseRequested"/>. Set by the owning <see cref="TabPanel"/> via <see cref="TabPanel.ClosableTabs"/>.</summary>
    internal bool Closable { get => _closable; set => SetAtomicProperty(ref _closable, value, updatesLayout: true, watch: (_, _) => Width = LabelWidth()); }

    /// <summary>Style of the active (selected) tab. Defaults to <see cref="IStyleTheme.Selection"/>.</summary>
    public Style ActiveStyle { get => _activeStyle; set => SetAtomicProperty(ref _activeStyle, value, themeOverride: true); }

    /// <summary>Style of inactive tabs. Defaults to <see cref="IStyleTheme.TextMuted"/>.</summary>
    public Style InactiveStyle { get => _inactiveStyle; set => SetAtomicProperty(ref _inactiveStyle, value, themeOverride: true); }

    /// <summary>Style of a hovered inactive tab. Defaults to <see cref="IStyleTheme.Hover"/>.</summary>
    public Style HoverStyle { get => _hoverStyle; set => SetAtomicProperty(ref _hoverStyle, value, themeOverride: true); }

    /// <summary>How the active tab is indicated — highlight / underline / caret. Set by the owning
    /// <see cref="TabPanel"/>; defaults to the theme's <see cref="IStyleTheme.SelectionStyle"/>.</summary>
    internal SelectionStyle SelectionStyle { get => _selectionStyle; set => SetAtomicProperty(ref _selectionStyle, value, updatesLayout: true, themeOverride: true, watch: (_, _) => Width = LabelWidth()); }
    #endregion

    #region Methods
    /// <inheritdoc/>
    protected override void ApplyTheme()
    {
        if (!IsThemeOverridden(nameof(ActiveStyle))) _activeStyle = UI.StyleTheme.Selection;
        if (!IsThemeOverridden(nameof(InactiveStyle))) _inactiveStyle = UI.StyleTheme.TextMuted;
        if (!IsThemeOverridden(nameof(HoverStyle))) _hoverStyle = UI.StyleTheme.Hover;
        if (!IsThemeOverridden(nameof(SelectionStyle))) _selectionStyle = UI.StyleTheme.SelectionStyle;
        _selectionCaret = UI.GlyphTheme.SelectionCaret;
        _closeGlyph = UI.GlyphTheme.TabClose;
    }

    /// <summary>Renders the tab label styled by active / hover / focus / disabled state, with the optional close (✕) slot.</summary>
    protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        Spectre.Console.Style style;
        string label;
        // In Caret mode every header reserves a caret-width gutter (the active one shows the caret, the others blank)
        // so the labels stay aligned and the caret never truncates the text.
        var gutter = _selectionStyle == SelectionStyle.Caret ? new string(' ', _selectionCaret.GetCellWidth()) : "";
        if (!_isEnabled)
        {
            // Disabled: dim the inactive style; a disabled tab is never the active one.
            var b = _inactiveStyle.SpectreConsoleStyle;
            style = new Spectre.Console.Style(b.Foreground, b.Background, b.Decoration | Spectre.Console.Decoration.Dim);
            label = $" {gutter}{_text} ";
        }
        else if (_isActive)
        {
            // The active tab is the "selected item": render it per SelectionStyle (highlight / underline / caret).
            style = _selectionStyle.TextStyle(_activeStyle.ForegroundColor, _activeStyle.BackgroundColor);
            label = $" {(_selectionStyle == SelectionStyle.Caret ? _selectionCaret : gutter)}{_text} ";
        }
        else
        {
            var s = IsMouseOver ? _hoverStyle : _inactiveStyle;
            if (IsFocused) s |= Style.Underline;   // show keyboard focus on an inactive header
            style = s.SpectreConsoleStyle;
            label = $" {gutter}{_text} ";
        }

        // Closable tabs reserve a close slot after the label. The ✕ shows only on the active or hovered tab; the
        // rest reserve a same-width blank so labels don't shift as selection/hover moves. The base label already
        // ends in a space, so appending "{cell} " yields "…text {cell} " with one space on each side of the glyph.
        if (_closable)
        {
            var cell = (_isActive || IsMouseOver) ? _closeGlyph : new string(' ', _closeGlyph.GetCellWidth());
            label += $"{cell} ";
        }

        if (label.Length < maxWidth) label = label.PadRight(maxWidth);
        else if (label.Length > maxWidth) label = label[..Math.Max(0, maxWidth)];

        yield return new Segment(label, style);
    }

    /// <inheritdoc/>
    protected override void OnClick(Position position)
    {
        if (!_isEnabled) return;
        // A click on the ✕ (only when it is shown) closes rather than selects; anything else selects.
        if (_closable && (_isActive || IsMouseOver))
        {
            var (start, width) = CloseGlyphSpan();
            if (position.X >= start && position.X < start + width)
            {
                CloseRequested?.Invoke(this, EventArgs.Empty);
                return;
            }
        }
        Activated?.Invoke(this, EventArgs.Empty);
    }

    // The second click of a rapid pair arrives here, not at OnClick — without this a double-click on a tab (or on
    // its ✕) would be swallowed.
    protected override void OnDoubleClick(Position position) => OnClick(position);

    // The column span of the close glyph within the header: one leading space + the caret/gutter + the label + the
    // separating space. Mirrors the layout built in Render and the widths in LabelWidth.
    private (int start, int width) CloseGlyphSpan()
    {
        var gutter = _selectionStyle == SelectionStyle.Caret ? _selectionCaret.GetCellWidth() : 0;
        return (gutter + _text.Length + 2, _closeGlyph.GetCellWidth());
    }

    /// <inheritdoc/>
    protected override void OnInput(InputEvent inputEvent)
    {
        if (!_isEnabled) return;

        // Enter/Space selects this tab; switching between tabs (Alt+arrows) is handled by the owning TabPanel.
        if (inputEvent.Key.Key is ConsoleKey.Enter or ConsoleKey.Spacebar)
        {
            Activated?.Invoke(this, EventArgs.Empty);
            inputEvent.Handled = true;
        }
    }

    // A space of padding either side, plus a caret-width gutter when the selection style is Caret, plus a close
    // slot (glyph width + one separating space) when the tab is closable.
    private int LabelWidth() =>
        _text.Length + 2
        + (_selectionStyle == SelectionStyle.Caret ? _selectionCaret.GetCellWidth() : 0)
        + (_closable ? _closeGlyph.GetCellWidth() + 1 : 0);
    #endregion

    #region Fields
    private int _index;
    private string _text;
    private bool _isActive;
    private bool _isEnabled = true;
    private bool _closable;
    private string _closeGlyph = "";
    private Style _activeStyle;
    private Style _inactiveStyle;
    private Style _hoverStyle;
    private SelectionStyle _selectionStyle;
    private string _selectionCaret = "";
    #endregion
}
