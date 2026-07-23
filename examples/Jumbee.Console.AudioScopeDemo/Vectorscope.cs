namespace ScopeTui;

using Jumbee.Console;

/// <summary>
/// Faithful port of scope-tui's Vectorscope display mode (display/vectorscope.rs): plots stereo pairs (L[i], R[i])
/// as a Lissajous figure in a square [-scale, scale]^2 plot, with a crosshair through the origin as its reference
/// geometry. Has no hotkey-driven knobs of its own — <see cref="Snapshot"/> is <see langword="null"/> — so, unlike
/// <see cref="Oscilloscope"/>, every member here is directly a pure function of (<see cref="GraphSnapshot"/>, the
/// channel matrix); nothing needs a per-instance state capture before crossing to a background thread. Likewise
/// has no cross-frame state, so <see cref="IDisplayMode.Process"/>'s <c>NextState</c> is always <see langword="null"/>,
/// and <see cref="IDisplayMode.HandleKey"/> always returns <see langword="false"/> (no mode-specific keys).
/// </summary>
public sealed class Vectorscope : IDisplayMode
{
    public const string ModeStr = "vector";

    /// <summary>Mirrors Vectorscope::channel_name — plain numeric index, unlike Oscilloscope's "L"/"R".</summary>
    public static string ChannelName(int index) => index.ToString();

    /// <summary>Mirrors Vectorscope::header — always "live" (no trigger concept for a Lissajous figure).</summary>
    public static string Header() => "live";

    /// <summary>Mirrors Vectorscope::references — a horizontal + vertical centerline through the origin.</summary>
    public static List<Series> References(GraphSnapshot g) =>
    [
        new(null, [-g.Scale, g.Scale], [0.0, 0.0], Scatter: false, g.AxisColor),
        new(null, [0.0, 0.0], [-g.Scale, g.Scale], Scatter: false, g.AxisColor),
    ];

    /// <summary>
    /// Mirrors Vectorscope::process: pairs channels up two at a time (chunks(2) in the Rust source) and plots
    /// (chunk[0][i], chunk[1][i]) as one Lissajous point per sample; a lone trailing channel falls back to
    /// (value, index). Each pair's points are split in half for two-colour rendering, exactly as scope-tui does
    /// ("split it in two for easier coloring").
    /// </summary>
    public static List<Series> Process(GraphSnapshot g, double[][] data)
    {
        var outSeries = new List<Series>();
        for (var n = 0; n < data.Length; n += 2)
        {
            List<(double x, double y)> tmp = [];
            if (n + 1 < data.Length)
            {
                var a = data[n];
                var b = data[n + 1];
                var count = int.Min(a.Length, b.Length);
                for (var i = 0; i < count && i <= g.Samples; i++) tmp.Add((a[i], b[i]));
            }
            else
            {
                var a = data[n];
                for (var i = 0; i < a.Length && i <= g.Samples; i++) tmp.Add((a[i], i));
            }

            var pivot = tmp.Count / 2;
            var half1 = tmp[pivot..];
            var half0 = tmp[..pivot];
            outSeries.Add(new Series(ChannelName(n * 2 + 1), [.. half1.Select(p => p.x)], [.. half1.Select(p => p.y)],
                g.Scatter, g.PaletteFor(n * 2 + 1)));
            outSeries.Add(new Series(ChannelName(n * 2), [.. half0.Select(p => p.x)], [.. half0.Select(p => p.y)],
                g.Scatter, g.PaletteFor(n * 2)));
        }

        return outSeries;
    }

    #region IDisplayMode
    string IDisplayMode.ModeStr => ModeStr;
    (string X, string Y) IDisplayMode.AxisCaptions(object? modeState) => ("left -", "| right");
    string IDisplayMode.ChannelName(int index) => ChannelName(index);
    object? IDisplayMode.Snapshot() => null;
    string IDisplayMode.Header(object? modeState) => Header();
    (double XMin, double XMax, double YMin, double YMax) IDisplayMode.AxisBounds(GraphSnapshot g, object? modeState) =>
        (-g.Scale, g.Scale, -g.Scale, g.Scale);
    IReadOnlyList<Series> IDisplayMode.References(GraphSnapshot g) => References(g);
    (IReadOnlyList<Series> Series, object? NextState) IDisplayMode.Process(GraphSnapshot g, object? modeState, object? priorState, double[][] channels) =>
        (Process(g, channels), null);
    bool IDisplayMode.HandleKey(ConsoleKeyInfo key, double magnitude) => false;
    #endregion
}
