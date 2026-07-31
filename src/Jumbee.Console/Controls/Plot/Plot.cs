namespace Jumbee.Console;

using System;
using System.Collections.Generic;
using System.Linq;

using ConsoleGUI.Space;

using ConsolePlot.Drawing.Tools;
using ConsolePlot.Plotting;

using CPlot = ConsolePlot.Plot;
using CColor = ConsoleGUI.Data.Color;

/// <summary>
/// A line/scatter chart backed by the ConsolePlot library, rendered into the control's buffer. Add data with
/// <see cref="AddSeries(double[], double[], PointPen)"/> and tune the axes/grid/ticks with the <c>Configure*</c> methods.
/// </summary>
/// <remarks>
/// The plot fills its container and re-draws to fit whenever the control is resized; all configuration is replayed
/// on each rebuild, so settings survive resizing.
/// <para>For data that changes every frame (a live scope, a streaming chart), add the series ONCE with
/// <see cref="AddLiveSeries"/> and feed it via the returned <see cref="PlotSeries"/> handle
/// (<c>SetData</c>/<c>Push</c>) rather than rebuilding with <see cref="Clear"/> + <see cref="AddSeries(double[], double[], PointPen)"/> each frame —
/// the live path mutates the data in place without re-allocating the plot, and it keeps your <c>Configure*</c> styling
/// (which <see cref="Clear"/> would otherwise drop from the data list).</para>
/// </remarks>
public class Plot : Control
{
    #region Constructors
    /// <summary>Initializes a new display-only <see cref="Plot"/> (not focusable), with its chrome colours and
    /// series palette taken from the current style theme.</summary>
    public Plot()
    {
        Focusable = false;   // display-only
        CaptureTheme();

        // ONE chrome action, closing over `this` so it reads whatever the colour fields hold at replay time. The
        // alternative -- a fresh closure per recolour -- leaks: ConfigureChrome APPENDS to _chrome, _chrome is never
        // trimmed, and it is replayed in full on every rebuild. A live scope recolours as often as its header
        // changes (once a second, when the fps figure ticks over), so that list would grow without bound.
        ConfigureChrome(p =>
        {
            p.Axis.Pen = new LinePen(p.Axis.Pen.Brush, (CColor)_axisColor);
            p.Grid.Pen = new LinePen(p.Grid.Pen.Brush, (CColor)_gridColor);
            p.Ticks.Pen = new LinePen(p.Ticks.Pen.Brush, (CColor)_tickColor);
            p.Ticks.Labels.Color = (CColor)_tickLabelColor;
        });
    }
    #endregion

    #region Properties
    /// <summary>Background colour painted behind the plot, or <see langword="null"/> (the default) for transparent.
    /// Themed from <see cref="IStyleTheme.PlotSurface"/> until set explicitly.</summary>
    public Color? Background
    {
        get => _background;
        set => SetAtomicProperty(ref _background, value, themeOverride: true, watch: (_, _) => _dirty = true);
    }

    /// <summary>Colour of the axis lines. Themed from <see cref="IStyleTheme.PlotAxis"/> until set explicitly.</summary>
    public Color AxisColor
    {
        get => _axisColor;
        set => SetAtomicProperty(ref _axisColor, value, themeOverride: true, watch: (_, _) => Rebuild());
    }

    /// <summary>Colour of the background grid lines. Themed from <see cref="IStyleTheme.PlotGrid"/> until set
    /// explicitly.</summary>
    public Color GridColor
    {
        get => _gridColor;
        set => SetAtomicProperty(ref _gridColor, value, themeOverride: true, watch: (_, _) => Rebuild());
    }

    /// <summary>Colour of the tick marks. Themed from <see cref="IStyleTheme.PlotTick"/> until set explicitly.</summary>
    public Color TickColor
    {
        get => _tickColor;
        set => SetAtomicProperty(ref _tickColor, value, themeOverride: true, watch: (_, _) => Rebuild());
    }

    /// <summary>Colour of the numeric tick labels. Themed from <see cref="IStyleTheme.PlotTickLabel"/> until set
    /// explicitly.</summary>
    public Color TickLabelColor
    {
        get => _tickLabelColor;
        set => SetAtomicProperty(ref _tickLabelColor, value, themeOverride: true, watch: (_, _) => Rebuild());
    }

    /// <summary>
    /// Opts into partial redraw: the plot reports only the cells each draw actually changed, so the compositor can
    /// skip the rest of its rect. Off by default.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Worth turning on for a plot whose figure is <em>sparse and localised</em> — a live scope trace, a small
    /// marker set — where most of the area is empty or static from frame to frame. The grid, axes and tick labels
    /// are rewritten every draw but with identical values, so they cost nothing in damage; only the data actually
    /// moving is reported. A figure that fills its area (a dense heatmap) has nothing to skip and should leave this
    /// off, since the diff is then pure overhead.
    /// </para>
    /// <para>
    /// Measured on a 220×53 live scope: a trace at 15% of the Y range drops the composite from ~1340µs to ~270µs
    /// (11660 dirty cells to 1966), roughly halving the frame; a full-scale trace at 95% saves nothing and costs
    /// about 4%, because the recorder sits in the write path and is paid for whether or not the frame benefits.
    /// The break-even is worse than a cell count suggests — see <see cref="Control.TracksDamage"/> for why.
    /// </para>
    /// <para>
    /// It does not reduce terminal output: ANSI bytes per frame were unchanged (15620 vs 15739), since the renderer
    /// already diffs per cell before emitting. Terminal load is set by data density, not by this flag.
    /// </para>
    /// </remarks>
    public bool DamageTracking
    {
        get => _damageTracking;
        // Rebuild: the damage recorder is part of the buffer chain the plot draws through, so adding or removing it
        // means rebuilding the plot around a new chain.
        set => SetAtomicProperty(ref _damageTracking, value, watch: (_, _) => _dirty = true);
    }

    /// <summary>Colours cycled for series added without an explicit colour. Themed from
    /// <see cref="IStyleTheme.PlotSeries"/> until set explicitly.</summary>
    /// <remarks>Applies to series added <em>after</em> it changes: a series' colour is baked into its pen when it is
    /// added, so recolouring existing series means re-adding them.</remarks>
    public PlotPalette SeriesPalette
    {
        get => _seriesPalette;
        set => SetAtomicProperty(ref _seriesPalette, value, themeOverride: true, watch: (_, _) => Rebuild());
    }
    #endregion

