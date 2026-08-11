# 3D sandbox verification harness (parked)

The headless harness the 3D sandbox was built against. Parked here rather than left in a session scratchpad,
because it has caught two shipped bugs and several wrong assumptions, and **M5 should promote it into a real test
project** (`tests/Jumbee.Console.SandboxDemo.Tests` or similar).

It is not wired into the solution. To run it, drop these three files in a folder and `dotnet run -c Release`; the
`.csproj` compiles the demo's own sources (minus `Program.cs`) against `Jumbee.Console` and `Jumbee.Console.Snapshot`,
with a `$(Repo)` property at the top pointing at the repo root.

## Modes

| | |
|---|---|
| *(none)* | 63 behaviour checks — the default |
| `--perf [WxH]` | frame cost of every renderer over the real `ConsoleManager`: scene, paint, emit, ANSI bytes |
| `--png out=DIR [WxH]` | PNG of each renderer; add `viewer` for the model-viewer scene instead |
| `--solid` | ASCII luminance dump (weaker than `--png`; see the note below) |
| `--probe` | on-screen size of a launched body, frame by frame |
| `--load` | OBJ parse time per model in the reference directory |

## What it covers that a normal test would not

- **Keyboard routed through the ROOT LAYOUT**, the way the live loop routes it — not `UI.SendInput`, which takes a
  different path through `ControlFrame` and passes even when the app receives nothing. This is the check that
  caught the dead arrow keys.
- **Click-to-select through the real mouse-listener routing** (`ConsoleSnapshot.ToTextAfterClick`).
- **The rasteriser against analytic ground truth**: for sub-pixels showing bare floor, cast that pixel's ray,
  intersect `y = 0` in closed form, and compare with the z-buffer. Runs at 0.00% error, and exercises the
  projection, the screen mapping and the perspective-correct depth interpolation together.
- **The silhouette detector's central claim** — that a wholly planar neighbourhood never registers as an edge,
  however steeply the plane recedes. Measured at 0 false positives out of ~3,500 sub-pixels.
- **Colour read back from emitted ANSI** (`AnsiConsoleSnapshot`), so a selection highlight is verified as pixels
  rather than as internal state.
- **The `obj` path-resolution rules**, all of which are edge cases, via `ModelLibrary.Resolve`.

## Caveat carried forward

The `--solid` ASCII dump is kept only for quick structural checks. **Do not judge a render from it** — piping it
through a shell mangles `▀`, braille and `◆◇◈◊` into `?`, and it is very easy to mistake that for the renderer's
output. Use `--png`, which sets `FontFamily = "Cascadia Mono"` for glyph coverage.
