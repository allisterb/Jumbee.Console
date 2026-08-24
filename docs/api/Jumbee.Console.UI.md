# <a id="Jumbee_Console_UI"></a> Class UI

Namespace: [Jumbee.Console](Jumbee.Console.md)  
Assembly: Jumbee.Console.dll  

Manages th overall UI and provides a paint event for controls to subscribe to.

```csharp
public static class UI
```

#### Inheritance

object ← 
[UI](Jumbee.Console.UI.md)

## Fields

### <a id="Jumbee_Console_UI_ProcessMetrics"></a> ProcessMetrics

Collector for process/frame performance metrics, sampled each frame and surfaced by the perf HUD.

```csharp
public static readonly ProcessMetrics ProcessMetrics
```

#### Field Value

 [ProcessMetrics](Jumbee.Console.ProcessMetrics.md)

## Properties

### <a id="Jumbee_Console_UI_AverageControlPaintTimes"></a> AverageControlPaintTimes

Per-control average paint time (ms) over the recent sample window, keyed by control.

```csharp
public static IDictionary<IFocusable, double> AverageControlPaintTimes { get; }
```

#### Property Value

 IDictionary<[IFocusable](Jumbee.Console.IFocusable.md), double\>

### <a id="Jumbee_Console_UI_AverageDrawTime"></a> AverageDrawTime

Average time (ms) the renderer spent compositing/drawing frames to the console.

```csharp
public static double AverageDrawTime { get; }
```

#### Property Value

 double

### <a id="Jumbee_Console_UI_AverageFrameLatency"></a> AverageFrameLatency

Average end-to-end time (ms) from starting to render a frame to it being written to the terminal: the render,
the wait for the previous write, and the write itself.

```csharp
public static double AverageFrameLatency { get; }
```

#### Property Value

 double

#### Remarks

This is LATENCY, not throughput. The write runs concurrent with the next frame's render, so the frame rate is
bounded by whichever side is slower, not by this sum — a latency of 3 ms does not mean a 333 fps ceiling.
Summed per frame, not from separate averages, so it describes real frames rather than a blend of populations.

### <a id="Jumbee_Console_UI_AveragePaintTime"></a> AveragePaintTime

Average time (ms) spent firing control <xref href="Jumbee.Console.UI.Paint" data-throw-if-not-resolved="false"></xref> handlers, over the recent sample window.

```csharp
public static double AveragePaintTime { get; }
```

#### Property Value

 double

### <a id="Jumbee_Console_UI_AverageWriteTime"></a> AverageWriteTime

Average time (ms) the actual terminal write took. Concurrent with the next frame's render, so it does NOT add
to <xref href="Jumbee.Console.UI.AverageDrawTime" data-throw-if-not-resolved="false"></xref> — the throughput ceiling is whichever of the two is larger.

```csharp
public static double AverageWriteTime { get; }
```

#### Property Value

 double

### <a id="Jumbee_Console_UI_AverageWriteWaitTime"></a> AverageWriteWaitTime

Average time (ms) a frame waited for the previous frame's write before its own could start.

```csharp
public static double AverageWriteWaitTime { get; }
```

#### Property Value

 double

### <a id="Jumbee_Console_UI_CancellationToken"></a> CancellationToken

A token that is cancelled when the UI stops.

```csharp
public static CancellationToken CancellationToken { get; }
```

#### Property Value

 CancellationToken

#### Remarks

Background work started alongside the UI (e.g. a Spectre progress/live loop) should observe this so
    it terminates on shutdown instead of running on.

### <a id="Jumbee_Console_UI_Dispatcher"></a> Dispatcher

The UI thread dispatcher.

```csharp
public static Dispatcher Dispatcher { get; }
```

#### Property Value

 [Dispatcher](Jumbee.Console.Dispatcher.md)

### <a id="Jumbee_Console_UI_Focused"></a> Focused

The currently focused registered control, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> if none.

```csharp
public static IFocusable? Focused { get; }
```

#### Property Value

 [IFocusable](Jumbee.Console.IFocusable.md)?

### <a id="Jumbee_Console_UI_GlyphTheme"></a> GlyphTheme

The active glyph theme. Defaults to <xref href="Jumbee.Console.DefaultGlyphTheme" data-throw-if-not-resolved="false"></xref>.

```csharp
public static IGlyphTheme GlyphTheme { get; set; }
```

#### Property Value

 [IGlyphTheme](Jumbee.Console.IGlyphTheme.md)

#### Remarks

