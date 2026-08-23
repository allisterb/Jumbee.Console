namespace Jumbee.Console.SandboxDemo;

using System.Threading;

using ConsoleGUI.Space;

using CCharacter = ConsoleGUI.Data.Character;
using CColor = ConsoleGUI.Data.Color;

/// <summary>How <see cref="HalfBlockSurface"/> draws detected silhouettes and creases.</summary>
public enum SilhouetteStyle
{
    /// <summary>No edge treatment.</summary>
    None,

    /// <summary>
    /// Brighten the edge sub-pixels in place. Keeps the surface's doubled vertical resolution.
    /// </summary>
    /// <remarks>
    /// Brightens rather than darkens, and matching <see cref="Glyph"/>'s direction is the point: the two styles are
    /// meant to differ in <em>resolution</em>, not colour. This one darkened for a while, purely because it was
    /// written before the glyph path learned to boost, and the resulting colour inversion between the two swamped
    /// the difference they exist to show — they read as unrelated features rather than two ways of drawing one
    /// outline.
    /// </remarks>
    Line,

    /// <summary>
    /// Replace the cell with a shaped glyph (<c>◆◇◈◊◌</c>), the technique in
    /// <c>reference/projects/c_ascii_render-main</c>.
    /// </summary>
    /// <remarks>
    /// A glyph carries one foreground and one background, so a cell drawn this way <b>gives up its two independent
    /// sub-pixels</b> — the edge lands on a half-resolution cell boundary rather than a half-cell one. That is free
    /// for a renderer sampling once per cell, as theirs does; here it is a genuine trade, which is why
    /// <see cref="Line"/> exists alongside it.
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
/// interpolate linearly in screen space; <see cref="HalfBlockSurface.TestAndSet"/> takes it in that form.
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
    /// <summary>Sub-pixel columns — <see cref="SamplesPerColumn"/> per character column.</summary>
    public int PixelWidth { get; private set; }

    /// <summary>Sub-pixel rows — two per character row.</summary>
    public int PixelHeight { get; private set; }

    /// <summary>Colour behind everything, used for sub-pixels nothing was drawn into.</summary>
    public CColor Background { get; set; } = new(12, 12, 18);

    /// <summary>How <see cref="HalfBlockSurface.DetectEdges"/>'s findings are drawn.</summary>
    public SilhouetteStyle EdgeStyle { get; set; } = SilhouetteStyle.None;

    /// <summary>
    /// Samples <b>twice per column</b> and composites each 2×2 block into the quadrant glyph (<c>▘▝▖▗▌▐▞▚</c>…)
    /// that best fits its four colours — doubling the surface's horizontal resolution. Off by default; takes
    /// effect at the next <see cref="BeginFrame"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A cell can carry exactly two colours, and the sixteen quadrant glyphs are exactly the sixteen ways to split a
    /// 2×2 block between them. So the compositor's whole job is picking the split: the two-means partition of the
    /// four sub-pixels, cheapest first. <c>▀</c> is one of those sixteen, and it wins whenever the block's structure
    /// really is a horizontal one — which is why this can only <em>add</em> resolution and never trade the vertical
    /// resolution away.
    /// </para>
    /// <para>
    /// <b>It introduces no new colour.</b> The two it emits are <em>members</em> of the block (each group's medoid),
    /// never a blend of them, so the picture stays on whatever quantised ramp <see cref="MeshRenderer.ShadeLevels"/>
    /// produced.
    /// </para>
    /// <para>
    /// That is the difference from the pass this replaced. A <c>SmoothEdges</c> post-process blended each detected
    /// edge sub-pixel toward its neighbours, which bought its softening in intermediate shades and — because it
    /// blends across the sub-pixel grid rather than subdividing it — <em>softened</em> the staircase without
    /// <em>moving</em> it: measured, the silhouette's placement error got slightly worse, not better. It was removed
    /// once this existed. See the demo README for the numbers.
    /// </para>
    /// <para>
    /// What it costs is <b>fill</b> — twice the sub-pixels to rasterise and shade — and, less obviously, <b>runs</b>.
    /// Measured on the sandbox at 200×50: shaded 2.9 → 4.4 ms and 18.1 → 25.7 KB of ANSI a frame, solid 1.7 → 2.4 ms
    /// and 12.0 → 17.4 KB. The emission rises even with the palette unchanged, because a boundary that now falls
    /// <em>between</em> two columns makes a cell differ from its neighbour where the two used to coalesce. Same
    /// currency the shade ramp is spent in, for the opposite kind of detail.
    /// </para>
    /// <para>
    /// The compositing itself is the cheap part: a block whose two columns agree — every flat interior, which is
    /// most of the screen — short-circuits straight to <c>▀</c>, so the partition search runs along boundaries only.
    /// </para>
    /// <para>
    /// It needs no edge detector, which is the second thing it gains: a boundary that is only a change of
    /// <em>colour</em> — the checkerboard's squares, a shade-band contour — gets the same half-cell precision a
    /// silhouette does. A depth-based detector cannot see those at all.
    /// </para>
    /// </remarks>
    public bool QuadrantSampling { get; set; }

