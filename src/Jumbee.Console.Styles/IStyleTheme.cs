namespace Jumbee.Console;

/// <summary>
/// The general appearance theme: a set of semantic <see cref="Style"/> tokens (foreground + background +
/// decoration) that controls compose when resolving their default appearance, plus the rest of a control's
/// non-glyph styling — e.g. a frame's border shape (<see cref="FrameBorder"/>) and its title's
/// position/border/colour.
/// </summary>
/// <remarks>
/// <para>
/// Only the literal glyphs rendered in controls live in <see cref="IGlyphTheme"/>. A theme defines
/// <em>appearance only</em>; it never changes a control's behaviour. Members are default-implemented, so a
/// custom theme overrides only what it wants to change. Because the members are default interface
/// implementations, hold and read a theme through this interface type (e.g. <c>UI.StyleTheme</c>), not
/// through a concrete class.
/// </para>
/// <para>
/// Controls must read these tokens <em>once</em> (in their constructor) into plain fields — never on the
/// render path — so theming costs nothing per frame. See <see cref="DefaultStyleTheme"/> for the built-in values.
/// </para>
/// </remarks>
public interface IStyleTheme
{
    #region Text
    /// <summary>Primary body/label text.</summary>
    Style Text => Style.Grey93;

    /// <summary>Secondary, de-emphasised text.</summary>
    Style TextMuted => Style.Grey58 | Style.Dim;

    /// <summary>Accent-coloured text/marks (a checked box's tick, a selected radio's dot, a switch's "on" thumb).</summary>
    Style TextAccent => Style.Green1;

    /// <summary>Disabled text/controls.</summary>
    Style TextDisabled => Style.Grey42 | Style.Dim;
    #endregion

    #region Structural
    /// <summary>A panel/container fill.</summary>
    Style Surface => Style.Bg(new Color(30, 30, 38));

    /// <summary>The text/character style of a frame border at rest (its colour).</summary>
    /// <remarks>Distinct from <see cref="FrameBorder"/>, which selects the border <em>shape</em>.</remarks>
    Style BorderText => Style.Grey50;

    /// <summary>The text/character style of a frame border when its control is focused.</summary>
    Style BorderFocusedText => Style.Cyan1;

    /// <summary>The text/character style of a frame title (its colour).</summary>
    /// <remarks>Distinct from <see cref="TitleStyle"/>, which controls the title's placement, border, and
    /// Normal/Reverse colouring.</remarks>
    Style TitleText => Style.Grey85;
    #endregion

    #region Interactive states
    /// <summary>A selected/highlighted row (foreground + background).</summary>
    Style Selection => Style.White | Style.Bg(new Color(40, 50, 80));

    /// <summary>How navigable controls (ListBox, Tree, TabPanel) indicate their selected item — a background
    /// highlight, an underline, or a caret prefix. Defaults to <see cref="SelectionStyle.Highlight"/>.</summary>
    SelectionStyle SelectionStyle => SelectionStyle.Highlight;

    /// <summary>The colour of a <c>Tree</c>'s leaf glyph (the marker before a childless node).
    /// Defaults to <see cref="TextAccent"/>.</summary>
    /// <remarks>Only its foreground is used.</remarks>
    Style TreeLeaf => TextAccent;

    /// <summary>A row/control under the pointer (background tint).</summary>
    Style Hover => Style.Bg(new Color(45, 45, 60));

    /// <summary>The default focus cue applied to a focused control that isn't framed with a visible border and
    /// doesn't indicate focus in its own way — so keyboard focus is always visible.</summary>
    /// <remarks>Controls that show focus themselves (buttons, tabs, editors with a cursor) opt out via
    /// <c>Control.RendersOwnFocus</c>. The colour is used per <see cref="FocusStyle"/>.</remarks>
    Style Focus => Style.Bg(new Color(48, 56, 82));

    /// <summary>How the default focus cue (see <see cref="Focus"/>) is drawn — a full tint, an edge ring, or an
    /// underline. Defaults to <see cref="FocusStyle.Tint"/>.</summary>
    FocusStyle FocusStyle => FocusStyle.Tint;

    /// <summary>A primary action surface at rest (e.g. a default button).</summary>
    Style Primary => Style.White | Style.Bg(new Color(40, 70, 120));

    /// <summary>A primary action surface under the pointer.</summary>
    Style PrimaryHover => Style.White | Style.Bg(new Color(60, 90, 150));

    /// <summary>A primary action surface while pressed/active.</summary>
    Style PrimaryActive => Style.White | Style.Bg(new Color(90, 130, 200));

    /// <summary>A secondary/neutral action surface at rest.</summary>
    Style Secondary => Style.Grey93 | Style.Bg(new Color(55, 55, 65));

    /// <summary>A secondary action surface under the pointer.</summary>
    Style SecondaryHover => Style.White | Style.Bg(new Color(75, 75, 88));

    /// <summary>A secondary action surface while pressed/active.</summary>
    Style SecondaryActive => Style.White | Style.Bg(new Color(100, 100, 115));
    #endregion

