# <a id="Jumbee_Console_Style"></a> Struct Style

Namespace: [Jumbee.Console](Jumbee.Console.md)  
Assembly: Jumbee.Console.Styles.dll  

A text style — foreground/background colour and text decoration — wrapping a Spectre.Console style. Exposes the
named colour palette (<xref href="Jumbee.Console.Style.Red1" data-throw-if-not-resolved="false"></xref>, <xref href="Jumbee.Console.Style.Cyan1" data-throw-if-not-resolved="false"></xref>, …) and decoration presets (<xref href="Jumbee.Console.Style.Bold" data-throw-if-not-resolved="false"></xref>,
<xref href="Jumbee.Console.Style.Italic" data-throw-if-not-resolved="false"></xref>, …) as ready-made tokens; combine them with <code>|</code>.

```csharp
public readonly struct Style
```

## Constructors

### <a id="Jumbee_Console_Style__ctor_Spectre_Console_Style_"></a> Style\(Style\)

Wraps an existing Spectre.Console <xref href="Spectre.Console.Style" data-throw-if-not-resolved="false"></xref>.

```csharp
public Style(Style spectreConsoleStyle)
```

#### Parameters

`spectreConsoleStyle` Style

### <a id="Jumbee_Console_Style__ctor_System_String_"></a> Style\(string\)

Parses a style from a Spectre markup style string (e.g. <code>"bold red on blue"</code>).

```csharp
public Style(string style)
```

#### Parameters

`style` string

### <a id="Jumbee_Console_Style__ctor_Jumbee_Console_Color_Jumbee_Console_Color_"></a> Style\(Color, Color\)

A style with both a foreground and a background colour.

```csharp
public Style(Color foreground, Color background)
```

#### Parameters

`foreground` [Color](Jumbee.Console.Color.md)

The text colour.

`background` [Color](Jumbee.Console.Color.md)

The colour behind the text.

#### Remarks

The direct form for the common "text on a background" case — an inverse-video key cap is
<code>new Style(Color.Black, Color.White)</code>. A single colour converts implicitly instead
(<code>Style s = myColor;</code> sets the foreground and leaves the background at the terminal default).
Add a decoration by composing with <code>|</code>: <code>new Style(fg, bg) | Style.Bold</code>.

## Fields

