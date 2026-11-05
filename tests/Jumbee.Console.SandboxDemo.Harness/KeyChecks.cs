namespace Render3d;

using System.Diagnostics;
using System.Numerics;

using ConsoleGUI.Space;

using Jumbee.Console;
using Jumbee.Console.Mesh;
using Jumbee.Console.SandboxDemo;
using Jumbee.Console.Snapshot;

/// <summary>
/// Every line of each viewport's F1 help, driven through the live input route and checked for the effect its
/// description promises — in the real sandbox and viewer shells.
/// </summary>
/// <remarks>
/// <para>
/// Driven from the help rather than from a list of its own: each help line is looked up in a table of checks, and a
/// line with no check FAILS. So adding a key without a check, or a help line for a key that does nothing, cannot pass
/// quietly — which is exactly how the viewer came to list the sandbox's spawn keys, none of which worked there.
/// </para>
/// <para>
/// Keys go through the ROOT LAYOUT, as the live loop routes them, not through <c>UI.SendInput</c>, which takes the
/// ControlFrame path and passes while the app receives nothing (memory: composite-key-routing-trap). The one global
/// hotkey on the list, <c>u</c>, goes through the global dispatch, which is where the live loop sends it first.
/// </para>
/// </remarks>
internal static class KeyChecks
{
    public static int Run(int width, int height, string[] args)
    {
        var failures = 0;
        void Check(string what, bool ok, string? detail = null)
        {
            Console.WriteLine($"  {(ok ? "PASS" : "FAIL")}  {what}{(detail is null ? "" : $"  [{detail}]")}");
            if (!ok) failures++;
        }

        // Two meshes, so stepping the model and cycling the spawn mesh both have somewhere to go.
        var library = new MeshLibrary();
        library.Add(Meshes.TorusKnot(), "knot");
        library.Add(Meshes.TorusKnot(p: 3, q: 5), "knot-35");

        Console.WriteLine($"sandbox {width}x{height}:");
        using (var app = SandboxShell.BuildSandbox(library, Populate))
        {
            var (root, view, runner) = (app.Root, app.View, app.Runner);
            Settle(runner, 120);
            Draw(root, view, width, height);
            UI.SetFocus(view);

            int Count() => runner.Frame.Count;
            var checks = new Dictionary<string, Func<(bool, string)>>
            {
                ["Arrows"] = () => Arrows(root, view),
                ["PgUp/PgDn"] = () => Zoom(root, view),
                ["Home"] = () => Home(root, view, 20f),
                ["Drag"] = () =>
                {
                    if (FindBody(root, view, width, height) is not var (cell, id, index)) return (false, "no body on screen");
                    var before = runner.Frame.Positions[index];
                    var dragged = ConsoleSnapshot.Drag(ConsoleSnapshot.Render(root, width, height), cell.X, cell.Y, cell.X + 8, cell.Y - 3, steps: 6);
                    var after = Settle(runner, 30);
                    var i = after.IndexOf(id);
                    var moved = i >= 0 ? Vector3.Distance(before, after.Positions[i]) : -1f;
                    Draw(root, view, width, height);
                    return (dragged && view.Selected == id && moved > 0.3f, $"grabbed #{id}, moved {moved:F2}");
                },
                ["Click"] = () =>
                {
                    view.Selected = null;
                    if (FindBody(root, view, width, height) is not var (cell, id, _)) return (false, "no body on screen");
                    var clicked = ConsoleSnapshot.Click(ConsoleSnapshot.Render(root, width, height), cell.X, cell.Y);
                    Settle(runner, 30);
                    Draw(root, view, width, height);
                    return (clicked && view.Selected == id, $"selected {view.Selected?.ToString() ?? "none"}, expected #{id}");
                },
                ["Tab / Esc"] = () =>
                {
                    view.Selected = null;
                    Send(root, ConsoleKey.Tab);
                    var first = view.Selected;
                    Send(root, ConsoleKey.Tab);
                    var second = view.Selected;
                    Send(root, ConsoleKey.Tab, shift: true);
                    var back = view.Selected;
                    Send(root, ConsoleKey.Escape);
                    return (first is not null && second is not null && second != first && back == first && view.Selected is null,
                        $"{first} -> {second} -> (Shift) {back} -> (Esc) {view.Selected?.ToString() ?? "none"}");
                },
                ["b"] = () =>
                {
                    var before = view.Spawn.Shape;
                    Send(root, ConsoleKey.B, 'b');
                    return (view.Spawn.Shape != before, $"{before} -> {view.Spawn.Shape}");
                },
                ["m"] = () =>
                {
                    var before = view.Spawn.MeshIndex;
                    Send(root, ConsoleKey.M, 'm');
                    return (view.Spawn.MeshIndex != before && view.Spawn.Shape == BodyShape.Mesh, $"{before} -> {view.Spawn.MeshIndex} ({view.Spawn.ShapeName})");
                },
                ["n"] = () =>
                {
                    var before = Count();
                    Send(root, ConsoleKey.N, 'n');
                    var after = Settle(runner, 10);
                    var top = after.Positions[^1].Y;
                    return (after.Count == before + 1 && top > view.Camera.Target.Y, $"{before} -> {after.Count} bodies, new one at y={top:F1}");
                },
                ["f"] = () =>
                {
                    var before = Count();
                    Send(root, ConsoleKey.F, 'f');
                    var after = Settle(runner, 2);
                    var along = after.Count > 0 ? Vector3.Dot(Vector3.Normalize(after.Velocities[^1]), view.Camera.GetView().Forward) : 0f;
                    return (after.Count == before + 1 && along > 0.9f, $"{before} -> {after.Count} bodies, {along:F2} along the view");
                },
                ["Del / x"] = () =>
                {
                    var before = Count();
                    view.Selected = runner.Frame.Ids[0];
                    Send(root, ConsoleKey.Delete);
                    var afterDel = Settle(runner, 5).Count;
                    view.Selected = runner.Frame.Ids[0];
                    Send(root, ConsoleKey.X, 'x');
                    var afterX = Settle(runner, 5).Count;
                    return (afterDel == before - 1 && afterX == before - 2, $"{before} -> (Del) {afterDel} -> (x) {afterX}");
                },
                ["c"] = () =>
                {
                    Send(root, ConsoleKey.C, 'c');
                    return (Settle(runner, 5).Count == 0, $"{runner.Frame.Count} bodies left");
                },
                ["v"] = () => NextRenderer(root, view),
                ["e"] = () => Edges(root, view),
                ["w"] = () => Wrap(root, view),
                ["+ / -"] = () =>
                {
                    var s0 = view.Spawn.Scale;
                    Send(root, ConsoleKey.Add, '+');
                    var s1 = view.Spawn.Scale;
                    Send(root, ConsoleKey.Subtract, '-');
                    return (s1 > s0 && Math.Abs(view.Spawn.Scale - s0) < 1e-4f, $"{s0:F2} -> {s1:F2} -> {view.Spawn.Scale:F2}");
                },
                ["] / ["] = () =>
                {
                    var v0 = view.Spawn.LaunchSpeed;
                    Send(root, ConsoleKey.Oem6, ']');
                    var v1 = view.Spawn.LaunchSpeed;
                    Send(root, ConsoleKey.Oem4, '[');
                    return (v1 > v0 && Math.Abs(view.Spawn.LaunchSpeed - v0) < 1e-4f, $"{v0:F1} -> {v1:F1} -> {view.Spawn.LaunchSpeed:F1}");
                },
                ["u"] = () => Sidebar(root, app.Sidebar),
            };
            RunHelp(view, checks, Check);
        }

        Console.WriteLine($"\nviewer {width}x{height}:");
        using (var app = SandboxShell.BuildViewer(library, 0))
        {
            var (root, view, model) = (app.Root, app.View, app.Model);
            model.SpinRate = 0f;
            Draw(root, view, width, height);
            UI.SetFocus(view);

            var checks = new Dictionary<string, Func<(bool, string)>>
            {
                ["Arrows"] = () => Arrows(root, view),
                ["PgUp/PgDn"] = () => Zoom(root, view),
                // Home is the FRAMED shot here, not the camera's built-in default distance.
                ["Home"] = () => Home(root, view, view.Camera.HomeDistance),
                ["Drag"] = () =>
                {
                    var origin = ViewOrigin(root, width, height);
                    var (cx, cy) = (origin.X + (view.ActualWidth / 2), origin.Y + (view.ActualHeight / 2));
                    var theta = view.Camera.Theta;
                    var dragged = ConsoleSnapshot.Drag(ConsoleSnapshot.Render(root, width, height), cx, cy, cx + 10, cy, steps: 5);
                    return (dragged && Math.Abs(view.Camera.Theta - theta) > 1e-3f && view.Selected is null,
                        $"theta {theta:F3} -> {view.Camera.Theta:F3}, selected {view.Selected?.ToString() ?? "none"}");
                },
                ["v"] = () => NextRenderer(root, view),
                ["e"] = () => Edges(root, view),
                ["w"] = () => Wrap(root, view),
                ["[ / ]"] = () =>
                {
                    var n0 = model.Name;
                    Send(root, ConsoleKey.Oem6, ']');
                    var n1 = model.Name;
                    Send(root, ConsoleKey.Oem4, '[');
                    return (n1 != n0 && model.Name == n0, $"{n0} -> {n1} -> {model.Name}");
                },
                ["x X  y Y  z Z"] = () =>
                {
                    model.ResetTransform();
                    var detail = "";
                    var ok = true;
                    foreach (var (lower, upper, key, axis) in new[] { ('x', 'X', ConsoleKey.X, 0), ('y', 'Y', ConsoleKey.Y, 1), ('z', 'Z', ConsoleKey.Z, 2) })
                    {
                        Send(root, key, lower);
                        var shrunk = Component(model.Scale, axis);
                        Send(root, key, upper, shift: true);
                        Send(root, key, upper, shift: true);
                        var grown = Component(model.Scale, axis);
                        var others = Enumerable.Range(0, 3).Where(a => a != axis).All(a => Component(model.Scale, a) == 1f);
                        ok &= shrunk < 1f && grown > 1f && others;
                        detail += $"{upper}: {shrunk:F2}/{grown:F2} ";
                        model.ResetTransform();
                    }
                    return (ok, detail.Trim());
                },
                [", .  ; '"] = () =>
                {
                    model.ResetTransform();
                    Send(root, ConsoleKey.OemComma, ',');
                    var left = model.Shear.X;
                    Send(root, ConsoleKey.OemPeriod, '.');
                    Send(root, ConsoleKey.OemPeriod, '.');
                    var right = model.Shear.X;
                    Send(root, ConsoleKey.Oem1, ';');
                    var back = model.Shear.Y;
                    Send(root, ConsoleKey.Oem7, '\'');
                    Send(root, ConsoleKey.Oem7, '\'');
                    var fwd = model.Shear.Y;
                    return (left < 0 && right > 0 && back < 0 && fwd > 0, $"X {left:+0.00;-0.00} -> {right:+0.00;-0.00}, Z {back:+0.00;-0.00} -> {fwd:+0.00;-0.00}");
                },
                ["0"] = () =>
                {
                    model.ScaleAxis(1, 1.5f);
                    model.Nudge(0.3f, 0.3f);
                    Send(root, ConsoleKey.D0, '0');
                    return (model.Scale == Vector3.One && model.Shear == Vector2.Zero, $"scale {model.Scale}, shear {model.Shear}");
                },
                ["a"] = () =>
                {
                    var before = model.UpAxis;
                    Send(root, ConsoleKey.A, 'a');
                    var flipped = model.UpAxis;
                    Send(root, ConsoleKey.A, 'a');
                    return (flipped != before && model.UpAxis == before, $"{before} -> {flipped} -> {model.UpAxis}");
                },
                ["p"] = () =>
                {
                    var before = model.SpinRate;
                    Send(root, ConsoleKey.P, 'p');
                    var toggled = model.SpinRate;
                    Send(root, ConsoleKey.P, 'p');
                    return ((before == 0f) != (toggled == 0f) && model.SpinRate == before, $"{before} -> {toggled} -> {model.SpinRate}");
                },
                ["u"] = () => Sidebar(root, app.Sidebar),
            };
            RunHelp(view, checks, Check);

            // The other half of "the help says what the keys do": keys that belong to the sandbox must not be taken
            // here. Taken, they did nothing visible, and Tab in particular trapped focus in the viewport.
            var passed = new[] { (ConsoleKey.N, 'n'), (ConsoleKey.F, 'f'), (ConsoleKey.C, 'c'), (ConsoleKey.B, 'b'),
                                 (ConsoleKey.Delete, '\0'), (ConsoleKey.Tab, '\0'), (ConsoleKey.Escape, '\0') }
                .Where(k => !Send(root, k.Item1, k.Item2)).Select(k => k.Item2 == '\0' ? k.Item1.ToString() : k.Item2.ToString()).ToArray();
            Check("the sandbox's keys pass through the viewer untaken", passed.Length == 7, string.Join(" ", passed));
        }

        // The one-liner: a mesh and nothing else must be a working viewer.
        Console.WriteLine("\none-liner:");
        using (var one = new MeshView(Meshes.TorusKnot()))
        using (var none = new MeshView(new ModelScene(new MeshLibrary()), new ShadedRenderer()))
        {
            // Cells in the model's colour, against the same view of an EMPTY scene. Not "non-blank cells": the
            // shaded renderer fills every cell with sky and floor, so that count is all of them either way. The
            // ground and sky are near-grey; the model is tinted, so saturation is what only the model can supply. Mint
            // is a soft tint, so only its brightest faces clear the bar: measured 50 at 80x30, against 0 with no model.
            int Tinted(MeshView v)
            {
                _ = ConsoleSnapshot.ToText(v, 80, 30);
                v.Renderer.Draw(v.Source.Frame, v.Camera);
                var b = ConsoleSnapshot.Render(v, 80, 30);
                var n = 0;
                for (var y = 0; y < 30; y++)
                    for (var x = 0; x < 80; x++)
                        if (b[x, y].Character.Foreground is { } c
                            && Math.Max(c.Red, Math.Max(c.Green, c.Blue)) - Math.Min(c.Red, Math.Min(c.Green, c.Blue)) > 60) n++;
                return n;
            }

            var (withMesh, empty) = (Tinted(one), Tinted(none));
            if (args.FirstOrDefault(a => a.StartsWith("out="))?[4..] is { } dir)
                ConsoleSnapshot.SavePng(one, 80, 30, Path.Combine(dir, "one-liner.png"),
                    new SnapshotImageOptions { FontFamily = "Cascadia Mono", CellWidth = 9, CellHeight = 18 });
            Check("new MeshView(mesh) draws the mesh", withMesh > 25 && empty == 0,
                $"{withMesh} tinted cells, {empty} with an empty scene");
            Check("with three renderers and the model keys", one.Renderers.Count == 3 && one.Model is not null
                && one.CompileHelp()!.Keys.Any(k => k.Keys == "[ / ]"), $"{one.Renderers.Count} renderers");
            Check("framed on the model", Math.Abs(one.Camera.HomeDistance - 20f) > 0.01f && one.Camera.Distance == one.Camera.HomeDistance,
                $"home distance {one.Camera.HomeDistance:F1}");
        }

        Agreement(library, width, height, Check);

        Console.WriteLine(failures == 0 ? "\nALL PASS" : $"\n{failures} FAILURE(S)");
        return failures == 0 ? 0 : 1;
    }

