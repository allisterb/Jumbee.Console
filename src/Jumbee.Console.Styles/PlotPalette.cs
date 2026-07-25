namespace Jumbee.Console;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// The ordered colours a plot cycles through for series that don't name one.
/// </summary>
/// <remarks>
/// <para>
/// A value type with structural equality, like every other grouped theme token, and for the same reason: themed
/// properties are compared with <see cref="EqualityComparer{T}.Default"/> on assignment, and a bare array or
/// <see cref="IReadOnlyList{T}"/> would compare by <em>reference</em>. Two identical palettes would then count as
/// different (repainting and re-laying-out needlessly), while a palette mutated in place would count as the same
/// (and the change would be silently dropped). Construction copies the caller's sequence, so a palette also cannot
/// change under a control that has already captured it.
/// </para>
/// <para>
/// The indexer wraps, so a palette is never "too short" for the number of series and there is no arity contract for
/// a theme to violate. A <see langword="default"/> instance behaves as <see cref="Default"/> rather than throwing.
/// </para>
/// </remarks>
public readonly struct PlotPalette : IEquatable<PlotPalette>
{
    #region Constructors
    /// <summary>Creates a palette from <paramref name="colors"/>, which is copied. An empty or <see langword="null"/>
    /// sequence yields <see cref="Default"/>.</summary>
    public PlotPalette(IEnumerable<Color>? colors)
    {
        var copy = colors?.ToArray();
        _colors = copy is { Length: > 0 } ? copy : null;
    }
    #endregion

    #region Properties
    /// <summary>Pleasant, high-contrast defaults, cycled by series index.</summary>
    /// <remarks>Deliberately not the plotting library's own defaults, whose first entry is black — invisible on a
    /// dark terminal.</remarks>
    public static readonly PlotPalette Default = new(DefaultColors);

    /// <summary>How many colours before the cycle repeats.</summary>
    public int Count => Items.Length;

    /// <summary>The colour for <paramref name="index"/>, wrapping once the palette is exhausted.</summary>
    public Color this[int index] => Items[((index % Items.Length) + Items.Length) % Items.Length];
    #endregion

    #region Methods
    /// <summary>The colours in order, for a caller that needs the whole sequence rather than one entry.</summary>
    public IReadOnlyList<Color> ToList() => (Color[])Items.Clone();

    /// <inheritdoc/>
    public bool Equals(PlotPalette other) => Items.AsSpan().SequenceEqual(other.Items);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is PlotPalette other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var color in Items) hash.Add(color);
        return hash.ToHashCode();
    }

    /// <summary>Value equality — see <see cref="Equals(PlotPalette)"/>.</summary>
    public static bool operator ==(PlotPalette left, PlotPalette right) => left.Equals(right);

    /// <summary>Value inequality — see <see cref="Equals(PlotPalette)"/>.</summary>
    public static bool operator !=(PlotPalette left, PlotPalette right) => !left.Equals(right);
    #endregion

    #region Fields
    // A default(PlotPalette) has a null array, so every read goes through Items rather than the field.
    private Color[] Items => _colors ?? DefaultColors;

    private static readonly Color[] DefaultColors =
    [
        new(89, 145, 240),  new(240, 120, 100), new(120, 200, 120), new(230, 190, 90),
        new(190, 130, 230), new(110, 205, 220), new(235, 140, 200), new(160, 170, 180),
    ];

    private readonly Color[]? _colors;
    #endregion
}