    #region Methods
    /// <summary>Sets the <see cref="Background"/> colour and returns this plot, for fluent chaining.</summary>
    public Plot WithBackground(Color? background)
    {
        Background = background;
        return this;
    }

    /// <summary>
    /// Adds a line series — consecutive points joined by straight segments (use <see cref="AddScatter"/> for
    /// unconnected markers). <paramref name="xs"/> and <paramref name="ys"/> must be the same length.
    /// </summary>
    /// <remarks>
    /// When <paramref name="pen"/> is left at its default a colour is taken from the control's palette (cycling by
    /// series index) and drawn with the Braille brush.
    /// <para>For dense or high-frequency data (e.g. an audio waveform) prefer <see cref="AddScatter"/>: a line
    /// rasterizes a segment between every consecutive pair of points, which is markedly more expensive than plotting
    /// points independently.</para>
    /// </remarks>
    public Plot AddSeries(double[] xs, double[] ys, PointPen pen = default)
    {
        UI.Invoke(() =>
        {
            var p = pen.Equals(default(PointPen))
                ? new PointPen(SystemPointBrushes.Braille, SeriesColor(_seriesCount))
                : pen;
            AddElement(plot => plot.AddSeries(xs, ys, p));
        });
        return this;
    }

    /// <summary>
    /// Adds a line series (consecutive points joined by straight segments) drawn with the given
    /// <paramref name="brush"/>. For unconnected markers use <see cref="AddScatter"/>.
    /// </summary>
    /// <remarks>
    /// The <paramref name="brush"/>'s sub-cell resolution — Braille 2×4, Quadrant 2×2, the rest 1×1 — sets how smooth
    /// the line looks. When <paramref name="color"/> is <see langword="null"/> a colour is taken from the control's
    /// palette, cycling by series index.
    /// <para>For dense or high-frequency data prefer <see cref="AddScatter"/> — see the note there.</para>
    /// </remarks>
    public Plot AddSeries(double[] xs, double[] ys, PlotBrush brush, Color? color = null)
    {
        UI.Invoke(() =>
        {
            var pen = new PointPen(BrushFor(brush), (CColor?)color ?? SeriesColor(_seriesCount));
            AddElement(plot => plot.AddSeries(xs, ys, pen));
        });
        return this;
    }

    /// <summary>
    /// Adds a scatter series — the points drawn as markers, without connecting lines.
    /// </summary>
    /// <remarks>
    /// The <paramref name="brush"/> sets the marker (and its sub-cell resolution); <paramref name="color"/> defaults
    /// to the palette.
    /// <para>Scatter is also markedly cheaper to draw than a line series (<see cref="AddSeries(double[], double[], PointPen)"/>)
    /// for dense or high-frequency data such as an audio waveform: a line rasterizes a segment between every
    /// consecutive pair of points, whereas scatter plots each point on its own. When the point count is high and the
    /// connecting lines add little, prefer scatter for a large drawing-cost win.</para>
    /// </remarks>
    public Plot AddScatter(double[] xs, double[] ys, PlotBrush brush = PlotBrush.Braille, Color? color = null)
    {
        UI.Invoke(() =>
        {
            var pen = new PointPen(BrushFor(brush), (CColor?)color ?? SeriesColor(_seriesCount));
            AddElement(plot => plot.AddScatter(xs, ys, pen));
        });
        return this;
    }

    /// <summary>
    /// Adds a stem series — a vertical line from <paramref name="baseline"/> (default 0) to each point, capped with
    /// a dot marker.
    /// </summary>
    /// <remarks><paramref name="color"/> defaults to the palette.</remarks>
    public Plot AddStem(double[] xs, double[] ys, Color? color = null, double baseline = 0)
    {
        UI.Invoke(() =>
        {
            var pen = new PointPen(SystemPointBrushes.Dot, (CColor?)color ?? SeriesColor(_seriesCount));
            AddElement(plot => plot.AddStem(xs, ys, pen, baseline));
        });
        return this;
    }

    /// <summary>
    /// Adds a vertical bar series — each point drawn as a filled bar from <paramref name="baseline"/> (default 0) to
    /// its value, with an eighth-block sub-cell top.
    /// </summary>
    /// <remarks>
    /// <paramref name="color"/> defaults to the palette; <paramref name="width"/> is the bar width as a fraction
    /// (0..1) of the spacing between bars.
    /// <para>
    /// Bars render as solid blocks — there is no <see cref="PlotBrush"/> parameter, so they cannot be drawn in
    /// braille. For a sub-cell <b>filled</b> or area look (the <c>drawille</c> style used by terminal system
    /// monitors), use <see cref="Canvas"/> with <see cref="Drawing.CanvasMarker.Braille"/> and one
    /// <see cref="Drawing.FilledLine"/> per column — with <c>using Jumbee.Console.Drawing;</c>,
    /// <c>canvas.Add(new FilledLine(x, 0, x, value, 0, color))</c>. That fills each column from the baseline at
    /// exact sub-cell resolution, one shape per column.
    /// </para>
    /// </remarks>
    /// <seealso cref="Drawing.FilledLine"/>
    public Plot AddBars(double[] xs, double[] ys, Color? color = null, double baseline = 0, double width = 0.8)
    {
        UI.Invoke(() =>
        {
            var c = (CColor?)color ?? SeriesColor(_seriesCount);
            AddElement(plot => plot.AddBars(xs, ys, c, baseline, width));
        });
        return this;
    }

    /// <summary>
    /// Adds a histogram of <paramref name="values"/> — the values are binned and each bin drawn as a touching bar
    /// (bar height = bin count).
    /// </summary>
    /// <remarks>
    /// <paramref name="bins"/> ≤ 0 picks a bin count automatically (√n, clamped); <paramref name="color"/> defaults
    /// to the palette.
    /// </remarks>
    public Plot AddHistogram(double[] values, int bins = 0, Color? color = null)
    {
        UI.Invoke(() =>
        {
            var (mids, counts) = Histogram(values, bins);
            if (mids.Length == 0) return;
            var c = (CColor?)color ?? SeriesColor(_seriesCount);
            // Width 1.0 so adjacent bins touch, as a histogram should.
            AddElement(plot => plot.AddBars(mids, counts, c, 0, 1.0));
        });
        return this;
    }

