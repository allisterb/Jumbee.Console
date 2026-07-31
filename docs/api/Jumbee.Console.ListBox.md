# <a id="Jumbee_Console_ListBox"></a> Class ListBox

Namespace: [Jumbee.Console](Jumbee.Console.md)  
Assembly: Jumbee.Console.dll  

Displays a flat list of items and allows user input navigation and selection.

```csharp
public class ListBox : RenderableControl, IFocusable
```

#### Inheritance

object ← 
Control ← 
[Control](Jumbee.Console.Control.md) ← 
[RenderableControl](Jumbee.Console.RenderableControl.md) ← 
[ListBox](Jumbee.Console.ListBox.md)

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

[ControlExtensions.WithAsciiBorder<ListBox\>\(ListBox, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithAsciiBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithBorder<ListBox\>\(ListBox, BorderStyle?, Color?, Color?, BorderPlacement?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_BorderPlacement\_\_), 
[ControlExtensions.WithDoubleBorder<ListBox\>\(ListBox, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithDoubleBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithFrame<ListBox\>\(ListBox, ControlFrame\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithFrame\_\_1\_\_\_0\_Jumbee\_Console\_ControlFrame\_), 
[ControlExtensions.WithFrame<ListBox\>\(ListBox, BorderStyle?, Offset?, Color?, Color?, string?, Color?, Color?, BorderPlacement?, BorderStyle?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithFrame\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_System\_Nullable\_ConsoleGUI\_Space\_Offset\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_String\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_BorderPlacement\_\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_), 
[ControlExtensions.WithHeavyBorder<ListBox\>\(ListBox, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithHeavyBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithHeight<ListBox\>\(ListBox, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithHeight\_\_1\_\_\_0\_System\_Int32\_), 
[ControlExtensions.WithMargin<ListBox\>\(ListBox, int, int, int, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithMargin\_\_1\_\_\_0\_System\_Int32\_System\_Int32\_System\_Int32\_System\_Int32\_), 
[ControlExtensions.WithMargin<ListBox\>\(ListBox, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithMargin\_\_1\_\_\_0\_System\_Int32\_), 
[ControlExtensions.WithNoBorder<ListBox\>\(ListBox\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithNoBorder\_\_1\_\_\_0\_), 
[ControlExtensions.WithRoundedBorder<ListBox\>\(ListBox, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithRoundedBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithScrollBarGlyphs<ListBox\>\(ListBox, ScrollBarGlyphs\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithScrollBarGlyphs\_\_1\_\_\_0\_Jumbee\_Console\_ScrollBarGlyphs\_), 
[ControlExtensions.WithScrollBarStyle<ListBox\>\(ListBox, ScrollBarStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithScrollBarStyle\_\_1\_\_\_0\_Jumbee\_Console\_ScrollBarStyle\_), 
[ControlExtensions.WithSize<ListBox\>\(ListBox, int?, int?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithSize\_\_1\_\_\_0\_System\_Nullable\_System\_Int32\_\_System\_Nullable\_System\_Int32\_\_), 
[ControlExtensions.WithSquareBorder<ListBox\>\(ListBox, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithSquareBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithTitle<ListBox\>\(ListBox, string\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_), 
[ControlExtensions.WithTitle<ListBox\>\(ListBox, string, TitleStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_Jumbee\_Console\_TitleStyle\_), 
[ControlExtensions.WithTitle<ListBox\>\(ListBox, string, TitlePos, TitleBorderStyle, TitleColorStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_Jumbee\_Console\_TitlePos\_Jumbee\_Console\_TitleBorderStyle\_Jumbee\_Console\_TitleColorStyle\_), 
[ControlExtensions.WithWidth<ListBox\>\(ListBox, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithWidth\_\_1\_\_\_0\_System\_Int32\_)

## Constructors

### <a id="Jumbee_Console_ListBox__ctor"></a> ListBox\(\)

Initializes an empty <xref href="Jumbee.Console.ListBox" data-throw-if-not-resolved="false"></xref>.

```csharp
public ListBox()
```

### <a id="Jumbee_Console_ListBox__ctor_Spectre_Console_Rendering_IRenderable___"></a> ListBox\(params IRenderable\[\]\)

Initializes a <xref href="Jumbee.Console.ListBox" data-throw-if-not-resolved="false"></xref> populated with the given renderable items.

```csharp
public ListBox(params IRenderable[] items)
```

#### Parameters

`items` IRenderable\[\]

### <a id="Jumbee_Console_ListBox__ctor_System_Collections_Generic_IEnumerable_System_String__"></a> ListBox\(params IEnumerable<string\>\)

Initializes a <xref href="Jumbee.Console.ListBox" data-throw-if-not-resolved="false"></xref> populated with the given text items (an array, a
    <xref href="System.Collections.Generic.List%601" data-throw-if-not-resolved="false"></xref>, a LINQ query, or individual strings).

```csharp
public ListBox(params IEnumerable<string> items)
```

#### Parameters

`items` IEnumerable<string\>

## Properties

### <a id="Jumbee_Console_ListBox_ContextMenu"></a> ContextMenu

An optional menu shown when a row is right-clicked. Left <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> = no menu.

```csharp
public ContextMenu? ContextMenu { get; set; }
```

#### Property Value

 [ContextMenu](Jumbee.Console.ContextMenu.md)?

#### Remarks

The right-click first selects that row and raises <xref href="Jumbee.Console.ListBox.ContextMenuOpening" data-throw-if-not-resolved="false"></xref> (with the item),
    then the menu floats at the pointer. Item handlers can read <xref href="Jumbee.Console.ListBox.SelectedItem" data-throw-if-not-resolved="false"></xref> to act on the
    right-clicked row.

### <a id="Jumbee_Console_ListBox_HandlesInput"></a> HandlesInput

Always <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>: the list handles its own selection keys. No opt-in needed — unlike
    the base default, this is on.

```csharp
public override bool HandlesInput { get; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_ListBox_HighlightFullWidth"></a> HighlightFullWidth

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, the selection style fills the entire row width, not just the item's own
    width — so a selected row reads as a full-width bar. Defaults to <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a>.

```csharp
public bool HighlightFullWidth { get; set; }
```

#### Property Value

 bool

#### Remarks

Most visible in <xref href="Jumbee.Console.SelectionStyle.Highlight" data-throw-if-not-resolved="false"></xref> mode (the selection background spans the
    row).

### <a id="Jumbee_Console_ListBox_Items"></a> Items

The items currently in the list.

```csharp
public ICollection<ListBox.ListBoxItem> Items { get; }
```

#### Property Value

 ICollection<[ListBox](Jumbee.Console.ListBox.md).[ListBoxItem](Jumbee.Console.ListBox.ListBoxItem.md)\>

### <a id="Jumbee_Console_ListBox_RendersOwnFocus"></a> RendersOwnFocus

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

### <a id="Jumbee_Console_ListBox_SelectedBackgroundColor"></a> SelectedBackgroundColor

Background of the selected row. Defaults to the theme's <xref href="Jumbee.Console.IStyleTheme.Selection" data-throw-if-not-resolved="false"></xref>.

```csharp
public Color? SelectedBackgroundColor { get; set; }
```

#### Property Value

 [Color](Jumbee.Console.Color.md)?

### <a id="Jumbee_Console_ListBox_SelectedForegroundColor"></a> SelectedForegroundColor

Foreground of the selected row. Defaults to the theme's <xref href="Jumbee.Console.IStyleTheme.Selection" data-throw-if-not-resolved="false"></xref>.

```csharp
public Color? SelectedForegroundColor { get; set; }
```

#### Property Value

 [Color](Jumbee.Console.Color.md)?

### <a id="Jumbee_Console_ListBox_SelectedIndex"></a> SelectedIndex

The index of the highlighted item (in item order), clamped to the item range.

```csharp
public int SelectedIndex { get; set; }
```

#### Property Value

 int

### <a id="Jumbee_Console_ListBox_SelectedItem"></a> SelectedItem

The highlighted item, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> when empty.

```csharp
public ListBox.ListBoxItem? SelectedItem { get; }
```

#### Property Value

 [ListBox](Jumbee.Console.ListBox.md).[ListBoxItem](Jumbee.Console.ListBox.ListBoxItem.md)?

### <a id="Jumbee_Console_ListBox_SelectionStyle"></a> SelectionStyle

How the selected row is indicated — highlight / underline / caret. Defaults to the theme's
    <xref href="Jumbee.Console.IStyleTheme.SelectionStyle" data-throw-if-not-resolved="false"></xref>.

```csharp
public SelectionStyle SelectionStyle { get; set; }
```

#### Property Value

 [SelectionStyle](Jumbee.Console.SelectionStyle.md)

## Methods

### <a id="Jumbee_Console_ListBox_AddItem_Spectre_Console_Rendering_IRenderable_"></a> AddItem\(IRenderable\)

Appends a single renderable item and returns it.

```csharp
public ListBox.ListBoxItem AddItem(IRenderable item)
```

#### Parameters

`item` IRenderable

#### Returns

 [ListBox](Jumbee.Console.ListBox.md).[ListBoxItem](Jumbee.Console.ListBox.ListBoxItem.md)

### <a id="Jumbee_Console_ListBox_AddItem_System_String_System_Nullable_Jumbee_Console_Color__System_Nullable_Jumbee_Console_Color__"></a> AddItem\(string, Color?, Color?\)

Appends a single text item with optional foreground/background colours and returns it.

```csharp
public ListBox.ListBoxItem AddItem(string text, Color? foreground = null, Color? background = null)
```

#### Parameters

`text` string

`foreground` [Color](Jumbee.Console.Color.md)?

`background` [Color](Jumbee.Console.Color.md)?

#### Returns

 [ListBox](Jumbee.Console.ListBox.md).[ListBoxItem](Jumbee.Console.ListBox.ListBoxItem.md)

### <a id="Jumbee_Console_ListBox_AddItems_System_Collections_Generic_IEnumerable_Spectre_Console_Rendering_IRenderable__"></a> AddItems\(params IEnumerable<IRenderable\>\)

Appends the given renderable items to the list.

```csharp
public void AddItems(params IEnumerable<IRenderable> items)
```

#### Parameters

`items` IEnumerable<IRenderable\>

### <a id="Jumbee_Console_ListBox_AddItems_System_Collections_Generic_IEnumerable_System_String__"></a> AddItems\(params IEnumerable<string\>\)

Appends the given text items to the list.

```csharp
public void AddItems(params IEnumerable<string> items)
```

#### Parameters

`items` IEnumerable<string\>

### <a id="Jumbee_Console_ListBox_AddItems_System_ValueTuple_System_String_System_Nullable_Jumbee_Console_Color__System_Nullable_Jumbee_Console_Color_____"></a> AddItems\(params \(string text, Color? fgColor, Color? bgColor\)\[\]\)

Appends the given text items with per-item foreground/background colours to the list.

```csharp
public void AddItems(params (string text, Color? fgColor, Color? bgColor)[] items)
```

#### Parameters

`items` \(string text, [Color](Jumbee.Console.Color.md)? fgColor, [Color](Jumbee.Console.Color.md)? bgColor\)\[\]

### <a id="Jumbee_Console_ListBox_ApplyTheme"></a> ApplyTheme\(\)

Re-captures this control's themed colours/glyphs from the current <xref href="Jumbee.Console.UI.StyleTheme" data-throw-if-not-resolved="false"></xref>/
<xref href="Jumbee.Console.UI.GlyphTheme" data-throw-if-not-resolved="false"></xref>. The default is a no-op for controls that don't use the theme.

```csharp
protected override void ApplyTheme()
```

#### Remarks

Called by themed controls from their constructor and again on a runtime theme switch (<xref href="Jumbee.Console.UI.SetTheme(Jumbee.Console.IStyleTheme%2cJumbee.Console.IGlyphTheme)" data-throw-if-not-resolved="false"></xref>).
Must read the themes <em>only here</em> (and in the constructor), never on the render path.

### <a id="Jumbee_Console_ListBox_Clear"></a> Clear\(\)

Removes all items from the list.

```csharp
public void Clear()
```

### <a id="Jumbee_Console_ListBox_GetHelpInfo"></a> GetHelpInfo\(\)

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

### <a id="Jumbee_Console_ListBox_MeasureHeight_System_Int32_"></a> MeasureHeight\(int\)

The control's intrinsic content height in rows at the given <code class="paramref">width</code>, or 0 when it has no
intrinsic height and should fill the space its parent gives it (the default).

```csharp
protected override int MeasureHeight(int width)
```

#### Parameters

`width` int

#### Returns

 int

#### Remarks

Consulted by <xref href="Jumbee.Console.Control.CalculateSize" data-throw-if-not-resolved="false"></xref> only when a parent leaves the height unbounded — i.e. inside a
scrolling <xref href="Jumbee.Console.ControlFrame" data-throw-if-not-resolved="false"></xref> — so the frame can size the control to its content and show an accurate
scrollbar instead of a tiny thumb over ~1000 empty rows. Override on content controls (lists, editors,
logs). A content change that alters the height must re-lay-out (<xref href="Jumbee.Console.Control.Initialize" data-throw-if-not-resolved="false"></xref>, not merely
<xref href="Jumbee.Console.Control.Invalidate" data-throw-if-not-resolved="false"></xref>) so the frame re-measures.

### <a id="Jumbee_Console_ListBox_OnClick_ConsoleGUI_Space_Position_"></a> OnClick\(Position\)

Called on a press+release on this control (relative position).

```csharp
protected override void OnClick(Position position)
```

#### Parameters

`position` Position

### <a id="Jumbee_Console_ListBox_OnDoubleClick_ConsoleGUI_Space_Position_"></a> OnDoubleClick\(Position\)

Called on two clicks within <xref href="Jumbee.Console.Control.DoubleClickMs" data-throw-if-not-resolved="false"></xref> at the same position.

```csharp
protected override void OnDoubleClick(Position position)
```

#### Parameters

`position` Position

### <a id="Jumbee_Console_ListBox_OnInput_ConsoleGUI_Input_InputEvent_"></a> OnInput\(InputEvent\)

Handles a keyboard input event; override on input-handling controls. The default is a no-op.

```csharp
protected override void OnInput(InputEvent inputEvent)
```

#### Parameters

`inputEvent` InputEvent

### <a id="Jumbee_Console_ListBox_RemoveItem_Jumbee_Console_ListBox_ListBoxItem_"></a> RemoveItem\(ListBoxItem\)

Removes an item.

```csharp
public bool RemoveItem(ListBox.ListBoxItem item)
```

#### Parameters

`item` [ListBox](Jumbee.Console.ListBox.md).[ListBoxItem](Jumbee.Console.ListBox.ListBoxItem.md)

#### Returns

 bool

#### Remarks

The result is reliable only when called on the UI thread; off-thread callers should not rely on it (the
removal is marshaled and applied on the next pump).

### <a id="Jumbee_Console_ListBox_Render_Spectre_Console_Rendering_RenderOptions_System_Int32_"></a> Render\(RenderOptions, int\)

Produces the Spectre.Console <xref href="Spectre.Console.Rendering.Segment" data-throw-if-not-resolved="false"></xref>s for the control's content within <code class="paramref">maxWidth</code>.

```csharp
protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
```

#### Parameters

`options` RenderOptions

`maxWidth` int

#### Returns

 IEnumerable<Segment\>

### <a id="Jumbee_Console_ListBox_Update"></a> Update\(\)

Requests a redraw of the list.

```csharp
public void Update()
```

### <a id="Jumbee_Console_ListBox_Cancelled"></a> Cancelled

Raised when the list is cancelled (Escape).

```csharp
public event EventHandler? Cancelled
```

#### Event Type

 EventHandler?

### <a id="Jumbee_Console_ListBox_Committed"></a> Committed

Raised when an item is committed (Enter or click).

```csharp
public event EventHandler<ListBox.ListBoxItem>? Committed
```

#### Event Type

 EventHandler<[ListBox](Jumbee.Console.ListBox.md).[ListBoxItem](Jumbee.Console.ListBox.ListBoxItem.md)\>?

### <a id="Jumbee_Console_ListBox_ContextMenuOpening"></a> ContextMenuOpening

Raised just before <xref href="Jumbee.Console.ListBox.ContextMenu" data-throw-if-not-resolved="false"></xref> is shown for a right-clicked row, with that item (now
    the selected one).

```csharp
public event EventHandler<ListBox.ListBoxItem>? ContextMenuOpening
```

#### Event Type

 EventHandler<[ListBox](Jumbee.Console.ListBox.md).[ListBoxItem](Jumbee.Console.ListBox.ListBoxItem.md)\>?

#### Remarks

Use it to tailor the menu, or read <xref href="Jumbee.Console.ListBox.SelectedItem" data-throw-if-not-resolved="false"></xref> from the menu's own item handlers.
    Only fires when <xref href="Jumbee.Console.ListBox.ContextMenu" data-throw-if-not-resolved="false"></xref> is set.

### <a id="Jumbee_Console_ListBox_SelectionChanged"></a> SelectionChanged

Raised when the highlighted index changes (navigation or selection).

```csharp
public event EventHandler<int>? SelectionChanged
```

#### Event Type

 EventHandler<int\>?

