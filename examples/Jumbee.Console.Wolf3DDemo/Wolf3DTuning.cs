#nullable enable

namespace Jumbee.Console.Wolf3DDemo;

/// <summary>
/// The movement and key-handling knobs, in one place so the sidebar can turn them while the demo runs.
/// </summary>
/// <remarks>
/// <para>
/// These started as constants and could not stay that way. The auto-repeat rate and initial delay behind
/// <see cref="FirstPressMs"/> and <see cref="CoastSeconds"/> are <em>operating-system settings</em>, different on
/// every machine and adjustable by the user — so a value tuned on one desktop is wrong on the next. See the
/// key-handling notes on <see cref="Wolf3DView"/> for what each one is bridging.
/// </para>
/// <para>
/// Owns the truth and raises <see cref="Changed"/>; the sidebar writes here and reads back. UI-thread owned.
/// </para>
/// </remarks>
public sealed class Wolf3DTuning
{
    #region Events
    /// <summary>Raised on the UI thread whenever any value changes.</summary>
    public event Action? Changed;
    #endregion

    #region Properties
    /// <summary>How long one press keeps an axis alive before the repeat stream has been identified, in ms.</summary>
    /// <remarks>The dominant term in how far a tap travels. Too low and a tap barely registers; too high and every
    /// tap overshoots.</remarks>
    public double FirstPressMs
    {
        get => firstPressMs;
        set => Set(ref firstPressMs, Math.Clamp(value, 40, 600));
    }

    /// <summary>Time constant of the speed decay that covers the OS initial repeat delay, in seconds.</summary>
    /// <remarks>
    /// 0 disables coasting, which restores the original hard stall at the start of every hold. Raising it smooths
    /// that stall at the cost of a tap running on further — the two cannot be improved independently, because
    /// until the first repeat arrives a tap and a hold are literally the same input.
    /// </remarks>
    public double CoastSeconds
    {
        get => coastSeconds;
        set => Set(ref coastSeconds, Math.Clamp(value, 0.0, 0.8));
    }

    /// <summary>Presses closer together than this are treated as auto-repeat rather than as new presses, in ms.</summary>
    /// <remarks>Wants to sit above the platform's repeat interval and below its initial delay. The Input tab shows
    /// the interval actually being measured, which is the number to set this against.</remarks>
    public double RepeatGapMs
    {
        get => repeatGapMs;
        set => Set(ref repeatGapMs, Math.Clamp(value, 60, 600));
    }

    /// <summary>How many measured repeat intervals a sustain window lasts.</summary>
    /// <remarks>Below about 2 a single late repeat drops the key; above about 4 the release drags.</remarks>
    public double RepeatWindows
    {
        get => repeatWindows;
        set => Set(ref repeatWindows, Math.Clamp(value, 1.0, 6.0));
    }

    /// <summary>Walking speed, in map tiles a second.</summary>
    public double WalkSpeed
    {
        get => walkSpeed;
        set => Set(ref walkSpeed, Math.Clamp(value, 0.5, 12.0));
    }

    /// <summary>Running speed (Shift), in map tiles a second.</summary>
    public double RunSpeed
    {
        get => runSpeed;
        set => Set(ref runSpeed, Math.Clamp(value, 0.5, 16.0));
    }

    /// <summary>Turn rate, in degrees a second.</summary>
    public double TurnDegrees
    {
        get => turnDegrees;
        set => Set(ref turnDegrees, Math.Clamp(value, 20, 400));
    }

    /// <summary>Turn rate while running, in degrees a second.</summary>
    public double RunTurnDegrees
    {
        get => runTurnDegrees;
        set => Set(ref runTurnDegrees, Math.Clamp(value, 20, 500));
    }
    #endregion

    #region Methods
    /// <summary>Restores every value to its shipped default, raising <see cref="Changed"/> once.</summary>
    public void Reset()
    {
        firstPressMs = DefaultFirstPressMs;
        coastSeconds = DefaultCoastSeconds;
        repeatGapMs = DefaultRepeatGapMs;
        repeatWindows = DefaultRepeatWindows;
        walkSpeed = DefaultWalkSpeed;
        runSpeed = DefaultRunSpeed;
        turnDegrees = DefaultTurnDegrees;
        runTurnDegrees = DefaultRunTurnDegrees;
        Changed?.Invoke();
    }

    private void Set(ref double field, double value)
    {
        if (field.Equals(value)) return;
        field = value;
        Changed?.Invoke();
    }
    #endregion

    #region Fields
    /// <summary>The shipped defaults, also the Reset targets and the sliders' starting points.</summary>
    public const double DefaultFirstPressMs = 150;
    public const double DefaultCoastSeconds = 0.22;
    public const double DefaultRepeatGapMs = 250;
    public const double DefaultRepeatWindows = 3;
    public const double DefaultWalkSpeed = 3.4;
    public const double DefaultRunSpeed = 6.0;
    public const double DefaultTurnDegrees = 126;
    public const double DefaultRunTurnDegrees = 195;

    private double firstPressMs = DefaultFirstPressMs;
    private double coastSeconds = DefaultCoastSeconds;
    private double repeatGapMs = DefaultRepeatGapMs;
    private double repeatWindows = DefaultRepeatWindows;
    private double walkSpeed = DefaultWalkSpeed;
    private double runSpeed = DefaultRunSpeed;
    private double turnDegrees = DefaultTurnDegrees;
    private double runTurnDegrees = DefaultRunTurnDegrees;
    #endregion
}
