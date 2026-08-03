# Jumbee.Console
[![NuGet Version](https://img.shields.io/nuget/v/Jumbee.Console?style=plastic)](https://www.nuget.org/packages/Jumbee.Console)

![](https://i.imgur.com/zM24PDY.gif)

## About
Jumbee.Console is a .NET library for advanced TUIs that focuses on performance and usability. Inspired by libs like [ratatui](https://ratatui.rs/) and [Textual](https://textual.textualize.io/), it tries to provide a high-performance retained-mode library that is easy-to-use with idiomatic .NET GUI and Task patterns, while flexible enough to create different types of TUI applications from news readers to animated dashboards to IDEs to agent harnesses to graphics apps.


## Features
* 100% managed AOT-compatible code.
* Retained-mode GUI framework with a modern API designed to be easy to use and extend.
* Sub-ms frame rendering times and minimal CPU consumption even with complex displays like multi-tab document editing and syntax highlighting.
* Uses modern terminal features: ANSI/VT control sequences, 24-bit colour, SGR-encoded mouse with motion tracking, bracketed paste, focus reporting, and the alternate screen buffer.
* Also supports the classic Windows non-ANSI console.
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

## Requirements
* .NET 10.0 SDK

## Running the examples from Docker
You can run the example apps from the [Docker image](https://hub.docker.com/r/allisterb/jumbee-console) without needing .NET 10 or anything installed.

The examples browser:

```sh
docker run --rm -it --pull=always allisterb/jumbee-console:latest
```

The agent harness example:

```sh
docker run --rm -it --pull=always allisterb/jumbee-console:latest agent-harness
```

The audio scope example, from the AOT image:

```sh
docker run --rm -it --pull=always allisterb/jumbee-console-aot:latest audio-scope
```


## Installation 
Install the package from NuGet:

`dotnet add package Jumbee.Console`

Note that Jumbee.Console uses its own compatible version of the Spectre.Console library - you shouldn't add both to your projects.

## Building
* Clone the repo with submodules:
  `git clone --recurse-submodules https://github.com/allisterb/Jumbee.Console`
  
* Switch to the repo dir and run the build script: `.\build.cmd` or `./build`

See [CONTRIBUTING.md](CONTRIBUTING.md) for more details on building and testing.


## Getting Started

A first app — a label and a button that increments a counter:

```csharp
using Jumbee.Console;

using static Jumbee.Console.Color;   // import the static colour names

var count = 0;
var label = new TextLabel(TextLabelOrientation.Horizontal, $"Count: {count}", Cyan1);
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
UI.SetFocus(button);                             // focus the button so Enter/Space activates

// Start the UI with a width/height. Mouse/hover need a VtInputSource.
// Returns a Task.
var t = UI.Start(root, width: 34, height: 6, input: new VtInputSource(anyMotion: true));

// Wait until the UI stops.
t.Wait();
```

For more examples see [GETTING-STARTED.md](https://github.com/allisterb/Jumbee.Console/blob/master/GETTING-STARTED.md), the [gallery](https://github.com/allisterb/Jumbee.Console/tree/master/gallery), the [documentation](https://github.com/allisterb/Jumbee.Console/tree/master/docs)
and a longer example [of building a TUI newsreader](examples/Jumbee.Console.NewsReaderDemo/README.md).
