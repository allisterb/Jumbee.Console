using Jumbee.Console;
using Jumbee.Console.Snapshot;
using Jumbee.Console.Wolf3DDemo;

// Headless checks for the Wolf3D demo. Drives the REAL shell (Wolf3DShell.Build), not a rebuild of it, and routes
// keys through the ROOT LAYOUT the way the live loop does — a key routed straight at a control takes a different
// path and can pass while the running app receives nothing.
const string GameData = @"C:\Projects\Jumbee.Console\examples\Jumbee.Console.Wolf3DDemo\GameData";
// Overridable as a bare WxH argument: a sidebar bug is usually a bug at ONE height, and the compact layout only
// appears below the threshold the pages derive for themselves.
var size = args.FirstOrDefault(a => a.Contains('x') && a.All(c => char.IsDigit(c) || c == 'x'))?.Split('x');
var W = size is null ? 200 : int.Parse(size[0]);
var H = size is null ? 52 : int.Parse(size[1]);


var failures = 0;
void Check(string what, bool ok, string? detail = null)
{
    Console.WriteLine($"  {(ok ? "PASS" : "FAIL")}  {what}{(detail is null ? "" : $"  [{detail}]")}");
    if (!ok) failures++;
}

var mode = args.FirstOrDefault(a => !a.StartsWith("-")) ?? "check";
var outDir = args.FirstOrDefault(a => a.StartsWith("out="))?[4..] ?? "out";

var scene = new Wolf3DScene(GameData);
using var shell = Wolf3DShell.Build(scene);
var view = shell.View;

// Lay the tree out and draw a frame, in that order: the surface has no size until the first render.
//
// TWICE, since the status bar arrived. It is a Bottom-docked sibling that derives its own height from the width the
// dock gives it, so pass one is what tells it how tall to be and pass two is the first layout where the viewport
// has its final height. With one pass the viewport is drawn at the pre-resize height and then composited at the
// post-resize one, which lights about a quarter of the screen and reads as "the renderer is broken" rather than as
// "the frame has not settled". The live app settles the same way, on frame two, in 33ms.
view.Focus();   // as Program.cs does before UI.Start
for (var settle = 0; settle < 2; settle++)
{
    _ = ConsoleSnapshot.ToText(shell.Root, W, H);
    shell.Hud.Reflow();   // what UI.Paint does in the running app
    view.DrawFrame();
}

if (mode == "png")
{
    Directory.CreateDirectory(outDir);
    var options = new SnapshotImageOptions { FontFamily = "Cascadia Mono" };
    foreach (var (label, quant, quad, tab) in
             new[] { ("shell", 6, false, 0), ("shell-full", 0, false, 0), ("shell-aa", 6, true, 0), ("shell-input", 6, false, 1) })
    {
        view.Renderer.QuantizeLevels = quant;
        view.Sampling = quad ? SurfaceMode.Quadrant : SurfaceMode.HalfBlock;
        shell.Sidebar.Tabs.SelectedIndex = tab;
        // Laid out, drawn, laid out, drawn: the status bar derives its own height from the width the dock gives it,
        // so the first pass is what tells it how wide it is and the second is the first frame drawn at the right
        // size. One pass captures the bar at its floor height, which reads as a deliberate design rather than as a
        // frame that has not settled -- exactly the kind of wrong a single-pass snapshot hides.
        for (var pass = 0; pass < 2; pass++)
        {
            _ = ConsoleSnapshot.ToText(shell.Root, W, H);
            shell.Hud.Reflow();
            view.DrawFrame();
        }

        var buffer = ConsoleSnapshot.Render(shell.Root, W, H);
        Console.WriteLine($"    hud {shell.Hud.ActualWidth}x{shell.Hud.ActualHeight} " +
                          $"(RowsFor={Wolf3DHud.RowsFor(shell.Hud.ActualWidth)}), view {shell.View.ActualWidth}x{shell.View.ActualHeight}");
        if (label == "shell-full")
            for (var y = H - 14; y < H - 2; y++)
            {
                var ch = buffer[20, y].Character;
                Console.WriteLine($"      row {y,2}: '{ch.Content}' fg={ch.Foreground?.Red},{ch.Foreground?.Green},{ch.Foreground?.Blue} bg={ch.Background?.Red},{ch.Background?.Green},{ch.Background?.Blue}");
            }
        if (label == "shell-full")
        {
            var top = H - 2 - shell.Hud.ActualHeight;
            for (var y = top; y < H - 2; y++)
            {
                var ink = 0;
                for (var x = 0; x < shell.Hud.ActualWidth; x++)
                {
                    var c = buffer[x, y].Character.Foreground;
                    if (c is { } f && !(f.Red < 20 && f.Green is > 40 and < 90 && f.Blue is > 40 and < 90)) ink++;
                }

                Console.WriteLine($"      hud row {y - top,2}: {ink,4} ink of {shell.Hud.ActualWidth}");
            }
        }

        var path = Path.Combine(outDir, $"{label}.png");
        ConsoleSnapshot.SavePng(buffer, path, options);
        Console.WriteLine($"  {label,-12} runs {view.LastCost.Runs,6:N0}  colours {view.LastCost.Colors,4}  -> {path}");
    }

    return 0;
}

// The status bar on its own, at several widths. Judged alone because in the shell it is ten rows at the bottom of
// a busy screen, which is exactly where a scaling bug hides.
if (mode == "hud")
{
    Directory.CreateDirectory(outDir);
    var hudOptions = new SnapshotImageOptions { FontFamily = "Cascadia Mono" };
    foreach (var columns in new[] { 80, 120, 168, 320 })
        foreach (var quad in new[] { false, true })
        {
            var solo = new Wolf3DHud(scene) { QuadrantSampling = quad };
            var rows = Wolf3DHud.RowsFor(columns);
            // Twice, for the same reason the shell snapshot draws twice: the bar derives its height from its width.
            for (var pass = 0; pass < 2; pass++) { _ = ConsoleSnapshot.ToText(solo, columns, rows); solo.Refresh(); }
            var b = ConsoleSnapshot.Render(solo, columns, rows);
            var name = $"hud-{columns}x{rows}-{(quad ? "quad" : "half")}.png";
            ConsoleSnapshot.SavePng(b, Path.Combine(outDir, name), hudOptions);
            Console.WriteLine($"  {name,-24} colours {solo.LastColors,4}");
        }

    return 0;
}

