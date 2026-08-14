namespace Jumbee.Console.Tests;

using ConsoleGUI.Input;

using Jumbee.Console.Snapshot;

using Xunit;

/// <summary>Headless tests for <see cref="MenuBar"/>: title rendering, opening a menu, and relaying activation.</summary>
public class MenuBarTests
{
    // These drive the ambient UI.Overlay, which is global: a UI loop still winding down from another test class can
    // leave it pointing elsewhere, so start from a stopped state. (xUnit builds a fresh instance per test.)
    public MenuBarTests() => UiTestHarness.EnsureStopped();

    private static void SendKey(Control c, ConsoleKey k)
        => ((Control)c).OnInput(new UI.InputEventArgs(new InputEvent(new ConsoleKeyInfo('\0', k, false, false, false))));

    private static (MenuBar bar, Overlay overlay) Build()
    {
        var bar = new MenuBar()
            .Add("File", new MenuItem("New"), new MenuItem("Open"), MenuItem.Separator, new MenuItem("Quit"))
            .Add("Edit", new MenuItem("Cut"), new MenuItem("Copy"), new MenuItem("Paste"));
        var overlay = new Overlay(new Grid([1], [40], [[bar]]));
        UI.Overlay = overlay;   // ambient host the drop-downs float into (headless: no UI.Start)
        return (bar, overlay);
    }

    [Fact]
    public void Renders_Titles()
    {
        var (_, overlay) = Build();
        var text = ConsoleSnapshot.ToText(overlay, 40, 12);
        Assert.Contains("File", text);
        Assert.Contains("Edit", text);
    }

    [Fact]
    public void OpenActive_ShowsMenu()
    {
        var (bar, overlay) = Build();
        ConsoleSnapshot.Render(overlay, 40, 12);   // size the bar first
        bar.OpenActive();                          // opens "File" (active index 0)

        Assert.True(overlay.IsShowing);
        var text = ConsoleSnapshot.ToText(overlay, 40, 12);
        Assert.Contains("New", text);
        Assert.Contains("Quit", text);
    }

    [Fact]
    public void RightThenOpen_OpensSecondMenu()
    {
        var (bar, overlay) = Build();
        ConsoleSnapshot.Render(overlay, 40, 12);
        SendKey(bar, ConsoleKey.RightArrow);   // move active File -> Edit
        bar.OpenActive();

        var text = ConsoleSnapshot.ToText(overlay, 40, 12);
        Assert.Contains("Copy", text);
        Assert.DoesNotContain("Quit", text);   // the File menu is not the one open
    }

    [Fact]
    public void ItemActivated_IsRelayed_FromOpenMenu()
    {
        var (bar, overlay) = Build();
        MenuItem? chosen = null;
        bar.ItemActivated += (_, it) => chosen = it;
        ConsoleSnapshot.Render(overlay, 40, 12);
        bar.OpenActive();

        var menu = (ContextMenu)overlay.Top!;
        SendKey(menu, ConsoleKey.Enter);   // activates "New"

        Assert.Equal("New", chosen?.Text);
        Assert.False(overlay.IsShowing);   // menu closed after choosing
    }

    [Fact]
    public void Menu_ClosesOnActivation_ResettingOpenState()
    {
        var (bar, overlay) = Build();
        ConsoleSnapshot.Render(overlay, 40, 12);
        bar.OpenActive();
        var menu = (ContextMenu)overlay.Top!;
        SendKey(menu, ConsoleKey.Enter);

        Assert.False(overlay.IsShowing);
    }

    // MenuItem is immutable, so a menu that reports state has to be rebuilt; the Func overload is what makes that
    // possible without recreating the bar.
    [Fact]
    public void DynamicMenu_RebuildsItsItemsEachTimeItOpens()
    {
        var mode = "wireframe";
        var bar = new MenuBar().Add("Render", () =>
            [
                new MenuItem("wireframe") { Checked = mode == "wireframe" },
                new MenuItem("solid") { Checked = mode == "solid" },
            ]);
        var overlay = new Overlay(new Grid([1], [40], [[bar]]));
        UI.Overlay = overlay;
        ConsoleSnapshot.Render(overlay, 40, 12);

        bar.OpenActive();
        var first = ConsoleSnapshot.ToLines(ConsoleSnapshot.Render(overlay, 40, 12));
        Assert.Contains(first, l => l.Contains('✓') && l.Contains("wireframe"));
        Assert.DoesNotContain(first, l => l.Contains('✓') && l.Contains("solid"));

        overlay.Hide();
        mode = "solid";
        bar.OpenActive();
        var second = ConsoleSnapshot.ToLines(ConsoleSnapshot.Render(overlay, 40, 12));
        Assert.Contains(second, l => l.Contains('✓') && l.Contains("solid"));
        Assert.DoesNotContain(second, l => l.Contains('✓') && l.Contains("wireframe"));
    }

    // A level that has any checkable item reserves the marker column on every row, so the labels of plain commands
    // in the same menu line up with the checkable ones rather than sitting a column to their left.
    [Fact]
    public void CheckColumn_AlignsEveryRowInTheLevel()
    {
        var bar = new MenuBar().Add("View",
            new MenuItem("Zoom in"),
            new MenuItem("Show grid") { Checked = true },
            new MenuItem("Show axes") { Checked = false });
        var overlay = new Overlay(new Grid([1], [40], [[bar]]));
        UI.Overlay = overlay;
        ConsoleSnapshot.Render(overlay, 40, 12);
        bar.OpenActive();

        var lines = ConsoleSnapshot.ToLines(ConsoleSnapshot.Render(overlay, 40, 12));
        var zoom = lines.Single(l => l.Contains("Zoom in"));
        var grid = lines.Single(l => l.Contains("Show grid"));
        var axes = lines.Single(l => l.Contains("Show axes"));

        Assert.Equal(grid.IndexOf("Show grid", StringComparison.Ordinal), axes.IndexOf("Show axes", StringComparison.Ordinal));
        Assert.Equal(grid.IndexOf("Show grid", StringComparison.Ordinal), zoom.IndexOf("Zoom in", StringComparison.Ordinal));
        Assert.Contains('✓', grid);
        Assert.DoesNotContain('✓', axes);
    }

    // Nothing checkable: no column reserved, so an ordinary menu is not silently widened by the feature.
    [Fact]
    public void CheckColumn_IsAbsentWhenNothingIsCheckable()
    {
        var (bar, overlay) = Build();
        ConsoleSnapshot.Render(overlay, 40, 12);
        bar.OpenActive();

        var lines = ConsoleSnapshot.ToLines(ConsoleSnapshot.Render(overlay, 40, 12));
        var open = lines.Single(l => l.Contains("Open"));
        Assert.Equal(2, open.IndexOf("Open", StringComparison.Ordinal));   // border + the single leading space
    }
}
