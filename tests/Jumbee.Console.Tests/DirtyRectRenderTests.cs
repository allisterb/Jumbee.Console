namespace Jumbee.Console.Tests;

using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using ConsoleGUI;
using ConsoleGUI.Api;
using ConsoleGUI.Data;
using ConsoleGUI.Space;

using Jumbee.Console;
using Jumbee.Console.Snapshot;

using Spectre.Console.Rendering;

using Xunit;

/// <summary>
/// Verifies the dirty-rectangle render path: a small content change re-composites only that control's screen
/// region, not the whole buffer. Drives the real <see cref="ConsoleManager"/> via <see cref="AnsiConsoleSession"/>
/// and reads <see cref="ConsoleManager.LastFrameDirtyCells"/> — the cell count the last flush actually scanned.
/// </summary>
public class DirtyRectRenderTests
{
    public DirtyRectRenderTests() => UiTestHarness.EnsureStopped();

    [Fact]
    public async Task SmallChange_RecompositesOnlyThatControlsArea_NotWholeScreen()
    {
        const int w = 40, h = 10;
        var top = new TextLabel(TextLabelOrientation.Horizontal, "aaaaa");
        var bottom = new TextLabel(TextLabelOrientation.Horizontal, "bbbbb");
        var stack = new VerticalStackPanel(top, bottom);

        using var session = await AnsiConsoleSession.StartAsync(stack.CControl, w, h);

        // The first frame is a full redraw (the surface was just initialized) — it touches the whole buffer.
        Assert.Equal((long)w * h, ConsoleManager.LastFrameDirtyCells);

        // Change one label's text (same length, so no relayout) and render one more frame.
        bottom.Text = "ccccc";
        await session.FrameAsync();

        // Only the changed control's region should have been re-composited — a small fraction of the buffer, not
        // the whole screen. (The exact count is the laid-out area of the label's stack, well under the full w*h.)
        var dirty = ConsoleManager.LastFrameDirtyCells;
        Assert.True(dirty > 0, "expected the change to dirty something");
        Assert.True(dirty <= w * 4, $"expected a partial redraw (a few rows), got {dirty} of {(long)w * h} cells");

        // And the change actually landed on screen (the diff still emitted the new glyphs).
        Assert.Contains("ccccc", ConsoleSnapshot.ToText(session.Screen.Buffer));
    }

    [Fact]
    public async Task IdleFrame_WithNoChange_RecompositesNothing()
    {
        var label = new TextLabel(TextLabelOrientation.Horizontal, "hello");
        using var session = await AnsiConsoleSession.StartAsync(new VerticalStackPanel(label).CControl, 30, 5);

        // A frame with nothing dirtied re-composites zero cells.
        await session.FrameAsync();
        Assert.Equal(0, ConsoleManager.LastFrameDirtyCells);
    }

    // Regression for the nested-layout damage drop: a control inside NESTED SplitPanels (a SplitPanel filling
    // another SplitPanel — the examples browser shape) must still localize its damage. The bug: a Jumbee Layout bound
    // as a parent's DrawingContext child is the Layout wrapper, but its wrapped ConsoleGUI control bubbles as itself,
    // so DrawingContext.Update's `control != Child` guard dropped every rect — leaving HasDirty false so the live
    // loop fell back to a full-screen redraw every frame. Fixed by binding the layout's CControl (RenderNode). Here,
    // a dropped rect would show as LastFrameDirtyCells == 0 (nothing composited) and the change missing on screen.
    [Fact]
    public async Task NestedSplitPanels_LocalizeDamage_NotDroppedToFullRedraw()
    {
        const int w = 60, h = 12;
        var target = new TextLabel(TextLabelOrientation.Horizontal, "AAA");
        // target lives two SplitPanels deep: Split(dummy | Split(dummy | target)).
        var inner = new SplitPanel(SplitOrientation.Horizontal,
            new TextLabel(TextLabelOrientation.Horizontal, "L2"), target, splitPosition: 10);
        var outer = new SplitPanel(SplitOrientation.Horizontal,
            new TextLabel(TextLabelOrientation.Horizontal, "L1"), inner, splitPosition: 10);
        using var session = await AnsiConsoleSession.StartAsync(outer.CControl, w, h);

        target.Text = "BBB";   // same-length content change
        await session.FrameAsync();

        var dirty = ConsoleManager.LastFrameDirtyCells;
        Assert.True(dirty > 0, "damage from a control in nested SplitPanels was dropped (never reached the compositor)");
        Assert.True(dirty < (long)w * h, $"expected a partial redraw, got the whole screen ({dirty})");
        Assert.Contains("BBB", ConsoleSnapshot.ToText(session.Screen.Buffer));   // the change actually composited
    }

