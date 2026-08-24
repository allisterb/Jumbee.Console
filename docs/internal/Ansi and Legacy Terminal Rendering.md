# ANSI and Legacy Terminal Rendering

This document describes how `ConsoleManager` renders the same composited cell grid to two
different classes of terminal: modern **ANSI** terminals that interpret VT escape sequences, and
**legacy** terminals that don't (and so are driven through plain `System.Console` writes). The most
visible difference between the two paths is the cursor, which is hardware-native on ANSI and a
hand-drawn "software" cell on legacy.

## 1. The `AnsiEnabled` switch

A single static flag selects the path:

```csharp
// ConsoleManager
public static bool AnsiEnabled = true;
```

It is set once at startup from the `isAnsiTerminal` argument to `UI.Start` ([UI.cs](../../src/Jumbee.Console/UI.cs)):

```csharp
public static Task Start(ILayout layout, ..., bool isAnsiTerminal = true, ...)
{
    ConsoleManager.AnsiEnabled = isAnsiTerminal;
    ...
    else if (!isAnsiTerminal)
        // Legacy terminal: 16-colour System.Console output.
        ConsoleManager.Console = new SimplifiedConsole();
```

So enabling legacy mode does two things: it flips `AnsiEnabled` to `false`, and it swaps the
`IConsole` for a `SimplifiedConsole` (see §4). Input also stays keyboard-only in legacy mode, since
VT mouse/paste/focus sequences aren't available.

Both paths share everything *above* the renderer — the layout pass, the pull-based cell composition
(see *ConsoleGUI Control Rendering*), and the `ConsoleBuffer` diff. They diverge only in how a
changed cell is turned into bytes, and how the cursor is drawn.

### 1.1 `isAnsiTerminal: true` is a request, not a fact

`isAnsiTerminal` defaults to `true`, so for a long time the library simply **assumed** ANSI unless an app passed
`--legacy`. On Windows that assumption can be flatly wrong, and when it is, the result is not a degraded picture —
it is no picture. A console only interprets escapes once an application enables
`ENABLE_VIRTUAL_TERMINAL_PROCESSING`, so every sequence is printed as literal text: screens of
`[38;2;148;148;148m` and `[59;1H` from the first frame.

`TerminalCapabilities.TryEnableAnsiOutput()` runs at the top of `Start`, before any other write (all the others are
escape sequences). Enabling and detecting are the same call:

| Situation | `SetConsoleMode(handle, mode \| VT)` | Result |
|---|---|---|
| Not Windows | — | ANSI |
| Redirected stdout (pipe, file, Docker log) | `GetConsoleMode` fails | **inconclusive → caller's choice kept** |
| Console, VT already on | — | ANSI |
| Console, VT accepted | succeeds | ANSI, and VT is now on |
| Console with **Use legacy console** ticked | fails, `ERROR_INVALID_PARAMETER` (87) | **legacy** |

Two constraints on that table are easy to get wrong:

- **A redirected stdout is not a legacy terminal.** `GetConsoleMode` fails on a pipe, which says nothing about what
  eventually reads the bytes — a recording, a container log, a snapshot harness. Those want the escapes, so an
  inconclusive probe must leave the caller's choice alone. Only a console that is *present and refuses* downgrades.
- **The probe only ever downgrades, and never touches a caller-supplied `IConsole`.** A stub console belongs to a
  test or a headless render; probing the real terminal on its behalf would silently push every snapshot onto the
  legacy path on a legacy-console machine. `AnsiCapabilityDetectionTests` pins both halves of that gate.

`TerminalCapabilities.Restore()` puts the console mode back, and it must be the **last** thing done to the terminal
on the way out — after it, escape sequences are printed rather than obeyed, so an SGR reset or an alternate-screen
leave emitted afterwards would land on screen as text.

"Use legacy console" is a **per-machine** setting, not per-window, so it changes the behaviour of every console app
at once — including anything the build or a test harness shells out to.

## 2. The branch point: `Update`

The renderer is dirty-rectangle: each frame `ConsoleManager.FlushDirty()` composites only the region(s)
damaged since the last frame (see *Multithreading* → the frame loop), calling `Update(rect)` once per dirty
rect — or once over the whole screen on a full redraw (startup, resize, or any action input). `Update(rect)`
clips the rect to the buffer/window and then forks on the flag:

