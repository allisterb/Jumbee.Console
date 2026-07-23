namespace Jumbee.Console.Tests;

using System;
using System.Linq;

using Jumbee.Console.Snapshot;

using Xunit;

using CColor = ConsoleGUI.Data.Color;

/// <summary>Live-update series handles: a held <see cref="PlotSeries"/> updates the plot in place (no Clear/re-add).</summary>
public class PlotLiveTests
{
    private static string Render(Plot p) => ConsoleSnapshot.ToText(p, 50, 16);
    private static bool HasBraille(string s) => s.IndexOfAny([.. Braille()]) >= 0;
    private static System.Collections.Generic.IEnumerable<char> Braille()
    {
        for (char c = '⠀'; c <= '⣿'; c++) yield return c;
    }

    [Fact]
    public void LiveBars_SetValues_UpdatesInPlace()
    {
        var plot = new Plot();
        var bars = plot.AddLiveBars(new CColor(90, 160, 240));
        plot.ConfigureGrid(g => g.IsVisible = false);

        bars.SetValues([2, 8, 4]);
        var first = Render(plot);
        Assert.Contains('█', first);

        bars.SetValues([8, 2, 4]);   // same handle, new data, no Clear/re-add
        var second = Render(plot);
        Assert.Contains('█', second);
        Assert.NotEqual(first, second);   // the plot reflects the new data
    }

    [Fact]
    public void LiveSeries_PushRollingWindow_RendersAndDoesNotThrow()
    {
        var plot = new Plot();
        var line = plot.AddLiveSeries(new CColor(240, 120, 100));
        plot.ConfigureGrid(g => g.IsVisible = false);

        // Stream 200 points through a 40-point window — the rolling window keeps memory/render bounded.
        var ex = Record.Exception(() =>
        {
            for (int i = 0; i < 200; i++) line.Push(i, Math.Sin(i * 0.2) * 10, maxPoints: 40);
        });
        Assert.Null(ex);
        Assert.True(HasBraille(Render(plot)), "a live line should render braille points");
    }

    [Fact]
    public void FixedYRange_PinsAxis_RegardlessOfData()
    {
        static string ToText(Plot p) => ConsoleSnapshot.ToText(p, 44, 16);

        // Small data (~10..25): auto-scaled, the axis tops out near 25 → no "80" tick.
        var auto = new Plot();
        auto.ConfigureGrid(g => g.IsVisible = false);
        auto.AddLiveSeries().SetData([0, 1, 2, 3], [10, 20, 15, 25]);
        Assert.DoesNotContain("80", ToText(auto));

        // Same data, axis pinned to 0..100 → a high tick (80) is present regardless of the small data...
        var pinned = new Plot();
        pinned.ConfigureGrid(g => g.IsVisible = false);
        pinned.SetYRange(0, 100);
        var s = pinned.AddLiveSeries();
        s.SetData([0, 1, 2, 3], [10, 20, 15, 25]);
        Assert.Contains("80", ToText(pinned));

        // ...and the axis stays put when the data changes completely (it doesn't rescale/jump).
        s.SetData([0, 1, 2, 3], [45, 55, 50, 60]);
        Assert.Contains("80", ToText(pinned));
    }

    [Fact]
    public void XWindow_ShowsLatestWindow_AndStaysNonNegative()
    {
        var p = new Plot();
        p.ConfigureGrid(g => g.IsVisible = false);
        p.SetXWindow(20);
        var s = p.AddLiveSeries();

        // Latest data around x = 80..100: the sliding window shows [80, 100], so a high x tick is present (the axis
        // followed the data forward) — the old low-x points scrolled off.
        s.SetData(Enumerable.Range(80, 21).Select(i => (double)i).ToArray(),
                  Enumerable.Range(0, 21).Select(i => (double)(i % 6)).ToArray());
        Assert.Contains("100", ConsoleSnapshot.ToText(p, 50, 12));

        // Early on (data hasn't filled the window), the window is floored at 0 — never a negative x axis.
        s.SetData([0, 1, 2, 3, 4, 5], [1, 3, 2, 4, 3, 5]);
        var early = ConsoleSnapshot.ToText(p, 50, 12);
        Assert.DoesNotContain("-", early.Split('\n').Last());   // the x-axis label row has no negative values
    }

