# <a id="Jumbee_Console_Grid"></a> Class Grid

Namespace: [Jumbee.Console](Jumbee.Console.md)  
Assembly: Jumbee.Console.dll  

A grid layout with controls arranged in rows and columns.

```csharp
public class Grid : Layout<Grid>, ILayout, IFocusable
```

#### Inheritance

object ← 
[Layout<Grid\>](Jumbee.Console.Layout\-1.md) ← 
[Grid](Jumbee.Console.Grid.md)

#### Implements

[ILayout](Jumbee.Console.ILayout.md), 
[IFocusable](Jumbee.Console.IFocusable.md)

#### Inherited Members

[Layout<Grid\>.this\[int, int\]](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_Item\_System\_Int32\_System\_Int32\_), 
[Layout<Grid\>.Rows](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_Rows), 
[Layout<Grid\>.Columns](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_Columns), 
[Layout<Grid\>.this\[Position\]](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_Item\_ConsoleGUI\_Space\_Position\_), 
[Layout<Grid\>.Size](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_Size), 
[Layout<Grid\>.CControl](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_CControl), 
[Layout<Grid\>.Context](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_Context), 
[Layout<Grid\>.Controls](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_Controls), 
[Layout<Grid\>.Focusable](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_Focusable), 
[Layout<Grid\>.FocusableControl](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_FocusableControl), 
[Layout<Grid\>.IsFocused](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_IsFocused), 
[Layout<Grid\>.HandlesInput](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_HandlesInput), 
[Layout<Grid\>.FocusedControl](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_FocusedControl), 
[Layout<Grid\>.OnFocus](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_OnFocus), 
[Layout<Grid\>.OnLostFocus](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_OnLostFocus), 
[Layout<Grid\>.OnRedraw\(DrawingContext\)](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_OnRedraw\_ConsoleGUI\_Common\_DrawingContext\_), 
[Layout<Grid\>.OnUpdate\(DrawingContext, Rect\)](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_OnUpdate\_ConsoleGUI\_Common\_DrawingContext\_ConsoleGUI\_Space\_Rect\_), 
[Layout<Grid\>.OnInput\(UI.InputEventArgs\)](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_OnInput\_Jumbee\_Console\_UI\_InputEventArgs\_), 
[Layout<Grid\>.InterceptInput\(UI.InputEventArgs\)](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_InterceptInput\_Jumbee\_Console\_UI\_InputEventArgs\_), 
[Layout<Grid\>.OnPaste\(string\)](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_OnPaste\_System\_String\_), 
[Layout<Grid\>.control](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_control)

## Constructors

### <a id="Jumbee_Console_Grid__ctor_System_Int32___System_Int32___Jumbee_Console_IFocusable_____"></a> Grid\(int\[\], int\[\], params IFocusable\[\]\[\]\)

Creates a grid layout with fixed row heights, fixed column widths, and a control for each cell.

```csharp
public Grid(int[] rowHeights, int[] columnWidths, params IFocusable[][] controls)
```

#### Parameters

`rowHeights` int\[\]

The fixed height in cells of each row, top to bottom.

`columnWidths` int\[\]

The fixed width in cells of each column, left to right.

`controls` [IFocusable](Jumbee.Console.IFocusable.md)\[\]\[\]

Row-major controls: one inner array per row, each with one control per column.

#### Remarks

Sizing is <b>fixed cells</b>: every value is an absolute cell count (a row's height, a column's width), and
the grid's own size is their sum. There is no proportional/"star" sizing and no auto-fill — unlike
<xref href="Jumbee.Console.DockPanel" data-throw-if-not-resolved="false"></xref>, a <code>0</code> here means a 0-cell (collapsed) row/column, <em>not</em> fill-the-parent.
Each cell's control is given its cell's fixed size (so a control that fills, i.e. <code>Width</code>/<code>Height</code>
0, fills that fixed cell). For proportional/fill layouts, compose <xref href="Jumbee.Console.DockPanel" data-throw-if-not-resolved="false"></xref>/<xref href="Jumbee.Console.SplitPanel" data-throw-if-not-resolved="false"></xref>
instead.

<p>
<b>A grid does not grow with the terminal.</b> The app is re-laid-out on resize, but a grid's extents are
absolute, so a grid used as the <em>root</em> renders at the size you specified and leaves the rest of the
screen empty (or clips, if the terminal is smaller). That's the right behaviour for a form or a fixed panel
nested inside a region; it is the wrong choice for an app shell. Build the shell from
<xref href="Jumbee.Console.DockPanel" data-throw-if-not-resolved="false"></xref>/<xref href="Jumbee.Console.SplitPanel" data-throw-if-not-resolved="false"></xref> and put grids inside the regions that genuinely want fixed
geometry. Nesting doesn't change this — a grid inside a docked panel's fill slot still won't grow.
</p>
<p>
To give a cell's content an explicit size of its own — needed for anything without <code>Width</code>/<code>Height</code>,
such as a <xref href="Jumbee.Console.ControlFrame" data-throw-if-not-resolved="false"></xref> or a nested layout — wrap it in a <xref href="Jumbee.Console.Boundary" data-throw-if-not-resolved="false"></xref>:
<code>new Boundary(child, width, height)</code>.
</p>

#### Exceptions

 ArgumentException

The control grid's row/column counts don't match
    <code class="paramref">rowHeights</code>/<code class="paramref">columnWidths</code>.

#### See Also

[Boundary](Jumbee.Console.Boundary.md)

## Properties

### <a id="Jumbee_Console_Grid_Columns"></a> Columns

Number of columns in the grid.

```csharp
public override int Columns { get; }
```

#### Property Value

 int

### <a id="Jumbee_Console_Grid_Rows"></a> Rows

Number of rows in the grid.

```csharp
public override int Rows { get; }
```

#### Property Value

 int

### <a id="Jumbee_Console_Grid_Item_System_Int32_System_Int32_"></a> this\[int, int\]

Gets the control at the given <code class="paramref">row</code> and <code class="paramref">column</code>.

```csharp
public override IFocusable this[int row, int column] { get; }
```

#### Property Value

 [IFocusable](Jumbee.Console.IFocusable.md)

## Methods

### <a id="Jumbee_Console_Grid_SetChild_System_Int32_System_Int32_Jumbee_Console_IFocusable_"></a> SetChild\(int, int, IFocusable\)

Places <code class="paramref">child</code> in the cell at the given <code class="paramref">row</code> and <code class="paramref">column</code>.

```csharp
public void SetChild(int row, int column, IFocusable child)
```

#### Parameters

`row` int

`column` int

`child` [IFocusable](Jumbee.Console.IFocusable.md)

