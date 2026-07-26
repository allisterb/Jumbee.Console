namespace Jumbee.Console.Tests;

using System;
using System.Linq;
using System.Threading.Tasks;

using ConsoleGUI;

using Jumbee.Console.Snapshot;

using Xunit;

/// <summary>
/// <see cref="Plot.DamageTracking"/>: the plot reports only the cells a draw changed, so the compositor can skip
/// the rest. Over-reporting is merely wasteful, but UNDER-reporting drops updates — so what these pin down is that
/// a damage-tracking plot renders <em>identically</em> to one without it, frame for frame.
/// </summary>
public class PlotDamageTests
{
    private static string Render(Plot p) => ConsoleSnapshot.ToText(p, 60, 18);

    private static Plot Build(bool damage)
    {
        var plot = new Plot { DamageTracking = damage };
        plot.SetXRange(0, 10);
        plot.SetYRange(-1, 1);
        return plot;
    }

    // Drives both plots through the same sequence of live updates and compares them at every step. Whatever the
    // tracking does to the buffer, the visible result has to match the untracked control exactly.
    private static void AssertMatchesUntracked(params double[][] frames)
    {
        var tracked = Build(damage: true);
        var plain = Build(damage: false);
        var trackedSeries = tracked.AddLiveSeries();
        var plainSeries = plain.AddLiveSeries();
        var xs = Enumerable.Range(0, frames[0].Length).Select(i => (double)i).ToArray();

        for (var i = 0; i < frames.Length; i++)
        {
            trackedSeries.SetData(xs, frames[i]);
            plainSeries.SetData(xs, frames[i]);
            Assert.Equal(Render(plain), Render(tracked));
        }
    }

    [Fact]
    public void TrackedPlot_RendersIdenticallyToUntracked_AcrossMovingTrace()
    {
        // Up, down, back up: every frame vacates cells the previous one drew.
        AssertMatchesUntracked(
            [0.8, 0.8, 0.8, 0.8, 0.8],
            [-0.8, -0.8, -0.8, -0.8, -0.8],
            [0.8, -0.8, 0.8, -0.8, 0.8],
            [0.0, 0.0, 0.0, 0.0, 0.0]);
    }

    [Fact]
    public void TrackedPlot_RendersIdenticallyToUntracked_WhenTraceRepeats()
    {
        // The same data twice: the second draw changes nothing, so damage should be empty — and the render must
        // still be correct rather than blank.
        AssertMatchesUntracked(
            [0.5, 0.2, -0.3, 0.7, 0.1],
            [0.5, 0.2, -0.3, 0.7, 0.1],
            [0.5, 0.2, -0.3, 0.7, 0.1]);
    }

    [Fact]
    public void TrackedPlot_SurvivesResize()
    {
        // A resize rebuilds the plot over a new buffer, so the previous frame's saved values no longer apply --
        // the plot must fall back to reporting everything rather than diffing against them.
        var plot = Build(damage: true);
        var series = plot.AddLiveSeries();
        series.SetData([0, 1, 2, 3], [0.5, -0.5, 0.5, -0.5]);

        var small = ConsoleSnapshot.ToText(plot, 40, 12);
        var large = ConsoleSnapshot.ToText(plot, 70, 20);
        var backToSmall = ConsoleSnapshot.ToText(plot, 40, 12);

        Assert.NotEqual(small, large);
        Assert.Equal(small, backToSmall);   // no residue carried across the size changes
    }

    [Fact]
    public void DamageTracking_DefaultsOff()
    {
        Assert.False(new Plot().DamageTracking);
    }

    // The tests above compare the plot's own BUFFER, which is necessary but not sufficient: the compositor only
    // re-scans the rects a control reports, so an under-reporting plot would have a correct buffer and a stale
    // SCREEN. These drive the real ConsoleManager and check both what got composited and what came out.
    #region End-to-end, through the compositor
    public PlotDamageTests() => UiTestHarness.EnsureStopped();

    private static async Task<(long Dirty, string Screen)> SecondFrame(bool damage)
    {
        const int w = 60, h = 20;
        var plot = Build(damage);
        var series = plot.AddLiveSeries();
        series.SetData([0, 2, 4, 6, 8, 10], [0.8, 0.8, 0.8, 0.8, 0.8, 0.8]);

        using var session = await AnsiConsoleSession.StartAsync(new VerticalStackPanel(plot).CControl, w, h);

        series.SetData([0, 2, 4, 6, 8, 10], [-0.8, -0.8, -0.8, -0.8, -0.8, -0.8]);   // trace jumps top -> bottom
        await session.FrameAsync();
        return (ConsoleManager.LastFrameDirtyCells, ConsoleSnapshot.ToText(session.Screen.Buffer));
    }

    [Fact]
    public async Task TrackedPlot_CompositesLessThanUntracked_AndShowsTheSameScreen()
    {
        var tracked = await SecondFrame(damage: true);
        var plain = await SecondFrame(damage: false);

        // Same pixels on screen: damage reported every cell that changed, so nothing was skipped that mattered.
        Assert.Equal(plain.Screen, tracked.Screen);

        // ...for less compositing. An untracked plot reports its whole rect every frame.
        Assert.True(tracked.Dirty > 0, "the moved trace should have dirtied something");
        Assert.True(tracked.Dirty < plain.Dirty,
            $"tracked composited {tracked.Dirty} cells, untracked {plain.Dirty} — expected fewer");
    }
    #endregion
}
