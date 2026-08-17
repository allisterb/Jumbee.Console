using System.Numerics;

using Jumbee.Console;
using Jumbee.Console.SandboxDemo;
using Jumbee.Console.Snapshot;

// Headless behaviour check for M1: build the same control tree Program.cs does and exercise spawn, launch, pick,
// grab-drag, delete and clear against the real physics thread.
if (args.Contains("--probe")) { Probe.LaunchProbe.Run(98, 30); return 0; }

if (args.Contains("--load"))
{
    var dir = args.FirstOrDefault(a => a.Contains("dir="))?.Split('=')[1]
              ?? @"C:\Projects\Jumbee.Console\reference\projects\voxcii-main\models";
    foreach (var f in Directory.GetFiles(dir, "*.obj").OrderBy(x => x))
    {
        var sw0 = System.Diagnostics.Stopwatch.StartNew();
        var m = ObjLoader.Load(f);
        Console.WriteLine($"  {Path.GetFileName(f),-12} {m.TriangleCount,7} tris  parse {sw0.ElapsedMilliseconds,5} ms  " +
                          $"{new FileInfo(f).Length / 1024,6} KB  " +
                          $"extents {m.Extents.X:F3},{m.Extents.Y:F3},{m.Extents.Z:F3}");
    }

    return 0;
}

var sizeArg = Array.Find(args, a => a.Contains('x') && char.IsDigit(a[0]))?.Split('x');
var W = sizeArg is null ? 100 : int.Parse(sizeArg[0]);
var H = sizeArg is null ? 34 : int.Parse(sizeArg[1]);
var failures = 0;

// --- M3: the shell -------------------------------------------------------------------------------------------
// Drives SandboxShell -- the REAL assembly Program.cs uses, not a rebuild of it -- and checks the claim the
// sidebar exists to make: that a key and its widget always agree, in both directions.
if (args.Contains("--shell")) return Render3d.ShellChecks.Run(W, H, args);

void Check(string what, bool ok, string? detail = null)
{
    Console.WriteLine($"  {(ok ? "PASS" : "FAIL")}  {what}{(detail is null ? "" : $"  [{detail}]")}");
    if (!ok) failures++;
}

// --- meshes -------------------------------------------------------------------------------------------------------
// Registered before anything else: the spawn path and the renderers all key off the registry.
var knotId = Meshes.Register(Meshes.TorusKnot(), "knot");
var teapotPath = @"C:\Projects\Jumbee.Console\reference\projects\voxcii-main\models\teapot.obj";
var teapotId = File.Exists(teapotPath) ? Meshes.Register(ObjLoader.Load(teapotPath), "teapot") : -1;

var runner = new PhysicsRunner(scene =>
{
    scene.AddStaticBox(new Vector3(0, -0.5f, 0), new Vector3(60, 1, 60));
    for (var i = 0; i < 7; i++)
        scene.AddBox(new Vector3(i * 0.06f, 0.5f + (i * 1.02f), 0), new Vector3(0.5f, 0.5f, 0.5f), i);
    for (var i = 0; i < 4; i++)
        scene.AddSphere(new Vector3(-4f + (i * 0.8f), 6f + (i * 1.5f), 1.5f), 0.45f, 7 + i);
});

ISceneRenderer renderer = new SolidRenderer();   // Pick is a default interface method: reachable only through the interface
var view = new SceneView(runner, renderer);
var footer = new SceneFooter(view);
view.Drew += s => footer.Snapshot = s;
_ = new ControlFrame(view, borderStyle: BorderStyle.Rounded);
var root = new DockPanel(DockedControlPlacement.Bottom, footer, view);

// Lays the tree out so ActualWidth/Height (and so the Viewport) are real.
_ = ConsoleSnapshot.ToText(root, W, H);

// Waits for the physics thread to drain posted commands and step past them.
SceneSnapshot Settle(int steps = 12)
{
    var target = runner.Snapshot.StepCount + steps;
    var spun = 0;
    while (runner.Snapshot.StepCount < target && spun++ < 400) Thread.Sleep(5);
    var s = runner.Snapshot;
    renderer.Draw(s, view.Camera);
    return s;
}

var start = Settle(180);
Console.WriteLine($"scene settled: {start.Count} bodies, {start.AwakeCount} awake, t={start.SimTime:F2}s");
Console.WriteLine($"viewport {renderer.Viewport.Width}x{renderer.Viewport.Height} " +
                  $"(cell aspect {renderer.Viewport.CellAspect:F3})\n");

// --- keyboard ---------------------------------------------------------------------------------------------------
// Routed the way the LIVE LOOP routes it: UI.OnInput hands the event to the ROOT LAYOUT, which walks down to the
// focused descendant. Not UI.SendInput -- that dispatches straight to view.FocusableControl and takes a different
// path through ControlFrame, so it can pass while the real app receives nothing.
Console.WriteLine("keyboard (routed through the root layout, as the live loop does):");
UI.SetFocus(view);
Check("the viewport takes focus", view.IsFocused, $"IsFocused={view.IsFocused}");

void SendKey(ConsoleKey key, char ch = '\0', bool shift = false) =>
    root.OnInput(new UI.InputEventArgs(new ConsoleGUI.Input.InputEvent(new ConsoleKeyInfo(ch, key, shift, false, false))));

var theta0 = view.Camera.Theta;
SendKey(ConsoleKey.RightArrow);
Check("Right arrow orbits the camera", Math.Abs(view.Camera.Theta - theta0) > 1e-4f,
    $"theta {theta0:F3} -> {view.Camera.Theta:F3}");

var phi0 = view.Camera.Phi;
SendKey(ConsoleKey.UpArrow);
Check("Up arrow tilts the camera", Math.Abs(view.Camera.Phi - phi0) > 1e-4f,
    $"phi {phi0:F3} -> {view.Camera.Phi:F3}");

