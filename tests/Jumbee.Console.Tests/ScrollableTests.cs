namespace Jumbee.Console.Tests;

using Jumbee.Console.Snapshot;

using Xunit;

/// <summary>
/// The <see cref="IScrollable"/> contract: a frame scrolls a control only when it opts in, and sizes everything else
/// to the viewport. The negative case is the important one — before the interface, a control that overrode nothing
/// was silently given an unbounded height and resolved to the 1000-row clamp, leaving a scrollbar with a thumb too
/// small to draw over ~1000 empty rows.
/// </summary>
public class ScrollableTests
{
    private const int Width = 30;
    private const int Height = 10;

    // A composite that opts out simply by not implementing IScrollable — the default a developer gets.
    private sealed class PlainPanel : CompositeControl
    {
        public PlainPanel() => SetContent(new VerticalStackPanel(Row("a"), Row("b"), Row("c")));

        private static TextLabel Row(string t) =>
            new(TextLabelOrientation.Horizontal, t) { Focusable = false, Height = 1 };
    }

    // The same panel, opting in and reporting a content height taller than any viewport used here.
    private sealed class TallPanel : CompositeControl, IScrollable
    {
        public TallPanel() => SetContent(new VerticalStackPanel(
            new TextLabel(TextLabelOrientation.Horizontal, "a") { Focusable = false, Height = 1 }));

        public int MeasureHeight(int width) => ContentRows;
    }

    private const int ContentRows = 42;

    private static int FramedChildHeight(Control c)
    {
        c.WithFrame(borderStyle: BorderStyle.Rounded);
        ConsoleSnapshot.Render(c, Width, Height);
        return c.ActualHeight;
    }

    [Fact]
    public void NonScrollable_IsSizedToViewport_NotTheThousandRowClamp()
    {
        var height = FramedChildHeight(new PlainPanel());

        Assert.True(height <= Height, $"a non-IScrollable control should fit its frame's viewport, but got {height} rows");
        Assert.NotEqual(1000, height);
    }

    [Fact]
    public void Scrollable_IsSizedToItsReportedContentHeight()
    {
        Assert.Equal(ContentRows, FramedChildHeight(new TallPanel()));
    }

    // Tree overrode nothing before IScrollable, so it scrolled only by accident: the old default ballooned it to the
    // 1000-row clamp and the frame windowed that. Its scroll range is now its actual row count.
    [Fact]
    public void Tree_ReportsItsRowCount_NotTheClamp()
    {
        var tree = new Tree("root");
        for (var i = 0; i < 20; i++) tree.AddNode($"node{i}");

        var height = FramedChildHeight(tree);

        Assert.Equal(21, height);   // the root plus its 20 children
    }

    // A 100-row control in an 8-row viewport, so there is somewhere to scroll to in both directions.
    private static ControlFrame TallFrame()
    {
        var panel = new TallPanel100();
        panel.WithFrame(borderStyle: BorderStyle.Rounded);
        ConsoleSnapshot.Render(panel, Width, Height);
        return panel.Frame!;
    }

    private sealed class TallPanel100 : CompositeControl, IScrollable
    {
        public TallPanel100() => SetContent(new VerticalStackPanel(
            new TextLabel(TextLabelOrientation.Horizontal, "x") { Focusable = false, Height = 1 }));

        public int MeasureHeight(int width) => 100;
    }

    [Fact]
    public void ScrollIntoView_AlreadyVisible_DoesNotMove()
    {
        var frame = TallFrame();
        frame.Top = 20;

        frame.ScrollIntoView(22);

        Assert.Equal(20, frame.Top);
    }

    [Fact]
    public void ScrollIntoView_Above_AlignsToTheRowsTop()
    {
        var frame = TallFrame();
        frame.Top = 40;

        frame.ScrollIntoView(12);

        Assert.Equal(12, frame.Top);
    }

    [Fact]
    public void ScrollIntoView_Below_ScrollsTheMinimumToRevealTheLastRow()
    {
        var frame = TallFrame();
        frame.Top = 0;
        var viewport = frame.ViewportSize.Height;

        frame.ScrollIntoView(30, 2);

        // The minimum move that puts row 31 (the span's last) on screen — not a jump that centres it.
        Assert.Equal(32 - viewport, frame.Top);
    }

    // Tree's private copy of this clamp lacked the guard and scrolled a too-tall node past its own first row, so the
    // thing you navigated to went off the top of the viewport. Shared now, so it can only be fixed once.
    [Fact]
    public void ScrollIntoView_SpanTallerThanViewport_ShowsItsTopNotItsBottom()
    {
        var frame = TallFrame();
        frame.Top = 0;
        var viewport = frame.ViewportSize.Height;

        frame.ScrollIntoView(30, viewport + 5);

        Assert.Equal(30, frame.Top);
    }
}
