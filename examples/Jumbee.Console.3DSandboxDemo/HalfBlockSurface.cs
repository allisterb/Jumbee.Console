namespace Jumbee.Console.SandboxDemo;

using ConsoleGUI.Space;

using CCharacter = ConsoleGUI.Data.Character;
using CColor = ConsoleGUI.Data.Color;

/// <summary>How <see cref="HalfBlockSurface"/> draws detected silhouettes and creases.</summary>
public enum SilhouetteStyle
{
    /// <summary>No edge treatment.</summary>
    None,

    /// <summary>
    /// Darken the edge sub-pixels — an ink outline. Keeps the surface's doubled vertical resolution.
    /// </summary>
    Ink,

    /// <summary>
    /// Replace the cell with a shaped glyph (<c>◆◇◈◊◌</c>), the technique in
    /// <c>reference/projects/c_ascii_render-main</c>.
    /// </summary>
    /// <remarks>
    /// A glyph carries one foreground and one background, so a cell drawn this way <b>gives up its two independent
    /// sub-pixels</b> — the edge lands on a half-resolution cell boundary rather than a half-cell one. That is free
    /// for a renderer sampling once per cell, as theirs does; here it is a genuine trade, which is why
    /// <see cref="Ink"/> exists alongside it.
    /// </remarks>
    Glyph,
}

/// <summary>
/// A drawing surface at <b>twice</b> the vertical resolution of the terminal: each character cell carries two
/// independently coloured sub-pixels, drawn as <c>▀</c> with the top half in the foreground colour and the bottom
/// half in the background. Owns a depth buffer alongside the colour buffer, so callers can draw in any order.
/// </summary>
/// <remarks>
/// <para>
/// The half-block technique <see cref="Globe"/> already uses, generalised and given a z-buffer. A sub-pixel is one
/// cell wide and half a cell tall, and a character cell is about twice as tall as it is wide, so sub-pixels come out
/// <em>square</em> — the <c>W × 2H</c> grid is isotropic and shares the same <see cref="Viewport"/> mapping the
/// wireframe canvas uses.
/// </para>
/// <para>
/// Depth is stored as a <em>reciprocal</em> (larger means nearer), which is what a perspective rasteriser can
/// interpolate linearly in screen space; <see cref="TestAndSet"/> takes it in that form.
/// </para>
/// </remarks>
public sealed class HalfBlockSurface : Control
{
    #region Constructors
    /// <summary>Creates the surface. Display-only — it draws what it is given and handles no input itself.</summary>
    public HalfBlockSurface()
    {
        Focusable = false;
        // The whole surface changes whenever the camera moves, so partial-redraw bookkeeping is pure overhead here.
        // See the M0.1 measurements in the plan.
    }
    #endregion

    #region Properties
    /// <summary>Sub-pixel columns — one per character column.</summary>
    public int PixelWidth { get; private set; }

    /// <summary>Sub-pixel rows — two per character row.</summary>
    public int PixelHeight { get; private set; }

    /// <summary>Colour behind everything, used for sub-pixels nothing was drawn into.</summary>
    public CColor Background { get; set; } = new(12, 12, 18);

    /// <summary>How <see cref="DetectEdges"/>'s findings are drawn.</summary>
    public SilhouetteStyle EdgeStyle { get; set; } = SilhouetteStyle.Glyph;
    #endregion

    #region Methods
    /// <summary>Resizes the sub-pixel buffers to the control's current size and clears them. Call once per frame,
    /// before drawing; returns <see langword="false"/> if the control has no area yet.</summary>
    public bool BeginFrame()
    {
        var w = ActualWidth;
        var h = ActualHeight * 2;
        if (w <= 0 || h <= 0) return false;

        if (w != PixelWidth || h != PixelHeight)
        {
            PixelWidth = w;
            PixelHeight = h;
            color = new CColor[w * h];
            depth = new float[w * h];
            group = new byte[w * h];
        }

        Array.Fill(color, Background);
        Array.Fill(depth, 0f);   // 0 = infinitely far, since depth is a reciprocal
        Array.Clear(group);
        return true;
    }

    /// <summary>
    /// Writes <paramref name="c"/> at the sub-pixel if it is nearer than what is already there.
    /// </summary>
    /// <param name="x">Sub-pixel column.</param>
    /// <param name="y">Sub-pixel row, top-down.</param>
    /// <param name="inverseDepth">Reciprocal camera-space depth — larger is nearer.</param>
    /// <param name="c">The colour to write.</param>
    /// <param name="group">
    /// What kind of surface this is: 0 for scenery, non-zero for a body. <see cref="DetectEdges"/> outlines only
    /// non-zero groups — outlining the ground plane's own outer boundary turns the horizon into speckle and reads
    /// as noise rather than as shape.
    /// </param>
    public void TestAndSet(int x, int y, float inverseDepth, CColor c, byte group = 0)
    {
        if ((uint)x >= (uint)PixelWidth || (uint)y >= (uint)PixelHeight) return;
        var i = (y * PixelWidth) + x;
        if (inverseDepth <= depth[i]) return;
        depth[i] = inverseDepth;
        color[i] = c;
        this.group[i] = group;
    }

