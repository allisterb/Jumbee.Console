# Eval findings — jc-curious / reviewer loop

Running backlog of Jumbee.Console API/doc gaps and bugs surfaced by the **jc-curious** port eval loop and its **jc-curious-reviewer** critic (see `.claude/agents/jc-curious*.md`). Each item is a *candidate* fix; graduate accepted ones into `CHANGELOG.md`. This log is the backlog, not a commitment.

Port targets so far: **eilmeldung** (RSS reader, rounds 1–4 below) · **scope-tui** (audio scope) · **vtop** (activity monitor, see the vtop section at the end). The agent `.md`s are single-target and get rewritten per target; prior briefs are in git history.

**Legend** — type: `doc-gap` · `capability-unknown` · `missing-feature` · `bug`. severity: `blocker` · `major` · `minor`. status: `open` · `fixed (ver)` · `documented (ver)` · `dismissed`.

---

## Round 1 — 2026-07-20 (foundation build: 3-region shell, off-thread fetch, custom row/tree/panel, 7 snapshot checks)

| ID | Sev | Type | Finding | Source | Status |
|----|-----|------|---------|--------|--------|
| R1-1 | major | doc-gap | No documented recipe for a **single-child `CompositeControl`** — the only worked example (`CodeEditor`) arranges ≥2 children via a `Layout`. Blocked wrapping one control (`ReaderPane`, `ArticleListPanel`) as a real composable control; fell back to thin wrapper classes. | jc-curious + reviewer | open |
| R1-2 | major | doc-gap | **`Grid(int[] rowHeights, int[] columnWidths, …)` sizing semantics undocumented** — fixed cells vs `0`=fill (as `DockPanel` uses) vs proportional/star. The API page explicitly says it doesn't cover this. Blocked the composable single-child-in-a-Grid path. | jc-curious | open |
| R1-3 | minor | bug / doc-gap | `SplitPanel.SplitPosition` **floors at 1 cell even with `MinFirst = 0`**, contradicting the doc's "setting SplitPosition to MinFirst collapses the first pane." Harmless visually but cost a debug cycle. Fix: allow a true 0, or document the floor. | jc-curious + reviewer | open |
| R1-4 | minor | missing-feature | **`Tree.TreeNode` (and `ListBox.ListBoxItem`) have no user-data / `Tag` slot** to bind a node/row to a domain object — forces a side `Dictionary<TreeNode, Feed>`. A generic payload (or `Tag`) would remove a bespoke pattern every real app needs. | jc-curious | open |
| R1-5 | minor | doc-gap | **`Jumbee.Console.Tree` vs `Spectre.Console.Tree` name collision** — near-certain once an app does what the docs recommend (build `IRenderable` rows, so `Spectre.Console` types are in scope). Not documented as a gotcha. | jc-curious | open |
| R1-6 | minor | doc-gap | **`MarkdownViewer` namespace is undiscoverable** — no `docs/api` page, and it lives in core (`Jumbee.Console`), not `Jumbee.Console.Documents` where the other viewers live. Only resolvable by re-reading the getting-started example's `using` list. | jc-curious | open |
| R1-7 | minor | capability-unknown | **No documented per-item "update one row" path** — `ListBox.Update()`'s contract is undocumented, so a read-state toggle rebuilds the whole list (`Clear()` + `AddItems()`). Won't scale to hundreds of rows. | jc-curious + reviewer | open |
| R1-8 | minor | capability-unknown | **`ConsoleSnapshot.ToTextAfter(Control, …)` renders the whole ambient parent tree**, not the control in isolation (a deeply-nested control still exercised sibling panes). Load-bearing but undocumented; jc-curious relied on it without knowing it was intended. | jc-curious | open |

**Round-1 status updates after round 2:** R1-7 **resolved** — `ListBoxItem.Content` setter *is* the documented per-item update path ("re-measures the list") and works cleanly; the confusion was only `ListBox.Update()`'s unclear contract. R1-1 **workaround found** (doc gap remains) — a single-child-ish composite was achievable by subclassing `CompositeControl` with nested `DockPanel`s (`ReaderPane`); still no *single-child* example in the docs.

## Round 2 — 2026-07-20 (tree→list query service in Core, real measured `IRenderable` row + per-item update, palette/reader fidelity; 11 snapshot checks)

| ID | Sev | Type | Finding | Source | Status |
|----|-----|------|---------|--------|--------|
| R2-1 | major | doc-gap / missing-feature | **No scoped/local hotkey**, and a delegate-level guard (`if (UI.Focused is TextInput) return;`) does NOT work: a matched global hotkey is marked handled and swallowed *before* the delegate runs, so a letter key never reaches a focused text field. Only fix is manual `UnregisterHotKey`/`RegisterHotKey` bracketing around every focus transition — easy to forget. Fix: a hotkey-scope/local-binding concept, or at minimum document the bracket pattern. | jc-curious | open |
| R2-2 | major | bug (packaging) | **`Jumbee.Console.Styles.xml` is not in the NuGet package** — CONFIRMED: `Jumbee.Console.0.1.3.nupkg` `lib/net10.0/` has `Jumbee.Console.Styles.dll` + `Jumbee.Console.xml` but no `Jumbee.Console.Styles.xml`. So `Color`/`IStyleTheme`/`IGlyphTheme`/`ITheme` have zero IntelliSense/doc surface for a consumer — blocks the whole theming/fidelity axis. Fix: enable `GenerateDocumentationFile` for Jumbee.Console.Styles and include its `.xml` in the bundle. **Highest-value easy fix.** | jc-curious (verified) | open |
| R2-3 | minor | bug / doc-gap | **`Tree.TreeNode.UpdateTree()` is documented as public** ("Requests a redraw of the owning tree…") **but the compiler reports `CS0122` inaccessible.** Doc/reality mismatch. Fix: make it public as documented, or correct the doc's accessibility. | jc-curious | open |
| R2-4 | minor | missing-feature | **`Tree` has no `SelectionChanged`-style event** — only `NodeActivated` (leaf, Enter/double-click). "Live-filter while arrow-navigating the tree" (standard RSS-reader UX) had to be built by polling after each forwarded key. Fix: a selection-changed event. | jc-curious | open |
| R2-5 | minor | doc-gap | **`Tree`'s initial-selection / first-keypress behavior is undocumented** (does the first `Down` select the root or its first child?) — had to determine empirically. | jc-curious | open |
| R2-6 | minor | capability-unknown | **`ListBox.Items` is `ICollection<ListBoxItem>` (not indexable)** — asserting "only row N changed" in a test needed `.ElementAt(n)` + reference-equality rather than an index compare. Minor testing friction. | jc-curious | open |

**Round-1/2 status updates after round 3:** R1-4 still open (no data/`Tag` slot — still needs side dictionaries) BUT `Tree.TreeNode` turned out to have rich, XML-documented per-node *styling* (`LeafGlyph`/`ExpandedGlyph`/`CollapsedGlyph`/`*GlyphColor`), so tree restyling was achievable — the gap is data-binding, not styling. R2-2 **narrowed**: tree styling came from `TreeNode`'s own properties (documented in `Jumbee.Console.xml`), so the missing `Styles.xml` was less blocking than feared for *this* — but the broader `Color`/`IStyleTheme`/`IGlyphTheme` surface is still undocumented, and R3-3 shows the same packaging class hits bundled Spectre too. R2-4 **reconfirmed** (see R3 note): `ListBox` has `SelectionChanged`, `Tree` doesn't — asymmetry.

## Round 3 — 2026-07-20 (single update flow + state-drift bug fix, row caching + measured-width columns, eilmeldung tree look + saved queries, isolated PNG captures; 14 snapshot checks)