Controls capture their indicator glyphs from it. Assigning raises <xref href="Jumbee.Console.UI.ThemeChanged" data-throw-if-not-resolved="false"></xref>
(on the UI thread), so every live control re-captures — i.e. assigning it is a runtime theme switch.

### <a id="Jumbee_Console_UI_HasFocus"></a> HasFocus

Whether the terminal window currently has focus (DEC mode 1004). Defaults to <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>.

```csharp
public static bool HasFocus { get; }
```

#### Property Value

 bool

#### Remarks

Updated from <xref href="Jumbee.Console.FocusInputEvent" data-throw-if-not-resolved="false"></xref>s once focus reporting is enabled by the raw input source.

### <a id="Jumbee_Console_UI_IsRunning"></a> IsRunning

True while the UI loop is running.

```csharp
public static bool IsRunning { get; }
```

#### Property Value

 bool

#### Remarks

Background work (e.g. a Spectre progress/live loop) can poll this to exit when the UI stops.

### <a id="Jumbee_Console_UI_Layout"></a> Layout

The root layout hosting the UI's controls, set by <xref href="Jumbee.Console.UI.Start(Jumbee.Console.ILayout%2cSystem.Int32%2cSystem.Int32%2cSystem.Int32%2cSystem.Boolean%2cConsoleGUI.Api.IConsole%2cJumbee.Console.IInputSource%2cSystem.Boolean)" data-throw-if-not-resolved="false"></xref>.

```csharp
public static ILayout Layout { get; }
```

#### Property Value

 [ILayout](Jumbee.Console.ILayout.md)

#### Remarks

Read-only: the root can't be swapped after <xref href="Jumbee.Console.UI.Start(Jumbee.Console.ILayout%2cSystem.Int32%2cSystem.Int32%2cSystem.Int32%2cSystem.Boolean%2cConsoleGUI.Api.IConsole%2cJumbee.Console.IInputSource%2cSystem.Boolean)" data-throw-if-not-resolved="false"></xref>. To reconfigure the UI at runtime
    (e.g. a full-screen "zen" toggle), change a container in place rather than the root — collapse a pane via
    <xref href="Jumbee.Console.SplitPanel.SplitPosition" data-throw-if-not-resolved="false"></xref>, or reassign a <xref href="Jumbee.Console.DockPanel.DockedControl" data-throw-if-not-resolved="false"></xref> /
    <xref href="Jumbee.Console.DockPanel.FillControl" data-throw-if-not-resolved="false"></xref>. Move focus to a still-visible control when you hide the focused one.

### <a id="Jumbee_Console_UI_MaxControlPaintTimes"></a> MaxControlPaintTimes

Per-control peak paint time (ms) over the recent sample window, keyed by control.

```csharp
public static IDictionary<IFocusable, double> MaxControlPaintTimes { get; }
```

#### Property Value

 IDictionary<[IFocusable](Jumbee.Console.IFocusable.md), double\>

### <a id="Jumbee_Console_UI_MouseButton"></a> MouseButton

The mouse button of the most recent press, latched until the next press.

```csharp
public static TerminalMouseButton MouseButton { get; }
```

#### Property Value

 [TerminalMouseButton](Jumbee.Console.TerminalMouseButton.md)

#### Remarks

A control's <code>OnClick</code>/<code>OnDoubleClick</code> reads this to distinguish a right-click (e.g. to open
    a context menu) from a left-click — the dispatch itself carries only a position, not a button.

### <a id="Jumbee_Console_UI_Overlay"></a> Overlay

The root overlay host, available once <xref href="Jumbee.Console.UI.Start(Jumbee.Console.ILayout%2cSystem.Int32%2cSystem.Int32%2cSystem.Int32%2cSystem.Boolean%2cConsoleGUI.Api.IConsole%2cJumbee.Console.IInputSource%2cSystem.Boolean)" data-throw-if-not-resolved="false"></xref> has run; <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> before <xref href="Jumbee.Console.UI.Start(Jumbee.Console.ILayout%2cSystem.Int32%2cSystem.Int32%2cSystem.Int32%2cSystem.Boolean%2cConsoleGUI.Api.IConsole%2cJumbee.Console.IInputSource%2cSystem.Boolean)" data-throw-if-not-resolved="false"></xref>.

```csharp
public static Overlay? Overlay { get; set; }
```

#### Property Value

 [Overlay](Jumbee.Console.Overlay.md)?

#### Remarks

