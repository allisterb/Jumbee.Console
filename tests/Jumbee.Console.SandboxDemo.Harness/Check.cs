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
              ?? RepoPaths.At("reference", "projects", "voxcii-main", "models");
    // `uvs` prices the texture-coordinate read: run with and without it on the same directory.
    var withUvs = args.Contains("uvs");
    foreach (var f in Directory.GetFiles(dir).Where(ModelLoader.IsModel).OrderBy(x => x))
    {
        var sw0 = System.Diagnostics.Stopwatch.StartNew();
        var m = ModelLoader.Load(f, withUvs: withUvs);
        Console.WriteLine($"  {Path.GetFileName(f),-14} {m.TriangleCount,7} tris  {m.Vertices.Length,7} verts  " +
                          $"parse {sw0.ElapsedMilliseconds,5} ms  {new FileInfo(f).Length / 1024,6} KB  " +
                          $"extents {m.Extents.X:F3},{m.Extents.Y:F3},{m.Extents.Z:F3}  " +
                          $"up {m.AuthoredUpAxis?.ToString() ?? "-"}  " +
                          $"uvs {(m.Uvs is { } u ? $"{u.Length}{(m.UvIndices is null ? " shared" : "")}" : "-")}");
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

// --- Switching shells without leaving the process ---------------------------------------------------------------
// Real UI.Start/Stop cycles over the real shells. Separate from --shell because it is the only mode that runs the
// UI LOOP; everything else renders through ConsoleSnapshot with no loop at all.
if (args.Contains("--switch")) return Render3d.SwitchChecks.Run(W, H);

// --- Edge smoothing: the same frame at several strengths, so the softening can be judged rather than asserted ----
if (args.Contains("--aa"))
{
    var outDir = args.FirstOrDefault(a => a.Contains("out="))?.Split('=')[1] ?? ".";
    var aaRunner = new PhysicsRunner(s =>
    {
        s.AddStaticBox(new Vector3(0, -0.5f, 0), new Vector3(60, 1, 60));
        s.GroundY = 0f;
        // A big sphere, and it has to be big: the thing being measured is the LEFT silhouette against the sky, so
        // the subject needs many consecutive rows of it. A body a dozen sub-pixels across gives a handful of
        // scattered rows and every placement statistic comes back empty — which is what the first version of this
        // scene did, and it reads as "no effect" rather than as "nothing measured".
        s.AddSphere(new Vector3(0f, 3f, 0f), 3f, 3);
        s.AddBox(new Vector3(4.2f, 0.7f, -0.4f), new Vector3(0.7f), 0);
    });

    var aaShaded = new ShadedRenderer { WrapLighting = true };
    var aaView = new SceneView(aaRunner, aaShaded);
    aaView.Camera.Distance = 11f;   // close enough that the sphere's outline spans most of the frame's height
    _ = new ControlFrame(aaView, borderStyle: BorderStyle.Rounded);
    var aaRoot = new DockPanel(DockedControlPlacement.Bottom, new SceneFooter(aaView), aaView);
    _ = ConsoleSnapshot.ToText(aaRoot, W, H);
    var settleTarget = aaRunner.Snapshot.StepCount + 240;
    while (aaRunner.Snapshot.StepCount < settleTarget) Thread.Sleep(5);

    var opt = new SnapshotImageOptions { FontFamily = "Cascadia Mono", CellWidth = 9, CellHeight = 18 };
    var aaSky = ((HalfBlockSurface)aaShaded.Surface).Background;

    // Draw twice around a layout pass: BeginFrame sizes the sub-pixel buffers from the control's ActualWidth, which
    // is only real once the tree has been laid out at least once.
    ConsoleBuffer AaFrame(string name)
    {
        var snap = aaRunner.Snapshot;
        aaShaded.Draw(snap, aaView.Camera);
        _ = ConsoleSnapshot.ToText(aaRoot, W, H);
        aaShaded.Draw(snap, aaView.Camera);
        var buffer = ConsoleSnapshot.Render(aaRoot, W, H);
        ConsoleSnapshot.SavePng(aaRoot, W, H, Path.Combine(outDir, name + ".png"), opt);
        return buffer;
    }

    // Distinct fg/bg pairs is the honest proxy for what a pass costs the emitter and a capture: a blend
    // re-introduces colours between the quantised plateaus, where a re-partitioned cell cannot.
    static int Pairs(ConsoleBuffer b)
    {
        var pairs = new HashSet<(uint, uint)>();
        for (var y = 0; y < b.Size.Height; y++)
            for (var x = 0; x < b.Size.Width; x++)
            {
                var ch = b[x, y].Character;
                if (ch.Foreground is { } f && ch.Background is { } g)
                    pairs.Add(((uint)(f.Red << 16 | f.Green << 8 | f.Blue), (uint)(g.Red << 16 | g.Green << 8 | g.Blue)));
            }

        return pairs.Count;
    }


    // The antialiasing comparison: the same frame with the boundary placed inside the cell, and without.
    //
    // This mode used to sweep an EdgeSmoothing slider as well, a post-process blending each detected edge sub-pixel
    // toward its neighbours. Both were live at once for a while, since they are independent stages -- one rewrote
    // the sub-pixel buffer, the other decides how a 2x2 block of it becomes a cell. The sweep is gone with the pass:
    // measured together, adding the blend on top of quadrants cost 43% more distinct fg/bg pairs (277 -> 395) and
    // made the placement error slightly WORSE (0.32 -> 0.35), so it earned nothing at any strength.
    Console.WriteLine("\nquadrant sampling — the boundary placed inside the cell:");
    foreach (var on in new[] { false, true })
    {
        aaShaded.QuadrantSampling = on;
        var aaBuffer = AaFrame($"quad-{(on ? "on" : "off")}");
        var insideCell = 0;
        var boundaries = Silhouette(aaBuffer, aaSky, W, H);
        foreach (var b in boundaries) if (b > 0 && (b & 1) == 1) insideCell++;
        Console.WriteLine($"  quadrants {(on ? "on " : "off")}  ->  " +
                          $"{Pairs(aaBuffer),5} distinct fg/bg pairs   " +
                          $"jag {Jaggedness(aaBuffer, aaSky, W, H).Rms:F2} ({Jaggedness(aaBuffer, aaSky, W, H).Rows} rows)   " +
                          $"{insideCell} of {boundaries.Count(b => b > 0)} silhouette rows on a half-cell");
    }

    aaRunner.Dispose();
    return 0;
}

// --- Textures, Phase 0: what does a per-sub-pixel tint cost the emitter? ----------------------------------------
// The question that decides whether image textures are worth building at all, and it is asked BEFORE any decoder,
// parser or asset is paid for -- see docs/internal/agent/3D Textures Plan.md §4. Three procedural sources bracket the
// range (checker cheapest, noise worst; see TextureMode) against the same frame untextured.
//
// Two passes, in this order and not the other. Pass A reads a settled frame through ConsoleSnapshot; pass B runs
// the real compositor, which replaces ConsoleManager's console and orbits the camera, so nothing ConsoleSnapshot
// needs can survive it.
if (args.Contains("--texture"))
{
    var texOut = args.FirstOrDefault(a => a.StartsWith("out="))?[4..];
    // model=PATH swaps the knots for a real file mapped through its OWN UVs -- Phase 1's exit, where the question
    // stops being "what does a texture cost" and becomes "do the seams land where the file says they do".
    var texModel = args.FirstOrDefault(a => a.StartsWith("model="))?[6..];
    var texMesh = texModel is null ? Meshes.TorusKnot() : ModelLoader.Load(texModel, withUvs: true);
    if (!texMesh.HasUvs)
    {
        Console.WriteLine($"{texModel} carries no texture coordinates, so there is nothing to map a texture through.");
        return 1;
    }

    var texTag = texModel is null ? "knot" : Path.GetFileNameWithoutExtension(texModel).ToLowerInvariant();

    // image=PATH attaches a real map (Phase 2). reduce= and levels= are the bake's two dials, so they can be swept
    // from the command line rather than by editing constants.
    var texImage = args.FirstOrDefault(a => a.StartsWith("image="))?[6..];
    var texReduce = int.TryParse(args.FirstOrDefault(a => a.StartsWith("reduce="))?[7..], out var rs) ? rs : Texture.DefaultSize;
    var texLevels = int.TryParse(args.FirstOrDefault(a => a.StartsWith("levels="))?[7..], out var ls) ? ls : Texture.DefaultLevels;
    if (texImage is not null)
    {
        var map = Texture.Load(texImage, texReduce, texLevels);
        // One material over every triangle, so the map applies whatever materials the model came with (or none, for
        // the knot). This is the explicit-override path; without image= a model's own .mtl is resolved by ModelLoader.
        texMesh = new Mesh(texMesh.Vertices, texMesh.Indices)
        {
            AuthoredUpAxis = texMesh.AuthoredUpAxis,
            Uvs = texMesh.Uvs,
            UvIndices = texMesh.UvIndices,
            MaterialNames = ["image"],
            MaterialIds = new int[texMesh.TriangleCount],
        }.WithMaterials([new Material("image", map.Average, map)]);
        texTag += $"-r{texReduce}-l{texLevels}";
        Console.WriteLine($"\n{Path.GetFileName(texImage)}: {map.SourceWidth}x{map.SourceHeight} -> {map.Width}x{map.Height}, " +
                          $"{texLevels} levels  busyness {map.Busyness:F3}  retain {map.Retain:F2}  " +
                          $"contrast {map.Contrast:F3}  {map.Colours} colours  gate: {(map.Legible ? "TEXTURE" : "flat")}");
    }

    var texMeshId = Meshes.Register(texMesh, texTag);
    var texRunner = new PhysicsRunner(s =>
    {
        s.AddStaticBox(new Vector3(0, -0.5f, 0), new Vector3(60, 1, 60));
        s.GroundY = 0f;
        if (texModel is not null)
        {
            // One model, large and central: this run is for looking at, and a seam needs sub-pixels to show in.
            s.AddMeshBody(texMeshId, new Vector3(0f, 2.5f, 0f), 5f, 3);
            return;
        }

        // Knots rather than spheres: they are the only generated mesh carrying UVs, and they are also what a real
        // textured model looks like to the rasteriser -- many small triangles at every angle to the camera. Two of
        // them, close in, so the textured surface is a large share of the frame rather than a detail in it.
        s.AddMeshBody(texMeshId, new Vector3(-1.9f, 1.7f, 0f), 3.2f, 3);
        s.AddMeshBody(texMeshId, new Vector3(2.1f, 1.5f, 1.1f), 2.8f, 1);
    });

    var texView = new SceneView(texRunner, new ShadedRenderer());
    texView.Camera.Distance = 9f;
    _ = new ControlFrame(texView, borderStyle: BorderStyle.Rounded);
    var texRoot = new DockPanel(DockedControlPlacement.Bottom, new SceneFooter(texView), texView);
    _ = ConsoleSnapshot.ToText(texRoot, W, H);
    var texSettle = texRunner.Snapshot.StepCount + 240;
    while (texRunner.Snapshot.StepCount < texSettle) Thread.Sleep(5);
    // One settled snapshot for every configuration below. The knots must have stopped moving or each run measures
    // a different scene while looking like a comparison -- the lesson the first bandwidth run in wolf3d taught.
    var texScene = texRunner.Snapshot;

    // Distinct fg/bg pairs: the honest proxy for what the emitter will have to spend, since a pair that differs
    // from its neighbour's is a run boundary. Same measure --aa uses, so the two modes' numbers are comparable.
    static int TexPairs(ConsoleBuffer b)
    {
        var pairs = new HashSet<(uint, uint)>();
        for (var y = 0; y < b.Size.Height; y++)
            for (var x = 0; x < b.Size.Width; x++)
            {
                var ch = b[x, y].Character;
                if (ch.Foreground is { } f && ch.Background is { } g)
                    pairs.Add(((uint)((f.Red << 16) | (f.Green << 8) | f.Blue),
                               (uint)((g.Red << 16) | (g.Green << 8) | g.Blue)));
            }

        return pairs.Count;
    }

    var texOpt = new SnapshotImageOptions { FontFamily = "Cascadia Mono", CellWidth = 9, CellHeight = 18 };
    // `imageonly` skips the procedural sources, for sweeping the bake's dials without re-pricing all three each time.
    TextureMode[] texModes = args.Contains("imageonly") ? [] : [TextureMode.Checker, TextureMode.Gradient, TextureMode.Noise];
    var texScales = new[] { 4f, 8f, 16f, 32f };

    // --- Pass A: colour cost, and a picture to judge it by -------------------------------------------------------
    foreach (var texRenderer in new MeshRenderer[] { new SolidRenderer(), new ShadedRenderer() })
    {
        texView.SetRenderer(texRenderer);
        _ = ConsoleSnapshot.ToText(texRoot, W, H);

        ConsoleBuffer TexFrame(string? name)
        {
            // Draw twice around a layout pass, as --aa does: BeginFrame sizes the sub-pixel buffers from the
            // control's ActualWidth, which is only real once the tree has been laid out at least once.
            texRenderer.Draw(texScene, texView.Camera);
            _ = ConsoleSnapshot.ToText(texRoot, W, H);
            texRenderer.Draw(texScene, texView.Camera);
            var buffer = ConsoleSnapshot.Render(texRoot, W, H);
            if (name is not null && texOut is not null)
                ConsoleSnapshot.SavePng(texRoot, W, H, Path.Combine(texOut, name + ".png"), texOpt);
            return buffer;
        }

        Console.WriteLine($"\n{texRenderer.Name} at {W}x{H} — distinct fg/bg pairs in one settled frame:");
        texRenderer.Texture = TextureMode.None;
        var texBase = TexPairs(TexFrame($"tex-{texTag}-{texRenderer.Name}-none"));
        Console.WriteLine($"  {"none",-9} {"",-6}  {texBase,6} pairs   (baseline)");
        if (texImage is not null)
        {
            texRenderer.Texture = TextureMode.Image;
            var imagePairs = TexPairs(TexFrame($"tex-{texTag}-{texRenderer.Name}-image"));
            Console.WriteLine($"  {"image",-9} {"",-6}  {imagePairs,6} pairs   {(double)imagePairs / texBase,5:F2}x baseline");
        }
        foreach (var mode in texModes)
        {
            texRenderer.Texture = mode;
            foreach (var scale in texScales)
            {
                texRenderer.TextureScale = scale;
                // A PNG at the default scale only. The number says what it costs; the picture is the only thing
                // that says whether it bought anything, and eight of them is enough to look at.
                var name = scale == 8f ? $"tex-{texTag}-{texRenderer.Name}-{mode}".ToLowerInvariant() : null;
                var pairs = TexPairs(TexFrame(name));
                Console.WriteLine($"  {mode,-9} x{scale,-5:F0}  {pairs,6} pairs   " +
                                  $"{(double)pairs / texBase,5:F2}x baseline");
            }
        }
    }

    // --- Pass B: the real emitter --------------------------------------------------------------------------------
    // Pairs predict the cost; ANSI bytes per frame ARE the cost, and the exit criterion is stated in them.
    Console.WriteLine($"\nframe cost over the real compositor at {W}x{H} (median of 120):");

    // PerfProbe orbits the camera one notch per frame and never puts it back, so without this each configuration
    // would be measured from a different angle -- a fair-looking comparison of unequal scenes. Coupled to
    // PerfProbe's own Warmup + N; if those change, so must this.
    const float PerfOrbit = (20 + 120) * 0.01f;
    void TexTime(string label, MeshRenderer r, TextureMode mode, float scale)
    {
        r.Texture = mode;
        r.TextureScale = scale;
        texView.SetRenderer(r);
        Probe.PerfProbe.Measure(label, r, texView.Camera, texScene, texRoot, W, H);
        texView.Camera.Orbit(-PerfOrbit, 0);
    }

    foreach (var r in new MeshRenderer[] { new SolidRenderer(), new ShadedRenderer() })
    {
        TexTime(r.Name, r, TextureMode.None, 8f);
        if (texImage is not null) TexTime("  ..image", r, TextureMode.Image, 8f);
        foreach (var mode in texModes) TexTime($"  ..{mode}".ToLowerInvariant(), r, mode, 8f);
    }

    texRunner.Dispose();
    return 0;
}

// The left silhouette against the sky, in HALF-CELL units, one entry per half-row: the leftmost half-cell that is
// not the background colour, PROVIDED some sky was crossed to reach it. 0 otherwise.
//
// That proviso is the definition of a silhouette and not a convenience. Without it every row the ground fills edge
// to edge reports a boundary in the first content column — always an even one, since there is nothing to place
// inside a cell — and in a scene of bodies standing on a floor those rows outnumber the real silhouettes six to
// one, diluting the very measurement they are not part of.
//
// Decoded through the quadrant mask, so ONE decoder reads both compositors -- ▀ is quadrant pattern 0011, and a
// surface not sampling quadrants simply never emits any of the other thirteen. An ODD result is therefore a
// boundary the ▀ compositor could not have expressed: it sits inside a cell.
static int[] Silhouette(ConsoleBuffer buffer, ConsoleGUI.Data.Color sky, int w, int h)
{
    const string patterns = " ▘▝▀▖▌▞▛▗▚▐▜▄▙▟█";
    var found = new int[(h - 3) * 2];
    for (var row = 0; row < h - 3; row++)
    {
        for (var halfRow = 0; halfRow < 2; halfRow++)
        {
            var at = 0;
            var sawSky = false;
            var done = false;
            for (var x = 0; x < w && !done; x++)
            {
                var ch = buffer[x, row].Character;
                if (ch.Content is not { } glyph) continue;
                var mask = patterns.IndexOf(glyph);
                if (mask < 0 || ch.Foreground is not { } fg || ch.Background is not { } bg) continue;
                // Left quadrant of this half-row, then the right one. Whichever is first to differ from the sky is
                // where the silhouette crosses.
                for (var side = 0; side < 2; side++)
                {
                    var lit = (mask & (1 << ((halfRow * 2) + side))) != 0 ? fg : bg;
                    if (lit == sky) { sawSky = true; continue; }
                    // The first thing that is not sky ends the scan either way: with sky behind it that is the
                    // silhouette, without it the row was never one.
                    if (sawSky) at = (x * 2) + side;
                    done = true;
                    break;
                }
            }

            found[(row * 2) + halfRow] = at;
        }
    }

    return found;
}

// The silhouette's QUANTISATION ERROR: how far each row's boundary sits from where its two neighbours say it should
// be, in half-cells, RMS. A body's outline is locally straight over three rows, so the midpoint of the neighbours is
// a fair stand-in for the truth without needing the analytic curve.
//
// This is the number the whole stage is about. A staircase can only place the boundary on whole cells, so a row that
// should sit half a cell along is a whole half-cell out and the residual is ~1; halving the placement grid should
// halve it. Note what it is NOT: the total zigzag (Σ|second difference|) is nearly INVARIANT here — an antialiased
// edge bends twice as often by half as much — so the obvious "how much does it wobble" measures cannot see this
// change at all, and the first version of this function, a median bend, reported 0.00 for every case.
static (double Rms, int Rows) Jaggedness(ConsoleBuffer buffer, ConsoleGUI.Data.Color sky, int w, int h)
{
    var edge = Silhouette(buffer, sky, w, h);
    double sum = 0;
    var n = 0;
    for (var i = 1; i < edge.Length - 1; i++)
    {
        if (edge[i - 1] <= 0 || edge[i] <= 0 || edge[i + 1] <= 0) continue;
        // Skip a step no locally-straight edge could make: at the top and bottom of a body the leftmost object
        // changes from one row to the next, and that is a jump rather than a placement error.
        if (Math.Abs(edge[i + 1] - edge[i - 1]) > 4) continue;
        var residual = edge[i] - ((edge[i - 1] + edge[i + 1]) / 2.0);
        sum += residual * residual;
        n++;
    }

    return (n == 0 ? 0 : Math.Sqrt(sum / n), n);
}


void Check(string what, bool ok, string? detail = null)
{
    Console.WriteLine($"  {(ok ? "PASS" : "FAIL")}  {what}{(detail is null ? "" : $"  [{detail}]")}");
    if (!ok) failures++;
}

// --- meshes -------------------------------------------------------------------------------------------------------
// Registered before anything else: the spawn path and the renderers all key off the registry.
var knotId = Meshes.Register(Meshes.TorusKnot(), "knot");
var teapotPath = RepoPaths.At("reference", "projects", "voxcii-main", "models", "teapot.obj");
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

// The drag plane faces the camera, so with the camera above the scene, dragging down the screen aims BELOW the
// floor. A kinematic body is not stopped by static geometry, so without a clamp it sinks through and the release
// either loses it or has the solver eject it. Aim well under the floor and expect it to sit ON the floor instead.
var floorTop = 0.5f;   // half-height of the unit boxes above, so a box resting on y = 0 has its centre here
runner.Post(s => s.BeginGrab(grabId));
Settle(2);
for (var i = 0; i < 20; i++)
{
    runner.Post(s => s.DragTo(grabId, new Vector3(from.X, -12f, from.Z)));
    Settle(2);
}

var sunk = runner.Snapshot;
var sunkIndex = sunk.IndexOf(grabId);
Check("dragging a held body below the floor keeps it on the floor", sunkIndex >= 0 && sunk.Positions[sunkIndex].Y > floorTop - 0.05f,
    sunkIndex < 0 ? "body was lost entirely" : $"y={sunk.Positions[sunkIndex].Y:F2}, floor is y={floorTop:F2}");

runner.Post(s => s.ReleaseGrab(Vector3.Zero));
var released = Settle(90);
var releasedIndex = released.IndexOf(grabId);
Check("and letting go there leaves it on the floor rather than under it",
    releasedIndex >= 0 && released.Positions[releasedIndex].Y > floorTop - 0.05f,
    releasedIndex < 0 ? "body was lost entirely" : $"y={released.Positions[releasedIndex].Y:F2}");

// Off the edge of the slab and gone: still solved on every step forever unless something drops it. Two bodies, one
// either side of the kill plane, so the check cannot pass by the posted command never having run at all.
runner.Post(s => s.AddBox(new Vector3(0, -80f, 0), new Vector3(0.5f), 0));
runner.Post(s => s.AddBox(new Vector3(0, 6f, 0), new Vector3(0.5f), 0));
var culled = Settle(4);
Check("a body that has fallen out of the world is destroyed, and only that one",
    culled.Count == released.Count + 1, $"{released.Count} + 2 spawned -> {culled.Count}");

// --- throw --------------------------------------------------------------------------------------------------
// Through the REAL gesture, not PhysicsScene.ReleaseGrab: a synthetic drag reports every move in the same
// microsecond, which is exactly the case that used to imply an absurd speed and fling the body off the screen.
Console.WriteLine("\nthrow:");
var settled = Settle(120);
var throwProbe = settled.Ids[0];
var throwIndex = settled.IndexOf(throwProbe);
cameraView = view.Camera.GetView();
renderer.Projection.TryProject(cameraView.Transform(settled.Positions[throwIndex]), out px, out py);
vp = renderer.Viewport;
var tCol = (int)Math.Round((px + 1f) / 2f * (vp.Width - 1));
var tRow = (int)Math.Round((vp.CellAspect - py) / (2 * vp.CellAspect) * (vp.Height - 1));
var buffer = ConsoleSnapshot.Render(root, W, H);
var flicked = ConsoleSnapshot.Drag(buffer, tCol + 1, tRow + 1, tCol + 25, tRow - 8, steps: 6);
var afterThrow = Settle(2);
var throwSpeed = afterThrow.IndexOf(throwProbe) is var ti && ti >= 0 ? afterThrow.Velocities[ti].Length() : -1f;
Check("a flick throws at a hand speed, not a muzzle speed", flicked && throwSpeed >= 0 && throwSpeed <= 15.5f,
    $"drag reached the viewport: {flicked}, speed={throwSpeed:F1} (cap 15)");

// Let the throw land and let anything it knocked off the table finish falling and be culled, so the counts the
// sections below read are not racing a body still on its way out of the world.
Settle(300);

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
    var planePath = RepoPaths.At("media", "models", "plane.obj");
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

// --- texture coordinates ----------------------------------------------------------------------------------------
// Asserted on RESOLVED CORNERS -- the (position, UV) pair each triangle corner ends up with -- not on the index
// arrays. The pairing is the contract and the arrays are one representation of it (a 1:1 file stores no UV index
// buffer at all), so a check on the arrays would pass for a loader that stored them faithfully and paired them wrong.
Console.WriteLine("\ntexture coordinates:");
static (int V, Vector2 Uv)[] Corners(Mesh m) =>
    m.Uvs is not { } uvs ? [] : [.. m.Indices.Select((v, k) => (v, uvs[(m.UvIndices ?? m.Indices)[k]]))];

string[] tri = ["v 0 0 0", "v 1 0 0", "v 0 1 0", "vt 0 0", "vt 1 0", "vt 0 1"];
Vector2 uv00 = new(0, 0), uv10 = new(1, 0), uv01 = new(0, 1), uv11 = new(1, 1);

// A quad whose vt order is deliberately the REVERSE of its v order, so no pairing can come out right by coinciding
// with v == vt. Both fan triangles must pair every corner with the coordinate the file declared for it.
var quadUv = ObjLoader.Parse(["v -1 0 -1", "v 1 0 -1", "v 1 0 1", "v -1 0 1",
                              "vt 0 0", "vt 1 0", "vt 1 1", "vt 0 1", "f 1/4 2/3 3/2 4/1"], withUvs: true);
var quadWant = new Dictionary<int, Vector2> { [0] = uv01, [1] = uv11, [2] = uv10, [3] = uv00 };
var quadCorners = Corners(quadUv);
Check("a quad's UVs take the same fan as its positions, each on the right corner",
    quadUv.TriangleCount == 2 && quadCorners.Length == 6 && quadCorners.All(c => quadWant[c.V] == c.Uv),
    $"{quadCorners.Length} corners, {quadCorners.Count(c => quadWant.TryGetValue(c.V, out var w) && w != c.Uv)} mispaired");

// The whole reason UvIndices exists: one position, two coordinates. Vertex 2 is (0.5,0) in the first triangle and
// (1,0) in the second -- a seam.
var seam = ObjLoader.Parse(["v 0 0 0", "v 1 0 0", "v 1 1 0", "v 0 1 0",
                            "vt 0 0", "vt 0.5 0", "vt 0.5 1", "vt 1 0", "vt 1 1",
                            "f 1/1 2/2 3/3", "f 2/4 4/5 3/3"], withUvs: true);
var seamUvs = Corners(seam).Where(c => c.V == 1).Select(c => c.Uv).Distinct().Count();
Check("a seam vertex keeps both of its UVs", seam.UvIndices is not null && seamUvs == 2,
    $"{seamUvs} distinct UVs on the shared vertex");

var shared = ObjLoader.Parse([.. tri, "f 1/1 2/2 3/3"], withUvs: true);
Check("a file whose vt indices equal its v indices stores no second index buffer",
    shared.HasUvs && shared.UvIndices is null, $"UvIndices {(shared.UvIndices is null ? "null" : "allocated")}");

var negativeUv = ObjLoader.Parse([.. tri, "f -3/-1 -2/-2 -1/-3"], withUvs: true);
var negativeWant = new[] { (0, uv01), (1, uv10), (2, uv00) };
Check("negative vt indices count back from the latest vt", Corners(negativeUv).SequenceEqual(negativeWant),
    string.Join(" ", Corners(negativeUv).Select(c => $"v{c.V}={c.Uv}")));

// UV indices must be dropped IN LOCK-STEP with the triangles the cleaning pass throws away. The first face names a
// vertex that does not exist and goes; if the UV list were not filtered with it, the survivor would inherit the
// dead face's coordinates.
var dropped = ObjLoader.Parse([.. tri, "f 1/1 2/2 9/3", "f 1/3 2/2 3/1"], withUvs: true);
var droppedWant = new[] { (0, uv01), (1, uv10), (2, uv00) };
Check("a dropped face takes its UVs with it, leaving the survivor's intact",
    dropped.TriangleCount == 1 && Corners(dropped).SequenceEqual(droppedWant),
    string.Join(" ", Corners(dropped).Select(c => $"v{c.V}={c.Uv}")));

// All or nothing. A bare corner or a vt index past the end costs the mesh its UVs -- not a throw, and not a guessed
// coordinate on the faces that happened to be complete.
var bareCorner = ObjLoader.Parse([.. tri, "f 1/1 2//2 3/3"], withUvs: true);
Check("a face with one bare corner drops UVs for the whole mesh", !bareCorner.HasUvs && bareCorner.TriangleCount == 1,
    $"HasUvs={bareCorner.HasUvs}");
var pastEnd = ObjLoader.Parse([.. tri, "f 1/1 2/2 3/9"], withUvs: true);
Check("a vt index past the end drops UVs rather than throwing", !pastEnd.HasUvs && pastEnd.TriangleCount == 1,
    $"HasUvs={pastEnd.HasUvs}");
var noVt = ObjLoader.Parse(["v 0 0 0", "v 1 0 0", "v 0 1 0", "f 1/1 2//2 3/3/3"], withUvs: true);
Check("asking for UVs from a file with no vt at all yields none", !noVt.HasUvs, $"HasUvs={noVt.HasUvs}");

// The opt-in is where the parse-time saving comes from, so it is asserted rather than assumed: a complete, valid
// UV'd file read WITHOUT asking must come back with no UVs, or every untextured load is paying for them.
var notAsked = ObjLoader.Parse([.. tri, "f 1/1 2/2 3/3"]);
Check("UVs are not read unless asked for", !notAsked.HasUvs && shared.HasUvs,
    $"not asked: HasUvs={notAsked.HasUvs}; asked: HasUvs={shared.HasUvs}");

// The real files. media/ is gitignored, so these are optional rather than failures in a fresh checkout.
if (RepoPaths.Optional("media", "models", "capsule.obj") is { } capsulePath)
{
    var capsule = ObjLoader.Load(capsulePath, withUvs: true);
    Check("capsule.obj loads all 5,252 of its UVs", capsule.Uvs?.Length == 5252,
        $"{capsule.Uvs?.Length} UVs, index buffer {(capsule.UvIndices is null ? "shared with positions" : "separate")}");
}
else Console.WriteLine("  skip  capsule.obj — media/models not present");

if (RepoPaths.Optional("media", "models", "plane.obj") is { } planeUvPath)
{
    var planeUv = ObjLoader.Load(planeUvPath, withUvs: true);
    // 43,171 vt against 40,654 v: these cannot share an index buffer, so a null here would be a wrong mesh.
    Check("plane.obj keeps a separate UV index buffer", planeUv.HasUvs && planeUv.UvIndices is not null,
        $"{planeUv.Uvs?.Length} UVs over {planeUv.Vertices.Length} vertices");
}
else Console.WriteLine("  skip  plane.obj — media/models not present");

// --- texture maps ------------------------------------------------------------------------------------------------
// Three claims Phase 2 rests on: a malformed PNG can never hang the decoder, Sample reads the map the right way up,
// and the bake reduces, quantises and measures what it says. The pngSubject PNG is ENCODED here, so none of this needs
// an asset that a fresh checkout might lack.
Console.WriteLine("\ntexture maps:");

// Every decode runs under a watchdog: a hang is the failure being tested for, and it cannot be caught. On its OWN
// background thread, not the pool -- with the guard mutated away, 17 decodes hung on pool threads and starved every
// watchdog after them, so "not a PNG at all" reported HANG. A watchdog that misreports WHICH input hangs is worse
// than none, because telling the inputs apart is its whole job.
static string Decoded(byte[] bytes)
{
    string? result = null;
    var worker = new Thread(() =>
    {
        try { var t = Texture.Decode(bytes); result = $"decoded {t.Width}x{t.Height}"; }
        catch (InvalidDataException e) { result = "refused: " + e.Message; }
        catch (Exception e) { result = $"WRONG TYPE {e.GetType().Name}: {e.Message}"; }
    }) { IsBackground = true };
    worker.Start();
    return worker.Join(5000) ? result! : "HANG";
}

static uint Crc32(ReadOnlySpan<byte> data)
{
    var crc = 0xFFFFFFFFu;
    foreach (var b in data)
    {
        crc ^= b;
        for (var k = 0; k < 8; k++) crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xEDB88320u : crc >> 1;
    }

    return ~crc;
}

// Rebuilds a PNG chunk by chunk, editing the named one's payload and recomputing every length and CRC -- so the
// result is structurally VALID and only its content is wrong. That is what gets past a structure check, and so it
// is the case that tests whether the check is sufficient.
static byte[] EditChunk(byte[] png, string type, Func<byte[], byte[]> edit)
{
    var output = new List<byte>(png[..8]);
    for (var pos = 8; pos + 12 <= png.Length;)
    {
        var length = (int)System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(png.AsSpan(pos));
        var name = System.Text.Encoding.ASCII.GetString(png, pos + 4, 4);
        var data = png[(pos + 8)..(pos + 8 + length)];
        if (name == type) data = edit(data);
        var chunk = new byte[12 + data.Length];
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(chunk, (uint)data.Length);
        System.Text.Encoding.ASCII.GetBytes(name, chunk.AsSpan(4));
        data.CopyTo(chunk, 8);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(chunk.AsSpan(8 + data.Length),
                                                                    Crc32(chunk.AsSpan(4, 4 + data.Length)));
        output.AddRange(chunk);
        pos += 12 + length;
    }

    return [.. output];
}

var pngSubjectRgb = new byte[96 * 64 * 3];
for (var i = 0; i < pngSubjectRgb.Length; i++) pngSubjectRgb[i] = (byte)(i * 7 % 251);
var pngSubject = PngSharp.Api.Png.EncodeToByteArray(PngSharp.Api.Png.CreateRgb(96, 64, pngSubjectRgb));
Check("an intact PNG decodes", Decoded(pngSubject).StartsWith("decoded"), Decoded(pngSubject));

// The defect that made this guard mandatory: PngSharp spins forever on a file cut mid-chunk. Every cut must come
// back as a refusal, and none as a hang.
var cuts = Enumerable.Range(1, 19).Select(p => Decoded(pngSubject[..(pngSubject.Length * p * 5 / 100)])).ToArray();
Check("every truncation from 5% to 95% is refused, and none hangs",
    cuts.All(r => r.StartsWith("refused")),
    $"{cuts.Count(r => r.StartsWith("refused"))}/19 refused, {cuts.Count(r => r == "HANG")} hung");

// The case the structure check cannot see: every chunk intact and correctly CRC'd, but the compressed image inside
// IDAT cut in half. The claim is that the DECODER's image path is safe on its own (its filter pass uses
// ReadExactly), so the guard only has to cover chunks. This is where that claim is tested rather than trusted.
var shortIdat = EditChunk(pngSubject, "IDAT", d => d[..(d.Length / 2)]);
Check("a structurally valid PNG with a short image stream fails cleanly, and does not hang",
    Decoded(shortIdat).StartsWith("refused"), Decoded(shortIdat));

// A header claiming an absurd size, with a VALID CRC, so PngSharp's own check does not catch it first. It must be
// refused before anything is allocated for it -- the alternative is an out-of-memory crash inside the decoder.
var absurd = EditChunk(pngSubject, "IHDR", d =>
{
    var copy = (byte[])d.Clone();
    System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(copy, 100_000);
    System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(copy.AsSpan(4), 100_000);
    return copy;
});
Check("an absurd size in a valid IHDR is refused before anything is allocated",
    Decoded(absurd).Contains("pixel limit"), Decoded(absurd));
Check("not a PNG at all is refused", Decoded("this is not a png"u8.ToArray()).StartsWith("refused"),
    Decoded("this is not a png"u8.ToArray()));

// Orientation. Built unreduced and unquantised so each texel is exactly what was put in. OBJ puts v = 0 at the
// BOTTOM of the image; a decoded image's first row is the TOP. Get this wrong and every real map is upside down --
// invisible with a procedural source, which is why it is pinned here rather than judged from a render.
Color red = new(255, 0, 0), blue = new(0, 0, 255);
static string Rgb(Color c) => $"{c.R},{c.G},{c.B}";
var tall = Texture.Bake([255, 0, 0, 0, 0, 255], 1, 2, levels: 0);   // top row red, bottom row blue
Check("v is flipped: a high v reads the TOP of the image", tall.Sample(new(0.5f, 0.9f)) == red && tall.Sample(new(0.5f, 0.1f)) == blue,
    $"v=0.9 -> {Rgb(tall.Sample(new(0.5f, 0.9f)))}, v=0.1 -> {Rgb(tall.Sample(new(0.5f, 0.1f)))}");
var wide = Texture.Bake([255, 0, 0, 0, 0, 255], 2, 1, levels: 0);   // left red, right blue
Check("u runs left to right", wide.Sample(new(0.1f, 0.5f)) == red && wide.Sample(new(0.9f, 0.5f)) == blue,
    $"u=0.1 -> {Rgb(wide.Sample(new(0.1f, 0.5f)))}, u=0.9 -> {Rgb(wide.Sample(new(0.9f, 0.5f)))}");
// An atlas puts edge texels at exactly 1.0. Wrapping there would fetch the OPPOSITE edge.
Check("u = 1.0 clamps to the right edge instead of wrapping to the left",
    wide.Sample(new(1f, 0.5f)) == blue && wide.Sample(new(0f, 0.5f)) == red, $"u=1.0 -> {Rgb(wide.Sample(new(1f, 0.5f)))}");
Check("outside [0,1] repeats", wide.Sample(new(1.25f, 0.5f)) == red && wide.Sample(new(-0.25f, 0.5f)) == blue,
    $"u=1.25 -> {Rgb(wide.Sample(new(1.25f, 0.5f)))}, u=-0.25 -> {Rgb(wide.Sample(new(-0.25f, 0.5f)))}");

// The bake. A smooth ramp is the case that needs quantising most -- every value distinct going in.
var ramp = new byte[256 * 4 * 3];
for (var x = 0; x < 256; x++)
    for (var y = 0; y < 4; y++) { var i = ((y * 256) + x) * 3; ramp[i] = ramp[i + 1] = ramp[i + 2] = (byte)x; }
var rampBaked = Texture.Bake(ramp, 256, 4, size: 64, levels: 10);
Check("the bake reduces to the requested longest side, keeping the aspect",
    rampBaked.Width == 64 && rampBaked.Height == 1, $"{rampBaked.Width}x{rampBaked.Height} from 256x4");
Check("the bake quantises: a 256-value ramp leaves at most 10 colours", rampBaked.Colours <= 10,
    $"{rampBaked.Colours} colours");

// Retain measures frequency against the BAKED resolution. A one-texel checker averages to flat grey (nothing
// survives); two big halves survive the same reduce untouched. Same content type, opposite verdicts.
byte[] Pattern(int w, int h, Func<int, int, bool> on)
{
    var rgb = new byte[w * h * 3];
    for (var y = 0; y < h; y++)
        for (var x = 0; x < w; x++) { var v = on(x, y) ? (byte)255 : (byte)0; var i = ((y * w) + x) * 3; rgb[i] = rgb[i + 1] = rgb[i + 2] = v; }
    return rgb;
}
var fine = Texture.Bake(Pattern(256, 256, (x, y) => ((x + y) & 1) == 0), 256, 256, size: 16);
var halves = Texture.Bake(Pattern(256, 256, (x, _) => x < 128), 256, 256, size: 16);
Check("detail finer than a baked texel scores near 0 retained", fine.Retain < 0.05f && fine.Contrast < 0.05f,
    $"retain {fine.Retain:F3}, contrast {fine.Contrast:F3}");
Check("detail coarser than a baked texel scores near 1 retained", halves.Retain > 0.95f && halves.Contrast > 0.4f,
    $"retain {halves.Retain:F3}, contrast {halves.Contrast:F3}");

// WithMaterials must carry every OTHER init property, or a material-bearing mesh silently loses one. Checked by
// reflection over the setters Mesh actually has, against a source with every one of them set -- so a property added
// later is caught here without anyone remembering to extend the check.
var carried = new Mesh([new(0, 0, 0), new(1, 0, 0), new(0, 1, 0)], [0, 1, 2])
{
    AuthoredUpAxis = ModelUpAxis.Z,
    FaceColors = [red],
    Uvs = [new(0, 0), new(1, 0), new(0, 1)],
    UvIndices = [2, 1, 0],
    MaterialNames = ["m"],
    MaterialIds = [0],
    Materials = [new Material("m", red, wide)],
};
Material[] replacement = [new Material("m", blue, tall)];
var withMap = carried.WithMaterials(replacement);
// Every property is compared, not only the settable ones, so a copy that forgot to pass Vertices or Indices to the
// constructor fails too. The settable ones must also all be SET on the source, or a dropped null passes vacuously.
var meshProps = typeof(Mesh).GetProperties();
var unset = meshProps.Where(p => p.SetMethod is not null && p.GetValue(carried) is null).Select(p => p.Name).ToArray();
var lost = meshProps.Where(p => p.Name != nameof(Mesh.Materials) && !Equals(p.GetValue(carried), p.GetValue(withMap)))
                    .Select(p => p.Name).ToArray();
Check("WithMaterials carries every other property",
    unset.Length == 0 && lost.Length == 0 && ReferenceEquals(withMap.Materials, replacement),
    unset.Length > 0 ? $"test source leaves unset: {string.Join(", ", unset)}"
                     : lost.Length > 0 ? $"dropped: {string.Join(", ", lost)}" : $"{meshProps.Length - 1} carried");

// And it refuses materials that do not parallel the names -- a silent misalignment would put every face on the
// wrong material.
Check("WithMaterials refuses a list that does not parallel the names",
    Throws<ArgumentException>(() => carried.WithMaterials([replacement[0], replacement[0]])));

static bool Throws<T>(Action action) where T : Exception
{
    try { action(); return false; }
    catch (T) { return true; }
}

if (RepoPaths.Optional("media", "models", "capsule0.png") is { } capsuleMap)
{
    var map = Texture.Load(capsuleMap);
    Check($"capsule0.png bakes from 2048x1024 to the default {Texture.DefaultSize}, keeping its 2:1 aspect",
        map.Width == Texture.DefaultSize && map.Height == Texture.DefaultSize / 2 && map.SourceWidth == 2048,
        $"{map.SourceWidth}x{map.SourceHeight} -> {map.Width}x{map.Height}, retain {map.Retain:F2}, " +
        $"contrast {map.Contrast:F3}, {map.Colours} colours");
}
else Console.WriteLine("  skip  capsule0.png — media/models not present (convert capsule0.jpg once, see the plan)");

// --- JPEG ---------------------------------------------------------------------------------------------------------
// Encoded here with libjpeg.net's own compressor, so nothing depends on media/. The decoder is only safe behind
// Texture's strict error manager: by default a truncated file decodes SILENTLY and an early cut throws
// IndexOutOfRange from inside the library. These are the checks that fail if that manager is ever removed.
static byte[] EncodeJpeg(byte[] pixels, int w, int h, int components)
{
    var encoder = new BitMiracle.LibJpeg.Classic.jpeg_compress_struct(new BitMiracle.LibJpeg.Classic.jpeg_error_mgr());
    using var stream = new MemoryStream();
    encoder.jpeg_stdio_dest(stream);
    encoder.Image_width = w;
    encoder.Image_height = h;
    encoder.Input_components = components;
    encoder.In_color_space = components == 1 ? BitMiracle.LibJpeg.Classic.J_COLOR_SPACE.JCS_GRAYSCALE
                                             : BitMiracle.LibJpeg.Classic.J_COLOR_SPACE.JCS_RGB;
    encoder.jpeg_set_defaults();
    encoder.jpeg_set_quality(95, true);
    encoder.jpeg_start_compress(true);
    var row = new byte[1][];
    for (var y = 0; y < h; y++)
    {
        row[0] = pixels[(y * w * components)..((y + 1) * w * components)];
        encoder.jpeg_write_scanlines(row, 1);
    }

    encoder.jpeg_finish_compress();
    return stream.ToArray();
}

var jpegSubject = EncodeJpeg(pngSubjectRgb, 96, 64, 3);
Check("a JPEG is recognised by its signature and decodes", Decoded(jpegSubject) == "decoded 96x64", Decoded(jpegSubject));

var jpegCuts = Enumerable.Range(1, 19).Select(p => Decoded(jpegSubject[..(jpegSubject.Length * p * 5 / 100)])).ToArray();
Check("every JPEG truncation from 5% to 95% is refused — none decodes silently, none hangs",
    jpegCuts.All(r => r.StartsWith("refused")),
    $"{jpegCuts.Count(r => r.StartsWith("refused"))}/19 refused, {jpegCuts.Count(r => r.StartsWith("decoded"))} decoded silently, " +
    $"{jpegCuts.Count(r => r.StartsWith("WRONG"))} wrong type, {jpegCuts.Count(r => r == "HANG")} hung");

// A frame header claiming 60000x60000: refused from the header, before the decoder allocates for the image.
var jpegAbsurd = (byte[])jpegSubject.Clone();
for (var i = 0; i + 8 < jpegAbsurd.Length; i++)
    if (jpegAbsurd[i] == 0xFF && jpegAbsurd[i + 1] is 0xC0 or 0xC1 or 0xC2)
    {
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(jpegAbsurd.AsSpan(i + 5), 60000);   // height
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(jpegAbsurd.AsSpan(i + 7), 60000);   // width
        break;
    }
Check("an absurd size in a JPEG frame header is refused before anything is allocated",
    Decoded(jpegAbsurd).Contains("pixel limit"), Decoded(jpegAbsurd));

// Orientation through the JPEG path, which shares nothing with the PNG path until the bake. Top half red, bottom
// half blue, split on the 8-row MCU boundary so the lossy coding cannot smear one into the other.
var halves16 = new byte[16 * 16 * 3];
for (var y = 0; y < 16; y++)
    for (var x = 0; x < 16; x++) { var i = ((y * 16) + x) * 3; if (y < 8) halves16[i] = 255; else halves16[i + 2] = 255; }
var jpegHalves = Texture.Decode(EncodeJpeg(halves16, 16, 16, 3), levels: 0);
var jpegTop = jpegHalves.Sample(new(0.5f, 0.9f));
var jpegBottom = jpegHalves.Sample(new(0.5f, 0.1f));
Check("a JPEG map is the right way up: high v reads the top (red) half",
    jpegTop.R > 200 && jpegTop.B < 60 && jpegBottom.B > 200 && jpegBottom.R < 60,
    $"v=0.9 -> {Rgb(jpegTop)}, v=0.1 -> {Rgb(jpegBottom)}");

// Greyscale JPEGs are common for masks and cheap assets; the port converts grey to RGB, so R = G = B.
var greyPixels = new byte[32 * 32];
for (var i = 0; i < greyPixels.Length; i++) greyPixels[i] = (byte)(i % 32 * 8);
var greyMap = Texture.Decode(EncodeJpeg(greyPixels, 32, 32, 1), levels: 0);
var greySample = greyMap.Sample(new(0.7f, 0.5f));
Check("a greyscale JPEG decodes to RGB with equal channels",
    greySample.R == greySample.G && greySample.G == greySample.B && greySample.R > 0, Rgb(greySample));

// The real pair: the capsule's JPEG decoded here, against the PNG Windows' own decoder made from it. Two conforming
// decoders differ by a few levels at most, so after the same reduce and quantise they should agree almost
// everywhere -- and anything flipped or channel-swapped would disagree nearly everywhere.
if (RepoPaths.Optional("media", "models", "capsule0.jpg") is { } capsuleJpg
    && RepoPaths.Optional("media", "models", "capsule0.png") is { } capsulePng)
{
    var viaJpeg = Texture.Load(capsuleJpg);
    var viaPng = Texture.Load(capsulePng);
    int samples = 0, differ = 0;
    for (var v = 0.01f; v < 1f; v += 0.02f)
        for (var u = 0.01f; u < 1f; u += 0.02f)
        {
            samples++;
            Color a = viaJpeg.Sample(new(u, v)), b = viaPng.Sample(new(u, v));
            if (Math.Abs(a.R - b.R) > 30 || Math.Abs(a.G - b.G) > 30 || Math.Abs(a.B - b.B) > 30) differ++;
        }
    Check("capsule0.jpg bakes to the same map as the PNG made from it",
        viaJpeg.Width == viaPng.Width && viaJpeg.Height == viaPng.Height && differ <= samples / 100,
        $"{viaJpeg.Width}x{viaJpeg.Height}, {differ} of {samples} samples differ by more than one quantisation step");
}
else Console.WriteLine("  skip  capsule0.jpg/.png — media/models not present");

// --- materials: MTL, usemtl, ModelLoader, the gate, and what each mode draws ----------------------------------------
Console.WriteLine("\nmaterials:");

var mtl = MtlLoader.Parse(["Kd 9 9 9", "newmtl red", "Kd 1 0 0", "map_Kd -s 2 2 1 -o 0.5 0 0 -clamp on my map.png",
                           "newmtl plain", "newmtl loud", "Kd 3 -1 0.5"]);
Check("an .mtl yields each material's Kd and map, in order, ignoring lines before the first newmtl",
    mtl.Count == 3 && mtl[0] is { Name: "red", MapKd: "my map.png" } && mtl[0].Kd == new Vector3(1, 0, 0),
    string.Join("; ", mtl.Select(d => $"{d.Name} Kd={d.Kd} map={d.MapKd ?? "-"}")));
Check("map_Kd options are skipped, and a filename with a space survives", mtl[0].MapKd == "my map.png", mtl[0].MapKd ?? "null");
Check("a material with no Kd is MTL's default white, and Kd is clamped to 0..1",
    mtl[1].Kd == Vector3.One && mtl[2].Kd == new Vector3(1, 0, 0.5f), $"plain {mtl[1].Kd}, loud {mtl[2].Kd}");

string[] quad = ["v -1 0 -1", "v 1 0 -1", "v 1 0 1", "v -1 0 1"];
var used = ObjLoader.Parse([.. quad, "f 1 2 3", "usemtl a", "f 1 3 4", "usemtl b", "f 2 3 4", "usemtl a", "f 1 2 4"]);
Check("usemtl gives each triangle its material, first-use order, and -1 before any usemtl",
    used.MaterialNames is ["a", "b"] && used.MaterialIds is [-1, 0, 1, 0],
    $"names [{string.Join(",", used.MaterialNames ?? [])}] ids [{string.Join(",", used.MaterialIds ?? [])}]");
// Lock-step, the lesson from the UVs: a dropped face must take its material id with it.
var droppedMat = ObjLoader.Parse([.. quad, "usemtl a", "f 1 2 9", "usemtl b", "f 1 2 3"]);
Check("a dropped face takes its material with it, leaving the survivor's",
    droppedMat.TriangleCount == 1 && droppedMat.MaterialIds is [1], $"ids [{string.Join(",", droppedMat.MaterialIds ?? [])}]");
Check("a file with no usemtl carries no material ids", ObjLoader.Parse([.. quad, "f 1 2 3"]) is { MaterialNames: null, MaterialIds: null });

// ModelLoader, against real files in a temp folder: an OBJ with two materials, one map that reads as an image and
// one that is noise.
var matDir = Directory.CreateTempSubdirectory("jc-materials-").FullName;
try
{
    byte[] Png(Func<int, int, byte[]> px, int w, int h)
    {
        var rgb = new byte[w * h * 3];
        for (var y = 0; y < h; y++) for (var x = 0; x < w; x++) px(x, y).CopyTo(rgb, ((y * w) + x) * 3);
        return PngSharp.Api.Png.EncodeToByteArray(PngSharp.Api.Png.CreateRgb(w, h, rgb));
    }

    var rng = new Random(7);
    File.WriteAllBytes(Path.Combine(matDir, "calm.png"), Png((x, _) => x < 32 ? [200, 40, 40] : [40, 40, 200], 64, 64));
    File.WriteAllBytes(Path.Combine(matDir, "busy.png"), Png((_, _) => [(byte)rng.Next(256), (byte)rng.Next(256), (byte)rng.Next(256)], 64, 64));
    File.WriteAllText(Path.Combine(matDir, "m.mtl"),
        "newmtl calm\nKd 1 1 1\nmap_Kd calm.png\nnewmtl busy\nKd 0.5 0.5 0.5\nmap_Kd busy.png\nnewmtl bare\nKd 0 1 0\nmap_Kd gone.png\n");
    string[] objBody = [.. quad, "vt 0 0", "vt 1 0", "vt 1 1", "vt 0 1",
                        "usemtl calm", "f 1/1 2/2 3/3", "usemtl busy", "f 1/1 3/3 4/4", "usemtl bare", "f 2/2 3/3 4/4"];
    File.WriteAllLines(Path.Combine(matDir, "m.obj"), ["mtllib m.mtl", .. objBody]);

    var loaded = ModelLoader.Load(Path.Combine(matDir, "m.obj"));
    var calm = loaded.Materials?.FirstOrDefault(m => m.Name == "calm");
    var busy = loaded.Materials?.FirstOrDefault(m => m.Name == "busy");
    var bare = loaded.Materials?.FirstOrDefault(m => m.Name == "bare");
    Check("ModelLoader resolves an OBJ's materials from its mtllib, maps and all",
        loaded.Materials?.Length == 3 && calm?.Map is not null && busy?.Map is not null,
        $"{loaded.Materials?.Length ?? 0} materials: {string.Join(", ", loaded.Materials?.Select(m => $"{m.Name}{(m.Map is null ? "" : "+map")}") ?? [])}");
    Check("UVs are read automatically when a material has a map", loaded.HasUvs, $"HasUvs={loaded.HasUvs}");
    Check("a map's flat colour is Kd times its average (MTL's rule), not Kd alone",
        busy is { } b && Math.Abs(b.Colour.R - (b.Map!.Average.R / 2)) <= 1 && calm!.Colour == calm.Map!.Average,
        $"calm {Rgb(calm?.Colour ?? default)} vs its average {Rgb(calm?.Map?.Average ?? default)}; busy {Rgb(busy?.Colour ?? default)}");
    Check("a map file that is absent leaves that material its Kd, not a failure",
        bare is { Map: null } && bare.Colour == new Color(0, 255, 0), bare is null ? "missing" : Rgb(bare.Colour));
    Check("the gate: a two-tone map reads as an image, per-texel noise does not",
        calm!.Map!.Legible && !busy!.Map!.Legible, $"calm busyness {calm.Map.Busyness:F2}, busy {busy!.Map!.Busyness:F2}");

    // Absent is not broken. No .mtl: no materials at all -- NOT a model's worth of MTL-default white.
    File.WriteAllLines(Path.Combine(matDir, "nomtl.obj"), ["mtllib absent.mtl", .. objBody]);
    var noMtl = ModelLoader.Load(Path.Combine(matDir, "nomtl.obj"));
    Check("a missing .mtl leaves no materials, so the model is drawn in its body colour rather than white",
        noMtl.Materials is null && noMtl.MaterialNames is not null, $"Materials {(noMtl.Materials is null ? "null" : "set")}");
    Check("and without a map to draw, UVs are not paid for", !noMtl.HasUvs, $"HasUvs={noMtl.HasUvs}");

    File.WriteAllLines(Path.Combine(matDir, "stranger.obj"), ["mtllib m.mtl", .. quad, "usemtl nobody", "f 1 2 3"]);
    Check("usemtl names the library does not define leave no materials",
        ModelLoader.Load(Path.Combine(matDir, "stranger.obj")).Materials is null);

    // Broken is not absent: a map that exists and will not decode fails the load, naming the file.
    File.WriteAllText(Path.Combine(matDir, "calm.png"), "not an image");
    string? brokenError = null;
    try { ModelLoader.Load(Path.Combine(matDir, "m.obj")); }
    catch (InvalidDataException e) { brokenError = e.Message; }
    Check("a map that exists but will not decode fails the load, naming the file",
        brokenError?.Contains("calm.png") == true, brokenError ?? "no exception -- the failure was swallowed");
}
finally
{
    Directory.Delete(matDir, recursive: true);
}

// What each mode puts on the SCREEN. Pixel comparisons between renders of the same knot, so each check is about
// what a human would see. The one that carries the gate: Auto with an unreadable map must render cell-for-cell as
// the same material drawn flat.
{
    var knotBase = Meshes.TorusKnot();
    Mesh Dressed(Material? material) => material is null ? knotBase : new Mesh(knotBase.Vertices, knotBase.Indices)
    {
        Uvs = knotBase.Uvs, MaterialNames = [material.Name], MaterialIds = new int[knotBase.TriangleCount],
    }.WithMaterials([material]);

    int[] Cells(Mesh mesh, TextureMode mode)
    {
        var snap = new SceneSnapshot(1) { Count = 1, AwakeCount = 1 };
        snap.Ids[0] = 1;
        snap.Shapes[0] = BodyShape.Mesh;
        snap.MeshIds[0] = Meshes.Register(mesh, "materials-check");
        snap.Positions[0] = new Vector3(0, 2f, 0);
        snap.Rotations[0] = Quaternion.Identity;
        snap.HalfExtents[0] = new Vector3(2f);
        snap.ColorKeys[0] = 1;
        snap.Awake[0] = true;
        var shaded = new ShadedRenderer { Texture = mode, Edges = SilhouetteStyle.None, OcclusionStrength = 0f };
        _ = ConsoleSnapshot.ToText(shaded.Surface, 80, 30);
        shaded.Draw(snap, new OrbitCamera { Target = new Vector3(0, 2f, 0), Distance = 7f });
        return CellColours((HalfBlockSurface)shaded.Surface);
    }

    static int Differ(int[] a, int[] b) => a.Zip(b).Count(p => p.First != p.Second);

    var flatColour = new Color(210, 120, 40);
    var noiseRgb = new byte[64 * 64 * 3];
    new Random(3).NextBytes(noiseRgb);
    var noiseMap = Texture.Bake(noiseRgb, 64, 64);
    var calmMap = Texture.Bake(Pattern(64, 64, (x, _) => x < 32), 64, 64);

    var plainCells = Cells(Dressed(null), TextureMode.Auto);
    var flatCells = Cells(Dressed(new Material("m", flatColour, null)), TextureMode.Auto);
    var autoNoise = Cells(Dressed(new Material("m", flatColour, noiseMap)), TextureMode.Auto);
    var imageNoise = Cells(Dressed(new Material("m", flatColour, noiseMap)), TextureMode.Image);
    var autoCalm = Cells(Dressed(new Material("m", flatColour, calmMap)), TextureMode.Auto);
    var imageCalm = Cells(Dressed(new Material("m", flatColour, calmMap)), TextureMode.Image);
    var offCalm = Cells(Dressed(new Material("m", flatColour, calmMap)), TextureMode.None);

    Check("Off draws no materials: the body colour, exactly as a mesh without any",
        Differ(offCalm, plainCells) == 0, $"{Differ(offCalm, plainCells)} cells differ");
    Check("a material without a map draws its flat colour", Differ(flatCells, plainCells) > 0,
        $"{Differ(flatCells, plainCells)} cells differ from the body colour");
    Check("THE GATE: Auto draws an unreadable map exactly as its flat colour",
        Differ(autoNoise, flatCells) == 0, $"{Differ(autoNoise, flatCells)} cells differ from flat");
    // Exact, not "the colour changed": every body cell changes in all of these, which a wrong flat colour would pass
    // just as well. Counting distinct colours was tried and is fragile -- a two-tone map gave 15 against flat's 14.
    Check("Auto draws a readable map exactly as On does, and that is not the flat colour",
        Differ(autoCalm, imageCalm) == 0 && Differ(imageCalm, flatCells) > 0,
        $"{Differ(autoCalm, imageCalm)} cells differ from On, {Differ(imageCalm, flatCells)} from flat");
    static int Distinct(int[] cells) => cells.Distinct().Count();
    Check("On draws every map, the unreadable one included",
        Distinct(imageNoise) > Distinct(flatCells) * 4, $"{Distinct(imageNoise)} distinct colours against flat's {Distinct(flatCells)}");
}

// End to end, as the app runs it: ModelLoader resolves the capsule's .mtl, the REAL viewer shell opens on it, and it
// is drawn with whatever renderer and mode the viewer starts in. Every link above is checked on its own; this is the
// check that the chain holds -- the answer to "if I load capsule.obj, do I see its texture?".
if (RepoPaths.Optional("media", "models", "capsule.obj") is { } capsuleObj)
{
    var capsuleMesh = ModelLoader.Load(capsuleObj);
    using var viewerApp = SandboxShell.BuildViewer(Meshes.Register(capsuleMesh, "capsule-e2e"));
    _ = ConsoleSnapshot.ToText(viewerApp.Root, W, H);
    var viewerView = viewerApp.View;

    int[] ViewerCells()
    {
        viewerView.Renderer.Draw(viewerApp.Model.Snapshot, viewerView.Camera);
        return CellColours((HalfBlockSurface)viewerView.Renderer.Surface);
    }

    Check("the viewer opens on the shaded renderer, texturing on Auto",
        viewerView.Renderer is ShadedRenderer && viewerView.Texture == TextureMode.Auto,
        $"{viewerView.Renderer.Name}, texture {viewerView.Texture?.ToString() ?? "n/a"}");
    Check("capsule.obj arrives with its material and its map",
        capsuleMesh.Materials is [{ Map: not null }] && capsuleMesh.HasUvs,
        $"{capsuleMesh.Materials?.Length ?? 0} material(s), map {(capsuleMesh.Materials?[0].Map is null ? "none" : "loaded")}");
    var opened = ViewerCells();

    // e2epng=DIR: the whole real viewer shell as a picture, at a size its sidebar fits -- the only way to judge
    // the Texture drop-down beside a textured model, and whether Colour really greys out.
    if (args.FirstOrDefault(a => a.StartsWith("e2epng="))?[7..] is { } e2eDir)
    {
        var e2eOpt = new SnapshotImageOptions { FontFamily = "Cascadia Mono", CellWidth = 9, CellHeight = 18 };
        _ = ConsoleSnapshot.ToText(viewerApp.Root, 200, 60);
        _ = ViewerCells();
        ConsoleSnapshot.SavePng(viewerApp.Root, 200, 60, Path.Combine(e2eDir, "viewer-capsule-auto.png"), e2eOpt);
    }

    viewerView.SetTexture(TextureMode.None);
    var untextured = ViewerCells();
    var texturedCells = opened.Zip(untextured).Count(p => p.First != p.Second);
    Check("so loading capsule.obj in the viewer shows its texture", texturedCells > 50,
        $"{texturedCells} cells differ from the same view with textures off");
}
else Console.WriteLine("  skip  capsule.obj end to end — media/models not present");

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
var modelDir = RepoPaths.At("reference", "projects", "voxcii-main", "models");

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

var emptyDir = ModelLibrary.Resolve(RepoPaths.At("src"));
Check("a directory with no models is an error", emptyDir.Error is not null, emptyDir.Error);

// No argument is NOT an error even with nothing to find: the viewer falls back to its generated mesh.
var noneGiven = ModelLibrary.Resolve(null, RepoPaths.At("src"));
Check("no argument is never an error", noneGiven.Error is null && noneGiven.Files.Length == 0,
    $"{noneGiven.Files.Length} files, error={noneGiven.Error ?? "none"}");

// With no argument the ONLY place looked at is a models/ folder beside the working directory -- never the working
// directory itself, which would scoop up whatever .obj happened to be next to you.
var modelsParent = Directory.GetParent(modelDir)!.FullName;
var defaulted = ModelLibrary.Resolve(null, modelsParent);
Check("no argument finds a models/ folder beside the working directory",
    defaulted.Files.Length == 4 && defaulted.Error is null,
    $"{defaulted.Files.Length} files from '{modelsParent}', error={defaulted.Error ?? "none"}");

// The .obj files sit IN modelDir, so resolving with modelDir as the working directory must find none: proof the
// rule is "a models/ subfolder", not "this folder".
var notCwd = ModelLibrary.Resolve(null, modelDir);
Check("and does not load the working directory itself", notCwd.Files.Length == 0 && notCwd.Error is null,
    $"{notCwd.Files.Length} files, error={notCwd.Error ?? "none"}");

// The start index must survive into the scene.
var startAt = new ModelScene(1);
Check("ModelScene opens on the requested index", startAt.MeshId == 1, $"{startAt.MeshId} ({startAt.Name})");
Check("and clamps an out-of-range one", new ModelScene(999).MeshId == Meshes.RegisteredCount - 1);

// --- STL ----------------------------------------------------------------------------------------------------------
// Checked against the FILE rather than against the loader's own opinion: the facet count, the degenerate facets and
// the winding of every triangle are all recomputed here from the raw bytes, so a loader that silently agreed with
// itself would fail.
Console.WriteLine("\nSTL:");
var stlPath = RepoPaths.At("media", "models", "cali-bee.stl");
if (!File.Exists(stlPath))
{
    Console.WriteLine("  SKIP  media/models/cali-bee.stl is not present");
}
else
{
    var stlBytes = File.ReadAllBytes(stlPath);
    var declared = BitConverter.ToUInt32(stlBytes, 80);

    Check("a binary STL is detected by its LENGTH, not its first word",
        StlLoader.IsBinary(stlBytes.Length, stlBytes), $"{declared} facets, {stlBytes.Length} bytes");

    // The trap the length test exists for: an 80-byte header is free-form, and exporters do write "solid ..." into
    // it. A reader that sniffs the first five bytes calls this binary file ASCII and returns an empty mesh.
    var disguised = (byte[])stlBytes.Clone();
    "solid "u8.CopyTo(disguised.AsSpan(0, 6));
    Check("even one whose header begins \"solid\"", StlLoader.IsBinary(disguised.Length, disguised),
        $"header now reads '{System.Text.Encoding.ASCII.GetString(disguised, 0, 12)}'");

    // Ground truth, recomputed from the bytes: which facets are degenerate, and which are wound against their own
    // stored normal.
    int degenerate = 0, misWound = 0;
    var misWoundAt = new List<int>();
    var facetNormals = new List<Vector3>();
    for (var f = 0u; f < declared; f++)
    {
        var at = 84 + ((int)f * 50);
        Vector3 Read(int o) => new(BitConverter.ToSingle(stlBytes, at + o),
                                   BitConverter.ToSingle(stlBytes, at + o + 4),
                                   BitConverter.ToSingle(stlBytes, at + o + 8));
        var n = Read(0);
        var (a, b, c) = (Read(12), Read(24), Read(36));
        var winding = Vector3.Cross(b - a, c - a);
        if (winding.LengthSquared() < 1e-20f) { degenerate++; continue; }
        if (Vector3.Dot(winding, n) < 0f) { misWound++; misWoundAt.Add(facetNormals.Count); }
        facetNormals.Add(n);
    }

    var bee = StlLoader.Load(stlPath);
    Check("every non-degenerate facet becomes a triangle", bee.TriangleCount == declared - degenerate,
        $"{declared} declared − {degenerate} degenerate = {declared - degenerate}, loaded {bee.TriangleCount}");

    // Welding is not cosmetic: a body's vertices are transformed once per frame and referenced by its triangles,
    // so a soup pays for every corner three times over.
    Check("corners are welded", bee.Vertices.Length < bee.TriangleCount * 3 / 2,
        $"{bee.TriangleCount * 3} corners in the file → {bee.Vertices.Length} vertices");

    // The winding invariant, asserted on the OUTPUT: every triangle must wind the way its own facet normal says,
    // which is what stops the back-face cull eating scattered facets and leaving a model full of holes.
    //
    // SLIVERS ARE EXCLUDED, and working out why was the useful part. This file has a dozen facets whose area after
    // normalisation is 5e-12 to 2e-9 against a median of 1.2e-3 — six to nine orders of magnitude down, one of them
    // underflowing to exactly zero. A cross product that small has no reliable DIRECTION left, so "which way does
    // it wind" is not a question with an answer. The loader's absolute degeneracy guard cannot catch them either:
    // it runs in the file's own units, before the model is scaled down. They cover no pixels either way.
    var areas = new List<float>();
    for (var t = 0; t < bee.TriangleCount && t < facetNormals.Count; t++)
    {
        var a = bee.Vertices[bee.Indices[t * 3]];
        var b = bee.Vertices[bee.Indices[(t * 3) + 1]];
        var c = bee.Vertices[bee.Indices[(t * 3) + 2]];
        areas.Add(Vector3.Cross(b - a, c - a).Length());
    }

    var sliver = areas.Order().ToArray()[areas.Count / 2] * 1e-5f;   // five orders below median; the gap measures nine
    int wrongAfter = 0, slivers = 0;
    for (var t = 0; t < areas.Count; t++)
    {
        if (areas[t] < sliver) { slivers++; continue; }
        var a = bee.Vertices[bee.Indices[t * 3]];
        var b = bee.Vertices[bee.Indices[(t * 3) + 1]];
        var c = bee.Vertices[bee.Indices[(t * 3) + 2]];
        if (Vector3.Dot(Vector3.Cross(b - a, c - a), facetNormals[t]) < 0f) wrongAfter++;
    }

    Check("every triangle winds the way its facet normal says", wrongAfter == 0,
        $"{wrongAfter} wrong of {areas.Count - slivers} facets ({slivers} slivers excluded)");

    // And the honest reading of this particular model: it needed no flipping at all. Every facet the loader
    // reversed was one of those slivers, so the fix is UNEXERCISED here — the synthetic ASCII facet below is what
    // proves it works, and this line is what would notice a real file that disagreed.
    Check("...and this file was already wound consistently",
        misWoundAt.All(i => i < areas.Count && areas[i] < sliver),
        $"{misWound} facets reversed at load, every one a sliver");

    Check("STL defaults to Z-up", bee.AuthoredUpAxis == ModelUpAxis.Z, bee.AuthoredUpAxis?.ToString() ?? "none");
    Check("and the extension decides the loader", ModelLoader.Load(stlPath).AuthoredUpAxis == ModelUpAxis.Z);
}

// ASCII STL, on a synthetic one so the geometry is known exactly. Written with the ragged indentation and the
// keywords a real writer emits, since that is what the reader has to survive.
string[] asciiStl =
[
    "solid tetra",
    "  facet normal 0 0 1",
    "    outer loop",
    "      vertex 0 0 0",
    "      vertex 1 0 0",
    "      vertex 0 1 0",
    "    endloop",
    "  endfacet",
    // Wound clockwise against a normal that says otherwise -- the malformed-but-common case the fix is for.
    "  facet normal 0 0 1",
    "    outer loop",
    "      vertex 0 0 0",
    "      vertex 0 1 0",
    "      vertex 1 0 0",
    "    endloop",
    "  endfacet",
    "endsolid tetra",
];

var ascii = StlLoader.ParseAscii(asciiStl);
Check("ASCII STL parses", ascii.TriangleCount == 2, $"{ascii.TriangleCount} triangles");
Check("and welds its shared corners", ascii.Vertices.Length == 3, $"{ascii.Vertices.Length} vertices");
var flipped = 0;
for (var t = 0; t < ascii.TriangleCount; t++)
{
    var a = ascii.Vertices[ascii.Indices[t * 3]];
    var b = ascii.Vertices[ascii.Indices[(t * 3) + 1]];
    var c = ascii.Vertices[ascii.Indices[(t * 3) + 2]];
    if (Vector3.Cross(b - a, c - a).Z > 0f) flipped++;
}

// Both facets name +Z, and the second is wound the other way in the file -- so a loader that ignored the stored
// normal would return one triangle facing each way, and the cull would drop one of them.
Check("a facet wound against its normal is corrected", flipped == 2, $"{flipped} of 2 face +Z");

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

    // Quadrant sampling doubles the sub-pixels to rasterise and shade, so it is priced against its own baseline
    // rather than against the rows above -- read each pair, not the column.
    Time("solid", new SolidRenderer());
    Time("  ..quadrants", new SolidRenderer { QuadrantSampling = true });
    Time("shaded+ao", new ShadedRenderer());
    Time("  ..quadrants", new ShadedRenderer { QuadrantSampling = true });

    
    runner.Dispose();
    return 0;
}

