namespace Render3d;

using System.Diagnostics;
using System.Text;

using ConsoleGUI.Api;
using ConsoleGUI.Data;
using ConsoleGUI.Input;
using ConsoleGUI.Space;

using Jumbee.Console;
using Jumbee.Console.SandboxDemo;

/// <summary>
/// Loads a folder behind <see cref="ModelLoadDialog"/> over the real model viewer, through a real
/// <see cref="UI.Start"/> — the only way to see it, since the loader reports to a running UI and stands down without one.
/// </summary>
/// <remarks>
/// The fixture is a temp folder this check owns: one large model first, so the modal is on screen long enough to be
/// captured and stopped; two small ones; and a file that is not a model, which must be reported rather than lost.
/// The screen is captured through a console that keeps what the renderer writes, so the modal is checked as text on
/// the composited screen — what a user would see — rather than as a control's state.
/// </remarks>
internal static class LoadChecks
{
    public static int Run(int width, int height, string[] args)
    {
        var failures = 0;
        void Check(string what, bool ok, string? detail = null)
        {
            Console.WriteLine($"  {(ok ? "PASS" : "FAIL")}  {what}{(detail is null ? "" : $"  [{detail}]")}");
            if (!ok) failures++;
        }

        var models = RepoPaths.At("examples", "Jumbee.Console.3DSandboxDemo", "models");
        var big = Path.Combine(models, "dragon.obj");
        if (!File.Exists(big))
        {
            Console.WriteLine($"  SKIP  needs {big} (models/ is not in git)");
            return 0;
        }

        var dir = Directory.CreateTempSubdirectory("jc-load-").FullName;
        try
        {
            File.Copy(big, Path.Combine(dir, "a-dragon.obj"));
            File.Copy(Path.Combine(models, "teapot.obj"), Path.Combine(dir, "b-teapot.obj"));
            File.Copy(Path.Combine(models, "cali-bee.stl"), Path.Combine(dir, "c-bee.stl"));
            File.WriteAllText(Path.Combine(dir, "d-broken.obj"), "this is not a model\n");
            var set = ModelLibrary.Resolve(dir);

            Meshes.Register(Meshes.TorusKnot(), "knot");
            var viewer = SandboxShell.BuildViewer(0);
            var frames = 0;
            viewer.View.Drew += _ => frames++;
            var screen = new CaptureConsole(width, height);
            var run = UI.Start(viewer.Root, width, height, fps: 60, isAnsiTerminal: false, console: screen,
                               input: new NoInput(), useAlternateScreen: false);
            WaitUntil(() => frames >= 5, 3000);

            // --- a full load ------------------------------------------------------------------------------------
            Console.WriteLine("full load:");
            var before = Meshes.RegisteredCount;
            ModelLoadResult? result = null;
            UI.Invoke(() => ModelLoadDialog.Load(set, r => result = r));

            var shown = WaitUntil(() => screen.Text().Contains("Loading models"), 3000);
            Check("the modal appears over the running viewer", shown);
            var midLoad = screen.Text();
            Check("and names the file being parsed", midLoad.Contains("a-dragon.obj"));
            var framesAtShow = frames;
            WaitUntil(() => frames >= framesAtShow + 5, 2000);
            Check("the viewer keeps painting under it", frames >= framesAtShow + 5, $"{frames - framesAtShow} frames while loading");
            if (args.Contains("show")) Console.WriteLine(midLoad);

            WaitUntil(() => result is not null, 15000);
            Check("the load completes", result is not null);
            if (result is { } r1)
            {
                Check("every good file is registered, in folder order", r1.Loaded == 3 && Meshes.RegisteredCount == before + 3
                    && Meshes.NameOf(before) == "a-dragon" && Meshes.NameOf(before + 2) == "c-bee",
                    $"{r1.Loaded} loaded");
                Check("the broken file is reported, not dropped", r1.Failures is [("d-broken.obj", _)],
                    string.Join("; ", r1.Failures.Select(f => $"{f.File}: {f.Reason}")));
                Check("and the load opens on the first model", r1.StartId == before, $"start {r1.StartId}");
                Check("not marked stopped", !r1.Stopped);
            }
            WaitUntil(() => !screen.Text().Contains("Loading models"), 2000);
            Check("the modal closes when done", !screen.Text().Contains("Loading models"));
            Check("and the loader is free again", !ModelLoadDialog.IsLoading);

            // --- Escape mid-load ----------------------------------------------------------------------------------
            Console.WriteLine("\nEscape mid-load:");
            before = Meshes.RegisteredCount;
            result = null;
            UI.Invoke(() => ModelLoadDialog.Load(set, r => result = r));
            WaitUntil(() => screen.Text().Contains("a-dragon.obj"), 3000);
            // Through the root overlay, exactly as a live key arrives.
            UI.Invoke(() => UI.Overlay!.OnInput(new UI.InputEventArgs(
                new InputEvent(new ConsoleKeyInfo('\0', ConsoleKey.Escape, false, false, false)))));
            WaitUntil(() => !screen.Text().Contains("Loading models"), 2000);
            Check("Escape closes the modal at once", !screen.Text().Contains("Loading models"));
            WaitUntil(() => result is not null, 15000);
            if (result is { } r2)
            {
                Check("the load stops at the next file", r2.Stopped && r2.Loaded == 1, $"{r2.Loaded} loaded, stopped={r2.Stopped}");
                Check("and keeps what it had parsed", Meshes.RegisteredCount == before + 1);
            }
            else Check("the stopped load still reports", false);

            UI.Stop();
            run.Wait(2000);
            viewer.Dispose();
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }

        Console.WriteLine(failures == 0 ? "\nALL PASS" : $"\n{failures} FAILURE(S)");
        return failures == 0 ? 0 : 1;
    }

    private static bool WaitUntil(Func<bool> condition, int milliseconds)
    {
        var sw = Stopwatch.StartNew();
        while (!condition() && sw.ElapsedMilliseconds < milliseconds) Thread.Sleep(10);
        return condition();
    }

    private sealed class NoInput : IInputSource
    {
        public bool TryRead(out TerminalInputEvent? evt) { evt = null; return false; }
    }

    // Keeps what the legacy renderer writes, cell by cell, so the composited screen can be read back as text.
    private sealed class CaptureConsole(int w, int h) : IConsole
    {
        public Size Size { get; set; } = new Size(w, h);
        public bool KeyAvailable => false;
        public void Initialize() { }
        public void OnRefresh() { }
        public void Write(Position position, in Character character)
        {
            if (position.X < 0 || position.Y < 0 || position.X >= w || position.Y >= h) return;
            lock (cells) cells[position.Y, position.X] = character.Content ?? ' ';
        }
        public ConsoleKeyInfo ReadKey() => throw new NotSupportedException();

        public string Text()
        {
            var sb = new StringBuilder();
            lock (cells)
            {
                for (var y = 0; y < h; y++)
                {
                    for (var x = 0; x < w; x++) sb.Append(cells[y, x] == '\0' ? ' ' : cells[y, x]);
                    sb.AppendLine();
                }
            }
            return sb.ToString();
        }

        private readonly char[,] cells = new char[h, w];
    }
}