<p><xref href="Jumbee.Console.UI.Start(Jumbee.Console.ILayout%2cSystem.Int32%2cSystem.Int32%2cSystem.Int32%2cSystem.Boolean%2cConsoleGUI.Api.IConsole%2cJumbee.Console.IInputSource%2cSystem.Boolean)" data-throw-if-not-resolved="false"></xref> wraps the app's root in this overlay (reusing it if the root already is an
<xref href="Jumbee.Console.UI.Overlay" data-throw-if-not-resolved="false"></xref>), so there is always a layer for pop-ups (dropdowns, menus, autocomplete, modals).
Controls that show pop-ups — <xref href="Jumbee.Console.Select" data-throw-if-not-resolved="false"></xref>, <xref href="Jumbee.Console.MenuBar" data-throw-if-not-resolved="false"></xref>, <xref href="Jumbee.Console.ContextMenu" data-throw-if-not-resolved="false"></xref>,
<xref href="Jumbee.Console.Autocomplete" data-throw-if-not-resolved="false"></xref> — show into this automatically, so there is no per-control overlay to wire up.</p>
<p>Normally you never set this — <xref href="Jumbee.Console.UI.Start(Jumbee.Console.ILayout%2cSystem.Int32%2cSystem.Int32%2cSystem.Int32%2cSystem.Boolean%2cConsoleGUI.Api.IConsole%2cJumbee.Console.IInputSource%2cSystem.Boolean)" data-throw-if-not-resolved="false"></xref> does. The setter exists for advanced hosting (and
tests) that need to designate the overlay pop-ups show into without going through <xref href="Jumbee.Console.UI.Start(Jumbee.Console.ILayout%2cSystem.Int32%2cSystem.Int32%2cSystem.Int32%2cSystem.Boolean%2cConsoleGUI.Api.IConsole%2cJumbee.Console.IInputSource%2cSystem.Boolean)" data-throw-if-not-resolved="false"></xref>; the
value must be the overlay that is actually being rendered as the root, or pop-ups won't be visible.</p>

### <a id="Jumbee_Console_UI_PeakFrameLatency"></a> PeakFrameLatency

The worst end-to-end frame latency (ms) in the recent window.

```csharp
public static double PeakFrameLatency { get; }
```

#### Property Value

 double

#### Remarks

Available only because render and write are paired per frame; adding separate averages could never
    yield a worst case.

### <a id="Jumbee_Console_UI_StyleTheme"></a> StyleTheme

The active style theme. Defaults to <xref href="Jumbee.Console.DefaultStyleTheme" data-throw-if-not-resolved="false"></xref>.

```csharp
public static IStyleTheme StyleTheme { get; set; }
```

#### Property Value

 [IStyleTheme](Jumbee.Console.IStyleTheme.md)

#### Remarks

Controls capture their default colours/decorations from it. Assigning raises <xref href="Jumbee.Console.UI.ThemeChanged" data-throw-if-not-resolved="false"></xref>
(on the UI thread), so every live control re-captures — i.e. assigning it is a runtime theme switch.

### <a id="Jumbee_Console_UI_WriteQueueDepth"></a> WriteQueueDepth

Frames rendered but not yet written to the terminal, right now — usually 0, since the queue drains
    between frames. <xref href="Jumbee.Console.UI.WriteQueueDepthPeak" data-throw-if-not-resolved="false"></xref> is the one that shows a backlog.

```csharp
public static int WriteQueueDepth { get; }
```

#### Property Value

 int

### <a id="Jumbee_Console_UI_WriteQueueDepthPeak"></a> WriteQueueDepthPeak

The most frames left waiting behind a terminal write over the recent window.

```csharp
public static int WriteQueueDepthPeak { get; }
```

#### Property Value

 int

#### Remarks

Coarse: frames also queue for a thread-pool slot, so a small peak appears even when the write costs
    nothing. <xref href="Jumbee.Console.UI.AverageWriteWaitTime" data-throw-if-not-resolved="false"></xref> is the reliable back-pressure signal — it measures time actually
    spent blocked.

## Methods

### <a id="Jumbee_Console_UI_CheckAccess"></a> CheckAccess\(\)

Returns <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a> when the caller is on the UI thread (or none is running).

```csharp
public static bool CheckAccess()
```

#### Returns

 bool

### <a id="Jumbee_Console_UI_CompileHelp"></a> CompileHelp\(\)

