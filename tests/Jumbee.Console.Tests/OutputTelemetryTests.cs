namespace Jumbee.Console.Tests;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using ConsoleGUI;
using ConsoleGUI.Api;
using ConsoleGUI.Data;
using ConsoleGUI.Space;

using Jumbee.Console;

using Spectre.Console.Rendering;

using Xunit;

/// <summary>
/// The terminal-write telemetry: the console write runs off the render loop, so frame time can never show it. These
/// drive the real render loop with a sink of KNOWN cost and check the numbers reflect it — including the case that
/// motivated the work, where the terminal is slower than the frame rate and the write queue backs up.
/// </summary>
public class OutputTelemetryTests
{
    public OutputTelemetryTests() => UiTestHarness.EnsureStopped();

    // Changes its content every paint, so every frame is dirty and a write is produced.
    private sealed class Ticker : RenderableControl
    {
        private int _n;

        public Ticker() { Focusable = false; UI.Paint += OnTick; }

        private void OnTick(object? sender, UI.PaintEventArgs e) => Invalidate();

        protected override int IntrinsicHeight() => 1;

        protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
        {
            yield return new Segment($"tick {_n++}".PadRight(Math.Max(1, maxWidth)));
        }
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
        public ConsoleKeyInfo ReadKey() => throw new NotSupportedException();
    }

    // Runs the real loop for `ms` with an AnsiOutput sink that takes `writeDelayMs`, then returns the telemetry.
    private static async Task<(double WriteMs, double WaitMs, int PeakDepth)> Measure(int writeDelayMs, int ms)
    {
        var prevOutput = ConsoleManager.AnsiOutput;
        try
        {
            ConsoleManager.AnsiOutput = async acsb =>
            {
                _ = acsb.ToString();                       // consume the frame like a real write would
                if (writeDelayMs > 0) await Task.Delay(writeDelayMs).ConfigureAwait(false);
            };

            using var ticker = new Ticker();
            _ = UI.Start(new VerticalStackPanel(ticker), 40, 10, fps: 60,
                isAnsiTerminal: true, console: new TestConsole(40, 10), input: new NoInput());

            // The telemetry is a rolling 60-sample window on static state, and a test this short does not produce 60
            // frames — so without this the averages are still partly the PREVIOUS test's sink. That contamination is
            // what made a free write appear to "wait" 20 ms.
            Thread.Sleep(50);
            ConsoleManager.ResetOutputTelemetry();
            Thread.Sleep(ms);

            // Peak, not instantaneous depth: the queue drains between frames, so an instant read from here lands
            // between writes and reports 0 even while frames are demonstrably blocked.
            return (UI.AverageWriteTime, UI.AverageWriteWaitTime, UI.WriteQueueDepthPeak);
        }
        finally
        {
            UI.Stop();
            await ConsoleManager.OutputIdle.ConfigureAwait(false);
            ConsoleManager.AnsiOutput = prevOutput;
        }
    }

    // The matching itself, tested directly and deterministically. A live loop cannot prove this: every frame there
    // costs the same, so a wrongly-paired write looks identical to a correctly-paired one. Giving the frames
    // DIFFERENT costs and attaching a write to exactly one of them is what makes mis-pairing visible.
    [Fact]
    public void RecordWrite_AttachesToTheFrameWithThatOrdinal()
    {
        using var m = new ProcessMetrics();
        m.RecordFrame(renderMs: 1, periodMs: 16, renderAllocBytes: 0, redrawn: true, ordinal: 1);
        m.RecordFrame(renderMs: 2, periodMs: 16, renderAllocBytes: 0, redrawn: true, ordinal: 2);
        m.RecordFrame(renderMs: 3, periodMs: 16, renderAllocBytes: 0, redrawn: true, ordinal: 3);

        m.RecordWrite(ordinal: 2, waitMs: 10, writeMs: 100);

        // Only frame 2 has a write, so it is the only one counted: 2 + 10 + 100. Pairing with frame 1 or 3 would
        // give 111 or 113, and folding in the unwritten frames would pull the average far below either.
        Assert.Equal(112, m.FrameLatencyMsAvg, 3);
        Assert.Equal(112, m.FrameLatencyMsPeak, 3);
    }

