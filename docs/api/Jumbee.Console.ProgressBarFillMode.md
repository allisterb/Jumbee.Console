# <a id="Jumbee_Console_ProgressBarFillMode"></a> Enum ProgressBarFillMode

Namespace: [Jumbee.Console](Jumbee.Console.md)  
Assembly: Jumbee.Console.Styles.dll  

How a <code>ProgressBar</code> draws its bar.

```csharp
public enum ProgressBarFillMode
```

## Fields

`Solid = 0` 

A solid colour band: the filled and empty portions are drawn as background-coloured runs, and the
    fill edge renders at <em>sub-cell</em> resolution using eighth-block glyphs so it advances smoothly. The
    glyph strings in <xref href="Jumbee.Console.ProgressBarGlyphs" data-throw-if-not-resolved="false"></xref> are ignored in this mode.



`Glyph = 1` 

Per-cell glyphs: each filled cell draws <xref href="Jumbee.Console.ProgressBarGlyphs.Fill" data-throw-if-not-resolved="false"></xref> and each empty cell
    <xref href="Jumbee.Console.ProgressBarGlyphs.Track" data-throw-if-not-resolved="false"></xref>, in the fill/track colours as <em>foreground</em>. Character-granular
    (no sub-cell edge) — the mode for a hatched, segmented or ASCII bar.



