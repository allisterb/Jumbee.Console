namespace Jumbee.Console.Tests;

using Jumbee.Console;
using Jumbee.Console.Snapshot;

using Xunit;

/// <summary>
/// Tests for headless pointer simulation (<see cref="ConsoleSnapshot.Click"/>, <see cref="ConsoleSnapshot.MouseMove"/>,
/// <see cref="ConsoleSnapshot.Wheel"/>) — the mouse counterpart to the key-driven <c>ToTextAfter</c> path.
/// </summary>
public class SnapshotMouseTests : System.IDisposable
{
    // Hover state is static (as it is at runtime), so clear it between tests.
    public void Dispose() => ConsoleSnapshot.ResetMouse();

    private static DataTable Table()
    {
        var t = new DataTable("Command", "CPU %");
        t.AddRow("node", "11.4");
        t.AddRow("firefox", "9.3");
        t.AddRow("gnome-shell", "2.1");
        return t;
    }

    [Fact]
    public void Click_SelectsTheRowUnderThePointer()
    {
        var table = Table();
        var buffer = ConsoleSnapshot.Render(table, 40, 10);
        Assert.Equal(0, table.SelectedIndex);

        // The table draws a full box: y=0 top border, y=1 header, y=2 separator, then data from y=3.
        Assert.True(ConsoleSnapshot.Click(buffer, 2, 5));

        Assert.Equal(2, table.SelectedIndex);
        Assert.Equal("gnome-shell", table.SelectedRow?[0]);
    }

    [Fact]
    public void DoubleClick_ActivatesTheRow()
    {
        var table = Table();
        var activated = -1;
        table.RowActivated += (_, i) => activated = i;

        var buffer = ConsoleSnapshot.Render(table, 40, 10);
        ConsoleSnapshot.Click(buffer, 2, 4, clicks: 2);   // second data row

        Assert.Equal(1, activated);
    }

    [Fact]
    public void MouseMove_FiresEnterAndLeaveAcrossControls()
    {
        var a = new Button("A");
        var b = new Button("B");
        var layout = new Grid([1, 1], [20], [[a], [b]]);
        var buffer = ConsoleSnapshot.Render(layout, 20, 2);

        int aEnter = 0, aLeave = 0, bEnter = 0;
        a.MouseEntered += (_, _) => aEnter++;
        a.MouseLeft += (_, _) => aLeave++;
        b.MouseEntered += (_, _) => bEnter++;

        ConsoleSnapshot.MouseMove(buffer, 1, 0);
        Assert.Equal(1, aEnter);
        Assert.Equal(0, aLeave);
        Assert.Equal(0, bEnter);

        ConsoleSnapshot.MouseMove(buffer, 1, 1);
        Assert.Equal(1, aLeave);   // left when the pointer moved off
        Assert.Equal(1, bEnter);
    }

    [Fact]
    public void Click_ActivatesAButton()
    {
        var activated = false;
        var button = new Button("Press me");
        button.Activated += (_, _) => activated = true;

        var buffer = ConsoleSnapshot.Render(button, 20, 1);
        Assert.True(ConsoleSnapshot.Click(buffer, 1, 0));

        Assert.True(activated);
    }

    [Fact]
    public void Wheel_ScrollsAControlThatOptsIn()
    {
        var log = new Log();
        for (var i = 0; i < 200; i++) log.Write($"line {i}");

        var before = ConsoleSnapshot.ToText(ConsoleSnapshot.Render(log, 30, 10));

        var buffer = ConsoleSnapshot.Render(log, 30, 10);
        Assert.True(ConsoleSnapshot.Wheel(buffer, 5, 5, -3));   // negative = up

        Assert.NotEqual(before, ConsoleSnapshot.ToText(ConsoleSnapshot.Render(log, 30, 10)));
    }

    [Fact]
    public void Click_ReturnsFalseWhereNothingOptsIntoTheMouse()
    {
        var table = Table();
        var buffer = ConsoleSnapshot.Render(table, 40, 10);

        // Well outside the rendered control — nothing to hit.
        Assert.False(ConsoleSnapshot.Click(buffer, 200, 200));
    }

    [Fact]
    public void ToTextAfterClick_RendersTheResultOfTheClick()
    {
        var table = Table();
        var text = ConsoleSnapshot.ToTextAfterClick(table, 40, 10, 2, 5);

        Assert.Contains("gnome-shell", text);
        Assert.Equal(2, table.SelectedIndex);
    }
}