    // Keys move the inspector: each key that has a widget, typed into a RUNNING UI through its input source -- the
    // path a terminal's keystrokes take -- and the widget read back off the captured LIVE screen. It has to be a
    // running UI: over a model the inspector refreshes once per drawn frame (a model has no change event), and
    // frames are only drawn by the loop, so a headless render would never see the refresh, and a check that called
    // Refresh by hand would pass with the wiring missing.
    //
    // Through the input SOURCE, not root.OnInput: UI.OnInput forces a full redraw for a keystroke, and that is what
    // a user gets. A full redraw also hides dropped damage, so each shell then makes the renderer change FROM CODE,
    // with no keystroke, while the viewport keeps repainting. That is how a folder load finishing reaches the
    // inspector. Damage from a control in a Grid nested in a stack panel (every Labelled drop-down here) used to be
    // dropped on that path, leaving the Renderer drop-down showing the old name.
    private static void Agreement(MeshLibrary library, int width, int height, Action<string, bool, string?> check)
    {
        Console.WriteLine($"\nkeys move the inspector, live UI loop {width}x{height}:");

        void Live(ILayout root, MeshView view, Action<Func<Func<string[], bool>, string, ConsoleKey, char, bool, bool>,
            Func<Action, Func<string[], bool>, string, bool>> body)
        {
            var screen = new LoadChecks.CaptureConsole(width, height);
            var keys = new System.Collections.Concurrent.ConcurrentQueue<TerminalInputEvent>();
            var run = UI.Start(root, width, height, fps: 60, isAnsiTerminal: false, console: screen,
                               input: new QueueInput(keys), useAlternateScreen: false);
            var frames = 0;
            view.Drew += _ => frames++;
            LoadChecks.WaitUntil(() => frames >= 5, 3000);
            UI.InvokeAsync(() => UI.SetFocus(view)).Wait(2000);

            // The sidebar's columns of the live screen.
            string[] Sidebar() => [.. screen.Text().Split('\n').Select(l => l.TrimEnd('\r'))
                .Select(l => l.Length > width - SidebarPanel.Columns ? l[(width - SidebarPanel.Columns)..] : "")];

            // Press, then wait for the screen to show it.
            bool Press(Func<string[], bool> shows, string what, ConsoleKey key, char ch, bool shift)
            {
                keys.Enqueue(new KeyInputEvent(key, ch, shift ? TerminalModifiers.Shift : TerminalModifiers.None));
                LoadChecks.WaitUntil(() => keys.IsEmpty, 2000);
                var ok = LoadChecks.WaitUntil(() => shows(Sidebar()), 2000);
                // The row the check looked at, for the report: the longest leading run of words in what that names a
                // row, else its last word (a model name). Section borders are skipped.
                var words = what.Split(' ');
                var rows = Sidebar().Where(l => !l.Contains("──")).ToArray();
                var line = Enumerable.Range(1, words.Length).Reverse().Select(n => string.Join(' ', words[..n]))
                    .Append(words[^1]).Select(p => rows.FirstOrDefault(l => l.Contains(p))).FirstOrDefault(l => l is not null)?.Trim() ?? "(not on screen)";
                check($"'{(ch == '\0' ? key.ToString() : ch.ToString())}' and the inspector shows {what}", ok, line);
                return ok;
            }

            // Change on the UI thread with no input, then wait for the screen to show it.
            bool Code(Action change, Func<string[], bool> shows, string what)
            {
                UI.InvokeAsync(change).Wait(2000);
                var ok = LoadChecks.WaitUntil(() => shows(Sidebar()), 2000);
                check($"from code, and the inspector shows {what}", ok, Row(Sidebar(), what.Split(' ')[0]) ?? "(not on screen)");
                return ok;
            }

            try { body(Press, Code); }
            finally
            {
                UI.Stop();
                run.Wait(2000);
            }
        }

        static string? Row(string[] lines, string label) => lines.FirstOrDefault(l => l.Contains(label))?.Trim();
        static bool Shows(string[] lines, string label, string text) => Row(lines, label)?.Contains(text) == true;
        static bool Switch(string[] lines, string label, bool on) => Shows(lines, label, on ? "(─●)" : "(●─)");

        // The three render keys, against whichever shell. Drives v round to the shaded renderer first, since the
        // edge and lighting controls are greyed under the others.
        void RenderKeys(MeshView view, Func<Func<string[], bool>, string, ConsoleKey, char, bool, bool> press)
        {
            for (var i = 0; i < view.Renderers.Count; i++)
            {
                var next = view.Renderers[(view.Renderers.ToList().IndexOf(view.Renderer) + 1) % view.Renderers.Count].Name;
                press(l => Shows(l, "Renderer", next), $"Renderer {next}", ConsoleKey.V, 'v', false);
            }

            foreach (var style in new[] { "line", "glyph", "none" })
                press(l => Shows(l, "Edges", style), $"Edges {style}", ConsoleKey.E, 'e', false);

            var wrap = view.WrapLighting ?? false;
            press(l => Switch(l, "Half-Lambert", !wrap), $"Half-Lambert light {(!wrap ? "on" : "off")}", ConsoleKey.W, 'w', false);
        }

        // The renderer cycle again, from code.
        void RenderFromCode(MeshView view, Func<Action, Func<string[], bool>, string, bool> code)
        {
            for (var i = 0; i < view.Renderers.Count; i++)
            {
                var next = view.Renderers[(view.Renderers.ToList().IndexOf(view.Renderer) + 1) % view.Renderers.Count].Name;
                code(view.NextRenderer, l => Shows(l, "Renderer", next), $"Renderer {next}");
            }
        }

        using (var app = SandboxShell.BuildSandbox(library, Populate))
        {
            Console.WriteLine("sandbox:");
            Live(app.Root, app.View, (press, code) =>
            {
                RenderKeys(app.View, press);
                RenderFromCode(app.View, code);
            });
        }

        using (var app = SandboxShell.BuildViewer(library, 0))
        {
            Console.WriteLine("viewer:");
            var model = app.Model;
            Live(app.Root, app.View, (press, code) =>
            {
                RenderKeys(app.View, press);
                var name = library.NameOf((model.MeshIndex + 1) % library.Count);
                press(l => l.Any(x => x.Trim().TrimEnd('│', '▇', ' ') == name), $"the model {name}", ConsoleKey.Oem6, ']', false);
                press(l => Shows(l, "Scale X", "1.15"), "Scale X 1.15", ConsoleKey.X, 'X', true);
                press(l => Shows(l, "Shear X", "0.08"), "Shear X 0.08", ConsoleKey.OemPeriod, '.', false);
                press(l => Shows(l, "Scale X", "1.00") && Shows(l, "Shear X", "0.00"), "Scale X 1.00 and Shear X 0.00", ConsoleKey.D0, '0', false);
                press(l => Switch(l, "Z-up", true), "Z-up file on", ConsoleKey.A, 'a', false);
                var spinning = model.SpinRate != 0f;
                press(l => Switch(l, "Turntable", !spinning),
                    $"Turntable {(spinning ? "off" : "on")}", ConsoleKey.P, 'p', false);

                // The turntable keeps the viewport repainting, so the frame has damage and no full redraw rescues it.
                if (spinning) press(l => Switch(l, "Turntable", true), "Turntable on", ConsoleKey.P, 'p', false);
                RenderFromCode(app.View, code);
            });
        }
    }