// The user's exact sequence: start maximized, shrink, maximize again. Each step reported so a bar that is right at
// one size and wrong after a round trip is visible as a table rather than as three screenshots.
if (mode == "resize")
{
    // A FRESH shell per size, never rendered at any other -- the startup path. The walk below reuses one shell and
    // so only ever tests a size CHANGE; a bar that is wrong until the first resize is invisible to it.
    Console.WriteLine("\n  Cold start (a new shell, rendered only at this size):\n");
    Console.WriteLine("  size       hud rows  RowsFor   verdict");
    foreach (var (cw, ch) in new[] { (200, 52), (120, 32), (90, 26), (70, 22) })
    {
        var cold = Wolf3DShell.Build(scene);
        for (var settleFrame = 0; settleFrame < 6; settleFrame++)
        {
            _ = ConsoleSnapshot.ToText(cold.Root, cw, ch);
            cold.Hud.Reflow();
            cold.View.DrawFrame();
        }

        var coldWant = Wolf3DHud.RowsFor(cold.Hud.ActualWidth);
        Console.WriteLine($"  {cw}x{ch,-6}  {cold.Hud.ActualHeight,8}  {coldWant,7}   " +
                          $"{(cold.Hud.ActualHeight == coldWant ? "ok" : "WRONG")}");
        cold.Dispose();
    }

    // The REAL startup sequence, which neither walk above reproduces. UI.Start lays the app out at the size the
    // app asked for (Program.cs: 200x52) and only then does the actual window size take over -- so a small
    // terminal gets one wide layout first. And it gets ONE frame at each, not six: six frames hide any bug that
    // settles late, which is every bug of this kind.
    Console.WriteLine("\n  Startup sequence (requested 200x52, then the real window, one frame each):\n");
    Console.WriteLine("  real size   hud rows  RowsFor  blank   verdict");
    foreach (var (rw, rh) in new[] { (120, 32), (90, 26), (200, 52) })
    {
        var boot = Wolf3DShell.Build(scene);
        // What UI.Start does: ConsoleManager.Resize to the requested size...
        _ = ConsoleSnapshot.ToText(boot.Root, 200, 52);
        boot.Hud.Reflow();
        boot.View.DrawFrame();
        // ...then AdjustBufferSize replaces it with the terminal's real size on the next frame.
        _ = ConsoleSnapshot.ToText(boot.Root, rw, rh);
        boot.Hud.Reflow();
        boot.View.DrawFrame();

        var bootWant = Wolf3DHud.RowsFor(boot.Hud.ActualWidth);
        // Height alone is not the test. It was RIGHT while the bar still drew at the wrong scale, because the
        // surface inside lags the control by a layout pass -- so count the ink actually on screen as well.
        var bootBuf = ConsoleSnapshot.Render(boot.Root, rw, rh);
        var bootTop = rh - 2 - boot.Hud.ActualHeight;
        var bootBlank = 0;
        for (var y = bootTop; y < rh - 2; y++)
        {
            var ink = 0;
            for (var x = 0; x < boot.Hud.ActualWidth; x++)
            {
                var c = bootBuf[x, y].Character.Foreground;
                if (c is { } f && !(f.Red < 20 && f.Green is > 40 and < 90 && f.Blue is > 40 and < 90)) ink++;
            }

            if (ink < boot.Hud.ActualWidth / 2) bootBlank++;
        }

        var bootOk = boot.Hud.ActualHeight == bootWant && bootBlank <= 1;
        Console.WriteLine($"  {rw}x{rh,-7}  {boot.Hud.ActualHeight,8}  {bootWant,7}  {bootBlank,6}   " +
                          $"{(bootOk ? "ok" : "WRONG")}");
        boot.Dispose();
    }

    Console.WriteLine("\n  Resize walk (one shell, sizes changed under it):\n");
    Console.WriteLine("  size       hud rows  RowsFor  surface     blank rows   verdict");
    foreach (var (w, h) in new[] { (200, 52), (120, 32), (200, 52), (90, 26), (200, 52) })
    {
        // Frames, not passes: this is what the live app does -- paint, reflow, tick -- repeated. If the bar needs
        // more than a couple of frames to settle, that is the bug rather than the measurement.
        for (var settleFrame = 0; settleFrame < 6; settleFrame++)
        {
            _ = ConsoleSnapshot.ToText(shell.Root, w, h);
            shell.Hud.Reflow();
            view.DrawFrame();
        }

        var buf = ConsoleSnapshot.Render(shell.Root, w, h);
        var rows = shell.Hud.ActualHeight;
        var top = h - 2 - rows;
        var blank = 0;
        for (var y = top; y < h - 2; y++)
        {
            var ink = 0;
            for (var x = 0; x < shell.Hud.ActualWidth; x++)
            {
                var c = buf[x, y].Character.Foreground;
                if (c is { } f && !(f.Red < 20 && f.Green is > 40 and < 90 && f.Blue is > 40 and < 90)) ink++;
            }

            if (ink < shell.Hud.ActualWidth / 2) blank++;
        }

        var want = Wolf3DHud.RowsFor(shell.Hud.ActualWidth);
        var ok = rows == want && blank <= 1;
        Console.WriteLine($"  {w}x{h,-6}  {rows,8}  {want,7}  {shell.Hud.ActualWidth,3}x{rows,-3}  {blank,10}   {(ok ? "ok" : "WRONG")}");
    }

    Console.WriteLine();
    return 0;
}

if (mode == "surfaces") return Surfaces.Run(scene, 200, 50);

if (mode == "braille") return BrailleProbe.Run(outDir);

if (mode == "rowfilter")
{
    RowFilterCheck.Run(scene, 200, 50);

    // The blit score above isolates the filter; this charges it end to end over the assembled shell, where the
    // quadrant compositor sits downstream of both strategies and only the emitted bytes matter.
    Console.WriteLine("\n  End to end, quadrant sampling, 200x52:\n");
    Console.WriteLine("  size      motion  fov    quant  quad  filter     scene    paint     emit    TOTAL   ANSI B/frame   runs");
    Measure(200, 52, 'w', "walk", true, 10, true, RowFilter.Nearest, quiet: true);
    foreach (var quant in new[] { 0, 10 })
        foreach (var filter in new[] { RowFilter.Nearest, RowFilter.Box })
            Measure(200, 52, 'w', "walk", true, quant, true, filter);

    // PNGs last, and of the SAME scene under both filters, because the numbers above cannot say whether the
    // reconstructed detail reads as detail or as mush. Cascadia Mono: a shell dump would mangle the quadrant glyphs.
    Directory.CreateDirectory(outDir);
    var filterOptions = new SnapshotImageOptions { FontFamily = "Cascadia Mono" };
    view.Sampling = SurfaceMode.Quadrant;
    foreach (var (quant, rowFilter) in
             new[] { (10, RowFilter.Nearest), (10, RowFilter.Box), (0, RowFilter.Nearest), (0, RowFilter.Box) })
    {
        view.Renderer.QuantizeLevels = quant;
        view.Renderer.RowFilter = rowFilter;
        _ = ConsoleSnapshot.ToText(shell.Root, W, H);
        view.DrawFrame();
        var name = $"rowfilter-{(quant == 0 ? "full" : quant + "ch")}-{rowFilter.ToString().ToLowerInvariant()}.png";
        ConsoleSnapshot.SavePng(ConsoleSnapshot.Render(shell.Root, W, H), Path.Combine(outDir, name), filterOptions);
        Console.WriteLine($"  {name,-32} colours {view.LastCost.Colors,4}");
    }

    Console.WriteLine();
    return 0;
}

