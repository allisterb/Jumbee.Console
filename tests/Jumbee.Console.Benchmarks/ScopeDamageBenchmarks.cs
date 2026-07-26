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

/// <summary>
/// <see cref="Plot.DamageTracking"/> on vs off, on the path the scope app actually uses: a held
/// <see cref="PlotSeries"/> fed with <c>SetData</c> every frame (<c>AddLiveSeries</c>), driven through the real
/// <see cref="ConsoleManager"/> compositor headlessly. One op = one frame (paint + composite).
/// </summary>
/// <remarks>
/// <para>
/// Deliberately NOT the <see cref="ScopeRenderBenchmarks"/> workload, which rebuilds the plot (<c>Clear</c> +
/// <c>AddSeries</c>) every frame. A rebuild replaces the whole figure, so the plot reports <c>DamageAll</c> and
/// tracking cannot help by design — measuring there would only price the bookkeeping, not the feature.
/// </para>
/// <para>
/// <see cref="Fill"/> is the variable that decides whether tracking pays: it scales the waveform against the plot's
/// fixed <c>[-1, 1]</c> bounds, so a low value leaves most rows permanently empty (the quiet-signal scope pane) and
/// a high one puts trace in nearly every row (a signal at full scale, where there is little left to skip).
/// </para>
/// <para>
/// BDN times OUR frame only. <see cref="ScopeDamageDiagnostics.Diagnose"/> (<c>-- --damage</c>) adds the number that
/// arguably matters more: the ANSI <b>bytes/frame</b> handed to the terminal. <c>ConsoleManager</c> emits
/// asynchronously, so the cost of the runtime, the OS and the terminal's own renderer ingesting that payload lands
/// outside every timing here — a saving in bytes can be worth having even when our measured frame time is a wash.
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class ScopeDamageBenchmarks
{
    private ScopeDamageDiagnostics.Workload _work = null!;
    private int _tick;

    [Params(true, false)]
    public bool Damage;

    /// <summary>Fraction of the plot's Y range the trace spans: 0.15 = a quiet signal hugging the centre line,
    /// 0.95 = a signal at full scale touching nearly every row.</summary>
    [Params(0.15, 0.95)]
    public double Fill;

    [GlobalSetup]
    public void Setup() => _work = ScopeDamageDiagnostics.Start(Damage, Fill, static _ => Task.CompletedTask);

    [Benchmark]
    public void LiveScopeFrame() => _work.Frame(++_tick * 13);
}

/// <summary>
/// The shared live-scope workload behind <see cref="ScopeDamageBenchmarks"/>, plus the emit-side measurement BDN
/// cannot show: dirty cells and ANSI bytes per frame, with damage tracking on and off.
/// </summary>
public static class ScopeDamageDiagnostics
{
    private const int W = 220, H = 53, N = 2048;

    private static readonly double[] Xs = BuildXs();
    private static readonly double[] Wave = BuildWaveform();

    /// <summary>A started headless session over a live plot, ready to be stepped a frame at a time.</summary>
    public sealed class Workload
    {
        internal Workload(Plot plot, PlotSeries left, PlotSeries right, double fill)
        {
            Plot = plot;
            _left = left;
            _right = right;
            _fill = fill;
            _yl = new double[N];
            _yr = new double[N];
        }

        public Plot Plot { get; }

        /// <summary>Pushes the waveform scrolled by <paramref name="offset"/> samples, then paints and composites.</summary>
        public void Frame(int offset)
        {
            Feed(offset);
            UI.PaintFrame();
            ConsoleManager.Draw();
        }

        /// <summary>Scrolls the waveform and pushes it through the held handles — no Clear, no re-add, so the plot
        /// updates in place and damage tracking sees a moved trace rather than a rebuilt figure.</summary>
        public void Feed(int offset)
        {
            for (var i = 0; i < N; i++)
            {
                var v = Wave[(i + offset) % N] * _fill;
                _yl[i] = v;
                _yr[i] = -v;
            }
            _left.SetData(Xs, _yl);
            _right.SetData(Xs, _yr);
        }

        private readonly PlotSeries _left, _right;
        private readonly double[] _yl, _yr;
        private readonly double _fill;
    }

