namespace Jumbee.Console.Tests;

using System;
using System.Collections.Generic;

using ConsoleGUI.Input;
using ConsoleGUI.Space;

using Jumbee.Console;
using Jumbee.Console.Snapshot;

using Xunit;

public class TabPanelTests
{
    // Two tabs with distinctive content so snapshots can tell which one is shown.
    private static TabPanel TwoTabs(out TextLabel a, out TextLabel b, TabBarDock dock = TabBarDock.Top)
    {
        a = new TextLabel(TextLabelOrientation.Horizontal, "AAAA");
        b = new TextLabel(TextLabelOrientation.Horizontal, "BBBB");
        return new TabPanel(dock, ("One", a), ("Two", b));
    }

    private static void Click(TabHeader header) => ClickAt(header, 0);

    private static void ClickAt(TabHeader header, int x)
    {
        var m = (IMouseListener)header;
        m.OnMouseDown(new Position(x, 0));
        m.OnMouseUp(new Position(x, 0));
    }

    #region Rendering / selection model
    [Fact]
    public void RendersHeaders_AndShowsFirstTabByDefault()
    {
        var panel = TwoTabs(out _, out _);

        var text = ConsoleSnapshot.ToText(panel, 24, 6);

        Assert.Contains("One", text);          // both tab labels in the bar
        Assert.Contains("Two", text);
        Assert.Contains("AAAA", text);         // first tab is selected by default
        Assert.DoesNotContain("BBBB", text);   // the inactive tab's content is not shown
        Assert.Equal(0, panel.SelectedIndex);
        Assert.Equal("One", panel.ActiveTabName);
    }

    [Fact]
    public void SelectedIndex_SwapsContent_MarksActive_AndRaisesEvent()
    {
        var panel = TwoTabs(out _, out _);
        int? changed = null;
        panel.SelectionChanged += (_, i) => changed = i;

        panel.SelectedIndex = 1;

        Assert.Equal(1, changed);
        Assert.True(panel.Headers[1].IsActive);
        Assert.False(panel.Headers[0].IsActive);
        var text = ConsoleSnapshot.ToText(panel, 24, 6);
        Assert.Contains("BBBB", text);
        Assert.DoesNotContain("AAAA", text);
    }

    [Fact]
    public void SelectedIndex_IsClamped_AndNoOpDoesNotRaise()
    {
        var panel = TwoTabs(out _, out _);
        var raised = 0;
        panel.SelectionChanged += (_, _) => raised++;

        panel.SelectedIndex = 0;     // already selected -> no event
        panel.SelectedIndex = 99;    // clamps to the last tab

        Assert.Equal(1, panel.SelectedIndex);
        Assert.Equal(1, raised);     // only the real change fired
    }
    #endregion

    #region Mouse
    [Fact]
    public void ClickingHeader_SelectsThatTab()
    {
        var panel = TwoTabs(out _, out _);
        ConsoleSnapshot.Render(panel, 24, 6);   // establish layout

        Click(panel.Headers[1]);

        Assert.Equal(1, panel.SelectedIndex);
        Assert.True(panel.Headers[1].IsActive);
        Assert.False(panel.Headers[0].IsActive);
    }
    #endregion

    #region Closable tabs / "+" button
    // The close glyph's column within a header: leading space + label + separating space (no caret gutter under the
    // default Highlight selection style). Mirrors TabHeader.CloseGlyphSpan.
    private static int CloseColumn(string label) => label.Length + 2;

    [Fact]
    public void ClosableTab_ShowsCloseGlyph_OnlyOnActiveTab()
    {
        var panel = TwoTabs(out _, out _);
        panel.ClosableTabs = true;

        var text = ConsoleSnapshot.ToText(panel, 30, 6);

        // The active tab (index 0) shows the ✕; the inactive one reserves a blank slot instead — so exactly one ✕.
        Assert.Equal(1, text.Split('✕').Length - 1);
    }

    [Fact]
    public void ClickingCloseGlyph_OnActiveTab_RaisesRequest_AndRemovesByDefault()
    {
        var panel = TwoTabs(out _, out _);
        panel.ClosableTabs = true;
        ConsoleSnapshot.Render(panel, 30, 6);   // establish layout; tab 0 ("One") is active
        TabItem? closed = null;
        panel.TabCloseRequested += (_, e) => closed = e.Tab;

        ClickAt(panel.Headers[0], CloseColumn("One"));

        Assert.NotNull(closed);                   // the close request fired for the clicked tab
        Assert.Equal(1, panel.TabCount);          // removed by default (no cancel)
        Assert.Equal("Two", panel.ActiveTabName); // selection moved to the survivor
    }

