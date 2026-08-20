#nullable enable

namespace Jumbee.Console.Wolf3DDemo;

/// <summary>
/// How the pixel surface maps its sub-cell samples onto glyphs.
/// </summary>
/// <remarks>
/// <para>
/// Every mode here gets exactly <b>two colours per character cell</b> — one foreground, one background. That is the
/// hard ceiling of character-cell rendering and no glyph escapes it; it is the whole difference between this and a
/// pixel protocol like Sixel, which addresses ~100–200 real device pixels per cell, each independently coloured.
/// </para>
/// <para>
/// So a finer grid is <em>not</em> a quality dial, it is a trade. <see cref="HalfBlock"/> is <b>exact</b>: two
/// samples, two colours, no quantisation error at all. <see cref="Quadrant"/> doubles the horizontal samples but
/// must then squeeze four sample colours into the same two, buying placement accuracy with colour accuracy. Which
/// wins depends entirely on the content — flat-shaded geometry has few colours per cell to lose, while dense
/// texture has little placement detail to gain.
/// </para>
/// </remarks>
public enum SurfaceMode
{
    /// <summary>
    /// One sample per column, two rows per cell, drawn as <c>▀</c>. Two samples, two colours — exact.
    /// </summary>
    HalfBlock,

    /// <summary>
    /// Two samples per column, two rows per cell, drawn as the quadrant glyphs (<c>▘▝▖▗▚▞</c>…). Four samples
    /// sharing two colours: sharper silhouettes, approximated colour.
    /// </summary>
    /// <remarks>
    /// A quadrant sub-pixel is half a cell wide but a whole half-cell tall — twice as tall as it is wide. Anything
    /// that assumes a square pixel is therefore wrong here, and the vendored sprite projector does assume one: it
    /// derives a single size and applies it to both axes. <c>Wolf3DRenderer</c> compensates by rendering its
    /// framebuffer at double height in this mode and sampling rows back down, which costs roughly twice the fill.
    /// </remarks>
    Quadrant,
}