```csharp
private static void Update(Rect rect)
{
    Console.OnRefresh();
    rect = Rect.Intersect(rect, Rect.OfSize(BufferSize));
    rect = Rect.Intersect(rect, Rect.OfSize(WindowSize));

    if (!AnsiEnabled) { UpdateLegacy(rect); return; }

    // ...ANSI path...
}
```

Both paths walk the same `rect`, ask `ContentContext[position]` for each cell, and use
`_buffer.Update(position, cell)` as the diff: it returns `true` only when the cell differs from what
was last drawn, so unchanged cells are skipped. The cursor cell (`Character.IsCursor`) is always
*noted* before the diff-skip, so the cursor is found even on frames where its cell didn't change.

## 3. The ANSI path

The ANSI path builds one batch of escape sequences per frame with an `AnsiControlSequenceBuilder`
(`acsb`) and writes it to the real console off the UI thread (`Task.Run(acsb.WriteToSystemConsole)`).

* **Colour & decoration** are emitted as truecolor SGR via `WriteCharacterAnsiSequence`, which only
  emits a new colour/decoration when it differs from the running `currentFg`/`currentBg`/
  `currentDecoration` — minimizing escape-sequence churn.
* **Positioning** uses `acsb.MoveCursorTo(y, x)` only when the next changed cell isn't contiguous
  with the previous one (`y != lastY || x != lastX + 1`).
* **The cursor is the terminal's own hardware cursor.** When a cell flagged `IsCursor` is seen, its
  position drives `acsb.MoveCursorTo` + `SetCursorVisibility(true)`; the encoded DECSCUSR style and
  optional OSC 12 colour (see §5) are emitted *only on change*, so the terminal's native blink phase
  isn't reset every frame. When no cursor cell is present on a full scan, the cursor is hidden.

How the *blink* is produced depends on `EmulateBlinkingCursor` (see §4.1). In the default (native)
mode the real DECSCUSR style is emitted and the terminal blinks the cursor itself — no blink timer.

### 3.1 Autowrap is off, and the emitter must keep it that way

`UI.Start` emits `CSI ?7l` (DECAWM off) for the session and restores it on `Stop`, the signal path, and via the
self-heal sequence for a run that was hard-killed.

The reason is the **bottom-right cell**. A full-screen renderer writes it, and with autowrap on that write leaves
the terminal holding a *pending wrap*. Terminals that defer that state — conhost, Windows Terminal, xterm — do
nothing until the next printable character, and the emitter's next act is an SGR reset or a CUP, so nothing
happens. A terminal that resolves it eagerly instead **scrolls the screen up one row**.

That is not a one-frame glitch. The emitter diffs against `_buffer`, its own model of what the display holds, so
after an unnoticed scroll every cell it believes unchanged is displayed one row out of place and is never
rewritten. The picture degrades into stale content smeared across the whole screen, with the status line repeating
down the bottom — one copy per frame, since only the cells that change get redrawn into the wrong place.
**ConEmu reproduces this and this change fixes it**, confirmed by the status line rendering correctly afterwards.
conhost and Windows Terminal defer the wrap correctly and never trip on it, which is why it went unnoticed.

Turning DECAWM off costs nothing here because **nothing in the emitter relies on wrapping**: `MoveCursorTo` is
issued whenever the next changed cell isn't contiguous, and a row boundary is never contiguous (`y != lastY`), so
every row already begins with an explicit CUP. With wrap off, writing the last column simply parks the cursor
there and the next row starts with its CUP as usual.

`AnsiWrapIndependenceTests` pins this against the raw escape stream rather than the parsed screen — a parser
rebuilds the same picture whether the emitter wrapped or jumped, so the distinction is invisible to
`AnsiConsoleSnapshot`. One test asserts no glyph is ever emitted past the last column; the other asserts the last
column *is* written, so the first cannot pass by never approaching the edge.

## 4. The legacy path: `UpdateLegacy`

Legacy terminals can't interpret the escape batch, so `UpdateLegacy` writes each changed cell
through the `IConsole` abstraction — concretely `SimplifiedConsole`, which maps each cell to
`System.Console` 16-colour output:

