# <a id="Jumbee_Console_Control"></a> Class Control

Namespace: [Jumbee.Console](Jumbee.Console.md)  
Assembly: Jumbee.Console.dll  

Base class for all Jumbee.Console controls.

```csharp
public abstract class Control : Control, IFocusable
```

#### Inheritance

object ← 
Control ← 
[Control](Jumbee.Console.Control.md)

#### Derived

[AnimatedControl](Jumbee.Console.AnimatedControl.md), 
[AsciiDocViewer](Jumbee.Console.Documents.AsciiDocViewer.md), 
[Canvas](Jumbee.Console.Canvas.md), 
[CompositeControl](Jumbee.Console.CompositeControl.md), 
[ContextMenu](Jumbee.Console.ContextMenu.md), 
[DataTable](Jumbee.Console.DataTable.md), 
[GlassPanel](Jumbee.Console.GlassPanel.md), 
[Globe](Jumbee.Console.Globe.md), 
[Log](Jumbee.Console.Log.md), 
[MarkdownViewer](Jumbee.Console.MarkdownViewer.md), 
[MermaidViewer](Jumbee.Console.Documents.MermaidViewer.md), 
[Plot](Jumbee.Console.Plot.md), 
[Prompt](Jumbee.Console.Prompt.md), 
[RenderableControl](Jumbee.Console.RenderableControl.md), 
[SpectreLiveDisplay](Jumbee.Console.SpectreLiveDisplay.md), 
[SpectreTaskProgress](Jumbee.Console.SpectreTaskProgress.md), 
[TerminalEmulator](Jumbee.Console.TerminalEmulator.md), 
[TextEditor](Jumbee.Console.TextEditor.md), 
[TextInput](Jumbee.Console.TextInput.md), 
[TextLabel](Jumbee.Console.TextLabel.md)

#### Implements

[IFocusable](Jumbee.Console.IFocusable.md)

#### Extension Methods

