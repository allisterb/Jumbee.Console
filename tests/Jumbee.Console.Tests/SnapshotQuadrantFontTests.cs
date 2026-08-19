namespace Jumbee.Console.Tests;

using Jumbee.Console.Snapshot;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using Xunit;

/// <summary>
/// The sixteen quadrant blocks (U+2596–U+259F, plus ▀▄▌▐█ and the space) must rasterise as sixteen <em>distinct</em>
/// glyphs, each with its ink in the right corners.
/// </summary>
/// <remarks>
/// The 3D demo's quadrant antialiasing places a silhouette at 2×2 resolution inside one cell by picking one of these
/// — so a font without them does not degrade the picture, it replaces every edge cell with the same missing-glyph
/// box. That fails silently in every text assertion, since <c>ToText</c> captures the glyph and not the font. Same
/// class of bug as <see cref="SnapshotBrailleFontTests"/>, and the same shape of check: patterns that must differ.
/// </remarks>
public class SnapshotQuadrantFontTests
{
    // Ink per quadrant of the rendered cell. Absolute counts are a font's business; what has to hold is that the
    // quadrants a glyph fills carry decisively more than the ones it leaves empty.
    private static (int TL, int TR, int BL, int BR) Quadrants(char glyph)
    {
        var buffer = new ConsoleBuffer { Size = new ConsoleGUI.Space.Size(1, 1) };
        buffer.Write(new ConsoleGUI.Space.Position(0, 0), new ConsoleGUI.Data.Character(glyph));
        using var image = ConsoleSnapshot.ToImage(buffer, Options);
        var bg = Options.DefaultBackground.ToPixel<Rgba32>();
        int tl = 0, tr = 0, bl = 0, br = 0;
        image.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (var x = 0; x < row.Length; x++)
                {
                    if (row[x].Equals(bg)) continue;
                    var cx = x - Options.Padding;
                    var cy = y - Options.Padding;
                    if ((uint)cx >= (uint)Options.CellWidth || (uint)cy >= (uint)Options.CellHeight) continue;
                    var left = cx * 2 < Options.CellWidth;
                    var top = cy * 2 < Options.CellHeight;
                    if (top && left) tl++;
                    else if (top) tr++;
                    else if (left) bl++;
                    else br++;
                }
            }
        });
        return (tl, tr, bl, br);
    }

    // Which quadrants each glyph fills, in TL TR BL BR order — the bit order HalfBlockSurface.QuadrantGlyph uses.
    public static TheoryData<char, bool, bool, bool, bool> Patterns() => new()
    {
        { '▘', true,  false, false, false },
        { '▝', false, true,  false, false },
        { '▖', false, false, true,  false },
        { '▗', false, false, false, true  },
        { '▀', true,  true,  false, false },
        { '▄', false, false, true,  true  },
        { '▌', true,  false, true,  false },
        { '▐', false, true,  false, true  },
        { '▚', true,  false, false, true  },
        { '▞', false, true,  true,  false },
        { '▛', true,  true,  true,  false },
        { '▜', true,  true,  false, true  },
        { '▙', true,  false, true,  true  },
        { '▟', false, true,  true,  true  },
    };

    [Theory]
    [MemberData(nameof(Patterns))]
    public void AQuadrantGlyphInksTheQuadrantsItNames(char glyph, bool tl, bool tr, bool bl, bool br)
    {
        var q = Quadrants(glyph);
        int[] ink = [q.TL, q.TR, q.BL, q.BR];
        bool[] filled = [tl, tr, bl, br];

        var leastFilled = int.MaxValue;
        var mostEmpty = 0;
        for (var i = 0; i < 4; i++)
        {
            if (filled[i]) leastFilled = Math.Min(leastFilled, ink[i]);
            else mostEmpty = Math.Max(mostEmpty, ink[i]);
        }

        // A missing glyph draws the same box for all sixteen, so every quadrant carries ink and this ratio is ~1.
        // Measured worst case across the set is 1.87 (▄), so 1.5 sits between the working and the broken state.
        Assert.True(leastFilled > mostEmpty * 1.5,
            $"'{glyph}' (U+{(int)glyph:X4}) did not ink the quadrants it names — filled floor {leastFilled}, " +
            $"empty ceiling {mostEmpty}: TL={q.TL} TR={q.TR} BL={q.BL} BR={q.BR}");
    }

    [Fact]
    public void TheEmptyCellDrawsNothing()
    {
        var q = Quadrants(' ');
        Assert.Equal((0, 0, 0, 0), q);
    }

    [Fact]
    public void TheFullCellInksEveryQuadrant()
    {
        var q = Quadrants('█');
        Assert.True(Math.Min(Math.Min(q.TL, q.TR), Math.Min(q.BL, q.BR)) > 0,
            $"'█' left a quadrant blank: TL={q.TL} TR={q.TR} BL={q.BL} BR={q.BR}");
    }

    private static readonly SnapshotImageOptions Options = new();
}