if (mode == "rows")
{
    // Left 44 columns and right 36 of the top rows, to see where a control's text is actually landing.
    shell.Sidebar.Tabs.SelectedIndex = args.Contains("display") ? 0 : 1;
    _ = ConsoleSnapshot.ToText(shell.Root, W, H);
    view.DrawFrame();
    var rows = ConsoleSnapshot.ToLines(ConsoleSnapshot.Render(shell.Root, W, H));
    for (var y = 0; y < 18; y++)
        Console.WriteLine($"{y,2} |{rows[y][..44].Replace(' ', '.')}| ... |{rows[y][^36..].Replace(' ', '.')}|");
    return 0;
}

if (mode == "perf")
{
    // The frame cost of the ASSEMBLED shell -- border, footer and all -- over the real ConsoleManager, so the
    // published numbers describe the app rather than a bare surface.
    Console.WriteLine("\n  size      motion  fov    quant  quad  filter     scene    paint     emit    TOTAL   ANSI B/frame   runs");
    Measure(200, 52, 'w', "walk", true, 10, false, RowFilter.Nearest, quiet: true);
    foreach (var (w, h) in new[] { (120, 32), (200, 52), (240, 62) })
    {
        foreach (var (motion, label) in new[] { ('\0', "still"), ('w', "walk"), ('d', "turn") })
        {
            foreach (var authentic in new[] { true, false })
            {
                foreach (var quant in new[] { 0, 6 })
                {
                    if (motion == '\0' && (!authentic || quant != 0)) continue;
                    Measure(w, h, motion, label, authentic, quant, false);
                }
            }

            if (motion != '\0') Measure(w, h, motion, label, true, 6, true);
        }

        Console.WriteLine();
    }

    return 0;
}

Console.WriteLine($"\n{scene.Edition} data · {scene.Levels.Count} levels · '{scene.Map.Name}'\n");

// --- the picture -------------------------------------------------------------------------------------------------
var frame = ConsoleSnapshot.Render(shell.Root, W, H);
var lit = 0;
var glyphs = new HashSet<char>();
for (var y = 0; y < H; y++)
    for (var x = 0; x < W; x++)
    {
        var ch = frame[x, y].Character;
        if (ch.Content is { } c) { glyphs.Add(c); if (ch.Foreground is not null) lit++; }
    }

Check("the viewport draws", lit > W * H / 2, $"{lit} lit cells of {W * H}");
Check("it draws with half-blocks", glyphs.Contains('\u2580'), $"{glyphs.Count} distinct glyphs");
Check("the frame has real colour variety", view.LastCost.Colors is > 4 and < 200, $"{view.LastCost.Colors} colours");

// --- movement, through the root layout -----------------------------------------------------------------------------
static void Send(ILayout root, char c) =>
    root.OnInput(new UI.InputEventArgs(new ConsoleGUI.Input.InputEvent(UI.HotKeys.Char(c))));

var (x0, y0) = (scene.X, scene.Y);
for (var i = 0; i < 10; i++) { Send(shell.Root, 'w'); view.DrawFrame(); }
var moved = Math.Abs(scene.X - x0) + Math.Abs(scene.Y - y0);
Check("w walks the player forward", moved > 0.5, $"moved {moved:F2} tiles from ({x0:F1},{y0:F1})");

var bearing0 = scene.Bearing;
for (var i = 0; i < 10; i++) { Send(shell.Root, 'd'); view.DrawFrame(); }
var turned = Math.Abs(((scene.Bearing - bearing0 + 540) % 360) - 180);
Check("d turns the player", turned < 175, $"turned {180 - turned:F0}° from {bearing0:F0}°");

// A press must not move the player forever: the sustain window has to lapse.
var (xs, ys) = (scene.X, scene.Y);
Thread.Sleep(400);
view.DrawFrame();
Check("movement stops when the key stops repeating",
    Math.Abs(scene.X - xs) + Math.Abs(scene.Y - ys) < 0.01, "sustain window lapsed");

// --- the opposite key must REVERSE the axis, not cancel it ---------------------------------------------------------
// The bug this replaced: forward and back held independent sustain windows, so a press on one while the other was
// still open summed to exactly zero. It read as heavy input lag rather than as a stuck key.
static double Delta(double from, double to) => ((to - from + 540) % 360) - 180;

scene.Restart();
for (var i = 0; i < 6; i++) { Send(shell.Root, 'd'); view.DrawFrame(); }
var bRight = scene.Bearing;
Send(shell.Root, 'a');
view.DrawFrame();
var reversal = Delta(bRight, scene.Bearing);
Check("a reverses an in-flight right turn on the very next frame", reversal < -0.5,
    $"{reversal:F2}° (negative = turned left)");

for (var i = 0; i < 6; i++) { Send(shell.Root, 'w'); view.DrawFrame(); }
var (xf, yf) = (scene.X, scene.Y);
var headingX = scene.DirectionX;
var headingY = scene.DirectionY;
Send(shell.Root, 's');
view.DrawFrame();
var along = ((scene.X - xf) * headingX) + ((scene.Y - yf) * headingY);
Check("s reverses an in-flight forward walk on the very next frame", along < -0.001,
    $"{along:F4} tiles along the heading (negative = backwards)");

// --- and different axes must still compose --------------------------------------------------------------------------
scene.Restart();
var (xc, yc) = (scene.X, scene.Y);
var bc = scene.Bearing;
Send(shell.Root, 'w');
Send(shell.Root, 'd');
view.DrawFrame();
Check("w and d in the same frame both move and turn",
    Math.Abs(scene.X - xc) + Math.Abs(scene.Y - yc) > 0.001 && Math.Abs(Delta(bc, scene.Bearing)) > 0.1,
    $"moved {Math.Abs(scene.X - xc) + Math.Abs(scene.Y - yc):F3}, turned {Delta(bc, scene.Bearing):F2}°");