    // Every help line must have a check, and every check a help line.
    private static void RunHelp(MeshView view, Dictionary<string, Func<(bool, string)>> checks, Action<string, bool, string?> check)
    {
        var help = view.CompileHelp()!;
        foreach (var line in help.Keys)
        {
            if (!checks.TryGetValue(line.Keys, out var run))
            {
                check($"{line.Keys,-14} has a check", false, line.Description);
                continue;
            }

            var (ok, detail) = run();
            check($"{line.Keys,-14} {line.Description}", ok, detail);
        }

        foreach (var stale in checks.Keys.Except(help.Keys.Select(k => k.Keys)))
            check($"{stale,-14} is in the help", false, "a check for a key the help no longer lists");
    }

    private static (bool, string) Arrows(ILayout root, MeshView view)
    {
        view.Camera.Reset();
        var t0 = view.Camera.Theta;
        Send(root, ConsoleKey.RightArrow);
        var coarse = view.Camera.Theta - t0;
        Send(root, ConsoleKey.RightArrow, shift: true);
        var fine = view.Camera.Theta - t0 - coarse;
        var p0 = view.Camera.Phi;
        Send(root, ConsoleKey.UpArrow);
        var tilt = view.Camera.Phi - p0;
        Send(root, ConsoleKey.LeftArrow);
        Send(root, ConsoleKey.DownArrow);
        return (coarse > 0 && Math.Abs(fine - (coarse * 0.2f)) < 1e-4f && tilt < 0,
            $"right {coarse:F3}, shift+right {fine:F3}, up {tilt:F3}");
    }

