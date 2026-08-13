namespace Jumbee.Console.Tests;

using ConsoleGUI.Input;
using ConsoleGUI.Space;

using Jumbee.Console;
using Jumbee.Console.Snapshot;

using Xunit;

public class SelectTests
{
    #region ListBox option-list behavior
    [Fact]
    public void ListBox_SelectedIndex_ClampsAndRaisesSelectionChanged()
    {
        var lb = new ListBox("A", "B", "C");
        var changed = -1;
        lb.SelectionChanged += (_, i) => changed = i;

        lb.SelectedIndex = 2;
        Assert.Equal(2, lb.SelectedIndex);
        Assert.Equal(2, changed);
        Assert.Equal("C", lb.SelectedItem?.Text);

        lb.SelectedIndex = 99;          // clamps to last
        Assert.Equal(2, lb.SelectedIndex);
    }

    [Fact]
    public void ListBox_Enter_CommitsSelectedItem()
    {
        var lb = new ListBox("A", "B", "C") { SelectedIndex = 1 };
        ListBox.ListBoxItem? committed = null;
        lb.Committed += (_, item) => committed = item;

        UI.SendInput(lb, ConsoleKey.Enter);

        Assert.Equal("B", committed?.Text);
    }

    [Fact]
    public void ListBox_Escape_RaisesCancelled()
    {
        var lb = new ListBox("A", "B");
        var cancelled = false;
        lb.Cancelled += (_, _) => cancelled = true;

        UI.SendInput(lb, ConsoleKey.Escape);

        Assert.True(cancelled);
    }

    [Fact]
    public void ListBox_Click_SelectsRowAndCommits()
    {
        var lb = new ListBox("A", "B", "C");
        ListBox.ListBoxItem? committed = null;
        lb.Committed += (_, item) => committed = item;

        var m = (IMouseListener)lb;
        m.OnMouseDown(new Position(0, 1));
        m.OnMouseUp(new Position(0, 1));   // row 1 -> "B"

        Assert.Equal(1, lb.SelectedIndex);
        Assert.Equal("B", committed?.Text);
    }
    #endregion

    #region Select
    // Builds the host overlay and registers it as the ambient UI.Overlay the dropdown shows into (these headless
    // tests don't start the UI loop, so they set UI.Overlay directly).
    private static Overlay HostOverlay()
    {
        var overlay = new Overlay(new Grid([1], [10], [[new Button("host")]]));
        UI.Overlay = overlay;
        return overlay;
    }

    [Fact]
    public void Select_Closed_RendersPlaceholderAndArrow()
    {
        var select = new Select("Red", "Green", "Blue");

        var text = ConsoleSnapshot.ToText(select, 20, 1);

        Assert.Contains("Select", text);   // placeholder
        Assert.Contains("▼", text);
    }

    [Fact]
    public void Select_Open_ShowsDropdownWithOptions()
    {
        var overlay = HostOverlay();
        var select = new Select("Red", "Green", "Blue");

        select.Open();

        Assert.True(overlay.IsShowing);
        Assert.IsType<ListBox>(overlay.Top);
        var dropdown = (ListBox)overlay.Top!;
        var text = ConsoleSnapshot.ToText(dropdown, 20, 8);
        Assert.Contains("Green", text);
    }

    [Fact]
    public void Select_CommitFromDropdown_SetsValueClosesAndRaisesChange()
    {
        var overlay = HostOverlay();
        var select = new Select("Red", "Green", "Blue");
        string? changed = null;
        select.SelectionChanged += (_, v) => changed = v;

        select.Open();
        var dropdown = (ListBox)overlay.Top!;
        dropdown.SelectedIndex = 1;                 // Green
        UI.SendInput(dropdown, ConsoleKey.Enter);

        Assert.Equal("Green", select.SelectedValue);
        Assert.Equal("Green", changed);
        Assert.False(overlay.IsShowing);            // closed after commit
    }

    [Fact]
    public void Select_EscapeInDropdown_ClosesWithoutChanging()
    {
        var overlay = HostOverlay();
        var select = new Select("Red", "Green", "Blue");

        select.Open();
        UI.SendInput((ListBox)overlay.Top!, ConsoleKey.Escape);

        Assert.False(overlay.IsShowing);
        Assert.Null(select.SelectedValue);
    }

    [Fact]
    public void Select_Open_WithoutOverlayHost_IsNoOp()
    {
        UI.Overlay = null;   // no ambient overlay (e.g. before UI.Start)
        var select = new Select("Red", "Green");

        select.Open();

        Assert.Null(select.SelectedValue);
    }
    #endregion

    #region Width
    private static int PaintedCells(ConsoleBuffer buffer, int width)
    {
        var n = 0;
        for (var x = 0; x < width; x++)
            if (ConsoleSnapshot.BackgroundAt(buffer, x, 0) is not null) n++;
        return n;
    }

    [Fact]
    public void ByDefault_TheClosedControlFillsTheWidthOffered()
    {
        UiTestHarness.EnsureStopped();
        var select = new Select("wireframe", "solid", "shaded") { SelectedIndex = 0 };
        var buffer = ConsoleSnapshot.Render(new Grid([1], [30], [[select]]), 30, 1);

        Assert.Equal(30, PaintedCells(buffer, 30));
    }

    [Fact]
    public void FitContent_SizesTheClosedControlToItsWidestOption()
    {
        UiTestHarness.EnsureStopped();
        var select = new Select("wireframe", "solid", "shaded") { SelectedIndex = 0, FitContent = true };
        var buffer = ConsoleSnapshot.Render(new Grid([1], [30], [[select]]), 30, 1);

        var painted = PaintedCells(buffer, 30);
        Assert.InRange(painted, 10, 14);            // "wireframe" + a space + the arrow
        Assert.Contains("wireframe", ConsoleSnapshot.ToText(buffer));
    }

    // Regression: the themed focus cue fills every UNPAINTED cell of a control, which for a fitted Select is the
    // whole rest of the row — so choosing an option, which hands focus back to the control, made it look like it
    // had sprung back to full width. It now draws focus inside its own box.
    [Fact]
    public void FitContent_KeepsItsWidthWhenFocused()
    {
        UiTestHarness.EnsureStopped();
        var select = new Select("wireframe", "solid", "shaded") { SelectedIndex = 0, FitContent = true };
        var grid = new Grid([1], [30], [[select]]);

        var unfocused = PaintedCells(ConsoleSnapshot.Render(grid, 30, 1), 30);
        UI.SetFocus(select);
        var focused = ConsoleSnapshot.Render(grid, 30, 1);

        Assert.True(select.IsFocused);
        Assert.Equal(unfocused, PaintedCells(focused, 30));

        // Still visibly focused, just inside the box: the fill is lifted toward white.
        Assert.True(ConsoleSnapshot.BackgroundAt(focused, 0, 0)?.R > select.Background.R,
            "focus is no longer visible on a fitted Select");
    }

    [Fact]
    public void ADefaultSelect_StillTintsTheWholeRowOnFocus()
    {
        UiTestHarness.EnsureStopped();
        var select = new Select("wireframe", "solid") { SelectedIndex = 0 };
        var grid = new Grid([1], [30], [[select]]);
        ConsoleSnapshot.Render(grid, 30, 1);

        UI.SetFocus(select);
        Assert.Equal(30, PaintedCells(ConsoleSnapshot.Render(grid, 30, 1), 30));
    }
    #endregion
}
