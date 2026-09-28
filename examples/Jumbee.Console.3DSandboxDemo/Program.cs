using System.CommandLine;
using System.Numerics;

using Jumbee.Console;
using Jumbee.Console.SandboxDemo;

using ShellType = Jumbee.Console.SandboxDemo.SandboxShell.ShellType;

// --- Jumbee.Console 3D sandbox ----------------------------------------------------------------------------------
// Two scenes over one renderer stack.
//
//   (default)  a real-time rigid-body sandbox: Box3D simulating on its own thread, an orbit camera, spawn, launch,
//              grab and throw.
//   obj        a model viewer: one asset filling the viewport on a turntable. A loaded model at sandbox scale is a
//              few dozen cells across, where a teapot and a rock look identical -- the parser and the renderers can
//              only really be judged at a size the terminal can resolve.
//
// Three renderers, cycled live with 'v' in either scene, in rising order of cost and fidelity:
//
//   wireframe -- projected edges on a Canvas at braille resolution; painter's sort, no fill.
//   solid     -- z-buffered flat-shaded triangles from a directional light; one colour per face.
//   shaded    -- per-pixel point light with specular, plus silhouettes and ambient occlusion.
//
// Threading (sandbox only) is the pattern from docs/controls/Live Data.md: the physics world, the body list and
// every Box3D handle belong to the physics thread, which publishes one immutable SceneSnapshot per tick. The UI
// thread only ever reads the newest snapshot and posts mutations back. There is no lock anywhere.

// The scene the running one asked to become, and which model the viewer should open on when it is next built. Both
// are read by Run between shells — never while one is running — so neither needs marshalling.
ShellType? pending = null;
var viewerStart = 0;
var scannedModelsFolder = false;
// A folder the next viewer should load once its UI is up — behind a modal, not before the UI appears.
ModelSet? pendingFolder = null;

var modelPath = new Argument<string?>("path")
{
    Arity = ArgumentArity.ZeroOrOne,
    Description = "An .obj, .stl or .ply file, or a directory of them. Either way the whole directory is loaded and '[' / ']' " +
                  "cycle through it; naming a file just decides which one opens first. With no path, a 'models' " +
                  "folder in the current directory is used if there is one, and otherwise the viewer opens on its " +
                  "generated torus knot. Models load after the viewer appears, behind a progress dialog; Esc stops " +
                  "the load and keeps what has loaded so far.",
};

// An OPTION on the root, not a positional argument: a positional there would be inherited by the `obj` subcommand
// (so its help lists `<models>` twice) and would make `app foo.obj` versus `app obj` ambiguous to parse.
var sandboxModels = new Option<string[]>("--model", "-m")
{
    Arity = ArgumentArity.ZeroOrMore,
    AllowMultipleArgumentsPerToken = true,
    Description = "Model files (.obj, .stl, .ply), or directories of them, to make spawnable. Cycle them with 'm', then " +
                  "drop with 'n' or fire with 'f'.",
};

var objCommand = new Command("obj", "Open the model viewer: one asset filling the viewport, on a turntable.")
{
    modelPath,
};

objCommand.SetAction(async (parse, ct) =>
{
    // Resolving is cheap and its errors are about the command line, so they still end the app before any UI. Only
    // the parsing — the slow part — waits for the viewer to be up.
    var set = ModelLibrary.Resolve(parse.GetValue(modelPath));
    if (set.Error is { } error)
    {
        Console.Error.WriteLine(error);
        return 1;
    }
    scannedModelsFolder = true;   // this IS the scan, whether or not a path narrowed it
    pendingFolder = set;

    // The generated knot goes FIRST: it is what the viewer shows while the folder loads, spinning behind the modal,
    // and what it still has to show if the folder turns out to hold nothing that parses.
    Meshes.Register(Meshes.TorusKnot(), "knot");
    return await Run(ShellType.ModelViewer, 0);
});

// A headless smoke check, on the ROOT so one invocation covers both scenes -- that is what a container build wants
// to run, and a flag it had to repeat per subcommand would get one of them checked and the other forgotten.
var verify = new Option<bool>("--verify")
{
    Description = "Render both scenes offscreen through every renderer, print one PASS/FAIL line and exit. For CI " +
                  "and container builds, where there is no terminal to look at.",
};

var root = new RootCommand("A real-time 3D rigid-body sandbox in the terminal, with three renderers.")
{
    sandboxModels,
    verify,
    objCommand,
};