    // Bins the finite values into equal-width buckets, returning each bin's midpoint (x) and count (bar height).
    private static (double[] mids, double[] counts) Histogram(double[] values, int bins)
    {
        var finite = values.Where(v => !double.IsNaN(v) && !double.IsInfinity(v)).ToArray();
        if (finite.Length == 0) return ([], []);

        double min = finite.Min(), max = finite.Max();
        if (bins <= 0) bins = Math.Clamp((int)Math.Ceiling(Math.Sqrt(finite.Length)), 1, 60);

        // All values equal: a single bar at that value.
        if (max <= min) return ([min], [finite.Length]);

        double width = (max - min) / bins;
        var counts = new double[bins];
        foreach (var v in finite)
            counts[Math.Clamp((int)((v - min) / width), 0, bins - 1)]++;   // the max value lands in the last bin

        var mids = new double[bins];
        for (int b = 0; b < bins; b++) mids[b] = min + (b + 0.5) * width;
        return (mids, counts);
    }

    /// <summary>
    /// Adds an OHLC candlestick series — each point drawn as a candle (high/low wick + open/close body) coloured by
    /// direction.
    /// </summary>
    /// <remarks><paramref name="up"/> defaults to green (close ≥ open), <paramref name="down"/> to red.</remarks>
    public Plot AddCandles(
        double[] xs, double[] opens, double[] highs,
        double[] lows, double[] closes, Color? up = null, Color? down = null)
    {
        UI.Invoke(() =>
        {
            var u = (CColor?)up ?? new CColor(80, 200, 120);
            var d = (CColor?)down ?? new CColor(230, 90, 90);
            AddElement(plot => plot.AddCandles(xs, opens, highs, lows, closes, u, d));
        });
        return this;
    }

    /// <summary>
    /// Adds a box-and-whisker series from the five-number summary of each box — <paramref name="mins"/>,
    /// <paramref name="q1s"/>, <paramref name="medians"/>, <paramref name="q3s"/>, <paramref name="maxes"/> (all the
    /// same length as <paramref name="xs"/>).
    /// </summary>
    /// <remarks>
    /// <paramref name="color"/> defaults to the palette; <paramref name="medianColor"/> defaults to
    /// <paramref name="color"/>; <paramref name="width"/> is the box width as a fraction (0..1) of the spacing.
    /// </remarks>
    public Plot AddBox(
        double[] xs, double[] mins, double[] q1s,
        double[] medians, double[] q3s, double[] maxes,
        Color? color = null, Color? medianColor = null, double width = 0.6)
    {
        UI.Invoke(() =>
        {
            var c = (CColor?)color ?? SeriesColor(_seriesCount);
            var m = (CColor?)medianColor ?? c;
            AddElement(plot => plot.AddBox(xs, mins, q1s, medians, q3s, maxes, c, m, width));
        });
        return this;
    }

    /// <summary>
    /// Adds a box-and-whisker series from raw data <paramref name="groups"/> — one box per group, with the quartiles
    /// (min/Q1/median/Q3/max, linear-interpolation percentiles) computed here.
    /// </summary>
    /// <remarks>
    /// Boxes are positioned at <paramref name="positions"/> (defaults to 1, 2, 3, …). <paramref name="color"/>
    /// defaults to the palette.
    /// </remarks>
    public Plot AddBoxes(
        double[][] groups, double[]? positions = null,
        Color? color = null, Color? medianColor = null, double width = 0.6)
    {
        UI.Invoke(() =>
        {
            var xs = new List<double>();
            var mins = new List<double>();
            var q1s = new List<double>();
            var medians = new List<double>();
            var q3s = new List<double>();
            var maxes = new List<double>();
            for (int i = 0; i < groups.Length; i++)
            {
                if (!Quartiles(groups[i], out var min, out var q1, out var med, out var q3, out var max))
                    continue;
                xs.Add(positions is not null && i < positions.Length ? positions[i] : i + 1);
                mins.Add(min); q1s.Add(q1); medians.Add(med); q3s.Add(q3); maxes.Add(max);
            }

            if (xs.Count == 0) return;
            var c = (CColor?)color ?? SeriesColor(_seriesCount);
            var m = (CColor?)medianColor ?? c;
            AddElement(plot => plot.AddBox(xs, mins, q1s, medians, q3s, maxes, c, m, width));
        });
        return this;
    }

    // The five-number summary of the finite values (linear-interpolation percentiles, numpy's default), or false
    // when there are no finite values.
    private static bool Quartiles(double[] values, out double min, out double q1, out double median, out double q3, out double max)
    {
        min = q1 = median = q3 = max = 0;
        var sorted = values.Where(v => !double.IsNaN(v) && !double.IsInfinity(v)).ToArray();
        if (sorted.Length == 0) return false;
        Array.Sort(sorted);

        min = sorted[0];
        max = sorted[^1];
        q1 = Percentile(sorted, 0.25);
        median = Percentile(sorted, 0.50);
        q3 = Percentile(sorted, 0.75);
        return true;

        static double Percentile(double[] s, double p)
        {
            if (s.Length == 1) return s[0];
            double rank = p * (s.Length - 1);
            int lo = (int)Math.Floor(rank);
            int hi = (int)Math.Ceiling(rank);
            return s[lo] + (s[hi] - s[lo]) * (rank - lo);
        }
    }

    /// <summary>
    /// Adds vertical error bars with symmetric error — each point (<paramref name="xs"/>, <paramref name="ys"/>)
    /// drawn as a whisker of ±<paramref name="errors"/> with caps and a centre marker.
    /// </summary>
    /// <remarks>
    /// <paramref name="color"/> defaults to the palette; <paramref name="capWidth"/> is the cap half-width in cells.
    /// </remarks>
    public Plot AddErrorBars(double[] xs, double[] ys, double[] errors, Color? color = null, int capWidth = 1) =>
        AddErrorBars(xs, ys, errors, errors, color, capWidth);

    /// <summary>
    /// Adds vertical error bars with asymmetric error — each point (<paramref name="xs"/>, <paramref name="ys"/>)
    /// drawn as a whisker from <c>y − errLow</c> to <c>y + errHigh</c> with caps and a centre marker.
    /// </summary>
    /// <remarks>
    /// <paramref name="color"/> defaults to the palette; <paramref name="capWidth"/> is the cap half-width in cells.
    /// </remarks>
    public Plot AddErrorBars(
        double[] xs, double[] ys, double[] errLows, double[] errHighs,
        Color? color = null, int capWidth = 1)
    {
        UI.Invoke(() =>
        {
            var c = (CColor?)color ?? SeriesColor(_seriesCount);
            AddElement(plot => plot.AddErrorBars(xs, ys, errLows, errHighs, c, capWidth));
        });
        return this;
    }

