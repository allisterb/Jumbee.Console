# <a id="Jumbee_Console_Snapshot_ConsoleSnapshot"></a> Class ConsoleSnapshot

Namespace: [Jumbee.Console.Snapshot](Jumbee.Console.Snapshot.md)  
Assembly: Jumbee.Console.Snapshot.dll  

Renders Jumbee.Console controls headlessly (without a real terminal) to a <xref href="Jumbee.Console.ConsoleBuffer" data-throw-if-not-resolved="false"></xref>,
and converts that buffer to a text or PNG snapshot. Intended for tests and visual verification.

```csharp
public static class ConsoleSnapshot
```

#### Inheritance

object ← 
[ConsoleSnapshot](Jumbee.Console.Snapshot.ConsoleSnapshot.md)

## Methods

### <a id="Jumbee_Console_Snapshot_ConsoleSnapshot_BackgroundAt_Jumbee_Console_ConsoleBuffer_System_Int32_System_Int32_"></a> BackgroundAt\(ConsoleBuffer, int, int\)

The background colour of the rendered cell at (<code class="paramref">x</code>, <code class="paramref">y</code>), or
    <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> for transparent/default. See <xref href="Jumbee.Console.Snapshot.ConsoleSnapshot.ForegroundAt(Jumbee.Console.ConsoleBuffer%2cSystem.Int32%2cSystem.Int32)" data-throw-if-not-resolved="false"></xref>.

```csharp
public static Color? BackgroundAt(ConsoleBuffer buffer, int x, int y)
```

#### Parameters

`buffer` [ConsoleBuffer](Jumbee.Console.ConsoleBuffer.md)

`x` int

`y` int

#### Returns

 [Color](Jumbee.Console.Color.md)?

### <a id="Jumbee_Console_Snapshot_ConsoleSnapshot_Click_Jumbee_Console_ConsoleBuffer_System_Int32_System_Int32_System_Int32_Jumbee_Console_TerminalMouseButton_"></a> Click\(ConsoleBuffer, int, int, int, TerminalMouseButton\)

Clicks at (<code class="paramref">x</code>, <code class="paramref">y</code>) in <code class="paramref">buffer</code>: moves the pointer there,
then presses and releases <code class="paramref">clicks</code> times.

```csharp
public static bool Click(ConsoleBuffer buffer, int x, int y, int clicks = 1, TerminalMouseButton button = TerminalMouseButton.Left)
```

#### Parameters

`buffer` [ConsoleBuffer](Jumbee.Console.ConsoleBuffer.md)

`x` int

`y` int

`clicks` int

`button` [TerminalMouseButton](Jumbee.Console.TerminalMouseButton.md)

#### Returns

 bool

#### Remarks

