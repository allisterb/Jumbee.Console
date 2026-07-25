namespace ScopeTui;

using Jumbee.Console;

/// <summary>
/// A colour scheme for the scope panes: the per-channel trace palette plus the axis/label/background colours,
/// selected with --scheme.
/// </summary>
/// <remarks>
/// The first three fields are exactly scope-tui's <c>--palette-color</c>, <c>--labels-color</c> and
/// <c>--axis-color</c> (cfg.rs), so a scheme is really just a named preset of the <see cref="GraphConfig"/> colours
/// that were already there. <see cref="Background"/> is ours and has no scope-tui counterpart -- ratatui draws on
/// the terminal's own background, which is why every scope-tui screenshot is whatever colour your terminal is.
/// <para/>A scheme with a <see cref="Background"/> has to paint more than the plot: the header row's labels and the
/// pane's frame are separate surfaces, and leaving them at the terminal default would strand a light plot inside a
/// dark terminal (and, worse, put dark label text on a dark strip). <see cref="ScopeView"/> and Program.cs colour
/// those from <see cref="Background"/>/<see cref="AxisColor"/> for exactly that reason.
/// </remarks>
public sealed record ScopeScheme(
    string Name,
    Color[] Palette,
    Color LabelsColor,
    Color AxisColor,
    Color? Background,
    Color? BorderColor)
{
    /// <summary>scope-tui's own defaults: red/yellow/green/magenta traces on whatever the terminal already is.</summary>
    public static readonly ScopeScheme ScopeTui = new(
        "scope-tui",
        [Color.Red1, Color.Yellow1, Color.Green1, Color.Magenta1],
        new(90, 220, 200),
        new(70, 100, 140),
        Background: null,
        BorderColor: null);

    /// <summary>Hardware-scope phosphor: a bright green primary trace and an amber second, on near-black with a
    /// dim green-grey graticule -- the dual-trace CRT look.</summary>
    public static readonly ScopeScheme Phosphor = new(
        "phosphor",
        [new(60, 255, 110), new(255, 190, 60), new(120, 240, 255), new(255, 120, 120)],
        new(190, 235, 190),
        new(60, 80, 62),
        new(8, 10, 8),
        new(70, 110, 75));

    /// <summary>Multi-channel DAW meter: saturated, well-separated per-channel hues on near-black with a fine dark
    /// graticule, so overlaid traces stay tellable apart.</summary>
    public static readonly ScopeScheme Multi = new(
        "multi",
        [new(255, 156, 51), new(72, 200, 96), new(64, 200, 224), new(110, 140, 255)],
        new(168, 180, 198),
        new(52, 56, 66),
        new(16, 18, 24),
        new(70, 76, 90));

    /// <summary>Plugin-UI daylight: dark traces on light steel blue. The only LIGHT scheme, so it is the one that
    /// depends on the header/frame being painted too.</summary>
    public static readonly ScopeScheme Daylight = new(
        "daylight",
        [new(26, 32, 40), new(64, 92, 124), new(132, 62, 62), new(56, 104, 76)],
        new(58, 80, 104),
        new(150, 175, 195),
        new(198, 218, 232),
        new(120, 148, 172));

    static readonly ScopeScheme[] all = [ScopeTui, Phosphor, Multi, Daylight];

    /// <summary>Scheme names accepted by --scheme, for the CLI's own validation and help text.</summary>
    public static string[] Names => [.. all.Select(s => s.Name)];

    /// <summary>Looks a scheme up by name (case-insensitive), falling back to <see cref="ScopeTui"/>.</summary>
    public static ScopeScheme FromName(string? name) =>
        all.FirstOrDefault(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase)) ?? ScopeTui;

    /// <summary>Copies this scheme's colours onto a <see cref="GraphConfig"/>. Does not <c>Publish</c> — the caller
    /// does that once it has finished setting the config up.</summary>
    public void ApplyTo(GraphConfig config)
    {
        config.Palette = Palette;
        config.LabelsColor = LabelsColor;
        config.AxisColor = AxisColor;
    }
}
