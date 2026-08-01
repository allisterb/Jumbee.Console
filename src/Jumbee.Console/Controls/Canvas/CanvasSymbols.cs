namespace Jumbee.Console.Drawing;

using System;

/// <summary>
/// Selects the glyph set (and thus the sub-cell resolution) a <see cref="Jumbee.Console.Canvas"/> draws its shapes
/// with. The default is <see cref="Braille"/>.
/// </summary>
/// <remarks>
/// Higher resolution packs more plotted points into each character cell for a smoother result.
/// <para>
/// Ratatui's <c>Sextant</c> and <c>Octant</c> markers are intentionally absent: their glyphs are supra-BMP characters
/// (U+1FBxx / U+1CCxx) that a single-<c>char</c> terminal cell cannot store. <see cref="Braille"/> already gives the
/// finest resolution and <see cref="HalfBlock"/> the only per-half colour.
/// </para>
/// </remarks>
public enum CanvasMarker
{
    /// <summary>A single <c>•</c> per point (1×1 resolution).</summary>
    Dot,
    /// <summary>A solid full block <c>█</c> per point (1×1). Also paints the cell background, so a later layer can
    /// overlay a foreground glyph on top.</summary>
    Block,
    /// <summary>A half-height bar <c>▄</c> per point (1×1).</summary>
    Bar,
    /// <summary>Braille dots — 2×4 sub-cells per character (8 points/cell), the smoothest. The default.</summary>
    /// <remarks>Needs a font with Braille coverage (U+2800–U+28FF). Terminals generally have it; a <b>PNG snapshot</b>
    /// may not — the snapshot renderer's default font is Consolas, which has no Braille glyphs, so a chart can come out
    /// as empty boxes in the image while looking perfect in the terminal and in a text snapshot. If that happens, set
    /// <c>SnapshotImageOptions.FontFamily</c> to <c>"Cascadia Mono"</c> or <c>"DejaVu Sans Mono"</c>.</remarks>
    Braille,
    /// <summary>Vertical half blocks — 1×2 sub-cells per character, with independent colour for the upper and lower
    /// half of each cell (the only marker that colours sub-cells individually).</summary>
    HalfBlock,
    /// <summary>Quadrant blocks — 2×2 sub-cells per character (4 points/cell).</summary>
    Quadrant,
    /// <summary>A caller-supplied character per point (1×1); see <see cref="Jumbee.Console.Canvas.CustomMarker"/>.</summary>
    Custom,
}

/// <summary>Glyph tables and single-glyph constants used by the canvas grids.</summary>
internal static class CanvasSymbols
{
    /// <summary>The dot marker glyph.</summary>
    public const char Dot = '•';
    /// <summary>The full-block glyph (block marker and full half-block cell).</summary>
    public const char Block = '█';
    /// <summary>The half-bar glyph (bar marker and lower half-block cell).</summary>
    public const char Bar = '▄';
    /// <summary>Upper half-block glyph.</summary>
    public const char HalfUpper = '▀';
    /// <summary>Lower half-block glyph.</summary>
    public const char HalfLower = '▄';
    /// <summary>Full-block glyph used when both halves of a half-block cell share a colour.</summary>
    public const char HalfFull = '█';

    /// <summary>
    /// Maps a 2×2 row-major bit pattern (bit = col + 2·row) to a quadrant block glyph. Indices 0..15; verbatim from
    /// ratatui's <c>QUADRANTS</c> table. All entries are BMP (U+2580 block).
    /// </summary>
    public static readonly char[] Quadrants =
    [
        ' ', '▘', '▝', '▀', '▖', '▌', '▞', '▛', '▗', '▚', '▐', '▜', '▄', '▙', '▟', '█',
    ];

    /// <summary>
    /// Maps a 2×4 row-major bit pattern (bit = col + 2·row) to a Unicode braille glyph. Indices 0..255. Generated
    /// rather than transcribed: the braille block (U+2800) is defined so that each dot toggles a fixed bit, so the
    /// row-major pattern is remapped onto the Unicode dot bits. Verified against ratatui's <c>BRAILLE</c> table by the
    /// ported golden geometry tests.
    /// </summary>
    public static readonly char[] Braille = BuildBraille();

    // Braille dot layout (Unicode):  1 4 / 2 5 / 3 6 / 7 8, with dot n at bit value below. The canvas uses a
    // row-major bit order (bit = col + 2·row); this maps each row-major bit to the Unicode dot bit for its cell.
    private static char[] BuildBraille()
    {
        // row-major bit index -> Unicode braille dot bit:
        // 0:(c0,r0)=dot1(0x01) 1:(c1,r0)=dot4(0x08) 2:(c0,r1)=dot2(0x02) 3:(c1,r1)=dot5(0x10)
        // 4:(c0,r2)=dot3(0x04) 5:(c1,r2)=dot6(0x20) 6:(c0,r3)=dot7(0x40) 7:(c1,r3)=dot8(0x80)
        ReadOnlySpan<int> dotBit = [0x01, 0x08, 0x02, 0x10, 0x04, 0x20, 0x40, 0x80];
        var table = new char[256];
        for (int pattern = 0; pattern < 256; pattern++)
        {
            int code = 0x2800;
            for (int bit = 0; bit < 8; bit++)
                if ((pattern & (1 << bit)) != 0)
                    code |= dotBit[bit];
            table[pattern] = (char)code;
        }
        return table;
    }
}