    /// <summary>
    /// Adds grouped (side-by-side) vertical bars — one sub-bar per series at each x.
    /// </summary>
    /// <remarks>
    /// <paramref name="series"/> is one value list per series (each the same length as <paramref name="xs"/>).
    /// <paramref name="colors"/> defaults to the palette (one per series); <paramref name="width"/> is the group width
    /// as a fraction (0..1) of the spacing.
    /// </remarks>
    public Plot AddGroupedBars(
        double[] xs, double[][] series,
        IReadOnlyList<Color>? colors = null, double baseline = 0, double width = 0.8)
    {
        UI.Invoke(() =>
        {
            var cs = ColorsFor(series.Length, colors);
            AddElement(plot => plot.AddGroupedBars(xs, series, cs, baseline, width));
        });
        return this;
    }

    /// <summary>
    /// Adds stacked vertical bars — the series stacked from <paramref name="baseline"/> at each x.
    /// </summary>
    /// <remarks>
    /// <paramref name="series"/> is one value list per series (each the same length as <paramref name="xs"/>).
    /// <paramref name="colors"/> defaults to the palette (one per series).
    /// </remarks>
    public Plot AddStackedBars(
        double[] xs, double[][] series,
        IReadOnlyList<Color>? colors = null, double baseline = 0, double width = 0.8)
    {
        UI.Invoke(() =>
        {
            var cs = ColorsFor(series.Length, colors);
            AddElement(plot => plot.AddStackedBars(xs, series, cs, baseline, width));
        });
        return this;
    }

    /// <summary>
    /// Adds horizontal bars — each category at a Y coordinate from <paramref name="positions"/> with its bar growing
    /// along X from <paramref name="baseline"/> to its value.
    /// </summary>
    /// <remarks>
    /// <paramref name="color"/> defaults to the palette; <paramref name="width"/> is the bar thickness as a fraction
    /// (0..1) of the spacing.
    /// </remarks>
    public Plot AddHBars(double[] positions, double[] values, Color? color = null, double baseline = 0, double width = 0.8)
    {
        UI.Invoke(() =>
        {
            var c = (CColor?)color ?? SeriesColor(_seriesCount);
            AddElement(plot => plot.AddHBars(positions, values, c, baseline, width));
        });
        return this;
    }

    /// <summary>
    /// Adds a heatmap: a grid of <paramref name="values"/> (one list per row, row 0 drawn at the top) tiled over
    /// the plot area, each cell coloured by <paramref name="colormap"/>.
    /// </summary>
    /// <remarks>
    /// Values are normalised into [<paramref name="min"/>, <paramref name="max"/>], defaulting to the data's own
    /// min/max. NaN cells are blank. Pass <paramref name="cellText"/> to draw each cell's value as centred text
    /// (readable-contrast on the cell colour) — e.g. <c>v =&gt; ((int)v).ToString()</c> for a confusion matrix.
    /// </remarks>
    public Plot AddHeatmap(
        double[][] values, PlotColormap colormap = PlotColormap.Viridis,
        double? min = null, double? max = null, Func<double, string>? cellText = null)
    {
        UI.Invoke(() =>
        {
            int rows = values.Length;
            if (rows == 0) return;
            int cols = values[0].Length;
            if (cols == 0) return;

            double dataMin = double.PositiveInfinity, dataMax = double.NegativeInfinity;
            foreach (var row in values)
                foreach (var v in row)
                    if (!double.IsNaN(v) && !double.IsInfinity(v))
                    {
                        if (v < dataMin) dataMin = v;
                        if (v > dataMax) dataMax = v;
                    }
            if (double.IsInfinity(dataMin)) return;   // no finite values

            double lo = min ?? dataMin, hi = max ?? dataMax;
            var map = ColormapFunc(colormap);
            // The grid tiles the unit-per-cell rectangle 0..cols × 0..rows; use Configure* to relabel the axes.
            // ConsolePlot's cellText param is nullable-oblivious (declared Func<,> with a null default) — passing null
            // means "no cell labels", which is the intent here; null! states that to the compiler.
            AddElement(plot => plot.AddHeatmap(values, 0, cols, 0, rows, lo, hi, v => map(v), cellText is null ? null! : v => cellText(v)));
        });
        return this;
    }

    /// <summary>
    /// Adds a confusion matrix — an annotated heatmap of <paramref name="counts"/> (row = actual class top-to-bottom,
    /// column = predicted class), each cell coloured by <paramref name="colormap"/> and labelled with its count.
    /// </summary>
    /// <remarks>
    /// When <paramref name="rowLabels"/>/<paramref name="colLabels"/> are given, the class names are placed as
    /// categorical axis ticks at the cell centres. A wrapper over <see cref="AddHeatmap"/> +
    /// <see cref="SetXTicks"/>/<see cref="SetYTicks"/>.
    /// </remarks>
    public Plot AddConfusionMatrix(
        double[][] counts, string[]? rowLabels = null,
        string[]? colLabels = null, PlotColormap colormap = PlotColormap.Heat)
    {
        AddHeatmap(counts, colormap, cellText: v => ((long)Math.Round(v)).ToString());

        int rows = counts.Length;
        int cols = rows > 0 ? counts[0].Length : 0;
        // The grid tiles 0..cols × 0..rows with row 0 at the top, so column c's centre is at x = c+0.5 and row r's
        // centre is at y = rows−r−0.5 (image y is up).
        if (colLabels is not null && cols > 0)
            SetXTicks([.. Enumerable.Range(0, cols).Select(c => (c + 0.5, c < colLabels.Length ? colLabels[c] : ""))]);
        if (rowLabels is not null && rows > 0)
            SetYTicks([.. Enumerable.Range(0, rows).Select(r => (rows - r - 0.5, r < rowLabels.Length ? rowLabels[r] : ""))]);
        // Categorical ticks keep the grid at exact bounds (edge to edge), so the labels need a reserved margin
        // rather than being attached to the axis inside the grid.
        if (rowLabels is not null || colLabels is not null)
            ConfigureTicks(t => t.Labels.AttachToAxis = false);
        return this;
    }

