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
using CColor = ConsoleGUI.Data.Color;

/// <summary>
/// The cost ceiling for a control that repaints its <b>whole area every frame</b> — the shape of every full-screen
/// animated viewport (3D scene, ray-marcher, video), and the one workload the dirty-rect renderer cannot help.
/// One op = one frame (paint + composite + emit).
/// </summary>
/// <remarks>
/// <para>
/// Three workloads bracket where the time goes. <c>Blank</c> paints nothing, so it prices the frame machinery alone.
/// <c>Static</c> writes every cell but always the same value, so paint is paid in full while the renderer diffs to
/// zero changed cells and emits nothing — the gap to <c>Blank</c> is our write cost, the gap to <c>Animated</c> is
/// the compositor's and the terminal's. <c>Animated</c> changes every cell's glyph plus both colours every frame,
/// which is what a moving camera actually does.
/// </para>
/// <para>
/// The interesting number is not our microseconds but the ANSI bytes/frame in <see cref="FullScreenDiagnostics"/>:
/// <c>ConsoleManager</c> emits asynchronously, so the runtime, the OS pipe and the terminal's own renderer ingest
/// that payload <em>after</em> every time reported here.
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class FullScreenBenchmarks
{
    private FullScreenDiagnostics.Workload _work = null!;
    private int _tick;

    /// <summary>Terminal size, <c>WxH</c>. 80x24 is the smallest anyone runs; 200x50 is a maximised window.</summary>
    [Params("80x24", "200x50")]
    public string Size = "200x50";

    [Params(FullScreenFill.Blank, FullScreenFill.Static, FullScreenFill.Animated)]
    public FullScreenFill Fill;

    [GlobalSetup]
    public void Setup()
    {
        var (w, h) = FullScreenDiagnostics.ParseSize(Size);
        _work = FullScreenDiagnostics.Start(w, h, Fill, static _ => Task.CompletedTask);
    }

    [Benchmark]
    public void FullScreenFrame() => _work.Frame(++_tick);
}

/// <summary>What a <see cref="FullScreenControl"/> writes on each paint.</summary>
public enum FullScreenFill
{
    /// <summary>Nothing at all — the frame machinery with no content.</summary>
    Blank,

    /// <summary>Every cell, but the same value every frame: full paint cost, zero changed cells.</summary>
    Static,

    /// <summary>Every cell's glyph and both colours change every frame, and no two neighbours agree — the
    /// worst case for the renderer's run-coalescing.</summary>
    Animated,

    /// <summary>Every cell changes every frame, but in large flat-shaded bands that move — what a solid-shaded
    /// 3D scene actually looks like, where neighbouring cells usually share a style.</summary>
    Shaded,
}

/// <summary>
/// A control that fills its area with half-blocks carrying independent foreground and background RGB — the
/// <see cref="Globe"/> technique, and the surface the 3D sandbox's solid renderer will use. Content is a cheap
/// function of (x, y, frame) so the measurement prices the writes and the compositor, not the shading.
/// </summary>
public sealed class FullScreenControl : Control
{
    public FullScreenControl(FullScreenFill fill) => _fill = fill;

    /// <summary>Advanced once per frame; every cell's colour derives from it, so nothing repeats between frames.</summary>
    public int Tick
    {
        get => _tick;
        set { _tick = value; Invalidate(); }
    }

    protected override void Render()
    {
        if (_fill == FullScreenFill.Blank) return;

        int w = ActualWidth, h = ActualHeight;
        int t = _fill == FullScreenFill.Static ? 0 : _tick;
        // Shaded quantises position into 8-cell bands that SHIFT one cell per frame: neighbours within a band share
        // a style so the renderer can coalesce them, while the pattern still moves every frame. That is the spatial
        // coherence a flat-shaded scene has and the noise fill deliberately destroys.
        int q = _fill == FullScreenFill.Shaded ? 3 : 0;
        for (var y = 0; y < h; y++)
        {
            for (var x = 0; x < w; x++)
            {
                int bx = (x + t) >> q, by = (y + t) >> q;
                var top = new CColor((byte)bx, (byte)by, (byte)(bx + by));
                var bot = new CColor((byte)by, (byte)bx, (byte)(bx - by));
                consoleBuffer.Write(new Position(x, y), new CCharacter(Glyphs[(bx + by) & 3], top, bot));
            }
        }
    }

