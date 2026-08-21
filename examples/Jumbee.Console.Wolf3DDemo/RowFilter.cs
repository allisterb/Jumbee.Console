#nullable enable

namespace Jumbee.Console.Wolf3DDemo;

/// <summary>
/// How <see cref="Wolf3DRenderer"/> reduces the framebuffer's extra rows to the one row a surface sub-pixel gets.
/// </summary>
/// <remarks>
/// <para>
/// Only <see cref="SurfaceMode.Quadrant"/> has extra rows to reduce: the framebuffer is rendered
/// <see cref="HalfBlockSurface.SamplesPerColumn"/> times taller than the surface so that a framebuffer pixel stays
/// square, then comes back down here. Under <see cref="SurfaceMode.HalfBlock"/> the ratio is 1:1 and both modes are
/// the same code path.
/// </para>
/// <para>
/// The trade is the usual one for this demo: <see cref="Nearest"/> emits only exact palette entries and so keeps
/// colour <em>runs</em> long, which is what ANSI bytes track; <see cref="Box"/> reconstructs detail the discarded
/// rows carried, at the risk of inventing colours between palette entries and fragmenting those runs. Which wins
/// depends on <see cref="Wolf3DRenderer.QuantizeLevels"/>, because the quantiser runs immediately after and snaps
/// invented colours back onto the ramp.
/// </para>
/// </remarks>
public enum RowFilter
{
    /// <summary>Take the first row of each group and discard the rest. Every emitted colour is an exact palette entry.</summary>
    Nearest,

    /// <summary>Average the rows in each group. A box filter over the reduced axis only — the horizontal axis is not
    /// reduced, so there is nothing to filter across it.</summary>
    Box,
}