    /// <summary>
    /// Pins the vertical axis to a fixed <paramref name="min"/>..<paramref name="max"/> range.
    /// </summary>
    /// <remarks>
    /// Live updates then move only the data (values outside the range are clipped) instead of the axis rescaling to
    /// the data each frame. Call once before streaming; <see cref="AutoRangeY"/> restores auto-scaling.
    /// </remarks>
    public Plot SetYRange(double min, double max) => Configure(p => p.FixedYRange = (min, max));

    /// <summary>Pins the horizontal axis to a fixed range; see <see cref="SetYRange"/>.</summary>
    public Plot SetXRange(double min, double max) => Configure(p => p.FixedXRange = (min, max));

    /// <summary>
    /// Makes the horizontal axis a sliding window of <paramref name="width"/> units — it shows the most recent
    /// <c>[max(0, dataMax − width), dataMax]</c> of a monotonic (time-like) series.
    /// </summary>
    /// <remarks>
    /// The axis only advances rightward and never shows x &lt; 0. Ideal for streaming/financial data.
    /// <see cref="AutoRangeX"/> restores auto.
    /// </remarks>
    public Plot SetXWindow(double width) => Configure(p => p.XWindow = width);

    /// <summary>Restores auto-scaling of the vertical axis to the data (undoes <see cref="SetYRange"/>).</summary>
    public Plot AutoRangeY() => Configure(p => p.FixedYRange = null);

    /// <summary>Restores auto-scaling of the horizontal axis (undoes <see cref="SetXRange"/>/<see cref="SetXWindow"/>).</summary>
    public Plot AutoRangeX() => Configure(p => { p.FixedXRange = null; p.XWindow = null; });

    /// <summary>Sets explicit horizontal-axis ticks (value + label) — e.g. categorical class names at cell centres.</summary>
    /// <remarks>
    /// Replaces the auto numeric ticks and keeps the data bounds unadjusted. For labels in a reserved margin (rather
    /// than attached inside the grid), pair with <c>ConfigureTicks(t =&gt; t.Labels.AttachToAxis = false)</c> —
    /// <see cref="AddConfusionMatrix"/> does this for you.
    /// </remarks>
    public Plot SetXTicks(IReadOnlyList<(double value, string label)> ticks) =>
        Configure(p => p.Ticks.CustomXTicks = ticks);

    /// <summary>Sets explicit vertical-axis ticks (value + label). See <see cref="SetXTicks"/>.</summary>
    public Plot SetYTicks(IReadOnlyList<(double value, string label)> ticks) =>
        Configure(p => p.Ticks.CustomYTicks = ticks);

    // Maps a colormap choice to a normalised-value → colour function.
    private static Func<double, CColor> ColormapFunc(PlotColormap colormap) => colormap switch
    {
        PlotColormap.Grayscale => t => Ramp(GrayscaleStops, t),
        PlotColormap.Heat => t => Ramp(HeatStops, t),
        PlotColormap.Cool => t => Ramp(CoolStops, t),
        _ => t => Ramp(ViridisStops, t),
    };

    // Piecewise-linear interpolation across evenly-spaced colour stops (t in [0, 1]).
    private static CColor Ramp(CColor[] stops, double t)
    {
        t = Math.Clamp(t, 0.0, 1.0);
        double scaled = t * (stops.Length - 1);
        int i = (int)Math.Floor(scaled);
        if (i >= stops.Length - 1) return stops[^1];
        double f = scaled - i;
        var a = stops[i];
        var b = stops[i + 1];
        return new CColor(
            (byte)(a.Red + (b.Red - a.Red) * f),
            (byte)(a.Green + (b.Green - a.Green) * f),
            (byte)(a.Blue + (b.Blue - a.Blue) * f));
    }

    private static readonly CColor[] ViridisStops =
    [
        new(68, 1, 84), new(59, 82, 139), new(33, 145, 140), new(94, 201, 98), new(253, 231, 37),
    ];
    private static readonly CColor[] HeatStops =
    [
        new(0, 0, 0), new(150, 0, 0), new(230, 90, 0), new(250, 200, 40), new(255, 255, 220),
    ];
    private static readonly CColor[] GrayscaleStops = [new(15, 15, 15), new(245, 245, 245)];
    private static readonly CColor[] CoolStops = [new(0, 220, 220), new(120, 120, 240), new(230, 60, 230)];

    // One colour per series: the caller's colours where given, else the palette cycled by series index. Instance
    // rather than static since the palette became per-plot (themed) state.
    private IReadOnlyList<CColor> ColorsFor(int count, IReadOnlyList<Color>? colors)
    {
        var result = new CColor[count];
        for (int j = 0; j < count; j++)
            result[j] = colors is not null && j < colors.Count ? (CColor)colors[j] : SeriesColor(j);
        return result;
    }

    /// <summary>
    /// Adds a text annotation anchored to the data point (<paramref name="x"/>, <paramref name="y"/>) — e.g. labelling
    /// a candle or data point.
    /// </summary>
    /// <remarks>
    /// <paramref name="fg"/> defaults to white; <paramref name="bg"/> is optional (transparent when null).
    /// <paramref name="dx"/>/<paramref name="dy"/> nudge the label in cells (dy &gt; 0 = above the point);
    /// <paramref name="align"/> anchors it horizontally. Does not rescale the axes.
    /// </remarks>
    public Plot AddLabel(double x, double y, string text, Color? fg = null, Color? bg = null, PlotLabelAlign align = PlotLabelAlign.Center, int dx = 0, int dy = 1)
    {
        UI.Invoke(() =>
        {
            var f = (CColor?)fg ?? CColor.White;
            CColor? b = bg;
            var a = align switch
            {
                PlotLabelAlign.Left => LabelAlignment.Left,
                PlotLabelAlign.Right => LabelAlignment.Right,
                _ => LabelAlignment.Center,
            };
            // A label is not a palette series, so it doesn't advance the colour cycle — add its config directly.
            _config.Add(plot => plot.AddLabel(x, y, text, f, b, a, dx, dy));
            Rebuild();
        });
        return this;
    }