    // The same drop for the layouts that route input through the child they bind: a Grid or a stack panel holding a
    // nested layout bound the Layout wrapper, so the nested layout's damage never left it. A sibling outside the
    // nest changes in the same frame, as in a live app, so the frame has damage either way and a dropped rect shows
    // only as the change missing from the screen — the full-redraw safety net can't rescue it.
    [Theory]
    [InlineData("grid-in-vstack")]
    [InlineData("grid-in-hstack")]
    [InlineData("vstack-in-grid")]
    [InlineData("hstack-in-vstack")]
    [InlineData("grid-in-grid")]
    public async Task LayoutNestedInGridOrStack_LocalizesDamage(string shape)
    {
        const int w = 60, h = 12;
        var target = new TextLabel(TextLabelOrientation.Horizontal, "AAA");
        var sibling = new TextLabel(TextLabelOrientation.Horizontal, "s1");
        ILayout root = shape switch
        {
            "grid-in-vstack" => new VerticalStackPanel(sibling, new Grid([1], [20], [[target]])),
            "grid-in-hstack" => new HorizontalStackPanel(sibling.WithWidth(10), new Grid([1], [20], [[target]])),
            "vstack-in-grid" => new Grid([1, 2], [30], [[sibling], [new VerticalStackPanel(target)]]),
            // A stack child gets the panel's full height, so a fill-width label would tile the screen on its own.
            "hstack-in-vstack" => new VerticalStackPanel(sibling, new HorizontalStackPanel(target.WithWidth(10))),
            "grid-in-grid" => new Grid([1, 1], [30], [[sibling], [new Grid([1], [20], [[target]])]]),
            _ => throw new System.ArgumentException(shape),
        };
        using var session = await AnsiConsoleSession.StartAsync(root.CControl, w, h);

        sibling.Text = "s2";
        target.Text = "BBB";
        await session.FrameAsync();

        var text = ConsoleSnapshot.ToText(session.Screen.Buffer);
        Assert.Contains("s2", text);
        Assert.Contains("BBB", text);   // dropped damage leaves AAA on screen
        Assert.True(ConsoleManager.LastFrameDirtyCells < (long)w * h, $"expected a partial redraw, got the whole screen ({ConsoleManager.LastFrameDirtyCells})");
    }

    // The case that found it: a drop-down labelled by a Grid inside a stack (MeshInspector's sidebar shape), its value
    // changed from code while a sibling repaints. Its new value must reach the screen through the partial path.
    [Fact]
    public async Task SelectInGridInStack_ChangedWithSibling_ReachesScreen()
    {
        const int w = 40, h = 8;
        var select = new Select("Alpha", "Bravo", "Charlie") { SelectedIndex = 0 };
        var sibling = new TextLabel(TextLabelOrientation.Horizontal, "s1");
        var root = new VerticalStackPanel(sibling, new Grid([1], [8, 20], [[new TextLabel(TextLabelOrientation.Horizontal, "Pick"), select]]));
        using var session = await AnsiConsoleSession.StartAsync(root.CControl, w, h);
        Assert.Contains("Alpha", ConsoleSnapshot.ToText(session.Screen.Buffer));

        select.SelectedIndex = 2;
        sibling.Text = "s2";
        await session.FrameAsync();

        var text = ConsoleSnapshot.ToText(session.Screen.Buffer);
        Assert.Contains("s2", text);
        Assert.Contains("Charlie", text);
        Assert.True(ConsoleManager.LastFrameDirtyCells < (long)w * h, $"expected a partial redraw, got the whole screen ({ConsoleManager.LastFrameDirtyCells})");
    }

    // A Plot nested deep (Plot -> ControlFrame -> Grid -> CompositeControl) must still localize its damage: a data
    // push re-composites only a partial region, not the whole screen — and never more than the buffer (the reported
    // count is clamped, since overlapping dirty rects would otherwise sum past the screen area).
    [Fact]
    public async Task NestedPlot_InComposite_LocalizesDamage_AndStaysUnderBufferArea()
    {
        const int w = 44, h = 22;
        var host = new PlotHost();
        using var session = await AnsiConsoleSession.StartAsync(host, w, h);

        host.Series.SetData([0, 1, 2, 3], [1, 0, 1, 0]);
        await session.FrameAsync();

        var dirty = ConsoleManager.LastFrameDirtyCells;
        Assert.True(dirty > 0, "the plot change should have dirtied something (damage localized, not dropped)");
        Assert.True(dirty < (long)w * h, $"expected a partial redraw, got the whole screen ({dirty} of {(long)w * h})");
        Assert.True(dirty <= (long)w * h, $"reported dirty cells must be clamped to the buffer area, got {dirty}");
    }

