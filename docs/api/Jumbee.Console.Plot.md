# <a id="Jumbee_Console_Plot"></a> Class Plot

Namespace: [Jumbee.Console](Jumbee.Console.md)  
Assembly: Jumbee.Console.dll  

A line/scatter chart backed by the ConsolePlot library, rendered into the control's buffer. Add data with
<xref href="Jumbee.Console.Plot.AddSeries(System.Double%5b%5d%2cSystem.Double%5b%5d%2cConsolePlot.Drawing.Tools.PointPen)" data-throw-if-not-resolved="false"></xref> and tune the axes/grid/ticks with the <code>Configure*</code> methods.

```csharp
public class Plot : Control, IFocusable
```

#### Inheritance

object ← 
Control ← 
[Control](Jumbee.Console.Control.md) ← 
[Plot](Jumbee.Console.Plot.md)

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

[ControlExtensions.WithAsciiBorder<Plot\>\(Plot, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithAsciiBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithBorder<Plot\>\(Plot, BorderStyle?, Color?, Color?, BorderPlacement?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_BorderPlacement\_\_), 
[ControlExtensions.WithDoubleBorder<Plot\>\(Plot, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithDoubleBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithFrame<Plot\>\(Plot, ControlFrame\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithFrame\_\_1\_\_\_0\_Jumbee\_Console\_ControlFrame\_), 
[ControlExtensions.WithFrame<Plot\>\(Plot, BorderStyle?, Offset?, Color?, Color?, string?, Color?, Color?, BorderPlacement?, BorderStyle?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithFrame\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_System\_Nullable\_ConsoleGUI\_Space\_Offset\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_String\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_BorderPlacement\_\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_), 
[ControlExtensions.WithHeavyBorder<Plot\>\(Plot, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithHeavyBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithHeight<Plot\>\(Plot, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithHeight\_\_1\_\_\_0\_System\_Int32\_), 
[ControlExtensions.WithMargin<Plot\>\(Plot, int, int, int, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithMargin\_\_1\_\_\_0\_System\_Int32\_System\_Int32\_System\_Int32\_System\_Int32\_), 
[ControlExtensions.WithMargin<Plot\>\(Plot, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithMargin\_\_1\_\_\_0\_System\_Int32\_), 
[ControlExtensions.WithNoBorder<Plot\>\(Plot\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithNoBorder\_\_1\_\_\_0\_), 
[ControlExtensions.WithRoundedBorder<Plot\>\(Plot, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithRoundedBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithScrollBarGlyphs<Plot\>\(Plot, ScrollBarGlyphs\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithScrollBarGlyphs\_\_1\_\_\_0\_Jumbee\_Console\_ScrollBarGlyphs\_), 
[ControlExtensions.WithScrollBarStyle<Plot\>\(Plot, ScrollBarStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithScrollBarStyle\_\_1\_\_\_0\_Jumbee\_Console\_ScrollBarStyle\_), 
[ControlExtensions.WithSize<Plot\>\(Plot, int?, int?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithSize\_\_1\_\_\_0\_System\_Nullable\_System\_Int32\_\_System\_Nullable\_System\_Int32\_\_), 
[ControlExtensions.WithSquareBorder<Plot\>\(Plot, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithSquareBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithTitle<Plot\>\(Plot, string\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_), 
[ControlExtensions.WithTitle<Plot\>\(Plot, string, TitleStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_Jumbee\_Console\_TitleStyle\_), 
[ControlExtensions.WithTitle<Plot\>\(Plot, string, TitlePos, TitleBorderStyle, TitleColorStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_Jumbee\_Console\_TitlePos\_Jumbee\_Console\_TitleBorderStyle\_Jumbee\_Console\_TitleColorStyle\_), 
[ControlExtensions.WithWidth<Plot\>\(Plot, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithWidth\_\_1\_\_\_0\_System\_Int32\_)

## Remarks

The plot fills its container and re-draws to fit whenever the control is resized; all configuration is replayed
on each rebuild, so settings survive resizing.

<p>For data that changes every frame (a live scope, a streaming chart), add the series ONCE with
<xref href="Jumbee.Console.Plot.AddLiveSeries(System.Nullable%7bJumbee.Console.Color%7d%2cJumbee.Console.PlotBrush)" data-throw-if-not-resolved="false"></xref> and feed it via the returned <xref href="Jumbee.Console.PlotSeries" data-throw-if-not-resolved="false"></xref> handle
(<code>SetData</code>/<code>Push</code>) rather than rebuilding with <xref href="Jumbee.Console.Plot.Clear" data-throw-if-not-resolved="false"></xref> + <xref href="Jumbee.Console.Plot.AddSeries(System.Double%5b%5d%2cSystem.Double%5b%5d%2cConsolePlot.Drawing.Tools.PointPen)" data-throw-if-not-resolved="false"></xref> each frame —
the live path mutates the data in place without re-allocating the plot, and it keeps your <code>Configure*</code> styling
(which <xref href="Jumbee.Console.Plot.Clear" data-throw-if-not-resolved="false"></xref> would otherwise drop from the data list).</p>

## Constructors

### <a id="Jumbee_Console_Plot__ctor"></a> Plot\(\)

Initializes a new display-only <xref href="Jumbee.Console.Plot" data-throw-if-not-resolved="false"></xref> (not focusable), with its chrome colours and
    series palette taken from the current style theme.

```csharp
public Plot()
```

## Properties

### <a id="Jumbee_Console_Plot_AxisColor"></a> AxisColor

Colour of the axis lines. Themed from <xref href="Jumbee.Console.IStyleTheme.PlotAxis" data-throw-if-not-resolved="false"></xref> until set explicitly.

```csharp
public Color AxisColor { get; set; }
```

#### Property Value

 [Color](Jumbee.Console.Color.md)

### <a id="Jumbee_Console_Plot_Background"></a> Background

Background colour painted behind the plot, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> (the default) for transparent.
    Themed from <xref href="Jumbee.Console.IStyleTheme.PlotSurface" data-throw-if-not-resolved="false"></xref> until set explicitly.

```csharp
public Color? Background { get; set; }
```

#### Property Value

 [Color](Jumbee.Console.Color.md)?

### <a id="Jumbee_Console_Plot_DamageTracking"></a> DamageTracking

Opts into partial redraw: the plot reports only the cells each draw actually changed, so the compositor can
skip the rest of its rect. Off by default.

```csharp
public bool DamageTracking { get; set; }
```

#### Property Value

 bool

#### Remarks

<p>
Worth turning on for a plot whose figure is <em>sparse and localised</em> — a live scope trace, a small
marker set — where most of the area is empty or static from frame to frame. The grid, axes and tick labels
are rewritten every draw but with identical values, so they cost nothing in damage; only the data actually
moving is reported. A figure that fills its area (a dense heatmap) has nothing to skip and should leave this
off, since the diff is then pure overhead.
</p>
<p>
Measured on a 220×53 live scope: a trace at 15% of the Y range drops the composite from ~1340µs to ~270µs
(11660 dirty cells to 1966), roughly halving the frame; a full-scale trace at 95% saves nothing and costs
about 4%, because the recorder sits in the write path and is paid for whether or not the frame benefits.
The break-even is worse than a cell count suggests — see <xref href="Jumbee.Console.Control.TracksDamage" data-throw-if-not-resolved="false"></xref> for why.
</p>
<p>
It does not reduce terminal output: ANSI bytes per frame were unchanged (15620 vs 15739), since the renderer
already diffs per cell before emitting. Terminal load is set by data density, not by this flag.
</p>

### <a id="Jumbee_Console_Plot_FillsFrameViewport"></a> FillsFrameViewport

Always <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>: the plot fills its frame's viewport and is never scrolled.

```csharp
protected override bool FillsFrameViewport { get; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_Plot_GridColor"></a> GridColor

Colour of the background grid lines. Themed from <xref href="Jumbee.Console.IStyleTheme.PlotGrid" data-throw-if-not-resolved="false"></xref> until set
    explicitly.

```csharp
public Color GridColor { get; set; }
```

#### Property Value

 [Color](Jumbee.Console.Color.md)

### <a id="Jumbee_Console_Plot_SeriesPalette"></a> SeriesPalette

Colours cycled for series added without an explicit colour. Themed from
    <xref href="Jumbee.Console.IStyleTheme.PlotSeries" data-throw-if-not-resolved="false"></xref> until set explicitly.

```csharp
public PlotPalette SeriesPalette { get; set; }
```

#### Property Value

 [PlotPalette](Jumbee.Console.PlotPalette.md)

#### Remarks

Applies to series added <em>after</em> it changes: a series' colour is baked into its pen when it is
    added, so recolouring existing series means re-adding them.

### <a id="Jumbee_Console_Plot_TickColor"></a> TickColor

Colour of the tick marks. Themed from <xref href="Jumbee.Console.IStyleTheme.PlotTick" data-throw-if-not-resolved="false"></xref> until set explicitly.

```csharp
public Color TickColor { get; set; }
```

#### Property Value

 [Color](Jumbee.Console.Color.md)

### <a id="Jumbee_Console_Plot_TickLabelColor"></a> TickLabelColor

Colour of the numeric tick labels. Themed from <xref href="Jumbee.Console.IStyleTheme.PlotTickLabel" data-throw-if-not-resolved="false"></xref> until set
    explicitly.

```csharp
public Color TickLabelColor { get; set; }
```

#### Property Value

 [Color](Jumbee.Console.Color.md)

### <a id="Jumbee_Console_Plot_TracksDamage"></a> TracksDamage

Whether this plot reports partial damage (see <xref href="Jumbee.Console.Plot.DamageTracking" data-throw-if-not-resolved="false"></xref>).

```csharp
protected override bool TracksDamage { get; }
```

#### Property Value

 bool

## Methods

### <a id="Jumbee_Console_Plot_AddBars_System_Double___System_Double___System_Nullable_Jumbee_Console_Color__System_Double_System_Double_"></a> AddBars\(double\[\], double\[\], Color?, double, double\)

Adds a vertical bar series — each point drawn as a filled bar from <code class="paramref">baseline</code> (default 0) to
its value, with an eighth-block sub-cell top.

```csharp
public Plot AddBars(double[] xs, double[] ys, Color? color = null, double baseline = 0, double width = 0.8)
```

#### Parameters

`xs` double\[\]

`ys` double\[\]

`color` [Color](Jumbee.Console.Color.md)?

`baseline` double

`width` double

#### Returns

 [Plot](Jumbee.Console.Plot.md)

#### Remarks

<code class="paramref">color</code> defaults to the palette; <code class="paramref">width</code> is the bar width as a fraction
(0..1) of the spacing between bars.

### <a id="Jumbee_Console_Plot_AddBox_System_Double___System_Double___System_Double___System_Double___System_Double___System_Double___System_Nullable_Jumbee_Console_Color__System_Nullable_Jumbee_Console_Color__System_Double_"></a> AddBox\(double\[\], double\[\], double\[\], double\[\], double\[\], double\[\], Color?, Color?, double\)

Adds a box-and-whisker series from the five-number summary of each box — <code class="paramref">mins</code>,
<code class="paramref">q1s</code>, <code class="paramref">medians</code>, <code class="paramref">q3s</code>, <code class="paramref">maxes</code> (all the
same length as <code class="paramref">xs</code>).

```csharp
public Plot AddBox(double[] xs, double[] mins, double[] q1s, double[] medians, double[] q3s, double[] maxes, Color? color = null, Color? medianColor = null, double width = 0.6)
```

#### Parameters

`xs` double\[\]

`mins` double\[\]

`q1s` double\[\]

`medians` double\[\]

`q3s` double\[\]

`maxes` double\[\]

`color` [Color](Jumbee.Console.Color.md)?

`medianColor` [Color](Jumbee.Console.Color.md)?

`width` double

#### Returns

 [Plot](Jumbee.Console.Plot.md)

#### Remarks

<code class="paramref">color</code> defaults to the palette; <code class="paramref">medianColor</code> defaults to
<code class="paramref">color</code>; <code class="paramref">width</code> is the box width as a fraction (0..1) of the spacing.

### <a id="Jumbee_Console_Plot_AddBoxes_System_Double_____System_Double___System_Nullable_Jumbee_Console_Color__System_Nullable_Jumbee_Console_Color__System_Double_"></a> AddBoxes\(double\[\]\[\], double\[\]?, Color?, Color?, double\)

Adds a box-and-whisker series from raw data <code class="paramref">groups</code> — one box per group, with the quartiles
(min/Q1/median/Q3/max, linear-interpolation percentiles) computed here.

```csharp
public Plot AddBoxes(double[][] groups, double[]? positions = null, Color? color = null, Color? medianColor = null, double width = 0.6)
```

#### Parameters

`groups` double\[\]\[\]

`positions` double\[\]?

`color` [Color](Jumbee.Console.Color.md)?

`medianColor` [Color](Jumbee.Console.Color.md)?

`width` double

#### Returns

 [Plot](Jumbee.Console.Plot.md)

#### Remarks

Boxes are positioned at <code class="paramref">positions</code> (defaults to 1, 2, 3, …). <code class="paramref">color</code>
defaults to the palette.

### <a id="Jumbee_Console_Plot_AddCandles_System_Double___System_Double___System_Double___System_Double___System_Double___System_Nullable_Jumbee_Console_Color__System_Nullable_Jumbee_Console_Color__"></a> AddCandles\(double\[\], double\[\], double\[\], double\[\], double\[\], Color?, Color?\)

Adds an OHLC candlestick series — each point drawn as a candle (high/low wick + open/close body) coloured by
direction.

```csharp
public Plot AddCandles(double[] xs, double[] opens, double[] highs, double[] lows, double[] closes, Color? up = null, Color? down = null)
```

#### Parameters

`xs` double\[\]

`opens` double\[\]

`highs` double\[\]

`lows` double\[\]

`closes` double\[\]

`up` [Color](Jumbee.Console.Color.md)?

`down` [Color](Jumbee.Console.Color.md)?

#### Returns

 [Plot](Jumbee.Console.Plot.md)

#### Remarks

<code class="paramref">up</code> defaults to green (close ≥ open), <code class="paramref">down</code> to red.

### <a id="Jumbee_Console_Plot_AddConfusionMatrix_System_Double_____System_String___System_String___Jumbee_Console_PlotColormap_"></a> AddConfusionMatrix\(double\[\]\[\], string\[\]?, string\[\]?, PlotColormap\)

Adds a confusion matrix — an annotated heatmap of <code class="paramref">counts</code> (row = actual class top-to-bottom,
column = predicted class), each cell coloured by <code class="paramref">colormap</code> and labelled with its count.

```csharp
public Plot AddConfusionMatrix(double[][] counts, string[]? rowLabels = null, string[]? colLabels = null, PlotColormap colormap = PlotColormap.Heat)
```

#### Parameters

`counts` double\[\]\[\]

`rowLabels` string\[\]?

`colLabels` string\[\]?

`colormap` [PlotColormap](Jumbee.Console.PlotColormap.md)

#### Returns

 [Plot](Jumbee.Console.Plot.md)

#### Remarks

When <code class="paramref">rowLabels</code>/<code class="paramref">colLabels</code> are given, the class names are placed as
categorical axis ticks at the cell centres. A wrapper over <xref href="Jumbee.Console.Plot.AddHeatmap(System.Double%5b%5d%5b%5d%2cJumbee.Console.PlotColormap%2cSystem.Nullable%7bSystem.Double%7d%2cSystem.Nullable%7bSystem.Double%7d%2cSystem.Func%7bSystem.Double%2cSystem.String%7d)" data-throw-if-not-resolved="false"></xref> +
<xref href="Jumbee.Console.Plot.SetXTicks(System.Collections.Generic.IReadOnlyList%7bSystem.ValueTuple%7bSystem.Double%2cSystem.String%7d%7d)" data-throw-if-not-resolved="false"></xref>/<xref href="Jumbee.Console.Plot.SetYTicks(System.Collections.Generic.IReadOnlyList%7bSystem.ValueTuple%7bSystem.Double%2cSystem.String%7d%7d)" data-throw-if-not-resolved="false"></xref>.

### <a id="Jumbee_Console_Plot_AddErrorBars_System_Double___System_Double___System_Double___System_Nullable_Jumbee_Console_Color__System_Int32_"></a> AddErrorBars\(double\[\], double\[\], double\[\], Color?, int\)

Adds vertical error bars with symmetric error — each point (<code class="paramref">xs</code>, <code class="paramref">ys</code>)
drawn as a whisker of ±<code class="paramref">errors</code> with caps and a centre marker.

```csharp
public Plot AddErrorBars(double[] xs, double[] ys, double[] errors, Color? color = null, int capWidth = 1)
```

#### Parameters

`xs` double\[\]

`ys` double\[\]

`errors` double\[\]

`color` [Color](Jumbee.Console.Color.md)?

`capWidth` int

#### Returns

 [Plot](Jumbee.Console.Plot.md)

#### Remarks

<code class="paramref">color</code> defaults to the palette; <code class="paramref">capWidth</code> is the cap half-width in cells.

### <a id="Jumbee_Console_Plot_AddErrorBars_System_Double___System_Double___System_Double___System_Double___System_Nullable_Jumbee_Console_Color__System_Int32_"></a> AddErrorBars\(double\[\], double\[\], double\[\], double\[\], Color?, int\)

Adds vertical error bars with asymmetric error — each point (<code class="paramref">xs</code>, <code class="paramref">ys</code>)
drawn as a whisker from <code>y − errLow</code> to <code>y + errHigh</code> with caps and a centre marker.

```csharp
public Plot AddErrorBars(double[] xs, double[] ys, double[] errLows, double[] errHighs, Color? color = null, int capWidth = 1)
```

#### Parameters

`xs` double\[\]

`ys` double\[\]

`errLows` double\[\]

`errHighs` double\[\]

`color` [Color](Jumbee.Console.Color.md)?

`capWidth` int

#### Returns

 [Plot](Jumbee.Console.Plot.md)

#### Remarks

<code class="paramref">color</code> defaults to the palette; <code class="paramref">capWidth</code> is the cap half-width in cells.

### <a id="Jumbee_Console_Plot_AddGroupedBars_System_Double___System_Double_____System_Collections_Generic_IReadOnlyList_Jumbee_Console_Color__System_Double_System_Double_"></a> AddGroupedBars\(double\[\], double\[\]\[\], IReadOnlyList<Color\>?, double, double\)

Adds grouped (side-by-side) vertical bars — one sub-bar per series at each x.

```csharp
public Plot AddGroupedBars(double[] xs, double[][] series, IReadOnlyList<Color>? colors = null, double baseline = 0, double width = 0.8)
```

#### Parameters

`xs` double\[\]

`series` double\[\]\[\]

`colors` IReadOnlyList<[Color](Jumbee.Console.Color.md)\>?

`baseline` double

`width` double

#### Returns

 [Plot](Jumbee.Console.Plot.md)

#### Remarks

<code class="paramref">series</code> is one value list per series (each the same length as <code class="paramref">xs</code>).
<code class="paramref">colors</code> defaults to the palette (one per series); <code class="paramref">width</code> is the group width
as a fraction (0..1) of the spacing.

### <a id="Jumbee_Console_Plot_AddHBars_System_Double___System_Double___System_Nullable_Jumbee_Console_Color__System_Double_System_Double_"></a> AddHBars\(double\[\], double\[\], Color?, double, double\)

Adds horizontal bars — each category at a Y coordinate from <code class="paramref">positions</code> with its bar growing
along X from <code class="paramref">baseline</code> to its value.

```csharp
public Plot AddHBars(double[] positions, double[] values, Color? color = null, double baseline = 0, double width = 0.8)
```

#### Parameters

`positions` double\[\]

`values` double\[\]

`color` [Color](Jumbee.Console.Color.md)?

`baseline` double

`width` double

#### Returns

 [Plot](Jumbee.Console.Plot.md)

#### Remarks

<code class="paramref">color</code> defaults to the palette; <code class="paramref">width</code> is the bar thickness as a fraction
(0..1) of the spacing.

### <a id="Jumbee_Console_Plot_AddHeatmap_System_Double_____Jumbee_Console_PlotColormap_System_Nullable_System_Double__System_Nullable_System_Double__System_Func_System_Double_System_String__"></a> AddHeatmap\(double\[\]\[\], PlotColormap, double?, double?, Func<double, string\>?\)

Adds a heatmap: a grid of <code class="paramref">values</code> (one list per row, row 0 drawn at the top) tiled over
the plot area, each cell coloured by <code class="paramref">colormap</code>.

```csharp
public Plot AddHeatmap(double[][] values, PlotColormap colormap = PlotColormap.Viridis, double? min = null, double? max = null, Func<double, string>? cellText = null)
```

#### Parameters

`values` double\[\]\[\]

`colormap` [PlotColormap](Jumbee.Console.PlotColormap.md)

`min` double?

`max` double?

`cellText` Func<double, string\>?

#### Returns

 [Plot](Jumbee.Console.Plot.md)

#### Remarks

Values are normalised into [<code class="paramref">min</code>, <code class="paramref">max</code>], defaulting to the data's own
min/max. NaN cells are blank. Pass <code class="paramref">cellText</code> to draw each cell's value as centred text
(readable-contrast on the cell colour) — e.g. <code>v =&gt; ((int)v).ToString()</code> for a confusion matrix.

### <a id="Jumbee_Console_Plot_AddHistogram_System_Double___System_Int32_System_Nullable_Jumbee_Console_Color__"></a> AddHistogram\(double\[\], int, Color?\)

Adds a histogram of <code class="paramref">values</code> — the values are binned and each bin drawn as a touching bar
(bar height = bin count).

```csharp
public Plot AddHistogram(double[] values, int bins = 0, Color? color = null)
```

#### Parameters

`values` double\[\]

`bins` int

`color` [Color](Jumbee.Console.Color.md)?

#### Returns

 [Plot](Jumbee.Console.Plot.md)

#### Remarks

<code class="paramref">bins</code> ≤ 0 picks a bin count automatically (√n, clamped); <code class="paramref">color</code> defaults
to the palette.

### <a id="Jumbee_Console_Plot_AddLabel_System_Double_System_Double_System_String_System_Nullable_Jumbee_Console_Color__System_Nullable_Jumbee_Console_Color__Jumbee_Console_PlotLabelAlign_System_Int32_System_Int32_"></a> AddLabel\(double, double, string, Color?, Color?, PlotLabelAlign, int, int\)

Adds a text annotation anchored to the data point (<code class="paramref">x</code>, <code class="paramref">y</code>) — e.g. labelling
a candle or data point.

```csharp
public Plot AddLabel(double x, double y, string text, Color? fg = null, Color? bg = null, PlotLabelAlign align = PlotLabelAlign.Center, int dx = 0, int dy = 1)
```

#### Parameters

`x` double

`y` double

`text` string

`fg` [Color](Jumbee.Console.Color.md)?

`bg` [Color](Jumbee.Console.Color.md)?

`align` [PlotLabelAlign](Jumbee.Console.PlotLabelAlign.md)

`dx` int

`dy` int

#### Returns

 [Plot](Jumbee.Console.Plot.md)

#### Remarks

<code class="paramref">fg</code> defaults to white; <code class="paramref">bg</code> is optional (transparent when null).
<code class="paramref">dx</code>/<code class="paramref">dy</code> nudge the label in cells (dy &gt; 0 = above the point);
<code class="paramref">align</code> anchors it horizontally. Does not rescale the axes.

### <a id="Jumbee_Console_Plot_AddLiveBars_System_Nullable_Jumbee_Console_Color__System_Double_System_Double_"></a> AddLiveBars\(Color?, double, double\)

Adds a live bar series and returns a <xref href="Jumbee.Console.PlotSeries" data-throw-if-not-resolved="false"></xref> handle.

```csharp
public PlotSeries AddLiveBars(Color? color = null, double baseline = 0, double width = 0.8)
```

#### Parameters

`color` [Color](Jumbee.Console.Color.md)?

`baseline` double

`width` double

#### Returns

 [PlotSeries](Jumbee.Console.PlotSeries.md)

#### Remarks

Feed it with <xref href="Jumbee.Console.PlotSeries.SetValues(System.Double%5b%5d)" data-throw-if-not-resolved="false"></xref> (bars at x = 1, 2, 3, …) or <xref href="Jumbee.Console.PlotSeries.SetData(System.Double%5b%5d%2cSystem.Double%5b%5d)" data-throw-if-not-resolved="false"></xref>.
<code class="paramref">color</code> defaults to the palette. Starts empty.

### <a id="Jumbee_Console_Plot_AddLiveScatter_System_Nullable_Jumbee_Console_Color__Jumbee_Console_PlotBrush_"></a> AddLiveScatter\(Color?, PlotBrush\)

Adds a live <b>scatter</b> series (points drawn as markers, no connecting lines) and returns a
<xref href="Jumbee.Console.PlotSeries" data-throw-if-not-resolved="false"></xref> handle to feed it data as it arrives. The scatter counterpart of
<xref href="Jumbee.Console.Plot.AddLiveSeries(System.Nullable%7bJumbee.Console.Color%7d%2cJumbee.Console.PlotBrush)" data-throw-if-not-resolved="false"></xref>, so live streaming data and the cheaper marker draw compose.

```csharp
public PlotSeries AddLiveScatter(Color? color = null, PlotBrush brush = PlotBrush.Braille)
```

#### Parameters

`color` [Color](Jumbee.Console.Color.md)?

`brush` [PlotBrush](Jumbee.Console.PlotBrush.md)

#### Returns

 [PlotSeries](Jumbee.Console.PlotSeries.md)

#### Remarks

<code class="paramref">color</code> defaults to the palette; <code class="paramref">brush</code> sets the marker (and its sub-cell
resolution). Starts empty. Markers are markedly cheaper to draw than a line for dense/high-frequency data —
see the note on <xref href="Jumbee.Console.Plot.AddScatter(System.Double%5b%5d%2cSystem.Double%5b%5d%2cJumbee.Console.PlotBrush%2cSystem.Nullable%7bJumbee.Console.Color%7d)" data-throw-if-not-resolved="false"></xref>.

### <a id="Jumbee_Console_Plot_AddLiveSeries_System_Nullable_Jumbee_Console_Color__Jumbee_Console_PlotBrush_"></a> AddLiveSeries\(Color?, PlotBrush\)

Adds a live <b>line</b> series (consecutive points joined) and returns a <xref href="Jumbee.Console.PlotSeries" data-throw-if-not-resolved="false"></xref> handle to
feed it data as it arrives (<xref href="Jumbee.Console.PlotSeries.SetData(System.Double%5b%5d%2cSystem.Double%5b%5d)" data-throw-if-not-resolved="false"></xref>/<xref href="Jumbee.Console.PlotSeries.Push(System.Double%2cSystem.Double%2cSystem.Int32)" data-throw-if-not-resolved="false"></xref>). For unconnected
markers use <xref href="Jumbee.Console.Plot.AddLiveScatter(System.Nullable%7bJumbee.Console.Color%7d%2cJumbee.Console.PlotBrush)" data-throw-if-not-resolved="false"></xref>.

```csharp
public PlotSeries AddLiveSeries(Color? color = null, PlotBrush brush = PlotBrush.Braille)
```

#### Parameters

`color` [Color](Jumbee.Console.Color.md)?

`brush` [PlotBrush](Jumbee.Console.PlotBrush.md)

#### Returns

 [PlotSeries](Jumbee.Console.PlotSeries.md)

#### Remarks

<code class="paramref">color</code> defaults to the palette; <code class="paramref">brush</code>'s sub-cell resolution (Braille 2×4,
Quadrant 2×2, the rest 1×1) sets how smooth the line looks. Starts empty. For dense or high-frequency data
(e.g. an audio waveform) prefer <xref href="Jumbee.Console.Plot.AddLiveScatter(System.Nullable%7bJumbee.Console.Color%7d%2cJumbee.Console.PlotBrush)" data-throw-if-not-resolved="false"></xref> — see the drawing-cost note on <xref href="Jumbee.Console.Plot.AddScatter(System.Double%5b%5d%2cSystem.Double%5b%5d%2cJumbee.Console.PlotBrush%2cSystem.Nullable%7bJumbee.Console.Color%7d)" data-throw-if-not-resolved="false"></xref>.

### <a id="Jumbee_Console_Plot_AddScatter_System_Double___System_Double___Jumbee_Console_PlotBrush_System_Nullable_Jumbee_Console_Color__"></a> AddScatter\(double\[\], double\[\], PlotBrush, Color?\)

Adds a scatter series — the points drawn as markers, without connecting lines.

```csharp
public Plot AddScatter(double[] xs, double[] ys, PlotBrush brush = PlotBrush.Braille, Color? color = null)
```

#### Parameters

`xs` double\[\]

`ys` double\[\]

`brush` [PlotBrush](Jumbee.Console.PlotBrush.md)

`color` [Color](Jumbee.Console.Color.md)?

#### Returns

 [Plot](Jumbee.Console.Plot.md)

#### Remarks

The <code class="paramref">brush</code> sets the marker (and its sub-cell resolution); <code class="paramref">color</code> defaults
to the palette.

<p>Scatter is also markedly cheaper to draw than a line series (<xref href="Jumbee.Console.Plot.AddSeries(System.Double%5b%5d%2cSystem.Double%5b%5d%2cConsolePlot.Drawing.Tools.PointPen)" data-throw-if-not-resolved="false"></xref>)
for dense or high-frequency data such as an audio waveform: a line rasterizes a segment between every
consecutive pair of points, whereas scatter plots each point on its own. When the point count is high and the
connecting lines add little, prefer scatter for a large drawing-cost win.</p>

### <a id="Jumbee_Console_Plot_AddSeries_System_Double___System_Double___ConsolePlot_Drawing_Tools_PointPen_"></a> AddSeries\(double\[\], double\[\], PointPen\)

Adds a line series — consecutive points joined by straight segments (use <xref href="Jumbee.Console.Plot.AddScatter(System.Double%5b%5d%2cSystem.Double%5b%5d%2cJumbee.Console.PlotBrush%2cSystem.Nullable%7bJumbee.Console.Color%7d)" data-throw-if-not-resolved="false"></xref> for
unconnected markers). <code class="paramref">xs</code> and <code class="paramref">ys</code> must be the same length.

```csharp
public Plot AddSeries(double[] xs, double[] ys, PointPen pen = default)
```

#### Parameters

`xs` double\[\]

`ys` double\[\]

`pen` PointPen

#### Returns

 [Plot](Jumbee.Console.Plot.md)

#### Remarks

When <code class="paramref">pen</code> is left at its default a colour is taken from the control's palette (cycling by
series index) and drawn with the Braille brush.

<p>For dense or high-frequency data (e.g. an audio waveform) prefer <xref href="Jumbee.Console.Plot.AddScatter(System.Double%5b%5d%2cSystem.Double%5b%5d%2cJumbee.Console.PlotBrush%2cSystem.Nullable%7bJumbee.Console.Color%7d)" data-throw-if-not-resolved="false"></xref>: a line
rasterizes a segment between every consecutive pair of points, which is markedly more expensive than plotting
points independently.</p>

### <a id="Jumbee_Console_Plot_AddSeries_System_Double___System_Double___Jumbee_Console_PlotBrush_System_Nullable_Jumbee_Console_Color__"></a> AddSeries\(double\[\], double\[\], PlotBrush, Color?\)

Adds a line series (consecutive points joined by straight segments) drawn with the given
<code class="paramref">brush</code>. For unconnected markers use <xref href="Jumbee.Console.Plot.AddScatter(System.Double%5b%5d%2cSystem.Double%5b%5d%2cJumbee.Console.PlotBrush%2cSystem.Nullable%7bJumbee.Console.Color%7d)" data-throw-if-not-resolved="false"></xref>.

```csharp
public Plot AddSeries(double[] xs, double[] ys, PlotBrush brush, Color? color = null)
```

#### Parameters

`xs` double\[\]

`ys` double\[\]

`brush` [PlotBrush](Jumbee.Console.PlotBrush.md)

`color` [Color](Jumbee.Console.Color.md)?

#### Returns

 [Plot](Jumbee.Console.Plot.md)

#### Remarks

The <code class="paramref">brush</code>'s sub-cell resolution — Braille 2×4, Quadrant 2×2, the rest 1×1 — sets how smooth
the line looks. When <code class="paramref">color</code> is <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> a colour is taken from the control's
palette, cycling by series index.

<p>For dense or high-frequency data prefer <xref href="Jumbee.Console.Plot.AddScatter(System.Double%5b%5d%2cSystem.Double%5b%5d%2cJumbee.Console.PlotBrush%2cSystem.Nullable%7bJumbee.Console.Color%7d)" data-throw-if-not-resolved="false"></xref> — see the note there.</p>

### <a id="Jumbee_Console_Plot_AddStackedBars_System_Double___System_Double_____System_Collections_Generic_IReadOnlyList_Jumbee_Console_Color__System_Double_System_Double_"></a> AddStackedBars\(double\[\], double\[\]\[\], IReadOnlyList<Color\>?, double, double\)

Adds stacked vertical bars — the series stacked from <code class="paramref">baseline</code> at each x.

```csharp
public Plot AddStackedBars(double[] xs, double[][] series, IReadOnlyList<Color>? colors = null, double baseline = 0, double width = 0.8)
```

#### Parameters

`xs` double\[\]

`series` double\[\]\[\]

`colors` IReadOnlyList<[Color](Jumbee.Console.Color.md)\>?

`baseline` double

`width` double

#### Returns

 [Plot](Jumbee.Console.Plot.md)

#### Remarks

<code class="paramref">series</code> is one value list per series (each the same length as <code class="paramref">xs</code>).
<code class="paramref">colors</code> defaults to the palette (one per series).

### <a id="Jumbee_Console_Plot_AddStem_System_Double___System_Double___System_Nullable_Jumbee_Console_Color__System_Double_"></a> AddStem\(double\[\], double\[\], Color?, double\)

Adds a stem series — a vertical line from <code class="paramref">baseline</code> (default 0) to each point, capped with
a dot marker.

```csharp
public Plot AddStem(double[] xs, double[] ys, Color? color = null, double baseline = 0)
```

#### Parameters

`xs` double\[\]

`ys` double\[\]

`color` [Color](Jumbee.Console.Color.md)?

`baseline` double

#### Returns

 [Plot](Jumbee.Console.Plot.md)

#### Remarks

<code class="paramref">color</code> defaults to the palette.

### <a id="Jumbee_Console_Plot_ApplyTheme"></a> ApplyTheme\(\)

Re-captures this control's themed colours/glyphs from the current <xref href="Jumbee.Console.UI.StyleTheme" data-throw-if-not-resolved="false"></xref>/
<xref href="Jumbee.Console.UI.GlyphTheme" data-throw-if-not-resolved="false"></xref>. The default is a no-op for controls that don't use the theme.

```csharp
protected override void ApplyTheme()
```

#### Remarks

Called by themed controls from their constructor and again on a runtime theme switch (<xref href="Jumbee.Console.UI.SetTheme(Jumbee.Console.IStyleTheme%2cJumbee.Console.IGlyphTheme)" data-throw-if-not-resolved="false"></xref>).
Must read the themes <em>only here</em> (and in the constructor), never on the render path.

### <a id="Jumbee_Console_Plot_AutoRangeX"></a> AutoRangeX\(\)

Restores auto-scaling of the horizontal axis (undoes <xref href="Jumbee.Console.Plot.SetXRange(System.Double%2cSystem.Double)" data-throw-if-not-resolved="false"></xref>/<xref href="Jumbee.Console.Plot.SetXWindow(System.Double)" data-throw-if-not-resolved="false"></xref>).

```csharp
public Plot AutoRangeX()
```

#### Returns

 [Plot](Jumbee.Console.Plot.md)

### <a id="Jumbee_Console_Plot_AutoRangeY"></a> AutoRangeY\(\)

Restores auto-scaling of the vertical axis to the data (undoes <xref href="Jumbee.Console.Plot.SetYRange(System.Double%2cSystem.Double)" data-throw-if-not-resolved="false"></xref>).

```csharp
public Plot AutoRangeY()
```

#### Returns

 [Plot](Jumbee.Console.Plot.md)

### <a id="Jumbee_Console_Plot_Clear"></a> Clear\(\)

Removes all series and data, leaving an empty plot. Axis/grid/tick styling set via the
    <code>Configure*</code> methods is retained — clearing the data does not reset how the plot is drawn.

```csharp
public Plot Clear()
```

#### Returns

 [Plot](Jumbee.Console.Plot.md)

### <a id="Jumbee_Console_Plot_Configure_System_Action_ConsolePlot_Plot__"></a> Configure\(Action<Plot\>\)

Records an arbitrary configuration step (applied to the underlying plot on every rebuild).

```csharp
public Plot Configure(Action<Plot> configure)
```

#### Parameters

`configure` Action<Plot\>

#### Returns

 [Plot](Jumbee.Console.Plot.md)

### <a id="Jumbee_Console_Plot_ConfigureAxis_System_Action_ConsolePlot_Plotting_AxisSettings__"></a> ConfigureAxis\(Action<AxisSettings\>\)

Configures the axis lines and their captions. This styling is retained across <xref href="Jumbee.Console.Plot.Clear" data-throw-if-not-resolved="false"></xref>.

```csharp
public Plot ConfigureAxis(Action<AxisSettings> configure)
```

#### Parameters

`configure` Action<AxisSettings\>

#### Returns

 [Plot](Jumbee.Console.Plot.md)

#### Remarks

For the common cases prefer the Jumbee-colour convenience methods <xref href="Jumbee.Console.Plot.SetAxisColor(Jumbee.Console.Color)" data-throw-if-not-resolved="false"></xref> and
    <xref href="Jumbee.Console.Plot.SetAxisTitles(System.String%2cSystem.String%2cSystem.Nullable%7bJumbee.Console.Color%7d)" data-throw-if-not-resolved="false"></xref> — this raw overload exposes ConsolePlot's <xref href="System.ConsoleColor" data-throw-if-not-resolved="false"></xref> surface.
    The passed settings expose <code>IsVisible</code> (default <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>) and <code>Pen</code> — an
    immutable pen taking a full-RGB colour, so recolour with <code>a.Pen = new LinePen(a.Pen.Brush, (Color)colour)</code>.
    Also optional <code>XTitle</code>/<code>YTitle</code> captions (with <code>TitleColor</code>, a full-RGB colour).
    The captions are <b>screen-anchored</b> — <code>YTitle</code> is pinned to the top-left, <code>XTitle</code> to the
    bottom-right — so they stay put when the axes rescale, unlike a data-anchored <xref href="Jumbee.Console.Plot.AddLabel(System.Double%2cSystem.Double%2cSystem.String%2cSystem.Nullable%7bJumbee.Console.Color%7d%2cSystem.Nullable%7bJumbee.Console.Color%7d%2cJumbee.Console.PlotLabelAlign%2cSystem.Int32%2cSystem.Int32)" data-throw-if-not-resolved="false"></xref>. Hide the
    axis with <code>ConfigureAxis(a =&gt; a.IsVisible = false)</code>; label it with
    <code>ConfigureAxis(a =&gt; { a.XTitle = "time"; a.YTitle = "amplitude"; })</code>.

### <a id="Jumbee_Console_Plot_ConfigureGrid_System_Action_ConsolePlot_Plotting_GridSettings__"></a> ConfigureGrid\(Action<GridSettings\>\)

Configures the background grid. This styling is retained across <xref href="Jumbee.Console.Plot.Clear" data-throw-if-not-resolved="false"></xref>.

```csharp
public Plot ConfigureGrid(Action<GridSettings> configure)
```

#### Parameters

`configure` Action<GridSettings\>

#### Returns

 [Plot](Jumbee.Console.Plot.md)

#### Remarks

Settings expose <code>IsVisible</code> (default <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, dashed dark-gray) and <code>Pen</code>.
    Hide the grid with <code>ConfigureGrid(g =&gt; g.IsVisible = false)</code> — combine with
    <code>ConfigureTicks(t =&gt; { t.IsVisible = false; t.Labels.IsVisible = false; })</code> for a bare, chrome-free chart
    (e.g. an oscilloscope trace).

### <a id="Jumbee_Console_Plot_ConfigureTicks_System_Action_ConsolePlot_Plotting_TickSettings__"></a> ConfigureTicks\(Action<TickSettings\>\)

Configures the axis ticks and their labels. This styling is retained across <xref href="Jumbee.Console.Plot.Clear" data-throw-if-not-resolved="false"></xref>.

```csharp
public Plot ConfigureTicks(Action<TickSettings> configure)
```

#### Parameters

`configure` Action<TickSettings\>

#### Returns

 [Plot](Jumbee.Console.Plot.md)

#### Remarks

Two separate visibility flags: <code>IsVisible</code> (default <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>) draws the tick
    <em>marks</em>, while <code>Labels.IsVisible</code> (default <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>) draws the numeric tick
    <em>labels</em> — hiding one does not hide the other. Also exposed: <code>Pen</code>, per-axis
    <code>DesiredXStep</code>/<code>DesiredYStep</code> spacing, <code>CustomXTicks</code>/<code>CustomYTicks</code> (see
    <xref href="Jumbee.Console.Plot.SetXTicks(System.Collections.Generic.IReadOnlyList%7bSystem.ValueTuple%7bSystem.Double%2cSystem.String%7d%7d)" data-throw-if-not-resolved="false"></xref>), and <code>Labels</code> (<code>Color</code>, <code>Format</code>, <code>AttachToAxis</code>). Hide the marks
    with <code>ConfigureTicks(t =&gt; t.IsVisible = false)</code> and the numbers with
    <code>ConfigureTicks(t =&gt; t.Labels.IsVisible = false)</code>.

### <a id="Jumbee_Console_Plot_Render"></a> Render\(\)

Rebuilds the underlying chart when needed and blits it to the buffer.

```csharp
protected override void Render()
```

### <a id="Jumbee_Console_Plot_SetAxisColor_Jumbee_Console_Color_"></a> SetAxisColor\(Color\)

Recolours the axis lines, in a full-RGB <xref href="Jumbee.Console.Color" data-throw-if-not-resolved="false"></xref> (keeping the current brush) — a
    convenience that hides ConsolePlot's immutable pen. Retained across <xref href="Jumbee.Console.Plot.Clear" data-throw-if-not-resolved="false"></xref>.

```csharp
public Plot SetAxisColor(Color color)
```

#### Parameters

`color` [Color](Jumbee.Console.Color.md)

#### Returns

 [Plot](Jumbee.Console.Plot.md)

#### Remarks

Sets <xref href="Jumbee.Console.Plot.AxisColor" data-throw-if-not-resolved="false"></xref>, so it also marks the axis colour as explicitly chosen and a later
    theme switch will leave it alone.

### <a id="Jumbee_Console_Plot_SetAxisTitles_System_String_System_String_System_Nullable_Jumbee_Console_Color__"></a> SetAxisTitles\(string?, string?, Color?\)

Sets the screen-anchored axis captions and (optionally) their colour, in <xref href="Jumbee.Console.Color" data-throw-if-not-resolved="false"></xref>s — a
    convenience over <xref href="Jumbee.Console.Plot.ConfigureAxis(System.Action%7bConsolePlot.Plotting.AxisSettings%7d)" data-throw-if-not-resolved="false"></xref> that takes a Jumbee colour instead of a <xref href="System.ConsoleColor" data-throw-if-not-resolved="false"></xref>.
    A <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> title is left unchanged; pass an <b>empty string</b> to clear a previously-set caption.
    Retained across <xref href="Jumbee.Console.Plot.Clear" data-throw-if-not-resolved="false"></xref>, so set it once at setup rather than per frame.

```csharp
public Plot SetAxisTitles(string? xTitle = null, string? yTitle = null, Color? titleColor = null)
```

#### Parameters

`xTitle` string?

`yTitle` string?

`titleColor` [Color](Jumbee.Console.Color.md)?

#### Returns

 [Plot](Jumbee.Console.Plot.md)

#### Remarks

<code>YTitle</code> pins to the top-left, <code>XTitle</code> to the bottom-right (see <xref href="Jumbee.Console.Plot.ConfigureAxis(System.Action%7bConsolePlot.Plotting.AxisSettings%7d)" data-throw-if-not-resolved="false"></xref>).
    Colours map onto the 16 console colours (see <xref href="Jumbee.Console.Color.ToConsoleColor" data-throw-if-not-resolved="false"></xref>); an exact console colour is loss-free.

### <a id="Jumbee_Console_Plot_SetGridColor_Jumbee_Console_Color_"></a> SetGridColor\(Color\)

Recolours the background grid lines, in a full-RGB <xref href="Jumbee.Console.Color" data-throw-if-not-resolved="false"></xref> (keeping the current brush).
    Retained across <xref href="Jumbee.Console.Plot.Clear" data-throw-if-not-resolved="false"></xref>.

```csharp
public Plot SetGridColor(Color color)
```

#### Parameters

`color` [Color](Jumbee.Console.Color.md)

#### Returns

 [Plot](Jumbee.Console.Plot.md)

#### Remarks

Sets <xref href="Jumbee.Console.Plot.GridColor" data-throw-if-not-resolved="false"></xref> — see <xref href="Jumbee.Console.Plot.SetAxisColor(Jumbee.Console.Color)" data-throw-if-not-resolved="false"></xref> on theme overriding.

### <a id="Jumbee_Console_Plot_SetTickColor_Jumbee_Console_Color_System_Nullable_Jumbee_Console_Color__"></a> SetTickColor\(Color, Color?\)

Recolours the tick marks and, when <code class="paramref">labelColor</code> is given, the numeric tick labels —
    in full-RGB <xref href="Jumbee.Console.Color" data-throw-if-not-resolved="false"></xref>s (keeping the tick brush). Retained across <xref href="Jumbee.Console.Plot.Clear" data-throw-if-not-resolved="false"></xref>.

```csharp
public Plot SetTickColor(Color color, Color? labelColor = null)
```

#### Parameters

`color` [Color](Jumbee.Console.Color.md)

`labelColor` [Color](Jumbee.Console.Color.md)?

#### Returns

 [Plot](Jumbee.Console.Plot.md)

#### Remarks

Sets <xref href="Jumbee.Console.Plot.TickColor" data-throw-if-not-resolved="false"></xref>/<xref href="Jumbee.Console.Plot.TickLabelColor" data-throw-if-not-resolved="false"></xref> — see <xref href="Jumbee.Console.Plot.SetAxisColor(Jumbee.Console.Color)" data-throw-if-not-resolved="false"></xref> on theme
    overriding.

### <a id="Jumbee_Console_Plot_SetXRange_System_Double_System_Double_"></a> SetXRange\(double, double\)

Pins the horizontal axis to a fixed range; see <xref href="Jumbee.Console.Plot.SetYRange(System.Double%2cSystem.Double)" data-throw-if-not-resolved="false"></xref>.

```csharp
public Plot SetXRange(double min, double max)
```

#### Parameters

`min` double

`max` double

#### Returns

 [Plot](Jumbee.Console.Plot.md)

### <a id="Jumbee_Console_Plot_SetXTicks_System_Collections_Generic_IReadOnlyList_System_ValueTuple_System_Double_System_String___"></a> SetXTicks\(IReadOnlyList<\(double value, string label\)\>\)

Sets explicit horizontal-axis ticks (value + label) — e.g. categorical class names at cell centres.

```csharp
public Plot SetXTicks(IReadOnlyList<(double value, string label)> ticks)
```

#### Parameters

`ticks` IReadOnlyList<\(double value, string label\)\>

#### Returns

 [Plot](Jumbee.Console.Plot.md)

#### Remarks

Replaces the auto numeric ticks and keeps the data bounds unadjusted. For labels in a reserved margin (rather
than attached inside the grid), pair with <code>ConfigureTicks(t =&gt; t.Labels.AttachToAxis = false)</code> —
<xref href="Jumbee.Console.Plot.AddConfusionMatrix(System.Double%5b%5d%5b%5d%2cSystem.String%5b%5d%2cSystem.String%5b%5d%2cJumbee.Console.PlotColormap)" data-throw-if-not-resolved="false"></xref> does this for you.

### <a id="Jumbee_Console_Plot_SetXWindow_System_Double_"></a> SetXWindow\(double\)

Makes the horizontal axis a sliding window of <code class="paramref">width</code> units — it shows the most recent
<code>[max(0, dataMax − width), dataMax]</code> of a monotonic (time-like) series.

```csharp
public Plot SetXWindow(double width)
```

#### Parameters

`width` double

#### Returns

 [Plot](Jumbee.Console.Plot.md)

#### Remarks

The axis only advances rightward and never shows x &lt; 0. Ideal for streaming/financial data.
<xref href="Jumbee.Console.Plot.AutoRangeX" data-throw-if-not-resolved="false"></xref> restores auto.

### <a id="Jumbee_Console_Plot_SetYRange_System_Double_System_Double_"></a> SetYRange\(double, double\)

Pins the vertical axis to a fixed <code class="paramref">min</code>..<code class="paramref">max</code> range.

```csharp
public Plot SetYRange(double min, double max)
```

#### Parameters

`min` double

`max` double

#### Returns

 [Plot](Jumbee.Console.Plot.md)

#### Remarks

Live updates then move only the data (values outside the range are clipped) instead of the axis rescaling to
the data each frame. Call once before streaming; <xref href="Jumbee.Console.Plot.AutoRangeY" data-throw-if-not-resolved="false"></xref> restores auto-scaling.

### <a id="Jumbee_Console_Plot_SetYTicks_System_Collections_Generic_IReadOnlyList_System_ValueTuple_System_Double_System_String___"></a> SetYTicks\(IReadOnlyList<\(double value, string label\)\>\)

Sets explicit vertical-axis ticks (value + label). See <xref href="Jumbee.Console.Plot.SetXTicks(System.Collections.Generic.IReadOnlyList%7bSystem.ValueTuple%7bSystem.Double%2cSystem.String%7d%7d)" data-throw-if-not-resolved="false"></xref>.

```csharp
public Plot SetYTicks(IReadOnlyList<(double value, string label)> ticks)
```

#### Parameters

`ticks` IReadOnlyList<\(double value, string label\)\>

#### Returns

 [Plot](Jumbee.Console.Plot.md)

### <a id="Jumbee_Console_Plot_WithBackground_System_Nullable_Jumbee_Console_Color__"></a> WithBackground\(Color?\)

Sets the <xref href="Jumbee.Console.Plot.Background" data-throw-if-not-resolved="false"></xref> colour and returns this plot, for fluent chaining.

```csharp
public Plot WithBackground(Color? background)
```

#### Parameters

`background` [Color](Jumbee.Console.Color.md)?

#### Returns

 [Plot](Jumbee.Console.Plot.md)

