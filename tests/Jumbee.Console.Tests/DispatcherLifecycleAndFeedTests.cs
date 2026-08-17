namespace Jumbee.Console.Tests;

using System;
using System.Threading;
using System.Threading.Tasks;

using ConsoleGUI.Api;
using ConsoleGUI.Data;
using ConsoleGUI.Space;

using Jumbee.Console;

using Xunit;

/// <summary>Regression coverage for three lifecycle fixes surfaced by the scope-tui port:
/// ① a pending <see cref="UI.InvokeAsync(Action)"/> must CANCEL (not hang) when the UI stops;
/// ② <see cref="UI.Paint"/> must fire handlers whose target isn't a control (an app-level fps hook);
/// ③ <see cref="FeedHandle"/> exposes a producer-error callback and a joinable <see cref="FeedHandle.Completion"/>.</summary>
public class DispatcherLifecycleAndFeedTests
{
    public DispatcherLifecycleAndFeedTests() => UiTestHarness.EnsureStopped();

    // ① ---------------------------------------------------------------------------------------------------------
    [Fact]
    public void InvokeAsync_CancelsPendingTask_OnStop_InsteadOfHanging()
    {
        UiTestHarness.EnsureStopped();
        var origOut = System.Console.Out;
        System.Console.SetOut(System.IO.TextWriter.Null);
        var started = new ManualResetEventSlim(false);
        var release = new ManualResetEventSlim(false);
        try
        {
            var run = UI.Start(new VerticalStackPanel(new TextLabel(TextLabelOrientation.Horizontal, "x")),
                40, 8, fps: 66, isAnsiTerminal: true, console: new StubConsole(40, 8), input: new NoInput());

            // Occupy the UI thread briefly so the marshalled op below stays queued while we Stop. The wait is BOUNDED
            // (self-releases) so Stop's join always succeeds well within its 1s timeout — never leaving a blocked UI
            // thread behind for the next test to race.
            UI.Post(() => { started.Set(); release.Wait(500); });
            Assert.True(started.Wait(2000), "the UI thread should pick up the blocker");

            // Marshal an async op from this (non-UI) thread — it queues behind the blocker and can't run yet.
            var pending = UI.InvokeAsync(() => { });
            Assert.False(pending.IsCompleted, "the marshalled op shouldn't have run while the UI thread is blocked");

            // Stop cancels the dispatcher lifetime SYNCHRONOUSLY (so pending is cancelled before it could ever run),
            // then joins once the blocker auto-releases.
            UI.Stop();

            Assert.True(SpinWait.SpinUntil(() => pending.IsCompleted, 2000), "the pending marshal must not hang after Stop");
            Assert.True(pending.IsCanceled, "it should be cancelled by the stop, not left hanging");
            Assert.True(run.Wait(2000));
        }
        finally
        {
            release.Set();
            UI.Stop();
            System.Console.SetOut(origOut);
        }
    }

    // ② ---------------------------------------------------------------------------------------------------------
    [Fact]
    public void Paint_FiresHandlerWhoseTargetIsNotAControl()
    {
        var fired = 0;
        EventHandler<UI.PaintEventArgs> h = (_, _) => fired++;   // a plain lambda — target is not an IFocusable control
        UI.Paint += h;
        try
        {
            UI.PaintFrame();
            UI.PaintFrame();
            Assert.Equal(2, fired);   // previously 0 — the non-control handler was silently dropped by the add accessor
        }
        finally
        {
            UI.Paint -= h;
        }

        UI.PaintFrame();
        Assert.Equal(2, fired);   // unsubscribed → no longer fires
    }

    // ③ ---------------------------------------------------------------------------------------------------------
    [Fact]
    public void FeedHandle_Completion_CompletesAfterCancel()
    {
        var probe = new FeedProbe();
        var h = probe.Tick(() => { }, 10);
        Assert.False(h.Completion.IsCompleted);

        h.Cancel();
        Assert.True(h.Completion.Wait(2000), "cancelling the feed should stop the loop and complete Completion");
    }