    /// <summary>
    /// Adds a live <b>line</b> series (consecutive points joined) and returns a <see cref="PlotSeries"/> handle to
    /// feed it data as it arrives (<see cref="PlotSeries.SetData"/>/<see cref="PlotSeries.Push"/>). For unconnected
    /// markers use <see cref="AddLiveScatter"/>.
    /// </summary>
    /// <remarks>
    /// <paramref name="color"/> defaults to the palette; <paramref name="brush"/>'s sub-cell resolution (Braille 2×4,
    /// Quadrant 2×2, the rest 1×1) sets how smooth the line looks. Starts empty. For dense or high-frequency data
    /// (e.g. an audio waveform) prefer <see cref="AddLiveScatter"/> — see the drawing-cost note on <see cref="AddScatter"/>.
    /// </remarks>
    public PlotSeries AddLiveSeries(Color? color = null, PlotBrush brush = PlotBrush.Braille)
    {
        var pen = new PointPen(BrushFor(brush), (CColor?)color ?? SeriesColor(_seriesCount));
        var handle = new PlotSeries(this, (cplot, xs, ys) => cplot.AddSeries(xs, ys, pen));
        RegisterLive(handle);
        return handle;
    }

    /// <summary>
    /// Adds a live <b>scatter</b> series (points drawn as markers, no connecting lines) and returns a
    /// <see cref="PlotSeries"/> handle to feed it data as it arrives. The scatter counterpart of
    /// <see cref="AddLiveSeries"/>, so live streaming data and the cheaper marker draw compose.
    /// </summary>
    /// <remarks>
    /// <paramref name="color"/> defaults to the palette; <paramref name="brush"/> sets the marker (and its sub-cell
    /// resolution). Starts empty. Markers are markedly cheaper to draw than a line for dense/high-frequency data —
    /// see the note on <see cref="AddScatter"/>.
    /// </remarks>
    public PlotSeries AddLiveScatter(Color? color = null, PlotBrush brush = PlotBrush.Braille)
    {
        var pen = new PointPen(BrushFor(brush), (CColor?)color ?? SeriesColor(_seriesCount));
        var handle = new PlotSeries(this, (cplot, xs, ys) => cplot.AddScatter(xs, ys, pen));
        RegisterLive(handle);
        return handle;
    }

    /// <summary>
    /// Adds a live bar series and returns a <see cref="PlotSeries"/> handle.
    /// </summary>
    /// <remarks>
    /// Feed it with <see cref="PlotSeries.SetValues"/> (bars at x = 1, 2, 3, …) or <see cref="PlotSeries.SetData"/>.
    /// <paramref name="color"/> defaults to the palette. Starts empty.
    /// <para>
    /// Like <see cref="AddBars(double[], double[], Color?, double, double)"/>, these render as solid blocks and take
    /// no <see cref="PlotBrush"/>. For a live braille-filled column chart (a system-monitor style CPU/memory graph),
    /// drive a <see cref="Canvas"/> with one <see cref="Drawing.FilledLine"/> per sample instead.
    /// </para>
    /// </remarks>
    /// <seealso cref="Drawing.FilledLine"/>
    public PlotSeries AddLiveBars(Color? color = null, double baseline = 0, double width = 0.8)
    {
        var c = (CColor?)color ?? SeriesColor(_seriesCount);
        var handle = new PlotSeries(this, (cplot, xs, ys) => cplot.AddBars(xs, ys, c, baseline, width));
        RegisterLive(handle);
        return handle;
    }

    // Registers a live series so its (mutable) data is replayed on every rebuild.
    private void RegisterLive(PlotSeries handle) =>
        UI.Invoke(() =>
        {
            _seriesCount++;
            _config.Add(handle.Apply);
            Rebuild();
        });

    // Applies a live-series data mutation on the UI thread, then redraws — WITHOUT rebuilding the underlying plot.
    // A rebuild would allocate a fresh PlotImage (image buffer) and re-create every series each frame; instead, the
    // underlying series alias these same buffers (ConsolePlot's Series holds the list by reference, see AsList), so
    // mutating them here is picked up by the next Draw. Only a resize or a structural change (add/remove series or
    // config) still rebuilds. The buffers are only ever touched here, so a plain List stays race-free even when data
    // arrives on a background thread.
    internal void UpdateSeries(Action mutate) => UI.Invoke(() => { mutate(); Invalidate(); });

    private void AddElement(Action<CPlot> config)
    {
        _seriesCount++;
        _config.Add(config);
        Rebuild();
    }

    private static IPointBrush BrushFor(PlotBrush brush) => brush switch
    {
        PlotBrush.Braille => SystemPointBrushes.Braille,
        PlotBrush.Quadrant => SystemPointBrushes.Quadrant,
        PlotBrush.Block => SystemPointBrushes.Block,
        PlotBrush.Dot => SystemPointBrushes.Dot,
        PlotBrush.Star => SystemPointBrushes.Star,
        _ => SystemPointBrushes.Braille,
    };

    /// <summary>Records an arbitrary configuration step (applied to the underlying plot on every rebuild).</summary>
    public Plot Configure(Action<CPlot> configure)
    {
        UI.Invoke(() => { _config.Add(configure); Rebuild(); });
        return this;
    }

    /// <summary>Configures the axis lines and their captions. This styling is retained across <see cref="Clear"/>.</summary>
    /// <remarks>For the common cases prefer the Jumbee-colour convenience methods <see cref="SetAxisColor"/> and
    /// <see cref="SetAxisTitles"/> — this raw overload exposes ConsolePlot's <see cref="System.ConsoleColor"/> surface.
    /// The passed settings expose <c>IsVisible</c> (default <see langword="true"/>) and <c>Pen</c> — an
    /// immutable pen taking a full-RGB colour, so recolour with <c>a.Pen = new LinePen(a.Pen.Brush, (Color)colour)</c>.
    /// Also optional <c>XTitle</c>/<c>YTitle</c> captions (with <c>TitleColor</c>, a full-RGB colour).
    /// The captions are <b>screen-anchored</b> — <c>YTitle</c> is pinned to the top-left, <c>XTitle</c> to the
    /// bottom-right — so they stay put when the axes rescale, unlike a data-anchored <see cref="AddLabel"/>. Hide the
    /// axis with <c>ConfigureAxis(a =&gt; a.IsVisible = false)</c>; label it with
    /// <c>ConfigureAxis(a =&gt; { a.XTitle = "time"; a.YTitle = "amplitude"; })</c>.</remarks>
    public Plot ConfigureAxis(Action<AxisSettings> configure) => ConfigureChrome(p => configure(p.Axis));