Compiles the help shown by <xref href="Jumbee.Console.UI.ShowHelp" data-throw-if-not-resolved="false"></xref>: a built-in "General" entry (global keys) followed by each
registered control's <xref href="Jumbee.Console.Control.GetHelpInfo" data-throw-if-not-resolved="false"></xref> (modified by its <xref href="Jumbee.Console.Control.OnHelp" data-throw-if-not-resolved="false"></xref>
handlers), deduplicated by <xref href="Jumbee.Console.HelpInfo.Name" data-throw-if-not-resolved="false"></xref>.

```csharp
public static IReadOnlyList<HelpInfo> CompileHelp()
```

#### Returns

 IReadOnlyList<[HelpInfo](Jumbee.Console.HelpInfo.md)\>

#### Remarks

Exposed so an app can present its own help UI.

### <a id="Jumbee_Console_UI_FocusDown"></a> FocusDown\(\)

Moves focus one cell down in the root layout's 2-D grid. Bound to <code>Ctrl+Down</code> by default.

```csharp
public static void FocusDown()
```

### <a id="Jumbee_Console_UI_FocusLeft"></a> FocusLeft\(\)

Moves focus one cell left/right/up/down in the root layout's 2-D grid (wraps; skips empties). Bound
    to <code>Ctrl+Left/Right/Up/Down</code> by default.

```csharp
public static void FocusLeft()
```

### <a id="Jumbee_Console_UI_FocusNext"></a> FocusNext\(\)

Moves focus to the next focusable control within the current root-layout region, wrapping. Bound to
    <code>Ctrl+N</code> by default.

```csharp
public static void FocusNext()
```

#### Remarks

A no-op unless the focused region is a multi-focusable nested layout.

### <a id="Jumbee_Console_UI_FocusPrevious"></a> FocusPrevious\(\)

Moves focus to the previous focusable control within the current region. Bound to <code>Ctrl+P</code>.

```csharp
public static void FocusPrevious()
```

### <a id="Jumbee_Console_UI_FocusRight"></a> FocusRight\(\)

Moves focus one cell right in the root layout's 2-D grid. Bound to <code>Ctrl+Right</code> by default.

```csharp
public static void FocusRight()
```

### <a id="Jumbee_Console_UI_FocusUp"></a> FocusUp\(\)

Moves focus one cell up in the root layout's 2-D grid. Bound to <code>Ctrl+Up</code> by default.

```csharp
public static void FocusUp()
```

### <a id="Jumbee_Console_UI_HideHelp"></a> HideHelp\(\)

Closes the global help dialog if it is open.

```csharp
public static void HideHelp()
```

### <a id="Jumbee_Console_UI_Invoke_System_Action_"></a> Invoke\(Action\)

Runs <code class="paramref">action</code> on the UI thread, marshaling automatically: <em>inline and immediately</em>
when the caller is already on the UI thread (or no UI thread is running, e.g. headless/initialization),
otherwise posted to the dispatcher queue. Then requests a redraw.

```csharp
public static void Invoke(Action action)
```

#### Parameters

`action` Action

The action to execute on the UI thread.

#### Remarks

<p>This is the primary, default way to mutate
control/layout state from anywhere — the change always ends up serialized on the UI thread with rendering, so
no lock is needed.</p>
<p>Does NOT block: off-thread it is fire-and-forget (the action runs later on the UI thread) — it does not wait
for completion or surface the action's exception to the caller. That is unlike the blocking, WPF-style
<xref href="Jumbee.Console.Dispatcher.Invoke(System.Action)" data-throw-if-not-resolved="false"></xref>; if you need to wait for the result use <xref href="Jumbee.Console.UI.InvokeAsync(System.Action)" data-throw-if-not-resolved="false"></xref>.
Differs from <xref href="Jumbee.Console.UI.Post(System.Action)" data-throw-if-not-resolved="false"></xref> in two ways: (1) it runs inline when already on the UI thread instead of
always deferring to a later frame, and (2) it requests a redraw afterwards. Prefer this for state changes;
reach for <xref href="Jumbee.Console.UI.Post(System.Action)" data-throw-if-not-resolved="false"></xref> only when you deliberately want to defer to a later frame.</p>

### <a id="Jumbee_Console_UI_InvokeAsync_System_Action_"></a> InvokeAsync\(Action\)

Runs an action on the UI thread and returns a task that completes when it finishes.

```csharp
public static Task InvokeAsync(Action action)
```

#### Parameters

`action` Action

#### Returns

 Task

### <a id="Jumbee_Console_UI_InvokeAsync__1_System_Func___0__"></a> InvokeAsync<T\>\(Func<T\>\)