    #region Buttons
    /// <summary>The default style for a primary <c>Button</c> (its per-state fills, border mode, and width).</summary>
    /// <remarks>Composed from the <see cref="Primary"/> family so a theme that recolours those gets a matching
    /// button for free. Flat by default; a button can opt into <see cref="ButtonShape.Modern"/> for the raised
    /// look.</remarks>
    ButtonStyle PrimaryButton => new(Primary, PrimaryHover, PrimaryActive, minWidth: 16);

    /// <summary>The default style for a secondary <c>Button</c>.</summary>
    /// <remarks>Composed from the <see cref="Secondary"/> family.</remarks>
    ButtonStyle SecondaryButton => new(Secondary, SecondaryHover, SecondaryActive, minWidth: 16);
    #endregion

    #region Status
    /// <summary>The style for success messages and indicators.</summary>
    Style Success => Style.Green1;
    /// <summary>The style for warning messages and indicators.</summary>
    Style Warning => Style.Yellow1;
    /// <summary>The style for error messages and indicators.</summary>
    Style Error => Style.Red1;
    /// <summary>The style for informational messages and indicators.</summary>
    Style Info => Style.SkyBlue1;
    #endregion

    #region Scrollbar
    /// <summary>The per-part colours/decoration a control frame's vertical scrollbar uses (glyphs come from
    /// <see cref="IGlyphTheme.ScrollBar"/>). Defaults to <see cref="ScrollBarStyle.Default"/>.</summary>
    ScrollBarStyle ScrollBar => ScrollBarStyle.Default;
    #endregion

    #region Gauge
    /// <summary>The fill/track/text colours a <c>Gauge</c> uses. Defaults to
    /// <see cref="GaugeStyle.Default"/>.</summary>
    GaugeStyle Gauge => GaugeStyle.Default;
    #endregion

    #region Frame
    /// <summary>The default border shape for a control frame when none is specified. Defaults to <see cref="BorderStyle.None"/>.</summary>
    BorderStyle FrameBorder => BorderStyle.None;

    /// <summary>The border shape a control frame uses while its control is focused, or <see langword="null"/> to keep
    /// <see cref="FrameBorder"/> unchanged (showing focus through the <see cref="BorderFocusedText"/> colour only).
    /// Defaults to <see langword="null"/>.</summary>
    /// <remarks>Switching shape on focus never changes the frame's geometry — the border offset comes from
    /// <c>BorderPlacement</c>, not the shape — so a focused frame restyles in place without reflowing its
    /// siblings.</remarks>
    BorderStyle? FocusedFrameBorder => null;

    /// <summary>The default title style for a control frame — its position, border placement, and Normal/Reverse
    /// colouring, in one value. Defaults to <see cref="TitleStyle.Default"/>.</summary>
    TitleStyle TitleStyle => TitleStyle.Default;
    #endregion

    #region Overlay
    /// <summary>The tint a modal overlay scrim blends the layer beneath it toward. Defaults to a near-black.</summary>
    /// <remarks>Only its background colour is used. Paired with <see cref="ScrimDim"/>.</remarks>
    Style Scrim => Style.Bg(new Color(10, 10, 15));

    /// <summary>How strongly a modal scrim dims the layer beneath it: 0 = fully see-through, 1 = a solid
    /// <see cref="Scrim"/> fill. Defaults to 0.6.</summary>
    float ScrimDim => 0.6f;
    #endregion

    #region Plot
    /// <summary>The fill painted behind a <c>Plot</c>'s drawing area.</summary>
    /// <remarks>Only its background colour is used; the default has none, leaving a plot transparent so whatever is
    /// behind it shows through.</remarks>
    Style PlotSurface => Style.Plain;

    /// <summary>The colour of a <c>Plot</c>'s axis lines.</summary>
    /// <remarks>Only its foreground colour is used. The axis <em>captions</em> are set per-plot with
    /// <c>SetAxisTitles</c> and are not themed.</remarks>
    Style PlotAxis => Style.White;

    /// <summary>The colour of a <c>Plot</c>'s background grid lines.</summary>
    /// <remarks>Only its foreground colour is used. The grid's dashed/solid <em>brush</em> is a per-plot setting.</remarks>
    Style PlotGrid => Style.Grey;

    /// <summary>The colour of a <c>Plot</c>'s tick marks and its numeric tick labels.</summary>
    /// <remarks>Only its foreground colour is used. Marks and labels share one token — they read as one piece of
    /// chrome, and every scheme that has motivated this so far colours them together.</remarks>
    Style PlotTickLabel => Style.White;

    /// <summary>The ordered colours a <c>Plot</c> cycles through for series that don't name one. Defaults to
    /// <see cref="PlotPalette.Default"/>.</summary>
    PlotPalette PlotSeries => PlotPalette.Default;
    #endregion
}

/// <summary>The built-in style theme: every token uses <see cref="IStyleTheme"/>'s default values.</summary>
public sealed class DefaultStyleTheme : IStyleTheme;
