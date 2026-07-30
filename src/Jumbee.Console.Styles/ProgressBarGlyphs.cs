namespace Jumbee.Console;

/// <summary>How a <c>ProgressBar</c> draws its bar.</summary>
public enum ProgressBarFillMode
{
    /// <summary>A solid colour band: the filled and empty portions are drawn as background-coloured runs, and the
    /// fill edge renders at <em>sub-cell</em> resolution using eighth-block glyphs so it advances smoothly. The
    /// glyph strings in <see cref="ProgressBarGlyphs"/> are ignored in this mode.</summary>
    Solid,

    /// <summary>Per-cell glyphs: each filled cell draws <see cref="ProgressBarGlyphs.Fill"/> and each empty cell
    /// <see cref="ProgressBarGlyphs.Track"/>, in the fill/track colours as <em>foreground</em>. Character-granular
    /// (no sub-cell edge) — the mode for a hatched, segmented or ASCII bar.</summary>
    Glyph,
}

/// <summary>
/// The glyphs (no colours) a <c>ProgressBar</c> draws its bar with: the <see cref="Fill"/> for a filled cell and the
/// <see cref="Track"/> for an empty one, plus the <see cref="Mode"/> that selects solid-band or per-cell-glyph
/// rendering.
/// </summary>
/// <remarks>
/// Mirrors <see cref="ScrollBarGlyphs"/>: colours come separately from <see cref="ProgressBarStyle"/> (via
/// <see cref="IStyleTheme.ProgressBar"/>), and <see cref="ProgressBarFillMode.Solid"/> (the default) ignores the
/// glyphs and draws a smooth sub-cell band. Glyphs such as <c>▨ ▓ █ ▱</c> need block/box-drawing font coverage; the
/// <see cref="Ascii"/> preset is the portable fallback.
/// </remarks>
public readonly struct ProgressBarGlyphs : System.IEquatable<ProgressBarGlyphs>
{
    #region Constructors
    /// <summary>Builds a <see cref="ProgressBarFillMode.Glyph"/> glyph set (explicit glyphs imply glyph mode).</summary>
    public ProgressBarGlyphs(string fill, string track)
    {
        Fill = fill;
        Track = track;
        Mode = ProgressBarFillMode.Glyph;
    }
    #endregion

    #region Properties
    /// <summary>Which bar to render (a solid sub-cell band, or per-cell glyphs). Defaults to
    /// <see cref="ProgressBarFillMode.Solid"/> for a default-constructed value and the <see cref="Solid"/> preset.</summary>
    public ProgressBarFillMode Mode { get; init; }

    /// <summary>The glyph for a filled cell. Glyph mode only.</summary>
    public string Fill { get; init; }

    /// <summary>The glyph for an empty cell. Glyph mode only.</summary>
    public string Track { get; init; }
    #endregion

    #region Presets
    /// <summary>The default: the solid sub-cell band (<see cref="ProgressBarFillMode.Solid"/>).</summary>
    public static ProgressBarGlyphs Default { get; } = Solid;

    /// <summary>The solid band. The glyph fields are placeholders and unused by the band renderer, which draws its
    /// own eighth-block cells.</summary>
    public static ProgressBarGlyphs Solid { get; } =
        new() { Mode = ProgressBarFillMode.Solid, Fill = "█", Track = " " };

    /// <summary>A diagonal-hatch fill (<c>▨</c>) on a light-shade track (<c>░</c>).</summary>
    public static ProgressBarGlyphs Hatched { get; } = new("▨", "░");

    /// <summary>A dark-shade fill (<c>▓</c>) on a light-shade track (<c>░</c>).</summary>
    public static ProgressBarGlyphs Shaded { get; } = new("▓", "░");

    /// <summary>A full-block fill (<c>█</c>) on a light-shade track (<c>░</c>) — reads as discrete segments once
    /// coloured.</summary>
    public static ProgressBarGlyphs Segmented { get; } = new("█", "░");

    /// <summary>Discrete segments with visible gaps: a filled parallelogram (<c>▰</c>) fill on an empty one (<c>▱</c>).</summary>
    public static ProgressBarGlyphs Dashed { get; } = new("▰", "▱");

    /// <summary>A thin line bar: a heavy horizontal (<c>━</c>) fill on a light one (<c>─</c>).</summary>
    public static ProgressBarGlyphs Line { get; } = new("━", "─");

    /// <summary>A braille-dot fill (<c>⣿</c>) on a low-dot track (<c>⣀</c>).</summary>
    public static ProgressBarGlyphs Dots { get; } = new("⣿", "⣀");

    /// <summary>A portable fallback for terminals without block glyphs: a <c>#</c> fill on a <c>-</c> track.</summary>
    public static ProgressBarGlyphs Ascii { get; } = new("#", "-");
    #endregion

    #region Equality
    // Hand-written: the string fields make this struct non-bitwise-comparable, so the runtime's default
    // ValueType.Equals falls back to a reflective, boxing compare. Ordinal — these are glyphs, not text. See
    // ScrollBarGlyphs.
    /// <summary>Determines whether this <see cref="ProgressBarGlyphs"/> equals <paramref name="other"/>.</summary>
    public bool Equals(ProgressBarGlyphs other) =>
        Mode == other.Mode
        && string.Equals(Fill, other.Fill, System.StringComparison.Ordinal)
        && string.Equals(Track, other.Track, System.StringComparison.Ordinal);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ProgressBarGlyphs other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new System.HashCode();
        hash.Add(Mode);
        hash.Add(Fill, System.StringComparer.Ordinal);
        hash.Add(Track, System.StringComparer.Ordinal);
        return hash.ToHashCode();
    }

    /// <summary>Equality operator.</summary>
    public static bool operator ==(ProgressBarGlyphs a, ProgressBarGlyphs b) => a.Equals(b);

    /// <summary>Inequality operator.</summary>
    public static bool operator !=(ProgressBarGlyphs a, ProgressBarGlyphs b) => !a.Equals(b);
    #endregion
}
