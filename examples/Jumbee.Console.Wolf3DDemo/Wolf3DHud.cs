#nullable enable

namespace Jumbee.Console.Wolf3DDemo;

using Wolfenshine.Game;
using Wolfenshine.Graphics;

using CColor = ConsoleGUI.Data.Color;

/// <summary>
/// The original 320x40 status bar, composed by the vendored engine and blitted through its own
/// <see cref="HalfBlockSurface"/> under the viewport.
/// </summary>
/// <remarks>
/// <para>
/// The engine already draws this: <see cref="WolfensteinHudGraphics.Render"/> stamps the face, weapon icon, keys
/// and five bitmap numbers onto a copy of the status-bar picture and hands back one indexed 320x40 image. So the
/// only work here is scale and palette — which is exactly where it gets interesting, because the HUD's numbers are
/// <b>8x16 pixel bitmaps</b> and a terminal cell is not a pixel.
/// </para>
/// <para>
/// The scale is isotropic and taken from the width, which reproduces the original's proportions: there a 320x160
/// view sits above a 320x40 bar, so the bar is a quarter of the view's height, and deriving both from one factor
/// keeps that true at any terminal size. See <see cref="RowsFor"/>.
/// </para>
/// <para>
/// Sampled nearest, never averaged, and unlike the viewport's <see cref="RowFilter"/> that is not a close call:
/// this is line art with single-pixel strokes, where averaging turns a digit's stem into two half-bright columns
/// and takes the legibility with it. The viewport's dense texture wants the opposite. Same renderer, opposite
/// answer, decided entirely by content.
/// </para>
/// </remarks>
public sealed class Wolf3DHud : CompositeControl
{
    #region Constructors
    /// <summary>Creates the status bar over <paramref name="scene"/>'s palette and HUD graphics.</summary>
    public Wolf3DHud(Wolf3DScene scene)
    {
        this.scene = scene;
        SetContent(new Boundary(surface));
    }
    #endregion

    #region Properties
    /// <summary>Ammunition shown in the bar.</summary>
    public int Ammo { get => ammo; set => Set(ref ammo, Math.Clamp(value, 0, 99)); }

    /// <summary>Health percentage shown in the bar.</summary>
    public int Health { get => health; set => Set(ref health, Math.Clamp(value, 0, 100)); }

    /// <summary>Score shown in the bar.</summary>
    public int Score { get => score; set => Set(ref score, Math.Max(0, value)); }

    /// <summary>Lives shown in the bar.</summary>
    public int Lives { get => lives; set => Set(ref lives, Math.Clamp(value, 0, 9)); }

    /// <summary>Which of the 23 face pictures to show: 0-20 are the health faces, 21 dead, 22 the chaingun grin.</summary>
    public int Face { get => face; set => Set(ref face, Math.Clamp(value, 0, 22)); }

    /// <summary>
    /// Samples twice per column and composites each 2x2 block into a quadrant glyph, as the viewport's
    /// <see cref="SurfaceMode.Quadrant"/> does.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Follows the viewport's Surface setting.</b> This went back and forth: coupled at first for visual
    /// consistency, then decoupled and forced on once measuring showed half block makes the bar's small labels
    /// unreadable, and now coupled again — because a display toggle that visibly does nothing to a third of the
    /// screen is a worse fault than an ugly bar. The demo exists to show the trade; pressing <c>2</c> should let
    /// you SEE why the bar wants quadrant rather than quietly protect you from finding out.
    /// </para>
    /// <para>
    /// The measurement stands and is worth knowing. Worth far more here than on the viewport: a letterform is
    /// horizontal stroke placement and almost nothing else, so doubling the horizontal sample rate is aimed
    /// straight at what the labels are losing, where on dense wall texture the same doubling buys detail nobody
    /// was reading. At 168 columns it is the difference between FLOOR SCORE LIVES HEALTH AMMO and five smudges,
    /// for 56 → 63 colours.
    /// </para>
    /// </remarks>
    public bool QuadrantSampling
    {
        get => surface.QuadrantSampling;
        set
        {
            if (surface.QuadrantSampling == value) return;
            surface.QuadrantSampling = value;
            drawnState = -1;   // the sub-pixel grid changed, so the cached picture no longer describes it
            Refresh();
        }
    }