var dist0 = view.Camera.Distance;
SendKey(ConsoleKey.PageUp);
Check("PageUp zooms in", view.Camera.Distance < dist0, $"{dist0:F2} -> {view.Camera.Distance:F2}");

SendKey(ConsoleKey.Home);
Check("Home resets the camera", Math.Abs(view.Camera.Distance - 20f) < 1e-3f, $"{view.Camera.Distance:F2}");

var shapeBefore = view.Spawn.Shape;
SendKey(ConsoleKey.B, 'b');
Check("a letter key reaches the viewport", view.Spawn.Shape != shapeBefore, $"{shapeBefore} -> {view.Spawn.Shape}");
view.Spawn.Shape = shapeBefore;
Console.WriteLine();

// --- spawn ------------------------------------------------------------------------------------------------------
Console.WriteLine("spawn:");
var before = start.Count;
view.SpawnAtTarget();
var afterSpawn = Settle();
Check("n drops a body in", afterSpawn.Count == before + 1, $"{before} -> {afterSpawn.Count}");
var spawned = afterSpawn.IndexOf(afterSpawn.Ids[^1]);
Check("it appears above the camera target", afterSpawn.Positions[spawned].Y > view.Camera.Target.Y,
    $"y={afterSpawn.Positions[spawned].Y:F2} vs target y={view.Camera.Target.Y:F2}");

view.Spawn.ToggleShape();
view.SpawnAtTarget();
var afterSphere = Settle();
Check("b switches the spawn shape", afterSphere.Shapes[afterSphere.Count - 1] == BodyShape.Sphere,
    afterSphere.Shapes[afterSphere.Count - 1].ToString());

// --- launch -----------------------------------------------------------------------------------------------------
Console.WriteLine("\nlaunch:");
before = afterSphere.Count;
view.Launch();
var afterLaunch = Settle(2);   // read it early, before gravity and contacts dominate
var launched = afterLaunch.Count - 1;
var forward = view.Camera.GetView().Forward;
var launchVelocity = afterLaunch.Velocities[launched];
Check("f fires a body", afterLaunch.Count == before + 1, $"{before} -> {afterLaunch.Count}");
Check("it moves away along the view direction", Vector3.Dot(Vector3.Normalize(launchVelocity), forward) > 0.9f,
    $"dot={Vector3.Dot(Vector3.Normalize(launchVelocity), forward):F3}");
Check("at roughly the launch speed", Math.Abs(launchVelocity.Length() - view.Spawn.LaunchSpeed) < 3f,
    $"{launchVelocity.Length():F1} vs {view.Spawn.LaunchSpeed:F1}");

// --- pick -------------------------------------------------------------------------------------------------------
Console.WriteLine("\npick:");
var scene = Settle(30);
// Project a known body to its screen cell, then pick that cell back and expect the same body -- this is the round
// trip that silently breaks if the renderer's bounds and the viewport's un-projection ever disagree.
var probe = 0;
var cameraView = view.Camera.GetView();
renderer.Projection.TryProject(cameraView.Transform(scene.Positions[probe]), out var px, out var py);
var vp = renderer.Viewport;
var col = (int)Math.Round((px + 1f) / 2f * (vp.Width - 1));
var row = (int)Math.Round((vp.CellAspect - py) / (2 * vp.CellAspect) * (vp.Height - 1));
var picked = renderer.Pick(col, row, scene, view.Camera);
Check("projecting a body and picking that cell returns it", picked == scene.Ids[probe],
    $"cell ({col},{row}) -> {picked?.ToString() ?? "null"}, expected {scene.Ids[probe]}");
Check("picking empty sky returns nothing", renderer.Pick(1, 1, scene, view.Camera) is null);

// --- mouse ------------------------------------------------------------------------------------------------------
// Routed as a real click, through ConsoleManager's cell mouse listeners -- the same question the arrow keys turned
// out to have: does the event reach the composite at all, given its child is display-only?
Console.WriteLine("\nmouse:");
view.Selected = null;
_ = ConsoleSnapshot.ToTextAfterClick(root, W, H, col + 1, row + 1);   // +1 for the frame border
Check("clicking a body selects it", view.Selected == scene.Ids[probe],
    $"cell ({col + 1},{row + 1}) -> {view.Selected?.ToString() ?? "null"}, expected {scene.Ids[probe]}");

// --- selection --------------------------------------------------------------------------------------------------
Console.WriteLine("\nselection:");
view.Selected = scene.Ids[0];
Check("the renderer is told what is selected", renderer.Selected == scene.Ids[0]);
view.SelectNext();
Check("Tab moves to the next body", view.Selected == scene.Ids[1], $"{view.Selected}");

// --- grab -------------------------------------------------------------------------------------------------------
Console.WriteLine("\ngrab:");
var grabId = scene.Ids[0];
var grabIndex = scene.IndexOf(grabId);
var from = scene.Positions[grabIndex];
var to = from + new Vector3(0, 5f, 0);
runner.Post(s => s.BeginGrab(grabId));
Settle(2);
for (var i = 0; i < 20; i++)
{
    runner.Post(s => s.DragTo(grabId, to));
    Settle(2);
}

var dragged = runner.Snapshot;
var draggedPos = dragged.Positions[dragged.IndexOf(grabId)];
Check("a held body tracks its drag target", Vector3.Distance(draggedPos, to) < 0.5f,
    $"{draggedPos.Y:F2} vs target {to.Y:F2}");

runner.Post(s => s.ReleaseGrab(new Vector3(0, -8f, 0)));
Settle(2);
var thrown = runner.Snapshot;
Check("releasing throws it at the given velocity",
    thrown.Velocities[thrown.IndexOf(grabId)].Y < -4f,
    $"vy={thrown.Velocities[thrown.IndexOf(grabId)].Y:F2}");
