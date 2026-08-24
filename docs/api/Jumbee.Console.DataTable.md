# <a id="Jumbee_Console_DataTable"></a> Class DataTable

Namespace: [Jumbee.Console](Jumbee.Console.md)  
Assembly: Jumbee.Console.dll  

An interactive data grid.

```csharp
public class DataTable : Control, IFocusable
```

#### Inheritance

object ← 
Control ← 
[Control](Jumbee.Console.Control.md) ← 
[DataTable](Jumbee.Console.DataTable.md)

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

[ControlExtensions.WithAsciiBorder<DataTable\>\(DataTable, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithAsciiBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithBorder<DataTable\>\(DataTable, BorderStyle?, Color?, Color?, BorderPlacement?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_BorderPlacement\_\_), 
[ControlExtensions.WithDoubleBorder<DataTable\>\(DataTable, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithDoubleBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithFrame<DataTable\>\(DataTable, ControlFrame\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithFrame\_\_1\_\_\_0\_Jumbee\_Console\_ControlFrame\_), 
[ControlExtensions.WithFrame<DataTable\>\(DataTable, BorderStyle?, Offset?, Color?, Color?, string?, Color?, Color?, BorderPlacement?, BorderStyle?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithFrame\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_System\_Nullable\_ConsoleGUI\_Space\_Offset\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_String\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_BorderPlacement\_\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_), 
[ControlExtensions.WithHeavyBorder<DataTable\>\(DataTable, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithHeavyBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithHeight<DataTable\>\(DataTable, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithHeight\_\_1\_\_\_0\_System\_Int32\_), 
[ControlExtensions.WithMargin<DataTable\>\(DataTable, int, int, int, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithMargin\_\_1\_\_\_0\_System\_Int32\_System\_Int32\_System\_Int32\_System\_Int32\_), 
[ControlExtensions.WithMargin<DataTable\>\(DataTable, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithMargin\_\_1\_\_\_0\_System\_Int32\_), 
[ControlExtensions.WithNoBorder<DataTable\>\(DataTable\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithNoBorder\_\_1\_\_\_0\_), 
[ControlExtensions.WithRoundedBorder<DataTable\>\(DataTable, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithRoundedBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithScrollBarGlyphs<DataTable\>\(DataTable, ScrollBarGlyphs\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithScrollBarGlyphs\_\_1\_\_\_0\_Jumbee\_Console\_ScrollBarGlyphs\_), 
[ControlExtensions.WithScrollBarStyle<DataTable\>\(DataTable, ScrollBarStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithScrollBarStyle\_\_1\_\_\_0\_Jumbee\_Console\_ScrollBarStyle\_), 
[ControlExtensions.WithSize<DataTable\>\(DataTable, int?, int?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithSize\_\_1\_\_\_0\_System\_Nullable\_System\_Int32\_\_System\_Nullable\_System\_Int32\_\_), 
[ControlExtensions.WithSquareBorder<DataTable\>\(DataTable, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithSquareBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithTitle<DataTable\>\(DataTable, string\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_), 
[ControlExtensions.WithTitle<DataTable\>\(DataTable, string, TitleStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_Jumbee\_Console\_TitleStyle\_), 
[ControlExtensions.WithTitle<DataTable\>\(DataTable, string, TitlePos, TitleBorderStyle, TitleColorStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_Jumbee\_Console\_TitlePos\_Jumbee\_Console\_TitleBorderStyle\_Jumbee\_Console\_TitleColorStyle\_), 
[ControlExtensions.WithWidth<DataTable\>\(DataTable, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithWidth\_\_1\_\_\_0\_System\_Int32\_)

## Remarks

Columns and rows are supplied as text; the visible row window is laid out by Spectre.Console's
<xref href="Spectre.Console.Table" data-throw-if-not-resolved="false"></xref> (column sizing, borders, wrapping), and this control adds the interactivity Spectre lacks: a
highlighted selected row, keyboard navigation, a scroll viewport over many rows with its own scrollbar,
click-to-select, and <xref href="Jumbee.Console.DataTable.SelectionChanged" data-throw-if-not-resolved="false"></xref>/<xref href="Jumbee.Console.DataTable.RowActivated" data-throw-if-not-resolved="false"></xref> events. The header stays fixed
while the rows scroll. (Inline cell editing is not supported yet.)

## Constructors

### <a id="Jumbee_Console_DataTable__ctor_System_String___"></a> DataTable\(params string\[\]\)

Initializes a new <xref href="Jumbee.Console.DataTable" data-throw-if-not-resolved="false"></xref> with the given column headers.

```csharp
public DataTable(params string[] columns)
```

#### Parameters

`columns` string\[\]

## Properties

### <a id="Jumbee_Console_DataTable_Columns"></a> Columns

The column headers.

```csharp
public IReadOnlyList<string> Columns { get; }
```

#### Property Value

 IReadOnlyList<string\>

### <a id="Jumbee_Console_DataTable_DropNarrowColumns"></a> DropNarrowColumns

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a> (the default), columns are dropped from the right rather than letting text wrap
once the table is too narrow to show them all.

```csharp
public bool DropNarrowColumns { get; set; }
```

#### Property Value

 bool

#### Remarks

A table squeezed below the width its content needs otherwise wraps: headers break mid-word and values split
across lines, which is unreadable and makes rows taller than one line. Dropping whole columns — keeping the
leftmost, which is normally the identifier — is what terminal process monitors do. Set this to
<a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a> to keep every column and accept the wrapping.

### <a id="Jumbee_Console_DataTable_HandlesInput"></a> HandlesInput

Always <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>: the table handles its own selection keys (arrows, Home/End, Page
    Up/Down, Enter). No opt-in needed — unlike the base default, this is on.

```csharp
public override bool HandlesInput { get; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_DataTable_RowCount"></a> RowCount

The number of data rows.

```csharp
public int RowCount { get; }
```

#### Property Value

 int

### <a id="Jumbee_Console_DataTable_SelectedIndex"></a> SelectedIndex

The selected row index, or -1 when there are no rows.

```csharp
public int SelectedIndex { get; set; }
```

#### Property Value

 int

### <a id="Jumbee_Console_DataTable_SelectedRow"></a> SelectedRow

The selected row's cells, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> when there are no rows.

```csharp
public string[]? SelectedRow { get; }
```

#### Property Value

 string\[\]?

### <a id="Jumbee_Console_DataTable_WantsMouse"></a> WantsMouse

Always <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>: rows receive hover and click, so a click selects a row and a
    double-click activates it. No opt-in needed — unlike the base default, this is on.

```csharp
protected override bool WantsMouse { get; }
```

#### Property Value

 bool

## Methods

### <a id="Jumbee_Console_DataTable_AddColumn_System_String_"></a> AddColumn\(string\)

Appends a column with the given header.

```csharp
public void AddColumn(string header)
```

#### Parameters

`header` string

### <a id="Jumbee_Console_DataTable_AddRow_System_String___"></a> AddRow\(params string\[\]\)

Appends a row and returns its index.

```csharp
public int AddRow(params string[] cells)
```

#### Parameters

`cells` string\[\]

#### Returns

 int

### <a id="Jumbee_Console_DataTable_Clear"></a> Clear\(\)

Removes all rows and resets the selection and scroll.

```csharp
public void Clear()
```

### <a id="Jumbee_Console_DataTable_GetHelpInfo"></a> GetHelpInfo\(\)

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

### <a id="Jumbee_Console_DataTable_OnClick_ConsoleGUI_Space_Position_"></a> OnClick\(Position\)

Called on a press+release on this control (relative position).

```csharp
protected override void OnClick(Position position)
```

#### Parameters

`position` Position

### <a id="Jumbee_Console_DataTable_OnDoubleClick_ConsoleGUI_Space_Position_"></a> OnDoubleClick\(Position\)

Called on two clicks within <xref href="Jumbee.Console.Control.DoubleClickMs" data-throw-if-not-resolved="false"></xref> at the same position.

```csharp
protected override void OnDoubleClick(Position position)
```

#### Parameters

`position` Position

### <a id="Jumbee_Console_DataTable_OnInput_ConsoleGUI_Input_InputEvent_"></a> OnInput\(InputEvent\)

Handles a keyboard input event; override on input-handling controls. The default is a no-op.

```csharp
protected override void OnInput(InputEvent inputEvent)
```

#### Parameters

`inputEvent` InputEvent

### <a id="Jumbee_Console_DataTable_OnMouseMove_ConsoleGUI_Space_Position_"></a> OnMouseMove\(Position\)

Called as the pointer moves within the control (relative position).

```csharp
protected override void OnMouseMove(Position position)
```

#### Parameters

`position` Position

### <a id="Jumbee_Console_DataTable_OnMousePress_ConsoleGUI_Space_Position_"></a> OnMousePress\(Position\)

Called when a button is pressed over the control (relative position).

```csharp
protected override void OnMousePress(Position position)
```

#### Parameters

`position` Position

### <a id="Jumbee_Console_DataTable_OnMouseRelease_ConsoleGUI_Space_Position_"></a> OnMouseRelease\(Position\)

Called when a button is released over the control (relative position).

```csharp
protected override void OnMouseRelease(Position position)
```

#### Parameters

`position` Position

### <a id="Jumbee_Console_DataTable_OnMouseWheel_ConsoleGUI_Space_Position_System_Int32_"></a> OnMouseWheel\(Position, int\)

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

### <a id="Jumbee_Console_DataTable_RemoveRow_System_Int32_"></a> RemoveRow\(int\)

Removes the row at <code class="paramref">index</code> (no-op if out of range).

```csharp
public void RemoveRow(int index)
```

#### Parameters

`index` int

### <a id="Jumbee_Console_DataTable_Render"></a> Render\(\)

This method renders the control's content to the console buffer.

```csharp
protected override void Render()
```

#### Remarks

Note that this does not actually draw the control on the console screen.

### <a id="Jumbee_Console_DataTable_RowActivated"></a> RowActivated

Raised when the selected row is activated (Enter / double-click); the argument is the row index.

```csharp
public event EventHandler<int>? RowActivated
```

#### Event Type

 EventHandler<int\>?

### <a id="Jumbee_Console_DataTable_SelectionChanged"></a> SelectionChanged

Raised when the selected row changes; the argument is the new row index (or -1 when empty).

```csharp
public event EventHandler<int>? SelectionChanged
```

#### Event Type

 EventHandler<int\>?

