namespace Jumbee.Console;

using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// A handle to a background job started by <c>Control.Job</c>. Call <see cref="Request"/> to ask for a run; cancel
/// it to stop the job.
/// </summary>
/// <remarks>
/// The counterpart to <see cref="FeedHandle"/>: a feed runs on a timer, a job runs when something asks it to — see
/// <see cref="Control.Job{T}(Func{T}, Action{T}, Action{Exception})"/>. Await <see cref="Completion"/> (or
/// <see cref="StopAsync"/>) after cancelling to know the in-flight run has finished, before disposing anything the
/// producer reads.
/// </remarks>
public sealed class JobHandle : IDisposable
{
    #region Constructors
    internal JobHandle(CancellationTokenSource cts, SemaphoreSlim signal)
    {
        _cts = cts;
        _signal = signal;
    }
    #endregion

    #region Properties
    /// <summary>Completes when the job's loop has fully stopped and the in-flight run has finished. Await this after
    /// <see cref="Cancel"/> (or use <see cref="StopAsync"/>) before tearing down anything the producer touches.</summary>
    public Task Completion { get; internal set; } = Task.CompletedTask;

    /// <summary>How many runs have completed. Diagnostics, and what a test waits on.</summary>
    public long Completed => Interlocked.Read(ref _completed);

    /// <summary>
    /// How many <see cref="Request"/> calls were absorbed into an already-pending run rather than causing one of
    /// their own.
    /// </summary>
    /// <remarks>
    /// The measure of how far the producer is behind its callers: a steadily climbing count means requests arrive
    /// faster than the job can serve them, which is the job doing its job — the alternative is an unbounded queue
    /// of stale work.
    /// </remarks>
    public long Coalesced => Interlocked.Read(ref _coalesced);
    #endregion

    #region Methods
    /// <summary>
    /// Asks for a run. Returns immediately.
    /// </summary>
    /// <remarks>
    /// <b>At most one run is ever in flight, and at most one more is ever queued.</b> Calling this a hundred times
    /// while a run is going produces exactly one further run, not a hundred — the requests collapse (see
    /// <see cref="Coalesced"/>). That is the whole point for a render queue: what a caller wants is "the newest
    /// state on screen soon", never "every intermediate state, eventually".
    /// </remarks>
    public void Request()
    {
        if (_cts.IsCancellationRequested) return;

        // 0 -> 1 is this caller claiming the pending slot and waking the loop. 1 -> 1 means a run is already queued,
        // so there is nothing to do but count it: the queued run will pick up whatever state exists when it starts.
        if (Interlocked.Exchange(ref _pending, 1) != 0)
        {
            Interlocked.Increment(ref _coalesced);
            return;
        }

        try { _signal.Release(); }
        catch (ObjectDisposedException) { /* cancelled and torn down between the check and here */ }
        catch (SemaphoreFullException) { /* loop has not consumed the last release yet; it will */ }
    }

    /// <summary>Requests the job to stop. Idempotent; does not wait — await <see cref="Completion"/> to join.</summary>
    public void Cancel() => _cts.Cancel();

    /// <summary>Stops the job and returns a task that completes once the in-flight run has finished.</summary>
    public Task StopAsync()
    {
        _cts.Cancel();
        return Completion;
    }

    /// <summary>Stops the job (same as <see cref="Cancel"/>). Disposing the control that started it cancels it too.</summary>
    public void Dispose() => _cts.Cancel();
    #endregion

    #region Internal members
    // Cleared by the loop BEFORE the producer runs, not after: a Request arriving mid-run must leave the slot claimed
    // again so the loop goes straight round for another pass. Clearing afterwards would drop that request and leave
    // the screen showing state the caller had already superseded.
    internal void ClaimPending() => Interlocked.Exchange(ref _pending, 0);

    internal void CountCompleted() => Interlocked.Increment(ref _completed);
    #endregion

    #region Fields
    private readonly CancellationTokenSource _cts;
    private readonly SemaphoreSlim _signal;
    private int _pending;
    private long _completed;
    private long _coalesced;
    #endregion
}
