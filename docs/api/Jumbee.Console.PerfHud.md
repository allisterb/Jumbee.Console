# <a id="Jumbee_Console_PerfHud"></a> Class PerfHud

Namespace: [Jumbee.Console](Jumbee.Console.md)  
Assembly: Jumbee.Console.dll  

A translucent "glass" HUD showing live UI telemetry — render, terminal-write and queue-wait times (µs), CPU,
working set, allocation rate and, the headline for a no-lock design, monitor lock contentions — floating over
the app.

```csharp
public sealed class PerfHud : GlassPanel, IFocusable
```

#### Inheritance

object ← 
Control ← 
[Control](Jumbee.Console.Control.md) ← 
[GlassPanel](Jumbee.Console.GlassPanel.md) ← 
[PerfHud](Jumbee.Console.PerfHud.md)

#### Implements

[IFocusable](Jumbee.Console.IFocusable.md)

#### Inherited Members

[GlassPanel.this\[Position\]](Jumbee.Console.GlassPanel.md\#Jumbee\_Console\_GlassPanel\_Item\_ConsoleGUI\_Space\_Position\_), 
[GlassPanel.Content](Jumbee.Console.GlassPanel.md\#Jumbee\_Console\_GlassPanel\_Content), 
[GlassPanel.Tint](Jumbee.Console.GlassPanel.md\#Jumbee\_Console\_GlassPanel\_Tint), 
[GlassPanel.Factor](Jumbee.Console.GlassPanel.md\#Jumbee\_Console\_GlassPanel\_Factor), 
[GlassPanel.TextColor](Jumbee.Console.GlassPanel.md\#Jumbee\_Console\_GlassPanel\_TextColor), 
[GlassPanel.IsShown](Jumbee.Console.GlassPanel.md\#Jumbee\_Console\_GlassPanel\_IsShown), 
[GlassPanel.Show\(int, int, Overlay?\)](Jumbee.Console.GlassPanel.md\#Jumbee\_Console\_GlassPanel\_Show\_System\_Int32\_System\_Int32\_Jumbee\_Console\_Overlay\_), 
[GlassPanel.Hide\(\)](Jumbee.Console.GlassPanel.md\#Jumbee\_Console\_GlassPanel\_Hide), 
[GlassPanel.Toggle\(int, int, Overlay?\)](Jumbee.Console.GlassPanel.md\#Jumbee\_Console\_GlassPanel\_Toggle\_System\_Int32\_System\_Int32\_Jumbee\_Console\_Overlay\_), 
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
[Control.OnPaste\(string\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_OnPaste\_System\_String\_), 
[Control.Dispose\(\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_Dispose), 
[Control.Focus\(\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_Focus), 
[Control.UnFocus\(\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_UnFocus), 
[Control.CompileHelp\(\)](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_CompileHelp), 
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
[Control.emptyChar](Jumbee.Console.Control.md\#Jumbee\_Console\_Control\_emptyChar)

#### Extension Methods

[ControlExtensions.WithAsciiBorder<PerfHud\>\(PerfHud, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithAsciiBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithBorder<PerfHud\>\(PerfHud, BorderStyle?, Color?, Color?, BorderPlacement?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_BorderPlacement\_\_), 
[ControlExtensions.WithDoubleBorder<PerfHud\>\(PerfHud, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithDoubleBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithFrame<PerfHud\>\(PerfHud, ControlFrame\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithFrame\_\_1\_\_\_0\_Jumbee\_Console\_ControlFrame\_), 
[ControlExtensions.WithFrame<PerfHud\>\(PerfHud, BorderStyle?, Offset?, Color?, Color?, string?, Color?, Color?, BorderPlacement?, BorderStyle?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithFrame\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_System\_Nullable\_ConsoleGUI\_Space\_Offset\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_String\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_BorderPlacement\_\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_), 
[ControlExtensions.WithHeavyBorder<PerfHud\>\(PerfHud, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithHeavyBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithHeight<PerfHud\>\(PerfHud, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithHeight\_\_1\_\_\_0\_System\_Int32\_), 
[ControlExtensions.WithMargin<PerfHud\>\(PerfHud, int, int, int, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithMargin\_\_1\_\_\_0\_System\_Int32\_System\_Int32\_System\_Int32\_System\_Int32\_), 
[ControlExtensions.WithMargin<PerfHud\>\(PerfHud, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithMargin\_\_1\_\_\_0\_System\_Int32\_), 
[ControlExtensions.WithNoBorder<PerfHud\>\(PerfHud\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithNoBorder\_\_1\_\_\_0\_), 
[ControlExtensions.WithRoundedBorder<PerfHud\>\(PerfHud, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithRoundedBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithScrollBarGlyphs<PerfHud\>\(PerfHud, ScrollBarGlyphs\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithScrollBarGlyphs\_\_1\_\_\_0\_Jumbee\_Console\_ScrollBarGlyphs\_), 
[ControlExtensions.WithScrollBarStyle<PerfHud\>\(PerfHud, ScrollBarStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithScrollBarStyle\_\_1\_\_\_0\_Jumbee\_Console\_ScrollBarStyle\_), 
[ControlExtensions.WithSize<PerfHud\>\(PerfHud, int?, int?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithSize\_\_1\_\_\_0\_System\_Nullable\_System\_Int32\_\_System\_Nullable\_System\_Int32\_\_), 
[ControlExtensions.WithSquareBorder<PerfHud\>\(PerfHud, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithSquareBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithTitle<PerfHud\>\(PerfHud, string\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_), 
[ControlExtensions.WithTitle<PerfHud\>\(PerfHud, string, TitleStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_Jumbee\_Console\_TitleStyle\_), 
[ControlExtensions.WithTitle<PerfHud\>\(PerfHud, string, TitlePos, TitleBorderStyle, TitleColorStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_Jumbee\_Console\_TitlePos\_Jumbee\_Console\_TitleBorderStyle\_Jumbee\_Console\_TitleColorStyle\_), 
[ControlExtensions.WithWidth<PerfHud\>\(PerfHud, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithWidth\_\_1\_\_\_0\_System\_Int32\_)

## Remarks

<p>The panel is frosted glass (the app shows through as soft tinted smudges, not raw glyphs); the readout is
drawn crisply on top; it refreshes itself a few times a second while shown.</p>
<p>Timing comes from <xref href="Jumbee.Console.UI.AverageDrawTime" data-throw-if-not-resolved="false"></xref>/<xref href="Jumbee.Console.UI.AveragePaintTime" data-throw-if-not-resolved="false"></xref>; process metrics
are read directly from <xref href="System.Diagnostics.Process" data-throw-if-not-resolved="false"></xref>/<xref href="System.GC" data-throw-if-not-resolved="false"></xref>/<xref href="System.Threading.Monitor" data-throw-if-not-resolved="false"></xref> and differenced across
refreshes, so no external sampling has to be running. Show it with <xref href="Jumbee.Console.GlassPanel.Show(System.Int32%2cSystem.Int32%2cJumbee.Console.Overlay)" data-throw-if-not-resolved="false"></xref> /
<xref href="Jumbee.Console.PerfHud.ShowTopRight(System.Int32%2cJumbee.Console.Overlay)" data-throw-if-not-resolved="false"></xref>, toggle with <xref href="Jumbee.Console.GlassPanel.Toggle(System.Int32%2cSystem.Int32%2cJumbee.Console.Overlay)" data-throw-if-not-resolved="false"></xref> or <xref href="Jumbee.Console.PerfHud.RegisterToggle(System.Nullable%7bSystem.ConsoleKeyInfo%7d%2cSystem.Int32)" data-throw-if-not-resolved="false"></xref>.</p>

## Constructors

### <a id="Jumbee_Console_PerfHud__ctor_System_Nullable_Jumbee_Console_Color__System_Single_System_Boolean_"></a> PerfHud\(Color?, float, bool\)

```csharp
public PerfHud(Color? tint = null, float factor = 0.6, bool frosted = true)
```

#### Parameters

`tint` [Color](Jumbee.Console.Color.md)?

Glass colour the app beneath is tinted toward.

`factor` float

Blend strength (0 = clear, 1 = opaque tint).

`frosted` bool

Frost the app beneath to a colour blur (clean readout, content shows as soft smudges)
    rather than letting its raw glyphs bleed through and clutter the readout.

## Methods

### <a id="Jumbee_Console_PerfHud_Dispose"></a> Dispose\(\)

Unsubscribes from the paint loop and releases the base control's resources.

```csharp
public override void Dispose()
```

### <a id="Jumbee_Console_PerfHud_Refresh"></a> Refresh\(\)

Rebuilds the telemetry readout from the current metrics. Called automatically while shown.

```csharp
public void Refresh()
```

### <a id="Jumbee_Console_PerfHud_RegisterToggle_System_Nullable_System_ConsoleKeyInfo__System_Int32_"></a> RegisterToggle\(ConsoleKeyInfo?, int\)

Registers a global hotkey (default <code>Ctrl+G</code>) that toggles the HUD in the top-right corner over
    the ambient <xref href="Jumbee.Console.UI.Overlay" data-throw-if-not-resolved="false"></xref>. Call once after <xref href="Jumbee.Console.UI.Start(Jumbee.Console.ILayout%2cSystem.Int32%2cSystem.Int32%2cSystem.Int32%2cSystem.Boolean%2cConsoleGUI.Api.IConsole%2cJumbee.Console.IInputSource%2cSystem.Boolean)" data-throw-if-not-resolved="false"></xref>.

```csharp
public void RegisterToggle(ConsoleKeyInfo? key = null, int margin = 1)
```

#### Parameters

`key` ConsoleKeyInfo?

`margin` int

### <a id="Jumbee_Console_PerfHud_ShowTopRight_System_Int32_Jumbee_Console_Overlay_"></a> ShowTopRight\(int, Overlay?\)

Floats the HUD in the top-right corner of the current UI, <code class="paramref">margin</code> cells in from
    the edges.

```csharp
public void ShowTopRight(int margin = 1, Overlay? overlay = null)
```

#### Parameters

`margin` int

`overlay` [Overlay](Jumbee.Console.Overlay.md)?

