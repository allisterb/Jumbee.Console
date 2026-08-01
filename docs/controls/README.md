# Controls

Around sixty controls and eight layouts, which is more than anyone wants to read through. This page exists to get
you to the right one quickly: skim the decision table, then read the guide for that category.

New here? [**Writing applications**](Writing%20Applications.md) explains the retained-mode model, what the framework
owns versus what you own, and how to choose a base class when you write your own control.

> Already running, and something behaves oddly? [**What happens when…**](What%20Happens%20When.md) answers the
> questions that cost people an afternoon — why the app doesn't fill the terminal, why an update never appears, why
> a control silently stopped drawing.

## Quick decision guide

| I want to… | Use |
|---|---|
| **Structure** | |
| Build the app shell — bars, sidebars, panes | `DockPanel` and `SplitPanel`. **Not `Grid`** — it never grows past the size you declare |
| Arrange a fixed region — a form, a dialog interior, a dashboard tile | `Grid` |
| Lay out a row or column of controls | `HorizontalStackPanel` / `VerticalStackPanel` (content-sized along the stack axis) |
| Give a control a border, title or scrollbar | `ControlFrame`, usually via `.WithFrame()` / `.WithRoundedBorder()` |
| Show pop-ups, menus or modals | `Overlay` — `UI.Start` sets `UI.Overlay` for you |
| Swap between whole pages | `TabPanel` |
| **Text and input** | |
| Take one line of text | `TextInput` (caret, selection, placeholder, password masking) |
| Take a line at a REPL-style prompt | `TextPrompt` |
| Show one line of text | `TextLabel` |
| Show a block of static formatted text | `TextPanel` (Spectre markup, multi-line) |
| Let the user edit a document | `TextEditor`, or `CodeEditor` for syntax highlighting plus a line-number gutter |
| Offer several open documents at once | `MultiTabCodeEditor` |
| Build an agent/chat input line | `ChatPrompt` (prompt glyph that becomes a busy spinner, plus a `TextInput`) |
| Add type-ahead to an input | `Autocomplete` |
| **Lists and data** | |
| Show a flat list to pick from | `ListBox` (scrolls, takes focus) or `Select` (collapsed drop-down) |
| Show tabular data | `DataTable` — row selection, fixed header, drops columns when narrow. You sort the data yourself; no inline cell editing |
| Show hierarchy | `Tree` |
| Show a scrolling stream of events | `Log`. It's viewport-virtualized (O(viewport), not O(entries)) and tails — don't reach for `ListBox` |
| **Selection** | |
| Toggle one thing | `Checkbox`, or `Switch` for an on/off setting |
| Pick one of N | `RadioSet` (all options visible) or `Select` (collapsed) |
| Pick several of N | `SelectionList` |
| **Charts and drawing** | |
| Plot numeric series with axes | `Plot` — line, scatter, bar, histogram, candlestick, heatmap and more |
| Stream live series with a legend and stats | `RunChart` (composition over `Plot`, fixed X window) |
| Fit a trend into one row | `Sparkline` (one cell per value, no axes) |
| Draw arbitrary graphics | `Canvas` plus the `Drawing` shapes; `WorldMap` for geography |
| Compare labelled categories | `BarChart` |
| Show a single proportion | `Gauge` (a meter — capacity, percent complete) |
| Show task progress | `ProgressBar` (one task, you drive it) or `SpectreTaskProgress` (multi-task Spectre widget) |
| **Status and chrome** | |
| Show a short status pill | `Badge` |
| Show activity with no known duration | `Spinner`, or `ProgressBar.IsIndeterminate` |
| Show a key-hints bar along the bottom | `Footer` |
| Show large numeric readouts | `Digits` |
| Show frame timings and allocation while developing | `PerfHud` |
| **Navigation and commands** | |
| Add a menu bar | `MenuBar` |
| Add a right-click menu | `ContextMenu` (draws its own nested submenu chain) |
| Ask a question, confirm an action | `Dialog.Confirm` / `Dialog.Message`, or a custom `Dialog` |
| Provide F1 help | `HelpInfo` on your controls; `HelpControl` renders it |
| Make something clickable that jumps elsewhere | `Link` |
| **Documents and embedding** | |
| Render Markdown | `MarkdownViewer` (core), or `MarkdownExtendedViewer` for embedded Mermaid |
| Render AsciiDoc or Mermaid | `AsciiDocViewer`, `MermaidViewer` — needs the `Jumbee.Console.Documents` package |
| Let the user edit one of those live | the `Interactive*Editor` controls — source pane plus rendered preview |
| Embed a real shell or child process | `TerminalEmulator` |
| **Spectre.Console interop** | |
| Use a Spectre `IRenderable` you already have | `SpectreControl<T>` |
| Use Spectre's `LiveDisplay` or `Progress` | `SpectreLiveDisplay`, `SpectreTaskProgress` |
| Write a new control using Spectre styling | subclass `RenderableControl` |

