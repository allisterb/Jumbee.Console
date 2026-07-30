namespace Jumbee.Console;

using System;
using System.Collections.Generic;

using ConsoleGUI.Space;

using Spectre.Console;
using Spectre.Console.Rendering;

/// <summary>
/// The "+" new-tab button appended to a <see cref="TabPanel"/>'s bar when <see cref="TabPanel.ShowAddButton"/> is set.
/// </summary>
/// <remarks>
/// Reachable by mouse (click) and by keyboard — the panel's tab-strip arrows step onto it past the last tab, and
/// Enter/Space activates it. Activating raises <see cref="Activated"/>, which the panel turns into
/// <see cref="TabPanel.NewTabRequested"/>. It is still excluded from the panel's logical rows, so global (Ctrl+arrow)
/// region nav treats the strip as one unit.
/// </remarks>
internal sealed class TabAddButton : RenderableControl
{
    #region Constructors
    internal TabAddButton()
    {
        ApplyTheme();
        Height = 1;
        Width = _glyph.GetCellWidth() + 2;   // a space of padding either side
    }
    #endregion

    #region Events
    /// <summary>Raised when the button is clicked or activated with Enter/Space. (Named <c>Activated</c>, like
    /// <see cref="Button"/>, rather than reusing the mouse-only <see cref="Control.Clicked"/>.)</summary>
    public event EventHandler? Activated;
    #endregion

    #region Properties
    // Tag the button's cells with a mouse listener, so it receives hover/click as well as keyboard focus.
    protected override bool WantsMouse => true;
    protected override bool RendersOwnFocus => true;   // underlines on focus
    #endregion

    #region Methods
    protected override void ApplyTheme()
    {
        _glyph = UI.GlyphTheme.TabAdd;
        _style = UI.StyleTheme.TextMuted;
        _hoverStyle = UI.StyleTheme.Hover;
    }

    protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        var style = (IsMouseOver ? _hoverStyle : _style);
        if (IsFocused) style |= Style.Underline;   // show keyboard focus, like an inactive tab header
        var label = $" {_glyph} ";
        if (label.Length < maxWidth) label = label.PadRight(maxWidth);
        else if (label.Length > maxWidth) label = label[..Math.Max(0, maxWidth)];
        yield return new Segment(label, style.SpectreConsoleStyle);
    }

    protected override void OnClick(Position position) => Activate();

    // Fires the click/activation. Keyboard activation (Enter/Space) is driven by the owning TabPanel's tunnel
    // (InterceptInput), because the button is not in the panel's logical rows, so Layout routing never dispatches
    // input to it directly.
    internal void Activate() => Activated?.Invoke(this, EventArgs.Empty);
    #endregion

    #region Fields
    private string _glyph = "";
    private Style _style;
    private Style _hoverStyle;
    #endregion
}