// --- a single tap coasts to a stop rather than running on -----------------------------------------------------------
Thread.Sleep(400);           // let the repeat stream lapse, so the next press is a genuinely fresh one
scene.Restart();
var (xt, yt) = (scene.X, scene.Y);
Send(shell.Root, 'w');
for (var i = 0; i < 50; i++) { view.DrawFrame(0.02); Thread.Sleep(20); }
var tap = Math.Abs(scene.X - xt) + Math.Abs(scene.Y - yt);
var (xe, ye) = (scene.X, scene.Y);
view.DrawFrame(0.02);
Check("a single tap moves a bounded distance and stops", tap is > 0.15 and < 2.0,
    $"{tap:F2} tiles");
Check("and is fully stopped afterwards",
    Math.Abs(scene.X - xe) + Math.Abs(scene.Y - ye) < 1e-9, "no residual drift");

// --- collision -----------------------------------------------------------------------------------------------------
scene.Restart();
var wallHits = 0;
for (var i = 0; i < 400; i++)
{
    Send(shell.Root, 'w');
    view.DrawFrame(0.05);
    if (scene.Map.IsSolid((int)scene.X, (int)scene.Y)) wallHits++;
}

Check("walking never ends up inside a wall", wallHits == 0, $"{wallHits} frames inside solid geometry");

// --- levels --------------------------------------------------------------------------------------------------------
var first = scene.Map.Name;
Send(shell.Root, ']');
view.DrawFrame();
Check("] loads the next level", scene.Map.Name != first && scene.LevelIndex == 1, $"'{first}' -> '{scene.Map.Name}'");
Check("and the new level starts on its player marker", !scene.Map.IsSolid((int)scene.X, (int)scene.Y));

var every = true;
for (var i = 0; i < scene.Levels.Count; i++)
{
    scene.LoadLevel(i);
    view.DrawFrame();
    if (scene.Map.IsSolid((int)scene.X, (int)scene.Y) || view.LastCost.Colors < 4) { every = false; break; }
}

Check($"all {scene.Levels.Count} levels load, start clear and render", every, every ? null : $"failed at {scene.Map.Name}");

// --- the footer reports what is on screen ----------------------------------------------------------------------------
scene.LoadLevel(0);
view.DrawFrame();
var lines = ConsoleSnapshot.ToLines(ConsoleSnapshot.Render(shell.Root, W, H));
var footer = string.Join("\n", lines.TakeLast(2));
Check("the footer names the level", footer.Contains(scene.Map.Name), scene.Map.Name);
Check("the footer reports the run count", footer.Contains("runs"));
Check("the footer lists the movement keys", footer.Contains("w/s move") && footer.Contains("shift run"));

// --- the render toggles actually change the render ----------------------------------------------------------------
view.Renderer.QuantizeLevels = 0;
view.DrawFrame();
var full = view.LastCost;
view.Renderer.QuantizeLevels = 6;
view.DrawFrame();
var quantized = view.LastCost;
Check("quantizing cuts the run count", quantized.Runs < full.Runs * 0.8,
    $"{full.Runs} runs -> {quantized.Runs} ({100.0 * quantized.Runs / full.Runs:F0}%)");
Check("and cuts the colour count", quantized.Colors < full.Colors, $"{full.Colors} -> {quantized.Colors}");

view.Renderer.AuthenticFov = false;
view.DrawFrame();
var wide = view.LastCost.Runs;
view.Renderer.AuthenticFov = true;
view.DrawFrame();
Check("the wide fov costs more runs than the authentic one", wide > view.LastCost.Runs,
    $"wide {wide} vs 66deg {view.LastCost.Runs}");

// --- the sidebar ------------------------------------------------------------------------------------------------
// A panel whose content is wider than its column allowance can spill into its neighbour, and on a viewport made of
// half-blocks that is easy to miss by eye. Assert it directly: nothing but block glyphs and the frame left of the
// sidebar's first column.
static int Spill(ILayout root, int w, int h)
{
    var rows = ConsoleSnapshot.ToLines(ConsoleSnapshot.Render(root, w, h));
    var viewportWidth = w - Wolf3DSidebar.Columns;
    var bad = 0;
    for (var y = 2; y < h - 3; y++)   // row 1 is the viewport frame's own title
        foreach (var c in rows[y][..viewportWidth])
            if (char.IsLetterOrDigit(c) || c is '×' or '°' or '·') bad++;
    return bad;
}

foreach (var page in new[] { 0, 1 })
{
    shell.Sidebar.Tabs.SelectedIndex = page;
    view.DrawFrame();
    var spill = Spill(shell.Root, W, H);
    Check($"the sidebar's {(page == 0 ? "Display" : "Input")} tab stays inside its columns", spill == 0,
        spill == 0 ? null : $"{spill} stray glyphs in the viewport");
}

static string Panel(ILayout root, int w, int h) =>
    string.Join("\n", ConsoleSnapshot.ToLines(ConsoleSnapshot.Render(root, w, h)).Select(l => l[^Wolf3DSidebar.Columns..]));

shell.Sidebar.Tabs.SelectedIndex = 0;
view.DrawFrame();
var displayTab = Panel(shell.Root, W, H);
Check("the sidebar shows both tabs", displayTab.Contains("Display") && displayTab.Contains("Input"));
Check("the Display tab lists its knobs",
    displayTab.Contains("Quantize") && displayTab.Contains("Surface") && displayTab.Contains("Row filter")
        && displayTab.Contains("Authentic FOV"));
// The cost readout belongs to the footer alone -- it is true of the app rather than of a page, and the footer is
// the one thing that cannot be hidden. Assert both halves: the footer has it, the panel does not duplicate it.
// (Sliced above the footer rows, since the footer spans the full width and so ends inside the sidebar's columns.)
var displayBody = string.Join("\n", ConsoleSnapshot.ToLines(ConsoleSnapshot.Render(shell.Root, W, H))
    .SkipLast(2).Select(l => l[^Wolf3DSidebar.Columns..]));
// Scoped to the LIVE tokens ("runs", "fps") rather than to the word "colours": the Sampling section legitimately
// says "2 colours/cell", which is a static fact about the mode and not a per-frame readout.
Check("the frame cost is reported in the footer, not duplicated in the panel",
    !displayBody.Contains("runs") && !displayBody.Contains("fps") && footer.Contains("runs"));

shell.Sidebar.Tabs.SelectedIndex = 1;
view.DrawFrame();
var inputTab = Panel(shell.Root, W, H);
Check("the Input tab lists the key-handling knobs",
    inputTab.Contains("First press") && inputTab.Contains("Coast") && inputTab.Contains("Repeat gap"),
    inputTab.Contains("First press") ? null : "missing First press");
Check("and the speed knobs", inputTab.Contains("Walk") && inputTab.Contains("Turn"));
Check("and shows the measured repeat interval", inputTab.Contains("Repeat:"));

