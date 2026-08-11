# Handoff — end of the 3D sandbox session (2026-08-10)

Living "where we are / what's next" note. Companion to [`eval-findings.md`](eval-findings.md), which is the full
backlog with evidence; this is the short version plus the operational context you'd otherwise have to rediscover.

## Next session starts here

**The 3D sandbox is through M2.5**, committed up to `3d79a59`. Read
[`3D Sandbox Plan.md`](3D%20Sandbox%20Plan.md) first — it now carries a result section per milestone with the
measured numbers and every finding, written as the work happened rather than recalled afterwards.

**Next: M3 — the sidebar UI.** Sidebar panels (mode, params, spawn, inspector showing the selected body's
mass/velocity/sleep state), footer key hints, F1 help via `HelpInfo`. The footer already carries a live inspector
line, so M3 is mostly moving that into a docked panel and adding the spawn/params blocks.

One library change also landed this session, outside the demo: `src/Jumbee.Console/Input/VtInputSource.cs` no
longer dies when a console read fails, plus `tests/Jumbee.Console.Tests/VtInputSourceTests.cs`. See the operational
note below for what it was.

### Open questions the next session should decide

1. **Should the wireframe renderer draw a convex hull for mesh bodies?** Right now it draws a thinned sample of
   the mesh's edges, capped at 64, and a dense model reads as a sparse cloud rather than a shape. The principled
   fix is hull edges (~30–60 edges, a real silhouette) but Box3D does not expose its hull's geometry, so it means
   writing a hull ourselves. Documented as a known limitation; nobody has asked for it yet.
2. **Vendor a model, or keep pointing at `reference/`?** The demo currently ships only a generated torus knot, so
   the mesh path works with no third-party asset. voxcii's *code* is MIT but its *model files* have their own
   provenance (the Stanford bunny in particular). If any model is vendored, `THIRD-PARTY-NOTICES.TXT` needs it.
3. **Promote the parked harness into a real test project.** 63 checks; it caught two shipped bugs (dead arrow
   keys, a clipped footer) plus several wrong assumptions of mine. Parked at
   [`scratch/`](scratch/README.md) — sources only, not wired into the solution, so it will rot unless adopted.
   M5 already lists this; it is worth doing sooner.

## Where things stand — the 3D sandbox

Milestones M0 through M2.5 are done. `examples/Jumbee.Console.3DSandboxDemo` has a README with the full key map.

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

**Five findings that cost real time**, all written up in the plan doc:

1. `CompositeControl` with no focusable children never receives `OnInput` from the layout route — handle keys in
   `InterceptInput`. Worse, `UI.SendInput` takes a *different* path, so the obvious test is green while the app is
   dead. This shipped, and the user found it, not a test.
2. `FillsFrameViewport` has a silent failure mode: without it a framed viewport balloons to the 1000-row clamp and
   renders *empty*, which reads as broken projection maths.
3. Backface culling sign: inverting Y for screen rows reverses handedness, so an outward-facing triangle arrives
   with a **negative** signed area. Culling `<= 0` discards every visible face.
4. `Body.AddMesh` requires a **static** body, so a mesh body must collide as a convex hull.
5. **Judge renders from a PNG, never an ASCII dump.** See below.

**0.1.10 is packed and committed** (`artifacts/`, three packages, not published). The session was documentation-led
and produced one breaking change:

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

Full suite **929/929** in Debug and Release. `ProjectAssemblyVersion` is `0.1.10`.

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