    /// <summary>The reciprocal depth currently at a sub-pixel, or 0 if nothing has been drawn there.</summary>
    public float DepthAt(int x, int y) =>
        (uint)x >= (uint)PixelWidth || (uint)y >= (uint)PixelHeight ? 0f : depth[(y * PixelWidth) + x];

    /// <summary>The colour currently at a sub-pixel.</summary>
    public CColor ColorAt(int x, int y) =>
        (uint)x >= (uint)PixelWidth || (uint)y >= (uint)PixelHeight ? Background : color[(y * PixelWidth) + x];

    /// <summary>
    /// Finds silhouettes and creases in the depth buffer, and applies <see cref="EdgeStyle"/> to them.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The test is the <b>second difference of the inverse-depth field</b>, and it works because of a property of
    /// this particular buffer: <c>1/z</c> is <em>linear in screen space</em> across any planar surface (it is why
    /// the rasteriser can interpolate it with barycentrics at all). So on a plane the second difference is exactly
    /// zero — however steeply that plane recedes. It goes non-zero in precisely two places: a <b>crease</b>, where
    /// two differently-oriented planes meet (a box edge), and a <b>silhouette</b>, where depth jumps to whatever is
    /// behind. That is the same set their box-specific SDF test finds, without being specific to boxes.
    /// </para>
    /// <para>
    /// A naive "do neighbouring depths differ" test cannot do this: on ground seen near the horizon, adjacent rows
    /// legitimately differ enormously in depth, so any threshold either lights up the whole far plane or misses
    /// real edges up close. The second difference sidesteps that entirely.
    /// </para>
    /// <para>
    /// Curved surfaces have a genuinely non-zero second difference, so a sphere's interior carries a small signal;
    /// <paramref name="threshold"/> is what keeps that below the line while its silhouette still fires hard.
    /// </para>
    /// </remarks>
    public void DetectEdges(float threshold)
    {
        if (EdgeStyle == SilhouetteStyle.None || PixelWidth < 3 || PixelHeight < 3) return;
        if (edge.Length != color.Length) edge = new bool[color.Length];
        Array.Clear(edge);

        for (var y = 1; y < PixelHeight - 1; y++)
        {
            var row = y * PixelWidth;
            for (var x = 1; x < PixelWidth - 1; x++)
            {
                var i = row + x;
                var d = depth[i];
                if (d <= 0) continue;   // background: an edge belongs to the surface, not to the sky behind it
                if (group[i] == 0) continue;   // scenery is not outlined; see TestAndSet

                var bendX = MathF.Abs((2f * d) - depth[i - 1] - depth[i + 1]);
                var bendY = MathF.Abs((2f * d) - depth[i - PixelWidth] - depth[i + PixelWidth]);
                if (MathF.Max(bendX, bendY) <= threshold * d) continue;

                edge[i] = true;
                if (EdgeStyle == SilhouetteStyle.Ink)
                {
                    var c = color[i];
                    color[i] = new CColor((byte)(c.Red * InkFactor), (byte)(c.Green * InkFactor), (byte)(c.Blue * InkFactor));
                }
            }
        }
    }

    /// <summary>
    /// Darkens sub-pixels that sit in a crevice — creases, and where a body meets the ground — by asking, for a ring
    /// of neighbours, whether anything intrudes in front of the local surface.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The cheap screen-space stand-in for the ambient occlusion a signed distance field gives away, and it leans on
    /// the same property <see cref="DetectEdges"/> does. The local inverse-depth <b>gradient</b>, estimated from the
    /// immediate neighbours, is <em>exact</em> on a plane, so extrapolating it predicts precisely where the surface
    /// should be at each ring sample. A sample nearer than that prediction is something genuinely sticking out in
    /// front, not just the surface receding — which is what distinguishes a corner from a floor viewed at a grazing
    /// angle, and is the distinction a naive depth comparison cannot make.
    /// </para>
    /// <para>
    /// Only reads depth and only writes colour, so it can run before <see cref="DetectEdges"/> without feeding back
    /// into it.
    /// </para>
    /// </remarks>
    public void ApplyContactShading(float strength)
    {
        if (strength <= 0f || PixelWidth < 5 || PixelHeight < 5) return;

        for (var y = 2; y < PixelHeight - 2; y++)
        {
            var row = y * PixelWidth;
            for (var x = 2; x < PixelWidth - 2; x++)
            {
                var i = row + x;
                var d = depth[i];
                if (d <= 0) continue;

                // Central differences: the exact screen-space gradient of 1/z for any planar surface.
                var gx = (depth[i + 1] - depth[i - 1]) * 0.5f;
                var gy = (depth[i + PixelWidth] - depth[i - PixelWidth]) * 0.5f;

                var occluded = 0;
                foreach (var (ox, oy) in ContactRing)
                {
                    var sx = x + ox;
                    var sy = y + oy;
                    if ((uint)sx >= (uint)PixelWidth || (uint)sy >= (uint)PixelHeight) continue;

                    var sample = depth[(sy * PixelWidth) + sx];
                    if (sample <= 0) continue;   // background is infinitely far and occludes nothing

                    var predicted = d + (gx * ox) + (gy * oy);
                    if (sample > predicted + (ContactBias * d)) occluded++;
                }

                if (occluded == 0) continue;
                var factor = 1f - (strength * occluded / ContactRing.Length);
                var c = color[i];
                color[i] = new CColor((byte)(c.Red * factor), (byte)(c.Green * factor), (byte)(c.Blue * factor));
            }
        }
    }