    private static (bool, string) Zoom(ILayout root, MeshView view)
    {
        var d0 = view.Camera.Distance;
        Send(root, ConsoleKey.PageUp);
        var d1 = view.Camera.Distance;
        Send(root, ConsoleKey.PageDown);
        var d2 = view.Camera.Distance;
        return (d1 < d0 && d2 > d1, $"{d0:F1} -> {d1:F1} -> {d2:F1}");
    }

    private static (bool, string) Home(ILayout root, MeshView view, float home)
    {
        view.Camera.Orbit(0.7f, 0.2f);
        view.Camera.Zoom(1.5f);
        Send(root, ConsoleKey.Home);
        return (Math.Abs(view.Camera.Distance - home) < 1e-3f && Math.Abs(view.Camera.Theta - (MathF.PI / 4)) < 1e-4f,
            $"distance {view.Camera.Distance:F2} (home {home:F2})");
    }

    private static (bool, string) NextRenderer(ILayout root, MeshView view)
    {
        var names = new List<string> { view.Renderer.Name };
        for (var i = 0; i < view.Renderers.Count; i++)
        {
            Send(root, ConsoleKey.V, 'v');
            names.Add(view.Renderer.Name);
        }

        return (names.Distinct().Count() == view.Renderers.Count && names[^1] == names[0], string.Join(" -> ", names));
    }