Runs a function on the UI thread and returns a task with its result.

```csharp
public static Task<T> InvokeAsync<T>(Func<T> func)
```

#### Parameters

`func` Func<T\>

#### Returns

 Task<T\>

#### Type Parameters

`T` 

### <a id="Jumbee_Console_UI_InvokeAsync_System_Func_System_Threading_Tasks_Task__"></a> InvokeAsync\(Func<Task\>\)

Runs an async delegate on the UI thread and returns a task that completes (unwrapped) when it finishes.

```csharp
public static Task InvokeAsync(Func<Task> func)
```

#### Parameters

`func` Func<Task\>

#### Returns

 Task

### <a id="Jumbee_Console_UI_InvokeAsync__1_System_Func_System_Threading_Tasks_Task___0___"></a> InvokeAsync<T\>\(Func<Task<T\>\>\)

Runs an async function on the UI thread and returns a task with its (unwrapped) result.

```csharp
public static Task<T> InvokeAsync<T>(Func<Task<T>> func)
```

#### Parameters

`func` Func<Task<T\>\>

#### Returns

 Task<T\>

#### Type Parameters

`T` 

### <a id="Jumbee_Console_UI_IsInteractiveTerminal"></a> IsInteractiveTerminal\(\)

<a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a> when both console input and output are real terminals (not redirected/piped),
    so it is safe to put the terminal into raw VT input mode.

```csharp
public static bool IsInteractiveTerminal()
```

#### Returns

 bool

#### Remarks

This is what <xref href="Jumbee.Console.UI.Start(Jumbee.Console.ILayout%2cSystem.Int32%2cSystem.Int32%2cSystem.Int32%2cSystem.Boolean%2cConsoleGUI.Api.IConsole%2cJumbee.Console.IInputSource%2cSystem.Boolean)" data-throw-if-not-resolved="false"></xref> uses to decide whether to default to a mouse-capable
    <xref href="Jumbee.Console.VtInputSource" data-throw-if-not-resolved="false"></xref> when no <code>input</code> is supplied.

### <a id="Jumbee_Console_UI_MarkDirty"></a> MarkDirty\(\)

Marks the UI as needing a redraw on the next frame.

```csharp
public static void MarkDirty()
```

#### Remarks

Called whenever control content or layout changes; idle frames skip the redraw until this is set.

### <a id="Jumbee_Console_UI_PaintFrame"></a> PaintFrame\(\)

Synchronously paints a single frame: fires the <xref href="Jumbee.Console.UI.Paint" data-throw-if-not-resolved="false"></xref> event so every control renders
into its buffer. Intended for headless/snapshot rendering when the UI timer loop is not running.

```csharp
public static void PaintFrame()
```

#### Remarks

This does not draw to a real console; callers compose the painted control buffers themselves
(for example via a <xref href="ConsoleGUI.Common.DrawingContext" data-throw-if-not-resolved="false"></xref>).

### <a id="Jumbee_Console_UI_Post_System_Action_"></a> Post\(Action\)

Queues <code class="paramref">action</code> to run on the UI thread on a later turn of the frame loop, and returns
immediately (never blocks, never runs inline).

```csharp
public static void Post(Action action)
```

#### Parameters

`action` Action

#### Remarks

<p>Unlike <xref href="Jumbee.Console.UI.Invoke(System.Action)" data-throw-if-not-resolved="false"></xref>, it <em>always</em> defers — even when the caller is already on the UI
thread — so the action runs after the current work/layout/paint settles (work posted while the queue is
draining waits for the next frame; see <xref href="Jumbee.Console.UI.Dispatcher" data-throw-if-not-resolved="false"></xref>).</p>
<p>Use this — rather than <xref href="Jumbee.Console.UI.Invoke(System.Action)" data-throw-if-not-resolved="false"></xref> — when you specifically want to defer: to break re-entrancy (e.g.
run something <em>after</em> the current input/layout pass), or for a self-feeding pump that must not starve
rendering. It is the low-level primitive: it does NOT request a redraw, so if the action changes visual state
it must invalidate itself (or call <xref href="Jumbee.Console.UI.MarkDirty" data-throw-if-not-resolved="false"></xref>). For "run on the UI thread, now if I already am,"
use <xref href="Jumbee.Console.UI.Invoke(System.Action)" data-throw-if-not-resolved="false"></xref> instead.</p>

### <a id="Jumbee_Console_UI_RegisterHotKey_System_ConsoleKeyInfo_System_Action_"></a> RegisterHotKey\(ConsoleKeyInfo, Action\)

