namespace Jumbee.Console;

using System;
using System.Collections.Generic;
using ConsoleGUI;

/// <summary>A layout that arranges its child controls in a single vertical column.</summary>
public class VerticalStackPanel : Layout<ConsoleGUI.Controls.VerticalStackPanel>
{
    /// <summary>Initializes a new <see cref="VerticalStackPanel"/> containing the given <paramref name="controls"/>.</summary>
    public VerticalStackPanel(params IFocusable[]? controls) : base(new ConsoleGUI.Controls.VerticalStackPanel())
    {
        if (controls != null) Add(controls);
    }

    /// <summary>Appends the given <paramref name="controls"/> to the column.</summary>
    public void Add(params IFocusable[] controls)
    {
        // Routing reads `children`; the panel binds the render node, so a nested layout's damage isn't dropped.
        foreach (var control in controls)
        {
            children.Add(control);
            this.control.Add(control.RenderNode());
        }
    }

    /// <summary>Removes the given <paramref name="controls"/> from the column.</summary>
    public void Remove(params IFocusable[] controls)
    {
        foreach (var control in controls)
        {
            if (children.Remove(control)) this.control.Remove(control.RenderNode());
        }
    }

    /// <summary>Number of rows, i.e. the child count.</summary>
    public override int Rows => children.Count;

    /// <summary>Number of columns in the layout grid (always 1).</summary>
    public override int Columns => 1;

    /// <summary>Gets the control at the given <paramref name="row"/> (<paramref name="column"/> must be 0).</summary>
    public override IFocusable this[int row, int column] =>
        column == 0 ? children[row] : throw new ArgumentOutOfRangeException(nameof(column));

    private readonly List<IFocusable> children = [];
}
