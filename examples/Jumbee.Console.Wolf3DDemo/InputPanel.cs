#nullable enable

namespace Jumbee.Console.Wolf3DDemo;

/// <summary>
/// The Input tab: how a stream of key presses becomes movement, and how fast that movement is.
/// </summary>
/// <remarks>
/// <para>
/// These are knobs rather than constants because the values behind them are <em>platform settings</em>. The
/// auto-repeat interval and initial delay differ per OS, per keyboard and per user preference pane, and a terminal
/// never reports a key-up — so the demo infers "still held" from the repeat stream, and the right constants for
/// that inference are different on every machine.
/// </para>
/// <para>
/// <b>Repeat</b> is the readout that matters: the interval measured from the keys actually being pressed here. Set
/// <c>Repeat gap</c> above it and below the initial delay, and <c>Windows</c> is then how many missed repeats it
/// takes to decide the key is up.
/// </para>
/// </remarks>
public sealed class InputPanel : CompositeControl
{
    #region Constructors
    /// <summary>Builds the page over <paramref name="view"/>; <paramref name="push"/> guards writes back to state.</summary>
    public InputPanel(Wolf3DView view, Action<Action> push)
    {
        this.view = view;
        var tuning = view.Tuning;

        firstPress.ValueChanged += (_, v) => push(() => tuning.FirstPressMs = v);
        coast.ValueChanged += (_, v) => push(() => tuning.CoastSeconds = v);
        repeatGap.ValueChanged += (_, v) => push(() => tuning.RepeatGapMs = v);
        windows.ValueChanged += (_, v) => push(() => tuning.RepeatWindows = v);
        walk.ValueChanged += (_, v) => push(() => tuning.WalkSpeed = v);
        run.ValueChanged += (_, v) => push(() => tuning.RunSpeed = v);
        turn.ValueChanged += (_, v) => push(() => tuning.TurnDegrees = v);
        runTurn.ValueChanged += (_, v) => push(() => tuning.RunTurnDegrees = v);
        reset.Activated += (_, _) => tuning.Reset();

        Build(spaced: true);
    }
    #endregion

    #region Properties
    /// <summary>Rows the page needs at its current spacing, for the sidebar's layout decision.</summary>
    public int Rows { get; private set; }
    #endregion

    #region Methods
    /// <summary>Lays the page out, roomy or compact.</summary>
    public void Build(bool spaced)
    {
        var gap = spaced ? 1 : 0;
        Panel.Section[] sections =
        [
            // The measured readout describes the four knobs above it, so it sits flush under them.
            new Panel.Section("Held keys", spaced
                    ? new VerticalStackPanel(Panel.Stack(true, firstPress, coast, repeatGap, windows), measured)
                    : new VerticalStackPanel(firstPress, coast, repeatGap, windows, measured),
                5 + (3 * gap)),
            new Panel.Section("Speed", Panel.Stack(spaced, walk, run, turn, runTurn), 4 + (3 * gap)),
            new Panel.Section("Tuning", Panel.Row(reset), 1),
            // The movement pad used to sit here, last and unspaced. It moved out to Wolf3DPadDock, under the
            // sidebar: it is the one widget you want while using every OTHER widget, and reaching it on this page
            // meant tabbing away from the Display knobs to move and back again to adjust. This page is now only
            // the knobs that tune input, which is what its name says.
        ];

        Rows = sections.Sum(s => s.OuterRows);
        SetContent(new VerticalStackPanel([.. sections]));
        // Deliberately NOT Refresh() here: Build runs from the constructor, and pushing state into a widget fires
        // its change event straight back at a sidebar whose fields do not exist yet. The sidebar refreshes once
        // both pages are up.
    }

    /// <summary>Re-reads every value from the tuning, and the measured repeat interval from the view.</summary>
    public void Refresh()
    {
        var tuning = view.Tuning;
        firstPress.Value = tuning.FirstPressMs;
        coast.Value = tuning.CoastSeconds;
        repeatGap.Value = tuning.RepeatGapMs;
        windows.Value = tuning.RepeatWindows;
        walk.Value = tuning.WalkSpeed;
        run.Value = tuning.RunSpeed;
        turn.Value = tuning.TurnDegrees;
        runTurn.Value = tuning.RunTurnDegrees;

        var repeat = view.MeasuredRepeatMs;
        measured.Text = repeat > 0 ? $" Repeat: {repeat} ms measured" : " Repeat: hold a key";
    }
    #endregion

    #region Fields
    private readonly Wolf3DView view;

    private readonly Slider firstPress =
        Panel.Knob("First press", 40, 600, Wolf3DTuning.DefaultFirstPressMs, 10, "0");
    private readonly Slider coast =
        Panel.Knob("Coast", 0, 0.8, Wolf3DTuning.DefaultCoastSeconds, 0.02, "0.00");
    private readonly Slider repeatGap =
        Panel.Knob("Repeat gap", 60, 600, Wolf3DTuning.DefaultRepeatGapMs, 10, "0");
    private readonly Slider windows =
        Panel.Knob("Windows", 1, 6, Wolf3DTuning.DefaultRepeatWindows, 0.5, "0.0");
    private readonly Slider walk =
        Panel.Knob("Walk", 0.5, 12, Wolf3DTuning.DefaultWalkSpeed, 0.1, "0.0");
    private readonly Slider run =
        Panel.Knob("Run", 0.5, 16, Wolf3DTuning.DefaultRunSpeed, 0.1, "0.0");
    private readonly Slider turn =
        Panel.Knob("Turn", 20, 400, Wolf3DTuning.DefaultTurnDegrees, 5, "0");
    private readonly Slider runTurn =
        Panel.Knob("Run turn", 20, 500, Wolf3DTuning.DefaultRunTurnDegrees, 5, "0");
    private readonly TextLabel measured = Panel.Line("", Panel.Muted);
    private readonly Button reset = new("Reset");
    #endregion
}
