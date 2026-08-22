#nullable enable

namespace Jumbee.Console.Wolf3DDemo;

/// <summary>
/// The movement pad in its own panel under the sidebar, filling the corner beside the status bar.
/// </summary>
/// <remarks>
/// <para>
/// The pad used to live on the sidebar's <b>Input</b> tab, which meant reaching it cost a tab switch away from the
/// Display knobs — so adjusting the render and then moving had to alternate between two pages. It is the one
/// widget you want available while using every other widget, and the bottom-right corner was empty anyway: the
/// sidebar is top-aligned and the status bar spans only the viewport, leaving a sidebar-wide hole beside it.
/// </para>
/// <para>
/// Sized to match the bar it sits beside, via <see cref="MatchHeight"/>, so the two read as one band across the
/// bottom rather than as two panels that happen to be adjacent.
/// </para>
/// </remarks>
public sealed class Wolf3DPadDock : CompositeControl
{
    #region Constructors
    /// <summary>Creates the docked pad over <paramref name="view"/>.</summary>
    public Wolf3DPadDock(Wolf3DView view)
    {
        Pad = new Wolf3DPad(view);
        SetContent(new VerticalStackPanel(Pad));
        this.WithFrame(borderStyle: BorderStyle.Rounded, borderFgColor: Panel.Muted)
            .WithTitle("Move", new TitleStyle(TitlePos.TopLeft, TitleBorderStyle.Inline));
    }
    #endregion

    #region Properties
    /// <summary>The pad itself, so checks can drive its buttons.</summary>
    public Wolf3DPad Pad { get; }

    /// <summary>Rows the panel needs: the pad plus the frame's two border rows.</summary>
    public const int Rows = Wolf3DPad.Rows + 2;

    // What the tab strip plus a whole Display page needs above the pad. Sized so the pad is the thing that goes on
    // a terminal that cannot hold both: every one of its buttons has a key, and the panels do not. At 8 this looked
    // fine at 200x52 and squeezed the Display knobs off screen at 120x32 -- the pad is a big-terminal affordance,
    // and pretending otherwise just moves the crowding somewhere less visible.
    private const int MinimumTabs = 16;
    #endregion

    #region Methods
    /// <summary>
    /// Takes the height the pad needs when <paramref name="columnRows"/> can spare it, and collapses otherwise.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Sized from what the PAD needs rather than from the status bar beside it. The first version matched the bar,
    /// so the two read as one band across the bottom — which was the nicer picture, but the bar is 10 rows at a
    /// typical width and the pad is 17, so matching it would now collapse the pad entirely. The pad won: it is a
    /// control you aim at, and the bar is a readout.
    /// </para>
    /// <para>
    /// Call from <see cref="UI.Paint"/>, never from a frame's draw — setting a docked control's Height re-runs
    /// layout, and doing that mid-composite corrupts the frame being drawn. The assignment is guarded on the value
    /// actually changing, so an unchanged frame costs one comparison.
    /// </para>
    /// <para>
    /// It collapses rather than clipping when the column cannot give it <see cref="Rows"/> plus something for the
    /// tabs above. Half a pad is worse than none — the arrows it cuts are the ones you reach for — and the keys do
    /// everything it does anyway.
    /// </para>
    /// </remarks>
    public void Fit()
    {
        if (Height != Rows) Height = Rows;
    }
    #endregion
}
