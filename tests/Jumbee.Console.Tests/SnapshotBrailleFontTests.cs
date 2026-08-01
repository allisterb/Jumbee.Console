namespace Jumbee.Console.Tests;

using Jumbee.Console;
using Jumbee.Console.Drawing;
using Jumbee.Console.Snapshot;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using Xunit;

/// <summary>
/// PNG snapshots must draw Braille glyphs, not missing-glyph boxes.
/// </summary>
/// <remarks>
/// Three separate ports reported Braille charts rasterising as missing-glyph boxes in their PNGs, one shipping a
/// twelve-image review package whose charts could not be seen at all. It fails silently: text snapshots are
/// unaffected, so nothing in a passing test suite reveals it.
/// <para>
/// <b>Run these as part of the whole suite, not in isolation.</b> <c>Consolas</c> (the default
/// <see cref="SnapshotImageOptions.FontFamily"/>) has no Braille coverage, but the imaging stack often substitutes a
/// covering font by itself — so a lone run passes even with the bug present. The failure only appears once other
/// renders have happened in the same process, which is exactly the real case: a review package rendering a dozen
/// PNGs in one run. Verified both ways against the full suite: without the explicit fallback the first test fails
/// here, with it the suite is green.
/// </para>
/// <para>
/// The assertion is font-agnostic — it names no font, and relies only on Braille patterns with different dot counts
/// having to differ. Every missing-glyph box is identical, so a broken renderer gives both the same ink and fails.
/// </para>
/// </remarks>
public class SnapshotBrailleFontTests
{
    // Count pixels that aren't the default background — the "ink" of the drawn glyph.
    private static int Ink(Image<Rgba32> image)
    {
        var bg = new SnapshotImageOptions().DefaultBackground.ToPixel<Rgba32>();
        var n = 0;
        image.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (var x = 0; x < row.Length; x++) if (!row[x].Equals(bg)) n++;
            }
        });
        return n;
    }

    private static int InkOf(char glyph)
    {
        var buffer = new ConsoleBuffer { Size = new ConsoleGUI.Space.Size(1, 1) };
        buffer.Write(new ConsoleGUI.Space.Position(0, 0), new ConsoleGUI.Data.Character(glyph));
        using var image = ConsoleSnapshot.ToImage(buffer);
        return Ink(image);
    }

    [Fact]
    public void BrailleGlyphsRenderWithTheDefaultFont()
    {
        var oneDot = InkOf('⠁');    // ⠁ a single raised dot
        var allDots = InkOf('⣿');   // ⣿ all eight raised

        Assert.True(oneDot > 0, "the one-dot Braille cell drew nothing at all");

        // A full cell must carry visibly more ink than a single dot. If the font lacked Braille both would be the
        // same missing-glyph box and these would be equal.
        Assert.True(allDots > oneDot * 2,
            $"Braille appears to be rendering as missing-glyph boxes: ⠁ ink={oneDot}, ⣿ ink={allDots}");
    }

    [Fact]
    public void ABrailleChartRendersVisibleDots()
    {
        var canvas = new Canvas().WithYBounds(0, 100).WithXBounds(0, 7);
        double[] samples = [10, 40, 90, 30, 70, 20, 100, 50];
        for (var x = 0; x < samples.Length; x++)
            canvas.Add(new FilledLine(x, 0, x, samples[x], fillToY: 0, Jumbee.Console.Color.Magenta1));

        var buffer = ConsoleSnapshot.Render(canvas, 16, 8);
        using var image = ConsoleSnapshot.ToImage(buffer);

        // An empty canvas of the same size is the floor; a filled chart must carry substantially more ink.
        var empty = ConsoleSnapshot.Render(new Canvas().WithYBounds(0, 100).WithXBounds(0, 7), 16, 8);
        using var emptyImage = ConsoleSnapshot.ToImage(empty);

        Assert.True(Ink(image) > Ink(emptyImage) + 100,
            $"chart ink={Ink(image)} vs empty={Ink(emptyImage)} — the chart did not draw");
    }
}
