namespace Jumbee.Console;

using System;
using System.Collections.Generic;
using System.Linq;

using ConsoleGUI;

/// <summary>
///  A grid layout with controls arranged in rows and columns.
/// </summary>
public class Grid : Layout<ConsoleGUI.Controls.Grid>
{
    #region Constructors
    /// <summary>
    /// Creates a grid layout with fixed row heights, fixed column widths, and a control for each cell.
    /// </summary>
    /// <remarks>
    /// Sizing is <b>fixed cells</b>: every value is an absolute cell count (a row's height, a column's width), and
    /// the grid's own size is their sum. There is no proportional/"star" sizing and no auto-fill — unlike
    /// <see cref="DockPanel"/>, a <c>0</c> here means a 0-cell (collapsed) row/column, <em>not</em> fill-the-parent.
    /// Each cell's control is given its cell's fixed size (so a control that fills, i.e. <c>Width</c>/<c>Height</c>
    /// 0, fills that fixed cell). For proportional/fill layouts, compose <see cref="DockPanel"/>/<see cref="SplitPanel"/>
    /// instead.
    /// <para>
    /// <b>A grid does not grow with the terminal.</b> The app is re-laid-out on resize, but a grid's extents are
    /// absolute, so a grid used as the <em>root</em> renders at the size you specified and leaves the rest of the
    /// screen empty (or clips, if the terminal is smaller). That's the right behaviour for a form or a fixed panel
    /// nested inside a region; it is the wrong choice for an app shell. Build the shell from
    /// <see cref="DockPanel"/>/<see cref="SplitPanel"/> and put grids inside the regions that genuinely want fixed
    /// geometry. Nesting doesn't change this — a grid inside a docked panel's fill slot still won't grow.
    /// </para>
    /// <para>
    /// To give a cell's content an explicit size of its own — needed for anything without <c>Width</c>/<c>Height</c>,
    /// such as a <see cref="ControlFrame"/> or a nested layout — wrap it in a <see cref="Boundary"/>:
    /// <c>new Boundary(child, width, height)</c>.
    /// </para>
    /// </remarks>
    /// <seealso cref="Boundary"/>
    /// <param name="rowHeights">The fixed height in cells of each row, top to bottom.</param>
    /// <param name="columnWidths">The fixed width in cells of each column, left to right.</param>
    /// <param name="controls">
    /// Row-major controls: one inner array per row, each with one control per column. A cell may be
    /// <see langword="null"/> to leave it empty, and a whole row may be written as <c>[]</c> to leave every cell in
    /// it empty — a sparse arrangement needs no filler controls.
    /// </param>
    /// <exception cref="ArgumentException">The control grid's row/column counts don't match
    /// <paramref name="rowHeights"/>/<paramref name="columnWidths"/>.</exception>
    public Grid(int[] rowHeights, int[] columnWidths, params IFocusable?[][] controls ) : base(new ConsoleGUI.Controls.Grid())
    {                
        control.Rows = rowHeights.Select(h => new ConsoleGUI.Controls.Grid.RowDefinition(h)).ToArray();
        control.Columns = columnWidths.Select(w => new ConsoleGUI.Controls.Grid.ColumnDefinition(w)).ToArray();
        
        if (controls.Length != rowHeights.Length)
        {
            throw new ArgumentException($"The number of control rows: {controls.Length} must match the number of row heights: {rowHeights.Length}.");
        }
        // A row is either fully specified or written as `[]` for an entirely empty one. Anything between is almost
        // always a miscount, and catching it is the only reason this check exists — so an empty row is allowed as an
        // unambiguous shorthand while a short row still throws.
        if (controls.Any(r => r.Length is not 0 && r.Length != columnWidths.Length))
        {
            var c = controls.First(r => r.Length is not 0 && r.Length != columnWidths.Length);
            var index = Array.IndexOf(controls, c);
            throw new ArgumentException($"The number of control columns in row {index}: {c.Length} must match the number of column widths: {columnWidths.Length}, or be 0 for an empty row.");
        }   
        for (int r = 0; r < controls.Length; r++)
        {
            for (int c = 0; c < controls[r].Length; c++)
            {
                // A null cell is simply never given a child: the underlying grid returns an empty character for a
                // cell it has no drawing context for, so a gap costs nothing rather than costing a blank control.
                if (controls[r][c] is { } cell) control.AddChild(c, r, cell.FocusableControl);
            }
        }       
    }
    #endregion

    #region Methods
    /// <summary>Places <paramref name="child"/> in the cell at the given <paramref name="row"/> and <paramref name="column"/>.</summary>
    public void SetChild(int row, int column, IFocusable child)
    {
        control.AddChild(column, row, child.FocusableControl);
    }

    /// <summary>Number of rows in the grid.</summary>
    public override int Rows => control.Rows.Length;

    /// <summary>Number of columns in the grid.</summary>
    public override int Columns => control.Columns.Length;

    /// <summary>Gets the control at the given <paramref name="row"/> and <paramref name="column"/>.</summary>
    public override IFocusable this[int row, int column] => (IFocusable) control.GetChild(column, row);
    #endregion   
}