// --- silhouettes ----------------------------------------------------------------------------------------------
// The detector's whole claim is that it fires on creases and silhouettes but NOT on flat ground, however steeply
// that ground recedes. Both halves of that are asserted, because a detector that lights up the floor would be
// worse than none.
Console.WriteLine("\nsilhouettes:");
solid.Draw(solidScene, view.Camera);

var edgeSurface = (HalfBlockSurface)solid.Surface;

// Every check above reads the DETECTOR -- which sub-pixels were marked. None of them read what the marking then
// DOES to the picture, and that gap let a deleted line ship: `Line` marked its edges and drew nothing, silently,
// for two commits. So compare the rendered cells with the outline off and on, and require that they differ.
var edgesOff = 0;
solid.Edges = SilhouetteStyle.None;
solid.Draw(solidScene, view.Camera);
var offCells = CellColours(edgeSurface);
solid.Edges = SilhouetteStyle.Line;
solid.Draw(solidScene, view.Camera);
var lineCells = CellColours(edgeSurface);
for (var c = 0; c < offCells.Length; c++) if (offCells[c] != lineCells[c]) edgesOff++;
Check("the Line style actually brightens pixels, not just the edge mask", edgesOff > 0,
    $"{edgesOff} cells differ between Edges=None and Edges=Line");

