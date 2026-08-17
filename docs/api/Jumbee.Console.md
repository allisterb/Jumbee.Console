# <a id="Jumbee_Console"></a> Namespace Jumbee.Console

### Namespaces

 [Jumbee.Console.Documents](Jumbee.Console.Documents.md)

 [Jumbee.Console.Drawing](Jumbee.Console.Drawing.md)

 [Jumbee.Console.Snapshot](Jumbee.Console.Snapshot.md)

### Classes

 [AnimatedControl](Jumbee.Console.AnimatedControl.md)

Base class for a control that advances through frames on a timer, repainting each frame while running.

 [AnsiConsoleBuffer](Jumbee.Console.AnsiConsoleBuffer.md)

An implementation of Spectre.Console.IAnsiConsole that writes to a ConsoleBuffer.

 [AnsiInputDecoder](Jumbee.Console.AnsiInputDecoder.md)

A streaming state machine that turns a raw terminal input char stream into <xref href="Jumbee.Console.TerminalInputEvent" data-throw-if-not-resolved="false"></xref>s:
printable text, control/navigation keys (CSI/SS3), SGR (1006) mouse reports, bracketed paste (DEC 2004), and
focus in/out (DEC 1004).

 [Autocomplete](Jumbee.Console.Autocomplete.md)

Attaches type-ahead suggestions to a <xref href="Jumbee.Console.TextInput" data-throw-if-not-resolved="false"></xref>.

 [Badge](Jumbee.Console.Badge.md)

A small inline status pill — short text on a filled background with a little horizontal padding (e.g.
<code>200 OK</code>, <code>read-only</code>, a method tag).

 [BarChart](Jumbee.Console.BarChart.md)

A bar chart.

 [BarChart.BarChartItem](Jumbee.Console.BarChart.BarChartItem.md)

A single labelled, coloured item in a <xref href="Jumbee.Console.BarChart" data-throw-if-not-resolved="false"></xref>.

 [Boundary](Jumbee.Console.Boundary.md)

A single-child layout that pins its child's size.

 [Button](Jumbee.Console.Button.md)

A focusable, clickable button that renders a fixed-width text label.

 [Canvas](Jumbee.Console.Canvas.md)

A blank drawing surface on which you paint <xref href="Jumbee.Console.Drawing.IShape" data-throw-if-not-resolved="false"></xref>s (<xref href="Jumbee.Console.Drawing.Line" data-throw-if-not-resolved="false"></xref>, <xref href="Jumbee.Console.Drawing.Rectangle" data-throw-if-not-resolved="false"></xref>,
<xref href="Jumbee.Console.Drawing.Circle" data-throw-if-not-resolved="false"></xref>, <xref href="Jumbee.Console.Drawing.Points" data-throw-if-not-resolved="false"></xref>, <xref href="Jumbee.Console.Drawing.FilledLine" data-throw-if-not-resolved="false"></xref>) in an arbitrary coordinate system, rendered
with sub-cell markers (braille by default).

 [ChatPrompt](Jumbee.Console.ChatPrompt.md)

The input area of an agent/chat CLI (Claude Code, Gemini CLI): a prompt glyph on the left that turns into an
animated <em>busy</em> spinner while an operation runs, and a single-line <xref href="Jumbee.Console.TextInput" data-throw-if-not-resolved="false"></xref> filling the
rest. Submitting (Enter) raises <xref href="Jumbee.Console.ChatPrompt.Submitted" data-throw-if-not-resolved="false"></xref>; any edit raises <xref href="Jumbee.Console.ChatPrompt.Changed" data-throw-if-not-resolved="false"></xref>.

 [Checkbox](Jumbee.Console.Checkbox.md)

A labelled checkbox that toggles an independent on/off state.

 [CodeEditor](Jumbee.Console.CodeEditor.md)

A composite control pairing a <xref href="Jumbee.Console.TextEditor" data-throw-if-not-resolved="false"></xref> with a <xref href="Jumbee.Console.LineNumberGutter" data-throw-if-not-resolved="false"></xref> docked to its left.

 [CompositeControl](Jumbee.Console.CompositeControl.md)

Base class for <em>composite</em> controls: a <xref href="Jumbee.Console.Control" data-throw-if-not-resolved="false"></xref> that owns and lays out several child
controls and presents them as a single control. It is a real <xref href="Jumbee.Console.Control" data-throw-if-not-resolved="false"></xref> (so it has its own
console buffer, participates in theming/painting, can be framed, and drops into any layout cell), but its
content is an internal <xref href="Jumbee.Console.ILayout" data-throw-if-not-resolved="false"></xref> arranging the children.

 [ConPty](Jumbee.Console.ConPty.md)

A pseudo-console (ConPTY) session: launches a process attached to a Windows pseudo console and exposes its
stdin/stdout as streams.

 [ConsoleBuffer](Jumbee.Console.ConsoleBuffer.md)

A ConsoleGUI.IConsole implementation that writes to a buffer.

 [ConsoleInputSource](Jumbee.Console.ConsoleInputSource.md)

The default <xref href="Jumbee.Console.IInputSource" data-throw-if-not-resolved="false"></xref>, reading keys from <xref href="System.Console" data-throw-if-not-resolved="false"></xref> and wrapping them as
<xref href="Jumbee.Console.KeyInputEvent" data-throw-if-not-resolved="false"></xref>s.

 [ContextMenu](Jumbee.Console.ContextMenu.md)

A floating, keyboard-navigable menu of <xref href="Jumbee.Console.MenuItem" data-throw-if-not-resolved="false"></xref>s, shown anchored in the ambient <xref href="Jumbee.Console.UI.Overlay" data-throw-if-not-resolved="false"></xref>.
The shared primitive behind drop-downs / context menus / a <xref href="Jumbee.Console.MenuBar" data-throw-if-not-resolved="false"></xref>'s menus.

 [Control](Jumbee.Console.Control.md)

Base class for all Jumbee.Console controls.

 [ControlExtensions](Jumbee.Console.ControlExtensions.md)

