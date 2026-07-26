namespace ScopeTui;

using System;
using System.Collections.Generic;

/// <summary>
/// Builders for a pane's explicit x-axis ticks, replacing the plotting library's cell-spacing heuristic.
/// </summary>
/// <remarks>
/// The heuristic asks "roughly how many cells apart?" and answers with the nearest round number in data space, which
/// is why the resulting spacing is hard to predict from the flag. These name positions in DATA space directly, so a
/// tick lands exactly where the caller asked. Each takes the current x range and the pane width, because both panes'
/// ranges move at runtime (Samples on the oscilloscope, Scale on the spectroscope) and because the width decides how
/// many ticks can be drawn before they collide.
/// </remarks>
public static class XTickBuilders
{
    /// <summary>Minimum cells between ticks before they are thinned — below this the labels overlap and the grid
    /// turns into a solid block.</summary>
    const int MinCellsPerTick = 8;

    /// <summary>
    /// A tick every <paramref name="step"/> data units, labelled with the value — the oscilloscope's x axis is
    /// sample index, so <c>--tick 200</c> really is a gridline every 200 samples.
    /// </summary>
    /// <remarks>
    /// Thins to every 2nd, 3rd... tick when the requested step would put them closer than
    /// <see cref="MinCellsPerTick"/> apart. The library's heuristic used to prevent that implicitly; naming the step
    /// directly means the caller can ask for something undrawable, so the guard has to be explicit.
    /// </remarks>
    public static Func<double, double, int, IReadOnlyList<(double Value, string Label)>> Every(double step) =>
        (min, max, width) =>
        {
            var ticks = new List<(double, string)>();
            if (step <= 0 || max <= min || width <= 0) return ticks;

            // Thin so ticks stay at least MinCellsPerTick apart on screen.
            var cellsPerStep = step / (max - min) * width;
            var stride = cellsPerStep >= MinCellsPerTick ? 1 : (int)Math.Ceiling(MinCellsPerTick / Math.Max(cellsPerStep, 1e-9));
            var effective = step * stride;

            for (var v = Math.Ceiling(min / effective) * effective; v <= max; v += effective)
                ticks.Add((v, v.ToString("0.###")));
            return ticks;
        };

    /// <summary>
    /// Ticks at fixed frequencies, positioned by <c>ln(Hz)</c> and labelled in Hz — for the spectroscope, whose x
    /// axis is a natural log so its own axis values ("3.5", "9.5") are unreadable as frequencies.
    /// </summary>
    /// <remarks>
    /// The decade/half-decade set below is the subset of the vertical reference lines the spectroscope already draws
    /// (Spectroscope.markerFrequencies) that is worth labelling; ticking every marker would collide. Frequencies
    /// outside the current range are dropped, and the set is thinned by the same width rule as <see cref="Every"/>.
    /// </remarks>
    public static Func<double, double, int, IReadOnlyList<(double Value, string Label)>> Frequencies() =>
        (min, max, width) =>
        {
            var ticks = new List<(double, string)>();
            if (max <= min || width <= 0) return ticks;

            // Widest-first: drop the in-between markers before the decades when space is tight.
            foreach (var tier in new[] { Decades, HalfDecades })
            {
                var candidates = new List<(double, string)>();
                foreach (var hz in tier)
                {
                    var x = Math.Log(hz);
                    if (x >= min && x <= max) candidates.Add((x, Label(hz)));
                }

                ticks.AddRange(candidates);
                // Stop adding tiers once the labels would crowd: each needs room for its text plus a gap.
                if (ticks.Count > 0 && width / (double)ticks.Count < MinCellsPerTick) break;
            }

            ticks.Sort((a, b) => a.Item1.CompareTo(b.Item1));
            return ticks;
        };

    static string Label(double hz) => hz >= 1000 ? $"{hz / 1000:0.#}k" : $"{hz:0}";

    static readonly double[] Decades = [100, 1000, 10000];
    static readonly double[] HalfDecades = [20, 50, 200, 500, 2000, 5000, 20000];
}
