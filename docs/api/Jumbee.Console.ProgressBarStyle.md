# <a id="Jumbee_Console_ProgressBarStyle"></a> Struct ProgressBarStyle

Namespace: [Jumbee.Console](Jumbee.Console.md)  
Assembly: Jumbee.Console.Styles.dll  

The per-part <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> a <code>ProgressBar</code> composes: the task <xref href="Jumbee.Console.ProgressBarStyle.Description" data-throw-if-not-resolved="false"></xref>, the
filled and empty portions of the bar (<xref href="Jumbee.Console.ProgressBarStyle.Fill" data-throw-if-not-resolved="false"></xref>/<xref href="Jumbee.Console.ProgressBarStyle.Track" data-throw-if-not-resolved="false"></xref>), and the three optional
readouts — <xref href="Jumbee.Console.ProgressBarStyle.Percentage" data-throw-if-not-resolved="false"></xref>, <xref href="Jumbee.Console.ProgressBarStyle.Time" data-throw-if-not-resolved="false"></xref> and <xref href="Jumbee.Console.ProgressBarStyle.Spinner" data-throw-if-not-resolved="false"></xref>.

```csharp
public readonly struct ProgressBarStyle
```

## Remarks

Like <code>GaugeStyle</code>, only the foreground colour of <xref href="Jumbee.Console.ProgressBarStyle.Fill" data-throw-if-not-resolved="false"></xref>/<xref href="Jumbee.Console.ProgressBarStyle.Track" data-throw-if-not-resolved="false"></xref> is used —
    the bar is drawn as a solid colour band.

## Constructors

### <a id="Jumbee_Console_ProgressBarStyle__ctor_Jumbee_Console_Style_Jumbee_Console_Style_Jumbee_Console_Style_Jumbee_Console_Style_Jumbee_Console_Style_Jumbee_Console_Style_"></a> ProgressBarStyle\(Style, Style, Style, Style, Style, Style\)

Initializes a new <xref href="Jumbee.Console.ProgressBarStyle" data-throw-if-not-resolved="false"></xref> from its part styles.

```csharp
public ProgressBarStyle(Style description, Style fill, Style track, Style percentage, Style time, Style spinner)
```

#### Parameters

`description` [Style](Jumbee.Console.Style.md)

`fill` [Style](Jumbee.Console.Style.md)

`track` [Style](Jumbee.Console.Style.md)

`percentage` [Style](Jumbee.Console.Style.md)

`time` [Style](Jumbee.Console.Style.md)

`spinner` [Style](Jumbee.Console.Style.md)

## Properties

### <a id="Jumbee_Console_ProgressBarStyle_Default"></a> Default

A green fill on a dim dark-grey track; grey description and percentage, a soft blue time, a green
    spinner — the Spectre progress-row look.

```csharp
public static ProgressBarStyle Default { get; }
```

#### Property Value

 [ProgressBarStyle](Jumbee.Console.ProgressBarStyle.md)

### <a id="Jumbee_Console_ProgressBarStyle_Description"></a> Description

The task status text drawn before the bar.

```csharp
public Style Description { get; init; }
```

#### Property Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_ProgressBarStyle_Fill"></a> Fill

The filled portion of the bar (its foreground colour fills the band).

```csharp
public Style Fill { get; init; }
```

#### Property Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_ProgressBarStyle_GradientTo"></a> GradientTo

When set, the bar fill is a gradient interpolated from <xref href="Jumbee.Console.ProgressBarStyle.Fill" data-throw-if-not-resolved="false"></xref>'s colour to this one across
    the bar width (per cell). <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> (the default) fills with the single <xref href="Jumbee.Console.ProgressBarStyle.Fill" data-throw-if-not-resolved="false"></xref> colour.

```csharp
public Color? GradientTo { get; init; }
```

#### Property Value

 [Color](Jumbee.Console.Color.md)?

### <a id="Jumbee_Console_ProgressBarStyle_Percentage"></a> Percentage

