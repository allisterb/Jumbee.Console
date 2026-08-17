# <a id="Jumbee_Console_SliderStyle"></a> Struct SliderStyle

Namespace: [Jumbee.Console](Jumbee.Console.md)  
Assembly: Jumbee.Console.Styles.dll  

The per-part <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> a <code>Slider</code> composes: the <xref href="Jumbee.Console.SliderStyle.Label" data-throw-if-not-resolved="false"></xref>, the filled and empty
portions of the track (<xref href="Jumbee.Console.SliderStyle.Fill" data-throw-if-not-resolved="false"></xref>/<xref href="Jumbee.Console.SliderStyle.Track" data-throw-if-not-resolved="false"></xref>), the draggable <xref href="Jumbee.Console.SliderStyle.Thumb" data-throw-if-not-resolved="false"></xref>, and the
numeric <xref href="Jumbee.Console.SliderStyle.Value" data-throw-if-not-resolved="false"></xref> readout.

```csharp
public readonly struct SliderStyle
```

## Remarks

Only the foreground colour of <xref href="Jumbee.Console.SliderStyle.Fill" data-throw-if-not-resolved="false"></xref>/<xref href="Jumbee.Console.SliderStyle.Track" data-throw-if-not-resolved="false"></xref> is used — like <code>Gauge</code> and
    <code>ProgressBar</code>, the track is drawn as a solid colour band. The thumb is a foreground glyph over that band,
    so <xref href="Jumbee.Console.SliderStyle.Thumb" data-throw-if-not-resolved="false"></xref>'s foreground is what makes the handle stand out.

## Constructors

### <a id="Jumbee_Console_SliderStyle__ctor_Jumbee_Console_Style_Jumbee_Console_Style_Jumbee_Console_Style_Jumbee_Console_Style_Jumbee_Console_Style_"></a> SliderStyle\(Style, Style, Style, Style, Style\)

Initializes a new <xref href="Jumbee.Console.SliderStyle" data-throw-if-not-resolved="false"></xref> from its part styles.

```csharp
public SliderStyle(Style label, Style fill, Style track, Style thumb, Style value)
```

#### Parameters

`label` [Style](Jumbee.Console.Style.md)

`fill` [Style](Jumbee.Console.Style.md)

`track` [Style](Jumbee.Console.Style.md)

`thumb` [Style](Jumbee.Console.Style.md)

`value` [Style](Jumbee.Console.Style.md)

## Properties

### <a id="Jumbee_Console_SliderStyle_Default"></a> Default

A blue fill on a dim dark-grey track under a near-white thumb, with grey label and readout.

```csharp
public static SliderStyle Default { get; }
```

#### Property Value

 [SliderStyle](Jumbee.Console.SliderStyle.md)

### <a id="Jumbee_Console_SliderStyle_Fill"></a> Fill

The track to the left of the thumb (its foreground colour fills the band).

```csharp
public Style Fill { get; init; }
```

#### Property Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_SliderStyle_Label"></a> Label

The caption drawn before the track.

```csharp
public Style Label { get; init; }
```

#### Property Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_SliderStyle_Thumb"></a> Thumb

The handle at the fill's leading edge (its foreground colour draws the glyph).

```csharp
public Style Thumb { get; init; }
```

#### Property Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_SliderStyle_Track"></a> Track

The track to the right of the thumb (its foreground colour fills the band).

```csharp
public Style Track { get; init; }
```

#### Property Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_SliderStyle_Value"></a> Value

The numeric readout drawn after the track.

```csharp
public Style Value { get; init; }
```

#### Property Value

 [Style](Jumbee.Console.Style.md)

## Methods

### <a id="Jumbee_Console_SliderStyle_Equals_Jumbee_Console_SliderStyle_"></a> Equals\(SliderStyle\)

Determines whether this <xref href="Jumbee.Console.SliderStyle" data-throw-if-not-resolved="false"></xref> equals <code class="paramref">other</code>.

```csharp
public bool Equals(SliderStyle other)
```

#### Parameters

`other` [SliderStyle](Jumbee.Console.SliderStyle.md)

#### Returns

 bool

### <a id="Jumbee_Console_SliderStyle_Equals_System_Object_"></a> Equals\(object?\)

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

### <a id="Jumbee_Console_SliderStyle_GetHashCode"></a> GetHashCode\(\)

Returns the hash code for this instance.

```csharp
public override int GetHashCode()
```

#### Returns

 int

A 32-bit signed integer that is the hash code for this instance.

### <a id="Jumbee_Console_SliderStyle_WithFill_Jumbee_Console_Color_"></a> WithFill\(Color\)

A copy with the filled portion recoloured (keeps every other part).

```csharp
public SliderStyle WithFill(Color fill)
```

#### Parameters

`fill` [Color](Jumbee.Console.Color.md)

#### Returns

 [SliderStyle](Jumbee.Console.SliderStyle.md)

### <a id="Jumbee_Console_SliderStyle_WithThumb_Jumbee_Console_Color_"></a> WithThumb\(Color\)

A copy with the thumb recoloured (keeps every other part).

```csharp
public SliderStyle WithThumb(Color thumb)
```

#### Parameters

`thumb` [Color](Jumbee.Console.Color.md)

#### Returns

 [SliderStyle](Jumbee.Console.SliderStyle.md)

## Operators

### <a id="Jumbee_Console_SliderStyle_op_Equality_Jumbee_Console_SliderStyle_Jumbee_Console_SliderStyle_"></a> operator ==\(SliderStyle, SliderStyle\)

Equality operator.

```csharp
public static bool operator ==(SliderStyle a, SliderStyle b)
```

#### Parameters

`a` [SliderStyle](Jumbee.Console.SliderStyle.md)

`b` [SliderStyle](Jumbee.Console.SliderStyle.md)

#### Returns

 bool

### <a id="Jumbee_Console_SliderStyle_op_Inequality_Jumbee_Console_SliderStyle_Jumbee_Console_SliderStyle_"></a> operator \!=\(SliderStyle, SliderStyle\)

Inequality operator.

```csharp
public static bool operator !=(SliderStyle a, SliderStyle b)
```

#### Parameters

`a` [SliderStyle](Jumbee.Console.SliderStyle.md)

`b` [SliderStyle](Jumbee.Console.SliderStyle.md)

#### Returns

 bool

