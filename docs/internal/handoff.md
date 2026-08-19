# Handoff — the 3D demo made public-ready (2026-08-17)

Living "where we are / what's next" note. Companion to [`eval-findings.md`](eval-findings.md), which is the full
backlog with evidence; this is the short version plus the operational context you'd otherwise have to rediscover.

## Next session starts here

**Working tree is clean. Everything below is committed**, ending at `7ef0016` "Fix bug in smoothing".

The session's theme was **getting the 3D sandbox ready to show in public** — a Reddit post and a recorded clip —
which turned out to mean far less rendering work than plumbing, packaging and bug-fixing. Nine commits:

| commit | what |
|---|---|
| `de29352`, `b37379f` | mesh thinning, control settings, renderable `Select` |
| `2464d4f` | two grab-gesture bugs + three sidebar buttons |
| `6212d36` | switching scenes without leaving the process *(the only library change: `UI.RestoreBuiltInHotKeys`)* |
| `f570062` | viewer `Scene` menu, `models/` folder resolution, scrolling sandbox sidebar |
| *(launchers/images)* | `3dsandbox` in `examples.sh` / `examples.cmd` / both Docker images |
| `9372746` | half-Lambert on by default in the sandbox |
| `fd3ca75` | sleep dimming removed, `ShadeLevels` slider |
| `7ef0016` | edge smoothing, and the `Line`-outline regression fix |

**The second stage of smoothing — quadrant-glyph AA — is done and it works**, unlike the first. `QuadrantSampling`
on `MeshRenderer`, an **AA** switch in both sidebars and an item in both Render menus, off by default. See
*Quadrant sampling* below for the numbers, the one place the plan was wrong about the cost, and the two things the
measurement itself got wrong before it got them right. It is **uncommitted**, along with a fix for the white-model
bug the user reported from play (see below), the handoff rewrite and `Recording the demos.md`.

**Next up is unclaimed.** The obvious candidates are M4 polish (colour modes, trails, scene presets — see *What this
session did* below) and open question 3, promoting the harness into a real test project, which is now overdue: it
is up to 97 + 34 checks and has caught six shipped bugs.

**New doc worth reading before recording anything:** [`Recording the demos.md`](Recording%20the%20demos.md) — the
measured WebP/GIF settings, why lossless beats lossy quality 100 by 2.7× on this content, and the two capture
decisions that outweigh every encoder setting.

### The 3D demo is now publicly reachable

- `./examples.sh 3dsandbox` (aliases `3d`, `sandbox`), `examples.cmd 3dsandbox`, `examples-aot.sh 3dsandbox`
- `docker run --rm -it jumbee-console 3dsandbox` and the same on `jumbee-console-aot`
- `3dsandbox obj` opens the model viewer; **Scene ▸ Switch** moves between the two without leaving the process
- Both images built and verified end to end; the slim AOT one publishes a 5.5 MB native binary and **Box3D's native
  P/Invoke steps correctly under NativeAOT**, which nobody had ever tried

### Rendering changes, and what each one actually taught

**Sleep dimming removed.** `Palette.For` drew a sleeping body at a third brightness to make the engine's sleep
behaviour visible. It went: a settled pile is the *common* case, so most of the scene sat dimmed most of the time,
and the one thing it degraded was the lighting — the whole point of the shaded renderer. It also read as a lighting
*bug* rather than as information, because bodies fall asleep on the same tick and dim together while the selected
one (which bypasses `Palette.For`) stays bright — indistinguishable from a light moving. Nothing is lost: the awake
count is in the footer and the `step` readout visibly drops when the solver stops working.

**`ShadeLevels` is a runtime dial** — on `MeshRenderer`, so both solid renderers have one; rounded and clamped to
[2, 24]; defaults 7 (shaded) / 5 (solid). `SceneView.ShadeLevels`/`SetShadeLevels` plus a slider in both
sidebars. Note it greys out under the *wireframe*, the opposite gating from Edges/Occlusion. It is the quality knob
*and* the largest performance lever, which used to argue for pinning it — the argument changed once the demo
started being recorded, where nothing waits on the terminal.

**Edge smoothing (cheap AA) — implemented, it disappointed, and it has since been removed entirely** (see *the smoothing pass was removed outright* below). `HalfBlockSurface.SmoothEdges` blends each edge
sub-pixel and its neighbours toward the local average, reusing what `DetectEdges` already marks, so cost tracks
silhouette *perimeter* not screen *area*. `ShadedRenderer.EdgeSmoothing` (0–1, default 0) drives it, **Smooth**
slider in both sidebars.

The measured result was not what the plan predicted. I expected the cost to be the colour count; it barely moved
(**93 → 99** distinct fg/bg pairs at full strength). The real limit is spatial: it blends **whole sub-pixels** where
coverage AA blends *within* one. On a sphere twelve sub-pixels across, a one-sub-pixel ring is ~8% of the diameter
per side, so **1.0 reads as erosion** — the body visibly shrinks and desaturates — and 0.3–0.4 merely softens the
staircase. It also cannot touch the checkerboard or the shade-band contours, which are *colour* boundaries and so
invisible to a depth-based detector; those are arguably the blockier things on screen.

**This is the evidence for doing quadrant glyphs next rather than supersampling.** A cell carries exactly two
colours and a silhouette is exactly a two-colour boundary, so a quadrant glyph (`▘▝▖▗`…) can place that boundary at
2×2 resolution *within* the cell at **zero colour cost** — spatial precision instead of smeared colour, which is
precisely what this experiment shows is missing. Picking the pattern needs per-quadrant coverage, so supersampling,
but only at cells the edge pass already flagged. Check quadrant-block font coverage (U+2596–259F) with the existing
multi-font snapshot test before committing.

Two mechanical notes, *both since undone with the pass itself*: `DetectEdges` stopped self-gating on `EdgeStyle`
(it took a `wanted` flag, since two consumers wanted the edge set) and `SmoothEdges` read from a copy so the result
did not depend on scan order. Frame cost with the pass **off** was unchanged within noise; the cost with it **on**
was never measured.

### Quadrant sampling — the second stage, and this one delivers

`HalfBlockSurface.QuadrantSampling` samples **twice per column** and composites each 2×2 block into whichever of the
sixteen quadrant glyphs best fits its four colours. Surfaced as `MeshRenderer.QuadrantSampling` (so **both** solid
renderers have it, like `ShadeLevels`), `SceneView.QuadrantSampling`/`SetQuadrantSampling`, an **AA** switch
in both sidebars (under Half-Lambert, with the post-processing settings) and an unshortcutted item in both Render menus.

**It came out simpler than the plan, and the simplification is the interesting part.** The plan said "picking the
pattern needs per-quadrant coverage, so supersampling, but only at cells the edge pass already flagged" — a second,
sparse rasterising pass. It needs neither. The compositor's whole job is a **two-means partition of four colours**:
seven candidate splits (a mask and its complement are one partition, and one group of four can never beat a split),
each scored by the colour spread it leaves inside its two groups. No detector, no flag, no second pass.

