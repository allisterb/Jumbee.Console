# Jumbee.Console API Reference

Auto-generated from the core libraries' XML-doc comments with [docfx](https://dotnet.github.io/docfx/).
**Do not edit by hand** - regenerate with `powershell -File build-api-docs.ps1` from the repo root.

## Jumbee.Console

### Classes

- [AnimatedControl](Jumbee.Console.AnimatedControl.md) — Base class for a control that advances through frames on a timer, repainting each frame while running.
- [AnsiConsoleBuffer](Jumbee.Console.AnsiConsoleBuffer.md) — An implementation of Spectre.Console.IAnsiConsole that writes to a ConsoleBuffer.
- [AnsiInputDecoder](Jumbee.Console.AnsiInputDecoder.md) — A streaming state machine that turns a raw terminal input char stream into `TerminalInputEvent`s: printable text, control/navigation keys (CSI/SS3), SGR (1006) mouse reports, bracketed paste (DEC 2004), and focus in/out (DEC 1004).
- [Autocomplete](Jumbee.Console.Autocomplete.md) — Attaches type-ahead suggestions to a `TextInput`.
- [Badge](Jumbee.Console.Badge.md) — A small inline status pill — short text on a filled background with a little horizontal padding (e.g. `200 OK`, `read-only`, a method tag).
- [BarChart](Jumbee.Console.BarChart.md) — A bar chart.
- [BarChart.BarChartItem](Jumbee.Console.BarChart.BarChartItem.md) — A single labelled, coloured item in a `BarChart`.
- [BarChart.HorizontalBar](Jumbee.Console.BarChart.HorizontalBar.md) — A horizontally-drawn bar in a `BarChart`, optionally showing its value and remaining track.
- [BarChart.VerticalBar](Jumbee.Console.BarChart.VerticalBar.md) — A vertically-drawn bar in a `BarChart`.
- [Boundary](Jumbee.Console.Boundary.md) — A single-child layout that pins its child's size.
- [Button](Jumbee.Console.Button.md) — A focusable, clickable button that renders a fixed-width text label.
- [Canvas](Jumbee.Console.Canvas.md) — A blank drawing surface on which you paint `IShape`s (`Line`, `Rectangle`, `Circle`, `Points`, `FilledLine`) in an arbitrary coordinate system, rendered with sub-cell markers (braille by default).
- [ChatPrompt](Jumbee.Console.ChatPrompt.md) — The input area of an agent/chat CLI (Claude Code, Gemini CLI): a prompt glyph on the left that turns into an animated *busy* spinner while an operation runs, and a single-line `TextInput` filling the rest. Submitting (Enter) raises `Submitted`; any edit raises `Changed`.
- [Checkbox](Jumbee.Console.Checkbox.md) — A labelled checkbox that toggles an independent on/off state.
- [CodeEditor](Jumbee.Console.CodeEditor.md) — A composite control pairing a `TextEditor` with a `LineNumberGutter` docked to its left.
- [CompositeControl](Jumbee.Console.CompositeControl.md) — Base class for *composite* controls: a `Control` that owns and lays out several child controls and presents them as a single control. It is a real `Control` (so it has its own console buffer, participates in theming/painting, can be framed, and drops into any layout cell), but its content is an internal `ILayout` arranging the children.
- [ConPty](Jumbee.Console.ConPty.md) — A pseudo-console (ConPTY) session: launches a process attached to a Windows pseudo console and exposes its stdin/stdout as streams.
- [ConsoleBuffer](Jumbee.Console.ConsoleBuffer.md) — A ConsoleGUI.IConsole implementation that writes to a buffer.
- [ConsoleInputSource](Jumbee.Console.ConsoleInputSource.md) — The default `IInputSource`, reading keys from `Console` and wrapping them as `KeyInputEvent`s.
- [ContextMenu](Jumbee.Console.ContextMenu.md) — A floating, keyboard-navigable menu of `MenuItem`s, shown anchored in the ambient `Overlay`. The shared primitive behind drop-downs / context menus / a `MenuBar`'s menus.
- [Control](Jumbee.Console.Control.md) — Base class for all Jumbee.Console controls.
- [ControlExtensions](Jumbee.Console.ControlExtensions.md) — Fluent extension helpers for configuring controls, frames, and geometry values.
- [ControlFrame](Jumbee.Console.ControlFrame.md) — Draws a border around a control together with margins and a title bar, and sets the foreground and background colors.
- [DataTable](Jumbee.Console.DataTable.md) — An interactive data grid.
- [DefaultGlyphTheme](Jumbee.Console.DefaultGlyphTheme.md) — The built-in glyph theme: every glyph uses `IGlyphTheme`'s default values.
- [DefaultStyleTheme](Jumbee.Console.DefaultStyleTheme.md) — The built-in style theme: every token uses `IStyleTheme`'s default values.
- [Dialog](Jumbee.Console.Dialog.md) — A modal dialog window shown over the ambient `Overlay`: a titled, bordered box that takes exclusive focus (the layer beneath is dimmed and click-blocked) until dismissed.
- [Digits](Jumbee.Console.Digits.md) — Renders text using large three-row "seven-segment" glyphs, for clocks, counters and headline figures.
- [Dispatcher](Jumbee.Console.Dispatcher.md) — Owns a single UI thread and a serialized work queue. UI state mutation and rendering are intended to run on this thread; other threads marshal work onto it via `Post`, `Invoke`, or `InvokeAsync`.
- [DockPanel](Jumbee.Console.DockPanel.md) — A two-child layout that pins one control to an edge and fills the remaining space with the other.
- [DocumentClosingEventArgs](Jumbee.Console.DocumentClosingEventArgs.md) — Arguments for `DocumentClosing`. Set `Cancel` to keep the document open (e.g. after confirming unsaved changes).
- [FeedHandle](Jumbee.Console.FeedHandle.md) — A handle to a running background feed started by Control.Feed(Action, int) and its overloads. Cancel it to stop the feed; await `Completion` (or `StopAsync`) to know the in-flight tick has finished — for safely disposing a resource the feed's producer reads.
- [FocusInputEvent](Jumbee.Console.FocusInputEvent.md) — Terminal focus gained/lost (DEC mode 1004).
- [Footer](Jumbee.Console.Footer.md) — A one-row key-hints bar (e.g. `^j Send ^t Method ^c Quit f1 Help`), filling the available width.
- [Gauge](Jumbee.Console.Gauge.md) — A single-row horizontal progress bar: the track is filled proportional to `Value` / `Max`, optionally followed by the percentage and the raw value — e.g. `████████░░░░ 34.5% (126)`. For dashboards (year/day progress, a deployment %, a capacity meter).
- [GlassBlend](Jumbee.Console.GlassBlend.md) — Colour blending for `GlassPanel`: a gamma-space lerp (cheap, matches `Mix`) or a gamma-correct blend in linear light via two lookup tables (no runtime `pow`), plus a rough estimate of how much of a cell a glyph inks (for the perceived-colour collapse).
- [GlassPanel](Jumbee.Console.GlassPanel.md) — A translucent "glass" panel: a fixed-size overlay that shows the layer beneath it, tinted, with its own `Content` drawn opaquely on top. Host it non-modally over the current UI with `Show` (it floats via `ShowPassive`, so the layer beneath keeps focus and keeps receiving input).
- [Globe](Jumbee.Console.Globe.md) — A ray-traced globe of the Earth — a shaded, colour-mapped sphere drawn into the control's buffer. Display-only by default; spin, tilt, and zoom it from a frame or timer feed.
- [Grid](Jumbee.Console.Grid.md) — A grid layout with controls arranged in rows and columns.
- [HelpControl](Jumbee.Console.HelpControl.md) — The global help dialog: a fixed-size modal composite showing one tab per `HelpInfo` (built by `ShowHelp` from every control's `GetHelpInfo`), with a Close button.
- [HelpInfo](Jumbee.Console.HelpInfo.md) — A control's entry in the global help dialog (one tab). Mutable so an `OnHelp` handler can tweak it in place.
- [HorizontalStackPanel](Jumbee.Console.HorizontalStackPanel.md) — A layout that arranges its child controls in a single horizontal row.
- [InteractiveMarkdownEditor](Jumbee.Console.InteractiveMarkdownEditor.md) — A live, split-pane Markdown editor for the terminal — the TUI equivalent of a web Markdown editor. The left pane is a `CodeEditor` with Markdown syntax highlighting; the right pane is a `MarkdownViewer` that re-renders the document — headings, tables and syntax-highlighted code — as you type (see `InteractiveSourceEditor` for the sync model).
- [InteractiveSourceEditor](Jumbee.Console.InteractiveSourceEditor.md) — Base for live, split-pane source editors: a `CodeEditor` in one pane and a read-only preview control in the other, wired so the preview re-renders as the source is edited. A draggable `SplitPanel` divider sits between them (drag it, or focus it and press the arrows). Subclasses supply the editor's language, the preview control, and how to push text into it (`ApplyPreviewText`).
- [KeyHelp](Jumbee.Console.KeyHelp.md) — One keystroke (or chord) and what it does, listed in a control's `HelpInfo`.
- [KeyInputEvent](Jumbee.Console.KeyInputEvent.md) — A key press.
- [Layout-1](Jumbee.Console.Layout-1.md) — Base class for Jumbee.Console layouts wrapping a ConsoleGUI layout control T` and exposing it through `ILayout`.
- [LineNumberGutter](Jumbee.Console.LineNumberGutter.md) — A narrow, non-interactive column of right-aligned line numbers, highlighting the active row. Intended as an adornment inside a composite (e.g. `CodeEditor`); width auto-grows with the digit count.
- [Link](Jumbee.Console.Link.md) — A focusable, clickable text link. Activating it (a mouse click, or Enter/Space while focused) opens `Url` with the system default handler and raises `Activated`.
- [ListBox](Jumbee.Console.ListBox.md) — Displays a flat list of items and allows user input navigation and selection.
- [ListBox.ListBoxItem](Jumbee.Console.ListBox.ListBoxItem.md) — An item in a ListBox.
- [Log](Jumbee.Console.Log.md) — An append-only, scrolling log of Spectre `IRenderable` entries — markup strings, tables, rules, etc. — that always shows the most recent entries that fit (a "tail" view). Each entry is a renderable, so log lines can be styled/coloured. `Write`/`Write` are safe to call from any thread.
- [MarkdownViewer](Jumbee.Console.MarkdownViewer.md) — A read-only, scrollable Markdown viewer. Renders CommonMark — headings, bold/italic, block-quotes, ordered and unordered lists, links, syntax-highlighted fenced code blocks, and box-drawn tables — via NTokenizers' Spectre.Console markdown writer. Wrap it in a `ControlFrame` (e.g. `viewer.WithFrame()`) to get a border, title and scrollbar; ↑/↓, PgUp/PgDn, Home/End and the mouse wheel scroll it.
- [MenuBar](Jumbee.Console.MenuBar.md) — A horizontal bar of top-level menu titles (e.g. `File Edit View`).
- [MenuItem](Jumbee.Console.MenuItem.md) — One entry in a `ContextMenu`: a label, an optional right-aligned shortcut hint, an action invoked when chosen, an enabled flag, and an optional `Submenu`. Use `Separator` for a non-selectable divider line.
- [MouseInputEvent](Jumbee.Console.MouseInputEvent.md) — A mouse action at cell coordinates (0-based).
- [MultiTabCodeEditor](Jumbee.Console.MultiTabCodeEditor.md) — A tabbed group of `CodeEditor`s — a VS-Code-style editor area. Each open document is a closable tab (click the ✕ on the active/hovered tab), and a "+" button at the end of the bar opens a new document.
- [Overlay](Jumbee.Console.Overlay.md) — A layered layout: a persistent `Bottom` layer with an optional floating popup composited on top (wraps ConsoleGUI's `Overlay`).
- [PasteInputEvent](Jumbee.Console.PasteInputEvent.md) — A bracketed-paste payload, delivered as one event so it is never re-interpreted as keystrokes.
- [PerfHud](Jumbee.Console.PerfHud.md) — A translucent "glass" HUD showing live UI telemetry — frame draw/paint times (µs), CPU, working set, allocation rate and, the headline for a no-lock design, monitor lock contentions — floating over the app.
- [Plot](Jumbee.Console.Plot.md) — A line/scatter chart backed by the ConsolePlot library, rendered into the control's buffer. Add data with `AddSeries` and tune the axes/grid/ticks with the `Configure*` methods.
- [PlotSeries](Jumbee.Console.PlotSeries.md) — A live, updatable series in a `Plot`. Returned by `AddLiveSeries` (line), `AddLiveScatter` (markers) or `AddLiveBars`; hold onto it and feed data as it arrives with `SetData`, `SetValues`, `Push` or `Clear`.
- [ProcessMetrics](Jumbee.Console.ProcessMetrics.md) — Collects live process/runtime performance metrics for the perf HUD by reading the runtime APIs the `System.Runtime` meter wraps (`GC`, `Environment`, `ThreadPool`, `Monitor`) directly — no `MeterListener`, so there is nothing to sample-schedule and no observable-instrument staleness.
- [Prompt](Jumbee.Console.Prompt.md) — Base class for controls that prompt the user for input.
- [Pty](Jumbee.Console.Pty.md) — Factory that opens an `IPty` using the right backend for the current OS: the Windows pseudo console (`ConPty`) or a Unix pseudo terminal (`UnixPty`).
- [RadioButton](Jumbee.Console.RadioButton.md) — A labelled radio button.
- [RadioSet](Jumbee.Console.RadioSet.md) — A vertical group of mutually-exclusive radio options, exactly one selected at a time.
- [RenderableControl](Jumbee.Console.RenderableControl.md) — A control that implements Spectre.Console.IRenderable
- [ResizeInputEvent](Jumbee.Console.ResizeInputEvent.md) — Terminal resized. Not decoded from the char stream — raised by the input source on a resize signal.
- [RunChart](Jumbee.Console.RunChart.md) — A live multi-series time chart with a legend — a streaming line `Plot` on the left and a per-series stat readout (name + current / delta / max / min) on the right, in the style of a monitoring "run chart".
- [RunSeries](Jumbee.Console.RunSeries.md) — A handle to one `RunChart` series: push values with `Push`. Tracks the current value, the delta from the previous value, and the running max/min shown in the chart's legend.
- [Select](Jumbee.Console.Select.md) — A drop-down selector.
- [SelectionList](Jumbee.Console.SelectionList.md) — A vertical list of independently-checkable options (multi-select).
- [SelectionStylesExtensions](Jumbee.Console.SelectionStylesExtensions.md) — Turns a `SelectionStyle` into the prefix + text style a control applies to its selected item.
- [Sparkline](Jumbee.Console.Sparkline.md) — A compact, single-row chart that draws a series of numeric values as block bars (one cell per value), scaling each value's height against the series maximum.
- [SpectreControl-1](Jumbee.Console.SpectreControl-1.md) — Wraps an existing Spectre.Console `IRenderable` control for use with ConsoleGUI control and layout types.
- [SpectreLiveDisplay](Jumbee.Console.SpectreLiveDisplay.md) — A Spectre.Console LiveDisplay widget.
- [SpectreTaskProgress](Jumbee.Console.SpectreTaskProgress.md) — A Spectre.Console Progress widget.
- [Spinner](Jumbee.Console.Spinner.md) — An animated spinner glyph with an optional label, cycling through a `Spinner`'s frames.
- [SplitDivider](Jumbee.Console.SplitDivider.md) — The draggable divider between a `SplitPanel`'s two panes.
- [SplitPanel](Jumbee.Console.SplitPanel.md) — A container that splits its area between two panes with a draggable divider between them.
- [SplitPanelDockPanel](Jumbee.Console.SplitPanelDockPanel.md) — The visual scaffold behind `SplitPanel`: two nested ConsoleGUI `DockPanel`s laying out `[first | divider | second]` along the split axis.
- [Switch](Jumbee.Console.Switch.md) — A sliding on/off switch.
- [TabCloseEventArgs](Jumbee.Console.TabCloseEventArgs.md) — Arguments for `TabCloseRequested`.
- [TabHeader](Jumbee.Console.TabHeader.md) — A single clickable tab label in a `TabPanel`'s tab bar.
- [TabItem](Jumbee.Console.TabItem.md) — A handle to a tab in a `TabPanel` — stable across add/remove/reorder (unlike a positional index).
- [TabPanel](Jumbee.Console.TabPanel.md) — A tabbed container: a bar of selectable `TabHeader` labels docked on one edge, with the selected tab's content filling the rest.
- [TabPanelDockPanel](Jumbee.Console.TabPanelDockPanel.md) — The visual scaffold behind `TabPanel`: a ConsoleGUI `DockPanel` that docks a thin tab bar (a horizontal or vertical stack of `TabHeader` cells) on one edge and fills the rest with the active tab's content. It does no selection bookkeeping — `TabPanel` owns the model and drives this through `SetHeaders` / `SetFill`.
- [TerminalEmulator](Jumbee.Console.TerminalEmulator.md) — A control that runs a child process in a pseudo-console (`ConPty`), parses its ANSI output with VtNetCore, and paints the emulated screen into the control's cell area. Input routed to the focused control is translated to terminal bytes and sent to the process.
- [TerminalInputEvent](Jumbee.Console.TerminalInputEvent.md) — Base type for the unified terminal input stream produced by `AnsiInputDecoder`: a single sequence of key / mouse / paste / focus events, replacing the keyboard-only `ConsoleKeyInfo` path.
- [TextEditor](Jumbee.Console.TextEditor.md) — A text editor control with syntax highlighting for supported languages.
- [TextInput](Jumbee.Console.TextInput.md) — A single-line text entry control: caret, selection (Shift+navigation), horizontal scrolling when the text exceeds the width, an optional muted placeholder shown while empty, and optional password masking.
- [TextLabel](Jumbee.Console.TextLabel.md) — Displays a single-line text label with a defined horizontal or vertical orientation, foreground and background colour, and optional text decoration (e.g. bold, underline).
- [TextPanel](Jumbee.Console.TextPanel.md) — Displays a block of multi-line Spectre `Markup` text — the counterpart to the single-line `TextLabel` and the append-only `Log`.
- [TextPrompt](Jumbee.Console.TextPrompt.md) — A single-line text input that shows a prompt label and edits an inline entry with a terminal cursor.
- [ToggleButton](Jumbee.Console.ToggleButton.md) — Shared base for the single-state toggle widgets (`Checkbox`, `RadioButton`, `Switch`).
- [ToggleList](Jumbee.Console.ToggleList.md) — Shared base for the vertical, navigable toggle lists (`RadioSet`, `SelectionList`).
- [Tree](Jumbee.Console.Tree.md) — Displays a hierarchical list of items in a tree layout.
- [Tree.TreeNode](Jumbee.Console.Tree.TreeNode.md) — Represents a tree node.
- [UI](Jumbee.Console.UI.md) — Manages th overall UI and provides a paint event for controls to subscribe to.
- [UI.GlobalInputListener](Jumbee.Console.UI.GlobalInputListener.md) — Input listener that dispatches globally-registered hotkeys before any control sees the event.
- [UI.HotKeys](Jumbee.Console.UI.HotKeys.md) — Factory helpers and well-known `ConsoleKeyInfo` constants for registering hotkeys.
- [UI.InputEventArgs](Jumbee.Console.UI.InputEventArgs.md) — Arguments for control input handling, wrapping the decoded `InputEvent`.
- [UI.PaintEventArgs](Jumbee.Console.UI.PaintEventArgs.md) — Arguments for the `Paint` event; carries no data (controls read their own state).
- [UnixPty](Jumbee.Console.UnixPty.md) — A Unix pseudo terminal (Linux/macOS) session.
- [VerticalStackPanel](Jumbee.Console.VerticalStackPanel.md) — A layout that arranges its child controls in a single vertical column.
- [VtInputSource](Jumbee.Console.VtInputSource.md) — An `IInputSource` that puts the terminal into VT input mode and enables mouse, bracketed-paste, and focus reporting, then reads the raw stdin byte stream, decodes it (UTF-8) and runs it through `AnsiInputDecoder` to produce `TerminalInputEvent`s.

### Structs

- [ButtonStyle](Jumbee.Console.ButtonStyle.md) — The appearance of a `Button`: its fill `Style` in each interaction state (text colour + background), its `Shape`, an optional fixed/minimum width, and whether the label is bold.
- [Color](Jumbee.Console.Color.md) — An RGB colour. Value type: two colours are equal when their channels are.
- [FooterHint](Jumbee.Console.FooterHint.md) — A single key-binding hint shown in a `Footer`: the key chord and what it does.
- [GaugeStyle](Jumbee.Console.GaugeStyle.md) — The per-part `Style` a `Gauge` composes: the filled portion of the bar, the empty track behind it, and the percent/value readout (and any inline label).
- [HScroll](Jumbee.Console.HScroll.md) — An opt-in horizontal-scroll offset for controls that render a fixed-width buffer wider than their viewport and window it in `Blit` (the `ControlFrame` only scrolls vertically).
- [PlotPalette](Jumbee.Console.PlotPalette.md) — The ordered colours a plot cycles through for series that don't name one.
- [ScrollBarGlyphs](Jumbee.Console.ScrollBarGlyphs.md) — The glyphs (no colours) a control frame's vertical scrollbar draws for each part: the moving thumb, the track behind it, and the two end arrows.
- [ScrollBarStyle](Jumbee.Console.ScrollBarStyle.md) — The per-part `Style` (foreground/background/decoration, no glyph) a control frame applies to its vertical scrollbar.
- [Style](Jumbee.Console.Style.md) — A text style — foreground/background colour and text decoration — wrapping a Spectre.Console style. Exposes the named colour palette (`Red1`, `Cyan1`, …) and decoration presets (`Bold`, `Italic`, …) as ready-made tokens; combine them with `|`.
- [TitleStyle](Jumbee.Console.TitleStyle.md) — Describes how a control frame title is aligned, bordered, and colored.

### Interfaces

- [BarChart.IBarControl](Jumbee.Console.BarChart.IBarControl.md) — A single bar renderable within a `BarChart`.
- [IFocusable](Jumbee.Console.IFocusable.md) — A control that can receive keyboard focus and input.
- [IGlyphTheme](Jumbee.Console.IGlyphTheme.md) — The glyphs controls use for state indicators.
- [IInputSource](Jumbee.Console.IInputSource.md) — Supplies `TerminalInputEvent`s (keys, mouse, paste, focus) to the UI input loop.
- [ILayout](Jumbee.Console.ILayout.md) — Common interface for Jumbee.Console layout classes: a 2-D grid of focusable cells over a ConsoleGUI control, with focus navigation and input routing.
- [IPty](Jumbee.Console.IPty.md) — A pseudo-terminal session: a child process attached to a PTY, exposing its stdin/stdout as streams.
- [IStyleTheme](Jumbee.Console.IStyleTheme.md) — The general appearance theme: a set of semantic `Style` tokens (foreground + background + decoration) that controls compose when resolving their default appearance, plus the rest of a control's non-glyph styling — e.g. a frame's border shape (`FrameBorder`) and its title's position/border/colour.
- [ITheme](Jumbee.Console.ITheme.md) — A complete theme bundling an `IStyleTheme` and an `IGlyphTheme`, for callers that want to customise both colours/styles and glyphs as one unit and apply them together (via `UI.SetTheme(ITheme)`).

### Enums

- [BadgeVariant](Jumbee.Console.BadgeVariant.md) — Preset colour schemes for a `Badge`, resolved from the active theme.
- [BorderPlacement](Jumbee.Console.BorderPlacement.md) — Which edges of a control frame's border are drawn.
- [BorderStyle](Jumbee.Console.BorderStyle.md) — The shape of a control frame's border.
- [ButtonShape](Jumbee.Console.ButtonShape.md) — The overall shape of a `Button`.
- [ChartOrientation](Jumbee.Console.ChartOrientation.md) — Whether a chart is laid out horizontally or vertically.
- [CursorStyle](Jumbee.Console.CursorStyle.md) — Terminal cursor shapes/blink, mapping to DECSCUSR (CSI Ps SP q) values. `Default` (0) leaves the terminal's configured cursor.
- [DialogButtons](Jumbee.Console.DialogButtons.md) — The predefined button set a `Dialog` shows along its bottom edge.
- [DialogResult](Jumbee.Console.DialogResult.md) — Which button dismissed a `Dialog` (or how it was dismissed).
- [DockedControlPlacement](Jumbee.Console.DockedControlPlacement.md) — Which edge a `DockPanel` pins its docked control to.
- [FocusStyle](Jumbee.Console.FocusStyle.md) — How the themed default focus cue is drawn on a focused control that isn't showing focus another way (no visible frame border, and `Control.RendersOwnFocus` is false).
- [Justify](Jumbee.Console.Justify.md) — Horizontal alignment of text within its available width.
- [Language](Jumbee.Console.Language.md) — A source language for syntax highlighting, shared by the text/code controls (e.g. `TextEditor`, `CodeEditor`). `None` renders plain, unhighlighted text.
- [PlotBrush](Jumbee.Console.PlotBrush.md) — Selects the glyph set (and thus the sub-cell resolution) a `Plot` series is drawn with.
- [PlotColormap](Jumbee.Console.PlotColormap.md) — Selects the colour map a `Plot` heatmap uses to turn cell values into colours.
- [PlotLabelAlign](Jumbee.Console.PlotLabelAlign.md) — Horizontal anchoring of a `Plot` annotation label relative to its point.
- [ScrollBarMode](Jumbee.Console.ScrollBarMode.md) — How a control frame renders its vertical scrollbar.
- [SelectionStyle](Jumbee.Console.SelectionStyle.md) — How a navigable control (e.g. `IStyleTheme` consumers like ListBox, Tree, TabPanel) renders its selected/active item.
- [SelectPopupPosition](Jumbee.Console.SelectPopupPosition.md) — Where a `Select`'s dropdown opens relative to the control.
- [SplitOrientation](Jumbee.Console.SplitOrientation.md) — How a `SplitPanel` arranges its two panes.
- [TabBarDock](Jumbee.Console.TabBarDock.md) — Which edge a `TabPanel` docks its tab bar on.
- [TerminalModifiers](Jumbee.Console.TerminalModifiers.md) — Keyboard/mouse modifier flags decoded from the input stream.
- [TerminalMouseButton](Jumbee.Console.TerminalMouseButton.md) — Mouse button (or wheel direction) reported by the terminal.
- [TerminalMouseKind](Jumbee.Console.TerminalMouseKind.md) — The kind of mouse action reported.
- [TextLabelOrientation](Jumbee.Console.TextLabelOrientation.md) — The layout direction of a `TextLabel`.
- [TitleBorderStyle](Jumbee.Console.TitleBorderStyle.md) — The way a control frame title is drawn relative to the top border.
- [TitleColorStyle](Jumbee.Console.TitleColorStyle.md) — The way a control frame title is colored relative to the border color.
- [TitlePos](Jumbee.Console.TitlePos.md) — Position of a control frame title: which border (top or bottom) it is drawn in, and its horizontal alignment within that border.
- [TreeGuide](Jumbee.Console.TreeGuide.md) — The line style used to draw the connecting guide lines of a `Tree`.

### Delegates

- [Control.InitializationHandler](Jumbee.Console.Control.InitializationHandler.md) — Delegate for the `OnInitialization` event.
- [FocusableEventHandler](Jumbee.Console.FocusableEventHandler.md) — Handler for the `OnFocus` and `OnLostFocus` events.

## Jumbee.Console.Documents

### Classes

- [AsciiDocLanguage](Jumbee.Console.Documents.AsciiDocLanguage.md) — A ColorCode `ILanguage` grammar for AsciiDoc source, for syntax-highlighting an AsciiDoc document in a `CodeEditor` (`new CodeEditor(AsciiDocLanguage.Instance)`).
- [AsciiDocStyles](Jumbee.Console.Documents.AsciiDocStyles.md) — Visual styling for `AsciiDocViewer`.
- [AsciiDocViewer](Jumbee.Console.Documents.AsciiDocViewer.md) — A read-only, scrollable AsciiDoc viewer.
- [InteractiveAsciiDocEditor](Jumbee.Console.Documents.InteractiveAsciiDocEditor.md) — A live, split-pane AsciiDoc editor for the terminal: the left pane is a `CodeEditor` with AsciiDoc syntax highlighting (see `AsciiDocLanguage`); the right pane is an `AsciiDocViewer` that re-renders the document — headings, formatting, lists, admonitions, tables and blocks — as you type.
- [InteractiveMarkdownExtendedEditor](Jumbee.Console.Documents.InteractiveMarkdownExtendedEditor.md) — A live, split-pane Markdown editor whose preview renders embedded ````mermaid` code blocks as diagrams — the interactive complement to `MarkdownExtendedViewer`.
- [InteractiveMermaidEditor](Jumbee.Console.Documents.InteractiveMermaidEditor.md) — A live, split-pane Mermaid editor for the terminal: the left pane is a `CodeEditor` with Mermaid syntax highlighting (see `MermaidLanguage`); the right pane is a `MermaidViewer` that re-renders the diagram as you type.
- [MarkdownExtendedViewer](Jumbee.Console.Documents.MarkdownExtendedViewer.md) — A `MarkdownViewer` that renders fenced ````mermaid` code blocks as diagrams (flowchart, sequence, class, ER, state) instead of showing their source.
- [MarkdownWithMermaidLanguage](Jumbee.Console.Documents.MarkdownWithMermaidLanguage.md) — A ColorCode `ILanguage` that highlights Markdown *and* the contents of embedded ````mermaid` fenced blocks (using the `MermaidLanguage` grammar) — for editing Markdown that contains mermaid diagrams in a `CodeEditor`.
- [MermaidLanguage](Jumbee.Console.Documents.MermaidLanguage.md) — A ColorCode `ILanguage` grammar for Mermaid diagram source, for syntax-highlighting a Mermaid document in a `CodeEditor` (`new CodeEditor(MermaidLanguage.Instance)`).
- [MermaidStyles](Jumbee.Console.Documents.MermaidStyles.md) — Colours and scale for `MermaidViewer`.
- [MermaidViewer](Jumbee.Console.Documents.MermaidViewer.md) — A read-only, scrollable viewer for Mermaid `flowchart`/`graph` and `stateDiagram` diagrams.

## Jumbee.Console.Drawing

### Classes

- [Circle](Jumbee.Console.Drawing.Circle.md) — A circle outline traced at one-degree steps around its centre, in canvas coordinates.
- [FilledLine](Jumbee.Console.Drawing.FilledLine.md) — A line whose area between the line and `FillToY` is filled — useful for area charts.
- [Line](Jumbee.Console.Drawing.Line.md) — A straight line between two points in canvas coordinates.
- [Points](Jumbee.Console.Drawing.Points.md) — A scatter of individual points in canvas coordinates.
- [Rectangle](Jumbee.Console.Drawing.Rectangle.md) — A rectangle outline. Positioned from its bottom-left corner (`X`, `Y`) in canvas coordinates — the mathematical convention, not terminal cells.
- [WorldMap](Jumbee.Console.Drawing.WorldMap.md) — A world map: the Earth's coastlines as a cloud of longitude/latitude points (EPSG:4326), for drawing on a `Canvas` whose bounds are set to `X [-180, 180]` and `Y [-90, 90]` (a sub-region zooms in).

### Interfaces

- [IShape](Jumbee.Console.Drawing.IShape.md) — A shape that can be drawn on a `Canvas` via `Add`.

### Enums

- [CanvasMarker](Jumbee.Console.Drawing.CanvasMarker.md) — Selects the glyph set (and thus the sub-cell resolution) a `Canvas` draws its shapes with. The default is `Braille`.
- [MapResolution](Jumbee.Console.Drawing.MapResolution.md) — The point density of a `WorldMap` — how finely the coastlines are sampled.

## Jumbee.Console.Snapshot

### Classes

- [AnsiConsoleSession](Jumbee.Console.Snapshot.AnsiConsoleSession.md) — A stateful counterpart to `AnsiConsoleSnapshot` for testing the *live* render — used to reproduce diff/cursor or async-ordering bugs that only appear across frames (e.g. press → release).
- [AnsiConsoleSnapshot](Jumbee.Console.Snapshot.AnsiConsoleSnapshot.md) — Drives the *real* `ConsoleManager` ANSI render path headlessly, captures the emitted escape sequences via `AnsiOutput`, and parses them back into an `AnsiScreen`.
- [AnsiScreen](Jumbee.Console.Snapshot.AnsiScreen.md) — A small VT/ANSI screen model that parses the subset of escape sequences `ConsoleManager` emits and maintains a cell grid, exactly as a terminal would.
- [ConsoleSnapshot](Jumbee.Console.Snapshot.ConsoleSnapshot.md) — Renders Jumbee.Console controls headlessly (without a real terminal) to a `ConsoleBuffer`, and converts that buffer to a text or PNG snapshot. Intended for tests and visual verification.
- [SnapshotImageOptions](Jumbee.Console.Snapshot.SnapshotImageOptions.md) — Options controlling how a `ConsoleBuffer` is rendered to a PNG image.