    /// <summary>Levels per colour channel to snap to; 0 or 1 leaves the palette untouched. Follows the viewport's
    /// Quantize setting.</summary>
    /// <remarks>
    /// The bar is a much worse case for the quantiser than the scene is — it is a handful of flat, deliberately
    /// chosen colours rather than dense texture, so coarse levels shift the panel blue and the label blue toward
    /// each other and the labels lose contrast rather than merely banding. That is precisely what a demo about
    /// colour cost should let you look at, so it follows the toggle instead of opting out of it.
    /// </remarks>
    public int QuantizeLevels
    {
        get => quantizeLevels;
        set
        {
            if (quantizeLevels == value) return;
            quantizeLevels = value;
            drawnState = -1;   // the palette changed, so the cached picture no longer describes it
            Refresh();
        }
    }

    /// <summary>Distinct colours in the last bar drawn — the HUD's own share of a frame's ANSI cost.</summary>
    public int LastColors { get; private set; }
    #endregion

    #region Methods
    /// <summary>Rows the bar occupies at <paramref name="columns"/> wide, keeping the original's proportions.</summary>
    /// <remarks>
    /// Derived rather than fixed: the bar is 40 of the original's 200 scanlines and the view is the other 160, so
    /// sizing it from the same factor as the view keeps it a quarter of the view's height however wide the terminal
    /// is. A constant would be right at one size and wrong at every other.
    /// </remarks>
    public static int RowsFor(int columns) =>
        Math.Max(3, (int)Math.Round(columns * HudHeight / (double)HudWidth / 2.0));

    /// <summary>
    /// Redraws the bar if anything it shows — or the space it has to show it in — has changed since the last call.
    /// </summary>
    /// <remarks>
    /// Called every frame from the viewport's tick, but the guard is what makes that cheap: the HUD changes on a
    /// pickup or a shot, not on a step, so in the ordinary case this compares two ints and returns. Without it the
    /// bar would re-blit ~6,000 sub-pixels a frame to produce the identical picture, and every one of those cells
    /// would be marked dirty for the compositor.
    /// </remarks>
    public void Refresh()
    {
        // Self-heal: if the pixel buffers no longer describe the surface's current size, the picture on screen was
        // drawn for a different geometry and must be redrawn whatever the state hash says. BeginFrame sizes those
        // buffers from the surface's size at the moment it runs, so any ordering that resizes the surface between
        // one draw and the next leaves them stale -- and the hash below cannot see it, because every value it
        // hashes is unchanged.
        //
        // This is the invariant rather than a guess at an ordering. Two orderings have already produced it (a
        // Height set mid-draw, and the requested-then-real size dance UI.Start does at startup), and a check on
        // "do the buffers match the control" catches any third without needing to know what it is.
        var samples = Math.Max(1, surface.SamplesPerColumn);
        if (surface.PixelWidth != surface.ActualWidth * samples || surface.PixelHeight != surface.ActualHeight * 2)
            drawnState = -1;

        // Keyed on the SURFACE's size, not this control's. Reflow sets Height, but the surface inside only picks
        // that up on the NEXT layout pass -- so a hash over this control's ActualHeight goes stale a pass early:
        // the bar redraws once at the size the surface still had, then never again because the hash already
        // matches. On screen that is a bar scaled to the wrong height and clipped by whatever is below it, which
        // is exactly what shipped. The surface's own size is the thing the blit actually maps onto, so keying on
        // it means the redraw happens on the pass where it is finally true.
        var state = HashCode.Combine(ammo, health, score, lives, face, surface.ActualWidth, surface.ActualHeight);
        if (state == drawnState) return;
        if (Draw()) drawnState = state;
    }

