using Jumbee.Console.Wolf3DDemo;

using Wolfenshine.Rendering;

/// <summary>
/// Measures how well each sub-cell glyph grid reproduces the true image, and what it costs to send.
/// </summary>
/// <remarks>
/// <para>
/// Ground truth is Wolfenshine's own RGBA framebuffer rendered at <b>2 × 12 sub-pixels per cell</b> — the least
/// common grid that 1×2 (half block), 2×2 (quadrant), 2×3 (sextant) and 2×4 (octant) all divide into evenly. Every
/// strategy is scored against those same 24 samples, so each is charged for exactly the detail its own grid cannot
/// represent. That is why half block does not score zero despite being exact at its own sampling rate.
/// </para>
/// <para>
/// Every grid here gets the same <b>two colours per cell</b>; they differ only in how finely those two colours can
/// be placed. Region colours are assigned by splitting the regions about the cell's mean luma and averaging each
/// side — the same two-means the compositor does.
/// </para>
/// <para>
/// Error is RMS per channel over sub-pixels (0–255). Cost counts (fg,bg) PAIR changes — the two truecolor SGRs the
/// emitter must write, ~36 bytes — separately from the glyph, which every cell emits anyway at ~3 bytes.
/// </para>
/// </remarks>
internal static class Surfaces
{
    private const int Cols = 2;    // sub-pixel columns per cell
    private const int Rows = 12;   // sub-pixel rows per cell: divisible by 2, 3, 4 and 6
    private const int Samples = Cols * Rows;

