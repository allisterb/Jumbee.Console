namespace Jumbee.Console.Tests;

using Jumbee.Console.Snapshot;

using Xunit;

/// <summary>Headless tests for <see cref="ProgressBar"/>: description, the sub-cell fill, the optional percentage /
/// time / spinner columns, indeterminate mode, and theming.</summary>
public class ProgressBarTests
{
    [Fact]
    public void Renders_DescriptionAndPercentage()
    {
        var text = ConsoleSnapshot.ToText(new ProgressBar("Loading", 50, 100), 40, 1);
        Assert.Contains("Loading", text);
        Assert.Contains("50%", text);
    }

    [Fact]
    public void Percentage_Hidden_WhenDisabled()
    {
        var text = ConsoleSnapshot.ToText(new ProgressBar("Work", 50) { ShowPercentage = false }, 40, 1);
        Assert.DoesNotContain("%", text);
    }

    [Fact]
    public void Fill_ProportionalToValue()
    {
        // The bar is two background bands — fill and track — so "has a background" is true even at 0%. Count cells
        // painted the FILL colour specifically. A distinctive fill keeps it unambiguous against the track.
        ConsoleGUI.Data.Color fill = Style.Red.ForegroundColor!.Value;
        int Filled(double value)
        {
            var buf = ConsoleSnapshot.Render(new ProgressBar(value: value, max: 100) { ShowPercentage = false }.WithFill(Style.Red), 30, 1);
            int n = 0;
            for (int x = 0; x < 30; x++) if (buf[x, 0].Background is { } bg && bg.Equals(fill)) n++;
            return n;
        }

        Assert.Equal(0, Filled(0));
        Assert.True(Filled(50) > Filled(0));
        Assert.True(Filled(100) > Filled(50));
    }

    [Fact]
    public void Indeterminate_SuppressesPercentage_ButKeepsABand()
    {
        var pb = new ProgressBar("Scanning", 50) { IsIndeterminate = true };
        var buf = ConsoleSnapshot.Render(pb, 30, 1);
        var text = ConsoleSnapshot.ToText(pb, 30, 1);
        Assert.DoesNotContain("%", text);
        int filled = 0;
        for (int x = 0; x < 30; x++) if (buf[x, 0].Background is not null) filled++;
        Assert.True(filled > 0, "indeterminate bar should still show a pulse band");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Indeterminate_NarrowWidth_DoesNotThrow(int width)
    {
        // The initial layout pass can render at a width of one cell; the pulse band must cope rather than throw.
        var text = ConsoleSnapshot.ToText(new ProgressBar("x", 0) { IsIndeterminate = true }, width, 1);
        Assert.NotNull(text);
    }

    [Fact]
    public void Time_Shown_WhenEnabled()
    {
        var pb = new ProgressBar("Work", 25) { TimeDisplay = ProgressTimeDisplay.Elapsed };
        pb.Start();
        Assert.Matches(@"\d\d:\d\d:\d\d", ConsoleSnapshot.ToText(pb, 40, 1));
    }

    [Fact]
    public void LongDescription_IsTruncated_KeepingTheBar()
    {
        // A description far wider than the control must not push the bar (or percentage) off the row.
        var text = ConsoleSnapshot.ToText(new ProgressBar(new string('x', 200), 50), 30, 1);
        Assert.Contains("…", text);
        Assert.Contains("50%", text);
    }

    [Fact]
    public void ThemedFill_HasBackground()
    {
        var buf = ConsoleSnapshot.Render(new ProgressBar(value: 100) { ShowPercentage = false }.WithFill(Style.Red), 20, 1);
        Assert.NotNull(buf[0, 0].Background);   // first bar cell is filled at 100%
    }

    [Fact]
    public void GlyphMode_Ascii_DrawsFillAndTrackGlyphs()
    {
        // Glyph mode paints foreground glyphs per cell rather than a background band, so the glyphs appear in text.
        var text = ConsoleSnapshot.ToText(new ProgressBar(value: 50) { ShowPercentage = false }.WithGlyphs(ProgressBarGlyphs.Ascii), 20, 1);
        Assert.Contains("#", text);   // filled cells
        Assert.Contains("-", text);   // empty track cells
    }

    [Fact]
    public void GlyphMode_Full_IsAllFillGlyph()
    {
        var text = ConsoleSnapshot.ToText(new ProgressBar(value: 100) { ShowPercentage = false }.WithGlyphs(ProgressBarGlyphs.Ascii), 12, 1);
        Assert.Contains("#", text);
        Assert.DoesNotContain("-", text);   // nothing empty at 100%
    }

    [Fact]
    public void GlyphMode_Hatched_UsesBlockGlyphs()
        => Assert.Contains("▨", ConsoleSnapshot.ToText(new ProgressBar(value: 50) { ShowPercentage = false }.WithGlyphs(ProgressBarGlyphs.Hatched), 20, 1));

    [Fact]
    public void GlyphMode_Indeterminate_NarrowWidth_DoesNotThrow()
        => Assert.NotNull(ConsoleSnapshot.ToText(new ProgressBar("x") { IsIndeterminate = true }.WithGlyphs(ProgressBarGlyphs.Ascii), 2, 1));
}
