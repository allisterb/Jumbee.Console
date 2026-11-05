namespace Render3d;

using System.Diagnostics;
using System.Numerics;
using System.Text;

using Jumbee.Console;
using Jumbee.Console.Mesh;
using Jumbee.Console.SandboxDemo;
using Jumbee.Console.Snapshot;

/// <summary>
/// Writes each shell's sidebar, cell by cell, to text files, so two builds can be diffed for "visually unchanged".
/// </summary>
/// <remarks>
/// <para>
/// Not a check: a before/after instrument. Each row is written twice: as text, and as a hash of every cell's
/// character, foreground and background, so a colour-only change (a control greyed out, a different border colour)
/// shows up as a differing row even when the text matches.
/// </para>
/// <para>
/// The sandbox's PNGs can't be compared by hash because physics settles differently on every run; the sidebar is
/// deterministic apart from its readout lines (the clock, the body counts), and those are easy to spot in a diff.
/// It is dumped under each renderer, because the renderer decides which controls are greyed out, in both of the
/// sandbox's layout tiers, and in the viewer over a model with no materials and over one with them.
/// </para>
/// </remarks>
internal static class SidebarDump
{
    public static int Run(string[] args)
    {
        var dir = args.FirstOrDefault(a => a.StartsWith("out="))?[4..] ?? ".";
        Directory.CreateDirectory(dir);

        var library = new MeshLibrary();
        library.Add(Meshes.TorusKnot(), "knot");
        if (RepoPaths.Optional("media", "models", "capsule.obj") is { } capsule) library.Add(ModelLoader.Load(capsule), "capsule");

        // The sandbox's two tiers: SpacedRows is 63 viewport rows, so 80 total is spaced and 48 is compact.
        foreach (var (w, h) in new[] { (200, 80), (200, 48) })
        {
            using var app = SandboxShell.BuildSandbox(library, Populate);
            Settle(app.Runner, 60);
            app.Runner.Paused = true;
            Dump(dir, $"sandbox-{w}x{h}", app.Root, app.View, w, h, () => app.Sidebar.Report(app.Runner.Frame, true));
        }

        using (var app = SandboxShell.BuildViewer(library, 0))
        {
            app.Model.SpinRate = 0f;
            Dump(dir, "viewer-knot", app.Root, app.View, 200, 60, app.Sidebar.Refresh);
            if (library.Count > 1)
            {
                app.Model.Step(+1);
                Dump(dir, "viewer-capsule", app.Root, app.View, 200, 60, app.Sidebar.Refresh);
            }
        }

        Console.WriteLine($"  wrote sidebar dumps to {dir}");
        return 0;
    }

    private static void Dump(string dir, string name, ILayout root, MeshView view, int width, int height, Action report)
    {
        foreach (var renderer in view.Renderers)
        {
            view.SetRenderer(renderer);
            _ = ConsoleSnapshot.ToText(root, width, height);
            report();
            _ = ConsoleSnapshot.ToText(root, width, height);
            var buffer = ConsoleSnapshot.Render(root, width, height);

            // The sidebar is docked right at its fixed width.
            var left = width - SidebarPanel.Columns;
            var sb = new StringBuilder();
            for (var y = 0; y < height; y++)
            {
                var text = new StringBuilder();
                var hash = 2166136261u;   // FNV-1a: HashCode is seeded per process, so it can't compare runs
                for (var x = left; x < width; x++)
                {
                    var c = buffer[x, y].Character;
                    text.Append(c.Content is { } ch && ch != '\0' ? ch : ' ');
                    foreach (var v in new[] { (int)(c.Content ?? '\0'), Rgb(c.Foreground), Rgb(c.Background) })
                        hash = (hash ^ (uint)v) * 16777619u;
                }

                sb.Append($"{y,2} {hash:x8} |{text}|\n");
            }

            File.WriteAllText(Path.Combine(dir, $"{name}-{renderer.Name}.txt"), sb.ToString());
        }
    }

    private static int Rgb(ConsoleGUI.Data.Color? c) => c is { } v ? (v.Red << 16) | (v.Green << 8) | v.Blue : -1;

    private static void Settle(PhysicsRunner runner, int steps)
    {
        var target = runner.Frame.StepCount + steps;
        var clock = Stopwatch.StartNew();
        while (runner.Frame.StepCount < target && clock.ElapsedMilliseconds < 5000) Thread.Sleep(5);
    }

    private static void Populate(PhysicsScene scene)
    {
        for (var i = 0; i < 7; i++)
            scene.AddBox(new Vector3(i * 0.06f, 0.5f + (i * 1.02f), 0), new Vector3(0.5f), i);
    }
}
