# <a id="Jumbee_Console_CompositeControl"></a> Class CompositeControl

Namespace: [Jumbee.Console](Jumbee.Console.md)  
Assembly: Jumbee.Console.dll  

Base class for <em>composite</em> controls: a <xref href="Jumbee.Console.Control" data-throw-if-not-resolved="false"></xref> that owns and lays out several child
controls and presents them as a single control. It is a real <xref href="Jumbee.Console.Control" data-throw-if-not-resolved="false"></xref> (so it has its own
console buffer, participates in theming/painting, can be framed, and drops into any layout cell), but its
content is an internal <xref href="Jumbee.Console.ILayout" data-throw-if-not-resolved="false"></xref> arranging the children.

```csharp
public abstract class CompositeControl : Control, IFocusable
```

#### Inheritance

object ← 
Control ← 
[Control](Jumbee.Console.Control.md) ← 
[CompositeControl](Jumbee.Console.CompositeControl.md)

#### Derived

[ChatPrompt](Jumbee.Console.ChatPrompt.md), 
[CodeEditor](Jumbee.Console.CodeEditor.md), 
[Dialog](Jumbee.Console.Dialog.md), 
[FileBrowser](Jumbee.Console.FileBrowser.md), 
[HelpControl](Jumbee.Console.HelpControl.md), 
[InteractiveSourceEditor](Jumbee.Console.InteractiveSourceEditor.md), 
[MultiTabCodeEditor](Jumbee.Console.MultiTabCodeEditor.md), 
[RunChart](Jumbee.Console.RunChart.md)

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
[Control.Job<T\>\(Func<T\>, Action<T\>, Action<Exception\>?\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_Job\_\_1\_System\_Func\_\_\_0\_\_System\_Action\_\_\_0\_\_System\_Action\_System\_Exception\_\_), 
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

[ControlExtensions.WithAsciiBorder<CompositeControl\>\(CompositeControl, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithAsciiBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithBorder<CompositeControl\>\(CompositeControl, BorderStyle?, Color?, Color?, BorderPlacement?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_BorderPlacement\_\_), 
[ControlExtensions.WithDoubleBorder<CompositeControl\>\(CompositeControl, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithDoubleBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithFrame<CompositeControl\>\(CompositeControl, ControlFrame\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithFrame\_\_1\_\_\_0\_Jumbee\_Console\_ControlFrame\_), 
[ControlExtensions.WithFrame<CompositeControl\>\(CompositeControl, BorderStyle?, Offset?, Color?, Color?, string?, Color?, Color?, BorderPlacement?, BorderStyle?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithFrame\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_System\_Nullable\_ConsoleGUI\_Space\_Offset\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_String\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_BorderPlacement\_\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_), 
[ControlExtensions.WithHeavyBorder<CompositeControl\>\(CompositeControl, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithHeavyBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithHeight<CompositeControl\>\(CompositeControl, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithHeight\_\_1\_\_\_0\_System\_Int32\_), 
[ControlExtensions.WithMargin<CompositeControl\>\(CompositeControl, int, int, int, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithMargin\_\_1\_\_\_0\_System\_Int32\_System\_Int32\_System\_Int32\_System\_Int32\_), 
[ControlExtensions.WithMargin<CompositeControl\>\(CompositeControl, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithMargin\_\_1\_\_\_0\_System\_Int32\_), 
[ControlExtensions.WithNoBorder<CompositeControl\>\(CompositeControl\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithNoBorder\_\_1\_\_\_0\_), 
[ControlExtensions.WithRoundedBorder<CompositeControl\>\(CompositeControl, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithRoundedBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithScrollBarGlyphs<CompositeControl\>\(CompositeControl, ScrollBarGlyphs\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithScrollBarGlyphs\_\_1\_\_\_0\_Jumbee\_Console\_ScrollBarGlyphs\_), 
[ControlExtensions.WithScrollBarStyle<CompositeControl\>\(CompositeControl, ScrollBarStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithScrollBarStyle\_\_1\_\_\_0\_Jumbee\_Console\_ScrollBarStyle\_), 
[ControlExtensions.WithSize<CompositeControl\>\(CompositeControl, int?, int?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithSize\_\_1\_\_\_0\_System\_Nullable\_System\_Int32\_\_System\_Nullable\_System\_Int32\_\_), 
[ControlExtensions.WithSquareBorder<CompositeControl\>\(CompositeControl, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithSquareBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithTitle<CompositeControl\>\(CompositeControl, string\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_), 
[ControlExtensions.WithTitle<CompositeControl\>\(CompositeControl, string, TitleStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_Jumbee\_Console\_TitleStyle\_), 
[ControlExtensions.WithTitle<CompositeControl\>\(CompositeControl, string, TitlePos, TitleBorderStyle, TitleColorStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_Jumbee\_Console\_TitlePos\_Jumbee\_Console\_TitleBorderStyle\_Jumbee\_Console\_TitleColorStyle\_), 
[ControlExtensions.WithWidth<CompositeControl\>\(CompositeControl, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithWidth\_\_1\_\_\_0\_System\_Int32\_)

## Remarks

The children are composited through a <xref href="ConsoleGUI.Common.DrawingContext" data-throw-if-not-resolved="false"></xref> over the internal layout (the same
mechanism <xref href="Jumbee.Console.ControlFrame" data-throw-if-not-resolved="false"></xref> uses for its single child): each child keeps its own buffer and renders
itself, and its cells — carrying its own mouse listener — are surfaced through this control's indexer, so
mouse hit-testing and click-to-focus reach the children unchanged. Keyboard input routes in via
<xref href="Jumbee.Console.CompositeControl.FocusedControl" data-throw-if-not-resolved="false"></xref>, which returns the focused descendant.

<p>
Subclasses build their child controls and an arranging layout in their constructor, wire any inter-child
behaviour (e.g. <code>editor.Changed += …</code>), and call <xref href="Jumbee.Console.CompositeControl.SetContent(Jumbee.Console.ILayout)" data-throw-if-not-resolved="false"></xref>. The composite draws no content
of its own by default; override <xref href="Jumbee.Console.Control.Render" data-throw-if-not-resolved="false"></xref> to paint a background/chrome into the buffer
behind the children.
</p>

## Constructors

### <a id="Jumbee_Console_CompositeControl__ctor"></a> CompositeControl\(\)

Initializes a new <xref href="Jumbee.Console.CompositeControl" data-throw-if-not-resolved="false"></xref> with no content; the subclass calls <xref href="Jumbee.Console.CompositeControl.SetContent(Jumbee.Console.ILayout)" data-throw-if-not-resolved="false"></xref> after building its children.

```csharp
protected CompositeControl()
```

### <a id="Jumbee_Console_CompositeControl__ctor_Jumbee_Console_ILayout_"></a> CompositeControl\(ILayout\)

Initializes a new <xref href="Jumbee.Console.CompositeControl" data-throw-if-not-resolved="false"></xref> with the given <code class="paramref">content</code> layout arranging its children.

```csharp
protected CompositeControl(ILayout content)
```

#### Parameters

`content` [ILayout](Jumbee.Console.ILayout.md)

## Properties

### <a id="Jumbee_Console_CompositeControl_Content"></a> Content

The internal layout arranging the child controls (set via <xref href="Jumbee.Console.CompositeControl.SetContent(Jumbee.Console.ILayout)" data-throw-if-not-resolved="false"></xref>).

```csharp
protected ILayout? Content { get; }
```

#### Property Value

 [ILayout](Jumbee.Console.ILayout.md)?

### <a id="Jumbee_Console_CompositeControl_FocusChild"></a> FocusChild

The child that receives focus when the composite is focused (and input/caret follow). Defaults to the
    child focus was last requested for (a clicked field, or a <xref href="Jumbee.Console.CompositeControl.MoveFocusToChild(System.Int32)" data-throw-if-not-resolved="false"></xref> step), else the first
    focusable child; override to choose a different default.

```csharp
protected virtual Control? FocusChild { get; }
```

#### Property Value

 [Control](Jumbee.Console.Control.md)?

### <a id="Jumbee_Console_CompositeControl_Focusables"></a> Focusables

The focusable children, in layout order — the stops <xref href="Jumbee.Console.CompositeControl.MoveFocusToChild(System.Int32)" data-throw-if-not-resolved="false"></xref> walks.

```csharp
protected IReadOnlyList<Control> Focusables { get; }
```

#### Property Value

 IReadOnlyList<[Control](Jumbee.Console.Control.md)\>

### <a id="Jumbee_Console_CompositeControl_FocusedControl"></a> FocusedControl

Returns the focused descendant so keyboard input routed by the parent layout reaches the right child;
falls back to the composite itself when it is focused and no child is.

```csharp
public override IFocusable? FocusedControl { get; }
```

#### Property Value

 [IFocusable](Jumbee.Console.IFocusable.md)?

### <a id="Jumbee_Console_CompositeControl_HandlesInput"></a> HandlesInput

A composite is an opaque focusable leaf to the navigation/routing layer: it reports that it handles
    input (keys reach its focused child via <xref href="Jumbee.Console.CompositeControl.FocusedControl" data-throw-if-not-resolved="false"></xref>) so <code>ILayout.Leaves</code> treats it as a
    single navigable unit rather than descending into its children.

```csharp
public override bool HandlesInput { get; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_CompositeControl_TabNavigatesChildren"></a> TabNavigatesChildren

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, Tab / Shift+Tab move focus between the composite's focusable children
    (cycling within it) instead of reaching the focused child. Off by default: Tab belongs to the focused control
    (a <xref href="Jumbee.Console.TextEditor" data-throw-if-not-resolved="false"></xref> indents with it). Turn it on for a form — several fields the user tabs between.

```csharp
protected virtual bool TabNavigatesChildren { get; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_CompositeControl_Item_ConsoleGUI_Space_Position_"></a> this\[Position\]

Gets the composited <xref href="ConsoleGUI.Data.Cell" data-throw-if-not-resolved="false"></xref> at <code class="paramref">position</code> — a child's cell (keeping its mouse listener) where a child covers it, otherwise the composite's own surface.

```csharp
public override Cell this[Position position] { get; }
```

#### Property Value

 Cell

## Methods

### <a id="Jumbee_Console_CompositeControl_Control_OnFocus"></a> Control\_OnFocus\(\)

Routes focus inward to <xref href="Jumbee.Console.CompositeControl.FocusChild" data-throw-if-not-resolved="false"></xref> when the composite gains focus.

```csharp
protected override void Control_OnFocus()
```

### <a id="Jumbee_Console_CompositeControl_Control_OnInitialization"></a> Control\_OnInitialization\(\)

Sizes the internal content layout to fill the composite's current area after initialization.

```csharp
protected override void Control_OnInitialization()
```

### <a id="Jumbee_Console_CompositeControl_Control_OnLostFocus"></a> Control\_OnLostFocus\(\)

Clears the focused descendant when the composite loses focus.

```csharp
protected override void Control_OnLostFocus()
```

### <a id="Jumbee_Console_CompositeControl_Dispose"></a> Dispose\(\)

Disposes the content drawing context and the base control.

```csharp
public override void Dispose()
```

### <a id="Jumbee_Console_CompositeControl_InterceptInput_Jumbee_Console_UI_InputEventArgs_"></a> InterceptInput\(InputEventArgs\)

A first look at each key on its way to the focused child, mirroring a layout's <code>InterceptInput</code> tunnel —
so a composite can define its own navigation keys.

```csharp
protected virtual bool InterceptInput(UI.InputEventArgs inputEventArgs)
```

#### Parameters

`inputEventArgs` [UI](Jumbee.Console.UI.md).[InputEventArgs](Jumbee.Console.UI.InputEventArgs.md)

#### Returns

 bool

#### Remarks

Return <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a> to consume the key. The base handles Tab / Shift+Tab when
<xref href="Jumbee.Console.CompositeControl.TabNavigatesChildren" data-throw-if-not-resolved="false"></xref> is set.

### <a id="Jumbee_Console_CompositeControl_MoveFocusToChild_System_Int32_"></a> MoveFocusToChild\(int\)

Moves focus to the next (<code>+1</code>) or previous (<code>-1</code>) focusable child, wrapping. Returns
    <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a> when there are fewer than two to move between.

```csharp
protected bool MoveFocusToChild(int direction)
```

#### Parameters

`direction` int

#### Returns

 bool

### <a id="Jumbee_Console_CompositeControl_Render"></a> Render\(\)

Renders the composite's own surface; the default draws nothing (children render themselves). Override to paint a background or chrome behind the children.

```csharp
protected override void Render()
```

### <a id="Jumbee_Console_CompositeControl_SetContent_Jumbee_Console_ILayout_"></a> SetContent\(ILayout\)

Sets the internal layout that arranges the children.

```csharp
protected void SetContent(ILayout content)
```

#### Parameters

`content` [ILayout](Jumbee.Console.ILayout.md)

#### Remarks

Call once from the subclass constructor after building the child controls and the layout, and after wiring
any inter-child event handlers.

