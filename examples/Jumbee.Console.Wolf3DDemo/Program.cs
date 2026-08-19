#nullable enable

using Jumbee.Console;
using Jumbee.Console.Wolf3DDemo;

using Wolfenshine.Resources;

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

var dataDirectory = args.FirstOrDefault(a => !a.StartsWith('-')) ?? "GameData";
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
await UI.Start(shell.Root, width: 200, height: 52, fps: fps);
return 0;
