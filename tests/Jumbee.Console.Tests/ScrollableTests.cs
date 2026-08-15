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
}
