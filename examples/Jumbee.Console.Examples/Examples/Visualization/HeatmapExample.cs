namespace Jumbee.Console.Examples;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// A heatmap — a 2D value grid mapped through the Viridis colour map into coloured cells.
/// Here a smooth 2D function so the gradient is easy to read.
/// </summary>
public sealed class HeatmapExample : Plot, IExample
{
    public HeatmapExample()
    {
        const int rows = 16, cols = 32;
        var grid = new List<double[]>();
        for (int r = 0; r < rows; r++)
        {
            // Row 0 is the top; build a smooth ridge that shifts across the grid.
            double y = (double)r / rows;
            grid.Add(Enumerable.Range(0, cols).Select(c =>
            {
                double x = (double)c / cols;
                return Math.Sin(x * Math.PI * 2) * Math.Cos(y * Math.PI) + x - y;
            }).ToArray());
        }

        AddHeatmap(grid.ToArray(), PlotColormap.Viridis);
        ConfigureGrid(g => g.IsVisible = false);
    }

    #region IExample
    string IExample.Category => "Visualization";
    string IExample.Title => "Heatmap";
    string IExample.Description =>
        "A 2D value grid coloured by the Viridis map — the color-grid plot family, built on per-cell colouring.";
    #endregion
}