Fluent extension helpers for configuring controls, frames, and geometry values.

 [ControlFrame](Jumbee.Console.ControlFrame.md)

Draws a border around a control together with margins and a title bar, and sets the foreground and background colors.

 [DataTable](Jumbee.Console.DataTable.md)

An interactive data grid.

 [DefaultGlyphTheme](Jumbee.Console.DefaultGlyphTheme.md)

The built-in glyph theme: every glyph uses <xref href="Jumbee.Console.IGlyphTheme" data-throw-if-not-resolved="false"></xref>'s default values.

 [DefaultStyleTheme](Jumbee.Console.DefaultStyleTheme.md)

The built-in style theme: every token uses <xref href="Jumbee.Console.IStyleTheme" data-throw-if-not-resolved="false"></xref>'s default values.

 [Dialog](Jumbee.Console.Dialog.md)

A modal dialog window shown over the ambient <xref href="Jumbee.Console.UI.Overlay" data-throw-if-not-resolved="false"></xref>: a titled, bordered box that takes
exclusive focus (the layer beneath is dimmed and click-blocked) until dismissed.

 [Digits](Jumbee.Console.Digits.md)

Renders text using large three-row "seven-segment" glyphs, for clocks, counters and headline figures.

 [Dispatcher](Jumbee.Console.Dispatcher.md)

Owns a single UI thread and a serialized work queue. UI state mutation and rendering are intended to
run on this thread; other threads marshal work onto it via <xref href="Jumbee.Console.Dispatcher.Post(System.Action)" data-throw-if-not-resolved="false"></xref>, <xref href="Jumbee.Console.Dispatcher.Invoke(System.Action)" data-throw-if-not-resolved="false"></xref>,
or <xref href="Jumbee.Console.Dispatcher.InvokeAsync(System.Action)" data-throw-if-not-resolved="false"></xref>.

 [DockPanel](Jumbee.Console.DockPanel.md)

A two-child layout that pins one control to an edge and fills the remaining space with the other.

 [DocumentClosingEventArgs](Jumbee.Console.DocumentClosingEventArgs.md)

Arguments for <xref href="Jumbee.Console.MultiTabCodeEditor.DocumentClosing" data-throw-if-not-resolved="false"></xref>. Set <xref href="Jumbee.Console.DocumentClosingEventArgs.Cancel" data-throw-if-not-resolved="false"></xref> to keep the
    document open (e.g. after confirming unsaved changes).

 [FeedHandle](Jumbee.Console.FeedHandle.md)

A handle to a running background feed started by <xref href="Jumbee.Console.Control.Feed(System.Action%2cSystem.Int32%2cSystem.Action%7bSystem.Exception%7d)" data-throw-if-not-resolved="false"></xref> and its overloads.
Cancel it to stop the feed; await <xref href="Jumbee.Console.FeedHandle.Completion" data-throw-if-not-resolved="false"></xref> (or <xref href="Jumbee.Console.FeedHandle.StopAsync" data-throw-if-not-resolved="false"></xref>) to know the in-flight tick
has finished — for safely disposing a resource the feed's producer reads.

 [FileBrowser](Jumbee.Console.FileBrowser.md)

A two-pane file chooser: a lazily-populated directory <xref href="Jumbee.Console.Tree" data-throw-if-not-resolved="false"></xref> on the left, the current directory's
contents in a <xref href="Jumbee.Console.ListBox" data-throw-if-not-resolved="false"></xref> on the right, a path field above and a filter drop-down below.

 [FocusInputEvent](Jumbee.Console.FocusInputEvent.md)

Terminal focus gained/lost (DEC mode 1004).

 [Footer](Jumbee.Console.Footer.md)

A one-row key-hints bar (e.g. <code>^j Send  ^t Method  ^c Quit  f1 Help</code>), filling the available width.

 [Gauge](Jumbee.Console.Gauge.md)

A single-row horizontal progress bar: the track is filled proportional to <xref href="Jumbee.Console.Gauge.Value" data-throw-if-not-resolved="false"></xref> / <xref href="Jumbee.Console.Gauge.Max" data-throw-if-not-resolved="false"></xref>,
optionally followed by the percentage and the raw value — e.g. <code>████████░░░░  34.5% (126)</code>. For dashboards
(year/day progress, a deployment %, a capacity meter).

 [GlassBlend](Jumbee.Console.GlassBlend.md)

Colour blending for <xref href="Jumbee.Console.GlassPanel" data-throw-if-not-resolved="false"></xref>: a gamma-space lerp (cheap, matches <xref href="ConsoleGUI.Data.Color.Mix(ConsoleGUI.Data.Color%40%2cSystem.Single)" data-throw-if-not-resolved="false"></xref>) or a
gamma-correct blend in linear light via two lookup tables (no runtime <code>pow</code>), plus a rough estimate of how
much of a cell a glyph inks (for the perceived-colour collapse).

 [GlassPanel](Jumbee.Console.GlassPanel.md)

A translucent "glass" panel: a fixed-size overlay that shows the layer beneath it, tinted, with its own
<xref href="Jumbee.Console.GlassPanel.Content" data-throw-if-not-resolved="false"></xref> drawn opaquely on top. Host it non-modally over the current UI with <xref href="Jumbee.Console.GlassPanel.Show(System.Int32%2cSystem.Int32%2cJumbee.Console.Overlay)" data-throw-if-not-resolved="false"></xref>
(it floats via <xref href="Jumbee.Console.Overlay.ShowPassive(Jumbee.Console.Control%2cSystem.Int32%2cSystem.Int32)" data-throw-if-not-resolved="false"></xref>, so the layer beneath keeps focus and keeps receiving input).

 [UI.GlobalInputListener](Jumbee.Console.UI.GlobalInputListener.md)

Input listener that dispatches globally-registered hotkeys before any control sees the event.

 [Globe](Jumbee.Console.Globe.md)

A ray-traced globe of the Earth — a shaded, colour-mapped sphere drawn into the control's buffer. Display-only
by default; spin, tilt, and zoom it from a frame or timer feed.

 [Grid](Jumbee.Console.Grid.md)

