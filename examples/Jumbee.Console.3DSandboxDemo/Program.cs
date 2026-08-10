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

var runner = new PhysicsRunner(BuildScene);
var renderer = new WireframeRenderer();
var view = new SceneView(runner, renderer);
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
