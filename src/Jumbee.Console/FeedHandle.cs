namespace Jumbee.Console;

using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// A handle to a running background feed started by <see cref="Control.Feed(Action, int, Action{Exception})"/> and its overloads.
/// Cancel it to stop the feed; await <see cref="Completion"/> (or <see cref="StopAsync"/>) to know the in-flight tick
/// has finished — for safely disposing a resource the feed's producer reads.
/// </summary>
public sealed class FeedHandle : IDisposable
{
    #region Constructors
    internal FeedHandle(CancellationTokenSource cts, Task completion)
    {
        _cts = cts;
        Completion = completion;
    }
    #endregion

    #region Properties
    /// <summary>Completes when the feed's loop has fully stopped — the in-flight tick has finished running. Await this
    /// after <see cref="Cancel"/> (or use <see cref="StopAsync"/>) before disposing anything the producer touches, so
    /// the resource is never torn down under a live tick.</summary>
    public Task Completion { get; }
    #endregion

    #region Methods
    /// <summary>Requests the feed to stop. Idempotent; does not wait — await <see cref="Completion"/> to join.</summary>
    public void Cancel() => _cts.Cancel();

    /// <summary>Stops the feed and returns a task that completes once the in-flight tick has finished.</summary>
    public Task StopAsync()
    {
        _cts.Cancel();
        return Completion;
    }

    /// <summary>Stops the feed (same as <see cref="Cancel"/>). Disposing the control that started it cancels it too.</summary>
    public void Dispose() => _cts.Cancel();
    #endregion

    #region Fields
    private readonly CancellationTokenSource _cts;
    #endregion
}
