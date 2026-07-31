# Getting Started with Jumbee.Console
Jumbee.Console is a .NET library for building high-performance TUIs that take advantage of modern terminal capabilities. It uses a retained-mode user interface model where controls are regularly sent messages to repaint and redraw themselves by the library and the library takes care of rendering the resulting changes to the terminal. If you've built a desktop UI with WinForms or WPF, then the user interface model should feel familiar: a tree of controls, a single UI thread, and property changes that trigger a repaint.

> Status: pre-release (v0.1.x). APIs may still change.

## Contents
- [Requirements](#requirements)
- [Running examples](#running-examples)
- [Add the library to a project](#add-the-library-to-a-project)
- [A simple TUI app](#your-first-app)
- [A two-pane app: a list with a detail view](#a-two-pane-app-a-list-with-a-detail-view)
- [The essential concepts](#the-essential-concepts)
- [Testing without a terminal](#testing-without-a-terminal)
- [Where to go next](#where-to-go-next)

## Requirements

- .NET 10 
- (Preferred) A terminal emulator that supports ANSI escape sequences and UTF-8 and 24-bit color like Windows Terminal and most modern Linux terminals. Mouse support and hover need a VT-capable terminal.

Non-ANSI terminals like the Windows legacy terminal are also supported but with degraded performance and features and no mouse support.

## Running examples 
Easiest way to try out the examples is to pull the [Docker image](https://hub.docker.com/r/allisterb/jumbee-console):

Pull the latest image and run the examples browser:
`docker run --rm -it allisterb/jumbee-console:latest` 

Pull the latest image and run the agent harness example: 
`docker run --rm -it allisterb/jumbee-console:latest agent-harness` 

## Add the library to a project

`dotnet add package Jumbee.Console`

Or in a file-based app like [1.basics.cs](docs/getting-started/1.basics.cs), you can just use `#:package Jumbee.Console@*` at the top of the file.

## A simple TUI app
This is a pretty simple TUI ([1.basics.cs](docs/getting-started/1.basics.cs)) that shows a counter: a label and a button that increments it. 

```csharp
using Jumbee.Console;

using static Jumbee.Console.Color; //Import static color names

var count = 0;

var label = new TextLabel(TextLabelOrientation.Horizontal, "Count: 0", Cyan1);
var button = new Button("Increment");

button.Activated += (_, _) =>
{
    count++;
    label.Text = $"Count: {count}";
};

// Arrange the two controls in a grid: one column, two rows
var root = new Grid(
    columnWidths: [30],
    rowHeights: [1, 3],
    controls:
    [
      [label],
      [button],   // wrap the button in a rounded border
   ]);

// Esc quits (Ctrl+Q already does by default)
UI.RegisterHotKey(UI.HotKeys.Escape, UI.Stop);

// Focus the button on startup so Enter/Space activates it.
UI.SetFocus(button);

// Start the UI. Mouse/hover need a VtInputSource; keyboard works without one.
var t = UI.Start(root, width: 34, height: 6, input: new VtInputSource(anyMotion: true));
// Wait till the UI stops.
t.Wait(); 
```
If you run it in a terminal you should see:

![](https://i.imgur.com/SPVPmW6.png)


Click **Increment** or focus it and press **Enter/Space**. Press **Esc** or **Ctrl+Q** to quit.

## A two-pane TUI app: a list with a detail view

Most real TUIs are some flavor of *master–detail*: a scrollable list on one side, the selected item's details on the other. Here's a small news reader ([3.newsreader.cs](docs/getting-started/3.newsreader.cs)) - it pulls headlines from an RSS feed into a `ListBox` and shows the selected story's summary in a `MarkdownViewer` beside it. `DockPanel` pins the list to the left and lets the article fill the rest; `ListBox.SelectionChanged` keeps the two in sync.

```csharp
using System.Xml.Linq;

using Jumbee.Console;

// --- Fetch a few headlines from a public RSS feed ---
var items = new List<(string Title, string Summary)>();
try
{
    using var http = new HttpClient();
    var xml = await http.GetStringAsync("https://feeds.bbci.co.uk/news/rss.xml");
    foreach (var item in XDocument.Parse(xml).Descendants("item").Take(20))
        items.Add((item.Element("title")?.Value ?? "(no title)",
                   item.Element("description")?.Value ?? ""));
}
catch (Exception ex)
{
    items.Add(("Failed to fetch feed", ex.Message));
}

// Left: the scrollable headline list. Right: the selected story's summary. ---
// DockPanel pins one control to an edge and fills the rest with the other. Give the docked control an explicit size with .WithWidth(),
// Otherwise a docked control otherwise takes its intrinsic width.
var headlines = 
  new ListBox([.. items.Select(i => i.Title)])
      .WithWidth(40)
      .WithBorder(BorderStyle.Double)
      .WithTitle("Headlines");
var article = 
  new MarkdownViewer(items.Count > 0 ? items[0].Summary : "")
      .WithBorder(BorderStyle.Double)
      .WithTitle("Article");
// Keep the detail pane in sync with the selected row.
headlines.SelectionChanged += (_, _) =>
{
    var i = headlines.SelectedIndex;
    if (i >= 0 && i < items.Count) 
      article.Markdown = items[i].Summary;
};

//
var root = new DockPanel(DockedControlPlacement.Left, headlines, article);

UI.RegisterHotKey(UI.HotKeys.Escape, UI.Stop);
UI.SetFocus(headlines);   // arrow keys drive the list
// Wait for the UI to stop.
UI.Start(root, width: 100, height: 30, input: new VtInputSource(anyMotion: true)).Wait();
```

Arrow keys (or the mouse) move the selection and the article pane updates as you go. This is the same list-plus-detail shape behind a file explorer, a chat app, or an IDE's editor tabs — swap `MarkdownViewer` for a `CodeEditor`, a `TextEditor`, or any other control.

The feed fetch needs outbound network access; offline or behind a proxy, the `catch` block shows a single error row instead of headlines.

**Adding search.** `ListBox` has no built-in filter — you rebuild it. Keep the full list, and on each query clear the box and re-add the matches (its constructor and `AddItems` both take any `IEnumerable<string>`):

```csharp
void Filter(string query)
{
    var matches = items.Where(i => i.Title.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
    headlines.Clear();
    headlines.AddItems(matches.Select(i => i.Title));
    // Re-point SelectionChanged at `matches` while filtered, since row indices no longer map to `items`.
}
```

## The essential concepts

### Startup
- `UI.Start(root, …)` takes over the terminal, spins up the UI thread, and renders frames until `UI.Stop()` is called. 
- It returns a `Task`. `.Wait()` blocks `Main` until the UI exits and the previous terminal output is restored.
- Setting `label.Text` automatically schedules a repaint on the UI thread from any thread.

### 1. The single UI thread

All UI state changes live in a single dedicated UI thread, driven by a `Dispatcher` on a frame loop and anything that modifies the UI needs to be marshalled onto that thread — similar to WinForms/WPF.

The API tries to make this marshalling as transparent as possible:
- Scalar property setters `label.Text`, `gauge.Value`, `globe.RotationAngle`, etc. from any thread is safe — the setter posts the change to the UI thread and requests a repaint.
- Multi-step changes and modifying collections must be marshalled explicitly. For anything that isn't a single scalar assignment like mutating a list or changing several fields together or reading mutable collection state, wrap it in `UI.Invoke` and related threading methods:
  - `UI.Invoke(() => { … })` — run now on the UI thread 
  - `UI.Post(() => { … })` — fire-and-forget onto the UI thread, run on the next frame event.
  - `UI.InvokeAsync(() => { … })` — awaitable.


Since scalar setters marshal automatically, a background thread can access the UI directly for many operations e.g. a live clock([2.clock.cs](docs/getting-started/2.clock.cs)):

```csharp
using Jumbee.Console;

var clock = new Digits(DateTime.Now.ToString("HH:mm:ss")) { DigitStyle = Color.Green1 };

var root = new Grid(rowHeights: [3], columnWidths: [26], controls: [[clock]]);

_ = Task.Run(async () =>
{
    while (true)
    {
        await Task.Delay(1000);
        clock.Text = DateTime.Now.ToString("HH:mm:ss");   // safe from any thread
    }
});

UI.Start(root, width: 30, height: 5).Wait();
```
![](https://i.imgur.com/GvpedMj.png)

### 2. Controls

Every widget derives from `Control`. The library ships a large catalogue, including:

- **Text & input:** `TextLabel`, `TextPanel`, `TextInput`, `TextEditor`, `CodeEditor`, `ChatPrompt`.
- **Buttons & toggles:** `Button`, `Checkbox`, `RadioButton`, `RadioSet`, `Switch`, `ToggleButton`,
  `SelectionList`, `Select`.
- **Lists & trees:** `ListBox`, `Tree`, `Menu`, `MenuBar`, `DataTable`.
- **Readouts & charts:** `Digits`, `Sparkline`, `Gauge`, `BarChart`, `RunChart`, `Plot`, `Badge`, `Log`.
- **Rich content:** `MarkdownViewer`, `Canvas`, `Globe`, `TerminalEmulator`, and the Spectre bridges
  (`SpectreControl<T>`, `SpectreLiveDisplay`, `SpectreTaskProgress`).

Controls auto-size to their content, so you usually let the layout place them and leave `Width`/`Height` alone.
See the full list in the [API reference](docs/api/) and the [control guides](docs/controls/).

### 3. Layouts

Controls are arranged by an `ILayout`. `UI.Start` takes a layout as the root. The common ones:

- `Grid(rowHeights, columnWidths, controls)` — rows × columns of controls (used above).
- `DockPanel` — pin one control to an edge, fill the rest with another.
- `VerticalStackPanel` / `HorizontalStackPanel` — stack controls in a line.
- `SplitPanel` — two panes with a draggable divider.
- `TabPanel` — tabbed pages.
- `Boundary` — pin a single child's size.
- `Overlay` — layer pop-ups, menus, and modal dialogs above the main content (`UI.Overlay` is the ambient one).

Layouts nest: a `Grid` cell can hold another layout wrapped in a control, and composite controls embed a layout
internally.

**Runtime layout: a "zen mode" toggle.** A `SplitPanel`'s orientation is about the *panes*, not the divider:
`Horizontal` puts them side by side (first = left), `Vertical` stacks them (first = top). Because the first pane's
size is just the `SplitPosition` property, you get a full-screen "focus the detail" toggle for free — collapse the
list to a sliver and restore it, with no relayout code:

```csharp
// A left list / right article split, like a news reader.
var split = new SplitPanel(SplitOrientation.Horizontal, headlines, article, splitPosition: 32);

int expanded = split.SplitPosition;                 // remember the open width
split.MinFirst = 1;                                  // allow the thinnest possible sliver (MinFirst floors at 1)

// UI.HotKeys.Char builds a bare-letter/punctuation hotkey that matches the real keystroke.
UI.RegisterHotKey(UI.HotKeys.Char('z'), () =>
    split.SplitPosition = split.SplitPosition > split.MinFirst ? split.MinFirst : expanded);
```

Toggling drives `SplitPosition` between the saved width and `MinFirst`, so `z` collapses the list to a 1-cell
sliver (the article fills the screen) and `z` again restores it. `SplitPosition` can't reach 0 — `MinFirst` is
clamped to at least 1 — so a hair of the first pane always remains.

### 4. Frames (borders, titles, scrollbars)

Any control can be wrapped in a `ControlFrame` — a border, margin, title bar, and scrollbar — with fluent helpers
that return the control so they chain:

```csharp
editor.WithRoundedBorder(Color.Cyan1).WithTitle("Program.cs");
list.WithBorder(BorderStyle.Double).WithTitle("Items");
panel.WithFrame(borderStyle: BorderStyle.Rounded, title: "Details");
```

If a framed control's content is taller than the frame, the frame shows a scrollbar and scrolls it.

### 5. Input and focus

- **Keyboard** works with the default input source.
- **Mouse and hover** need a VT terminal and a `VtInputSource`; pass `anyMotion: true` for hover highlighting:
  `UI.Start(root, input: new VtInputSource(anyMotion: true))`.
- **Focus** decides where keystrokes go. Clicking a control focuses it (click-to-focus); set it in code with
  `UI.SetFocus(control)` and read `control.IsFocused`.
- **Built-in focus navigation:** `Ctrl+N` / `Ctrl+P` cycle focus within the current layout region, and
  `Ctrl+←/→/↑/↓` move between regions. `Tab` is intentionally *not* bound by default (apps often want it) — bind
  it yourself if you want tab traversal:
  ```csharp
  UI.RegisterHotKey(UI.HotKeys.Tab, UI.FocusNext);
  UI.RegisterHotKey(UI.HotKeys.ShiftTab, UI.FocusPrevious);
  ```

### 6. Global hotkeys

Register app-wide keys with `UI.RegisterHotKey(key, action)`; they run before the focused control sees the key.
`Ctrl+Q` (quit) and `F1` (help) are registered by default. Use the `UI.HotKeys` constants/helpers so the key
matches what the input decoder produces:

```csharp
UI.RegisterHotKey(UI.HotKeys.Escape, UI.Stop);
UI.RegisterHotKey(UI.HotKeys.Ctrl(ConsoleKey.S), Save);
```

### 7. Styling and theming

- **Per control:** set style properties directly. They take a `Style` (foreground + background + decoration), and
  a plain `Color` converts to one. Colours and styles expose the full named palette (`Color.Cyan1`, `Style.Bold`,
  `Style.Grey50`, …). Text content that accepts markup understands Spectre markup (`"[green]OK[/]"`).
- **App-wide:** set a theme once and every themed control follows it. `UI.StyleTheme` / `UI.GlyphTheme` hold the
  active theme; `UI.SetTheme(...)` swaps both at runtime and re-themes live controls. Explicit per-control style
  values you set survive a theme switch; everything you leave unset follows the theme.

### 8. App lifecycle

```csharp
UI.RegisterHotKey(UI.HotKeys.Escape, UI.Stop);   // wire a quit key
UI.Start(root, width: 100, height: 30).Wait();    // blocks until UI.Stop()
```

`UI.Start` renders on an alternate screen buffer by default and the shell's scrollback is restored on exit). Use `useAlternateScreen: false` to render inline. `UI.Stop()` ends the loop and
restores the terminal.

### 9. Building your own composite control
When several controls always travel together (an editor + its gutter, a labelled input), package them as a
`CompositeControl`: build the children, arrange them in any layout, wire their events, and call `SetContent`. The
result is a `Control`, so it drops into any layout cell and can be framed. See [Composite Controls](docs/controls/Composite%20Controls.md).


For changes that aren't a single property assignment (updating a list, mutating a wrapped Spectre widget), wrap
them in `UI.Invoke(() => { … })`. If you're authoring a control that needs a periodic tick, the protected `Control.Feed(tick, interval)` helper runs a repeating timer that posts to the UI thread and cancels on dispose.

## Testing without a terminal

`Jumbee.Console.Snapshot` renders a layout offscreen so you can assert on the output in a unit test or a CI smoke
check — no real terminal required:

```csharp
using Jumbee.Console.Snapshot;

var text = ConsoleSnapshot.ToText(root, width: 80, height: 24);
Assert.Contains("Count: 0", text);
```

`ConsoleSnapshot` can also save a PNG (`SavePng`) and render *after* feeding keystrokes — including global hotkeys:

```csharp
// Feed keys to a focused control, then re-render. Build keys with ConsoleSnapshot.Key(...).
var afterNav = ConsoleSnapshot.ToTextAfter(list, 80, 24, [ConsoleSnapshot.Key(ConsoleKey.DownArrow)]);

// Fire a GLOBAL hotkey (one registered with UI.RegisterHotKey) — pass routeGlobal: true.
var afterHotkey = ConsoleSnapshot.ToTextAfter(list, 80, 24, [ConsoleSnapshot.Key(ConsoleKey.R)], routeGlobal: true);

// For a punctuation hotkey (e.g. '/'), register AND test with UI.HotKeys.Char('/') — it produces exactly what
// the decoder emits, which ConsoleSnapshot.Key(ConsoleKey.Oem2) does not (its char is '\0'), so this matches.
var afterSearch = ConsoleSnapshot.ToTextAfter(list, 80, 24, [UI.HotKeys.Char('/')], routeGlobal: true);
```

Two things to know when asserting: text snapshots are **glyphs only** — colour and decoration aren't captured, so check colour with `SavePng`/`ToImage` (or render a visible marker and assert on that); and `ToTextAfter` sends the keys to the single control you pass, so to prove an effect that spans panes, target the control that actually changes.

### Testing a modal dialog

A modal doesn't live in your layout — it attaches to an [`Overlay`](docs/api/Jumbee.Console.Overlay.md). At runtime
`UI.Start` creates one for you, but a snapshot test has no UI loop, so build the overlay yourself and **snapshot the
overlay rather than the root**. That last part is the whole trick: the dialog is a layer *on top of* your root
layout, so a snapshot of the root alone renders a frame with no dialog in it.

```csharp
using Jumbee.Console;
using Jumbee.Console.Snapshot;

var root    = new Grid([1], [40], [[new TextInput("behind the dialog")]]);
var overlay = new Overlay(root);
UI.Overlay  = overlay;                       // so Dialog.Show() can find it
ConsoleSnapshot.Render(overlay, 60, 20);     // lay the root out once

var confirmed = false;
var dialog = Dialog.Confirm("Kill process", "Kill 'node'?", yes => confirmed = yes);
dialog.Show();                               // or dialog.Show(overlay), and skip setting UI.Overlay

// Snapshot the OVERLAY — snapshotting `root` here would show no dialog at all.
var text = ConsoleSnapshot.ToText(overlay, 60, 20);
Assert.Contains("Kill 'node'?", text);

// Drive it with keys the same way, still through the overlay.
var after = ConsoleSnapshot.ToText(
    ConsoleSnapshot.RenderAfter(overlay, 60, 20, [ConsoleSnapshot.Key(ConsoleKey.Enter)]));
Assert.True(confirmed);
Assert.DoesNotContain("Kill 'node'?", after);   // dismissed
```

`Dialog.Show()` throws `InvalidOperationException: "No ambient UI.Overlay is available"` if you skip the
`UI.Overlay` assignment *and* don't use the `Show(overlay)` overload — that exception is the signal you've hit this
case. The same pattern works for anything else that renders into the overlay layer, such as a `ContextMenu`.

## Where to go next

- Case study 1 : [building a real TUI](examples/Jumbee.Console.NewsReaderDemo/README.md) — a longer worked
  example that picks up where the two-pane app above leaves off. It walks through porting a terminal RSS reader
  (feed tree, article list, markdown reader, vim-style keys) from the ground up: the threading model, splitting
  domain logic from the UI, off-thread feed fetching, custom `IRenderable`/`CompositeControl` controls, and
  headless snapshot testing. The full runnable source is in
  [`examples/Jumbee.Console.NewsReaderDemo`](examples/Jumbee.Console.NewsReaderDemo).
- Case study 2: [porting the scope-TUI oscillocope app](examples/Jumbee.Console.AudioScopeDemo-basic/README.md) — another longer worked
  example that demonstrates builing a graphics-heavy oscilloscope app from realtime audio data. Source is in
  [`examples/Jumbee.Console.AudioScopeDemo-basic`](examples/Jumbee.Console.AudioScopeDemo/).
- [Troubleshooting](TROUBLESHOOTING.md) — the Spectre.Console collision, submodules/CS1704, non-TTY rendering, and mouse input.
- [API reference](docs/api/) — every public type, grouped by namespace, with summaries.
- [Control guides](docs/controls/) — task-focused walkthroughs (selection controls, display widgets, links,
  composite controls).
- [Internals](docs/internal/) — architecture notes: the rendering pipeline, input model, multithreading, and
  theming.
- Examples — `examples/Jumbee.Console.Examples` is a browsable gallery (each example shown next to its source);
  `examples/Jumbee.Console.IdeDemo`, `examples/Jumbee.Console.AgentHarnessDemo`, and
  `examples/Jumbee.Console.NewsReaderDemo` are larger, full apps.