// And Glyph must differ from Line too -- they are different presentations of the same edge set, so a build where
// one silently fell back to the other would otherwise look fine.
solid.Edges = SilhouetteStyle.Glyph;
solid.Draw(solidScene, view.Camera);
var glyphCells = CellColours(edgeSurface);
var lineVsGlyph = 0;
for (var c = 0; c < lineCells.Length; c++) if (lineCells[c] != glyphCells[c]) lineVsGlyph++;
Check("and Glyph differs from Line", lineVsGlyph > 0, $"{lineVsGlyph} cells differ");

static int[] CellColours(HalfBlockSurface s)
{
    var cells = new int[s.PixelWidth * (s.PixelHeight / 2)];
    for (var y = 0; y < s.PixelHeight / 2; y++)
        for (var x = 0; x < s.PixelWidth; x++)
        {
            var c = s.ColorAt(x, y * 2);
            cells[(y * s.PixelWidth) + x] = (c.Red << 16) | (c.Green << 8) | c.Blue;
        }

    return cells;
}
solid.Edges = SilhouetteStyle.Glyph;
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

// --- quadrant sampling ------------------------------------------------------------------------------------------
// Everything here reads the CONSOLE CELLS, not the sub-pixel buffer. The pass is a compositor: it changes nothing
// about what was rasterised, only how a 2x2 block is turned into one cell, so a check that reads the surface would
// be reading the half of the pipeline this feature does not touch. (Same lesson as the deleted Line statement: a
// check that asserts an internal mark is half a check.)
Console.WriteLine("\nquadrant sampling:");
solid.Edges = SilhouetteStyle.None;

