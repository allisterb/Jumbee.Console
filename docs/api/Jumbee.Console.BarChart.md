# <a id="Jumbee_Console_BarChart"></a> Class BarChart

Namespace: [Jumbee.Console](Jumbee.Console.md)  
Assembly: Jumbee.Console.dll  

A bar chart.

```csharp
public class BarChart : RenderableControl, IFocusable
```

#### Inheritance

object ← 
Control ← 
[Control](Jumbee.Console.Control.md) ← 
[RenderableControl](Jumbee.Console.RenderableControl.md) ← 
[BarChart](Jumbee.Console.BarChart.md)

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

[ControlExtensions.WithAsciiBorder<BarChart\>\(BarChart, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithAsciiBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithBorder<BarChart\>\(BarChart, BorderStyle?, Color?, Color?, BorderPlacement?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_BorderPlacement\_\_), 
[ControlExtensions.WithDoubleBorder<BarChart\>\(BarChart, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithDoubleBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithFrame<BarChart\>\(BarChart, ControlFrame\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithFrame\_\_1\_\_\_0\_Jumbee\_Console\_ControlFrame\_), 
[ControlExtensions.WithFrame<BarChart\>\(BarChart, BorderStyle?, Offset?, Color?, Color?, string?, Color?, Color?, BorderPlacement?, BorderStyle?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithFrame\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_System\_Nullable\_ConsoleGUI\_Space\_Offset\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_String\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_BorderPlacement\_\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_), 
[ControlExtensions.WithHeavyBorder<BarChart\>\(BarChart, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithHeavyBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithHeight<BarChart\>\(BarChart, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithHeight\_\_1\_\_\_0\_System\_Int32\_), 
[ControlExtensions.WithMargin<BarChart\>\(BarChart, int, int, int, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithMargin\_\_1\_\_\_0\_System\_Int32\_System\_Int32\_System\_Int32\_System\_Int32\_), 
[ControlExtensions.WithMargin<BarChart\>\(BarChart, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithMargin\_\_1\_\_\_0\_System\_Int32\_), 
[ControlExtensions.WithNoBorder<BarChart\>\(BarChart\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithNoBorder\_\_1\_\_\_0\_), 
[ControlExtensions.WithRoundedBorder<BarChart\>\(BarChart, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithRoundedBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithScrollBarGlyphs<BarChart\>\(BarChart, ScrollBarGlyphs\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithScrollBarGlyphs\_\_1\_\_\_0\_Jumbee\_Console\_ScrollBarGlyphs\_), 
[ControlExtensions.WithScrollBarStyle<BarChart\>\(BarChart, ScrollBarStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithScrollBarStyle\_\_1\_\_\_0\_Jumbee\_Console\_ScrollBarStyle\_), 
[ControlExtensions.WithSize<BarChart\>\(BarChart, int?, int?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithSize\_\_1\_\_\_0\_System\_Nullable\_System\_Int32\_\_System\_Nullable\_System\_Int32\_\_), 
[ControlExtensions.WithSquareBorder<BarChart\>\(BarChart, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithSquareBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithTitle<BarChart\>\(BarChart, string\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_), 
[ControlExtensions.WithTitle<BarChart\>\(BarChart, string, TitleStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_Jumbee\_Console\_TitleStyle\_), 
[ControlExtensions.WithTitle<BarChart\>\(BarChart, string, TitlePos, TitleBorderStyle, TitleColorStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_Jumbee\_Console\_TitlePos\_Jumbee\_Console\_TitleBorderStyle\_Jumbee\_Console\_TitleColorStyle\_), 
[ControlExtensions.WithWidth<BarChart\>\(BarChart, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithWidth\_\_1\_\_\_0\_System\_Int32\_)

## Remarks

Based on Spectre.Console.BarChart.

## Constructors

### <a id="Jumbee_Console_BarChart__ctor_Jumbee_Console_ChartOrientation_System_ValueTuple_System_String_System_Double_Jumbee_Console_Color____"></a> BarChart\(ChartOrientation, params \(string label, double value, Color color\)\[\]\)

Initializes a new <xref href="Jumbee.Console.BarChart" data-throw-if-not-resolved="false"></xref> with the given orientation and initial items.

```csharp
public BarChart(ChartOrientation orientation, params (string label, double value, Color color)[] items)
```

#### Parameters

`orientation` [ChartOrientation](Jumbee.Console.ChartOrientation.md)

`items` \(string label, double value, [Color](Jumbee.Console.Color.md) color\)\[\]

### <a id="Jumbee_Console_BarChart__ctor_System_ValueTuple_System_String_System_Double_Jumbee_Console_Color____"></a> BarChart\(params \(string label, double value, Color color\)\[\]\)

Initializes a new horizontal <xref href="Jumbee.Console.BarChart" data-throw-if-not-resolved="false"></xref> with the given initial items.

```csharp
public BarChart(params (string label, double value, Color color)[] items)
```

#### Parameters

`items` \(string label, double value, [Color](Jumbee.Console.Color.md) color\)\[\]

## Fields

### <a id="Jumbee_Console_BarChart__bars"></a> \_bars

The bar renderables in render order.

```csharp
protected List<BarChart.IBarControl> _bars
```

#### Field Value

 List<[BarChart](Jumbee.Console.BarChart.md).[IBarControl](Jumbee.Console.BarChart.IBarControl.md)\>

### <a id="Jumbee_Console_BarChart__containerGrid"></a> \_containerGrid

The optional outer grid stacking the label above <xref href="Jumbee.Console.BarChart._grid" data-throw-if-not-resolved="false"></xref>, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> when there is no label.

```csharp
protected Grid? _containerGrid
```

#### Field Value

 Grid?

### <a id="Jumbee_Console_BarChart__grid"></a> \_grid

The grid holding the bar renderables.

```csharp
protected Grid _grid
```

#### Field Value

 Grid

### <a id="Jumbee_Console_BarChart_data"></a> data

The chart items keyed by their index.

```csharp
protected readonly Dictionary<int, BarChart.BarChartItem> data
```

#### Field Value

 Dictionary<int, [BarChart](Jumbee.Console.BarChart.md).[BarChartItem](Jumbee.Console.BarChart.BarChartItem.md)\>

### <a id="Jumbee_Console_BarChart_itemIndex"></a> itemIndex

The last-assigned item index; incremented atomically to key new items.

```csharp
protected int itemIndex
```

#### Field Value

 int

## Properties

### <a id="Jumbee_Console_BarChart_AsciiBar"></a> AsciiBar

The glyph used to draw bars in ASCII (non-Unicode) mode.

```csharp
protected char AsciiBar { get; set; }
```

#### Property Value

 char

### <a id="Jumbee_Console_BarChart_BarWidth"></a> BarWidth

The chart width in cells; setting it re-lays out all bars.

```csharp
public int? BarWidth { get; set; }
```

#### Property Value

 int?

### <a id="Jumbee_Console_BarChart_CenterLabel"></a> CenterLabel

When set to <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, centers the chart label (sets <xref href="Jumbee.Console.BarChart.LabelAlignment" data-throw-if-not-resolved="false"></xref> to <xref href="Jumbee.Console.Justify.Center" data-throw-if-not-resolved="false"></xref>).

```csharp
public bool CenterLabel { set; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_BarChart_Culture"></a> Culture

The culture used to format values, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> for the invariant culture.

```csharp
public CultureInfo? Culture { get; set; }
```

#### Property Value

 CultureInfo?

### <a id="Jumbee_Console_BarChart_Data"></a> Data

The chart's items.

```csharp
public ICollection<BarChart.BarChartItem> Data { get; }
```

#### Property Value

 ICollection<[BarChart](Jumbee.Console.BarChart.md).[BarChartItem](Jumbee.Console.BarChart.BarChartItem.md)\>

### <a id="Jumbee_Console_BarChart_HorizontalUnicodeBar"></a> HorizontalUnicodeBar

The glyph used to draw filled horizontal bars.

```csharp
protected static char HorizontalUnicodeBar { get; set; }
```

#### Property Value

 char

### <a id="Jumbee_Console_BarChart_Label"></a> Label

Gets or sets the bar chart label.

```csharp
public string? Label { get; set; }
```

#### Property Value

 string?

### <a id="Jumbee_Console_BarChart_LabelAlignment"></a> LabelAlignment

Gets or sets the bar chart label alignment.

```csharp
public Justify? LabelAlignment { get; set; }
```

#### Property Value

 [Justify](Jumbee.Console.Justify.md)?

### <a id="Jumbee_Console_BarChart_MaxValue"></a> MaxValue

The axis maximum, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to derive it from the largest item value. Never negative.

```csharp
public double? MaxValue { get; set; }
```

#### Property Value

 double?

### <a id="Jumbee_Console_BarChart_Orientation"></a> Orientation

Gets or sets the bar chart orientation.

```csharp
public ChartOrientation Orientation { get; set; }
```

#### Property Value

 [ChartOrientation](Jumbee.Console.ChartOrientation.md)

### <a id="Jumbee_Console_BarChart_RendersInteractiveState"></a> RendersInteractiveState

Whether this control's rendered output depends on interactive state (focus / mouse hover / press) — i.e.
whether <xref href="Jumbee.Console.RenderableControl.Render(Spectre.Console.Rendering.RenderOptions%2cSystem.Int32)" data-throw-if-not-resolved="false"></xref> reads <xref href="Jumbee.Console.Control.IsFocused" data-throw-if-not-resolved="false"></xref>, <code>IsMouseOver</code>,
or <code>IsMousePressed</code>.

```csharp
protected override bool RendersInteractiveState { get; }
```

#### Property Value

 bool

#### Remarks

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a>, focus/mouse changes skip the (expensive) Spectre re-render and reuse the cached
buffer — the retained-mode fast path. Defaults to <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a> (always re-render), so controls that
highlight on hover/focus keep working without opting in.

### <a id="Jumbee_Console_BarChart_ShowValues"></a> ShowValues

Whether each bar's value is shown alongside it. Defaults to <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>.

```csharp
public bool ShowValues { get; set; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_BarChart_ValueFormatter"></a> ValueFormatter

An optional custom formatter for bar values, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> to use the default culture formatting.

```csharp
public Func<double, CultureInfo, string>? ValueFormatter { get; set; }
```

#### Property Value

 Func<double, CultureInfo, string\>?

### <a id="Jumbee_Console_BarChart_VerticalUnicodeBar"></a> VerticalUnicodeBar

The glyph used to draw filled vertical bars.

```csharp
protected char VerticalUnicodeBar { get; set; }
```

#### Property Value

 char

### <a id="Jumbee_Console_BarChart_Item_System_String___"></a> this\[string\[\]\]

Sets the values of the items matching the given labels (counts must match).

```csharp
public double[] this[params string[] labels] { set; }
```

#### Property Value

 double\[\]

## Methods

### <a id="Jumbee_Console_BarChart_AddItem_System_String_System_Double_Jumbee_Console_Color_"></a> AddItem\(string, double, Color\)

Adds an item with the given label, value and colour, and returns it.

```csharp
public BarChart.BarChartItem AddItem(string label, double value, Color color)
```

#### Parameters

`label` string

`value` double

`color` [Color](Jumbee.Console.Color.md)

#### Returns

 [BarChart](Jumbee.Console.BarChart.md).[BarChartItem](Jumbee.Console.BarChart.BarChartItem.md)

### <a id="Jumbee_Console_BarChart_AddItems_System_ValueTuple_System_String_System_Double_Jumbee_Console_Color____"></a> AddItems\(params \(string label, double value, Color color\)\[\]\)

Adds multiple items and returns this chart for chaining.

```csharp
public BarChart AddItems(params (string label, double value, Color color)[] items)
```

#### Parameters

`items` \(string label, double value, [Color](Jumbee.Console.Color.md) color\)\[\]

#### Returns

 [BarChart](Jumbee.Console.BarChart.md)

### <a id="Jumbee_Console_BarChart_CreateChartElements"></a> CreateChartElements\(\)

Rebuilds the grid and bar renderables from the current data and orientation.

```csharp
protected void CreateChartElements()
```

### <a id="Jumbee_Console_BarChart_CreateChartLabel"></a> CreateChartLabel\(\)

Builds the optional container grid that stacks the chart <xref href="Jumbee.Console.BarChart.Label" data-throw-if-not-resolved="false"></xref> above the bars.

```csharp
protected void CreateChartLabel()
```

### <a id="Jumbee_Console_BarChart_Measure_Spectre_Console_Rendering_RenderOptions_System_Int32_"></a> Measure\(RenderOptions, int\)

Measures the control's desired width; the default reports <code class="paramref">maxWidth</code> as both minimum and maximum. Override for intrinsic sizing.

```csharp
protected override Measurement Measure(RenderOptions options, int maxWidth)
```

#### Parameters

`options` RenderOptions

`maxWidth` int

#### Returns

 Measurement

### <a id="Jumbee_Console_BarChart_RemoveItem_System_Int32_"></a> RemoveItem\(int\)

Removes an item by index.

```csharp
public bool RemoveItem(int index)
```

#### Parameters

`index` int

#### Returns

 bool

#### Remarks

The result is reliable only when called on the UI thread.

### <a id="Jumbee_Console_BarChart_RemoveItem_Jumbee_Console_BarChart_BarChartItem_"></a> RemoveItem\(BarChartItem\)

Removes the given item. Reliable only when called on the UI thread.

```csharp
public bool RemoveItem(BarChart.BarChartItem item)
```

#### Parameters

`item` [BarChart](Jumbee.Console.BarChart.md).[BarChartItem](Jumbee.Console.BarChart.BarChartItem.md)

#### Returns

 bool

### <a id="Jumbee_Console_BarChart_Render_Spectre_Console_Rendering_RenderOptions_System_Int32_"></a> Render\(RenderOptions, int\)

Produces the Spectre.Console <xref href="Spectre.Console.Rendering.Segment" data-throw-if-not-resolved="false"></xref>s for the control's content within <code class="paramref">maxWidth</code>.

```csharp
protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
```

#### Parameters

`options` RenderOptions

`maxWidth` int

#### Returns

 IEnumerable<Segment\>

### <a id="Jumbee_Console_BarChart_Update"></a> Update\(\)

Requests a redraw of the chart.

```csharp
public void Update()
```

### <a id="Jumbee_Console_BarChart_UpdateAllBars"></a> UpdateAllBars\(\)

Recomputes every bar's max value and size to match the current data and control dimensions.

```csharp
protected void UpdateAllBars()
```