var afterFall = Settle(60);
Check("and it falls back under gravity", afterFall.Positions[afterFall.IndexOf(grabId)].Y < draggedPos.Y,
    $"{draggedPos.Y:F2} -> {afterFall.Positions[afterFall.IndexOf(grabId)].Y:F2}");

// --- delete and clear -------------------------------------------------------------------------------------------
Console.WriteLine("\ndelete / clear:");
var current = runner.Snapshot;
before = current.Count;
view.Selected = current.Ids[2];
var doomed = current.Ids[2];
view.DeleteSelected();
var afterDelete = Settle();
Check("x deletes the selected body", afterDelete.Count == before - 1, $"{before} -> {afterDelete.Count}");
Check("and it is really gone", afterDelete.IndexOf(doomed) < 0);
Check("the selection clears with it", view.Selected is null, $"{view.Selected}");

view.ClearScene();
var afterClear = Settle();
Check("c removes every body", afterClear.Count == 0, $"{afterClear.Count}");

// --- the picture still draws ------------------------------------------------------------------------------------
Console.WriteLine("\nrender:");
runner.Post(s =>
{
    for (var i = 0; i < 7; i++) s.AddBox(new Vector3(i * 0.06f, 0.5f + (i * 1.02f), 0), new Vector3(0.5f), i);
});

var final = Settle(120);
view.Selected = final.Ids[3];
footer.Snapshot = final;   // in the app this arrives via SceneView.Drew
renderer.Draw(final, view.Camera);
var text = ConsoleSnapshot.ToText(root, W, H);
var ink = text.Count(c => c is not (' ' or '\n' or '\r'));
Check("the scene still renders", ink > 500, $"{ink} non-blank glyphs");
Check("the footer reports the selection", text.Contains($"#{final.Ids[3]}"), "looking for the selected body id");

// --- the highlight actually reaches the screen --------------------------------------------------------------------
// Text alone cannot show this: the selection is a COLOUR change plus a crosshair. Render through the real ANSI path
// and count cells emitted in the selection colour.
Console.WriteLine("\nhighlight:");

int SelectionCells()
{
    var screen = AnsiConsoleSnapshot.RenderAsync(root, W, H).GetAwaiter().GetResult();
    var n = 0;
    for (var y = 1; y < H - 3; y++)
    {
        for (var x = 1; x < W - 1; x++)
        {
            // Not an exact match on Palette.Selection: the tint is lit and quantised like any other surface, so it
            // arrives dimmed. Its signature is that it is ACHROMATIC and bright — every body colour and both
            // ground shades have unequal channels, so nothing else in the scene can produce this.
            var fg = screen.Buffer[x, y].Character.Foreground;
            if (fg is { } c && c.Red == c.Green && c.Green == c.Blue && c.Red > 120) n++;
        }
    }

    return n;
}

view.Selected = null;
renderer.Draw(final, view.Camera);
var unselectedCells = SelectionCells();
view.Selected = final.Ids[3];
renderer.Draw(final, view.Camera);
var selectedCells = SelectionCells();
Check("selecting a body lights cells in the selection colour", selectedCells > unselectedCells,
    $"{unselectedCells} -> {selectedCells} cells");

// --- meshes -------------------------------------------------------------------------------------------------------
Console.WriteLine("\nmeshes:");
var knot = Meshes.Get(knotId);
Check("the torus knot generates", knot.TriangleCount > 500, $"{knot.TriangleCount} triangles, {knot.Vertices.Length} verts");

if (teapotId >= 0)
{
    var teapot = Meshes.Get(teapotId);
    Check("the teapot OBJ loads", teapot.TriangleCount > 3000,
        $"{teapot.TriangleCount} triangles, {teapot.Vertices.Length} verts");

    var radius = 0f;
    foreach (var v in teapot.Vertices) radius = Math.Max(radius, Math.Max(Math.Abs(v.X), Math.Max(Math.Abs(v.Y), Math.Abs(v.Z))));
    Check("it is normalised to a half-extent of 0.5", Math.Abs(radius - 0.5f) < 0.01f, $"largest extent {radius:F3}");
}
else
{
    Console.WriteLine("  SKIP  teapot.obj not found at the reference path");
}

