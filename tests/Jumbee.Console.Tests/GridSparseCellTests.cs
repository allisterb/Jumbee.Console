namespace Jumbee.Console.Tests;

using System;

using Jumbee.Console.Snapshot;

using Xunit;

/// <summary>
/// A <see cref="Grid"/> cell may be <see langword="null"/>, and a whole row may be <c>[]</c>.
/// </summary>
/// <remarks>
/// Surfaced by the Wolf3D movement pad, which is a cross: five columns and seven rows of which twenty-one cells are
/// empty. Written against the old API it was twenty-one calls to a helper returning a blank <c>TextLabel</c> per
/// cell — a control allocated purely to be invisible, and enough noise to bury the shape being drawn.
/// </remarks>
public class GridSparseCellTests
{
    [Fact]
    public void NullCellsLeaveTheirCellEmptyAndDrawTheRest()
    {
        var grid = new Grid([1, 1], [3, 3],
            [null, new TextLabel(TextLabelOrientation.Horizontal, "b")],
            [new TextLabel(TextLabelOrientation.Horizontal, "c"), null]);

        var lines = ConsoleSnapshot.ToLines(ConsoleSnapshot.Render(grid, 6, 2));

        Assert.Equal("   b", lines[0].TrimEnd());   // the empty first cell is blank, not collapsed
        Assert.Equal("c", lines[1].TrimEnd());
    }

    [Fact]
    public void AnEmptyRowLeavesEveryCellInItEmpty()
    {
        var grid = new Grid([1, 1, 1], [3],
            [new TextLabel(TextLabelOrientation.Horizontal, "a")],
            [],
            [new TextLabel(TextLabelOrientation.Horizontal, "c")]);

        var lines = ConsoleSnapshot.ToLines(ConsoleSnapshot.Render(grid, 3, 3));

        Assert.Equal("a", lines[0].TrimEnd());
        Assert.Equal("", lines[1].TrimEnd());
        Assert.Equal("c", lines[2].TrimEnd());
    }

    // Enumerating the cells must skip the empties, not walk into them. The first version of this feature rendered
    // fine standalone and threw the moment the grid went inside a CompositeControl, which enumerates Controls to
    // collect its children -- so a render-only test proved nothing about the case that actually broke.
    [Fact]
    public void EnumeratingControlsSkipsEmptyCells()
    {
        var grid = new Grid([1, 1], [3, 3],
            [null, new TextLabel(TextLabelOrientation.Horizontal, "b")],
            []);

        var controls = System.Linq.Enumerable.ToList(((ILayout)grid).Controls);

        Assert.Single(controls);   // only the populated cell
    }

    // The arity check is the only thing standing between a miscounted row and a silently wrong layout, so allowing
    // `[]` must not have opened the door to a row that is merely SHORT.
    [Fact]
    public void AShortRowStillThrows()
    {
        var ex = Assert.Throws<ArgumentException>(() => new Grid([1], [3, 3],
            [new TextLabel(TextLabelOrientation.Horizontal, "a")]));

        Assert.Contains("must match the number of column widths", ex.Message);
    }
}
