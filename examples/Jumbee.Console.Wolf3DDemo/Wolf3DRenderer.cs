#nullable enable

namespace Jumbee.Console.Wolf3DDemo;

using Wolfenshine.Game;
using Wolfenshine.Rendering;

using CColor = ConsoleGUI.Data.Color;

/// <summary>
/// Draws a <see cref="Wolf3DScene"/> into a <see cref="HalfBlockSurface"/> — one ray per sub-pixel column, through
/// Wolfenshine's own renderer into an RGBA framebuffer, then blitted to the surface.
/// </summary>
/// <remarks>
/// <para>
/// The RGBA intermediate keeps <c>SoftwareRaycastRenderer</c> byte-for-byte the vendored original, so what reaches
/// the terminal is the reference engine's output rather than a reimplementation of it. It costs one W×H copy a
/// frame, which measured at well under the emit cost it sits beside.
/// </para>
/// <para>
/// <b>Colour quantisation is the lever that matters.</b> A frame's ANSI cost is proportional to the number of
/// <em>runs</em> — cells whose colour pair differs from the one before them — and Wolfenstein's art is dense
/// texture detail, so runs are short. Snapping colours to a coarse ramp merges neighbouring texels back into runs
/// and roughly halves both the run count and the bytes, for a picture that is very hard to tell apart. See the
/// demo README for the measurements.
/// </para>
/// </remarks>
public sealed class Wolf3DRenderer
{
    #region Constructors
    /// <summary>Creates a renderer over <paramref name="scene"/>.</summary>
    public Wolf3DRenderer(Wolf3DScene scene) => this.scene = scene;
    #endregion

    #region Properties
    /// <summary>Levels per colour channel to snap to; 0 or 1 leaves the palette untouched. 6 roughly halves ANSI bytes.</summary>
    public int QuantizeLevels { get; set; } = DefaultQuantizeLevels;

    /// <summary>Draws plane-one scenery as depth-sorted billboards.</summary>
    public bool DrawSprites { get; set; } = true;

    /// <summary>Draws the player's weapon over the scene, as the original always does.</summary>
    public bool DrawWeapon { get; set; } = true;

    /// <summary>Which of the weapon's five frames to draw; 0 is at rest. Set by the view's fire animation.</summary>
    public int WeaponFrame { get; set; }

    /// <summary>
    /// Use the original's projection plane (0.66, a 66° horizontal FOV) rather than one derived from the surface.
    /// </summary>
    /// <remarks>
    /// The original's 320×160 3D view is 2:1, which a half-block surface reproduces exactly at 200×50 cells — so on
    /// a typical terminal the authentic setting is a uniform downscale of the original frame rather than a
    /// reinterpretation of it. The derived plane assumes square sub-pixels and comes out near 90°: a wider view that
    /// fills an unusually wide terminal, at about a third more ANSI bytes.
    /// </remarks>
    public bool AuthenticFov { get; set; } = true;

    /// <summary>Distinct colours in the last frame drawn.</summary>
    public int LastColors { get; private set; }

    /// <summary>
    /// Cells in the last frame whose colour pair differs from the cell before them in scan order — the SGR changes
    /// the emitter has to write, and what ANSI bytes track. Zero when quadrant sampling is on, where a cell's pair
    /// is chosen by the compositor rather than being its two sub-pixels.
    /// </summary>
    public int LastRuns { get; private set; }
    #endregion

    #region Methods
    /// <summary>Renders one frame. Call on the UI thread.</summary>
    public void Draw(HalfBlockSurface surface)
    {
        if (!surface.BeginFrame()) return;

        var w = surface.PixelWidth;
        var h = surface.PixelHeight;

        // The framebuffer is rendered TALLER than the surface by exactly the horizontal sampling factor, then row-
        // sampled back down on the way out. That keeps a framebuffer pixel SQUARE in both modes, and squareness is
        // load-bearing: the vendored sprite projector computes a single RenderedSize and uses it for width and
        // height alike, so a sprite is a square in framebuffer space. Under quadrant sampling the surface's own
        // sub-pixels are twice as tall as they are wide, and drawing that square straight onto them rendered every
        // sprite — and the weapon — at half its proper width, while the walls (drawn per column from ray angles)
        // stayed correct and hid the cause.
        var rows = surface.SamplesPerColumn;
        var bufferHeight = h * rows;
        if (columns.Length != w) columns = new WallColumn[w];
        if (pixels.Length != w * bufferHeight * 4) pixels = new byte[w * bufferHeight * 4];

        // With square framebuffer pixels the un-stretched plane length is simply W / 2H, in both modes.
        var camera = scene.GetCamera(AuthenticFov ? AuthenticPlaneLength : w / (2.0 * bufferHeight));

        Raycaster.Cast(scene.Map, scene.Doors, camera, columns);
        SoftwareRaycastRenderer.Render(columns, bufferHeight, bufferHeight, pixels, scene.WallTextures, scene.Palette);

        if (DrawSprites && scene.StaticObjects.Count > 0)
        {
            if (projected.Length < scene.StaticObjects.Count)
                projected = new ProjectedWorldSprite[scene.StaticObjects.Count];
            var visible = WorldSpriteProjector.Project(
                scene.StaticObjects, camera, w, bufferHeight, bufferHeight, projected);
            SoftwareRaycastRenderer.DrawWorldSprites(
                projected.AsSpan(0, visible), scene.Sprites, scene.Palette, columns, pixels, w, bufferHeight);
        }

        // Last, and over everything: the weapon is screen furniture rather than part of the world, so it takes no
        // part in the depth sort. A square the height of the view, bottom-aligned and centred, which is the
        // original's own composition.
        if (DrawWeapon)
        {
            SoftwareRaycastRenderer.DrawSprite(
                scene.Sprites.GetWeaponFrame(PlayerWeapon.Pistol, Math.Clamp(WeaponFrame, 0, 4)),
                scene.Palette, w / 2, bufferHeight, bufferHeight + 1, pixels, w, bufferHeight);
        }

        Blit(surface, w, h, rows);
        surface.EndFrame();
    }

