# <a id="Jumbee_Console_Canvas"></a> Class Canvas

Namespace: [Jumbee.Console](Jumbee.Console.md)  
Assembly: Jumbee.Console.dll  

A blank drawing surface on which you paint <xref href="Jumbee.Console.Drawing.IShape" data-throw-if-not-resolved="false"></xref>s (<xref href="Jumbee.Console.Drawing.Line" data-throw-if-not-resolved="false"></xref>, <xref href="Jumbee.Console.Drawing.Rectangle" data-throw-if-not-resolved="false"></xref>,
<xref href="Jumbee.Console.Drawing.Circle" data-throw-if-not-resolved="false"></xref>, <xref href="Jumbee.Console.Drawing.Points" data-throw-if-not-resolved="false"></xref>, <xref href="Jumbee.Console.Drawing.FilledLine" data-throw-if-not-resolved="false"></xref>) in an arbitrary coordinate system, rendered
with sub-cell markers (braille by default).

```csharp
public class Canvas : Control, IFocusable
```

#### Inheritance

object ← 
Control ← 
[Control](Jumbee.Console.Control.md) ← 
[Canvas](Jumbee.Console.Canvas.md)

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

[ControlExtensions.WithAsciiBorder<Canvas\>\(Canvas, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithAsciiBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithBorder<Canvas\>\(Canvas, BorderStyle?, Color?, Color?, BorderPlacement?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_BorderPlacement\_\_), 
[ControlExtensions.WithDoubleBorder<Canvas\>\(Canvas, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithDoubleBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithFrame<Canvas\>\(Canvas, ControlFrame\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithFrame\_\_1\_\_\_0\_Jumbee\_Console\_ControlFrame\_), 
[ControlExtensions.WithFrame<Canvas\>\(Canvas, BorderStyle?, Offset?, Color?, Color?, string?, Color?, Color?, BorderPlacement?, BorderStyle?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithFrame\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_System\_Nullable\_ConsoleGUI\_Space\_Offset\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_String\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_BorderPlacement\_\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_), 
[ControlExtensions.WithHeavyBorder<Canvas\>\(Canvas, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithHeavyBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithHeight<Canvas\>\(Canvas, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithHeight\_\_1\_\_\_0\_System\_Int32\_), 
[ControlExtensions.WithMargin<Canvas\>\(Canvas, int, int, int, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithMargin\_\_1\_\_\_0\_System\_Int32\_System\_Int32\_System\_Int32\_System\_Int32\_), 
[ControlExtensions.WithMargin<Canvas\>\(Canvas, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithMargin\_\_1\_\_\_0\_System\_Int32\_), 
[ControlExtensions.WithNoBorder<Canvas\>\(Canvas\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithNoBorder\_\_1\_\_\_0\_), 
[ControlExtensions.WithRoundedBorder<Canvas\>\(Canvas, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithRoundedBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithScrollBarGlyphs<Canvas\>\(Canvas, ScrollBarGlyphs\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithScrollBarGlyphs\_\_1\_\_\_0\_Jumbee\_Console\_ScrollBarGlyphs\_), 
[ControlExtensions.WithScrollBarStyle<Canvas\>\(Canvas, ScrollBarStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithScrollBarStyle\_\_1\_\_\_0\_Jumbee\_Console\_ScrollBarStyle\_), 
[ControlExtensions.WithSize<Canvas\>\(Canvas, int?, int?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithSize\_\_1\_\_\_0\_System\_Nullable\_System\_Int32\_\_System\_Nullable\_System\_Int32\_\_), 
[ControlExtensions.WithSquareBorder<Canvas\>\(Canvas, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithSquareBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithTitle<Canvas\>\(Canvas, string\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_), 
[ControlExtensions.WithTitle<Canvas\>\(Canvas, string, TitleStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_Jumbee\_Console\_TitleStyle\_), 
[ControlExtensions.WithTitle<Canvas\>\(Canvas, string, TitlePos, TitleBorderStyle, TitleColorStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_Jumbee\_Console\_TitlePos\_Jumbee\_Console\_TitleBorderStyle\_Jumbee\_Console\_TitleColorStyle\_), 
[ControlExtensions.WithWidth<Canvas\>\(Canvas, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithWidth\_\_1\_\_\_0\_System\_Int32\_)

## Remarks

Ported from Ratatui's canvas widget.

<p>Shapes accumulate in a retained list: <xref href="Jumbee.Console.Canvas.Add(Jumbee.Console.Drawing.IShape)" data-throw-if-not-resolved="false"></xref> appends to the current layer, <xref href="Jumbee.Console.Canvas.Layer" data-throw-if-not-resolved="false"></xref>
starts a new one (composited top-down per property, so a higher layer's glyph/colour wins each cell while lower
layers show through where it doesn't paint), and <xref href="Jumbee.Console.Canvas.Clear" data-throw-if-not-resolved="false"></xref> empties the surface. Layers may use different
markers via <xref href="Jumbee.Console.Canvas.Layer(Jumbee.Console.Drawing.CanvasMarker)" data-throw-if-not-resolved="false"></xref> — e.g. a <xref href="Jumbee.Console.Drawing.CanvasMarker.Block" data-throw-if-not-resolved="false"></xref> backdrop showing through a
<xref href="Jumbee.Console.Drawing.CanvasMarker.Braille" data-throw-if-not-resolved="false"></xref> overlay; <xref href="Jumbee.Console.Canvas.Marker" data-throw-if-not-resolved="false"></xref> sets the starting marker. Set the visible window
with <xref href="Jumbee.Console.Canvas.XBounds" data-throw-if-not-resolved="false"></xref>/<xref href="Jumbee.Console.Canvas.YBounds" data-throw-if-not-resolved="false"></xref> — the canvas origin is the bottom-left corner. Display-only; it
fills its container and re-fits on resize.</p>
<p>A filled/area chart — the dense look of a terminal system monitor — is one
<xref href="Jumbee.Console.Drawing.FilledLine" data-throw-if-not-resolved="false"></xref> per column with <code>fillToY</code> at the baseline; <code>Plot</code>'s bar methods take no Braille
brush, so this is the way to get it. See the Live Data guide for driving one from a running data source.</p>
<p><b>Snapshotting Braille to PNG:</b> the image renderer's default font (Consolas) has no Braille glyphs, so a
Braille canvas can rasterise as empty boxes in a PNG while rendering perfectly in the terminal and in a text
snapshot. If you see that, set <code>SnapshotImageOptions.FontFamily</code> to <code>"Cascadia Mono"</code> or
<code>"DejaVu Sans Mono"</code>.</p>

## Constructors

### <a id="Jumbee_Console_Canvas__ctor"></a> Canvas\(\)

Initializes a new display-only <xref href="Jumbee.Console.Canvas" data-throw-if-not-resolved="false"></xref> (not focusable).

```csharp
public Canvas()
```

## Properties

### <a id="Jumbee_Console_Canvas_Background"></a> Background

Background colour painted behind every cell, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> (the default) for transparent.

```csharp
public Color? Background { get; set; }
```

#### Property Value

 [Color](Jumbee.Console.Color.md)?

### <a id="Jumbee_Console_Canvas_CustomMarker"></a> CustomMarker

The glyph used when <xref href="Jumbee.Console.Canvas.Marker" data-throw-if-not-resolved="false"></xref> is <xref href="Jumbee.Console.Drawing.CanvasMarker.Custom" data-throw-if-not-resolved="false"></xref>. Default <code>•</code>.

```csharp
public char CustomMarker { get; set; }
```

#### Property Value

 char

### <a id="Jumbee_Console_Canvas_DamageTracking"></a> DamageTracking

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a> (the default), the canvas reports only the cells that actually changed since the
last render, so the compositor skips the unchanged remainder instead of re-scanning the whole panel.

```csharp
public bool DamageTracking { get; set; }
```

#### Property Value

 bool

#### Remarks

This is what makes a mostly-static canvas cheap to update — a world map whose coastline never moves but whose
few markers and labels do costs about what the markers cost, not what the map costs. That is the usual canvas
workload, so it is on by default. It is not free when the picture changes everywhere, though: the recorder
sits in the write path, so a canvas that redraws its whole area every frame pays the bookkeeping and skips
nothing — measurably a few percent worse than leaving it off. Set <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a> for that case, or
to A/B the optimization. See <xref href="Jumbee.Console.Control.TracksDamage" data-throw-if-not-resolved="false"></xref> for the cost model.

### <a id="Jumbee_Console_Canvas_HandlesInput"></a> HandlesInput

Receives keyboard input only while <xref href="Jumbee.Console.Canvas.Interactive" data-throw-if-not-resolved="false"></xref>; otherwise keys pass through for navigation.

```csharp
public override bool HandlesInput { get; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_Canvas_Interactive"></a> Interactive

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, the canvas responds to user input by panning and zooming its
<xref href="Jumbee.Console.Canvas.XBounds" data-throw-if-not-resolved="false"></xref>/<xref href="Jumbee.Console.Canvas.YBounds" data-throw-if-not-resolved="false"></xref> window. The default (<a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a>) leaves it display-only.

```csharp
public bool Interactive { get; set; }
```

#### Property Value

 bool

#### Remarks

<b>Drag</b> to pan (the content follows the cursor), the <b>mouse wheel</b> to zoom about the pointer, and
(while focused) the <b>arrow keys</b> to pan with <b>+/-</b> to zoom about the centre (Shift = larger step).
Enabling it makes the canvas focusable.

### <a id="Jumbee_Console_Canvas_Marker"></a> Marker

The marker (and thus sub-cell resolution) shapes are drawn with. Default <xref href="Jumbee.Console.Drawing.CanvasMarker.Braille" data-throw-if-not-resolved="false"></xref>.

```csharp
public CanvasMarker Marker { get; set; }
```

#### Property Value

 [CanvasMarker](Jumbee.Console.Drawing.CanvasMarker.md)

### <a id="Jumbee_Console_Canvas_TracksDamage"></a> TracksDamage

Whether partial-redraw damage tracking is enabled (see <xref href="Jumbee.Console.Canvas.DamageTracking" data-throw-if-not-resolved="false"></xref>).

```csharp
protected override bool TracksDamage { get; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_Canvas_WantsMouse"></a> WantsMouse

Receives mouse events only while <xref href="Jumbee.Console.Canvas.Interactive" data-throw-if-not-resolved="false"></xref> (drag-pan / wheel-zoom).

```csharp
protected override bool WantsMouse { get; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_Canvas_XBounds"></a> XBounds

The horizontal window (left, right) of the coordinate system mapped across the control's width. Default (0, 0).

```csharp
public (double Min, double Max) XBounds { get; set; }
```

#### Property Value

 \(double Min, double Max\)

### <a id="Jumbee_Console_Canvas_YBounds"></a> YBounds

The vertical window (bottom, top) of the coordinate system mapped across the control's height. Default (0, 0). The origin is bottom-left.

```csharp
public (double Min, double Max) YBounds { get; set; }
```

#### Property Value

 \(double Min, double Max\)

## Methods

### <a id="Jumbee_Console_Canvas_Add_Jumbee_Console_Drawing_IShape_"></a> Add\(IShape\)

Appends a shape to the current layer and redraws. Fluent.

```csharp
public Canvas Add(IShape shape)
```

#### Parameters

`shape` [IShape](Jumbee.Console.Drawing.IShape.md)

#### Returns

 [Canvas](Jumbee.Console.Canvas.md)

### <a id="Jumbee_Console_Canvas_Clear"></a> Clear\(\)

Removes every shape, layer and label, leaving a blank canvas. Fluent.

```csharp
public Canvas Clear()
```

#### Returns

 [Canvas](Jumbee.Console.Canvas.md)

### <a id="Jumbee_Console_Canvas_ClearLabels"></a> ClearLabels\(\)

Removes every label, leaving the shapes and layers intact. Fluent.

```csharp
public Canvas ClearLabels()
```

#### Returns

 [Canvas](Jumbee.Console.Canvas.md)

#### Remarks

Prefer this to <xref href="Jumbee.Console.Canvas.Clear" data-throw-if-not-resolved="false"></xref> when only the annotations change: shapes are rasterized into layers and
cached, and clearing them all forces that raster to be rebuilt from scratch on the next render. For a canvas
whose shapes are an unchanging backdrop — a world map, a floor plan, a chart's axes — with labels moving over
it, re-adding the backdrop each update is most of the frame's cost and buys nothing.

### <a id="Jumbee_Console_Canvas_Layer"></a> Layer\(\)

Starts a new layer (keeping the current marker) — shapes added after this compose on top of (and can
    show through) earlier layers. Fluent.

```csharp
public Canvas Layer()
```

#### Returns

 [Canvas](Jumbee.Console.Canvas.md)

### <a id="Jumbee_Console_Canvas_Layer_Jumbee_Console_Drawing_CanvasMarker_"></a> Layer\(CanvasMarker\)

Starts a new layer drawn with <code class="paramref">marker</code> (which becomes the current marker for this and
    subsequent layers until changed again). Lets layers mix resolutions — e.g. a <xref href="Jumbee.Console.Drawing.CanvasMarker.Block" data-throw-if-not-resolved="false"></xref>
    backdrop under a <xref href="Jumbee.Console.Drawing.CanvasMarker.Braille" data-throw-if-not-resolved="false"></xref> overlay, where the block's background shows through the
    braille glyphs. Custom markers use the current <xref href="Jumbee.Console.Canvas.CustomMarker" data-throw-if-not-resolved="false"></xref>. Fluent.

```csharp
public Canvas Layer(CanvasMarker marker)
```

#### Parameters

`marker` [CanvasMarker](Jumbee.Console.Drawing.CanvasMarker.md)

#### Returns

 [Canvas](Jumbee.Console.Canvas.md)

### <a id="Jumbee_Console_Canvas_OnInput_ConsoleGUI_Input_InputEvent_"></a> OnInput\(InputEvent\)

Handles arrow-key pan and <code>+</code>/<code>-</code> zoom when interactive.

```csharp
protected override void OnInput(InputEvent inputEvent)
```

#### Parameters

`inputEvent` InputEvent

### <a id="Jumbee_Console_Canvas_OnMouseMove_ConsoleGUI_Space_Position_"></a> OnMouseMove\(Position\)

Pans the viewport to follow the cursor while dragging.

```csharp
protected override void OnMouseMove(Position position)
```

#### Parameters

`position` Position

### <a id="Jumbee_Console_Canvas_OnMousePress_ConsoleGUI_Space_Position_"></a> OnMousePress\(Position\)

Begins a drag-pan and captures the mouse when interactive.

```csharp
protected override void OnMousePress(Position position)
```

#### Parameters

`position` Position

### <a id="Jumbee_Console_Canvas_OnMouseRelease_ConsoleGUI_Space_Position_"></a> OnMouseRelease\(Position\)

Ends the drag-pan and releases the mouse capture.

```csharp
protected override void OnMouseRelease(Position position)
```

#### Parameters

`position` Position

### <a id="Jumbee_Console_Canvas_OnMouseWheel_ConsoleGUI_Space_Position_System_Int32_"></a> OnMouseWheel\(Position, int\)

Zooms the viewport about the cursor when interactive; otherwise defers to the base scroll behavior.

```csharp
protected override void OnMouseWheel(Position position, int delta)
```

#### Parameters

`position` Position

`delta` int

### <a id="Jumbee_Console_Canvas_Print_System_Double_System_Double_System_String_System_Nullable_Jumbee_Console_Color__System_Nullable_Jumbee_Console_Color__"></a> Print\(double, double, string, Color?, Color?\)

Prints <code class="paramref">text</code> at canvas coordinate (<code class="paramref">x</code>, <code class="paramref">y</code>), with its
first character at that point and running right. Fluent.

```csharp
public Canvas Print(double x, double y, string text, Color? fg = null, Color? bg = null)
```

#### Parameters

`x` double

`y` double

`text` string

`fg` [Color](Jumbee.Console.Color.md)?

`bg` [Color](Jumbee.Console.Color.md)?

#### Returns

 [Canvas](Jumbee.Console.Canvas.md)

#### Remarks

Labels are always drawn on top of every layer (they are not part of a layer and are unaffected by the marker),
and clipped at the right edge. <code class="paramref">fg</code> defaults to white; <code class="paramref">bg</code> is transparent
when null.

### <a id="Jumbee_Console_Canvas_PrintMarkup_System_Double_System_Double_System_String_"></a> PrintMarkup\(double, double, string\)

Prints a <b>Spectre markup</b> label at canvas coordinate (<code class="paramref">x</code>, <code class="paramref">y</code>) — e.g.
<code>"[red]⚠ Outage[/] [grey]Tokyo[/]"</code> — so a single label can mix colours and decorations. Fluent.

```csharp
public Canvas PrintMarkup(double x, double y, string markup)
```

#### Parameters

`x` double

`y` double

`markup` string

#### Returns

 [Canvas](Jumbee.Console.Canvas.md)

#### Remarks

Like <xref href="Jumbee.Console.Canvas.Print(System.Double%2cSystem.Double%2cSystem.String%2cSystem.Nullable%7bJumbee.Console.Color%7d%2cSystem.Nullable%7bJumbee.Console.Color%7d)" data-throw-if-not-resolved="false"></xref> it is drawn on top of every layer and clipped
at the right edge. Use BMP symbols for icons (a single terminal cell can't hold surrogate-pair emoji). Invalid
markup falls back to literal text.

### <a id="Jumbee_Console_Canvas_Render"></a> Render\(\)

Rebuilds the composited layers when needed and blits them (with labels) to the buffer.

```csharp
protected override void Render()
```

### <a id="Jumbee_Console_Canvas_WithBackground_System_Nullable_Jumbee_Console_Color__"></a> WithBackground\(Color?\)

Sets the <xref href="Jumbee.Console.Canvas.Background" data-throw-if-not-resolved="false"></xref> and returns this canvas, for fluent chaining.

```csharp
public Canvas WithBackground(Color? background)
```

#### Parameters

`background` [Color](Jumbee.Console.Color.md)?

#### Returns

 [Canvas](Jumbee.Console.Canvas.md)

### <a id="Jumbee_Console_Canvas_WithMarker_Jumbee_Console_Drawing_CanvasMarker_"></a> WithMarker\(CanvasMarker\)

Sets the <xref href="Jumbee.Console.Canvas.Marker" data-throw-if-not-resolved="false"></xref> and returns this canvas, for fluent chaining.

```csharp
public Canvas WithMarker(CanvasMarker marker)
```

#### Parameters

`marker` [CanvasMarker](Jumbee.Console.Drawing.CanvasMarker.md)

#### Returns

 [Canvas](Jumbee.Console.Canvas.md)

### <a id="Jumbee_Console_Canvas_WithXBounds_System_Double_System_Double_"></a> WithXBounds\(double, double\)

Sets the <xref href="Jumbee.Console.Canvas.XBounds" data-throw-if-not-resolved="false"></xref> and returns this canvas, for fluent chaining.

```csharp
public Canvas WithXBounds(double min, double max)
```

#### Parameters

`min` double

`max` double

#### Returns

 [Canvas](Jumbee.Console.Canvas.md)

### <a id="Jumbee_Console_Canvas_WithYBounds_System_Double_System_Double_"></a> WithYBounds\(double, double\)

Sets the <xref href="Jumbee.Console.Canvas.YBounds" data-throw-if-not-resolved="false"></xref> and returns this canvas, for fluent chaining.

```csharp
public Canvas WithYBounds(double min, double max)
```

#### Parameters

`min` double

`max` double

#### Returns

 [Canvas](Jumbee.Console.Canvas.md)

