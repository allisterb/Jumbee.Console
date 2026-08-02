# <a id="Jumbee_Console_SplitPanel"></a> Class SplitPanel

Namespace: [Jumbee.Console](Jumbee.Console.md)  
Assembly: Jumbee.Console.dll  

A container that splits its area between two panes with a draggable divider between them.

```csharp
public class SplitPanel : Layout<SplitPanelDockPanel>, ILayout, IFocusable
```

#### Inheritance

object ← 
[Layout<SplitPanelDockPanel\>](Jumbee.Console.Layout\-1.md) ← 
[SplitPanel](Jumbee.Console.SplitPanel.md)

#### Implements

[ILayout](Jumbee.Console.ILayout.md), 
[IFocusable](Jumbee.Console.IFocusable.md)

#### Inherited Members

[Layout<SplitPanelDockPanel\>.this\[int, int\]](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_Item\_System\_Int32\_System\_Int32\_), 
[Layout<SplitPanelDockPanel\>.Rows](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_Rows), 
[Layout<SplitPanelDockPanel\>.Columns](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_Columns), 
[Layout<SplitPanelDockPanel\>.this\[Position\]](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_Item\_ConsoleGUI\_Space\_Position\_), 
[Layout<SplitPanelDockPanel\>.Size](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_Size), 
[Layout<SplitPanelDockPanel\>.CControl](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_CControl), 
[Layout<SplitPanelDockPanel\>.Context](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_Context), 
[Layout<SplitPanelDockPanel\>.Controls](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_Controls), 
[Layout<SplitPanelDockPanel\>.Focusable](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_Focusable), 
[Layout<SplitPanelDockPanel\>.FocusableControl](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_FocusableControl), 
[Layout<SplitPanelDockPanel\>.IsFocused](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_IsFocused), 
[Layout<SplitPanelDockPanel\>.HandlesInput](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_HandlesInput), 
[Layout<SplitPanelDockPanel\>.FocusedControl](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_FocusedControl), 
[Layout<SplitPanelDockPanel\>.OnFocus](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_OnFocus), 
[Layout<SplitPanelDockPanel\>.OnLostFocus](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_OnLostFocus), 
[Layout<SplitPanelDockPanel\>.OnRedraw\(DrawingContext\)](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_OnRedraw\_ConsoleGUI\_Common\_DrawingContext\_), 
[Layout<SplitPanelDockPanel\>.OnUpdate\(DrawingContext, Rect\)](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_OnUpdate\_ConsoleGUI\_Common\_DrawingContext\_ConsoleGUI\_Space\_Rect\_), 
[Layout<SplitPanelDockPanel\>.OnInput\(UI.InputEventArgs\)](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_OnInput\_Jumbee\_Console\_UI\_InputEventArgs\_), 
[Layout<SplitPanelDockPanel\>.InterceptInput\(UI.InputEventArgs\)](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_InterceptInput\_Jumbee\_Console\_UI\_InputEventArgs\_), 
[Layout<SplitPanelDockPanel\>.OnPaste\(string\)](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_OnPaste\_System\_String\_), 
[Layout<SplitPanelDockPanel\>.control](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_control)

## Remarks

The first pane has a fixed extent (<xref href="Jumbee.Console.SplitPanel.SplitPosition" data-throw-if-not-resolved="false"></xref>, in cells) and the second fills the rest, so
resizing the container keeps the first pane put and grows/shrinks the second. Resize by dragging the divider, or by
focusing it and pressing the arrow keys (Shift = larger step). Nest split panels for richer layouts (e.g. a sidebar
beside a vertically-split editor/terminal). Composes with the same focus-routing model as <xref href="Jumbee.Console.TabPanel" data-throw-if-not-resolved="false"></xref>.

## Constructors

### <a id="Jumbee_Console_SplitPanel__ctor_Jumbee_Console_SplitOrientation_Jumbee_Console_IFocusable_Jumbee_Console_IFocusable_System_Int32_"></a> SplitPanel\(SplitOrientation, IFocusable, IFocusable, int\)

Initializes a new <xref href="Jumbee.Console.SplitPanel" data-throw-if-not-resolved="false"></xref> splitting <code class="paramref">first</code> and <code class="paramref">second</code> along <code class="paramref">orientation</code>, with the first pane given <code class="paramref">splitPosition</code> cells.

```csharp
public SplitPanel(SplitOrientation orientation, IFocusable first, IFocusable second, int splitPosition = 20)
```

#### Parameters

`orientation` [SplitOrientation](Jumbee.Console.SplitOrientation.md)

`first` [IFocusable](Jumbee.Console.IFocusable.md)

`second` [IFocusable](Jumbee.Console.IFocusable.md)

`splitPosition` int

## Properties

### <a id="Jumbee_Console_SplitPanel_Columns"></a> Columns