Pass <code>clicks: 2</code> for a double-click (the presses land within the double-click window, so a control that
distinguishes them — e.g. <code>DataTable</code>'s row activation — sees a double-click). Pass
<code class="paramref">button</code> to simulate a right-click: the dispatch itself carries only a position, so this
latches <code>UI.MouseButton</code> the way the live input path does, which is what a control reads to tell the
buttons apart (e.g. <code>ListBox</code> opening its <code>ContextMenu</code>). The pointer is left hovering the target
afterwards, as it would be after a real click. Returns <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a> if nothing is under it.

### <a id="Jumbee_Console_Snapshot_ConsoleSnapshot_Drag_Jumbee_Console_ConsoleBuffer_System_Int32_System_Int32_System_Int32_System_Int32_System_Int32_"></a> Drag\(ConsoleBuffer, int, int, int, int, int\)

Drags from (<code class="paramref">fromX</code>, <code class="paramref">fromY</code>) to (<code class="paramref">toX</code>,
<code class="paramref">toY</code>) in <code class="paramref">buffer</code>: press at the start, <code class="paramref">steps</code> moves
along the way, release at the end.

```csharp
public static bool Drag(ConsoleBuffer buffer, int fromX, int fromY, int toX, int toY, int steps = 4)
```

#### Parameters

`buffer` [ConsoleBuffer](Jumbee.Console.ConsoleBuffer.md)

`fromX` int

`fromY` int

`toX` int

`toY` int

`steps` int

#### Returns

 bool

#### Remarks

<p>
Honours mouse capture the way the live path does: if the control takes the capture on press (a splitter, a
scrollbar thumb, a <code>Slider</code>), every later move and the release go to <em>it</em>, in its own frame,
with no hit-test — so a drag that wanders off the control still steers it. Without that, a test drag would
silently retarget whatever cell it passed over, and pass for the wrong reason.
</p>
<p>
The capture origin is computed here from the press hit-test rather than read from <code>ConsoleManager</code>,
which latches its own from a pointer position this headless path never sets. Returns <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a>
if nothing is under the start point.
</p>

### <a id="Jumbee_Console_Snapshot_ConsoleSnapshot_ForegroundAt_Jumbee_Console_ConsoleBuffer_System_Int32_System_Int32_"></a> ForegroundAt\(ConsoleBuffer, int, int\)

The foreground colour of the rendered cell at (<code class="paramref">x</code>, <code class="paramref">y</code>) as a
    <xref href="Jumbee.Console.Color" data-throw-if-not-resolved="false"></xref>, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> for the terminal default — for asserting a
    rendered colour in a test without reaching into the buffer's internal cell type. Text snapshots
    (<xref href="Jumbee.Console.Snapshot.ConsoleSnapshot.ToText(Jumbee.Console.ConsoleBuffer)" data-throw-if-not-resolved="false"></xref>) drop colour, so this is how you check it without a PNG.

```csharp
public static Color? ForegroundAt(ConsoleBuffer buffer, int x, int y)
```

#### Parameters

`buffer` [ConsoleBuffer](Jumbee.Console.ConsoleBuffer.md)

`x` int

`y` int

#### Returns

 [Color](Jumbee.Console.Color.md)?

### <a id="Jumbee_Console_Snapshot_ConsoleSnapshot_GlyphAt_Jumbee_Console_ConsoleBuffer_System_Int32_System_Int32_"></a> GlyphAt\(ConsoleBuffer, int, int\)

The glyph rendered at (<code class="paramref">x</code>, <code class="paramref">y</code>), or a space for an empty cell —
    the per-cell counterpart of <xref href="Jumbee.Console.Snapshot.ConsoleSnapshot.ForegroundAt(Jumbee.Console.ConsoleBuffer%2cSystem.Int32%2cSystem.Int32)" data-throw-if-not-resolved="false"></xref>/<xref href="Jumbee.Console.Snapshot.ConsoleSnapshot.BackgroundAt(Jumbee.Console.ConsoleBuffer%2cSystem.Int32%2cSystem.Int32)" data-throw-if-not-resolved="false"></xref>. Read a cell's glyph and
    colour together this way instead of mapping a <xref href="Jumbee.Console.Snapshot.ConsoleSnapshot.ToText(Jumbee.Console.ConsoleBuffer)" data-throw-if-not-resolved="false"></xref> index back to coordinates
    (rows are right-trimmed, so that mapping is error-prone).

```csharp
public static char GlyphAt(ConsoleBuffer buffer, int x, int y)
```

#### Parameters

`buffer` [ConsoleBuffer](Jumbee.Console.ConsoleBuffer.md)

`x` int

`y` int

#### Returns

 char

### <a id="Jumbee_Console_Snapshot_ConsoleSnapshot_Key_System_ConsoleKey_System_Boolean_System_Boolean_System_Boolean_"></a> Key\(ConsoleKey, bool, bool, bool\)

Builds a <xref href="System.ConsoleKeyInfo" data-throw-if-not-resolved="false"></xref> for a key with optional modifiers. For letter and digit keys
    the <code>KeyChar</code> is filled in (lowercase, uppercase under Shift, the control char under Ctrl) so the result
    matches how a hotkey registered with <xref href="Jumbee.Console.UI.RegisterHotKey(System.ConsoleKeyInfo%2cSystem.Action)" data-throw-if-not-resolved="false"></xref> — or a real keystroke — is keyed. That
    matters for <xref href="Jumbee.Console.Snapshot.ConsoleSnapshot.RenderAfter(Jumbee.Console.Control%2cSystem.Int32%2cSystem.Int32%2cSystem.Collections.Generic.IReadOnlyList%7bSystem.ConsoleKeyInfo%7d%2cSystem.Boolean)" data-throw-if-not-resolved="false"></xref> with
    <code>routeGlobal</code>: a bare-letter global hotkey only fires when the simulated key's char matches. Non-character
    keys (arrows, function keys, …) keep <code>'\0'</code>. For a punctuation hotkey (e.g. <code>'/'</code>), this method's
    char is <code>'\0'</code> and won't match — use <code>UI.HotKeys.Char('/')</code> to build the key instead.

```csharp
public static ConsoleKeyInfo Key(ConsoleKey key, bool shift = false, bool alt = false, bool control = false)
```

#### Parameters

`key` ConsoleKey

`shift` bool

`alt` bool

`control` bool

#### Returns

 ConsoleKeyInfo

### <a id="Jumbee_Console_Snapshot_ConsoleSnapshot_MouseMove_Jumbee_Console_ConsoleBuffer_System_Int32_System_Int32_"></a> MouseMove\(ConsoleBuffer, int, int\)

Moves the pointer to (<code class="paramref">x</code>, <code class="paramref">y</code>) in <code class="paramref">buffer</code>, firing
enter/leave/move on the controls under the old and new positions.

```csharp
public static bool MouseMove(ConsoleBuffer buffer, int x, int y)
```

#### Parameters

`buffer` [ConsoleBuffer](Jumbee.Console.ConsoleBuffer.md)

`x` int

`y` int

#### Returns

 bool

#### Remarks

Hover state persists across calls (as it does at runtime), so call <xref href="Jumbee.Console.Snapshot.ConsoleSnapshot.ResetMouse" data-throw-if-not-resolved="false"></xref> between
    tests to avoid leaving a control hovered. Returns <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a> if a control is under the pointer —
    a <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a> here usually means the target doesn't opt into the mouse (see the control's
    <code>WantsMouse</code>) or the coordinates are outside it.