Three things fall out of that, and none were in the plan:

- **`▀` is one of the sixteen patterns**, so it wins whenever a block's structure really is horizontal. The pass can
  therefore only *add* resolution — there is no case where it trades the vertical resolution away, which was the
  worry that made `SilhouetteStyle.Glyph` a "genuine trade".
- **A block whose two columns agree needs no search at all.** That is every flat interior, so the partition runs
  along boundaries only and the compositing stays cheap despite being on the whole-screen path.
- **No detector means no blind spot.** `SmoothEdges` cannot see the checkerboard or the shade-band contours because
  they are colour boundaries and its detector reads depth. This gives them the same half-cell precision it gives a
  silhouette, and in the PNGs the near-field checkerboard diagonals are the most obviously improved thing on screen.

**Measured** (`--aa`, a 3-unit sphere against the sky at 120×40): the silhouette's placement error **halves**,
0.65 → **0.32** half-cells RMS, and **11 of 21** silhouette rows move to a boundary *inside* a cell where before
none could. Cost, at 200×50: shaded+ao **2.9 → 4.4 ms** and **18.1 → 25.7 KB** of ANSI a frame; solid
**1.7 → 2.4 ms** and **12.0 → 17.4 KB**. So ~1.5× a frame, not 2× — the fill doubles, the emit and paint do not.

**The one place the plan was wrong: "at zero colour cost" is half true, and the wrong half is the one that shows up
in a capture.** The *palette* genuinely does not grow — each group is represented by one of its own **members** (the
medoid), never their average, so every colour emitted is one the renderer already produced, and there is a check
asserting exactly that against the sub-pixel buffer. But emission still rises **42%**, because a boundary that now
falls *between* two columns makes a cell differ from its neighbour where the two used to coalesce. Colours and runs
are separate currencies and this spends the second one. Worth remembering before the next "free" idea: on this
medium, *anything* that adds detail is paid for in run length.

**Two lessons from the measurement, which was wrong twice before it was right.**

1. **The obvious jaggedness metrics cannot see antialiasing at all.** Total zigzag (Σ|second difference| along the
   silhouette) is nearly *invariant* here — an antialiased edge bends twice as often by half as much — and the first
   version, a median absolute bend, reported **0.00 for every case including the broken ones**. What works is
   *quantisation error*: how far each row's boundary sits from the midpoint of its two neighbours, RMS. A staircase
   can only place a boundary on whole cells, so a row that should sit half a cell along is a whole half-cell out.
2. **The scene has to be built for the measurement.** The `--aa` scene was a 0.6-unit sphere seen from 20 units; its
   left silhouette gave 18 *scattered* rows and every placement statistic came back empty — which reads as "no
   effect" rather than "nothing measured". It is now a 3-unit sphere at distance 11. Related: a "silhouette row" has
   to be *defined* as one where sky was crossed to reach the boundary, or every row the ground fills edge to edge
   reports a boundary in the first column — always an even one — and outnumbers the real silhouettes six to one.

**The font gate the plan asked for passed**, and is now a permanent test rather than a one-off:
[`SnapshotQuadrantFontTests`](../../tests/Jumbee.Console.Tests/SnapshotQuadrantFontTests.cs) renders each of the
sixteen patterns and requires its ink to land in the quadrants it names. All sixteen are covered by the default
snapshot font. Same failure shape as the Braille one — a missing glyph draws the *same box* for all sixteen, and no
text assertion can see it.

### And then the smoothing pass was removed outright

Once quadrant AA existed, the user's read on the first stage was that it *"didn't do much"* — which is what the
numbers had already said — so `ShadedRenderer.EdgeSmoothing`, `HalfBlockSurface.SmoothEdges`, its `blend` buffer,
`SceneView.EdgeSmoothing`/`SetEdgeSmoothing` and the **Smooth** slider in both sidebars are all gone. The remaining
toggle is relabelled **AA** (the menu item, **Antialiasing**); the property stays `QuadrantSampling`, which names
the mechanism rather than the feature and is still accurate.

**The measurement that decided it**, from `--aa` with both live at once — they are independent stages, so this was a
real question rather than a choice between two implementations of one thing:

| | distinct fg/bg pairs | placement error | rows inside a cell |
|---|---:|---:|---:|
| neither | 242 | 0.65 | 0 of 21 |
| quadrants | **277** | **0.32** | **11 of 21** |
| smooth 0.35 | 390 | 0.87 | 0 of 21 |
| both | 395 | 0.35 | 10 of 21 |

Adding the blend on top of quadrants costs **43% more distinct pairs and makes the placement slightly worse**. It
was removed rather than left at a default of zero, because two controls where one does nothing you can see is worse
than one — and a slider at 0.36 was exactly what the user had been running.

Two knock-ons. `DetectEdges` **self-gates on `EdgeStyle` again** — the `wanted` flag existed only because two
consumers could want the edge set, and the outline is now the only one. And the sidebar constants moved *back*:
`SpacedRows` 63 → **61**, harness floors 37 → **36** and viewer 52 → **49** (49 measured, where the pre-quadrant
figure of 50 had only ever been observed to pass).

**Then two labelling changes, at the user's request.** `AA` moved directly under `Half-Lambert light` so the two
switches sit together above the two sliders, and `Shades` became **`Shade Levels`**. The rename is not free: a
`Slider`'s `LabelWidth` is *exact*, not a minimum, so a longer label is ellipsized rather than pushing its own track
right — every slider in a panel has to share one width or the tracks stop lining up. Both panels now derive it from
a single `LabelWidth = 12` constant, up from 9, which costs three cells of track.

That also surfaced a **pre-existing one-cell misalignment**: `Slider.BuildLabel` appends a gutter cell of its own
(`cells = target + 1`), so the caption column for the drop-downs has to be `LabelWidth + 1`. The viewer's panel had
that right and the sandbox's did not, so every sandbox drop-down had been sitting one column left of every slider
track. Both are `LabelWidth + 1` now — and it was only visible in a PNG, which is the third time that has been true.

### STL support — the format, and what the sample files taught

`StlLoader` reads **binary and ASCII** STL; `ModelLoader` dispatches on extension and is now the single place that
knows which formats exist (the file browser's filter, the viewer's directory scan and its "nothing here" message all
read `ModelLoader.Extensions`). `ModelLibrary` enumerates and filters rather than globbing per extension, so a mixed
directory cycles in one name order rather than all the OBJs and then all the STLs. `ObjLoader.Normalise` is
`internal` now and shared: the viewer frames a model from `Mesh.Extents` and stands it on the floor from `Mesh.Min`,
so two loaders normalising differently would put one format's models through the floor.

**Both traps the reference implementation walks into are worth carrying forward.**

