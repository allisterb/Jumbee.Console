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
    /// To give a cell's content an explicit size of its own — needed for anything without <c>Width</c>/<c>Height</c>,
    /// such as a <see cref="ControlFrame"/> or a nested layout — wrap it in a <see cref="Boundary"/>:
    /// <c>new Boundary(child, width, height)</c>.
    /// </para>
    /// </remarks>
    /// <seealso cref="Boundary"/>
    /// <param name="rowHeights">The fixed height in cells of each row, top to bottom.</param>
    /// <param name="columnWidths">The fixed width in cells of each column, left to right.</param>
    /// <param name="controls">Row-major controls: one inner array per row, each with one control per column.</param>
    /// <exception cref="ArgumentException">The control grid's row/column counts don't match
    /// <paramref name="rowHeights"/>/<paramref name="columnWidths"/>.</exception>
    public Grid(int[] rowHeights, int[] columnWidths, params IFocusable[][] controls ) : base(new ConsoleGUI.Controls.Grid())
    {                
        control.Rows = rowHeights.Select(h => new ConsoleGUI.Controls.Grid.RowDefinition(h)).ToArray();
        control.Columns = columnWidths.Select(w => new ConsoleGUI.Controls.Grid.ColumnDefinition(w)).ToArray();
        
        if (controls.Length != rowHeights.Length)
        {
            throw new ArgumentException($"The number of control rows: {controls.Length} must match the number of row heights: {rowHeights.Length}.");
        }
        if (controls.Any(r => r.Length != columnWidths.Length))
        {
            var c = controls.First(r => r.Length != columnWidths.Length);
            var index = Array.IndexOf(controls, c);
            throw new ArgumentException($"The number of control columns in row {index}: {c.Length} must match the number of column widths: {columnWidths.Length}.");
        }   
        for (int r = 0; r < controls.Length; r++)
        {
            for (int c = 0; c < controls[r].Length; c++)
            {
                control.AddChild(c, r, controls[r][c].FocusableControl);
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
