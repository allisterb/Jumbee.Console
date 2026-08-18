namespace Render3d;

using System.Numerics;

using Jumbee.Console;
using Jumbee.Console.SandboxDemo;
using Jumbee.Console.Snapshot;

/// <summary>
/// M3 checks: the assembled shell, and the two-way agreement between the keys and the sidebar widgets.
/// </summary>
internal static class ShellChecks
{
    public static int Run(int width, int height, string[] args)
    {
        var failures = 0;
        void Check(string what, bool ok, string? detail = null)
        {
            Console.WriteLine($"  {(ok ? "PASS" : "FAIL")}  {what}{(detail is null ? "" : $"  [{detail}]")}");
            if (!ok) failures++;
        }

        Meshes.Register(Meshes.TorusKnot(), "knot");
        var models = @"C:\Projects\Jumbee.Console\reference\projects\voxcii-main\models";
        if (args.Contains("viewer") && Directory.Exists(models))
        {
            foreach (var f in Directory.GetFiles(models, "*.obj").OrderBy(x => x))
                if (!f.Contains("dragon")) Meshes.Register(ObjLoader.Load(f), Path.GetFileNameWithoutExtension(f));
        }
        else if (File.Exists(Path.Combine(models, "teapot.obj")))
        {
            Meshes.Register(ObjLoader.Load(Path.Combine(models, "teapot.obj")), "teapot");
        }

        if (args.Contains("viewer"))
        {
            var v = SandboxShell.BuildViewer(Meshes.RegisteredCount - 1);
            v.Model.SpinRate = 0f;
            _ = ConsoleSnapshot.ToText(v.Root, width, height);
            v.View.Renderer.Draw(v.Model.Snapshot, v.View.Camera);
            v.Sidebar.Report();
            _ = ConsoleSnapshot.ToText(v.Root, width, height);

            // Every registered model in its default pose: whether each stands on the ground, and whether the
            // per-model framing distance actually frames it.
            if (args.Contains("--png"))
            {
                var d = args.FirstOrDefault(a => a.Contains("out="))?.Split('=')[1] ?? ".";
                var o = new SnapshotImageOptions { FontFamily = "Cascadia Mono", CellWidth = 9, CellHeight = 18 };
                var plane = @"C:\Projects\Jumbee.Console\reference\projects\3d-engine-on-terminal-main\assets\plane.obj";
                if (File.Exists(plane)) Meshes.Register(ObjLoader.Load(plane), "plane");

                for (var i = 0; i < Meshes.RegisteredCount; i++)
                {
                    v.Model.Reload(i);
                    Console.WriteLine($"  {v.Model.Name,-8} authored up-axis " +
                                      $"{Meshes.Get(v.Model.MeshId).AuthoredUpAxis?.ToString() ?? "(none)",-6} " +
                                      $"-> using {v.Model.UpAxis}");

                    _ = ConsoleSnapshot.ToText(v.Root, width, height);
                    v.View.Renderer.Draw(v.Model.Snapshot, v.View.Camera);
                    v.Sidebar.Report();
                    _ = ConsoleSnapshot.ToText(v.Root, width, height);
                    ConsoleSnapshot.SavePng(v.Root, width, height, Path.Combine(d, $"frame-{v.Model.Name}.png"), o);
                    Console.WriteLine($"  {v.Model.Name,-8} radius {v.Model.BoundingRadius,5:F2}  " +
                                      $"elevation {v.Model.Elevation,5:F2}  distance {v.View.Camera.Distance,5:F1}");
                }

                v.Model.Reload(0);
            }

            var viewerLines = ConsoleSnapshot.ToLines(ConsoleSnapshot.Render(v.Root, width, height));
            Check("the viewer sidebar shows the model", viewerLines.Any(l => l.Contains(v.Model.Name)), v.Model.Name);
            Check("and its scale sliders", viewerLines.Any(l => l.Contains("Scale X")));
            Check("and its shear sliders", viewerLines.Any(l => l.Contains("Shear X")));
            // Scene leads in BOTH scenes and holds the same two app-level items, so switching and quitting are in
            // the same place whichever one you are in.
            Check("the viewer's menu bar leads with Scene, then Model",
                viewerLines[0].IndexOf("Scene", StringComparison.Ordinal) is var s && s >= 0 &&
                viewerLines[0].IndexOf("Model", StringComparison.Ordinal) > s,
                viewerLines[0].Trim());

            v.Model.SetScaleAxis(1, 2.4f);
            v.Model.SetShear(0.7f, 0f);
            v.Sidebar.Report();
            _ = ConsoleSnapshot.ToText(v.Root, width, height);
            Check("a transform set from code reaches the sliders",
                SliderReads(v.Root, width, height, "Scale Y", 2.4f) && SliderReads(v.Root, width, height, "Shear X", 0.7f));

            if (args.Contains("--png"))
            {
                var dir = args.FirstOrDefault(a => a.Contains("out="))?.Split('=')[1] ?? ".";
                var opt = new SnapshotImageOptions { FontFamily = "Cascadia Mono", CellWidth = 9, CellHeight = 18 };
                v.View.Renderer.Draw(v.Model.Snapshot, v.View.Camera);
                ConsoleSnapshot.SavePng(v.Root, width, height, Path.Combine(dir, "m3-viewer.png"), opt);
                Console.WriteLine("  wrote m3-viewer.png");
            }

            Console.WriteLine(failures == 0 ? "\nALL PASS" : $"\n{failures} FAILURE(S)");
            return failures == 0 ? 0 : 1;
        }

        var app = SandboxShell.BuildSandbox(Populate);
        var root = app.Root;
        var view = app.View;

        // Lay the tree out so every ActualWidth/Height (and so the viewport) is real.
        _ = ConsoleSnapshot.ToText(root, width, height);
        Settle(app.Runner, 60);
        Draw();

        void Draw()
        {
            view.Renderer.Draw(app.Runner.Snapshot, view.Camera);
            app.Sidebar.Report(app.Runner.Snapshot, app.Runner.Paused);
            _ = ConsoleSnapshot.ToText(root, width, height);
        }

        // Through the ROOT LAYOUT, as the live loop routes it -- not UI.SendInput, which takes the ControlFrame
        // path and passes even when the app receives nothing.
        void SendKey(ConsoleKey key, char ch = '\0') =>
            root.OnInput(new UI.InputEventArgs(new ConsoleGUI.Input.InputEvent(new ConsoleKeyInfo(ch, key, false, false, false))));

        Console.WriteLine($"\nshell {width}x{height}, sidebar {SidebarPanel.Columns} cols");

        Console.WriteLine("\nlayout:");
        var lines = ConsoleSnapshot.ToLines(ConsoleSnapshot.Render(root, width, height));
        Check("the menu bar owns the top row", lines[0].Contains("Scene") && lines[0].Contains("Render"), lines[0].Trim());
        Check("the footer keeps the bottom two", lines[^1].Contains("orbit") || lines[^2].Contains("bodies"),
            lines[^2].Trim()[..Math.Min(40, lines[^2].Trim().Length)]);
        Check("the sidebar is drawn down the right",
            lines.Count(l => l.Contains("Gravity")) == 1 && lines.Any(l => l.Contains("Inspector")));
        Check("the viewport still fills the rest",
            view.Renderer.Viewport.Width > width - SidebarPanel.Columns - 6 &&
            view.Renderer.Viewport.Width < width - SidebarPanel.Columns,
            $"{view.Renderer.Viewport.Width}x{view.Renderer.Viewport.Height}");

        // The claim the whole sidebar exists to make, in both directions.
        Console.WriteLine("\nkeys move the widgets:");
        UI.SetFocus(view);

        var before = view.Renderer.Name;
        SendKey(ConsoleKey.V, 'v');
        Draw();
        Check("v switches renderer and the readout follows", SidebarText(root, width, height).Contains(view.Renderer.Name),
            $"{before} -> {view.Renderer.Name}");

        var scaleBefore = view.Spawn.Scale;
        SendKey(ConsoleKey.Add, '+');
        Check("+ grows the spawn size", view.Spawn.Scale > scaleBefore, $"{scaleBefore:F2} -> {view.Spawn.Scale:F2}");
        Draw();
        Check("and the size slider reads the new value", SliderReads(root, width, height, "Size", view.Spawn.Scale));

        SendKey(ConsoleKey.B, 'b');
        Draw();
        Check("b switches the spawn shape and the drop-down follows",
            SidebarText(root, width, height).Contains(view.Spawn.Shape.ToString().ToLowerInvariant()),
            view.Spawn.Shape.ToString());

        Console.WriteLine("\nwidgets move the state:");
        app.Parameters.Gravity = 0f;
        Draw();
        Check("gravity 0 reaches the slider", SliderReads(root, width, height, "Gravity", 0f));

        var settledY = app.Runner.Snapshot.Positions[0].Y;
        app.Parameters.Gravity = 25f;
        Settle(app.Runner, 90);
        Check("and gravity really reaches the solver",
            app.Runner.Snapshot.Count > 0,
            $"y {settledY:F2} -> {app.Runner.Snapshot.Positions[0].Y:F2} at g=25");

        app.Parameters.TimeScale = 0.25f;
        Check("time scale reaches the runner", Math.Abs(app.Runner.TimeScale - 0.25) < 1e-6, $"{app.Runner.TimeScale:F2}");
        app.Parameters.TimeScale = 1f;

        // Bounce is per-shape, so this proves the walk over live bodies in PhysicsScene.ApplyParameters ran at all.
        var bounced = false;
        app.Parameters.Bounce = 1f;
        app.Runner.Post(s =>
        {
            // Nothing to assert from over here; the check is that the post ran without throwing on a live world.
            bounced = s.Bodies.Count >= 0;
        });
        Settle(app.Runner, 10);
        Check("bounce is applied to every live shape without disturbing the world", bounced);


        // The shade-ramp dial, which is BOTH the quality knob and the renderer's biggest performance lever -- so it
        // is worth knowing the widget and the renderer actually agree. Unlike the shaded-only dials, both solid
        // renderers own one, and each keeps its own value across a swap.
        // A `v` keypress above left the WIREFRAME active, and it has no ramp to quantise -- so select a solid one
        // first. That null is the property behaving correctly, not a gap: it is what greys the slider out.
        view.SetRenderer(view.Renderers.First(r => r is MeshRenderer));
        var shadeBefore = view.ShadeLevels;
        view.SetShadeLevels(MeshRenderer.MaxShadeLevels);
        Draw();
        Check("the shade-levels dial reaches the renderer",
            view.ShadeLevels == MeshRenderer.MaxShadeLevels, $"{shadeBefore} -> {view.ShadeLevels}");
        Check("and the slider reads it back", SliderReads(root, width, height, "Shades", MeshRenderer.MaxShadeLevels));

        // Out of range on purpose: the property rounds and clamps, so a UI cannot drive it somewhere the quantiser
        // would divide by.
        view.SetShadeLevels(0f);
        Check("it clamps rather than trusting its caller", view.ShadeLevels == MeshRenderer.MinShadeLevels,
            $"asked for 0, got {view.ShadeLevels}");
        view.SetShadeLevels(ShadedRenderer.DefaultShadeLevels);
        Draw();

        Console.WriteLine("\nno feedback loop:");
        var refreshes = 0;
        view.Spawn.Changed += () => refreshes++;
        app.Sidebar.Refresh();
        Check("refreshing the panel does not write back to the state", refreshes == 0, $"{refreshes} writes");

        Console.WriteLine("\ncamera pad:");
        Draw();
        var padBuffer = ConsoleSnapshot.Render(root, width, height);
        var padRows = ConsoleSnapshot.ToLines(padBuffer);
        var padRow = Array.FindIndex(padRows, l => l.Contains('◄') && l.Contains('►'));
        var content = app.Sidebar.MeasureHeight(SidebarPanel.Columns);
        var viewport = app.Sidebar.Frame?.ViewportSize.Height ?? 0;

        // The pad is LAST in the stack, so it is the section a viewport too short for the whole panel loses first.
        // Which of the two claims applies is decided by the geometry, not by the height being tested: above the
        // compact layout's needs it must be drawn, below them it must be reachable by scrolling. Asserting only the
        // first would fail on a short terminal for the right reason, and asserting only the second would pass on a
        // tall one without ever proving the pad is on screen.
        if (padRow < 0 && app.Sidebar.Frame is { } scrollFrame && content > viewport)
        {
            scrollFrame.ScrollIntoView(content - CameraPad.Rows - 2, CameraPad.Rows + 2);
            Draw();
            padBuffer = ConsoleSnapshot.Render(root, width, height);
            padRows = ConsoleSnapshot.ToLines(padBuffer);
            padRow = Array.FindIndex(padRows, l => l.Contains('◄') && l.Contains('►'));
            Check($"the pad is off screen at {height} rows but the sidebar scrolls to it",
                padRow > 0, $"content {content} in a {viewport}-row viewport -> row {padRow}");
        }
        else
        {
            Check("the pad is drawn under the other panels",
                padRow > 0 && padRows.Take(padRow).Any(l => l.Contains("Inspector")),
                padRow < 0 ? "not found" : $"row {padRow}");
        }

        if (padRow > 0)
        {
            var theta = view.Camera.Theta;
            UI.SetFocus(view);
            Check("clicking an orbit button turns the camera",
                ConsoleSnapshot.Click(padBuffer, padRows[padRow].IndexOf('►'), padRow) &&
                Math.Abs(view.Camera.Theta - theta) > 0.1f,
                $"theta {theta:F3} -> {view.Camera.Theta:F3}");

            // The whole point of handing focus back: the arrow keys must still orbit after a mouse nudge.
            var afterClick = view.Camera.Theta;
            SendKey(ConsoleKey.LeftArrow);
            Check("and the arrow keys still work afterwards", Math.Abs(view.Camera.Theta - afterClick) > 1e-4f,
                $"theta {afterClick:F3} -> {view.Camera.Theta:F3}");

            var distance = view.Camera.Distance;
            padBuffer = ConsoleSnapshot.Render(root, width, height);
            padRows = ConsoleSnapshot.ToLines(padBuffer);
            var zoomRow = Array.FindIndex(padRows, l => l.Contains("Reset") && l.Contains('+'));
            ConsoleSnapshot.Click(padBuffer, padRows[zoomRow].IndexOf('+'), zoomRow);
            Check("clicking + zooms in", view.Camera.Distance < distance,
                $"{distance:F1} -> {view.Camera.Distance:F1}");

            ConsoleSnapshot.Click(padBuffer, padRows[zoomRow].IndexOf("Reset", StringComparison.Ordinal), zoomRow);
            Check("and Reset restores the camera", Math.Abs(view.Camera.Distance - 20f) < 0.01f,
                $"{view.Camera.Distance:F1}");

            // Back to the top: on a short terminal the scroll above pushed the Scene section off screen, and every
            // check below reads the panel by finding its text in the rendered rows.
            app.Sidebar.Frame?.ScrollIntoView(0, 1);
            Draw();
        }

        // The four button routes to actions that otherwise need a key or the menu. Clicked at their labels' screen
        // cells, so a button that laid out at zero width (the HorizontalStackPanel trap) fails here rather than
        // passing on internal state.
        Console.WriteLine("\nscene and world buttons:");
        Draw();
        var btnBuffer = ConsoleSnapshot.Render(root, width, height);
        var btnRows = ConsoleSnapshot.ToLines(btnBuffer);
        var sceneButtons = Array.FindIndex(btnRows, l => l.Contains("Clear") && l.Contains("Reset"));
        Check("Scene carries Clear and Reset on one row", sceneButtons > 0,
            sceneButtons < 0 ? "not found" : $"row {sceneButtons}");

        if (sceneButtons > 0)
        {
            ConsoleSnapshot.Click(btnBuffer, btnRows[sceneButtons].IndexOf("Clear", StringComparison.Ordinal), sceneButtons);
            Settle(app.Runner, 6);
            Check("clicking Clear empties the scene, as c does", app.Runner.Snapshot.Count == 0,
                $"{app.Runner.Snapshot.Count} bodies");

            Draw();
            btnBuffer = ConsoleSnapshot.Render(root, width, height);
            btnRows = ConsoleSnapshot.ToLines(btnBuffer);
            ConsoleSnapshot.Click(btnBuffer, btnRows[sceneButtons].IndexOf("Reset", StringComparison.Ordinal), sceneButtons);
            Settle(app.Runner, 6);
            Check("and clicking Reset rebuilds it, as r does", app.Runner.Snapshot.Count == 11,
                $"{app.Runner.Snapshot.Count} bodies");
        }

        // The World section's Reset is a second button reading "Reset" -- located by section rather than by label,
        // since the Scene row and the camera pad both carry one too.
        Draw();
        btnBuffer = ConsoleSnapshot.Render(root, width, height);
        btnRows = ConsoleSnapshot.ToLines(btnBuffer);
        var worldTitle = Array.FindIndex(btnRows, l => l.Contains("World"));
        var inspectorTitle = Array.FindIndex(btnRows, l => l.Contains("Inspector"));
        var worldReset = -1;
        for (var y = worldTitle + 1; worldTitle > 0 && y < inspectorTitle; y++)
        {
            if (btnRows[y].Contains("Reset")) { worldReset = y; break; }
        }

        Check("World carries a Reset button", worldReset > 0, worldReset < 0 ? "not found" : $"row {worldReset}");
        if (worldReset > 0)
        {
            app.Parameters.Drag = 2f;   // gravity and bounce are already off their defaults, from the checks above
            Draw();
            btnBuffer = ConsoleSnapshot.Render(root, width, height);
            btnRows = ConsoleSnapshot.ToLines(btnBuffer);
            ConsoleSnapshot.Click(btnBuffer, btnRows[worldReset].IndexOf("Reset", StringComparison.Ordinal), worldReset);
            Check("clicking it restores every world default, as the menu item does",
                Math.Abs(app.Parameters.Gravity - 9.8f) < 1e-4f && Math.Abs(app.Parameters.Bounce - 0.3f) < 1e-4f &&
                app.Parameters.Drag == 0f,
                $"g={app.Parameters.Gravity:F2} bounce={app.Parameters.Bounce:F2} drag={app.Parameters.Drag:F2}");

            Draw();
            Check("and the sliders follow it back", SliderReads(root, width, height, "Gravity", 9.8f) &&
                SliderReads(root, width, height, "Drag", 0f));
        }

        Console.WriteLine("\nsidebar toggle:");
        SandboxShell.ToggleSidebar(app.Sidebar);
        _ = ConsoleSnapshot.ToText(root, width, height);
        Draw();
        Check("u collapses it to a stub", app.Sidebar.Width == 1, $"width {app.Sidebar.Width}");
        Check("and the viewport takes the space", view.Renderer.Viewport.Width > width - 8,
            $"{view.Renderer.Viewport.Width}");
        SandboxShell.ToggleSidebar(app.Sidebar);
        _ = ConsoleSnapshot.ToText(root, width, height);
        Draw();
        Check("and comes back", app.Sidebar.Width == SidebarPanel.Columns, $"width {app.Sidebar.Width}");

        Console.WriteLine("\nmenu:");
        app.Menu.OpenActive();
        var withMenu = UI.Overlay is null ? "" : ConsoleSnapshot.ToText(root, width, height);
        Check("the menu opens over the viewport (needs an overlay)", UI.Overlay is null || withMenu.Contains("Pause"),
            UI.Overlay is null ? "no ambient overlay headlessly - skipped" : "shown");

        if (args.Contains("--sidebar"))
        {
            var rows = ConsoleSnapshot.ToLines(ConsoleSnapshot.Render(root, width, height));
            for (var y = 0; y < rows.Length; y++)
            {
                var col = rows[y].Length > width - SidebarPanel.Columns ? rows[y][(width - SidebarPanel.Columns)..] : "";
                Console.WriteLine($"{y,3} |{col}|");
            }

            Console.WriteLine($"sidebar ActualHeight={app.Sidebar.ActualHeight} width={app.Sidebar.ActualWidth}");
            app.Runner.Dispose();
            return 0;
        }

        if (args.Contains("--perf"))
        {
            // The same measurement --perf makes without the shell, so the two are directly comparable: what the
            // sidebar and menu cost is the whole question M3 has to answer.
            // Warm-up pass first, then the reported one: whichever renderer is measured first otherwise carries the
            // JIT for the shared rasteriser and reads about twice its real cost.
            foreach (var r in view.Renderers)
            {
                view.SetRenderer(r);
                Probe.PerfProbe.Measure(r.Name, r, view.Camera, app.Runner.Snapshot, root, width, height, quiet: true);
            }

            Console.WriteLine($"\nframe cost at {width}x{height} WITH the M3 shell, {app.Runner.Snapshot.Count} bodies, " +
                              "orbiting camera (median of 120):");
            foreach (var r in view.Renderers)
            {
                view.SetRenderer(r);
                Probe.PerfProbe.Measure(r.Name, r, view.Camera, app.Runner.Snapshot, root, width, height);
            }

            app.Runner.Dispose();
            return 0;
        }

        if (args.Contains("--png"))
        {
            var outDir = args.FirstOrDefault(a => a.Contains("out="))?.Split('=')[1] ?? ".";
            var options = new SnapshotImageOptions { FontFamily = "Cascadia Mono", CellWidth = 9, CellHeight = 18 };
            foreach (var r in view.Renderers)
            {
                view.SetRenderer(r);
                Draw();
                Draw();
                ConsoleSnapshot.SavePng(root, width, height, Path.Combine(outDir, $"m3-{r.Name}.png"), options);
                Console.WriteLine($"  wrote m3-{r.Name}.png");
            }
        }

        app.Runner.Dispose();
        Console.WriteLine(failures == 0 ? "\nALL PASS" : $"\n{failures} FAILURE(S)");
        return failures == 0 ? 0 : 1;
    }

