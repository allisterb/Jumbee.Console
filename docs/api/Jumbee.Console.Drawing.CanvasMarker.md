# <a id="Jumbee_Console_Drawing_CanvasMarker"></a> Enum CanvasMarker

Namespace: [Jumbee.Console.Drawing](Jumbee.Console.Drawing.md)  
Assembly: Jumbee.Console.dll  

Selects the glyph set (and thus the sub-cell resolution) a <xref href="Jumbee.Console.Canvas" data-throw-if-not-resolved="false"></xref> draws its shapes
with. The default is <xref href="Jumbee.Console.Drawing.CanvasMarker.Braille" data-throw-if-not-resolved="false"></xref>.

```csharp
public enum CanvasMarker
```

## Fields

`Dot = 0` 

A single <code>•</code> per point (1×1 resolution).



`Block = 1` 

A solid full block <code>█</code> per point (1×1). Also paints the cell background, so a later layer can
    overlay a foreground glyph on top.



`Bar = 2` 

A half-height bar <code>▄</code> per point (1×1).



`Braille = 3` 

Braille dots — 2×4 sub-cells per character (8 points/cell), the smoothest. The default.

Needs a font with Braille coverage (U+2800–U+28FF). Terminals generally have it; a <b>PNG snapshot</b>
    may not — the snapshot renderer's default font is Consolas, which has no Braille glyphs, so a chart can come out
    as empty boxes in the image while looking perfect in the terminal and in a text snapshot. If that happens, set
    <code>SnapshotImageOptions.FontFamily</code> to <code>"Cascadia Mono"</code> or <code>"DejaVu Sans Mono"</code>.

`HalfBlock = 4` 

Vertical half blocks — 1×2 sub-cells per character, with independent colour for the upper and lower
    half of each cell (the only marker that colours sub-cells individually).



`Quadrant = 5` 

Quadrant blocks — 2×2 sub-cells per character (4 points/cell).



`Custom = 6` 

A caller-supplied character per point (1×1); see <xref href="Jumbee.Console.Canvas.CustomMarker" data-throw-if-not-resolved="false"></xref>.



## Remarks

Higher resolution packs more plotted points into each character cell for a smoother result.

<p>
Ratatui's <code>Sextant</code> and <code>Octant</code> markers are intentionally absent: their glyphs are supra-BMP characters
(U+1FBxx / U+1CCxx) that a single-<code>char</code> terminal cell cannot store. <xref href="Jumbee.Console.Drawing.CanvasMarker.Braille" data-throw-if-not-resolved="false"></xref> already gives the
finest resolution and <xref href="Jumbee.Console.Drawing.CanvasMarker.HalfBlock" data-throw-if-not-resolved="false"></xref> the only per-half colour.
</p>