ConsoleBuffer QuadFrame()
{
    solid.Draw(solidScene, view.Camera);
    _ = ConsoleSnapshot.ToText(root, W, H);
    solid.Draw(solidScene, view.Camera);
    return ConsoleSnapshot.Render(root, W, H);
}

solid.QuadrantSampling = false;
var plainFrame = QuadFrame();
var plainSamples = edgeSurface.SamplesPerColumn;
var plainWidth = edgeSurface.PixelWidth;

solid.QuadrantSampling = true;
var quadFrame = QuadFrame();

Check("the buffer samples twice per column", edgeSurface.SamplesPerColumn == 2 && plainSamples == 1,
    $"{plainSamples} -> {edgeSurface.SamplesPerColumn}");
Check("and is twice as wide in sub-pixels", edgeSurface.PixelWidth == plainWidth * 2,
    $"{plainWidth} -> {edgeSurface.PixelWidth}");

int changed = 0, quadGlyphs = 0, offRamp = 0;
var rampColours = new HashSet<(byte, byte, byte)>();
for (var sy = 0; sy < edgeSurface.PixelHeight; sy++)
    for (var sx = 0; sx < edgeSurface.PixelWidth; sx++)
    {
        var c = edgeSurface.ColorAt(sx, sy);
        rampColours.Add((c.Red, c.Green, c.Blue));
    }