    // Regression: a same-length Sparkline value push (the streaming case) reports its area ONCE. It used to fire
    // three overlapping full-area reports per update (watch: Width= + updatesLayout Initialize + the paint), which
    // inflated dirty% and re-scanned the same cells. One row of the container is the single-report ceiling.
    [Fact]
    public async Task SparklineUpdate_SameLength_ReportsRegionOnce_NotThrice()
    {
        const int w = 40, h = 5;
        var spark = new Sparkline(new double[16]);
        using var session = await AnsiConsoleSession.StartAsync(new VerticalStackPanel(spark).CControl, w, h);

        spark.Values = [1, 2, 3, 4, 5, 6, 7, 8, 8, 7, 6, 5, 4, 3, 2, 1];
        await session.FrameAsync();

        var dirty = ConsoleManager.LastFrameDirtyCells;
        Assert.True(dirty > 0, "the value change should dirty the sparkline's row");
        Assert.True(dirty <= w, $"a same-length update should report the row once (~{w}), got {dirty} (over-report regressed)");
    }

    // Regression: a same-length TextLabel text change (e.g. a gauge "52%"->"54%") reports its row once, not twice
    // (the setter used to unconditionally Resize on top of the paint's own damage report).
    [Fact]
    public async Task TextLabelUpdate_SameLength_ReportsRegionOnce_NotTwice()
    {
        const int w = 40, h = 5;
        var label = new TextLabel(TextLabelOrientation.Horizontal, "50%");
        using var session = await AnsiConsoleSession.StartAsync(new VerticalStackPanel(label).CControl, w, h);

        label.Text = "54%";
        await session.FrameAsync();

        var dirty = ConsoleManager.LastFrameDirtyCells;
        Assert.True(dirty > 0, "the text change should dirty the label's row");
        Assert.True(dirty <= w, $"a same-length text change should report the row once (~{w}), got {dirty}");
    }

    // Plot (Controls/Plot) nested in a Grid inside a CompositeControl — the dashboard panel shape.
    private sealed class PlotHost : CompositeControl
    {
        public readonly PlotSeries Series;
        public PlotHost()
        {
            var plot = new Plot();
            Series = plot.AddLiveSeries();
            Series.SetData([0, 1, 2, 3], [0, 1, 0, 1]);
            SetContent(new Grid([20], [40], [[plot.WithFrame(title: "P")]]));
        }
    }

    // Regression for the startup-blank bug: the LIVE UI loop (UI.OnFrame) must paint controls into their buffers
    // BEFORE compositing, so the first frames emit real content — not the empty buffers a composite-then-paint order
    // would flush. The AnsiConsoleSession harness can't catch this (it already paints then draws); only the real
    // frame loop can, so this drives UI.Start end to end and reads back the emitted ANSI.
    [Fact]
    public async Task LiveUiLoop_EmitsPaintedContent_OnStartup()
    {
        UiTestHarness.EnsureStopped();
        const int w = 60, h = 8;
        var capture = new StringBuilder();
        var prevOutput = ConsoleManager.AnsiOutput;
        try
        {
            ConsoleManager.AnsiOutput = acsb =>
            {
                var s = acsb.ToString();
                return Task.Run(() => { lock (capture) capture.Append(s); });
            };

            // A SplitPanel of two framed panes — the browser's actual layout shape. Both panes must appear on the
            // live loop's first frames, exactly as they do through the full-redraw snapshot path (Bisect above).
            _ = UI.Start(TwoPaneSplit(), w, h, fps: 66, isAnsiTerminal: true, console: new TestConsole(w, h), input: new NoInput());
            Thread.Sleep(250);                 // let several frames run
            await ConsoleManager.OutputIdle.ConfigureAwait(false);
        }
        finally
        {
            UI.Stop();
            await ConsoleManager.OutputIdle.ConfigureAwait(false);
            ConsoleManager.AnsiOutput = prevOutput;
        }

        var screen = new AnsiScreen(w, h);
        lock (capture) screen.Feed(capture.ToString());
        var text = ConsoleSnapshot.ToText(screen.Buffer);
        Assert.Contains("PANELEFT", text);     // both panes composited on startup, not left blank
        Assert.Contains("PANERIGHT", text);
    }

    private static SplitPanel TwoPaneSplit() => new(
        SplitOrientation.Horizontal,
        new TextLabel(TextLabelOrientation.Horizontal, "PANELEFT").WithFrame(title: "L"),
        new TextLabel(TextLabelOrientation.Horizontal, "PANERIGHT").WithFrame(title: "R"),
        splitPosition: 24);

