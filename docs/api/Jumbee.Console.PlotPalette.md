# <a id="Jumbee_Console_PlotPalette"></a> Struct PlotPalette

Namespace: [Jumbee.Console](Jumbee.Console.md)  
Assembly: Jumbee.Console.Styles.dll  

The ordered colours a plot cycles through for series that don't name one.

```csharp
public readonly struct PlotPalette
```

## Remarks

<p>
A value type with structural equality, like every other grouped theme token, and for the same reason: themed
properties are compared with <xref href="System.Collections.Generic.EqualityComparer%601.Default" data-throw-if-not-resolved="false"></xref> on assignment, and a bare array or
<xref href="System.Collections.Generic.IReadOnlyList%601" data-throw-if-not-resolved="false"></xref> would compare by <em>reference</em>. Two identical palettes would then count as
different (repainting and re-laying-out needlessly), while a palette mutated in place would count as the same
(and the change would be silently dropped). Construction copies the caller's sequence, so a palette also cannot
change under a control that has already captured it.
</p>
<p>
The indexer wraps, so a palette is never "too short" for the number of series and there is no arity contract for
a theme to violate. A <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/default">default</a> instance behaves as <xref href="Jumbee.Console.PlotPalette.Default" data-throw-if-not-resolved="false"></xref> rather than throwing.
</p>

## Constructors

### <a id="Jumbee_Console_PlotPalette__ctor_System_Collections_Generic_IEnumerable_Jumbee_Console_Color__"></a> PlotPalette\(IEnumerable<Color\>?\)

Creates a palette from <code class="paramref">colors</code>, which is copied. An empty or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a>
    sequence yields <xref href="Jumbee.Console.PlotPalette.Default" data-throw-if-not-resolved="false"></xref>.

```csharp
public PlotPalette(IEnumerable<Color>? colors)
```

#### Parameters

`colors` IEnumerable<[Color](Jumbee.Console.Color.md)\>?

## Fields

### <a id="Jumbee_Console_PlotPalette_Default"></a> Default

Pleasant, high-contrast defaults, cycled by series index.

```csharp
public static readonly PlotPalette Default
```

#### Field Value

 [PlotPalette](Jumbee.Console.PlotPalette.md)

#### Remarks

Deliberately not the plotting library's own defaults, whose first entry is black — invisible on a
    dark terminal.

## Properties

### <a id="Jumbee_Console_PlotPalette_Count"></a> Count

How many colours before the cycle repeats.

```csharp
public int Count { get; }
```

#### Property Value

 int

### <a id="Jumbee_Console_PlotPalette_Item_System_Int32_"></a> this\[int\]

The colour for <code class="paramref">index</code>, wrapping once the palette is exhausted.

```csharp
public Color this[int index] { get; }
```

#### Property Value

 [Color](Jumbee.Console.Color.md)

## Methods

### <a id="Jumbee_Console_PlotPalette_Equals_Jumbee_Console_PlotPalette_"></a> Equals\(PlotPalette\)

Indicates whether the current object is equal to another object of the same type.

```csharp
public bool Equals(PlotPalette other)
```

#### Parameters

`other` [PlotPalette](Jumbee.Console.PlotPalette.md)

An object to compare with this object.

#### Returns

 bool

<a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a> if the current object is equal to the <code class="paramref">other</code> parameter; otherwise, <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a>.

### <a id="Jumbee_Console_PlotPalette_Equals_System_Object_"></a> Equals\(object?\)

Indicates whether this instance and a specified object are equal.

```csharp
public override bool Equals(object? obj)
```

#### Parameters

`obj` object?

The object to compare with the current instance.

#### Returns

 bool

<a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">true</a> if <code class="paramref">obj</code> and this instance are the same type and represent the same value; otherwise, <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool">false</a>.

### <a id="Jumbee_Console_PlotPalette_GetHashCode"></a> GetHashCode\(\)

Returns the hash code for this instance.

```csharp
public override int GetHashCode()
```

#### Returns

 int

A 32-bit signed integer that is the hash code for this instance.

### <a id="Jumbee_Console_PlotPalette_ToList"></a> ToList\(\)

The colours in order, for a caller that needs the whole sequence rather than one entry.

```csharp
public IReadOnlyList<Color> ToList()
```

#### Returns

 IReadOnlyList<[Color](Jumbee.Console.Color.md)\>

## Operators

### <a id="Jumbee_Console_PlotPalette_op_Equality_Jumbee_Console_PlotPalette_Jumbee_Console_PlotPalette_"></a> operator ==\(PlotPalette, PlotPalette\)

Value equality — see <xref href="Jumbee.Console.PlotPalette.Equals(Jumbee.Console.PlotPalette)" data-throw-if-not-resolved="false"></xref>.

```csharp
public static bool operator ==(PlotPalette left, PlotPalette right)
```

#### Parameters

`left` [PlotPalette](Jumbee.Console.PlotPalette.md)

`right` [PlotPalette](Jumbee.Console.PlotPalette.md)

#### Returns

 bool

### <a id="Jumbee_Console_PlotPalette_op_Inequality_Jumbee_Console_PlotPalette_Jumbee_Console_PlotPalette_"></a> operator \!=\(PlotPalette, PlotPalette\)

Value inequality — see <xref href="Jumbee.Console.PlotPalette.Equals(Jumbee.Console.PlotPalette)" data-throw-if-not-resolved="false"></xref>.

```csharp
public static bool operator !=(PlotPalette left, PlotPalette right)
```

#### Parameters

`left` [PlotPalette](Jumbee.Console.PlotPalette.md)

`right` [PlotPalette](Jumbee.Console.PlotPalette.md)

#### Returns

 bool

