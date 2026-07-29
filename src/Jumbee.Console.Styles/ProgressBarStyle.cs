namespace Jumbee.Console;

/// <summary>
/// The per-part <see cref="Style"/> a <c>ProgressBar</c> composes: the task <see cref="Description"/>, the
/// filled and empty portions of the bar (<see cref="Fill"/>/<see cref="Track"/>), and the three optional
/// readouts — <see cref="Percentage"/>, <see cref="Time"/> and <see cref="Spinner"/>.
/// </summary>
/// <remarks>Like <c>GaugeStyle</c>, only the foreground colour of <see cref="Fill"/>/<see cref="Track"/> is used —
/// the bar is drawn as a solid colour band.</remarks>
public readonly struct ProgressBarStyle : System.IEquatable<ProgressBarStyle>
{
    #region Constructors
    /// <summary>Initializes a new <see cref="ProgressBarStyle"/> from its part styles.</summary>
    public ProgressBarStyle(Style description, Style fill, Style track, Style percentage, Style time, Style spinner)
    {
        Description = description;
        Fill = fill;
        Track = track;
        Percentage = percentage;
        Time = time;
        Spinner = spinner;
    }
    #endregion

    #region Properties
    /// <summary>The task status text drawn before the bar.</summary>
    public Style Description { get; init; }

    /// <summary>The filled portion of the bar (its foreground colour fills the band).</summary>
    public Style Fill { get; init; }

    /// <summary>The empty track behind the fill (its foreground colour fills the band).</summary>
    public Style Track { get; init; }

    /// <summary>The percentage readout (e.g. <c>96%</c>).</summary>
    public Style Percentage { get; init; }

    /// <summary>The elapsed/remaining time readout (e.g. <c>00:00:00</c>).</summary>
    public Style Time { get; init; }

    /// <summary>The animated spinner glyph.</summary>
    public Style Spinner { get; init; }
    #endregion

    #region Methods
    /// <summary>A copy with the bar fill recoloured (keeps every other part).</summary>
    public ProgressBarStyle WithFill(Color fill) => this with { Fill = fill };
    #endregion

    #region Presets
    /// <summary>A green fill on a dim dark-grey track; grey description and percentage, a soft blue time, a green
    /// spinner — the Spectre progress-row look.</summary>
    public static ProgressBarStyle Default { get; } = new(
        description: Style.Grey85,
        fill: new Color(90, 200, 120),
        track: new Color(48, 48, 58),
        percentage: Style.Grey85,
        time: new Color(120, 170, 240),
        spinner: new Color(90, 200, 120));
    #endregion

    #region Equality
    // Hand-written: Style holds a reference, so the runtime's default ValueType.Equals falls back to a reflective,
    // boxing field-by-field compare — which is what SetAtomicProperty would run on every assignment. See GaugeStyle.
    /// <summary>Determines whether this <see cref="ProgressBarStyle"/> equals <paramref name="other"/>.</summary>
    public bool Equals(ProgressBarStyle other) =>
        Description == other.Description && Fill == other.Fill && Track == other.Track &&
        Percentage == other.Percentage && Time == other.Time && Spinner == other.Spinner;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ProgressBarStyle other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => System.HashCode.Combine(Description, Fill, Track, Percentage, Time, Spinner);

    /// <summary>Equality operator.</summary>
    public static bool operator ==(ProgressBarStyle a, ProgressBarStyle b) => a.Equals(b);

    /// <summary>Inequality operator.</summary>
    public static bool operator !=(ProgressBarStyle a, ProgressBarStyle b) => !a.Equals(b);
    #endregion
}