// --- the wireframe's mesh thinning ---------------------------------------------------------------------------
// Its budget follows the body's ON-SCREEN size, so the same mesh must draw substantially more ink when it fills
// the viewport than when it is a sandbox-sized body across the floor -- that is the whole claim, and a flat cap
// (which is what was here) fails it. Counted as lit sub-pixels rather than as an internal edge count, so it is
// what a human would see. Bounded above too: MaxTriangles must actually bound the ink.
{
    int LitCells(ISceneRenderer r, SceneSnapshot s, OrbitCamera c)
    {
        _ = ConsoleSnapshot.ToText(r.Surface, W, H);
        r.Draw(s, c);
        return ConsoleSnapshot.ToText(r.Surface, W, H).Count(ch => ch is not (' ' or '\n' or '\r'));
    }

    var meshId = teapotId >= 0 ? teapotId : knotId;
    var one = new SceneSnapshot(1) { Count = 1, AwakeCount = 1 };
    one.Ids[0] = 1;
    one.Shapes[0] = BodyShape.Mesh;
    one.MeshIds[0] = meshId;
    one.Positions[0] = new Vector3(0, 5.5f, 0);
    one.Rotations[0] = Quaternion.Identity;
    one.HalfExtents[0] = new Vector3(ModelScene.ViewRadius);
    one.ColorKeys[0] = 1;
    one.Awake[0] = true;

    var wire = new WireframeRenderer();
    var near = new OrbitCamera { Target = new Vector3(0, 5.5f, 0), Distance = 20f };
    var far = new OrbitCamera { Target = new Vector3(0, 5.5f, 0), Distance = 55f };

    var nearCells = LitCells(wire, one, near);
    var farCells = LitCells(wire, one, far);
    Check("a mesh filling the viewport draws far more than a distant one", nearCells > farCells * 2,
        $"{nearCells} lit cells near, {farCells} far");

    // The floor grid is drawn every frame whatever the body does, so a lower bound on the near shot is really a
    // lower bound on grid + body. It still separates the two regimes: the old flat 64-edge cap put both shots
    // within a few dozen cells of each other.
    Check("the near shot is bounded, not a filled scribble", nearCells < W * H / 2,
        $"{nearCells} of {W * H} cells");

    // And the thinned sample must cover the WHOLE model, not a prefix of its triangle list. This is the check that
    // would have caught drawing the bunny with its back half missing: every earlier check passed while a third to a
    // half of the mesh was never even considered, because the drawn TOTAL was right and only its extent was wrong.
    //
    // Run over TWO subjects, and the second is the point. A uniformly tessellated model cannot fail the density
    // half of this: it takes an authored asset, where detail geometry holds most of the triangles while flat
    // panels hold most of the area, to catch a sampler that spends the budget evenly by triangle count. The
    // reference plane is that asset -- 81k triangles whose wings are a tiny fraction of the list.
    //
    // Rendered LARGER than the rest of these checks, and that is load-bearing. The budget follows on-screen area,
    // so at 100x34 the teapot gets ~190 triangles and a tenth of the frame can be legitimately blank just from the
    // sampling rate -- which swamps the signal (measured: 80% with the code correct against 84% with the bug, i.e.
    // backwards). At 240x80 it gets ~1,100 and blank means missing.
    const int CW = 240, CH = 80;
    var planePath = @"C:\Projects\Jumbee.Console\media\models\plane.obj";
    var subjects = new List<(string Name, int Id)> { (teapotId >= 0 ? "teapot" : "knot", meshId) };
    if (File.Exists(planePath)) subjects.Add(("plane", Meshes.Register(ObjLoader.Load(planePath), "plane")));
    else Console.WriteLine("  SKIP  plane.obj not found -- the non-uniform-tessellation subject");

    foreach (var (subject, id) in subjects)
    {
    one.MeshIds[0] = id;
    var bare = new WireframeRenderer { GridHalfExtent = 0 };   // no floor grid, so the ink IS the body
    var mesh = Meshes.Get(id);
    var worst = 1.0;
    foreach (var theta in new[] { 0f, MathF.PI / 3, MathF.PI / 2, 2f * MathF.PI / 3, MathF.PI })
    {
        var camera = new OrbitCamera { Target = new Vector3(0, 5.5f, 0), Distance = 22f, Theta = theta };
        _ = ConsoleSnapshot.ToText(bare.Surface, CW, CH);
        bare.Draw(one, camera);
        var rows = ConsoleSnapshot.ToText(bare.Surface, CW, CH).Split('\n');

        // A coarse occupancy grid over the viewport, not a bounding box: the failure this guards against leaves a
        // HOLE in the middle of the model, and a bounding box cannot see one -- the extremities still get drawn, so
        // the box comes out the right size while a third of the body is missing. (Learned the hard way: the first
        // version of this check passed with the bug deliberately reintroduced.)
        const int G = 12;
        var inked = new bool[G, G];
        var wanted = new bool[G, G];
        for (var cy = 0; cy < rows.Length; cy++)
        {
            for (var cx = 0; cx < rows[cy].Length; cx++)
            {
                if (rows[cy][cx] is ' ' or '\r') continue;
                inked[Math.Min(G - 1, cx * G / CW), Math.Min(G - 1, cy * G / CH)] = true;
            }
        }

        // What the geometry says should be occupied, computed the same way the renderer decides -- over the whole
        // triangle list with nothing thinned, and counting only FRONT-FACING triangles. Using every vertex instead
        // makes the metric mush: the teapot is hollow with a handle and a spout, so cells covered only by faces
        // pointing away are permanently unreachable and the shortfall they cause (7%) is the same size as the bug's.
        var basis = camera.GetView();
        var port = bare.Viewport;
        var scale = Matrix4x4.CreateScale(ModelScene.ViewRadius / 0.5f);
        var posed = new Vector3[mesh.Vertices.Length];
        for (var v = 0; v < posed.Length; v++)
            posed[v] = one.Positions[0] + Vector3.Transform(mesh.Vertices[v], scale);

        for (var t = 0; t + 2 < mesh.Indices.Length; t += 3)
        {
            var a = posed[mesh.Indices[t]];
            var b = posed[mesh.Indices[t + 1]];
            var c = posed[mesh.Indices[t + 2]];
            if (Vector3.Dot(Vector3.Cross(b - a, c - a), basis.Eye - a) <= 0) continue;
            if (!bare.Projection.TryProject(basis.Transform((a + b + c) / 3f), out var nx, out var ny)) continue;
            var cx = (int)((nx + 1f) / 2f * (port.Width - 1));
            var cy = (int)((port.CellAspect - ny) / (2.0 * port.CellAspect) * (port.Height - 1));
            if (cx < 0 || cy < 0 || cx >= port.Width || cy >= port.Height) continue;
            wanted[Math.Min(G - 1, cx * G / CW), Math.Min(G - 1, cy * G / CH)] = true;
        }

        int want = 0, got = 0;
        for (var gx = 0; gx < G; gx++)
        {
            for (var gy = 0; gy < G; gy++)
            {
                if (!wanted[gx, gy]) continue;
                want++;
                if (inked[gx, gy]) got++;
            }
        }

        worst = Math.Min(worst, got / Math.Max(1.0, want));
    }

    // Threshold sits between measured states, not at an aspiration. All three were measured by deliberately
    // breaking the renderer and re-running, which is the only way to know a check is not vacuous:
    //
    //   correct                                     teapot 96%   plane 95%
    //   pass 2 picking evenly by count              teapot 96%   plane 90%   <- the tessellation-density bug
    //   pass 1 exiting early at the budget                        both ~60%   <- the missing-region bug
    //
    // 100% is not reachable: a thinned sample always misses some sliver-thin cell at the silhouette. Note the
    // teapot is blind to the density bug -- it is uniformly tessellated, so only the plane's row moves. An
    // earlier, coarser version of this grid (G = 6) could not see it either and passed at 100% both ways.
    Check($"the thinned sample leaves no holes in the {subject}, from every angle", worst > 0.93,
        $"worst angle inks {worst:P0} of the cells the geometry occupies");
    }
}

