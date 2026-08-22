#nullable enable

namespace Jumbee.Console.Wolf3DDemo;

/// <summary>
/// The right-hand sidebar: a tab per group of knobs, turnable while the demo runs.
/// </summary>
/// <remarks>
/// <para>
/// Tabs rather than one long stack because the groups answer different questions — Display is "what does a frame
/// look like", Input is "how does holding a key feel" — and because the list is expected to grow with the demo.
/// Each page is its own <see cref="CompositeControl"/>, so adding a tab is adding a class, not editing a stack.
/// </para>
/// <para>
/// <b>Widgets and keys stay in agreement because neither talks to the other.</b> The state objects
/// (<see cref="Wolf3DRenderer"/>, <see cref="Wolf3DTuning"/>, <see cref="Wolf3DView"/>) own the truth; a widget
/// writes to the state, and <see cref="Refresh"/> reads it back. The <c>syncing</c> guard stops the round trip
/// looping: while the panel pushes state into a widget, that widget's own change event does nothing.
/// </para>
/// </remarks>
public sealed class Wolf3DSidebar : CompositeControl
{
    #region Constructors
    /// <summary>Builds the sidebar over <paramref name="view"/>.</summary>
    public Wolf3DSidebar(Wolf3DView view)
    {
        Width = Columns;

        display = new DisplayPanel(view, Push);
        input = new InputPanel(view, Push);
        tabs = new TabPanel(TabBarDock.Top, ("Display", display), ("Input", input));
        Pad = new Wolf3DPadDock(view);
        Pad.Fit();

        view.Changed += Refresh;
        view.Tuning.Changed += Refresh;

        // The pad docks INSIDE the sidebar rather than beside it in the shell. A sibling column would have been the
        // obvious shape, but the sidebar's borderless scroll frame reports two columns more than its Width, so a
        // nested dock resolved the shared column to 34 and quietly took two columns off the viewport. Owning the
        // dock here keeps the shell's layout exactly as it was, keeps the column at Columns, and means the sidebar
        // collapse (`u`) still hides both with one Width.
        SetContent(new DockPanel(DockedControlPlacement.Bottom, Pad, tabs));
        Refresh();
    }
    #endregion

    #region Properties
    /// <summary>Columns the sidebar occupies when docked.</summary>
    public const int Columns = Panel.Columns;

    /// <summary>The tab strip, so the shell can move between pages by key.</summary>
    public TabPanel Tabs => tabs;

    /// <summary>The Display page, so headless checks can drive its widgets the way a user does.</summary>
    public DisplayPanel Display => display;

    /// <summary>The movement pad docked below the tabs, reachable from either page.</summary>
    public Wolf3DPadDock Pad { get; }

    /// <summary>Whether the pad is currently in the layout; false on a terminal too short to hold it and a page.</summary>
    public bool PadVisible { get; private set; } = true;
    /// <summary>
    /// Viewport rows below which the pages drop to their compact spacing.
    /// </summary>
    /// <remarks>
    /// Derived from the pages themselves rather than hand-maintained: the sandbox's equivalent is a constant that
    /// has to be re-measured every time a control is added, and getting it wrong is silent. Here the tallest page
    /// reports its own height, so adding a knob moves the threshold on its own.
    /// </remarks>
    public int SpacedRows => Math.Max(display.Rows, input.Rows) + TabBarRows;

    // A form of many fields rather than a composite built around one editor, so Tab walks the widgets instead of
    // being handed to whichever one has focus.
    /// <inheritdoc/>
    protected override bool TabNavigatesChildren => true;

    // Roomy when there is room, compact when there is not, so a short terminal shows a whole page rather than
    // making you scroll for the last section. Measured against the FRAME'S VIEWPORT, not this control's own
    // ActualHeight: inside a scrolling frame a control is laid out at the height it reports, so ActualHeight is the
    // content height and the comparison silently becomes "is the spaced layout as tall as the spaced layout".
    /// <inheritdoc/>
    protected override void Control_OnInitialization()
    {
        base.Control_OnInitialization();
        var rows = Frame?.ViewportSize.Height ?? ActualHeight;
        if (rows <= 0) return;   // before the first real layout; deciding from 0 would rebuild twice for nothing

        var wanted = rows >= SpacedRows;
        if (wanted == spaced || rebuilding) return;

        rebuilding = true;
        try
        {
            spaced = wanted;
            display.Build(wanted);
            input.Build(wanted);
            Refresh();
        }
        finally
        {
            rebuilding = false;
        }
    }
    #endregion

    #region Methods
    /// <summary>
    /// Shows or hides the pad for a column <paramref name="columnRows"/> tall, swapping the content when the verdict
    /// changes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Call from <see cref="UI.Paint"/>, never from a draw. It swaps the whole content layout rather than setting
    /// the pad's Height to 0, because <b>0 means "fill the parent"</b>, not "collapse" — setting it made the pad
    /// take the entire sidebar and the tabs vanish, which is the opposite of hiding it. There is no collapse
    /// primitive on Control, so removing it from the layout is the honest way to remove it.
    /// </para>
    /// <para>
    /// The pad is a big-terminal affordance: it wants 19 rows and a usable page wants 16 more, so below about 35
    /// rows of sidebar it goes. Every button on it has a keyboard equivalent; the panels do not.
    /// </para>
    /// </remarks>
    public void FitPad(int columnRows)
    {
        var show = columnRows >= Wolf3DPadDock.Rows + MinimumTabs;
        if (show == PadVisible) return;
        PadVisible = show;
        SetContent(show ? new DockPanel(DockedControlPlacement.Bottom, Pad, tabs) : tabs);
    }

    /// <summary>Re-reads every value from the state objects. Cheap, and called after any of them changes.</summary>
    public void Refresh()
    {
        if (syncing) return;
        syncing = true;
        try
        {
            display.Refresh();
            input.Refresh();
        }
        finally
        {
            syncing = false;
        }
    }

    // Widget -> state. Inert while the panel is pushing state INTO the widgets, which is what stops the loop.
    private void Push(Action write)
    {
        if (syncing) return;
        write();
        Refresh();
    }
    #endregion

    #region Fields
    // The tab strip plus the row of frame the TabPanel draws under it.
    private const int TabBarRows = 2;

    private readonly DisplayPanel display;
    private readonly InputPanel input;
    private readonly TabPanel tabs;

    // Rows the tab strip plus a whole Display page needs above the pad, so the pad is what goes when both cannot fit.
    private const int MinimumTabs = 16;
    private bool spaced = true;
    private bool rebuilding;
    private bool syncing;
    #endregion
}
