using System.Numerics;

using Jumbee.Console;
using Jumbee.Console.SandboxDemo;

// --- Jumbee.Console 3D physics sandbox -------------------------------------------------------------------------
// A real-time rigid-body scene rendered in the terminal: Box3D (Erin Catto's engine, via a C# binding) simulating
// on its own thread, an orbit camera, and a wireframe renderer drawing projected edges onto a Canvas at braille
// resolution.
//
// The camera and projection are ~200 lines of System.Numerics (Camera.cs) -- Box3D speaks those types, so nothing
// is converted anywhere between the engine and the screen.
//
// Threading is the pattern from docs/controls/Live Data.md: the physics world, the body list and every Box3D handle
// belong to the physics thread, which publishes one immutable SceneSnapshot per tick. The UI thread only ever reads
// the newest snapshot (a reference swap, so it always sees a whole consistent tick) and posts scene mutations back
// through PhysicsRunner.Post. There is no lock anywhere. Picking, selection and the drag maths all run on the UI
// thread against the snapshot, so none of them can ever see a body mid-step.

// Complex geometry to spawn, grab and throw. The torus knot is generated, so the mesh path always works with no
// third-party asset and no licensing question; any .obj passed on the command line is registered alongside it.
//
// A mesh body RENDERS as its triangles but COLLIDES as its convex hull -- Box3D's triangle-mesh shape is static
// only, so that is not a shortcut but the only way a mesh can be a dynamic rigid body. See PhysicsScene.AddMeshBody.
Meshes.Register(Meshes.TorusKnot(), "knot");
foreach (var path in args.Where(a => a.EndsWith(".obj", StringComparison.OrdinalIgnoreCase)))
{
    try
    {
        Meshes.Register(ObjLoader.Load(path), Path.GetFileNameWithoutExtension(path));
    }
    catch (Exception ex) when (ex is IOException or InvalidDataException)
    {
        Console.Error.WriteLine($"could not load '{path}': {ex.Message}");
        return 1;
    }
}

var runner = new PhysicsRunner(BuildScene);

// Three renderers over one scene, cycled live with 'v', in rising order of cost and fidelity:
//
//   wireframe -- projected edges on a Canvas at braille resolution; painter's sort, no fill.
//   solid     -- z-buffered flat-shaded triangles from a directional light; one colour per face.
//   shaded    -- per-pixel point light with specular, plus silhouettes and contact darkening.
//
// They reach the screen by genuinely different routes (a Canvas versus half-block cells with a private z-buffer),
// which is why each brings its own surface and SceneView swaps the child rather than the drawing code. The two
// solid ones share their rasteriser through MeshRenderer and differ only in shading.
var view = new SceneView(runner, new ShadedRenderer());
view.AddRenderer(new WireframeRenderer());
view.AddRenderer(new SolidRenderer());
var footer = new SceneFooter(view);

// The footer reports the snapshot that was actually DRAWN, not the newest one, so its body count and clock always
// agree with the picture above it.
view.Drew += snapshot =>
{
    footer.Snapshot = snapshot;
    footer.Paused = runner.Paused;
};

_ = new ControlFrame(view, borderStyle: BorderStyle.Rounded);

// DockPanel, never a Grid at the root: the footer takes its two lines and the viewport fills whatever is left, at
// every terminal size, with no split positions to recompute.
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

// Runs on the physics thread before the first step.
static void BuildScene(PhysicsScene scene)
{
    // A wide, thin static slab centred under the origin, its top face at y = 0 so the floor grid the renderer draws
    // at y = 0 sits exactly on it.
    scene.AddStaticBox(new Vector3(0, -0.5f, 0), new Vector3(60, 1, 60));
    Populate(scene);
}

// A tower of boxes with a slight lean, plus a few spheres dropped alongside -- enough motion to show the camera,
// the painter's sort and the sleep dimming all working, and enough contact for the solver to have to do something.
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