// Triangulation gets its own synthetic case: every model in the reference set is ALREADY triangulated, so loading
// one exercises nothing here. A quad and a pentagon must fan out to 2 and 3 triangles.
var ngon = ObjLoader.Parse(
[
    "v -1 0 -1", "v 1 0 -1", "v 1 0 1", "v -1 0 1", "v 0 1 0",
    "f 1 2 3 4",          // quad -> 2 triangles
    "f 1 2 3 4 5",        // pentagon -> 3 triangles
]);
Check("n-gon faces fan-triangulate", ngon.TriangleCount == 5, $"{ngon.TriangleCount} triangles, expected 2+3");

// Face-index forms must all resolve to the same geometry: bare, v/vt, v//vn, v/vt/vn, and negative (relative).
var forms = ObjLoader.Parse(["v 0 0 0", "v 1 0 0", "v 0 1 0", "f 1/1 2//2 3/3/3"]);
Check("v/vt/vn index forms parse", forms.TriangleCount == 1, $"{forms.TriangleCount} triangles");
var relative = ObjLoader.Parse(["v 0 0 0", "v 1 0 0", "v 0 1 0", "f -3 -2 -1"]);
Check("negative (relative) indices parse", relative.TriangleCount == 1, $"{relative.TriangleCount} triangles");

// A mesh body must be a real dynamic rigid body: it falls, it lands, it sleeps. Spawned well clear of the box
// tower -- an earlier version dropped it at the origin, straight into the stack, and "it never fell" was the tower
// holding it up rather than anything wrong with the body.
var meshSpawn = teapotId >= 0 ? teapotId : knotId;
runner.Post(s => s.AddMeshBody(meshSpawn, new Vector3(6f, 8f, 6f), 1.5f, 42));
var withMesh = Settle(20);
var meshIndex = withMesh.Count - 1;
Check("a mesh body spawns", withMesh.Shapes[meshIndex] == BodyShape.Mesh, withMesh.Shapes[meshIndex].ToString());
Check("it carries its mesh id", withMesh.MeshIds[meshIndex] == meshSpawn, $"{withMesh.MeshIds[meshIndex]}");
Check("it has mass from its hull", withMesh.Masses[meshIndex] > 0, $"{withMesh.Masses[meshIndex]:F1}kg");

var startY = withMesh.Positions[meshIndex].Y;
var landed = Settle(240);
var landedIndex = landed.IndexOf(withMesh.Ids[meshIndex]);
Check("it falls under gravity", landed.Positions[landedIndex].Y < startY,
    $"{startY:F2} -> {landed.Positions[landedIndex].Y:F2}");
Check("and comes to rest on the floor", landed.Positions[landedIndex].Y > 0f && landed.Positions[landedIndex].Y < 2f,
    $"y={landed.Positions[landedIndex].Y:F2}");

// --- model viewer -------------------------------------------------------------------------------------------------
// The `obj` scene: no physics, one body, full affine transform. The claim under test is that shear reaches the
// rasteriser at all -- a quaternion cannot carry it, so it travels via SceneSnapshot.LocalTransforms.
Console.WriteLine("\nmodel viewer:");
var scene3 = new ModelScene();
Check("it publishes one body", scene3.Snapshot.Count == 1, $"{scene3.Snapshot.Count}");
Check("that body is a mesh", scene3.Snapshot.Shapes[0] == BodyShape.Mesh);
Check("with a local transform", scene3.Snapshot.LocalTransforms is not null);

var startName = scene3.Name;
scene3.Step(+1);
Check("] steps to the next model", scene3.Name != startName || Meshes.RegisteredCount == 1,
    $"{startName} -> {scene3.Name}");

// Shear must actually move geometry: transform a point above the origin and check it slides sideways.
scene3.ResetTransform();
var upright = Vector3.Transform(new Vector3(0, 0.5f, 0), scene3.Snapshot.LocalTransforms![0]);
scene3.Nudge(0.8f, 0);
var sheared = Vector3.Transform(new Vector3(0, 0.5f, 0), scene3.Snapshot.LocalTransforms![0]);
Check("shear displaces a point by its height", Math.Abs(sheared.X - upright.X) > 0.5f,
    $"x {upright.X:F2} -> {sheared.X:F2}");
Check("and leaves the origin alone",
    Vector3.Transform(Vector3.Zero, scene3.Snapshot.LocalTransforms![0]).Length() < 1e-4f);

scene3.ResetTransform();
scene3.ScaleAxis(1, 2f);
var stretched = Vector3.Transform(new Vector3(0, 0.5f, 0), scene3.Snapshot.LocalTransforms![0]);
Check("non-uniform scale stretches one axis only", stretched.Y > upright.Y * 1.5f,
    $"y {upright.Y:F2} -> {stretched.Y:F2}");

