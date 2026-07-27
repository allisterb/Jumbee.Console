# <a id="Jumbee_Console_Globe"></a> Class Globe

Namespace: [Jumbee.Console](Jumbee.Console.md)  
Assembly: Jumbee.Console.dll  

A ray-traced globe of the Earth — a shaded, colour-mapped sphere drawn into the control's buffer. Display-only
by default; spin, tilt, and zoom it from a frame or timer feed.

```csharp
public class Globe : Control, IFocusable
```

#### Inheritance

object ← 
Control ← 
[Control](Jumbee.Console.Control.md) ← 
[Globe](Jumbee.Console.Globe.md)

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

[ControlExtensions.WithAsciiBorder<Globe\>\(Globe, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithAsciiBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithBorder<Globe\>\(Globe, BorderStyle?, Color?, Color?, BorderPlacement?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_BorderPlacement\_\_), 
[ControlExtensions.WithDoubleBorder<Globe\>\(Globe, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithDoubleBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithFrame<Globe\>\(Globe, ControlFrame\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithFrame\_\_1\_\_\_0\_Jumbee\_Console\_ControlFrame\_), 
[ControlExtensions.WithFrame<Globe\>\(Globe, BorderStyle?, Offset?, Color?, Color?, string?, Color?, Color?, BorderPlacement?, BorderStyle?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithFrame\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_System\_Nullable\_ConsoleGUI\_Space\_Offset\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_String\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_BorderPlacement\_\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_), 
[ControlExtensions.WithHeavyBorder<Globe\>\(Globe, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithHeavyBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithHeight<Globe\>\(Globe, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithHeight\_\_1\_\_\_0\_System\_Int32\_), 
[ControlExtensions.WithMargin<Globe\>\(Globe, int, int, int, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithMargin\_\_1\_\_\_0\_System\_Int32\_System\_Int32\_System\_Int32\_System\_Int32\_), 
[ControlExtensions.WithMargin<Globe\>\(Globe, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithMargin\_\_1\_\_\_0\_System\_Int32\_), 
[ControlExtensions.WithNoBorder<Globe\>\(Globe\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithNoBorder\_\_1\_\_\_0\_), 
[ControlExtensions.WithRoundedBorder<Globe\>\(Globe, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithRoundedBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithScrollBarGlyphs<Globe\>\(Globe, ScrollBarGlyphs\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithScrollBarGlyphs\_\_1\_\_\_0\_Jumbee\_Console\_ScrollBarGlyphs\_), 
[ControlExtensions.WithScrollBarStyle<Globe\>\(Globe, ScrollBarStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithScrollBarStyle\_\_1\_\_\_0\_Jumbee\_Console\_ScrollBarStyle\_), 
[ControlExtensions.WithSize<Globe\>\(Globe, int?, int?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithSize\_\_1\_\_\_0\_System\_Nullable\_System\_Int32\_\_System\_Nullable\_System\_Int32\_\_), 
[ControlExtensions.WithSquareBorder<Globe\>\(Globe, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithSquareBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithTitle<Globe\>\(Globe, string\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_), 
[ControlExtensions.WithTitle<Globe\>\(Globe, string, TitleStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_Jumbee\_Console\_TitleStyle\_), 
[ControlExtensions.WithTitle<Globe\>\(Globe, string, TitlePos, TitleBorderStyle, TitleColorStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_Jumbee\_Console\_TitlePos\_Jumbee\_Console\_TitleBorderStyle\_Jumbee\_Console\_TitleColorStyle\_), 
[ControlExtensions.WithWidth<Globe\>\(Globe, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithWidth\_\_1\_\_\_0\_System\_Int32\_)

## Remarks

Two rays are shot through every character cell (upper/lower half, drawn with the ▀/▄ half-block glyphs for double
vertical resolution); where a ray hits the sphere the surface point is mapped to a lat/long and coloured from an
ocean-depth → land-elevation → polar-ice ramp, and (with <xref href="Jumbee.Console.Globe.DisplayNight" data-throw-if-not-resolved="false"></xref>) shaded by a fixed light so the
day/night terminator sweeps across as it turns. Spin it by advancing <xref href="Jumbee.Console.Globe.RotationAngle" data-throw-if-not-resolved="false"></xref> (or the
<xref href="Jumbee.Console.Globe.Spin(System.Double)" data-throw-if-not-resolved="false"></xref> helper) from a frame/timer feed; tilt and zoom the camera with <xref href="Jumbee.Console.Globe.CameraAlpha" data-throw-if-not-resolved="false"></xref>,
<xref href="Jumbee.Console.Globe.CameraBeta" data-throw-if-not-resolved="false"></xref> and <xref href="Jumbee.Console.Globe.Zoom" data-throw-if-not-resolved="false"></xref>.

<p>
The land/ocean map is generated at runtime from public-domain Natural Earth land polygons
(<code>Geo/land_110m.txt</code>). The ray-traced-sphere idea is a common one (cf. the ASCII globes by DinoZ1729 /
adamsky) but this is an independent implementation — no code or data derives from a copyleft source.
</p>

## Constructors

### <a id="Jumbee_Console_Globe__ctor"></a> Globe\(\)

Initializes a new display-only <xref href="Jumbee.Console.Globe" data-throw-if-not-resolved="false"></xref> (not focusable).

```csharp
public Globe()
```

## Properties

### <a id="Jumbee_Console_Globe_CameraAlpha"></a> CameraAlpha

Camera longitude angle (radians) in the equatorial plane — orbits the globe left/right.

```csharp
public double CameraAlpha { get; set; }
```

#### Property Value

 double

### <a id="Jumbee_Console_Globe_CameraBeta"></a> CameraBeta

Camera latitude angle (radians) — tilts the view up/down towards the poles. Clamped to ±1.5.

```csharp
public double CameraBeta { get; set; }
```

#### Property Value

 double

### <a id="Jumbee_Console_Globe_CellAspect"></a> CellAspect

Your terminal's cell height-to-width ratio, used to keep the disc circular (a monospace cell is
    taller than it is wide). Default 2.0 suits most terminals; clamped to ≥ 0.5.

```csharp
public double CellAspect { get; set; }
```

#### Property Value

 double

#### Remarks

Raise it if the globe looks vertically stretched, lower it if it looks squashed.

### <a id="Jumbee_Console_Globe_Colored"></a> Colored

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a> (the default) the surface is coloured from an ocean → land → ice ramp;
    when <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a> the globe is drawn in a single <xref href="Jumbee.Console.Globe.Foreground" data-throw-if-not-resolved="false"></xref> tone (classic monochrome).

```csharp
public bool Colored { get; set; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_Globe_DamageTracking"></a> DamageTracking

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a> (the default), the globe reports only its drawn disc as damaged each
    frame so the compositor skips the blank margins around it (opt-in partial redraw). Set <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a>
    to fall back to reporting the whole control rect — used to A/B the optimization.

```csharp
public bool DamageTracking { get; set; }
```

#### Property Value

 bool

#### Remarks

On by default because the disc is inscribed in the control, so the margins are always skippable
    whatever the globe is doing. See <xref href="Jumbee.Console.Control.TracksDamage" data-throw-if-not-resolved="false"></xref> for when partial redraw pays in
    general.

### <a id="Jumbee_Console_Globe_DisplayNight"></a> DisplayNight

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, shade the sphere by a fixed light so a day/night terminator is drawn;
    otherwise the whole globe is lit evenly. Default <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>.

```csharp
public bool DisplayNight { get; set; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_Globe_FillsFrameViewport"></a> FillsFrameViewport

Always <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>: the globe fills its frame's viewport and is never scrolled.

```csharp
protected override bool FillsFrameViewport { get; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_Globe_Foreground"></a> Foreground

Base colour used when <xref href="Jumbee.Console.Globe.Colored" data-throw-if-not-resolved="false"></xref> is <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a> (default a soft cyan).

```csharp
public Color Foreground { get; set; }
```

#### Property Value

 [Color](Jumbee.Console.Color.md)

### <a id="Jumbee_Console_Globe_HandlesInput"></a> HandlesInput

Receives keyboard input only while <xref href="Jumbee.Console.Globe.Interactive" data-throw-if-not-resolved="false"></xref>; otherwise keys pass through for navigation.

```csharp
public override bool HandlesInput { get; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_Globe_Interactive"></a> Interactive

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, the globe responds to user input: <b>drag</b> to rotate/tilt, the <b>mouse wheel</b>
to zoom, and (while focused) the <b>arrow keys</b> to spin/tilt and <b>+/-</b> to zoom (Shift = larger step).
The default (<a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a>) leaves it display-only.

```csharp
public bool Interactive { get; set; }
```

#### Property Value

 bool

#### Remarks

Enabling it makes the globe focusable, so it joins keyboard navigation.

### <a id="Jumbee_Console_Globe_RotationAngle"></a> RotationAngle

Rotation of the globe about its polar axis, in radians. Advance it each tick to spin the world.

```csharp
public double RotationAngle { get; set; }
```

#### Property Value

 double

### <a id="Jumbee_Console_Globe_TracksDamage"></a> TracksDamage

Whether partial-redraw damage tracking is enabled (see <xref href="Jumbee.Console.Globe.DamageTracking" data-throw-if-not-resolved="false"></xref>).

```csharp
protected override bool TracksDamage { get; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_Globe_WantsMouse"></a> WantsMouse

Receives mouse events only while <xref href="Jumbee.Console.Globe.Interactive" data-throw-if-not-resolved="false"></xref> (drag-rotate / wheel-zoom).

```csharp
protected override bool WantsMouse { get; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_Globe_Zoom"></a> Zoom

Camera distance from the centre; smaller is closer (a bigger globe). Clamped to ≥ 1.05.

```csharp
public double Zoom { get; set; }
```

#### Property Value

 double

## Methods

### <a id="Jumbee_Console_Globe_OnInput_ConsoleGUI_Input_InputEvent_"></a> OnInput\(InputEvent\)

Handles arrow-key spin/tilt and <code>+</code>/<code>-</code> zoom when interactive.

```csharp
protected override void OnInput(InputEvent inputEvent)
```

#### Parameters

`inputEvent` InputEvent

### <a id="Jumbee_Console_Globe_OnMouseMove_ConsoleGUI_Space_Position_"></a> OnMouseMove\(Position\)

Spins the globe (horizontal drag) and tilts the camera (vertical drag) while dragging.

```csharp
protected override void OnMouseMove(Position position)
```

#### Parameters

`position` Position

### <a id="Jumbee_Console_Globe_OnMousePress_ConsoleGUI_Space_Position_"></a> OnMousePress\(Position\)

Begins a drag and captures the mouse when interactive.

```csharp
protected override void OnMousePress(Position position)
```

#### Parameters

`position` Position

### <a id="Jumbee_Console_Globe_OnMouseRelease_ConsoleGUI_Space_Position_"></a> OnMouseRelease\(Position\)

Ends the drag and releases the mouse capture.

```csharp
protected override void OnMouseRelease(Position position)
```

#### Parameters

`position` Position

### <a id="Jumbee_Console_Globe_OnMouseWheel_ConsoleGUI_Space_Position_System_Int32_"></a> OnMouseWheel\(Position, int\)

Zooms the camera when interactive; otherwise defers to the base scroll behavior.

```csharp
protected override void OnMouseWheel(Position position, int delta)
```

#### Parameters

`position` Position

`delta` int

### <a id="Jumbee_Console_Globe_Render"></a> Render\(\)

Ray-traces the sphere and writes the shaded half-block cells to the buffer.

```csharp
protected override void Render()
```

### <a id="Jumbee_Console_Globe_SetLight_System_Double_System_Double_System_Double_System_Double_"></a> SetLight\(double, double, double, double\)

Sets the directional light (world space) used for day/night shading and its
    <code class="paramref">softness</code> (terminator sharpness; higher = harder edge). The direction is normalized.

```csharp
public void SetLight(double x, double y, double z, double softness)
```

#### Parameters

`x` double

`y` double

`z` double

`softness` double

### <a id="Jumbee_Console_Globe_Spin_System_Double_"></a> Spin\(double\)

Spins the globe about its polar axis by <code class="paramref">delta</code> radians — turning the world under a
    fixed camera and light, so the day/night terminator stays put on screen (the natural "rotating earth, fixed
    sun" look). One invalidation.

```csharp
public void Spin(double delta = 0.01)
```

#### Parameters

`delta` double