    private static (bool, string) Edges(ILayout root, MeshView view)
    {
        view.SetRenderer(view.Renderers.First(r => r is ShadedRenderer));
        var styles = new List<SilhouetteStyle?> { view.Edges };
        for (var i = 0; i < 3; i++)
        {
            Send(root, ConsoleKey.E, 'e');
            styles.Add(view.Edges);
        }

        return (styles.Distinct().Count() == 3 && styles[^1] == styles[0], string.Join(" -> ", styles));
    }

    private static (bool, string) Wrap(ILayout root, MeshView view)
    {
        view.SetRenderer(view.Renderers.First(r => r is ShadedRenderer));
        var before = view.WrapLighting;
        Send(root, ConsoleKey.W, 'w');
        var toggled = view.WrapLighting;
        Send(root, ConsoleKey.W, 'w');
        return (toggled != before && view.WrapLighting == before, $"{before} -> {toggled} -> {view.WrapLighting}");
    }

    // A global hotkey, so through the global dispatch -- where the live loop sends every key first.
    private static (bool, string) Sidebar(ILayout root, Control sidebar)
    {
        var w0 = sidebar.Width;
        UI.SendInput(root, UI.HotKeys.Char('u'), routeGlobal: true);
        var w1 = sidebar.Width;
        UI.SendInput(root, UI.HotKeys.Char('u'), routeGlobal: true);
        return (w1 == 1 && sidebar.Width == w0, $"{w0} -> {w1} -> {sidebar.Width} cols");
    }