### <a id="Jumbee_Console_Snapshot_ConsoleSnapshot_Render_ConsoleGUI_IControl_System_Int32_System_Int32_"></a> Render\(IControl, int, int\)

Composes a control tree into a <xref href="Jumbee.Console.ConsoleBuffer" data-throw-if-not-resolved="false"></xref> at the given size, without a real console.

```csharp
public static ConsoleBuffer Render(IControl content, int width, int height)
```

#### Parameters

`content` IControl

`width` int

`height` int

#### Returns

 [ConsoleBuffer](Jumbee.Console.ConsoleBuffer.md)

### <a id="Jumbee_Console_Snapshot_ConsoleSnapshot_Render_Jumbee_Console_Control_System_Int32_System_Int32_"></a> Render\(Control, int, int\)

Composes a single control (using its frame when present) into a buffer.

```csharp
public static ConsoleBuffer Render(Control control, int width, int height)
```

#### Parameters

`control` [Control](Jumbee.Console.Control.md)

`width` int

`height` int

#### Returns

 [ConsoleBuffer](Jumbee.Console.ConsoleBuffer.md)

### <a id="Jumbee_Console_Snapshot_ConsoleSnapshot_Render_Jumbee_Console_ILayout_System_Int32_System_Int32_"></a> Render\(ILayout, int, int\)

Composes a layout into a buffer.

```csharp
public static ConsoleBuffer Render(ILayout layout, int width, int height)
```

#### Parameters

`layout` [ILayout](Jumbee.Console.ILayout.md)

`width` int

`height` int

#### Returns

 [ConsoleBuffer](Jumbee.Console.ConsoleBuffer.md)

### <a id="Jumbee_Console_Snapshot_ConsoleSnapshot_RenderAfter_Jumbee_Console_Control_System_Int32_System_Int32_System_ConsoleKey___"></a> RenderAfter\(Control, int, int, params ConsoleKey\[\]\)

Renders <code class="paramref">control</code> once to establish layout, sends the given keys to it (routed via
<xref href="Jumbee.Console.UI.SendInput(Jumbee.Console.IFocusable%2cSystem.ConsoleKeyInfo%2cSystem.Boolean)" data-throw-if-not-resolved="false"></xref>), then renders and returns the result.

```csharp
public static ConsoleBuffer RenderAfter(Control control, int width, int height, params ConsoleKey[] keys)
```

#### Parameters

`control` [Control](Jumbee.Console.Control.md)

`width` int

`height` int

`keys` ConsoleKey\[\]

#### Returns

 [ConsoleBuffer](Jumbee.Console.ConsoleBuffer.md)

#### Remarks

Handy for snapshotting a control after navigation/editing. The keys are delivered to
    <code class="paramref">control</code> itself — <em>not</em> to whatever <xref href="Jumbee.Console.UI.SetFocus(Jumbee.Console.IFocusable)" data-throw-if-not-resolved="false"></xref> last targeted
    elsewhere in the tree — so pass the control that actually changes. For a composite app, that's the specific
    child under test (e.g. the list), not the root layout.

### <a id="Jumbee_Console_Snapshot_ConsoleSnapshot_RenderAfter_Jumbee_Console_Control_System_Int32_System_Int32_System_Collections_Generic_IReadOnlyList_System_ConsoleKeyInfo__System_Boolean_"></a> RenderAfter\(Control, int, int, IReadOnlyList<ConsoleKeyInfo\>, bool\)

