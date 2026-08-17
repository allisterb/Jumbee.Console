# <a id="Jumbee_Console_MenuItem"></a> Class MenuItem

Namespace: [Jumbee.Console](Jumbee.Console.md)  
Assembly: Jumbee.Console.dll  

One entry in a <xref href="Jumbee.Console.ContextMenu" data-throw-if-not-resolved="false"></xref>: a label, an optional right-aligned shortcut hint, an action invoked
when chosen, an enabled flag, and an optional <xref href="Jumbee.Console.MenuItem.Submenu" data-throw-if-not-resolved="false"></xref>. Use <xref href="Jumbee.Console.MenuItem.Separator" data-throw-if-not-resolved="false"></xref> for a
non-selectable divider line.

```csharp
public sealed class MenuItem
```

#### Inheritance

object ← 
[MenuItem](Jumbee.Console.MenuItem.md)

## Constructors

### <a id="Jumbee_Console_MenuItem__ctor"></a> MenuItem\(\)

Initializes an empty <xref href="Jumbee.Console.MenuItem" data-throw-if-not-resolved="false"></xref>.

```csharp
public MenuItem()
```

### <a id="Jumbee_Console_MenuItem__ctor_System_String_System_Action_"></a> MenuItem\(string, Action?\)

Initializes a <xref href="Jumbee.Console.MenuItem" data-throw-if-not-resolved="false"></xref> with the given label and optional action.

```csharp
public MenuItem(string text, Action? action = null)
```

#### Parameters

`text` string

`action` Action?

### <a id="Jumbee_Console_MenuItem__ctor_System_String_System_Collections_Generic_IEnumerable_Jumbee_Console_MenuItem__"></a> MenuItem\(string, IEnumerable<MenuItem\>\)

A parent item that opens <code class="paramref">submenu</code> when chosen (Right/Enter, or hover).

```csharp
public MenuItem(string text, IEnumerable<MenuItem> submenu)
```

#### Parameters

`text` string

`submenu` IEnumerable<[MenuItem](Jumbee.Console.MenuItem.md)\>

## Fields

### <a id="Jumbee_Console_MenuItem_Separator"></a> Separator

A non-selectable divider row.

```csharp
public static readonly MenuItem Separator
```

#### Field Value

 [MenuItem](Jumbee.Console.MenuItem.md)

## Properties

### <a id="Jumbee_Console_MenuItem_Action"></a> Action

The action invoked when the item is chosen.

```csharp
public Action? Action { get; init; }
```

#### Property Value

 Action?

### <a id="Jumbee_Console_MenuItem_Checked"></a> Checked

The item's check state, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> (the default) for an item that is not
    checkable.

```csharp
public bool? Checked { get; init; }
```

#### Property Value

 bool?

#### Remarks

A menu level containing any checkable item reserves a marker column across <em>all</em> its rows, so labels
stay aligned whether or not anything is currently checked — which is why this is nullable rather than a plain
<a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">bool</a>: <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a> means "checkable, currently off" and reserves the column,
<a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> means "not that kind of item". Use it for a mode a menu both sets and reports (which
renderer is live, say). The glyph comes from <xref href="Jumbee.Console.IGlyphTheme.MenuChecked" data-throw-if-not-resolved="false"></xref>.

### <a id="Jumbee_Console_MenuItem_Enabled"></a> Enabled

Whether the item is selectable; disabled items are shown muted and skipped.

```csharp
public bool Enabled { get; init; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_MenuItem_IsSeparator"></a> IsSeparator

Whether the item is a non-selectable divider row.

```csharp
public bool IsSeparator { get; init; }
```

#### Property Value

 bool

### <a id="Jumbee_Console_MenuItem_Shortcut"></a> Shortcut

An optional right-aligned shortcut hint (display only).

```csharp
public string? Shortcut { get; init; }
```

#### Property Value

 string?

### <a id="Jumbee_Console_MenuItem_Submenu"></a> Submenu

Child items shown as a submenu to the right when this item is highlighted or chosen.

```csharp
public IReadOnlyList<MenuItem>? Submenu { get; init; }
```

#### Property Value

 IReadOnlyList<[MenuItem](Jumbee.Console.MenuItem.md)\>?

#### Remarks

When set, the item opens the submenu instead of running its <xref href="Jumbee.Console.MenuItem.Action" data-throw-if-not-resolved="false"></xref>. Nest to any
    depth.

### <a id="Jumbee_Console_MenuItem_Text"></a> Text

The item's label text.

```csharp
public string Text { get; init; }
```

#### Property Value

 string

