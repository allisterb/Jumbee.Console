# <a id="Jumbee_Console_TerminalEmulator"></a> Class TerminalEmulator

Namespace: [Jumbee.Console](Jumbee.Console.md)  
Assembly: Jumbee.Console.dll  

A control that runs a child process in a pseudo-console (<xref href="Jumbee.Console.ConPty" data-throw-if-not-resolved="false"></xref>), parses its ANSI output with
VtNetCore, and paints the emulated screen into the control's cell area. Input routed to the focused control is
translated to terminal bytes and sent to the process.

```csharp
public class TerminalEmulator : Control, IFocusable
```

#### Inheritance

object ← 
Control ← 
[Control](Jumbee.Console.Control.md) ← 
[TerminalEmulator](Jumbee.Console.TerminalEmulator.md)

#### Implements

[IFocusable](Jumbee.Console.IFocusable.md)

#### Inherited Members

[Control.this\[Position\]](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_Item\_ConsoleGUI\_Space\_Position\_), 
[Control.Width](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_Width), 
[Control.ActualWidth](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_ActualWidth), 
[Control.Height](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_Height), 
[Control.ActualHeight](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_ActualHeight), 
[Control.HasLayout](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_HasLayout), 
[Control.Frame](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_Frame), 
[Control.HasFrame](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_HasFrame), 
[Control.Focusable](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_Focusable), 
[Control.IsFocused](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_IsFocused), 
[Control.FocusableControl](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_FocusableControl), 
[Control.FocusedControl](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_FocusedControl), 
[Control.HandlesInput](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_HandlesInput), 
[Control.OnInput\(UI.InputEventArgs\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_OnInput\_Jumbee\_Console\_UI\_InputEventArgs\_), 
[Control.OnInput\(InputEvent\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_OnInput\_ConsoleGUI\_Input\_InputEvent\_), 
[Control.IsMouseOver](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_IsMouseOver), 
[Control.IsMousePressed](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_IsMousePressed), 
[Control.WantsMouse](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_WantsMouse), 
[Control.RendersOwnFocus](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_RendersOwnFocus), 
[Control.OnMouseEnter\(\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_OnMouseEnter), 
[Control.OnMouseLeave\(\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_OnMouseLeave), 
[Control.OnMouseMove\(Position\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_OnMouseMove\_ConsoleGUI\_Space\_Position\_), 
[Control.OnMousePress\(Position\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_OnMousePress\_ConsoleGUI\_Space\_Position\_), 
[Control.OnMouseRelease\(Position\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_OnMouseRelease\_ConsoleGUI\_Space\_Position\_), 
[Control.OnClick\(Position\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_OnClick\_ConsoleGUI\_Space\_Position\_), 
[Control.OnDoubleClick\(Position\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_OnDoubleClick\_ConsoleGUI\_Space\_Position\_), 
[Control.OnMouseWheel\(Position, int\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_OnMouseWheel\_ConsoleGUI\_Space\_Position\_System\_Int32\_), 
[Control.ScrollIntoView\(int, int\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_ScrollIntoView\_System\_Int32\_System\_Int32\_), 
[Control.CaptureMouse\(\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_CaptureMouse), 
[Control.ReleaseMouse\(\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_ReleaseMouse), 
[Control.OnPaste\(string\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_OnPaste\_System\_String\_), 
[Control.Dispose\(\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_Dispose), 
[Control.ApplyTheme\(\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_ApplyTheme), 
[Control.IsThemeOverridden\(string\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_IsThemeOverridden\_System\_String\_), 
[Control.Focus\(\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_Focus), 
[Control.UnFocus\(\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_UnFocus), 
[Control.GetHelpInfo\(\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_GetHelpInfo), 
[Control.CompileHelp\(\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_CompileHelp), 
[Control.Control\_OnInitialization\(\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_Control\_OnInitialization), 
[Control.Control\_OnLostFocus\(\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_Control\_OnLostFocus), 
[Control.Control\_OnFocus\(\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_Control\_OnFocus), 
[Control.Render\(\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_Render), 
[Control.Initialize\(\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_Initialize), 
[Control.Paint\(\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_Paint), 
[Control.Invalidate\(\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_Invalidate), 
[Control.InvalidateInteractive\(\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_InvalidateInteractive), 
[Control.TracksDamage](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_TracksDamage), 
[Control.Damage\(in Rect\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_Damage\_ConsoleGUI\_Space\_Rect\_\_), 
[Control.DamageAll\(\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_DamageAll), 
[Control.Feed\(Action, TimeSpan, Action<Exception\>?\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_Feed\_System\_Action\_System\_TimeSpan\_System\_Action\_System\_Exception\_\_), 
[Control.Feed\(Action, int, Action<Exception\>?\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_Feed\_System\_Action\_System\_Int32\_System\_Action\_System\_Exception\_\_), 
[Control.Feed<T\>\(Func<T\>, Action<T\>, TimeSpan, Action<Exception\>?\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_Feed\_\_1\_System\_Func\_\_\_0\_\_System\_Action\_\_\_0\_\_System\_TimeSpan\_System\_Action\_System\_Exception\_\_), 
[Control.Feed<T\>\(Func<T\>, Action<T\>, int, Action<Exception\>?\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_Feed\_\_1\_System\_Func\_\_\_0\_\_System\_Action\_\_\_0\_\_System\_Int32\_System\_Action\_System\_Exception\_\_), 
[Control.Feeds](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_Feeds), 
[Control.SetAtomicProperty<T\>\(ref T, T, bool, Func<T, T\>?, Action<T, T\>?, bool, string?\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_SetAtomicProperty\_\_1\_\_\_0\_\_\_\_0\_System\_Boolean\_System\_Func\_\_\_0\_\_\_0\_\_System\_Action\_\_\_0\_\_\_0\_\_System\_Boolean\_System\_String\_), 
[Control.Validate\(\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_Validate), 
[Control.CalculateSize\(\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_CalculateSize), 
[Control.IntrinsicWidth\(\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_IntrinsicWidth), 
[Control.IntrinsicHeight\(\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_IntrinsicHeight), 
[Control.ClampWidth\(int\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_ClampWidth\_System\_Int32\_), 
[Control.ClampHeight\(int\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_ClampHeight\_System\_Int32\_), 
[Control.OnInitialization](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_OnInitialization), 
[Control.OnFocus](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_OnFocus), 
[Control.OnLostFocus](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_OnLostFocus), 
[Control.OnHelp](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_OnHelp), 
[Control.MouseEntered](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_MouseEntered), 
[Control.MouseLeft](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_MouseLeft), 
[Control.MouseMoved](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_MouseMoved), 
[Control.MousePressed](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_MousePressed), 
[Control.MouseReleased](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_MouseReleased), 
[Control.Clicked](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_Clicked), 
[Control.DoubleClicked](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_DoubleClicked), 
[Control.MouseWheeled](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_MouseWheeled), 
[Control.emptyChar](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_emptyChar), 
[Control.emptyCell](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_emptyCell), 
[Control.paintRequests](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_paintRequests), 
[Control.consoleBuffer](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_consoleBuffer), 
[Control.ansiConsole](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_ansiConsole), 
[Control.DoubleClickMs](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_DoubleClickMs)

#### Extension Methods

[ControlExtensions.WithAsciiBorder<TerminalEmulator\>\(TerminalEmulator, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithAsciiBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithBorder<TerminalEmulator\>\(TerminalEmulator, BorderStyle?, Color?, Color?, BorderPlacement?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_BorderPlacement\_\_), 
[ControlExtensions.WithDoubleBorder<TerminalEmulator\>\(TerminalEmulator, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithDoubleBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithFrame<TerminalEmulator\>\(TerminalEmulator, ControlFrame\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithFrame\_\_1\_\_\_0\_Jumbee\_Console\_ControlFrame\_), 
[ControlExtensions.WithFrame<TerminalEmulator\>\(TerminalEmulator, BorderStyle?, Offset?, Color?, Color?, string?, Color?, Color?, BorderPlacement?, BorderStyle?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithFrame\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_System\_Nullable\_ConsoleGUI\_Space\_Offset\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_String\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_BorderPlacement\_\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_), 
[ControlExtensions.WithHeavyBorder<TerminalEmulator\>\(TerminalEmulator, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithHeavyBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithHeight<TerminalEmulator\>\(TerminalEmulator, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithHeight\_\_1\_\_\_0\_System\_Int32\_), 
[ControlExtensions.WithMargin<TerminalEmulator\>\(TerminalEmulator, int, int, int, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithMargin\_\_1\_\_\_0\_System\_Int32\_System\_Int32\_System\_Int32\_System\_Int32\_), 
[ControlExtensions.WithMargin<TerminalEmulator\>\(TerminalEmulator, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithMargin\_\_1\_\_\_0\_System\_Int32\_), 
[ControlExtensions.WithNoBorder<TerminalEmulator\>\(TerminalEmulator\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithNoBorder\_\_1\_\_\_0\_), 
[ControlExtensions.WithRoundedBorder<TerminalEmulator\>\(TerminalEmulator, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithRoundedBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithScrollBarGlyphs<TerminalEmulator\>\(TerminalEmulator, ScrollBarGlyphs\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithScrollBarGlyphs\_\_1\_\_\_0\_Jumbee\_Console\_ScrollBarGlyphs\_), 
[ControlExtensions.WithScrollBarStyle<TerminalEmulator\>\(TerminalEmulator, ScrollBarStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithScrollBarStyle\_\_1\_\_\_0\_Jumbee\_Console\_ScrollBarStyle\_), 
[ControlExtensions.WithSize<TerminalEmulator\>\(TerminalEmulator, int?, int?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithSize\_\_1\_\_\_0\_System\_Nullable\_System\_Int32\_\_System\_Nullable\_System\_Int32\_\_), 
[ControlExtensions.WithSquareBorder<TerminalEmulator\>\(TerminalEmulator, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithSquareBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithTitle<TerminalEmulator\>\(TerminalEmulator, string\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_), 
[ControlExtensions.WithTitle<TerminalEmulator\>\(TerminalEmulator, string, TitleStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_Jumbee\_Console\_TitleStyle\_), 
[ControlExtensions.WithTitle<TerminalEmulator\>\(TerminalEmulator, string, TitlePos, TitleBorderStyle, TitleColorStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_Jumbee\_Console\_TitlePos\_Jumbee\_Console\_TitleBorderStyle\_Jumbee\_Console\_TitleColorStyle\_), 
[ControlExtensions.WithWidth<TerminalEmulator\>\(TerminalEmulator, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithWidth\_\_1\_\_\_0\_System\_Int32\_)

## Constructors

### <a id="Jumbee_Console_TerminalEmulator__ctor_System_String_System_String_"></a> TerminalEmulator\(string?, string?\)

Creates a terminal. With a <code class="paramref">commandLine</code> the control launches that process in a pseudo
console once it is sized.

```csharp
public TerminalEmulator(string? commandLine = "cmd.exe", string? workingDirectory = null)
```

#### Parameters

`commandLine` string?

`workingDirectory` string?

#### Remarks

Pass <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> (or whitespace) to skip spawning and drive the emulator manually by pushing bytes
through <xref href="Jumbee.Console.TerminalEmulator.Feed(System.Byte%5b%5d)" data-throw-if-not-resolved="false"></xref> — useful for embedding an existing stream (e.g. an SSH channel) or for headless tests.

<p><code class="paramref">workingDirectory</code> sets the child process's initial directory (e.g. a project folder
so <code>dotnet build</code> resolves there); <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> inherits the host process's directory.</p>

## Properties

### <a id="Jumbee_Console_TerminalEmulator_DefaultBackground"></a> DefaultBackground

Background colour for cells the running program leaves at the terminal default (it didn't set an explicit
background).

```csharp
public Color? DefaultBackground { get; set; }
```

#### Property Value

 [Color](Jumbee.Console.Color.md)?

#### Remarks

Lets the emulator blend with the control/theme instead of painting default cells solid black.
<a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> (the default) paints them with no background, so the control's own background shows
through; set a colour to force a specific default background across the whole terminal area. Cells that carry
their own (non-default) background are unaffected.

### <a id="Jumbee_Console_TerminalEmulator_HandlesInput"></a> HandlesInput

Always <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>: the terminal consumes keyboard input to forward to the child process.

```csharp
public override bool HandlesInput { get; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_TerminalEmulator_IsRunning"></a> IsRunning

Whether a child process is currently attached and running.

```csharp
public bool IsRunning { get; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_TerminalEmulator_RendersOwnFocus"></a> RendersOwnFocus

Always <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>: the terminal's own cursor indicates focus, so no focus tint is drawn.

```csharp
protected override bool RendersOwnFocus { get; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_TerminalEmulator_WantsMouse"></a> WantsMouse

Always <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>: motion/hover as well as clicks reach the control so mouse tracking can be forwarded.

```csharp
protected override bool WantsMouse { get; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_TerminalEmulator_WindowTitle"></a> WindowTitle

The window title the running program set via OSC 0/2, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> if none. Hosts can
    bind this (or <xref href="Jumbee.Console.TerminalEmulator.TitleChanged" data-throw-if-not-resolved="false"></xref>) to a frame title.

```csharp
public string? WindowTitle { get; }
```

#### Property Value

 string?

## Methods

### <a id="Jumbee_Console_TerminalEmulator_Control_OnInitialization"></a> Control\_OnInitialization\(\)

Starts or resizes the PTY once the control has a real cell area.

```csharp
protected override void Control_OnInitialization()
```

### <a id="Jumbee_Console_TerminalEmulator_Dispose"></a> Dispose\(\)

Tears down the child process and PTY, then disposes the base control.

```csharp
public override void Dispose()
```

### <a id="Jumbee_Console_TerminalEmulator_Feed_System_Byte___"></a> Feed\(byte\[\]\)

Pushes raw terminal output bytes into the emulator and repaints.

```csharp
public void Feed(byte[] data)
```

#### Parameters

`data` byte\[\]

#### Remarks

Available for manually-driven terminals (see the <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a>-command-line constructor). Safe to
call from any thread — the work is marshaled onto the UI thread. (The PTY read loop uses the flow-controlled
path, not this.)

### <a id="Jumbee_Console_TerminalEmulator_OnInput_ConsoleGUI_Input_InputEvent_"></a> OnInput\(InputEvent\)

Translates keystrokes to bytes for the child process; Shift+PageUp/PageDown scroll the scrollback instead.

```csharp
protected override void OnInput(InputEvent inputEvent)
```

#### Parameters

`inputEvent` InputEvent

### <a id="Jumbee_Console_TerminalEmulator_OnMousePress_ConsoleGUI_Space_Position_"></a> OnMousePress\(Position\)

Forwards a left-button press to the child program when it is tracking the mouse.

```csharp
protected override void OnMousePress(Position position)
```

#### Parameters

`position` Position

### <a id="Jumbee_Console_TerminalEmulator_OnMouseRelease_ConsoleGUI_Space_Position_"></a> OnMouseRelease\(Position\)

Forwards a left-button release to the child program when it is tracking the mouse.

```csharp
protected override void OnMouseRelease(Position position)
```

#### Parameters

`position` Position

### <a id="Jumbee_Console_TerminalEmulator_OnMouseWheel_ConsoleGUI_Space_Position_System_Int32_"></a> OnMouseWheel\(Position, int\)

Forwards the wheel to the child program when it is tracking the mouse; otherwise scrolls the scrollback.

```csharp
protected override void OnMouseWheel(Position position, int delta)
```

#### Parameters

`position` Position

`delta` int

### <a id="Jumbee_Console_TerminalEmulator_OnPaste_System_String_"></a> OnPaste\(string\)

Sends pasted text to the child process, wrapping it in bracketed-paste markers when the program enabled DECSET 2004.

```csharp
public override void OnPaste(string text)
```

#### Parameters

`text` string

### <a id="Jumbee_Console_TerminalEmulator_Render"></a> Render\(\)

Paints the visible terminal screen (or scrollback view) and scrollbar to the buffer.

```csharp
protected override void Render()
```

### <a id="Jumbee_Console_TerminalEmulator_SendText_System_String_"></a> SendText\(string\)

Sends text to the process as if typed (UTF-8, no bracketed-paste wrapping).

```csharp
public void SendText(string text)
```

#### Parameters

`text` string

#### Remarks

Include a trailing <code>"\r"</code> to submit a line. Snaps the view back to the live bottom.

### <a id="Jumbee_Console_TerminalEmulator_StartProcess"></a> StartProcess\(\)

Requests that the child process run: starts it now if the terminal is sized and idle, otherwise it starts as
soon as the control is first laid out.

```csharp
public void StartProcess()
```

#### Remarks

Use to restart a shell previously ended with <xref href="Jumbee.Console.TerminalEmulator.StopProcess" data-throw-if-not-resolved="false"></xref> (e.g. a host that stops the shell while
the terminal is hidden). No-op for a manually-driven terminal (null command line) or when a process is already
running.

### <a id="Jumbee_Console_TerminalEmulator_StopProcess"></a> StopProcess\(\)

Stops the child process and its output reader, leaving the emulator screen as drawn.

```csharp
public void StopProcess()
```

#### Remarks

Restart with <xref href="Jumbee.Console.TerminalEmulator.StartProcess" data-throw-if-not-resolved="false"></xref>. Does not raise <xref href="Jumbee.Console.TerminalEmulator.Exited" data-throw-if-not-resolved="false"></xref> (a deliberate stop is not a
process exit the host should hear about). No-op if not running.

### <a id="Jumbee_Console_TerminalEmulator_TranslateKey_System_ConsoleKeyInfo_"></a> TranslateKey\(ConsoleKeyInfo\)

Translates a key event into the bytes to send the process, honoring the emulator's current modes
(application-cursor mode, keypad, …).

```csharp
public byte[]? TranslateKey(ConsoleKeyInfo key)
```

#### Parameters

`key` ConsoleKeyInfo

#### Returns

 byte\[\]?

#### Remarks

Navigation/function keys are mapped by VtNetCore so the sequences track those modes; Enter/Escape/printables
are handled here. Returns <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> for a key that produces no input.

### <a id="Jumbee_Console_TerminalEmulator_Exited"></a> Exited

Raised on the UI thread after the child process exits (never for a manually-driven terminal).

```csharp
public event EventHandler? Exited
```

#### Event Type

 EventHandler?

### <a id="Jumbee_Console_TerminalEmulator_TitleChanged"></a> TitleChanged

Raised on the UI thread when the running program changes the window title (OSC 0/2).

```csharp
public event EventHandler<string>? TitleChanged
```

#### Event Type

 EventHandler<string\>?