As <xref href="Jumbee.Console.Snapshot.ConsoleSnapshot.RenderAfter(Jumbee.Console.Control%2cSystem.Int32%2cSystem.Int32%2cSystem.ConsoleKey%5b%5d)" data-throw-if-not-resolved="false"></xref> but accepts full key info, so modifier
keys (e.g. <code>Alt+Down</code> via <xref href="Jumbee.Console.Snapshot.ConsoleSnapshot.Key(System.ConsoleKey%2cSystem.Boolean%2cSystem.Boolean%2cSystem.Boolean)" data-throw-if-not-resolved="false"></xref>) can be sent. When <code class="paramref">routeGlobal</code> is
<a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, each key runs the global hotkey dispatch first (see
<xref href="Jumbee.Console.UI.SendInput(Jumbee.Console.IFocusable%2cSystem.ConsoleKeyInfo%2cSystem.Boolean)" data-throw-if-not-resolved="false"></xref>) so a snapshot can exercise hotkeys registered
with <xref href="Jumbee.Console.UI.RegisterHotKey(System.ConsoleKeyInfo%2cSystem.Action)" data-throw-if-not-resolved="false"></xref> — build the keys the same way they were registered (e.g. with
<xref href="Jumbee.Console.UI.HotKeys" data-throw-if-not-resolved="false"></xref>). As with the other overload, the keys go to <code class="paramref">control</code> itself,
not to whatever <xref href="Jumbee.Console.UI.SetFocus(Jumbee.Console.IFocusable)" data-throw-if-not-resolved="false"></xref> designates.

```csharp
public static ConsoleBuffer RenderAfter(Control control, int width, int height, IReadOnlyList<ConsoleKeyInfo> keys, bool routeGlobal = false)
```

#### Parameters

`control` [Control](Jumbee.Console.Control.md)

`width` int

`height` int

`keys` IReadOnlyList<ConsoleKeyInfo\>

`routeGlobal` bool

#### Returns

 [ConsoleBuffer](Jumbee.Console.ConsoleBuffer.md)

### <a id="Jumbee_Console_Snapshot_ConsoleSnapshot_RenderAfter_Jumbee_Console_ILayout_System_Int32_System_Int32_System_ConsoleKey___"></a> RenderAfter\(ILayout, int, int, params ConsoleKey\[\]\)

As <xref href="Jumbee.Console.Snapshot.ConsoleSnapshot.RenderAfter(Jumbee.Console.Control%2cSystem.Int32%2cSystem.Int32%2cSystem.ConsoleKey%5b%5d)" data-throw-if-not-resolved="false"></xref> but for a whole layout, so a
    key-driven multi-control screen (e.g. a header plus a plot) can be snapshotted as one unit. The keys go to
    <code class="paramref">layout</code> itself.

```csharp
public static ConsoleBuffer RenderAfter(ILayout layout, int width, int height, params ConsoleKey[] keys)
```

#### Parameters

`layout` [ILayout](Jumbee.Console.ILayout.md)

`width` int

`height` int

`keys` ConsoleKey\[\]

#### Returns

 [ConsoleBuffer](Jumbee.Console.ConsoleBuffer.md)

### <a id="Jumbee_Console_Snapshot_ConsoleSnapshot_RenderAfter_Jumbee_Console_ILayout_System_Int32_System_Int32_System_Collections_Generic_IReadOnlyList_System_ConsoleKeyInfo__System_Boolean_"></a> RenderAfter\(ILayout, int, int, IReadOnlyList<ConsoleKeyInfo\>, bool\)

As <xref href="Jumbee.Console.Snapshot.ConsoleSnapshot.RenderAfter(Jumbee.Console.Control%2cSystem.Int32%2cSystem.Int32%2cSystem.Collections.Generic.IReadOnlyList%7bSystem.ConsoleKeyInfo%7d%2cSystem.Boolean)" data-throw-if-not-resolved="false"></xref> but for a
    whole layout. With <code class="paramref">routeGlobal</code> each key runs the global hotkey dispatch first — the usual
    case for a layout, whose behaviour is driven by <xref href="Jumbee.Console.UI.RegisterHotKey(System.ConsoleKeyInfo%2cSystem.Action)" data-throw-if-not-resolved="false"></xref> rather than a single focused
    child.

```csharp
public static ConsoleBuffer RenderAfter(ILayout layout, int width, int height, IReadOnlyList<ConsoleKeyInfo> keys, bool routeGlobal = false)
```

#### Parameters

`layout` [ILayout](Jumbee.Console.ILayout.md)

`width` int

`height` int

`keys` IReadOnlyList<ConsoleKeyInfo\>

`routeGlobal` bool

#### Returns

 [ConsoleBuffer](Jumbee.Console.ConsoleBuffer.md)

### <a id="Jumbee_Console_Snapshot_ConsoleSnapshot_RenderAfterClick_Jumbee_Console_ILayout_System_Int32_System_Int32_System_Int32_System_Int32_System_Int32_"></a> RenderAfterClick\(ILayout, int, int, int, int, int\)

