# <a id="Jumbee_Console_ProgressBarGlyphs"></a> Struct ProgressBarGlyphs

Namespace: [Jumbee.Console](Jumbee.Console.md)  
Assembly: Jumbee.Console.Styles.dll  

The glyphs (no colours) a <code>ProgressBar</code> draws its bar with: the <xref href="Jumbee.Console.ProgressBarGlyphs.Fill" data-throw-if-not-resolved="false"></xref> for a filled cell and the
<xref href="Jumbee.Console.ProgressBarGlyphs.Track" data-throw-if-not-resolved="false"></xref> for an empty one, plus the <xref href="Jumbee.Console.ProgressBarGlyphs.Mode" data-throw-if-not-resolved="false"></xref> that selects solid-band or per-cell-glyph
rendering.

```csharp
public readonly struct ProgressBarGlyphs
```

## Remarks

Mirrors <xref href="Jumbee.Console.ScrollBarGlyphs" data-throw-if-not-resolved="false"></xref>: colours come separately from <xref href="Jumbee.Console.ProgressBarStyle" data-throw-if-not-resolved="false"></xref> (via
<xref href="Jumbee.Console.IStyleTheme.ProgressBar" data-throw-if-not-resolved="false"></xref>), and <xref href="Jumbee.Console.ProgressBarFillMode.Solid" data-throw-if-not-resolved="false"></xref> (the default) ignores the
glyphs and draws a smooth sub-cell band. Glyphs such as <code>▨ ▓ █ ▱</code> need block/box-drawing font coverage; the
<xref href="Jumbee.Console.ProgressBarGlyphs.Ascii" data-throw-if-not-resolved="false"></xref> preset is the portable fallback.

## Constructors

### <a id="Jumbee_Console_ProgressBarGlyphs__ctor_System_String_System_String_"></a> ProgressBarGlyphs\(string, string\)

Builds a <xref href="Jumbee.Console.ProgressBarFillMode.Glyph" data-throw-if-not-resolved="false"></xref> glyph set (explicit glyphs imply glyph mode).

```csharp
public ProgressBarGlyphs(string fill, string track)
```

#### Parameters

`fill` string

`track` string

## Properties

### <a id="Jumbee_Console_ProgressBarGlyphs_Ascii"></a> Ascii

A portable fallback for terminals without block glyphs: a <code>#</code> fill on a <code>-</code> track.

```csharp
public static ProgressBarGlyphs Ascii { get; }
```

#### Property Value

 [ProgressBarGlyphs](Jumbee.Console.ProgressBarGlyphs.md)

### <a id="Jumbee_Console_ProgressBarGlyphs_Dashed"></a> Dashed

Discrete segments with visible gaps: a filled parallelogram (<code>▰</code>) fill on an empty one (<code>▱</code>).

```csharp
public static ProgressBarGlyphs Dashed { get; }
```

#### Property Value

 [ProgressBarGlyphs](Jumbee.Console.ProgressBarGlyphs.md)

### <a id="Jumbee_Console_ProgressBarGlyphs_Default"></a> Default

The default: the solid sub-cell band (<xref href="Jumbee.Console.ProgressBarFillMode.Solid" data-throw-if-not-resolved="false"></xref>).

```csharp
public static ProgressBarGlyphs Default { get; }
```

#### Property Value

 [ProgressBarGlyphs](Jumbee.Console.ProgressBarGlyphs.md)

### <a id="Jumbee_Console_ProgressBarGlyphs_Dots"></a> Dots

A braille-dot fill (<code>⣿</code>) on a low-dot track (<code>⣀</code>).

```csharp
public static ProgressBarGlyphs Dots { get; }
```

#### Property Value

 [ProgressBarGlyphs](Jumbee.Console.ProgressBarGlyphs.md)

### <a id="Jumbee_Console_ProgressBarGlyphs_Fill"></a> Fill

The glyph for a filled cell. Glyph mode only.

```csharp
public string Fill { get; init; }
```

#### Property Value

 string

### <a id="Jumbee_Console_ProgressBarGlyphs_Hatched"></a> Hatched

A diagonal-hatch fill (<code>▨</code>) on a light-shade track (<code>░</code>).

```csharp
public static ProgressBarGlyphs Hatched { get; }
```

#### Property Value

 [ProgressBarGlyphs](Jumbee.Console.ProgressBarGlyphs.md)

### <a id="Jumbee_Console_ProgressBarGlyphs_Line"></a> Line

A thin line bar: a heavy horizontal (<code>━</code>) fill on a light one (<code>─</code>).

