# <a id="Jumbee_Console_ProgressBar"></a> Class ProgressBar

Namespace: [Jumbee.Console](Jumbee.Console.md)  
Assembly: Jumbee.Console.dll  

A single-row task progress display, modelled on one row of a Spectre.Console <code>Progress</code>: a
<xref href="Jumbee.Console.ProgressBar.Description" data-throw-if-not-resolved="false"></xref>, a bar filled to <xref href="Jumbee.Console.ProgressBar.Value" data-throw-if-not-resolved="false"></xref> / <xref href="Jumbee.Console.ProgressBar.Max" data-throw-if-not-resolved="false"></xref>, then optional
<xref href="Jumbee.Console.ProgressBar.ShowPercentage?text=percentage" data-throw-if-not-resolved="false"></xref>, <xref href="Jumbee.Console.ProgressBar.TimeDisplay?text=time" data-throw-if-not-resolved="false"></xref> and
<xref href="Jumbee.Console.ProgressBar.ShowSpinner?text=spinner" data-throw-if-not-resolved="false"></xref> columns — e.g. <code>Consulting the oracle ──── 96% 00:00:00 ⣷</code>.

```csharp
public class ProgressBar : RenderableControl, IFocusable
```

#### Inheritance

object ← 
Control ← 
[Control](Jumbee.Console.Control.md) ← 
[RenderableControl](Jumbee.Console.RenderableControl.md) ← 
[ProgressBar](Jumbee.Console.ProgressBar.md)

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
[Control.MeasureHeight\(int\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_MeasureHeight\_System\_Int32\_), 
[Control.FillsFrameViewport](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_FillsFrameViewport), 
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

[ControlExtensions.WithAsciiBorder<ProgressBar\>\(ProgressBar, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithAsciiBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithBorder<ProgressBar\>\(ProgressBar, BorderStyle?, Color?, Color?, BorderPlacement?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_BorderPlacement\_\_), 
[ControlExtensions.WithDoubleBorder<ProgressBar\>\(ProgressBar, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithDoubleBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithFrame<ProgressBar\>\(ProgressBar, ControlFrame\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithFrame\_\_1\_\_\_0\_Jumbee\_Console\_ControlFrame\_), 
[ControlExtensions.WithFrame<ProgressBar\>\(ProgressBar, BorderStyle?, Offset?, Color?, Color?, string?, Color?, Color?, BorderPlacement?, BorderStyle?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithFrame\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_System\_Nullable\_ConsoleGUI\_Space\_Offset\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_String\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_BorderPlacement\_\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_), 
[ControlExtensions.WithHeavyBorder<ProgressBar\>\(ProgressBar, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithHeavyBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithHeight<ProgressBar\>\(ProgressBar, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithHeight\_\_1\_\_\_0\_System\_Int32\_), 
[ControlExtensions.WithMargin<ProgressBar\>\(ProgressBar, int, int, int, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithMargin\_\_1\_\_\_0\_System\_Int32\_System\_Int32\_System\_Int32\_System\_Int32\_), 
[ControlExtensions.WithMargin<ProgressBar\>\(ProgressBar, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithMargin\_\_1\_\_\_0\_System\_Int32\_), 
[ControlExtensions.WithNoBorder<ProgressBar\>\(ProgressBar\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithNoBorder\_\_1\_\_\_0\_), 
[ControlExtensions.WithRoundedBorder<ProgressBar\>\(ProgressBar, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithRoundedBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithScrollBarGlyphs<ProgressBar\>\(ProgressBar, ScrollBarGlyphs\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithScrollBarGlyphs\_\_1\_\_\_0\_Jumbee\_Console\_ScrollBarGlyphs\_), 
[ControlExtensions.WithScrollBarStyle<ProgressBar\>\(ProgressBar, ScrollBarStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithScrollBarStyle\_\_1\_\_\_0\_Jumbee\_Console\_ScrollBarStyle\_), 
[ControlExtensions.WithSize<ProgressBar\>\(ProgressBar, int?, int?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithSize\_\_1\_\_\_0\_System\_Nullable\_System\_Int32\_\_System\_Nullable\_System\_Int32\_\_), 
[ControlExtensions.WithSquareBorder<ProgressBar\>\(ProgressBar, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithSquareBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithTitle<ProgressBar\>\(ProgressBar, string\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_), 
[ControlExtensions.WithTitle<ProgressBar\>\(ProgressBar, string, TitleStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_Jumbee\_Console\_TitleStyle\_), 
[ControlExtensions.WithTitle<ProgressBar\>\(ProgressBar, string, TitlePos, TitleBorderStyle, TitleColorStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_Jumbee\_Console\_TitlePos\_Jumbee\_Console\_TitleBorderStyle\_Jumbee\_Console\_TitleColorStyle\_), 
[ControlExtensions.WithWidth<ProgressBar\>\(ProgressBar, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithWidth\_\_1\_\_\_0\_System\_Int32\_)

## Remarks

Unlike <xref href="Jumbee.Console.SpectreTaskProgress" data-throw-if-not-resolved="false"></xref> (the multi-task Spectre widget), this is a plain composable control you
place, theme and drive yourself: set <xref href="Jumbee.Console.ProgressBar.Value" data-throw-if-not-resolved="false"></xref> as work advances. It is the task-oriented sibling of
<xref href="Jumbee.Console.Gauge" data-throw-if-not-resolved="false"></xref> (a dashboard meter). Call <xref href="Jumbee.Console.ProgressBar.Start" data-throw-if-not-resolved="false"></xref> to begin the internal clock and, when a spinner
or <xref href="Jumbee.Console.ProgressBar.IsIndeterminate" data-throw-if-not-resolved="false"></xref> pulse is shown, its animation; <xref href="Jumbee.Console.ProgressBar.Stop" data-throw-if-not-resolved="false"></xref> freezes both. The bar is a
smooth sub-cell band by default; <xref href="Jumbee.Console.ProgressBar.Glyphs" data-throw-if-not-resolved="false"></xref> switches it to a per-cell glyph fill (hatch, segments, ASCII).

## Constructors

### <a id="Jumbee_Console_ProgressBar__ctor_System_String_System_Double_System_Double_"></a> ProgressBar\(string?, double, double\)

Initializes a new <xref href="Jumbee.Console.ProgressBar" data-throw-if-not-resolved="false"></xref> with an optional <code class="paramref">description</code>, current
    <code class="paramref">value</code> and full-bar <code class="paramref">max</code>.

```csharp
public ProgressBar(string? description = null, double value = 0, double max = 100)
```

#### Parameters

`description` string?

`value` double

`max` double

## Properties

### <a id="Jumbee_Console_ProgressBar_Description"></a> Description

The task status text drawn before the bar. Null/empty draws none. Ellipsis-truncated to keep the bar
    at least a few cells wide.

```csharp
public string? Description { get; set; }
```

#### Property Value

 string?

### <a id="Jumbee_Console_ProgressBar_Glyphs"></a> Glyphs

The bar glyphs and fill mode (solid band vs per-cell glyphs like a hatch or segments). Defaults to
    <xref href="Jumbee.Console.IGlyphTheme.ProgressBar" data-throw-if-not-resolved="false"></xref>.

```csharp
public ProgressBarGlyphs Glyphs { get; set; }
```

#### Property Value

 [ProgressBarGlyphs](Jumbee.Console.ProgressBarGlyphs.md)

### <a id="Jumbee_Console_ProgressBar_IsIndeterminate"></a> IsIndeterminate

Whether the total is unknown: the bar shows a moving pulse rather than a fill, and the percentage is
    suppressed. The pulse animates only while <xref href="Jumbee.Console.ProgressBar.Start" data-throw-if-not-resolved="false"></xref>ed. Default <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a>.

```csharp
public bool IsIndeterminate { get; set; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_ProgressBar_LeftPad"></a> LeftPad

Blank cells reserved at the left edge, before the description. Default 0.

```csharp
public int LeftPad { get; set; }
```

#### Property Value

 int

### <a id="Jumbee_Console_ProgressBar_Max"></a> Max

The value mapped to a full bar. Coerced to at least a tiny positive number so the fraction is defined.

```csharp
public double Max { get; set; }
```

#### Property Value

 double

### <a id="Jumbee_Console_ProgressBar_RendersInteractiveState"></a> RendersInteractiveState

Content-only render, so the cached buffer is reused on interactive-state changes.

```csharp
protected override bool RendersInteractiveState { get; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_ProgressBar_RightPad"></a> RightPad

Blank cells reserved at the right edge, after the readouts — breathing room from the frame/edge. Default 0.

```csharp
public int RightPad { get; set; }
```

#### Property Value

 int

### <a id="Jumbee_Console_ProgressBar_ShowPercentage"></a> ShowPercentage

Whether to draw the percentage (<code>96%</code>) after the bar. Ignored when
    <xref href="Jumbee.Console.ProgressBar.IsIndeterminate" data-throw-if-not-resolved="false"></xref>. Default <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>.

```csharp
public bool ShowPercentage { get; set; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_ProgressBar_ShowSpinner"></a> ShowSpinner

Whether to draw the animated spinner glyph after the other columns. Animates only while
    <xref href="Jumbee.Console.ProgressBar.Start" data-throw-if-not-resolved="false"></xref>ed. Default <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a>.

```csharp
public bool ShowSpinner { get; set; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_ProgressBar_SpinnerType"></a> SpinnerType

The spinner animation (frame set and interval) used when <xref href="Jumbee.Console.ProgressBar.ShowSpinner" data-throw-if-not-resolved="false"></xref> is set.

```csharp
public Spinner SpinnerType { get; set; }
```

#### Property Value

 Spinner

### <a id="Jumbee_Console_ProgressBar_Style"></a> Style

The per-part colours. Defaults to <xref href="Jumbee.Console.IStyleTheme.ProgressBar" data-throw-if-not-resolved="false"></xref>.

```csharp
public ProgressBarStyle Style { get; set; }
```

#### Property Value

 [ProgressBarStyle](Jumbee.Console.ProgressBarStyle.md)

### <a id="Jumbee_Console_ProgressBar_TimeDisplay"></a> TimeDisplay

What the time column shows (elapsed, estimated-remaining, or nothing). Timed from <xref href="Jumbee.Console.ProgressBar.Start" data-throw-if-not-resolved="false"></xref>.
    Default <xref href="Jumbee.Console.ProgressTimeDisplay.None" data-throw-if-not-resolved="false"></xref>.

```csharp
public ProgressTimeDisplay TimeDisplay { get; set; }
```

#### Property Value

 [ProgressTimeDisplay](Jumbee.Console.ProgressTimeDisplay.md)

### <a id="Jumbee_Console_ProgressBar_Value"></a> Value

The current value. The filled fraction is <xref href="Jumbee.Console.ProgressBar.Value" data-throw-if-not-resolved="false"></xref> / <xref href="Jumbee.Console.ProgressBar.Max" data-throw-if-not-resolved="false"></xref> (clamped to 0..1).

```csharp
public double Value { get; set; }
```

#### Property Value

 double

## Methods

### <a id="Jumbee_Console_ProgressBar_ApplyTheme"></a> ApplyTheme\(\)

Re-captures this control's themed colours/glyphs from the current <xref href="Jumbee.Console.UI.StyleTheme" data-throw-if-not-resolved="false"></xref>/
<xref href="Jumbee.Console.UI.GlyphTheme" data-throw-if-not-resolved="false"></xref>. The default is a no-op for controls that don't use the theme.

```csharp
protected override void ApplyTheme()
```

#### Remarks

Called by themed controls from their constructor and again on a runtime theme switch (<xref href="Jumbee.Console.UI.SetTheme(Jumbee.Console.IStyleTheme%2cJumbee.Console.IGlyphTheme)" data-throw-if-not-resolved="false"></xref>).
Must read the themes <em>only here</em> (and in the constructor), never on the render path.

### <a id="Jumbee_Console_ProgressBar_IntrinsicHeight"></a> IntrinsicHeight\(\)

Fixed one row tall; fills the width its parent offers.

```csharp
protected override int IntrinsicHeight()
```

#### Returns

 int

### <a id="Jumbee_Console_ProgressBar_Render_Spectre_Console_Rendering_RenderOptions_System_Int32_"></a> Render\(RenderOptions, int\)

Produces the Spectre.Console <xref href="Spectre.Console.Rendering.Segment" data-throw-if-not-resolved="false"></xref>s for the control's content within <code class="paramref">maxWidth</code>.

```csharp
protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
```

#### Parameters

`options` RenderOptions

`maxWidth` int

#### Returns

 IEnumerable<Segment\>

### <a id="Jumbee_Console_ProgressBar_Start"></a> Start\(\)

Starts the internal clock (for the time column) and, when a spinner or indeterminate pulse is shown,
    its animation. Idempotent.

```csharp
public void Start()
```

### <a id="Jumbee_Console_ProgressBar_Stop"></a> Stop\(\)

Freezes the clock and the animation at the current frame. Idempotent.

```csharp
public void Stop()
```

### <a id="Jumbee_Console_ProgressBar_WithFill_Jumbee_Console_Color_"></a> WithFill\(Color\)

Recolours the bar fill (a fluent shorthand for <code>Style = Style.WithFill(color)</code>); marks it an override.

```csharp
public ProgressBar WithFill(Color color)
```

#### Parameters

`color` [Color](Jumbee.Console.Color.md)

#### Returns

 [ProgressBar](Jumbee.Console.ProgressBar.md)

### <a id="Jumbee_Console_ProgressBar_WithGlyphs_Jumbee_Console_ProgressBarGlyphs_"></a> WithGlyphs\(ProgressBarGlyphs\)

Sets the bar glyphs/fill mode fluently (e.g. <code>WithGlyphs(ProgressBarGlyphs.Hatched)</code>); marks it an override.

```csharp
public ProgressBar WithGlyphs(ProgressBarGlyphs glyphs)
```

#### Parameters

`glyphs` [ProgressBarGlyphs](Jumbee.Console.ProgressBarGlyphs.md)

#### Returns

 [ProgressBar](Jumbee.Console.ProgressBar.md)

### <a id="Jumbee_Console_ProgressBar_WithGradient_Jumbee_Console_Color_Jumbee_Console_Color_"></a> WithGradient\(Color, Color\)

Makes the fill a gradient from <code class="paramref">from</code> to <code class="paramref">to</code> across the bar (a
    fluent shorthand for <code>Style = Style.WithGradient(from, to)</code>); marks the style an override.

```csharp
public ProgressBar WithGradient(Color from, Color to)
```

#### Parameters

`from` [Color](Jumbee.Console.Color.md)

`to` [Color](Jumbee.Console.Color.md)

#### Returns

 [ProgressBar](Jumbee.Console.ProgressBar.md)

### <a id="Jumbee_Console_ProgressBar_WithPadding_System_Int32_System_Int32_"></a> WithPadding\(int, int\)

Reserves <code class="paramref">left</code>/<code class="paramref">right</code> blank cells at the row's edges (a fluent
    shorthand for setting <xref href="Jumbee.Console.ProgressBar.LeftPad" data-throw-if-not-resolved="false"></xref>/<xref href="Jumbee.Console.ProgressBar.RightPad" data-throw-if-not-resolved="false"></xref>).

```csharp
public ProgressBar WithPadding(int left, int right)
```

#### Parameters

`left` int

`right` int

#### Returns

 [ProgressBar](Jumbee.Console.ProgressBar.md)