| ID | Sev | Type | Finding | Source | Status |
|----|-----|------|---------|--------|--------|
| R3-1 | major | capability-unknown / doc-gap | **`UI.RegisterHotKey` is process-global static state with no per-instance scope.** Constructing a second app (what "fresh app per PNG/test capture" needs) re-registers the same keys and silently re-points an *earlier* instance's `routeGlobal` input at the *newer* one — no exception, the old instance just stops responding. Surfaced as two flaky, log-less test failures. Directly breaks the multi-instance headless-testing workflow the Snapshot story encourages. Fix: document that hotkeys are process-global (not scoped to the `UI.Start` root), and/or offer an instance-scoped hotkey table. **Ties to R2-1.** | jc-curious | open |
| R3-2 | major | missing-feature | **`TreeGuide` has no `None`/connector-less mode** — only `Ascii`/`Line`/`BoldLine`/`DoubleLine`. eilmeldung's tree has *zero* connector lines (hierarchy = icon + indent). Only workaround is dimming `guideStyle` to near-invisible, which is theme-fragile (a light theme makes the lines reappear). Fix: add `TreeGuide.None` (or a hide-guides flag). | jc-curious | open |
| R3-3 | minor | doc-gap (packaging) | **Bundled `Spectre.Console.dll` inside the core package ships with no XML docs** (only `Jumbee.Console.xml` is present) — so `GetCellWidth`/`Segment.CellCount`, the exact primitives needed for correct column-width layout (vs `string.Length`), are bare signatures with no guidance a Jumbee-only consumer could discover. Same packaging class as R2-2. Fix: consider shipping the bundled forks' XML docs, or surface the key primitives in Jumbee's own docs. | jc-curious | open |
| R3-4 | minor | capability-unknown | **No render-invocation hook** to assert "a control's `Render` was NOT re-called this frame" via the Snapshot API — so a per-item caching claim (`ArticleRow`) can be exercised (content still correct) but not directly *proven* headlessly. Fix: a render/frame counter hook for tests that assert render-frequency. | jc-curious | open |

## Round 3 reviewer notes (candidates to confirm — not yet jc-curious-verified library findings)

- **C-1 (investigate)** — `MarkdownViewer` body **clips instead of word-wrapping** inside a width-constrained split pane (wraps fine in zen/full-width). If real, a long article loses everything past the first line — a genuine wrapping bug worth confirming against the library, not just her layout.
- **Lesson (eval harness, not a library gap):** jc-curious's snapshot checks assert **model state** (indices, counts, dot glyphs), so two *visible* regressions (age column absent, tree icons = tofu) shipped "green." Tests must also assert **rendered text** (`ToText`) — the recurring "test observable behavior" principle. Also her `ArticleRow` cache key omits time-derived `Age` (would freeze in a long session) — her bug, not the library's.

## Round 4 — 2026-07-20 (modernize eilmeldung to 0.1.4 + fix the r3 visible defects; 18 snapshot checks, rendered-text assertions)

Modernization **validated the 0.1.4 doc/packaging work**: jc-curious found & adopted all of `TreeGuide.None`, `TreeNode.Tag`/`ListBoxItem.Tag`, `Tree.SelectionChanged`, `UI.HotKeys.Char`, and the now-shipped `Styles` XML (glyph default values visible) — **from the doc mirror alone**, no source. The CHANGELOG "named the exact bug in the exact code pattern" she'd shipped. Two NEW findings:

| ID | Sev | Type | Finding | Source | Status |
|----|-----|------|---------|--------|--------|
| R4-1 | major | bug | **`MarkdownViewer` does not word-wrap plain paragraph text — it clips mid-word** and drops the tail, despite its doc implying it reflows to the control width. Reproduced on a BARE `MarkdownViewer` at width 40 (not a composite/nesting issue). Confirms round-3 candidate C-1. | jc-curious (bare-control repro) | **fixed (0.1.4)** — root cause: the markdown write path clips at the buffer edge (`wrap=false`); the shared char-level `wrap` is owned by TextEditor's caret math so couldn't change. Added opt-in `AnsiConsoleBuffer.wrapWords` (word-boundaried + char fallback); `MarkdownViewer` enables it. `MarkdownViewerWrapTests` guards it. |
| R4-2 | major | doc-gap | **`ListBox` calls an item's `Render(options, maxWidth)` at a large probe width (observed ~1000), not the item's real on-screen column width.** A custom row that trusts `maxWidth` for right-alignment/multi-column layout paints past the real edge and gets clipped (the "vanished age column" root cause). Fix: clamp to `List.ActualWidth`. Undocumented — nothing in `ListBox`/`ListBoxItem` XML says which width governs layout vs. the probe measure. | jc-curious | open |

## 0.1.4 disposition (2026-07-20)