```csharp
public static ProgressBarGlyphs Line { get; }
```

#### Property Value

 [ProgressBarGlyphs](Jumbee.Console.ProgressBarGlyphs.md)

### <a id="Jumbee_Console_ProgressBarGlyphs_Mode"></a> Mode

Which bar to render (a solid sub-cell band, or per-cell glyphs). Defaults to
    <xref href="Jumbee.Console.ProgressBarFillMode.Solid" data-throw-if-not-resolved="false"></xref> for a default-constructed value and the <xref href="Jumbee.Console.ProgressBarGlyphs.Solid" data-throw-if-not-resolved="false"></xref> preset.

```csharp
public ProgressBarFillMode Mode { get; init; }
```

#### Property Value

 [ProgressBarFillMode](Jumbee.Console.ProgressBarFillMode.md)

### <a id="Jumbee_Console_ProgressBarGlyphs_Segmented"></a> Segmented

A full-block fill (<code>█</code>) on a light-shade track (<code>░</code>) — reads as discrete segments once
    coloured.

```csharp
public static ProgressBarGlyphs Segmented { get; }
```

#### Property Value

 [ProgressBarGlyphs](Jumbee.Console.ProgressBarGlyphs.md)

### <a id="Jumbee_Console_ProgressBarGlyphs_Shaded"></a> Shaded

A dark-shade fill (<code>▓</code>) on a light-shade track (<code>░</code>).

```csharp
public static ProgressBarGlyphs Shaded { get; }
```

#### Property Value

 [ProgressBarGlyphs](Jumbee.Console.ProgressBarGlyphs.md)

### <a id="Jumbee_Console_ProgressBarGlyphs_Solid"></a> Solid

The solid band. The glyph fields are placeholders and unused by the band renderer, which draws its
    own eighth-block cells.

```csharp
public static ProgressBarGlyphs Solid { get; }
```

#### Property Value

 [ProgressBarGlyphs](Jumbee.Console.ProgressBarGlyphs.md)

### <a id="Jumbee_Console_ProgressBarGlyphs_Track"></a> Track

The glyph for an empty cell. Glyph mode only.

```csharp
public string Track { get; init; }
```

#### Property Value

 string

## Methods

### <a id="Jumbee_Console_ProgressBarGlyphs_Equals_Jumbee_Console_ProgressBarGlyphs_"></a> Equals\(ProgressBarGlyphs\)

Determines whether this <xref href="Jumbee.Console.ProgressBarGlyphs" data-throw-if-not-resolved="false"></xref> equals <code class="paramref">other</code>.

```csharp
public bool Equals(ProgressBarGlyphs other)
```

#### Parameters

`other` [ProgressBarGlyphs](Jumbee.Console.ProgressBarGlyphs.md)

#### Returns

 bool

### <a id="Jumbee_Console_ProgressBarGlyphs_Equals_System_Object_"></a> Equals\(object?\)

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

### <a id="Jumbee_Console_ProgressBarGlyphs_GetHashCode"></a> GetHashCode\(\)

Returns the hash code for this instance.

```csharp
public override int GetHashCode()
```

#### Returns

 int

A 32-bit signed integer that is the hash code for this instance.

## Operators

### <a id="Jumbee_Console_ProgressBarGlyphs_op_Equality_Jumbee_Console_ProgressBarGlyphs_Jumbee_Console_ProgressBarGlyphs_"></a> operator ==\(ProgressBarGlyphs, ProgressBarGlyphs\)

Equality operator.

```csharp
public static bool operator ==(ProgressBarGlyphs a, ProgressBarGlyphs b)
```

#### Parameters

`a` [ProgressBarGlyphs](Jumbee.Console.ProgressBarGlyphs.md)

`b` [ProgressBarGlyphs](Jumbee.Console.ProgressBarGlyphs.md)

#### Returns

 bool

### <a id="Jumbee_Console_ProgressBarGlyphs_op_Inequality_Jumbee_Console_ProgressBarGlyphs_Jumbee_Console_ProgressBarGlyphs_"></a> operator \!=\(ProgressBarGlyphs, ProgressBarGlyphs\)

Inequality operator.

```csharp
public static bool operator !=(ProgressBarGlyphs a, ProgressBarGlyphs b)
```

#### Parameters

`a` [ProgressBarGlyphs](Jumbee.Console.ProgressBarGlyphs.md)

`b` [ProgressBarGlyphs](Jumbee.Console.ProgressBarGlyphs.md)

#### Returns

 bool