    [Fact]
    public void Scroll_KeepsFixedStripAxis_NoMatterHowManyValues()
    {
        var p = new Plot();
        p.ConfigureGrid(g => g.IsVisible = false);
        p.SetXRange(0, 19);
        var s = p.AddLiveSeries();

        // Scroll 1000 values through a 20-wide strip: x stays 0..19, so the axis never grows past ~19 — no large,
        // ever-increasing x labels (which is what a Push-based ever-increasing counter would produce).
        for (int i = 0; i < 1000; i++) s.Scroll(50 + 20 * Math.Sin(i * 0.2), 20);
        var text = ConsoleSnapshot.ToText(p, 44, 12);

        Assert.DoesNotContain("100", text);   // would appear as an x tick only if the axis had grown
        Assert.DoesNotContain("500", text);
    }

    [Fact]
    public void LiveScatter_DrawsUnconnectedMarkers_NotAConnectedLine()
    {
        // Same sparse, widely-spaced points fed to a live line vs a live scatter. A line rasterizes the segments
        // between points (filling many intermediate cells); scatter marks only the points — so it fills strictly
        // fewer braille cells. This is what lets the no-rebuild live path also render markers (round-8 gap fix).
        double[] xs = [0, 1, 2, 3, 4];
        double[] ys = [0, 9, 1, 8, 2];

        var lp = new Plot();
        lp.ConfigureGrid(g => g.IsVisible = false);
        lp.AddLiveSeries(new CColor(120, 200, 255)).SetData(xs, ys);
        int lineCells = BrailleCount(Render(lp));

        var sp = new Plot();
        sp.ConfigureGrid(g => g.IsVisible = false);
        sp.AddLiveScatter(new CColor(120, 200, 255)).SetData(xs, ys);
        int scatterCells = BrailleCount(Render(sp));

        Assert.True(scatterCells > 0, "a live scatter should draw markers");
        Assert.True(scatterCells < lineCells,
            $"scatter ({scatterCells} cells) should fill fewer cells than the connected line ({lineCells})");
    }

    [Fact]
    public void Scatter_PointsOutsideFixedAxisRange_AreClippedNotThrown()
    {
        // Regression: a finite scatter point outside the pinned axis range maps to a sub-pixel column past the last
        // cell. VirtualGraphics.DrawPoint must drop it against the REAL buffer bounds, not the scaled virtual bounds.
        // (The scaled-bounds check alone passes for a sub-pixel column that is still out of the actual image; the
        // hot-path SetPixel no longer guards, so an unclipped write would throw IndexOutOfRangeException. Several
        // overshoot factors are used so at least one lands in that just-past-the-edge band regardless of plot size.)
        var p = new Plot();
        p.ConfigureGrid(g => g.IsVisible = false);
        p.SetXRange(0, 10);
        p.SetYRange(0, 10);
        // 5,5 is inside the pinned window; the rest overshoot both axes by 1.3x–4x.
        p.AddScatter([5.0, 13.0, 16.0, 20.0, 40.0], [5.0, 13.0, 16.0, 20.0, 40.0], PlotBrush.Braille);

        var ex = Record.Exception(() => Render(p));
        Assert.Null(ex);                    // off-axis points are clipped, not fatal
        Assert.True(HasBraille(Render(p))); // the in-range point still draws
    }

    private static int BrailleCount(string s) => s.Count(c => c is >= '⠀' and <= '⣿');

    // Sub-cell braille dot bits: dots 1/4 (top row of the cell) are bits 0/3 = 0x09; dots 7/8 (bottom row) are
    // bits 6/7 = 0xC0.
    private static bool RowHasTopDot(string row) => row.Any(c => c is >= '⠀' and <= '⣿' && (c & 0x09) != 0);
    private static bool RowHasBottomDot(string row) => row.Any(c => c is >= '⠀' and <= '⣿' && (c & 0xC0) != 0);

    [Fact]
    public void BrailleSubCell_TopOfPlotLightsTopDots_BottomLightsBottomDots()
    {
        // Regression for the ConsolePlot sub-pixel vertical inversion: a horizontal line at the TOP of the plot must
        // light a braille cell's TOP dots (1/4), and one at the BOTTOM its BOTTOM dots (7/8) — matching ratatui.
        // Before the ScaledConverter "+ (res-1)" fix, the top vRes-1 sub-rows were unreachable, so data-max wrongly
        // lit the cell's BOTTOM dots.
        static Plot Bare()
        {
            var p = new Plot();
            p.ConfigureGrid(g => g.IsVisible = false);
            p.ConfigureTicks(t => { t.IsVisible = false; t.Labels.IsVisible = false; });
            p.ConfigureAxis(a => a.IsVisible = false);
            return p;
        }

        // Anchors at y=0 and y=10 (right column) pin the range to [0,10]; the line under test spans the left columns.
        var topPlot = Bare();
        topPlot.AddScatter([9.0, 9.0], [0.0, 10.0], PlotBrush.Braille);
        topPlot.AddSeries([0.0, 8.0], [10.0, 10.0], PlotBrush.Braille);   // line at the very top
        var topRows = ConsoleSnapshot.ToLines(ConsoleSnapshot.Render(topPlot, 10, 4));
        Assert.True(RowHasTopDot(topRows[0]), "a line at the top of the plot should light a braille cell's top dots");

        var botPlot = Bare();
        botPlot.AddScatter([9.0, 9.0], [0.0, 10.0], PlotBrush.Braille);
        botPlot.AddSeries([0.0, 8.0], [0.0, 0.0], PlotBrush.Braille);     // line at the very bottom
        var botRows = ConsoleSnapshot.ToLines(ConsoleSnapshot.Render(botPlot, 10, 4));
        Assert.True(RowHasBottomDot(botRows[^1]), "a line at the bottom of the plot should light a braille cell's bottom dots");
    }