    [Fact]
    public void CancelingClose_KeepsTheTab()
    {
        var panel = TwoTabs(out _, out _);
        panel.ClosableTabs = true;
        ConsoleSnapshot.Render(panel, 30, 6);
        panel.TabCloseRequested += (_, e) => e.Cancel = true;

        ClickAt(panel.Headers[0], CloseColumn("One"));

        Assert.Equal(2, panel.TabCount);          // vetoed -> nothing removed
        Assert.Equal("One", panel.ActiveTabName);
    }

    [Fact]
    public void ClickingLabel_OnClosableTab_SelectsInsteadOfClosing()
    {
        var panel = TwoTabs(out _, out _);
        panel.ClosableTabs = true;
        ConsoleSnapshot.Render(panel, 30, 6);
        var closeRaised = 0;
        panel.TabCloseRequested += (_, _) => closeRaised++;

        ClickAt(panel.Headers[1], 1);   // click the label area (not the ✕ column) of the inactive tab

        Assert.Equal(0, closeRaised);
        Assert.Equal(2, panel.TabCount);
        Assert.Equal(1, panel.SelectedIndex);   // it selected
    }

    [Fact]
    public void ShowAddButton_RendersPlusGlyph()
    {
        var panel = TwoTabs(out _, out _);
        panel.ShowAddButton = true;

        var text = ConsoleSnapshot.ToText(panel, 30, 6);

        Assert.Contains("+", text);   // the "+" affordance is in the bar
    }

    [Fact]
    public void ClickingAddButton_RaisesNewTabRequested()
    {
        var panel = TwoTabs(out _, out _);
        panel.ShowAddButton = true;   // builds the button
        var raised = 0;
        panel.NewTabRequested += (_, _) => raised++;

        var button = panel.AddButton;   // internal, visible via InternalsVisibleTo
        Assert.NotNull(button);
        var m = (IMouseListener)button!;
        m.OnMouseDown(new Position(0, 0));
        m.OnMouseUp(new Position(0, 0));

        Assert.Equal(1, raised);
    }

    [Fact]
    public void ArrowPastLastTab_FocusesAddButton_EnterActivates_BackArrowReturns()
    {
        var panel = TwoTabs(out _, out _);
        panel.ShowAddButton = true;
        ConsoleSnapshot.Render(panel, 30, 6);
        panel.SelectedIndex = 1;         // last tab selected
        panel.Headers[1].Focus();        // focus on the last header

        // Right past the last tab lands on the "+" button.
        UI.SendInput(panel, new ConsoleKeyInfo('\0', ConsoleKey.RightArrow, false, false, false));
        Assert.True(panel.AddButton!.IsFocused);
        Assert.False(panel.Headers[1].IsFocused);

        // Enter activates it (the tunnel drives it — it isn't in the panel's logical rows).
        var raised = 0;
        panel.NewTabRequested += (_, _) => raised++;
        UI.SendInput(panel, new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false));
        Assert.Equal(1, raised);

