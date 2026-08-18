# Handoff — mesh thinning, control settings, renderable Select, grab fixes, shell switching (2026-08-17)

Living "where we are / what's next" note. Companion to [`eval-findings.md`](eval-findings.md), which is the full
backlog with evidence; this is the short version plus the operational context you'd otherwise have to rediscover.

## Next session starts here

**Committed, in two commits.** `de29352` "Fix bugs in wireframe renderer" carries the first half of the mesh-thinning
arc (screen-area budget, whole triangles, backface culling, the two-pass draw, `Mesh.WireEdges` removed);
`b37379f` "Add color control to 3D demo, add shaded renderer detail options" carries everything else — 22 files,
including the two new test files. Verified at that point: build clean, 1034/1034 tests, doc snippets, the parked
harness, and `--shell` at 40 / 50 / 56 rows plus the viewer.

**Then `2464d4f` "Fix sandbox physics bugs, add UI controls"** — two grab-gesture bugs the user hit by playing with
the sandbox, and three sidebar buttons they asked for. Both written up below.

**Then `6212d36` "Support switching scenes from menus"** — **switching between the two scenes without leaving the
process**, the only one of the three that reached the library and the only one with a behaviour change in
`CHANGELOG.txt`. Written up next.

**Then `f570062` "Sandbox sidebar implements IScrollable. Fix model viewer reading 'models' dir on switch"** — four
follow-ups on the switching work, all written up below:

- The viewer got a **`Scene` menu** of its own, leading the bar and holding Switch and Quit, so those two live in the
  same place in both scenes.
- **`obj` with no path** now looks for a `models` folder beside the working directory instead of scanning the working
  directory itself, falling back to the generated knot. `ModelLibrary.DefaultDirectoryName` names the folder, and the
  match *enumerates* rather than probing `Directory.Exists`, which is case-insensitive on Windows and not on Linux,
  where the demo's container runs.
- **That scan now also happens when switching into the viewer from the UI**, which is what the user actually hit: only
  the `obj` verb looked at the folder, so a sandbox launched without it switched into a viewer holding nothing but
  the generated knot. `Program.OpenModelsFolder` does it **once** per process — a directory of the reference models
  is ~750 ms to parse and switching back and forth must not pay that twice — and what it loads stays in the registry,
  so those models become spawnable back in the sandbox too.
- **The sandbox sidebar scrolls**, like the viewer's: `SidebarPanel` is `IScrollable` with `MeasureHeight` summed from
  its sections, wrapped in the same borderless frame. See below — the tier logic needed one non-obvious change.

**Uncommitted after that:** shipping the sandbox through the launchers and both images.

- **The launchers and both Docker images carry the 3D sandbox**, reachable as `./examples.sh 3dsandbox` (aliases `3d`,
  `sandbox`), `examples.cmd 3dsandbox`, and `docker run --rm -it jumbee-console 3dsandbox`. `3dsandbox obj` opens the
  model viewer. The models folder is handled differently per image, and that asymmetry is the only interesting part:
  the **full** image already has the repo at `/src`, so `examples.sh` just runs the demo **from its own project
  directory** and `models/` is found with no staging step — the same one line that makes it work on a host checkout
  from any cwd; the **slim AOT** image has no repo, so `Dockerfile.aot` stages `models/` to `/app/models` beside the
  binaries, exactly as it already does for the AudioScope track. Both are soft: `models/` is untracked (18 MB of
  third-party research meshes), and without it the sandbox falls back to its generated torus knot, which is a working
  app. Build-time notes in both Dockerfiles and both `build-docker` scripts say which happened.
  **Both images are verified end to end, built and run.** Full: `3dsandbox obj` in the container opens on `bunny`,
  which is the whole claim — the launcher's working-directory switch reaches the models `COPY . .` brought in. Slim:
  the sandbox **does** NativeAOT-publish (5.5 MB binary, 364 MB image), `obj` finds `/app/models`, and the plain
  sandbox reports `11 bodies` with the sim clock advancing to `t=1.0s` — so **Box3D's native P/Invoke loads and steps
  under NativeAOT**, which was the one part of this nobody had ever tried. Build notes read `5 model(s) found.` and
  `5 model(s) staged at /app/models.`

  One cosmetic difference under AOT: System.CommandLine prints the usage line as `Jumbee.Console [command]` rather
  than `Jumbee.Console.3DSandboxDemo` — it derives the program name differently from a trimmed native binary. Left
  alone.

- **Sleep dimming removed.** `Palette.For` drew a sleeping body at a third brightness, to make the engine's sleep
  behaviour visible. It went, and the reasoning is worth keeping: a settled pile is the *common* case, so most of the
  scene sat dimmed most of the time, and the one thing it degraded was the lighting — which is the whole point of the
  shaded renderer. It also read as a lighting bug rather than as information: three boxes falling asleep on the same
  tick dim simultaneously, and the selected body is exempt (`Palette.Selection` bypasses `Palette.For`), so one object
  stays bright while its neighbours darken — which looks exactly like a light moving. Nothing is lost: the footer and
  the Scene panel both report the awake count in words, and the step-time readout visibly drops when the solver stops
  working, which is a better demonstration than a colour nobody can interpret.

  Two pieces of prose justified themselves by it and were rewritten, not just re-worded: `Program.Populate`'s comment,
  and — the one worth knowing about — the silhouette section of `docs/3D Rendering in a Terminal.md`, which explained
  why outlines *brighten* by pointing at the third-brightness sleeping body. The design is still right, but the reason
  is now the general one: an outline inheriting its surface colour vanishes on the unlit side of a body, which is
  where a silhouette matters most.

### The sandbox sidebar scrolls, and the trap in making it

`SpacedRows` is demoted rather than deleted: the panel still picks the compact layout in a short terminal, because
seeing the whole panel beats scrolling for the camera pad. Scrolling is the **backstop** under that, so a stale
`SpacedRows` now costs a scroll instead of a section, and the undocumented compact floor (42 rows, and 40 and 36
before it) is gone — the harness passes at every height from **34** rows up, where the old floor was 42, and the app
itself renders and scrolls correctly at 30.

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

**The sidebar layout constants are hand-maintained and bit three times this session.** `SidebarPanel.SpacedRows` is
now 53 and the compact tier's floor is a 40-row terminal; neither is derived from anything. A stale `SpacedRows`
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

Still open from the vtop eval loop. Evidence for each is in `eval-findings.md`.

1. **V-13 — `ControlFrame` can't put a second label on a border edge.** Four separate runs hit this; it's why
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