    [Fact]
    public void Feed_ThrowingProducer_InvokesOnError_AndStops()
    {
        UiTestHarness.EnsureStopped();
        var origOut = System.Console.Out;
        System.Console.SetOut(System.IO.TextWriter.Null);
        Exception? captured = null;
        var gotError = new ManualResetEventSlim(false);
        try
        {
            var run = UI.Start(new VerticalStackPanel(new TextLabel(TextLabelOrientation.Horizontal, "x")),
                40, 8, fps: 66, isAnsiTerminal: true, console: new StubConsole(40, 8), input: new NoInput());

            var probe = new FeedProbe();
            var h = probe.Produce<int>(
                produce: () => throw new InvalidOperationException("boom"),
                apply: _ => { },
                intervalMs: 10,
                onError: ex => { captured = ex; gotError.Set(); });

            Assert.True(gotError.Wait(2000), "a throwing producer should invoke onError on the UI thread");
            Assert.IsType<InvalidOperationException>(captured);
            Assert.True(h.Completion.Wait(2000), "and the feed should stop after the throw");
        }
        finally
        {
            UI.Stop();
            System.Console.SetOut(origOut);
        }
    }

    // ④ ---------------------------------------------------------------------------------------------------------
    // A hotkey belongs to the session that registered it. This only matters to an app that starts the UI more than
    // once in a process -- swapping between two screens, say -- and there it matters a lot: the surviving binding is
    // a closure over the previous session's objects, which have been disposed by the time it fires.
    // No UI session at all: with no UI thread running, RegisterHotKey/RestoreBuiltInHotKeys marshal inline, so the
    // reset's own behaviour is asserted without depending on a loop starting, running and stopping first.
    [Fact]
    public void RestoreBuiltInHotKeys_DropsAppKeys_AndKeepsTheLibrarys()
    {
        UiTestHarness.EnsureStopped();
        var fired = 0;
        var key = UI.HotKeys.Char('~');
        var label = new TextLabel(TextLabelOrientation.Horizontal, "x");

        UI.RegisterHotKey(key, () => fired++);
        UI.SendInput(label, key, routeGlobal: true);
        Assert.Equal(1, fired);

        UI.RestoreBuiltInHotKeys();

        UI.SendInput(label, key, routeGlobal: true);
        Assert.Equal(1, fired);

        // F1 is the built-in help toggle, and a no-op with no control supplying help — so the observable claim is
        // that the global route still CONSUMES the key, i.e. the library's own bindings survived the reset.
        var f1 = new ConsoleGUI.Input.InputEvent(UI.HotKeys.F1);
        new UI.GlobalInputListener().OnInput(f1);
        Assert.True(f1.Handled);
    }

    // The scenario the reset exists for: one process, two UIs. Without it the second session inherits a binding
    // closed over the first session's objects, which by then have been disposed.
    [Fact]
    public void HotKeysRegisteredForASession_DoNotReachTheNextOne()
    {
        UiTestHarness.EnsureStopped();
        var fired = 0;
        var key = UI.HotKeys.Char('~');
        var first = new TextLabel(TextLabelOrientation.Horizontal, "first");
        var second = new TextLabel(TextLabelOrientation.Horizontal, "second");

        // Registered BEFORE Start, as an app assembling its shell does — and as it happens the only ordering with no
        // UI thread to marshal through, so the registration cannot be dropped by a loop that is still settling.
        UI.RegisterHotKey(key, () => fired++);
        _ = UI.Start(new VerticalStackPanel(first), 40, 8, fps: 66,
            isAnsiTerminal: true, console: new StubConsole(40, 8), input: new NoInput());
        UI.SendInput(first, key, routeGlobal: true);
        Assert.Equal(1, fired);
        UI.Stop();

        _ = UI.Start(new VerticalStackPanel(second), 40, 8, fps: 66,
            isAnsiTerminal: true, console: new StubConsole(40, 8), input: new NoInput());
        try
        {
            UI.SendInput(second, key, routeGlobal: true);
            Assert.Equal(1, fired);
        }
        finally
        {
            UI.Stop();
        }
    }

    // Helpers -------------------------------------------------------------------------------------------------
    private sealed class FeedProbe : Control
    {
        protected override void Render() { }
        public FeedHandle Tick(Action tick, int intervalMs) => Feed(tick, intervalMs);
        public FeedHandle Produce<T>(Func<T> produce, Action<T> apply, int intervalMs, Action<Exception>? onError) =>
            Feed(produce, apply, intervalMs, onError);
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
        public ConsoleKeyInfo ReadKey() => throw new System.NotSupportedException();
    }
}
