namespace Jumbee.Console;

using System;
using System.Collections.Generic;
using System.Text;

using Spectre.Console;

using CColor = ConsoleGUI.Data.Color;

/// <summary>
/// A live multi-series time chart with a legend — a streaming line <see cref="Plot"/> on the left and a per-series
/// stat readout (name + current / delta / max / min) on the right, in the style of a monitoring "run chart".
/// </summary>
/// <remarks>
/// Add a series with <see cref="AddSeries"/> and feed it through the returned <see cref="RunSeries"/> handle
/// (<see cref="RunSeries.Push"/>); the data flows through a stationary strip axis (fixed X window) so the frame stays
/// put and the values scroll through it. Pure composition over <see cref="Plot"/> — it adds a legend, not a new
/// rendering path.
/// </remarks>
public class RunChart : CompositeControl
{
    #region Constructors
    /// <summary>Initializes a new empty <see cref="RunChart"/> (a streaming <see cref="Plot"/> with a docked legend).</summary>
    public RunChart()
    {
        _plot = new Plot();
        _plot.SetXRange(0, _window - 1);           // stationary strip axis; data scrolls through it
        _legend = new TextPanel { Width = LegendWidth };
        // Plot fills the left; the fixed-width legend docks on the right.
        SetContent(new DockPanel(DockedControlPlacement.Right, _legend, _plot));
    }
    #endregion

    #region Properties
    /// <summary>Numeric format for the legend's stat values (default <c>"0.##"</c>).</summary>
    public string ValueFormat
    {
        get => _format;
        set => UI.Invoke(() => { _format = string.IsNullOrEmpty(value) ? "0.##" : value; RefreshLegend(); });
    }

    // Deliberately NOT IScrollable: the chart owns its scrolling via the strip axis, so it is sized to the viewport
    // rather than scrolled.

    internal int Window => _window;
    #endregion

    #region Methods
    /// <summary>Adds a series drawn in <paramref name="color"/> and returns a handle to feed it. Starts empty.</summary>
    public RunSeries AddSeries(string name, Color color)
    {
        var handle = new RunSeries(this, name ?? "", color, _plot.AddLiveSeries(color));
        UI.Invoke(() => { _series.Add(handle); RefreshLegend(); });
        return handle;
    }

    /// <summary>Pins the vertical axis to a fixed range so streaming moves only the data, not the axis.</summary>
    public RunChart SetYRange(double min, double max) { _plot.SetYRange(min, max); return this; }

    /// <summary>Restores auto-scaling of the vertical axis to the data.</summary>
    public RunChart AutoRangeY() { _plot.AutoRangeY(); return this; }

    /// <summary>Sets the strip width — how many recent points are shown before the oldest scrolls off (default 60).</summary>
    public RunChart SetXWindow(int points)
    {
        UI.Invoke(() => { _window = Math.Max(2, points); _plot.SetXRange(0, _window - 1); });
        return this;
    }

    // Coalesces legend rebuilds: many Push calls in one tick (e.g. three series pushed per frame) each dirty the
    // legend, but with the frame loop running only one rebuild runs on the next dispatcher drain — sparing the
    // repeated string build and TextPanel layout re-measure. Headless (no loop to drain) it rebuilds synchronously
    // so a render that immediately follows a Push is current. UI-thread only, so the flag needs no synchronization.
    internal void InvalidateLegend()
    {
        if (!UI.IsRunning) { RefreshLegend(); return; }
        if (_legendDirty) return;
        _legendDirty = true;
        UI.Post(() => { _legendDirty = false; RefreshLegend(); });
    }

    // Rebuilds the legend markup from the current per-series stats. Runs on the UI thread (callers marshal). Reuses a
    // cached builder and span-formats values so a per-frame refresh allocates only the final markup string.
    internal void RefreshLegend()
    {
        var sb = _legendBuilder;
        sb.Clear();
        bool first = true;
        foreach (var s in _series)
        {
            if (!first) sb.Append('\n');   // separate entries without a trailing newline to trim off the end
            first = false;
            var c = s.Color;
            sb.Append("[#");
            AppendHex(sb, c.Red); AppendHex(sb, c.Green); AppendHex(sb, c.Blue);
            sb.Append("]● ").Append(Markup.Escape(s.Name)).Append("[/]\n");
            sb.Append(" cur "); AppendValue(sb, s.Current); sb.Append("  dlt "); AppendValue(sb, s.Delta); sb.Append('\n');
            sb.Append(" max "); AppendValue(sb, s.Max); sb.Append("  min "); AppendValue(sb, s.Min);
        }
        _legend.Markup = sb.ToString();
    }

    private static void AppendHex(StringBuilder sb, byte b)
    {
        const string hex = "0123456789ABCDEF";
        sb.Append(hex[b >> 4]).Append(hex[b & 0xF]);
    }

    private void AppendValue(StringBuilder sb, double v)
    {
        if (!double.IsFinite(v)) { sb.Append('-'); return; }
        Span<char> buf = stackalloc char[32];
        if (v.TryFormat(buf, out int n, _format))   // null provider = current culture, matching double.ToString(format)
            sb.Append(buf[..n]);
        else
            sb.Append(v.ToString(_format));
    }
    #endregion

    #region Fields
    private const int LegendWidth = 22;
    private readonly Plot _plot;
    private readonly TextPanel _legend;
    private readonly List<RunSeries> _series = new();
    private readonly StringBuilder _legendBuilder = new();
    private bool _legendDirty;
    private int _window = 60;
    private string _format = "0.##";
    #endregion
}

/// <summary>
/// A handle to one <see cref="RunChart"/> series: push values with <see cref="Push"/>. Tracks the current value, the
/// delta from the previous value, and the running max/min shown in the chart's legend.
/// </summary>
/// <remarks>Thread-safe (each push is marshaled onto the UI thread).</remarks>
public sealed class RunSeries
{
    #region Constructors
    internal RunSeries(RunChart chart, string name, CColor color, PlotSeries plot)
    {
        _chart = chart;
        _plot = plot;
        Name = name;
        Color = color;
    }
    #endregion

    #region Properties
    internal string Name { get; }
    internal CColor Color { get; }
    internal double Current { get; private set; }
    internal double Delta { get; private set; }
    internal double Max { get; private set; } = double.PositiveInfinity;   // sentinels until the first push
    internal double Min { get; private set; } = double.NegativeInfinity;
    #endregion

    #region Methods
    /// <summary>Appends <paramref name="value"/>: scrolls it into the strip chart and updates cur/dlt/max/min.</summary>
    public void Push(double value) => UI.Invoke(() =>
    {
        Delta = _has ? value - Current : 0;
        Current = value;
        Max = _has ? Math.Max(Max, value) : value;
        Min = _has ? Math.Min(Min, value) : value;
        _has = true;
        _plot.Scroll(value, _chart.Window);   // fixed strip positions 0..window-1
        _chart.InvalidateLegend();
    });
    #endregion

    #region Fields
    private readonly RunChart _chart;
    private readonly PlotSeries _plot;
    private bool _has;
    #endregion
}