    public static int Run(Wolf3DScene scene, int cells, int cellRows)
    {
        var w = cells * Cols;
        var h = cellRows * Rows;
        var columns = new WallColumn[w];
        var pixels = new byte[w * h * 4];

        Console.WriteLine($"\nGround truth: {w}x{h} sub-pixels ({Cols}x{Rows} per cell) over {cells}x{cellRows} cells.");
        Console.WriteLine("Bytes are MODELLED: 36 per (fg,bg) change + 3 per cell for the glyph.\n");
        Console.WriteLine("  quantize  grid                 RMS err   vs half   pairs/frame   est bytes   vs half");

        foreach (var levels in new[] { 0, 6 })
        {
            var totals = new Dictionary<string, (double SumSq, long Cells, long Pairs)>();
            var frames = 0;

            for (var level = 0; level < Math.Min(6, scene.Levels.Count); level++)
            {
                scene.LoadLevel(level);
                for (var step = 0; step < 4; step++)
                {
                    scene.Turn(step * 0.9);
                    var camera = scene.GetCamera(0.66);
                    Raycaster.Cast(scene.Map, scene.Doors, camera, columns);
                    SoftwareRaycastRenderer.Render(columns, h, h, pixels, scene.WallTextures, scene.Palette);
                    frames++;

                    foreach (var (name, score) in Score(pixels, w, cells, cellRows, levels))
                    {
                        var t = totals.GetValueOrDefault(name);
                        totals[name] = (t.SumSq + score.SumSq, t.Cells + score.Cells, t.Pairs + score.Pairs);
                    }
                }
            }

            const int PairBytes = 36, GlyphBytes = 3;
            var half = totals["half block  1x2"];
            var halfRms = Math.Sqrt(half.SumSq / (half.Cells * Samples * 3.0));
            var halfBytes = ((half.Pairs * PairBytes) + (half.Cells * GlyphBytes)) / (double)frames;

            foreach (var name in Grids.Select(g => g.Name))
            {
                var (sumSq, cellCount, pairs) = totals[name];
                var rms = Math.Sqrt(sumSq / (cellCount * Samples * 3.0));
                var bytes = ((pairs * PairBytes) + (cellCount * GlyphBytes)) / (double)frames;
                Console.WriteLine(
                    $"  {(levels == 0 ? "off " : levels + "/ch"),-8}  {name,-18}  {rms,8:F2}  {rms / halfRms,7:P0}   " +
                    $"{pairs / (double)frames,11:N0}  {bytes,10:N0}   {bytes / halfBytes,7:P0}");
            }

            Console.WriteLine();
        }

        // The grid ladder is capped by the two-colour ceiling, so the remaining error is overwhelmingly COLOUR
        // error. Sweep the quantiser at a fixed grid to see what that dial is actually worth.
        Console.WriteLine("  Quantiser sweep at the quadrant grid:\n");
        Console.WriteLine("  levels    RMS err   vs full   pairs/frame   est bytes   vs full");
        var sweep = new List<(int Levels, double Rms, double Bytes, double Pairs)>();
        foreach (var levels in new[] { 4, 5, 6, 8, 10, 12, 16, 0 })
        {
            double sumSq = 0;
            long cellCount = 0, pairs = 0;
            var frames = 0;
            for (var level = 0; level < Math.Min(6, scene.Levels.Count); level++)
            {
                scene.LoadLevel(level);
                for (var step = 0; step < 4; step++)
                {
                    scene.Turn(step * 0.9);
                    var camera = scene.GetCamera(0.66);
                    Raycaster.Cast(scene.Map, scene.Doors, camera, columns);
                    SoftwareRaycastRenderer.Render(columns, h, h, pixels, scene.WallTextures, scene.Palette);
                    frames++;
                    var s = Score(pixels, w, cells, cellRows, levels).First(x => x.Name == "quadrant    2x2");
                    sumSq += s.Score.SumSq;
                    cellCount += s.Score.Cells;
                    pairs += s.Score.Pairs;
                }
            }

            sweep.Add((levels, Math.Sqrt(sumSq / (cellCount * Samples * 3.0)),
                ((pairs * 36.0) + (cellCount * 3.0)) / frames, pairs / (double)frames));
        }

        var full = sweep[^1];
        foreach (var (levels, rms, bytes, pairsPerFrame) in sweep)
            Console.WriteLine(
                $"  {(levels == 0 ? "off" : levels.ToString()),-6}  {rms,9:F2}  {rms / full.Rms,7:P0}   " +
                $"{pairsPerFrame,11:N0}  {bytes,10:N0}   {bytes / full.Bytes,7:P0}");

        // An even RGB lattice ignores where the colours actually ARE. The source art is a fixed 256-entry VGA
        // palette, so a palette-aware quantiser -- k-means over those 256 colours down to K -- should beat a
        // lattice of comparable size. Same grid, same scenes, so the only variable is how the colours are chosen.
        Console.WriteLine("  Palette-snap, quadrant grid. 'all-256' clusters every VGA entry equally;");
        Console.WriteLine("  'observed' weights them by how much of the screen each actually covers.\n");
        Console.WriteLine("  palette         RMS err   vs full   pairs/frame   est bytes   vs full");
        foreach (var (k, observed) in new[]
                 {
                     (16, false), (32, false), (64, false), (128, false),
                     (16, true), (32, true), (64, true), (128, true),
                 })
        {
            var palette = observed ? ReduceObserved(scene, k, w, h, pixels, columns) : Reduce(scene, k);
            double sumSq = 0;
            long cellCount = 0, pairs = 0;
            var frames = 0;
            for (var level = 0; level < Math.Min(6, scene.Levels.Count); level++)
            {
                scene.LoadLevel(level);
                for (var step = 0; step < 4; step++)
                {
                    scene.Turn(step * 0.9);
                    var camera = scene.GetCamera(0.66);
                    Raycaster.Cast(scene.Map, scene.Doors, camera, columns);
                    SoftwareRaycastRenderer.Render(columns, h, h, pixels, scene.WallTextures, scene.Palette);
                    frames++;
                    var s = Score(pixels, w, cells, cellRows, 0, palette).First(x => x.Name == "quadrant    2x2");
                    sumSq += s.Score.SumSq;
                    cellCount += s.Score.Cells;
                    pairs += s.Score.Pairs;
                }
            }

            var rms = Math.Sqrt(sumSq / (cellCount * Samples * 3.0));
            var bytes = ((pairs * 36.0) + (cellCount * 3.0)) / frames;
            Console.WriteLine(
                $"  {(observed ? "observed " : "all-256  ") + k,-14}  {rms,9:F2}  {rms / full.Rms,7:P0}   " +
                $"{pairs / (double)frames,11:N0}  {bytes,10:N0}   {bytes / full.Bytes,7:P0}");
        }

        Console.WriteLine();
        return 0;
    }

