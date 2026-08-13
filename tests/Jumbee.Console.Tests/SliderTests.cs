namespace Jumbee.Console.Tests;

using ConsoleGUI.Space;

using Jumbee.Console;
using Jumbee.Console.Snapshot;

using Xunit;

public class SliderTests
{
    public SliderTests()
    {
        UiTestHarness.EnsureStopped();
        ConsoleSnapshot.ResetMouse();
    }

    // The default fill/thumb colours (SliderStyle.Default): a cell belongs to the filled band when its background
    // is the fill colour, and carries the thumb when its foreground is the thumb colour.
    private const byte FillRed = 90;
    private const byte ThumbRed = 235;

    private static int FillCells(ConsoleBuffer b, int width)
    {
        var n = 0;
        for (var x = 0; x < width; x++)
            if (ConsoleSnapshot.BackgroundAt(b, x, 0)?.R == FillRed) n++;
        return n;
    }

    private static int ThumbX(ConsoleBuffer b, int width)
    {
        for (var x = 0; x < width; x++)
            if (ConsoleSnapshot.ForegroundAt(b, x, 0)?.R == ThumbRed) return x;
        return -1;
    }

    [Fact]
    public void Value_IsClampedToTheRange()
    {
        var s = new Slider(0, 10, 5);

        s.Value = 99;
        Assert.Equal(10, s.Value);

        s.Value = -99;
        Assert.Equal(0, s.Value);
    }

    [Fact]
    public void ValueChanged_FiresOnlyWhenTheValueMoves()
    {
        var s = new Slider(0, 10, 5);
        var fired = 0;
        double last = -1;
        s.ValueChanged += (_, v) => { fired++; last = v; };

        s.Value = 7;
        s.Value = 7;          // no change
        s.Value = 100;        // clamps to 10
        s.Value = 10;         // already there after the clamp

        Assert.Equal(2, fired);
        Assert.Equal(10, last);
    }

    [Fact]
    public void Step_DefaultsToAHundredthOfTheRange_AndArrowsUseIt()
    {
        var s = new Slider(0, 200, 100);
        Assert.Equal(2, s.Step);

        s.StepBy(3);
        Assert.Equal(106, s.Value);
    }

    [Fact]
    public void SnapToStep_QuantisesEveryPath()
    {
        // Continuous first: a fraction lands wherever it lands.
        var s = new Slider(0, 10, 0) { Step = 2 };
        s.SetFraction(0.55);
        Assert.Equal(5.5, s.Value, 3);

        s.SnapToStep = true;
        Assert.Equal(6, s.Value);      // the existing value re-quantises immediately

        s.SetFraction(0.31);           // 3.1 -> nearest multiple of 2
        Assert.Equal(4, s.Value);
    }

    [Fact]
    public void Keys_StepPageAndJumpToTheEnds()
    {
        var s = new Slider(0, 100, 50) { Step = 1 };

        ConsoleSnapshot.RenderAfter(s, 40, 1, ConsoleKey.RightArrow);
        Assert.Equal(51, s.Value);

        ConsoleSnapshot.RenderAfter(s, 40, 1, ConsoleKey.LeftArrow, ConsoleKey.LeftArrow);
        Assert.Equal(49, s.Value);

        ConsoleSnapshot.RenderAfter(s, 40, 1, ConsoleKey.PageUp);
        Assert.Equal(59, s.Value);

        ConsoleSnapshot.RenderAfter(s, 40, 1, ConsoleKey.End);
        Assert.Equal(100, s.Value);

        ConsoleSnapshot.RenderAfter(s, 40, 1, ConsoleKey.Home);
        Assert.Equal(0, s.Value);
    }

    [Fact]
    public void ShiftArrow_IsTheFineAdjust()
    {
        var s = new Slider(0, 100, 50) { Step = 10 };
        ConsoleSnapshot.RenderAfter(s, 40, 1, [ConsoleSnapshot.Key(ConsoleKey.RightArrow, shift: true)]);
        Assert.Equal(52, s.Value);   // a fifth of a step
    }

    [Fact]
    public void ClickingTheTrack_SetsTheValue()
    {
        // No label, no readout: the track is the whole 21-cell width, so cell 0 is the minimum and cell 20 the max.
        var s = new Slider(0, 100, 0) { ShowValue = false };
        var b = ConsoleSnapshot.Render(s, 21, 1);

        Assert.True(ConsoleSnapshot.Click(b, 10, 0));
        Assert.Equal(50, s.Value);

        Assert.True(ConsoleSnapshot.Click(b, 20, 0));
        Assert.Equal(100, s.Value);   // the last cell must reach the maximum

        Assert.True(ConsoleSnapshot.Click(b, 0, 0));
        Assert.Equal(0, s.Value);     // and the first the minimum
    }