Renders <code class="paramref">layout</code>, clicks at (<code class="paramref">x</code>, <code class="paramref">y</code>), then
    re-renders and returns the result.

```csharp
public static ConsoleBuffer RenderAfterClick(ILayout layout, int width, int height, int x, int y, int clicks = 1)
```

#### Parameters

`layout` [ILayout](Jumbee.Console.ILayout.md)

`width` int

`height` int

`x` int

`y` int

`clicks` int

#### Returns

 [ConsoleBuffer](Jumbee.Console.ConsoleBuffer.md)

### <a id="Jumbee_Console_Snapshot_ConsoleSnapshot_RenderAfterClick_Jumbee_Console_Control_System_Int32_System_Int32_System_Int32_System_Int32_System_Int32_"></a> RenderAfterClick\(Control, int, int, int, int, int\)

Renders <code class="paramref">control</code>, clicks at (<code class="paramref">x</code>, <code class="paramref">y</code>), then
    re-renders and returns the result.

```csharp
public static ConsoleBuffer RenderAfterClick(Control control, int width, int height, int x, int y, int clicks = 1)
```

#### Parameters

`control` [Control](Jumbee.Console.Control.md)

`width` int

`height` int

`x` int

`y` int

`clicks` int

#### Returns

 [ConsoleBuffer](Jumbee.Console.ConsoleBuffer.md)

#### Remarks

Coordinates are relative to the rendered snapshot, so they include the control's own frame when it
    has one — a click on the first row of a bordered control's content is <code>y: 1</code>, not <code>y: 0</code>.

### <a id="Jumbee_Console_Snapshot_ConsoleSnapshot_ResetMouse"></a> ResetMouse\(\)

Clears the remembered pointer position, firing a leave on whatever is currently hovered.

```csharp
public static void ResetMouse()
```

#### Remarks

Call between tests: hover state is static (as at runtime), so it would otherwise carry over.

### <a id="Jumbee_Console_Snapshot_ConsoleSnapshot_SavePng_Jumbee_Console_ConsoleBuffer_System_String_Jumbee_Console_Snapshot_SnapshotImageOptions_"></a> SavePng\(ConsoleBuffer, string, SnapshotImageOptions?\)

Renders a buffer to a PNG file.

```csharp
public static void SavePng(ConsoleBuffer buffer, string path, SnapshotImageOptions? options = null)
```

#### Parameters

`buffer` [ConsoleBuffer](Jumbee.Console.ConsoleBuffer.md)

`path` string

`options` [SnapshotImageOptions](Jumbee.Console.Snapshot.SnapshotImageOptions.md)?

### <a id="Jumbee_Console_Snapshot_ConsoleSnapshot_SavePng_Jumbee_Console_Control_System_Int32_System_Int32_System_String_Jumbee_Console_Snapshot_SnapshotImageOptions_"></a> SavePng\(Control, int, int, string, SnapshotImageOptions?\)

Renders a control and saves it to a PNG file.

```csharp
public static void SavePng(Control control, int width, int height, string path, SnapshotImageOptions? options = null)
```

#### Parameters

`control` [Control](Jumbee.Console.Control.md)

`width` int

`height` int

`path` string

`options` [SnapshotImageOptions](Jumbee.Console.Snapshot.SnapshotImageOptions.md)?

### <a id="Jumbee_Console_Snapshot_ConsoleSnapshot_SavePng_Jumbee_Console_ILayout_System_Int32_System_Int32_System_String_Jumbee_Console_Snapshot_SnapshotImageOptions_"></a> SavePng\(ILayout, int, int, string, SnapshotImageOptions?\)

Renders a layout and saves it to a PNG file.

```csharp
public static void SavePng(ILayout layout, int width, int height, string path, SnapshotImageOptions? options = null)
```

#### Parameters

`layout` [ILayout](Jumbee.Console.ILayout.md)

`width` int

`height` int

`path` string

`options` [SnapshotImageOptions](Jumbee.Console.Snapshot.SnapshotImageOptions.md)?

### <a id="Jumbee_Console_Snapshot_ConsoleSnapshot_SavePngAfter_Jumbee_Console_Control_System_Int32_System_Int32_System_String_System_ConsoleKey___"></a> SavePngAfter\(Control, int, int, string, params ConsoleKey\[\]\)

Renders a control after sending the given keys and saves it to a PNG file.

```csharp
public static void SavePngAfter(Control control, int width, int height, string path, params ConsoleKey[] keys)
```

#### Parameters

`control` [Control](Jumbee.Console.Control.md)

`width` int

`height` int

`path` string

`keys` ConsoleKey\[\]