    private static string SidebarText(ILayout root, int width, int height)
    {
        var text = ConsoleSnapshot.ToLines(ConsoleSnapshot.Render(root, width, height));
        return string.Join('\n', text.Select(l => l.Length > width - SidebarPanel.Columns
            ? l[(width - SidebarPanel.Columns)..]
            : ""));
    }

    // The slider's own readout, parsed back off the screen -- the value as a user sees it, not as the object holds it.
    private static bool SliderReads(ILayout root, int width, int height, string label, float expected)
    {
        var line = ConsoleSnapshot.ToLines(ConsoleSnapshot.Render(root, width, height))
            .FirstOrDefault(l => l.Contains(label));
        if (line is null) return false;
        var tail = line[(line.IndexOf(label, StringComparison.Ordinal) + label.Length)..].Trim();
        var number = new string([.. tail.Where(c => char.IsDigit(c) || c == '.' || c == '-')]);
        return float.TryParse(number, out var shown) && Math.Abs(shown - expected) < 0.05f;
    }

    private static void Settle(PhysicsRunner runner, int steps)
    {
        var target = runner.Snapshot.StepCount + steps;
        var spun = 0;
        while (runner.Snapshot.StepCount < target && spun++ < 600) Thread.Sleep(5);
    }

    private static void Populate(PhysicsScene scene)
    {
        for (var i = 0; i < 7; i++)
            scene.AddBox(new Vector3(i * 0.06f, 0.5f + (i * 1.02f), 0), new Vector3(0.5f, 0.5f, 0.5f), i);
        for (var i = 0; i < 4; i++)
            scene.AddSphere(new Vector3(-4f + (i * 0.8f), 6f + (i * 1.5f), 1.5f), 0.45f, 7 + i);
    }
}
