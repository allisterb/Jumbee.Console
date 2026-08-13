using System.CommandLine;
using System.Numerics;

using Jumbee.Console;
using Jumbee.Console.SandboxDemo;

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
//   shaded    -- per-pixel point light with specular, plus silhouettes and contact darkening.
//
// Threading (sandbox only) is the pattern from docs/controls/Live Data.md: the physics world, the body list and
// every Box3D handle belong to the physics thread, which publishes one immutable SceneSnapshot per tick. The UI
// thread only ever reads the newest snapshot and posts mutations back. There is no lock anywhere.

var modelPath = new Argument<string?>("path")
{
    Arity = ArgumentArity.ZeroOrOne,
    Description = "An .obj file, or a directory of them. Either way the whole directory is loaded and '[' / ']' " +
                  "cycle through it; naming a file just decides which one opens first. Defaults to the current " +
                  "directory. NOTE: models are parsed before the UI appears, so a directory holding large ones " +
                  "pauses at startup (a 250k-triangle model takes ~600ms; a 6k-triangle one takes ~4ms).",
};

// An OPTION on the root, not a positional argument: a positional there would be inherited by the `obj` subcommand
// (so its help lists `<models>` twice) and would make `app foo.obj` versus `app obj` ambiguous to parse.
var sandboxModels = new Option<string[]>("--model", "-m")
{
    Arity = ArgumentArity.ZeroOrMore,
    AllowMultipleArgumentsPerToken = true,
    Description = "Wavefront .obj files to make spawnable. Cycle them with 'm', then drop with 'n' or fire with 'f'.",
};

var objCommand = new Command("obj", "Open the model viewer: one asset filling the viewport, on a turntable.")
{
    modelPath,
};

objCommand.SetAction(async (parse, ct) =>
{
    var start = LoadModelDirectory(parse.GetValue(modelPath));
    if (start < 0) return 1;

    // The generated knot goes last, so it is always reachable with '[' / ']' even when a directory was given, and
    // so the viewer still has something to show if the directory turns out to hold no models at all.
    Meshes.Register(Meshes.TorusKnot(), "knot");
    return await RunModelViewer(start);
});

var root = new RootCommand("A real-time 3D rigid-body sandbox in the terminal, with three renderers.")
{
    sandboxModels,
    objCommand,
};

root.SetAction(async (parse, ct) =>
{
    // A generated torus knot is always available: it gives the renderers geometry with real curvature and
    // self-occlusion to be compared on, and it means the mesh path works with no third-party asset.
    Meshes.Register(Meshes.TorusKnot(), "knot");
    if (!LoadModels(parse.GetValue(sandboxModels))) return 1;
    return await RunSandbox();
});

return await root.Parse(args).InvokeAsync();

// Loads every model the path resolved to, returning the index to open on, or -1 on a reported failure.
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
        Meshes.Register(ObjLoader.Load(path), Path.GetFileNameWithoutExtension(path));
        return true;
    }
    catch (Exception ex) when (ex is IOException or InvalidDataException)
    {
        Console.Error.WriteLine($"could not load '{path}': {ex.Message}");
        return false;
    }
}

// Registers each .obj, reporting the first failure rather than starting a UI that is missing what was asked for.
static bool LoadModels(string[]? paths) => (paths ?? []).All(LoadModel);

// The default scene: physics, a floor, and everything you can do to a pile of bodies.
static async Task<int> RunSandbox()
{
    // The whole shell — viewport, sidebar, menu, footer, keys — is assembled by SandboxShell so the headless
    // harness can drive the real one rather than a rebuild of it.
    SandboxShell.Sandbox app = default;
    app = SandboxShell.BuildSandbox(Populate, () => LoadMeshDialog(app));
    UI.Post(() => UI.SetFocus(app.View));

    await UI.Start(app.Root, width: 120, height: 44, fps: 60);
    app.Runner.Dispose();
    return 0;
}

// Load a model while the app is running, which is what --model used to be the only way to do. The chosen mesh
// becomes what the spawn keys produce, so the very next `n` or `f` throws it into the scene.
static void LoadMeshDialog(SandboxShell.Sandbox app) =>
    FileBrowser.OpenFile("Load a model", null, ["*.obj"], path =>
    {
        if (path is null) return;
        try
        {
            var id = Meshes.Register(ObjLoader.Load(path), Path.GetFileNameWithoutExtension(path));
            app.Sidebar.RefreshMeshes();
            app.View.Spawn.MeshId = id;
            app.View.Spawn.Shape = BodyShape.Mesh;
        }
        catch (Exception ex) when (ex is IOException or InvalidDataException)
        {
            Dialog.Message("Could not load", $"{Path.GetFileName(path)}: {ex.Message}");
        }
    });

// The viewer browses by DIRECTORY: '[' and ']' cycle everything in it, so opening a folder of models is the useful
// unit — the same rule ModelLibrary.Resolve applies to the command line.
static void OpenModelsDialog(SandboxShell.Viewer app) =>
    FileBrowser.OpenDirectory("Open a model folder", null, directory =>
    {
        if (directory is null) return;
        var start = LoadModelDirectory(directory);
        if (start < 0)
        {
            Dialog.Message("Nothing to show", $"No .obj files in {directory}.");
            return;
        }

        app.Model.Reload(start);
        app.Sidebar.Report();
    });

// The `obj` scene: one model, no physics. Same camera, same three renderers, same edge styles. The checkerboard
// ground stays — it costs nothing and it earns its place, giving the model a sense of scale and somewhere for the
// contact darkening to land, both of which a model floating in a void loses.
static async Task<int> RunModelViewer(int startIndex)
{
    if (Meshes.RegisteredCount == 0)
    {
        Console.Error.WriteLine("nothing to show.");
        return 1;
    }

    SandboxShell.Viewer app = default;
    app = SandboxShell.BuildViewer(startIndex, () => OpenModelsDialog(app));
    UI.Post(() => UI.SetFocus(app.View));

    await UI.Start(app.Root, width: 120, height: 44, fps: 60);
    return 0;
}

// A tower of boxes with a slight lean, plus a few spheres dropped alongside -- enough motion to show the camera,
// the depth sorting and the sleep dimming all working, and enough contact for the solver to have to do something.
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
