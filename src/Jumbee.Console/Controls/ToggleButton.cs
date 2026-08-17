namespace Jumbee.Console;

using System;
using System.Collections.Generic;

using ConsoleGUI.Input;
using ConsoleGUI.Space;

using Spectre.Console.Rendering;

/// <summary>
/// Shared base for the single-state toggle widgets (<see cref="Checkbox"/>, <see cref="RadioButton"/>,
/// <see cref="Switch"/>).
/// </summary>
/// <remarks>
/// <para>
/// Renders a state indicator followed by an optional text label, toggles on a mouse click or Enter/Space while
/// focused, and raises <see cref="Changed"/> when the state flips.
/// </para>
/// <para>
/// Appearance is theme-driven: the style tokens are captured once from <see cref="UI.StyleTheme"/> in the
/// constructor, and each subclass calls <see cref="SetGlyphs"/> with its <see cref="UI.GlyphTheme"/> glyphs.
/// The indicator width (and hence the control's width) is derived from the themed glyph, so a glyph theme with
/// wider/narrower markers re-sizes the control without any control-specific code. Captured tokens are read on
/// the render path as plain fields, so theming costs nothing per frame; callers may still override any token
/// via its setter.
/// </para>
/// </remarks>
public abstract class ToggleButton : RenderableControl
{
    #region Constructors
    /// <summary>Initializes a new <see cref="ToggleButton"/> with the given label <paramref name="text"/>.</summary>
    protected ToggleButton(string text)
    {
        _text = text;
        Height = 1;
        // Styles + glyphs are captured by ApplyTheme, which the subclass calls from its constructor (and which
        // re-runs on a runtime theme switch). Width is set there via SetGlyphs once the themed glyphs are known.
    }
    #endregion

    #region Events
    /// <summary>Raised with the new state whenever <see cref="IsChecked"/> changes.</summary>
    public event EventHandler<bool>? Changed;
    #endregion

    #region Properties
    /// <summary>Reports <see langword="true"/> so input routing delivers keys to the control.</summary>
    public override bool HandlesInput => true;

    /// <summary>Whether the toggle is in its on state. Setting it raises <see cref="Changed"/> when it flips.</summary>
    public bool IsChecked
    {
        get => _isChecked;
        set => SetAtomicProperty(ref _isChecked, value, watch: (_, v) => Changed?.Invoke(this, v));
    }

    /// <summary>The label drawn after the state indicator. Setting it re-sizes the control.</summary>
    public string Text
    {
        get => _text;
        set => SetAtomicProperty(ref _text, value, updatesLayout: true, watch: (_, _) => RefreshWidth());
    }

    /// <summary>Label style. Defaults to <see cref="IStyleTheme.Text"/>.</summary>
    public Style LabelStyle { get => _labelStyle; set => SetAtomicProperty(ref _labelStyle, value, themeOverride: true); }

    /// <summary>Indicator style when checked. Defaults to <see cref="IStyleTheme.TextAccent"/>.</summary>
    public Style AccentStyle { get => _accentStyle; set => SetAtomicProperty(ref _accentStyle, value, themeOverride: true); }

    /// <summary>Indicator style when unchecked. Defaults to <see cref="IStyleTheme.TextMuted"/>.</summary>
    public Style MutedStyle { get => _mutedStyle; set => SetAtomicProperty(ref _mutedStyle, value, themeOverride: true); }

    /// <summary>Style merged across the row while hovered (typically a background). Defaults to <see cref="IStyleTheme.Hover"/>.</summary>
    public Style HoverStyle { get => _hoverStyle; set => SetAtomicProperty(ref _hoverStyle, value, themeOverride: true); }

    /// <summary>
    /// Whether the control responds to the user. A disabled toggle draws in <see cref="DisabledStyle"/>, ignores
    /// clicks and keys, and is skipped by Tab.
    /// </summary>
    /// <remarks>
    /// It still reports its state truthfully: <see cref="IsChecked"/> and <see cref="Toggle"/> keep working, so a
    /// panel can show what a setting <em>is</em> while the user is not in a position to change it. That is the
    /// whole point of having this rather than hiding the control — a switch drawn in the off position because it
    /// does not currently apply is indistinguishable from one that is genuinely off.
    /// </remarks>
    public bool Enabled
    {
        get => _enabled;
        set => SetAtomicProperty(ref _enabled, value, watch: (_, on) => ApplyEnabledToFocus(on));
    }