    /// <summary>Whether <see cref="DetectEdges"/> marked this sub-pixel.</summary>
    public bool EdgeAt(int x, int y) =>
        (uint)x < (uint)PixelWidth && (uint)y < (uint)PixelHeight && edge.Length == color.Length && edge[(y * PixelWidth) + x];

    /// <summary>Composites the sub-pixel buffer into character cells and asks for a repaint.</summary>
    public void EndFrame() => Invalidate();
    #endregion

    #region Protected methods
    // A viewport, not a document: it must be exactly as tall as the frame's visible area, or a wrapping ControlFrame
    // hands it the unbounded scroll height and it balloons to the 1000-row clamp.
    /// <inheritdoc/>
    protected override bool FillsFrameViewport => true;

    /// <inheritdoc/>
    protected override void Render()
    {
        // Two sub-pixel rows per character row: the upper is the glyph's foreground, the lower its background.
        // Emitting one glyph for both halves is what buys the doubled vertical resolution.
        var rows = Math.Min(ActualHeight, PixelHeight / 2);
        var glyphEdges = EdgeStyle == SilhouetteStyle.Glyph && edge.Length == color.Length;
        for (var row = 0; row < rows; row++)
        {
            var top = row * 2 * PixelWidth;
            var bottom = top + PixelWidth;
            for (var x = 0; x < PixelWidth; x++)
            {
                var upper = color[top + x];
                var lower = color[bottom + x];
                if (glyphEdges && (edge[top + x] || edge[bottom + x]))
                {
                    // Either half being an edge marks the whole cell — a glyph cannot say "the top half only".
                    // Brighter colour to the glyph, darker behind it, so the outline reads against its own surface.
                    var (fg, bg) = Luminance(upper) >= Luminance(lower) ? (upper, lower) : (lower, upper);
                    // Boost rather than inherit. An outline that merely follows its surface disappears exactly where
                    // it is most needed: a sleeping body is drawn at a third brightness, so its silhouette came out
                    // as the faintest glyph in a dark colour on a dark background — present, and invisible.
                    var ink = Brighten(fg, EdgeBoost);
                    consoleBuffer.Write(new Position(x, row), new CCharacter(EdgeGlyph(Luminance(ink)), ink, bg));
                    continue;
                }

                consoleBuffer.Write(new Position(x, row), new CCharacter('▀', upper, lower));
            }
        }
    }
    #endregion

    // Brightness-ordered, echoing the ramp in c_ascii_render: a denser glyph for a brighter edge.
    private static char EdgeGlyph(float luminance) => luminance switch
    {
        > 0.62f => '◆',
        > 0.44f => '◇',
        > 0.26f => '◈',
        > 0.12f => '◊',
        _ => '◌',
    };

    private static float Luminance(CColor c) => ((0.299f * c.Red) + (0.587f * c.Green) + (0.114f * c.Blue)) / 255f;

    // Scale toward white, with a floor so an almost-black surface still yields a visible outline.
    private static CColor Brighten(CColor c, float factor) => new(
        (byte)Math.Clamp((c.Red * factor) + EdgeFloor, 0, 255),
        (byte)Math.Clamp((c.Green * factor) + EdgeFloor, 0, 255),
        (byte)Math.Clamp((c.Blue * factor) + EdgeFloor, 0, 255));

    #region Fields
    private const float InkFactor = 0.35f;
    private const float EdgeBoost = 1.8f;
    private const int EdgeFloor = 60;

    // Sampled at a couple of sub-pixels out: near enough to catch a contact seam, far enough not to just re-measure
    // the gradient it was estimated from. Sub-pixels are square, so this is a real circle on screen.
    private static readonly (int X, int Y)[] ContactRing =
        [(3, 0), (-3, 0), (0, 3), (0, -3), (2, 2), (2, -2), (-2, 2), (-2, -2)];

    private const float ContactBias = 0.02f;

    // Empty until the first BeginFrame sizes them; the bounds checks in TestAndSet/Render cover that window, so they
    // need no null handling on the per-sub-pixel hot path.
    private CColor[] color = [];
    private float[] depth = [];
    private byte[] group = [];
    private bool[] edge = [];
    #endregion
}