A grid layout with controls arranged in rows and columns.

 [HelpControl](Jumbee.Console.HelpControl.md)

The global help dialog: a fixed-size modal composite showing one tab per <xref href="Jumbee.Console.HelpInfo" data-throw-if-not-resolved="false"></xref> (built by
<xref href="Jumbee.Console.UI.ShowHelp" data-throw-if-not-resolved="false"></xref> from every control's <xref href="Jumbee.Console.Control.GetHelpInfo" data-throw-if-not-resolved="false"></xref>), with a Close button.

 [HelpInfo](Jumbee.Console.HelpInfo.md)

A control's entry in the global help dialog (one tab). Mutable so an <xref href="Jumbee.Console.Control.OnHelp" data-throw-if-not-resolved="false"></xref> handler can
tweak it in place.

 [BarChart.HorizontalBar](Jumbee.Console.BarChart.HorizontalBar.md)

A horizontally-drawn bar in a <xref href="Jumbee.Console.BarChart" data-throw-if-not-resolved="false"></xref>, optionally showing its value and remaining track.

 [HorizontalStackPanel](Jumbee.Console.HorizontalStackPanel.md)

A layout that arranges its child controls in a single horizontal row.

 [UI.HotKeys](Jumbee.Console.UI.HotKeys.md)

Factory helpers and well-known <xref href="System.ConsoleKeyInfo" data-throw-if-not-resolved="false"></xref> constants for registering hotkeys.

 [UI.InputEventArgs](Jumbee.Console.UI.InputEventArgs.md)

Arguments for control input handling, wrapping the decoded <xref href="Jumbee.Console.UI.InputEventArgs.InputEvent" data-throw-if-not-resolved="false"></xref>.

 [InteractiveMarkdownEditor](Jumbee.Console.InteractiveMarkdownEditor.md)

A live, split-pane Markdown editor for the terminal — the TUI equivalent of a web Markdown editor. The left pane
is a <xref href="Jumbee.Console.CodeEditor" data-throw-if-not-resolved="false"></xref> with Markdown syntax highlighting; the right pane is a <xref href="Jumbee.Console.MarkdownViewer" data-throw-if-not-resolved="false"></xref>
that re-renders the document — headings, tables and syntax-highlighted code — as you type (see
<xref href="Jumbee.Console.InteractiveSourceEditor" data-throw-if-not-resolved="false"></xref> for the sync model).

 [InteractiveSourceEditor](Jumbee.Console.InteractiveSourceEditor.md)

Base for live, split-pane source editors: a <xref href="Jumbee.Console.CodeEditor" data-throw-if-not-resolved="false"></xref> in one pane and a read-only preview control in
the other, wired so the preview re-renders as the source is edited. A draggable <xref href="Jumbee.Console.SplitPanel" data-throw-if-not-resolved="false"></xref> divider
sits between them (drag it, or focus it and press the arrows). Subclasses supply the editor's language, the preview
control, and how to push text into it (<xref href="Jumbee.Console.InteractiveSourceEditor.ApplyPreviewText(System.String)" data-throw-if-not-resolved="false"></xref>).

 [KeyHelp](Jumbee.Console.KeyHelp.md)

One keystroke (or chord) and what it does, listed in a control's <xref href="Jumbee.Console.HelpInfo" data-throw-if-not-resolved="false"></xref>.

 [KeyInputEvent](Jumbee.Console.KeyInputEvent.md)

A key press.

 [Layout<T\>](Jumbee.Console.Layout\-1.md)

Base class for Jumbee.Console layouts wrapping a ConsoleGUI layout control <code class="typeparamref">T</code> and exposing it through <xref href="Jumbee.Console.ILayout" data-throw-if-not-resolved="false"></xref>.

 [LineNumberGutter](Jumbee.Console.LineNumberGutter.md)

A narrow, non-interactive column of right-aligned line numbers, highlighting the active row. Intended as an
adornment inside a composite (e.g. <xref href="Jumbee.Console.CodeEditor" data-throw-if-not-resolved="false"></xref>); width auto-grows with the digit count.

 [Link](Jumbee.Console.Link.md)

A focusable, clickable text link. Activating it (a mouse click, or Enter/Space while focused) opens
<xref href="Jumbee.Console.Link.Url" data-throw-if-not-resolved="false"></xref> with the system default handler and raises <xref href="Jumbee.Console.Link.Activated" data-throw-if-not-resolved="false"></xref>.

 [ListBox](Jumbee.Console.ListBox.md)

Displays a flat list of items and allows user input navigation and selection.

 [ListBox.ListBoxItem](Jumbee.Console.ListBox.ListBoxItem.md)

An item in a ListBox.

 [Log](Jumbee.Console.Log.md)

An append-only, scrolling log of Spectre <xref href="Spectre.Console.Rendering.IRenderable" data-throw-if-not-resolved="false"></xref> entries — markup strings, tables, rules,
etc. — that always shows the most recent entries that fit (a "tail" view). Each entry is a renderable, so log
lines can be styled/coloured. <xref href="Jumbee.Console.Log.Write(System.String)" data-throw-if-not-resolved="false"></xref>/<xref href="Jumbee.Console.Log.Write(Spectre.Console.Rendering.IRenderable)" data-throw-if-not-resolved="false"></xref> are safe to call
from any thread.

 [MarkdownViewer](Jumbee.Console.MarkdownViewer.md)

A read-only, scrollable Markdown viewer. Renders CommonMark — headings, bold/italic, block-quotes, ordered and
unordered lists, links, syntax-highlighted fenced code blocks, and box-drawn tables — via NTokenizers'
Spectre.Console markdown writer. Wrap it in a <xref href="Jumbee.Console.ControlFrame" data-throw-if-not-resolved="false"></xref> (e.g. <code>viewer.WithFrame()</code>) to get a
border, title and scrollbar; ↑/↓, PgUp/PgDn, Home/End and the mouse wheel scroll it.

 [MenuBar](Jumbee.Console.MenuBar.md)

A horizontal bar of top-level menu titles (e.g. <code>File  Edit  View</code>).

 [MenuItem](Jumbee.Console.MenuItem.md)

One entry in a <xref href="Jumbee.Console.ContextMenu" data-throw-if-not-resolved="false"></xref>: a label, an optional right-aligned shortcut hint, an action invoked
when chosen, an enabled flag, and an optional <xref href="Jumbee.Console.MenuItem.Submenu" data-throw-if-not-resolved="false"></xref>. Use <xref href="Jumbee.Console.MenuItem.Separator" data-throw-if-not-resolved="false"></xref> for a
non-selectable divider line.

 [MouseInputEvent](Jumbee.Console.MouseInputEvent.md)

A mouse action at cell coordinates (0-based).

 [MultiTabCodeEditor](Jumbee.Console.MultiTabCodeEditor.md)

A tabbed group of <xref href="Jumbee.Console.CodeEditor" data-throw-if-not-resolved="false"></xref>s — a VS-Code-style editor area. Each open document is a closable tab
(click the ✕ on the active/hovered tab), and a "+" button at the end of the bar opens a new document.

 [Overlay](Jumbee.Console.Overlay.md)

A layered layout: a persistent <xref href="Jumbee.Console.Overlay.Bottom" data-throw-if-not-resolved="false"></xref> layer with an optional floating popup composited on top
(wraps ConsoleGUI's <xref href="ConsoleGUI.Controls.Overlay" data-throw-if-not-resolved="false"></xref>).

 [UI.PaintEventArgs](Jumbee.Console.UI.PaintEventArgs.md)

Arguments for the <xref href="Jumbee.Console.UI.Paint" data-throw-if-not-resolved="false"></xref> event; carries no data (controls read their own state).

 [PasteInputEvent](Jumbee.Console.PasteInputEvent.md)

A bracketed-paste payload, delivered as one event so it is never re-interpreted as keystrokes.

 [PerfHud](Jumbee.Console.PerfHud.md)

A translucent "glass" HUD showing live UI telemetry — render, terminal-write and queue-wait times (µs), CPU,
working set, allocation rate and, the headline for a no-lock design, monitor lock contentions — floating over
the app.

 [Plot](Jumbee.Console.Plot.md)

A line/scatter chart backed by the ConsolePlot library, rendered into the control's buffer. Add data with
<xref href="Jumbee.Console.Plot.AddSeries(System.Double%5b%5d%2cSystem.Double%5b%5d%2cConsolePlot.Drawing.Tools.PointPen)" data-throw-if-not-resolved="false"></xref> and tune the axes/grid/ticks with the <code>Configure*</code> methods.

 [PlotSeries](Jumbee.Console.PlotSeries.md)

A live, updatable series in a <xref href="Jumbee.Console.Plot" data-throw-if-not-resolved="false"></xref>. Returned by <xref href="Jumbee.Console.Plot.AddLiveSeries(System.Nullable%7bJumbee.Console.Color%7d%2cJumbee.Console.PlotBrush)" data-throw-if-not-resolved="false"></xref> (line),
<xref href="Jumbee.Console.Plot.AddLiveScatter(System.Nullable%7bJumbee.Console.Color%7d%2cJumbee.Console.PlotBrush)" data-throw-if-not-resolved="false"></xref> (markers) or <xref href="Jumbee.Console.Plot.AddLiveBars(System.Nullable%7bJumbee.Console.Color%7d%2cSystem.Double%2cSystem.Double)" data-throw-if-not-resolved="false"></xref>; hold onto it and feed data as it
arrives with <xref href="Jumbee.Console.PlotSeries.SetData(System.Double%5b%5d%2cSystem.Double%5b%5d)" data-throw-if-not-resolved="false"></xref>, <xref href="Jumbee.Console.PlotSeries.SetValues(System.Double%5b%5d)" data-throw-if-not-resolved="false"></xref>, <xref href="Jumbee.Console.PlotSeries.Push(System.Double%2cSystem.Double%2cSystem.Int32)" data-throw-if-not-resolved="false"></xref> or <xref href="Jumbee.Console.PlotSeries.Clear" data-throw-if-not-resolved="false"></xref>.

 [ProcessMetrics](Jumbee.Console.ProcessMetrics.md)

Collects live process/runtime performance metrics for the perf HUD by reading the runtime APIs the
<code>System.Runtime</code> meter wraps (<xref href="System.GC" data-throw-if-not-resolved="false"></xref>, <xref href="System.Environment" data-throw-if-not-resolved="false"></xref>, <xref href="System.Threading.ThreadPool" data-throw-if-not-resolved="false"></xref>,
<xref href="System.Threading.Monitor" data-throw-if-not-resolved="false"></xref>) directly — no <code>MeterListener</code>, so there is nothing to sample-schedule and no
observable-instrument staleness.

 [ProgressBar](Jumbee.Console.ProgressBar.md)

A single-row task progress display, modelled on one row of a Spectre.Console <code>Progress</code>: a
<xref href="Jumbee.Console.ProgressBar.Description" data-throw-if-not-resolved="false"></xref>, a bar filled to <xref href="Jumbee.Console.ProgressBar.Value" data-throw-if-not-resolved="false"></xref> / <xref href="Jumbee.Console.ProgressBar.Max" data-throw-if-not-resolved="false"></xref>, then optional
<xref href="Jumbee.Console.ProgressBar.ShowPercentage?text=percentage" data-throw-if-not-resolved="false"></xref>, <xref href="Jumbee.Console.ProgressBar.TimeDisplay?text=time" data-throw-if-not-resolved="false"></xref> and
<xref href="Jumbee.Console.ProgressBar.ShowSpinner?text=spinner" data-throw-if-not-resolved="false"></xref> columns — e.g. <code>Consulting the oracle ──── 96% 00:00:00 ⣷</code>.

 [Prompt](Jumbee.Console.Prompt.md)

Base class for controls that prompt the user for input.

 [Pty](Jumbee.Console.Pty.md)

Factory that opens an <xref href="Jumbee.Console.IPty" data-throw-if-not-resolved="false"></xref> using the right backend for the current OS: the Windows pseudo console
(<xref href="Jumbee.Console.ConPty" data-throw-if-not-resolved="false"></xref>) or a Unix pseudo terminal (<xref href="Jumbee.Console.UnixPty" data-throw-if-not-resolved="false"></xref>).

 [RadioButton](Jumbee.Console.RadioButton.md)

A labelled radio button.

 [RadioSet](Jumbee.Console.RadioSet.md)

A vertical group of mutually-exclusive radio options, exactly one selected at a time.

 [RenderableControl](Jumbee.Console.RenderableControl.md)

A control that implements Spectre.Console.IRenderable

 [ResizeInputEvent](Jumbee.Console.ResizeInputEvent.md)

Terminal resized. Not decoded from the char stream — raised by the input source on a resize signal.

 [RunChart](Jumbee.Console.RunChart.md)

A live multi-series time chart with a legend — a streaming line <xref href="Jumbee.Console.Plot" data-throw-if-not-resolved="false"></xref> on the left and a per-series
stat readout (name + current / delta / max / min) on the right, in the style of a monitoring "run chart".

 [RunSeries](Jumbee.Console.RunSeries.md)

A handle to one <xref href="Jumbee.Console.RunChart" data-throw-if-not-resolved="false"></xref> series: push values with <xref href="Jumbee.Console.RunSeries.Push(System.Double)" data-throw-if-not-resolved="false"></xref>. Tracks the current value, the
delta from the previous value, and the running max/min shown in the chart's legend.

 [Select](Jumbee.Console.Select.md)

A drop-down selector.

 [SelectionList](Jumbee.Console.SelectionList.md)

A vertical list of independently-checkable options (multi-select).

 [SelectionStylesExtensions](Jumbee.Console.SelectionStylesExtensions.md)

Turns a <xref href="Jumbee.Console.SelectionStyle" data-throw-if-not-resolved="false"></xref> into the prefix + text style a control applies to its selected item.

 [Slider](Jumbee.Console.Slider.md)

A single-row draggable value control: an optional <xref href="Jumbee.Console.Slider.Label" data-throw-if-not-resolved="false"></xref>, a track filled to <xref href="Jumbee.Console.Slider.Value" data-throw-if-not-resolved="false"></xref>
between <xref href="Jumbee.Console.Slider.Minimum" data-throw-if-not-resolved="false"></xref> and <xref href="Jumbee.Console.Slider.Maximum" data-throw-if-not-resolved="false"></xref>, and an optional numeric readout — e.g.
<code>Gravity ████▌      9.80</code>.

 [Sparkline](Jumbee.Console.Sparkline.md)

A compact, single-row chart that draws a series of numeric values as block bars (one cell per value),
scaling each value's height against the series maximum.

 [SpectreControl<T\>](Jumbee.Console.SpectreControl\-1.md)

Wraps an existing Spectre.Console <xref href="Spectre.Console.Rendering.IRenderable" data-throw-if-not-resolved="false"></xref> control for use with ConsoleGUI control and layout types.

 [SpectreLiveDisplay](Jumbee.Console.SpectreLiveDisplay.md)

A Spectre.Console LiveDisplay widget.

 [SpectreTaskProgress](Jumbee.Console.SpectreTaskProgress.md)

A Spectre.Console Progress widget.

 [Spinner](Jumbee.Console.Spinner.md)

An animated spinner glyph with an optional label, cycling through a <xref href="Spectre.Console.Spinner" data-throw-if-not-resolved="false"></xref>'s frames.

 [SplitDivider](Jumbee.Console.SplitDivider.md)

The draggable divider between a <xref href="Jumbee.Console.SplitPanel" data-throw-if-not-resolved="false"></xref>'s two panes.

 [SplitPanel](Jumbee.Console.SplitPanel.md)

A container that splits its area between two panes with a draggable divider between them.

 [SplitPanelDockPanel](Jumbee.Console.SplitPanelDockPanel.md)

The visual scaffold behind <xref href="Jumbee.Console.SplitPanel" data-throw-if-not-resolved="false"></xref>: two nested ConsoleGUI <xref href="Jumbee.Console.DockPanel" data-throw-if-not-resolved="false"></xref>s laying out
<code>[first | divider | second]</code> along the split axis.

 [Switch](Jumbee.Console.Switch.md)

A sliding on/off switch.

 [TabCloseEventArgs](Jumbee.Console.TabCloseEventArgs.md)

Arguments for <xref href="Jumbee.Console.TabPanel.TabCloseRequested" data-throw-if-not-resolved="false"></xref>.

 [TabHeader](Jumbee.Console.TabHeader.md)

A single clickable tab label in a <xref href="Jumbee.Console.TabPanel" data-throw-if-not-resolved="false"></xref>'s tab bar.

 [TabItem](Jumbee.Console.TabItem.md)

A handle to a tab in a <xref href="Jumbee.Console.TabPanel" data-throw-if-not-resolved="false"></xref> — stable across add/remove/reorder (unlike a positional index).

 [TabPanel](Jumbee.Console.TabPanel.md)

A tabbed container: a bar of selectable <xref href="Jumbee.Console.TabHeader" data-throw-if-not-resolved="false"></xref> labels docked on one edge, with the selected
tab's content filling the rest.

 [TabPanelDockPanel](Jumbee.Console.TabPanelDockPanel.md)

The visual scaffold behind <xref href="Jumbee.Console.TabPanel" data-throw-if-not-resolved="false"></xref>: a ConsoleGUI <xref href="ConsoleGUI.Controls.DockPanel" data-throw-if-not-resolved="false"></xref> that
docks a thin tab bar (a horizontal or vertical stack of <xref href="Jumbee.Console.TabHeader" data-throw-if-not-resolved="false"></xref> cells) on one edge and fills the
rest with the active tab's content. It does no selection bookkeeping — <xref href="Jumbee.Console.TabPanel" data-throw-if-not-resolved="false"></xref> owns the model and
drives this through <xref href="Jumbee.Console.TabPanelDockPanel.SetHeaders(System.Collections.Generic.IEnumerable%7bConsoleGUI.IControl%7d)" data-throw-if-not-resolved="false"></xref> / <xref href="Jumbee.Console.TabPanelDockPanel.SetFill(ConsoleGUI.IControl)" data-throw-if-not-resolved="false"></xref>.

 [TerminalEmulator](Jumbee.Console.TerminalEmulator.md)

A control that runs a child process in a pseudo-console (<xref href="Jumbee.Console.ConPty" data-throw-if-not-resolved="false"></xref>), parses its ANSI output with
VtNetCore, and paints the emulated screen into the control's cell area. Input routed to the focused control is
translated to terminal bytes and sent to the process.

 [TerminalInputEvent](Jumbee.Console.TerminalInputEvent.md)

Base type for the unified terminal input stream produced by <xref href="Jumbee.Console.AnsiInputDecoder" data-throw-if-not-resolved="false"></xref>: a single
sequence of key / mouse / paste / focus events, replacing the keyboard-only <xref href="System.ConsoleKeyInfo" data-throw-if-not-resolved="false"></xref> path.

 [TextEditor](Jumbee.Console.TextEditor.md)

A text editor control with syntax highlighting for supported languages.

 [TextInput](Jumbee.Console.TextInput.md)

A single-line text entry control: caret, selection (Shift+navigation), horizontal scrolling when the text
exceeds the width, an optional muted placeholder shown while empty, and optional password masking.

 [TextLabel](Jumbee.Console.TextLabel.md)

Displays a single-line text label with a defined horizontal or vertical orientation, foreground and background
colour, and optional text decoration (e.g. bold, underline).

 [TextPanel](Jumbee.Console.TextPanel.md)

Displays a block of multi-line Spectre <xref href="Jumbee.Console.TextPanel.Markup" data-throw-if-not-resolved="false"></xref> text — the counterpart to the single-line
<xref href="Jumbee.Console.TextLabel" data-throw-if-not-resolved="false"></xref> and the append-only <xref href="Jumbee.Console.Log" data-throw-if-not-resolved="false"></xref>.

 [TextPrompt](Jumbee.Console.TextPrompt.md)

A single-line text input that shows a prompt label and edits an inline entry with a terminal cursor.

 [ToggleButton](Jumbee.Console.ToggleButton.md)

Shared base for the single-state toggle widgets (<xref href="Jumbee.Console.Checkbox" data-throw-if-not-resolved="false"></xref>, <xref href="Jumbee.Console.RadioButton" data-throw-if-not-resolved="false"></xref>,
<xref href="Jumbee.Console.Switch" data-throw-if-not-resolved="false"></xref>).

 [ToggleList](Jumbee.Console.ToggleList.md)

Shared base for the vertical, navigable toggle lists (<xref href="Jumbee.Console.RadioSet" data-throw-if-not-resolved="false"></xref>, <xref href="Jumbee.Console.SelectionList" data-throw-if-not-resolved="false"></xref>).

 [Tree](Jumbee.Console.Tree.md)

Displays a hierarchical list of items in a tree layout.

 [Tree.TreeNode](Jumbee.Console.Tree.TreeNode.md)

Represents a tree node.

 [UI](Jumbee.Console.UI.md)

Manages th overall UI and provides a paint event for controls to subscribe to.

 [UnixPty](Jumbee.Console.UnixPty.md)

A Unix pseudo terminal (Linux/macOS) session.

 [BarChart.VerticalBar](Jumbee.Console.BarChart.VerticalBar.md)

A vertically-drawn bar in a <xref href="Jumbee.Console.BarChart" data-throw-if-not-resolved="false"></xref>.

 [VerticalStackPanel](Jumbee.Console.VerticalStackPanel.md)

A layout that arranges its child controls in a single vertical column.

 [VtInputSource](Jumbee.Console.VtInputSource.md)

An <xref href="Jumbee.Console.IInputSource" data-throw-if-not-resolved="false"></xref> that puts the terminal into VT input mode and enables mouse, bracketed-paste, and
focus reporting, then reads the raw stdin byte stream, decodes it (UTF-8) and runs it through
<xref href="Jumbee.Console.AnsiInputDecoder" data-throw-if-not-resolved="false"></xref> to produce <xref href="Jumbee.Console.TerminalInputEvent" data-throw-if-not-resolved="false"></xref>s.

### Structs

 [ButtonStyle](Jumbee.Console.ButtonStyle.md)

The appearance of a <code>Button</code>: its fill <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in each interaction state (text colour +
background), its <xref href="Jumbee.Console.ButtonStyle.Shape" data-throw-if-not-resolved="false"></xref>, an optional fixed/minimum width, and whether the label
is bold.

 [Color](Jumbee.Console.Color.md)

An RGB colour. Value type: two colours are equal when their channels are.

 [FooterHint](Jumbee.Console.FooterHint.md)

A single key-binding hint shown in a <xref href="Jumbee.Console.Footer" data-throw-if-not-resolved="false"></xref>: the key chord and what it does.

 [GaugeStyle](Jumbee.Console.GaugeStyle.md)

The per-part <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> a <code>Gauge</code> composes: the filled portion of the bar,
the empty track behind it, and the percent/value readout (and any inline label).

 [HScroll](Jumbee.Console.HScroll.md)

An opt-in horizontal-scroll offset for controls that render a fixed-width buffer wider than their viewport and
window it in <code>Blit</code> (the <xref href="Jumbee.Console.ControlFrame" data-throw-if-not-resolved="false"></xref> only scrolls vertically).

 [PlotPalette](Jumbee.Console.PlotPalette.md)

The ordered colours a plot cycles through for series that don't name one.

 [ProgressBarGlyphs](Jumbee.Console.ProgressBarGlyphs.md)

The glyphs (no colours) a <code>ProgressBar</code> draws its bar with: the <xref href="Jumbee.Console.ProgressBarGlyphs.Fill" data-throw-if-not-resolved="false"></xref> for a filled cell and the
<xref href="Jumbee.Console.ProgressBarGlyphs.Track" data-throw-if-not-resolved="false"></xref> for an empty one, plus the <xref href="Jumbee.Console.ProgressBarGlyphs.Mode" data-throw-if-not-resolved="false"></xref> that selects solid-band or per-cell-glyph
rendering.

 [ProgressBarStyle](Jumbee.Console.ProgressBarStyle.md)

The per-part <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> a <code>ProgressBar</code> composes: the task <xref href="Jumbee.Console.ProgressBarStyle.Description" data-throw-if-not-resolved="false"></xref>, the
filled and empty portions of the bar (<xref href="Jumbee.Console.ProgressBarStyle.Fill" data-throw-if-not-resolved="false"></xref>/<xref href="Jumbee.Console.ProgressBarStyle.Track" data-throw-if-not-resolved="false"></xref>), and the three optional
readouts — <xref href="Jumbee.Console.ProgressBarStyle.Percentage" data-throw-if-not-resolved="false"></xref>, <xref href="Jumbee.Console.ProgressBarStyle.Time" data-throw-if-not-resolved="false"></xref> and <xref href="Jumbee.Console.ProgressBarStyle.Spinner" data-throw-if-not-resolved="false"></xref>.

 [RowSpan](Jumbee.Console.RowSpan.md)

A run of content rows: <code class="paramref">Start</code> and the <code class="paramref">Height</code> rows following it.

 [ScrollBarGlyphs](Jumbee.Console.ScrollBarGlyphs.md)

The glyphs (no colours) a control frame's vertical scrollbar draws for each part: the moving thumb, the track
behind it, and the two end arrows.

 [ScrollBarStyle](Jumbee.Console.ScrollBarStyle.md)

The per-part <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> (foreground/background/decoration, no glyph) a control frame applies to its
vertical scrollbar.

 [SliderStyle](Jumbee.Console.SliderStyle.md)

The per-part <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> a <code>Slider</code> composes: the <xref href="Jumbee.Console.SliderStyle.Label" data-throw-if-not-resolved="false"></xref>, the filled and empty
portions of the track (<xref href="Jumbee.Console.SliderStyle.Fill" data-throw-if-not-resolved="false"></xref>/<xref href="Jumbee.Console.SliderStyle.Track" data-throw-if-not-resolved="false"></xref>), the draggable <xref href="Jumbee.Console.SliderStyle.Thumb" data-throw-if-not-resolved="false"></xref>, and the
numeric <xref href="Jumbee.Console.SliderStyle.Value" data-throw-if-not-resolved="false"></xref> readout.

 [Style](Jumbee.Console.Style.md)

A text style — foreground/background colour and text decoration — wrapping a Spectre.Console style. Exposes the
named colour palette (<xref href="Jumbee.Console.Style.Red1" data-throw-if-not-resolved="false"></xref>, <xref href="Jumbee.Console.Style.Cyan1" data-throw-if-not-resolved="false"></xref>, …) and decoration presets (<xref href="Jumbee.Console.Style.Bold" data-throw-if-not-resolved="false"></xref>,
<xref href="Jumbee.Console.Style.Italic" data-throw-if-not-resolved="false"></xref>, …) as ready-made tokens; combine them with <code>|</code>.

 [TitleStyle](Jumbee.Console.TitleStyle.md)

Describes how a control frame title is aligned, bordered, and colored.

### Interfaces

 [BarChart.IBarControl](Jumbee.Console.BarChart.IBarControl.md)

A single bar renderable within a <xref href="Jumbee.Console.BarChart" data-throw-if-not-resolved="false"></xref>.

 [IFocusable](Jumbee.Console.IFocusable.md)

A control that can receive keyboard focus and input.

 [IGlyphTheme](Jumbee.Console.IGlyphTheme.md)

The glyphs controls use for state indicators.

 [IInputSource](Jumbee.Console.IInputSource.md)

Supplies <xref href="Jumbee.Console.TerminalInputEvent" data-throw-if-not-resolved="false"></xref>s (keys, mouse, paste, focus) to the UI input loop.

 [ILayout](Jumbee.Console.ILayout.md)

Common interface for Jumbee.Console layout classes: a 2-D grid of focusable cells over a ConsoleGUI control, with focus navigation and input routing.

 [IPty](Jumbee.Console.IPty.md)

A pseudo-terminal session: a child process attached to a PTY, exposing its stdin/stdout as streams.

 [IScrollable](Jumbee.Console.IScrollable.md)

A control whose content can be taller than the space it is given, and which therefore wants its
<xref href="Jumbee.Console.ControlFrame" data-throw-if-not-resolved="false"></xref> to scroll it.

 [IStyleTheme](Jumbee.Console.IStyleTheme.md)

The general appearance theme: a set of semantic <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> tokens (foreground + background +
decoration) that controls compose when resolving their default appearance, plus the rest of a control's
non-glyph styling — e.g. a frame's border shape (<xref href="Jumbee.Console.IStyleTheme.FrameBorder" data-throw-if-not-resolved="false"></xref>) and its title's
position/border/colour.

 [ITheme](Jumbee.Console.ITheme.md)

A complete theme bundling an <xref href="Jumbee.Console.IStyleTheme" data-throw-if-not-resolved="false"></xref> and an <xref href="Jumbee.Console.IGlyphTheme" data-throw-if-not-resolved="false"></xref>, for callers that want
to customise both colours/styles and glyphs as one unit and apply them together (via <code>UI.SetTheme(ITheme)</code>).

### Enums

 [BadgeVariant](Jumbee.Console.BadgeVariant.md)

Preset colour schemes for a <xref href="Jumbee.Console.Badge" data-throw-if-not-resolved="false"></xref>, resolved from the active theme.

 [BorderPlacement](Jumbee.Console.BorderPlacement.md)

Which edges of a control frame's border are drawn.

 [BorderStyle](Jumbee.Console.BorderStyle.md)

The shape of a control frame's border.

 [ButtonShape](Jumbee.Console.ButtonShape.md)

The overall shape of a <code>Button</code>.

 [ChartOrientation](Jumbee.Console.ChartOrientation.md)

Whether a chart is laid out horizontally or vertically.

 [CursorStyle](Jumbee.Console.CursorStyle.md)

Terminal cursor shapes/blink, mapping to DECSCUSR (CSI Ps SP q) values. <xref href="Jumbee.Console.CursorStyle.Default" data-throw-if-not-resolved="false"></xref> (0) leaves
the terminal's configured cursor.

 [DialogButtons](Jumbee.Console.DialogButtons.md)

The predefined button set a <xref href="Jumbee.Console.Dialog" data-throw-if-not-resolved="false"></xref> shows along its bottom edge.

 [DialogResult](Jumbee.Console.DialogResult.md)

Which button dismissed a <xref href="Jumbee.Console.Dialog" data-throw-if-not-resolved="false"></xref> (or how it was dismissed).

 [DockedControlPlacement](Jumbee.Console.DockedControlPlacement.md)

Which edge a <xref href="Jumbee.Console.DockPanel" data-throw-if-not-resolved="false"></xref> pins its docked control to.

 [FileBrowserMode](Jumbee.Console.FileBrowserMode.md)

What a <xref href="Jumbee.Console.FileBrowser" data-throw-if-not-resolved="false"></xref> is asking the user to choose.

 [FocusStyle](Jumbee.Console.FocusStyle.md)

How the themed default focus cue is drawn on a focused control that isn't showing focus another way (no visible
frame border, and <code>Control.RendersOwnFocus</code> is false).

 [Justify](Jumbee.Console.Justify.md)

Horizontal alignment of text within its available width.

 [Language](Jumbee.Console.Language.md)

A source language for syntax highlighting, shared by the text/code controls (e.g. <xref href="Jumbee.Console.TextEditor" data-throw-if-not-resolved="false"></xref>,
<xref href="Jumbee.Console.CodeEditor" data-throw-if-not-resolved="false"></xref>). <xref href="Jumbee.Console.Language.None" data-throw-if-not-resolved="false"></xref> renders plain, unhighlighted text.

 [PlotBrush](Jumbee.Console.PlotBrush.md)

Selects the glyph set (and thus the sub-cell resolution) a <xref href="Jumbee.Console.Plot" data-throw-if-not-resolved="false"></xref> series is drawn with.

 [PlotColormap](Jumbee.Console.PlotColormap.md)

Selects the colour map a <xref href="Jumbee.Console.Plot" data-throw-if-not-resolved="false"></xref> heatmap uses to turn cell values into colours.

 [PlotLabelAlign](Jumbee.Console.PlotLabelAlign.md)

Horizontal anchoring of a <xref href="Jumbee.Console.Plot" data-throw-if-not-resolved="false"></xref> annotation label relative to its point.

 [ProgressBarFillMode](Jumbee.Console.ProgressBarFillMode.md)

How a <code>ProgressBar</code> draws its bar.

 [ProgressTimeDisplay](Jumbee.Console.ProgressTimeDisplay.md)

What the optional time column shows.

 [ScrollBarMode](Jumbee.Console.ScrollBarMode.md)

How a control frame renders its vertical scrollbar.

 [SelectPopupPosition](Jumbee.Console.SelectPopupPosition.md)

Where a <xref href="Jumbee.Console.Select" data-throw-if-not-resolved="false"></xref>'s dropdown opens relative to the control.

 [SelectionStyle](Jumbee.Console.SelectionStyle.md)

How a navigable control (e.g. <xref href="Jumbee.Console.IStyleTheme" data-throw-if-not-resolved="false"></xref> consumers like ListBox, Tree, TabPanel) renders its
selected/active item.

 [SplitOrientation](Jumbee.Console.SplitOrientation.md)

How a <xref href="Jumbee.Console.SplitPanel" data-throw-if-not-resolved="false"></xref> arranges its two panes.

 [TabBarDock](Jumbee.Console.TabBarDock.md)

Which edge a <xref href="Jumbee.Console.TabPanel" data-throw-if-not-resolved="false"></xref> docks its tab bar on.

 [TerminalModifiers](Jumbee.Console.TerminalModifiers.md)

Keyboard/mouse modifier flags decoded from the input stream.

 [TerminalMouseButton](Jumbee.Console.TerminalMouseButton.md)

Mouse button (or wheel direction) reported by the terminal.

 [TerminalMouseKind](Jumbee.Console.TerminalMouseKind.md)

The kind of mouse action reported.

 [TextLabelOrientation](Jumbee.Console.TextLabelOrientation.md)

The layout direction of a <xref href="Jumbee.Console.TextLabel" data-throw-if-not-resolved="false"></xref>.

 [TitleBorderStyle](Jumbee.Console.TitleBorderStyle.md)

The way a control frame title is drawn relative to the top border.

 [TitleColorStyle](Jumbee.Console.TitleColorStyle.md)

The way a control frame title is colored relative to the border color.

 [TitlePos](Jumbee.Console.TitlePos.md)

Position of a control frame title: which border (top or bottom) it is drawn in, and its horizontal alignment
within that border.

 [TreeGuide](Jumbee.Console.TreeGuide.md)

The line style used to draw the connecting guide lines of a <xref href="Jumbee.Console.Tree" data-throw-if-not-resolved="false"></xref>.

### Delegates

 [FocusableEventHandler](Jumbee.Console.FocusableEventHandler.md)

Handler for the <xref href="Jumbee.Console.IFocusable.OnFocus" data-throw-if-not-resolved="false"></xref> and <xref href="Jumbee.Console.IFocusable.OnLostFocus" data-throw-if-not-resolved="false"></xref> events.

 [Control.InitializationHandler](Jumbee.Console.Control.InitializationHandler.md)

Delegate for the <xref href="Jumbee.Console.Control.OnInitialization" data-throw-if-not-resolved="false"></xref> event.

