# About
This directory contains (mostly) AI generated documentation about Jumbee.Console internals.

## Contents
- [ConsoleGUI and Spectre.Console Control Integration](ConsoleGUI%20and%20Spectre.Console%20Control%20Integration.md)
- [Rendering Model](Rendering%20Model.md) — why compositing pulls cells through the control tree (vs a flat push-buffer like ratatui): the composability/DX rationale, the per-cell cost, and the model-preserving perf levers (damage tracking).
- [ConsoleGUI Control Rendering](ConsoleGUI%20Control%20Rendering.md)
- [Spectre.Console Control Rendering](Spectre.Console%20Control%20Rendering.md)
- [ANSI and Legacy Terminal Rendering](Ansi%20and%20Legacy%20Terminal%20Rendering.md) — how the renderer drives ANSI vs non-ANSI terminals, and the hardware vs software cursor.
- [Input Routing](Input.md) — how keyboard and mouse events are routed to layouts, controls, composite controls, and nested layouts (the `FocusedControl` chain vs. spatial cell hit-testing).
- [Mouse Input and Overlays](Mouse%20Input%20and%20Overlays.md) — per-control mouse events (hover/click/wheel), the floating overlay/popup layer, modal routing, and the `Select` widget.
- [Theming](Theming.md) — the glyph/style theme split, styling primitives, how controls capture the theme, live theme switching (`UI.SetTheme`), and override-aware re-application.
- [Multithreading](Multithreading.md)
- [Snapshot Testing](Snapshot%20Testing.md) — the headless `ConsoleSnapshot` text/PNG renderer, and why to snapshot widgets under multiple fonts (glyph coverage varies by terminal font; block-element coverage table).
- [Handoff](handoff.md) — current state, what to pick up next (ranked), and the operational notes for running the eval loop.
- [Eval findings](eval-findings.md) — the running backlog of API/doc gaps surfaced by the jc-curious port evals, with evidence and disposition.
