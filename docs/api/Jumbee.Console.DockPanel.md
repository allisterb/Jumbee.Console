# <a id="Jumbee_Console_DockPanel"></a> Class DockPanel

Namespace: [Jumbee.Console](Jumbee.Console.md)  
Assembly: Jumbee.Console.dll  

A two-child layout that pins one control to an edge and fills the remaining space with the other.

```csharp
public class DockPanel : Layout<DockPanel>, ILayout, IFocusable
```

#### Inheritance

object ← 
[Layout<DockPanel\>](Jumbee.Console.Layout\-1.md) ← 
[DockPanel](Jumbee.Console.DockPanel.md)

#### Implements

[ILayout](Jumbee.Console.ILayout.md), 
[IFocusable](Jumbee.Console.IFocusable.md)

#### Inherited Members

[Layout<DockPanel\>.this\[int, int\]](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_Item\_System\_Int32\_System\_Int32\_), 
[Layout<DockPanel\>.Rows](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_Rows), 
[Layout<DockPanel\>.Columns](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_Columns), 
[Layout<DockPanel\>.this\[Position\]](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_Item\_ConsoleGUI\_Space\_Position\_), 
[Layout<DockPanel\>.Size](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_Size), 
[Layout<DockPanel\>.CControl](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_CControl), 
[Layout<DockPanel\>.Context](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_Context), 
[Layout<DockPanel\>.Controls](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_Controls), 
[Layout<DockPanel\>.Focusable](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_Focusable), 
[Layout<DockPanel\>.FocusableControl](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_FocusableControl), 
[Layout<DockPanel\>.IsFocused](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_IsFocused), 
[Layout<DockPanel\>.HandlesInput](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_HandlesInput), 
[Layout<DockPanel\>.FocusedControl](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_FocusedControl), 
[Layout<DockPanel\>.OnFocus](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_OnFocus), 
[Layout<DockPanel\>.OnLostFocus](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_OnLostFocus), 
[Layout<DockPanel\>.OnRedraw\(DrawingContext\)](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_OnRedraw\_ConsoleGUI\_Common\_DrawingContext\_), 
[Layout<DockPanel\>.OnUpdate\(DrawingContext, Rect\)](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_OnUpdate\_ConsoleGUI\_Common\_DrawingContext\_ConsoleGUI\_Space\_Rect\_), 
[Layout<DockPanel\>.OnInput\(UI.InputEventArgs\)](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_OnInput\_Jumbee\_Console\_UI\_InputEventArgs\_), 
[Layout<DockPanel\>.InterceptInput\(UI.InputEventArgs\)](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_InterceptInput\_Jumbee\_Console\_UI\_InputEventArgs\_), 
[Layout<DockPanel\>.OnPaste\(string\)](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_OnPaste\_System\_String\_), 
[Layout<DockPanel\>.control](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_control)

## Constructors

### <a id="Jumbee_Console_DockPanel__ctor_Jumbee_Console_DockedControlPlacement_Jumbee_Console_IFocusable_Jumbee_Console_IFocusable_"></a> DockPanel\(DockedControlPlacement, IFocusable, IFocusable\)

Initializes a new <xref href="Jumbee.Console.DockPanel" data-throw-if-not-resolved="false"></xref> that docks <code class="paramref">dockedControl</code> at <code class="paramref">placement</code> and fills the rest with <code class="paramref">fillControl</code>.

```csharp
public DockPanel(DockedControlPlacement placement, IFocusable dockedControl, IFocusable fillControl)
```

#### Parameters

`placement` [DockedControlPlacement](Jumbee.Console.DockedControlPlacement.md)

`dockedControl` [IFocusable](Jumbee.Console.IFocusable.md)

`fillControl` [IFocusable](Jumbee.Console.IFocusable.md)

## Properties

### <a id="Jumbee_Console_DockPanel_Columns"></a> Columns

Number of columns in the layout grid (2 when docked left/right, otherwise 1).

```csharp
public override int Columns { get; }
```

#### Property Value

 int

### <a id="Jumbee_Console_DockPanel_DockedControl"></a> DockedControl

The control pinned to the docked edge. Settable at runtime: reassign it to swap the docked pane in
    place — e.g. a "zen"/full-screen toggle that swaps this to a small placeholder and back — without rebuilding
    the layout.

```csharp
public IFocusable DockedControl { get; set; }
```

#### Property Value

 [IFocusable](Jumbee.Console.IFocusable.md)

#### Remarks

Give the docked control a positive width (for a Left/Right dock) or height (Top/Bottom). A width or
height of 0 is the "fill the parent" sentinel, so a 0-sized docked control takes the whole panel and starves
the fill control — use a positive extent, or swap this to a narrow control, to collapse the pane instead.

<p>
Controls that expose <code>Width</code>/<code>Height</code> can be sized directly (or via <code>WithWidth</code>/<code>WithHeight</code>).
Anything that doesn't — a <xref href="Jumbee.Console.ControlFrame" data-throw-if-not-resolved="false"></xref>, a nested layout — has no extent of its own, so wrap
it in a <xref href="Jumbee.Console.Boundary" data-throw-if-not-resolved="false"></xref> to give it one: <code>new Boundary(framedChart, width, height)</code>. Forgetting this
is the common failure: the dock silently fills the whole panel and the fill control never appears, with no error.
</p>

#### See Also

[Boundary](Jumbee.Console.Boundary.md)

### <a id="Jumbee_Console_DockPanel_FillControl"></a> FillControl

The control that fills the space left after docking. Settable at runtime.

```csharp
public IFocusable FillControl { get; set; }
```

#### Property Value

 [IFocusable](Jumbee.Console.IFocusable.md)

### <a id="Jumbee_Console_DockPanel_Rows"></a> Rows

Number of rows in the layout grid (2 when docked top/bottom, otherwise 1).

```csharp
public override int Rows { get; }
```

#### Property Value

 int

### <a id="Jumbee_Console_DockPanel_Item_System_Int32_System_Int32_"></a> this\[int, int\]

Gets the control at the given <code class="paramref">row</code> and <code class="paramref">column</code>.

```csharp
public override IFocusable this[int row, int column] { get; }
```

#### Property Value

 [IFocusable](Jumbee.Console.IFocusable.md)