    // k-means over the 256 VGA entries, seeded evenly through the palette so the result is deterministic. Each
    // entry counts once, whatever its share of the screen -- which is exactly this variant's weakness.
    private static double[][] Reduce(Wolf3DScene scene, int k)
    {
        var source = new double[256][];
        var weights = new double[256];
        for (var i = 0; i < 256; i++)
        {
            var c = scene.Palette.GetColor((byte)i);
            source[i] = [c.Red, c.Green, c.Blue];
            weights[i] = 1;
        }

        return KMeans(source, weights, k);
    }

    // The same clustering, but weighted by how much of the SCREEN each colour actually covers. Only a few dozen of
    // the 256 entries are ever visible, so the unweighted version above spends most of its budget on colours that
    // never appear -- which is why it loses to a plain RGB lattice at the same byte cost.
    private static double[][] ReduceObserved(Wolf3DScene scene, int k, int w, int h, byte[] pixels,
                                             WallColumn[] columns)
    {
        var histogram = new Dictionary<int, long>();
        for (var level = 0; level < Math.Min(6, scene.Levels.Count); level++)
        {
            scene.LoadLevel(level);
            for (var step = 0; step < 4; step++)
            {
                scene.Turn(step * 0.9);
                var camera = scene.GetCamera(0.66);
                Raycaster.Cast(scene.Map, scene.Doors, camera, columns);
                SoftwareRaycastRenderer.Render(columns, h, h, pixels, scene.WallTextures, scene.Palette);
                for (var o = 0; o < pixels.Length; o += 4)
                {
                    var key = (pixels[o] << 16) | (pixels[o + 1] << 8) | pixels[o + 2];
                    histogram[key] = histogram.GetValueOrDefault(key) + 1;
                }
            }
        }

        var source = new double[histogram.Count][];
        var weights = new double[histogram.Count];
        var n = 0;
        foreach (var (key, count) in histogram)
        {
            source[n] = [(key >> 16) & 0xFF, (key >> 8) & 0xFF, key & 0xFF];
            weights[n] = count;
            n++;
        }

        return KMeans(source, weights, Math.Min(k, source.Length));
    }

    private static double[][] KMeans(double[][] source, double[] weights, int k)
    {
        var centres = new double[k][];
        for (var i = 0; i < k; i++) centres[i] = [.. source[i * source.Length / k]];

        for (var pass = 0; pass < 24; pass++)
        {
            var sums = new double[k][];
            var counts = new double[k];
            for (var i = 0; i < k; i++) sums[i] = [0, 0, 0];
            for (var s = 0; s < source.Length; s++)
            {
                var nearest = Nearest(source[s], centres);
                for (var c = 0; c < 3; c++) sums[nearest][c] += source[s][c] * weights[s];
                counts[nearest] += weights[s];
            }

            for (var i = 0; i < k; i++)
                if (counts[i] > 0)
                    for (var c = 0; c < 3; c++) centres[i][c] = sums[i][c] / counts[i];
        }

        return centres;
    }

    private static int Nearest(double[] colour, double[][] palette)
    {
        var best = 0;
        var bestDistance = double.MaxValue;
        for (var i = 0; i < palette.Length; i++)
        {
            var d = 0.0;
            for (var c = 0; c < 3; c++)
            {
                var delta = palette[i][c] - colour[c];
                d += delta * delta;
            }

            if (d < bestDistance) { bestDistance = d; best = i; }
        }

        return best;
    }

    // Each grid is (name, sub-pixel columns, sub-pixel rows) within a cell. One region per grid square; every
    // region is uniform, and the regions share the cell's two colours.
    private static readonly (string Name, int Cx, int Cy)[] Grids =
    [
        ("solid       1x1", 1, 1),
        ("half block  1x2", 1, 2),
        ("quadrant    2x2", 2, 2),
        ("sextant     2x3", 2, 3),
        ("octant      2x4", 2, 4),
        ("2x6 (max)      ", 2, 6),
    ];