Registers an application-wide hotkey handled <em>before</em> any control (so it works regardless of focus),
overwriting any existing action for the same key.

```csharp
public static void RegisterHotKey(ConsoleKeyInfo key, Action action)
```

#### Parameters

`key` ConsoleKeyInfo

`action` Action

#### Remarks

<p><xref href="Jumbee.Console.UI.HotKeys.CtrlQ" data-throw-if-not-resolved="false"></xref> → <xref href="Jumbee.Console.UI.Stop" data-throw-if-not-resolved="false"></xref> is registered by default. Typically called before
<xref href="Jumbee.Console.UI.Start(Jumbee.Console.ILayout%2cSystem.Int32%2cSystem.Int32%2cSystem.Int32%2cSystem.Boolean%2cConsoleGUI.Api.IConsole%2cJumbee.Console.IInputSource%2cSystem.Boolean)" data-throw-if-not-resolved="false"></xref>; the key must match what the input decoder produces (use the <xref href="Jumbee.Console.UI.HotKeys" data-throw-if-not-resolved="false"></xref>
constants/helpers, e.g. <xref href="Jumbee.Console.UI.HotKeys.Char(System.Char)" data-throw-if-not-resolved="false"></xref> for a bare letter).</p>
<p><b>Scope:</b> the hotkey table is <b>process-global</b> — one table shared across the whole app, not
scoped to a <xref href="Jumbee.Console.UI.Start(Jumbee.Console.ILayout%2cSystem.Int32%2cSystem.Int32%2cSystem.Int32%2cSystem.Boolean%2cConsoleGUI.Api.IConsole%2cJumbee.Console.IInputSource%2cSystem.Boolean)" data-throw-if-not-resolved="false"></xref> root or a control tree. Two consequences worth knowing:</p>
<ul><li>A matched hotkey is dispatched <em>before</em> the focused control and marks the event handled, so a
letter registered as a hotkey never reaches a focused text field. To let a <code>TextInput</code>/<code>TextEditor</code>
receive those letters, <xref href="Jumbee.Console.UI.UnregisterHotKey(System.ConsoleKeyInfo)" data-throw-if-not-resolved="false"></xref> them while it has focus and re-register on blur.</li><li>Constructing a second app/control tree (e.g. a fresh instance per headless snapshot capture) re-registers
the same keys onto the one table, so input then routes to the newest registrant. In tests, register/exercise
one instance at a time (or unregister between them).</li></ul>

### <a id="Jumbee_Console_UI_RestoreBuiltInHotKeys"></a> RestoreBuiltInHotKeys\(\)

Drops every hotkey registered via <xref href="Jumbee.Console.UI.RegisterHotKey(System.ConsoleKeyInfo%2cSystem.Action)" data-throw-if-not-resolved="false"></xref>, leaving only the library's own
    (Ctrl+Q, the Ctrl focus tier, F1).

```csharp
public static void RestoreBuiltInHotKeys()
```

#### Remarks

Called by <xref href="Jumbee.Console.UI.Stop" data-throw-if-not-resolved="false"></xref>, so an app's hotkeys are scoped to the session that registered them. That matters
when one process starts the UI more than once — swapping between two screens, say: without it the second
session inherits the first's bindings, still pointing at the first session's objects, which by then are
disposed. Register hotkeys per session, alongside the tree they act on.

### <a id="Jumbee_Console_UI_SendInput_Jumbee_Console_IFocusable_System_ConsoleKeyInfo_System_Boolean_"></a> SendInput\(IFocusable, ConsoleKeyInfo, bool\)

Sends a key to <code class="paramref">target</code>. When <code class="paramref">routeGlobal</code> is <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>,
the global hotkey dispatch runs first (mirroring the live <xref href="Jumbee.Console.UI.OnInput(Jumbee.Console.TerminalInputEvent)" data-throw-if-not-resolved="false"></xref> path): a matching hotkey
registered via <xref href="Jumbee.Console.UI.RegisterHotKey(System.ConsoleKeyInfo%2cSystem.Action)" data-throw-if-not-resolved="false"></xref> fires and marks the event handled, in which case the focused
control never sees the key. Lets headless/snapshot tests exercise global hotkeys, not just control-routed
input. To match a registered hotkey, build <code class="paramref">key</code> the same way it was registered (e.g. with
the <xref href="Jumbee.Console.UI.HotKeys" data-throw-if-not-resolved="false"></xref> helpers, or a <xref href="System.ConsoleKeyInfo" data-throw-if-not-resolved="false"></xref> whose char matches).

