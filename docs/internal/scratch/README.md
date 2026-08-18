# 3D sandbox verification harness (parked)

The headless harness the 3D sandbox was built against. Parked here rather than left in a session scratchpad,
because it has caught two shipped bugs and several wrong assumptions, and **M5 should promote it into a real test
project** (`tests/Jumbee.Console.SandboxDemo.Tests` or similar).

It is not wired into the solution. To run it, drop these files in a folder and `dotnet run -c Release`; the
`.csproj` compiles the demo's own sources (minus `Program.cs`) against `Jumbee.Console` and `Jumbee.Console.Snapshot`,
with a `$(Repo)` property at the top pointing at the repo root.

## Modes

| | |
|---|---|
| *(none)* | 63 behaviour checks — the default |
| `--shell [viewer] [WxH]` | the M3 UI: layout, key↔widget agreement in both directions, sidebar toggle |
| `--switch [WxH]` | three real `UI.Start`/`UI.Stop` cycles over the real shells — the scene-switch path |
| `--perf [WxH]` | frame cost of every renderer over the real `ConsoleManager`: scene, paint, emit, ANSI bytes |
| `--png out=DIR [WxH]` | PNG of each renderer; add `viewer` for the model-viewer scene instead |
| `--solid` | ASCII luminance dump (weaker than `--png`; see the note below) |
| `--probe` | on-screen size of a launched body, frame by frame |
| `--load` | OBJ parse time per model in the reference directory |

`--shell` accepts `--png out=DIR` too, which is the only way to judge the sidebar beside a live viewport.

**`--shell` has a floor of 36 rows**, and it is the harness's rather than the app's. The sandbox sidebar scrolls now,
so at any height it is *reachable*; but most checks read the panel by finding its text in the rendered rows, and
below 36 the World section is off screen at the top of the scroll. Only the camera-pad check scrolls to what it is
looking for. The app itself is fine down there — 30 rows renders and scrolls correctly.

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
- **The only mode that runs the UI LOOP.** Everything else renders through `ConsoleSnapshot` with no loop at all, so
  `--switch` is where anything lifecycle-shaped shows up: a second `UI.Start` after a `Stop`, session state that
  outlives the session that made it, a background feed still posting after its control is gone. It counts frames
  through `SceneView.Drew` rather than asserting a method returned, because the question is whether a shell is
  *alive*. Note it runs with `isAnsiTerminal: false` — the ANSI renderer writes to stdout through
  `ConsoleManager.AnsiOutput` and never consults the `IConsole`, so a stub console silences nothing.
- **The real shell, not a rebuild of it.** `--shell` drives `SandboxShell`, the same assembly `Program.cs` uses —
  which matters precisely because keyboard routing differs between the root-layout path and a hand-built container.
  It also asserts the sidebar's central claim in both directions: press `v` and the drop-down follows; set a
  parameter and the slider's *on-screen readout* follows.

## Caveat carried forward

The `--solid` ASCII dump is kept only for quick structural checks. **Do not judge a render from it** — piping it
through a shell mangles `▀`, braille and `◆◇◈◊` into `?`, and it is very easy to mistake that for the renderer's
output. Use `--png`, which sets `FontFamily = "Cascadia Mono"` for glyph coverage.
