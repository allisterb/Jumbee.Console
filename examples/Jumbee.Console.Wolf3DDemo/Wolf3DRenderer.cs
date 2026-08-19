#nullable enable

namespace Jumbee.Console.Wolf3DDemo;

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
    /// <summary>Levels per colour channel to snap to; 0 leaves the palette untouched. 6 roughly halves ANSI bytes.</summary>
    public int QuantizeLevels { get; set; } = 6;

    /// <summary>Draws plane-one scenery as depth-sorted billboards.</summary>
    public bool DrawSprites { get; set; } = true;

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
        if (columns.Length != w) columns = new WallColumn[w];
        if (pixels.Length != w * h * 4) pixels = new byte[w * h * 4];

        // A sub-pixel is square without quadrant sampling and half as wide as tall with it, so the plane length that
        // leaves the world un-stretched is pixelAspect * W / 2H.
        var pixelAspect = 1.0 / surface.SamplesPerColumn;
        var camera = scene.GetCamera(AuthenticFov ? AuthenticPlaneLength : pixelAspect * w / (2.0 * h));

        Raycaster.Cast(scene.Map, scene.Doors, camera, columns);
        SoftwareRaycastRenderer.Render(columns, h, h, pixels, scene.WallTextures, scene.Palette);

        if (DrawSprites && scene.StaticObjects.Count > 0)
        {
            if (projected.Length < scene.StaticObjects.Count)
                projected = new ProjectedWorldSprite[scene.StaticObjects.Count];
            var visible = WorldSpriteProjector.Project(scene.StaticObjects, camera, w, h, h, projected);
            SoftwareRaycastRenderer.DrawWorldSprites(
                projected.AsSpan(0, visible), scene.Sprites, scene.Palette, columns, pixels, w, h);
        }

        Blit(surface, w, h);
        surface.EndFrame();
    }

    private void Blit(HalfBlockSurface surface, int w, int h)
    {
        // Depth is already resolved in the framebuffer, so every sub-pixel goes in at the same reciprocal depth and
        // the z-test always passes. The surface is a compositor here, not a z-buffer.
        var quantize = QuantizeLevels > 1;
        var step = quantize ? 255.0 / (QuantizeLevels - 1) : 0.0;
        distinct.Clear();
        for (var y = 0; y < h; y++)
        {
            for (var x = 0; x < w; x++)
            {
                var o = ((y * w) + x) * 4;
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
    private const double AuthenticPlaneLength = 0.66;
    private readonly Wolf3DScene scene;
    private readonly HashSet<int> distinct = [];
    private WallColumn[] columns = [];
    private byte[] pixels = [];
    private ProjectedWorldSprite[] projected = [];
    #endregion
}
