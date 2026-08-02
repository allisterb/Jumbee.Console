# <a id="Jumbee_Console_TabPanel"></a> Class TabPanel

Namespace: [Jumbee.Console](Jumbee.Console.md)  
Assembly: Jumbee.Console.dll  

A tabbed container: a bar of selectable <xref href="Jumbee.Console.TabHeader" data-throw-if-not-resolved="false"></xref> labels docked on one edge, with the selected
tab's content filling the rest.

```csharp
public class TabPanel : Layout<TabPanelDockPanel>, ILayout, IFocusable
```

#### Inheritance

object ← 
[Layout<TabPanelDockPanel\>](Jumbee.Console.Layout\-1.md) ← 
[TabPanel](Jumbee.Console.TabPanel.md)

#### Implements

[ILayout](Jumbee.Console.ILayout.md), 
[IFocusable](Jumbee.Console.IFocusable.md)

#### Inherited Members

[Layout<TabPanelDockPanel\>.this\[int, int\]](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_Item\_System\_Int32\_System\_Int32\_), 
[Layout<TabPanelDockPanel\>.Rows](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_Rows), 
[Layout<TabPanelDockPanel\>.Columns](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_Columns), 
[Layout<TabPanelDockPanel\>.this\[Position\]](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_Item\_ConsoleGUI\_Space\_Position\_), 
[Layout<TabPanelDockPanel\>.Size](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_Size), 
[Layout<TabPanelDockPanel\>.CControl](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_CControl), 
[Layout<TabPanelDockPanel\>.Context](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_Context), 
[Layout<TabPanelDockPanel\>.Controls](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_Controls), 
[Layout<TabPanelDockPanel\>.Focusable](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_Focusable), 
[Layout<TabPanelDockPanel\>.FocusableControl](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_FocusableControl), 
[Layout<TabPanelDockPanel\>.IsFocused](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_IsFocused), 
[Layout<TabPanelDockPanel\>.HandlesInput](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_HandlesInput), 
[Layout<TabPanelDockPanel\>.FocusedControl](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_FocusedControl), 
[Layout<TabPanelDockPanel\>.OnFocus](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_OnFocus), 
[Layout<TabPanelDockPanel\>.OnLostFocus](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_OnLostFocus), 
[Layout<TabPanelDockPanel\>.OnRedraw\(DrawingContext\)](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_OnRedraw\_ConsoleGUI\_Common\_DrawingContext\_), 
[Layout<TabPanelDockPanel\>.OnUpdate\(DrawingContext, Rect\)](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_OnUpdate\_ConsoleGUI\_Common\_DrawingContext\_ConsoleGUI\_Space\_Rect\_), 
[Layout<TabPanelDockPanel\>.OnInput\(UI.InputEventArgs\)](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_OnInput\_Jumbee\_Console\_UI\_InputEventArgs\_), 
[Layout<TabPanelDockPanel\>.InterceptInput\(UI.InputEventArgs\)](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_InterceptInput\_Jumbee\_Console\_UI\_InputEventArgs\_), 
[Layout<TabPanelDockPanel\>.OnPaste\(string\)](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_OnPaste\_System\_String\_), 
[Layout<TabPanelDockPanel\>.control](Jumbee.Console.Layout\-1.md\#Jumbee\_Console\_Layout\_1\_control)

## Remarks

Select a tab by clicking its label, by the arrow keys while the bar is focused (Left/Right for a top/bottom bar,
Up/Down for a left/right bar), or programmatically via <xref href="Jumbee.Console.TabPanel.SelectedIndex" data-throw-if-not-resolved="false"></xref>. Tabs can be added, removed,
hidden, disabled, and relabelled at runtime — via <xref href="Jumbee.Console.TabPanel.AddTab(System.String%2cJumbee.Console.IFocusable%2cSystem.Int32)" data-throw-if-not-resolved="false"></xref> / <xref href="Jumbee.Console.TabPanel.RemoveTab(Jumbee.Console.TabItem)" data-throw-if-not-resolved="false"></xref> and the
returned <xref href="Jumbee.Console.TabItem" data-throw-if-not-resolved="false"></xref> handle (whose identity is stable across structural changes, unlike an index).

## Constructors

### <a id="Jumbee_Console_TabPanel__ctor_Jumbee_Console_TabBarDock_System_ValueTuple_System_String_Jumbee_Console_IFocusable____"></a> TabPanel\(TabBarDock, params \(string Name, IFocusable Content\)\[\]\)

Initializes a new <xref href="Jumbee.Console.TabPanel" data-throw-if-not-resolved="false"></xref> with its bar docked at <code class="paramref">tabBarDock</code> and the given <code class="paramref">tabs</code> (first selectable tab auto-selects).

```csharp
public TabPanel(TabBarDock tabBarDock, params (string Name, IFocusable Content)[] tabs)
```

#### Parameters

`tabBarDock` [TabBarDock](Jumbee.Console.TabBarDock.md)

`tabs` \(string Name, [IFocusable](Jumbee.Console.IFocusable.md) Content\)\[\]

## Properties

### <a id="Jumbee_Console_TabPanel_ActiveContent"></a> ActiveContent

The selected tab's content, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> when no tab is selected.

```csharp
public IFocusable? ActiveContent { get; }
```

#### Property Value

 [IFocusable](Jumbee.Console.IFocusable.md)?

### <a id="Jumbee_Console_TabPanel_ActiveTab"></a> ActiveTab

The selected tab handle, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> when no tab is selectable.

```csharp
public TabItem? ActiveTab { get; }
```

#### Property Value

 [TabItem](Jumbee.Console.TabItem.md)?

### <a id="Jumbee_Console_TabPanel_ActiveTabName"></a> ActiveTabName

The selected tab's name, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> when no tab is selected.

```csharp
public string? ActiveTabName { get; }
```

#### Property Value

 string?

### <a id="Jumbee_Console_TabPanel_ClosableTabs"></a> ClosableTabs

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, every tab shows a clickable close (✕) glyph (on the active/hovered
    tab) and clicking it raises the cancelable <xref href="Jumbee.Console.TabPanel.TabCloseRequested" data-throw-if-not-resolved="false"></xref>. Default <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a>.

```csharp
public bool ClosableTabs { get; set; }
```

#### Property Value

 bool

#### Remarks

Applies to existing and future tabs.

### <a id="Jumbee_Console_TabPanel_Columns"></a> Columns

Number of columns in the layout grid (always 1).

```csharp
public override int Columns { get; }
```

#### Property Value

 int

### <a id="Jumbee_Console_TabPanel_FocusedControl"></a> FocusedControl

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

### <a id="Jumbee_Console_TabPanel_Headers"></a> Headers

The tab header labels, in order (for inspection or per-tab styling).

```csharp
public IReadOnlyList<TabHeader> Headers { get; }
```

#### Property Value

 IReadOnlyList<[TabHeader](Jumbee.Console.TabHeader.md)\>

### <a id="Jumbee_Console_TabPanel_Rows"></a> Rows

Number of logical rows for input routing: the selectable tab headers plus one for the active content.

```csharp
public override int Rows { get; }
```

#### Property Value

 int

### <a id="Jumbee_Console_TabPanel_SelectedIndex"></a> SelectedIndex

The zero-based selected tab, or -1 when no tab is selected.

```csharp
public int SelectedIndex { get; set; }
```

#### Property Value

 int

#### Remarks

Setting it activates that tab (clamped to range) if it is selectable (not hidden or disabled); raises
    <xref href="Jumbee.Console.TabPanel.SelectionChanged" data-throw-if-not-resolved="false"></xref> when it actually changes.

### <a id="Jumbee_Console_TabPanel_SelectionStyle"></a> SelectionStyle

How the active tab is indicated — highlight / underline / caret.

```csharp
public SelectionStyle SelectionStyle { get; set; }
```

#### Property Value

 [SelectionStyle](Jumbee.Console.SelectionStyle.md)

#### Remarks

Defaults to the theme's <xref href="Jumbee.Console.IStyleTheme.SelectionStyle" data-throw-if-not-resolved="false"></xref>; setting it applies to every tab
    header (and future ones).

### <a id="Jumbee_Console_TabPanel_ShowAddButton"></a> ShowAddButton

When <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a>, a "+" button is shown at the end of the bar; clicking it raises
    <xref href="Jumbee.Console.TabPanel.NewTabRequested" data-throw-if-not-resolved="false"></xref>. Default <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a>.

```csharp
public bool ShowAddButton { get; set; }
```

#### Property Value

 bool

#### Remarks

The button is mouse-only (not part of keyboard tab traversal).

### <a id="Jumbee_Console_TabPanel_TabCount"></a> TabCount

The number of tabs (including hidden and disabled ones).

```csharp
public int TabCount { get; }
```

#### Property Value

 int

### <a id="Jumbee_Console_TabPanel_Tabs"></a> Tabs

The tab handles, in order (including hidden and disabled tabs).

```csharp
public IReadOnlyList<TabItem> Tabs { get; }
```

#### Property Value

 IReadOnlyList<[TabItem](Jumbee.Console.TabItem.md)\>

### <a id="Jumbee_Console_TabPanel_Item_System_Int32_System_Int32_"></a> this\[int, int\]

Gets the logical child at <code class="paramref">row</code>: a selectable tab header, or the active content in the last row.

```csharp
public override IFocusable this[int row, int column] { get; }
```

#### Property Value

 [IFocusable](Jumbee.Console.IFocusable.md)

## Methods

### <a id="Jumbee_Console_TabPanel_AddTab_System_String_Jumbee_Console_IFocusable_System_Int32_"></a> AddTab\(string, IFocusable, int\)

Adds a tab, optionally at <code class="paramref">index</code> (default appends). Returns its handle.

```csharp
public TabItem AddTab(string name, IFocusable content, int index = -1)
```

#### Parameters

`name` string

`content` [IFocusable](Jumbee.Console.IFocusable.md)

`index` int

#### Returns

 [TabItem](Jumbee.Console.TabItem.md)

#### Remarks

If nothing is selected yet and the new tab is selectable, it becomes selected.

### <a id="Jumbee_Console_TabPanel_InterceptInput_Jumbee_Console_UI_InputEventArgs_"></a> InterceptInput\(InputEventArgs\)

Intercepts Alt+arrow tab switching, "+"-button keys, and header arrow navigation before input routes to the focused control.

```csharp
protected override bool InterceptInput(UI.InputEventArgs inputEventArgs)
```

#### Parameters

`inputEventArgs` [UI](Jumbee.Console.UI.md).[InputEventArgs](Jumbee.Console.UI.InputEventArgs.md)

#### Returns

 bool

### <a id="Jumbee_Console_TabPanel_RemoveTab_Jumbee_Console_TabItem_"></a> RemoveTab\(TabItem\)

Removes the tab. If it was selected, selection moves to the nearest selectable tab (or clears).

```csharp
public void RemoveTab(TabItem tab)
```

#### Parameters

`tab` [TabItem](Jumbee.Console.TabItem.md)

### <a id="Jumbee_Console_TabPanel_RemoveTab_System_Int32_"></a> RemoveTab\(int\)

Removes the tab at <code class="paramref">index</code>.

```csharp
public void RemoveTab(int index)
```

#### Parameters

`index` int

### <a id="Jumbee_Console_TabPanel_SelectTab_System_Int32_"></a> SelectTab\(int\)

Selects the tab at <code class="paramref">index</code> (clamped). Equivalent to setting <xref href="Jumbee.Console.TabPanel.SelectedIndex" data-throw-if-not-resolved="false"></xref>.

```csharp
public void SelectTab(int index)
```

#### Parameters

`index` int

### <a id="Jumbee_Console_TabPanel_SelectTab_Jumbee_Console_TabItem_"></a> SelectTab\(TabItem\)

Selects the given tab if it is selectable.

```csharp
public void SelectTab(TabItem tab)
```

#### Parameters

`tab` [TabItem](Jumbee.Console.TabItem.md)

### <a id="Jumbee_Console_TabPanel_NewTabRequested"></a> NewTabRequested

Raised when the "+" new-tab button is clicked (see <xref href="Jumbee.Console.TabPanel.ShowAddButton" data-throw-if-not-resolved="false"></xref>). The handler
    typically opens a new tab.

```csharp
public event EventHandler? NewTabRequested
```

#### Event Type

 EventHandler?

### <a id="Jumbee_Console_TabPanel_SelectionChanged"></a> SelectionChanged

Raised after the selected tab changes, with the new index (-1 when no tab is selectable).

```csharp
public event EventHandler<int>? SelectionChanged
```

#### Event Type

 EventHandler<int\>?

### <a id="Jumbee_Console_TabPanel_TabCloseRequested"></a> TabCloseRequested

Raised when a closable tab's ✕ is clicked (see <xref href="Jumbee.Console.TabPanel.ClosableTabs" data-throw-if-not-resolved="false"></xref>).

```csharp
public event EventHandler<TabCloseEventArgs>? TabCloseRequested
```

#### Event Type

 EventHandler<[TabCloseEventArgs](Jumbee.Console.TabCloseEventArgs.md)\>?

#### Remarks

Set <xref href="Jumbee.Console.TabCloseEventArgs.Cancel" data-throw-if-not-resolved="false"></xref> to keep the tab (e.g. after prompting about unsaved
    changes); otherwise the panel removes it.

### <a id="Jumbee_Console_TabPanel_TabRemoved"></a> TabRemoved

Raised after a tab has been removed (via ✕, <xref href="Jumbee.Console.TabPanel.RemoveTab(Jumbee.Console.TabItem)" data-throw-if-not-resolved="false"></xref>, etc.), with its handle.

```csharp
public event EventHandler<TabItem>? TabRemoved
```

#### Event Type

 EventHandler<[TabItem](Jumbee.Console.TabItem.md)\>?