    // Keystrokes for a running UI, read by its input loop exactly as a terminal's would be.
    private sealed class QueueInput(System.Collections.Concurrent.ConcurrentQueue<TerminalInputEvent> queue) : IInputSource
    {
        public bool TryRead(out TerminalInputEvent? evt)
        {
            var got = queue.TryDequeue(out var e);
            evt = e;
            return got;
        }
    }

    private static float Component(Vector3 v, int axis) => axis switch { 0 => v.X, 1 => v.Y, _ => v.Z };

    // Through the root layout, as the live loop routes it. Returns whether anything took the key.
    private static bool Send(ILayout root, ConsoleKey key, char ch = '\0', bool shift = false)
    {
        var e = new ConsoleGUI.Input.InputEvent(new ConsoleKeyInfo(ch, key, shift, false, false));
        root.OnInput(new UI.InputEventArgs(e));
        return e.Handled;
    }

    private static PhysicsFrame Settle(PhysicsRunner runner, int steps)
    {
        var target = runner.Frame.StepCount + steps;
        var clock = Stopwatch.StartNew();
        while (runner.Frame.StepCount < target && clock.ElapsedMilliseconds < 5000) Thread.Sleep(5);
        return runner.Frame;
    }

    // No UI loop runs here, so the view's own feed never ticks: lay the tree out and draw a frame by hand, so the
    // viewport and projection that picking reads are the ones on screen.
    private static void Draw(ILayout root, MeshView view, int width, int height)
    {
        _ = ConsoleSnapshot.ToText(root, width, height);
        view.Renderer.Draw(view.Source.Frame, view.Camera);
        _ = ConsoleSnapshot.ToText(root, width, height);
    }