1. **Detect by arithmetic, not by the word `solid`.** voxcii (and most readers) sniff the first five bytes. A binary
   STL's 80-byte header is free-form and exporters write descriptions into it — `cali-bee.stl` starts *"Exported
   from Blender-2.80"*, but plenty start with "solid" and are then read as ASCII, yielding an empty mesh. A binary
   file's length is fully determined (`84 + n × 50`), so the length test identifies the format exactly — and makes
   the declared count trustworthy, which is what lets the reader size buffers from it with no sanity cap. There is
   a check that plants "solid " into the bee's header and requires it still to be detected as binary.
2. **The stored facet normal decides the winding.** Our rasteriser derives its normal from the vertex order and
   culls on that sign, and STL files in the wild routinely wind facets against the normal their tool recorded —
   those facets vanish into the cull and the model reads as full of holes. Where the two disagree the loader swaps
   two corners.

**And STL is a soup, which the rest of the renderer is not built for.** Every facet repeats its corners in full;
`Mesh`'s whole premise is that a body's vertices are transformed once per frame and referenced by its triangles.
Corners are welded on **exact** equality at read time — the bee goes 7,653 → **1,286**, a factor of six — rather
than with a tolerance, which would merge corners a model meant to keep apart.

**What measuring it actually turned up** (the checks recompute the facet count, the degenerate facets and every
triangle's winding from the raw bytes, so the loader cannot agree with itself):

- The bee declares 2,568 facets and yields **2,551**. The other 17 are exactly collapsed in the file.
- **A dozen more are slivers**, at an area six to nine orders of magnitude below the median, one underflowing to
  zero once the model is scaled down. Their winding direction is numerically meaningless — and *every facet the
  loader reversed on this model was one of them*. So the winding fix is **unexercised by the real file**; the
  synthetic ASCII facet, wound deliberately against its normal, is what proves it works. The check says both things
  rather than claiming coverage it does not have.
- The degeneracy guard is absolute and runs in the file's own units, before normalisation, which is why it cannot
  catch the slivers. Left alone deliberately: they cover no pixels either way, and a relative threshold would put a
  heuristic on the load path for no visible gain.

**A harness lesson, again.** Registering `media/models` for the viewer moved the subject of every later viewer check
from the teapot to a thin aeroplane, which then failed a coverage threshold calibrated on the teapot — for a reason
with nothing to do with what was being tested. `--shell viewer` now opens on a model chosen **by name**, not on
whatever is registered last.

**Not done, and deliberately:** MTL. voxcii's version is `newmtl` + `Kd` and no textures — and `capsule.mtl`, the
sample, has `Kd 1 1 1` with everything in `map_Kd capsule0.jpg`, so that level of support would render it white.
Textures mean UV plumbing (cheap — `Fill` already interpolates perspective-correctly), an image decoder (the demo
has no imaging dependency and ships NativeAOT-trimmed), and a real risk to the emission budget, since a per-pixel
tint through `Quantise` produces thousands of distinct pairs. Measure with a procedural texture before writing any
parser.

### The white-model bug — pre-existing, reported from play, and a good example of its class

**Symptom, in the user's words:** *"sometimes when I grab the model to rotate it it turns white and setting the
colour doesn't change anything. This white color persists to the other models too."*

**Cause:** `SceneView.OnMousePress` picks a body and selects it, and a selected body is tinted `Palette.Selection`
— which is pure white. The *model viewer* has no reason to select anything: one subject, no delete, no inspector,
nothing in its key map that clears a selection (`Escape` does, but the viewer's footer never mentions it). So a
press near the model turned it white for the rest of the session, and because selection is by **id** and the
viewer's body keeps its id across a reload, it followed you into every model after it. The Colour drop-down went on
working perfectly and changing nothing, because the selection tint replaces the tint rather than modulating it.

**Why it read as intermittent:** `Pick` matches against a body's projected **centre** within 0.08 NDC. Grab the
bunny's ear and you orbit; grab its body and you do not. Worse, a press on the model in the viewer did *neither* —
it suppressed the orbit (a grab was starting) and then the grab posted to a `PhysicsRunner` the viewer does not
have, so the drag was simply dead.

**Fix:** `SceneView.SupportsSelection`, derived from `runner is not null` rather than passed in by the shell,
because it is a fact about the scene and not a preference — selecting is only meaningful where something can be
grabbed, thrown or deleted, and all three go through the runner. Two places honour it: `OnMousePress` skips the
pick entirely (so a press **orbits**, which is the actual UX fix), and the `Selected` setter refuses a value. The
setter guard is the one that matters for the future — the mouse, `Tab` and the select-the-newest-spawn tick all
write that property, and guarding one of three is a bug waiting for the fourth route.

**Two things worth carrying forward.** First: **a feature that is merely useless in one scene is not neutral there
— it was overriding the one control that scene exists to demonstrate.** Second, on testing it: the obvious check is
to count the model's coloured cells and require them to survive the drag, and that check **passes with the bug
present** (295 → 428 saturated cells in the viewport, because the count is dominated by things that are not the
model, including the sidebar's own colour swatch). It is now two checks that do separate the states — the camera
orbits, and nothing gets selected — plus a sandbox check that selecting still *does* repaint a body, which is the
guard on the other direction of the fix. **A pixel check is only better than a state check when the pixels it
counts are the ones the bug changes.**

**And the sidebar constants moved for the fourth session running.** `SpacedRows` 61 → **63** at this point (exact: it is the sum
of the sections, 7+21+11+13+6+5). The harness's own floors moved too — `--shell` 36 → **37**, `--shell viewer`
50 → **52** — because those checks read a panel by finding its text on screen, and one more row pushes the last
section below the fold. Both are documented in the harness README now. This is the fourth time; open question 3.

### Two bugs I caused, and what each one says about the tests

**A crash, found by accident.** `HalfBlockSurface.Render` clamped its row loop against `ActualHeight` but its
**column loop against `PixelWidth`** — the pixel buffer's own width. The buffer is sized at `BeginFrame`; a
re-layout before the next paint can leave the control *smaller* than the frame drawn into it, and the write runs
off the end of the console buffer. **Collapsing the sidebar with `u` and restoring it does exactly that**, so
`u`, `u` with a solid or shaded renderer active could take the app down. Fixed by clamping both axes.

It had hidden because the harness presses `v` early, leaving the *wireframe* — a `Canvas`, not a
`HalfBlockSurface` — active for the sidebar-toggle checks. **A check that leaves global state behind hides bugs
from every check after it.**

**A deleted line of working code, which shipped.** `DetectEdges` ended with
`if (EdgeStyle == SilhouetteStyle.Line) color[i] = Brighten(color[i], EdgeBoost);` — the entire visible effect of
the `Line` outline style. A comment above it was being replaced, the replacement consumed one line too many, and
because the result was a *complete statement removal* it compiled clean. `Line` marked its edges and drew nothing
for two commits, until the user noticed a model looking wrong.

The same slip happened twice in one session; the other ate `var ink = Brighten(fg, EdgeBoost);` and the compiler
caught it in seconds. That difference is the lesson: **an edit that removes a self-contained statement leaves no
syntactic evidence, so only a test can find it.**

And no test could have. Every silhouette check read the *detector* — which sub-pixels got marked — and none read
what the marking does to the picture. Two new checks close it (render with `Edges=None` vs `Edges=Line`, require
the cells to differ; require `Glyph` to differ from `Line`), verified non-vacuous by re-deleting the line.
**Generalisable rule: a check that asserts an internal mark is only half a check. Assert the pixels.**

### The sandbox sidebar scrolls, and the trap in making it

`SpacedRows` is demoted rather than deleted: the panel still picks the compact layout in a short terminal, because
seeing the whole panel beats scrolling for the camera pad. Scrolling is the **backstop** under that, so a stale
`SpacedRows` now costs a scroll instead of a section, and the undocumented compact floor (42 rows, and 40 and 36
before it) is gone — the harness passes at every height from **37** rows up (re-measured after the Shades, Smooth and
Quadrant AA rows), where the old floor was 42; the app itself renders and scrolls correctly below that.

**The trap: the tier must be measured against the FRAME'S VIEWPORT, not the panel's `ActualHeight`.** Inside a
scrolling frame a control is laid out at the height `IScrollable.MeasureHeight` reports (`Control.CalculateSize`
honours it when the parent leaves the height unbounded), so `ActualHeight` becomes the *content* height and
`ActualHeight >= SpacedRows` degenerates into "is the spaced layout at least as tall as the spaced layout". It
latches on the first answer and the tier never changes again, at any terminal size, silently. `ControlFrame.ViewportSize`
is the real visible height and cannot feed back, since the frame is sized by its parent.

**And one in the harness, worth remembering as a class:** the camera-pad check now scrolls to the pad when the
viewport is too short for it — and that scroll left every *later* check reading a panel scrolled past the section it
was looking for. A test that changes the state it shares with the tests after it has to put it back.

### Switching shells in one process — what restarting the UI actually costs

**Scene ▸ Switch to model viewer** and **Model ▸ Switch to sandbox** now tear one shell down and build the other,
same process. `SandboxShell.ShellType` names the two; the menu item records the request and calls `UI.Stop`, and a
**loop in `Program`** runs the next one after the awaited `Start` completes.

**A loop, not a menu item that calls `RunSandbox`/`RunModelViewer`.** That was the original sketch and it is the one
thing worth not copying: a menu handler runs on the UI thread inside the frame loop of the shell it belongs to, so
starting the next UI from there starts one from inside another and keeps a shell alive per switch. As a request it
costs nothing — the switch is just a field the loop reads once `Start`'s task has completed and the UI thread is gone.

**The restart mechanism itself was already sound**, which was the surprise. `Dispatcher.Start` explicitly re-creates
its cancelled lifetime and joins a previous loop; `UI.Stop` already clears `controls`, joins the reader thread, and
notes in a comment that a reader from a *previous* Start would otherwise steal this one's input. The test suite has
been starting and stopping the UI hundreds of times per run all along. What restarting exposes is not the loop —
it is **everything scoped to a session that nobody scoped**:

1. **Hotkeys outlived their session.** *(library fix.)* `GlobalHotKeys` is static and `Stop` never touched it, so the
   viewer inherited the sandbox's `space`, `.` and `r` — closures over a **disposed `PhysicsRunner`**. `Stop` now
   calls the new public `UI.RestoreBuiltInHotKeys()`, which drops app registrations and keeps the library's own
   (Ctrl+Q, the Ctrl focus tier, F1). Two tests, and the `--switch` mode fails on exactly this when the call is
   removed (`2 hits, expected 1`).
2. **A cancelled feed is not a stopped feed, twice over.** `SceneView` runs a 60 Hz render feed. `Control.Dispose`
   cancels it but does not join it, so the tick already in flight still ran — and, worse, **the dispatcher queue is
   not cleared between a `Stop` and the next `Start`**, so ticks posted in the gap were picked up and run by the
   *next* session. That gap is deliberate (it is how `Program` posts its initial `SetFocus` for the session it is
   about to start), so the fix belongs at the producer: `SceneView.Dispose` now joins via `FeedHandle.StopAsync` and
   sets a flag that makes any already-queued tick a no-op. Measured: 2 stale frames per switch before, 0 after.
3. **Every `Control` subscribes to the static `UI.Paint`/`UI.ThemeChanged` in its constructor**, and only `Dispose`
   unsubscribes. So a discarded tree keeps painting into a detached buffer for the life of the process. The shell
   records are `IDisposable` now and tear down what they hold (view, sidebar, menu, runner), but the leaves — labels,
   sliders, sections — are not reachable from there and stay subscribed. Bounded and cheap (a no-op delegate call per
   control per frame), so it is **not** fixed here. **A tree-walking dispose is the library follow-up**, and
   `ILayout.Controls` is per-cell rather than deep, so it is a real piece of work rather than a one-liner.

Verified by `--switch`, a new harness mode and the only one that runs the UI **loop**: three sessions
(sandbox → viewer → sandbox), asserting each starts and paints, that a hotkey from the previous session no longer
fires, and that no earlier shell is still drawing. 13 checks, and 3 of them fail if either fix is backed out.

### Three sidebar buttons, and what they cost

**Scene** now carries `Clear` and `Reset` (the `c` and `r` keys), **World** carries `Reset`
(`SandboxParameters.Reset`, what the menu item calls). `SidebarPanel`'s constructor takes the reset action, because
repopulating the world needs `populate`, which only `SandboxShell.BuildSandbox` holds — so the local `Reset` moved
above the sidebar's construction and one function now serves the key, the menu item and the button.

**Clear moved out of the Inspector rather than being duplicated.** Two identically-labelled buttons in a 32-column
panel is a worse answer than one in the right place; the Inspector acts on the selection, which is what `Delete`
does, and the menu already files "Clear bodies" under Scene.

**The cost is two rows in each tier, and it moves both hand-maintained thresholds.** `SpacedRows` 53 → **57**, and
the compact layout's undocumented floor 40 → **42 rows** — below that the camera pad is clipped even compact. Both
were verified by sweeping `--shell 200xH` rather than spot-checked: 42 is the first height that passes, 40 and 41
fail exactly as they should, and 57 is exact (a terminal of 60 gives the sidebar 57 and picks the spaced layout,
which then fits with nothing to spare). This is the third session in a row those constants have bitten; if a fourth
control lands here, making the sandbox sidebar scroll the way `ModelSidebarPanel` already does is probably cheaper
than maintaining them.

The harness clicks all four buttons at their labels' **screen cells** and asserts the effect on the scene and on the
parameters — so a button that laid out at zero width fails there rather than passing on internal state.

### Two grab bugs, fixed after those commits — both reported from play

**Dragging a body toward the camera pushed it through the floor, and letting go lost it.** The drag plane faces the
camera, so whenever the camera looks *down* at the scene that plane is tilted — and pulling the pointer toward the
bottom of the screen aims below `y = 0`, not merely nearer. Nothing resisted: a grabbed body is **kinematic**, so it
is moved by us and not by the solver, and static geometry does not stop it. It sank, and the release either dropped
it out of the world or handed the solver a body penetrating the slab by several units to eject. Fixed by clamping
the steered target to `GroundY + halfExtent.Y` (`PhysicsScene.OnGround`, applied in `Step` where the target is
consumed), with `SandboxShell` now stating `GroundY = 0f` next to the slab it builds so the two cannot drift apart.

**A short flick fired the body off the screen.** `ThrowVelocity` measured over the last *five pointer events* and
divided by however long they happened to span, guarded only at 1 ms. A terminal reports the pointer in whole cells
and in bursts, so two adjacent events are routinely several world units and a couple of milliseconds apart — and
`distance / 2 ms` is a speed nothing on screen ever had, capped at a `MaxThrowSpeed` of 40 that was itself half the
cannon `f` fires. Now measured over a **120 ms window of real time** ending at the release, divided by at least
`MinThrowSeconds` (40 ms) so a two-event flick is bounded by how far it went rather than extrapolated from how
briefly it took, and capped at 15. Plus the guard the old comment claimed and the code never had: a release **after
the pointer stopped** now drops the body, because samples arrive only on movement and the last delta would otherwise
still be sitting there waiting to be believed.

**And one thing neither bug needed but both revealed:** a body that leaves the slab fell forever and was solved on
every step for the life of the process. `CullFallen` destroys anything `KillDepth` (60) below `GroundY`.

Verified: 74/74 in the parked harness over three consecutive runs, `--shell` at 40 / 50 / 56 rows. The four new
checks are non-vacuous — reverting the three fixes fails exactly three of them (the buried body reads `y = -12.00`
and the flick releases at **36.3** u/s). Two notes for whoever touches this next:

- **The harness's own throw check is the extreme case by accident and that is the point.** A synthetic drag reports
  every move in the same microsecond, which is precisely the input that used to imply an absurd speed. It also
  perturbs the scene: the first version of it flicked a body off the table, which was then culled mid-`Settle` and
  made the *pre-existing* delete check fail intermittently at `14 -> 12`. A `Settle(300)` after the throw lets
  anything on its way out finish leaving before the counts below are read.
- **A cull check passes vacuously if the posted command never ran**, since both outcomes leave the count unchanged.
  It spawns two bodies, one either side of the kill plane, and expects `+1`.

### What this session did

Five arcs, in order. The 3D plan doc carries the full write-up for the first; the rest are below.

1. **The wireframe drew loaded models as a dot cloud.** Four separate bugs, each hiding the next — see
   *follow-ups 1–4* in [`3D Sandbox Plan.md`](3D%20Sandbox%20Plan.md), which is where the evidence lives. The
   short version is in open question 4 below.
2. **Those tradeoffs became settings** rather than tuned constants: `Stratify`, `ScanCap`, `SubPixelsPerTriangle`
   on `WireframeRenderer`, surfaced through `SceneView` into both sidebars and both menus.
3. **`Enabled` on `ToggleButton` / `Select` / `Slider`** (library), because a sidebar had no way to say "this
   setting is real but not from here" without lying about its value.
4. **`Select` takes `IRenderable` options** (library), for a colour swatch beside a name — which markup in a string
   cannot do, because the closed control emits one `Segment` and never parsed markup.
5. **Shaded lighting audited and re-defaulted**: `WrapLighting` now off, `ContactStrength` renamed to
   `OcclusionStrength` and exposed as a dial.

**Next: M4 — polish.** Colour modes (velocity / mass / sleep), motion trails, scene presets (stack, tower, pyramid,
domino run, wrecking ball). Then M5 — ship: promote the harness, README, gallery entry, Docker target. Open
questions 6 and 7 (the Detail dial's ceiling, and the dragon needing a different primitive) are deferred by
agreement — the current behaviour ships and performs fine.

### The two library changes, in short

**`Enabled`** — on `ToggleButton` (so `Checkbox`/`RadioButton`/`Switch`), `Select` and `Slider`, each with a themed
`DisabledStyle` defaulting to `IStyleTheme.TextDisabled`. Disabled means inert to the user, out of the Tab order,
drawn muted, no hover cue. **It stays settable in code and keeps showing its value** — that is the point of
disabling rather than hiding, and the easiest thing to regress. The focus half could not be an override
(`Control.Focusable` is not virtual), so `RenderableControl.ApplyEnabledToFocus` clears it and restores whatever it
was. 24 tests; verified non-vacuous by removing the guards (16 fail).

**`Select` + `IRenderable`** — `SelectOption` is text *or* a renderable, mirroring `ListBoxItem`; plus `Items`,
`SelectedItem` and `Tag`. Text options are untouched, markup included (still literal). Two traps, both caught by
existing tests: routing the constructors through `SetOptions` auto-selected the first option and lost the
placeholder, and `SelectionChanged` was guarded on the text so a renderable-only `Select` would never have raised
it. 9 tests.

### Where the 3D demo's UI stands

Every keyboard action has a mouse route: a `MenuBar`, a 32-column sidebar, a camera D-pad, a runtime model loader.
`SandboxShell` assembles both scenes so the headless harness drives the **real** shell rather than a rebuild of it.
There are **two** sidebars and wiring one leaves the other untouched — `SidebarPanel` (sandbox, a fixed two-tier
stack that *clips*) and `ModelSidebarPanel` (the `obj` viewer, `IScrollable` with `MeasureHeight` summed from its
sections, so it *scrolls*). Sections are now grouped by which renderer owns them: **Render** for what applies to
all three, **Shaded detail** (Edges + Half-Lambert light + Occlusion), **Wireframe mesh detail** (Even over screen
+ Detail + Scan).

**The sidebar layout constants are hand-maintained and bit three times that session.** `SidebarPanel.SpacedRows` was
53 then and is **63** now; neither it nor the harness floors are derived from anything. A stale `SpacedRows`
fails only in the narrow band just above the threshold, where the spaced layout is chosen and then does not fit —
so sweep heights, don't spot-check one. `--shell 200xH` asserts the camera pad is on screen.

### From the M3 UI session (2026-08-14), for context

**Seven library changes landed** then, all driven by something the demo could not otherwise express:

| Change | Why |
|---|---|
| **`Slider`** (+ `SliderStyle`, theme tokens) | nothing in the library was a draggable value; `Gauge`/`ProgressBar` are display-only |
| **`FileBrowser`** (+ `Dialog` helpers) | models were chosen by a CLI arg and unchangeable at runtime |
| `MenuBar.Add(title, Func<MenuItem[]>)` + `MenuItem.Checked` | `MenuItem` is immutable, so a checkmark could never update |
| `Tree.NodeExpanding`, public `Tree.SelectNode` | no way to populate a subtree on demand |
| `Select.SetOptions`, `Select.FitContent` | options were fixed for life; the closed control never matched its pop-up |
| `ListBox.CommitOnClick` | a single click committed, so a chooser opened whatever you glanced at |
| `ConsoleSnapshot.Drag` | no capture-aware drag in the test API |

**And one real library bug, found by using the app, not by a test:** `Dialog` treated *any* loss of focus as a
dismissal. Clicking a field inside a dialog moves focus to that child, so the dialog silently completed with its
cancel result on the **first click inside it**, after which every button was dead and it sat there un-closable.
Only content with no focusable children (the `Message`/`Confirm` helpers) escaped it, which is why it survived.

**New public doc:** [`docs/3D Rendering in a Terminal.md`](../3D%20Rendering%20in%20a%20Terminal.md) — a from-first-principles
explainer of the whole 3D implementation for a C# developer who has never written a renderer. Linked from
`docs/README.md` under a new "Deep dives" heading. **Not** in the doc-snippet scan set (its fences are demo
excerpts, not library snippets), so it is not machine-checked — see the open question below.

### Open questions the next session should decide

1. **Should the doc-snippet harness *render* as well as compile?** The starter example in
   `docs/controls/Writing Applications.md` — the first thing a new user copies — **did not render its button** for
   an unknown length of time. It compiled perfectly; the label filled the row and the button laid out at zero
   width. Compiling proves a fence is valid C#, not that it produces a UI. Fixed, but the class of bug is open.
2. **Should `HorizontalStackPanel` get weighting / star sizing?** The above is the general trap: the panel offers
   each child everything unclaimed and most controls report *fill*, so the first one swallows the row. Documented
   in three places now, with `Grid` as the answer. A weighting policy would make the trap unnecessary, but it means
   changing an `ext/` class.
3. **Promote the parked harness into a real test project.** Now ~67 checks plus a `--shell` mode covering the M3
   UI (layout, key↔widget agreement both ways, sidebar toggle, camera pad) and a `--sidebar` row dump. It has
   caught four shipped bugs. Still sources only at [`scratch/`](scratch/README.md), not wired into the solution.
   **This is the one that will rot.**
4. ~~**Should the wireframe renderer draw a convex hull for mesh bodies?**~~ **Answered without the hull, over four
   bugs.** Full evidence in *follow-ups 1–4* of [`3D Sandbox Plan.md`](3D%20Sandbox%20Plan.md); what a mesh sampler
   now does is a screen-area budget, whole triangles, back faces culled, an absolute `ScanCap`, and a round-robin
   spend across screen buckets. What is worth carrying forward is not the fix but **how the bugs hid**:

   - **A thinning step with a fixed output size makes every measure of output *volume* right by construction.**
     Lit-cell counts, byte counts and bounding boxes all passed while a third of the bunny was missing — the total
     was correct and only the distribution was wrong. The extremities still get drawn, so even the on-screen
     bounding box is the right size. Catching it needs an occupancy grid.
   - **And the mirror of that.** The first stratification had the right distribution and the wrong total (a fixed
     `budget / buckets` quota that sparse buckets cannot spend — a zoomed teapot drew 57% of its budget). **No
     check that measures only one of the two sees both.**
   - **Test subject and resolution are part of the test.** A uniformly tessellated model is blind to the
     tessellation-density bug by construction, so the check runs over the teapot *and* the plane. A grid coarse
     enough to expose the missing-region bug cannot see the density one at all — it passed at 100% either way.
     Ground truth must be *front-facing* geometry: using all vertices halves the separation, because a hollow model
     has cells only back faces reach. Thresholds sit between measured states, obtained by deliberately re-breaking
     the renderer, never at an aspiration.

   **Still open for very dense meshes**, and this is the real ceiling: at 250k triangles a sampled triangle is
   sub-pixel (median 0.20 cells against the bunny's 1.46), so the dragon is a correctly-shaped stipple rather than
   a surface. More budget only makes denser dots. That is the case hull or silhouette/crease edges would fix, and
   Box3D still does not expose hull geometry. See question 7.
5. **Vendor a model, or keep pointing at `reference/`?** Unchanged. The demo ships only a generated torus knot.
   If any model is vendored, `THIRD-PARTY-NOTICES.TXT` needs it.
6. **Should `WireframeRenderer.MaxTriangles` scale with the Detail dial?** *Deferred — the current behaviour ships
   and performs fine.* The 1,200 ceiling was written as a cost guard when the density was a fixed constant, and it
   now bites before the top half of the user-facing slider does, on every model that fills the frame:

   | Detail | 1 | 2 (default) | 4 | 8 |
   |---|---:|---:|---:|---:|
   | bunny (4,968 tris) | 422 | 844 | 1,689 → **1,200** | 3,379 → **1,200** |
   | teapot (6,320) | 545 | 1,090 | 2,181 → **1,200** | 4,363 → **1,200** |
   | dragon (249,882) | 577 | 1,155 | 2,311 → **1,200** | 4,622 → **1,200** |

   So Detail 4 and Detail 8 are the same picture, and going from 2 to 8 asks for 4× and gets +4%. The lower half of
   the dial works. Someone turning Detail *up* is explicitly asking to spend more, so a ceiling derived from the
   dial (rather than a constant) would make the control honest across its range; the cost would rise as asked.
7. **The dragon needs a different primitive, not a bigger budget.** *Deferred, and it is the real ceiling on the
   wireframe.* Median projected size of a front-facing triangle with the model filling a 200×50 viewport: bunny
   **1.46 cells**, teapot **0.84**, dragon **0.20**. A fifth of a cell lights one sub-pixel, so a dragon triangle
   draws as a dot rather than an edge and 1,155 dots read as a stipple. Coverage is already 97–99% — nothing is
   missing, the primitive is simply below the resolution of the medium, and more of them only makes denser dots.
   The fix is edges that *span* many triangles — silhouette/crease extraction, or the convex hull (question 4).

## Where things stand — the 3D sandbox

Milestones M0 through M3.5 are done. `examples/Jumbee.Console.3DSandboxDemo` has a README with the full key map.

**The M3 result that matters:** a menu bar, a 32-column sidebar of live widgets and a two-line footer make every
renderer emit **fewer** ANSI bytes than the bare viewport did (wireframe 12,929 → 11,598 B; shaded 17,452 →
14,738 B). The sidebar replaces 32 columns of continuously-changing 3D with static text the compositor emits
nothing for. Whatever the objection is to putting UI beside a 3D viewport, it is not the frame budget.

**Keys and widgets agree** because neither talks to the other: the state objects own the truth and raise a change
event; a widget writes to state, and the panel reads it back behind a `syncing` re-entrancy guard. Both directions
are asserted headlessly, and the widget direction against *what is on screen* — parse the value back out of the
slider's rendered readout, not off the object.

**Three renderers over one scene**, cycled with `v`, sharing a rasteriser through `MeshRenderer`:
`WireframeRenderer` (braille edges on a `Canvas`), `SolidRenderer` (flat per-triangle, directional),
`ShadedRenderer` (per-pixel point light + specular, silhouettes, contact darkening). Plus an `obj` verb opening a
model-viewer scene with shear and non-uniform scale.

**The perf picture, in bytes rather than microseconds** (this desktop swings 2.3× on timings; byte counts reproduce
exactly). At 200×50 with an orbiting camera, everything sits between 10% and 18% of a 60 fps frame. The single
biggest lever is **quantising the shade ramp**: coherent content emits ~7× fewer ANSI bytes than content where
every neighbour differs. Two results worth remembering because they invert the intuition:

- the **solid** renderer emits *fewer* bytes than the wireframe, despite covering every cell — a wireframe's lit
  cells are scattered singletons, each needing its own cursor move and SGR;
- **half-lambert wrapping** emits **16% fewer** bytes than clamping, because compressing the lit range into fewer
  distinct levels coalesces better.

**Five findings from M3**, all written up in the plan doc, and all of the "silent" kind:

1. **A control cannot scroll to a row it has just made reachable.** Expanding a tree node is what makes it tall
   enough to scroll, but a frame clamps its scroll offset against the content height it last *measured* — so a
   scroll issued in the same pass is dropped to zero, and the next pass computes the row from the pre-expansion
   layout. Two wrong answers, both silent. (Self-inflicted variant met first: an explicit `Height` pins a control
   so its frame can never scroll it at all.)
2. **`SetAtomicProperty` only invalidates when the *value* changes**, so a control fed a mutable object updated in
   place paints once and then freezes. The demo footer sat naming the wrong model for the life of the viewer.
3. **Any control that deliberately paints less than the width it is offered must set `RendersOwnFocus`.** The
   themed focus cue fills a control's *unpainted* cells, so a content-sized `Select` sprang back to full width the
   moment focus returned to it.
4. **A fill-width control in a `HorizontalStackPanel` swallows the row.** See open question 2.
5. **Judging a change from a PNG changed a design.** The `Slider`'s thumb was drawn with the eighth-block sub-cell
   ramp, exactly as `ProgressBar` draws its fill edge, and every test passed. The image showed a marker one eighth
   of a cell wide at some values and a full cell at others — a stack read as a ragged bar chart. Whole cell now.

**Five findings from M0–M2.5**, still worth knowing:

1. `CompositeControl` with no focusable children never receives `OnInput` from the layout route — handle keys in
   `InterceptInput`. Worse, `UI.SendInput` takes a *different* path, so the obvious test is green while the app is
   dead. This shipped, and the user found it, not a test.
2. `FillsFrameViewport` has a silent failure mode: without it a framed viewport balloons to the 1000-row clamp and
   renders *empty*, which reads as broken projection maths.
3. Backface culling sign: inverting Y for screen rows reverses handedness, so an outward-facing triangle arrives
   with a **negative** signed area. Culling `<= 0` discards every visible face.
4. `Body.AddMesh` requires a **static** body, so a mesh body must collide as a convex hull.
5. **Judge renders from a PNG, never an ASCII dump.** See below.

### From the 0.1.10 documentation session, for context

**0.1.10 was packed and committed** (`artifacts/` holds the 0.1.11 packages; `ProjectAssemblyVersion` has since
moved to `0.2.0` for the scrolling/metrics release). That session was documentation-led and produced one breaking
change:

- **Breaking: eleven control events moved from `Action`/`Action<T>` to `EventHandler`/`EventHandler<T>`.** The same
  event name had different shapes on different controls (`TabPanel.SelectionChanged` was `Action<int>`,
  `ListBox`/`DataTable` were `EventHandler<int>`). Migration table is in `CHANGELOG.txt`.
- **`DataTable` no longer probes a degenerate table before layout.** Setting `SelectedIndex` at construction ran
  the scroll maths at `ActualWidth == 0`, clamping the measure probe to one cell wide and asking Spectre to divide
  space between ratios summing to zero. Debug tripped the assert; Release compiled it out and carried on with the
  meaningless measurement.
- **New control-guide section**, `docs/controls/` — a hub with a task-to-control decision table plus a guide per
  category. `Select`, `Button`, `GlassPanel` and the status widgets were documented for the first time.
- **`tests/Jumbee.Console.DocSnippets`** — compiles all 102 code fences in the guides, both READMEs and
  GETTING-STARTED against the real assemblies, ignoring only CS0103. Runs under `dotnet test`.
- **Embedded PDBs + SourceLink** on all ten bundled assemblies, each mapped to its own public repo and commit. No
  symbols package: a `.snupkg` only reaches people who enabled the NuGet.org symbol server.

Full suite was **929/929** at that point; it is **976/976** now.

**`CHANGELOG.md` is now `CHANGELOG.txt`** — plain text, because it is packed verbatim as `PackageReleaseNotes` and
neither nuget.org nor the VS package manager renders Markdown there. Every reference was updated
(`src/Directory.Build.props`, `build-api-docs.ps1`, `preview.ps1`, `llms.txt`).

### Release-process change worth knowing

SourceLink bakes the commit hash in at build time. **Pack only from a clean, pushed tree** — packing with
uncommitted or unpushed work gives consumers 404s, or worse, source that silently doesn't match the binary.

## Backlog, in order

1. **Quadrant-glyph silhouette AA** — the agreed next task, and the sequel to the edge blend that shipped and
   underwhelmed. A cell carries exactly two colours; a silhouette is exactly a two-colour boundary; so a quadrant
   glyph places that boundary at 2×2 resolution *within* the cell at zero colour cost. Needs per-quadrant coverage,
   so supersampling — but only at cells `DetectEdges` already flagged, making the cost track perimeter not area.
   **First step is a font check**: quadrant blocks are U+2596–259F; run them through the multi-font snapshot test
   before building anything. The `EdgeStyle.Glyph` path already does "substitute a glyph at an edge cell", so the
   machinery exists.

The rest are still open from the vtop eval loop; evidence for each is in `eval-findings.md`.

2. **V-13 — `ControlFrame` can't put a second label on a border edge.** Four separate runs hit this; it's why
   vtop's `─ CPU Usage ──────── 19% ─` can't be expressed and everyone prints into the content area instead.
   Highest evidence count of anything still open. Now also noted as a known limitation in
   `docs/controls/Control Model.md`.
2. **V-28 — `DataTable` has no per-row update.** `Live Data.md` no longer claims otherwise (it now documents the
   rebuild-and-restore-by-key pattern), so this is a missing feature rather than a contradiction. Adding
   `UpdateRow(int, params string[])` still beats every live table rebuilding itself each tick.
3. **V-21 — no proportional split and no layout-changed event.** The 3D sandbox wanted the second one and worked
   around it by reading `ActualWidth`/`ActualHeight` inside `Draw()`; that makes three demos asking.
4. **Cheap doc cross-links** — V-24 (no stated way to turn *off* a control's mouse handling), V-26
   (`ConsoleSnapshot`'s mouse API isn't mentioned in "Testing without a terminal", where everyone starts), V-19
   (`TitleBorderStyle` has no `None`, and the default adds a divider under the title). V-16 is closed.
5. **A tree-walking dispose.** Every `Control` subscribes to the static `UI.Paint`/`UI.ThemeChanged` in its
   constructor and only `Dispose` unsubscribes, so any app that discards a control tree (rather than exiting) leaks
   the whole tree's handlers. There is no way to tear a tree down: `ILayout.Controls` is per-cell, not deep, and a
   `CompositeControl`'s children are not in it at all. Found by the shell-switch work above; deferred there because
   the residual cost is a no-op delegate call per stale control per frame.
6. **V-31 — `DataTable` grid style.** Attempted in the 0.1.9 session and **reverted**; see `eval-findings.md` for
   the three geometry traps. Short version: don't model Spectre's table layout, *measure* it — `Render` already
   renders to segments and counts them, so derive both the top chrome and the horizontal overhead from the real
   table and delete the `- 1` and `3n + 1` constants before adding any enum.

Also pending, small: fold the reviewer's threading rule into `Live Data.md` — *a control has a single
thread-affine mutation point, and the whole state change plus invalidation crosses the boundary as one unit.* The
guide currently tells the **caller** to marshal; the control owning it is the stronger rule.

Two API inconsistencies noticed while documenting, neither actioned: `Button` has no enabled/disabled state (so
"disable submit until valid" can't be expressed), and `Control.OnHelp` plus the `On*` focus hooks still use
bespoke delegate types while everything else is now `EventHandler`.

## Operational notes

- **Editing a `.cmd` file with `sed`/`awk` on Git Bash silently converts it to LF, and `.gitattributes` pins
  `*.cmd` to CRLF on purpose** — its own comment says why: *"cmd.exe drops leading characters from LF-only .cmd/.bat
  files."* An `awk '{...}' f > tmp && mv tmp f` rewrite preserves the `\r` on lines it copies through but writes
  LF-only for lines it inserts, so the file ends up mixed; a `sed -i` over the whole file finishes the job and makes
  it uniformly LF. **`git diff` will not tell you**: with `core.autocrlf=true` the blob normalises either way, so the
  diff looks clean and small while the file the user actually runs is broken. The tell is a byte-level count, not a
  diff:

  ```sh
  perl -ne '$t++; $c++ if /\r\n$/; END{print "$ARGV: $c of $t CRLF\n"}' examples.cmd
  ```

  Everything else in this working tree is LF (`.sh` by `.gitattributes`, the rest by convention — `pack.ps1`,
  `CHANGELOG.txt` and `.dockerignore` are all LF), so `.cmd` is the only family that needs the check. It bit here and
  the user caught it, not any test.
- **The full-suite flakiness improved, and the cause is now half-understood.** The suite used to fail a *different*
  1–2 tests per run while all passed in isolation. `MenuBarTests` had no `UiTestHarness.EnsureStopped()` in its
  constructor while most classes do, and it drives the ambient `UI.Overlay`, which is global; adding it gave three
  consecutive clean 976/976 runs — the cleanest the suite has been. If a stray failure returns, **check whether the
  class touches `UI.Overlay`, `UI.SetFocus` or `ConsoleSnapshot`'s static mouse state without resetting it** before
  suspecting a regression.

  **It has not gone away.** This session it recurred once as
  `ValueTypeEqualityTests.BehavesAsAValueType(type: "Character")` — 1035/1036 — then three consecutive clean
  1036/1036 runs with no change in between, and nothing in the session went near `Character`. The signature is
  unchanged: a *different* test each time, always green in isolation. Treat a lone full-suite failure as this until
  a second run reproduces it.
- **A doc example can be compile-clean and visibly broken.** See open question 1. When touching a doc snippet that
  builds a layout, render it — the three-line check is `ConsoleSnapshot.ToText(layout, w, h)` plus reading
  `ActualWidth` on each child.
- **The demo holds its own output DLLs while running**, exactly as the examples browser does: a build fails with
  `MSB3021`/`MSB3027` copy locks and zero `CS` diagnostics. The parked harness sidesteps this — it compiles the
  demo's *sources* into its own output, so it can verify a change while the app is still open.
- **Judge anything visual from a PNG, not an ASCII dump.** `ConsoleSnapshot.SavePng` needs one setting —
  `SnapshotImageOptions.FontFamily = "Cascadia Mono"`, because the default Consolas has no braille and poor
  geometric-shape coverage. Printing a text dump through the Bash tool mangles `▀`, braille and `◆◇◈◊` into `?`,
  and it is very easy to read your own encoding damage as the renderer's output. The first real PNG this session
  exposed three defects that every passing test had missed — a renderer that was visibly *darker* than the one it
  replaced, edge detection outlining the horizon, and outlines invisible on dimmed bodies. **Tests verify the claim
  you thought to make; an image shows the ones you didn't.**
- **`Task.Wait(timeout)` rethrows a faulted task exactly like `.Result` does.** `VtInputSource.ReaderLoop` caught
  around `.Result` but waited with `Wait`, so a failed console read escaped and killed the reader thread — the app
  lost all keyboard input to an unhandled `AggregateException`. Fixed, with three regression tests and an internal
  test-seam constructor taking a `Stream`. Triggered by a window resize under the VS debugger; never reproduced
  standalone in Windows Terminal, so the root cause is still unconfirmed (the fix is defensive either way).
- **Don't trust a near-empty compiler error list.** A single bad `using` (CS0234/CS9229) suppresses semantic
  analysis for the whole compilation — 288 diagnostics vanished this session and the run looked clean. Same class
  of trap as parse errors hiding semantic ones.
- **Grep is not a valid test for SourceLink.** The embedded PDB is deflate-compressed, so the URL isn't findable
  in the DLL bytes. Read the PDB via `System.Reflection.Metadata` (there's a working scratch reader from this
  session; it's ~40 lines).
- **PowerShell 5.1 reads `.ps1` as ANSI without a BOM.** Em-dashes in script string literals become mojibake in
  generated output. `build-api-docs.ps1` already keeps one out of a literal via `[char]0x2014`; keep new literals
  ASCII.
- **The examples browser holds its output DLLs while running.** A build then fails with ~22 `MSB3027`/`MSB3021`
  copy-lock errors containing zero `CS` diagnostics. Build to a scratch `OutputPath` to check compilation without
  killing the process.
- **Verify agent-reported numbers before acting.** Two reviewers gave confident, precise, mutually incompatible
  figures for the same sampling loop (23 ms/tick vs 9.8 s/pass); direct measurement said ~17 ms.
- **Reproduce in the configuration the bug was reported from.** V-22 passed in an isolated test *with the bug
  present* and only failed once other renders ran in the same process.
- **The eval loop** (jc-curious cold starts against a `preview.ps1` snapshot, each in its own workspace under
  `C:\Users\Allister\Agents\jc-curious\`, forbidden from reading earlier runs) is idle. Agent `.md` edits need a
  session reload, or the agent runs the previous target's brief.
