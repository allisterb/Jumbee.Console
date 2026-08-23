namespace Jumbee.Console.Tests;

using System;
using System.Collections.Generic;
using System.Threading;

using ConsoleGUI.Api;
using ConsoleGUI.Data;
using ConsoleGUI.Input;
using ConsoleGUI.Space;

using Jumbee.Console;

using Xunit;

/// <summary>
/// Contract coverage for <see cref="Control.Job{T}(Func{T}, Action{T}, Action{Exception})"/> — the on-demand
/// background queue behind off-thread rendering.
/// </summary>
/// <remarks>
/// The three claims worth pinning are the ones a naive implementation gets wrong: runs never overlap, a burst of
/// requests collapses to one follow-up rather than a backlog, and a request arriving <em>during</em> a run is not
/// swallowed by it. The last is the subtle one — clearing the pending flag after the producer instead of before
/// loses exactly the request that carries the newest state, and the symptom is a display that intermittently stops
/// one frame short.
/// </remarks>
public class JobQueueTests
{
    public JobQueueTests() => UiTestHarness.EnsureStopped();

    [Fact]
    public void Job_ProducesOffTheUiThread_AndAppliesOnIt()
    {
        Run((probe, _) =>
        {
            var produceOffUiThread = false;
            var applied = new ManualResetEventSlim(false);
            var appliedOnUiThread = false;

            var job = probe.Start(
                produce: () => { produceOffUiThread = !UI.CheckAccess(); return 42; },
                apply: v => { appliedOnUiThread = UI.CheckAccess() && v == 42; applied.Set(); });

            job.Request();

            Assert.True(applied.Wait(2000), "the job should have produced and applied");
            Assert.True(appliedOnUiThread, "apply must run on the UI thread, with the produced value");
            Assert.True(produceOffUiThread, "produce must NOT run on the UI thread");
        });
    }

    [Fact]
    public void Job_CoalescesABurst_IntoAtMostOneFollowUpRun()
    {
        Run((probe, _) =>
        {
            var inProducer = new ManualResetEventSlim(false);
            var release = new ManualResetEventSlim(false);
            var runs = 0;

            var job = probe.Start(
                produce: () =>
                {
                    Interlocked.Increment(ref runs);
                    inProducer.Set();
                    release.Wait(2000);   // bounded, so a failure cannot hang the suite
                    return 0;
                },
                apply: _ => { });

            job.Request();
            Assert.True(inProducer.Wait(2000), "the first request should start a run");

            // 50 requests while that run is blocked. All but the first must collapse into the single queued pass.
            for (var i = 0; i < 50; i++) job.Request();
            release.Set();

            Assert.True(SpinWait.SpinUntil(() => job.Completed >= 2, 2000), "the queued pass should run");
            Thread.Sleep(100);   // give any wrongly-queued extra runs a chance to appear

            Assert.Equal(2, Volatile.Read(ref runs));
            Assert.Equal(49, job.Coalesced);
        });
    }

    // The regression this file exists for. A request that lands mid-run carries state the running producer did not
    // see, so it MUST cause another pass -- even though nothing requests again afterwards.
    [Fact]
    public void Job_RequestDuringARun_IsNotSwallowedByIt()
    {
        Run((probe, _) =>
        {
            var inFirstRun = new ManualResetEventSlim(false);
            var release = new ManualResetEventSlim(false);
            var runs = 0;

            var job = probe.Start(
                produce: () =>
                {
                    if (Interlocked.Increment(ref runs) == 1) { inFirstRun.Set(); release.Wait(2000); }
                    return 0;
                },
                apply: _ => { });

            job.Request();
            Assert.True(inFirstRun.Wait(2000));

            job.Request();      // exactly one, while the first run is still inside the producer
            release.Set();

            Assert.True(SpinWait.SpinUntil(() => Volatile.Read(ref runs) >= 2, 2000),
                "a request made during a run must schedule another pass, not be absorbed by it");
        });
    }

    [Fact]
    public void Job_ThrowingProducer_InvokesOnError_AndStops()
    {
        Run((probe, _) =>
        {
            Exception? captured = null;
            var gotError = new ManualResetEventSlim(false);

            var job = probe.Start<int>(
                produce: () => throw new InvalidOperationException("boom"),
                apply: _ => { },
                onError: ex => { captured = ex; gotError.Set(); });

            job.Request();

            Assert.True(gotError.Wait(2000), "a throwing producer should surface on the UI thread");
            Assert.IsType<InvalidOperationException>(captured);
            Assert.True(job.Completion.Wait(2000), "and the job should stop after the throw");
        });
    }

    [Fact]
    public void Job_StopsWithTheControl_AndJoinsCleanly()
    {
        JobHandle? job = null;
        Run((probe, _) =>
        {
            job = probe.Start(() => 0, _ => { });
            job.Request();
            Assert.True(SpinWait.SpinUntil(() => job.Completed >= 1, 2000));
            probe.Dispose();
            Assert.True(job.Completion.Wait(2000), "disposing the control should end its jobs");
        });

        // A request after cancellation is a no-op rather than a throw -- teardown order is not something a caller
        // should have to police.
        job!.Request();
    }

    // A job never requested never runs: the queue is demand-driven, unlike a feed.
    [Fact]
    public void Job_WithoutARequest_NeverRuns()
    {
        Run((probe, _) =>
        {
            var job = probe.Start(() => 0, _ => { });
            Thread.Sleep(150);
            Assert.Equal(0, job.Completed);
        });
    }

    #region Harness
    private static void Run(Action<JobProbe, IReadOnlyList<int>> body)
    {
        UiTestHarness.EnsureStopped();
        var origOut = System.Console.Out;
        System.Console.SetOut(System.IO.TextWriter.Null);
        var probe = new JobProbe();
        try
        {
            var run = UI.Start(new VerticalStackPanel(new TextLabel(TextLabelOrientation.Horizontal, "x")),
                40, 8, fps: 66, isAnsiTerminal: true, console: new StubConsole(40, 8), input: new NoInput());
            body(probe, []);
            _ = run;
        }
        finally
        {
            probe.Dispose();
            UI.Stop();
            System.Console.SetOut(origOut);
        }
    }

    private sealed class JobProbe : Control
    {
        protected override void Render() { }

        public JobHandle Start<T>(Func<T> produce, Action<T> apply, Action<Exception>? onError = null) =>
            Job(produce, apply, onError);
    }

    private sealed class NoInput : IInputSource
    {
        public bool TryRead(out TerminalInputEvent? evt) { evt = null; return false; }
    }

    private sealed class StubConsole(int w, int h) : IConsole
    {
        public Size Size { get; set; } = new Size(w, h);
        public bool KeyAvailable => false;
        public void Initialize() { }
        public void OnRefresh() { }
        public void Write(Position position, in Character character) { }
        public ConsoleKeyInfo ReadKey() => throw new NotSupportedException();
    }
    #endregion
}