## Categories

| Category | Controls | What it covers |
|---|---|---|
| [Control Model](Control%20Model.md) | `Control`, `ControlFrame`, `ILayout`, `CompositeControl` | What nests inside what, and who owns sizing, focus and input |
| [Layouts](Layouts.md) | `DockPanel`, `SplitPanel`, `Grid`, stack panels, `Boundary`, `TabPanel`, `Overlay` | Arranging controls — and **which layouts fill the terminal and which don't** |
| [Text and Input](Text%20and%20Input.md) | `TextInput`, `TextLabel`, `TextEditor`, `CodeEditor`, `MultiTabCodeEditor`, `ChatPrompt`, `TextPrompt`, `Autocomplete` | Entering and editing text, from one line to a tabbed editor |
| [Lists and Data](Lists%20and%20Data.md) | `ListBox`, `DataTable`, `Tree`, `Log`, `TextPanel` | Presenting rows, hierarchy and streams — and picking between them |
| [Selection Controls](Selection%20Controls.md) | `Checkbox`, `RadioButton`, `Switch`, `RadioSet`, `SelectionList`, `Select` | Toggles and the single-/multi-select list controls |
| [Charts](Charts.md) | `Plot`, `Canvas`, `BarChart`, `RunChart`, `Globe`, `Drawing` shapes | Plotting data and drawing graphics |
| [Display Widgets](Display%20Widgets.md) | `Sparkline`, `Digits`, `Gauge`, `ProgressBar`, `Spinner`, `Badge`, `Footer`, `Log` | Small self-contained readouts and status indicators |
| [Navigation](Navigation.md) | `MenuBar`, `ContextMenu`, `Dialog`, `HelpControl`, `TabPanel` | Menus, modals, help, and moving around the app |
| [Links](Links.md) | `Link` | Clickable links, and wiring app-level keys |
| [Documents](Documents.md) | `MarkdownViewer`, `AsciiDocViewer`, `MermaidViewer`, `Interactive*Editor` | Rendering and editing Markdown, AsciiDoc and Mermaid |
| [Terminal](Terminal.md) | `TerminalEmulator`, `ConPty`, `UnixPty` | Running a child process in an embedded pseudo-console |
| [Spectre Interop](Spectre%20Interop.md) | `SpectreControl<T>`, `SpectreLiveDisplay`, `SpectreTaskProgress`, `RenderableControl` | Bringing existing Spectre.Console widgets in, and building on Spectre rendering |
| [Composite Controls](Composite%20Controls.md) | `CompositeControl`, `RenderableControl`, `AnimatedControl` | Building a control out of other controls |

## Concepts

These cut across every category. Read them when the symptom is behavioural rather than "which control".

| Concept | Read it when |
|---|---|
| [Writing Applications](Writing%20Applications.md) | You're starting out, or deciding how to build a control of your own |
| [What happens when…](What%20Happens%20When.md) | Something already behaves unexpectedly and you want the answer, not a tour |
| [Live Data](Live%20Data.md) | Data arrives on a background thread and you need it on screen safely |
| [Theming](../internal/Theming.md) | You want app-wide colours and glyphs, or a control ignores your theme |
| [Input](../internal/Input.md) | Keys land on the wrong control, or focus doesn't move as expected |
| [Snapshot Testing](../internal/Snapshot%20Testing.md) | You want to assert what the UI actually renders, headlessly |

## See also

- [Getting Started](../../GETTING-STARTED.md) — install, first app, essential concepts.
- [API reference](../api) — generated per-type member documentation.
- The examples browser (`examples/Jumbee.Console.Examples`) — every control below, running, with its source beside it.
