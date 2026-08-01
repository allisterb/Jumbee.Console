namespace Jumbee.Console.Tests;

using System.Linq;

using Jumbee.Console;
using Jumbee.Console.Snapshot;

using Xunit;

/// <summary>
/// Tests that <see cref="DataTable"/>'s drawn geometry agrees with its model at any width — the selection bar must
/// land on the selected row, and a click must map back to the row the user pointed at.
/// </summary>
/// <remarks>
/// Regression cover for the header-wrap bug: chrome height was estimated from a probe table filled with placeholder
/// cells, but Spectre allocates column widths from cell content, so the probe's header wrapped to a different number
/// of lines than the real table's. The highlight drifted up one row per extra header line as the control narrowed —
/// invisibly to any test that only asserted <c>SelectedIndex</c>.
/// <para>
/// Widths here stay at or above 40 for this four-column table, which is where cells still fit their columns. Below
/// roughly that, Spectre wraps the cell content too (despite <c>NoWrap</c>), rows stop being one line tall and the
/// offsets drift again — a known limit noted in <c>DataTable.Render</c>, whose real fix is a drop-columns-when-narrow
/// policy rather than more geometry arithmetic.
/// </para>
/// </remarks>
public class DataTableGeometryTests
{
    private static DataTable Table()
    {
        var t = new DataTable("Command", "CPU %", "Count", "Memory %");
        t.AddRow("node", "11.4", "1", "2.2");
        t.AddRow("firefox", "9.3", "1", "6.6");
        t.AddRow("Xorg", "2.2", "1", "3.6");
        t.AddRow("gnome-shell", "2.1", "1", "8.6");
        return t;
    }

    // The rendered row carrying the full-width selection background, or -1.
    private static int HighlightedRow(ConsoleBuffer buffer, int width, int height)
    {
        var best = -1;
        var bestCount = 0;
        for (var y = 0; y < height; y++)
        {
            var n = 0;
            for (var x = 0; x < width; x++) if (buffer[x, y].Character.Background is not null) n++;
            if (n > bestCount) { bestCount = n; best = y; }
        }
        return bestCount > width / 2 ? best : -1;   // a real bar spans the row, not a stray styled cell
    }

    [Theory]
    [InlineData(60)]
    [InlineData(50)]
    [InlineData(40)]
    public void SelectionBarLandsOnTheSelectedRow(int width)
    {
        var table = Table();
        table.SelectedIndex = 3;                       // gnome-shell

        var buffer = ConsoleSnapshot.Render(table, width, 12);
        var row = HighlightedRow(buffer, width, 12);
        Assert.True(row >= 0, "no selection bar was drawn");

        // "gnome" rather than the full name: at the narrow end the cell is truncated to fit its column.
        var line = ConsoleSnapshot.ToText(buffer).Split('\n')[row];
        Assert.Contains("gnome", line);
    }

    [Theory]
    [InlineData(60)]
    [InlineData(50)]
    [InlineData(40)]
    public void ClickingTheHighlightedRowKeepsTheSameSelection(int width)
    {
        var table = Table();
        table.SelectedIndex = 3;

        var buffer = ConsoleSnapshot.Render(table, width, 12);
        var row = HighlightedRow(buffer, width, 12);

        // Clicking the row the user can see as selected must not move the selection somewhere else.
        Assert.True(ConsoleSnapshot.Click(buffer, 2, row));
        Assert.Equal(3, table.SelectedIndex);
        ConsoleSnapshot.ResetMouse();
    }

    [Fact]
    public void EveryRowIsClickableToItsOwnIndex()
    {
        const int width = 40;
        var table = Table();
        var buffer = ConsoleSnapshot.Render(table, width, 12);

        // Walk down from the first data row: each successive line selects the next row.
        table.SelectedIndex = 0;
        var first = HighlightedRow(buffer, width, 12);

        for (var i = 0; i < 4; i++)
        {
            ConsoleSnapshot.ResetMouse();
            Assert.True(ConsoleSnapshot.Click(buffer, 2, first + i));
            Assert.Equal(i, table.SelectedIndex);
        }
        ConsoleSnapshot.ResetMouse();
    }
}