// State -> widget: a change made anywhere must show up in the panel.
shell.Tuning.CoastSeconds = 0.5;
view.DrawFrame();
Check("a tuning change reaches the slider readout", Panel(shell.Root, W, H).Contains("0.50"),
    "CoastSeconds = 0.5");

shell.Tuning.Reset();
view.DrawFrame();
Check("Reset restores the defaults", Math.Abs(shell.Tuning.CoastSeconds - Wolf3DTuning.DefaultCoastSeconds) < 1e-9 &&
                                     Panel(shell.Root, W, H).Contains("0.22"));

// The knobs must actually reach the behaviour, not just the display.
shell.Tuning.CoastSeconds = 0.0;          // coasting off => a tap stops the moment its window lapses
shell.Tuning.FirstPressMs = 60;
Thread.Sleep(400);
scene.Restart();
var (xq, yq) = (scene.X, scene.Y);
Send(shell.Root, 'w');
for (var i = 0; i < 40; i++) { view.DrawFrame(0.02); Thread.Sleep(20); }
var snappy = Math.Abs(scene.X - xq) + Math.Abs(scene.Y - yq);

shell.Tuning.Reset();
Thread.Sleep(400);
scene.Restart();
var (xd, yd) = (scene.X, scene.Y);
Send(shell.Root, 'w');
for (var i = 0; i < 40; i++) { view.DrawFrame(0.02); Thread.Sleep(20); }
var relaxed = Math.Abs(scene.X - xd) + Math.Abs(scene.Y - yd);
Check("the Input knobs change the actual movement", snappy < relaxed * 0.6,
    $"tap travels {snappy:F2} tiles tightened vs {relaxed:F2} at defaults");

// Display knobs likewise.
shell.Sidebar.Tabs.SelectedIndex = 0;
view.Renderer.QuantizeLevels = 0;
view.DrawFrame();
Check("the Display tab follows a renderer change made elsewhere",
    Panel(shell.Root, W, H).Contains("off — full palette"));
view.Renderer.QuantizeLevels = Wolf3DRenderer.DefaultQuantizeLevels;

// Collapse must preserve state, not rebuild the panel.
shell.Tuning.WalkSpeed = 7.5;
Wolf3DShell.ToggleSidebar(shell.Sidebar);
view.DrawFrame();
_ = ConsoleSnapshot.ToText(shell.Root, W, H);
Check("u collapses the sidebar", shell.Sidebar.Width == 0);
Wolf3DShell.ToggleSidebar(shell.Sidebar);
shell.Sidebar.Tabs.SelectedIndex = 1;      // Walk lives on the Input tab, not the Display one
view.DrawFrame();
Check("and restoring it keeps the knobs where they were",
    shell.Sidebar.Width == Wolf3DSidebar.Columns && Math.Abs(shell.Tuning.WalkSpeed - 7.5) < 1e-9 &&
    Panel(shell.Root, W, H).Contains("7.5"),
    $"width {shell.Sidebar.Width}, walk {shell.Tuning.WalkSpeed}");
shell.Tuning.Reset();

// --- the movement pad ------------------------------------------------------------------------------------------
// The claim is that a pad click IS a key tap, not something resembling one: both go through Wolf3DView.Send and
// nothing synthesises a keystroke. Test the command API, then the same thing through a REAL mouse click on the
// rendered button, which is the only way to know the wiring survives the layout.
shell.Sidebar.Tabs.SelectedIndex = 1;
view.DrawFrame();

Thread.Sleep(400);
scene.Restart();
var (xp, yp) = (scene.X, scene.Y);
view.Send(Wolf3DCommand.Forward);
view.DrawFrame();
Check("Send(Forward) moves the player", Math.Abs(scene.X - xp) + Math.Abs(scene.Y - yp) > 0.001,
    $"{Math.Abs(scene.X - xp) + Math.Abs(scene.Y - yp):F3} tiles");

var bp = scene.Bearing;
view.Send(Wolf3DCommand.TurnRight);
view.DrawFrame();
Check("Send(TurnRight) turns the player", Delta(bp, scene.Bearing) > 0.1, $"{Delta(bp, scene.Bearing):F2}°");

// Locate the pad's forward button on screen and click it for real.
var padRows = ConsoleSnapshot.ToLines(ConsoleSnapshot.Render(shell.Root, W, H));
var found = (X: -1, Y: -1);
for (var y = 0; y < H && found.X < 0; y++)
{
    var x = padRows[y].IndexOf('▲');       // the pad's forward arrow
    if (x >= W - Wolf3DSidebar.Columns) found = (x, y);
}

Check("the pad's forward button is on screen", found.X >= 0, $"at {found.X},{found.Y}");

if (found.X >= 0)
{
    Thread.Sleep(400);
    scene.Restart();
    var (xClick, yClick) = (scene.X, scene.Y);
    var clicked = ConsoleSnapshot.Click(ConsoleSnapshot.Render(shell.Root, W, H), found.X, found.Y);
    view.DrawFrame();
    Check("clicking it walks the player", clicked && Math.Abs(scene.X - xClick) + Math.Abs(scene.Y - yClick) > 0.001,
        $"routed={clicked}, moved {Math.Abs(scene.X - xClick) + Math.Abs(scene.Y - yClick):F3} tiles");

    // The trap the sandbox's pad documents: focus left on the button kills the movement keys.
    Check("and leaves focus on the viewport, so the keys still work", view.IsFocused);
}

// --- Open ----------------------------------------------------------------------------------------------------
// Walk at the level's nearest door until it is within reach, then operate it and watch the panel slide. Asserting
// on OpenAmount rather than on "the call returned true" — a door that reports itself operated but never animates
// would pass the weaker check and be an invisible wall on screen.
scene.LoadLevel(0);
var target = scene.Doors.Items
    .OrderBy(d => Math.Abs(d.X + 0.5 - scene.X) + Math.Abs(d.Y + 0.5 - scene.Y))
    .FirstOrDefault();

Check("the level has doors to operate", target is not null, $"{scene.Doors.Items.Count} doors");