    // A write usually finishes BEFORE its own frame is recorded: Emit runs mid-frame during the composite, while
    // RecordFrame only runs in the finally at the end. Dropping those kept just the frames whose writes were slow
    // enough to arrive late, which inflated every latency reading — the examples browser showed 7.6 ms against a
    // render+wait+write of 2.9 ms.
    [Fact]
    public void WriteThatFinishesBeforeItsFrameIsRecorded_IsStillCounted()
    {
        using var m = new ProcessMetrics();

        m.RecordWrite(ordinal: 1, waitMs: 0, writeMs: 100);   // the write lands first...
        m.RecordFrame(renderMs: 1, periodMs: 16, renderAllocBytes: 0, redrawn: true, ordinal: 1);

        Assert.Equal(101, m.FrameLatencyMsAvg, 3);
    }

    [Fact]
    public void RecordWrite_ForAnUnknownFrame_IsIgnored()
    {
        using var m = new ProcessMetrics();
        m.RecordFrame(renderMs: 1, periodMs: 16, renderAllocBytes: 0, redrawn: true, ordinal: 1);

        m.RecordWrite(ordinal: 999, waitMs: 10, writeMs: 100);   // aged out of the window

        Assert.Equal(0, m.FrameLatencyMsAvg);   // nothing was attributed to the wrong frame
    }

    // A slot reused a whole window later must not inherit the previous occupant's write timings.
    [Fact]
    public void ReusingAFrameSlot_ClearsTheOldWrite()
    {
        using var m = new ProcessMetrics(capacity: 2);
        m.RecordFrame(renderMs: 1, periodMs: 16, renderAllocBytes: 0, redrawn: true, ordinal: 1);
        m.RecordWrite(ordinal: 1, waitMs: 10, writeMs: 100);
        Assert.Equal(111, m.FrameLatencyMsAvg, 3);

        m.RecordFrame(renderMs: 5, periodMs: 16, renderAllocBytes: 0, redrawn: true, ordinal: 2);
        m.RecordFrame(renderMs: 5, periodMs: 16, renderAllocBytes: 0, redrawn: true, ordinal: 3);   // evicts ordinal 1

        Assert.Equal(0, m.FrameLatencyMsAvg);   // no frame in the window has a write of its own any more
    }

    // Frames that never drew never wrote, so they must not be counted as zero-latency and drag the average down.
    [Fact]
    public async Task Latency_IgnoresFramesThatNeverWrote()
    {
        var (_, _, _) = await Measure(writeDelayMs: 20, ms: 800);

        // Every counted frame carries a real write of ~20 ms, so the average cannot collapse toward zero the way it
        // would if un-written frames were folded in.
        Assert.True(UI.AverageFrameLatency > 10, $"latency averaged {UI.AverageFrameLatency:F1} ms");
    }

    [Fact]
    public async Task WriteTime_ReflectsTheCostOfTheActualWrite()
    {
        var (writeMs, _, _) = await Measure(writeDelayMs: 20, ms: 600);

        // The sink sleeps 20ms; allow generous slack for timer resolution, but it must be unmistakably non-trivial —
        // before this telemetry existed, this cost was reported nowhere at all.
        Assert.True(writeMs >= 10, $"the write cost should be measured, but averaged {writeMs:F1} ms");
    }

    // The motivating case: writes slower than the frame period. The render loop never blocks, so frame time stays
    // healthy while frames pile up behind the terminal — which is exactly what the queue depth and wait expose.
    [Fact]
    public async Task WhenTheTerminalCannotKeepUp_TheQueueBacksUpAndFramesWait()
    {
        var (_, waitMs, peakDepth) = await Measure(writeDelayMs: 30, ms: 800);

        // Wait is the load-bearing assertion: a terminal slower than the frame rate must show frames blocked for a
        // meaningful slice of the write cost. Depth is checked too, but loosely — see the note in the test below.
        Assert.True(waitMs > 5, $"frames should block behind a slow terminal, but averaged {waitMs:F1} ms waiting");
        Assert.True(peakDepth > 1, $"frames should queue behind a slow terminal, but the backlog peaked at {peakDepth}");
    }

    [Fact]
    public async Task WhenTheTerminalKeepsUp_FramesBarelyWait()
    {
        var (_, waitMs, _) = await Measure(writeDelayMs: 0, ms: 600);

        // Asserted on WAIT, not on queue depth. A free write still peaks at a depth of ~4 here, because frames can
        // pile up waiting for a thread-pool slot rather than for the terminal — so depth alone cannot tell "the
        // terminal is slow" from "the pool was busy". Time actually spent blocked can.
        Assert.True(waitMs < 5, $"frames should barely wait when the write is free, but averaged {waitMs:F1} ms");
    }
}