    // Varying the glyph as well as the colours keeps the diff honest: a colour-only change is a cheaper SGR
    // update than a change the renderer must also re-emit text for.
    private static readonly char[] Glyphs = ['▀', '▄', '█', '▌'];

    private readonly FullScreenFill _fill;
    private int _tick;
}

/// <summary>
/// The full-screen repaint ceiling, measured over the real <see cref="ConsoleManager"/> headlessly: median paint,
/// composite, changed cells and ANSI bytes per frame at several terminal sizes. Run via <c>-- --fullscreen</c>.
/// </summary>
public static class FullScreenDiagnostics
{
    /// <summary>A started headless session over a full-screen control, ready to be stepped a frame at a time.</summary>
    public sealed class Workload(FullScreenControl control)
    {
        public FullScreenControl Control { get; } = control;

        /// <summary>Advances the animation, then paints and composites one frame.</summary>
        public void Frame(int tick)
        {
            Control.Tick = tick;
            UI.PaintFrame();
            ConsoleManager.Draw();
        }
    }

    /// <summary>Builds the control and starts a headless compositor session on it. <paramref name="sink"/> receives
    /// the emitted ANSI (a no-op for timing, a counter for <see cref="Diagnose"/>).</summary>
    public static Workload Start(int width, int height, FullScreenFill fill, Func<object, Task> sink)
    {
        var control = new FullScreenControl(fill);
        ConsoleManager.AnsiEnabled = true;
        ConsoleManager.AnsiOutput = acsb => sink(acsb);
        ConsoleManager.Console = new NullConsole { Size = new Size(width, height) };
        ConsoleManager.Setup();
        ConsoleManager.Content = control;
        UI.PaintFrame();
        ConsoleManager.Draw();   // prime: the first frame is a full redraw whatever the fill
        return new Workload(control);
    }

    /// <summary>Prints the per-frame cost of a whole-area repaint at each size and fill.</summary>
    public static void Diagnose(string? sizes)
    {
        Console.WriteLine("full-screen repaint — the ceiling for an animated viewport (3D scene, video, ray-marcher)");
        Console.WriteLine("  blank    = paints nothing        (frame machinery only)");
        Console.WriteLine("  static   = every cell, unchanged (full paint, zero changed cells, nothing emitted)");
        Console.WriteLine("  animated = every cell, changed   (and no two neighbours agree: worst case for coalescing)");
        Console.WriteLine("  shaded   = every cell, changed   (in moving 8-cell bands: what a flat-shaded scene looks like)");
        Console.WriteLine("  (ConsoleManager emits asynchronously: the terminal pays for the bytes AFTER our frame.)");
        Console.WriteLine("  ('scanned' is cells RE-COMPOSITED, not cells emitted — a whole-area repainter always");
        Console.WriteLine("   scans everything. The bytes column is what actually reaches the terminal.)");

        foreach (var size in (sizes ?? "80x24,120x30,200x50,240x67").Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var (w, h) = ParseSize(size.Trim());
            Console.WriteLine($"\n{w}x{h} ({(long)w * h} cells):");
            foreach (var fill in Enum.GetValues<FullScreenFill>())
            {
                var r = Measure(w, h, fill);
                var frame = r.Paint + r.Draw;
                Console.WriteLine(
                    $"  {fill,-8}  paint {r.Paint,7:F1} us  draw {r.Draw,7:F1} us  frame {frame,7:F1} us  " +
                    $"({(frame > 0 ? 1e6 / frame : 0),6:F0} fps ceiling)  scanned {r.Cells,6}/{(long)w * h}  " +
                    $"ANSI {r.Bytes,8} B/frame ({r.Bytes * 60.0 / 1024:F0} KB/s at 60 fps)");
            }
        }
    }

    internal static (int Width, int Height) ParseSize(string size)
    {
        var parts = size.Split('x');
        return (int.Parse(parts[0]), int.Parse(parts[1]));
    }

    private static Result Measure(int width, int height, FullScreenFill fill)
    {
        long frameBytes = 0;
        var work = Start(width, height, fill, acsb => { frameBytes += acsb.ToString()!.Length; return Task.CompletedTask; });

        const int warmup = 50, n = 200;
        var paint = new List<double>(n);
        var draw = new List<double>(n);
        var cells = new List<long>(n);
        var bytes = new List<long>(n);
        var sw = new Stopwatch();
        for (var i = 1; i <= warmup + n; i++)
        {
            work.Control.Tick = i;
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