    private void Blit(HalfBlockSurface surface, int w, int h, int rows)
    {
        // Depth is already resolved in the framebuffer, so every sub-pixel goes in at the same reciprocal depth and
        // the z-test always passes. The surface is a compositor here, not a z-buffer.
        //
        // Row-SAMPLED, not averaged: the framebuffer is `rows` times taller than the surface, and taking one row of
        // each group keeps every emitted colour an exact palette entry. Averaging would invent colours between
        // palette entries, and this demo's whole cost model is that bytes track colour RUNS — new colours in the
        // middle of a wall would fragment them for no visible gain.
        var quantize = QuantizeLevels > 1;
        var step = quantize ? 255.0 / (QuantizeLevels - 1) : 0.0;
        distinct.Clear();
        for (var y = 0; y < h; y++)
        {
            for (var x = 0; x < w; x++)
            {
                var o = (((y * rows) * w) + x) * 4;
                byte r = pixels[o], g = pixels[o + 1], b = pixels[o + 2];
                if (quantize)
                {
                    r = (byte)(Math.Round(r / step) * step);
                    g = (byte)(Math.Round(g / step) * step);
                    b = (byte)(Math.Round(b / step) * step);
                    // Written back so the run count below measures the pixels actually emitted.
                    pixels[o] = r;
                    pixels[o + 1] = g;
                    pixels[o + 2] = b;
                }

                distinct.Add((r << 16) | (g << 8) | b);
                surface.TestAndSet(x, y, 1f, new CColor(r, g, b));
            }
        }

        LastColors = distinct.Count;
        LastRuns = surface.SamplesPerColumn == 1 ? CountRuns(w, h) : 0;
    }

    private int CountRuns(int w, int h)
    {
        var runs = 0;
        var previous = -1L;
        for (var cy = 0; cy < h / 2; cy++)
        {
            for (var cx = 0; cx < w; cx++)
            {
                var pair = ((long)Rgb(((cy * 2 * w) + cx) * 4) << 24) | (uint)Rgb((((cy * 2 + 1) * w) + cx) * 4);
                if (pair != previous) runs++;
                previous = pair;
            }
        }

        return runs;

        int Rgb(int o) => (pixels[o] << 16) | (pixels[o + 1] << 8) | pixels[o + 2];
    }
    #endregion

    #region Fields
    /// <summary>
    /// The shipped quantisation level, and what the Display tab's slider opens on.
    /// </summary>
    /// <remarks>
    /// Measured, not guessed. Against a 2×12 sub-pixel ground truth over six levels, 6 levels/channel sat at 155%
    /// of the full-palette colour error for 55% of the bytes, while <b>10 sits at 113% for 68%</b> — a 27% error
    /// reduction that still saves a third of the bandwidth, and the point at which the banding stops being
    /// obvious on a gradient. The sweep is also <em>non-monotonic</em> (10 beats 12), because the source art is
    /// itself on a lattice — the original VGA palette expands six-bit channels — so an even RGB grid lands well or
    /// badly depending on how it aligns. Pick this empirically; do not reason it upward.
    /// </remarks>
    public const int DefaultQuantizeLevels = 10;

    private const double AuthenticPlaneLength = 0.66;
    private readonly Wolf3DScene scene;
    private readonly HashSet<int> distinct = [];
    private WallColumn[] columns = [];
    private byte[] pixels = [];
    private ProjectedWorldSprite[] projected = [];
    #endregion
}
