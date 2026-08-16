namespace Jumbee.Console.Tests;

using ConsoleGUI.Data;
using ConsoleGUI.Space;

using Jumbee.Console;
using Jumbee.Console.Snapshot;

using Xunit;

using CColor = ConsoleGUI.Data.Color;
using S = Spectre.Console;

public class GlassPanelTests
{
    #region Blend math (GlassBlend)
    [Fact]
    public void Blend_GammaSpace_IsChannelMidpoint()
    {
        var mid = GlassBlend.Blend(new CColor(0, 0, 0), new CColor(254, 254, 254), 0.5f, gammaCorrect: false);
        Assert.Equal(127, mid.Red);   // plain lerp: 254 * 0.5
    }

    [Fact]
    public void Blend_Endpoints_AreExact()
    {
        var a = new CColor(10, 20, 30);
        var b = new CColor(200, 100, 50);
        Assert.Equal(a, GlassBlend.Blend(a, b, 0f, gammaCorrect: false));
        Assert.Equal(b, GlassBlend.Blend(a, b, 1f, gammaCorrect: true));
    }

    [Fact]
    public void Blend_GammaCorrect_IsBrighterThanGammaSpace_ButInRange()
    {
        var a = new CColor(0, 0, 0);
        var b = new CColor(255, 255, 255);
        var gamma = GlassBlend.Blend(a, b, 0.5f, gammaCorrect: false);
        var linear = GlassBlend.Blend(a, b, 0.5f, gammaCorrect: true);

        // A 50% mix of black and white in linear light comes out perceptually lighter than the naive gamma-space
        // midpoint (~188 vs 127), and every channel stays a valid byte.
        Assert.True(linear.Red > gamma.Red, $"linear {linear.Red} should exceed gamma {gamma.Red}");
        Assert.InRange(linear.Red, 0, 255);
    }

    [Theory]
    [InlineData(' ', 0f)]
    [InlineData('█', 1f)]
    [InlineData('░', 0.25f)]
    [InlineData('▓', 0.75f)]
    public void EstimateCoverage_KnownGlyphs(char glyph, float expected)
        => Assert.Equal(expected, GlassBlend.EstimateCoverage(glyph), 3);
    #endregion

    #region Compositing (through the public Show / Overlay path)
    // A backdrop control that paints every cell a known glyph/colour, so the glass has a deterministic layer to
    // read through.
    private sealed class Solid : Control
    {
        private readonly CColor _bg;
        private readonly CColor _fg;
        private readonly char _glyph;
        public Solid(CColor bg, char glyph = ' ', CColor? fg = null) { _bg = bg; _glyph = glyph; _fg = fg ?? CColor.White; Focusable = false; }
        protected override void Render()
        {
            for (var y = 0; y < Size.Height; y++)
                for (var x = 0; x < Size.Width; x++)
                    consoleBuffer.Write(new Position(x, y), new Character(_glyph, _fg, _bg));
        }
    }

    private const int W = 20, H = 8;

    private static (Overlay overlay, GlassPanel glass) Compose(Solid below, GlassPanel glass, int ax, int ay)
    {
        var overlay = new Overlay(new Grid([H], [W], [[below]]));
        ConsoleSnapshot.Render(overlay, W, H);   // establish sizing/paint of the backdrop
        glass.Show(ax, ay, overlay);
        return (overlay, glass);
    }

    [Fact]
    public void SeeThrough_BlendsBackdrop_BetweenBelowAndTint()
    {
        var belowBg = new CColor(200, 40, 40);
        var tint = new CColor(0, 0, 120);
        var below = new Solid(belowBg);
        var glass = new GlassPanel(6, 3, tint, factor: 0.5f, frosted: false);

        var (overlay, _) = Compose(below, glass, ax: 2, ay: 1);
        var buf = ConsoleSnapshot.Render(overlay, W, H);

        // A cell inside the glass (screen 4,2 -> glass local 2,1): backdrop blended halfway to the tint.
        var inside = buf[4, 2].Background;
        Assert.True(inside.HasValue);
        Assert.Equal(belowBg.Mix(tint, 0.5f), inside!.Value);
        Assert.True(inside.Value.Red < belowBg.Red && inside.Value.Red > tint.Red);   // strictly between
        Assert.True(inside.Value.Blue > belowBg.Blue && inside.Value.Blue < tint.Blue);

        // A cell outside the glass is the untouched backdrop.
        Assert.Equal(belowBg, buf[0, 0].Background!.Value);
    }

    [Fact]
    public void OpaqueContent_RendersCrisply_OverGlass()
    {
        var belowBg = new CColor(180, 60, 60);
        var tint = new CColor(10, 20, 40);
        var below = new Solid(belowBg);
        var glass = new GlassPanel(6, 3, tint, factor: 0.6f, frosted: true) { Content = new S.Markup("X") };

        var (overlay, _) = Compose(below, glass, ax: 3, ay: 2);
        var buf = ConsoleSnapshot.Render(overlay, W, H);

        // The content glyph lands at the glass top-left (screen 3,2), drawn opaquely on the glass background.
        var cell = buf[3, 2];
        Assert.Equal('X', cell.Content);
        Assert.True(cell.Background.HasValue);                 // sits on the glass, not a hole to terminal default
        Assert.NotEqual(belowBg, cell.Background!.Value);      // glass-tinted, not the raw backdrop
    }

    [Fact]
    public void Frosted_CollapsesGlyphInk_ToPerceivedColour()
    {
        var belowBg = new CColor(150, 30, 30);
        var green = new CColor(0, 240, 0);
        var tint = new CColor(20, 20, 20);

        // Backdrop fully inked with a green full-block: frosted glass should perceive the cell as green (coverage 1),
        // so its blended background pulls toward green — unlike see-through glass, which blends the cell's background.
        GlassPanel Frost() => new(6, 3, tint, factor: 0.5f, frosted: true);
        GlassPanel See() => new(6, 3, tint, factor: 0.5f, frosted: false);

        var (frostOverlay, _) = Compose(new Solid(belowBg, '█', green), Frost(), 2, 1);
        var (seeOverlay, _) = Compose(new Solid(belowBg, '█', green), See(), 2, 1);

        var frost = ConsoleSnapshot.Render(frostOverlay, W, H)[4, 2].Background!.Value;
        var see = ConsoleSnapshot.Render(seeOverlay, W, H)[4, 2].Background!.Value;

        Assert.True(frost.Green > see.Green, $"frosted green {frost.Green} should exceed see-through {see.Green}");
    }
    #endregion

    #region PerfHud
    [Fact]
    public void PerfHud_ShowsFramedTelemetry_OverOverlay()
    {
        // The viewport must be tall enough for the whole HUD at y=1, or the assertions below silently test a
        // clipped panel — the last row ("locks") is the one that goes first.
        using var hud = new PerfHud();
        var overlay = new Overlay(new Grid([16], [40], [[new Solid(new CColor(30, 30, 30))]]));
        ConsoleSnapshot.Render(overlay, 40, 16);
        hud.Show(2, 1, overlay);

        var text = ConsoleSnapshot.ToText(overlay, 40, 16);
        Assert.Contains("perf", text);      // the panel header rendered (markup parsed without throwing)
        // Every label, first to last. The HUD derives its height from the rows it emits, and "locks" — the last row —
        // is the canary: when the height was a hand-kept constant that fell behind, that row was silently clipped
        // away rather than erroring.
        foreach (var label in new[] { "render", "write", "wait", "latency", "busy", "locks" })
            Assert.Contains(label, text);
        Assert.True(hud.IsShown);
    }
    #endregion
}
