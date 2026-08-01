# Terminal

`TerminalEmulator` runs a child process in a pseudo-console and paints its output as a control — a real shell
inside your app.

It's in the core `Jumbee.Console` package — no extra reference needed.

## How it works

Three pieces: a pseudo-console (`ConPty` on Windows, `UnixPty` elsewhere, behind `IPty`) hosts the child process;
VtNetCore parses the ANSI stream the child writes; and the emulated screen is painted into the control's cells.
Input routed to the focused control is translated back into terminal bytes and sent to the process.

Both platforms are supported, including WSL from a Windows host.

## Using it

```csharp
var term = new TerminalEmulator("pwsh", workingDirectory);
term.WithFrame(title: "Terminal");

term.TitleChanged += (_, title) => term.Frame!.Title = title;   // follow the child's OSC title
term.Exited       += (_, _) => status.Text = "shell exited";
```

The constructor is `TerminalEmulator(string? command = null, string? workingDirectory = null)`. The process starts
when the control initializes; `StartProcess()` and `StopProcess()` control it explicitly and `IsRunning` reports
the state, so you can offer a restart without rebuilding the control.

`WindowTitle` carries whatever the child set via OSC, with `TitleChanged` when it changes — worth forwarding to the
frame title as above. `Exited` fires when the process ends.

`SendText(string)` writes text to the child as if typed; `Feed(byte[])` writes raw bytes for anything that isn't
plain text. `DefaultBackground` sets the colour behind cells the child hasn't painted.

## Input and focus

While focused, the emulator takes essentially every key and forwards it — that's the point, and it means your
app-level keybindings won't fire while the terminal has focus. Give the user an unambiguous way out: a global
hotkey registered with `UI.RegisterHotKey` still works, since global hotkeys are dispatched before control input.

```csharp
// Works even while the terminal is focused — global hotkeys are dispatched before control input.
UI.RegisterHotKey(UI.HotKeys.Ctrl(ConsoleKey.F6), () => tree.Focus());
```

Mouse press, release and wheel are forwarded to the child, so mouse-aware programs inside the terminal behave
normally. Paste is forwarded through `OnPaste`.

## Lifecycle

Dispose the control to tear the child process down — `TerminalEmulator` owns the pty and the reader. In practice
that means disposing it when you close the pane that holds it, not just removing it from the layout, or you leave
an orphan process running.

It fills its framing viewport rather than sizing to content, so it takes whatever the layout gives it and the
child is told that size.

## See also

- [Layouts](Layouts.md) — giving the terminal a region, typically a `SplitPanel` pane.
- [Control Model](Control%20Model.md) — focus, and why a focused control sees keys first.
- The IDE demo (`examples/Jumbee.Console.IdeDemo`) — a tree, a tabbed editor and a terminal in one app.