```csharp
// SimplifiedConsole.Write
var foreground = character.Foreground ?? Color.White;
var background = character.Background ?? Color.Black;
SafeConsole.WriteOrThrow(position.X, position.Y,
    ColorConverter.GetNearestConsoleColor(background),
    ColorConverter.GetNearestConsoleColor(foreground),
    content);
```

Note two consequences of this resolution: null colours become concrete white/black, and truecolor is
quantized to the nearest of the 16 console colours.

### The software cursor

By default the cursor here is a **software cursor** — the hardware cursor can't be repositioned
per-cell without visibly jumping, so it's kept hidden and the cursor is drawn as a cell. (The one
exception is the native-blink mode in §4.1, where the `System.Console` hardware cursor is shown for
*blinking* styles.) The cursor cell is skipped in the draw loop and rendered separately afterwards:

```csharp
if (cell.Character.IsCursor) continue;   // drawn as the software cursor below, not as a raw glyph
```

After the cell loop, `UpdateLegacy` renders the cursor cell itself. Its shape follows the DECSCUSR
style encoded on the cursor `Character` (§5):

```csharp
// RenderLegacyCursorCell, "on" phase
: style <= 2 ? new Character(content, bg, fg)   // block: invert fg/bg
: style <= 4 ? new Character('_', fg, bg)        // underline
             : new Character('|', fg, bg);       // bar
```

> **Colour resolution matters.** A block cursor renders by inverting the cell's fg/bg. On an empty
> cell both are `null`, and inverting `null`↔`null` yields a plain space (an invisible cursor). So
> `fg`/`bg` are resolved to white/black *before* inverting, guaranteeing a visible block.

### Blink

Blinking is done by us, off a wall-clock derived phase so the rate is independent of frame cadence:

```csharp
private const long BlinkHalfPeriodMs = 530;          // ~1Hz
private static bool CursorBlinkOn() => (Environment.TickCount64 / BlinkHalfPeriodMs) % 2 == 0;
```

`TickLegacyCursorBlink` is called once per frame from `AdjustBufferSize` (on the UI thread), so the
cursor keeps blinking even on otherwise-idle frames without an extra timer or thread. It re-renders
the cursor cell only when the blink phase actually flips (`on != _legacyCursorShown`). `IsBlinkingStyle`
decides which styles blink: DECSCUSR `0/1/3/5` blink; `2/4/6` are steady. (This software blink runs
only in emulated mode — see §4.1.)

## 4.1 Cursor blink: native vs emulated (`EmulateBlinkingCursor`)

A blinking cursor and a continuously-animating UI are in tension: writing characters anywhere moves
the terminal's one-and-only cursor, so each animated frame forces a reposition (`MoveCursorTo` on
ANSI; `SetCursorPosition` on legacy) back to the logical cursor — and most terminals **reset the
cursor's blink phase on a reposition**. With animation at ~20fps the native blink never completes a
cycle, so it stutters or appears solid. `ConsoleManager.EmulateBlinkingCursor` (default `false`) picks
the trade-off; it only affects *blinking* styles (steady styles are unaffected):

* **`false` — native blink (default).** The real blinking style is used and the terminal blinks the
  cursor: on ANSI the blinking DECSCUSR style is emitted; on legacy the `System.Console` hardware
  cursor is shown over the plain glyph and left to blink. Cheapest (no per-blink work) and friendliest
  to screen readers, but the blink goes erratic under continuous animation. Best when nothing animates.
* **`true` — emulated blink.** We blink the cursor ourselves: the **steady** DECSCUSR variant
  (`SteadyCursorStyle`, ANSI) or the steady software cell (legacy) is rendered, and its visibility is
  toggled on/off at the `CursorBlinkOn()` wall-clock rate. A per-frame reposition then only moves a
  *steady* cursor, so the blink stays constant regardless of animation. The constant rate costs: under
  animation it rides the redraw that's happening anyway; when idle, ANSI emits just the `DECTCEM`
  visibility toggle via `ConsoleManager.TickCursorBlink` (a few bytes, no buffer scan) and legacy flips
  the software cell via `TickLegacyCursorBlink`.

