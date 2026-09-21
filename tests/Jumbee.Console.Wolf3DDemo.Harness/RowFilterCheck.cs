using Jumbee.Console.Wolf3DDemo;

using Wolfenshine.Rendering;

/// <summary>
/// Scores the two <see cref="RowFilter"/> strategies against the framebuffer rows they stand for.
/// </summary>
/// <remarks>
/// <para>
/// Under <see cref="SurfaceMode.Quadrant"/> the framebuffer is rendered twice as tall as the surface, so every
/// surface sub-pixel stands for <b>two</b> framebuffer rows. <see cref="RowFilter.Nearest"/> emits the first and
/// discards the second; <see cref="RowFilter.Box"/> emits their average. Ground truth is those two rows, so each
/// strategy is charged for exactly the detail it fails to carry — nearest scores zero on the row it kept and pays
/// the full difference on the row it dropped.
/// </para>
/// <para>
/// Cost is reported two ways because the demo's model has two parts: distinct colours (does the filter leave the
/// palette, and how far), and sample-to-sample changes in scan order, which is what colour <em>runs</em> — and so
/// ANSI bytes — actually track. Real end-to-end bytes come from the <c>perf</c> mode over the assembled shell; this
/// isolates the blit.
/// </para>
/// <para>
/// The quantiser is swept because it is the whole question: it runs immediately after the filter, so a colour the
/// average invented between palette entries may be snapped straight back onto the ramp. If that is what happens,
/// box costs nothing in runs and the reconstruction is free.
/// </para>
/// </remarks>
internal static class RowFilterCheck
{
    public static int Run(Wolf3DScene scene, int cells, int cellRows)
    {
        // The dimensions Wolf3DRenderer itself uses under quadrant sampling: two sub-pixel columns per cell, two
        // sub-pixel rows per cell, and a framebuffer rendered SamplesPerColumn (2) times taller again.
        var w = cells * 2;
        var h = cellRows * 2;
        var bufferHeight = h * 2;
        var columns = new WallColumn[w];
        var pixels = new byte[w * bufferHeight * 4];

        Console.WriteLine($"\nGround truth: the {w}x{bufferHeight} framebuffer. Surface is {w}x{h}, so each sub-pixel");
        Console.WriteLine("stands for 2 framebuffer rows. RMS error is per channel over both of them (0-255).\n");
        Console.WriteLine("  quantize  filter     RMS err   vs nearest   colours   run breaks   vs nearest");

        foreach (var levels in new[] { 0, 6, 8, 10, 12, 16 })
        {
            var results = new Dictionary<RowFilter, (double SumSq, long Samples, double Colors, long Breaks, int Frames)>();

            foreach (var filter in new[] { RowFilter.Nearest, RowFilter.Box })
            {
                double sumSq = 0, colors = 0;
                long samples = 0, breaks = 0;
                var frames = 0;

                for (var level = 0; level < Math.Min(6, scene.Levels.Count); level++)
                {
                    scene.LoadLevel(level);
                    for (var step = 0; step < 4; step++)
                    {
                        scene.Turn(step * 0.9);
                        var camera = scene.GetCamera(0.66);
                        Raycaster.Cast(scene.Map, scene.Doors, camera, columns);
                        SoftwareRaycastRenderer.Render(columns, bufferHeight, bufferHeight, pixels,
                            scene.WallTextures, scene.Palette);
                        frames++;

                        var (sq, n, distinct, br) = Score(pixels, w, h, levels, filter);
                        sumSq += sq;
                        samples += n;
                        colors += distinct;
                        breaks += br;
                    }
                }

                results[filter] = (sumSq, samples, colors, breaks, frames);
            }

            var near = results[RowFilter.Nearest];
            var nearRms = Math.Sqrt(near.SumSq / (near.Samples * 3.0));
            foreach (var filter in new[] { RowFilter.Nearest, RowFilter.Box })
            {
                var (sq, n, colorSum, br, frames) = results[filter];
                var rms = Math.Sqrt(sq / (n * 3.0));
                Console.WriteLine(
                    $"  {(levels == 0 ? "off " : levels + "/ch"),-8}  {filter,-8}  {rms,9:F2}   {rms / nearRms,9:P0}   " +
                    $"{colorSum / frames,7:N0}   {br / (double)frames,10:N0}   {br / (double)near.Breaks,9:P0}");
            }

            Console.WriteLine();
        }

        return 0;
    }

    // One frame, one strategy. Reduces the framebuffer the way Wolf3DRenderer.Blit does -- filter first, quantiser
    // second, which is the ordering the whole experiment is about -- and charges the result against both of the
    // framebuffer rows it replaced.
    private static (double SumSq, long Samples, int Colors, long Breaks) Score(
        byte[] pixels, int w, int h, int levels, RowFilter filter)
    {
        var quantize = levels > 1;
        var step = quantize ? 255.0 / (levels - 1) : 0.0;
        var distinct = new HashSet<int>();
        double sumSq = 0;
        long breaks = 0;
        var previous = -1;

        for (var y = 0; y < h; y++)
        {
            for (var x = 0; x < w; x++)
            {
                var o = (((y * 2) * w) + x) * 4;
                var p = o + (w * 4);
                int r = pixels[o], g = pixels[o + 1], b = pixels[o + 2];
                if (filter == RowFilter.Box)
                {
                    r = (r + pixels[p]) / 2;
                    g = (g + pixels[p + 1]) / 2;
                    b = (b + pixels[p + 2]) / 2;
                }

                if (quantize)
                {
                    r = (byte)(Math.Round(r / step) * step);
                    g = (byte)(Math.Round(g / step) * step);
                    b = (byte)(Math.Round(b / step) * step);
                }

                // Charged against BOTH source rows: this one colour is all the surface will carry for the pair.
                sumSq += Sq(r - pixels[o]) + Sq(g - pixels[o + 1]) + Sq(b - pixels[o + 2]);
                sumSq += Sq(r - pixels[p]) + Sq(g - pixels[p + 1]) + Sq(b - pixels[p + 2]);

                var packed = (r << 16) | (g << 8) | b;
                distinct.Add(packed);
                if (packed != previous) breaks++;
                previous = packed;
            }
        }

        return (sumSq, (long)w * h * 2, distinct.Count, breaks);

        static double Sq(int d) => (double)d * d;
    }
}
