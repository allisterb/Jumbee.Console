# Jumbee.Console
![](https://i.imgur.com/oCTmJul.gif)

## About
[Jumbee.Console](https://github.com/allisterb/Jumbee.Console) is a .NET library for advanced TUIs that focuses on performance and usability. Inspired by libs like [ratatui](https://ratatui.rs/) and [Textual](https://textual.textualize.io/), it tries to provide a high-performance retained-mode library that is easy-to-use with idiomatic .NET GUI and Task patterns, while flexible enough to create different types of TUI applications from news readers to animated dashboards to IDEs to agent harnesses to graphics apps.


## Features

* 100% managed AOT-compatible code.
* Retained-mode GUI framework with an API designed to be easy to use and extend.
* Sub-ms frame rendering times and minimal CPU consumption even with complex displays like multi-tab document editing and syntax highlighting.
* Uses modern terminal features: ANSI/VT control sequences, 24-bit colour, SGR-encoded mouse with motion tracking, bracketed paste, focus reporting, and the alternate screen buffer.
* Also support legacy non-ANSI terminal emulators like the classic Windows console.
* Uses Spectre.Console-compatible markup, styles, text rendering, and widgets in a retained-mode rendering pipeline.
* Supports both fixed-width layouts like `Grid` and flexible, resizable layouts like `DockPanel`, `HorizontalStack`, `VerticalStack`, resizable `SplitPanel`.
* Large set of common GUI controls: menus, buttons, trees, text inputs with autocomplete, modal dialog windows, etc...., supports easy composition of controls.    
* Control frames support adornments like titles, borders, margins, and scrollbars.
* Cross-platform 100% managed code terminal-emulator.
* Multi-tab editor that supports C#, JavaScript, C++, Markdown + a dozen other languages.
* Split-pane interactive editors with preview for Markdown, AsciiDoc, Mermaid documents, Mermaid embedded in Markdown.
* Visualization and graphics: Many different types of plots and graphs like candlestick & heatmap plots & bar/run charts, world maps, sub-cell drawing canvas and shapes, 3D texture rendering, and support for animation.
* Flexible themes that support styling both colors and glyphs independently.
* Headless snapshot testing: render any control or layout to text or PNG without a real terminal.

## Getting Started

`dotnet add package Jumbee.Console`

A first app — a label and a button that increments a counter:

```csharp
using Jumbee.Console;

using static Jumbee.Console.Color;   // import the static color names

var count = 0;

var label = new TextLabel(TextLabelOrientation.Horizontal, "Count: 0", Cyan1);
var button = new Button("Increment");

// Change the label text when the button is clicked or pressed
button.Activated += (_, _) => label.Text = $"Count: {++count}";

// One column, two rows: the label above the button.
var root = new Grid(
    columnWidths: [30],
    rowHeights: [1, 3],
    controls:
    [
        [label],
        [button],
    ]);

UI.RegisterHotKey(UI.HotKeys.Escape, UI.Stop);   // Esc quits (Ctrl+Q also quits by default)
UI.SetFocus(button);                             // focus it so Enter/Space activates

// Start the UI. Mouse/hover need a VtInputSource or omit for keyboard only.
// Returns a task.
var t = UI.Start(root, width: 34, height: 6, input: new VtInputSource(anyMotion: true));

// Wait for the UI to stop.
t.Wait();
```

See [GETTING-STARTED.md](https://github.com/allisterb/Jumbee.Console/blob/master/GETTING-STARTED.md) and the [documentation](https://github.com/allisterb/Jumbee.Console/tree/master/docs). To find the right control for a job, start with the [control guides](https://github.com/allisterb/Jumbee.Console/blob/master/docs/controls/README.md) — a decision table over the whole library, then a guide per category.

## For LLMs and coding agents

[**llms.txt**](https://raw.githubusercontent.com/allisterb/Jumbee.Console/master/llms.txt) ([what this is](https://llmstxt.org)) is a curated index of every doc and API page in this project, as absolute links to raw Markdown. Fetch it first to find the page you need, then fetch that page directly.