for (var y = 1; y < H - 3; y++)
{
    for (var x = 1; x < W - 1; x++)
    {
        var plainCell = plainFrame[x, y].Character;
        var after = quadFrame[x, y].Character;
        if (plainCell.Content != after.Content || plainCell.Foreground != after.Foreground
            || plainCell.Background != after.Background) changed++;
        if (after.Content is { } glyph && glyph is not ('▀' or ' ') && "▘▝▖▌▞▛▗▚▐▜▄▙▟█".Contains(glyph)) quadGlyphs++;
        // The colour discipline: every colour a quadrant cell emits must be one the RENDERER put in the sub-pixel
        // buffer. Averaging the two groups instead of picking a member of each would fail this on the first
        // boundary cell, and that is the whole difference from the smoothing pass -- which buys its softening in
        // exactly this currency.
        if (after.Foreground is { } f && !rampColours.Contains((f.Red, f.Green, f.Blue))) offRamp++;
        if (after.Background is { } b && !rampColours.Contains((b.Red, b.Green, b.Blue))) offRamp++;
    }
}

Check("the picture changes", changed > 100, $"{changed} cells differ");
Check("quadrant glyphs reach the screen", quadGlyphs > 20, $"{quadGlyphs} cells carry one");
Check("and every colour they emit is one the renderer produced", offRamp == 0,
    $"{offRamp} cell colours are not in the sub-pixel buffer");

