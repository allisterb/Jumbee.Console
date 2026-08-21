using Jumbee.Console;
using Jumbee.Console.Drawing;
using Jumbee.Console.Snapshot;

/// <summary>
/// Renders the same solid fill and hard edge through each sub-cell marker, to see what the glyphs actually LOOK
/// like rather than how finely they can be addressed.
/// </summary>
/// <remarks>
/// <para>
/// The grid ladder in <see cref="Surfaces"/> scores addressability: how finely two colours can be placed inside a
/// cell. It silently assumes a sub-pixel that <b>fills</b> its share of the cell, which is true of the block
/// glyphs and false of braille — a braille dot is a small round mark with gaps around it. That difference does not
/// show up in an error metric at all, and it is the whole question for a renderer drawing solid surfaces.
/// </para>
/// <para>
/// So this draws no error numbers. It saves a PNG, because the answer is "what does it look like" and the only
/// honest instrument for that is looking. Cascadia Mono: braille needs U+2800-U+28FF, which the snapshot
/// renderer's default font does not carry.
/// </para>
/// </remarks>
internal static class BrailleProbe
{
    public static int Run(string outDir)
    {
        Directory.CreateDirectory(outDir);
        var options = new SnapshotImageOptions { FontFamily = "Cascadia Mono" };

        foreach (var (marker, name, cw, ch) in new[]
                 {
                     (CanvasMarker.HalfBlock, "halfblock", 1, 2),
                     (CanvasMarker.Quadrant, "quadrant", 2, 2),
                     (CanvasMarker.Braille, "braille", 2, 4),
                 })
        {
            const int Cols = 48, Rows = 16;
            var canvas = new Canvas { Marker = marker };
            // Sub-pixel coordinates, so the shapes below are specified in the grid's own units and each marker gets
            // the same PICTURE rather than the same number of dots.
            var w = Cols * cw;
            var h = Rows * ch;
            canvas.XBounds = (0, w);
            canvas.YBounds = (0, h);

            // Left half solid, right half a 50% checker -- a flat surface and a dithered one, which is the pair a
            // textured raycast view is mostly made of.
            var solid = new List<(double, double)>();
            var checker = new List<(double, double)>();
            for (var y = 0; y < h; y++)
                for (var x = 0; x < w / 2; x++)
                {
                    solid.Add((x, y));
                    if (((x + y) & 1) == 0) checker.Add((x + (w / 2), y));
                }

            canvas.Add(new Points(solid, new Color(220, 60, 50)));
            canvas.Add(new Points(checker, new Color(220, 60, 50)));

            var buffer = ConsoleSnapshot.Render(canvas, Cols, Rows);
            var path = Path.Combine(outDir, $"fill-{name}.png");
            ConsoleSnapshot.SavePng(buffer, path, options);
            Console.WriteLine($"  {name,-10} {cw}x{ch} per cell, {w}x{h} sub-pixels -> {path}");
        }

        return 0;
    }
}