    /// <summary>Style for the indicator and label while disabled. Defaults to <see cref="IStyleTheme.TextDisabled"/>.</summary>
    public Style DisabledStyle { get => _disabledStyle; set => SetAtomicProperty(ref _disabledStyle, value, themeOverride: true); }
    #endregion

    #region Methods
    /// <summary>Flips the state (the same path as a click). Overridden by <see cref="RadioButton"/> to latch on.</summary>
    public virtual void Toggle() => IsChecked = !IsChecked;

    /// <inheritdoc/>
    // Captures the label/indicator styles from the style theme. Subclasses override to also set their glyphs
    // (via SetGlyphs) from the glyph theme, calling base first.
    protected override void ApplyTheme()
    {
        if (!IsThemeOverridden(nameof(LabelStyle))) _labelStyle = UI.StyleTheme.Text;
        if (!IsThemeOverridden(nameof(AccentStyle))) _accentStyle = UI.StyleTheme.TextAccent;
        if (!IsThemeOverridden(nameof(MutedStyle))) _mutedStyle = UI.StyleTheme.TextMuted;
        if (!IsThemeOverridden(nameof(HoverStyle))) _hoverStyle = UI.StyleTheme.Hover;
        if (!IsThemeOverridden(nameof(DisabledStyle))) _disabledStyle = UI.StyleTheme.TextDisabled;
    }

    /// <summary>Sets the on/off indicator glyphs (from the glyph theme), measures the indicator width from them,
    /// and re-sizes the control. Subclasses call this from their constructor.</summary>
    protected void SetGlyphs(string on, string off)
    {
        _on = on;
        _off = off;
        _indicatorWidth = IGlyphTheme.CellWidth(on, off);
        RefreshWidth();
    }

    /// <summary>The width in cells of the state indicator glyph.</summary>
    protected int IndicatorWidth => _indicatorWidth;

    /// <inheritdoc/>
    protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        var indicator = _isChecked ? _accentStyle : _mutedStyle;
        var label = _labelStyle;
        // Disabled wins over hover, and suppresses it: a control that highlights under the pointer but does nothing
        // when clicked is worse than one that plainly does not react.
        if (!_enabled) indicator = label = _disabledStyle;
        else if (IsMouseOver) { indicator |= _hoverStyle; label |= _hoverStyle; }

        yield return new Segment(_isChecked ? _on : _off, indicator);

        var text = _text.Length > 0 ? " " + _text : string.Empty;
        var fill = Math.Max(0, maxWidth - _indicatorWidth);
        text = text.Length > fill ? text[..fill] : text.PadRight(fill);
        yield return new Segment(text, label);
    }

    // The three input paths all check Enabled; Toggle itself does not, so code can still set the state.
    /// <inheritdoc/>
    protected override void OnClick(Position position)
    {
        if (_enabled) Toggle();
    }

    /// <inheritdoc/>
    // A double-click is two presses; for a toggle that means two state changes (the same as clicking twice).
    protected override void OnDoubleClick(Position position)
    {
        if (_enabled) Toggle();
    }

    /// <inheritdoc/>
    protected override void OnInput(InputEvent inputEvent)
    {
        if (!_enabled) return;
        if (inputEvent.Key.Key is ConsoleKey.Enter or ConsoleKey.Spacebar)
        {
            Toggle();
            inputEvent.Handled = true;
        }
    }

    private void RefreshWidth() => Width = _indicatorWidth + (_text.Length > 0 ? _text.Length + 1 : 0);
    #endregion

    #region Fields
    private bool _isChecked;
    private string _text;
    private string _on = "";
    private string _off = "";
    private int _indicatorWidth;
    private Style _labelStyle;
    private Style _accentStyle;
    private Style _mutedStyle;
    private Style _hoverStyle;
    private Style _disabledStyle;
    private bool _enabled = true;
    #endregion
}