The percentage readout (e.g. <code>96%</code>).

```csharp
public Style Percentage { get; init; }
```

#### Property Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_ProgressBarStyle_Spinner"></a> Spinner

The animated spinner glyph.

```csharp
public Style Spinner { get; init; }
```

#### Property Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_ProgressBarStyle_Time"></a> Time

The elapsed/remaining time readout (e.g. <code>00:00:00</code>).

```csharp
public Style Time { get; init; }
```

#### Property Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_ProgressBarStyle_Track"></a> Track

The empty track behind the fill (its foreground colour fills the band).

```csharp
public Style Track { get; init; }
```

#### Property Value

 [Style](Jumbee.Console.Style.md)

## Methods

### <a id="Jumbee_Console_ProgressBarStyle_Equals_Jumbee_Console_ProgressBarStyle_"></a> Equals\(ProgressBarStyle\)

Determines whether this <xref href="Jumbee.Console.ProgressBarStyle" data-throw-if-not-resolved="false"></xref> equals <code class="paramref">other</code>.

```csharp
public bool Equals(ProgressBarStyle other)
```

#### Parameters

`other` [ProgressBarStyle](Jumbee.Console.ProgressBarStyle.md)

#### Returns

 bool

### <a id="Jumbee_Console_ProgressBarStyle_Equals_System_Object_"></a> Equals\(object?\)

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

### <a id="Jumbee_Console_ProgressBarStyle_GetHashCode"></a> GetHashCode\(\)

Returns the hash code for this instance.

```csharp
public override int GetHashCode()
```

#### Returns

 int

A 32-bit signed integer that is the hash code for this instance.

### <a id="Jumbee_Console_ProgressBarStyle_WithFill_Jumbee_Console_Color_"></a> WithFill\(Color\)

A copy with the bar fill recoloured (keeps every other part).

```csharp
public ProgressBarStyle WithFill(Color fill)
```

#### Parameters

`fill` [Color](Jumbee.Console.Color.md)

#### Returns

 [ProgressBarStyle](Jumbee.Console.ProgressBarStyle.md)

### <a id="Jumbee_Console_ProgressBarStyle_WithGradient_Jumbee_Console_Color_Jumbee_Console_Color_"></a> WithGradient\(Color, Color\)

A copy whose fill is a gradient from <code class="paramref">from</code> to <code class="paramref">to</code> across the bar.

```csharp
public ProgressBarStyle WithGradient(Color from, Color to)
```

#### Parameters

`from` [Color](Jumbee.Console.Color.md)

`to` [Color](Jumbee.Console.Color.md)

#### Returns

 [ProgressBarStyle](Jumbee.Console.ProgressBarStyle.md)

## Operators

### <a id="Jumbee_Console_ProgressBarStyle_op_Equality_Jumbee_Console_ProgressBarStyle_Jumbee_Console_ProgressBarStyle_"></a> operator ==\(ProgressBarStyle, ProgressBarStyle\)

Equality operator.

```csharp
public static bool operator ==(ProgressBarStyle a, ProgressBarStyle b)
```

#### Parameters

`a` [ProgressBarStyle](Jumbee.Console.ProgressBarStyle.md)

`b` [ProgressBarStyle](Jumbee.Console.ProgressBarStyle.md)

#### Returns

 bool

### <a id="Jumbee_Console_ProgressBarStyle_op_Inequality_Jumbee_Console_ProgressBarStyle_Jumbee_Console_ProgressBarStyle_"></a> operator \!=\(ProgressBarStyle, ProgressBarStyle\)

Inequality operator.

```csharp
public static bool operator !=(ProgressBarStyle a, ProgressBarStyle b)
```

#### Parameters

`a` [ProgressBarStyle](Jumbee.Console.ProgressBarStyle.md)

`b` [ProgressBarStyle](Jumbee.Console.ProgressBarStyle.md)

#### Returns

 bool