### <a id="Jumbee_Console_Snapshot_ConsoleSnapshot_SavePngAfter_Jumbee_Console_Control_System_Int32_System_Int32_System_String_System_Collections_Generic_IReadOnlyList_System_ConsoleKeyInfo__System_Boolean_"></a> SavePngAfter\(Control, int, int, string, IReadOnlyList<ConsoleKeyInfo\>, bool\)

Renders a control after sending the given keys (with modifiers) and saves it to a PNG file.

```csharp
public static void SavePngAfter(Control control, int width, int height, string path, IReadOnlyList<ConsoleKeyInfo> keys, bool routeGlobal = false)
```

#### Parameters

`control` [Control](Jumbee.Console.Control.md)

`width` int

`height` int

`path` string

`keys` IReadOnlyList<ConsoleKeyInfo\>

`routeGlobal` bool

#### Remarks

Pass <code class="paramref">routeGlobal</code> to fire hotkeys registered with <code>UI.RegisterHotKey</code>, the
    same as <xref href="Jumbee.Console.Snapshot.ConsoleSnapshot.RenderAfter(Jumbee.Console.Control%2cSystem.Int32%2cSystem.Int32%2cSystem.Collections.Generic.IReadOnlyList%7bSystem.ConsoleKeyInfo%7d%2cSystem.Boolean)" data-throw-if-not-resolved="false"></xref>.

### <a id="Jumbee_Console_Snapshot_ConsoleSnapshot_SavePngAfter_Jumbee_Console_ILayout_System_Int32_System_Int32_System_String_System_ConsoleKey___"></a> SavePngAfter\(ILayout, int, int, string, params ConsoleKey\[\]\)

Renders a layout after sending the given keys and saves it to a PNG file.

```csharp
public static void SavePngAfter(ILayout layout, int width, int height, string path, params ConsoleKey[] keys)
```

#### Parameters

`layout` [ILayout](Jumbee.Console.ILayout.md)

`width` int

`height` int

`path` string

`keys` ConsoleKey\[\]

### <a id="Jumbee_Console_Snapshot_ConsoleSnapshot_SavePngAfter_Jumbee_Console_ILayout_System_Int32_System_Int32_System_String_System_Collections_Generic_IReadOnlyList_System_ConsoleKeyInfo__System_Boolean_"></a> SavePngAfter\(ILayout, int, int, string, IReadOnlyList<ConsoleKeyInfo\>, bool\)

Renders a layout after sending the given keys (with modifiers) and saves it to a PNG file.

```csharp
public static void SavePngAfter(ILayout layout, int width, int height, string path, IReadOnlyList<ConsoleKeyInfo> keys, bool routeGlobal = false)
```

#### Parameters

`layout` [ILayout](Jumbee.Console.ILayout.md)

`width` int

`height` int

`path` string

`keys` IReadOnlyList<ConsoleKeyInfo\>

`routeGlobal` bool

#### Remarks

Use this to capture an <code>Overlay</code> — a modal's frame is only in the overlay, not in the root
    layout. Pass <code class="paramref">routeGlobal</code> to fire hotkeys registered with <code>UI.RegisterHotKey</code>.

### <a id="Jumbee_Console_Snapshot_ConsoleSnapshot_ToImage_Jumbee_Console_ConsoleBuffer_Jumbee_Console_Snapshot_SnapshotImageOptions_"></a> ToImage\(ConsoleBuffer, SnapshotImageOptions?\)

Renders a buffer to an image, drawing each cell's glyph and colors.

```csharp
public static Image<Rgba32> ToImage(ConsoleBuffer buffer, SnapshotImageOptions? options = null)
```

#### Parameters

`buffer` [ConsoleBuffer](Jumbee.Console.ConsoleBuffer.md)

`options` [SnapshotImageOptions](Jumbee.Console.Snapshot.SnapshotImageOptions.md)?

#### Returns

 Image<Rgba32\>

### <a id="Jumbee_Console_Snapshot_ConsoleSnapshot_ToLines_Jumbee_Console_ConsoleBuffer_"></a> ToLines\(ConsoleBuffer\)

The buffer as one right-trimmed string per row — the safe way to index a text snapshot by row
    (<xref href="Jumbee.Console.Snapshot.ConsoleSnapshot.ToText(Jumbee.Console.ConsoleBuffer)" data-throw-if-not-resolved="false"></xref> is exactly these joined by, and terminated with, <code>\n</code>). Because rows
    are right-trimmed of trailing spaces, a flat <code>index → (index % width, index / width)</code> mapping is wrong;
    index the row here, then the column within it. Colour and decoration are not captured — use
    <xref href="Jumbee.Console.Snapshot.ConsoleSnapshot.GlyphAt(Jumbee.Console.ConsoleBuffer%2cSystem.Int32%2cSystem.Int32)" data-throw-if-not-resolved="false"></xref>/<xref href="Jumbee.Console.Snapshot.ConsoleSnapshot.ForegroundAt(Jumbee.Console.ConsoleBuffer%2cSystem.Int32%2cSystem.Int32)" data-throw-if-not-resolved="false"></xref>/<xref href="Jumbee.Console.Snapshot.ConsoleSnapshot.BackgroundAt(Jumbee.Console.ConsoleBuffer%2cSystem.Int32%2cSystem.Int32)" data-throw-if-not-resolved="false"></xref> for per-cell checks that need no
    row arithmetic at all.