### "Cursor gone" detection

Only a **full** scan is authoritative about the cursor having disappeared. A partial update that
doesn't cover the cursor cell leaves the software cursor in place; a full update with no `IsCursor`
cell clears `_cursorPosition` (the cell it occupied was redrawn as a normal glyph by the loop). The
ANSI path makes the identical full-vs-partial distinction before hiding the hardware cursor.

## 5. Shared cursor encoding (`CursorEncoding`)

Both paths get the cursor's style and colour from the **same place**: bits packed into the high end
of the cursor cell's `Decoration`, above the real decoration flags
([Character.cs](../../ext/C-sharp-console-gui-framework/ConsoleGUI/Data/Character.cs)). This lets a
cursor cell carry its style/colour up through compositing to the renderer without any extra fields:

* `EncodeStyle` / `DecodeStyle` — DECSCUSR style 0–6 in 3 bits (shift 9).
* `WithColorFlag` / `HasColor` — a flag meaning "an explicit cursor colour is present" (the colour
  itself rides in the cell's `Foreground`).
* `StripCursorBits` — clears those high bits so they never leak into SGR decorations.

The ANSI renderer decodes the style into DECSCUSR and the colour into OSC 12, and strips the bits
before emitting SGR. The legacy renderer decodes the style to pick block/underline/bar and to decide
whether the cursor blinks. The cursor cell itself is produced by `AnsiConsoleBufferCursor.ShowCursor`
in [AnsiConsoleBuffer.cs](../../src/Jumbee.Console/AnsiConsoleBuffer.cs), which is agnostic to the
render path — the same `IsCursor` cell drives both.

## 6. Choosing a path

| | ANSI (`AnsiEnabled = true`) | Legacy (`AnsiEnabled = false`) |
|---|---|---|
| Output | Batched VT escape sequences, written off-thread | `IConsole.Write` per cell (`SimplifiedConsole`) |
| Colour | Truecolor SGR | Nearest of 16 `System.Console` colours |
| Cursor | Terminal hardware cursor (DECSCUSR + OSC 12) | Software cell cursor (hardware cursor for native-blink) |
| Blink | Native, or emulated — see §4.1 (`EmulateBlinkingCursor`) | Native, or emulated — see §4.1 (`EmulateBlinkingCursor`) |
| Input | VT input (mouse/paste/focus available) | Keyboard-only (`ConsoleInputSource`) |

Default is ANSI. Pass `isAnsiTerminal: false` to `UI.Start` for terminals that don't interpret
escape sequences.

## 7. Known limitations of the legacy path

Validated interactively in a genuine `cmd.exe` (not Windows Terminal): the full control set renders —
including the embedded `TerminalEmulator` (a live ConPTY child, ANSI-in → cells → legacy-out), coloured
charts, and the ray-traced `Globe`. Two inherent limitations follow from writing cell-by-cell through
`System.Console` with no atomic full-frame flush:

- **Cost scales with the number of cells *written* per frame, not with frame rate.** Idle/static content is
  cheap — the in-memory composite + diff is the same as ANSI and *zero* cells reach `System.Console`
  (steady state measured ~0.6 ms/frame). The per-cell cost only appears when many cells change at once: the
  first paint, a resize, or full-screen animation. Heavy animation is much slower than ANSI — the spinning
  `Globe` runs ~30 ms/frame on legacy vs sub-ms on ANSI, because it repaints most of the screen every frame.
- **Tearing under heavy churn.** Because a frame is written one cell at a time (no double-buffer swap or
  single-stream flush), a burst of many changing cells — fast-streaming terminal output, rapid scroll — is
  painted *incrementally*, so mid-repaint states are briefly visible. The final settled frame is always
  correct; ANSI avoids this by emitting the whole frame diff as one byte sequence.

**Mitigation (not yet done):** coalesce contiguous same-colour cells on a row into a single
`System.Console.Write`, and drop the per-cell `SetCursorPosition` calls in favour of one per run. That shrinks
the per-frame write window, which reduces both the animation cost and the visible tearing. Deferred — the
current behaviour is acceptable for the mostly-static UIs the legacy path targets.
