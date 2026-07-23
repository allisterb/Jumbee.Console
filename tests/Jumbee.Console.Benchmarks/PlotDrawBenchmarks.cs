namespace Jumbee.Console.Benchmarks;

using BenchmarkDotNet.Attributes;

using ConsolePlot;
using ConsolePlot.Drawing.Tools;

using CColor = ConsoleGUI.Data.Color;

/// <summary>
/// Measures the per-frame allocation of <see cref="ConsolePlot.Plot.Draw"/> — the ConsolePlot render hot path the
/// Jumbee <c>Plot</c> control runs every frame for a live chart. Representative dashboard workload: a fixed-axis line
/// series plus bars, with grid, axis, ticks and labels all on (the tick/label + bounds + converter machinery). Use
/// <c>--filter *PlotDrawBenchmarks*</c> and watch the <c>Allocated</c> column.
/// </summary>
[MemoryDiagnoser]
public class PlotDrawBenchmarks
{
    private Plot _line = null!;
    private Plot _bars = null!;
    private List<double> _xs = null!;
    private List<double> _ys = null!;
    private PointPen _pen;

    // Large series to isolate the per-point draw-loop cost: one backed by double[] (draw loop's span fast-path), one
    // by an opaque IReadOnlyList (forces the interface indexer). The gap = what the array/span fast-path saves.
    private const int BigN = 2000;
    private Plot _bigLineArray = null!;
    private Plot _bigLineOpaque = null!;
    private Plot _bigScatterArray = null!;
    private Plot _bigScatterOpaque = null!;

    // Scope-scale draw: the AudioScope demo's exact per-frame Draw() shape — two braille series (L/R, 2048 pts each)
    // over the real 220x53 terminal, fixed ±1 Y / [0,N] X, grid+ticks+labels off. `ScopeLine_Draw` vs
    // `ScopeScatter_Draw` is the like-for-like connected-line vs unconnected-dots cost at true scope size; both feed
    // the SAME summed-sine waveform so the only difference is DrawLines vs DrawPoints.
    private const int ScopeW = 220, ScopeH = 53, ScopeN = 2048;
    private Plot _scopeLine = null!;
    private Plot _scopeScatter = null!;

    [Params(60)]
    public int Width;

    [Params(20)]
    public int Height;

    [GlobalSetup]
    public void Setup()
    {
        _line = BuildLine();
        _bars = BuildBars();

        _xs = new List<double>();
        _ys = new List<double>();
        for (int i = 0; i < 64; i++) { _xs.Add(i); _ys.Add(50 + 40 * System.Math.Sin(i * 0.2)); }
        _pen = new PointPen(SystemPointBrushes.Braille, new CColor(90, 160, 240));

        _bigLineArray = BuildBig(scatter: false, opaque: false);
        _bigLineOpaque = BuildBig(scatter: false, opaque: true);
        _bigScatterArray = BuildBig(scatter: true, opaque: false);
        _bigScatterOpaque = BuildBig(scatter: true, opaque: true);

        _scopeLine = BuildScope(scatter: false);
        _scopeScatter = BuildScope(scatter: true);
    }

    // A scope-scale plot mirroring the AudioScope demo's per-frame Draw() workload. Two braille series over the real
    // 220x53 terminal with a fixed ±1 Y window; a summed-sine "audio-ish" waveform (not a single smooth sine, so
    // segment steepness — and thus the dense-segment fast-path's hit rate — is realistic rather than best-case).
    private Plot BuildScope(bool scatter)
    {
        var xs = new double[ScopeN];
        var yl = new double[ScopeN];
        var yr = new double[ScopeN];
        for (int i = 0; i < ScopeN; i++)
        {
            xs[i] = i;
            double t = i * 0.05;
            yl[i] = 0.6 * System.Math.Sin(t) + 0.3 * System.Math.Sin(t * 3.1) + 0.1 * System.Math.Sin(t * 7.3);
            yr[i] = 0.6 * System.Math.Sin(t + 0.7) + 0.3 * System.Math.Sin(t * 2.7) + 0.1 * System.Math.Sin(t * 6.1);
        }

        var plot = new Plot(ScopeW, ScopeH);
        plot.FixedXRange = (0, ScopeN - 1);
        plot.FixedYRange = (-1, 1);
        plot.Grid.IsVisible = false;
        plot.Ticks.IsVisible = false;
        plot.Ticks.Labels.IsVisible = false;
        var penL = new PointPen(SystemPointBrushes.Braille, new CColor(220, 60, 60));
        var penR = new PointPen(SystemPointBrushes.Braille, new CColor(220, 200, 60));
        if (scatter) { plot.AddScatter(xs, yl, penL); plot.AddScatter(xs, yr, penR); }
        else { plot.AddSeries(xs, yl, penL); plot.AddSeries(xs, yr, penR); }
        return plot;
    }

