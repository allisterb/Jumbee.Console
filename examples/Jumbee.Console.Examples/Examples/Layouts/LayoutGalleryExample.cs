namespace Jumbee.Console.Examples;

/// <summary>
/// One tab per layout — a fixed <see cref="Grid"/> tile board, a docked app shell, stacked cards and nested
/// <see cref="SplitPanel"/>s — each shown the way it is really used. The tabs are themselves a <see cref="TabPanel"/>.
/// </summary>
public sealed class LayoutGalleryExample : CompositeControl, IExample
{
    public LayoutGalleryExample() =>
        SetContent(new TabPanel(TabBarDock.Top,
            ("Grid", GridTab()),
            ("Dock", DockTab()),
            ("Stack", StackTab()),
            ("Split", SplitTab())));

    // Each tab's layout fills the tab area — except the Grid, which deliberately doesn't: its cells are fixed-size.

    #region Grid — fixed rows x columns
    // A KPI board: every tile is exactly the same size no matter how the pane resizes. That's the Grid trade-off —
    // total control over the cells, no fluidity (rowHeights/columnWidths are cells, not weights).
    private static IFocusable GridTab() =>
        new Grid([5, 5], [18, 18, 18],
        [
            [Tile("Requests", "[white]1.2M[/]\n[green]▲ 4.1%[/]", Green),
             Tile("Latency",  "[white]84 ms[/]\n[green]▼ 2.0%[/]", Blue),
             Tile("Errors",   "[white]0.03%[/]\n[red]▲ 0.1%[/]", Red)],
            [Tile("CPU",      "[white]38%[/]\n[grey62]steady[/]", Orange),
             Tile("Memory",   "[white]2.1 GB[/]\n[yellow]▲ 60 MB[/]", Purple),
             Tile("Uptime",   "[white]99.98%[/]\n[grey62]30 days[/]", Cyan)],
        ]);

    private static IFocusable Tile(string name, string markup, Color color) =>
        Framed(new TextPanel(markup), name, color);
    #endregion

    #region DockPanel — bands on the edges, content filling the rest
    // The classic app shell. One DockPanel per edge, nested: each takes a docked band and gives the rest to its fill
    // slot. This browser's own shell is built exactly this way (panes over a status bar).
    private static IFocusable DockTab() =>
        new DockPanel(DockedControlPlacement.Top,
            Band("Toolbar · Top", "[black on #7fb3ff] File [/] [black on #7fb3ff] Edit [/] [black on #7fb3ff] View [/]", Blue),
            new DockPanel(DockedControlPlacement.Bottom,
                Band("Status · Bottom", "[green]●[/] [grey70]ready[/]   [grey54]Ln 1, Col 1   UTF-8[/]", Green),
                new DockPanel(DockedControlPlacement.Left,
                    Framed(new ListBox("Explorer", "Search", "Source", "Debug", "Extensions").WithWidth(15), "Nav · Left", Orange),
                    Framed(new TextPanel(
                        "[grey70]The fill slot gets whatever the docked bands leave over — so the content grows and\n" +
                        "shrinks with the pane while the bands keep their size.[/]\n\n" +
                        "[grey54]Dock order matters: each nested panel docks inside the previous one's fill slot.[/]"),
                        "Content · Fill", Purple))));

    private static IFocusable Band(string title, string markup, Color color) =>
        Framed(new TextPanel(markup).WithHeight(1), title, color);
    #endregion

