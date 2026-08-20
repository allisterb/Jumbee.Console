#nullable enable

using System.IO;

using Wolfenshine.Resources;

using Jumbee.Console;
using Jumbee.Console.Wolf3DDemo;
using System.Reflection;

// --- Jumbee.Console Wolfenstein 3D walkthrough -------------------------------------------------------------------
// A static scene from the original 1992 game, rendered in the terminal: real maps, real wall textures, real
// scenery sprites, walked with a real raycaster. No enemies, no doors opening, no weapons, no sound.
//
// The engine under `engine/` is Wolfenshine (github.com/deanthecoder/Wolfenshine), vendored unmodified -- its
// raycaster and software renderer write into an RGBA framebuffer with no idea what a terminal is. Everything in
// this directory is the ~350 lines that put that framebuffer on a HalfBlockSurface, which carries two independently
// coloured sub-pixels per character cell. At 200x50 cells that is 200x100 square pixels: the exact 2:1 aspect of
// the original's 320x160 3D view, so the picture is a uniform downscale of the real thing.
//
// Game data is not distributed with this demo -- see README.md.

var dataDirectory = args.FirstOrDefault(a => !a.StartsWith('-')) ?? Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!, "GameData");
var fps = 30;
if (args.FirstOrDefault(a => a.StartsWith("--fps=")) is { } f && int.TryParse(f[6..], out var parsed))
    fps = Math.Clamp(parsed, 5, 60);

Wolf3DScene scene;
try
{
    scene = new Wolf3DScene(dataDirectory);
}
catch (WolfensteinDataNotFoundException exception)
{
    // The overwhelmingly likely failure, and it is a setup problem rather than a bug -- so it gets an explanation
    // and the fix, not a stack trace.
    Console.Error.WriteLine($"No Wolfenstein 3D game data in '{Path.GetFullPath(dataDirectory)}'.");
    Console.Error.WriteLine();
    Console.Error.WriteLine(exception.Message);
    Console.Error.WriteLine();
    Console.Error.WriteLine("Put the eight shareware .WL1 files (or the full game's .WL6 files) in that folder,");
    Console.Error.WriteLine("or pass the folder holding them as the first argument. See README.md.");
    return 1;
}

using var shell = Wolf3DShell.Build(scene, fps);
shell.View.Focus();

// A headless smoke check, as the examples browser, the IDE demo and the agent harness all have. Worth the dozen
// lines: it is the only way to tell whether the demo actually WORKS somewhere there is no terminal to look at --
// a container build, or CI. Without it the answer to "does the image run?" is "start it and see", which under
// `docker run` with no TTY means a full-screen UI painting escape codes at a pipe until something times out.
if (args.Contains("--verify"))
{
    // Lay the tree out FIRST, then draw: the viewport sizes its pixel buffer from the control's actual size, so a
    // frame drawn before the first layout renders nothing at all and the check would pass on an empty screen.
    const int Width = 200, Height = 52;
    _ = Jumbee.Console.Snapshot.ConsoleSnapshot.Render(shell.Root, Width, Height);
    shell.View.DrawFrame();
    var buffer = Jumbee.Console.Snapshot.ConsoleSnapshot.Render(shell.Root, Width, Height);

    // Assert against the COMPOSITED CELLS rather than the renderer's own counters — those would happily report a
    // healthy frame that never reached the screen.
    var lit = 0;
    var halfBlocks = 0;
    for (var y = 0; y < Height; y++)
        for (var x = 0; x < Width; x++)
        {
            var ch = buffer[x, y].Character;
            if (ch.Foreground is not null) lit++;
            if (ch.Content == '▀') halfBlocks++;
        }

    var ok = lit > Width * Height / 2 && halfBlocks > 1000;
    Console.WriteLine(ok
        ? $"PASS  Wolf3D verify — {scene.Edition} data, {scene.Levels.Count} levels, '{scene.Map.Name}' renders " +
          $"({lit} lit cells, {halfBlocks} half-blocks, {shell.View.LastCost.Runs} runs)."
        : $"FAIL  Wolf3D verify — '{scene.Map.Name}' drew {lit} lit cells and {halfBlocks} half-blocks.");
    return ok ? 0 : 1;
}

await UI.Start(shell.Root, width: 200, height: 52, fps: fps);
return 0;