[ControlExtensions.WithAsciiBorder<Control\>\(Control, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithAsciiBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithBorder<Control\>\(Control, BorderStyle?, Color?, Color?, BorderPlacement?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_BorderPlacement\_\_), 
[ControlExtensions.WithDoubleBorder<Control\>\(Control, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithDoubleBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithFrame<Control\>\(Control, ControlFrame\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithFrame\_\_1\_\_\_0\_Jumbee\_Console\_ControlFrame\_), 
[ControlExtensions.WithFrame<Control\>\(Control, BorderStyle?, Offset?, Color?, Color?, string?, Color?, Color?, BorderPlacement?, BorderStyle?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithFrame\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_System\_Nullable\_ConsoleGUI\_Space\_Offset\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_String\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_BorderPlacement\_\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_), 
[ControlExtensions.WithHeavyBorder<Control\>\(Control, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithHeavyBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithHeight<Control\>\(Control, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithHeight\_\_1\_\_\_0\_System\_Int32\_), 
[ControlExtensions.WithMargin<Control\>\(Control, int, int, int, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithMargin\_\_1\_\_\_0\_System\_Int32\_System\_Int32\_System\_Int32\_System\_Int32\_), 
[ControlExtensions.WithMargin<Control\>\(Control, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithMargin\_\_1\_\_\_0\_System\_Int32\_), 
[ControlExtensions.WithNoBorder<Control\>\(Control\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithNoBorder\_\_1\_\_\_0\_), 
[ControlExtensions.WithRoundedBorder<Control\>\(Control, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithRoundedBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithScrollBarGlyphs<Control\>\(Control, ScrollBarGlyphs\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithScrollBarGlyphs\_\_1\_\_\_0\_Jumbee\_Console\_ScrollBarGlyphs\_), 
[ControlExtensions.WithScrollBarStyle<Control\>\(Control, ScrollBarStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithScrollBarStyle\_\_1\_\_\_0\_Jumbee\_Console\_ScrollBarStyle\_), 
[ControlExtensions.WithSize<Control\>\(Control, int?, int?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithSize\_\_1\_\_\_0\_System\_Nullable\_System\_Int32\_\_System\_Nullable\_System\_Int32\_\_), 
[ControlExtensions.WithSquareBorder<Control\>\(Control, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithSquareBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithTitle<Control\>\(Control, string\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_), 
[ControlExtensions.WithTitle<Control\>\(Control, string, TitleStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_Jumbee\_Console\_TitleStyle\_), 
[ControlExtensions.WithTitle<Control\>\(Control, string, TitlePos, TitleBorderStyle, TitleColorStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_Jumbee\_Console\_TitlePos\_Jumbee\_Console\_TitleBorderStyle\_Jumbee\_Console\_TitleColorStyle\_), 
[ControlExtensions.WithWidth<Control\>\(Control, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithWidth\_\_1\_\_\_0\_System\_Int32\_)

## Constructors

### <a id="Jumbee_Console_Control__ctor"></a> Control\(\)

Initializes a new <xref href="Jumbee.Console.Control" data-throw-if-not-resolved="false"></xref>, creating its render buffers and wiring up paint, theme, focus, and initialization handlers.

```csharp
public Control()
```

## Fields

### <a id="Jumbee_Console_Control_DoubleClickMs"></a> DoubleClickMs

Maximum gap (ms) between two clicks for them to register as a double-click.

```csharp
protected const long DoubleClickMs = 400
```

#### Field Value

 long

### <a id="Jumbee_Console_Control_ansiConsole"></a> ansiConsole

The Spectre.Console <xref href="Jumbee.Console.AnsiConsoleBuffer" data-throw-if-not-resolved="false"></xref> that writes styled output into <xref href="Jumbee.Console.Control.consoleBuffer" data-throw-if-not-resolved="false"></xref>.

```csharp
protected readonly AnsiConsoleBuffer ansiConsole
```

#### Field Value

 [AnsiConsoleBuffer](Jumbee.Console.AnsiConsoleBuffer.md)

### <a id="Jumbee_Console_Control_consoleBuffer"></a> consoleBuffer

The buffer the control renders its cells into.

```csharp
protected readonly ConsoleBuffer consoleBuffer
```

#### Field Value

 [ConsoleBuffer](Jumbee.Console.ConsoleBuffer.md)

### <a id="Jumbee_Console_Control_emptyCell"></a> emptyCell

A shared empty <xref href="ConsoleGUI.Data.Cell" data-throw-if-not-resolved="false"></xref>, returned for positions outside the control's size.

```csharp
protected static readonly Cell emptyCell
```

#### Field Value

 Cell

### <a id="Jumbee_Console_Control_emptyChar"></a> emptyChar

A shared empty <xref href="ConsoleGUI.Data.Character" data-throw-if-not-resolved="false"></xref>.

```csharp
public static Character emptyChar
```

#### Field Value

 Character

### <a id="Jumbee_Console_Control_paintRequests"></a> paintRequests

Count of pending paint requests; a non-zero value triggers a repaint on the next paint tick.

```csharp
protected uint paintRequests
```

#### Field Value

 uint

## Properties

### <a id="Jumbee_Console_Control_ActualHeight"></a> ActualHeight

The control's actual laid-out height in cells.

```csharp
public int ActualHeight { get; }
```

#### Property Value

 int

### <a id="Jumbee_Console_Control_ActualWidth"></a> ActualWidth

The control's actual laid-out width in cells.

```csharp
public int ActualWidth { get; }
```

#### Property Value

 int

### <a id="Jumbee_Console_Control_Feeds"></a> Feeds

A thread-safe snapshot of this control's currently live feed handles (each <xref href="Jumbee.Console.Control.Feed(System.Action%2cSystem.Int32%2cSystem.Action%7bSystem.Exception%7d)" data-throw-if-not-resolved="false"></xref> call
registers one; it self-unregisters when cancelled or completed).

```csharp
protected IReadOnlyList<CancellationTokenSource> Feeds { get; }
```

#### Property Value

 IReadOnlyList<CancellationTokenSource\>

#### Remarks

Cancelling these stops the feeds without disposing the control — handy for pausing background work while the
control is hidden. Feeds are also cancelled automatically on <xref href="Jumbee.Console.Control.Dispose" data-throw-if-not-resolved="false"></xref>. Returns a copy, so
iterating it never races the background feed threads.

### <a id="Jumbee_Console_Control_Focusable"></a> Focusable

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a> (the default), this control can receive keyboard focus.

```csharp
public bool Focusable { get; set; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_Control_FocusableControl"></a> FocusableControl

The focus target the UI registers for this control — its <xref href="Jumbee.Console.Control.Frame" data-throw-if-not-resolved="false"></xref> when framed (so the frame handles input routing), otherwise the control itself.

```csharp
public IFocusable FocusableControl { get; }
```

#### Property Value

 [IFocusable](Jumbee.Console.IFocusable.md)

### <a id="Jumbee_Console_Control_FocusedControl"></a> FocusedControl

The focused control within this one — itself (<em>not</em> its <xref href="Jumbee.Console.Control.FocusableControl" data-throw-if-not-resolved="false"></xref>/frame, so a
frame forwarding input inward doesn't loop back to itself) when focused, otherwise <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a>.

```csharp
public virtual IFocusable? FocusedControl { get; }
```

#### Property Value

 [IFocusable](Jumbee.Console.IFocusable.md)?

#### Remarks

A composite (<xref href="Jumbee.Console.CompositeControl" data-throw-if-not-resolved="false"></xref>) overrides this to return its focused descendant, letting
keyboard input route through the composite to the right child.

### <a id="Jumbee_Console_Control_Frame"></a> Frame

The optional <xref href="Jumbee.Console.ControlFrame" data-throw-if-not-resolved="false"></xref> drawing borders, margins, scrollbars, and a titlebar around this control, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a>.

```csharp
public ControlFrame? Frame { get; set; }
```

#### Property Value

 [ControlFrame](Jumbee.Console.ControlFrame.md)?

### <a id="Jumbee_Console_Control_HandlesInput"></a> HandlesInput

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, this control processes keyboard input dispatched to it; the default (<a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a>) ignores it.

```csharp
public virtual bool HandlesInput { get; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_Control_HasFrame"></a> HasFrame

<a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a> when this control has a <xref href="Jumbee.Console.Control.Frame" data-throw-if-not-resolved="false"></xref>.

```csharp
public bool HasFrame { get; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_Control_HasLayout"></a> HasLayout

<a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a> once the control has a non-empty laid-out size.

```csharp
public bool HasLayout { get; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_Control_Height"></a> Height

The requested height in cells; setting it resizes the control. 0 (the default) fills the space the parent offers.

```csharp
public virtual int Height { get; set; }
```

#### Property Value

 int

#### Remarks

See <xref href="Jumbee.Console.Control.Width" data-throw-if-not-resolved="false"></xref> — same reasoning; this is the axis the bug was found on.

### <a id="Jumbee_Console_Control_IsFocused"></a> IsFocused

Whether this control currently holds keyboard focus; setting it raises the focus events and repaints so the terminal cursor moves.

```csharp
public bool IsFocused { get; set; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_Control_IsMouseOver"></a> IsMouseOver

<a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a> while the pointer is over this control (between enter and leave).

```csharp
protected bool IsMouseOver { get; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_Control_IsMousePressed"></a> IsMousePressed

<a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a> while a press started on this control and has not yet been released.

```csharp
protected bool IsMousePressed { get; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_Control_RendersOwnFocus"></a> RendersOwnFocus

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, this control indicates keyboard focus in its own way (e.g. a button's fill
change, a tab's underline, an editor's cursor), so the base class does <em>not</em> paint the themed default
focus tint over it.

```csharp
protected virtual bool RendersOwnFocus { get; }
```

#### Property Value

 bool

#### Remarks

Override and return <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a> on controls with their own focus styling; the
default (<a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a>) gives unstyled focusable controls an automatic, always-visible focus cue.

### <a id="Jumbee_Console_Control_TracksDamage"></a> TracksDamage

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, this control reports only the sub-rect(s) it changed each paint — accumulated via
<xref href="Jumbee.Console.Control.Damage(ConsoleGUI.Space.Rect%40)" data-throw-if-not-resolved="false"></xref> during <xref href="Jumbee.Console.Control.Render" data-throw-if-not-resolved="false"></xref> — instead of its whole area, so the compositor skips
the unchanged remainder. Default <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a> (report the full rect every paint, as before).

```csharp
protected virtual bool TracksDamage { get; }
```

#### Property Value

 bool

#### Remarks

<p>
A control that opts in MUST report every changed cell; over-reporting is safe, under-reporting drops updates.
</p>
<p>
Opt in only when the control has a <em>large drawing area of which little changes per frame</em> — a live
trace, a moving sprite, a marker set over a static backdrop. It is not a free win, for two reasons. The scan
it replaces is a <em>linear</em> walk of the double buffer, the friendliest access pattern there is, whereas
damage bookkeeping is scattered by nature (save the previous value on first touch, then flush a list of
touched indices); a cell avoided is therefore cheaper than a cell tracked, so trading N scanned cells for N
tracked ones is a net loss and only a large ratio wins. And the recorder sits in the write path, so every
cell write pays for it whether or not the frame benefits. A control that redraws its whole area every frame
should leave this off — there the bookkeeping is pure overhead.
</p>
<p>
It also does not reduce terminal output. The renderer already diffs per cell before emitting, so the escape
sequences sent are decided by which cells genuinely differ, never by which rects were declared damaged.
Damage narrows what is <em>scanned</em>, never what is <em>sent</em>. Measure rather than assume; see
"When damage tracking pays" in <code>docs/internal/Rendering Model.md</code>.
</p>

### <a id="Jumbee_Console_Control_WantsMouse"></a> WantsMouse

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, the control's cells are tagged with a mouse listener even if it is not
<xref href="Jumbee.Console.Control.Focusable" data-throw-if-not-resolved="false"></xref>, so it still receives hover/click (e.g. a non-focusable clickable Link).

```csharp
protected virtual bool WantsMouse { get; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_Control_Width"></a> Width

The requested width in cells; setting it resizes the control. 0 (the default) fills the space the parent offers.

```csharp
public virtual int Width { get; set; }
```

#### Property Value

 int

#### Remarks

Goes through <code>Initialize</code> rather than resizing directly, and that matters for anything with content.
A bare <code>Resize</code> changes <code>Size</code> but skips everything <code>Initialize</code> does around it: the console
buffer is never re-sized (so the control reports an area its own buffer cannot serve), and
<code>OnInitialization</code> never fires — which is what tells a <xref href="Jumbee.Console.CompositeControl" data-throw-if-not-resolved="false"></xref> to re-lay-out its
content. A composite whose Height was set at runtime therefore kept its children at the OLD size
indefinitely, drawing a stale picture into a correctly-sized control until some unrelated event forced a
real layout pass.

<p>
It also fixes a second-order bug in the old form: <code>Resize(new Size(value, Height))</code> paired the new
width with the <em>requested</em> Height, which is 0 for the common "fill the parent" case — so setting a
Width could collapse the control's height to nothing. <code>CalculateSize</code> resolves both axes properly.
</p>

### <a id="Jumbee_Console_Control_Item_ConsoleGUI_Space_Position_"></a> this\[Position\]

Gets the composited <xref href="ConsoleGUI.Data.Cell" data-throw-if-not-resolved="false"></xref> at <code class="paramref">position</code>, applying the default focus cue and attaching this control as the cell's mouse listener where applicable.

```csharp
public override Cell this[Position position] { get; }
```

#### Property Value

 Cell

## Methods

### <a id="Jumbee_Console_Control_ApplyTheme"></a> ApplyTheme\(\)

Re-captures this control's themed colours/glyphs from the current <xref href="Jumbee.Console.UI.StyleTheme" data-throw-if-not-resolved="false"></xref>/
<xref href="Jumbee.Console.UI.GlyphTheme" data-throw-if-not-resolved="false"></xref>. The default is a no-op for controls that don't use the theme.

```csharp
protected virtual void ApplyTheme()
```

#### Remarks

Called by themed controls from their constructor and again on a runtime theme switch (<xref href="Jumbee.Console.UI.SetTheme(Jumbee.Console.IStyleTheme%2cJumbee.Console.IGlyphTheme)" data-throw-if-not-resolved="false"></xref>).
Must read the themes <em>only here</em> (and in the constructor), never on the render path.

### <a id="Jumbee_Console_Control_CalculateSize"></a> CalculateSize\(\)

Calculates the size of the control based on its own dimensions and the maximum and minimum size constraints set by its parent.

```csharp
protected (int, int) CalculateSize()
```

#### Returns

 \(int, int\)

### <a id="Jumbee_Console_Control_CaptureMouse"></a> CaptureMouse\(\)

Grabs the mouse so this control receives all subsequent move/press/release (in its own frame) until
<xref href="Jumbee.Console.Control.ReleaseMouse" data-throw-if-not-resolved="false"></xref>, even when the pointer leaves its cells — for drags (a splitter divider, a
scrollbar thumb, a slider). Call from <xref href="Jumbee.Console.Control.OnMousePress(ConsoleGUI.Space.Position)" data-throw-if-not-resolved="false"></xref>; pair with <xref href="Jumbee.Console.Control.ReleaseMouse" data-throw-if-not-resolved="false"></xref> in
<xref href="Jumbee.Console.Control.OnMouseRelease(ConsoleGUI.Space.Position)" data-throw-if-not-resolved="false"></xref>.

```csharp
protected void CaptureMouse()
```

### <a id="Jumbee_Console_Control_ClampHeight_System_Int32_"></a> ClampHeight\(int\)

Clamps <code class="paramref">height</code> to the range [0, this control's height].

```csharp
public int ClampHeight(int height)
```

#### Parameters

`height` int

#### Returns

 int

### <a id="Jumbee_Console_Control_ClampWidth_System_Int32_"></a> ClampWidth\(int\)

Clamps <code class="paramref">width</code> to the range [0, this control's width].

```csharp
public int ClampWidth(int width)
```

#### Parameters

`width` int

#### Returns

 int

### <a id="Jumbee_Console_Control_CompileHelp"></a> CompileHelp\(\)

The effective help for this control: <xref href="Jumbee.Console.Control.GetHelpInfo" data-throw-if-not-resolved="false"></xref> with any <xref href="Jumbee.Console.Control.OnHelp" data-throw-if-not-resolved="false"></xref>
    handlers applied (they mutate it in place, and may supply help even when <xref href="Jumbee.Console.Control.GetHelpInfo" data-throw-if-not-resolved="false"></xref> returned
    <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> — a blank entry named after the type is created first). <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> when the
    control has no help.

```csharp
public HelpInfo? CompileHelp()
```

#### Returns

 [HelpInfo](Jumbee.Console.HelpInfo.md)?

### <a id="Jumbee_Console_Control_Control_OnFocus"></a> Control\_OnFocus\(\)

Called when this control gains focus; override to react. The default is a no-op.

```csharp
protected virtual void Control_OnFocus()
```

### <a id="Jumbee_Console_Control_Control_OnInitialization"></a> Control\_OnInitialization\(\)

Fired when a control's Initialize method is called.

```csharp
protected virtual void Control_OnInitialization()
```

#### Remarks

This method is always called inside UI.Invoke.

### <a id="Jumbee_Console_Control_Control_OnLostFocus"></a> Control\_OnLostFocus\(\)

Called when this control loses focus; override to react. The default is a no-op.

```csharp
protected virtual void Control_OnLostFocus()
```

### <a id="Jumbee_Console_Control_Damage_ConsoleGUI_Space_Rect__"></a> Damage\(in Rect\)

Records a screen region (in this control's local coordinates) changed by the current paint.

```csharp
protected void Damage(in Rect rect)
```

#### Parameters

`rect` Rect

#### Remarks

Clipped to the control; empty rects are ignored. Call from <xref href="Jumbee.Console.Control.Render" data-throw-if-not-resolved="false"></xref>. No effect unless
<xref href="Jumbee.Console.Control.TracksDamage" data-throw-if-not-resolved="false"></xref> is <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>.

### <a id="Jumbee_Console_Control_DamageAll"></a> DamageAll\(\)

Records the whole control as damaged — use on the first paint or after a full-content change so the
    entire area is re-composited. See <xref href="Jumbee.Console.Control.Damage(ConsoleGUI.Space.Rect%40)" data-throw-if-not-resolved="false"></xref>.

```csharp
protected void DamageAll()
```

### <a id="Jumbee_Console_Control_Dispose"></a> Dispose\(\)

Cancels any live feeds and detaches the control's paint, theme, and frame event handlers.

```csharp
public virtual void Dispose()
```

### <a id="Jumbee_Console_Control_Feed_System_Action_System_TimeSpan_System_Action_System_Exception__"></a> Feed\(Action, TimeSpan, Action<Exception\>?\)

Starts a repeating feed that runs <code class="paramref">tick</code> <b>on the UI thread</b> every <code class="paramref">interval</code>
— for animations and periodic UI updates, without hand-rolling a timer loop.

```csharp
protected FeedHandle Feed(Action tick, TimeSpan interval, Action<Exception>? onError = null)
```

#### Parameters

`tick` Action

`interval` TimeSpan

`onError` Action<Exception\>?

#### Returns

 [FeedHandle](Jumbee.Console.FeedHandle.md)

#### Remarks

    The returned <xref href="Jumbee.Console.FeedHandle" data-throw-if-not-resolved="false"></xref> stops the feed when cancelled/disposed; the control also cancels every
    live feed when it is <xref href="Jumbee.Console.Control.Dispose?text=disposed" data-throw-if-not-resolved="false"></xref>. Await <xref href="Jumbee.Console.FeedHandle.Completion" data-throw-if-not-resolved="false"></xref> (or
    <xref href="Jumbee.Console.FeedHandle.StopAsync" data-throw-if-not-resolved="false"></xref>) to join the in-flight tick before tearing down a resource it uses. The
    first tick fires after one interval.

    <p></p><code class="paramref">tick</code> <b>always</b> runs on the UI thread — it may read and mutate control state directly (no
    marshaling), but it also means heavy work in it runs at frame start and delays the frame. For a tick that needs
    expensive <em>off-thread</em> work, use the <xref href="Jumbee.Console.Control.Feed%60%601(System.Func%7b%60%600%7d%2cSystem.Action%7b%60%600%7d%2cSystem.TimeSpan%2cSystem.Action%7bSystem.Exception%7d)" data-throw-if-not-resolved="false"></xref> overload
    instead, which runs the work on a background thread and only applies the result on the UI thread.

    <p></p>
Implementation note: the tick is delivered via <xref href="Jumbee.Console.UI.Post(System.Action)" data-throw-if-not-resolved="false"></xref> (not a direct call) so its redraw
    request and state change land together in the same dispatcher drain, before that frame's paint — see the note
    on <xref href="Jumbee.Console.UI.Post(System.Action)" data-throw-if-not-resolved="false"></xref> vs <xref href="Jumbee.Console.UI.Invoke(System.Action)" data-throw-if-not-resolved="false"></xref>.

### <a id="Jumbee_Console_Control_Feed_System_Action_System_Int32_System_Action_System_Exception__"></a> Feed\(Action, int, Action<Exception\>?\)

Convenience overload taking the interval in milliseconds. See <xref href="Jumbee.Console.Control.Feed(System.Action%2cSystem.TimeSpan%2cSystem.Action%7bSystem.Exception%7d)" data-throw-if-not-resolved="false"></xref>.

```csharp
protected FeedHandle Feed(Action tick, int intervalMs, Action<Exception>? onError = null)
```

#### Parameters

`tick` Action

`intervalMs` int

`onError` Action<Exception\>?

#### Returns

 [FeedHandle](Jumbee.Console.FeedHandle.md)

### <a id="Jumbee_Console_Control_Feed__1_System_Func___0__System_Action___0__System_TimeSpan_System_Action_System_Exception__"></a> Feed<T\>\(Func<T\>, Action<T\>, TimeSpan, Action<Exception\>?\)

A producer/consumer feed: every <code class="paramref">interval</code>, <code class="paramref">produce</code> runs on the feed's
<b>background thread</b> and its result is posted to <code class="paramref">apply</code> on the <b>UI thread</b>.

```csharp
protected FeedHandle Feed<T>(Func<T> produce, Action<T> apply, TimeSpan interval, Action<Exception>? onError = null)
```

#### Parameters

`produce` Func<T\>

`apply` Action<T\>

`interval` TimeSpan

`onError` Action<Exception\>?

#### Returns

 [FeedHandle](Jumbee.Console.FeedHandle.md)

#### Type Parameters

`T` 

#### Remarks

    Use this when each tick needs expensive off-thread work (querying the OS, hitting the network, heavy
    computation) whose result should update the control — only the cheap <code class="paramref">apply</code> touches the UI
    thread, so the frame isn't blocked. Cancellation and disposal behave as in <xref href="Jumbee.Console.Control.Feed(System.Action%2cSystem.TimeSpan%2cSystem.Action%7bSystem.Exception%7d)" data-throw-if-not-resolved="false"></xref>.

    <p></p>
If <code class="paramref">produce</code> throws, the feed stops and — when <code class="paramref">onError</code> is supplied —
    the exception is marshaled to it via <xref href="Jumbee.Console.UI.Post(System.Action)" data-throw-if-not-resolved="false"></xref> (so it runs on the UI thread and can surface the
    failure as visible state); without <code class="paramref">onError</code> the throw silently ends the feed. A throw in
    <code class="paramref">apply</code> is not caught here (it also runs on the UI thread via <xref href="Jumbee.Console.UI.Post(System.Action)" data-throw-if-not-resolved="false"></xref>). Because
    the callback is posted, it is delivered only while a <xref href="Jumbee.Console.UI.Start(Jumbee.Console.ILayout%2cSystem.Int32%2cSystem.Int32%2cSystem.Int32%2cSystem.Boolean%2cConsoleGUI.Api.IConsole%2cJumbee.Console.IInputSource%2cSystem.Boolean)" data-throw-if-not-resolved="false"></xref> loop is running to drain the queue —
    a headless test must run a real UI loop to observe it (<code>ConsoleSnapshot</code>/<code>UI.PaintFrame</code> paint without
    draining posted work).

### <a id="Jumbee_Console_Control_Feed__1_System_Func___0__System_Action___0__System_Int32_System_Action_System_Exception__"></a> Feed<T\>\(Func<T\>, Action<T\>, int, Action<Exception\>?\)

Convenience overload taking the interval in milliseconds. See <xref href="Jumbee.Console.Control.Feed%60%601(System.Func%7b%60%600%7d%2cSystem.Action%7b%60%600%7d%2cSystem.TimeSpan%2cSystem.Action%7bSystem.Exception%7d)" data-throw-if-not-resolved="false"></xref>.

```csharp
protected FeedHandle Feed<T>(Func<T> produce, Action<T> apply, int intervalMs, Action<Exception>? onError = null)
```

#### Parameters

`produce` Func<T\>

`apply` Action<T\>

`intervalMs` int

`onError` Action<Exception\>?

#### Returns

 [FeedHandle](Jumbee.Console.FeedHandle.md)

#### Type Parameters

`T` 

### <a id="Jumbee_Console_Control_Focus"></a> Focus\(\)

Moves keyboard focus to this control exclusively (via <xref href="Jumbee.Console.UI.SetFocus(Jumbee.Console.IFocusable)" data-throw-if-not-resolved="false"></xref>), clearing focus from any other control.

```csharp
public void Focus()
```

### <a id="Jumbee_Console_Control_GetHelpInfo"></a> GetHelpInfo\(\)

The help shown for this control in the global help dialog (F1), or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> for no help.

```csharp
protected virtual HelpInfo? GetHelpInfo()
```

#### Returns

 [HelpInfo](Jumbee.Console.HelpInfo.md)?

#### Remarks

Override to describe the control and its keys. The result is deduplicated across the UI by
<xref href="Jumbee.Console.HelpInfo.Name" data-throw-if-not-resolved="false"></xref>, so give controls of the same kind the same name. <xref href="Jumbee.Console.Control.OnHelp" data-throw-if-not-resolved="false"></xref> handlers
can further modify (or create) it.

### <a id="Jumbee_Console_Control_Initialize"></a> Initialize\(\)

Lays the control out on the UI thread: computes and applies its size, sizes the buffer, invalidates, and raises <xref href="Jumbee.Console.Control.OnInitialization" data-throw-if-not-resolved="false"></xref>.

```csharp
protected override void Initialize()
```

### <a id="Jumbee_Console_Control_IntrinsicHeight"></a> IntrinsicHeight\(\)

The intrinsic, fixed height counterpart of <xref href="Jumbee.Console.Control.IntrinsicWidth" data-throw-if-not-resolved="false"></xref> (e.g. a horizontal
    <xref href="Jumbee.Console.TextLabel" data-throw-if-not-resolved="false"></xref> is one row tall). Returns 0 to fill the parent's height (the default).

```csharp
protected virtual int IntrinsicHeight()
```

#### Returns

 int

### <a id="Jumbee_Console_Control_IntrinsicWidth"></a> IntrinsicWidth\(\)

An intrinsic, fixed width in cells this control always wants regardless of the space its parent offers, or
0 (the default) to fill the parent's width.

```csharp
protected virtual int IntrinsicWidth()
```

#### Returns

 int

#### Remarks

Unlike <xref href="Jumbee.Console.IScrollable.MeasureHeight(System.Int32)" data-throw-if-not-resolved="false"></xref> — a content height honored only when the parent is unbounded —
an intrinsic size is authoritative even under a finite parent. Override on adornment controls with a genuine
fixed extent (e.g. a vertical <xref href="Jumbee.Console.TextLabel" data-throw-if-not-resolved="false"></xref>, one column wide) so a docking/layout parent can't
stretch them to fill the region.

### <a id="Jumbee_Console_Control_Invalidate"></a> Invalidate\(\)

Indicates the control should be repainted on the next UI update tick.

```csharp
protected virtual void Invalidate()
```

### <a id="Jumbee_Console_Control_InvalidateInteractive"></a> InvalidateInteractive\(\)

Requests a repaint in response to an <em>interactive-state</em> change (focus gained/lost, mouse
enter/leave/press/release) rather than a content change.

```csharp
protected virtual void InvalidateInteractive()
```

#### Remarks

Defaults to <xref href="Jumbee.Console.Control.Invalidate" data-throw-if-not-resolved="false"></xref>, so controls behave exactly as before. <xref href="Jumbee.Console.RenderableControl" data-throw-if-not-resolved="false"></xref>
overrides this to skip the (expensive) re-render of its wrapped renderable when that renderable's output
does not depend on interactive state.

### <a id="Jumbee_Console_Control_IsThemeOverridden_System_String_"></a> IsThemeOverridden\(string\)

<a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a> if <code class="paramref">property</code> was explicitly set by the caller (a themeable token
setter passed it to <xref href="Jumbee.Console.Control.SetAtomicProperty%60%601(%60%600%40%2c%60%600%2cSystem.Boolean%2cSystem.Func%7b%60%600%2c%60%600%7d%2cSystem.Action%7b%60%600%2c%60%600%7d%2cSystem.Boolean%2cSystem.String)" data-throw-if-not-resolved="false"></xref>).

```csharp
protected bool IsThemeOverridden(string property)
```

#### Parameters

`property` string

#### Returns

 bool

#### Remarks

A control's <xref href="Jumbee.Console.Control.ApplyTheme" data-throw-if-not-resolved="false"></xref> guards each themed field with this so a runtime theme switch re-themes
only the properties the caller left at default.

### <a id="Jumbee_Console_Control_Job__1_System_Func___0__System_Action___0__System_Action_System_Exception__"></a> Job<T\>\(Func<T\>, Action<T\>, Action<Exception\>?\)

Starts an <b>on-demand</b> background job: each <xref href="Jumbee.Console.JobHandle.Request" data-throw-if-not-resolved="false"></xref> runs <code class="paramref">produce</code>
on a background thread and posts its result to <code class="paramref">apply</code> on the UI thread.

```csharp
protected JobHandle Job<T>(Func<T> produce, Action<T> apply, Action<Exception>? onError = null)
```

#### Parameters

`produce` Func<T\>

`apply` Action<T\>

`onError` Action<Exception\>?

#### Returns

 [JobHandle](Jumbee.Console.JobHandle.md)

#### Type Parameters

`T` 

#### Remarks

<p>
The on-demand counterpart to <xref href="Jumbee.Console.Control.Feed%60%601(System.Func%7b%60%600%7d%2cSystem.Action%7b%60%600%7d%2cSystem.TimeSpan%2cSystem.Action%7bSystem.Exception%7d)" data-throw-if-not-resolved="false"></xref>. A feed
asks "what is the value now?" every N milliseconds; a job asks "the state changed — produce the new content
when you can". Use it when a control's content is <b>expensive to compute and cheap to display</b>: a chart
over a large series, a rasterised 3D scene, a laid-out document. The UI thread requests and paints; the work
happens elsewhere.
</p>
<p>
<b>Runs never overlap and requests coalesce.</b> At most one <code class="paramref">produce</code> is in flight and at
most one more is queued, however many times <xref href="Jumbee.Console.JobHandle.Request" data-throw-if-not-resolved="false"></xref> is called — so a producer slower
than its callers falls behind by a bounded amount instead of accumulating a backlog of stale work. This is
what makes it a render queue rather than a work queue: the goal is the newest state on screen soon, not every
intermediate state eventually. <xref href="Jumbee.Console.JobHandle.Coalesced" data-throw-if-not-resolved="false"></xref> counts how often that happened.
</p>
<p>
<b>The producer must not touch UI state.</b> It runs off the UI thread while the UI thread is free to paint,
lay out and handle input, so anything it reads has to be either immutable or private to it — capture what it
needs at <xref href="Jumbee.Console.JobHandle.Request" data-throw-if-not-resolved="false"></xref> time and hand it over, or have the producer write into a buffer the
paint path does not read until <code class="paramref">apply</code> publishes it. Reading a live control property from
<code class="paramref">produce</code> is a race, and the usual symptom is a torn frame rather than an exception.
</p>
<p>
Cancellation and disposal behave as in <xref href="Jumbee.Console.Control.Feed(System.Action%2cSystem.TimeSpan%2cSystem.Action%7bSystem.Exception%7d)" data-throw-if-not-resolved="false"></xref>: the control
cancels every live job when disposed, and a throwing producer ends the job, surfacing to
<code class="paramref">onError</code> on the UI thread when one is supplied. Like a feed, <code class="paramref">apply</code> is
delivered by <xref href="Jumbee.Console.UI.Post(System.Action)" data-throw-if-not-resolved="false"></xref>, so it only arrives while a <xref href="Jumbee.Console.UI.Start(Jumbee.Console.ILayout%2cSystem.Int32%2cSystem.Int32%2cSystem.Int32%2cSystem.Boolean%2cConsoleGUI.Api.IConsole%2cJumbee.Console.IInputSource%2cSystem.Boolean)" data-throw-if-not-resolved="false"></xref> loop is draining the
queue — a headless test must run a real UI loop to observe it.
</p>

### <a id="Jumbee_Console_Control_OnClick_ConsoleGUI_Space_Position_"></a> OnClick\(Position\)

Called on a press+release on this control (relative position).

```csharp
protected virtual void OnClick(Position position)
```

#### Parameters

`position` Position

### <a id="Jumbee_Console_Control_OnDoubleClick_ConsoleGUI_Space_Position_"></a> OnDoubleClick\(Position\)

Called on two clicks within <xref href="Jumbee.Console.Control.DoubleClickMs" data-throw-if-not-resolved="false"></xref> at the same position.

```csharp
protected virtual void OnDoubleClick(Position position)
```

#### Parameters

`position` Position

### <a id="Jumbee_Console_Control_OnInput_Jumbee_Console_UI_InputEventArgs_"></a> OnInput\(InputEventArgs\)

Dispatches a UI input event to <xref href="Jumbee.Console.Control.OnInput(ConsoleGUI.Input.InputEvent)" data-throw-if-not-resolved="false"></xref> when <xref href="Jumbee.Console.Control.HandlesInput" data-throw-if-not-resolved="false"></xref> is set.

```csharp
public void OnInput(UI.InputEventArgs inputEventArgs)
```

#### Parameters

`inputEventArgs` [UI](Jumbee.Console.UI.md).[InputEventArgs](Jumbee.Console.UI.InputEventArgs.md)

### <a id="Jumbee_Console_Control_OnInput_ConsoleGUI_Input_InputEvent_"></a> OnInput\(InputEvent\)

Handles a keyboard input event; override on input-handling controls. The default is a no-op.

```csharp
protected virtual void OnInput(InputEvent inputEvent)
```

#### Parameters

`inputEvent` InputEvent

### <a id="Jumbee_Console_Control_OnMouseEnter"></a> OnMouseEnter\(\)

Called when the pointer enters the control.

```csharp
protected virtual void OnMouseEnter()
```

### <a id="Jumbee_Console_Control_OnMouseLeave"></a> OnMouseLeave\(\)

Called when the pointer leaves the control.

```csharp
protected virtual void OnMouseLeave()
```

### <a id="Jumbee_Console_Control_OnMouseMove_ConsoleGUI_Space_Position_"></a> OnMouseMove\(Position\)

Called as the pointer moves within the control (relative position).

```csharp
protected virtual void OnMouseMove(Position position)
```

#### Parameters

`position` Position

### <a id="Jumbee_Console_Control_OnMousePress_ConsoleGUI_Space_Position_"></a> OnMousePress\(Position\)

Called when a button is pressed over the control (relative position).

```csharp
protected virtual void OnMousePress(Position position)
```

#### Parameters

`position` Position

### <a id="Jumbee_Console_Control_OnMouseRelease_ConsoleGUI_Space_Position_"></a> OnMouseRelease\(Position\)

Called when a button is released over the control (relative position).

```csharp
protected virtual void OnMouseRelease(Position position)
```

#### Parameters

`position` Position

### <a id="Jumbee_Console_Control_OnMouseWheel_ConsoleGUI_Space_Position_System_Int32_"></a> OnMouseWheel\(Position, int\)

Handles a wheel notch over the control (<code class="paramref">delta</code>: negative up, positive down). Default
scrolls the nearest enclosing scrolling frame; override to consume the wheel directly.

```csharp
protected virtual void OnMouseWheel(Position position, int delta)
```

#### Parameters

`position` Position

`delta` int

#### Remarks

"Nearest enclosing" rather than just <xref href="Jumbee.Console.Control.Frame" data-throw-if-not-resolved="false"></xref>, because the control under the pointer is usually a
child several levels inside the thing being scrolled — a button in a panel of framed sections has no frame of
its own, and scrolling only its own would drop the notch.

### <a id="Jumbee_Console_Control_OnPaste_System_String_"></a> OnPaste\(string\)

Handles a bracketed-paste payload.

```csharp
public virtual void OnPaste(string text)
```

#### Parameters

`text` string

#### Remarks

Default replays it as character key events so existing text controls receive it; controls that can insert
text in bulk (e.g. <xref href="Jumbee.Console.TextEditor" data-throw-if-not-resolved="false"></xref>) should override this.

### <a id="Jumbee_Console_Control_Paint"></a> Paint\(\)

Invoked in the control's OnPaint event handler.

```csharp
protected virtual void Paint()
```

### <a id="Jumbee_Console_Control_ReleaseMouse"></a> ReleaseMouse\(\)

Releases a capture taken by <xref href="Jumbee.Console.Control.CaptureMouse" data-throw-if-not-resolved="false"></xref>.

```csharp
protected void ReleaseMouse()
```

### <a id="Jumbee_Console_Control_Render"></a> Render\(\)

This method renders the control's content to the console buffer.

```csharp
protected abstract void Render()
```

#### Remarks

Note that this does not actually draw the control on the console screen.

### <a id="Jumbee_Console_Control_ScrollIntoView_System_Int32_System_Int32_"></a> ScrollIntoView\(int, int\)

Scrolls the surrounding <xref href="Jumbee.Console.Control.Frame" data-throw-if-not-resolved="false"></xref> so content rows <code class="paramref">start</code> through
<code class="paramref">start</code> + <code class="paramref">height</code> are visible. A no-op when unframed or already visible.

```csharp
protected void ScrollIntoView(int start, int height = 1)
```

#### Parameters

`start` int

`height` int

#### Remarks

Nothing scrolls on its own, so an <xref href="Jumbee.Console.IScrollable" data-throw-if-not-resolved="false"></xref> whose selection, caret or focus can move must call
this when it does — otherwise the interesting row can sit outside the viewport with the cue drawn where it
cannot be seen. Rows are in this control's own content coordinates, the space
<xref href="Jumbee.Console.IScrollable.MeasureHeight(System.Int32)" data-throw-if-not-resolved="false"></xref> measures.

### <a id="Jumbee_Console_Control_SetAtomicProperty__1___0____0_System_Boolean_System_Func___0___0__System_Action___0___0__System_Boolean_System_String_"></a> SetAtomicProperty<T\>\(ref T, T, bool, Func<T, T\>?, Action<T, T\>?, bool, string?\)

Assigns a backing field and requests a redraw, but only when the value actually changes.

```csharp
protected T SetAtomicProperty<T>(ref T field, T value, bool updatesLayout = false, Func<T, T>? validate = null, Action<T, T>? watch = null, bool themeOverride = false, string? propertyName = null)
```

#### Parameters

`field` T

The backing field to assign.

`value` T

The new value.

`updatesLayout` bool

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, the change affects layout and <xref href="Jumbee.Console.Control.Initialize" data-throw-if-not-resolved="false"></xref> is called (which
re-lays-out on the UI thread and invalidates). Otherwise <xref href="Jumbee.Console.Control.Invalidate" data-throw-if-not-resolved="false"></xref> is called.

`validate` Func<T, T\>?

Optional coercion (e.g. clamp) applied before the equality check and assignment.

`watch` Action<T, T\>?

Optional change callback receiving the old and new values, run before the invalidate/initialize.

`themeOverride` bool

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, marks the calling property (captured automatically via
<code class="paramref">propertyName</code>) as an explicit theme override, so a later runtime theme switch through
<xref href="Jumbee.Console.Control.ApplyTheme" data-throw-if-not-resolved="false"></xref> leaves it alone. Themeable token setters pass <code>themeOverride: true</code>.

`propertyName` string?

The calling member's name, supplied automatically by the compiler (<xref href="System.Runtime.CompilerServices.CallerMemberNameAttribute" data-throw-if-not-resolved="false"></xref>).
Inside a property setter this is the property name (e.g. <code>AccentStyle</code>), so it matches the
<code>nameof(AccentStyle)</code> used by <xref href="Jumbee.Console.Control.ApplyTheme" data-throw-if-not-resolved="false"></xref>. Do not pass it explicitly.

#### Returns

 T

The resulting field value (the coerced new value when changed, otherwise the existing one).

#### Type Parameters

`T` 

#### Remarks

    Centralizes the (optional coerce) + equality-check + assign + (optional notify) + invalidate pattern
    required of visual property setters.

    <p></p>
Only valid for atomically-assignable fields (a single value or reference store). State that cannot be
    written atomically (e.g. collections inside a wrapped control) must use a copy-on-write update instead.
    When supplied, <code class="paramref">validate</code> runs first, so the equality check and stored value use the
    coerced value; <code class="paramref">watch</code> then runs after assignment and before the invalidate/initialize.

### <a id="Jumbee_Console_Control_UnFocus"></a> UnFocus\(\)

Removes keyboard focus from this control.

```csharp
public void UnFocus()
```

### <a id="Jumbee_Console_Control_Validate"></a> Validate\(\)

Indicates that any pending paint requests have been handled and the control does not need re-painting.

```csharp
protected virtual void Validate()
```

### <a id="Jumbee_Console_Control_Clicked"></a> Clicked

Raised on a press+release on this control (relative position).

```csharp
public event EventHandler<Position>? Clicked
```

#### Event Type

 EventHandler<Position\>?

### <a id="Jumbee_Console_Control_DoubleClicked"></a> DoubleClicked

Raised on two clicks within <xref href="Jumbee.Console.Control.DoubleClickMs" data-throw-if-not-resolved="false"></xref> at the same position.

```csharp
public event EventHandler<Position>? DoubleClicked
```

#### Event Type

 EventHandler<Position\>?

### <a id="Jumbee_Console_Control_MouseEntered"></a> MouseEntered

Raised when the pointer enters the control.

```csharp
public event EventHandler? MouseEntered
```

#### Event Type

 EventHandler?

### <a id="Jumbee_Console_Control_MouseLeft"></a> MouseLeft

Raised when the pointer leaves the control.

```csharp
public event EventHandler? MouseLeft
```

#### Event Type

 EventHandler?

### <a id="Jumbee_Console_Control_MouseMoved"></a> MouseMoved

Raised as the pointer moves within the control (relative position).

```csharp
public event EventHandler<Position>? MouseMoved
```

#### Event Type

 EventHandler<Position\>?

### <a id="Jumbee_Console_Control_MousePressed"></a> MousePressed

Raised when a button is pressed over the control (relative position).

```csharp
public event EventHandler<Position>? MousePressed
```

#### Event Type

 EventHandler<Position\>?

### <a id="Jumbee_Console_Control_MouseReleased"></a> MouseReleased

Raised when a button is released over the control (relative position).

```csharp
public event EventHandler<Position>? MouseReleased
```

#### Event Type

 EventHandler<Position\>?

### <a id="Jumbee_Console_Control_MouseWheeled"></a> MouseWheeled

Raised on a wheel notch over the control (negative up, positive down).

```csharp
public event EventHandler<int>? MouseWheeled
```

#### Event Type

 EventHandler<int\>?

### <a id="Jumbee_Console_Control_OnFocus"></a> OnFocus

Raised when the control gains focus.

```csharp
public event FocusableEventHandler? OnFocus
```

#### Event Type

 [FocusableEventHandler](Jumbee.Console.FocusableEventHandler.md)?

### <a id="Jumbee_Console_Control_OnHelp"></a> OnHelp

Raised while the global help dialog is compiled, letting code add or modify this control's help
    without subclassing. The handler receives a mutable <xref href="Jumbee.Console.HelpInfo" data-throw-if-not-resolved="false"></xref> (created if
    <xref href="Jumbee.Console.Control.GetHelpInfo" data-throw-if-not-resolved="false"></xref> returned <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a>) and edits it in place.

```csharp
public event Action<HelpInfo>? OnHelp
```

#### Event Type

 Action<[HelpInfo](Jumbee.Console.HelpInfo.md)\>?

### <a id="Jumbee_Console_Control_OnInitialization"></a> OnInitialization

Raised when the control is initialized (laid out); always invoked on the UI thread.

```csharp
public event Control.InitializationHandler OnInitialization
```

#### Event Type

 [Control](Jumbee.Console.Control.md).[InitializationHandler](Jumbee.Console.Control.InitializationHandler.md)

### <a id="Jumbee_Console_Control_OnLostFocus"></a> OnLostFocus

Raised when the control loses focus.

```csharp
public event FocusableEventHandler? OnLostFocus
```

#### Event Type

 [FocusableEventHandler](Jumbee.Console.FocusableEventHandler.md)?