    // A BigN-point line/scatter with a pinned axis. The series data is either a plain double[] (fast path) or wrapped
    // in Opaque (interface indexer). Both hold identical values, so the benchmarks differ only in the draw-loop index.
    private Plot BuildBig(bool scatter, bool opaque)
    {
        var xs = new double[BigN];
        var ys = new double[BigN];
        for (int i = 0; i < BigN; i++) { xs[i] = i; ys[i] = 50 + 40 * System.Math.Sin(i * 0.05); }

        var plot = new Plot(Width, Height);
        plot.FixedXRange = (0, BigN - 1);
        plot.FixedYRange = (0, 100);
        var pen = new PointPen(SystemPointBrushes.Braille, new CColor(90, 160, 240));
        IReadOnlyCollection<double> px = opaque ? new Opaque(xs) : xs;
        IReadOnlyCollection<double> py = opaque ? new Opaque(ys) : ys;
        if (scatter) plot.AddScatter(px, py, pen);
        else plot.AddSeries(px, py, pen);
        return plot;
    }

    // An IReadOnlyList<double> whose concrete type is neither double[] nor List<double>, so the draw loop's span
    // fast-path (TryAsSpan) misses and it exercises the interface indexer — the pre-fast-path baseline.
    private sealed class Opaque(double[] data) : IReadOnlyList<double>
    {
        public double this[int i] => data[i];
        public int Count => data.Length;
        public IEnumerator<double> GetEnumerator() { foreach (var d in data) yield return d; }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    // A streaming line chart with a pinned axis, grid and tick labels — the common live-monitor configuration.
    private Plot BuildLine()
    {
        var xs = new double[64];
        var ys = new double[64];
        for (int i = 0; i < xs.Length; i++) { xs[i] = i; ys[i] = 50 + 40 * System.Math.Sin(i * 0.2); }

        var plot = new Plot(Width, Height);
        plot.FixedXRange = (0, 63);
        plot.FixedYRange = (0, 100);
        plot.AddSeries(xs, ys, new PointPen(SystemPointBrushes.Braille, new CColor(90, 160, 240)));
        return plot;
    }

    private Plot BuildBars()
    {
        var xs = new double[12];
        var ys = new double[12];
        for (int i = 0; i < xs.Length; i++) { xs[i] = i + 1; ys[i] = 3 + (i % 7); }

        var plot = new Plot(Width, Height);
        plot.FixedYRange = (0, 11);
        plot.AddBars(xs, ys, new CColor(90, 160, 240));
        return plot;
    }

    [Benchmark(Baseline = true)]
    public void DrawLine() => _line.Draw();

    [Benchmark]
    public void DrawBars() => _bars.Draw();

    // The old per-frame cost a live Jumbee plot paid: rebuild the whole plot (new image + re-add series) then Draw.
    // The gap between this and DrawLine (a plain redraw of the reused plot) is what the no-rebuild change saves per
    // frame — the underlying series now alias the live buffers, so a data update only redraws.
    [Benchmark]
    public void RebuildAndDraw()
    {
        var plot = new Plot(Width, Height);
        plot.FixedXRange = (0, 63);
        plot.FixedYRange = (0, 100);
        plot.AddSeries(_xs, _ys, _pen);
        plot.Draw();
    }

    // BigN-point line: array/span fast path vs. the interface indexer. Their gap is the draw-loop indexing saving.
    [Benchmark]
    public void BigLine_ArraySpan() => _bigLineArray.Draw();

    [Benchmark]
    public void BigLine_InterfaceIndexer() => _bigLineOpaque.Draw();

    // BigN-point scatter: DrawPoints has no per-segment Bresenham, so the per-point indexing is a larger share here —
    // the clearest read on the fast-path's effect.
    [Benchmark]
    public void BigScatter_ArraySpan() => _bigScatterArray.Draw();

    [Benchmark]
    public void BigScatter_InterfaceIndexer() => _bigScatterOpaque.Draw();

    // Scope-scale connected line (the shipped oscilloscope) vs unconnected scatter, same waveform. The gap is the cost
    // of rasterizing the connecting segments (per-segment clip + Bresenham) that a scatter skips — i.e. what a
    // dense-segment fast-path in DrawLines can shave without changing the connected-line look.
    [Benchmark]
    public void ScopeLine_Draw() => _scopeLine.Draw();

    [Benchmark]
    public void ScopeScatter_Draw() => _scopeScatter.Draw();
}
