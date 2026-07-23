namespace Jumbee.Console.Examples;

using System.Collections.Generic;

/// <summary>
/// A stacked bar chart — three components stacked from the baseline at each x, so each bar's total height is the sum.
/// A <c>MultiBarSeries</c> (stacked mode) whose segments abut exactly.
/// </summary>
public sealed class StackedBarExample : Plot, IExample
{
    public StackedBarExample()
    {
        double[] xs = [1, 2, 3, 4, 5];
        double[][] series =
        [
            [8, 10, 7, 12, 9],    // base layer
            [5, 6, 9, 4, 7],      // middle layer
            [3, 4, 2, 6, 5],      // top layer
        ];
        Color[] colors = [new(89, 145, 240), new(120, 200, 120), new(240, 200, 90)];

        AddStackedBars(xs, series, colors);
        ConfigureGrid(g => g.IsVisible = false);
    }

    #region IExample
    string IExample.Category => "Visualization";
    string IExample.Title => "Stacked Bars";
    string IExample.Description =>
        "Series stacked from the baseline at each x — each bar's total height is the sum of its components.";
    #endregion
}