        // Left returns to the last tab.
        UI.SendInput(panel, new ConsoleKeyInfo('\0', ConsoleKey.LeftArrow, false, false, false));
        Assert.True(panel.Headers[1].IsFocused);
        Assert.False(panel.AddButton!.IsFocused);
    }
    #endregion

    #region Per-tab closable
    [Fact]
    public void PinnedTab_ShowsNoCloseGlyph_EvenWhenActive()
    {
        var panel = TwoTabs(out _, out _);
        panel.ClosableTabs = true;        // panel default: closable
        panel.Tabs[0].Closable = false;   // but pin the (active) first tab

        var text = ConsoleSnapshot.ToText(panel, 40, 6);

        Assert.DoesNotContain("✕", text);   // active tab pinned -> no ✕; the other is inactive (blank slot)
    }

    [Fact]
    public void PerTab_Closable_CanEnableOnASingleTab_WhenPanelDefaultIsOff()
    {
        var panel = TwoTabs(out _, out _);   // ClosableTabs defaults false
        panel.Tabs[0].Closable = true;       // only tab 0 (active) closable

        var text = ConsoleSnapshot.ToText(panel, 40, 6);

        Assert.Equal(1, text.Split('✕').Length - 1);   // exactly the one active closable tab shows the ✕
    }
    #endregion

    #region Keyboard
    [Fact]
    public void AltLeftRight_SwitchTabs_OnHorizontalBar()
    {
        var panel = TwoTabs(out _, out _);
        ConsoleSnapshot.Render(panel, 24, 6);

        // The panel intercepts Alt+arrows in its tunnel (no header focus needed).
        UI.SendInput(panel, UI.HotKeys.AltRight);
        Assert.Equal(1, panel.SelectedIndex);

        UI.SendInput(panel, UI.HotKeys.AltLeft);
        Assert.Equal(0, panel.SelectedIndex);
    }

    [Fact]
    public void AltUpDown_SwitchTabs_OnVerticalBar()
    {
        var panel = TwoTabs(out _, out _, TabBarDock.Left);
        ConsoleSnapshot.Render(panel, 24, 6);

        UI.SendInput(panel, UI.HotKeys.AltDown);
        Assert.Equal(1, panel.SelectedIndex);

        UI.SendInput(panel, UI.HotKeys.AltUp);
        Assert.Equal(0, panel.SelectedIndex);
    }

    [Fact]
    public void PlainArrow_ReachesFocusedContent_NestedInTab()
    {
        // Regression guard for "ListBox arrows don't work in a tab": a plain arrow must route grid -> tabpanel ->
        // the focused content (the panel's Alt-arrow tunnel must NOT swallow unmodified arrows).
        var list = new ListBox();
        list.AddItem("one"); list.AddItem("two"); list.AddItem("three");
        var panel = new TabPanel(TabBarDock.Top, ("L", list), ("X", new TextLabel(TextLabelOrientation.Horizontal, "x")));
        var grid = new Grid([6], [20], [[panel]]);
        ConsoleSnapshot.Render(grid, 20, 6);
        list.Focus();
        Assert.Equal(0, list.SelectedIndex);

        UI.SendInput(grid, new ConsoleKeyInfo('\0', ConsoleKey.DownArrow, false, false, false));

        Assert.Equal(1, list.SelectedIndex);   // the arrow reached the ListBox and moved its selection
    }

    [Fact]
    public void SwitchingTab_FocusesNewContent_WhenPanelHasFocus()
    {
        var listA = new ListBox();
        var listB = new ListBox();
        listA.AddItem("a"); listB.AddItem("b");
        var panel = new TabPanel(TabBarDock.Top, ("A", listA), ("B", listB));
        ConsoleSnapshot.Render(panel, 20, 6);
        panel.Headers[0].Focus();   // focus is inside the panel (on a header)

        UI.SendInput(panel, UI.HotKeys.AltRight);

        Assert.Equal(1, panel.SelectedIndex);
        Assert.True(listB.IsFocused);    // focus followed into the newly active tab's content
        Assert.False(listA.IsFocused);
    }

    [Fact]
    public void SwitchingToNonInteractiveContent_FocusesTheHeader()
    {
        var list = new ListBox();
        var label = new TextLabel(TextLabelOrientation.Horizontal, "info");   // not interactive
        var panel = new TabPanel(TabBarDock.Top, ("List", list), ("Info", label));
        ConsoleSnapshot.Render(panel, 20, 6);
        panel.Headers[0].Focus();

        UI.SendInput(panel, UI.HotKeys.AltRight);

        Assert.Equal(1, panel.SelectedIndex);
        Assert.True(panel.Headers[1].IsFocused);   // a label can't take keys, so focus rests on its header
    }

    [Fact]
    public void ProgrammaticSelect_DoesNotStealFocus_WhenPanelNotFocused()
    {
        var outside = new Button("out");
        var panel = new TabPanel(TabBarDock.Top, ("A", new ListBox()), ("B", new ListBox()));
        ConsoleSnapshot.Render(panel, 20, 6);
        ConsoleSnapshot.Render(outside, 10, 1);
        outside.Focus();   // focus is OUTSIDE the panel

        panel.SelectedIndex = 1;

        Assert.Equal(1, panel.SelectedIndex);
        Assert.True(outside.IsFocused);   // selection change didn't steal focus
    }

    [Fact]
    public void AltArrows_SwitchTabs_WhileNestedAndContentFocused()
    {
        // The whole point of handling this in the panel's tunnel: switch tabs from a parent layout, while the
        // active tab's CONTENT (not a header) holds focus. Exercises nested-layout tunnel routing.
        var contentA = new Button("A");
        var contentB = new Button("B");
        var panel = new TabPanel(TabBarDock.Top, ("One", contentA), ("Two", contentB));
        var grid = new Grid([6], [20], [[panel]]);   // panel nested inside a grid (the root)
        ConsoleSnapshot.Render(grid, 20, 6);
        contentA.Focus();                            // focus is in tab 0's content, not a tab header

        UI.SendInput(grid, UI.HotKeys.AltRight);     // routed through the grid -> into the nested panel's tunnel

        Assert.Equal(1, panel.SelectedIndex);        // tab switched even though a content button was focused
    }

    [Fact]
    public void FocusedControl_SurfacesFocusedHeader_ForNestedRouting()
    {
        var panel = TwoTabs(out _, out _);
        Assert.Null(panel.FocusedControl);          // nothing focused yet

        panel.Headers[1].Focus();

        Assert.Same(panel.Headers[1], panel.FocusedControl);   // a parent layout routes input here
    }
    #endregion

    #region Selection style
    [Fact]
    public void CaretSelectionStyle_PrefixesTheActiveTabHeader()
    {
        var panel = TwoTabs(out _, out _);
        panel.SelectionStyle = SelectionStyle.Caret;

        var bar = ConsoleSnapshot.ToText(panel, 28, 6).Split('\n')[0];   // the tab bar row

        Assert.Contains("▶", bar);          // the active tab shows the caret
        Assert.Contains("One", bar);        // ...before the active tab's label
        Assert.Contains("Two", bar);        // the inactive tab is still drawn (no caret, blank gutter)
    }
    #endregion

    #region Orientation
    [Fact]
    public void VerticalBar_RendersStackedHeadersAndContent()
    {
        var panel = TwoTabs(out _, out _, TabBarDock.Left);

        var text = ConsoleSnapshot.ToText(panel, 24, 6);

        Assert.Contains("One", text);
        Assert.Contains("Two", text);
        Assert.Contains("AAAA", text);
    }
    #endregion

    #region Dynamic tabs
    private static TabPanel ThreeTabs(out TextLabel a, out TextLabel b, out TextLabel c)
    {
        a = new TextLabel(TextLabelOrientation.Horizontal, "AAAA");
        b = new TextLabel(TextLabelOrientation.Horizontal, "BBBB");
        c = new TextLabel(TextLabelOrientation.Horizontal, "CCCC");
        return new TabPanel(TabBarDock.Top, ("One", a), ("Two", b), ("Three", c));
    }

    private static TextLabel Label(string s) => new(TextLabelOrientation.Horizontal, s);

    [Fact]
    public void AddTab_IntoEmptyPanel_AutoSelectsFirstTab()
    {
        var panel = new TabPanel(TabBarDock.Top);
        Assert.Equal(-1, panel.SelectedIndex);

        var first = panel.AddTab("First", Label("F1"));

        Assert.Equal(0, panel.SelectedIndex);
        Assert.Same(first, panel.ActiveTab);
        Assert.Contains("F1", ConsoleSnapshot.ToText(panel, 20, 4));
    }

    [Fact]
    public void AddTab_WhenAlreadySelected_KeepsSelection_AndAppends()
    {
        var panel = TwoTabs(out _, out _);
        panel.AddTab("Three", Label("CCCC"));

        Assert.Equal(3, panel.TabCount);
        Assert.Equal(0, panel.SelectedIndex);          // selection unchanged
        Assert.Contains("Three", ConsoleSnapshot.ToText(panel, 30, 6));
    }

    [Fact]
    public void AddTab_AtIndex_InsertsThere()
    {
        var panel = TwoTabs(out _, out _);
        var mid = panel.AddTab("Mid", Label("MMMM"), index: 1);

        Assert.Same(mid, panel.Tabs[1]);
        Assert.Equal("Mid", panel.Headers[1].Text);
        Assert.Equal(1, panel.Headers[1].Index);       // headers reindexed after insert
        Assert.Equal(2, panel.Headers[2].Index);
    }

    [Fact]
    public void RemoveTab_Selected_MovesSelectionToNeighbour()
    {
        var panel = ThreeTabs(out _, out var b, out _);
        panel.SelectedIndex = 1;                        // select "Two"

        panel.RemoveTab(panel.Tabs[1]);

        Assert.Equal(2, panel.TabCount);
        Assert.Equal(1, panel.SelectedIndex);           // "Three" took slot 1 and is now selected
        Assert.Equal("Three", panel.ActiveTabName);
        var text = ConsoleSnapshot.ToText(panel, 30, 6);
        Assert.Contains("CCCC", text);
        Assert.DoesNotContain("BBBB", text);
    }

    [Fact]
    public void RemoveTab_BeforeSelected_KeepsTheSameTabSelected()
    {
        var panel = ThreeTabs(out _, out _, out _);
        panel.SelectedIndex = 2;                         // select "Three"
        var three = panel.Tabs[2];

        panel.RemoveTab(0);                              // remove "One" — indices shift

        Assert.Same(three, panel.ActiveTab);            // same tab, by reference
        Assert.Equal(1, panel.SelectedIndex);           // now at index 1
        Assert.Equal("Three", panel.ActiveTabName);
    }

    [Fact]
    public void RemoveTab_LastRemaining_ClearsSelectionAndFill()
    {
        var panel = new TabPanel(TabBarDock.Top, ("Only", Label("ZZZZ")));
        var raised = new List<int>();
        panel.SelectionChanged += (_, i) => raised.Add(i);

        panel.RemoveTab(0);

        Assert.Equal(0, panel.TabCount);
        Assert.Equal(-1, panel.SelectedIndex);
        Assert.Null(panel.ActiveContent);
        Assert.Contains(-1, raised);                    // cleared -> -1
        Assert.DoesNotContain("ZZZZ", ConsoleSnapshot.ToText(panel, 20, 4));
    }

    [Fact]
    public void HiddenTab_LeavesBar_AndArrowsSkipIt()
    {
        var panel = ThreeTabs(out _, out _, out _);
        ConsoleSnapshot.Render(panel, 30, 6);

        panel.Tabs[1].IsHidden = true;                  // hide "Two"

        Assert.DoesNotContain("Two", ConsoleSnapshot.ToText(panel, 30, 6));  // gone from the bar
        Assert.Equal(3, panel.TabCount);                // still in the model

        panel.Headers[0].Focus();
        UI.SendInput(panel, new ConsoleKeyInfo('\0', ConsoleKey.RightArrow, false, false, false));
        Assert.Equal(2, panel.SelectedIndex);           // skipped hidden "Two" -> "Three"
    }

    [Fact]
    public void HidingSelectedTab_SelectsNeighbour()
    {
        var panel = ThreeTabs(out _, out _, out _);
        panel.SelectedIndex = 0;
        Assert.True(panel.Tabs[0].IsSelected);          // handle reflects selection

        panel.Tabs[0].IsHidden = true;

        Assert.Equal(1, panel.SelectedIndex);           // moved off the hidden tab
        Assert.Contains("BBBB", ConsoleSnapshot.ToText(panel, 30, 6));
    }

    [Fact]
    public void UnhidingIntoEmptyPanel_SelectsTheTab()
    {
        var panel = new TabPanel(TabBarDock.Top, ("One", Label("AAAA")));
        panel.Tabs[0].IsHidden = true;
        Assert.Equal(-1, panel.SelectedIndex);          // nothing selectable

        panel.Tabs[0].IsHidden = false;

        Assert.Equal(0, panel.SelectedIndex);
    }

    [Fact]
    public void DisabledTab_CannotBeSelectedByClick()
    {
        var panel = ThreeTabs(out _, out _, out _);
        ConsoleSnapshot.Render(panel, 30, 6);
        panel.Tabs[1].IsDisabled = true;

        Click(panel.Headers[1]);

        Assert.Equal(0, panel.SelectedIndex);           // click on a disabled tab is ignored
        Assert.False(panel.Headers[1].IsActive);
    }

    [Fact]
    public void DisabledTab_IsSkippedByArrowNavigation()
    {
        var panel = ThreeTabs(out _, out _, out _);
        ConsoleSnapshot.Render(panel, 30, 6);
        panel.Tabs[1].IsDisabled = true;
        panel.Headers[0].Focus();

        UI.SendInput(panel, new ConsoleKeyInfo('\0', ConsoleKey.RightArrow, false, false, false));

        Assert.Equal(2, panel.SelectedIndex);           // skipped disabled "Two" -> "Three"
    }

    [Fact]
    public void DisablingSelectedTab_SelectsNeighbour()
    {
        var panel = ThreeTabs(out _, out _, out _);
        panel.SelectedIndex = 1;

        panel.Tabs[1].IsDisabled = true;

        Assert.Equal(2, panel.SelectedIndex);           // moved off the disabled tab
        Assert.False(panel.Headers[1].IsActive);
    }

    [Fact]
    public void ProgrammaticSelect_OfDisabledOrHiddenTab_IsIgnored()
    {
        var panel = ThreeTabs(out _, out _, out _);
        panel.Tabs[2].IsDisabled = true;

        panel.SelectedIndex = 2;                         // can't select a disabled tab

        Assert.Equal(0, panel.SelectedIndex);
    }

    [Fact]
    public void Relabel_UpdatesHeaderAndBar()
    {
        var panel = TwoTabs(out _, out _);

        panel.Tabs[0].Name = "Renamed";

        Assert.Equal("Renamed", panel.Headers[0].Text);
        Assert.Equal("Renamed", panel.ActiveTabName);
        Assert.Contains("Renamed", ConsoleSnapshot.ToText(panel, 30, 6));
    }
    #endregion
}
