# Jumbee.Console IDE demo

A minimal **VS Code–style IDE**, in the terminal, built entirely from `Jumbee.Console` controls. It’s a standalone
app (not part of the examples browser) that shows the library’s heavier controls working together:

- a **file explorer** (`Tree`) with per-node file/folder glyphs,
- a **tabbed C# editor** (`MultiTabCodeEditor`) with syntax highlighting, dirty tracking, and save,
- an embedded **terminal** (`TerminalEmulator`) running a real shell in the project directory, so you can
  `dotnet build` / `dotnet run` the code you’re editing,
- a **menu bar**, **status bar** (file · Ln/Col · dirty), and **key-hint footer**.

```
 File  Build  View
╭ Explorer ─────╮╭ Editor ───────────────────────────────╮
│ ▼ SampleProj  ││  ● Program.cs                          │
│  ◆ Calculator ││  namespace SampleApp;                  │
│  ◆ Greeter.cs ││  ...                                   │
│  ◆ Program.cs │╰───────────────────────────────────────╯
│  ◆ SampleApp  │╭ Terminal ─────────────────────────────╮
│               ││  > dotnet build                        │
│               ││  Build succeeded.                      │
╰───────────────╯╰───────────────────────────────────────╯
  Program.cs ●    Ln 3  Col 5   C:\...\SampleProject
^E Explorer  ^L Editor  ^T Terminal  ^S Save  ^B Build  ^Q Quit
```

## Run it

```bash
# Opens the bundled sample C# project:
dotnet run --project examples/Jumbee.Console.IdeDemo -c Release

# Or open any directory:
dotnet run --project examples/Jumbee.Console.IdeDemo -c Release -- /path/to/a/csharp/project
```

In Docker, as a native binary:

```bash
docker run --rm -it allisterb/jumbee-console ide
```

The image carries no .NET SDK, so the first time you choose **Build**, **Run** or **Clean** the IDE offers to install
one (`apt-get install dotnet-sdk-10.0`, about 150 MB), running the install in its own terminal pane. See
[docker.md](../../docker.md#building-the-ide-demos-sample-project).

Headless: `-- --verify` renders the layout offscreen and exits (a CI smoke check); `-- --dump` prints that render.

## Keys

| Key | Action |
|-----|--------|
| Click / Enter a `.cs` file | Open it in a new tab |
| `Ctrl+S` | Save the active file |
| `Ctrl+B` | `dotnet build` in the terminal (also on the **Build** menu, with Run / Clean) |
| `Ctrl+E` / `Ctrl+L` / `Ctrl+T` | Focus explorer / editor / terminal |
| `Ctrl+Q` | Quit |
| `Alt+←` / `Alt+→` | Switch editor tabs |

## Scope (v1)

Deliberately limited to make the composition legible: **C# / `.csproj` files only**, a **real shell** in the
terminal (no custom build-output parsing — the .NET SDK does the work). The explorer skips `bin`/`obj`/`.git`.

## Notes

- **Sample project** — with no path argument, the bundled `SampleProject/` is copied to a writable temp directory
  (`%TEMP%/JumbeeIdeDemo`) and opened there, so edits and builds don’t touch the app’s own output.
- **Terminal working directory** — the terminal is launched with `workingDirectory: <project>` (a `TerminalEmulator`
  feature added for this demo), so `dotnet` commands resolve against the open project.
- **Build output** — the demo sets `MSBUILDTERMINALLOGGER=off` so `dotnet build` produces plain, linearly-scrolling
  output that reads well in a short terminal pane. Remove that line in `Program.cs` to see MSBuild’s Terminal Logger
  layout instead (the emulator renders it correctly).