root.SetAction(async (parse, ct) =>
{
    // A generated torus knot is always available: it gives the renderers geometry with real curvature and
    // self-occlusion to be compared on, and it means the mesh path works with no third-party asset.
    Meshes.Register(Meshes.TorusKnot(), "knot");
    if (!LoadModels(parse.GetValue(sandboxModels))) return 1;
    // Before the UI, and after the models: verifying is instead of running, and the viewer half of the check wants
    // whatever was loaded to be registered so a --model that fails to parse fails the check too.
    if (parse.GetValue(verify)) return Verify.Run();
    return await Run(ShellType.Sandbox, 0);
});

return await root.Parse(args).InvokeAsync();

// The app's whole lifetime: run one scene until its UI stops, and if it stopped in order to become the other one,
// build that and go round again. Quit leaves `pending` null and falls out.
//
// A LOOP, not a menu item that calls RunSandbox/RunModelViewer directly. A menu handler runs on the UI thread inside
// the frame loop of the shell it belongs to, so starting the next UI from there would start one from inside another
// and keep a shell alive per switch. Here the switch is only a request: the handler records it and calls UI.Stop,
// the awaited Start completes, the shell is disposed, and only then is the next one built. Nothing overlaps.
async Task<int> Run(ShellType shell, int startIndex)
{
    while (true)
    {
        pending = null;
        var code = shell == ShellType.Sandbox ? await RunSandbox() : await RunModelViewer(startIndex);
        if (code != 0 || pending is not { } next) return code;

        shell = next;
        startIndex = next == ShellType.ModelViewer ? OpenModelsFolder(viewerStart) : viewerStart;
    }
}

// Switching INTO the viewer from a sandbox that was launched without the obj verb: nothing has looked at the models
// folder, because only that verb does at startup — so the viewer arrived with just the generated knot even with a
// folder full of models sitting right there. Look now, once: repeated switches must not re-parse the directory, and
// models loaded this way stay in the registry, spawnable back in the sandbox. The viewer opens on `fallback` and
// moves to the folder's first model when the load behind its modal finishes.
int OpenModelsFolder(int fallback)
{
    if (scannedModelsFolder) return fallback;
    scannedModelsFolder = true;

    if (ModelLibrary.Resolve(null) is { Error: null, Files.Length: > 0 } set) pendingFolder = set;
    return fallback;
}

// Loads every model the path resolved to, before any UI, returning the index to open on, or -1 on a reported
// failure. Only the sandbox's --model uses it now: the viewer loads its folder behind ModelLoadDialog instead.
//
// Loading is EAGER. Parsing on first display would trade a one-off startup cost for a stall in the middle of
// cycling, which is the worse place to put it: measured, the four reference models total ~750 ms and the
// 250k-triangle dragon is 608 ms of that on its own.
static int LoadModelDirectory(string? path)
{
    var set = ModelLibrary.Resolve(path);
    if (set.Error is { } error)
    {
        Console.Error.WriteLine(error);
        return -1;
    }

    // Offset by what is already registered: ModelSet.StartIndex counts within the directory, and after a runtime
    // reload the registry already holds the previous folder's models (plus the generated knot) ahead of these.
    var offset = Meshes.RegisteredCount;
    return set.Files.All(LoadModel) ? offset + set.StartIndex : -1;
}

static bool LoadModel(string path)
{
    try
    {
        Meshes.Register(ModelLoader.Load(path), Path.GetFileNameWithoutExtension(path));
        return true;
    }
    // UnauthorizedAccessException is not an IOException: it is what Windows reports for a path that can't be opened
    // as a file — a permissions problem, or a directory handed to a file reader.
    catch (Exception ex) when (ex is IOException or InvalidDataException or UnauthorizedAccessException)
    {
        Console.Error.WriteLine($"could not load '{path}': {ex.Message}");
        return false;
    }
}

// Registers each model, reporting the first failure rather than starting a UI that is missing what was asked for. A
// directory contributes every model in it, by the same rules as the viewer's path argument.
static bool LoadModels(string[]? paths) =>
    (paths ?? []).All(p => Directory.Exists(p) ? LoadModelDirectory(p) >= 0 : LoadModel(p));

// The default scene: physics, a floor, and everything you can do to a pile of bodies.
async Task<int> RunSandbox()
{
    // The whole shell — viewport, sidebar, menu, footer, keys — is assembled by SandboxShell so the headless
    // harness can drive the real one rather than a rebuild of it.
    SandboxShell.Sandbox app = default;
    app = SandboxShell.BuildSandbox(Populate, () => LoadMeshDialog(app), RequestSwitch);
    UI.Post(() => UI.SetFocus(app.View));

    await UI.Start(app.Root, width: 120, height: 48, fps: 60);
    // Whatever the spawn drop-down was pointing at is what the viewer opens on, so switching to it shows the model
    // you were about to throw rather than an arbitrary one.
    viewerStart = Math.Max(0, app.View.Spawn.MeshId);
    app.Dispose();
    return 0;
}

