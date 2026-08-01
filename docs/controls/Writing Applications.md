# Writing applications

What the library does for you, what you're responsible for, and how to build a control when no stock one fits.

Read this before your first non-trivial app — particularly if you've used Spectre.Console or another
immediate-mode TUI library, because the model here is the opposite one and the habits don't transfer.

## Retained mode: you build a tree, not a frame

Terminal UI libraries split into two families, and which one you're in changes the shape of your entire program.

**Immediate mode** — ratatui, Dear ImGui, and Spectre.Console's own widgets. There is no persistent UI. You run a
loop, and on every pass you describe the entire screen from your own application state. Widgets are transient
descriptions, thrown away after each frame. Your state is the only thing that lives.

**Retained mode** — Jumbee.Console, WPF, Terminal.Gui. You construct control *objects* once. They persist, they
hold their own state, and they know how to draw themselves. You change a control by setting a property on it, and
the library works out what that means for the screen.

Jumbee is retained mode, with an immediate-mode renderer underneath it: controls hold state and are re-composited
by the framework, but the actual drawing goes through Spectre.Console's rendering. That combination is the point of
the library, and it's also the thing most likely to catch you out — **if you arrive from Spectre.Console, the
widgets look familiar but the ownership rules are inverted.**

| | Immediate mode | Retained mode (here) |
|---|---|---|
| Who holds UI state | you do, entirely | the controls do |
| Your main loop | draws every widget, every frame | doesn't exist |
| To change something | change your state; next frame reflects it | set a property on the control |
| To redraw | you redraw everything | the framework redraws what changed |
| Widget lifetime | one frame | the life of the app |

The consequence worth stating outright: **your application has no render loop.** You will not write
`while (running) { draw(); }`. You build controls, hand the root to `UI.Start`, and from then on your code runs only
in response to something — a keypress, an event, a timer tick. If you find yourself wanting to redraw the screen,
what you actually want is to set a property and let the framework schedule the redraw.

## What the framework owns

You do not have to write, and should not try to replace:

- **The frame loop.** One dedicated UI thread runs it. It drains a work queue, dispatches input, and repaints.
- **Redraw scheduling.** Controls mark themselves dirty; the loop coalesces that into at most one repaint per frame,
  and only re-renders what changed.
- **Terminal setup and restore.** `UI.Start` puts the terminal into the right modes (alternate screen, mouse
  reporting, focus reporting) and `UI.Stop` puts them back, in the right order, including on a kill signal.
- **Input decoding and routing.** Raw bytes become key, mouse and focus events, routed to the focused control and
  tunnelled outward through frames and layouts.
- **Layout.** On every terminal resize the tree is re-laid-out from the root. There's nothing to subscribe to.
- **Diffing to the terminal.** Only changed cells become ANSI output.

## What you own

### 1. The control tree, and how it sizes

Constructing controls and choosing the layouts that arrange them. **Sizing is the decision that matters** and it's
the one people get wrong: some layouts fill the terminal and some are deliberately fixed. Build the app shell from
`DockPanel` and `SplitPanel`; use `Grid` for regions where you want fixed geometry. See [Layouts](Layouts.md).

### 2. Getting data into controls, on the right thread

There is one UI thread and no lock. Scalar properties marshal themselves; **collections do not**, and collections
are what a live app updates. Sample on a background thread, marshal the update with `UI.Invoke`, and keep the whole
state change plus its invalidation on one side of that boundary. See [Live Data](Live%20Data.md).

### 3. Focus and app-level keys

Which controls are focusable, what `Tab` order means for your app, and any global chords —
`UI.RegisterHotKey(UI.HotKeys.Escape, UI.Stop)` and friends. Controls handle their own keys; anything
application-wide is yours. See [Input](../internal/Input.md).

### 4. Keeping the frame path cheap

The framework will happily run your expensive code every frame. A slow `Render()` isn't a slow control, it's a slow
application — everything shares the UI thread. Cache what doesn't change, rebuild on data change rather than per
paint, and cap history buffers.

### 5. Shutdown

`UI.Stop` when the user quits, and dispose what you created. Controls cancel their own feeds on disposal; your
background loops are yours to cancel — `UI.CancellationToken` is cancelled when the UI stops.

## The shape of an app

Four phases, in order. Everything after `UI.Start` is event-driven.

```csharp
// 1. Construct the controls. They persist — keep references to any you'll update later.
var label  = new TextLabel(TextLabelOrientation.Horizontal, "Count: 0", Color.Cyan1);
var button = new Button("Increment");

// 2. Compose them into a layout — children go to the constructor. This is where sizing is decided.
var bar  = new HorizontalStackPanel(label, button);
var root = new DockPanel(DockedControlPlacement.Top, bar, content);

// 3. Wire behaviour: control events, and app-level keys.
var count = 0;
button.Activated += (_, _) => label.Text = $"Count: {++count}";   // setting the property redraws it
UI.RegisterHotKey(UI.HotKeys.Escape, UI.Stop);

// 4. Hand the root over and let the loop run.
UI.Start(root);
```