    /// <summary>Configures the background grid. This styling is retained across <see cref="Clear"/>.</summary>
    /// <remarks>Settings expose <c>IsVisible</c> (default <see langword="true"/>, dashed dark-gray) and <c>Pen</c>.
    /// Hide the grid with <c>ConfigureGrid(g =&gt; g.IsVisible = false)</c> — combine with
    /// <c>ConfigureTicks(t =&gt; { t.IsVisible = false; t.Labels.IsVisible = false; })</c> for a bare, chrome-free chart
    /// (e.g. an oscilloscope trace).</remarks>
    public Plot ConfigureGrid(Action<GridSettings> configure) => ConfigureChrome(p => configure(p.Grid));

    /// <summary>Configures the axis ticks and their labels. This styling is retained across <see cref="Clear"/>.</summary>
    /// <remarks>Two separate visibility flags: <c>IsVisible</c> (default <see langword="true"/>) draws the tick
    /// <em>marks</em>, while <c>Labels.IsVisible</c> (default <see langword="true"/>) draws the numeric tick
    /// <em>labels</em> — hiding one does not hide the other. Also exposed: <c>Pen</c>, per-axis
    /// <c>DesiredXStep</c>/<c>DesiredYStep</c> spacing, <c>CustomXTicks</c>/<c>CustomYTicks</c> (see
    /// <see cref="SetXTicks"/>), and <c>Labels</c> (<c>Color</c>, <c>Format</c>, <c>AttachToAxis</c>). Hide the marks
    /// with <c>ConfigureTicks(t =&gt; t.IsVisible = false)</c> and the numbers with
    /// <c>ConfigureTicks(t =&gt; t.Labels.IsVisible = false)</c>.</remarks>
    public Plot ConfigureTicks(Action<TickSettings> configure) => ConfigureChrome(p => configure(p.Ticks));

    /// <summary>Sets the screen-anchored axis captions and (optionally) their colour, in <see cref="Color"/>s — a
    /// convenience over <see cref="ConfigureAxis"/> that takes a Jumbee colour instead of a <see cref="System.ConsoleColor"/>.
    /// A <see langword="null"/> title is left unchanged; pass an <b>empty string</b> to clear a previously-set caption.
    /// Retained across <see cref="Clear"/>, so set it once at setup rather than per frame.</summary>
    /// <remarks><c>YTitle</c> pins to the top-left, <c>XTitle</c> to the bottom-right (see <see cref="ConfigureAxis"/>).
    /// Colours map onto the 16 console colours (see <see cref="Color.ToConsoleColor"/>); an exact console colour is loss-free.</remarks>
    public Plot SetAxisTitles(string? xTitle = null, string? yTitle = null, Color? titleColor = null) =>
        ConfigureChrome(p =>
        {
            if (xTitle is not null) p.Axis.XTitle = xTitle;
            if (yTitle is not null) p.Axis.YTitle = yTitle;
            if (titleColor is { } c) p.Axis.TitleColor = (CColor)c;
        });

    /// <summary>Recolours the axis lines, in a full-RGB <see cref="Color"/> (keeping the current brush) — a
    /// convenience that hides ConsolePlot's immutable pen. Retained across <see cref="Clear"/>.</summary>
    /// <remarks>Sets <see cref="AxisColor"/>, so it also marks the axis colour as explicitly chosen and a later
    /// theme switch will leave it alone.</remarks>
    public Plot SetAxisColor(Color color)
    {
        AxisColor = color;
        return this;
    }

    /// <summary>Recolours the background grid lines, in a full-RGB <see cref="Color"/> (keeping the current brush).
    /// Retained across <see cref="Clear"/>.</summary>
    /// <remarks>Sets <see cref="GridColor"/> — see <see cref="SetAxisColor"/> on theme overriding.</remarks>
    public Plot SetGridColor(Color color)
    {
        GridColor = color;
        return this;
    }

    /// <summary>Recolours the tick marks and, when <paramref name="labelColor"/> is given, the numeric tick labels —
    /// in full-RGB <see cref="Color"/>s (keeping the tick brush). Retained across <see cref="Clear"/>.</summary>
    /// <remarks>Sets <see cref="TickColor"/>/<see cref="TickLabelColor"/> — see <see cref="SetAxisColor"/> on theme
    /// overriding.</remarks>
    public Plot SetTickColor(Color color, Color? labelColor = null)
    {
        TickColor = color;
        if (labelColor is { } lc) TickLabelColor = lc;
        return this;
    }

    // Records persistent axis/grid/tick styling. Unlike Configure/AddSeries (which land in the per-data _config list
    // that Clear() empties), chrome is replayed on every rebuild AND survives Clear(), so a plot rebuilt or cleared to
    // swap data each frame keeps how it's drawn — the fix for a per-frame Clear()+AddSeries loop silently dropping the
    // configured chrome (e.g. a hidden grid reappearing on the first frame after setup).
    /// <summary>The colour for series <paramref name="index"/>, from <see cref="SeriesPalette"/> (which wraps).</summary>
    private CColor SeriesColor(int index) => (CColor)_seriesPalette[index];

    // Re-reads the themed chrome from the current style theme, leaving anything the caller set explicitly alone.
    // Called from the constructor (nothing is overridden yet, so everything is captured) and on a runtime theme
    // switch. PlotSurface contributes a BACKGROUND colour, the rest foregrounds; a token with no colour of the kind
    // it is read for leaves the corresponding field at its existing value.
    private void CaptureTheme()
    {
        var theme = UI.StyleTheme;
        if (!IsThemeOverridden(nameof(Background))) _background = theme.PlotSurface.BackgroundColor;
        if (!IsThemeOverridden(nameof(AxisColor))) _axisColor = theme.PlotAxis.ForegroundColor ?? _axisColor;
        if (!IsThemeOverridden(nameof(GridColor))) _gridColor = theme.PlotGrid.ForegroundColor ?? _gridColor;
        if (!IsThemeOverridden(nameof(TickColor))) _tickColor = theme.PlotTick.ForegroundColor ?? _tickColor;
        if (!IsThemeOverridden(nameof(TickLabelColor))) _tickLabelColor = theme.PlotTickLabel.ForegroundColor ?? _tickLabelColor;
        if (!IsThemeOverridden(nameof(SeriesPalette))) _seriesPalette = theme.PlotSeries;
    }