Graduated into 0.1.4 (see `CHANGELOG.md`):
- **Fixed (code/packaging):** R2-2 + R3-3 (bundle private assemblies' XML docs — Styles/Spectre/etc., verified in the packed nupkg) · R1-4 (`TreeNode.Tag` + `ListBoxItem.Tag`) · R2-3 (`TreeNode.UpdateTree()` → public) · R2-4 (`Tree.SelectionChanged`) · R3-2 (`TreeGuide.None`). New `TreeApiTests.cs` covers the four Tree/item adds; full suite 819/819.
- **Documented:** R2-1 + R3-1 (hotkey process-global scope on `UI.RegisterHotKey`) · R1-2 (`Grid` fixed-cell sizing) · R1-3 (`SplitPanel.MinFirst` 1-cell floor) · R1-1 (single-child composite = `SetContent(new Boundary(child))`) · R1-5 (`Tree`/`Spectre.Console.Tree` `CS0104` note) · R1-6 (`MarkdownViewer` is in core, not Documents). *(XML-doc changes reach the generated `docs/api/*.md` on the next `pack`/`build-api-docs`.)*

Still **open** (not in the graduated clusters, all minor): R2-5 (`Tree` initial-selection behavior undocumented) · R2-6 (`ListBox.Items` not indexable) · R3-4 (no render-invocation hook to prove caching headlessly) · C-1 (investigate `MarkdownViewer` clip-vs-wrap in a constrained pane). A bigger design item deferred past docs: **scoped/focus-routed hotkeys** (R2-1/R3-1) — 0.1.4 only documents the global model; instance-scoped input is a 0.1.5+ decision.

## Highest-value fix candidates (across all rounds)
1. **R2-2** — ship `Jumbee.Console.Styles.xml` in the package (packaging omission; unblocks the whole theming/`Color` doc surface). Easy + high value.
2. **R2-1 + R3-1** — hotkey scope: document the process-global model and the unregister/re-register pattern, and/or add scoped/instance hotkeys. Two independent rounds hit this; it breaks focused-text-entry AND multi-instance testing.
3. **R1-1 + R1-2** — `Grid` sizing semantics doc + a single-child `CompositeControl` example (blocked composable custom controls).
4. **R3-2** `TreeGuide.None`; **R2-4** `Tree.SelectionChanged` (mirror `ListBox`); **R2-3** `TreeNode.UpdateTree()` accessibility fix; **R1-4** `TreeNode`/`ListBoxItem` `Tag` data slot — a cluster of small `Tree`/item API gaps.

---

# vtop port — 2026-07-31 (target #3, against published 0.1.7)

Two rounds against the **published 0.1.7 packages** (not a preview feed): jc-curious built a .NET port of [vtop](https://github.com/MrRio/vtop), the Node activity monitor whose signature is CPU/memory charts drawn as **braille-filled columns** via `drawille`. The reviewer critiqued between rounds. Workspace `C:\Users\Allister\Agents\jc-curious\vtop`; reference at `reference/projects/vtop-master`.

**Why this target:** a .NET dev offered on [ConsoleGUI issue #10](https://github.com/TomaszRewak/C-sharp-console-gui-framework/issues/10#issuecomment-5122659775) to port vtop to Jumbee.Console and other libraries for a performance comparison. We deliberately **did not pre-build a braille area/sparkline control** before the run, so the eval could tell us whether the existing surface composes. It did — see V-1.

| ID | Sev | Type | Finding | Source | Status |
|----|-----|------|---------|--------|--------|
| V-1 | major | doc-gap | **`Plot`'s bar methods don't cross-reference `Canvas`/`FilledLine`, the actual answer for a filled/area/braille chart.** `Plot.AddBars`/`AddLiveBars` take no `PlotBrush` (verified), so there is no braille bar/area chart on `Plot`. Round 1 burned an entire round approximating the fill with a dense scatter point cloud (~200 points/column, ~20,000 points/chart rebuilt every tick) because `Plot` is what a search for "chart" surfaces first and nothing there points elsewhere. Round 2 found `Drawing.FilledLine` — "a line whose area between the line and `FillToY` is filled" — which **is** vtop's per-column fill as one documented primitive: one shape per column, exact sub-cell rasterization, zoom free via `XBounds`. `Canvas`'s page answered the question completely once she was on it. Fix: cross-link from the bar-chart methods. | jc-curious | **documented** — `<remarks>` added to `Plot.AddBars`/`AddLiveBars` |
| V-2 | major | doc-gap | **`DockPanel.DockedControl`'s "0 = fill the parent" trap names the symptom but not the cure.** Its remarks correctly warn that a 0-sized docked control "takes the whole panel and starves the fill control" and say to use a positive extent — but never say *how* when the child has no `Width`/`Height` of its own (`ControlFrame` has neither). `Boundary` is the fix and was named nowhere on `DockPanel` or `Grid`. Cost the most debugging time of round 1: a framed chart docked at the top silently ate the whole screen, no error. Her "fix this first" in both rounds. Recurrence of R1-1/R1-2 from a fresh angle. | jc-curious | **documented** — `<remarks>` on `DockPanel.DockedControl` + `Grid` ctor now name `Boundary` |
| V-3 | major | doc-gap | **`DataTable.HandlesInput` renders the *inherited* base-class summary** — "the default (`false`) ignores it" — on an override that returns `true` (`DataTable.cs:46`; `WantsMouse` likewise, `:48`). Nothing on the page states what the override returns. The evidence is unusually strong: she concluded the table's keyboard/mouse story "is not usable as documented" and **deleted working input handling**, hand-rolling global hotkeys instead. Re-tested on an untouched `DataTable` in round 2: arrow-key nav works with zero configuration. Root cause: **CS1591 does not fire on overrides** (they inherit docs implicitly), so an override with `<inheritdoc/>` — or no doc at all — compiles clean while docfx renders the base class's summary. Fix: give the overrides their own `<summary>`. | jc-curious + orchestrator verification | **documented** — own `<summary>` on both overrides, plus the 7 other controls with the same defect |
| V-4 | major | **doc-gap** (was filed as capability-unknown — **that was wrong**) | **Headless `Dialog`/`Overlay` testing is fully supported and completely undocumented.** Three independent cold-start attempts across two runs all concluded it was impossible; the third finally surfaced the answer in the exception text it hit — *"No ambient UI.Overlay is available. Call UI.Start first, **or use Show(overlay)**"*. `Dialog.Show(Overlay)` (`Dialog.cs:146`) and `Overlay(ILayout)` (`Overlay.cs:28`) are both public, and **our own `tests/Jumbee.Console.Tests/DialogTests.cs` has done exactly this all along**: `new Overlay(bottom)` → `UI.Overlay = overlay` → `ConsoleSnapshot.Render(overlay, w, h)` → `dialog.Show()` → assert. Verified 2026-07-31: **10/10 pass.** The failing consumer attempts all passed the *root layout* to the snapshot instead of the *overlay*. No API work needed. Fix: put that pattern verbatim in GETTING-STARTED's "Testing without a terminal". | jc-curious (3 attempts, 2 runs) + orchestrator verification | **documented 2026-07-31** — new "Testing a modal dialog" subsection in `GETTING-STARTED.md`, written as a runnable snippet, verified to compile and pass verbatim before shipping |
| V-5 | minor | missing-feature | **No simulated mouse input in `ConsoleSnapshot`** — zero hits for "Mouse"/"Click" in `Jumbee.Console.Snapshot.xml`. `ToTextAfter` covers keys only, so click-to-select and scroll-wheel (both real vtop features, and `DataTable` supports them) are structurally untestable headlessly. Pairs with V-3: she could not re-verify click-to-select even after the correction. | jc-curious | **fixed 2026-07-31 (0.1.9)** — added `Click`/`MouseMove`/`Wheel`/`ResetMouse` + `RenderAfterClick`/`ToTextAfterClick`. Implementation note: `ConsoleSnapshot` composes into its own buffer, not `ConsoleManager`'s, so it can't drive `ConsoleManager.MousePosition`/`MouseDown` — but `Control` tags its cells with a `MouseContext` (when `Focusable \|\| WantsMouse`), so the hit-test is a bounds-checked cell lookup, and the dispatch order (leave → enter → move, then down/up) is copied from `ConsoleManager.MouseContext`'s setter. Hover state is static like the runtime's, hence `ResetMouse`. 7 tests in `SnapshotMouseTests`. |
| V-6 | minor | doc-gap | **`Jumbee.Console.Style` is documented in `Jumbee.Console.Styles.xml`, not `Jumbee.Console.xml`** — the type's namespace (`Jumbee.Console`) doesn't match its assembly (`Jumbee.Console.Styles`), so a consumer grepping the obvious XML file finds nothing. She concluded `Style` was wholly undocumented (it has **293 documented members**, and the file *does* ship in 0.1.7 — the R2-2 packaging bug is fixed), reflected the assembly instead, found the get-only computed `ForegroundColor`/`BackgroundColor` (`Style.cs:84-87`), assumed they were settable, and filed a spurious major against `Footer.KeyStyle`. The real answer is `new Style("black on white")` — the only constructors are a Spectre wrap and a markup-string parse. Fix: note the assembly split in the theming docs, or `<seealso>` from types that expose `Style`. | orchestrator (jc-curious's own finding was wrong) | open |
| V-7 | minor | capability-unknown | **No "any key" / catch-all input hook.** `UI.GlobalInputListener` dispatches only exactly-registered chords, so vtop's `lastKey`-reset idiom (a `dd` sequencer that resets on *any* other keystroke) can only be approximated by resetting from every key the app happens to register. | jc-curious | open |
| V-8 | minor | missing-feature | **`Footer` has no right-aligned/trailing-text slot** (vtop pushes its URL to the far right of the hint row). | jc-curious | open |

**Dismissed / corrected mid-run** (recorded so they aren't re-filed):
- *"`Plot`'s axis/grid/tick chrome is undocumented"* — **wrong**; `ConfigureAxis`/`ConfigureGrid`/`ConfigureTicks` appear 15× on the Plot API page. Narrowed to the real residue: no worked "bare, chrome-free chart" example, and the settings types (`AxisSettings`/`GridSettings`/`TickSettings`) live in the vendored `ConsolePlot.xml`, one level removed from `Jumbee.Console.xml`.
- *"`DataTable.HandlesInput`/`WantsMouse` are documented as settable but aren't"* — **wrong**; the signatures on the page are correct. The real defect is V-3.
- *"`Footer.KeyStyle` doesn't render a background"* — **wrong**; `Footer.Render` emits the key segment with the full style (`Footer.cs:93`). Root cause was V-6.

**Reviewer notes (app quality, not library findings)** — the round-1 build had defects the library can't be blamed for, kept here as evidence of what a cold-start dev actually ships: charts auto-scaled to the data instead of pinning 0–100 (34% memory drew full height, so idle and saturated looked identical); process CPU% was *cumulative lifetime* time rather than a rate; memory% divided by the sum of working sets so the column always totalled 100%; several hundred undisposed `Process` handles leaked per second; `q`/`Esc` were never bound at all. All fixed in round 2. The reviewer's framing is worth reusing for any monitor-class target: *per-frame cost is a product feature, not a micro-optimisation.*

## Cold-start re-run — 2026-07-31, against `0.1.8-preview` (post doc-fix)

After shipping V-1/V-2 (the two cross-links) and V-3, a **fresh** jc-curious cold-started the same port against a `0.1.8-preview` snapshot, in a new workspace, explicitly forbidden from reading the earlier attempt. Purpose: measure whether the doc fixes are findable by someone who doesn't already know the answer.

**Result on the headline question: much better.** Round 1 last time lost an entire round to the `Plot` scatter-cloud workaround. This time she reached `Canvas` + `Drawing.FilledLine` in two doc hops and shipped a ~110-line chart with no source-reading and no guessing.

**But do not over-credit the fixes — neither cross-link was on her path.** Her trace: `GETTING-STARTED.md` "Readouts & charts" → tried `Sparkline` first → bounced off it correctly *because its doc says plainly it's single-row block bars, not sub-cell braille* → `Jumbee.Console.md` namespace index → `Jumbee.Console.Drawing.md` (which names `FilledLine` with "useful for area charts") → `Canvas.md`. She never opened `Plot.md`, and she built the shell with `Grid` rather than `DockPanel`, so she never hit the `0 = fill` trap either. The honest read: **the namespace-index route works, and `Sparkline`'s doc correctly steers people away** — both good — but V-1/V-2 remain unexercised and their value is still unmeasured.

| ID | Sev | Type | Finding | Source | Status |
|----|-----|------|---------|--------|--------|
| V-9 | major | missing-feature | **`ConsoleSnapshot.SavePngAfter` has no `routeGlobal` overload** although `ToTextAfter` and `RenderAfter` both do (CS1739). Forces a two-step `RenderAfter(…, routeGlobal: true)` → `SavePng(ConsoleBuffer, path)` workaround, discoverable only by reading the full method list. An asymmetry in the overload set, not a design decision as far as anyone can tell. | jc-curious (cold start) | **fixed 2026-07-31** — added `routeGlobal` to the `ConsoleKeyInfo` overload **and** the two missing `ILayout` overloads (the latter matters for the V-4 modal workflow: a PNG of a dialog needs to capture the *overlay*). Covered by `ConsoleSnapshotTests.SavePngAfter_AcceptsALayoutAndRoutesAGlobalHotKey`. |
| V-10 | minor | missing-feature | **No `Style(Color fg, Color bg)` constructor**, despite `implicit operator Style(Color)` existing for the single-colour case (CS1729). The workaround is to bridge through `new Spectre.Console.Style(foreground:, background:)` — an idiom documented only as an aside on `IStyleTheme.LabelText`, not on `Style` itself. **Second run in a row that `Style`'s colour API caused friction** (see V-6, where the get-only `ForegroundColor`/`BackgroundColor` were mistaken for settable). Two independent devs, two different wrong turns, same type. Fix: a two-colour ctor or factory on `Style`. | jc-curious (cold start), reinforces V-6 | **fixed 2026-07-31** — added `Style(Color foreground, Color background)`. Decoration still composes via `|` (`new Style(fg, bg) \| Style.Bold`) rather than becoming a third parameter, since there is no Jumbee `Decoration` enum in the Styles project and taking Spectre's would leak it onto a public signature. Covered by `StyleApiTests` (4 cases, incl. equivalence with the markup and `Bg(..)\|` forms). |
| V-11 | minor | capability-unknown | **No documented `Canvas` data-coordinate → screen-cell mapping**, so a snapshot test cannot assert "value X paints around row Y" for a braille chart. Her `GlyphAt` probe returned identical ink for an all-0% and an all-100% series at the row she guessed, so she could only prove the *readout text*, not the bar geometry — the actual thing the chart exists to draw. Fix: document the `XBounds`/`YBounds`-to-cell formula, or add a canvas-aware snapshot helper (e.g. ink density within a region). | jc-curious (cold start) | open |
| V-12 | minor | missing-feature | **A framed control that holds focus overrides the theme's border colour** (`IStyleTheme.FocusedFrameBorder`), with no documented way to opt a frame out. vtop has no focus cue — all three of its panels share one border colour — so the focused Process List renders a different border from the two chart panels and breaks theme consistency (visible in `06-theme-gruvbox.png`). `RendersOwnFocus` exists but isn't presented as the consumer-facing opt-out. Raised independently by both jc-curious and the reviewer. | jc-curious + reviewer (cold start) | open |
| V-13 | minor | missing-feature | **`ControlFrame` has exactly one `Title`/`TitlePos` slot**, so vtop's title-left + live-percentage-right *in the same border row* can't be expressed; both runs worked around it by printing the readout into the content area instead. **Second occurrence** — raised in the first run too. Fix: a second independent subtitle slot. | jc-curious (both runs) | open |

### The exception storm (answers a direct question from the maintainer, who saw it at runtime)

Both cold starts independently wrote the same construct — `p.TotalProcessorTime` / `p.WorkingSet64` inside `try { … } catch { continue; }`, over every process, every tick. **Two competent developers, two clean-slate attempts, identical code.** This is what a .NET dev naturally writes for this task, not a slip.

Measured with an independent .NET 10 probe running the same loop shape on this machine:

- **~595 processes/tick, ~190 first-chance `Win32Exception: Access is denied` per tick, forever.** The count is stable across four independent measurements (190/191/192) and two different ports, so it's a property of the machine, not of anyone's code.
- **Only `TotalProcessorTime` throws** — `ProcessName` and `WorkingSet64` never do, since those come from the enumeration snapshot.
- **It silently corrupts the data:** `catch { continue; }` discards the whole process, so ~190 of ~595 never reach the UI — their memory and their contribution to `Count` are simply missing, and any total summed over the survivors under-reports by an unknown amount.

**Cost, measured directly (2026-07-31) — and both reviewers were wrong about this, in opposite directions.** A full sampler-shaped pass over 595 processes (enumerate, then read CPU time + working set + name each) is **~17 ms**: ~12 ms of that is `Process.GetProcesses()` itself and only ~5 ms is the per-process reads *including* all 190 throws. Three consecutive passes: 31.7 / 17.5 / 16.5 ms.

- The **first** reviewer's "~23 ms/tick" was roughly right on the total but misattributed it to the exceptions; the enumeration dominates.
- The **second** reviewer claimed **9.8 s per pass**, with 192 processes on a ~50 ms slow path, and concluded the app "delivers one sample every ten seconds". **Not reproducible — wrong by roughly 600×.** In my probe *zero* processes took over 10 ms. Most likely it measured with a debugger attached, where first-chance exceptions are enormously more expensive than in a normal run.

**Net:** the storm is real and worth fixing (it's ugly in a profiler, it inflates debugger runs, and the swallow corrupts the data), but it is **not** a throughput problem — a 1 Hz sampler has ~17 ms of work per tick, not 10 s. **Lesson: measure agent-reported performance numbers before repeating them.** Two reviewers produced confident, precise, mutually incompatible figures for the same loop.

**Not a library finding**, but it matters to this project: the port exists so an outside dev can benchmark Jumbee.Console against other TUI frameworks ([ConsoleGUI issue #10](https://github.com/TomaszRewak/C-sharp-console-gui-framework/issues/10#issuecomment-5122659775)). A monitor burning ~20 ms and hundreds of exceptions per second in its *metrics* layer will be read as the *UI* being slow. Worth raising with anyone who takes on that comparison — and worth a note in the docs if we ever ship a system-monitor sample.

## Second cold-start re-run — 2026-07-31, against `0.1.9-preview` (post V-5/V-9/V-10/V-14/V-15)

Third cold start of the vtop port, fresh agent, new workspace, forbidden from reading either earlier attempt. **This is the run where the fixes finally got exercised.**

**Three of the four doc/API fixes landed directly on her path, and each removed a wall that had cost a previous run:**

| Fix | Evidence from this run | What it replaced |
|---|---|---|
| **V-2** (`Boundary` named on `Grid`/`DockPanel`) | *"`Grid.md`'s remarks (fixed-cell, wrap non-`Width`/`Height` content — frames, nested layouts — in `Boundary`) told me exactly how to nest a header `Grid` and a bottom `Grid` inside an outer `Grid`. **Compiled first try after applying the `Boundary` wrapping rule.**"* | Run 1 lost more debugging time to this than to anything else — a framed chart silently ate the whole screen, no error. |
| **V-4** (modal-testing example) | *"the modal-testing recipe … **is written out almost verbatim in the docs and worked exactly as documented**."* The `dd` confirmation is **snapshot-proved for the first time**. | Three attempts across two runs concluded it was impossible; one shipped a PNG byte-identical to the no-dialog frame. |
| **V-5** (headless mouse) | **M7 reached and proved for the first time in three runs** — click-to-select via `Click`, wheel via `Wheel`. | Both prior runs ran out of budget or hit "structurally untestable". |
| **V-3** (`DataTable` override docs) | *"`DataTable.md` states click-to-select and `WantsMouse` are on unconditionally — **no opt-in code needed**."* | Run 1 read the inherited "default: false" text and **deleted working input handling**. |

**V-1 (`Plot.md` → `Canvas`/`FilledLine`) is still unexercised after two runs.** She reached the primitive in minutes again, but via the *Display Widgets guide → `Drawing` namespace* route, not `Plot.md` — same as the 0.1.8 run. The cross-link is correct and cheap to keep, but two independent cold starts now suggest the namespace/guide path is what people actually walk. Don't count V-1 as validated.

### New findings

| ID | Sev | Type | Finding | Status |
|----|-----|------|---------|--------|
| V-16 | major | doc-gap | **Control pages don't name the theme token that drives their appearance.** She wanted vtop's accent-coloured selected row, found no selection property on `DataTable.md`, saw `DefaultStyleTheme` is `sealed`, and concluded a custom theme might mean implementing ~35 members — so she gave up and filed it as a capability gap. **She was wrong, and the answer was one page away:** `IStyleTheme`'s members are default-implemented (`IStyleTheme.cs:26+` — `Style Text => Style.Grey93;`), and `IStyleTheme.md` says so in as many words. The *previous* cold start found that note and wrote a 5-member theme; this one never reached the page because `DataTable.md` never points there. Same shape as V-1/V-2: the fix exists, the page you start from doesn't mention it. Fix: on each themeable control, name the token its selection/surface colours come from and link `IStyleTheme`. | open |
| V-17 | minor | missing-feature | **No hex-string → `Color` parse.** `Color`'s only constructor is `(byte, byte, byte)`, so loading an external palette (vtop ships `themes/*.json` full of `#a537fd`) means hand-transcribing to `0xNN` literals. She did exactly that for three themes. A `Color.Parse`/`TryParse("#RRGGBB")` directly serves the "themes from external config" case the milestone asks about — and this is the **third** run to hand-roll hex parsing. | open |
| V-18 | minor | doc-gap | **`Control.Feed` is `protected`, and its docs present it as *the* periodic-tick pattern without saying so.** App code composing a stock `Canvas`/`DataTable` can't call it — only a subclass can. She found out by writing the call and hitting a compiler error, then fell back to `Task.Run` + `UI.InvokeAsync`. Fix: one line on `Control.Feed` — it's for controls authoring their own tick; app code uses `Task.Run` + `UI.Invoke`. | open |
| V-19 | minor | missing-feature | **No `TitleBorderStyle.None`** — only `Inline`/`Double`. `Inline` happened to match vtop, but there's no way to suppress title-border decoration entirely. (Note: the default is `Double`, which draws an extra divider under the title; she had to discover `TitleStyle(TitlePos.TopLeft, TitleBorderStyle.Inline)` to get vtop's look.) | open |
| V-13 | minor | missing-feature | **Third occurrence** — `ControlFrame`'s single title slot. vtop puts `CPU Usage` top-left and `19%` top-right *in the border row*; she moved the readout into the canvas content, one row lower. Raised by all three runs now. | open |

### V-20 — the layout docs said what each layout *arranges*, not how it *sizes*

**All three cold starts shipped a fixed-size app** that renders at its hardcoded dimensions and leaves the rest of the terminal empty (the maintainer confirmed it from a live run). Every one of them built the shell from `Grid`.

That isn't a library limitation — the app *is* re-laid-out on resize (`ConsoleManager.AdjustBufferSize` runs each frame and pushes the new size into the root). It's that `Grid`'s extents are absolute, and `Grid` is what `GETTING-STARTED` §3 both demonstrates and lists first. The section listed each layout by *what it arranges* ("rows × columns", "pin one control to an edge") and never mentioned sizing, which is the actual decision.

Measured (framed content, so the border reveals each layout's allocation):

| Layout | 40×10 | 100×30 |
|---|---|---|
| `Grid([5], [30], …)` | 30×5 | **30×5** |
| `DockPanel` | 40×10 | **100×30** |
| `SplitPanel` | 40×10 | **100×30** |
| `VerticalStackPanel` | 40×6 | **100×6** (fills width, height = content) |

**Fixed 2026-07-31:** `GETTING-STARTED` §3 now leads with a sizing table and a "fills the terminal?" column, states plainly that `Grid` is for fixed regions and `DockPanel`/`SplitPanel` for the shell, notes that nesting doesn't change it, and carries a verified nested-`DockPanel` shell recipe (header/status/sidebar/content — snapshot-checked to fill 40×12 and 100×30 before shipping). `Grid`'s own XML remarks now name the consequence directly: *"A grid does not grow with the terminal."*

**Also fixed 2026-07-31:** `docs/controls/` had guides for Composite Controls, Display Widgets, Links and Selection Controls but **none for layouts**. Added [`docs/controls/Layouts.md`](../controls/Layouts.md) (156 lines) — the sizing rule with the measured table, a "choosing" section, `Boundary` and the constrain-one-axis idiom, three recipes (app shell / master-detail + zen / dashboard tiles), overlays, and a gotchas section covering the `Grid` `0` vs `Width` `0` collision. Linked from `docs/README.md` and GETTING-STARTED's "Where to go next". **Every sample was compiled and snapshot-measured before shipping** — which caught that the shell recipe was passing `0` for the unconstrained `Boundary` axis when the idiom is to omit it (`null` = size freely); that example has been corrected in both the guide and GETTING-STARTED.

## Fourth cold start — 2026-07-31, first run with the layouts guide

**The layouts guide worked, on the first milestone, before any chart code was written.** Her M0 trace, verbatim:

> `docs/controls/Layouts.md`'s table ("which layouts fill the terminal") told me directly to build the root from `DockPanel`+`SplitPanel`, not `Grid` — the doc explicitly warns "a `Grid` at the root is almost always why" an app looks squashed at other sizes.

**First responsive shell in four runs**, with PNGs at 100×40 and 160×50 to show it reflowing. The previous three all built the shell from `Grid` and shipped the fixed-size app the maintainer photographed. This is also the first run where V-1's target came up cleanly *and* the whole M0→M7 ladder was attempted.

### New findings

| ID | Sev | Type | Finding | Status |
|----|-----|------|---------|--------|
| V-21 | major | missing-feature | **No proportional split, and no layout/resize event to build one from.** `SplitPanel.SplitPosition` is an absolute cell count (verified: `int`, and the class remarks say "resizing the container keeps the first pane put and grows/shrinks the second"). vtop's defining proportion is "the CPU chart is half the screen" — that can be *filled* but not *maintained*: it's half only at the size it was tuned for. There is also **no resize/layout-changed event anywhere on `Control` or `UI`** (verified by grep), so recomputing it is app code with nothing to hang off. This gap was only reachable *because* the shell finally reflowed — the previous three runs couldn't discover it. Fix: a fractional `SplitPosition` (or a proportional layout), and/or a documented layout-changed hook. | open |
| V-22 | major | bug (was filed as doc-gap) | **The braille/PNG font trap isn't cross-referenced from where you'd hit it.** `SnapshotImageOptions.FontFamily`'s remarks explain it exactly ("The default (Consolas) has no Braille glyphs… set this to `Cascadia Mono`"), but nothing on `Canvas` or `CanvasMarker.Braille` points there, so a braille chart's PNGs come out as tofu boxes and you only find the answer after going looking. **Third run in a row to hit this.** Fix: one sentence linking `SnapshotImageOptions.FontFamily` from `Canvas`/`CanvasMarker.Braille`. | open |

**V-21 independently confirmed by the reviewer, with numbers.** It measured the PNGs without knowing the library: the CPU panel is 15 of 40 rows (37%) at 100×40 and still 15 of 50 (**30%**) at 160×50; the memory pane is a fixed 40 columns — 40% of a 100-wide terminal, **25%** of a 160-wide one. Its summary: *"the panels do not re-proportion, they just get a bigger empty frame."* Two independent observers, one of whom has never seen the API, reaching the same conclusion is about as clean as this log gets.

**What the reviewer verified as genuinely fixed this round** (each was a defect in an earlier run): the braille chart's pixel pitch is a real filled column chart — *"two lit dot-columns per character cell, no dropped columns"*, right-anchored with history running left; the `dd` dialog **demonstrably rendered** rather than being claimed (run 1 shipped a PNG identical to the no-dialog frame); and the themes **really are parsed from vtop's JSON** — the nord title samples to the pink in `themes/nord.json` (run 3 claimed transcription and had invented two of three palettes).

**Recurring eval-harness problem: review packages keep containing claims their artifacts don't support.** Run 1: a `dd` PNG that was byte-identical to the no-dialog frame. Run 3: a stale `out.txt` reporting a failure the final code didn't have, plus two fabricated themes. Run 4: `05`/`06-sort-*.png` described as "what `RebuildProcessTable` produces after `c`/`m`" when the test hand-adds two rows and never calls the sort path; a `WALKTHROUGH.md` that cites a "port report" seven times which doesn't exist in the workspace; and a gap in the PNG numbering. The reviewer has caught this every single time, which is the loop working — but it's worth a line in the agent brief: **a review package may only describe what the artifact actually demonstrates; staged data must be labelled as staged.**

**Also fixed 2026-07-31 (a gap in the guide, found by the very next run):** she correctly noted that `Layouts.md`'s sizing table "is only about *which axis fills*, never about *proportional splits*". The guide now separates the two — a new "**Filling is not the same as staying proportional**" note states that both `DockPanel` and `SplitPanel` size their docked pane in absolute cells, that "fixed sidebar" holds at every size while "half the screen" doesn't, and that there's no fractional split or layout-changed event to lean on.

**The recurring shape, now seen five times.** V-1, V-2, V-16, V-20 and V-22 are all the same defect: *the capability exists and is documented on its own page; the page a developer starts from doesn't point at it.* `Plot`→`Canvas`, `DockPanel`/`Grid`→`Boundary`, `DataTable`→`IStyleTheme`, layouts→sizing, `Canvas`→`SnapshotImageOptions.FontFamily`. Every one was fixed by a cross-reference, not by new API. Worth treating as a standing review question for any new control page: **what will someone be holding when they need this, and does that page say so?**

Two self-inflicted bugs she hit are worth keeping as usability signal, since both cost a milestone: building `Canvas` geometry in a setter using `ActualWidth` (0/stale before the first layout pass, so the chart collapsed to one invisible column — fixed by rebuilding in the `Render()` override instead), and a global-hotkey test that passed for the wrong reason because the hotkey was never registered in the test process (`ToTextAfter(…, routeGlobal: true)` returns successfully when nothing matches). The second is a genuine harness footgun: **assert the effect, never just that the call succeeded.**

### Harness bug: the brief was inverting `h`/`l`, not the developers

Both cold starts bound `h`/`l` backwards versus vtop (`h`/Left zooms **in**, `l`/Right zooms **out**), and the second even documented the inversion as intended. Two independent developers making the same specific error points at a shared source, and it was the milestone text in `.claude/agents/jc-curious.md`:

> `h`/`l` (and left/right) zoom the charts in and out — vtop's `graphScale` **halves/doubles** between 0.125 and 8

Read positionally, the first clause pairs `h`→in and the second pairs `h`→halves. In vtop, zooming in **doubles** `graphScale`. The two halves of the sentence contradict each other, and both runs followed the second. **Fixed 2026-07-31** — M5 now states each key's direction explicitly, names `README.md` + `app.js` as the source of truth, and warns that the inversion is invisible in a screenshot.

The reviewer brief was never wrong here: it states no direction at all, just points at `README.md` and the key handler — which is why the reviewer independently caught the inversion in both runs. **Lesson for harness authoring: in a task brief, either state a fact unambiguously or don't state it and cite the source. A half-specified fact is worse than none — it overrides the source material the agent would otherwise have read.** Same class as the earlier harness-vs-app drift lesson: a defect in the eval scaffolding masquerading as a finding about the developer.

**Also worth keeping:** `SnapshotImageOptions.md`'s note that the default PNG font lacks braille coverage saved her a debugging session — she hit empty boxes in the PNG (live rendering and `ToText` were fine) and the doc told her to set `FontFamily = "Cascadia Mono"`. That's a doc note earning its keep; the same class of note is what V-16 is asking for.

## Mouse-behaviour audit — 2026-07-31 (follow-on from V-5)

Once V-5 made pointer input testable, the `DataTable` double-click bug it immediately exposed raised the obvious question: what else? Audited every mouse-reachable control (the 11 with an explicit `WantsMouse` override, plus every `Focusable` one — `Control` tags cells for both) by cross-referencing the mouse hooks each overrides against what its own docs promise.

| ID | Sev | Type | Finding | Status |
|----|-----|------|---------|--------|
| V-14 | major | bug | **A control that overrides `OnClick` but not `OnDoubleClick` silently swallows every second rapid click.** `Control.OnMouseUp` routes the *second* click of a pair to `OnDoubleClick`, which is an empty virtual — so the click is consumed and nothing happens. **Confirmed empirically: two rapid clicks on a `Button` produce one `Activated`, not two.** Every other GUI toolkit fires the click twice. Affects `Button`, `Link`, `Select`, `Menu`, `MenuBar`, `TabHeader`, `TabAddButton`, `Autocomplete`, and `Dialog`'s button bar. Not a doc mismatch (none of them document double-click) but a real UX defect: an impatient double-click on a button, menu item, or dropdown does nothing the second time. **The codebase already knows about this** — `ToggleButton` does `OnDoubleClick => Toggle()` and `ToggleList` does `OnDoubleClick => OnClick(position)` precisely to avoid it; the fix is that one-liner on the nine controls that lack it. | **fixed 2026-07-31 (0.1.9)** — `OnDoubleClick => OnClick(position)` added to all nine. Guarded by a theory in `SnapshotMouseTests` asserting 1/2/3 rapid clicks produce 1/2/3 activations (3 works because `Control` resets its double-click latch after a pair, so the third is a fresh single). |
| V-15 | major | capability-unknown | **`ListBox`'s documented right-click context menu cannot be exercised by any test.** `ListBox` reads `UI.MouseButton` to tell a right-click from a left-click (`ListBox.cs:351,362`), but `UI.MouseButton` has a **`private set`** assigned only from live input processing. The new snapshot mouse API can't set it, and neither can `Jumbee.Console.Tests` — `InternalsVisibleTo` doesn't reach a private setter. So `ContextMenu`/`ContextMenuOpening` — a documented, user-facing feature — has no test path at all, and V-5's mouse simulation is left-button-only. Fix: an internal setter plus `InternalsVisibleTo` for `Jumbee.Console.Snapshot`, and a `button` parameter on `ConsoleSnapshot.Click`. | **fixed 2026-07-31 (0.1.9)** — did exactly that: `UI.MouseButton`'s setter is now `internal` (public surface unchanged), `Jumbee.Console.Snapshot` was added to `InternalsVisibleTo`, and `Click` takes `button:`, latching and restoring `UI.MouseButton` around the dispatch. Two tests cover it: a right-click opens `ListBox`'s `ContextMenu` and announces the row, a left-click selects without opening it. |

**Audited clean** (implementation matches the documented promise): `Canvas` and `Globe` (drag-pan/rotate + wheel-zoom, both gated on `Interactive`, with proper mouse capture) · `Log` (wheel scroll) · `TerminalEmulator` (forwards press/release/wheel to the child process) · `SplitDivider` (drag to resize, with capture) · `DataTable` (now, after the double-click fix) · `ListBox` and `Tree` (click + double-click both handled; right-click aside, see V-15) · `ToggleButton` and its `Checkbox`/`Switch`/`RadioButton` subclasses, and `ToggleList`/`RadioSet` (both hooks) · `MarkdownViewer` — its doc correctly *conditions* wheel-scrolling on being wrapped in a `ControlFrame`, which is exactly what the inherited `OnMouseWheel => Frame?.Scroll(delta)` does.

**Method note:** V-14 was found by reading the override map, but only *confirmed* by running it through the new mouse API — the code reading alone looked like it might be intentional. A first attempt to check `Link` the same way returned a misleading "2 activations", because `Link.Activate()` opens the URL through the system handler and that took longer than the double-click window. Worth remembering when probing activation counts: use a control whose activation is cheap, and don't point a test at a control with an external side effect.

## vtop disposition

1. **V-1 + V-2 — the two cross-links.** Both are one-`<remarks>` edits to source XML that regenerate into `docs/api/*.md`. Between them they account for the entire round-1 detour. **Done 2026-07-31.**
2. **V-3** — **Done 2026-07-31.** Fixed across all 8 affected controls, not just `DataTable`: `Button`, `DataTable`, `Link`, `ListBox`, `MarkdownViewer`, `Menu`, `MenuBar`, `Tree` (+ `Autocomplete.WantsMouse`, which had no doc at all). Most controls already stated the override's real value (`Select`, `TextEditor`, `TextInput`, `ToggleButton`, `ToggleList`, `TerminalEmulator`, `TabHeader`, `Canvas`, `Globe`, `Log`, …) — these 8 were the outliers using `<inheritdoc/>`. **Standing rule: never `<inheritdoc/>` an override whose value differs from the base default** — the compiler won't warn (CS1591 skips overrides) and the docs will silently state the opposite of the truth.
3. **V-4 — Done 2026-07-31.** Originally filed (by me) as a capability hole needing an API. Wrong: `Dialog.Show(Overlay)` + `new Overlay(root)` + snapshot *the overlay* works today and is covered by 10 passing internal tests. Three cold-start attempts across two runs all concluded it was impossible, which made a missing example the single best-evidenced gap in this log. Now a "Testing a modal dialog" subsection in `GETTING-STARTED.md`. **Process note: the snippet was written as a real xUnit test and run before being pasted into the doc** — the first draft didn't compile (`TextLabel`'s first parameter is the orientation, not the text). A doc example that three developers failed to derive must not be shipped unverified; write it as a test, run it, then paste it.
4. **V-9** (`SavePngAfter` `routeGlobal` overload) and **V-10** (two-colour `Style` ctor) — **done**, shipped in `0.1.9`.
5. **V-5** (headless mouse) — **done in `0.1.9`**, and it paid for itself immediately: the very first test written against it found that **`DataTable` never raised `RowActivated` on double-click**, despite the event's own doc promising "Enter / double-click". The control overrode `OnClick` but not `OnDoubleClick`. A whole class of mouse behaviour had been undocumented-by-testing until there was a way to test it — worth checking the other `WantsMouse` controls for the same kind of gap now that it's cheap to.

**Release split:** `0.1.8` shipped the doc-only fixes (V-1, V-2, V-3, V-4). `0.1.9` carries the API work (V-5, V-9, V-10) plus the `DataTable` double-click fix.
4. **The braille control question is settled: don't build one.** `Canvas` + `FilledLine` already does the drawille-style filled column, more cheaply and more faithfully than the `Plot` workaround. Holding the control back until after the run was the right call — building it first would have produced a redundant API *and* hidden V-1.

---

## V-23 — no guidance on the one thing all three ports had to do

Every jc-curious target has been a live-data app — eilmeldung (HTTP feed fetch), scope-tui (realtime audio), vtop
(system metrics) — and every one of them had to answer the same question with no doc to answer it from: *how do I
sample continuously without stuttering, tearing or freezing the UI?* Each solved it differently and each got
something wrong. The vtop run's reviewer found the sharpest version: a background sampler calling `PushSample`
straight into a chart's `List<double>` while the render path enumerated it — a plain data race on the frame path.

The maintainer confirmed it from a live run: `PerfHud` showed **203 exc/s** (matching the ~190 access-denied
throws per sampling pass measured earlier) alongside a climbing lock count.

**Fixed 2026-07-31:** new [`docs/controls/Live Data.md`](../controls/Live%20Data.md) — the single-UI-thread rule and
why there's no lock, the snapshot-per-tick pattern, an `Invoke`/`Post`/`InvokeAsync` table, what marshals itself
(scalar properties) versus what doesn't (collections — i.e. everything a live app does), split sampling cadences,
cancellation and fault observation, frame-path costs, and how to read each `PerfHud` counter. Linked from
`docs/README.md`, GETTING-STARTED's "Where to go next", and given a curated `llms.txt` note. All samples compiled
and run before shipping.

Two things the guide states that the API docs alone would leave a .NET developer to guess wrong:

- **`UI.Invoke` does not block and does not surface the action's exception.** A WPF developer will read it as
  `Dispatcher.Invoke`; it behaves like `BeginInvoke`. `UI.InvokeAsync` is the one that waits.
- **`PerfHud`'s `locks` counter measures contention, not correctness**, and is *cumulative* (`Monitor.LockContentionCount`),
  not a rate. The dangerous bug — unsynchronized writes from a background thread — produces **zero** contention and
  still corrupts. Read it to confirm you haven't introduced locking, never to prove your threading is right.

---

## Fifth cold start — 2026-07-31, first run with the Live Data guide

**The Live Data guide worked.** `Program.cs` came out as *"three independent `PeriodicTimer` sampling loops marshaled via `UI.Invoke`"* — the snapshot-per-tick pattern and split cadences, adopted straight from the guide. **No data race this round** (the previous run's sampler wrote into a chart's list from a background thread while the render path enumerated it). She also explicitly followed the frame-path section: *"'Keeping the frame path cheap' … is exactly the right content and I followed it (capped history list, rebuild-on-push not per-frame)."*

**The layouts guide worked again**, second run running: *"Layouts.md's table + shell recipe told me exactly what to do: nested `DockPanel`s for header/footer, `SplitPanel` for the 50/50 body split. Compiled first try."*

**V-16 resolved in practice.** She found `IStyleTheme`'s default-interface-implementation note unaided and wrote a 6-line theme, calling it *"the single nicest thing I found in the whole exercise"* — and unlike run 3, actually themed the selection colour. Two of three runs now reach it; leaving V-16 open only for the missing `DataTable`→`IStyleTheme` pointer.

### V-1 was never actually fixed — my cross-link was on the wrong page

Three consecutive cold starts have now reached `Canvas`+`FilledLine`, and **none of them opened `Plot.md`**, which is where I put the cross-link. This run she grepped the entire API-reference folder for "braille". Her "one thing to fix first" is exactly right, and the measurement backs it: `docs/controls/Display Widgets.md` — the page that owns `Sparkline`, where anyone looking for a chart lands — had **zero** mentions of `Canvas` or `FilledLine`. So did `Live Data.md`, which I had just written *about streaming into charts*.

**Fixed 2026-07-31:** a "Looking for a bigger chart?" decision table at the top of `Display Widgets.md` (Sparkline / `Plot` / `Canvas`+`FilledLine`), with a verified runnable snippet, plus a "Choosing what to stream into" table at the top of `Live Data.md`. Both say plainly that `Canvas` reads as a general drawing surface and that `Plot`'s bars take no braille brush, because that's the inference every run has had to make unaided.

**Lesson, and it generalises past this finding:** a cross-reference is only worth what the traffic to its host page is worth. I fixed V-1 on the page that *owns* the API rather than the page a developer *starts* from, then counted it as fixed for two runs. When adding a pointer, put it where the reader already is — and verify with a run that they actually get there.

### New findings

| ID | Sev | Type | Finding | Status |
|----|-----|------|---------|--------|
| V-24 | major | capability-unknown | **No documented way to turn off a control's built-in mouse handling.** `DataTable.WantsMouse`/`HandlesInput` are documented as "always true, no opt-in needed" (the V-3 fix) with no counterpart opt-out, so her `--no-mouse` flag is a no-op and M7 was abandoned. The V-3 wording solved "is it on?" and created "can I turn it off?". Fix: state the override point (`WantsMouse` is `protected virtual`, so a subclass can suppress it) on the same paragraph. | open |
| V-25 | minor | missing-feature | **`DataTable` has no narrow-width column policy.** At 80 columns the headers wrap mid-word ("Memory %" → "Memor"/"y %"); vtop's own `drawTable` progressively drops columns as width shrinks. Fix: a min-width/auto-hide knob, or document that the caller should hide columns above a width threshold. | open |
| V-26 | minor | doc-gap | **`ConsoleSnapshot`'s mouse API is not reliably discoverable.** She concluded no click/hover/wheel simulation exists and reported M7 as possibly untestable *in principle* — but `Click`/`MouseMove`/`Wheel`/`RenderAfterClick` shipped in 0.1.9 (V-5), and the *previous* run found them on the `ConsoleSnapshot` page. Found by one run, missed by the next. Fix: mention mouse simulation in GETTING-STARTED's "Testing without a terminal", which is where both runs started. | open |

**Also fixed (a fair criticism of the guide I'd just written):** `Live Data.md`'s cadence table listed "cheap counters (CPU, memory)" at 200–300 ms, implying system metrics are cheap to obtain. They aren't on .NET — she measured her own app at **~20% of one core at idle**, traced to enumerating every process every 300 ms to sum `TotalProcessorTime` because there's no cheap portable BCL call for system CPU. The table now says *a* counter rather than naming CPU/memory, and carries a "don't assume the fast signal is cheap — measure it" note using exactly this trap as the example.

**Runtime reporting worked, partially.** Asked to report on behaviour rather than appearance, she ran the real exe for ~47 s and sampled it externally: working set flat (+1.4 MB, warmup not a leak), handles stable at ~377, no crash — and she flagged the ~20% CPU as her own sampler's cost. But she used `Get-Process`, not `PerfHud`, and was honest that this is the weaker measurement: *"I did not wire `PerfHud` … which is itself a gap in my own validation."* `PerfHud` is documented in `Live Data.md`; reaching for it still didn't happen under budget pressure.

### V-27 — CONFIRMED BUG: `DataTable`'s selection highlight lands on the wrong row at narrow widths

The reviewer's top blocker for the fifth run was that the selection bar highlighted the wrong process. It read that off the PNGs and blamed the app. **It's a library bug**, reproduced directly against `DataTable` with no app code involved:

```csharp
var t = new DataTable("Command", "CPU %", "Count", "Memory %");
t.AddRow("node", "11.4", "1", "2.2");      t.AddRow("firefox", "9.3", "1", "6.6");
t.AddRow("Xorg", "2.2", "1", "3.6");       t.AddRow("gnome-shell", "2.1", "1", "8.6");
t.SelectedIndex = 3;                        // gnome-shell
```

Rendering at three widths and locating the full-width highlight bar (counting backgrounded cells per row):

| Width | Highlight lands on | Correct? |
|---|---|---|
| 60 | `gnome-shell` | ✅ |
| 40 | `Xorg` (index 2) | ❌ off by one |
| 30 | `firefox` (index 1) | ❌ off by two |

The drift grows as the table narrows, which points at `Measure()`'s chrome estimate diverging from the real render: it probes with a synthetic single-space data row and derives `_chromeTop` (rows above the first data row) from the probe's line count. `_chromeTop` also feeds `OnClick`/`OnDoubleClick` row mapping, so **mouse row-hit-testing is wrong at the same widths**, not just the highlight.

Severity is higher than it looks: the highlight *is* the navigation feedback, so `j`/`k`/`g`/`G` silently point at the wrong row, and every visible symptom is invisible to a test that asserts `SelectedIndex` — which is exactly what her checks did, and why they passed while the UI was wrong. Related to V-25 (no narrow-width column policy) but distinct and worse: V-25 is ugly, V-27 is incorrect.

**FIXED 2026-07-31 (0.1.9), with a documented limit.** Root cause: `Measure()` probes a table whose cells are placeholders, but Spectre allocates column widths from cell *content*, so the probe's columns — and therefore how many lines its header wrapped to — differed from the real table's. `Render` now counts the segments of the table it is actually about to write (free: `Write` enumerates them anyway) and derives the exact chrome height from it. A new `RowTop()` returns that measured value and is used by everything that positions against drawn rows — the highlight, the scrollbar draw, the scrollbar drag, and `OnClick`/`OnDoubleClick` hit-testing; `ChromeTop()` stays the pre-render estimate used only to decide how many rows to request. Verified: the repro above now highlights `gnome-shell` at 60, 50 and 40 columns (40 was previously off by one), and clicking the highlighted row is a no-op instead of jumping. Guarded by `DataTableGeometryTests` (7 cases, incl. clicking every row to its own index).

**Known limit, deliberately not papered over:** the fix derives header height by subtracting data lines, assuming one line per row. Below roughly 40 columns for a four-column table, Spectre wraps the *cells* too despite `NoWrap` (`11.4` becomes `11`/`.4`), rows stop being one line tall, and the offsets drift again. Measured boundary: correct at 40+, wrong at 36 and below. The right fix there is V-25's narrow-width column policy — drop columns rather than wrap, as `top` and vtop both do — since a table whose values have wrapped is already unreadable and geometry is the lesser problem.

### V-22 — FIXED, and it was a bug, not a doc gap

Filed twice as "the braille/PNG font trap isn't cross-referenced", and twice left as a documentation item. It cost the fifth run's entire review round: all twelve PNGs rendered their charts as missing-glyph boxes, so the reviewer could not judge the one thing the package existed to show.

**Root cause (real, and mine to have found sooner):** `ConsoleSnapshot.ToImage` resolves **one** font family for the whole image, and `SnapshotImageOptions.FallbackFontFamilies` only applied when the *named* family wasn't installed. On Windows the default Consolas is installed and — verified directly against the font metrics — has **no** glyphs at U+2801 or U+28FF. So every Braille cell rasterised as `.notdef`. Fixed by handing the resolved fallbacks to the text renderer (`RichTextOptions.FallbackFontFamilies`), which substitutes *per glyph*: Braille comes from Cascadia Mono while ordinary text stays in the requested font.

**Why it took three rounds to see, and a warning for next time:** the failure is **process-state dependent**. Rendered alone, the imaging stack usually substitutes a covering font by itself and Braille comes out fine — so an isolated repro passes *with the bug present*. It only fails once other renders have run in the same process, which is precisely the real-world case (a review package emitting a dozen PNGs in one run). Chasing it with single-test runs produced three mutually contradictory measurements and two wrong conclusions — first "confirmed broken", then "my fix is a no-op, revert it" — before running the **full suite** both ways settled it: without the fallback the Braille test fails in-suite, with it the suite is green at 918/918.

**Lesson: when a measurement contradicts itself between runs, stop measuring the isolated case and reproduce in the configuration the bug was reported from.** The agents hit it while batch-rendering; a one-shot render was never going to show it.

Also cross-linked, since a font without coverage is still possible: `CanvasMarker.Braille` and `Canvas`'s remarks now name the PNG symptom and the `FontFamily` fix, and `Display Widgets.md` carries the same note under its braille-chart snippet. Guarded by `SnapshotBrailleFontTests` — which documents that it must run in-suite to be meaningful.
