# <a id="Jumbee_Console_Slider"></a> Class Slider

Namespace: [Jumbee.Console](Jumbee.Console.md)  
Assembly: Jumbee.Console.dll  

A single-row draggable value control: an optional <xref href="Jumbee.Console.Slider.Label" data-throw-if-not-resolved="false"></xref>, a track filled to <xref href="Jumbee.Console.Slider.Value" data-throw-if-not-resolved="false"></xref>
between <xref href="Jumbee.Console.Slider.Minimum" data-throw-if-not-resolved="false"></xref> and <xref href="Jumbee.Console.Slider.Maximum" data-throw-if-not-resolved="false"></xref>, and an optional numeric readout — e.g.
<code>Gravity ████▌      9.80</code>.

```csharp
public class Slider : RenderableControl, IFocusable
```

#### Inheritance

object ← 
Control ← 
[Control](Jumbee.Console.Control.md) ← 
[RenderableControl](Jumbee.Console.RenderableControl.md) ← 
[Slider](Jumbee.Console.Slider.md)

#### Implements

[IFocusable](Jumbee.Console.IFocusable.md)

#### Inherited Members

[RenderableControl.Render\(RenderOptions, int\)](Jumbee.Console.RenderableControl.md\#Jumbee\_Console\_RenderableControl\_Render\_Spectre\_Console\_Rendering\_RenderOptions\_System\_Int32\_), 
[RenderableControl.Measure\(RenderOptions, int\)](Jumbee.Console.RenderableControl.md\#Jumbee\_Console\_RenderableControl\_Measure\_Spectre\_Console\_Rendering\_RenderOptions\_System\_Int32\_), 
[RenderableControl.RendersInteractiveState](Jumbee.Console.RenderableControl.md\#Jumbee\_Console\_RenderableControl\_RendersInteractiveState), 
[RenderableControl.Invalidate\(\)](Jumbee.Console.RenderableControl.md\#Jumbee\_Console\_RenderableControl\_Invalidate), 
[RenderableControl.InvalidateInteractive\(\)](Jumbee.Console.RenderableControl.md\#Jumbee\_Console\_RenderableControl\_InvalidateInteractive), 
[RenderableControl.Initialize\(\)](Jumbee.Console.RenderableControl.md\#Jumbee\_Console\_RenderableControl\_Initialize), 
[RenderableControl.Render\(\)](Jumbee.Console.RenderableControl.md\#Jumbee\_Console\_RenderableControl\_Render), 
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

[ControlExtensions.WithAsciiBorder<Slider\>\(Slider, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithAsciiBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithBorder<Slider\>\(Slider, BorderStyle?, Color?, Color?, BorderPlacement?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_BorderPlacement\_\_), 
[ControlExtensions.WithDoubleBorder<Slider\>\(Slider, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithDoubleBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithFrame<Slider\>\(Slider, ControlFrame\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithFrame\_\_1\_\_\_0\_Jumbee\_Console\_ControlFrame\_), 
[ControlExtensions.WithFrame<Slider\>\(Slider, BorderStyle?, Offset?, Color?, Color?, string?, Color?, Color?, BorderPlacement?, BorderStyle?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithFrame\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_System\_Nullable\_ConsoleGUI\_Space\_Offset\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_String\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_BorderPlacement\_\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_), 
[ControlExtensions.WithHeavyBorder<Slider\>\(Slider, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithHeavyBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithHeight<Slider\>\(Slider, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithHeight\_\_1\_\_\_0\_System\_Int32\_), 
[ControlExtensions.WithMargin<Slider\>\(Slider, int, int, int, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithMargin\_\_1\_\_\_0\_System\_Int32\_System\_Int32\_System\_Int32\_System\_Int32\_), 
[ControlExtensions.WithMargin<Slider\>\(Slider, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithMargin\_\_1\_\_\_0\_System\_Int32\_), 
[ControlExtensions.WithNoBorder<Slider\>\(Slider\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithNoBorder\_\_1\_\_\_0\_), 
[ControlExtensions.WithRoundedBorder<Slider\>\(Slider, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithRoundedBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithScrollBarGlyphs<Slider\>\(Slider, ScrollBarGlyphs\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithScrollBarGlyphs\_\_1\_\_\_0\_Jumbee\_Console\_ScrollBarGlyphs\_), 
[ControlExtensions.WithScrollBarStyle<Slider\>\(Slider, ScrollBarStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithScrollBarStyle\_\_1\_\_\_0\_Jumbee\_Console\_ScrollBarStyle\_), 
[ControlExtensions.WithSize<Slider\>\(Slider, int?, int?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithSize\_\_1\_\_\_0\_System\_Nullable\_System\_Int32\_\_System\_Nullable\_System\_Int32\_\_), 
[ControlExtensions.WithSquareBorder<Slider\>\(Slider, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithSquareBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithTitle<Slider\>\(Slider, string\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_), 
[ControlExtensions.WithTitle<Slider\>\(Slider, string, TitleStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_Jumbee\_Console\_TitleStyle\_), 
[ControlExtensions.WithTitle<Slider\>\(Slider, string, TitlePos, TitleBorderStyle, TitleColorStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_Jumbee\_Console\_TitlePos\_Jumbee\_Console\_TitleBorderStyle\_Jumbee\_Console\_TitleColorStyle\_), 
[ControlExtensions.WithWidth<Slider\>\(Slider, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithWidth\_\_1\_\_\_0\_System\_Int32\_)

## Remarks

<p>
The interactive sibling of <xref href="Jumbee.Console.Gauge" data-throw-if-not-resolved="false"></xref> and <xref href="Jumbee.Console.ProgressBar" data-throw-if-not-resolved="false"></xref>, which only display a value. Drag
the thumb or click anywhere on the track to set it; arrows step by <xref href="Jumbee.Console.Slider.Step" data-throw-if-not-resolved="false"></xref>, Page Up/Down by ten
steps, Home/End jump to the ends, and the wheel steps while the pointer is over the control.
</p>
<p>
The thumb occupies one whole cell at the fill's leading edge, so a stack of sliders reads as a row of controls
rather than as a bar chart. Only its <em>position</em> is quantised to a cell — the value itself stays
continuous. Set <xref href="Jumbee.Console.Slider.SnapToStep" data-throw-if-not-resolved="false"></xref> for a slider whose value should only ever land on a step (an integer
count, say).
</p>

## Constructors

### <a id="Jumbee_Console_Slider__ctor_System_Double_System_Double_System_Double_System_String_"></a> Slider\(double, double, double, string?\)

Initializes a new <xref href="Jumbee.Console.Slider" data-throw-if-not-resolved="false"></xref> over the range <code class="paramref">minimum</code>..<code class="paramref">maximum</code>
    at <code class="paramref">value</code>, with an optional <code class="paramref">label</code>.

```csharp
public Slider(double minimum = 0, double maximum = 1, double value = 0, string? label = null)
```

#### Parameters

`minimum` double

`maximum` double

`value` double

`label` string?

## Properties

### <a id="Jumbee_Console_Slider_FocusedStyle"></a> FocusedStyle

Style merged into the label and readout while focused. Defaults to <xref href="Jumbee.Console.IStyleTheme.Focus" data-throw-if-not-resolved="false"></xref>.

```csharp
public Style FocusedStyle { get; set; }
```

#### Property Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Slider_Fraction"></a> Fraction

The value as a 0..1 position along the track.

```csharp
public double Fraction { get; }
```

#### Property Value

 double

### <a id="Jumbee_Console_Slider_HandlesInput"></a> HandlesInput

Reports <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a> so input routing delivers keys to the control.

```csharp
public override bool HandlesInput { get; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_Slider_HoverStyle"></a> HoverStyle

Style merged into the label and readout while hovered. Defaults to <xref href="Jumbee.Console.IStyleTheme.Hover" data-throw-if-not-resolved="false"></xref>.

```csharp
public Style HoverStyle { get; set; }
```

#### Property Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Slider_Label"></a> Label

The caption drawn before the track. Null/empty draws none.

```csharp
public string? Label { get; set; }
```

#### Property Value

 string?

### <a id="Jumbee_Console_Slider_LabelWidth"></a> LabelWidth

Cells reserved for the label, so a stack of sliders lines its tracks up. 0 (the default) sizes to
    the label itself.

```csharp
public int LabelWidth { get; set; }
```

#### Property Value

 int

### <a id="Jumbee_Console_Slider_Maximum"></a> Maximum

The high end of the range. Coerced above <xref href="Jumbee.Console.Slider.Minimum" data-throw-if-not-resolved="false"></xref> so the range is never empty, and
    re-clamps <xref href="Jumbee.Console.Slider.Value" data-throw-if-not-resolved="false"></xref>.

```csharp
public double Maximum { get; set; }
```

#### Property Value

 double

### <a id="Jumbee_Console_Slider_Minimum"></a> Minimum

The low end of the range. Re-clamps <xref href="Jumbee.Console.Slider.Value" data-throw-if-not-resolved="false"></xref>.

```csharp
public double Minimum { get; set; }
```

#### Property Value

 double

### <a id="Jumbee_Console_Slider_RendersOwnFocus"></a> RendersOwnFocus

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, this control indicates keyboard focus in its own way (e.g. a button's fill
change, a tab's underline, an editor's cursor), so the base class does <em>not</em> paint the themed default
focus tint over it.

```csharp
protected override bool RendersOwnFocus { get; }
```

#### Property Value

 bool

#### Remarks

Override and return <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a> on controls with their own focus styling; the
default (<a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a>) gives unstyled focusable controls an automatic, always-visible focus cue.

### <a id="Jumbee_Console_Slider_ShowValue"></a> ShowValue

Whether to draw the numeric readout after the track. Default <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>.

```csharp
public bool ShowValue { get; set; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_Slider_SnapToStep"></a> SnapToStep

Whether every value — including one set by dragging or clicking the track — is quantised to a
    multiple of <xref href="Jumbee.Console.Slider.Step" data-throw-if-not-resolved="false"></xref> from <xref href="Jumbee.Console.Slider.Minimum" data-throw-if-not-resolved="false"></xref>. Default <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a> (continuous).

```csharp
public bool SnapToStep { get; set; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_Slider_Step"></a> Step

How far one arrow key moves the value (Page Up/Down move ten of these). Defaults to a hundredth of
    the range.

```csharp
public double Step { get; set; }
```

#### Property Value

 double

### <a id="Jumbee_Console_Slider_Style"></a> Style

The per-part colours. Defaults to <xref href="Jumbee.Console.IStyleTheme.Slider" data-throw-if-not-resolved="false"></xref>.

```csharp
public SliderStyle Style { get; set; }
```

#### Property Value

 [SliderStyle](Jumbee.Console.SliderStyle.md)

### <a id="Jumbee_Console_Slider_ThumbGlyph"></a> ThumbGlyph

The whole-cell thumb glyph. Defaults to <xref href="Jumbee.Console.IGlyphTheme.SliderThumb" data-throw-if-not-resolved="false"></xref>.

```csharp
public string ThumbGlyph { get; set; }
```

#### Property Value

 string

### <a id="Jumbee_Console_Slider_Value"></a> Value

The current value, clamped to <xref href="Jumbee.Console.Slider.Minimum" data-throw-if-not-resolved="false"></xref>..<xref href="Jumbee.Console.Slider.Maximum" data-throw-if-not-resolved="false"></xref>. Setting it raises
    <xref href="Jumbee.Console.Slider.ValueChanged" data-throw-if-not-resolved="false"></xref> when it actually moves.

```csharp
public double Value { get; set; }
```

#### Property Value

 double

### <a id="Jumbee_Console_Slider_ValueFormat"></a> ValueFormat

The format passed to <xref href="System.Double.ToString(System.String)" data-throw-if-not-resolved="false"></xref> for the readout. Default <code>"F2"</code>.

```csharp
public string ValueFormat { get; set; }
```

#### Property Value

 string

### <a id="Jumbee_Console_Slider_WantsMouse"></a> WantsMouse

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, the control's cells are tagged with a mouse listener even if it is not
<xref href="Jumbee.Console.Control.Focusable" data-throw-if-not-resolved="false"></xref>, so it still receives hover/click (e.g. a non-focusable clickable Link).

```csharp
protected override bool WantsMouse { get; }
```

#### Property Value

 bool

## Methods

### <a id="Jumbee_Console_Slider_ApplyTheme"></a> ApplyTheme\(\)

Re-captures this control's themed colours/glyphs from the current <xref href="Jumbee.Console.UI.StyleTheme" data-throw-if-not-resolved="false"></xref>/
<xref href="Jumbee.Console.UI.GlyphTheme" data-throw-if-not-resolved="false"></xref>. The default is a no-op for controls that don't use the theme.

```csharp
protected override void ApplyTheme()
```

#### Remarks

Called by themed controls from their constructor and again on a runtime theme switch (<xref href="Jumbee.Console.UI.SetTheme(Jumbee.Console.IStyleTheme%2cJumbee.Console.IGlyphTheme)" data-throw-if-not-resolved="false"></xref>).
Must read the themes <em>only here</em> (and in the constructor), never on the render path.

### <a id="Jumbee_Console_Slider_GetHelpInfo"></a> GetHelpInfo\(\)

The help shown for this control in the global help dialog (F1), or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> for no help.

```csharp
protected override HelpInfo? GetHelpInfo()
```

#### Returns

 [HelpInfo](Jumbee.Console.HelpInfo.md)?

#### Remarks

Override to describe the control and its keys. The result is deduplicated across the UI by
<xref href="Jumbee.Console.HelpInfo.Name" data-throw-if-not-resolved="false"></xref>, so give controls of the same kind the same name. <xref href="Jumbee.Console.Control.OnHelp" data-throw-if-not-resolved="false"></xref> handlers
can further modify (or create) it.

### <a id="Jumbee_Console_Slider_IntrinsicHeight"></a> IntrinsicHeight\(\)

Fixed one row tall; fills the width its parent offers.

```csharp
protected override int IntrinsicHeight()
```

#### Returns

 int

### <a id="Jumbee_Console_Slider_OnInput_ConsoleGUI_Input_InputEvent_"></a> OnInput\(InputEvent\)

Handles a keyboard input event; override on input-handling controls. The default is a no-op.

```csharp
protected override void OnInput(InputEvent inputEvent)
```

#### Parameters

`inputEvent` InputEvent

### <a id="Jumbee_Console_Slider_OnMouseMove_ConsoleGUI_Space_Position_"></a> OnMouseMove\(Position\)

Called as the pointer moves within the control (relative position).

```csharp
protected override void OnMouseMove(Position position)
```

#### Parameters

`position` Position

### <a id="Jumbee_Console_Slider_OnMousePress_ConsoleGUI_Space_Position_"></a> OnMousePress\(Position\)

Called when a button is pressed over the control (relative position).

```csharp
protected override void OnMousePress(Position position)
```

#### Parameters

`position` Position

### <a id="Jumbee_Console_Slider_OnMouseRelease_ConsoleGUI_Space_Position_"></a> OnMouseRelease\(Position\)

Called when a button is released over the control (relative position).

```csharp
protected override void OnMouseRelease(Position position)
```

#### Parameters

`position` Position

### <a id="Jumbee_Console_Slider_OnMouseWheel_ConsoleGUI_Space_Position_System_Int32_"></a> OnMouseWheel\(Position, int\)

Handles a wheel notch over the control (<code class="paramref">delta</code>: negative up, positive down). Default
scrolls the nearest enclosing scrolling frame; override to consume the wheel directly.

```csharp
protected override void OnMouseWheel(Position position, int delta)
```

#### Parameters

`position` Position

`delta` int

#### Remarks

"Nearest enclosing" rather than just <xref href="Jumbee.Console.Control.Frame" data-throw-if-not-resolved="false"></xref>, because the control under the pointer is usually a
child several levels inside the thing being scrolled — a button in a panel of framed sections has no frame of
its own, and scrolling only its own would drop the notch.

### <a id="Jumbee_Console_Slider_Render_Spectre_Console_Rendering_RenderOptions_System_Int32_"></a> Render\(RenderOptions, int\)

Produces the Spectre.Console <xref href="Spectre.Console.Rendering.Segment" data-throw-if-not-resolved="false"></xref>s for the control's content within <code class="paramref">maxWidth</code>.

```csharp
protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
```

#### Parameters

`options` RenderOptions

`maxWidth` int

#### Returns

 IEnumerable<Segment\>

### <a id="Jumbee_Console_Slider_SetFraction_System_Double_"></a> SetFraction\(double\)

Sets the value from a 0..1 position along the track (a fluent shorthand for the drag/click path).

```csharp
public void SetFraction(double fraction)
```

#### Parameters

`fraction` double

### <a id="Jumbee_Console_Slider_StepBy_System_Int32_"></a> StepBy\(int\)

Moves the value by <code class="paramref">steps</code> × <xref href="Jumbee.Console.Slider.Step" data-throw-if-not-resolved="false"></xref>.

```csharp
public void StepBy(int steps)
```

#### Parameters

`steps` int

### <a id="Jumbee_Console_Slider_WithFill_Jumbee_Console_Color_"></a> WithFill\(Color\)

Recolours the filled portion of the track (marks the style an override).

```csharp
public Slider WithFill(Color color)
```

#### Parameters

`color` [Color](Jumbee.Console.Color.md)

#### Returns

 [Slider](Jumbee.Console.Slider.md)

### <a id="Jumbee_Console_Slider_WithLabel_System_String_System_Int32_"></a> WithLabel\(string, int\)

Sets the label and the cells reserved for it, so sliders in a stack align.

```csharp
public Slider WithLabel(string label, int width = 0)
```

#### Parameters

`label` string

`width` int

#### Returns

 [Slider](Jumbee.Console.Slider.md)

### <a id="Jumbee_Console_Slider_ValueChanged"></a> ValueChanged

Raised with the new value whenever <xref href="Jumbee.Console.Slider.Value" data-throw-if-not-resolved="false"></xref> changes.

```csharp
public event EventHandler<double>? ValueChanged
```

#### Event Type

 EventHandler<double\>?

