namespace Jumbee.Console;

/// <summary>
/// The per-part <see cref="Style"/> a <c>Slider</c> composes: the <see cref="Label"/>, the filled and empty
/// portions of the track (<see cref="Fill"/>/<see cref="Track"/>), the draggable <see cref="Thumb"/>, and the
/// numeric <see cref="Value"/> readout.
/// </summary>
/// <remarks>Only the foreground colour of <see cref="Fill"/>/<see cref="Track"/> is used — like <c>Gauge</c> and
/// <c>ProgressBar</c>, the track is drawn as a solid colour band. The thumb is a foreground glyph over that band,
/// so <see cref="Thumb"/>'s foreground is what makes the handle stand out.</remarks>
public readonly struct SliderStyle : System.IEquatable<SliderStyle>
{
    #region Constructors
    /// <summary>Initializes a new <see cref="SliderStyle"/> from its part styles.</summary>
    public SliderStyle(Style label, Style fill, Style track, Style thumb, Style value)
    {
        Label = label;
        Fill = fill;
        Track = track;
        Thumb = thumb;
        Value = value;
    }
    #endregion

    #region Properties
    /// <summary>The caption drawn before the track.</summary>
    public Style Label { get; init; }

    /// <summary>The track to the left of the thumb (its foreground colour fills the band).</summary>
    public Style Fill { get; init; }

    /// <summary>The track to the right of the thumb (its foreground colour fills the band).</summary>
    public Style Track { get; init; }

    /// <summary>The handle at the fill's leading edge (its foreground colour draws the glyph).</summary>
    public Style Thumb { get; init; }

    /// <summary>The numeric readout drawn after the track.</summary>
    public Style Value { get; init; }
    #endregion

    #region Methods
    /// <summary>A copy with the filled portion recoloured (keeps every other part).</summary>
    public SliderStyle WithFill(Color fill) => this with { Fill = fill };

    /// <summary>A copy with the thumb recoloured (keeps every other part).</summary>
    public SliderStyle WithThumb(Color thumb) => this with { Thumb = thumb };
    #endregion

    #region Presets
    /// <summary>A blue fill on a dim dark-grey track under a near-white thumb, with grey label and readout.</summary>
    public static SliderStyle Default { get; } = new(
        label: Style.Grey85,
        fill: new Color(90, 160, 240),
        track: new Color(48, 48, 58),
        thumb: new Color(235, 240, 250),
        value: Style.Grey85);
    #endregion

    #region Equality
    // Hand-written: Style holds a reference, so the runtime's default ValueType.Equals falls back to a reflective,
    // boxing field-by-field compare — which is what SetAtomicProperty would run on every assignment. See GaugeStyle.
    /// <summary>Determines whether this <see cref="SliderStyle"/> equals <paramref name="other"/>.</summary>
    public bool Equals(SliderStyle other) =>
        Label == other.Label && Fill == other.Fill && Track == other.Track &&
        Thumb == other.Thumb && Value == other.Value;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is SliderStyle other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => System.HashCode.Combine(Label, Fill, Track, Thumb, Value);

    /// <summary>Equality operator.</summary>
    public static bool operator ==(SliderStyle a, SliderStyle b) => a.Equals(b);

    /// <summary>Inequality operator.</summary>
    public static bool operator !=(SliderStyle a, SliderStyle b) => !a.Equals(b);
    #endregion
}