// And it renders: a viewer over the same renderers must fill the viewport with the model.
scene3.ResetTransform();
var viewerRenderer = new ShadedRenderer();
var viewer = new SceneView(scene3, viewerRenderer) { Model = scene3 };
var viewerFooter = new SceneFooter(viewer);
_ = new ControlFrame(viewer, borderStyle: BorderStyle.Rounded);
var viewerRoot = new DockPanel(DockedControlPlacement.Bottom, viewerFooter, viewer);
viewer.Camera.Distance = 16f;
viewer.Camera.Target = Vector3.Zero;
_ = ConsoleSnapshot.ToText(viewerRoot, W, H);
viewerRenderer.Draw(scene3.Snapshot, viewer.Camera);
var viewerText = ConsoleSnapshot.ToText(viewerRoot, W, H);
Check("the viewer renders the model", viewerText.Count(c => c is not (' ' or '\n' or '\r')) > 500,
    $"{viewerText.Count(c => c is not (' ' or '\n' or '\r'))} glyphs");
Check("its footer names the model", viewerText.Contains(scene3.Name), scene3.Name);

// --- obj path resolution ------------------------------------------------------------------------------------------
// One argument, two meanings. Every branch here is an edge case, which is why it lives in ModelLibrary rather than
// inside Program where it could only be exercised by launching a UI.
Console.WriteLine("\nobj path resolution:");
var modelDir = @"C:\Projects\Jumbee.Console\reference\projects\voxcii-main\models";

var dirSet = ModelLibrary.Resolve(modelDir);
Check("a directory loads every .obj in it", dirSet.Files.Length == 4 && dirSet.Error is null,
    $"{dirSet.Files.Length} files, error={dirSet.Error ?? "none"}");
Check("and opens on the first by name", Path.GetFileName(dirSet.Files[dirSet.StartIndex]) == "bunny.obj",
    Path.GetFileName(dirSet.Files[dirSet.StartIndex]));

// A named FILE must not narrow the set -- it only chooses where cycling starts.
var fileSet = ModelLibrary.Resolve(Path.Combine(modelDir, "teapot.obj"));
Check("a named file still loads the whole directory", fileSet.Files.Length == 4, $"{fileSet.Files.Length} files");
Check("but opens on that file", Path.GetFileName(fileSet.Files[fileSet.StartIndex]) == "teapot.obj",
    Path.GetFileName(fileSet.Files[fileSet.StartIndex]));

var missing = ModelLibrary.Resolve(Path.Combine(modelDir, "nope-does-not-exist"));
Check("a path that is neither file nor directory is an error", missing.Error is not null, missing.Error);

var emptyDir = ModelLibrary.Resolve(@"C:\Projects\Jumbee.Console\src");
Check("a directory with no models is an error", emptyDir.Error is not null, emptyDir.Error);

// No argument is NOT an error even with nothing to find: the viewer falls back to its generated mesh.
var noneGiven = ModelLibrary.Resolve(null, @"C:\Projects\Jumbee.Console\src");
Check("no argument is never an error", noneGiven.Error is null && noneGiven.Files.Length == 0,
    $"{noneGiven.Files.Length} files, error={noneGiven.Error ?? "none"}");

// The start index must survive into the scene.
var startAt = new ModelScene(1);
Check("ModelScene opens on the requested index", startAt.MeshId == 1, $"{startAt.MeshId} ({startAt.Name})");
Check("and clamps an out-of-range one", new ModelScene(999).MeshId == Meshes.RegisteredCount - 1);

// --- solid renderer -----------------------------------------------------------------------------------------------
Console.WriteLine("\nsolid renderer:");
var solid = new ShadedRenderer();
view.AddRenderer(solid);
view.NextRenderer();
Check("v switches renderer", ReferenceEquals(view.Renderer, solid), view.Renderer.Name);

var solidScene = Settle(4);
solid.Draw(solidScene, view.Camera);
Check("the solid surface got a viewport", solid.Viewport.IsValid,
    $"{solid.Viewport.Width}x{solid.Viewport.Height}");

var solidScreen = AnsiConsoleSnapshot.RenderAsync(root, W, H).GetAwaiter().GetResult();
var half = 0;
var distinct = new HashSet<(byte, byte, byte, byte, byte, byte)>();
for (var y = 1; y < H - 3; y++)
{
    for (var x = 1; x < W - 1; x++)
    {
        var ch = solidScreen.Buffer[x, y].Character;
        if (ch.Content != '▀') continue;
        half++;
        if (ch.Foreground is { } f && ch.Background is { } b)
            distinct.Add((f.Red, f.Green, f.Blue, b.Red, b.Green, b.Blue));
    }
}

Check("it fills the viewport with half-blocks", half > (W - 2) * (H - 4) * 0.8, $"{half} cells");
// The whole point of quantising the shade ramp: a handful of levels, not a continuum. If this ran into the hundreds
// the renderer would be in the expensive column measured at M0.1 (7x the ANSI bytes).
//
// The bound is loose because the count GROWS WITH THE VIEWPORT -- a bigger frame shows more of the ground at more
// distances, so more distinct fg/bg pairs even though the ramp itself has not changed: 105 at 100x34 against 127 at
// 200x50. The old fixed 120 therefore passed at the harness default and failed under `--perf 200x50`, which reads
// as a renderer regression and is not one. What it is really guarding against is a continuum, which is thousands.
Check("the shade ramp is quantised", distinct.Count is > 2 and < 400, $"{distinct.Count} distinct fg/bg pairs");

// Depth: with the camera outside the scene, every drawn sub-pixel must be in front of it, and the ground alone
// cannot explain a body standing on it -- so check something is nearer than the ground plane directly behind it.
var nearest = 0f;
for (var y = 0; y < solid.Viewport.Height * 2; y++)
    for (var x = 0; x < solid.Viewport.Width; x++)
        nearest = Math.Max(nearest, ((HalfBlockSurface)solid.Surface).DepthAt(x, y));
Check("the z-buffer holds real depths", nearest > 0, $"nearest 1/z = {nearest:F4} (={1 / Math.Max(nearest, 1e-6f):F2} units)");

