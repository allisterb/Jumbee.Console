namespace Jumbee.Console;

using Spectre.Console;

/// <summary>
/// The glyphs controls use for state indicators.
/// </summary>
/// <remarks>
/// Like <see cref="IStyleTheme"/>, a glyph theme affects <em>appearance only</em>: swapping glyphs never changes
/// behaviour, though glyphs of a different cell width do change a control's measured size (controls derive their
/// layout from the themed glyph's width rather than assuming a constant). Members are default-implemented;
/// override only the glyphs you want to change, and read the theme through this interface type (e.g.
/// <c>UI.GlyphTheme</c>).
/// </remarks>
public interface IGlyphTheme
{
    #region Checkbox
    /// <summary>Glyph for a checked checkbox.</summary>
    string CheckboxChecked => "[X]";
    /// <summary>Glyph for an unchecked checkbox.</summary>
    string CheckboxUnchecked => "[ ]";
    #endregion

    #region Radio
    /// <summary>Glyph for a selected radio button.</summary>
    string RadioSelected => "(●)";     // (●)
    /// <summary>Glyph for an unselected radio button.</summary>
    string RadioUnselected => "( )";
    #endregion

    #region Switch
    /// <summary>Glyph for a switch in the on position.</summary>
    string SwitchOn => "(─●)";    // (─●)
    /// <summary>Glyph for a switch in the off position.</summary>
    string SwitchOff => "(●─)";   // (●─)
    #endregion

    #region Scrollbar
    /// <summary>The glyphs a control frame's vertical scrollbar uses (colours come from <see cref="IStyleTheme.ScrollBar"/>).
    /// Defaults to <see cref="ScrollBarGlyphs.Default"/>.</summary>
    ScrollBarGlyphs ScrollBar => ScrollBarGlyphs.Default;
    #endregion

    #region ProgressBar
    /// <summary>The glyphs a <c>ProgressBar</c> draws its bar with (colours come from <see cref="IStyleTheme.ProgressBar"/>).
    /// Defaults to <see cref="ProgressBarGlyphs.Default"/> (the solid sub-cell band).</summary>
    ProgressBarGlyphs ProgressBar => ProgressBarGlyphs.Default;
    #endregion

    #region Slider
    /// <summary>The glyph a <c>Slider</c> draws its thumb with (colours come from <see cref="IStyleTheme.Slider"/>).
    /// Defaults to <c>"█"</c>.</summary>
    /// <remarks>Should be one cell wide: the thumb marks a single position on the track, and a wider glyph would
    /// push the rest of the track out of alignment with the pointer.</remarks>
    string SliderThumb => "█";
    #endregion

    #region Files
    /// <summary>The marker a <c>FileBrowser</c> draws before a folder. Defaults to <c>"▸"</c>.</summary>
    /// <remarks>Should share a cell width with <see cref="File"/> so names stay aligned down the listing.</remarks>
    string FolderClosed => "▸";

    /// <summary>The marker a <c>FileBrowser</c> draws before a file. Defaults to a blank of the same width as
    /// <see cref="FolderClosed"/>, so only folders carry a marker.</summary>
    string File => " ";
    #endregion

    #region Menu
    /// <summary>The marker drawn against a checked <c>MenuItem</c>. Defaults to <c>"✓"</c> (override with
    /// <c>"*"</c> for an ASCII terminal).</summary>
    /// <remarks>Its width sets the marker column a menu level reserves once any of its items is checkable, so all
    /// the labels in that level stay aligned.</remarks>
    string MenuChecked => "✓";
    #endregion

    #region Selection
    /// <summary>The glyph prefixed to the selected item when a control's <see cref="SelectionStyle"/> is
    /// <see cref="SelectionStyle.Caret"/> (includes its trailing spacing). Defaults to <c>"▶ "</c>.</summary>
    string SelectionCaret => "▶ ";
    #endregion

    #region Tabs
    /// <summary>The close glyph shown on a closable tab. Defaults to <c>"✕"</c> (override with <c>"x"</c> for an
    /// ASCII terminal).</summary>
    /// <remarks>Drawn only on the active/hovered tab; other tabs reserve a same-width blank.</remarks>
    string TabClose => "✕";

    /// <summary>The glyph on a tab panel's "+" new-tab button. Defaults to <c>"+"</c>.</summary>
    string TabAdd => "+";
    #endregion

    #region Tree
    /// <summary>Disclosure glyph shown before an <em>expanded</em> node that has children (includes trailing
    /// spacing). Defaults to <c>"▼ "</c>.</summary>
    /// <remarks>Both tree glyphs should share a cell width so labels stay aligned.</remarks>
    string TreeExpanded => "▼ ";
    /// <summary>Disclosure glyph shown before a <em>collapsed</em> node that has children (includes trailing
    /// spacing). Defaults to <c>"► "</c>.</summary>
    /// <remarks>U+25BA — the text-presentation counterpart of <c>▼</c>; the emoji-variant <c>▶</c> U+25B6 tofus in
    /// some fonts.</remarks>
    string TreeCollapsed => "► ";

    /// <summary>Glyph shown before a node that has <em>no</em> children (a leaf), including trailing spacing.
    /// Defaults to <c>"• "</c>.</summary>
    /// <remarks>Should share a cell width with the disclosure glyphs so labels stay aligned.</remarks>
    string TreeLeaf => "• ";
    #endregion

    #region Helpers
    /// <summary>The cell width of the widest of two state glyphs (both states of a toggle should be equal width;
    /// this guards against a theme that isn't).</summary>
    static int CellWidth(string a, string b) => System.Math.Max(a.GetCellWidth(), b.GetCellWidth());
    #endregion
}

/// <summary>The built-in glyph theme: every glyph uses <see cref="IGlyphTheme"/>'s default values.</summary>
public sealed class DefaultGlyphTheme : IGlyphTheme;