    /// <summary>
    /// Sets the bar's height for the width it currently has. Call on a terminal resize, <b>never from inside a
    /// frame's draw</b>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Separate from <see cref="Refresh"/>, and the separation is the whole point. The first version derived the
    /// height inside Refresh, which the viewport calls from its per-frame tick — so every frame mutated a control's
    /// Height in the middle of compositing that same frame. A <c>DockPanel</c> re-lays-out on that, and the result
    /// was not a subtle glitch: the viewport went black, the bar drew past the footer, and the overlay composited
    /// garbage over the sidebar. Layout mutation belongs on the paint hook, guarded by an actual size change, which
    /// is the pattern the AudioScope demo's split reproportioning already uses.
    /// </para>
    /// <para>
    /// Height-from-width inside a dock is exactly the feedback shape <c>docs/internal</c> records as the
    /// DockPanel/Boundary convergence trap. It is safe only because it happens once per resize rather than once per
    /// frame, and because it returns immediately when the width has not moved.
    /// </para>
    /// </remarks>
    public void Reflow()
    {
        if (ActualWidth <= 0) return;
        var rows = RowsFor(ActualWidth);
        // Compared against the CURRENT Height, not against the width last seen. The first version latched on the
        // width -- "same width as last time, nothing to do" -- which is wrong whenever the width it latched was
        // transient. UI.Start resizes to the app's requested 200x52 before the real window size takes over, so a
        // small terminal lays out wide once, the bar sizes itself for that, and the latch then refuses to correct
        // it: the bar stayed wrong until the user resized the window by hand.
        //
        // Comparing Height still keeps the common case to one integer test and still avoids re-laying-out on a
        // frame where nothing moved, which is the only thing the guard was ever needed for. It just cannot get
        // stuck, because it re-derives from whatever the width is now rather than from a remembered one.
        if (Height != rows) Height = rows;
    }
    #endregion

    #region Private methods
    private bool Draw()
    {
        if (!surface.BeginFrame()) return false;

        // Composed fresh rather than cached: the engine copies the background and stamps ~10 small pictures onto it,
        // which is far under the blit below, and a cache would need every setter above to know to invalidate it.
        var picture = scene.Hud.Render(PlayerWeapon.Pistol, ammo, score, health, face, lives);

        // Same snap the viewport uses (Wolf3DRenderer.Blit), so the two halves of the screen quantise alike.
        var quantize = quantizeLevels > 1;
        var step = quantize ? 255.0 / (quantizeLevels - 1) : 0.0;
        var pw = surface.PixelWidth;
        var ph = surface.PixelHeight;
        distinct.Clear();
        for (var y = 0; y < ph; y++)
        {
            // Nearest on both axes -- see the class remarks. Hoisted out of the inner loop so the row divide is not
            // paid per sub-pixel.
            var sy = Math.Min(HudHeight - 1, y * HudHeight / ph);
            for (var x = 0; x < pw; x++)
            {
                var sx = Math.Min(HudWidth - 1, x * HudWidth / pw);
                var rgba = scene.Palette.GetColor(picture.GetIndex(sx, sy));
                byte r = rgba.Red, g = rgba.Green, b = rgba.Blue;
                if (quantize)
                {
                    r = (byte)(Math.Round(r / step) * step);
                    g = (byte)(Math.Round(g / step) * step);
                    b = (byte)(Math.Round(b / step) * step);
                }

                distinct.Add((r << 16) | (g << 8) | b);
                surface.TestAndSet(x, y, 1f, new CColor(r, g, b));
            }
        }

        LastColors = distinct.Count;
        surface.EndFrame();
        return true;
    }

    private void Set(ref int field, int value)
    {
        if (field == value) return;
        field = value;
        Refresh();
    }
    #endregion

    #region Fields
    private const int HudWidth = 320, HudHeight = 40;
    private readonly Wolf3DScene scene;
    // Quadrant from construction: see the QuadrantSampling remarks -- the labels are unreadable without it.
    private readonly HalfBlockSurface surface = new() { Background = new CColor(0, 0, 0), QuadrantSampling = true };
    private readonly HashSet<int> distinct = [];
    private int ammo = 8;
    private int health = 100;
    private int score;
    private int lives = 3;
    private int face;
    private int quantizeLevels = Wolf3DRenderer.DefaultQuantizeLevels;
    private int drawnState = -1;
    #endregion
}