    [Fact]
    public void ChromeColours_RenderTrueColour_NotSnappedTo16()
    {
        // An arbitrary RGB that is NOT one of the 16 console colours. Before ConsolePlot's chrome went full-RGB this
        // was downsampled (nearest-of-16) when set on the axis/grid/tick pens and captions; now it must survive exactly.
        var rgb = new Color(30, 144, 255);   // DodgerBlue — not a console colour
        var plot = new Plot();
        plot.SetAxisColor(rgb);
        plot.SetGridColor(rgb);
        plot.SetTickColor(rgb, rgb);
        plot.SetAxisTitles("t", "a", rgb);
        plot.AddLiveSeries(Color.Yellow).SetData([0, 1, 2, 3], [0, 5, 2, 8]);

        var buffer = ConsoleSnapshot.Render(plot, 44, 16);

        // The only source of this exact RGB is the chrome (the series is yellow), so an exact match anywhere proves
        // the axis/grid/tick/title colour reached the buffer without a 16-colour approximation.
        bool exact = false;
        for (int y = 0; y < 16 && !exact; y++)
            for (int x = 0; x < 44 && !exact; x++)
                if (ConsoleSnapshot.ForegroundAt(buffer, x, y) == rgb) exact = true;
        Assert.True(exact, "axis/grid/tick/title chrome should render the exact RGB, not a 16-colour approximation");
    }

    [Fact]
    public void AxisColorSetters_ApplyInJumbeeColour_AndSurviveClear()
    {
        // The Jumbee-colour setters (no ConsoleColor / immutable-LinePen dance) route to retained chrome, so a
        // Clear()+re-add of the data must not drop the captions or their colour.
        var plot = new Plot();
        plot.ConfigureGrid(g => g.IsVisible = false);
        plot.SetAxisTitles("time", "amp", Color.Aqua);   // Aqua == cyan
        plot.SetAxisColor(Color.Aqua);
        var s = plot.AddLiveSeries(Color.Yellow);
        s.SetData([0, 1, 2, 3], [0, 5, 2, 8]);

        var buffer = ConsoleSnapshot.Render(plot, 44, 16);
        var text = ConsoleSnapshot.ToText(buffer);
        Assert.Contains("time", text);   // XTitle bottom-right
        Assert.Contains("amp", text);    // YTitle top-left

        // Assert the 'amp' caption rendered cyan-ish via the colour-readback API (not a PNG). Exact RGB depends on
        // the 16-colour render path, so check the channel signature (low R, high G+B) rather than an exact match.
        var lines = text.Split('\n');
        bool cyanish = false;
        for (int y = 0; y < lines.Length && !cyanish; y++)
        {
            int x = lines[y].IndexOf("amp", StringComparison.Ordinal);
            if (x >= 0 && ConsoleSnapshot.ForegroundAt(buffer, x, y) is { } c)
                cyanish = c.R < 128 && c.G > 128 && c.B > 128;
        }
        Assert.True(cyanish, "the 'amp' caption should render cyan-ish");

        // Clear the data and re-add — the retained chrome (titles + colour) must persist.
        plot.Clear();
        s = plot.AddLiveSeries(Color.Yellow);
        s.SetData([0, 1, 2, 3], [1, 2, 3, 4]);
        var after = ConsoleSnapshot.ToText(plot, 44, 16);
        Assert.Contains("time", after);
        Assert.Contains("amp", after);
    }

    [Fact]
    public void LiveSeries_SetData_ThenClear()
    {
        var plot = new Plot();
        var line = plot.AddLiveSeries();
        line.SetData([0, 1, 2, 3], [0, 5, 2, 8]);
        Assert.True(HasBraille(Render(plot)));

        line.Clear();
        Assert.False(HasBraille(Render(plot)), "a cleared live series draws no points");
    }
}