    /// <inheritdoc/>
    protected override void ApplyTheme()
    {
        CaptureTheme();
        _dirty = true;
        Rebuild();   // the chrome action reads the fields above, so a replay is what actually recolours the plot
    }

    private Plot ConfigureChrome(Action<CPlot> configure)
    {
        UI.Invoke(() => { _chrome.Add(configure); Rebuild(); });
        return this;
    }

    /// <summary>Removes all series and data, leaving an empty plot. Axis/grid/tick styling set via the
    /// <c>Configure*</c> methods is retained — clearing the data does not reset how the plot is drawn.</summary>
    public Plot Clear()
    {
        UI.Invoke(() => { _config.Clear(); _seriesCount = 0; Rebuild(); });
        return this;
    }

    private void Rebuild()
    {
        _dirty = true;
        Invalidate();
    }

    // A plot fills its container and re-fits on resize; it must never be scrolled. Inside a ControlFrame this makes
    // the frame hand the plot the bounded viewport height instead of the unbounded scroll height (which would
    // otherwise balloon the plot to the size clamp and show only a thin slice).
    /// <summary>Always <see langword="true"/>: the plot fills its frame's viewport and is never scrolled.</summary>
    protected internal override bool FillsFrameViewport => true;

    /// <summary>Rebuilds the underlying chart when needed and blits it to the buffer.</summary>
    protected override void Render()
    {
        var w = Size.Width;
        var h = Size.Height;
        if (w <= 0 || h <= 0) return;

        // Rebuild the underlying plot when the content changed or the control was resized.
        var rebuilt = false;
        if (_dirty || _plot is null || _builtWidth != w || _builtHeight != h)
        {
            _dirty = false;
            _builtWidth = w;
            _builtHeight = h;
            _plot = BuildPlot(w, h);
            rebuilt = true;
        }

        // Skip when there's nothing to draw or no room for the axes/labels. Draw won't run to fill the buffer here, so
        // clear it explicitly. ConsolePlot pads degenerate data ranges (a single point / flat series) internally, so
        // Draw is safe for any non-empty data at a usable size — no try/catch needed, and the UI frame loop is the
        // ultimate backstop for anything unforeseen.
        if (_plot is null || w < MinWidth || h < MinHeight)
        {
            consoleBuffer.Initialize();
            return;
        }

        // Draw straight into consoleBuffer — PlotImage's cell surface IS this buffer, so there's no copy pass. Draw's
        // own Clear() erases the previous draw's cells, so a separate consoleBuffer.Initialize() would be redundant.
        _damage?.BeginFrame();
        _plot.Draw();

        if (!_damageTracking) return;
        if (_damage is null || rebuilt)
        {
            // A rebuild replaces the whole figure (and a resize replaces the buffer), so nothing about the previous
            // frame carries over — report everything once rather than diff against a surface that no longer applies.
            DamageAll();
            return;
        }

        _damage.ResetRows();
        var changedRows = _damage.Flush();
        if (changedRows < 0) { DamageAll(); return; }   // too dense to track: report everything
        if (changedRows == 0) return;
        for (var y = 0; y < h; y++)
            if (_damage.ChangedRow(y, out var first, out var last))
                Damage(new Rect(first, y, last - first + 1, 1));
    }

    private PlotImage? BuildPlot(int width, int height)
    {
        if (_config.Count == 0 && _chrome.Count == 0) return null;
        _damage = _damageTracking ? new DamageBuffer(consoleBuffer) : null;
        var plot = new PlotImage(consoleBuffer, (CColor?)_background, _damage);
        foreach (var apply in _chrome) apply(plot);    // persistent styling first (axis/grid/ticks)
        foreach (var apply in _config) apply(plot);    // then per-data series/labels/ranges
        return plot;
    }
    #endregion

    #region Fields
    private const int MinWidth = 8;
    private const int MinHeight = 4;

    /// <summary>Whether this plot reports partial damage (see <see cref="DamageTracking"/>).</summary>
    protected override bool TracksDamage => _damageTracking;

    private bool _damageTracking;
    private DamageBuffer? _damage;
    private Color _axisColor;
    private Color _gridColor;
    private Color _tickColor;
    private Color _tickLabelColor;
    private PlotPalette _seriesPalette;

    private readonly List<Action<CPlot>> _config = [];
    // Persistent axis/grid/tick styling — replayed on every rebuild and NOT emptied by Clear() (see ConfigureChrome).
    private readonly List<Action<CPlot>> _chrome = [];
    private PlotImage? _plot;
    private int _seriesCount;
    private int _builtWidth = -1;
    private int _builtHeight = -1;
    private bool _dirty = true;
    private CColor? _background;
    #endregion
}

/// <summary>
/// Selects the glyph set (and thus the sub-cell resolution) a <see cref="Plot"/> series is drawn with.
/// </summary>
/// <remarks>Higher resolution packs more plotted points into each character cell for a smoother line.</remarks>
public enum PlotBrush
{
    /// <summary>Braille dots — 2×4 sub-cells per character (8 points/cell), the smoothest. The default.</summary>
    Braille,
    /// <summary>Quadrant blocks — 2×2 sub-cells per character (4 points/cell), solid blocks rather than dots.</summary>
    Quadrant,
    /// <summary>A solid full block <c>█</c> per point (1×1).</summary>
    Block,
    /// <summary>A <c>•</c> per point (1×1).</summary>
    Dot,
    /// <summary>A <c>*</c> per point (1×1).</summary>
    Star,
}

/// <summary>Selects the colour map a <see cref="Plot"/> heatmap uses to turn cell values into colours.</summary>
public enum PlotColormap
{
    /// <summary>Perceptually-uniform dark-purple → blue → teal → green → yellow (the default).</summary>
    Viridis,
    /// <summary>Classic heat: black → red → orange → yellow → white.</summary>
    Heat,
    /// <summary>Dark → light grey.</summary>
    Grayscale,
    /// <summary>Cyan → blue → magenta.</summary>
    Cool,
}

/// <summary>Horizontal anchoring of a <see cref="Plot"/> annotation label relative to its point.</summary>
public enum PlotLabelAlign
{
    /// <summary>The text starts at the point and runs right.</summary>
    Left,
    /// <summary>The text is centred on the point.</summary>
    Center,
    /// <summary>The text ends at the point.</summary>
    Right,
}