// Ground truth for the rasteriser: for a sub-pixel that shows bare ground, cast that pixel's ray, intersect y=0
// analytically, and compare the camera-space depth with what the z-buffer holds. This catches a projection or
// interpolation that is subtly wrong in a way a picture never would.
var surface = (HalfBlockSurface)solid.Surface;
var solidView = view.Camera.GetView();
var checkedPixels = 0;
var worstError = 0.0;
for (var sy = surface.PixelHeight - 2; sy > surface.PixelHeight / 2 && checkedPixels < 12; sy -= 3)
{
    // Well off to the side, so the sample lands on open ground rather than on the tower.
    var sx = surface.PixelWidth / 6;
    var stored = surface.DepthAt(sx, sy);
    if (stored <= 0) continue;

    // The sub-pixel's NDC, inverted through the same mapping ToScreen uses.
    var ndcX = (sx + 0.5f - (surface.PixelWidth / 2f)) / (surface.PixelWidth / 2f);
    var ndcY = ((surface.PixelHeight / 2f) - (sy + 0.5f)) / (float)(surface.PixelHeight / (2.0 * solid.Viewport.CellAspect));
    var dir = Vector3.Normalize((solidView.Right * (ndcX / solid.Projection.Focal))
                                + (solidView.Up * (ndcY / solid.Projection.Focal)) + solidView.Forward);
    if (!Projection.TryPlaneHit(solidView.Eye, dir, Vector3.Zero, Vector3.UnitY, out var groundHit)) continue;

    var expected = solidView.Transform(groundHit).Z;
    var actual = 1f / stored;
    worstError = Math.Max(worstError, Math.Abs(actual - expected) / expected);
    checkedPixels++;
}

Check("ground depths match analytic ray/plane intersections", checkedPixels >= 6 && worstError < 0.02,
    $"{checkedPixels} pixels, worst error {worstError:P2}");

if (args.Contains("--png"))
{
    if (args.Contains("viewer"))
    {
        var outDirV = args.FirstOrDefault(a => a.Contains("out="))?.Split(Char.Parse("="))[1] ?? ".";
        var vopt = new SnapshotImageOptions { FontFamily = "Cascadia Mono", CellWidth = 9, CellHeight = 18 };
        var vm = new ModelScene();
        var vv = new SceneView(vm, new ShadedRenderer()) { Model = vm };
        var vf = new SceneFooter(vv);
        _ = new ControlFrame(vv, borderStyle: BorderStyle.Rounded);
        var vr = new DockPanel(DockedControlPlacement.Bottom, vf, vv);
        vv.Camera.Distance = 16f; vv.Camera.Target = Vector3.Zero;
        vm.SpinRate = 0f;
        void VShot(ISceneRenderer r, string name)
        {
            vv.SetRenderer(r);
            _ = ConsoleSnapshot.ToText(vr, W, H);
            r.Draw(vm.Snapshot, vv.Camera);
            vf.Snapshot = vm.Snapshot;
            ConsoleSnapshot.SavePng(vr, W, H, Path.Combine(outDirV, name + ".png"), vopt);
            Console.WriteLine("  wrote " + name + ".png");
        }
        vm.Step(+1);   // the teapot
        VShot(new ShadedRenderer(), "v1-teapot-shaded");
        VShot(new SolidRenderer(), "v2-teapot-solid");
        VShot(new WireframeRenderer(), "v3-teapot-wire");
        vm.Nudge(0.7f, 0); vm.ScaleAxis(1, 1.3f);
        VShot(new ShadedRenderer(), "v4-teapot-sheared");
        runner.Dispose();
        return 0;
    }

    // Cascadia Mono, NOT the default Consolas: Consolas has no Braille (U+2800-U+28FF), so the wireframe would
    // rasterise as missing-glyph boxes in the image while looking perfect in a terminal.
    var imageOptions = new SnapshotImageOptions { FontFamily = "Cascadia Mono", CellWidth = 9, CellHeight = 18 };
    var outDir = args.FirstOrDefault(a => a.Contains("out="))?.Split('=')[1] ?? ".";

    void Shot(ISceneRenderer r, string name)
    {
        view.SetRenderer(r);
        r.Selected = view.Selected;
        r.Draw(runner.Snapshot, view.Camera);
        _ = ConsoleSnapshot.ToText(root, W, H);
        r.Draw(runner.Snapshot, view.Camera);
        ConsoleSnapshot.SavePng(root, W, H, Path.Combine(outDir, $"{name}.png"), imageOptions);
        Console.WriteLine($"  wrote {name}.png");
    }

    // A scene of loaded meshes -- the whole point being that uniform boxes and spheres flatter every renderer
    // equally, so a fair comparison needs geometry with real curvature and self-occlusion.
    view.Selected = null;
    runner.Post(s =>
    {
        s.ClearBodies();
        s.AddMeshBody(teapotId >= 0 ? teapotId : knotId, new Vector3(-1.6f, 0.9f, 0), 3.2f, 3);
        s.AddMeshBody(knotId, new Vector3(2.2f, 1.0f, 1.2f), 3.0f, 1);
        s.AddSphere(new Vector3(0.4f, 0.7f, -2.4f), 0.7f, 4);
    });
    for (var i = 0; i < 60 && runner.Snapshot.Count < 3; i++) Thread.Sleep(20);
    Settle(150);
    Shot(new WireframeRenderer(), "1-wireframe");
    Shot(new SolidRenderer(), "2-solid");
    Shot(new ShadedRenderer { Edges = SilhouetteStyle.None }, "3-shaded-no-edges");
    Shot(new ShadedRenderer { Edges = SilhouetteStyle.Line }, "4-shaded-line");
    Shot(new ShadedRenderer { Edges = SilhouetteStyle.Glyph }, "5-shaded-glyph");
    Shot(new ShadedRenderer { Edges = SilhouetteStyle.Glyph, WrapLighting = false }, "7-clamped-lambert");
    Shot(new ShadedRenderer { Edges = SilhouetteStyle.Glyph, WrapLighting = true }, "8-half-lambert");

    // Same comparison from the OTHER side, where the visible faces are turned AWAY from the lamp -- which is the
    // only configuration where wrapping can actually buy anything.
    view.Camera.Orbit(MathF.PI, 0);
    Shot(new ShadedRenderer { Edges = SilhouetteStyle.Glyph, WrapLighting = false }, "9-darkside-clamped");
    Shot(new ShadedRenderer { Edges = SilhouetteStyle.Glyph, WrapLighting = true }, "10-darkside-half-lambert");
    runner.Dispose();
    return 0;
}

