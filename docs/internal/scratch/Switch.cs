namespace Render3d;

using System.Diagnostics;
using System.Numerics;

using ConsoleGUI.Api;
using ConsoleGUI.Data;
using ConsoleGUI.Space;

using Jumbee.Console;
using Jumbee.Console.SandboxDemo;

/// <summary>
/// Runs the app's two shells back to back through real <see cref="UI.Start"/> / <see cref="UI.Stop"/> cycles, the
/// way the Switch menu item does — the check for the one thing the sandbox demo had never asked of the library.
/// </summary>
/// <remarks>
/// <para>
/// Headless: a stub <see cref="IConsole"/> that discards cells and an <see cref="IInputSource"/> that never yields,
/// so the loop runs for real without touching the terminal. Frames are counted through <c>SceneView.Drew</c>,
/// which is the honest signal — it fires from the view's own feed, so it says the shell is <em>alive</em> rather
/// than that a method returned.
/// </para>
/// <para>
/// Three questions, and each one was a real defect before the fix that answers it: does a second Start run at all;
/// does a hotkey registered by the first session still fire in the second (it did, pointing at a disposed physics
/// world); and does the first shell's 60 Hz render feed stop when its shell is disposed (it did not).
/// </para>
/// </remarks>
internal static class SwitchChecks
{
    public static int Run(int width, int height)
    {
        var failures = 0;
        void Check(string what, bool ok, string? detail = null)
        {
            Console.WriteLine($"  {(ok ? "PASS" : "FAIL")}  {what}{(detail is null ? "" : $"  [{detail}]")}");
            if (!ok) failures++;
        }

        Meshes.Register(Meshes.TorusKnot(), "knot");

        // --- session 1: the sandbox -------------------------------------------------------------------------------
        Console.WriteLine("session 1 (sandbox):");
        var stale = 0;
        SandboxShell.ShellType? pending = null;

        var sandbox = SandboxShell.BuildSandbox(Populate, null, to => { pending = to; UI.Stop(); });
        var sandboxFrames = 0;
        sandbox.View.Drew += _ => sandboxFrames++;

        // A hotkey belonging to THIS session, registered exactly as SandboxShell registers its own.
        UI.RegisterHotKey(UI.HotKeys.Char('~'), () => stale++);

        var run1 = Start(sandbox.Root, width, height);
        WaitForFrames(() => sandboxFrames, 5);
        Check("the sandbox shell runs", sandboxFrames >= 5, $"{sandboxFrames} frames");
        UI.SendInput(sandbox.View, UI.HotKeys.Char('~'), routeGlobal: true);
        Check("and its own hotkey fires while it is running", stale == 1, $"{stale} hits");

        // What the menu item does: record the request, stop the UI. Start's task completes, and only then is the
        // shell torn down -- nothing is built from inside the running frame loop.
        pending = SandboxShell.ShellType.ModelViewer;
        UI.Stop();
        Check("stopping completes the Start task", run1.Wait(2000), "waited 2s");
        Check("and the switch request survives the stop", pending == SandboxShell.ShellType.ModelViewer, $"{pending}");

        sandbox.Dispose();
        var framesAtDispose = sandboxFrames;

        // --- session 2: the model viewer --------------------------------------------------------------------------
        Console.WriteLine("\nsession 2 (model viewer), same process:");
        var viewer = SandboxShell.BuildViewer(0, null, to => { pending = to; UI.Stop(); });
        var viewerFrames = 0;
        viewer.View.Drew += _ => viewerFrames++;

        var run2 = Start(viewer.Root, width, height);
        Check("a second Start runs after a Stop", UI.IsRunning);
        WaitForFrames(() => viewerFrames, 5);
        Check("and the second shell paints", viewerFrames >= 5, $"{viewerFrames} frames");

        // The two defects a second session exposes and a single one never can.
        UI.SendInput(viewer.View, UI.HotKeys.Char('~'), routeGlobal: true);
        Check("a hotkey from the previous session no longer fires", stale == 1, $"{stale} hits, expected 1");
        Check("and the disposed shell's render feed has stopped", sandboxFrames == framesAtDispose,
            $"{framesAtDispose} at dispose -> {sandboxFrames} now");

        // --- session 3: back again ----------------------------------------------------------------------------
        // Twice is the interesting number: once could be a Start that happens to survive its first teardown.
        UI.Stop();
        run2.Wait(2000);
        viewer.Dispose();
        var viewerAtDispose = viewerFrames;

        Console.WriteLine("\nsession 3 (back to the sandbox):");
        var again = SandboxShell.BuildSandbox(Populate, null, to => { pending = to; UI.Stop(); });
        var againFrames = 0;
        again.View.Drew += _ => againFrames++;
        var run3 = Start(again.Root, width, height);
        WaitForFrames(() => againFrames, 5);
        Check("a third session runs", againFrames >= 5, $"{againFrames} frames");
        Check("the physics thread of the third is live", again.Runner.Snapshot.Count == 11,
            $"{again.Runner.Snapshot.Count} bodies");
        Check("and neither earlier shell is still painting", sandboxFrames == framesAtDispose && viewerFrames == viewerAtDispose,
            $"sandbox {sandboxFrames}/{framesAtDispose}, viewer {viewerFrames}/{viewerAtDispose}");

        UI.Stop();
        run3.Wait(2000);
        again.Dispose();
        Check("and the last stop leaves the UI stopped", !UI.IsRunning && !UI.Dispatcher.IsRunning);

        Console.WriteLine(failures == 0 ? "\nALL PASS" : $"\n{failures} FAILURE(S)");
        return failures == 0 ? 0 : 1;
    }

    // isAnsiTerminal FALSE, and not for tidiness: the ANSI renderer writes frames to stdout through
    // ConsoleManager.AnsiOutput, which never consults the IConsole at all -- so a stub console silences nothing and
    // the whole run drowns in escape sequences. The legacy path routes every cell through IConsole.Write, which the
    // stub discards. Everything this mode asks about is lifecycle, not rendering, and both paths share it.
    private static Task Start(ILayout root, int width, int height) =>
        UI.Start(root, width, height, fps: 60, isAnsiTerminal: false, console: new StubConsole(width, height),
                 input: new NoInput(), useAlternateScreen: false);

    private static void WaitForFrames(Func<int> count, int target)
    {
        var sw = Stopwatch.StartNew();
        while (count() < target && sw.ElapsedMilliseconds < 3000) Thread.Sleep(5);
    }

    private static void Populate(PhysicsScene scene)
    {
        for (var i = 0; i < 7; i++)
            scene.AddBox(new Vector3(i * 0.06f, 0.5f + (i * 1.02f), 0), new Vector3(0.5f, 0.5f, 0.5f), i);
        for (var i = 0; i < 4; i++)
            scene.AddSphere(new Vector3(-4f + (i * 0.8f), 6f + (i * 1.5f), 1.5f), 0.45f, 7 + i);
    }

    private sealed class NoInput : IInputSource
    {
        public bool TryRead(out TerminalInputEvent? evt) { evt = null; return false; }
    }

    private sealed class StubConsole(int w, int h) : IConsole
    {
        public Size Size { get; set; } = new Size(w, h);
        public bool KeyAvailable => false;
        public void Initialize() { }
        public void OnRefresh() { }
        public void Write(Position position, in Character character) { }
        public ConsoleKeyInfo ReadKey() => throw new NotSupportedException();
    }
}