if (target is not null)
{
    // Aim at it and close the distance; the walk is deliberate rather than pathfound, so allow a generous budget.
    for (var i = 0; i < 600 && Math.Abs(target.X + 0.5 - scene.X) + Math.Abs(target.Y + 0.5 - scene.Y) > 1.0; i++)
    {
        var dx = target.X + 0.5 - scene.X;
        var dy = target.Y + 0.5 - scene.Y;
        var want = Math.Atan2(dx, -dy);
        var have = Math.Atan2(scene.DirectionX, -scene.DirectionY);
        var turnBy = ((want - have + (3 * Math.PI)) % (2 * Math.PI)) - Math.PI;
        scene.Turn(Math.Clamp(turnBy, -0.15, 0.15));
        if (Math.Abs(turnBy) < 0.3) scene.Move(0.05, 0);
    }

    var reached = Math.Abs(target.X + 0.5 - scene.X) + Math.Abs(target.Y + 0.5 - scene.Y);
    Check("the player can reach a door", reached <= 1.5, $"{reached:F2} tiles away");

    var before = target.OpenAmount;
    view.Send(Wolf3DCommand.Open);
    for (var i = 0; i < 60; i++) view.DrawFrame(0.05);
    Check("Open slides the door open", target.OpenAmount > before + 0.5,
        $"OpenAmount {before:F2} -> {target.OpenAmount:F2}");

    // And the doorway must become walkable — a door tile reads as solid in plane zero, so this is a real trap.
    // Drive Move DIRECTLY rather than through a command. What is under test is the collision rule — a door tile
    // reads as solid in plane zero, so an open door stays an invisible wall unless it is asked about first — and
    // routing through the input model would fold in sustain-window timing that has nothing to do with it. (It also
    // drifted the player off-target here: the axes were still live from the pad checks above, and the frames spent
    // animating the door carried them a tile past the doorway.)
    var doorCentreX = target.X + 0.5;
    var doorCentreY = target.Y + 0.5;
    var steps = 0;
    for (; steps < 200 && Math.Abs(doorCentreX - scene.X) + Math.Abs(doorCentreY - scene.Y) > 0.15; steps++)
    {
        var want = Math.Atan2(doorCentreX - scene.X, -(doorCentreY - scene.Y));
        var have = Math.Atan2(scene.DirectionX, -scene.DirectionY);
        scene.Turn(Math.Clamp(((want - have + (3 * Math.PI)) % (2 * Math.PI)) - Math.PI, -0.4, 0.4));
        scene.Move(0.04, 0);
    }

    var onDoor = Math.Abs(doorCentreX - scene.X) + Math.Abs(doorCentreY - scene.Y);
    Check("and the open doorway is walkable", onDoor <= 0.15,
        $"reached the doorway to within {onDoor:F2} tiles in {steps} steps");
}

// --- Fire ----------------------------------------------------------------------------------------------------
// The weapon must be ON SCREEN and must change while firing. Compare rendered cells rather than the frame index:
// an animation that advanced a counter without reaching the framebuffer would pass an index check.
static long Fingerprint(ConsoleBuffer buffer, int w, int h)
{
    // Bottom-centre of the viewport, where the weapon sits.
    long hash = 17;
    for (var y = h - 14; y < h - 3; y++)
        for (var x = (w / 2) - 12; x < (w / 2) + 12; x++)
        {
            var ch = buffer[x, y].Character;
            hash = (hash * 31) + (ch.Foreground?.Red ?? 0) + ((ch.Foreground?.Green ?? 0) << 8);
        }

    return hash;
}

// From a FRESH level, so nothing in the scene is animating: LoadLevel rebuilds the doors from the map with every
// one shut. Without that the "returns to rest" comparison picks up the door from the test above still sliding, and
// fails for a reason that has nothing to do with the weapon.
scene.LoadLevel(0);
Thread.Sleep(400);            // let any movement axis lapse too
view.Renderer.DrawWeapon = true;
for (var i = 0; i < 5; i++) view.DrawFrame(0.05);
var rest = Fingerprint(ConsoleSnapshot.Render(shell.Root, W, H), W, H);

view.Renderer.DrawWeapon = false;
view.DrawFrame();
var noWeapon = Fingerprint(ConsoleSnapshot.Render(shell.Root, W, H), W, H);
Check("the weapon is actually drawn", rest != noWeapon);

view.Renderer.DrawWeapon = true;
view.Send(Wolf3DCommand.Fire);
view.DrawFrame(0.05);
var firing = Fingerprint(ConsoleSnapshot.Render(shell.Root, W, H), W, H);
Check("Fire changes the weapon on screen", firing != rest);

for (var i = 0; i < 20; i++) view.DrawFrame(0.05);
var settled = Fingerprint(ConsoleSnapshot.Render(shell.Root, W, H), W, H);
Check("and it returns to rest afterwards", settled == rest);

// --- sprite aspect across surface modes --------------------------------------------------------------------------
// A sprite must occupy the same PHYSICAL width whichever surface mode is on. The vendored projector computes one
// RenderedSize and uses it for both axes, so it assumes a square framebuffer pixel — which quadrant sampling
// silently breaks, halving every sprite's width while leaving the walls (drawn per column) correct.
static int WeaponWidthCells(Wolf3DShell.Shell s, int w, int h)
{
    s.View.Renderer.DrawWeapon = false;
    s.View.DrawFrame();
    var without = ConsoleSnapshot.Render(s.Root, w, h);
    s.View.Renderer.DrawWeapon = true;
    s.View.DrawFrame();
    var with = ConsoleSnapshot.Render(s.Root, w, h);

    int left = int.MaxValue, right = int.MinValue;
    for (var y = 1; y < h - 2; y++)
        for (var x = 0; x < w - Wolf3DSidebar.Columns; x++)
        {
            var a = with[x, y].Character;
            var b = without[x, y].Character;
            if (a.Foreground?.Red == b.Foreground?.Red && a.Foreground?.Green == b.Foreground?.Green &&
                a.Background?.Red == b.Background?.Red) continue;
            left = Math.Min(left, x);
            right = Math.Max(right, x);
        }

    return right < left ? 0 : right - left + 1;
}

scene.LoadLevel(0);
Thread.Sleep(400);
view.Sampling = SurfaceMode.HalfBlock;
var halfWidth = WeaponWidthCells(shell, W, H);
view.Sampling = SurfaceMode.Quadrant;
var quadWidth = WeaponWidthCells(shell, W, H);
view.Sampling = SurfaceMode.HalfBlock;

Check("the weapon has a measurable width in both modes", halfWidth > 10 && quadWidth > 10,
    $"half {halfWidth} cells, quadrant {quadWidth} cells");
Check("and the same width in both — sprites are not squeezed by quadrant sampling",
    Math.Abs(halfWidth - quadWidth) <= Math.Max(2, halfWidth / 10),
    $"half {halfWidth} vs quadrant {quadWidth} cells");

// --- the status bar ------------------------------------------------------------------------------------------------
// Read off the SCREEN, not off the properties: the bar's whole risk is that it scales to something unreadable, and
// every property can be right while the picture is mush.
shell.Sidebar.Tabs.SelectedIndex = 0;
for (var pass = 0; pass < 2; pass++) { _ = ConsoleSnapshot.ToText(shell.Root, W, H); shell.Hud.Reflow(); view.DrawFrame(); }