// The claim itself, measured on what a reader would see: a silhouette that lands between two columns. Whole-cell
// compositing cannot express one, so the count off must be exactly zero -- which is what makes the count on
// meaningful rather than merely larger.
var plainEdge = Silhouette(plainFrame, edgeSurface.Background, W, H);
var quadEdge = Silhouette(quadFrame, edgeSurface.Background, W, H);
int plainHalves = plainEdge.Count(b => b > 0 && (b & 1) == 1), quadHalves = quadEdge.Count(b => b > 0 && (b & 1) == 1);
Check("no silhouette lands inside a cell without it", plainHalves == 0, $"{plainHalves} of {plainEdge.Count(b => b > 0)}");
Check("and a good share of them do with it", quadHalves > quadEdge.Count(b => b > 0) / 5,
    $"{quadHalves} of {quadEdge.Count(b => b > 0)} silhouette rows");

solid.QuadrantSampling = false;

// --- the selection tint -------------------------------------------------------------------------------------
// The sandbox is the scene that HAS a selection (SceneView.SupportsSelection, which the model viewer does not), and
// what a selection does is repaint the body in Palette.Selection. Asserted on the cells rather than on the flag,
// because the flag was never the problem: the viewer bug was that a body could be tintedPixels white in a scene whose
// only colour control was the Colour drop-down, and nothing there could clear it.
//
// This is also the guard on the OTHER direction of that fix: gating selection on having a physics runner must not
// have taken selection away from the scene that wants it.
Console.WriteLine("\nselection tint:");
Check("the sandbox supports selection", view.SupportsSelection);

view.Selected = null;
solid.Draw(solidScene, view.Camera);
var selectionOffCells = CellColours(edgeSurface);
view.Selected = solidScene.Ids[0];
solid.Draw(solidScene, view.Camera);
var selectionOnCells = CellColours(edgeSurface);
var tintedPixels = 0;
for (var c = 0; c < selectionOffCells.Length; c++) if (selectionOffCells[c] != selectionOnCells[c]) tintedPixels++;
Check("selecting a body repaints it", view.Selected == solidScene.Ids[0] && tintedPixels > 20,
    $"{tintedPixels} sub-pixels changed");

view.Selected = null;
solid.Draw(solidScene, view.Camera);

// Put back what this section borrowed. A check that leaves global state behind hides bugs from every check after
// it -- this is the harness's own lesson, learnt when an early `v` left the wireframe active for the rest of a run.
solid.Edges = SilhouetteStyle.Glyph;

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