    // The viewport's top-left cell on screen: just inside its rounded frame, which is the first corner glyph on the
    // screen (the menu bar above it has none).
    private static Position ViewOrigin(ILayout root, int width, int height)
    {
        var lines = ConsoleSnapshot.ToLines(ConsoleSnapshot.Render(root, width, height));
        for (var y = 0; y < lines.Length; y++)
        {
            var x = lines[y].IndexOf('╭');
            if (x >= 0) return new Position(x + 1, y + 1);
        }

        return new Position(0, 0);
    }

    // A screen cell over a body, found the way the view itself picks: every viewport cell asked in turn.
    private static (Position Cell, int Id, int Index)? FindBody(ILayout root, MeshView view, int width, int height)
    {
        Draw(root, view, width, height);
        var origin = ViewOrigin(root, width, height);
        var frame = view.Source.Frame;
        for (var y = 0; y < view.ActualHeight; y++)
        {
            for (var x = 0; x < view.ActualWidth; x++)
            {
                if (view.Pick(new Position(x, y)) is { } id && frame.IndexOf(id) is >= 0 and var index)
                    return (new Position(origin.X + x, origin.Y + y), id, index);
            }
        }

        return null;
    }

    private static void Populate(PhysicsScene scene)
    {
        for (var i = 0; i < 7; i++)
            scene.AddBox(new Vector3(i * 0.06f, 0.5f + (i * 1.02f), 0), new Vector3(0.5f), i);
        for (var i = 0; i < 4; i++)
            scene.AddSphere(new Vector3(-4f + (i * 0.8f), 6f + (i * 1.5f), 1.5f), 0.45f, 7 + i);
    }
}
