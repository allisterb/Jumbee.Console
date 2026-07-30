namespace Jumbee.Console.Tests;

using Jumbee.Console.Snapshot;

using Xunit;

/// <summary>
/// A framed control placed in a stack panel must render its frame. The stacks used to add the raw control to the
/// underlying ConsoleGUI panel instead of its <see cref="Control.FocusableControl"/> (the frame when framed), so a
/// border/margin/scrollbar/title was silently dropped — unlike Grid/DockPanel, which bind the frame. This guards the
/// fix (and that unframed controls are unaffected, since FocusableControl == the control when there is no frame).
/// </summary>
public class StackPanelFrameTests
{
    [Fact]
    public void VerticalStack_RendersChildBorder()
    {
        var panel = new VerticalStackPanel(
            new ProgressBar("a", 50).WithBorder(BorderStyle.Rounded),
            new ProgressBar("b", 50));
        var text = ConsoleSnapshot.ToText(ConsoleSnapshot.Render(panel, 30, 8));
        Assert.Contains("╭", text);   // the framed child's top-left corner now renders
        Assert.Contains("╰", text);
    }

    [Fact]
    public void HorizontalStack_RendersChildBorder_ViaAdd()
    {
        // HorizontalStackPanel.Add (not just its ctor) must bind the frame too — it used to add the raw control.
        var panel = new HorizontalStackPanel();
        panel.Add(new ProgressBar("x", 50).WithBorder(BorderStyle.Rounded));
        var text = ConsoleSnapshot.ToText(ConsoleSnapshot.Render(panel, 30, 3));
        Assert.Contains("╭", text);
    }

    [Fact]
    public void VerticalStack_MarginReservesWidth()
    {
        // A right margin of 8 narrows the framed child's content, leaving the rightmost 8 columns blank. Scan the
        // whole buffer (the frame's border reserves a row, so the bar isn't on row 0) rather than a fixed row.
        var panel = new VerticalStackPanel(
            new ProgressBar(value: 100) { ShowPercentage = false }.WithFill(Style.Red).WithMargin(0, 0, 8, 0));
        var buf = ConsoleSnapshot.Render(panel, 30, 4);
        var fill = Style.Red.ForegroundColor!.Value;
        int total = 0, inRightMargin = 0;
        for (var y = 0; y < 4; y++)
            for (var x = 0; x < 30; x++)
                if (buf[x, y].Background is { } bg && bg.Equals(fill)) { total++; if (x >= 30 - 8) inRightMargin++; }
        Assert.True(total > 0, "the bar still renders");
        Assert.Equal(0, inRightMargin);   // nothing filled in the reserved right-margin columns
    }

    [Fact]
    public void VerticalStack_UnframedControl_FillsFullWidth()
    {
        // Regression guard: the fix must not change unframed controls (FocusableControl == the control).
        var buf = ConsoleSnapshot.Render(
            new VerticalStackPanel(new ProgressBar(value: 100) { ShowPercentage = false }.WithFill(Style.Red)), 20, 1);
        var fill = Style.Red.ForegroundColor!.Value;
        Assert.True(buf[0, 0].Background is { } b0 && b0.Equals(fill));
        Assert.True(buf[19, 0].Background is { } b19 && b19.Equals(fill));   // reaches the far edge
    }
}