    [Fact]
    public async Task SplitPanel_RendersBothPanes_ViaSnapshotPath()
    {
        // Baseline for the live-loop test below: the two-pane split renders both panes through the full-redraw path.
        var text = await AnsiConsoleSnapshot.ToTextAsync(TwoPaneSplit(), 60, 8);
        Assert.Contains("PANELEFT", text);
        Assert.Contains("PANERIGHT", text);
    }

    // Regression for the dirty-% blow-up: a control that refreshes from its own paint handler (the StatusBar/PerfHud
    // pattern) invalidates during the paint pass, so its repaint bubbles next frame. The UI loop must treat that as a
    // partial redraw of the control's own row — NOT promote it to a full-screen redraw. An earlier safety net did the
    // latter, pushing dirty-area toward 100% on every tick.
    [Fact]
    public async Task SelfRefreshingControl_StaysPartial_DoesNotForceFullRedraws()
    {
        UiTestHarness.EnsureStopped();
        const int w = 40, h = 20;
        var prevOutput = ConsoleManager.AnsiOutput;
        try
        {
            ConsoleManager.AnsiOutput = acsb => { _ = acsb.ToString(); return Task.CompletedTask; };   // discard bytes

            // A one-row ticker docked at the bottom (like the status bar) over a filling body. The ticker invalidates
            // itself every frame from a UI.Paint handler.
            var ticker = new TickLabel();
            var body = new TextLabel(TextLabelOrientation.Horizontal, "BODY");
            var root = new DockPanel(DockedControlPlacement.Bottom, ticker, body);

            _ = UI.Start(root, w, h, fps: 66, isAnsiTerminal: true, console: new TestConsole(w, h), input: new NoInput());
            Thread.Sleep(400);                 // many frames of self-refresh
            await ConsoleManager.OutputIdle.ConfigureAwait(false);

            // The ticker occupies one row of h; a partial redraw of it is ~1/h of the screen. A regression to
            // full-screen redraws would push this toward 100%.
            var dirtyAvg = UI.ProcessMetrics.DirtyAreaPercentAvg;
            Assert.True(dirtyAvg < 25.0, $"a self-refreshing control should stay a partial redraw; DirtyAreaPercentAvg={dirtyAvg:F1}%");
        }
        finally
        {
            UI.Stop();
            await ConsoleManager.OutputIdle.ConfigureAwait(false);
            ConsoleManager.AnsiOutput = prevOutput;
        }
    }

    // A one-row control that re-renders every frame by invalidating itself from the UI.Paint tick (as StatusBar and
    // PerfHud do). Content changes each render so the diff actually emits.
    private sealed class TickLabel : RenderableControl
    {
        private int _n;
        private int _paints;
        public TickLabel() { Focusable = false; UI.Paint += OnTick; }
        // Throttled like StatusBar/PerfHud: invalidate only occasionally, from the paint tick. On the invalidate
        // frame the control's own OnPaint has already run, so nothing bubbles that frame — the exact condition that
        // tripped the old full-redraw fallback.
        private void OnTick(object? sender, UI.PaintEventArgs e) { if (++_paints % 4 == 0) Invalidate(); }
        protected override int IntrinsicHeight() => 1;
        protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
        {
            yield return new Segment($"tick {_n++}".PadRight(System.Math.Max(1, maxWidth)));
        }
    }

    // Regression for the input-lag bug: swapping a composite's content (the ExampleHost.Show path — a tree
    // activation swaps the middle pane) must become visible on its own, not only after the *next* input event.
    // The composite escalates a child change to its own repaint one frame later; if that follow-up frame isn't
    // composited, the swap sits invisible until something else forces a redraw.
    [Fact]
    public async Task CompositeContentSwap_BecomesVisible_WithoutFurtherInput()
    {
        UiTestHarness.EnsureStopped();
        const int w = 40, h = 10;
        var capture = new StringBuilder();
        var prevOutput = ConsoleManager.AnsiOutput;
        try
        {
            ConsoleManager.AnsiOutput = acsb =>
            {
                var s = acsb.ToString();
                return Task.Run(() => { lock (capture) capture.Append(s); });
            };

            var host = new SwapHost();
            host.ShowText("OLDCONTENT");
            _ = UI.Start(new VerticalStackPanel(host), w, h, fps: 66, isAnsiTerminal: true, console: new TestConsole(w, h), input: new NoInput());
            // Parse the emitted ANSI into a screen — the per-cell diff re-emits only changed cells, so a swapped
            // string is not a contiguous substring of the raw byte stream; only the parsed grid is authoritative.
            bool OnScreen(string s) { var scr = new AnsiScreen(w, h); scr.Feed(Captured(capture)); return ConsoleSnapshot.ToText(scr.Buffer).Contains(s); }
            Assert.True(WaitFor(() => OnScreen("OLDCONTENT"), 2000), "initial content should render");

            // Swap content as an input handler would (posted onto the UI thread), then send NO further input.
            UI.Invoke(() => host.ShowText("NEWCONTENT"));
            Assert.True(WaitFor(() => OnScreen("NEWCONTENT"), 2000),
                "swapped content must appear on its own, not wait for the next input event");
        }
        finally
        {
            UI.Stop();
            await ConsoleManager.OutputIdle.ConfigureAwait(false);
            ConsoleManager.AnsiOutput = prevOutput;
        }
    }