Number of columns in the layout grid (3 for a horizontal split's side-by-side panes+divider, otherwise 1).

```csharp
public override int Columns { get; }
```

#### Property Value

 int

### <a id="Jumbee_Console_SplitPanel_Divider"></a> Divider

The draggable divider (for theming or focusing).

```csharp
public SplitDivider Divider { get; }
```

#### Property Value

 [SplitDivider](Jumbee.Console.SplitDivider.md)

### <a id="Jumbee_Console_SplitPanel_First"></a> First

The first pane (left/top).

```csharp
public IFocusable First { get; }
```

#### Property Value

 [IFocusable](Jumbee.Console.IFocusable.md)

### <a id="Jumbee_Console_SplitPanel_FocusedControl"></a> FocusedControl

The focused descendant within this layout (or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a>), so a parent can tell that this layout
is on the focus path and route input into it.

```csharp
public override IFocusable? FocusedControl { get; }
```

#### Property Value

 [IFocusable](Jumbee.Console.IFocusable.md)?

#### Remarks

Walks <xref href="Jumbee.Console.Layout%601.Controls" data-throw-if-not-resolved="false"></xref> for the focused leaf; this is what lets keyboard input — and each ancestor
layout's tunnel (<xref href="Jumbee.Console.Layout%601.InterceptInput(Jumbee.Console.UI.InputEventArgs)" data-throw-if-not-resolved="false"></xref>) — reach a control even when the layout is nested several
levels deep.

### <a id="Jumbee_Console_SplitPanel_MinFirst"></a> MinFirst

Minimum extent of the first pane in cells (default 3). Clamped to at least <code>1</code>, so
    <xref href="Jumbee.Console.SplitPanel.SplitPosition" data-throw-if-not-resolved="false"></xref> can never reach <code>0</code> — a "fully collapsed" first pane is really a 1-cell
    sliver (set <code>MinFirst = 1</code> for the thinnest zen collapse).

```csharp
public int MinFirst { get; set; }
```

#### Property Value

 int

### <a id="Jumbee_Console_SplitPanel_MinSecond"></a> MinSecond

Minimum extent of the second pane in cells (default 3).

```csharp
public int MinSecond { get; set; }
```

#### Property Value

 int

### <a id="Jumbee_Console_SplitPanel_Orientation"></a> Orientation

How the two panes are arranged (fixed at construction).

```csharp
public SplitOrientation Orientation { get; }
```

#### Property Value

 [SplitOrientation](Jumbee.Console.SplitOrientation.md)

### <a id="Jumbee_Console_SplitPanel_Rows"></a> Rows

Number of rows in the layout grid (3 for a vertical split's stacked panes+divider, otherwise 1).

```csharp
public override int Rows { get; }
```

#### Property Value

 int

### <a id="Jumbee_Console_SplitPanel_Second"></a> Second

The second pane (right/bottom).

```csharp
public IFocusable Second { get; }
```

#### Property Value

 [IFocusable](Jumbee.Console.IFocusable.md)

### <a id="Jumbee_Console_SplitPanel_SplitPosition"></a> SplitPosition

The first pane's extent in cells (width for a horizontal split, height for a vertical one).

```csharp
public int SplitPosition { get; set; }
```

#### Property Value

 int

#### Remarks

Clamped to <xref href="Jumbee.Console.SplitPanel.MinFirst" data-throw-if-not-resolved="false"></xref> and to leaving the divider plus <xref href="Jumbee.Console.SplitPanel.MinSecond" data-throw-if-not-resolved="false"></xref> for the
    second pane; raises <xref href="Jumbee.Console.SplitPanel.SplitChanged" data-throw-if-not-resolved="false"></xref> when it actually changes. Set it to <xref href="Jumbee.Console.SplitPanel.MinFirst" data-throw-if-not-resolved="false"></xref> to
    collapse the first pane to a sliver (a "focus"/zen toggle); save the previous value and restore it to expand
    again — the simplest runtime layout change, since it's just a resize.

### <a id="Jumbee_Console_SplitPanel_Item_System_Int32_System_Int32_"></a> this\[int, int\]

Gets the logical child at the given <code class="paramref">row</code> and <code class="paramref">column</code>: first pane, divider, or second pane.

```csharp
public override IFocusable this[int row, int column] { get; }
```

#### Property Value

 [IFocusable](Jumbee.Console.IFocusable.md)

### <a id="Jumbee_Console_SplitPanel_SplitChanged"></a> SplitChanged

Raised after <xref href="Jumbee.Console.SplitPanel.SplitPosition" data-throw-if-not-resolved="false"></xref> changes, with the new first-pane extent.

```csharp
public event EventHandler<int>? SplitChanged
```

#### Event Type

 EventHandler<int\>?

