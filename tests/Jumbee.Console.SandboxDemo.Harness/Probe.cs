namespace Probe;

using System.Numerics;

using Jumbee.Console.SandboxDemo;

/// <summary>
/// Renders the solid view and prints it as an ASCII luminance map (two rows per character row, since each cell
/// carries two sub-pixels) — a terminal-independent way to check the GEOMETRY is right, not just that cells are lit.
/// </summary>
public static class SolidProbe
{
    public static void Dump(Jumbee.Console.ConsoleBuffer buffer, int width, int height)
    {
        const string Ramp = " .:-=+*#%@";
        for (var row = 0; row < height; row++)
        {
            var top = new System.Text.StringBuilder(width);
            var bottom = new System.Text.StringBuilder(width);
            for (var x = 0; x < width; x++)
            {
                var ch = buffer[x, row].Character;
                if (ch.Content is null) { top.Append(' '); bottom.Append(' '); continue; }
                if (ch.Content != '▀')
                {
                    // An edge cell: show the actual glyph on both sub-rows so the outline is visible in the dump.
                    top.Append(ch.Content.Value);
                    bottom.Append(ch.Content.Value);
                    continue;
                }

                top.Append(Ramp[Level(ch.Foreground)]);
                bottom.Append(Ramp[Level(ch.Background)]);
            }

            Console.WriteLine(top);
            Console.WriteLine(bottom);
        }

        static int Level(ConsoleGUI.Data.Color? c)
        {
            if (c is not { } v) return 0;
            var lum = ((0.299 * v.Red) + (0.587 * v.Green) + (0.114 * v.Blue)) / 255.0;
            return Math.Clamp((int)(lum * 9.999), 0, 9);
        }
    }
}

/// <summary>Frame cost of a renderer over the real compositor: scene draw, paint, composite+emit, and ANSI bytes.</summary>
public static class PerfProbe
{
    public static void Measure(string label, ISceneRenderer renderer, OrbitCamera camera, SceneSnapshot snapshot,
                               Jumbee.Console.ILayout root, int w, int h, bool quiet = false)
    {
        long frameBytes = 0;
        ConsoleGUI.ConsoleManager.AnsiEnabled = true;
        ConsoleGUI.ConsoleManager.AnsiOutput = acsb => { frameBytes += acsb.ToString()!.Length; return Task.CompletedTask; };
        ConsoleGUI.ConsoleManager.Console = new NullConsole { Size = new ConsoleGUI.Space.Size(w, h) };
        ConsoleGUI.ConsoleManager.Setup();
        ConsoleGUI.ConsoleManager.Content = root.CControl;
        Jumbee.Console.UI.PaintFrame();
        ConsoleGUI.ConsoleManager.Draw();

        const int Warmup = 20, N = 120;
        var draws = new List<double>(N);
        var paints = new List<double>(N);
        var emits = new List<double>(N);
        var bytes = new List<long>(N);
        var sw = new System.Diagnostics.Stopwatch();
        for (var i = 1; i <= Warmup + N; i++)
        {
            camera.Orbit(0.01f, 0);   // a static camera would let the per-cell diff skip nearly everything
            sw.Restart(); renderer.Draw(snapshot, camera); sw.Stop();
            var d = sw.Elapsed.TotalMicroseconds;
            sw.Restart(); Jumbee.Console.UI.PaintFrame(); sw.Stop();
            var p = sw.Elapsed.TotalMicroseconds;
            frameBytes = 0;
            sw.Restart(); ConsoleGUI.ConsoleManager.Draw(); sw.Stop();
            ConsoleGUI.ConsoleManager.OutputIdle.GetAwaiter().GetResult();
            var e = sw.Elapsed.TotalMicroseconds;
            if (i <= Warmup) continue;
            draws.Add(d); paints.Add(p); emits.Add(e); bytes.Add(frameBytes);
        }

        draws.Sort(); paints.Sort(); emits.Sort(); bytes.Sort();
        var total = draws[N / 2] + paints[N / 2] + emits[N / 2];
        if (quiet) return;
        Console.WriteLine($"  {label,-10}  scene {draws[N / 2],7:F0}us  paint {paints[N / 2],7:F0}us  " +
                          $"emit {emits[N / 2],7:F0}us  TOTAL {total,7:F0}us  " +
                          $"({total / 16666.0 * 100,4:F0}% of a 60fps frame)  ANSI {bytes[N / 2],7} B/frame");
    }

    private sealed class NullConsole : ConsoleGUI.Api.IConsole
    {
        public ConsoleGUI.Space.Size Size { get; set; }
        public bool KeyAvailable => false;
        public void Initialize() { }
        public void OnRefresh() { }
        public void Write(ConsoleGUI.Space.Position position, in ConsoleGUI.Data.Character character) { }
        public ConsoleKeyInfo ReadKey() => throw new NotSupportedException();
    }
}

/// <summary>Measures how big a launched body is on screen, frame by frame, to explain what 'fire' looks like.</summary>
public static class LaunchProbe
{
    public static void Run(int viewportWidth, int viewportHeight)
    {
        var camera = new OrbitCamera();
        var projection = new Projection(60f);
        var viewport = new Viewport(viewportWidth, viewportHeight);
        var view = camera.GetView();

        var settings = new SpawnSettings { Shape = BodyShape.Sphere };
        var Radius = settings.BoundingRadius;
        const float Speed = 20f;
        const float Dt = 1f / 60f;

        // Mirrors SceneView.Launch: the muzzle distance is derived from the body's size.
        var muzzle = MathF.Max(2f, projection.Focal * settings.BoundingRadius / 0.2f);

        Console.WriteLine($"camera eye {view.Eye:F2}, target {camera.Target:F2}, distance {camera.Distance:F1}");
        Console.WriteLine($"viewport {viewport.Width}x{viewport.Height}, NDC y spans +/-{viewport.CellAspect:F3}");
        Console.WriteLine($"launch: {settings.Shape} r={Radius:F2} at eye + forward*{muzzle:F2}, speed {Speed} m/s\n");
        Console.WriteLine("  frame   dist    ndc radius   % of viewport height   rows tall");

        var position = view.Eye + (view.Forward * muzzle);
        var velocity = view.Forward * Speed;
        for (var frame = 0; frame <= 12; frame++)
        {
            var camSpace = view.Transform(position);
            var distance = camSpace.Z;
            if (projection.TryProject(camSpace, out var cx, out var cy)
                && projection.TryProject(view.Transform(position + (view.Right * Radius)), out var ex, out var ey))
            {
                var ndcRadius = MathF.Sqrt(((ex - cx) * (ex - cx)) + ((ey - cy) * (ey - cy)));
                var fraction = ndcRadius / (float)viewport.CellAspect;      // 1.0 = half the viewport height
                var rows = fraction * viewport.Height;
                Console.WriteLine($"  {frame,5}  {distance,6:F2}   {ndcRadius,10:F3}   {fraction * 100,18:F0}%   {rows,9:F0}");
            }
            else
            {
                Console.WriteLine($"  {frame,5}  {distance,6:F2}   (behind the near plane -- not drawn)");
            }

            // Gravity is irrelevant over these few frames; straight-line is close enough to show the size curve.
            position += velocity * Dt;
        }
    }
}