### <a id="Jumbee_Console_Style_Aqua"></a> Aqua

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Aqua" colour (<xref href="Jumbee.Console.Color.Aqua" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Aqua
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Aquamarine1"></a> Aquamarine1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Aquamarine1" colour (<xref href="Jumbee.Console.Color.Aquamarine1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Aquamarine1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Aquamarine1_1"></a> Aquamarine1\_1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Aquamarine1_1" colour (<xref href="Jumbee.Console.Color.Aquamarine1_1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Aquamarine1_1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Aquamarine3"></a> Aquamarine3

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Aquamarine3" colour (<xref href="Jumbee.Console.Color.Aquamarine3" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Aquamarine3
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Black"></a> Black

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Black" colour (<xref href="Jumbee.Console.Color.Black" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Black
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Blue"></a> Blue

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Blue" colour (<xref href="Jumbee.Console.Color.Blue" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Blue
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Blue1"></a> Blue1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Blue1" colour (<xref href="Jumbee.Console.Color.Blue1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Blue1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Blue3"></a> Blue3

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Blue3" colour (<xref href="Jumbee.Console.Color.Blue3" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Blue3
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Blue3_1"></a> Blue3\_1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Blue3_1" colour (<xref href="Jumbee.Console.Color.Blue3_1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Blue3_1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_BlueViolet"></a> BlueViolet

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "BlueViolet" colour (<xref href="Jumbee.Console.Color.BlueViolet" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style BlueViolet
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Bold"></a> Bold

The <code>Bold</code> text decoration.

```csharp
public static readonly Style Bold
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_CadetBlue"></a> CadetBlue

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "CadetBlue" colour (<xref href="Jumbee.Console.Color.CadetBlue" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style CadetBlue
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_CadetBlue_1"></a> CadetBlue\_1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "CadetBlue_1" colour (<xref href="Jumbee.Console.Color.CadetBlue_1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style CadetBlue_1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Chartreuse1"></a> Chartreuse1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Chartreuse1" colour (<xref href="Jumbee.Console.Color.Chartreuse1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Chartreuse1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Chartreuse2"></a> Chartreuse2

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Chartreuse2" colour (<xref href="Jumbee.Console.Color.Chartreuse2" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Chartreuse2
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Chartreuse2_1"></a> Chartreuse2\_1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Chartreuse2_1" colour (<xref href="Jumbee.Console.Color.Chartreuse2_1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Chartreuse2_1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Chartreuse3"></a> Chartreuse3

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Chartreuse3" colour (<xref href="Jumbee.Console.Color.Chartreuse3" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Chartreuse3
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Chartreuse3_1"></a> Chartreuse3\_1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Chartreuse3_1" colour (<xref href="Jumbee.Console.Color.Chartreuse3_1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Chartreuse3_1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Chartreuse4"></a> Chartreuse4

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Chartreuse4" colour (<xref href="Jumbee.Console.Color.Chartreuse4" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Chartreuse4
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Conceal"></a> Conceal

The <code>Conceal</code> text decoration.

```csharp
public static readonly Style Conceal
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_CornflowerBlue"></a> CornflowerBlue

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "CornflowerBlue" colour (<xref href="Jumbee.Console.Color.CornflowerBlue" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style CornflowerBlue
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Cornsilk1"></a> Cornsilk1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Cornsilk1" colour (<xref href="Jumbee.Console.Color.Cornsilk1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Cornsilk1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Cyan1"></a> Cyan1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Cyan1" colour (<xref href="Jumbee.Console.Color.Cyan1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Cyan1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Cyan2"></a> Cyan2

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Cyan2" colour (<xref href="Jumbee.Console.Color.Cyan2" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Cyan2
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Cyan3"></a> Cyan3

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Cyan3" colour (<xref href="Jumbee.Console.Color.Cyan3" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Cyan3
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_DarkBlue"></a> DarkBlue

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "DarkBlue" colour (<xref href="Jumbee.Console.Color.DarkBlue" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style DarkBlue
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_DarkCyan"></a> DarkCyan

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "DarkCyan" colour (<xref href="Jumbee.Console.Color.DarkCyan" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style DarkCyan
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_DarkGoldenrod"></a> DarkGoldenrod

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "DarkGoldenrod" colour (<xref href="Jumbee.Console.Color.DarkGoldenrod" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style DarkGoldenrod
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_DarkGreen"></a> DarkGreen

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "DarkGreen" colour (<xref href="Jumbee.Console.Color.DarkGreen" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style DarkGreen
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_DarkKhaki"></a> DarkKhaki

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "DarkKhaki" colour (<xref href="Jumbee.Console.Color.DarkKhaki" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style DarkKhaki
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_DarkMagenta"></a> DarkMagenta

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "DarkMagenta" colour (<xref href="Jumbee.Console.Color.DarkMagenta" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style DarkMagenta
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_DarkMagenta_1"></a> DarkMagenta\_1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "DarkMagenta_1" colour (<xref href="Jumbee.Console.Color.DarkMagenta_1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style DarkMagenta_1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_DarkOliveGreen1"></a> DarkOliveGreen1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "DarkOliveGreen1" colour (<xref href="Jumbee.Console.Color.DarkOliveGreen1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style DarkOliveGreen1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_DarkOliveGreen1_1"></a> DarkOliveGreen1\_1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "DarkOliveGreen1_1" colour (<xref href="Jumbee.Console.Color.DarkOliveGreen1_1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style DarkOliveGreen1_1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_DarkOliveGreen2"></a> DarkOliveGreen2

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "DarkOliveGreen2" colour (<xref href="Jumbee.Console.Color.DarkOliveGreen2" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style DarkOliveGreen2
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_DarkOliveGreen3"></a> DarkOliveGreen3

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "DarkOliveGreen3" colour (<xref href="Jumbee.Console.Color.DarkOliveGreen3" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style DarkOliveGreen3
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_DarkOliveGreen3_1"></a> DarkOliveGreen3\_1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "DarkOliveGreen3_1" colour (<xref href="Jumbee.Console.Color.DarkOliveGreen3_1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style DarkOliveGreen3_1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_DarkOliveGreen3_2"></a> DarkOliveGreen3\_2

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "DarkOliveGreen3_2" colour (<xref href="Jumbee.Console.Color.DarkOliveGreen3_2" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style DarkOliveGreen3_2
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_DarkOrange"></a> DarkOrange

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "DarkOrange" colour (<xref href="Jumbee.Console.Color.DarkOrange" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style DarkOrange
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_DarkOrange3"></a> DarkOrange3

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "DarkOrange3" colour (<xref href="Jumbee.Console.Color.DarkOrange3" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style DarkOrange3
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_DarkOrange3_1"></a> DarkOrange3\_1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "DarkOrange3_1" colour (<xref href="Jumbee.Console.Color.DarkOrange3_1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style DarkOrange3_1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_DarkRed"></a> DarkRed

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "DarkRed" colour (<xref href="Jumbee.Console.Color.DarkRed" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style DarkRed
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_DarkRed_1"></a> DarkRed\_1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "DarkRed_1" colour (<xref href="Jumbee.Console.Color.DarkRed_1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style DarkRed_1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_DarkSeaGreen"></a> DarkSeaGreen

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "DarkSeaGreen" colour (<xref href="Jumbee.Console.Color.DarkSeaGreen" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style DarkSeaGreen
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_DarkSeaGreen1"></a> DarkSeaGreen1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "DarkSeaGreen1" colour (<xref href="Jumbee.Console.Color.DarkSeaGreen1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style DarkSeaGreen1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_DarkSeaGreen1_1"></a> DarkSeaGreen1\_1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "DarkSeaGreen1_1" colour (<xref href="Jumbee.Console.Color.DarkSeaGreen1_1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style DarkSeaGreen1_1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_DarkSeaGreen2"></a> DarkSeaGreen2

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "DarkSeaGreen2" colour (<xref href="Jumbee.Console.Color.DarkSeaGreen2" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style DarkSeaGreen2
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_DarkSeaGreen2_1"></a> DarkSeaGreen2\_1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "DarkSeaGreen2_1" colour (<xref href="Jumbee.Console.Color.DarkSeaGreen2_1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style DarkSeaGreen2_1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_DarkSeaGreen3"></a> DarkSeaGreen3

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "DarkSeaGreen3" colour (<xref href="Jumbee.Console.Color.DarkSeaGreen3" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style DarkSeaGreen3
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_DarkSeaGreen3_1"></a> DarkSeaGreen3\_1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "DarkSeaGreen3_1" colour (<xref href="Jumbee.Console.Color.DarkSeaGreen3_1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style DarkSeaGreen3_1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_DarkSeaGreen4"></a> DarkSeaGreen4

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "DarkSeaGreen4" colour (<xref href="Jumbee.Console.Color.DarkSeaGreen4" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style DarkSeaGreen4
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_DarkSeaGreen4_1"></a> DarkSeaGreen4\_1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "DarkSeaGreen4_1" colour (<xref href="Jumbee.Console.Color.DarkSeaGreen4_1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style DarkSeaGreen4_1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_DarkSlateGray1"></a> DarkSlateGray1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "DarkSlateGray1" colour (<xref href="Jumbee.Console.Color.DarkSlateGray1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style DarkSlateGray1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_DarkSlateGray2"></a> DarkSlateGray2

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "DarkSlateGray2" colour (<xref href="Jumbee.Console.Color.DarkSlateGray2" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style DarkSlateGray2
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_DarkSlateGray3"></a> DarkSlateGray3

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "DarkSlateGray3" colour (<xref href="Jumbee.Console.Color.DarkSlateGray3" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style DarkSlateGray3
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_DarkTurquoise"></a> DarkTurquoise

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "DarkTurquoise" colour (<xref href="Jumbee.Console.Color.DarkTurquoise" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style DarkTurquoise
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_DarkViolet"></a> DarkViolet

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "DarkViolet" colour (<xref href="Jumbee.Console.Color.DarkViolet" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style DarkViolet
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_DarkViolet_1"></a> DarkViolet\_1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "DarkViolet_1" colour (<xref href="Jumbee.Console.Color.DarkViolet_1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style DarkViolet_1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_DeepPink1"></a> DeepPink1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "DeepPink1" colour (<xref href="Jumbee.Console.Color.DeepPink1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style DeepPink1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_DeepPink1_1"></a> DeepPink1\_1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "DeepPink1_1" colour (<xref href="Jumbee.Console.Color.DeepPink1_1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style DeepPink1_1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_DeepPink2"></a> DeepPink2

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "DeepPink2" colour (<xref href="Jumbee.Console.Color.DeepPink2" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style DeepPink2
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_DeepPink3"></a> DeepPink3

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "DeepPink3" colour (<xref href="Jumbee.Console.Color.DeepPink3" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style DeepPink3
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_DeepPink3_1"></a> DeepPink3\_1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "DeepPink3_1" colour (<xref href="Jumbee.Console.Color.DeepPink3_1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style DeepPink3_1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_DeepPink4"></a> DeepPink4

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "DeepPink4" colour (<xref href="Jumbee.Console.Color.DeepPink4" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style DeepPink4
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_DeepPink4_1"></a> DeepPink4\_1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "DeepPink4_1" colour (<xref href="Jumbee.Console.Color.DeepPink4_1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style DeepPink4_1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_DeepPink4_2"></a> DeepPink4\_2

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "DeepPink4_2" colour (<xref href="Jumbee.Console.Color.DeepPink4_2" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style DeepPink4_2
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_DeepSkyBlue1"></a> DeepSkyBlue1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "DeepSkyBlue1" colour (<xref href="Jumbee.Console.Color.DeepSkyBlue1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style DeepSkyBlue1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_DeepSkyBlue2"></a> DeepSkyBlue2

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "DeepSkyBlue2" colour (<xref href="Jumbee.Console.Color.DeepSkyBlue2" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style DeepSkyBlue2
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_DeepSkyBlue3"></a> DeepSkyBlue3

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "DeepSkyBlue3" colour (<xref href="Jumbee.Console.Color.DeepSkyBlue3" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style DeepSkyBlue3
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_DeepSkyBlue3_1"></a> DeepSkyBlue3\_1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "DeepSkyBlue3_1" colour (<xref href="Jumbee.Console.Color.DeepSkyBlue3_1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style DeepSkyBlue3_1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_DeepSkyBlue4"></a> DeepSkyBlue4

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "DeepSkyBlue4" colour (<xref href="Jumbee.Console.Color.DeepSkyBlue4" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style DeepSkyBlue4
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_DeepSkyBlue4_1"></a> DeepSkyBlue4\_1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "DeepSkyBlue4_1" colour (<xref href="Jumbee.Console.Color.DeepSkyBlue4_1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style DeepSkyBlue4_1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_DeepSkyBlue4_2"></a> DeepSkyBlue4\_2

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "DeepSkyBlue4_2" colour (<xref href="Jumbee.Console.Color.DeepSkyBlue4_2" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style DeepSkyBlue4_2
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Dim"></a> Dim

The <code>Dim</code> text decoration.

```csharp
public static readonly Style Dim
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_DodgerBlue1"></a> DodgerBlue1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "DodgerBlue1" colour (<xref href="Jumbee.Console.Color.DodgerBlue1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style DodgerBlue1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_DodgerBlue2"></a> DodgerBlue2

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "DodgerBlue2" colour (<xref href="Jumbee.Console.Color.DodgerBlue2" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style DodgerBlue2
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_DodgerBlue3"></a> DodgerBlue3

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "DodgerBlue3" colour (<xref href="Jumbee.Console.Color.DodgerBlue3" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style DodgerBlue3
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Fuchsia"></a> Fuchsia

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Fuchsia" colour (<xref href="Jumbee.Console.Color.Fuchsia" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Fuchsia
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Gold1"></a> Gold1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Gold1" colour (<xref href="Jumbee.Console.Color.Gold1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Gold1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Gold3"></a> Gold3

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Gold3" colour (<xref href="Jumbee.Console.Color.Gold3" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Gold3
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Gold3_1"></a> Gold3\_1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Gold3_1" colour (<xref href="Jumbee.Console.Color.Gold3_1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Gold3_1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Green"></a> Green

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Green" colour (<xref href="Jumbee.Console.Color.Green" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Green
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Green1"></a> Green1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Green1" colour (<xref href="Jumbee.Console.Color.Green1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Green1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Green3"></a> Green3

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Green3" colour (<xref href="Jumbee.Console.Color.Green3" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Green3
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Green3_1"></a> Green3\_1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Green3_1" colour (<xref href="Jumbee.Console.Color.Green3_1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Green3_1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Green4"></a> Green4

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Green4" colour (<xref href="Jumbee.Console.Color.Green4" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Green4
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_GreenYellow"></a> GreenYellow

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "GreenYellow" colour (<xref href="Jumbee.Console.Color.GreenYellow" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style GreenYellow
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Grey"></a> Grey

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Grey" colour (<xref href="Jumbee.Console.Color.Grey" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Grey
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Grey0"></a> Grey0

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Grey0" colour (<xref href="Jumbee.Console.Color.Grey0" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Grey0
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Grey100"></a> Grey100

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Grey100" colour (<xref href="Jumbee.Console.Color.Grey100" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Grey100
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Grey11"></a> Grey11

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Grey11" colour (<xref href="Jumbee.Console.Color.Grey11" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Grey11
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Grey15"></a> Grey15

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Grey15" colour (<xref href="Jumbee.Console.Color.Grey15" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Grey15
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Grey19"></a> Grey19

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Grey19" colour (<xref href="Jumbee.Console.Color.Grey19" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Grey19
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Grey23"></a> Grey23

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Grey23" colour (<xref href="Jumbee.Console.Color.Grey23" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Grey23
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Grey27"></a> Grey27

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Grey27" colour (<xref href="Jumbee.Console.Color.Grey27" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Grey27
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Grey3"></a> Grey3

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Grey3" colour (<xref href="Jumbee.Console.Color.Grey3" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Grey3
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Grey30"></a> Grey30

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Grey30" colour (<xref href="Jumbee.Console.Color.Grey30" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Grey30
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Grey35"></a> Grey35

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Grey35" colour (<xref href="Jumbee.Console.Color.Grey35" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Grey35
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Grey37"></a> Grey37

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Grey37" colour (<xref href="Jumbee.Console.Color.Grey37" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Grey37
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Grey39"></a> Grey39

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Grey39" colour (<xref href="Jumbee.Console.Color.Grey39" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Grey39
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Grey42"></a> Grey42

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Grey42" colour (<xref href="Jumbee.Console.Color.Grey42" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Grey42
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Grey46"></a> Grey46

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Grey46" colour (<xref href="Jumbee.Console.Color.Grey46" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Grey46
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Grey50"></a> Grey50

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Grey50" colour (<xref href="Jumbee.Console.Color.Grey50" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Grey50
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Grey53"></a> Grey53

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Grey53" colour (<xref href="Jumbee.Console.Color.Grey53" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Grey53
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Grey54"></a> Grey54

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Grey54" colour (<xref href="Jumbee.Console.Color.Grey54" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Grey54
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Grey58"></a> Grey58

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Grey58" colour (<xref href="Jumbee.Console.Color.Grey58" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Grey58
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Grey62"></a> Grey62

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Grey62" colour (<xref href="Jumbee.Console.Color.Grey62" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Grey62
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Grey63"></a> Grey63

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Grey63" colour (<xref href="Jumbee.Console.Color.Grey63" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Grey63
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Grey66"></a> Grey66

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Grey66" colour (<xref href="Jumbee.Console.Color.Grey66" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Grey66
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Grey69"></a> Grey69

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Grey69" colour (<xref href="Jumbee.Console.Color.Grey69" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Grey69
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Grey7"></a> Grey7

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Grey7" colour (<xref href="Jumbee.Console.Color.Grey7" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Grey7
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Grey70"></a> Grey70

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Grey70" colour (<xref href="Jumbee.Console.Color.Grey70" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Grey70
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Grey74"></a> Grey74

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Grey74" colour (<xref href="Jumbee.Console.Color.Grey74" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Grey74
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Grey78"></a> Grey78

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Grey78" colour (<xref href="Jumbee.Console.Color.Grey78" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Grey78
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Grey82"></a> Grey82

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Grey82" colour (<xref href="Jumbee.Console.Color.Grey82" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Grey82
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Grey84"></a> Grey84

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Grey84" colour (<xref href="Jumbee.Console.Color.Grey84" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Grey84
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Grey85"></a> Grey85

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Grey85" colour (<xref href="Jumbee.Console.Color.Grey85" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Grey85
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Grey89"></a> Grey89

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Grey89" colour (<xref href="Jumbee.Console.Color.Grey89" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Grey89
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Grey93"></a> Grey93

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Grey93" colour (<xref href="Jumbee.Console.Color.Grey93" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Grey93
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Honeydew2"></a> Honeydew2

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Honeydew2" colour (<xref href="Jumbee.Console.Color.Honeydew2" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Honeydew2
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_HotPink"></a> HotPink

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "HotPink" colour (<xref href="Jumbee.Console.Color.HotPink" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style HotPink
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_HotPink2"></a> HotPink2

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "HotPink2" colour (<xref href="Jumbee.Console.Color.HotPink2" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style HotPink2
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_HotPink3"></a> HotPink3

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "HotPink3" colour (<xref href="Jumbee.Console.Color.HotPink3" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style HotPink3
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_HotPink3_1"></a> HotPink3\_1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "HotPink3_1" colour (<xref href="Jumbee.Console.Color.HotPink3_1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style HotPink3_1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_HotPink_1"></a> HotPink\_1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "HotPink_1" colour (<xref href="Jumbee.Console.Color.HotPink_1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style HotPink_1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_IndianRed"></a> IndianRed

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "IndianRed" colour (<xref href="Jumbee.Console.Color.IndianRed" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style IndianRed
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_IndianRed1"></a> IndianRed1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "IndianRed1" colour (<xref href="Jumbee.Console.Color.IndianRed1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style IndianRed1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_IndianRed1_1"></a> IndianRed1\_1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "IndianRed1_1" colour (<xref href="Jumbee.Console.Color.IndianRed1_1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style IndianRed1_1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_IndianRed_1"></a> IndianRed\_1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "IndianRed_1" colour (<xref href="Jumbee.Console.Color.IndianRed_1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style IndianRed_1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Invert"></a> Invert

The <code>Invert</code> text decoration.

```csharp
public static readonly Style Invert
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Italic"></a> Italic

The <code>Italic</code> text decoration.

```csharp
public static readonly Style Italic
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Khaki1"></a> Khaki1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Khaki1" colour (<xref href="Jumbee.Console.Color.Khaki1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Khaki1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Khaki3"></a> Khaki3

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Khaki3" colour (<xref href="Jumbee.Console.Color.Khaki3" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Khaki3
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_LightCoral"></a> LightCoral

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "LightCoral" colour (<xref href="Jumbee.Console.Color.LightCoral" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style LightCoral
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_LightCyan1"></a> LightCyan1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "LightCyan1" colour (<xref href="Jumbee.Console.Color.LightCyan1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style LightCyan1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_LightCyan3"></a> LightCyan3

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "LightCyan3" colour (<xref href="Jumbee.Console.Color.LightCyan3" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style LightCyan3
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_LightGoldenrod1"></a> LightGoldenrod1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "LightGoldenrod1" colour (<xref href="Jumbee.Console.Color.LightGoldenrod1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style LightGoldenrod1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_LightGoldenrod2"></a> LightGoldenrod2

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "LightGoldenrod2" colour (<xref href="Jumbee.Console.Color.LightGoldenrod2" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style LightGoldenrod2
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_LightGoldenrod2_1"></a> LightGoldenrod2\_1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "LightGoldenrod2_1" colour (<xref href="Jumbee.Console.Color.LightGoldenrod2_1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style LightGoldenrod2_1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_LightGoldenrod2_2"></a> LightGoldenrod2\_2

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "LightGoldenrod2_2" colour (<xref href="Jumbee.Console.Color.LightGoldenrod2_2" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style LightGoldenrod2_2
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_LightGoldenrod3"></a> LightGoldenrod3

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "LightGoldenrod3" colour (<xref href="Jumbee.Console.Color.LightGoldenrod3" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style LightGoldenrod3
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_LightGreen"></a> LightGreen

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "LightGreen" colour (<xref href="Jumbee.Console.Color.LightGreen" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style LightGreen
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_LightGreen_1"></a> LightGreen\_1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "LightGreen_1" colour (<xref href="Jumbee.Console.Color.LightGreen_1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style LightGreen_1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_LightPink1"></a> LightPink1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "LightPink1" colour (<xref href="Jumbee.Console.Color.LightPink1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style LightPink1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_LightPink3"></a> LightPink3

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "LightPink3" colour (<xref href="Jumbee.Console.Color.LightPink3" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style LightPink3
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_LightPink4"></a> LightPink4

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "LightPink4" colour (<xref href="Jumbee.Console.Color.LightPink4" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style LightPink4
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_LightSalmon1"></a> LightSalmon1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "LightSalmon1" colour (<xref href="Jumbee.Console.Color.LightSalmon1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style LightSalmon1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_LightSalmon3"></a> LightSalmon3

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "LightSalmon3" colour (<xref href="Jumbee.Console.Color.LightSalmon3" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style LightSalmon3
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_LightSalmon3_1"></a> LightSalmon3\_1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "LightSalmon3_1" colour (<xref href="Jumbee.Console.Color.LightSalmon3_1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style LightSalmon3_1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_LightSeaGreen"></a> LightSeaGreen

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "LightSeaGreen" colour (<xref href="Jumbee.Console.Color.LightSeaGreen" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style LightSeaGreen
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_LightSkyBlue1"></a> LightSkyBlue1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "LightSkyBlue1" colour (<xref href="Jumbee.Console.Color.LightSkyBlue1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style LightSkyBlue1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_LightSkyBlue3"></a> LightSkyBlue3

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "LightSkyBlue3" colour (<xref href="Jumbee.Console.Color.LightSkyBlue3" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style LightSkyBlue3
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_LightSkyBlue3_1"></a> LightSkyBlue3\_1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "LightSkyBlue3_1" colour (<xref href="Jumbee.Console.Color.LightSkyBlue3_1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style LightSkyBlue3_1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_LightSlateBlue"></a> LightSlateBlue

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "LightSlateBlue" colour (<xref href="Jumbee.Console.Color.LightSlateBlue" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style LightSlateBlue
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_LightSlateGrey"></a> LightSlateGrey

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "LightSlateGrey" colour (<xref href="Jumbee.Console.Color.LightSlateGrey" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style LightSlateGrey
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_LightSteelBlue"></a> LightSteelBlue

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "LightSteelBlue" colour (<xref href="Jumbee.Console.Color.LightSteelBlue" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style LightSteelBlue
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_LightSteelBlue1"></a> LightSteelBlue1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "LightSteelBlue1" colour (<xref href="Jumbee.Console.Color.LightSteelBlue1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style LightSteelBlue1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_LightSteelBlue3"></a> LightSteelBlue3

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "LightSteelBlue3" colour (<xref href="Jumbee.Console.Color.LightSteelBlue3" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style LightSteelBlue3
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_LightYellow3"></a> LightYellow3

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "LightYellow3" colour (<xref href="Jumbee.Console.Color.LightYellow3" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style LightYellow3
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Lime"></a> Lime

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Lime" colour (<xref href="Jumbee.Console.Color.Lime" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Lime
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Magenta1"></a> Magenta1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Magenta1" colour (<xref href="Jumbee.Console.Color.Magenta1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Magenta1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Magenta2"></a> Magenta2

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Magenta2" colour (<xref href="Jumbee.Console.Color.Magenta2" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Magenta2
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Magenta2_1"></a> Magenta2\_1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Magenta2_1" colour (<xref href="Jumbee.Console.Color.Magenta2_1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Magenta2_1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Magenta3"></a> Magenta3

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Magenta3" colour (<xref href="Jumbee.Console.Color.Magenta3" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Magenta3
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Magenta3_1"></a> Magenta3\_1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Magenta3_1" colour (<xref href="Jumbee.Console.Color.Magenta3_1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Magenta3_1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Magenta3_2"></a> Magenta3\_2

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Magenta3_2" colour (<xref href="Jumbee.Console.Color.Magenta3_2" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Magenta3_2
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Maroon"></a> Maroon

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Maroon" colour (<xref href="Jumbee.Console.Color.Maroon" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Maroon
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_MediumOrchid"></a> MediumOrchid

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "MediumOrchid" colour (<xref href="Jumbee.Console.Color.MediumOrchid" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style MediumOrchid
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_MediumOrchid1"></a> MediumOrchid1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "MediumOrchid1" colour (<xref href="Jumbee.Console.Color.MediumOrchid1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style MediumOrchid1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_MediumOrchid1_1"></a> MediumOrchid1\_1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "MediumOrchid1_1" colour (<xref href="Jumbee.Console.Color.MediumOrchid1_1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style MediumOrchid1_1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_MediumOrchid3"></a> MediumOrchid3

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "MediumOrchid3" colour (<xref href="Jumbee.Console.Color.MediumOrchid3" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style MediumOrchid3
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_MediumPurple"></a> MediumPurple

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "MediumPurple" colour (<xref href="Jumbee.Console.Color.MediumPurple" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style MediumPurple
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_MediumPurple1"></a> MediumPurple1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "MediumPurple1" colour (<xref href="Jumbee.Console.Color.MediumPurple1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style MediumPurple1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_MediumPurple2"></a> MediumPurple2

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "MediumPurple2" colour (<xref href="Jumbee.Console.Color.MediumPurple2" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style MediumPurple2
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_MediumPurple2_1"></a> MediumPurple2\_1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "MediumPurple2_1" colour (<xref href="Jumbee.Console.Color.MediumPurple2_1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style MediumPurple2_1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_MediumPurple3"></a> MediumPurple3

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "MediumPurple3" colour (<xref href="Jumbee.Console.Color.MediumPurple3" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style MediumPurple3
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_MediumPurple3_1"></a> MediumPurple3\_1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "MediumPurple3_1" colour (<xref href="Jumbee.Console.Color.MediumPurple3_1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style MediumPurple3_1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_MediumPurple4"></a> MediumPurple4

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "MediumPurple4" colour (<xref href="Jumbee.Console.Color.MediumPurple4" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style MediumPurple4
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_MediumSpringGreen"></a> MediumSpringGreen

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "MediumSpringGreen" colour (<xref href="Jumbee.Console.Color.MediumSpringGreen" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style MediumSpringGreen
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_MediumTurquoise"></a> MediumTurquoise

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "MediumTurquoise" colour (<xref href="Jumbee.Console.Color.MediumTurquoise" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style MediumTurquoise
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_MediumVioletRed"></a> MediumVioletRed

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "MediumVioletRed" colour (<xref href="Jumbee.Console.Color.MediumVioletRed" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style MediumVioletRed
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_MistyRose1"></a> MistyRose1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "MistyRose1" colour (<xref href="Jumbee.Console.Color.MistyRose1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style MistyRose1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_MistyRose3"></a> MistyRose3

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "MistyRose3" colour (<xref href="Jumbee.Console.Color.MistyRose3" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style MistyRose3
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_NavajoWhite1"></a> NavajoWhite1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "NavajoWhite1" colour (<xref href="Jumbee.Console.Color.NavajoWhite1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style NavajoWhite1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_NavajoWhite3"></a> NavajoWhite3

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "NavajoWhite3" colour (<xref href="Jumbee.Console.Color.NavajoWhite3" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style NavajoWhite3
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Navy"></a> Navy

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Navy" colour (<xref href="Jumbee.Console.Color.Navy" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Navy
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_NavyBlue"></a> NavyBlue

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "NavyBlue" colour (<xref href="Jumbee.Console.Color.NavyBlue" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style NavyBlue
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Olive"></a> Olive

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Olive" colour (<xref href="Jumbee.Console.Color.Olive" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Olive
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Orange1"></a> Orange1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Orange1" colour (<xref href="Jumbee.Console.Color.Orange1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Orange1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Orange3"></a> Orange3

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Orange3" colour (<xref href="Jumbee.Console.Color.Orange3" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Orange3
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Orange4"></a> Orange4

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Orange4" colour (<xref href="Jumbee.Console.Color.Orange4" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Orange4
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Orange4_1"></a> Orange4\_1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Orange4_1" colour (<xref href="Jumbee.Console.Color.Orange4_1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Orange4_1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_OrangeRed1"></a> OrangeRed1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "OrangeRed1" colour (<xref href="Jumbee.Console.Color.OrangeRed1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style OrangeRed1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Orchid"></a> Orchid

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Orchid" colour (<xref href="Jumbee.Console.Color.Orchid" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Orchid
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Orchid1"></a> Orchid1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Orchid1" colour (<xref href="Jumbee.Console.Color.Orchid1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Orchid1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Orchid2"></a> Orchid2

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Orchid2" colour (<xref href="Jumbee.Console.Color.Orchid2" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Orchid2
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_PaleGreen1"></a> PaleGreen1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "PaleGreen1" colour (<xref href="Jumbee.Console.Color.PaleGreen1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style PaleGreen1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_PaleGreen1_1"></a> PaleGreen1\_1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "PaleGreen1_1" colour (<xref href="Jumbee.Console.Color.PaleGreen1_1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style PaleGreen1_1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_PaleGreen3"></a> PaleGreen3

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "PaleGreen3" colour (<xref href="Jumbee.Console.Color.PaleGreen3" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style PaleGreen3
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_PaleGreen3_1"></a> PaleGreen3\_1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "PaleGreen3_1" colour (<xref href="Jumbee.Console.Color.PaleGreen3_1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style PaleGreen3_1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_PaleTurquoise1"></a> PaleTurquoise1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "PaleTurquoise1" colour (<xref href="Jumbee.Console.Color.PaleTurquoise1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style PaleTurquoise1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_PaleTurquoise4"></a> PaleTurquoise4

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "PaleTurquoise4" colour (<xref href="Jumbee.Console.Color.PaleTurquoise4" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style PaleTurquoise4
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_PaleVioletRed1"></a> PaleVioletRed1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "PaleVioletRed1" colour (<xref href="Jumbee.Console.Color.PaleVioletRed1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style PaleVioletRed1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Pink1"></a> Pink1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Pink1" colour (<xref href="Jumbee.Console.Color.Pink1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Pink1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Pink3"></a> Pink3

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Pink3" colour (<xref href="Jumbee.Console.Color.Pink3" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Pink3
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Plain"></a> Plain

A plain style with no colour or text decoration.

```csharp
public static readonly Style Plain
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Plum1"></a> Plum1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Plum1" colour (<xref href="Jumbee.Console.Color.Plum1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Plum1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Plum2"></a> Plum2

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Plum2" colour (<xref href="Jumbee.Console.Color.Plum2" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Plum2
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Plum3"></a> Plum3

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Plum3" colour (<xref href="Jumbee.Console.Color.Plum3" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Plum3
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Plum4"></a> Plum4

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Plum4" colour (<xref href="Jumbee.Console.Color.Plum4" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Plum4
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Purple"></a> Purple

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Purple" colour (<xref href="Jumbee.Console.Color.Purple" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Purple
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Purple3"></a> Purple3

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Purple3" colour (<xref href="Jumbee.Console.Color.Purple3" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Purple3
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Purple4"></a> Purple4

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Purple4" colour (<xref href="Jumbee.Console.Color.Purple4" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Purple4
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Purple4_1"></a> Purple4\_1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Purple4_1" colour (<xref href="Jumbee.Console.Color.Purple4_1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Purple4_1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Purple_1"></a> Purple\_1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Purple_1" colour (<xref href="Jumbee.Console.Color.Purple_1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Purple_1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Purple_2"></a> Purple\_2

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Purple_2" colour (<xref href="Jumbee.Console.Color.Purple_2" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Purple_2
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_RapidBlink"></a> RapidBlink

The <code>RapidBlink</code> text decoration.

```csharp
public static readonly Style RapidBlink
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Red"></a> Red

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Red" colour (<xref href="Jumbee.Console.Color.Red" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Red
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Red1"></a> Red1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Red1" colour (<xref href="Jumbee.Console.Color.Red1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Red1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Red3"></a> Red3

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Red3" colour (<xref href="Jumbee.Console.Color.Red3" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Red3
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Red3_1"></a> Red3\_1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Red3_1" colour (<xref href="Jumbee.Console.Color.Red3_1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Red3_1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_RosyBrown"></a> RosyBrown

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "RosyBrown" colour (<xref href="Jumbee.Console.Color.RosyBrown" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style RosyBrown
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_RoyalBlue1"></a> RoyalBlue1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "RoyalBlue1" colour (<xref href="Jumbee.Console.Color.RoyalBlue1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style RoyalBlue1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Salmon1"></a> Salmon1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Salmon1" colour (<xref href="Jumbee.Console.Color.Salmon1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Salmon1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_SandyBrown"></a> SandyBrown

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "SandyBrown" colour (<xref href="Jumbee.Console.Color.SandyBrown" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style SandyBrown
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_SeaGreen1"></a> SeaGreen1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "SeaGreen1" colour (<xref href="Jumbee.Console.Color.SeaGreen1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style SeaGreen1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_SeaGreen1_1"></a> SeaGreen1\_1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "SeaGreen1_1" colour (<xref href="Jumbee.Console.Color.SeaGreen1_1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style SeaGreen1_1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_SeaGreen2"></a> SeaGreen2

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "SeaGreen2" colour (<xref href="Jumbee.Console.Color.SeaGreen2" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style SeaGreen2
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_SeaGreen3"></a> SeaGreen3

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "SeaGreen3" colour (<xref href="Jumbee.Console.Color.SeaGreen3" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style SeaGreen3
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Silver"></a> Silver

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Silver" colour (<xref href="Jumbee.Console.Color.Silver" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Silver
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_SkyBlue1"></a> SkyBlue1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "SkyBlue1" colour (<xref href="Jumbee.Console.Color.SkyBlue1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style SkyBlue1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_SkyBlue2"></a> SkyBlue2

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "SkyBlue2" colour (<xref href="Jumbee.Console.Color.SkyBlue2" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style SkyBlue2
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_SkyBlue3"></a> SkyBlue3

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "SkyBlue3" colour (<xref href="Jumbee.Console.Color.SkyBlue3" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style SkyBlue3
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_SlateBlue1"></a> SlateBlue1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "SlateBlue1" colour (<xref href="Jumbee.Console.Color.SlateBlue1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style SlateBlue1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_SlateBlue3"></a> SlateBlue3

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "SlateBlue3" colour (<xref href="Jumbee.Console.Color.SlateBlue3" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style SlateBlue3
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_SlateBlue3_1"></a> SlateBlue3\_1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "SlateBlue3_1" colour (<xref href="Jumbee.Console.Color.SlateBlue3_1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style SlateBlue3_1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_SlowBlink"></a> SlowBlink

The <code>SlowBlink</code> text decoration.

```csharp
public static readonly Style SlowBlink
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_SpectreConsoleStyle"></a> SpectreConsoleStyle

The wrapped Spectre.Console style this <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> delegates to.

```csharp
public readonly Style SpectreConsoleStyle
```

#### Field Value

 Style

### <a id="Jumbee_Console_Style_SpringGreen1"></a> SpringGreen1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "SpringGreen1" colour (<xref href="Jumbee.Console.Color.SpringGreen1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style SpringGreen1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_SpringGreen2"></a> SpringGreen2

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "SpringGreen2" colour (<xref href="Jumbee.Console.Color.SpringGreen2" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style SpringGreen2
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_SpringGreen2_1"></a> SpringGreen2\_1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "SpringGreen2_1" colour (<xref href="Jumbee.Console.Color.SpringGreen2_1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style SpringGreen2_1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_SpringGreen3"></a> SpringGreen3

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "SpringGreen3" colour (<xref href="Jumbee.Console.Color.SpringGreen3" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style SpringGreen3
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_SpringGreen3_1"></a> SpringGreen3\_1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "SpringGreen3_1" colour (<xref href="Jumbee.Console.Color.SpringGreen3_1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style SpringGreen3_1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_SpringGreen4"></a> SpringGreen4

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "SpringGreen4" colour (<xref href="Jumbee.Console.Color.SpringGreen4" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style SpringGreen4
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_SteelBlue"></a> SteelBlue

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "SteelBlue" colour (<xref href="Jumbee.Console.Color.SteelBlue" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style SteelBlue
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_SteelBlue1"></a> SteelBlue1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "SteelBlue1" colour (<xref href="Jumbee.Console.Color.SteelBlue1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style SteelBlue1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_SteelBlue1_1"></a> SteelBlue1\_1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "SteelBlue1_1" colour (<xref href="Jumbee.Console.Color.SteelBlue1_1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style SteelBlue1_1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_SteelBlue3"></a> SteelBlue3

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "SteelBlue3" colour (<xref href="Jumbee.Console.Color.SteelBlue3" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style SteelBlue3
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Strikethrough"></a> Strikethrough

The <code>Strikethrough</code> text decoration.

```csharp
public static readonly Style Strikethrough
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Tan"></a> Tan

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Tan" colour (<xref href="Jumbee.Console.Color.Tan" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Tan
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Teal"></a> Teal

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Teal" colour (<xref href="Jumbee.Console.Color.Teal" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Teal
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Thistle1"></a> Thistle1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Thistle1" colour (<xref href="Jumbee.Console.Color.Thistle1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Thistle1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Thistle3"></a> Thistle3

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Thistle3" colour (<xref href="Jumbee.Console.Color.Thistle3" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Thistle3
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Turquoise2"></a> Turquoise2

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Turquoise2" colour (<xref href="Jumbee.Console.Color.Turquoise2" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Turquoise2
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Turquoise4"></a> Turquoise4

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Turquoise4" colour (<xref href="Jumbee.Console.Color.Turquoise4" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Turquoise4
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Underline"></a> Underline

The <code>Underline</code> text decoration.

```csharp
public static readonly Style Underline
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Violet"></a> Violet

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Violet" colour (<xref href="Jumbee.Console.Color.Violet" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Violet
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Wheat1"></a> Wheat1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Wheat1" colour (<xref href="Jumbee.Console.Color.Wheat1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Wheat1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Wheat4"></a> Wheat4

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Wheat4" colour (<xref href="Jumbee.Console.Color.Wheat4" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Wheat4
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_White"></a> White

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "White" colour (<xref href="Jumbee.Console.Color.White" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style White
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Yellow"></a> Yellow

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Yellow" colour (<xref href="Jumbee.Console.Color.Yellow" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Yellow
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Yellow1"></a> Yellow1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Yellow1" colour (<xref href="Jumbee.Console.Color.Yellow1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Yellow1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Yellow2"></a> Yellow2

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Yellow2" colour (<xref href="Jumbee.Console.Color.Yellow2" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Yellow2
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Yellow3"></a> Yellow3

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Yellow3" colour (<xref href="Jumbee.Console.Color.Yellow3" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Yellow3
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Yellow3_1"></a> Yellow3\_1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Yellow3_1" colour (<xref href="Jumbee.Console.Color.Yellow3_1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Yellow3_1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Yellow4"></a> Yellow4

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Yellow4" colour (<xref href="Jumbee.Console.Color.Yellow4" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Yellow4
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Yellow4_1"></a> Yellow4\_1

A foreground <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref> in the "Yellow4_1" colour (<xref href="Jumbee.Console.Color.Yellow4_1" data-throw-if-not-resolved="false"></xref>).

```csharp
public static readonly Style Yellow4_1
```

#### Field Value

 [Style](Jumbee.Console.Style.md)

## Properties

### <a id="Jumbee_Console_Style_BackgroundColor"></a> BackgroundColor

The background colour, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> when the style leaves it as the terminal default.

```csharp
public Color? BackgroundColor { get; }
```

#### Property Value

 [Color](Jumbee.Console.Color.md)?

### <a id="Jumbee_Console_Style_ForegroundColor"></a> ForegroundColor

The foreground colour, or <a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null">null</a> when the style leaves it as the terminal default.

```csharp
public Color? ForegroundColor { get; }
```

#### Property Value

 [Color](Jumbee.Console.Color.md)?

### <a id="Jumbee_Console_Style_Item_System_String_"></a> this\[string\]

Wraps <code class="paramref">text</code> in this style's markup, producing a Spectre <xref href="Spectre.Console.Markup" data-throw-if-not-resolved="false"></xref>
    renderable (the text is markup-escaped first).

```csharp
public Markup this[string text] { get; }
```

#### Property Value

 Markup

## Methods

### <a id="Jumbee_Console_Style_Bg_Jumbee_Console_Color_"></a> Bg\(Color\)

A style carrying only a background colour, for composing with a foreground via <code>|</code>
    (e.g. <code>Style.White | Style.Bg(color)</code>).

```csharp
public static Style Bg(Color background)
```

#### Parameters

`background` [Color](Jumbee.Console.Color.md)

#### Returns

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_Equals_Jumbee_Console_Style_"></a> Equals\(Style\)

Determines whether this style equals <code class="paramref">other</code> (compared by the wrapped Spectre style).

```csharp
public bool Equals(Style other)
```

#### Parameters

`other` [Style](Jumbee.Console.Style.md)

#### Returns

 bool

### <a id="Jumbee_Console_Style_Equals_System_Object_"></a> Equals\(object?\)

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

### <a id="Jumbee_Console_Style_EscapeMarkup_System_String_"></a> EscapeMarkup\(string\)

Escapes Spectre markup control characters in <code class="paramref">text</code> so it renders literally.

```csharp
public static string EscapeMarkup(string text)
```

#### Parameters

`text` string

#### Returns

 string

### <a id="Jumbee_Console_Style_GetHashCode"></a> GetHashCode\(\)

Returns the hash code for this instance.

```csharp
public override int GetHashCode()
```

#### Returns

 int

A 32-bit signed integer that is the hash code for this instance.

### <a id="Jumbee_Console_Style_ToMarkup"></a> ToMarkup\(\)

Returns the Spectre markup representation of this style (e.g. <code>"bold red"</code>).

```csharp
public string ToMarkup()
```

#### Returns

 string

## Operators

### <a id="Jumbee_Console_Style_op_BitwiseOr_Jumbee_Console_Style_Jumbee_Console_Style_"></a> operator |\(Style, Style\)

Combines two styles; properties set on the right operand override the left.

```csharp
public static Style operator |(Style a, Style b)
```

#### Parameters

`a` [Style](Jumbee.Console.Style.md)

`b` [Style](Jumbee.Console.Style.md)

#### Returns

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_op_Equality_Jumbee_Console_Style_Jumbee_Console_Style_"></a> operator ==\(Style, Style\)

Equality operator; see <xref href="Jumbee.Console.Style.Equals(Jumbee.Console.Style)" data-throw-if-not-resolved="false"></xref>.

```csharp
public static bool operator ==(Style a, Style b)
```

#### Parameters

`a` [Style](Jumbee.Console.Style.md)

`b` [Style](Jumbee.Console.Style.md)

#### Returns

 bool

### <a id="Jumbee_Console_Style_op_Implicit_Jumbee_Console_Style__Spectre_Console_Style"></a> implicit operator Style\(Style\)

Unwraps the underlying Spectre.Console <xref href="Spectre.Console.Style" data-throw-if-not-resolved="false"></xref>.

```csharp
public static implicit operator Style(Style style)
```

#### Parameters

`style` [Style](Jumbee.Console.Style.md)

#### Returns

 Style

### <a id="Jumbee_Console_Style_op_Implicit_Spectre_Console_Style__Jumbee_Console_Style"></a> implicit operator Style\(Style\)

Wraps a Spectre.Console <xref href="Spectre.Console.Style" data-throw-if-not-resolved="false"></xref> as a <xref href="Jumbee.Console.Style" data-throw-if-not-resolved="false"></xref>.

```csharp
public static implicit operator Style(Style spectreConsoleStyle)
```

#### Parameters

`spectreConsoleStyle` Style

#### Returns

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_op_Implicit_Jumbee_Console_Style__System_String"></a> implicit operator string\(Style\)

Converts the style to its Spectre markup string (see <xref href="Jumbee.Console.Style.ToMarkup" data-throw-if-not-resolved="false"></xref>).

```csharp
public static implicit operator string(Style style)
```

#### Parameters

`style` [Style](Jumbee.Console.Style.md)

#### Returns

 string

### <a id="Jumbee_Console_Style_op_Implicit_Jumbee_Console_Color__Jumbee_Console_Style"></a> implicit operator Style\(Color\)

Builds a foreground-only style from a <xref href="Jumbee.Console.Color" data-throw-if-not-resolved="false"></xref>.

```csharp
public static implicit operator Style(Color color)
```

#### Parameters

`color` [Color](Jumbee.Console.Color.md)

#### Returns

 [Style](Jumbee.Console.Style.md)

### <a id="Jumbee_Console_Style_op_Implicit_Jumbee_Console_Style__Jumbee_Console_Color"></a> implicit operator Color\(Style\)

Extracts the style's foreground <xref href="Jumbee.Console.Color" data-throw-if-not-resolved="false"></xref>.

```csharp
public static implicit operator Color(Style style)
```

#### Parameters

`style` [Style](Jumbee.Console.Style.md)

#### Returns

 [Color](Jumbee.Console.Color.md)

### <a id="Jumbee_Console_Style_op_Implicit_Jumbee_Console_Style__System_Drawing_Color"></a> implicit operator Color\(Style\)

Converts the style's foreground colour to a <xref href="System.Drawing.Color" data-throw-if-not-resolved="false"></xref>.

```csharp
public static implicit operator Color(Style style)
```

#### Parameters

`style` [Style](Jumbee.Console.Style.md)

#### Returns

 Color

### <a id="Jumbee_Console_Style_op_Inequality_Jumbee_Console_Style_Jumbee_Console_Style_"></a> operator \!=\(Style, Style\)

Inequality operator; see <xref href="Jumbee.Console.Style.Equals(Jumbee.Console.Style)" data-throw-if-not-resolved="false"></xref>.

```csharp
public static bool operator !=(Style a, Style b)
```

#### Parameters

`a` [Style](Jumbee.Console.Style.md)

`b` [Style](Jumbee.Console.Style.md)

#### Returns

 bool