```csharp
public static string[] ToLines(ConsoleBuffer buffer)
```

#### Parameters

`buffer` [ConsoleBuffer](Jumbee.Console.ConsoleBuffer.md)

#### Returns

 string\[\]

### <a id="Jumbee_Console_Snapshot_ConsoleSnapshot_ToText_Jumbee_Console_ConsoleBuffer_"></a> ToText\(ConsoleBuffer\)

Converts a buffer to a plain-text snapshot: one <code>\n</code>-terminated line per row, each row
    <b>right-trimmed of trailing spaces</b> (so snapshots are stable regardless of right-padding). Because rows are
    trimmed, a flat <code>index → (index % width, index / width)</code> back-mapping to buffer coordinates is wrong — use
    <xref href="Jumbee.Console.Snapshot.ConsoleSnapshot.ToLines(Jumbee.Console.ConsoleBuffer)" data-throw-if-not-resolved="false"></xref> to index by row, or <xref href="Jumbee.Console.Snapshot.ConsoleSnapshot.GlyphAt(Jumbee.Console.ConsoleBuffer%2cSystem.Int32%2cSystem.Int32)" data-throw-if-not-resolved="false"></xref> for a specific cell. Colour and
    text decoration are NOT captured, so state distinguished only by colour (e.g. a dimmed "read" row) is invisible
    to a text assertion — use <xref href="Jumbee.Console.Snapshot.ConsoleSnapshot.ForegroundAt(Jumbee.Console.ConsoleBuffer%2cSystem.Int32%2cSystem.Int32)" data-throw-if-not-resolved="false"></xref>/<xref href="Jumbee.Console.Snapshot.ConsoleSnapshot.BackgroundAt(Jumbee.Console.ConsoleBuffer%2cSystem.Int32%2cSystem.Int32)" data-throw-if-not-resolved="false"></xref>, or <code>ToImage</code>/<code>SavePng</code>.

```csharp
public static string ToText(ConsoleBuffer buffer)
```

#### Parameters

`buffer` [ConsoleBuffer](Jumbee.Console.ConsoleBuffer.md)

#### Returns

 string

### <a id="Jumbee_Console_Snapshot_ConsoleSnapshot_ToText_Jumbee_Console_Control_System_Int32_System_Int32_"></a> ToText\(Control, int, int\)

Renders a control and returns its text snapshot.

```csharp
public static string ToText(Control control, int width, int height)
```

#### Parameters

`control` [Control](Jumbee.Console.Control.md)

`width` int

`height` int

#### Returns

 string

### <a id="Jumbee_Console_Snapshot_ConsoleSnapshot_ToText_Jumbee_Console_ILayout_System_Int32_System_Int32_"></a> ToText\(ILayout, int, int\)

Renders a layout and returns its text snapshot.

```csharp
public static string ToText(ILayout layout, int width, int height)
```

#### Parameters

`layout` [ILayout](Jumbee.Console.ILayout.md)

`width` int

`height` int

#### Returns

 string

### <a id="Jumbee_Console_Snapshot_ConsoleSnapshot_ToTextAfter_Jumbee_Console_Control_System_Int32_System_Int32_System_ConsoleKey___"></a> ToTextAfter\(Control, int, int, params ConsoleKey\[\]\)

Renders a control after sending the given keys and returns its text snapshot.

```csharp
public static string ToTextAfter(Control control, int width, int height, params ConsoleKey[] keys)
```

#### Parameters

`control` [Control](Jumbee.Console.Control.md)

`width` int

`height` int

`keys` ConsoleKey\[\]

#### Returns

 string

### <a id="Jumbee_Console_Snapshot_ConsoleSnapshot_ToTextAfter_Jumbee_Console_Control_System_Int32_System_Int32_System_Collections_Generic_IReadOnlyList_System_ConsoleKeyInfo__System_Boolean_"></a> ToTextAfter\(Control, int, int, IReadOnlyList<ConsoleKeyInfo\>, bool\)

