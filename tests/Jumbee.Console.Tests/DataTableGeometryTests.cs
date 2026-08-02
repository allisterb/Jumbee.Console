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
/// The narrow widths matter: they used to be unfixable by geometry alone, because a squeezed table wrapped its cell
/// content too and rows stopped being one line tall. <see cref="DataTable.DropNarrowColumns"/> removed that by
/// dropping columns instead of wrapping, so the offsets now hold all the way down to a single column.
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
    [InlineData(34)]
    [InlineData(28)]
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
    [InlineData(34)]
    [InlineData(28)]
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
    public void SelectingBeforeTheFirstLayoutStillScrollsTheRowIntoView()
    {
        // Setting the selection at construction time is the normal way to restore a saved cursor, and it happens
        // before the control has any size. There is no geometry to scroll against yet, so the scroll is deferred to
        // the first render — it used to be resolved immediately against a table clamped to one cell wide, which asked
        // Spectre to divide that cell between four columns whose minimums were all 0 (Debug.Assert "Sum or ratios
        // must be > 0"; Release compiled the assert out and carried on with the degenerate measurement).
        var table = new DataTable("Command", "CPU %");
        for (var i = 0; i < 40; i++) table.AddRow($"proc{i}", $"{i}.0");

        table.SelectedIndex = 37;                       // far below any first screenful

        var text = ConsoleSnapshot.ToText(table, 40, 10);
        Assert.Contains("proc37", text);                // scrolled into view on the first render
        Assert.Equal(37, table.SelectedIndex);
    }

    [Theory]
    [InlineData(60, "Command", "CPU %", "Count", "Memory %")]   // everything fits
    [InlineData(38, "Command", "CPU %", "Count")]               // Memory % dropped
    [InlineData(30, "Command", "CPU %")]                        // Count dropped too
    [InlineData(22, "Command")]                                 // only the identifier survives
    public void ColumnsAreDroppedFromTheRightRatherThanWrapped(int width, params string[] expected)
    {
        var text = ConsoleSnapshot.ToText(Table(), width, 12);
        var header = text.Split('\n').First(l => l.Contains("Command"));

        foreach (var column in expected) Assert.Contains(column, header);

        // Anything not expected must be gone entirely, not wrapped onto another line.
        foreach (var dropped in new[] { "Command", "CPU %", "Count", "Memory %" }.Except(expected))
            Assert.DoesNotContain(dropped, text);
    }

    [Fact]
    public void NothingWrapsAtAnyWidth()
    {
        // Wrapping shows up as extra lines: a table of N rows is always top border + header + separator + N + bottom.
        // Any header or value split across lines makes it taller, which is also what breaks the row offsets.
        for (var width = 22; width <= 60; width += 2)
        {
            var lines = ConsoleSnapshot.ToText(Table(), width, 12)
                .Split('\n').Count(l => l.TrimEnd().Length > 0);

            Assert.True(lines == 4 + 4, $"width {width} rendered {lines} lines for 4 rows — something wrapped");
        }
    }

    [Fact]
    public void DroppingCanBeTurnedOff()
    {
        // Column count is visible in the top border's separators (┬), which survives wrapping.
        static int ColumnsIn(string text) => text.Split('\n')[0].Count(c => c == '┬') + 1;

        var dropping = ConsoleSnapshot.ToText(Table(), 30, 12);

        var kept = Table();
        kept.DropNarrowColumns = false;
        var wrapping = ConsoleSnapshot.ToText(kept, 30, 12);

        Assert.Equal(4, ColumnsIn(wrapping));                          // all columns kept...
        Assert.True(ColumnsIn(dropping) < ColumnsIn(wrapping),         // ...versus dropped by default
            $"expected fewer columns with dropping on: {ColumnsIn(dropping)} vs {ColumnsIn(wrapping)}");
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
