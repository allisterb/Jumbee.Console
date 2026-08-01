namespace Jumbee.Console.Tests;

using System;

using ConsoleGUI.Input;
using ConsoleGUI.Space;

using Jumbee.Console;
using Jumbee.Console.Snapshot;

using Xunit;

public class SplitPanelTests
{
    private static SplitPanel Make(out TextLabel a, out TextLabel b, SplitOrientation o = SplitOrientation.Horizontal, int pos = 10)
    {
        a = new TextLabel(TextLabelOrientation.Horizontal, "AAAA");
        b = new TextLabel(TextLabelOrientation.Horizontal, "BBBB");
        return new SplitPanel(o, a, b, pos);
    }

    private static void Arrow(SplitPanel p, ConsoleKey key, bool shift = false)
        => UI.SendInput(p, new ConsoleKeyInfo('\0', key, shift, false, false));

    #region Geometry / model
    [Fact]
    public void RendersBothPanes_AndDivider()
    {
        var p = Make(out _, out _, pos: 8);
        var text = ConsoleSnapshot.ToText(p, 30, 6);

        Assert.Contains("AAAA", text);
        Assert.Contains("BBBB", text);
        Assert.Contains("│", text);          // the vertical divider
        Assert.Equal(8, p.SplitPosition);
    }

    [Fact]
    public void SplitPosition_ClampsToMinFirst()
    {
        var p = Make(out _, out _, pos: 10);
        ConsoleSnapshot.Render(p, 30, 6);

        p.SplitPosition = 0;

        Assert.Equal(p.MinFirst, p.SplitPosition);   // can't shrink the first pane below its minimum
    }

    [Fact]
    public void SplitPosition_ClampsToLeaveRoomForSecondPane()
    {
        var p = Make(out _, out _, pos: 10);
        ConsoleSnapshot.Render(p, 24, 6);            // total width 24

        p.SplitPosition = 999;

        Assert.Equal(24 - 1 - p.MinSecond, p.SplitPosition);   // total - divider - min(second)
    }

    [Fact]
    public void SplitChanged_RaisedOnChange_NotOnNoOp()
    {
        var p = Make(out _, out _, pos: 10);
        ConsoleSnapshot.Render(p, 30, 6);
        var raised = 0;
        p.SplitChanged += (_, _) => raised++;

        p.SplitPosition = 10;   // no-op
        p.SplitPosition = 14;   // real change

        Assert.Equal(1, raised);
        Assert.Equal(14, p.SplitPosition);
    }
    #endregion

    #region Keyboard resize
    [Fact]
    public void FocusedDivider_ArrowKeys_Resize()
    {
        var p = Make(out _, out _, pos: 10);
        ConsoleSnapshot.Render(p, 30, 6);
        p.Divider.Focus();

        Arrow(p, ConsoleKey.RightArrow);
        Assert.Equal(11, p.SplitPosition);

        Arrow(p, ConsoleKey.LeftArrow);
        Assert.Equal(10, p.SplitPosition);
    }

    [Fact]
    public void FocusedDivider_ShiftArrow_LargerStep()
    {
        var p = Make(out _, out _, pos: 10);
        ConsoleSnapshot.Render(p, 40, 6);
        p.Divider.Focus();

        Arrow(p, ConsoleKey.RightArrow, shift: true);

        Assert.Equal(15, p.SplitPosition);   // Shift = step of 5
    }

    [Fact]
    public void VerticalSplit_UpDownArrows_Resize()
    {
        var p = Make(out _, out _, SplitOrientation.Vertical, pos: 4);
        ConsoleSnapshot.Render(p, 24, 16);
        p.Divider.Focus();

        Arrow(p, ConsoleKey.DownArrow);
        Assert.Equal(5, p.SplitPosition);

        Arrow(p, ConsoleKey.UpArrow);
        Assert.Equal(4, p.SplitPosition);
    }
    #endregion

    #region Mouse drag (divider capture)
    [Fact]
    public void DraggingDivider_MovesSplit()
    {
        var p = Make(out _, out _, pos: 10);
        ConsoleSnapshot.Render(p, 40, 6);
        var d = (IMouseListener)p.Divider;

        d.OnMouseDown(new Position(0, 0));
        d.OnMouseMove(new Position(4, 0));   // pointer 4 cells right of where it grabbed
        d.OnMouseUp(new Position(4, 0));

        Assert.Equal(14, p.SplitPosition);   // first pane grew by 4
    }

    [Fact]
    public void DraggingDivider_ClampsAtMin()
    {
        var p = Make(out _, out _, pos: 10);
        ConsoleSnapshot.Render(p, 40, 6);
        var d = (IMouseListener)p.Divider;

        d.OnMouseDown(new Position(0, 0));
        d.OnMouseMove(new Position(-50, 0));   // drag far past the left edge
        d.OnMouseUp(new Position(-50, 0));

        Assert.Equal(p.MinFirst, p.SplitPosition);
    }
    #endregion

    #region Focus routing / nesting
    [Fact]
    public void SurfacesFirstDividerSecond_ForRouting()
    {
        var p = Make(out _, out _);   // horizontal: panes laid out as columns
        Assert.Equal(1, p.Rows);
        Assert.Equal(3, p.Columns);
        Assert.Same(p.First, p[0, 0]);
        Assert.Same(p.Divider, p[0, 1]);
        Assert.Same(p.Second, p[0, 2]);
    }

    [Fact]
    public void FocusedControl_SurfacesFocusedDivider()
    {
        var p = Make(out _, out _);
        Assert.Null(p.FocusedControl);

        p.Divider.Focus();

        Assert.Same(p.Divider, p.FocusedControl);
    }

    [Fact]
    public void Nested_SplitPanels_RenderAllThreePanes()
    {
        var a = new TextLabel(TextLabelOrientation.Horizontal, "AAAA");
        var b = new TextLabel(TextLabelOrientation.Horizontal, "BBBB");
        var c = new TextLabel(TextLabelOrientation.Horizontal, "CCCC");
        var inner = new SplitPanel(SplitOrientation.Vertical, b, c, 3);
        var outer = new SplitPanel(SplitOrientation.Horizontal, a, inner, 8);

        var text = ConsoleSnapshot.ToText(outer, 40, 12);

        Assert.Contains("AAAA", text);
        Assert.Contains("BBBB", text);
        Assert.Contains("CCCC", text);
    }
    #endregion
}
