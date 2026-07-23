namespace Jumbee.Console.Benchmarks;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

using BenchmarkDotNet.Attributes;

using ConsoleGUI;
using ConsoleGUI.Api;
using ConsoleGUI.Space;

using Jumbee.Console;

using CCharacter = ConsoleGUI.Data.Character;

/// <summary>Amplitude regime for the scope waveform: how the sample values sit relative to the plot's fixed
/// <c>[-1, 1]</c> Y bounds.</summary>
public enum ScopeAmplitude
{
    /// <summary>Data within <c>±1</c> — fills the window without clipping.</summary>
    Fitted,

    /// <summary>Data boosted <c>×5</c> (scope-tui's <c>AmplitudeGain</c>) so it overshoots the window and clips —
    /// the regime the shipped app runs in. Counter-intuitively cheaper to render (more points clip off-screen).</summary>
    Overshoot,
}

/// <summary>
/// The scope-tui oscilloscope's per-frame render cost: a full-screen <see cref="Plot"/> holding two braille line
/// series (L/R, 2048 points each), rebuilt (<c>Clear</c> + <c>AddSeries</c>) and re-drawn every frame through the real
/// compositor, headless. One op = one refresh frame (rebuild + paint + composite).
/// <para>
/// BenchmarkDotNet times the whole frame here; <see cref="ScopeRenderDiagnostics.Diagnose"/> (<c>-- --scope</c>) splits
/// it into compute (paint) vs composite/emit (draw) AND — the number BDN can't show — the ANSI <b>bytes/frame</b> sent
/// to the terminal, which is the real ceiling: a dense moving waveform changes most cells, so the escape-sequence
/// payload (not our compute) is what a terminal struggles to ingest at high fps.
/// </para>
/// </summary>
[MemoryDiagnoser]
public class ScopeRenderBenchmarks
{
    [Params(ScopeAmplitude.Overshoot, ScopeAmplitude.Fitted)]
    public ScopeAmplitude Amplitude;

    private Plot _plot = null!;
    private double[] _yl = null!, _yr = null!;
    private int _tick;

    [GlobalSetup]
    public void Setup() => _plot = ScopeRenderDiagnostics.NewSession(out _yl, out _yr);

    [Benchmark]
    public void ScopeRefreshFrame() =>
        ScopeRenderDiagnostics.RefreshFrame(_plot, _yl, _yr, ScopeRenderDiagnostics.GainOf(Amplitude), ++_tick);
}

/// <summary>
/// The same scope frame as <see cref="ScopeRenderBenchmarks"/>, but composited through an ambient
/// <see cref="ConsoleGUI.Controls.Overlay"/> (mirroring <c>UI.Start</c>), so the per-cell composite reads route
/// through <c>Overlay.get_Item</c> — the render-pipeline frame the profiler flagged at ~14%. Used to measure the
/// effect of sealing <c>Overlay</c> (PGO guarded-devirtualization of that interface indexer call).
/// </summary>
[MemoryDiagnoser]
public class ScopeOverlayBenchmarks
{
    private Plot _plot = null!;
    private double[] _yl = null!, _yr = null!;
    private int _tick;

    [GlobalSetup]
    public void Setup() => _plot = ScopeRenderDiagnostics.NewOverlaySession(out _yl, out _yr);

    [Benchmark]
    public void ScopeRefreshFrameViaOverlay() =>
        ScopeRenderDiagnostics.RefreshFrame(_plot, _yl, _yr, ScopeRenderDiagnostics.GainOf(ScopeAmplitude.Overshoot), ++_tick);
}

/// <summary>The compute/composite/bytes split behind <see cref="ScopeRenderBenchmarks"/>, and the shared workload.</summary>
public static class ScopeRenderDiagnostics
{
    // A representative full-screen scope: a wide terminal, one oscilloscope buffer (2048 samples) per stereo channel.
    private const int W = 220, H = 53, N = 2048;

    private static readonly double[] Xs = BuildXs();
    private static readonly double[] BaseYs = BuildWaveform();   // audio-ish, normalised to ~[-1, 1]

    public static double GainOf(ScopeAmplitude a) => a == ScopeAmplitude.Overshoot ? 5.0 : 1.0;

    /// <summary>Builds the scope plot (grid/ticks off, fixed <c>±1</c> Y bounds like the oscilloscope) and starts a
    /// headless compositor session on it.</summary>
    public static Plot NewSession(out double[] yl, out double[] yr)
    {
        var plot = new Plot();
        plot.ConfigureGrid(g => g.IsVisible = false)
            .ConfigureTicks(t => { t.IsVisible = false; t.Labels.IsVisible = false; })
            .SetYRange(-1, 1).SetXRange(0, N);
        yl = new double[N];
        yr = new double[N];
        Rebuild(plot, yl, yr, 5.0, 0);
        ConsoleManager.AnsiOutput = static _ => Task.CompletedTask;   // no-op sink for the BDN timing path
        StartSession(plot);
        return plot;
    }

    /// <summary>Like <see cref="NewSession"/> but composites the plot THROUGH an ambient ConsoleGUI
    /// <see cref="ConsoleGUI.Controls.Overlay"/> (as the real app does under <c>UI.Start</c>), so every composited
    /// cell read routes through <c>Overlay.get_Item</c> — the ~14% frame the profiler flagged. The plot is returned
    /// for <see cref="RefreshFrame"/>; the Overlay stays as <c>ConsoleManager.Content</c>.</summary>
    public static Plot NewOverlaySession(out double[] yl, out double[] yr)
    {
        var plot = new Plot();
        plot.ConfigureGrid(g => g.IsVisible = false)
            .ConfigureTicks(t => { t.IsVisible = false; t.Labels.IsVisible = false; })
            .SetYRange(-1, 1).SetXRange(0, N);
        yl = new double[N];
        yr = new double[N];
        Rebuild(plot, yl, yr, 5.0, 0);
        ConsoleManager.AnsiOutput = static _ => Task.CompletedTask;
        StartSession(new ConsoleGUI.Controls.Overlay { BottomContent = plot });
        return plot;
    }

