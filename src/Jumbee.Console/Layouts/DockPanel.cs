namespace Jumbee.Console;

using System;
using ConsoleGUI;

/// <summary>Which edge a <see cref="DockPanel"/> pins its docked control to.</summary>
public enum DockedControlPlacement
{
    /// <summary>Dock the control to the top edge.</summary>
    Top,
    /// <summary>Dock the control to the right edge.</summary>
    Right,
    /// <summary>Dock the control to the bottom edge.</summary>
    Bottom,
    /// <summary>Dock the control to the left edge.</summary>
    Left
}

/// <summary>A two-child layout that pins one control to an edge and fills the remaining space with the other.</summary>
public class DockPanel : Layout<ConsoleGUI.Controls.DockPanel>
{
    /// <summary>Initializes a new <see cref="DockPanel"/> that docks <paramref name="dockedControl"/> at <paramref name="placement"/> and fills the rest with <paramref name="fillControl"/>.</summary>
    public DockPanel(DockedControlPlacement placement, IFocusable dockedControl, IFocusable fillControl)
        : base(new ConsoleGUI.Controls.DockPanel())
    {
        this.DockedControl = dockedControl;
        this.FillControl = fillControl;
        control.Placement = placement switch
        {
            DockedControlPlacement.Top => ConsoleGUI.Controls.DockPanel.DockedControlPlacement.Top,
            DockedControlPlacement.Right => ConsoleGUI.Controls.DockPanel.DockedControlPlacement.Right,
            DockedControlPlacement.Bottom => ConsoleGUI.Controls.DockPanel.DockedControlPlacement.Bottom,
            DockedControlPlacement.Left => ConsoleGUI.Controls.DockPanel.DockedControlPlacement.Left,
            _ => throw new NotSupportedException($"Unknown DockedControlPlacement value {placement}.")
        };
    }

    /// <summary>The control pinned to the docked edge. Settable at runtime: reassign it to swap the docked pane in
    /// place — e.g. a "zen"/full-screen toggle that swaps this to a small placeholder and back — without rebuilding
    /// the layout.</summary>
    /// <remarks>
    /// Give the docked control a positive width (for a Left/Right dock) or height (Top/Bottom). A width or
    /// height of 0 is the "fill the parent" sentinel, so a 0-sized docked control takes the whole panel and starves
    /// the fill control — use a positive extent, or swap this to a narrow control, to collapse the pane instead.
    /// <para>
    /// Controls that expose <c>Width</c>/<c>Height</c> can be sized directly (or via <c>WithWidth</c>/<c>WithHeight</c>).
    /// Anything that doesn't — a <see cref="ControlFrame"/>, a nested layout — has no extent of its own, so wrap
    /// it in a <see cref="Boundary"/> to give it one: <c>new Boundary(framedChart, width, height)</c>. Forgetting this
    /// is the common failure: the dock silently fills the whole panel and the fill control never appears, with no error.
    /// </para>
    /// </remarks>
    /// <seealso cref="Boundary"/>
    public IFocusable DockedControl
    {
        get => field;
        set
        {
            field = value;
            control.DockedControl = value.RenderNode();
        }
    }

    /// <summary>The control that fills the space left after docking. Settable at runtime.</summary>
    public IFocusable FillControl
    {
        get => field;
        set
        {
            field = value;
            control.FillingControl = value.RenderNode();
        }
    }

    /// <summary>Number of rows in the layout grid (2 when docked top/bottom, otherwise 1).</summary>
    public override int Rows => (control.Placement == ConsoleGUI.Controls.DockPanel.DockedControlPlacement.Top ||
                                 control.Placement == ConsoleGUI.Controls.DockPanel.DockedControlPlacement.Bottom) ? 2 : 1;

    /// <summary>Number of columns in the layout grid (2 when docked left/right, otherwise 1).</summary>
    public override int Columns => (control.Placement == ConsoleGUI.Controls.DockPanel.DockedControlPlacement.Left ||
                                    control.Placement == ConsoleGUI.Controls.DockPanel.DockedControlPlacement.Right) ? 2 : 1;

    /// <summary>Gets the control at the given <paramref name="row"/> and <paramref name="column"/>.</summary>
    public override IFocusable this[int row, int column]
    {
        get
        {
            if (row < 0 || row >= Rows) throw new ArgumentOutOfRangeException(nameof(row));
            if (column < 0 || column >= Columns) throw new ArgumentOutOfRangeException(nameof(column));

            switch (control.Placement)
            {
                case ConsoleGUI.Controls.DockPanel.DockedControlPlacement.Top:
                    return row == 0 ? DockedControl : FillControl;
                case ConsoleGUI.Controls.DockPanel.DockedControlPlacement.Bottom:
                    return row == 0 ? FillControl : DockedControl;
                case ConsoleGUI.Controls.DockPanel.DockedControlPlacement.Left:
                    return column == 0 ? DockedControl : FillControl;
                case ConsoleGUI.Controls.DockPanel.DockedControlPlacement.Right:
                    return column == 0 ? FillControl : DockedControl;
                default:
                    throw new NotSupportedException($"Unknown DockedControlPlacement value {control.Placement}.");
            }
        }
    }
}
