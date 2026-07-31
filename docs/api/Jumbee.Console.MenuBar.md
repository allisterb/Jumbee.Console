# <a id="Jumbee_Console_MenuBar"></a> Class MenuBar

Namespace: [Jumbee.Console](Jumbee.Console.md)  
Assembly: Jumbee.Console.dll  

A horizontal bar of top-level menu titles (e.g. <code>File  Edit  View</code>).

```csharp
public class MenuBar : RenderableControl, IFocusable
```

#### Inheritance

object ← 
Control ← 
[Control](Jumbee.Console.Control.md) ← 
[RenderableControl](Jumbee.Console.RenderableControl.md) ← 
[MenuBar](Jumbee.Console.MenuBar.md)

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

[ControlExtensions.WithAsciiBorder<MenuBar\>\(MenuBar, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithAsciiBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithBorder<MenuBar\>\(MenuBar, BorderStyle?, Color?, Color?, BorderPlacement?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_BorderPlacement\_\_), 
[ControlExtensions.WithDoubleBorder<MenuBar\>\(MenuBar, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithDoubleBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithFrame<MenuBar\>\(MenuBar, ControlFrame\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithFrame\_\_1\_\_\_0\_Jumbee\_Console\_ControlFrame\_), 
[ControlExtensions.WithFrame<MenuBar\>\(MenuBar, BorderStyle?, Offset?, Color?, Color?, string?, Color?, Color?, BorderPlacement?, BorderStyle?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithFrame\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_System\_Nullable\_ConsoleGUI\_Space\_Offset\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_String\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_BorderPlacement\_\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_), 
[ControlExtensions.WithHeavyBorder<MenuBar\>\(MenuBar, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithHeavyBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithHeight<MenuBar\>\(MenuBar, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithHeight\_\_1\_\_\_0\_System\_Int32\_), 
[ControlExtensions.WithMargin<MenuBar\>\(MenuBar, int, int, int, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithMargin\_\_1\_\_\_0\_System\_Int32\_System\_Int32\_System\_Int32\_System\_Int32\_), 
[ControlExtensions.WithMargin<MenuBar\>\(MenuBar, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithMargin\_\_1\_\_\_0\_System\_Int32\_), 
[ControlExtensions.WithNoBorder<MenuBar\>\(MenuBar\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithNoBorder\_\_1\_\_\_0\_), 
[ControlExtensions.WithRoundedBorder<MenuBar\>\(MenuBar, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithRoundedBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithScrollBarGlyphs<MenuBar\>\(MenuBar, ScrollBarGlyphs\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithScrollBarGlyphs\_\_1\_\_\_0\_Jumbee\_Console\_ScrollBarGlyphs\_), 
[ControlExtensions.WithScrollBarStyle<MenuBar\>\(MenuBar, ScrollBarStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithScrollBarStyle\_\_1\_\_\_0\_Jumbee\_Console\_ScrollBarStyle\_), 
[ControlExtensions.WithSize<MenuBar\>\(MenuBar, int?, int?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithSize\_\_1\_\_\_0\_System\_Nullable\_System\_Int32\_\_System\_Nullable\_System\_Int32\_\_), 
[ControlExtensions.WithSquareBorder<MenuBar\>\(MenuBar, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithSquareBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithTitle<MenuBar\>\(MenuBar, string\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_), 
[ControlExtensions.WithTitle<MenuBar\>\(MenuBar, string, TitleStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_Jumbee\_Console\_TitleStyle\_), 
[ControlExtensions.WithTitle<MenuBar\>\(MenuBar, string, TitlePos, TitleBorderStyle, TitleColorStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_Jumbee\_Console\_TitlePos\_Jumbee\_Console\_TitleBorderStyle\_Jumbee\_Console\_TitleColorStyle\_), 
[ControlExtensions.WithWidth<MenuBar\>\(MenuBar, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithWidth\_\_1\_\_\_0\_System\_Int32\_)

## Remarks

Clicking a title — or focusing the bar and pressing Enter / Down — opens that title's items as a non-modal
<xref href="Jumbee.Console.ContextMenu" data-throw-if-not-resolved="false"></xref> anchored just below it (so the rest of the app stays live; clicking elsewhere dismisses
the menu). Left/Right move between titles. The drop-downs float in the ambient <xref href="Jumbee.Console.UI.Overlay" data-throw-if-not-resolved="false"></xref>. Choosing
an item runs its <xref href="Jumbee.Console.MenuItem.Action" data-throw-if-not-resolved="false"></xref> and raises <xref href="Jumbee.Console.MenuBar.ItemActivated" data-throw-if-not-resolved="false"></xref>.

## Constructors

### <a id="Jumbee_Console_MenuBar__ctor"></a> MenuBar\(\)

Initializes an empty <xref href="Jumbee.Console.MenuBar" data-throw-if-not-resolved="false"></xref>.

```csharp
public MenuBar()
```

## Properties

### <a id="Jumbee_Console_MenuBar_HandlesInput"></a> HandlesInput

Always <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>: the bar handles its own navigation and activation keys. No opt-in
    needed — unlike the base default, this is on.

```csharp
public override bool HandlesInput { get; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_MenuBar_RendersOwnFocus"></a> RendersOwnFocus

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

### <a id="Jumbee_Console_MenuBar_WantsMouse"></a> WantsMouse

Always <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>: menu titles receive hover and click. No opt-in needed — unlike the
    base default, this is on.

```csharp
protected override bool WantsMouse { get; }
```

#### Property Value

 bool

## Methods

### <a id="Jumbee_Console_MenuBar_Add_System_String_Jumbee_Console_MenuItem___"></a> Add\(string, params MenuItem\[\]\)

Adds a top-level menu with the given title and items. Returns this for chaining.

```csharp
public MenuBar Add(string title, params MenuItem[] items)
```

#### Parameters

`title` string

`items` [MenuItem](Jumbee.Console.MenuItem.md)\[\]

#### Returns

 [MenuBar](Jumbee.Console.MenuBar.md)

### <a id="Jumbee_Console_MenuBar_GetHelpInfo"></a> GetHelpInfo\(\)

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

### <a id="Jumbee_Console_MenuBar_IntrinsicHeight"></a> IntrinsicHeight\(\)

The intrinsic, fixed height counterpart of <xref href="Jumbee.Console.Control.IntrinsicWidth" data-throw-if-not-resolved="false"></xref> (e.g. a horizontal
    <xref href="Jumbee.Console.TextLabel" data-throw-if-not-resolved="false"></xref> is one row tall). Returns 0 to fill the parent's height (the default).

```csharp
protected override int IntrinsicHeight()
```

#### Returns

 int

### <a id="Jumbee_Console_MenuBar_IntrinsicWidth"></a> IntrinsicWidth\(\)

An intrinsic, fixed width in cells this control always wants regardless of the space its parent offers, or
0 (the default) to fill the parent's width.

```csharp
protected override int IntrinsicWidth()
```

#### Returns

 int

#### Remarks

Unlike <xref href="Jumbee.Console.Control.MeasureHeight(System.Int32)" data-throw-if-not-resolved="false"></xref> — a content height honored only when the parent is unbounded — an
intrinsic size is authoritative even under a finite parent. Override on adornment controls with a genuine
fixed extent (e.g. a vertical <xref href="Jumbee.Console.TextLabel" data-throw-if-not-resolved="false"></xref>, one column wide) so a docking/layout parent can't
stretch them to fill the region.

### <a id="Jumbee_Console_MenuBar_OnClick_ConsoleGUI_Space_Position_"></a> OnClick\(Position\)

Called on a press+release on this control (relative position).

```csharp
protected override void OnClick(Position position)
```

#### Parameters

`position` Position

### <a id="Jumbee_Console_MenuBar_OnDoubleClick_ConsoleGUI_Space_Position_"></a> OnDoubleClick\(Position\)

Called on two clicks within <xref href="Jumbee.Console.Control.DoubleClickMs" data-throw-if-not-resolved="false"></xref> at the same position.

```csharp
protected override void OnDoubleClick(Position position)
```

#### Parameters

`position` Position

### <a id="Jumbee_Console_MenuBar_OnInput_ConsoleGUI_Input_InputEvent_"></a> OnInput\(InputEvent\)

Handles a keyboard input event; override on input-handling controls. The default is a no-op.

```csharp
protected override void OnInput(InputEvent inputEvent)
```

#### Parameters

`inputEvent` InputEvent

### <a id="Jumbee_Console_MenuBar_OnMouseMove_ConsoleGUI_Space_Position_"></a> OnMouseMove\(Position\)

Called as the pointer moves within the control (relative position).

```csharp
protected override void OnMouseMove(Position position)
```

#### Parameters

`position` Position

### <a id="Jumbee_Console_MenuBar_OpenActive"></a> OpenActive\(\)

Opens the currently-active title's menu in the ambient <xref href="Jumbee.Console.UI.Overlay" data-throw-if-not-resolved="false"></xref> (no-op before
    <xref href="Jumbee.Console.UI.Start(Jumbee.Console.ILayout%2cSystem.Int32%2cSystem.Int32%2cSystem.Int32%2cSystem.Boolean%2cConsoleGUI.Api.IConsole%2cJumbee.Console.IInputSource%2cSystem.Boolean)" data-throw-if-not-resolved="false"></xref> or with no menus).

```csharp
public void OpenActive()
```

### <a id="Jumbee_Console_MenuBar_Render_Spectre_Console_Rendering_RenderOptions_System_Int32_"></a> Render\(RenderOptions, int\)

Produces the Spectre.Console <xref href="Spectre.Console.Rendering.Segment" data-throw-if-not-resolved="false"></xref>s for the control's content within <code class="paramref">maxWidth</code>.

```csharp
protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
```

#### Parameters

`options` RenderOptions

`maxWidth` int

#### Returns

 IEnumerable<Segment\>

### <a id="Jumbee_Console_MenuBar_ItemActivated"></a> ItemActivated

Raised when an item in any of the bar's menus is chosen.

```csharp
public event EventHandler<MenuItem>? ItemActivated
```

#### Event Type

 EventHandler<[MenuItem](Jumbee.Console.MenuItem.md)\>?