Note what isn't there: no draw call, no dirty flag, no repaint request. `label.Text = …` is the entire update path
— the setter marks the control dirty and the next frame picks it up.

[GETTING-STARTED](../../GETTING-STARTED.md) walks a complete version of this, and the examples browser
(`examples/Jumbee.Console.Examples`) shows every control running with its source alongside.

## Building your own control

Most "I need a custom control" turns out not to. Work down this ladder and stop at the first rung that fits:

1. **Use a stock control.** Check the [decision table](README.md) first — the library is wider than it looks.
2. **Configure and compose.** A stock control plus a `ControlFrame` plus a layout covers a lot of what looks
   custom. A titled, bordered, scrollable panel is `.WithFrame()`, not a new class.
3. **Subclass an existing control** when you want its behaviour with a difference. This is the cheapest real
   customisation and the library uses it heavily: `Checkbox`, `RadioButton` and `Switch` are all `ToggleButton`
   with different glyphs and semantics; `MarkdownExtendedViewer` is a `MarkdownViewer` with an extra render seam.
4. **Write a new control** only when the drawing or the input handling is genuinely new.

### Choosing a base class

| Base | You implement | Use when | In the library |
|---|---|---|---|
| `CompositeControl` | build children + an arranging layout in the constructor, then `SetContent` | the control *is* several existing controls acting as one unit | `CodeEditor`, `ChatPrompt`, `Dialog`, `RunChart` |
| `RenderableControl` | `Render(RenderOptions, int maxWidth)` returning `Segment`s; optionally `Measure` | a new leaf control whose look is text and styling | `Button`, `Badge`, `Gauge`, `ProgressBar`, `ListBox`, `Tree` |
| `AnimatedControl` | frame fields (`interval`, `frameCount`), then `Start()` / `Stop()` | it animates on its own timer rather than on data | `Spinner` |
| `Control` | `Render()`, writing cells into `consoleBuffer` | you're drawing at the cell level — graphics, an emulator, a virtualized viewport | `Canvas`, `Globe`, `Log`, `Plot`, `DataTable`, `TerminalEmulator` |
| `SpectreControl<T>` | nothing — you wrap | you already have a Spectre `IRenderable` | wrapping any Spectre widget |

The two that get confused: **`RenderableControl` produces Spectre segments and lets Spectre lay them out;
`Control` writes cells directly.** Choose the first for anything expressible as styled text — it's far less code
and you inherit Spectre's measurement. Choose the second when you need to own every cell, which in practice means
graphics, sub-cell glyphs, or a viewport you're virtualizing yourself.

`CompositeControl` is not a layout. It's a single control that happens to be built from others — it drops into a
layout cell, gets framed, and routes focus among its children like one unit. If you only want to arrange controls,
you want an [`ILayout`](Layouts.md). See [Composite Controls](Composite%20Controls.md) for the authoring detail.

### What you must get right

Whichever base you pick, these are yours:

- **Invalidate when visual state changes.** Use `SetAtomicProperty` for scalar properties — it does the equality
  check, the assignment and the invalidation — or call `Invalidate()` directly. A property that changes what's on
  screen without invalidating simply won't appear.
- **Don't add locks.** All UI state lives on the one UI thread. Marshal with `UI.Invoke` instead.
- **Keep `Render()` cheap**, and don't read `ActualWidth`/`ActualHeight` before layout has run — they're `0` until
  then, so size-dependent geometry belongs in `Render()`, not a constructor or setter.
- **Override `OnDoubleClick` if you override `OnClick`.** Otherwise the second of two rapid clicks is silently
  swallowed: `protected override void OnDoubleClick(Position position) => OnClick(position);`
- **Don't `<inheritdoc/>` an override whose behaviour differs from the base.** The compiler won't warn, and the
  generated docs will confidently state the opposite of the truth.
- **Expose `Jumbee.Console.Color`,** not the ConsoleGUI or Spectre colour types, on anything a consumer touches.

Batch related changes into one setter with one invalidation where you can, and remember that an exception thrown
during a frame is swallowed — a control that throws every frame fails invisibly. See
[What happens when…](What%20Happens%20When.md).

## See also

- [Controls](README.md) — finding the right control in the first place.
- [Layouts](Layouts.md) — sizing, and which layouts fill the terminal.
- [Live Data](Live%20Data.md) — the threading model in full.
- [Composite Controls](Composite%20Controls.md) — authoring a control built from other controls.