    private static IEnumerable<(string Name, (double SumSq, long Cells, long Pairs) Score)> Score(
        byte[] pixels, int w, int cells, int cellRows, int levels, double[][]? palette = null)
    {
        var results = new Dictionary<string, (double SumSq, long Cells, long Pairs)>();
        var previous = new Dictionary<string, long>();
        var sample = new double[Samples][];

        for (var cy = 0; cy < cellRows; cy++)
        {
            for (var cx = 0; cx < cells; cx++)
            {
                for (var sy = 0; sy < Rows; sy++)
                    for (var sx = 0; sx < Cols; sx++)
                        sample[(sy * Cols) + sx] = At(pixels, w, (cx * Cols) + sx, (cy * Rows) + sy);

                foreach (var (name, gx, gy) in Grids)
                {
                    var (err, pair) = Evaluate(sample, gx, gy, levels, palette);
                    var t = results.GetValueOrDefault(name);
                    var changed = !previous.TryGetValue(name, out var prev) || prev != pair;
                    previous[name] = pair;
                    results[name] = (t.SumSq + err, t.Cells + 1, t.Pairs + (changed ? 1 : 0));
                }
            }
        }

        return results.Select(kv => (kv.Key, kv.Value));
    }

    // Splits a cell's grid regions between two colours and returns the squared error plus the colour-pair key.
    private static (double Error, long Pair) Evaluate(double[][] sample, int gx, int gy, int levels, double[][]? palette)
    {
        var regions = gx * gy;
        var perX = Cols / gx;
        var perY = Rows / gy;

        // Region means.
        var means = new double[regions][];
        for (var r = 0; r < regions; r++) means[r] = [0, 0, 0];
        for (var sy = 0; sy < Rows; sy++)
            for (var sx = 0; sx < Cols; sx++)
            {
                var r = ((sy / perY) * gx) + (sx / perX);
                for (var c = 0; c < 3; c++) means[r][c] += sample[(sy * Cols) + sx][c];
            }

        var per = perX * perY;
        for (var r = 0; r < regions; r++)
            for (var c = 0; c < 3; c++) means[r][c] /= per;

        // Two-means about the cell's mean luma, exactly as the compositor picks fg and bg.
        double[] cellMean = [0, 0, 0];
        for (var r = 0; r < regions; r++)
            for (var c = 0; c < 3; c++) cellMean[c] += means[r][c] / regions;
        var pivot = Luma(cellMean);

        double[] hi = [0, 0, 0], lo = [0, 0, 0];
        int hiN = 0, loN = 0;
        var high = new bool[regions];
        for (var r = 0; r < regions; r++)
        {
            high[r] = Luma(means[r]) >= pivot;
            if (high[r]) { for (var c = 0; c < 3; c++) hi[c] += means[r][c]; hiN++; }
            else { for (var c = 0; c < 3; c++) lo[c] += means[r][c]; loN++; }
        }

        var fg = Q(hiN > 0 ? [hi[0] / hiN, hi[1] / hiN, hi[2] / hiN] : cellMean, levels, palette);
        var bg = Q(loN > 0 ? [lo[0] / loN, lo[1] / loN, lo[2] / loN] : cellMean, levels, palette);

        // Every sub-pixel displays whichever of the two colours its region was assigned.
        var err = 0.0;
        for (var sy = 0; sy < Rows; sy++)
            for (var sx = 0; sx < Cols; sx++)
            {
                var r = ((sy / perY) * gx) + (sx / perX);
                var shown = high[r] ? fg : bg;
                var s = sample[(sy * Cols) + sx];
                for (var c = 0; c < 3; c++)
                {
                    var d = shown[c] - s[c];
                    err += d * d;
                }
            }

        return (err, (Pack(fg) << 24) ^ Pack(bg));
    }

    private static double[] At(byte[] pixels, int w, int x, int y)
    {
        var o = ((y * w) + x) * 4;
        return [pixels[o], pixels[o + 1], pixels[o + 2]];
    }

    private static double Luma(double[] rgb) => (0.299 * rgb[0]) + (0.587 * rgb[1]) + (0.114 * rgb[2]);

    private static double[] Q(double[] rgb, int levels, double[][]? palette = null)
    {
        if (palette is not null) return palette[Nearest(rgb, palette)];
        return levels <= 1 ? rgb : [QRound(rgb[0], levels), QRound(rgb[1], levels), QRound(rgb[2], levels)];
    }

    private static double QRound(double v, int levels) =>
        Math.Round(v / (255.0 / (levels - 1))) * (255.0 / (levels - 1));

    private static long Pack(double[] rgb) =>
        ((long)Math.Round(rgb[0]) << 16) | ((long)Math.Round(rgb[1]) << 8) | (long)Math.Round(rgb[2]);
}