    /// <summary>One refresh: swap in fresh (moving) waveform data, then paint + composite.</summary>
    public static void RefreshFrame(Plot plot, double[] yl, double[] yr, double gain, int tick)
    {
        Rebuild(plot, yl, yr, gain, tick * 13);
        UI.PaintFrame();
        ConsoleManager.Draw();
    }

    /// <summary>
    /// Prints the render split for one amplitude regime: compute (paint) vs composite+emit (draw) time, the dirty-cell
    /// count, and the ANSI bytes emitted per frame with the implied terminal throughput at 30/60 fps. Run via
    /// <c>-- --scope [overshoot|fitted]</c>.
    /// </summary>
    public static void Diagnose(string? mode)
    {
        var amp = string.Equals(mode, "fitted", StringComparison.OrdinalIgnoreCase)
            ? ScopeAmplitude.Fitted : ScopeAmplitude.Overshoot;
        var r = Measure(GainOf(amp));
        Console.WriteLine($"scope {W}x{H}, {N} pts/ch x2, braille, {amp}:");
        Console.WriteLine($"  compute (paint) {r.Paint,7:F1} us   composite+emit (draw) {r.Draw,7:F1} us   " +
                          $"total {r.Paint + r.Draw,7:F1} us   dirty {r.Cells,5}/{(long)W * H} cells");
        Console.WriteLine($"  ANSI {r.Bytes,7} bytes/frame  ->  {r.Bytes * 60.0 / 1024:F0} KB/s at 60 fps, " +
                          $"{r.Bytes * 30.0 / 1024:F0} KB/s at 30 fps");
    }

    private static Result Measure(double gain)
    {
        long frameBytes = 0;
        var plot = new Plot();
        plot.ConfigureGrid(g => g.IsVisible = false)
            .ConfigureTicks(t => { t.IsVisible = false; t.Labels.IsVisible = false; })
            .SetYRange(-1, 1).SetXRange(0, N);
        var yl = new double[N];
        var yr = new double[N];
        Rebuild(plot, yl, yr, gain, 0);
        ConsoleManager.AnsiOutput = acsb => { frameBytes += acsb.ToString().Length; return Task.CompletedTask; };
        StartSession(plot);

        const int warmup = 200, n = 200;
        var paint = new List<double>(n);
        var draw = new List<double>(n);
        var cells = new List<long>(n);
        var bytes = new List<long>(n);
        var sw = new Stopwatch();
        for (int i = 1; i <= warmup + n; i++)
        {
            Rebuild(plot, yl, yr, gain, i * 13);
            sw.Restart(); UI.PaintFrame(); sw.Stop();
            var p = sw.Elapsed.TotalMicroseconds;
            frameBytes = 0;
            sw.Restart(); ConsoleManager.Draw(); sw.Stop();
            ConsoleManager.OutputIdle.GetAwaiter().GetResult();   // ensure the (synchronous) sink has run
            var d = sw.Elapsed.TotalMicroseconds;
            if (i <= warmup) continue;
            paint.Add(p); draw.Add(d); cells.Add(ConsoleManager.LastFrameDirtyCells); bytes.Add(frameBytes);
        }
        paint.Sort(); draw.Sort(); cells.Sort(); bytes.Sort();
        return new Result(paint[n / 2], draw[n / 2], cells[n / 2], bytes[n / 2]);
    }

    // Rebuild the plot the way the app does each audio frame: clear, then re-add both channels' braille line series.
    private static void Rebuild(Plot plot, double[] yl, double[] yr, double gain, int phase)
    {
        plot.Clear();
        for (int i = 0; i < N; i++)
        {
            yl[i] = BaseYs[(i + phase) % N] * gain;
            yr[i] = BaseYs[(i + 7 + phase) % N] * gain;
        }
        plot.AddSeries(Xs, yl, PlotBrush.Braille, new Color(220, 60, 60));
        plot.AddSeries(Xs, yr, PlotBrush.Braille, new Color(220, 200, 60));
    }

    // The real compositor, headless: a no-op console; the caller sets ConsoleManager.AnsiOutput first (no-op for the
    // BDN timing path, byte-counting for --scope).
    private static void StartSession(IControl content)
    {
        ConsoleManager.AnsiEnabled = true;
        ConsoleManager.Console = new NullConsole { Size = new Size(W, H) };
        ConsoleManager.Setup();
        ConsoleManager.Content = content;
        UI.PaintFrame();
        ConsoleManager.Draw();
    }

    private static double[] BuildXs()
    {
        var xs = new double[N];
        for (int i = 0; i < N; i++) xs[i] = i;
        return xs;
    }

    private static double[] BuildWaveform()
    {
        var rnd = new Random(1);
        var ys = new double[N];
        for (int i = 0; i < N; i++)
            ys[i] = 0.6 * Math.Sin(i * 0.05) + 0.3 * Math.Sin(i * 0.31) + 0.1 * (rnd.NextDouble() * 2 - 1);
        return ys;
    }

    private readonly record struct Result(double Paint, double Draw, long Cells, long Bytes);

    private sealed class NullConsole : IConsole
    {
        public Size Size { get; set; }
        public bool KeyAvailable => false;
        public void Initialize() { }
        public void OnRefresh() { }
        public void Write(Position position, in CCharacter character) { }
        public ConsoleKeyInfo ReadKey() => throw new NotSupportedException();
    }
}