if (args.Contains("--perf"))
{
    Console.WriteLine($"\nframe cost at {W}x{H}, {solidScene.Count} bodies, orbiting camera (median of 120):");
    // Each renderer must be the one in the layout before it is timed, or its surface has no size and Draw bails.
    void Time(string label, ISceneRenderer r)
    {
        view.SetRenderer(r);
        Probe.PerfProbe.Measure(label, r, view.Camera, solidScene, root, W, H);
    }

    Time("wireframe", new WireframeRenderer());
    Time("solid", new SolidRenderer());
    Time("shaded", new ShadedRenderer { Edges = SilhouetteStyle.None, OcclusionStrength = 0f });
    Time("shaded+ao", new ShadedRenderer { Edges = SilhouetteStyle.None });
    Time("shaded+ao+line", new ShadedRenderer { Edges = SilhouetteStyle.Line });
    Time("shaded+ao+glyph", new ShadedRenderer { Edges = SilhouetteStyle.Glyph });
    Time("  ..clamped", new ShadedRenderer { Edges = SilhouetteStyle.Glyph, WrapLighting = false });
    Time("  ..wrapped", new ShadedRenderer { Edges = SilhouetteStyle.Glyph, WrapLighting = true });

    
    runner.Dispose();
    return 0;
}

// --- silhouettes ----------------------------------------------------------------------------------------------
// The detector's whole claim is that it fires on creases and silhouettes but NOT on flat ground, however steeply
// that ground recedes. Both halves of that are asserted, because a detector that lights up the floor would be
// worse than none.
Console.WriteLine("\nsilhouettes:");
solid.Edges = SilhouetteStyle.Glyph;
solid.Draw(solidScene, view.Camera);

var edgeSurface = (HalfBlockSurface)solid.Surface;
var solidCam = view.Camera.GetView();

// Classify every sub-pixel: is it showing the bare floor? (Compare its depth against the analytic ray/plane
// intersection — anything nearer than the floor is a body in front of it.)
var isGround = new bool[edgeSurface.PixelWidth * edgeSurface.PixelHeight];
for (var sy = 0; sy < edgeSurface.PixelHeight; sy++)
{
    for (var sx = 0; sx < edgeSurface.PixelWidth; sx++)
    {
        var d = edgeSurface.DepthAt(sx, sy);
        if (d <= 0) continue;
        var ndcX = (sx + 0.5f - (edgeSurface.PixelWidth / 2f)) / (edgeSurface.PixelWidth / 2f);
        var ndcY = ((edgeSurface.PixelHeight / 2f) - (sy + 0.5f))
                   / (float)(edgeSurface.PixelHeight / (2.0 * solid.Viewport.CellAspect));
        var dir = Vector3.Normalize((solidCam.Right * (ndcX / solid.Projection.Focal))
                                    + (solidCam.Up * (ndcY / solid.Projection.Focal)) + solidCam.Forward);
        isGround[(sy * edgeSurface.PixelWidth) + sx] =
            Projection.TryPlaneHit(solidCam.Eye, dir, Vector3.Zero, Vector3.UnitY, out var hit)
            && Math.Abs((1f / d) - solidCam.Transform(hit).Z) < 0.25f;
    }
}

int edgesOnBodies = 0, edgesOnOpenGround = 0, openGroundPixels = 0;
bool Ground(int x, int y) => isGround[(y * edgeSurface.PixelWidth) + x];
for (var sy = 1; sy < edgeSurface.PixelHeight - 1; sy++)
{
    for (var sx = 1; sx < edgeSurface.PixelWidth - 1; sx++)
    {
        if (edgeSurface.DepthAt(sx, sy) <= 0) continue;
        if (!Ground(sx, sy))
        {
            if (edgeSurface.EdgeAt(sx, sy)) edgesOnBodies++;
            continue;
        }

        // OPEN ground means the whole neighbourhood the detector looks at is floor. A floor pixel next to a body or
        // to the sky is a real silhouette and is not what this claim is about.
        if (!Ground(sx - 1, sy) || !Ground(sx + 1, sy) || !Ground(sx, sy - 1) || !Ground(sx, sy + 1)) continue;
        openGroundPixels++;
        if (edgeSurface.EdgeAt(sx, sy)) edgesOnOpenGround++;
    }
}

Check("edges are found on the bodies", edgesOnBodies > 20, $"{edgesOnBodies} sub-pixels");
// The claim under test: 1/z is linear across a plane, so a wholly-planar neighbourhood must not bend at all —
// however steeply that plane recedes. This is what a naive neighbour-difference detector cannot achieve.
Check("a wholly-planar neighbourhood never registers as an edge", edgesOnOpenGround == 0,
    $"{edgesOnOpenGround}/{openGroundPixels} open-ground sub-pixels");

if (args.Contains("--solid"))
{
    Probe.SolidProbe.Dump(solidScreen.Buffer, W, H - 3);
    runner.Dispose();
    return 0;
}

view.NextRenderer();
Check("v cycles onward", view.Renderer.Name is "solid" or "wireframe", view.Renderer.Name);

Console.WriteLine($"\n{(failures == 0 ? "ALL PASS" : $"{failures} FAILED")}");
if (args.Contains("--show")) Console.WriteLine(text);
runner.Dispose();
return failures == 0 ? 0 : 1;
