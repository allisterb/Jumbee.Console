# <a id="Jumbee_Console_Badge"></a> Class Badge

Namespace: [Jumbee.Console](Jumbee.Console.md)  
Assembly: Jumbee.Console.dll  

A small inline status pill — short text on a filled background with a little horizontal padding (e.g.
<code>200 OK</code>, <code>read-only</code>, a method tag).

```csharp
public class Badge : RenderableControl, IFocusable
```

#### Inheritance

object ← 
Control ← 
[Control](Jumbee.Console.Control.md) ← 
[RenderableControl](Jumbee.Console.RenderableControl.md) ← 
[Badge](Jumbee.Console.Badge.md)

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
[RenderableControl.ApplyEnabledToFocus\(bool\)](Jumbee.Console.RenderableControl.md\#Jumbee\_Console\_RenderableControl\_ApplyEnabledToFocus\_System\_Boolean\_), 
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

[ControlExtensions.WithAsciiBorder<Badge\>\(Badge, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithAsciiBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithBorder<Badge\>\(Badge, BorderStyle?, Color?, Color?, BorderPlacement?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_BorderPlacement\_\_), 
[ControlExtensions.WithDoubleBorder<Badge\>\(Badge, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithDoubleBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithFrame<Badge\>\(Badge, ControlFrame\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithFrame\_\_1\_\_\_0\_Jumbee\_Console\_ControlFrame\_), 
[ControlExtensions.WithFrame<Badge\>\(Badge, BorderStyle?, Offset?, Color?, Color?, string?, Color?, Color?, BorderPlacement?, BorderStyle?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithFrame\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_System\_Nullable\_ConsoleGUI\_Space\_Offset\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_String\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_BorderPlacement\_\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_), 
[ControlExtensions.WithHeavyBorder<Badge\>\(Badge, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithHeavyBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithHeight<Badge\>\(Badge, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithHeight\_\_1\_\_\_0\_System\_Int32\_), 
[ControlExtensions.WithMargin<Badge\>\(Badge, int, int, int, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithMargin\_\_1\_\_\_0\_System\_Int32\_System\_Int32\_System\_Int32\_System\_Int32\_), 
[ControlExtensions.WithMargin<Badge\>\(Badge, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithMargin\_\_1\_\_\_0\_System\_Int32\_), 
[ControlExtensions.WithNoBorder<Badge\>\(Badge\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithNoBorder\_\_1\_\_\_0\_), 
[ControlExtensions.WithRoundedBorder<Badge\>\(Badge, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithRoundedBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithScrollBarGlyphs<Badge\>\(Badge, ScrollBarGlyphs\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithScrollBarGlyphs\_\_1\_\_\_0\_Jumbee\_Console\_ScrollBarGlyphs\_), 
[ControlExtensions.WithScrollBarStyle<Badge\>\(Badge, ScrollBarStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithScrollBarStyle\_\_1\_\_\_0\_Jumbee\_Console\_ScrollBarStyle\_), 
[ControlExtensions.WithSize<Badge\>\(Badge, int?, int?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithSize\_\_1\_\_\_0\_System\_Nullable\_System\_Int32\_\_System\_Nullable\_System\_Int32\_\_), 
[ControlExtensions.WithSquareBorder<Badge\>\(Badge, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithSquareBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithTitle<Badge\>\(Badge, string\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_), 
[ControlExtensions.WithTitle<Badge\>\(Badge, string, TitleStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_Jumbee\_Console\_TitleStyle\_), 
[ControlExtensions.WithTitle<Badge\>\(Badge, string, TitlePos, TitleBorderStyle, TitleColorStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_Jumbee\_Console\_TitlePos\_Jumbee\_Console\_TitleBorderStyle\_Jumbee\_Console\_TitleColorStyle\_), 
[ControlExtensions.WithWidth<Badge\>\(Badge, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithWidth\_\_1\_\_\_0\_System\_Int32\_)

## Remarks

Non-interactive and fixed-width (sizes to its text + padding). Use a <xref href="Jumbee.Console.BadgeVariant" data-throw-if-not-resolved="false"></xref> for a themed
scheme, or pass an explicit <xref href="Jumbee.Console.Badge.Style" data-throw-if-not-resolved="false"></xref>.

## Constructors

### <a id="Jumbee_Console_Badge__ctor_System_String_Jumbee_Console_BadgeVariant_"></a> Badge\(string, BadgeVariant\)

Initializes a new <xref href="Jumbee.Console.Badge" data-throw-if-not-resolved="false"></xref> with the given text and themed <xref href="Jumbee.Console.BadgeVariant" data-throw-if-not-resolved="false"></xref>.

```csharp
public Badge(string text, BadgeVariant variant = BadgeVariant.Default)
```

#### Parameters

`text` string

`variant` [BadgeVariant](Jumbee.Console.BadgeVariant.md)

### <a id="Jumbee_Console_Badge__ctor_System_String_Jumbee_Console_Style_"></a> Badge\(string, Style\)

Creates a badge with an explicit style (overrides the themed variant).

```csharp
public Badge(string text, Style style)
```

#### Parameters

`text` string

`style` [Style](Jumbee.Console.Style.md)

## Properties

### <a id="Jumbee_Console_Badge_Padding"></a> Padding

Spaces added on each side of the text. Defaults to 1.

```csharp
public int Padding { get; set; }
```

#### Property Value

 int

### <a id="Jumbee_Console_Badge_RendersInteractiveState"></a> RendersInteractiveState

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

### <a id="Jumbee_Console_Badge_Style"></a> Style

The fill/text style. Defaults to the <xref href="Jumbee.Console.Badge.Variant" data-throw-if-not-resolved="false"></xref>'s themed scheme.

```csharp
public Style Style { get; set; }
```

#### Property Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Badge_Text"></a> Text

The badge's label text.

```csharp
public string Text { get; set; }
```

#### Property Value

 string

### <a id="Jumbee_Console_Badge_Variant"></a> Variant

The themed colour scheme; ignored once <xref href="Jumbee.Console.Badge.Style" data-throw-if-not-resolved="false"></xref> is set explicitly.

```csharp
public BadgeVariant Variant { get; set; }
```

#### Property Value

 [BadgeVariant](Jumbee.Console.BadgeVariant.md)

## Methods

### <a id="Jumbee_Console_Badge_ApplyTheme"></a> ApplyTheme\(\)

Re-captures this control's themed colours/glyphs from the current <xref href="Jumbee.Console.UI.StyleTheme" data-throw-if-not-resolved="false"></xref>/
<xref href="Jumbee.Console.UI.GlyphTheme" data-throw-if-not-resolved="false"></xref>. The default is a no-op for controls that don't use the theme.

```csharp
protected override void ApplyTheme()
```

#### Remarks

Called by themed controls from their constructor and again on a runtime theme switch (<xref href="Jumbee.Console.UI.SetTheme(Jumbee.Console.IStyleTheme%2cJumbee.Console.IGlyphTheme)" data-throw-if-not-resolved="false"></xref>).
Must read the themes <em>only here</em> (and in the constructor), never on the render path.

### <a id="Jumbee_Console_Badge_IntrinsicHeight"></a> IntrinsicHeight\(\)

The intrinsic, fixed height counterpart of <xref href="Jumbee.Console.Control.IntrinsicWidth" data-throw-if-not-resolved="false"></xref> (e.g. a horizontal
    <xref href="Jumbee.Console.TextLabel" data-throw-if-not-resolved="false"></xref> is one row tall). Returns 0 to fill the parent's height (the default).

```csharp
protected override int IntrinsicHeight()
```

#### Returns

 int

### <a id="Jumbee_Console_Badge_IntrinsicWidth"></a> IntrinsicWidth\(\)

An intrinsic, fixed width in cells this control always wants regardless of the space its parent offers, or
0 (the default) to fill the parent's width.

```csharp
protected override int IntrinsicWidth()
```

#### Returns

 int

#### Remarks

Unlike <xref href="Jumbee.Console.IScrollable.MeasureHeight(System.Int32)" data-throw-if-not-resolved="false"></xref> — a content height honored only when the parent is unbounded —
an intrinsic size is authoritative even under a finite parent. Override on adornment controls with a genuine
fixed extent (e.g. a vertical <xref href="Jumbee.Console.TextLabel" data-throw-if-not-resolved="false"></xref>, one column wide) so a docking/layout parent can't
stretch them to fill the region.

### <a id="Jumbee_Console_Badge_Render_Spectre_Console_Rendering_RenderOptions_System_Int32_"></a> Render\(RenderOptions, int\)

Produces the Spectre.Console <xref href="Spectre.Console.Rendering.Segment" data-throw-if-not-resolved="false"></xref>s for the control's content within <code class="paramref">maxWidth</code>.

```csharp
protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
```

#### Parameters

`options` RenderOptions

`maxWidth` int

#### Returns

 IEnumerable<Segment\>

