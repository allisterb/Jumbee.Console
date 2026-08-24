# <a id="Jumbee_Console_SelectOption"></a> Class SelectOption

Namespace: [Jumbee.Console](Jumbee.Console.md)  
Assembly: Jumbee.Console.dll  

One option in a <xref href="Jumbee.Console.Select" data-throw-if-not-resolved="false"></xref>: either plain text, or an <xref href="Spectre.Console.Rendering.IRenderable" data-throw-if-not-resolved="false"></xref> for a row that carries
more than a string — a colour swatch beside a name, an icon, a two-column layout.

```csharp
public sealed class SelectOption
```

#### Inheritance

object ← 
[SelectOption](Jumbee.Console.SelectOption.md)

## Remarks

The same either/or a ListBoxItem makes, and for the same reason: a text option is rendered as text
(so nothing about existing <xref href="Jumbee.Console.Select" data-throw-if-not-resolved="false"></xref>s changes), and a renderable option is rendered by itself.

## Constructors

### <a id="Jumbee_Console_SelectOption__ctor_System_String_"></a> SelectOption\(string\)

Creates a text option.

```csharp
public SelectOption(string text)
```

#### Parameters

`text` string

### <a id="Jumbee_Console_SelectOption__ctor_Spectre_Console_Rendering_IRenderable_"></a> SelectOption\(IRenderable\)

Creates a renderable option. It is drawn on one row, in the closed control and in the drop-down
    alike, so a renderable that spans several lines is clipped to its first.

```csharp
public SelectOption(IRenderable content)
```

#### Parameters

`content` IRenderable

## Properties

### <a id="Jumbee_Console_SelectOption_Content"></a> Content

The option's renderable, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> when it was created from text.

```csharp
public IRenderable? Content { get; }
```

#### Property Value

 IRenderable?

### <a id="Jumbee_Console_SelectOption_Tag"></a> Tag

Application data for this option — typically the value it stands for, so a selection maps back to
    your model without a parallel array. Not used by the control.

```csharp
public object? Tag { get; set; }
```

#### Property Value

 object?

#### Remarks

The obvious companion to a renderable option: the row draws a swatch, and this carries the colour.

### <a id="Jumbee_Console_SelectOption_Text"></a> Text

The option's text, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> when it was created from a renderable.

```csharp
public string? Text { get; }
```

#### Property Value

 string?