var hudRows = shell.Hud.ActualHeight;
Check("the status bar sizes itself from its width", hudRows == Wolf3DHud.RowsFor(shell.Hud.ActualWidth) && hudRows > 3,
    $"{shell.Hud.ActualWidth}x{hudRows}");
// The floor of 3 is RowsFor's minimum, and the bar sat there for a while because it derived its height from a
// viewport width that was still 0. It looked deliberate. Assert it is past the floor, not merely non-zero.
Check("and is past the floor a stale width would pin it at", hudRows >= 8, $"{hudRows} rows");

var hudFrame = ConsoleSnapshot.Render(shell.Root, W, H);
var hudLit = 0;
var hudGlyphs = new HashSet<char>();
for (var y = H - 2 - hudRows; y < H - 2; y++)
    for (var x = 0; x < shell.Hud.ActualWidth; x++)
    {
        var ch = hudFrame[x, y].Character;
        if (ch.Foreground is not null) hudLit++;
        if (ch.Content is { } c) hudGlyphs.Add(c);
    }

Check("the bar draws", hudLit > shell.Hud.ActualWidth * hudRows / 2, $"{hudLit} lit cells");
// What the bar is drawn WITH, in each mode, read off the screen. Its labels are unreadable without quadrant
// sampling, so "the setting reached the property" is not the interesting assertion -- "the glyphs changed" is.
HashSet<char> BarGlyphs()
{
    view.DrawFrame();
    var frame = ConsoleSnapshot.Render(shell.Root, W, H);
    var glyphs = new HashSet<char>();
    for (var y = H - 2 - shell.Hud.ActualHeight; y < H - 2; y++)
        for (var x = 0; x < shell.Hud.ActualWidth; x++)
            if (frame[x, y].Character.Content is { } c) glyphs.Add(c);
    return glyphs;
}

view.Sampling = SurfaceMode.Quadrant;
Check("the bar draws with quadrant glyphs under quadrant sampling",
    BarGlyphs().Overlaps("▘▝▖▗▌▐▞▚▛▜▙▟▄█"), "found quadrant glyphs");
view.Sampling = SurfaceMode.HalfBlock;
Check("and with half blocks alone under half block",
    !BarGlyphs().Overlaps("▘▝▖▗▌▐▞▚▛▜▙▟▄"), "no quadrant glyphs");
view.Sampling = SurfaceMode.HalfBlock;

// The bar must FOLLOW the viewport's display toggles. It shipped ignoring both, so a user pressing 2 or 1 saw a
// third of the screen not respond -- and asserting the properties would not have caught it, since the bar's own
// properties were perfectly self-consistent while nothing ever wrote to them.
view.Sampling = SurfaceMode.HalfBlock;
view.DrawFrame();
Check("the bar follows the viewport's Surface setting", !shell.Hud.QuadrantSampling, "half block reached the bar");
view.Sampling = SurfaceMode.Quadrant;
view.DrawFrame();
Check("and back", shell.Hud.QuadrantSampling, "quadrant reached the bar");

var quantBefore = shell.Hud.LastColors;
view.Renderer.QuantizeLevels = 0;
view.DrawFrame();
var quantOff = shell.Hud.LastColors;
view.Renderer.QuantizeLevels = Wolf3DRenderer.DefaultQuantizeLevels;
view.DrawFrame();
Check("and its Quantize setting, which changes the bar's colour count",
    quantOff != quantBefore && shell.Hud.LastColors == quantBefore,
    $"{quantBefore} colours quantised, {quantOff} at full palette");
Check("and its colours reach the readout", shell.Hud.LastColors is > 20 and < 200, $"{shell.Hud.LastColors} colours");

// Per ROW, not just in total, and this is the check that earns its keep. The bar was twice drawn at a size the
// surface had already left behind -- once leaving its top four rows empty, once scaled too tall and clipped -- and
// a whole-bar lit-cell count passed both times. "Ink" is anything that is not the bar's dark teal surround, so a
// row of pure background reads as 0 and only the picture's own one-row top border is allowed to.
var hudTop = H - 2 - hudRows;
var blankRows = 0;
for (var y = hudTop; y < H - 2; y++)
{
    var ink = 0;
    for (var x = 0; x < shell.Hud.ActualWidth; x++)
    {
        var c = hudFrame[x, y].Character.Foreground;
        if (c is { } f && !(f.Red < 20 && f.Green is > 40 and < 90 && f.Blue is > 40 and < 90)) ink++;
    }

    if (ink < shell.Hud.ActualWidth / 2) blankRows++;
}

Check("and fills its rows rather than being drawn at a stale size", blankRows <= 1,
    $"{blankRows} of {hudRows} rows are background");

// The bar's height follows the terminal's width, so a ROUND TRIP is the interesting case: maximize, shrink,
// maximize. Every size below was correct on the way down and wrong on the way back up, and one of them crashed
// outright -- none of which a single-size check can see. `resize` prints the same walk as a table.
foreach (var (rw, rh) in new[] { (200, 52), (120, 32), (200, 52), (90, 26), (200, 52) })
{
    for (var settleFrame = 0; settleFrame < 6; settleFrame++)
    {
        _ = ConsoleSnapshot.ToText(shell.Root, rw, rh);
        shell.Hud.Reflow();
        view.DrawFrame();
    }

    var want = Wolf3DHud.RowsFor(shell.Hud.ActualWidth);
    Check($"the bar is right at {rw}x{rh}", shell.Hud.ActualHeight == want,
        $"{shell.Hud.ActualHeight} rows, wanted {want} for {shell.Hud.ActualWidth} columns");
}

// Back to the size the rest of the checks assume.
for (var settleFrame = 0; settleFrame < 2; settleFrame++)
{
    _ = ConsoleSnapshot.ToText(shell.Root, W, H);
    shell.Hud.Reflow();
    view.DrawFrame();
}

// --- the row filter ----------------------------------------------------------------------------------------------
// Compared as PICTURES rather than by asserting the property, because the filter reduces rows that only exist under
// quadrant sampling: reading RowFilter back would pass just as happily if the blit ignored it.
string Picture(SurfaceMode sampling, RowFilter filter)
{
    view.Sampling = sampling;
    view.Renderer.RowFilter = filter;
    _ = ConsoleSnapshot.ToText(shell.Root, W, H);
    view.DrawFrame();
    var buffer = ConsoleSnapshot.Render(shell.Root, W, H);
    var sb = new System.Text.StringBuilder();
    // The VIEWPORT only, not the whole screen. The footer carries a live frames-per-second readout, so hashing
    // everything makes this compare the clock as well as the filter -- it passed until a second DrawFrame per call
    // was enough to tick fps over, then failed for a reason that has nothing to do with row filtering.
    for (var y = 0; y < view.ActualHeight; y++)
        for (var x = 0; x < view.ActualWidth; x++)
        {
            var ch = buffer[x, y].Character;
            sb.Append(ch.Content).Append(ch.Foreground).Append(ch.Background);
        }

    return sb.ToString();
}

