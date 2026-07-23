namespace Jumbee.Console;

using System;
using System.Collections.Generic;

using ConsolePlot.Plotting;

using CPlot = ConsolePlot.Plot;

/// <summary>
/// A live, updatable series in a <see cref="Plot"/>. Returned by <see cref="Plot.AddLiveSeries"/> (line),
/// <see cref="Plot.AddLiveScatter"/> (markers) or <see cref="Plot.AddLiveBars"/>; hold onto it and feed data as it
/// arrives with <see cref="SetData"/>, <see cref="SetValues"/>, <see cref="Push"/> or <see cref="Clear"/>.
/// </summary>
/// <remarks>
/// Every update is marshalled onto the UI thread and re-draws the plot, so the methods are safe to call from any
/// data-source thread.
/// </remarks>
public sealed class PlotSeries
{
    #region Constructors
    internal PlotSeries(Plot plot, Action<CPlot, IReadOnlyList<double>, IReadOnlyList<double>> draw)
    {
        _plot = plot;
        _draw = draw;
    }
    #endregion

    #region Methods
    /// <summary>Replaces the series data with the paired <paramref name="xs"/>/<paramref name="ys"/> (same length).
    /// Passing empty lists is valid and draws nothing — a live series can be emptied (equivalently, <see cref="Clear"/>)
    /// and refilled without removing/re-adding it, so a series that appears only under some state (a trigger marker,
    /// peaks) can be toggled by feeding it data or emptying it.</summary>
    public void SetData(double[] xs, double[] ys)
    {
        if (xs.Length != ys.Length)
            throw new ArgumentException("xs and ys must have the same length.");

        _plot.UpdateSeries(() =>
        {
            _xs.Clear();
            _ys.Clear();
            for (int i = 0; i < xs.Length; i++) { _xs.Add(xs[i]); _ys.Add(ys[i]); }
        });
    }

    /// <summary>Replaces the series with <paramref name="values"/> at implicit x positions 1, 2, 3, … — for bars.</summary>
    public void SetValues(double[] values)
    {
        _plot.UpdateSeries(() =>
        {
            _xs.Clear();
            _ys.Clear();
            for (int i = 0; i < values.Length; i++) { _xs.Add(i + 1); _ys.Add(values[i]); }
        });
    }

    /// <summary>
    /// Appends a point, keeping at most <paramref name="maxPoints"/> (0 = unbounded) by dropping the oldest — a
    /// rolling window for streaming time-series.
    /// </summary>
    public void Push(double x, double y, int maxPoints = 0)
    {
        _plot.UpdateSeries(() =>
        {
            _xs.Add(x);
            _ys.Add(y);
            if (maxPoints > 0)
                while (_xs.Count > maxPoints) { _xs.RemoveAt(0); _ys.RemoveAt(0); }
        });
    }

    /// <summary>
    /// Scrolls a new value into a fixed-width strip chart: the series keeps the last <paramref name="window"/> values
    /// at fixed x positions 0..window−1, so the data flows through a <b>stationary</b> x axis (a real-time monitor).
    /// </summary>
    /// <remarks>
    /// Unlike <see cref="Push"/> there is no ever-growing x — pair with <c>SetXRange(0, window − 1)</c> to pin the axis.
    /// </remarks>
    public void Scroll(double value, int window)
    {
        _plot.UpdateSeries(() =>
        {
            _ys.Add(value);
            while (_ys.Count > window) _ys.RemoveAt(0);
            // Keep x = 0, 1, …, count−1 (fixed positions; once the window is full this never changes).
            while (_xs.Count < _ys.Count) _xs.Add(_xs.Count);
            while (_xs.Count > _ys.Count) _xs.RemoveAt(_xs.Count - 1);
        });
    }

    /// <summary>Removes all points from the series.</summary>
    public void Clear() => _plot.UpdateSeries(() => { _xs.Clear(); _ys.Clear(); });

    // Applied by the plot on every rebuild — reads the current buffers, which the element then snapshots.
    internal void Apply(CPlot cplot) => _draw(cplot, _xs, _ys);
    #endregion

    #region Fields
    private readonly Plot _plot;
    private readonly Action<CPlot, IReadOnlyList<double>, IReadOnlyList<double>> _draw;
    private readonly List<double> _xs = [];
    private readonly List<double> _ys = [];
    #endregion
}
