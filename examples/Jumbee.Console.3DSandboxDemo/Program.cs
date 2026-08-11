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

    return set.Files.All(LoadModel) ? set.StartIndex : -1;
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
    var runner = new PhysicsRunner(BuildScene);

    // They reach the screen by genuinely different routes (a Canvas versus half-block cells with a private
    // z-buffer), which is why each brings its own surface and SceneView swaps the child rather than the drawing
    // code. The two solid ones share their rasteriser through MeshRenderer and differ only in shading.
    var view = new SceneView(runner, new ShadedRenderer());
    view.AddRenderer(new WireframeRenderer());
    view.AddRenderer(new SolidRenderer());
    var footer = new SceneFooter(view);

    // The footer reports the snapshot that was actually DRAWN, not the newest one, so its body count and clock
    // always agree with the picture above it.
    view.Drew += snapshot =>
    {
        footer.Snapshot = snapshot;
        footer.Paused = runner.Paused;
    };

    _ = new ControlFrame(view, borderStyle: BorderStyle.Rounded);

    // DockPanel, never a Grid at the root: the footer takes its two lines and the viewport fills whatever is left,
    // at every terminal size, with no split positions to recompute.
    var root = new DockPanel(DockedControlPlacement.Bottom, footer, view);

    // App-level keys are global hotkeys; everything that acts on the SCENE lives in SceneView.OnInput instead, so it
    // only fires while the viewport has focus and travels with the control.
    UI.RegisterHotKey(UI.HotKeys.Char(' '), () => runner.Paused = !runner.Paused);
    UI.RegisterHotKey(UI.HotKeys.Char('.'), runner.StepOnce);
    UI.RegisterHotKey(UI.HotKeys.Char('r'), () =>
    {
        view.Selected = null;
        runner.Post(scene => { scene.ClearBodies(); Populate(scene); });
    });

    UI.RegisterHotKey(UI.HotKeys.Char('q'), UI.Stop);
    UI.RegisterHotKey(UI.HotKeys.Ctrl(ConsoleKey.C), UI.Stop);
    UI.Post(() => UI.SetFocus(view));

    await UI.Start(root, width: 120, height: 40, fps: 60);
    runner.Dispose();
    return 0;
}

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

    var model = new ModelScene(startIndex);
    var viewer = new SceneView(model, new ShadedRenderer()) { Model = model };
    viewer.AddRenderer(new WireframeRenderer());
    viewer.AddRenderer(new SolidRenderer());

    var footer = new SceneFooter(viewer);
    viewer.Drew += s => footer.Snapshot = s;
    _ = new ControlFrame(viewer, borderStyle: BorderStyle.Rounded);
    var root = new DockPanel(DockedControlPlacement.Bottom, footer, viewer);

    // Only the app-level keys are global here; everything acting on the MODEL lives in SceneView so it travels with
    // the control. Deliberately no pause/step/reset -- there is no simulation to pause.
    UI.RegisterHotKey(UI.HotKeys.Char('q'), UI.Stop);
    UI.RegisterHotKey(UI.HotKeys.Ctrl(ConsoleKey.C), UI.Stop);
    UI.Post(() => UI.SetFocus(viewer));

    // Framed on the model rather than on a floor: it is scaled to ModelScene.ViewRadius and centred at the origin.
    viewer.Camera.Distance = 16f;
    viewer.Camera.Target = Vector3.Zero;

    await UI.Start(root, width: 120, height: 40, fps: 60);
    return 0;
}

// Runs on the physics thread before the first step.
static void BuildScene(PhysicsScene scene)
{
    // A wide, thin static slab centred under the origin, its top face at y = 0 so the floor grid the renderer draws
    // at y = 0 sits exactly on it.
    scene.AddStaticBox(new Vector3(0, -0.5f, 0), new Vector3(60, 1, 60));
    Populate(scene);
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