    [Fact]
    public void Dragging_TracksThePointer_AndKeepsSteeringOffTheControl()
    {
        var s = new Slider(0, 100, 0) { ShowValue = false };
        var b = ConsoleSnapshot.Render(s, 21, 3);

        // Ends two rows below the slider: the capture taken on press is what keeps the drag on target.
        Assert.True(ConsoleSnapshot.Drag(b, 0, 0, 20, 2));
        Assert.Equal(100, s.Value);
    }

    [Fact]
    public void Wheel_StepsTheValue()
    {
        var s = new Slider(0, 100, 50) { Step = 5, ShowValue = false };
        var b = ConsoleSnapshot.Render(s, 20, 1);

        ConsoleSnapshot.Wheel(b, 5, 0, -1);
        Assert.Equal(55, s.Value);

        ConsoleSnapshot.Wheel(b, 5, 0, 1);
        Assert.Equal(50, s.Value);
    }

    [Fact]
    public void Track_FillsProportionally()
    {
        var s = new Slider(0, 100, 50) { ShowValue = false };
        var b = ConsoleSnapshot.Render(s, 20, 1);

        // Half of a 20-cell track: 10 filled cells plus the thumb, which sits on the filled side.
        Assert.InRange(FillCells(b, 20), 10, 12);
    }

    [Fact]
    public void Thumb_IsVisibleAtBothEndsOfTheRange()
    {
        var min = ConsoleSnapshot.Render(new Slider(0, 100, 0) { ShowValue = false }, 20, 1);
        Assert.Equal(0, ThumbX(min, 20));

        var max = ConsoleSnapshot.Render(new Slider(0, 100, 100) { ShowValue = false }, 20, 1);
        Assert.Equal(19, ThumbX(max, 20));

        var mid = ConsoleSnapshot.Render(new Slider(0, 100, 50) { ShowValue = false }, 20, 1);
        Assert.InRange(ThumbX(mid, 20), 8, 11);
    }

    [Fact]
    public void Readout_KeepsAFixedWidth_SoTheTrackDoesNotJitter()
    {
        // 9.99 and 10.00 format to different lengths; the track must not move between them.
        var s = new Slider(0, 10, 9.99);
        var narrow = ConsoleSnapshot.Render(s, 30, 1);
        var atNine = ThumbX(narrow, 30);

        s.Value = 10;
        var wide = ConsoleSnapshot.Render(s, 30, 1);

        Assert.Contains("10.00", ConsoleSnapshot.ToText(wide));
        Assert.Equal(atNine, ThumbX(wide, 30));   // 9.99 and 10.00 both land on the last track cell
    }

    [Fact]
    public void Label_IsDrawnBeforeTheTrack_AndReservedWidthAligns()
    {
        var s = new Slider(0, 10, 5, "Gravity") { LabelWidth = 12, ShowValue = false };
        var text = ConsoleSnapshot.ToText(s, 40, 1);
        Assert.StartsWith("Gravity", text);

        var b = ConsoleSnapshot.Render(s, 40, 1);
        for (var x = 0; x < 13; x++)
            Assert.Null(ConsoleSnapshot.BackgroundAt(b, x, 0));   // label field, then the gap: no track band
        Assert.NotNull(ConsoleSnapshot.BackgroundAt(b, 13, 0));
    }

    [Fact]
    public void Label_EllipsizesRatherThanStarvingTheTrack()
    {
        var s = new Slider(0, 10, 5, "A very long parameter name indeed") { ShowValue = false };
        var b = ConsoleSnapshot.Render(s, 20, 1);

        Assert.Contains("…", ConsoleSnapshot.ToText(b));
        // Whatever the label wanted, at least the minimum track survives.
        var band = 0;
        for (var x = 0; x < 20; x++)
            if (ConsoleSnapshot.BackgroundAt(b, x, 0) is not null) band++;
        Assert.True(band >= 4, $"track collapsed to {band} cells");
    }

    [Fact]
    public void Maximum_BelowMinimum_IsCoerced_NotDivideByZero()
    {
        var s = new Slider(5, 10, 7);
        s.Maximum = 1;

        Assert.Equal(6, s.Maximum);   // coerced above Minimum
        Assert.Equal(6, s.Value);     // and the value re-clamps into the new range
    }

    [Fact]
    public void WithFill_RecoloursTheBand()
    {
        var s = new Slider(0, 100, 100) { ShowValue = false }.WithFill(new Color(200, 40, 40));
        var b = ConsoleSnapshot.Render(s, 10, 1);
        Assert.Equal((byte?)200, ConsoleSnapshot.BackgroundAt(b, 0, 0)?.R);
    }
}