    /// <summary>Builds the live scope plot and starts a headless compositor session on it. <paramref name="sink"/>
    /// receives the emitted ANSI (a no-op for timing, a counter for <see cref="Diagnose"/>).</summary>
    public static Workload Start(bool damageTracking, double fill, Func<object, Task> sink)
    {
        var plot = new Plot { DamageTracking = damageTracking };
        plot.ConfigureGrid(g => g.IsVisible = false)
            .ConfigureTicks(t => { t.IsVisible = false; t.Labels.IsVisible = false; })
            .SetYRange(-1, 1).SetXRange(0, N);

        var work = new Workload(plot, plot.AddLiveSeries(), plot.AddLiveSeries(), fill);
        work.Feed(0);

        ConsoleManager.AnsiEnabled = true;
        ConsoleManager.AnsiOutput = acsb => sink(acsb);
        ConsoleManager.Console = new NullConsole { Size = new Size(W, H) };
        ConsoleManager.Setup();
        ConsoleManager.Content = plot;
        UI.PaintFrame();
        ConsoleManager.Draw();   // prime: the first frame is a full redraw either way
        return work;
    }

    /// <summary>
    /// Prints, for each (tracking, fill) combination, the median paint/composite time, dirty cells and — the point
    /// of this diagnostic — ANSI bytes per frame with the implied terminal throughput. Run via <c>-- --damage</c>.
    /// </summary>
    public static void Diagnose()
    {
        Console.WriteLine($"live scope {W}x{H}, {N} pts/ch x2, braille — damage tracking on vs off");
        Console.WriteLine($"  (ConsoleManager emits asynchronously, so the bytes below are paid by the terminal");
        Console.WriteLine($"   AFTER our frame time — they are not included in any of the times reported here.)");
        foreach (var fill in new[] { 0.15, 0.95 })
        {
            Console.WriteLine($"\nfill {fill:P0} of the Y range:");
            foreach (var damage in new[] { false, true })
            {
                var r = Measure(damage, fill);
                Console.WriteLine(
                    $"  damage {(damage ? "on " : "off")}  paint {r.Paint,7:F1} us  draw {r.Draw,7:F1} us  " +
                    $"dirty {r.Cells,6}/{(long)W * H} cells  ANSI {r.Bytes,7} B/frame  " +
                    $"({r.Bytes * 60.0 / 1024:F0} KB/s at 60 fps)");
            }
        }
    }

    private static Result Measure(bool damageTracking, double fill)
    {
        long frameBytes = 0;
        var work = Start(damageTracking, fill, acsb => { frameBytes += acsb.ToString()!.Length; return Task.CompletedTask; });

        const int warmup = 100, n = 200;
        var paint = new List<double>(n);
        var draw = new List<double>(n);
        var cells = new List<long>(n);
        var bytes = new List<long>(n);
        var sw = new Stopwatch();
        for (var i = 1; i <= warmup + n; i++)
        {
            work.Feed(i * 13);
            sw.Restart(); UI.PaintFrame(); sw.Stop();
            var p = sw.Elapsed.TotalMicroseconds;
            frameBytes = 0;
            sw.Restart(); ConsoleManager.Draw(); sw.Stop();
            ConsoleManager.OutputIdle.GetAwaiter().GetResult();   // ensure the sink has run before reading the count
            var d = sw.Elapsed.TotalMicroseconds;
            if (i <= warmup) continue;
            paint.Add(p); draw.Add(d); cells.Add(ConsoleManager.LastFrameDirtyCells); bytes.Add(frameBytes);
        }
        paint.Sort(); draw.Sort(); cells.Sort(); bytes.Sort();
        return new Result(paint[n / 2], draw[n / 2], cells[n / 2], bytes[n / 2]);
    }

    private readonly record struct Result(double Paint, double Draw, long Cells, long Bytes);

    private static double[] BuildXs()
    {
        var xs = new double[N];
        for (var i = 0; i < N; i++) xs[i] = i;
        return xs;
    }

    private static double[] BuildWaveform()
    {
        var rnd = new Random(1);
        var ys = new double[N];
        for (var i = 0; i < N; i++)
            ys[i] = (0.6 * Math.Sin(i * 0.05)) + (0.3 * Math.Sin(i * 0.31)) + (0.1 * ((rnd.NextDouble() * 2) - 1));
        return ys;
    }

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
