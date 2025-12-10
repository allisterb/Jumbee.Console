# Jumbie.Console

**Jumbie.Console** is a .NET library that bridges the layout capabilities of [ConsoleGUI](https://github.com/TomaszRewak/C-sharp-console-gui-framework) with the rich styling and widget set of [Spectre.Console](https://github.com/spectreconsole/spectre.console). It allows you to embed `Spectre.Console` widgets (like Tables, Trees, Charts) directly into `ConsoleGUI`'s windowing and layout system.

## Features

- **SpectreWidgetControl**: A `ConsoleGUI` control that hosts any `Spectre.Console` renderable.
- **Full Color Support**: Translates `Spectre.Console` TrueColor styles to `ConsoleGUI`'s 24-bit color system.
- **Layout Integration**: Use `Spectre` widgets inside `DockPanel`, `Grid`, `Window`, and other `ConsoleGUI` containers.
- **Virtual Console**: Implements `IAnsiConsole` to redirect Spectre output to an in-memory buffer for rendering.

## Usage

### 1. Setup ConsoleGUI
Initialize `ConsoleGUI` as usual.

```csharp
ConsoleManager.Setup();
ConsoleManager.Resize(new Size(80, 25));
```

### 2. Create a Spectre.Console Widget
Create any standard Spectre widget (e.g., a Table).

```csharp
var table = new Table();
table.AddColumn("Item");
table.AddColumn("Value");
table.AddRow("CPU", "90%");
table.AddRow("RAM", "40%");
table.Border(TableBorder.Rounded);
```

### 3. Wrap in SpectreWidgetControl
Wrap the widget in `SpectreWidgetControl` to make it compatible with `ConsoleGUI`.

```csharp
var spectreControl = new SpectreWidgetControl(table);
```

### 4. Add to Layout
Add the control to any `ConsoleGUI` layout.

```csharp
var layout = new DockPanel
{
    Placement = DockPanel.DockedControlPlacement.Top,
    DockedControl = new TextBlock { Text = "System Status" },
    FillingControl = spectreControl
};

ConsoleManager.Content = layout;
```

## How it Works

`Jumbie.Console` creates a virtual `IAnsiConsole` implementation (`ConsoleGuiAnsiConsole`) that captures `Spectre`'s rendering instructions. Instead of writing to the terminal stream, it writes to an internal `Cell[,]` buffer. The `SpectreWidgetControl` then exposes this buffer as a `ConsoleGUI` control, allowing it to be composed with other controls.

## Limitations

- **Interactive Spectre Widgets**: Interactive widgets (like `Prompt`, `SelectionPrompt`) that require their own input loop are not supported directly inside the control, as `ConsoleGUI` manages the main loop. Only static renderables work out-of-the-box.
- **Animations**: To animate a Spectre widget (e.g., a Spinner or Progress bar), you must manually update the widget instance and call `Redraw()` or update the `Content` property of `SpectreWidgetControl` within your application loop.