```csharp
public static void SendInput(IFocusable target, ConsoleKeyInfo key, bool routeGlobal = false)
```

#### Parameters

`target` [IFocusable](Jumbee.Console.IFocusable.md)

`key` ConsoleKeyInfo

`routeGlobal` bool

### <a id="Jumbee_Console_UI_SendInput_Jumbee_Console_IFocusable_System_ConsoleKey_System_Boolean_System_Boolean_System_Boolean_System_Boolean_"></a> SendInput\(IFocusable, ConsoleKey, bool, bool, bool, bool\)

Sends a key (with optional modifiers) to <code class="paramref">target</code>. See
<xref href="Jumbee.Console.UI.SendInput(Jumbee.Console.IFocusable%2cSystem.ConsoleKeyInfo%2cSystem.Boolean)" data-throw-if-not-resolved="false"></xref> for <code class="paramref">routeGlobal</code>.

```csharp
public static void SendInput(IFocusable target, ConsoleKey key, bool shift = false, bool alt = false, bool control = false, bool routeGlobal = false)
```

#### Parameters

`target` [IFocusable](Jumbee.Console.IFocusable.md)

`key` ConsoleKey

`shift` bool

`alt` bool

`control` bool

`routeGlobal` bool

### <a id="Jumbee_Console_UI_SetFocus_Jumbee_Console_IFocusable_"></a> SetFocus\(IFocusable\)

Moves keyboard focus to <code class="paramref">target</code>, clearing focus on all other registered
    controls (single-focus).

```csharp
public static void SetFocus(IFocusable target)
```

#### Parameters

`target` [IFocusable](Jumbee.Console.IFocusable.md)

#### Remarks

Used by click-to-focus; runs on the UI thread.

### <a id="Jumbee_Console_UI_SetTheme_Jumbee_Console_IStyleTheme_Jumbee_Console_IGlyphTheme_"></a> SetTheme\(IStyleTheme, IGlyphTheme\)

Convenience to set both themes at once.

```csharp
public static void SetTheme(IStyleTheme styleTheme, IGlyphTheme glyphTheme)
```

#### Parameters

`styleTheme` [IStyleTheme](Jumbee.Console.IStyleTheme.md)

`glyphTheme` [IGlyphTheme](Jumbee.Console.IGlyphTheme.md)

#### Remarks

<p>The work (and the <xref href="Jumbee.Console.UI.ThemeChanged" data-throw-if-not-resolved="false"></xref> notification that makes live controls re-capture) is done
by the <xref href="Jumbee.Console.UI.StyleTheme" data-throw-if-not-resolved="false"></xref>/<xref href="Jumbee.Console.UI.GlyphTheme" data-throw-if-not-resolved="false"></xref> setters.</p>
<p>Re-capture happens only on assignment, never on the render path, so frame-to-frame cost is unchanged.
Each control re-applies the theme only to properties it has not explicitly overridden (see
<code>ThemeOverrides</code>), so explicit per-control styling — including a frame's border — survives the switch.</p>

### <a id="Jumbee_Console_UI_SetTheme_Jumbee_Console_ITheme_"></a> SetTheme\(ITheme\)

Applies both halves of a bundled <xref href="Jumbee.Console.ITheme" data-throw-if-not-resolved="false"></xref> (see <xref href="Jumbee.Console.UI.SetTheme(Jumbee.Console.IStyleTheme%2cJumbee.Console.IGlyphTheme)" data-throw-if-not-resolved="false"></xref>).

```csharp
public static void SetTheme(ITheme theme)
```

#### Parameters

`theme` [ITheme](Jumbee.Console.ITheme.md)

### <a id="Jumbee_Console_UI_ShowHelp"></a> ShowHelp\(\)

Opens the global help dialog — a modal with one tab per control (compiled from every control's
<xref href="Jumbee.Console.Control.GetHelpInfo" data-throw-if-not-resolved="false"></xref>, deduplicated by <xref href="Jumbee.Console.HelpInfo.Name" data-throw-if-not-resolved="false"></xref>), the focused control's tab
shown first.

```csharp
public static void ShowHelp()
```

#### Remarks