    private static string Captured(StringBuilder sb) { lock (sb) return sb.ToString(); }
    private static bool WaitFor(System.Func<bool> cond, int ms)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < ms) { if (cond()) return true; Thread.Sleep(15); }
        return cond();
    }

    // The user's exact bug: activating a tree item (Enter / double-click) that swaps another pane must take effect on
    // the first activation, not the next. Drives a real Tree via the input source, non-ANSI so the render lands in a
    // readable ConsoleBuffer.
    [Fact]
    public void TreeActivation_SwapsPaneOnFirstEnter_NotTheNext()
    {
        UiTestHarness.EnsureStopped();
        var origOut = System.Console.Out;
        System.Console.SetOut(System.IO.TextWriter.Null);

        var host = new SwapHost();
        host.ShowText("PANEOLD");
        var tree = new Tree("ROOT");
        tree.AddNode("LEAFITEM");
        tree.NodeActivated += (_, _) => host.ShowText("PANENEW");

        var screen = new ConsoleBuffer { Size = new Size(44, 12) };
        var input = new FakeKeys();
        Task run;
        try
        {
            run = UI.Start(new Grid([12], [22, 22], [[tree, host]]), 44, 12, fps: 50, isAnsiTerminal: false, console: screen, input: input);
            UI.Invoke(() => UI.SetFocus(tree));
            Assert.True(WaitFor(() => ScreenHas(screen, "LEAFITEM") && ScreenHas(screen, "PANEOLD"), 3000), "tree + old pane should render");

            input.Push(ConsoleKey.DownArrow);   // root (index 0 of the flattened tree)
            input.Push(ConsoleKey.DownArrow);   // the leaf
            Assert.True(WaitFor(() => tree.SelectedNode is { Nodes.Count: 0 }, 2000), "leaf should be selected");

            input.Push(ConsoleKey.Enter);        // activate it — ONE press
            Assert.True(WaitFor(() => ScreenHas(screen, "PANENEW"), 2500),
                "the pane must switch on the first Enter, not require a second activation");
        }
        finally { UI.Stop(); System.Console.SetOut(origOut); }
        Assert.True(run.Wait(2000));
    }

    private static bool ScreenHas(ConsoleBuffer screen, string text)
    {
        for (int y = 0; y < screen.Size.Height; y++)
        {
            var sb = new StringBuilder();
            for (int x = 0; x < screen.Size.Width; x++) sb.Append(screen[x, y].Content ?? ' ');
            if (sb.ToString().Contains(text)) return true;
        }
        return false;
    }

    private sealed class FakeKeys : IInputSource
    {
        private readonly System.Collections.Concurrent.ConcurrentQueue<TerminalInputEvent> _q = new();
        public void Push(ConsoleKey key) => _q.Enqueue(new KeyInputEvent(key, '\0', TerminalModifiers.None));
        public bool TryRead(out TerminalInputEvent? evt) { if (_q.TryDequeue(out var e)) { evt = e; return true; } evt = null; return false; }
    }

    private sealed class SwapHost : CompositeControl
    {
        public void ShowText(string t) =>
            SetContent(new VerticalStackPanel(new TextLabel(TextLabelOrientation.Horizontal, t)));
    }

    private sealed class NoInput : IInputSource
    {
        public bool TryRead(out TerminalInputEvent? evt) { evt = null; return false; }
    }

    private sealed class TestConsole(int w, int h) : IConsole
    {
        public Size Size { get; set; } = new Size(w, h);
        public bool KeyAvailable => false;
        public void Initialize() { }
        public void OnRefresh() { }
        public void Write(Position position, in Character character) { }
        public ConsoleKeyInfo ReadKey() => throw new System.NotSupportedException();
    }
}
