# Handoff — end of the 0.1.10 session (2026-08-02)

Living "where we are / what's next" note. Companion to [`eval-findings.md`](eval-findings.md), which is the full
backlog with evidence; this is the short version plus the operational context you'd otherwise have to rediscover.

## Next session starts here

**Build the 3D physics sandbox.** The plan is written and approved:
[`3D Sandbox Plan.md`](3D%20Sandbox%20Plan.md). Read it first — everything in it was verified against the sources,
not recalled, and it carries the API facts, the maths, and the file references so you don't have to re-derive them.

Two decisions already made, so don't reopen them:

- **Solid shading is in v1.** Wireframe alone is an inertia port; the half-block z-buffered renderer is the reason
  to build this at all.
- **Fix the `Canvas` batch API properly in the library**, not around it in the demo. Note the cost model is milder
  than first thought: `Rebuild()` is only `_dirty = true; Invalidate();` (`Canvas.cs:294`) and `BuildLayers` runs
  once per frame, so per-shape `Add` costs a closure and a redundant invalidate — ~12k allocations/sec at 200
  shapes and 60 fps — not a re-rasterisation. Worth fixing, **not** a prerequisite for starting.
  `DamageTracking` is *not* a mitigation and should stay off for the 3D view: it narrows the compositor's scan,
  not `Add`/`BuildLayers`, and an orbiting camera changes nearly every cell every frame.

Scaffold exists at `examples/Jumbee.Console.3DSandboxDemo` (Box3D.NET 0.3.0 referenced, `PublishAot`, `Program.cs`
still the template hello-world).

## Where things stand

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
3. **V-21 — no proportional split and no layout-changed event.** The 3D sandbox will want the second one; that
   would make three demos asking.
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