Check("the row filter defaults to Box", view.Renderer.RowFilter == RowFilter.Box, $"{view.Renderer.RowFilter}");
Check("it changes the picture under quadrant sampling",
    Picture(SurfaceMode.Quadrant, RowFilter.Nearest) != Picture(SurfaceMode.Quadrant, RowFilter.Box),
    "nearest and box differ");
// The framebuffer is only taller than the surface when there are extra columns to compensate for, so under half
// block there is nothing to average and the two must be the same code path -- not merely similar.
Check("and is a no-op under half block, where there are no extra rows",
    Picture(SurfaceMode.HalfBlock, RowFilter.Nearest) == Picture(SurfaceMode.HalfBlock, RowFilter.Box),
    "identical frames");
// The dial, driven the way a user drives it: through the Select, not by setting the property it writes to.
shell.Sidebar.Tabs.SelectedIndex = 0;
view.Sampling = SurfaceMode.Quadrant;
shell.Sidebar.Refresh();
view.DrawFrame();
var rowsDial = shell.Sidebar.Display.RowFilterDial;
Check("the Rows dial is enabled under quadrant sampling", rowsDial.Enabled);
Check("and reads back the renderer's filter", rowsDial.SelectedIndex == (int)view.Renderer.RowFilter,
    $"dial {rowsDial.SelectedIndex}, renderer {view.Renderer.RowFilter}");

rowsDial.SelectedIndex = (int)RowFilter.Nearest;
view.DrawFrame();
Check("and changing it reaches the renderer", view.Renderer.RowFilter == RowFilter.Nearest,
    $"{view.Renderer.RowFilter}");
rowsDial.SelectedIndex = (int)RowFilter.Box;
view.DrawFrame();

// Greyed out rather than merely ineffective: under half block there are no extra rows, and a live-looking control
// that does nothing is the worse of the two failures.
view.Sampling = SurfaceMode.HalfBlock;
shell.Sidebar.Refresh();
view.DrawFrame();
Check("and greys out under half block, where it has nothing to do", !rowsDial.Enabled);

view.Sampling = SurfaceMode.HalfBlock;
view.Renderer.RowFilter = RowFilter.Box;

Console.WriteLine($"\n{(failures == 0 ? "all checks passed" : $"{failures} FAILED")}\n");
return failures == 0 ? 0 : 1;


// `quiet` discards the row. The FIRST Measure in a process reports a wildly inflated paint and emit -- medians of
// 2-12 ms against 0.4-2 ms for every call after it -- because ConsoleManager's first real Setup/Draw pays for the
// whole render and emit stack at once, and 15 in-loop warmup frames do not cover it. It is not the configuration
// on that row being slow, it is being first, and reading it as a result is how you conclude that whichever option
// you happened to measure first is the expensive one. So burn one throwaway run before measuring anything.
void Measure(int w, int h, char motion, string label, bool authentic, int quant, bool quad,
    RowFilter filter = RowFilter.Nearest, bool quiet = false)
{
    var s = new Wolf3DScene(GameData);
    using var sh = Wolf3DShell.Build(s);
    sh.View.Renderer.AuthenticFov = authentic;
    sh.View.Renderer.QuantizeLevels = quant;
    sh.View.Sampling = quad ? SurfaceMode.Quadrant : SurfaceMode.HalfBlock;
    sh.View.Renderer.RowFilter = filter;

    long frameBytes = 0;
    ConsoleGUI.ConsoleManager.AnsiEnabled = true;
    ConsoleGUI.ConsoleManager.AnsiOutput = sb => { frameBytes += sb.ToString()!.Length; return Task.CompletedTask; };
    ConsoleGUI.ConsoleManager.Console = new NullConsole { Size = new ConsoleGUI.Space.Size(w, h) };
    ConsoleGUI.ConsoleManager.Setup();
    ConsoleGUI.ConsoleManager.Content = sh.Root.CControl;
    UI.PaintFrame();
    ConsoleGUI.ConsoleManager.Draw();
    sh.View.Focus();

    const int Warmup = 15, N = 90;
    List<double> draws = [], paints = [], emits = [];
    List<long> bytes = [];
    List<int> runs = [];
    var sw = new System.Diagnostics.Stopwatch();
    for (var i = 1; i <= Warmup + N; i++)
    {
        if (motion != '\0') Send(sh.Root, motion);
        // A gentle drift, or the walk runs into the first wall it meets and every measured frame after that is
        // a stationary one -- which reads as "walking is free" when it is the opposite.
        if (label == "walk" && i % 3 == 0) Send(sh.Root, 'd');
        sw.Restart(); sh.View.DrawFrame(1.0 / 30.0); sw.Stop();
        var d = sw.Elapsed.TotalMicroseconds;
        sw.Restart(); UI.PaintFrame(); sw.Stop();
        var p = sw.Elapsed.TotalMicroseconds;
        frameBytes = 0;
        sw.Restart(); ConsoleGUI.ConsoleManager.Draw(); sw.Stop();
        ConsoleGUI.ConsoleManager.OutputIdle.GetAwaiter().GetResult();
        var e = sw.Elapsed.TotalMicroseconds;
        if (i <= Warmup) continue;
        draws.Add(d); paints.Add(p); emits.Add(e); bytes.Add(frameBytes); runs.Add(sh.View.LastCost.Runs);
    }

    if (quiet) return;

    draws.Sort(); paints.Sort(); emits.Sort(); bytes.Sort(); runs.Sort();
    var total = draws[N / 2] + paints[N / 2] + emits[N / 2];
    Console.WriteLine(
        $"  {w}x{h,-4} {label,7}  {(authentic ? "66deg" : "wide"),-5}  {(quant == 0 ? "none" : quant + "/ch"),6}  " +
        $"{(quad ? "on" : "off"),-4}  {(quad ? filter.ToString() : "-"),-8}  {draws[N / 2],6:F0}us {paints[N / 2],6:F0}us {emits[N / 2],6:F0}us " +
        $"{total,7:F0}us  {bytes[N / 2],9:N0} B  {runs[N / 2],6:N0}");
}

internal sealed class NullConsole : ConsoleGUI.Api.IConsole
{
    public ConsoleGUI.Space.Size Size { get; set; }
    public bool KeyAvailable => false;
    public void Initialize() { }
    public void OnRefresh() { }
    public void Write(ConsoleGUI.Space.Position position, in ConsoleGUI.Data.Character character) { }
    public ConsoleKeyInfo ReadKey() => throw new NotSupportedException();
}
