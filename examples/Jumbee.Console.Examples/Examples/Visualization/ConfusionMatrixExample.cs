namespace Jumbee.Console.Examples;

using System.Collections.Generic;

/// <summary>
/// A confusion matrix — an annotated heatmap with each cell's count in contrast text on the cell colour.
/// Row = actual class, column = predicted class; the bright diagonal is the correctly-classified count.
/// </summary>
public sealed class ConfusionMatrixExample : Plot, IExample
{
    public ConfusionMatrixExample()
    {
        string[] classes = ["cat", "dog", "bird", "fish", "frog"];
        // A 5-class classifier's counts: a strong diagonal with some believable off-diagonal confusion.
        double[][] counts =
        [
            [58, 3, 1, 0, 2],
            [4, 47, 6, 1, 0],
            [1, 5, 51, 8, 2],
            [0, 2, 7, 44, 3],
            [2, 1, 0, 4, 63],
        ];

        AddConfusionMatrix(counts, rowLabels: classes, colLabels: classes);
        ConfigureGrid(g => g.IsVisible = false);
    }

    #region IExample
    string IExample.Category => "Visualization";
    string IExample.Title => "Confusion Matrix";
    string IExample.Description =>
        "An annotated heatmap — counts drawn in each cell with contrast text on the cell colour (per-cell background).";
    #endregion
}
