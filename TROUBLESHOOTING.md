# Troubleshooting

## `CS1704: An assembly with the same simple name 'Spectre.Console' has already been imported`

Jumbee.Console ships a private fork of Spectre.Console bundled inside the package. If your project *also* references the upstream `Spectre.Console` NuGet package, two assemblies claim the same identity and the compiler stops.

Remove the `Spectre.Console` package reference from your project. Jumbee.Console re-exposes the Spectre types it needs (markup, styles, `IRenderable`, `Segment`, and the widget bridges), so the one package is enough.

If you hit CS1704 while building the **repository** rather than your own app, the cause is usually the opposite: missing submodules. See the next entry.

## `CS0104: 'Tree' is an ambiguous reference between 'Jumbee.Console.Tree' and 'Spectre.Console.Tree'`

Jumbee.Console re-exposes the bundled Spectre.Console types, so once you build custom content (`IRenderable` rows, `Markup`, `Segment`) you'll have both namespaces in scope — and a few short type names collide (`Tree` is the common one). Qualify the one you mean, e.g. `Jumbee.Console.Tree`, or add a `using Tree = Jumbee.Console.Tree;` alias. (In a file already inside the `Jumbee.Console` namespace, the Jumbee type wins automatically.)

**Where the document viewers live:** `MarkdownViewer` is part of the **core `Jumbee.Console`** package (namespace `Jumbee.Console`) — only the `MarkdownExtendedViewer`, `AsciiDocViewer`, and `MermaidViewer` are in the separate **`Jumbee.Console.Documents`** add-on.

## Build errors after cloning the repo (empty `ext/` folders)

The repository builds against vendored forks under `ext/`, wired in as git submodules. A plain clone leaves those directories empty and the build fails, often with a CS1704 or a "project not found".

Clone with submodules:

    git clone --recurse-submodules https://github.com/allisterb/Jumbee.Console

If you already cloned without them:

    git submodule update --init --recursive

## Nothing renders, or the output is garbled

The apps are full-screen TUIs and need a real terminal.

- Run them in a VT-capable terminal (Windows Terminal, or most modern Linux/macOS terminals). The legacy Windows console works, with reduced features.
- In Docker, pass `-it` so the container gets a TTY: `docker run --rm -it ...`. Without it the UI has nowhere to draw.
- Piped or redirected output (`app > log.txt`, most CI) is detected as non-interactive; the app falls back to keyboard-only, inline rendering rather than taking over the screen.

## Mouse, hover, or paste don't work

Mouse clicks, hover highlighting, bracketed paste, and focus reporting need VT input. Pass a `VtInputSource` to `UI.Start` and use a VT-capable terminal:

    UI.Start(root, input: new VtInputSource(anyMotion: true));

`anyMotion: true` turns on hover highlighting. Keyboard input works without any of this.

## The terminal is broken after force-killing an app (stray characters, stuck full-screen)

If a Jumbee.Console app is **hard-killed** — Task Manager "End task", `kill -9`, or stopping it from a debugger — the shell may afterwards echo stray sequences as you move the mouse (mouse-tracking reports), or stay stuck showing the app's last screen. A hard kill runs no cleanup code at all — no `finally`, no exit handler, nothing — so the app never got to turn those modes off. This affects every full-screen terminal app, not just those based on Jumbee.Console; only a normal quit (Ctrl+Q) or a catchable exit restores the terminal.

To recover:

- **Easiest:** launch any Jumbee app again and quit it with **Ctrl+Q**. Its clean shutdown restores the terminal, and it also resets a clean baseline at startup — so simply relaunching clears the inherited state.
- **Linux/macOS:** You can also run the `reset` command to reset the terminal.


## Colours look wrong or washed out

Colour depth is detected from the terminal. When a terminal only reports 16 colours, the 24-bit palette is downsampled. Use a terminal that advertises truecolor (and a `TERM` such as `xterm-256color`) for the full palette.
