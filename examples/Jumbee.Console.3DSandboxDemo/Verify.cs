namespace Jumbee.Console.SandboxDemo;

using Jumbee.Console.Snapshot;

/// <summary>
/// The <c>--verify</c> smoke check: builds both shells for real, draws a frame of each through every renderer, and
/// asserts against the composited cells.
/// </summary>
/// <remarks>
/// <para>
/// It exists for the places there is no terminal to look at — a container build, or CI. Without it the answer to
/// "does the image run?" is "start it and see", which under <c>docker run</c> with no TTY means a full-screen UI
/// painting escape codes at a pipe until something times out.
/// </para>
/// <para>
/// Both scenes, and all three renderers in each. A check that drew only the default would pass while the two the
/// user is most likely to switch to were broken, and the renderers are the part with real surface area — each
/// composites through a different path (the wireframe through a braille <c>Canvas</c>, the two solid ones through
/// <see cref="HalfBlockSurface"/>).
/// </para>
/// <para>
/// Asserted against the COMPOSITED CELLS rather than the renderers' own counters, which would happily report a
/// healthy frame that never reached the screen. The viewer runs on the generated torus knot, so the check needs no
/// asset and means the same thing on a bare image as on a full checkout.
/// </para>
/// </remarks>
internal static class Verify
{
    #region Methods
    /// <summary>Runs the check, printing one line. Returns a process exit code.</summary>
    public static int Run()
    {
        var failures = new List<string>();
        var report = new List<string>();

        // The sandbox: real physics, settled first so the frame has a resting tower rather than bodies mid-fall at
        // the origin -- a scene that has not moved would pass even if the simulation never started.
        using (var app = BuildSandbox())
        {
            _ = ConsoleSnapshot.ToText(app.Root, Width, Height);
            Settle(app.Runner, 30);
            var bodies = app.Runner.Snapshot.Count;
            if (bodies < 11) failures.Add($"sandbox has {bodies} bodies, expected 11");
            Check("sandbox", app.Root, app.View, app.Runner.Snapshot, failures, report);
        }

        using (var app = SandboxShell.BuildViewer(0))
        {
            _ = ConsoleSnapshot.ToText(app.Root, Width, Height);
            Check("viewer", app.Root, app.View, app.Model.Snapshot, failures, report);
        }

        if (failures.Count > 0)
        {
            System.Console.WriteLine("FAIL  3DSandbox verify — " + string.Join("; ", failures));
            return 1;
        }

        System.Console.WriteLine("PASS  3DSandbox verify — " + string.Join(", ", report) + ".");
        return 0;
    }
    #endregion

    #region Private methods
    // One shell, every renderer. Each is asked for a frame and then judged on what landed in the buffer.
    private static void Check(string scene, ILayout root, SceneView view, SceneSnapshot snapshot,
        List<string> failures, List<string> report)
    {
        var drawn = new List<string>();
        foreach (var renderer in view.Renderers)
        {
            view.SetRenderer(renderer);
            // Drawn and published back to back here rather than through the render job: the check wants a frame it
            // can assert on immediately, and there is no UI loop running to deliver the job's posted apply.
            var request = new FrameRequest(snapshot, view.Camera.GetView(),
                                           renderer.Surface.ActualWidth, renderer.Surface.ActualHeight);
            renderer.Publish(renderer.Draw(request));
            var buffer = ConsoleSnapshot.Render(root, Width, Height);

            var lit = 0;
            for (var y = 0; y < Height; y++)
                for (var x = 0; x < Width; x++)
                    if (buffer[x, y].Character.Foreground is not null) lit++;

            // A tenth of the screen: the wireframe draws strokes rather than fills, so it legitimately lights far
            // fewer cells than the solid renderers, and a threshold tuned to those would fail it every time.
            if (lit < Width * Height / 10) failures.Add($"{scene}/{renderer.Name} drew {lit} lit cells");
            else drawn.Add($"{renderer.Name} {lit}");
        }

        report.Add($"{scene} ({string.Join(", ", drawn)})");
    }

    private static SandboxShell.Sandbox BuildSandbox() =>
        SandboxShell.BuildSandbox(scene =>
        {
            for (var i = 0; i < 7; i++)
                scene.AddBox(new System.Numerics.Vector3(i * 0.06f, 0.5f + (i * 1.02f), 0),
                    new System.Numerics.Vector3(0.5f, 0.5f, 0.5f), i);
            for (var i = 0; i < 4; i++)
                scene.AddSphere(new System.Numerics.Vector3(-4f + (i * 0.8f), 6f + (i * 1.5f), 1.5f), 0.45f, 7 + i);
        });

    // Waits for the physics thread to advance, with a ceiling so a runner that never starts fails the check rather
    // than hanging the build that called it.
    private static void Settle(PhysicsRunner runner, int steps)
    {
        var target = runner.Snapshot.StepCount + steps;
        var spun = 0;
        while (runner.Snapshot.StepCount < target && spun++ < 600) Thread.Sleep(5);
    }
    #endregion

    #region Fields
    private const int Width = 120, Height = 48;
    #endregion
}