// A switch is recorded and the UI stopped; Run's loop does the rest. Setting `pending` from a menu handler is safe
// without marshalling because that handler already runs on the UI thread, and Run only reads it after Start's task
// has completed — which happens after the UI thread has gone.
void RequestSwitch(ShellType to)
{
    pending = to;
    UI.Stop();
}

// Load a model while the app is running, which is what --model used to be the only way to do. The chosen mesh
// becomes what the spawn keys produce, so the very next `n` or `f` throws it into the scene.
static void LoadMeshDialog(SandboxShell.Sandbox app) =>
    FileBrowser.OpenFile("Load a model", null, ModelLoader.Patterns, path =>
    {
        if (path is null) return;
        try
        {
            var id = Meshes.Register(ModelLoader.Load(path), Path.GetFileNameWithoutExtension(path));
            app.Sidebar.RefreshMeshes();
            app.View.Spawn.MeshId = id;
            app.View.Spawn.Shape = BodyShape.Mesh;
        }
        catch (Exception ex) when (ex is IOException or InvalidDataException or UnauthorizedAccessException)
        {
            Dialog.Message("Could not load", $"{Path.GetFileName(path)}: {ex.Message}");
        }
    });

// The viewer browses by DIRECTORY: '[' and ']' cycle everything in it, so opening a folder of models is the useful
// unit — the same rule ModelLibrary.Resolve applies to the command line.
static void OpenModelsDialog(SandboxShell.Viewer app)
{
    // 'o' is a global hotkey, and global hotkeys fire even under a modal — so it can arrive mid-load.
    if (ModelLoadDialog.IsLoading) return;

    FileBrowser.OpenDirectory("Open a model folder", null, directory =>
    {
        if (directory is null) return;
        var set = ModelLibrary.Resolve(directory);
        if (set.Error is { } error)
        {
            Dialog.Message("Nothing to show", error);
            return;
        }

        LoadFolder(app, set);
    });
}

// Loads a folder behind the progress modal, then shows its first model — or the one the path named. The viewer is
// live throughout: it keeps turning whatever it was showing until the load hands it something new.
static void LoadFolder(SandboxShell.Viewer app, ModelSet set) =>
    ModelLoadDialog.Load(set, result =>
    {
        if (result.StartId >= 0) app.Model.Reload(result.StartId);
        app.Sidebar.Report();

        if (result.Failures.Length > 0)
        {
            var lines = string.Join("\n", result.Failures.Select(f => $"{f.File}: {f.Reason}"));
            Dialog.Message("Some models did not load",
                $"{result.Failures.Length} of {result.Failures.Length + result.Loaded} files could not be loaded.\n\n{lines}");
        }
    });

// The `obj` scene: one model, no physics. Same camera, same three renderers, same edge styles. The checkerboard
// ground stays — it costs nothing and it earns its place, giving the model a sense of scale and somewhere for the
// ambient occlusion to land, both of which a model floating in a void loses.
async Task<int> RunModelViewer(int startIndex)
{
    if (Meshes.RegisteredCount == 0)
    {
        Console.Error.WriteLine("nothing to show.");
        return 1;
    }

    SandboxShell.Viewer app = default;
    app = SandboxShell.BuildViewer(startIndex, () => OpenModelsDialog(app), RequestSwitch);
    UI.Post(() => UI.SetFocus(app.View));
    // Posted, so it runs once Start is up: the modal needs the overlay Start creates.
    if (pendingFolder is { } folder)
    {
        pendingFolder = null;
        UI.Post(() => LoadFolder(app, folder));
    }

    await UI.Start(app.Root, width: 120, height: 48, fps: 60);
    // Come back to the model you left on, if this one is switched away from and later returned to.
    viewerStart = app.Model.MeshId;
    app.Dispose();
    return 0;
}

// A tower of boxes with a slight lean, plus a few spheres dropped alongside -- enough motion to show the camera,
// the depth sorting and the shading all working, and enough contact for the solver to have to do something.
static void Populate(PhysicsScene scene)
{
    const int TowerHeight = 7;
    for (var i = 0; i < TowerHeight; i++)
    {
        var lean = i * 0.06f;
        scene.AddBox(new Vector3(lean, 0.5f + (i * 1.02f), 0), new Vector3(0.5f, 0.5f, 0.5f), i);
    }

    for (var i = 0; i < 4; i++)
        scene.AddSphere(new Vector3(-4f + (i * 0.8f), 6f + (i * 1.5f), 1.5f), 0.45f, TowerHeight + i);
}
