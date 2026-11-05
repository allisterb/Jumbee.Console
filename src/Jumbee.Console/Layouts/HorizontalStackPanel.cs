namespace Jumbee.Console;

using System;
using System.Collections.Generic;
using ConsoleGUI;

/// <summary>A layout that arranges its child controls in a single horizontal row.</summary>
public class HorizontalStackPanel : Layout<ConsoleGUI.Controls.HorizontalStackPanel>
{
    /// <summary>Initializes a new <see cref="HorizontalStackPanel"/> containing the given <paramref name="controls"/>.</summary>
    public HorizontalStackPanel(params IFocusable[]? controls) : base(new ConsoleGUI.Controls.HorizontalStackPanel())
    {
        if (controls != null) Add(controls);
    }

    /// <summary>Appends the given <paramref name="controls"/> to the row.</summary>
    public void Add(params IFocusable[] controls)
    {
        // Routing reads `children`; the panel binds the render node, so a nested layout's damage isn't dropped.
        foreach (var control in controls)
        {
            children.Add(control);
            this.control.Add(control.RenderNode());
        }
    }

    /// <summary>Removes the given <paramref name="controls"/> from the row.</summary>
    public void Remove(params IFocusable[] controls)
    {
        foreach (var control in controls)
        {
            if (children.Remove(control)) this.control.Remove(control.RenderNode());
        }
    }

    /// <summary>Number of rows in the layout grid (always 1).</summary>
    public override int Rows => 1;

    /// <summary>Number of columns, i.e. the child count.</summary>
    public override int Columns => children.Count;

    /// <summary>Gets the control at the given <paramref name="column"/> (<paramref name="row"/> must be 0).</summary>
    public override IFocusable this[int row, int column] =>
        row == 0 ? children[column] : throw new ArgumentOutOfRangeException(nameof(row));

    private readonly List<IFocusable> children = [];
}