    /// <summary>Sub-pixels per character column: 2 under <see cref="QuadrantSampling"/>, otherwise 1. Fixed for the
    /// frame at <see cref="BeginFrame"/>, so a mid-frame toggle cannot tear the buffer against the compositor.</summary>
    public int SamplesPerColumn { get; private set; } = 1;
    #endregion

    #region Methods
    /// <summary>
    /// Takes a frame to draw into, sized to <paramref name="cells"/>×<paramref name="rows"/> character cells and
    /// cleared. Call once before drawing; returns <see langword="false"/> if that size has no area.
    /// </summary>
    /// <remarks>
    /// <b>The size is passed in rather than read from the control</b>, because this may run off the UI thread —
    /// see the threading note on the class. The caller captures <see cref="Control.ActualWidth"/>/
    /// <see cref="Control.ActualHeight"/> on the UI thread and hands them over, so a re-layout mid-rasterisation
    /// cannot change the geometry underneath a frame that is half drawn.
    /// </remarks>
    public bool BeginFrame(int cells, int rows)
    {
        var samples = QuadrantSampling ? 2 : 1;
        var w = cells * samples;
        var h = rows * 2;
        if (w <= 0 || h <= 0) return false;

        // Rent exclusively: whatever is in the spare slot is a frame the UI thread has finished painting and handed
        // back. Taking it with an exchange means two rasterisations can never be handed the same buffer, and an
        // empty slot simply costs an allocation rather than a race.
        var frame = Interlocked.Exchange(ref spare, null) ?? new SurfaceFrame();
        if (frame.Color.Length != w * h)
        {
            frame.Color = new CColor[w * h];
            frame.Depth = new float[w * h];
            frame.Group = new byte[w * h];
            frame.Edge = [];
        }

        frame.Width = w;
        frame.Height = h;
        frame.Samples = samples;
        current = frame;

        // The drawing path addresses these directly, so point them at the rented frame and leave the rasteriser and
        // its post-processes exactly as they were -- they write the frame in flight and never the one being painted.
        PixelWidth = w;
        PixelHeight = h;
        // After the resize, not before: the compositor reads this to map cells back to sub-pixels, so it has to
        // describe the buffer that is about to be drawn into rather than the property's current value.
        SamplesPerColumn = samples;
        color = frame.Color;
        depth = frame.Depth;
        group = frame.Group;
        edge = frame.Edge;

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
    /// What kind of surface this is: 0 for scenery, non-zero for a body. <see cref="HalfBlockSurface.DetectEdges"/> outlines only
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
        // Self-gated again. It took a `wanted` flag while the smoothing pass consumed the same edge set and could
        // want it with the outline off; with that pass gone the outline is the only consumer, so the surface's own
        // EdgeStyle is the whole answer.
        if (EdgeStyle == SilhouetteStyle.None || PixelWidth < 3 || PixelHeight < 3) return;
        if (edge.Length != color.Length) edge = new bool[color.Length];
        Array.Clear(edge);

        // A second difference scales with the SQUARE of the sample spacing, so under QuadrantSampling — where a
        // column step is half the distance it was — the x term would read a quarter of its old value and quietly
        // recalibrate the threshold. Scaled back so one threshold means one thing at either sampling rate.
        var bendScaleX = SamplesPerColumn * SamplesPerColumn;

        for (var y = 1; y < PixelHeight - 1; y++)
        {
            var row = y * PixelWidth;
            for (var x = 1; x < PixelWidth - 1; x++)
            {
                var i = row + x;
                var d = depth[i];
                if (d <= 0) continue;   // background: an edge belongs to the surface, not to the sky behind it
                if (group[i] == 0) continue;   // scenery is not outlined; see TestAndSet

                var bendX = MathF.Abs((2f * d) - depth[i - 1] - depth[i + 1]) * bendScaleX;
                var bendY = MathF.Abs((2f * d) - depth[i - PixelWidth] - depth[i + PixelWidth]);
                if (MathF.Max(bendX, bendY) <= threshold * d) continue;

                edge[i] = true;
                // Same boost the glyph path applies, for the same reason: an outline that merely follows its
                // surface disappears exactly where it is most needed — on the unlit side of a body, where it would
                // be dark on dark.
                if (EdgeStyle == SilhouetteStyle.Line) color[i] = Brighten(color[i], EdgeBoost);
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
    /// the same property <see cref="HalfBlockSurface.DetectEdges"/> does. The local inverse-depth <b>gradient</b>, estimated from the
    /// immediate neighbours, is <em>exact</em> on a plane, so extrapolating it predicts precisely where the surface
    /// should be at each ring sample. A sample nearer than that prediction is something genuinely sticking out in
    /// front, not just the surface receding — which is what distinguishes a corner from a floor viewed at a grazing
    /// angle, and is the distinction a naive depth comparison cannot make.
    /// </para>
    /// <para>
    /// Only reads depth and only writes colour, so it can run before <see cref="HalfBlockSurface.DetectEdges"/> without feeding back
    /// into it.
    /// </para>
    /// </remarks>
    public void ApplyOcclusion(float strength)
    {
        // The ring is a circle on SCREEN, so its horizontal reach is in sub-pixels only while sub-pixels are square.
        // Under QuadrantSampling they are half as wide, and an unscaled ring would sample an ellipse half as wide as
        // it is tall — measuring a different neighbourhood at the same dial setting.
        var reachX = SamplesPerColumn;
        var marginX = 2 * reachX;
        if (strength <= 0f || PixelWidth < 2 * marginX + 1 || PixelHeight < 5) return;

        for (var y = 2; y < PixelHeight - 2; y++)
        {
            var row = y * PixelWidth;
            for (var x = marginX; x < PixelWidth - marginX; x++)
            {
                var i = row + x;
                var d = depth[i];
                if (d <= 0) continue;

                // Central differences: the exact screen-space gradient of 1/z for any planar surface.
                var gx = (depth[i + 1] - depth[i - 1]) * 0.5f;
                var gy = (depth[i + PixelWidth] - depth[i - PixelWidth]) * 0.5f;

                var occluded = 0;
                foreach (var (ox, oy) in OcclusionRing)
                {
                    var stepX = ox * reachX;
                    var sx = x + stepX;
                    var sy = y + oy;
                    if ((uint)sx >= (uint)PixelWidth || (uint)sy >= (uint)PixelHeight) continue;

                    var sample = depth[(sy * PixelWidth) + sx];
                    if (sample <= 0) continue;   // background is infinitely far and occludes nothing

                    // gx is per sub-pixel column, so the prediction extrapolates over the SCALED step — the same
                    // screen distance the sample was taken at.
                    var predicted = d + (gx * stepX) + (gy * oy);
                    if (sample > predicted + (OcclusionBias * d)) occluded++;
                }

                if (occluded == 0) continue;
                var factor = 1f - (strength * occluded / OcclusionRing.Length);
                var c = color[i];
                color[i] = new CColor((byte)(c.Red * factor), (byte)(c.Green * factor), (byte)(c.Blue * factor));
            }
        }
    }

    /// <summary>Whether <see cref="HalfBlockSurface.DetectEdges"/> marked this sub-pixel.</summary>
    public bool EdgeAt(int x, int y) =>
        (uint)x < (uint)PixelWidth && (uint)y < (uint)PixelHeight && edge.Length == color.Length && edge[(y * PixelWidth) + x];

    /// <summary>Composites the sub-pixel buffer into character cells and asks for a repaint.</summary>
    /// <summary>
    /// Finishes the frame started by <see cref="BeginFrame"/> and returns it, ready to hand to <see cref="Publish"/>
    /// on the UI thread. Returns <see langword="null"/> if no frame was started.
    /// </summary>
    /// <remarks>
    /// It <b>returns</b> the frame rather than installing it, and that is the whole thread-safety argument: a
    /// rasteriser running off the UI thread never touches what the paint path reads, and hand-off happens by value
    /// at a point the UI thread controls. Publishing from here instead would race a paint already in progress.
    /// </remarks>
    public SurfaceFrame? EndFrame()
    {
        var finished = current;
        // Post-processes may have grown the edge buffer; keep it with its frame so the compositor sees it.
        if (finished is not null) finished.Edge = edge;
        current = null;
        return finished;
    }

    /// <summary>
    /// Installs a finished frame as the one being displayed and asks for a repaint. <b>UI thread only.</b>
    /// </summary>
    /// <remarks>
    /// The previous frame goes back into the spare slot for the next <see cref="BeginFrame"/> to rent, so a steady
    /// state of one frame on screen and one being drawn allocates nothing.
    /// </remarks>
    public void Publish(SurfaceFrame? frame)
    {
        if (frame is null) return;

        var previous = front;
        front = frame;
        Invalidate();
        Volatile.Write(ref spare, previous);
    }
    #endregion

    #region Protected methods

    /// <inheritdoc/>
    protected override void Render()
    {
        // PAINT THE PUBLISHED FRAME, never the one being drawn. Everything below reads `f` and its own dimensions
        // rather than the surface's fields, because those describe whatever a background rasteriser is filling in
        // right now -- reading them here is how a half-drawn frame reaches the screen.
        var f = front;
        var color = f.Color;
        var edge = f.Edge;
        var pixelWidth = f.Width;

        // Two sub-pixel rows per character row: the upper is the glyph's foreground, the lower its background.
        // Emitting one glyph for both halves is what buys the doubled vertical resolution.
        // Clamped on BOTH axes against the control's CURRENT size, not just the pixel buffer's. The buffer is sized
        // at BeginFrame; a re-layout between that and this paint can leave the control SMALLER than the frame drawn
        // into it -- collapsing the sidebar and restoring it does exactly that, and the write then runs off the end
        // of the console buffer. The row clamp was already here; the column one was not, and that asymmetry was the
        // bug: hiding the sidebar with `u` while a solid renderer was active could take the app down.
        var samples = Math.Max(1, f.Samples);
        var rows = Math.Min(ActualHeight, f.Height / 2);
        var cols = Math.Min(ActualWidth, pixelWidth / samples);
        var glyphEdges = EdgeStyle == SilhouetteStyle.Glyph && edge.Length == color.Length;
        for (var row = 0; row < rows; row++)
        {
            var top = row * 2 * pixelWidth;
            var bottom = top + pixelWidth;
            for (var x = 0; x < cols; x++)
            {
                var left = x * samples;
                var upperIndex = top + left;
                var lowerIndex = bottom + left;
                var upper = color[upperIndex];
                var lower = color[lowerIndex];
                if (glyphEdges && (edge[upperIndex] || edge[lowerIndex]
                    || (samples == 2 && (edge[upperIndex + 1] || edge[lowerIndex + 1]))))
                {
                    // Either half being an edge marks the whole cell — a glyph cannot say "the top half only".
                    // Brighter colour to the glyph, darker behind it, so the outline reads against its own surface.
                    var (fg, bg) = Luminance(upper) >= Luminance(lower) ? (upper, lower) : (lower, upper);
                    // Boost rather than inherit. An outline that merely follows its surface disappears exactly where
                    // it is most needed: on the unlit side of a body, where the surface is already near ambient and
                    // the background behind it is dark -- present in the buffer, invisible on screen.
                    var ink = Brighten(fg, EdgeBoost);
                    consoleBuffer.Write(new Position(x, row), new CCharacter(EdgeGlyph(Luminance(ink)), ink, bg));
                    continue;
                }

                if (samples == 2)
                {
                    var upperRight = color[upperIndex + 1];
                    var lowerRight = color[lowerIndex + 1];
                    // The short circuit that keeps the search off the interior: when a row's two samples agree
                    // there is nothing horizontal to resolve, and ▀ is already the exact answer -- which is the
                    // case for every flat plateau, so the partition below runs along boundaries only.
                    if (upper != upperRight || lower != lowerRight)
                    {
                        var (glyph, ink, behind) = Quadrant(upper, upperRight, lower, lowerRight);
                        consoleBuffer.Write(new Position(x, row), new CCharacter(glyph, ink, behind));
                        continue;
                    }
                }

                consoleBuffer.Write(new Position(x, row), new CCharacter('▀', upper, lower));
            }
        }
    }
    #endregion

    // The best two-colour rendition of one 2x2 block: the quadrant glyph whose split leaves the least colour spread
    // inside each of the two groups, with each group represented by one of its own members.
    //
    // Seven candidate splits, not sixteen. A mask and its complement describe the SAME partition with foreground and
    // background exchanged, and putting all four sub-pixels in one group can never win -- splitting a set never
    // increases its spread -- so it needs no candidate of its own; four equal colours simply tie at zero and the
    // first split wins, which draws the same solid cell either way.
    private static (char Glyph, CColor Fg, CColor Bg) Quadrant(CColor tl, CColor tr, CColor bl, CColor br)
    {
        Span<CColor> block = [tl, tr, bl, br];
        var best = 1;
        var bestSpread = float.MaxValue;
        for (var mask = 1; mask < 8; mask++)
        {
            var spread = Spread(block, mask) + Spread(block, ~mask & 0xF);
            if (spread >= bestSpread) continue;
            bestSpread = spread;
            best = mask;
        }

        return (QuadrantGlyphs[best], Medoid(block, best), Medoid(block, ~best & 0xF));
    }

    // Sum of squared distance from the group's own mean, over the three channels -- the two-means cost. Computed
    // from the running sums rather than in two passes: Σ(c²) - (Σc)²/n is the same quantity.
    private static float Spread(ReadOnlySpan<CColor> block, int mask)
    {
        int n = 0, sr = 0, sg = 0, sb = 0, qr = 0, qg = 0, qb = 0;
        for (var i = 0; i < 4; i++)
        {
            if ((mask & (1 << i)) == 0) continue;
            var c = block[i];
            n++;
            sr += c.Red;
            sg += c.Green;
            sb += c.Blue;
            qr += c.Red * c.Red;
            qg += c.Green * c.Green;
            qb += c.Blue * c.Blue;
        }

        return n == 0 ? 0f : qr + qg + qb - ((((float)sr * sr) + ((float)sg * sg) + ((float)sb * sb)) / n);
    }

    // The group member nearest the group's mean -- NOT the mean itself, which is the whole colour discipline of this
    // pass: a mean is a colour the renderer never produced, and a screen full of them is exactly the intermediate
    // shading ShadeLevels exists to suppress. Picking a member keeps every emitted colour on the quantised ramp.
    private static CColor Medoid(ReadOnlySpan<CColor> block, int mask)
    {
        int n = 0, sr = 0, sg = 0, sb = 0;
        for (var i = 0; i < 4; i++)
        {
            if ((mask & (1 << i)) == 0) continue;
            n++;
            sr += block[i].Red;
            sg += block[i].Green;
            sb += block[i].Blue;
        }

        if (n == 0) return default;
        float mr = (float)sr / n, mg = (float)sg / n, mb = (float)sb / n;

        var best = default(CColor);
        var bestDistance = float.MaxValue;
        for (var i = 0; i < 4; i++)
        {
            if ((mask & (1 << i)) == 0) continue;
            var c = block[i];
            var dr = c.Red - mr;
            var dg = c.Green - mg;
            var db = c.Blue - mb;
            var distance = (dr * dr) + (dg * dg) + (db * db);
            if (distance >= bestDistance) continue;
            bestDistance = distance;
            best = c;
        }

        return best;
    }

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
    // Indexed by the foreground mask, bit 0 top-left through bit 3 bottom-right. Only 1..7 are ever looked up (see
    // Quadrant), but the table is written in full because the index IS the pattern and a gap would invite a bug.
    private static readonly char[] QuadrantGlyphs =
        [' ', '▘', '▝', '▀', '▖', '▌', '▞', '▛', '▗', '▚', '▐', '▜', '▄', '▙', '▟', '█'];

    private const float EdgeBoost = 1.8f;
    private const int EdgeFloor = 60;

    // Sampled at a couple of sub-pixels out: near enough to catch a contact seam, far enough not to just re-measure
    // the gradient it was estimated from. Sub-pixels are square, so this is a real circle on screen.
    private static readonly (int X, int Y)[] OcclusionRing =
        [(3, 0), (-3, 0), (0, 3), (0, -3), (2, 2), (2, -2), (-2, 2), (-2, -2)];

    private const float OcclusionBias = 0.02f;

    // Aliases into the frame currently being drawn (see BeginFrame). Empty until the first BeginFrame sizes them;
    // the bounds checks in TestAndSet/Render cover that window, so they need no null handling on the per-sub-pixel
    // hot path. Only the rasterising thread touches these -- the paint path reads `front` instead.
    private CColor[] color = [];
    private float[] depth = [];
    private byte[] group = [];
    private bool[] edge = [];

    // The frame being drawn, the frame being displayed, and one kept for reuse. See the threading note on the class.
    private SurfaceFrame? current;
    private SurfaceFrame front = new();
    private SurfaceFrame? spare;
    #endregion
}

/// <summary>
/// One rasterised frame's sub-pixel buffers, passed from a rasteriser to the compositor by value.
/// </summary>
/// <remarks>
/// Its own type rather than fields on <see cref="HalfBlockSurface"/> so a frame can be drawn on one thread while
/// another is painted on the UI thread: the two never share an array, and hand-off is a single reference assignment
/// made on the UI thread in <see cref="HalfBlockSurface.Publish"/>. Top-level rather than nested because
/// <c>Control.Frame</c> already means a control's border adornment.
/// </remarks>
public sealed class SurfaceFrame
{
    /// <summary>Sub-pixel colours, row-major, <see cref="Width"/> per row.</summary>
    public CColor[] Color = [];

    /// <summary>Reciprocal depth per sub-pixel — larger is nearer, 0 is empty.</summary>
    public float[] Depth = [];

    /// <summary>Surface kind per sub-pixel: 0 scenery, non-zero body. See <see cref="HalfBlockSurface.TestAndSet"/>.</summary>
    public byte[] Group = [];

    /// <summary>Edge flags from <see cref="HalfBlockSurface.DetectEdges"/>, or empty when it did not run.</summary>
    public bool[] Edge = [];

    /// <summary>Sub-pixel columns.</summary>
    public int Width;

    /// <summary>Sub-pixel rows.</summary>
    public int Height;

    /// <summary>Sub-pixels per character column, 1 or 2.</summary>
    public int Samples = 1;
}
