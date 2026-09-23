# Wolf3D walkthrough harness

The headless harness the Wolf3D demo was built against: **49 checks**, and it has caught several shipped bugs —
the sprite-aspect break under quadrant sampling, the pad's focus trap, the input dead zone.

```bash
dotnet run --project tests/Jumbee.Console.Wolf3DDemo.Harness -c Release             # the 49 checks
dotnet run --project tests/Jumbee.Console.Wolf3DDemo.Harness -c Release -- surfaces # glyph-grid + quantiser measurements
dotnet run --project tests/Jumbee.Console.Wolf3DDemo.Harness -c Release -- perf     # ANSI bytes per frame
dotnet run --project tests/Jumbee.Console.Wolf3DDemo.Harness -c Release -- png out=DIR
dotnet run --project tests/Jumbee.Console.Wolf3DDemo.Harness -c Release -- rows     # buffer rows as text, to settle "is that spill?"
```

It needs the demo's game data present at `examples/Jumbee.Console.Wolf3DDemo/GameData` (see that folder's
README). The data is not redistributable and is gitignored, so this harness cannot run in a fresh checkout — it
**compiles** in one, which is what keeps it from rotting.

Like its sibling, it is an `Exe` with its own runner rather than an xunit project, and it is in
`src/Jumbee.Console.sln` so an ordinary solution build compiles it. It moved here from `docs/internal/scratch/wolf3d`
on 2026-09-21; see [the sandbox harness README](../Jumbee.Console.SandboxDemo.Harness/README.md) for why, and for
the failure mode that prompted it.

Paths come from [`RepoPaths.cs`](RepoPaths.cs) rather than the hard-coded `C:\Projects\Jumbee.Console` literals
they used to be. It is a 15-line duplicate of the sandbox harness's copy: two independent harnesses sharing one
linked source file is a coupling neither of them wants.

Two lessons from writing the checks that are worth keeping:

- **Test what reaches the screen, not what a counter says.** The `--verify` path and the sprite-aspect check both
  assert against composited cells; a frame drawn before the first layout renders nothing at all, and would have
  passed any check that trusted the renderer's own numbers.
- **Reset the scene between measurements.** The first bandwidth run left the camera buried in a wall by row three,
  and every row after that was measuring a static frame while looking like a real result.