    #region Stack panels — laid end to end, sized by their content
    // A toolbar of chips (horizontal) above a column of cards (vertical). Each stack sizes along its own axis and
    // STRETCHES its children across the other one — so a VerticalStackPanel's rows are as tall as their content but
    // full width. A HorizontalStackPanel fills whatever height it's offered, so a toolbar row nested in a vertical
    // stack must be height-bounded, here by a fixed-height Group (a layout has no size of its own to set).
    private static IFocusable StackTab() =>
        new VerticalStackPanel(
            new Group(new HorizontalStackPanel(Chip("+ New", Green), Chip("Open", Blue), Chip("Save", Orange))) { Height = 3 },
            Card("Inbox", "[white]12[/] unread · [grey54]2m ago[/]", Blue),
            Card("Drafts", "[white]3[/] saved · [grey54]1h ago[/]", Purple),
            Card("Archive", "[white]1,204[/] items · [grey54]last year[/]", Cyan),
            Framed(new TextPanel("[grey54]Cards stack at their own height, full width; the row\n" +
                                 "above is a HorizontalStackPanel bounded to 3 rows.[/]").WithHeight(2), null, Grey));

    // Wraps a layout as a Control so it can be given a fixed size — a stack panel has none of its own.
    private sealed class Group : CompositeControl
    {
        public Group(ILayout content) => SetContent(content);
    }

    private static IFocusable Chip(string label, Color color) =>
        Framed(new TextPanel($"[white]{label}[/]").WithSize(width: 10, height: 1), null, color);

    private static IFocusable Card(string name, string markup, Color color) =>
        Framed(new TextPanel(markup).WithHeight(1), name, color);
    #endregion

    #region SplitPanel — draggable dividers, both orientations
    // A reviewer: files on the left, and on the right a vertical split of preview over output. Drag either divider
    // (or focus it and press the arrows) — unlike the Grid, the panes are re-proportioned at runtime.
    private static IFocusable SplitTab() =>
        new SplitPanel(SplitOrientation.Horizontal,
            Framed(new ListBox("Program.cs", "Canvas.cs", "Plot.cs", "Tree.cs", "UI.cs", "Control.cs"), "Files", Orange),
            new SplitPanel(SplitOrientation.Vertical,
                Framed(new TextPanel(
                    "[grey70]Horizontal split → panes side by side (Ctrl+←/→).\n" +
                    "Vertical split → panes stacked (Ctrl+↑/↓).[/]\n\n" +
                    "[grey54]Nest them for an IDE shell: a sidebar beside an\neditor stacked over a terminal.[/]"),
                    "Preview", Blue),
                Framed(new TextPanel("[green]✓[/] [grey70]12 passed[/]\n[red]✕[/] [grey70]1 failed[/]  [grey54]0.4s[/]"),
                    "Output", Green),
                splitPosition: 9),
            splitPosition: 20);
    #endregion

    #region Helpers
    // Frames `content` and returns the FRAME (ControlFrame.FocusableControl is itself). Grid/Dock/Split resolve the
    // frame for you, but VerticalStackPanel binds whatever it's handed — so hand every layout the frame directly.
    private static IFocusable Framed(Control content, string? title, Color color)
    {
        content.WithFrame(borderStyle: BorderStyle.Rounded, borderFgColor: color);
        if (title is not null) content.WithTitle(title, InlineTitle);
        return content.FocusableControl;
    }
    #endregion

    #region IExample
    string IExample.Category => "Layouts";
    string IExample.Title => "Layout Gallery";
    string IExample.Description =>
        "One tab per layout — a fixed Grid tile board, a docked app shell, stacked cards, and nested split panes. The tabs themselves are a TabPanel.";
    #endregion

    #region Fields
    private static readonly TitleStyle InlineTitle = new(TitlePos.TopLeft, TitleBorderStyle.Inline);
    private static readonly Color Blue = new(0x5c, 0x9c, 0xff);
    private static readonly Color Green = new(0x8f, 0xd0, 0x66);
    private static readonly Color Orange = new(0xe0, 0xa0, 0x50);
    private static readonly Color Red = new(0xe0, 0x6c, 0x6c);
    private static readonly Color Purple = new(0xc8, 0x92, 0xf0);
    private static readonly Color Cyan = new(0x5c, 0xc8, 0xc8);
    private static readonly Color Grey = new(0x6a, 0x70, 0x84);
    #endregion
}
