# Changelog

## 0.1.9 (unreleased)

### Added

- **Headless mouse simulation in `Jumbee.Console.Snapshot`** — `ConsoleSnapshot.Click` (with `clicks:` for double-clicks and `button:` for right-clicks), `MouseMove`, `Wheel`, `ResetMouse`, plus `RenderAfterClick`/`ToTextAfterClick`. Snapshot tests could send keys but not pointer input, so click-to-select, double-click activation, hover and wheel scrolling — real behaviour on `DataTable`, `ListBox`, `Tree`, `Button` and others — could not be tested without a terminal. The hit-test and the enter/leave/press/release dispatch order mirror the live `ConsoleManager` path, so a simulated click reaches the same handler a real one does.
- **`Style(Color foreground, Color background)`** — a direct two-colour constructor. Previously a style with a background needed the markup-string form (`new Style("black on white")`) or composition (`(Style)fg | Style.Bg(bg)`); the get-only `ForegroundColor`/`BackgroundColor` properties made the two-argument form look like it should already exist.
- **`ConsoleSnapshot.SavePngAfter` gained `routeGlobal` and `ILayout` overloads**, matching `RenderAfter`/`ToTextAfter`. A PNG capture driven by a global hotkey, or of an `Overlay` (where a modal's frame actually lives), no longer needs a two-step `RenderAfter` → `SavePng(buffer)` workaround.

### Fixed

- **PNG snapshots draw Braille instead of missing-glyph boxes.** `ToImage` resolved one font for the whole image and only consulted `FallbackFontFamilies` when the named font wasn't installed — so the default (Consolas, which has no Braille glyphs) always won and every Braille cell rasterised as a box. Braille charts looked correct in the terminal and in text snapshots, so it failed silently in saved images only. The fallbacks are now given to the text renderer, which substitutes per glyph.
- **`DataTable` drops columns instead of wrapping when it's too narrow**, controlled by the new `DropNarrowColumns` property (on by default). Squeezed below the width its content needed, the table used to break headers mid-word and split values across lines (`11.4` rendering as `11` / `.4`); it now drops whole columns from the right, keeping the leftmost identifier column, the way terminal process monitors do. Widths are measured from the rows currently on screen. Set the property to `false` for the old behaviour.
- **`DataTable`'s selection bar and click hit-testing land on the right row when a header wraps.** Chrome height was estimated from a probe table filled with placeholder cells, but column widths are allocated from cell content, so the probe's header wrapped to a different number of lines than the real table's. The selection bar drifted up one row per extra header line as the control narrowed — highlighting the wrong row, and mapping clicks to it — while `SelectedIndex` stayed correct, so the app looked right in state and wrong on screen. Chrome is now measured from the table actually being drawn. (Now correct at every width, since the column-dropping change above removes the wrapping that used to break it.)
- **Rapid clicks are no longer swallowed.** `Control` routes the *second* click of a rapid pair to `OnDoubleClick`, so any control that overrode only `OnClick` silently consumed it — double-clicking a `Button` activated it once, not twice. Fixed on `Button`, `Link`, `Select`, `Menu`, `MenuBar`, `Autocomplete`, `TabHeader`, the tab bar's "+" button, and `Dialog`'s buttons. (`ToggleButton`, `ToggleList`, `ListBox` and `Tree` already handled both.)
- **`DataTable` now raises `RowActivated` on double-click.** The event's own documentation said it fires on "Enter / double-click", but only the Enter path was implemented — the control overrode `OnClick` and never `OnDoubleClick`, so a double-click only ever re-selected the row. Found by the new mouse simulation above, on its first use.

### Changed

- **`UI.MouseButton`'s setter is now `internal`** (was `private`) so `Jumbee.Console.Snapshot` can latch it when simulating a right-click. The property itself is unchanged for consumers.

## 0.1.8

### Fixed

- Several controls had the documentation for overridden members replaced by the base class's text, because an override with `<inheritdoc/>` (or no doc comment) inherits the base summary. On `HandlesInput`/`WantsMouse` this stated the opposite of the truth — the pages read "the default (false) ignores it" for controls that override it to `true`, so `DataTable`, `ListBox`, `Tree`, `Menu`, `MenuBar`, `Button`, `Link` and `MarkdownViewer` all appeared to need an opt-in that was already on.

### Docs

- **Testing a modal dialog** — new worked example in GETTING-STARTED's "Testing without a terminal", covering the `Overlay` a modal attaches to. Snapshotting the root layout renders a frame with no dialog in it; you have to snapshot the overlay.
- **`Plot.AddBars`/`AddLiveBars`** now point at `Canvas` + `Drawing.FilledLine` for a sub-cell filled/area chart, and state that bars take no `PlotBrush` and so can't be drawn in braille.
- **`DockPanel.DockedControl` and `Grid`** now name `Boundary` as the way to give a child a fixed extent. The "0 means fill the parent" trap was documented; the fix for a child with no `Width`/`Height` of its own (a `ControlFrame`, a nested layout) was not.

## 0.1.7

### Added

- **`ProgressBar` control** — a composable, themeable single-row task-progress display: a settable description, a smooth sub-cell fill bar, and optional percentage, elapsed/remaining-time and spinner columns, plus an indeterminate pulse for work whose total isn't known. 

### Fixed

- **Framed controls now render their frame inside a `VerticalStackPanel` / `HorizontalStackPanel`. A margin on a stacked control now reserves space as expected.

### Docs

- Added `/llms.txt` generation (per [llmstxt.org](https://llmstxt.org)) to the API-docs script — a curated, link-listed index of the docs and public API for LLM tooling.


## 0.1.6

### Added

- **`PlotPalette`** (`Jumbee.Console.Styles`) and **`IStyleTheme.PlotSeries`** — plot colours are now themeable: the axis, grid, tick and surface chrome plus the series palette come from the active theme instead of being passed per call, so a runtime theme switch re-colours existing plots.
- **`Plot.DamageTracking`** — opt-in partial redraw. The plot reports only the sub-rectangles a draw actually changed and the compositor skips the rest; roughly 5× off the composite for a sparse figure. Off by default — see the remarks on the property for when it pays and when it costs.
- **`ConsolePlot`: axis ticks can be set to `0`**, which removes that axis's grid lines and labels.

### Changed

- **`TextLabel` takes its colours from `IStyleTheme.LabelText`** (both foreground and background) until one is set explicitly. Colours passed to the constructor still win and now register as theme overrides, so a later theme switch re-colours only the labels that never asked for a colour. A theme supplying a background lets labels sit on a coloured strip without every caller passing one.

### Fixed

- **`CompositeControl` swallowed clicks on display composites.** A composite that opted into the mouse (`WantsMouse`) but whose children were not themselves focusable or mouse-listening produced cells with no listener, so a click anywhere over such a child never reached the composite. Those cells now carry the composite's own listener, restoring click-to-focus for panes like a plot; a child with its own listener is unaffected.
- `ConsolePlot`: custom tick values are honoured, and `Clear` no longer rewrites the whole buffer.

### Docs

- Documented the **cost model of damage tracking**, not just its benefit: it narrows what the compositor *scans* and never what the terminal *receives*, and because the scan it replaces is a linear buffer walk while damage bookkeeping is scattered, break-even needs the changing region to be a small fraction of the control — see `Control.TracksDamage` and "When damage tracking pays" in `docs/internal/Rendering Model.md`.

### Examples

- **New AudioScope demo** — a real-time oscilloscope, spectroscope and vectorscope over one shared audio source. Fully managed and cross-platform: MP3/WAV decode through NLayer, capture through WASAPI (Windows) or ALSA (Linux), with device selection, loopback, mono, `--overlap`, `--tick` and four colour schemes.
- Both Docker images gained an `audio-scope` target with a bundled sample track, and now ship the ALSA runtime so `audio-scope live` works on a Linux host given `--device /dev/snd`.

## 0.1.5


### Added

- **`Color.FromHexString`** — parses a hex colour string (e.g. `"#FF8800"`) into a `Jumbee.Console.Color`.
- **24-bit (true-color) support in ConsolePlot** — plot chrome and series take full RGB `Jumbee.Console.Color` values.
- `Plot` **screen-anchored axis titles** — the Y title pins to the top-left and the X title to the bottom-right, so they stay put as the axes rescale.

### Changed
- Performance optimizations for `Plot` control.
- **`UI.Start`: the frame-rate parameter is now `fps`** (frames per second) instead of `paintInterval`
- `Overlay` and `TextLabel` are now `sealed`.

### Fixed

- Fixed a latent crash when drawing **scatter points outside a fixed axis range** — such a point mapped past the plot buffer and threw `IndexOutOfRangeException`; it is now correctly clipped.
- UI/threading robustness: self-heal a UI run that was previously hard-killed, plus assorted input/UI/threading fixes.

## 0.1.4

### Fixed

- `MarkdownViewer` now **word-wraps paragraph text** to the control width. Previously a paragraph wider than the view clipped at the right edge and dropped everything past the first row (its doc already claimed it "reflows to the control width"). Implemented via a new opt-in `wrapWords` mode on `AnsiConsoleBuffer` (word-boundaried, with a character-level fallback for an over-long word); other controls are unaffected.
- The package now bundles the **XML documentation** of its private assemblies (`Jumbee.Console.Styles`, the Spectre.Console fork, ConsolePlot, …) alongside their DLLs in `lib/`. Previously only the DLLs shipped, so the theming/`Color`/`IStyleTheme`/`IGlyphTheme` API (in `Jumbee.Console.Styles`) and the bundled Spectre types had **no IntelliSense/doc surface** for consumers. (The bundling target's `.xml` match never fired because the XML doc copies don't carry `ReferenceSourceTarget=ProjectReference`.)

### Added

- `UI.HotKeys.Char(char)` — builds a `ConsoleKeyInfo` for a bare letter, digit, punctuation, or space key so it can be registered as a global hotkey (e.g. `UI.RegisterHotKey(UI.HotKeys.Char('q'), UI.Stop)`). It mirrors the input decoder exactly — including that punctuation keys (e.g. `/`) carry key code `0` with the character — so a registered hotkey matches a real keypress, and the same value drives a headless `routeGlobal` snapshot test.
- `Tree.SelectionChanged` event — raised whenever the highlighted node changes (arrow/vim keys, Home/End/PageUp/PageDown, or a mouse click), mirroring `ListBox.SelectionChanged`. Lets a detail pane follow tree navigation instead of only reacting to `NodeActivated` (leaf Enter/double-click).
- `TreeGuide.None` — a connector-less tree where hierarchy is shown by indentation (and node glyphs) alone.
- `Tree.TreeNode.Tag` and `ListBox.ListBoxItem.Tag` — an `object?` slot for arbitrary application data, so a node/row can map back to its domain object without a side dictionary.

### Changed

- `Tree.TreeNode.UpdateTree()` is now `public` (was `protected`) — it appeared in the docs but wasn't callable (`CS0122`). It forces a redraw of the owning tree, for the rare case a mutation didn't go through a property setter.

### Docs

- Documented that `ConsoleSnapshot.ToTextAfter`/`RenderAfter` deliver keys to the `control` argument itself, not to whatever `UI.SetFocus` last targeted — pass the control that actually changes (for a composite app, the specific child under test, not the root).
- Refreshed the "Snapshot Testing" internals page to the current input API (`routeGlobal`, `UI.HotKeys.Char`) and cross-linked the getting-started "Testing without a terminal" guide. The getting-started hotkey examples now use `UI.HotKeys.Char`; the previous punctuation example built the key with `ConsoleKey.Oem2`, which does not match a real `/` keypress.
- `UI.RegisterHotKey`: documented that the hotkey table is **process-global** (not scoped to a `UI.Start` root) — so a hotkey letter never reaches a focused text field (unregister/re-register around focus), and a second app instance re-registers the same keys (register/exercise one at a time in headless tests).
- `Grid`: documented its **fixed-cell** sizing (each value is an absolute cell count; the grid's size is their sum; `0` is a collapsed row/column, **not** fill-the-parent like `DockPanel`) — use `DockPanel`/`SplitPanel` for proportional/fill layouts.
- `SplitPanel.MinFirst`: documented it clamps to ≥ 1, so `SplitPosition` can't reach 0 (a "fully collapsed" pane is a 1-cell sliver).
- Composite Controls: added the single-child idiom (`SetContent(new Boundary(child))`).
- Troubleshooting: added the `Jumbee.Console.Tree` vs `Spectre.Console.Tree` name-ambiguity (`CS0104`) fix, and clarified that `MarkdownViewer` lives in core `Jumbee.Console` while the other viewers are in `Jumbee.Console.Documents`.

## 0.1.3

### Changed

- `ConsoleSnapshot.Key(key, …)` now fills in `KeyChar` for letter and digit keys (lowercase, uppercase under Shift, the control char under Ctrl), so a simulated key matches a hotkey registered the natural way (a bare letter). Previously it left `KeyChar='\0'`, so `ToTextAfter(…, routeGlobal: true)` silently failed to fire bare-letter global hotkeys. Non-character keys (arrows, function keys) are unchanged.

### Docs

- Documented that text snapshots (`ConsoleSnapshot.ToText`) don't capture colour or decoration — assert colour with `SavePng`/`ToImage`, or render a visible marker.
- Documented the runtime-reconfiguration pattern on `UI.Layout` (read-only): for a full-screen "zen" toggle, collapse a `SplitPanel` pane via `SplitPosition` or reassign `DockPanel.DockedControl`/`FillControl`, rather than swapping the root.

## 0.1.2

### Added

- `UI.SendInput(target, key, routeGlobal)` — an opt-in overload that runs the global hotkey dispatch (keys registered with `UI.RegisterHotKey`) before routing to the focused control, mirroring the live input path. Backward-compatible; existing calls route straight to the control as before.
- `Jumbee.Console.Snapshot`: `ConsoleSnapshot.RenderAfter` and `ToTextAfter` now accept a `routeGlobal` flag, so a headless snapshot test can exercise an app's global keybindings, not just control-routed input. Build the simulated key the same way the hotkey was registered so it compares equal.

### Changed

- The bundled package README now includes a runnable first-app example and a note about the private Spectre.Console fork: do not also reference the upstream `Spectre.Console` NuGet package (the assembly identities collide and the build fails with `CS1704`).

## 0.1.1

- Initial public release.
