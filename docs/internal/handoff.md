# Handoff — end of the M3 UI session (2026-08-14)

Living "where we are / what's next" note. Companion to [`eval-findings.md`](eval-findings.md), which is the full
backlog with evidence; this is the short version plus the operational context you'd otherwise have to rediscover.

## Next session starts here

**The 3D sandbox is through M3.5**, committed up to `159f8b9`. Read
[`3D Sandbox Plan.md`](3D%20Sandbox%20Plan.md) first — it carries a result section per milestone with the measured
numbers and every finding, written as the work happened rather than recalled afterwards.

**Next: M4 — polish.** Colour modes (velocity / mass / sleep), motion trails, scene presets (stack, tower, pyramid,
domino run, wrecking ball). Then M5 — ship: promote the harness, README, gallery entry, Docker target.

The demo now has a **complete mouse-driven UI**, which was the whole of M3 and more than the milestone originally
scoped. Every keyboard action has a mouse route: a `MenuBar`, a 32-column sidebar (Scene / Render / Spawn / World /
Inspector / Camera), a camera D-pad, and a runtime model loader. `SandboxShell` assembles both scenes so the
headless harness drives the **real** shell rather than a rebuild of it.

**Seven library changes landed**, all driven by something the demo could not otherwise express:

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
4. ~~**Should the wireframe renderer draw a convex hull for mesh bodies?**~~ **Largely answered without the hull.**
   The sparse cloud was three separable problems, not one: a flat 64-edge cap that ignored how big the body was on
   screen, thinning by *edge* so every pick landed as a floating segment, and no backface culling so the far side
   was interleaved with the near one. Fixing all three — a budget of one triangle per 40 sub-pixels of projected
   area, whole triangles kept together, back faces culled — makes the bunny and the teapot read as themselves at
   viewer size, and leaves sandbox bodies denser than before at a floor of 64 triangles. Costs 0.4 ms/frame for the
   bunny at 200×50, 1.0 ms for the 250k-triangle dragon (which is dominated by the pre-existing per-vertex
   transform, not the new work). **Still open for very dense meshes**: at 250k triangles a sampled triangle is
   sub-pixel, so the dragon is a correctly-shaped cloud rather than a surface. That case is what hull edges would
   actually fix, and Box3D still does not expose hull geometry.

   **A second bug lived inside the first fix**, and it is the more interesting one — see follow-up 2 in the plan
   doc. Finding visible faces and choosing which to draw in one pass ends the walk partway down the triangle list,
   and OBJ index order is spatially coherent, so the tail of the model is never considered. Models drew with
   chunks missing, and the missing chunk moved as the camera orbited. The lesson generalises past this renderer:
   **when a thinning step has a fixed output size, every measure of output volume is right by construction and only
   the distribution can be wrong** — so lit-cell counts, byte counts and bounding boxes all pass. The harness now
   has an occupancy-grid check with front-facing ground truth (89% correct vs 60% buggy); the bounding-box version
   I wrote first scored 93% with the bug reintroduced.
5. **Vendor a model, or keep pointing at `reference/`?** Unchanged. The demo ships only a generated torus knot.
   If any model is vendored, `THIRD-PARTY-NOTICES.TXT` needs it.

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
5. **V-31 — `DataTable` grid style.** Attempted in the 0.1.9 session and **reverted**; see `eval-findings.md` for
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