Bound to <xref href="Jumbee.Console.UI.HotKeys.F1" data-throw-if-not-resolved="false"></xref> by default; pressing it again closes the dialog. A no-op when
no control supplies help. The dialog is shown on the UI-owned system overlay (see <xref href="Jumbee.Console.UI.Start(Jumbee.Console.ILayout%2cSystem.Int32%2cSystem.Int32%2cSystem.Int32%2cSystem.Boolean%2cConsoleGUI.Api.IConsole%2cJumbee.Console.IInputSource%2cSystem.Boolean)" data-throw-if-not-resolved="false"></xref>).

### <a id="Jumbee_Console_UI_Start_Jumbee_Console_ILayout_System_Int32_System_Int32_System_Int32_System_Boolean_ConsoleGUI_Api_IConsole_Jumbee_Console_IInputSource_System_Boolean_"></a> Start\(ILayout, int, int, int, bool, IConsole?, IInputSource?, bool\)

Initializes the console and starts the UI, returning a task that completes when the UI stops.

```csharp
public static Task Start(ILayout layout, int width = 110, int height = 25, int fps = 10, bool isAnsiTerminal = true, IConsole? console = null, IInputSource? input = null, bool useAlternateScreen = true)
```

#### Parameters

`layout` [ILayout](Jumbee.Console.ILayout.md)

The root layout to display.

`width` int

Initial console width in cells. On an ANSI terminal the real size is read and the physical window is never resized.

`height` int

Initial console height in cells.

`fps` int

Target repaint rate in frames per second (default 10 ≈ a 100 ms frame). It sets the UI
    thread's per-frame interval to <code>1000 / fps</code> ms — the thread wakes at most once per interval to drain
    input and repaint dirty regions. Raise it for smoother animation or live data (e.g. 30–60) at the cost of more
    CPU; lower it to idle cheaper. A frame is skipped when nothing is dirty, so a higher <code class="paramref">fps</code>
    only costs CPU while the UI is actually changing. Values ≤ 0 are treated as the slowest rate and the
    interval is floored at 1 ms (~1000 fps max), so no setting can divide-by-zero or busy-loop the thread.

`isAnsiTerminal` bool

Emit ANSI escape sequences (<a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>) vs. 16-colour System.Console output (<a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a>).

`console` IConsole?

An explicit console sink; when <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> one is chosen from <code class="paramref">isAnsiTerminal</code>.

`input` [IInputSource](Jumbee.Console.IInputSource.md)?

An explicit input source; when <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> one is chosen for the terminal (see <xref href="Jumbee.Console.UI.DefaultInputSource(System.Boolean)" data-throw-if-not-resolved="false"></xref>).

`useAlternateScreen` bool

Run on the alternate screen buffer so the user's prior terminal contents are restored on exit.

#### Returns

 Task

### <a id="Jumbee_Console_UI_Stop"></a> Stop\(\)

Stops the UI update loop and disposes of the timer.

```csharp
public static void Stop()
```

### <a id="Jumbee_Console_UI_UnregisterHotKey_System_ConsoleKeyInfo_"></a> UnregisterHotKey\(ConsoleKeyInfo\)

Removes a global hotkey registered via <xref href="Jumbee.Console.UI.RegisterHotKey(System.ConsoleKeyInfo%2cSystem.Action)" data-throw-if-not-resolved="false"></xref>.

```csharp
public static void UnregisterHotKey(ConsoleKeyInfo key)
```

#### Parameters

`key` ConsoleKeyInfo

### <a id="Jumbee_Console_UI_VerifyAccess"></a> VerifyAccess\(\)

Throws when the caller is not on the UI thread.

```csharp
public static void VerifyAccess()
```

### <a id="Jumbee_Console_UI_Paint"></a> Paint

Raised each frame so subscribed controls render their state; the subscriber's target control is
    tracked for per-control paint timing and focus.

```csharp
public static event EventHandler<UI.PaintEventArgs> Paint
```

#### Event Type

 EventHandler<[UI](Jumbee.Console.UI.md).[PaintEventArgs](Jumbee.Console.UI.PaintEventArgs.md)\>

### <a id="Jumbee_Console_UI_ThemeChanged"></a> ThemeChanged

Raised by <xref href="Jumbee.Console.UI.SetTheme(Jumbee.Console.IStyleTheme%2cJumbee.Console.IGlyphTheme)" data-throw-if-not-resolved="false"></xref> after the active themes change, so live controls re-apply them.

```csharp
public static event EventHandler? ThemeChanged
```

#### Event Type

 EventHandler?

#### Remarks

Controls subscribe in their constructor and unsubscribe on <xref href="System.IDisposable.Dispose" data-throw-if-not-resolved="false"></xref>.

