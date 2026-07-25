namespace ScopeTui;

using Jumbee.Console;

/// <summary>
/// A scope colour scheme, expressed as an <see cref="IStyleTheme"/> and selected with --scheme.
/// </summary>
/// <remarks>
/// <para>
/// Everything the scope draws is themed rather than passed around: <see cref="Plot"/> reads the plot tokens for its
/// axis/grid/tick/surface colours and its series palette, and <see cref="ControlFrame"/> reads
/// <see cref="IStyleTheme.BorderText"/> for a pane's at-rest border. Program.cs applies one of these with
/// <c>UI.SetTheme</c> and then constructs the panes normally — no colour arguments anywhere.
/// </para>
/// <para>
/// Only <see cref="BorderFocusedText"/> is deliberately left at the interface default: it is the Tab focus cue, and
/// a scheme that recoloured it to match itself would defeat the point of a cue that has to stand out.
/// </para>
/// <para>
/// The trace colours also reach the display modes as data (a <see cref="Series"/> carries its own colour, computed
/// off the UI thread), so <see cref="GraphConfig"/> still holds a palette. It sources it from here rather than
/// duplicating it — see <c>Program.cs</c>.
/// </para>
/// </remarks>
public sealed class ScopeTheme : IStyleTheme
{
    #region Constructors
    private ScopeTheme(string name, PlotPalette palette, Color labels, Color axis, Color? background, Color? border)
    {
        Name = name;
        Palette = palette;
        LabelsColor = labels;
        AxisColor = axis;
        BackgroundColor = background;
        BorderColor = border;
    }
    #endregion

    #region Properties
    /// <summary>The --scheme name this theme answers to.</summary>
    public string Name { get; }

    /// <summary>Trace colours, cycled per channel. Also handed to <see cref="GraphConfig.Palette"/> so the display
    /// modes can colour their own series.</summary>
    public PlotPalette Palette { get; }

    /// <summary>Colour of tick numbers, header text and axis captions.</summary>
    public Color LabelsColor { get; }

    /// <summary>Colour of the axis lines, grid and tick marks.</summary>
    public Color AxisColor { get; }

    /// <summary>Pane fill, or <see langword="null"/> to leave the terminal's own background showing (scope-tui's
    /// behaviour).</summary>
    public Color? BackgroundColor { get; }

    /// <summary>At-rest pane border, or <see langword="null"/> for the default theme's.</summary>
    public Color? BorderColor { get; }
    #endregion

    #region IStyleTheme
    // Color converts implicitly to Style (as a foreground), which is how the built-in tokens are written too.

    /// <summary>Axis lines take <see cref="AxisColor"/>.</summary>
    Style IStyleTheme.PlotAxis => AxisColor;

    /// <summary>The grid is the same structure as the axis, so it shares its colour.</summary>
    Style IStyleTheme.PlotGrid => AxisColor;

    /// <summary>Tick marks group with the axis...</summary>
    Style IStyleTheme.PlotTick => AxisColor;

    /// <summary>...while the tick numbers group with the other text.</summary>
    Style IStyleTheme.PlotTickLabel => LabelsColor;

    /// <summary>The pane fill; <see cref="Style.Plain"/> (no background) when the scheme has none.</summary>
    Style IStyleTheme.PlotSurface => BackgroundColor is { } bg ? Style.Bg(bg) : Style.Plain;

    /// <summary>Trace colours for series added without an explicit colour.</summary>
    PlotPalette IStyleTheme.PlotSeries => Palette;

    /// <summary>A pane's at-rest border. Falls back to the interface default when the scheme names none.</summary>
    Style IStyleTheme.BorderText => BorderColor is { } border ? border : Style.Grey50;

    /// <summary>Header label text.</summary>
    Style IStyleTheme.Text => LabelsColor;

    /// <summary>The header strip: label text in <see cref="LabelsColor"/> over the pane fill, so the strip repaints
    /// with the rest of the pane on a theme switch instead of stranding dark text on a dark row.</summary>
    Style IStyleTheme.LabelText =>
        BackgroundColor is { } bg ? LabelsColor | Style.Bg(bg) : (Style)LabelsColor;
    #endregion

    #region Methods
    /// <summary>Scheme names accepted by --scheme, for the CLI's own validation and help text.</summary>
    public static string[] Names => [.. All.Select(t => t.Name)];

    /// <summary>Looks a scheme up by name (case-insensitive), falling back to <see cref="ScopeTui"/>.</summary>
    public static ScopeTheme FromName(string? name) =>
        All.FirstOrDefault(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase)) ?? ScopeTui;

    /// <summary>The next scheme in the list, wrapping — what the runtime Ctrl+T cycle steps through.</summary>
    public ScopeTheme Next() => All[(Array.IndexOf(All, this) + 1) % All.Length];
    #endregion

    #region Fields
    /// <summary>scope-tui's own defaults: red/yellow/green/magenta traces on whatever the terminal already is.</summary>
    public static readonly ScopeTheme ScopeTui = new(
        "scope-tui",
        new PlotPalette([Color.Red1, Color.Yellow1, Color.Green1, Color.Magenta1]),
        new(90, 220, 200),
        new(70, 100, 140),
        background: null,
        border: null);

    /// <summary>Hardware-scope phosphor: a bright green primary trace and an amber second, on near-black with a
    /// dim green-grey graticule -- the dual-trace CRT look.</summary>
    public static readonly ScopeTheme Phosphor = new(
        "phosphor",
        new PlotPalette([new(60, 255, 110), new(255, 190, 60), new(120, 240, 255), new(255, 120, 120)]),
        new(190, 235, 190),
        new(60, 80, 62),
        new(8, 10, 8),
        new(70, 110, 75));

    /// <summary>Multi-channel DAW meter: saturated, well-separated per-channel hues on near-black with a fine dark
    /// graticule, so overlaid traces stay tellable apart.</summary>
    public static readonly ScopeTheme Multi = new(
        "multi",
        new PlotPalette([new(255, 156, 51), new(72, 200, 96), new(64, 200, 224), new(110, 140, 255)]),
        new(168, 180, 198),
        new(52, 56, 66),
        new(16, 18, 24),
        new(70, 76, 90));

    /// <summary>Plugin-UI daylight: dark traces on light steel blue. The only LIGHT scheme, so it is the one that
    /// depends on the header and frame being painted from the theme too.</summary>
    public static readonly ScopeTheme Daylight = new(
        "daylight",
        new PlotPalette([new(26, 32, 40), new(64, 92, 124), new(132, 62, 62), new(56, 104, 76)]),
        new(58, 80, 104),
        new(150, 175, 195),
        new(198, 218, 232),
        new(120, 148, 172));

    static readonly ScopeTheme[] All = [ScopeTui, Phosphor, Multi, Daylight];
    #endregion
}