Renders a control after sending the given keys (with modifiers) and returns its text snapshot.
    Pass <code class="paramref">routeGlobal</code> to run each key through the global hotkey dispatch first (see
    <xref href="Jumbee.Console.Snapshot.ConsoleSnapshot.RenderAfter(Jumbee.Console.Control%2cSystem.Int32%2cSystem.Int32%2cSystem.Collections.Generic.IReadOnlyList%7bSystem.ConsoleKeyInfo%7d%2cSystem.Boolean)" data-throw-if-not-resolved="false"></xref>).

```csharp
public static string ToTextAfter(Control control, int width, int height, IReadOnlyList<ConsoleKeyInfo> keys, bool routeGlobal = false)
```

#### Parameters

`control` [Control](Jumbee.Console.Control.md)

`width` int

`height` int

`keys` IReadOnlyList<ConsoleKeyInfo\>

`routeGlobal` bool

#### Returns

 string

### <a id="Jumbee_Console_Snapshot_ConsoleSnapshot_ToTextAfter_Jumbee_Console_ILayout_System_Int32_System_Int32_System_ConsoleKey___"></a> ToTextAfter\(ILayout, int, int, params ConsoleKey\[\]\)

Renders a layout after sending the given keys and returns its text snapshot.

```csharp
public static string ToTextAfter(ILayout layout, int width, int height, params ConsoleKey[] keys)
```

#### Parameters

`layout` [ILayout](Jumbee.Console.ILayout.md)

`width` int

`height` int

`keys` ConsoleKey\[\]

#### Returns

 string

### <a id="Jumbee_Console_Snapshot_ConsoleSnapshot_ToTextAfter_Jumbee_Console_ILayout_System_Int32_System_Int32_System_Collections_Generic_IReadOnlyList_System_ConsoleKeyInfo__System_Boolean_"></a> ToTextAfter\(ILayout, int, int, IReadOnlyList<ConsoleKeyInfo\>, bool\)

Renders a layout after sending the given keys (with modifiers) and returns its text snapshot. Pass
    <code class="paramref">routeGlobal</code> to run each key through the global hotkey dispatch first — the usual case for a
    layout driven by <xref href="Jumbee.Console.UI.RegisterHotKey(System.ConsoleKeyInfo%2cSystem.Action)" data-throw-if-not-resolved="false"></xref>.

```csharp
public static string ToTextAfter(ILayout layout, int width, int height, IReadOnlyList<ConsoleKeyInfo> keys, bool routeGlobal = false)
```

#### Parameters

`layout` [ILayout](Jumbee.Console.ILayout.md)

`width` int

`height` int

`keys` IReadOnlyList<ConsoleKeyInfo\>

`routeGlobal` bool

#### Returns

 string

### <a id="Jumbee_Console_Snapshot_ConsoleSnapshot_ToTextAfterClick_Jumbee_Console_ILayout_System_Int32_System_Int32_System_Int32_System_Int32_System_Int32_"></a> ToTextAfterClick\(ILayout, int, int, int, int, int\)

Renders a layout, clicks, and returns the resulting screen as text.

```csharp
public static string ToTextAfterClick(ILayout layout, int width, int height, int x, int y, int clicks = 1)
```

#### Parameters

`layout` [ILayout](Jumbee.Console.ILayout.md)

`width` int

`height` int

`x` int

`y` int

`clicks` int

#### Returns

 string

### <a id="Jumbee_Console_Snapshot_ConsoleSnapshot_ToTextAfterClick_Jumbee_Console_Control_System_Int32_System_Int32_System_Int32_System_Int32_System_Int32_"></a> ToTextAfterClick\(Control, int, int, int, int, int\)

Renders a control, clicks, and returns the resulting screen as text.

```csharp
public static string ToTextAfterClick(Control control, int width, int height, int x, int y, int clicks = 1)
```

#### Parameters

`control` [Control](Jumbee.Console.Control.md)

`width` int

`height` int

`x` int

`y` int

`clicks` int

#### Returns

 string

### <a id="Jumbee_Console_Snapshot_ConsoleSnapshot_Wheel_Jumbee_Console_ConsoleBuffer_System_Int32_System_Int32_System_Int32_"></a> Wheel\(ConsoleBuffer, int, int, int\)

Rotates the wheel by <code class="paramref">delta</code> notches over (<code class="paramref">x</code>, <code class="paramref">y</code>) —
negative scrolls up, positive scrolls down.

```csharp
public static bool Wheel(ConsoleBuffer buffer, int x, int y, int delta)
```

#### Parameters

`buffer` [ConsoleBuffer](Jumbee.Console.ConsoleBuffer.md)

`x` int

`y` int

`delta` int

#### Returns

 bool

#### Remarks

Only reaches controls that implement <code>IMouseWheelListener</code>; returns <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a>
    otherwise (and when nothing is under the pointer).

