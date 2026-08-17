# <a id="Jumbee_Console_Tree"></a> Class Tree

Namespace: [Jumbee.Console](Jumbee.Console.md)  
Assembly: Jumbee.Console.dll  

Displays a hierarchical list of items in a tree layout.

```csharp
public class Tree : RenderableControl, IFocusable, IScrollable
```

#### Inheritance

object ← 
Control ← 
[Control](Jumbee.Console.Control.md) ← 
[RenderableControl](Jumbee.Console.RenderableControl.md) ← 
[Tree](Jumbee.Console.Tree.md)

#### Implements

[IFocusable](Jumbee.Console.IFocusable.md), 
[IScrollable](Jumbee.Console.IScrollable.md)

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

[ControlExtensions.WithAsciiBorder<Tree\>\(Tree, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithAsciiBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithBorder<Tree\>\(Tree, BorderStyle?, Color?, Color?, BorderPlacement?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_BorderPlacement\_\_), 
[ControlExtensions.WithDoubleBorder<Tree\>\(Tree, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithDoubleBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithFrame<Tree\>\(Tree, ControlFrame\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithFrame\_\_1\_\_\_0\_Jumbee\_Console\_ControlFrame\_), 
[ControlExtensions.WithFrame<Tree\>\(Tree, BorderStyle?, Offset?, Color?, Color?, string?, Color?, Color?, BorderPlacement?, BorderStyle?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithFrame\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_System\_Nullable\_ConsoleGUI\_Space\_Offset\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_String\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_BorderPlacement\_\_System\_Nullable\_Jumbee\_Console\_BorderStyle\_\_), 
[ControlExtensions.WithHeavyBorder<Tree\>\(Tree, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithHeavyBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithHeight<Tree\>\(Tree, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithHeight\_\_1\_\_\_0\_System\_Int32\_), 
[ControlExtensions.WithMargin<Tree\>\(Tree, int, int, int, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithMargin\_\_1\_\_\_0\_System\_Int32\_System\_Int32\_System\_Int32\_System\_Int32\_), 
[ControlExtensions.WithMargin<Tree\>\(Tree, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithMargin\_\_1\_\_\_0\_System\_Int32\_), 
[ControlExtensions.WithNoBorder<Tree\>\(Tree\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithNoBorder\_\_1\_\_\_0\_), 
[ControlExtensions.WithRoundedBorder<Tree\>\(Tree, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithRoundedBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithScrollBarGlyphs<Tree\>\(Tree, ScrollBarGlyphs\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithScrollBarGlyphs\_\_1\_\_\_0\_Jumbee\_Console\_ScrollBarGlyphs\_), 
[ControlExtensions.WithScrollBarStyle<Tree\>\(Tree, ScrollBarStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithScrollBarStyle\_\_1\_\_\_0\_Jumbee\_Console\_ScrollBarStyle\_), 
[ControlExtensions.WithSize<Tree\>\(Tree, int?, int?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithSize\_\_1\_\_\_0\_System\_Nullable\_System\_Int32\_\_System\_Nullable\_System\_Int32\_\_), 
[ControlExtensions.WithSquareBorder<Tree\>\(Tree, Color?, Color?\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithSquareBorder\_\_1\_\_\_0\_System\_Nullable\_Jumbee\_Console\_Color\_\_System\_Nullable\_Jumbee\_Console\_Color\_\_), 
[ControlExtensions.WithTitle<Tree\>\(Tree, string\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_), 
[ControlExtensions.WithTitle<Tree\>\(Tree, string, TitleStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_Jumbee\_Console\_TitleStyle\_), 
[ControlExtensions.WithTitle<Tree\>\(Tree, string, TitlePos, TitleBorderStyle, TitleColorStyle\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithTitle\_\_1\_\_\_0\_System\_String\_Jumbee\_Console\_TitlePos\_Jumbee\_Console\_TitleBorderStyle\_Jumbee\_Console\_TitleColorStyle\_), 
[ControlExtensions.WithWidth<Tree\>\(Tree, int\)](Jumbee.Console.ControlExtensions.md\#Jumbee\_Console\_ControlExtensions\_WithWidth\_\_1\_\_\_0\_System\_Int32\_)

## Remarks

Based on <xref href="Spectre.Console.Tree" data-throw-if-not-resolved="false"></xref> but modified to support mutable tree nodes, concurrent updates, and node selection via user input.

## Constructors

### <a id="Jumbee_Console_Tree__ctor_Spectre_Console_Rendering_IRenderable_System_Nullable_Jumbee_Console_TreeGuide__System_Nullable_Jumbee_Console_Style__System_Boolean_"></a> Tree\(IRenderable, TreeGuide?, Style?, bool\)

Create a tree with a root label.

```csharp
public Tree(IRenderable rootLabel, TreeGuide? guide = null, Style? guideStyle = null, bool expanded = true)
```

#### Parameters

`rootLabel` IRenderable

The tree root label.

`guide` [TreeGuide](Jumbee.Console.TreeGuide.md)?

The connector-line glyph set (defaults to <xref href="Jumbee.Console.TreeGuide.Line" data-throw-if-not-resolved="false"></xref>).

`guideStyle` [Style](Jumbee.Console.Style.md)?

The style of the connector lines (defaults to <xref href="Jumbee.Console.Style.Plain" data-throw-if-not-resolved="false"></xref>).

`expanded` bool

Whether the root starts expanded.

### <a id="Jumbee_Console_Tree__ctor_System_String_System_Nullable_Jumbee_Console_TreeGuide__System_Nullable_Jumbee_Console_Style__System_Boolean_"></a> Tree\(string, TreeGuide?, Style?, bool\)

Initializes a new instance of the <xref href="Jumbee.Console.Tree" data-throw-if-not-resolved="false"></xref> class.

```csharp
public Tree(string rootText, TreeGuide? guide = null, Style? guideStyle = null, bool expanded = true)
```

#### Parameters

`rootText` string

The tree root label as a string.

`guide` [TreeGuide](Jumbee.Console.TreeGuide.md)?

The connector-line glyph set (defaults to <xref href="Jumbee.Console.TreeGuide.Line" data-throw-if-not-resolved="false"></xref>).

`guideStyle` [Style](Jumbee.Console.Style.md)?

The style of the connector lines (defaults to <xref href="Jumbee.Console.Style.Plain" data-throw-if-not-resolved="false"></xref>).

`expanded` bool

Whether the root starts expanded.

## Fields

### <a id="Jumbee_Console_Tree__expanded"></a> \_expanded

Backing field for <xref href="Jumbee.Console.Tree.Expanded" data-throw-if-not-resolved="false"></xref>.

```csharp
protected bool _expanded
```

#### Field Value

 bool

### <a id="Jumbee_Console_Tree__guide"></a> \_guide

Backing field for <xref href="Jumbee.Console.Tree.Guide" data-throw-if-not-resolved="false"></xref>.

```csharp
protected TreeGuide _guide
```

#### Field Value

 [TreeGuide](Jumbee.Console.TreeGuide.md)

### <a id="Jumbee_Console_Tree__root"></a> \_root

Backing field for <xref href="Jumbee.Console.Tree.Root" data-throw-if-not-resolved="false"></xref>.

```csharp
public Tree.TreeNode _root
```

#### Field Value

 [Tree](Jumbee.Console.Tree.md).[TreeNode](Jumbee.Console.Tree.TreeNode.md)

### <a id="Jumbee_Console_Tree__rootLabel"></a> \_rootLabel

The renderable used as the root node's label.

```csharp
public IRenderable _rootLabel
```

#### Field Value

 IRenderable

### <a id="Jumbee_Console_Tree__style"></a> \_style

Backing field for <xref href="Jumbee.Console.Tree.Style" data-throw-if-not-resolved="false"></xref>.

```csharp
protected Style _style
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Tree_scguide"></a> scguide

The Spectre.Console guide derived from <xref href="Jumbee.Console.Tree._guide" data-throw-if-not-resolved="false"></xref>, used when rendering guide lines.

```csharp
protected TreeGuide scguide
```

#### Field Value

 TreeGuide

## Properties

### <a id="Jumbee_Console_Tree_ContextMenu"></a> ContextMenu

An optional menu shown when a node is right-clicked. Left <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> = no menu.

```csharp
public ContextMenu? ContextMenu { get; set; }
```

#### Property Value

 [ContextMenu](Jumbee.Console.ContextMenu.md)?

#### Remarks

The right-click first selects that node and raises <xref href="Jumbee.Console.Tree.ContextMenuOpening" data-throw-if-not-resolved="false"></xref> (with the node),
    then the menu floats at the pointer. Item handlers can read <xref href="Jumbee.Console.Tree.SelectedNode" data-throw-if-not-resolved="false"></xref> to act on the
    right-clicked node.

### <a id="Jumbee_Console_Tree_Expanded"></a> Expanded

Gets or sets a value indicating whether or not the tree is expanded or not.

```csharp
public bool Expanded { get; set; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_Tree_Guide"></a> Guide

Gets or sets the tree guide lines.

```csharp
public TreeGuide Guide { get; set; }
```

#### Property Value

 [TreeGuide](Jumbee.Console.TreeGuide.md)

### <a id="Jumbee_Console_Tree_HandlesInput"></a> HandlesInput

Always <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>: the tree handles its own navigation and expand/collapse keys. No
    opt-in needed — unlike the base default, this is on.

```csharp
public override bool HandlesInput { get; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_Tree_HoverHighlighting"></a> HoverHighlighting

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, the node under the mouse pointer is tinted with <xref href="Jumbee.Console.Tree.HoverStyle" data-throw-if-not-resolved="false"></xref>.
    Defaults to <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a> (off — pointer movement is ignored so it doesn't distract from selection).

```csharp
public bool HoverHighlighting { get; set; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_Tree_HoverStyle"></a> HoverStyle

The style applied to the node under the mouse pointer when <xref href="Jumbee.Console.Tree.HoverHighlighting" data-throw-if-not-resolved="false"></xref> is on
    (typically a background tint). Defaults to the theme's <xref href="Jumbee.Console.IStyleTheme.Hover" data-throw-if-not-resolved="false"></xref>.

```csharp
public Style HoverStyle { get; set; }
```

#### Property Value

 [Style](Jumbee.Console.Style.md)

#### Remarks

Like selection, it tints the glyph + label of text nodes only.

### <a id="Jumbee_Console_Tree_LeafGlyph"></a> LeafGlyph

The glyph shown before a node that has no children (a leaf), including trailing spacing. Defaults to
    the theme's <xref href="Jumbee.Console.IGlyphTheme.TreeLeaf" data-throw-if-not-resolved="false"></xref>.

```csharp
public string LeafGlyph { get; set; }
```

#### Property Value

 string

#### Remarks

Keep its width equal to the disclosure glyphs so labels align.

### <a id="Jumbee_Console_Tree_LeafGlyphColor"></a> LeafGlyphColor

Foreground colour of the <xref href="Jumbee.Console.Tree.LeafGlyph" data-throw-if-not-resolved="false"></xref>, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> for the default text
    colour. Defaults to the theme's <xref href="Jumbee.Console.IStyleTheme.TreeLeaf" data-throw-if-not-resolved="false"></xref> colour.

```csharp
public Color? LeafGlyphColor { get; set; }
```

#### Property Value

 [Color](Jumbee.Console.Color.md)?

#### Remarks

When a (text) leaf is selected the glyph is highlighted together with the text instead of using this
    colour.

### <a id="Jumbee_Console_Tree_Root"></a> Root

The root node of the tree.

```csharp
public Tree.TreeNode Root { get; }
```

#### Property Value

 [Tree](Jumbee.Console.Tree.md).[TreeNode](Jumbee.Console.Tree.TreeNode.md)

### <a id="Jumbee_Console_Tree_SelectedBackgroundColor"></a> SelectedBackgroundColor

Background of the selected node. Defaults to the theme's <xref href="Jumbee.Console.IStyleTheme.Selection" data-throw-if-not-resolved="false"></xref>.

```csharp
public Color? SelectedBackgroundColor { get; set; }
```

#### Property Value

 [Color](Jumbee.Console.Color.md)?

### <a id="Jumbee_Console_Tree_SelectedForegroundColor"></a> SelectedForegroundColor

Foreground of the selected node. Defaults to the theme's <xref href="Jumbee.Console.IStyleTheme.Selection" data-throw-if-not-resolved="false"></xref>.

```csharp
public Color? SelectedForegroundColor { get; set; }
```

#### Property Value

 [Color](Jumbee.Console.Color.md)?

### <a id="Jumbee_Console_Tree_SelectedNode"></a> SelectedNode

The currently selected visible node, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> if none is selected.

```csharp
public Tree.TreeNode? SelectedNode { get; }
```

#### Property Value

 [Tree](Jumbee.Console.Tree.md).[TreeNode](Jumbee.Console.Tree.TreeNode.md)?

### <a id="Jumbee_Console_Tree_SelectionStyle"></a> SelectionStyle

How the selected node is indicated — highlight / underline / caret. Defaults to the theme's
    <xref href="Jumbee.Console.IStyleTheme.SelectionStyle" data-throw-if-not-resolved="false"></xref>.

```csharp
public SelectionStyle SelectionStyle { get; set; }
```

#### Property Value

 [SelectionStyle](Jumbee.Console.SelectionStyle.md)

### <a id="Jumbee_Console_Tree_Style"></a> Style

Gets or sets the tree style.

```csharp
public Style Style { get; set; }
```

#### Property Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Tree_WantsMouse"></a> WantsMouse

Always <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>: nodes receive hover and click (click to select or toggle, hover to
    highlight). No opt-in needed — unlike the base default, this is on.

```csharp
protected override bool WantsMouse { get; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_Tree_Item_System_UInt32_"></a> this\[uint\]

Gets the direct child of the root node at <code class="paramref">index</code>, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> if none.

```csharp
public Tree.TreeNode? this[uint index] { get; }
```

#### Property Value

 [Tree](Jumbee.Console.Tree.md).[TreeNode](Jumbee.Console.Tree.TreeNode.md)?

## Methods

### <a id="Jumbee_Console_Tree_AddNode_Spectre_Console_Rendering_IRenderable_"></a> AddNode\(IRenderable\)

Adds a top-level node with the given renderable label and returns it.

```csharp
public Tree.TreeNode AddNode(IRenderable label)
```

#### Parameters

`label` IRenderable

#### Returns

 [Tree](Jumbee.Console.Tree.md).[TreeNode](Jumbee.Console.Tree.TreeNode.md)

### <a id="Jumbee_Console_Tree_AddNode_System_String_"></a> AddNode\(string\)

Adds a top-level node with the given text label and returns it.

```csharp
public Tree.TreeNode AddNode(string label)
```

#### Parameters

`label` string

#### Returns

 [Tree](Jumbee.Console.Tree.md).[TreeNode](Jumbee.Console.Tree.TreeNode.md)

### <a id="Jumbee_Console_Tree_AddNodes_Spectre_Console_Rendering_IRenderable___"></a> AddNodes\(params IRenderable\[\]\)

Adds several top-level nodes from the given renderable labels; returns this tree for chaining.

```csharp
public Tree AddNodes(params IRenderable[] labels)
```

#### Parameters

`labels` IRenderable\[\]

#### Returns

 [Tree](Jumbee.Console.Tree.md)

### <a id="Jumbee_Console_Tree_AddNodes_System_String___"></a> AddNodes\(params string\[\]\)

Adds several top-level nodes from the given text labels; returns this tree for chaining.

```csharp
public Tree AddNodes(params string[] labels)
```

#### Parameters

`labels` string\[\]

#### Returns

 [Tree](Jumbee.Console.Tree.md)

### <a id="Jumbee_Console_Tree_ApplyTheme"></a> ApplyTheme\(\)

Re-captures this control's themed colours/glyphs from the current <xref href="Jumbee.Console.UI.StyleTheme" data-throw-if-not-resolved="false"></xref>/
<xref href="Jumbee.Console.UI.GlyphTheme" data-throw-if-not-resolved="false"></xref>. The default is a no-op for controls that don't use the theme.

```csharp
protected override void ApplyTheme()
```

#### Remarks

Called by themed controls from their constructor and again on a runtime theme switch (<xref href="Jumbee.Console.UI.SetTheme(Jumbee.Console.IStyleTheme%2cJumbee.Console.IGlyphTheme)" data-throw-if-not-resolved="false"></xref>).
Must read the themes <em>only here</em> (and in the constructor), never on the render path.

### <a id="Jumbee_Console_Tree_GetSpectreConsoleTreeGuide_Jumbee_Console_TreeGuide_"></a> GetSpectreConsoleTreeGuide\(TreeGuide\)

Maps a Jumbee <xref href="Jumbee.Console.TreeGuide" data-throw-if-not-resolved="false"></xref> to the corresponding Spectre.Console tree guide.

```csharp
protected static TreeGuide GetSpectreConsoleTreeGuide(TreeGuide guide)
```

#### Parameters

`guide` [TreeGuide](Jumbee.Console.TreeGuide.md)

#### Returns

 TreeGuide

### <a id="Jumbee_Console_Tree_MeasureHeight_System_Int32_"></a> MeasureHeight\(int\)

The control's content height in rows at the given <code class="paramref">width</code> — the frame's scroll range.

```csharp
public virtual int MeasureHeight(int width)
```

#### Parameters

`width` int

#### Returns

 int

#### Remarks

Measure the content, not the viewport: a list returns its item count, a text control its wrapped row count.
Returning the visible height instead defeats the purpose, leaving a scrollbar that never moves.

### <a id="Jumbee_Console_Tree_OnClick_ConsoleGUI_Space_Position_"></a> OnClick\(Position\)

Called on a press+release on this control (relative position).

```csharp
protected override void OnClick(Position position)
```

#### Parameters

`position` Position

### <a id="Jumbee_Console_Tree_OnDoubleClick_ConsoleGUI_Space_Position_"></a> OnDoubleClick\(Position\)

Called on two clicks within <xref href="Jumbee.Console.Control.DoubleClickMs" data-throw-if-not-resolved="false"></xref> at the same position.

```csharp
protected override void OnDoubleClick(Position position)
```

#### Parameters

`position` Position

### <a id="Jumbee_Console_Tree_OnInput_ConsoleGUI_Input_InputEvent_"></a> OnInput\(InputEvent\)

Handles a keyboard input event; override on input-handling controls. The default is a no-op.

```csharp
protected override void OnInput(InputEvent inputEvent)
```

#### Parameters

`inputEvent` InputEvent

### <a id="Jumbee_Console_Tree_OnMouseLeave"></a> OnMouseLeave\(\)

Called when the pointer leaves the control.

```csharp
protected override void OnMouseLeave()
```

### <a id="Jumbee_Console_Tree_OnMouseMove_ConsoleGUI_Space_Position_"></a> OnMouseMove\(Position\)

Called as the pointer moves within the control (relative position).

```csharp
protected override void OnMouseMove(Position position)
```

#### Parameters

`position` Position

### <a id="Jumbee_Console_Tree_RemoveNode_Jumbee_Console_Tree_TreeNode_"></a> RemoveNode\(TreeNode\)

Removes <code class="paramref">node</code> from the root's children; returns <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a> if it was removed.

```csharp
public bool RemoveNode(Tree.TreeNode node)
```

#### Parameters

`node` [Tree](Jumbee.Console.Tree.md).[TreeNode](Jumbee.Console.Tree.TreeNode.md)

#### Returns

 bool

### <a id="Jumbee_Console_Tree_Render_Spectre_Console_Rendering_RenderOptions_System_Int32_"></a> Render\(RenderOptions, int\)

Produces the Spectre.Console <xref href="Spectre.Console.Rendering.Segment" data-throw-if-not-resolved="false"></xref>s for the control's content within <code class="paramref">maxWidth</code>.

```csharp
protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
```

#### Parameters

`options` RenderOptions

`maxWidth` int

#### Returns

 IEnumerable<Segment\>

### <a id="Jumbee_Console_Tree_SelectNode_Jumbee_Console_Tree_TreeNode_"></a> SelectNode\(TreeNode\)

Moves the selection to <code class="paramref">node</code>, clearing the previous selection, scrolling it into view and
raising <xref href="Jumbee.Console.Tree.SelectionChanged" data-throw-if-not-resolved="false"></xref>.

```csharp
public void SelectNode(Tree.TreeNode node)
```

#### Parameters

`node` [Tree](Jumbee.Console.Tree.md).[TreeNode](Jumbee.Console.Tree.TreeNode.md)

#### Remarks

Does nothing if the node is not currently visible — it is under a collapsed ancestor, or it has been
    removed. Expand its ancestors first (see <xref href="Jumbee.Console.Tree.SetExpanded(Jumbee.Console.Tree.TreeNode%2cSystem.Boolean)" data-throw-if-not-resolved="false"></xref>) when driving the tree from elsewhere,
    such as a search field or a path.

### <a id="Jumbee_Console_Tree_SetExpanded_Jumbee_Console_Tree_TreeNode_System_Boolean_"></a> SetExpanded\(TreeNode, bool\)

Expands or collapses <code class="paramref">node</code> the way the keyboard and mouse do — raising
<xref href="Jumbee.Console.Tree.NodeExpanding" data-throw-if-not-resolved="false"></xref> first when it is about to open.

```csharp
public void SetExpanded(Tree.TreeNode node, bool expanded)
```

#### Parameters

`node` [Tree](Jumbee.Console.Tree.md).[TreeNode](Jumbee.Console.Tree.TreeNode.md)

`expanded` bool

#### Remarks

Prefer this over setting <xref href="Jumbee.Console.Tree.TreeNode.Expanded" data-throw-if-not-resolved="false"></xref> directly whenever a
    <xref href="Jumbee.Console.Tree.NodeExpanding" data-throw-if-not-resolved="false"></xref> handler is attached; the property is the raw state and does not announce
    itself.

### <a id="Jumbee_Console_Tree_ContextMenuOpening"></a> ContextMenuOpening

Raised just before <xref href="Jumbee.Console.Tree.ContextMenu" data-throw-if-not-resolved="false"></xref> is shown for a right-clicked node, with that node (now
    the selected one). Only fires when <xref href="Jumbee.Console.Tree.ContextMenu" data-throw-if-not-resolved="false"></xref> is set.

```csharp
public event EventHandler<Tree.TreeNode>? ContextMenuOpening
```

#### Event Type

 EventHandler<[Tree](Jumbee.Console.Tree.md).[TreeNode](Jumbee.Console.Tree.TreeNode.md)\>?

#### Remarks

Use it to tailor the menu to the node, or read <xref href="Jumbee.Console.Tree.SelectedNode" data-throw-if-not-resolved="false"></xref> from the menu's own item
    handlers.

### <a id="Jumbee_Console_Tree_FocusRowChanged"></a> FocusRowChanged

Raised when the control's point of interest — a selected item, a caret — moves, carrying the content rows it
now occupies. The wrapping <xref href="Jumbee.Console.ControlFrame" data-throw-if-not-resolved="false"></xref> subscribes and scrolls them into view.

```csharp
public event EventHandler<RowSpan>? FocusRowChanged
```

#### Event Type

 EventHandler<[RowSpan](Jumbee.Console.RowSpan.md)\>?

#### Remarks

<p>
Declare it as a plain field-like event and raise it wherever the selection moves:

<pre><code class="lang-csharp">public event EventHandler&lt;RowSpan&gt;? FocusRowChanged;
private void Select(int i) { _index = i; FocusRowChanged?.Invoke(this, new RowSpan(RowOf(i))); }</code></pre>

Written that way the compiler reports <code>CS0067</code> if it is never raised, which is the mistake worth
catching: a control that says it has a moving selection and then leaves it to scroll off screen.
</p>
<p>
The default implementation does nothing, so a control with no moving point of interest — a document viewer,
a panel of static text — simply omits it. Scrolls that are not selection moves (following new output to the
bottom, restoring a saved position) are not what this is for; call
<xref href="Jumbee.Console.ControlFrame.ScrollIntoView(System.Int32%2cSystem.Int32)" data-throw-if-not-resolved="false"></xref> directly instead.
</p>

### <a id="Jumbee_Console_Tree_NodeActivated"></a> NodeActivated

Raised when a leaf node (one with no children) is activated — double-clicked, or Enter/Space pressed
    while it is selected. Parent nodes toggle expansion instead of raising this.

```csharp
public event EventHandler<Tree.TreeNode>? NodeActivated
```

#### Event Type

 EventHandler<[Tree](Jumbee.Console.Tree.md).[TreeNode](Jumbee.Console.Tree.TreeNode.md)\>?

### <a id="Jumbee_Console_Tree_NodeExpanding"></a> NodeExpanding

Raised just before a node with children is expanded — by the keyboard, by a click on its disclosure glyph,
or by a double-click on its label.

```csharp
public event EventHandler<Tree.TreeNode>? NodeExpanding
```

#### Event Type

 EventHandler<[Tree](Jumbee.Console.Tree.md).[TreeNode](Jumbee.Console.Tree.TreeNode.md)\>?

#### Remarks

This is the hook for a tree too large to build up front (a file system, a remote hierarchy): populate the
node's real children here. Give the unexpanded node one placeholder child so it renders as a parent and can
be opened at all — a node with no children is a leaf, and nothing will ever ask it to expand — then replace
that placeholder when this fires.

### <a id="Jumbee_Console_Tree_SelectionChanged"></a> SelectionChanged

Raised whenever the selected (highlighted) node changes — via the arrow/vim keys,
    Home/End/PageUp/PageDown, or a mouse click. Mirrors <xref href="Jumbee.Console.ListBox.SelectionChanged" data-throw-if-not-resolved="false"></xref>; use it to follow
    highlight movement (e.g. update a detail pane as the user arrows through the tree), rather than only reacting
    to <xref href="Jumbee.Console.Tree.NodeActivated" data-throw-if-not-resolved="false"></xref> (which fires only on leaf Enter/double-click).

```csharp
public event EventHandler<Tree.TreeNode>? SelectionChanged
```

#### Event Type

 EventHandler<[Tree](Jumbee.Console.Tree.md).[TreeNode](Jumbee.Console.Tree.TreeNode.md)\>?

