namespace Jumbee.Console;

using System;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Threading;

/// <summary>
/// Collects live process/runtime performance metrics for the perf HUD by reading the runtime APIs the
/// <c>System.Runtime</c> meter wraps (<see cref="GC"/>, <see cref="Environment"/>, <see cref="ThreadPool"/>,
/// <see cref="Monitor"/>) directly — no <c>MeterListener</c>, so there is nothing to sample-schedule and no
/// observable-instrument staleness.
/// </summary>
/// <remarks>
/// <para>Two measurement styles: <see cref="RecordFrame"/> takes the <em>directly measured</em> cost of one
/// draw/paint cycle (high-resolution wall time + allocation delta, bracketed around the render in the UI loop) —
/// this is immune to the coarse (~15&#160;ms) resolution of process CPU time and, read as a <em>peak</em>, surfaces
/// bursts an average would hide. <see cref="Sample"/> snapshots cumulative process counters (CPU time, GC pause,
/// exceptions), differenced over a rolling window for whole-process rates.</para>
/// <para>All state is written and read on the UI thread; only the first-chance exception count is cross-thread
/// (it uses <see cref="Interlocked"/>).</para>
/// </remarks>
public sealed class ProcessMetrics : IDisposable
{
    #region Constructors
    /// <param name="capacity">Number of snapshots/frames retained (covers <paramref name="windowMs"/> at the frame
    /// rate; the default covers several seconds at a 100&#160;ms tick).</param>
    /// <param name="windowMs">Rolling window over which whole-process rates (CPU%, GC-pause%, exceptions/s) are measured.</param>
    public ProcessMetrics(int capacity = 128, int windowMs = 1000)
    {
        capacity = Math.Max(2, capacity);
        _samples = new Snapshot[capacity];
        _frameRenderMs = new double[capacity];
        _framePeriodMs = new double[capacity];
        _frameAlloc = new long[capacity];
        _frameRedrawn = new bool[capacity];
        _frameDirty = new double[capacity];
        _frameOrdinal = new long[capacity];
        _frameWaitMs = new double[capacity];
        _frameWriteMs = new double[capacity];
        _frameWritten = new bool[capacity];
        _windowMs = windowMs;
        // dotnet.process.cpu.time is platform-guarded in the runtime; Environment.CpuUsage throws
        // PlatformNotSupportedException on Browser/WASI/tvOS/iOS(non-Catalyst). Mirror that guard.
        _cpuSupported = !OperatingSystem.IsBrowser() && !OperatingSystem.IsWasi()
            && !OperatingSystem.IsTvOS() && !(OperatingSystem.IsIOS() && !OperatingSystem.IsMacCatalyst());
    }
    #endregion

    #region Properties — per-frame render cost (directly measured, high-resolution)
    /// <summary>Mean draw/paint cycle wall time over the retained frames, in milliseconds.</summary>
    public double RenderTimeMsAvg => Avg(_frameRenderMs);

    /// <summary>Mean draw/paint cycle wall time over the frames that actually REDREW, in milliseconds.</summary>
    /// <remarks>
    /// <see cref="RenderTimeMsAvg"/> counts idle frames too, which are cheap, so it understates what a frame that
    /// draws actually costs — the more retained the UI, the further apart the two drift. This is the one to pair
    /// with the terminal-write timings, since those exist only for frames that drew.
    /// </remarks>
    public double RenderTimeMsDrawnAvg
    {
        get
        {
            double total = 0;
            int count = 0;
            for (int i = 0; i < _fCount; i++)
            {
                if (!_frameRedrawn[i]) continue;
                total += _frameRenderMs[i];
                count++;
            }
            return count > 0 ? total / count : 0;
        }
    }

    /// <summary>
    /// Mean end-to-end time (ms) from starting a frame to it reaching the terminal — render, queue wait and write,
    /// summed PER FRAME over the frames whose write has completed.
    /// </summary>
    /// <remarks>
    /// LATENCY, not throughput: the write overlaps the next frame's render, so the frame rate is bounded by
    /// whichever side is slower, never by this sum.
    /// </remarks>
    public double FrameLatencyMsAvg
    {
        get
        {
            double total = 0;
            int count = 0;
            for (int i = 0; i < _fCount; i++)
            {
                if (!_frameWritten[i]) continue;
                total += _frameRenderMs[i] + _frameWaitMs[i] + _frameWriteMs[i];
                count++;
            }
            return count > 0 ? total / count : 0;
        }
    }

    /// <summary>The worst end-to-end frame latency (ms) in the window — the frame that took longest to reach the
    /// terminal.</summary>
    /// <remarks>Only a per-frame pairing can give this: adding the separate averages yields a mean and can say
    /// nothing about the worst case.</remarks>
    public double FrameLatencyMsPeak
    {
        get
        {
            double peak = 0;
            for (int i = 0; i < _fCount; i++)
            {
                if (!_frameWritten[i]) continue;
                double latency = _frameRenderMs[i] + _frameWaitMs[i] + _frameWriteMs[i];
                if (latency > peak) peak = latency;
            }
            return peak;
        }
    }

    /// <summary>Longest draw/paint cycle over the retained frames, in milliseconds — the peak render cost, which a
    /// burst (e.g. a paste re-rendering the editor) pushes up and which lingers until it ages out of the window.</summary>
    public double RenderTimeMsPeak => Max(_frameRenderMs);

    /// <summary>Peak UI-thread utilisation over the retained frames (0..100): the busiest frame's render time as a
    /// fraction of that frame's period.</summary>
    /// <remarks>~0 when idle (short render, long wait), rising toward 100 when frames run back-to-back under load.
    /// High-resolution, so unlike process CPU% it reflects brief bursts.</remarks>
    public double BusyPercentPeak
    {
        get
        {
            double peak = 0;
            for (int i = 0; i < _fCount; i++)
            {
                double period = _framePeriodMs[i];
                if (period <= 0) continue;
                double util = _frameRenderMs[i] / period * 100.0;
                if (util > peak) peak = util;
            }
            return Math.Min(100, peak);
        }
    }

    /// <summary>Mean UI-thread utilisation over the retained frames (0..100) — the typical busy fraction, which
    /// stays low for a retained UI while <see cref="BusyPercentPeak"/> spikes on a burst frame.</summary>
    public double BusyPercentAvg
    {
        get
        {
            double total = 0; int n = 0;
            for (int i = 0; i < _fCount; i++)
            {
                double period = _framePeriodMs[i];
                if (period <= 0) continue;
                total += Math.Min(100, _frameRenderMs[i] / period * 100.0);
                n++;
            }
            return n > 0 ? total / n : 0;
        }
    }

    /// <summary>Mean bytes allocated on the managed heap per draw/paint cycle.</summary>
    /// <remarks>The headline retained-mode number: an idle UI allocates ~nothing per frame.</remarks>
    public double AllocatedBytesPerFrame => Avg(_frameAlloc);

    /// <summary>Peak bytes allocated in a single draw/paint cycle over the retained frames — surfaces an allocation
    /// burst an average would dilute away.</summary>
    public long PeakAllocatedBytesPerFrame
    {
        get { long p = 0; for (int i = 0; i < _fCount; i++) if (_frameAlloc[i] > p) p = _frameAlloc[i]; return p; }
    }

    /// <summary>Percentage of retained frames that actually redrew the screen (took the full draw path) versus those
    /// that idled (cheap resize-check only).</summary>
    /// <remarks>The retained-mode headline: a mostly-static UI redraws only the few frames where content changed, so
    /// this sits low and climbs toward 100 only under interaction/animation.</remarks>
    public double RedrawPercent
    {
        get { if (_fCount == 0) return 0; int n = 0; for (int i = 0; i < _fCount; i++) if (_frameRedrawn[i]) n++; return n * 100.0 / _fCount; }
    }

    /// <summary>Mean fraction of the screen (0..100) re-composited on the frames that <em>did</em> redraw.</summary>
    /// <remarks>With dirty-rectangle rendering a small change (a status-bar tick) touches only its own rows, so this
    /// reads a few percent even while <see cref="RedrawPercent"/> is high; only a resize/theme-switch pushes a frame
    /// to 100.</remarks>
    public double DirtyAreaPercentAvg
    {
        get
        {
            double total = 0; int n = 0;
            for (int i = 0; i < _fCount; i++) if (_frameRedrawn[i]) { total += _frameDirty[i]; n++; }
            return n > 0 ? total / n * 100.0 : 0;
        }
    }

    /// <summary>Largest fraction of the screen (0..100) re-composited in a single frame over the window — a resize
    /// pushes this to 100; steady interaction keeps it small.</summary>
    public double DirtyAreaPercentPeak
    {
        get { double p = 0; for (int i = 0; i < _fCount; i++) if (_frameDirty[i] > p) p = _frameDirty[i]; return p * 100.0; }
    }
    #endregion

    #region Properties — whole-process rates & gauges
    /// <summary><see langword="true"/> if per-process CPU time is available on this OS (see the constructor guard).</summary>
    public bool CpuSupported => _cpuSupported;

    /// <summary>Whole-process CPU usage over the rolling window, as a percentage of total machine capacity (user +
    /// kernel, divided by <see cref="Environment.ProcessorCount"/>) — the same figure Task Manager shows for the
    /// process. 0 when unavailable.</summary>
    /// <remarks><em>Coarse</em>: process CPU time advances in ~15&#160;ms OS ticks, so it reflects sustained load but
    /// not brief per-frame bursts (use <see cref="BusyPercentPeak"/> / <see cref="RenderTimeMsPeak"/> for those).</remarks>
    public double CpuUsagePercent
    {
        get
        {
            if (!_cpuSupported || !TryWindow(out var o, out var n, out var ms)) return 0;
            return Math.Max(0, (n.CpuMs - o.CpuMs) / ms * 100.0 / Math.Max(1, Environment.ProcessorCount));
        }
    }

    /// <summary>Physical memory mapped to the process (<see cref="Environment.WorkingSet"/>) right now, in bytes —
    /// an instantaneous gauge, not a rate.</summary>
    /// <remarks>Use <see cref="WorkingSetBytesAvg"/>/<see cref="WorkingSetBytesPeak"/> for the windowed figures.</remarks>
    public long WorkingSetBytes => Environment.WorkingSet;

    /// <summary>Mean working set over the retained snapshots (sampled once per frame), in bytes.</summary>
    /// <remarks>Since the working set is sticky, this tracks close to the current value — a resize that grows it
    /// lifts the average and it stays up (unlike the alloc average, which falls back once the burst frame ages
    /// out).</remarks>
    public double WorkingSetBytesAvg
    {
        get { if (_count == 0) return WorkingSetBytes; long t = 0; for (int i = 0; i < _count; i++) t += At(i).WorkingSet; return (double)t / _count; }
    }

    /// <summary>Highest working set over the retained snapshots, in bytes — the footprint high-water mark within the
    /// window (a resize/paste burst pushes it up and it lingers until it ages out).</summary>
    public long WorkingSetBytesPeak
    {
        get { long p = 0; for (int i = 0; i < _count; i++) { var w = At(i).WorkingSet; if (w > p) p = w; } return p; }
    }

    /// <summary>Current managed heap size (<see cref="GC.GetTotalMemory(bool)"/>), in bytes.</summary>
    public long ManagedHeapBytes => GC.GetTotalMemory(forceFullCollection: false);

    /// <summary>Managed heap allocation rate over the rolling window, in bytes per second (whole process).</summary>
    public double AllocatedBytesPerSecond
        => TryWindow(out var o, out var n, out var ms) ? Math.Max(0, (n.Allocated - o.Allocated) / (ms / 1000.0)) : 0;

    /// <summary>Fraction of wall time (0..100) the runtime spent paused for GC over the rolling window — the
    /// clearest signal that GC is hitching the UI.</summary>
    public double GcPausePercent
        => TryWindow(out var o, out var n, out var ms) ? Math.Clamp((n.GcPauseMs - o.GcPauseMs) / ms * 100.0, 0, 100) : 0;

    /// <summary>First-chance managed exceptions per second over the rolling window (thrown, caught or not).</summary>
    public double ExceptionsPerSecond
        => TryWindow(out var o, out var n, out var ms) ? Math.Max(0, (n.Exceptions - o.Exceptions) / (ms / 1000.0)) : 0;

    /// <summary>Monitor lock contentions per second over the rolling window — the no-lock dagger; 0 for the
    /// single-threaded UI.</summary>
    /// <remarks>
    /// A RATE, not a total, so it describes contention happening now. <see cref="LockContentions"/> only ever
    /// climbs, so a handful picked up during startup reads as permanent trouble long after everything has settled.
    /// </remarks>
    public double LockContentionsPerSecond
        => TryWindow(out var o, out var n, out var ms) ? Math.Max(0, (n.LockContentions - o.LockContentions) / (ms / 1000.0)) : 0;

    /// <summary>Total monitor lock contentions since process start.</summary>
    /// <remarks>Cumulative and never reset, so it cannot say whether contention is happening <em>now</em> — prefer
    /// <see cref="LockContentionsPerSecond"/> for that.</remarks>
    public long LockContentions => Monitor.LockContentionCount;

    /// <summary>Cumulative number of generation-0 garbage collections since process start.</summary>
    public int Gen0Collections => GC.CollectionCount(0);
    /// <summary>Cumulative number of generation-1 garbage collections since process start.</summary>
    public int Gen1Collections => GC.CollectionCount(1);
    /// <summary>Cumulative number of generation-2 garbage collections since process start.</summary>
    public int Gen2Collections => GC.CollectionCount(2);

    /// <summary>Current number of thread-pool worker threads.</summary>
    public int ThreadPoolThreadCount => ThreadPool.ThreadCount;
    /// <summary>Number of work items currently queued to the thread pool.</summary>
    public long ThreadPoolQueueLength => ThreadPool.PendingWorkItemCount;
    #endregion

    #region Methods
    /// <summary>Begins collection: clears the retained frame/snapshot history (so a new UI session doesn't average
    /// in the previous one's frames), subscribes to first-chance exceptions, and takes a baseline cumulative sample.</summary>
    public void Start()
    {
        if (_started) return;
        _started = true;
        _fStart = _fCount = 0;
        _start = _count = 0;
        AppDomain.CurrentDomain.FirstChanceException += OnFirstChanceException;
        Sample();
    }

    /// <summary>Stops collection and unsubscribes.</summary>
    public void Stop()
    {
        if (!_started) return;
        _started = false;
        AppDomain.CurrentDomain.FirstChanceException -= OnFirstChanceException;
    }

    /// <summary>Records the directly-measured cost of one draw/paint cycle (from the UI loop, which brackets the
    /// render), and takes a cumulative sample for the windowed process rates.</summary>
    /// <remarks>Call once per frame on the UI thread.</remarks>
    /// <param name="renderMs">Wall time the draw/paint cycle took (high-resolution).</param>
    /// <param name="periodMs">Wall time since the previous frame started (for utilisation).</param>
    /// <param name="renderAllocBytes">Bytes allocated on the managed heap during the cycle.</param>
    /// <param name="redrawn"><see langword="true"/> if this frame took the full draw path; <see langword="false"/>
    /// for an idle frame. Feeds <see cref="RedrawPercent"/>.</param>
    /// <param name="dirtyAreaFraction">Fraction (0..1) of the screen re-composited this frame. Feeds
    /// <see cref="DirtyAreaPercentAvg"/>/<see cref="DirtyAreaPercentPeak"/> (only counted on redrawn frames).</param>
    /// <param name="ordinal">This frame's monotonic number, so a terminal write completing later can be matched back
    /// to it by <see cref="RecordWrite"/>. 0 when the caller does not track one.</param>
    public void RecordFrame(double renderMs, double periodMs, long renderAllocBytes, bool redrawn = false, double dirtyAreaFraction = 0, long ordinal = 0)
    {
        int idx = (_fStart + _fCount) % _frameRenderMs.Length;
        _frameRenderMs[idx] = renderMs;
        _framePeriodMs[idx] = periodMs;
        _frameAlloc[idx] = renderAllocBytes < 0 ? 0 : renderAllocBytes;
        _frameRedrawn[idx] = redrawn;
        _frameDirty[idx] = dirtyAreaFraction < 0 ? 0 : dirtyAreaFraction;
        _frameOrdinal[idx] = ordinal;
        // Take this frame's write if it already finished (the usual case for a fast write — see RecordWrite),
        // otherwise clear the slot: stale timings from the frame that occupied it a window ago would otherwise be
        // read as this frame's.
        if (ordinal != 0 && TryClaimPendingWrite(ordinal, out var pendingWait, out var pendingWrite))
        {
            _frameWaitMs[idx] = pendingWait;
            _frameWriteMs[idx] = pendingWrite;
            _frameWritten[idx] = true;
        }
        else
        {
            _frameWritten[idx] = false;
            _frameWaitMs[idx] = 0;
            _frameWriteMs[idx] = 0;
        }
        if (_fCount < _frameRenderMs.Length) _fCount++;
        else _fStart = (_fStart + 1) % _frameRenderMs.Length;

        Sample();
    }

    /// <summary>
    /// Attaches the terminal-write timings for frame <paramref name="ordinal"/>, completing that frame's record.
    /// </summary>
    /// <remarks>
    /// Called from the write continuation, off the UI thread, however many frames after the render. Silently does
    /// nothing when the frame has already aged out of the window — a write that slow has no frame left to belong to.
    /// </remarks>
    public void RecordWrite(long ordinal, double waitMs, double writeMs)
    {
        int count = _fCount, start = _fStart, len = _frameRenderMs.Length;
        for (int i = 0; i < count; i++)
        {
            int idx = (start + i) % len;
            if (_frameOrdinal[idx] != ordinal) continue;
            _frameWaitMs[idx] = waitMs;
            _frameWriteMs[idx] = writeMs;
            _frameWritten[idx] = true;
            return;
        }

        // No slot yet: the write finished before its own frame was recorded. That is the COMMON case for a fast
        // write — Emit runs mid-frame during the composite, RecordFrame only in the finally at the end — so dropping
        // these would silently keep just the slow frames and inflate every latency reading. Park it for RecordFrame.
        int p = (int)((uint)Interlocked.Increment(ref _pendingIndex) % (uint)_pendingOrdinal.Length);
        _pendingWaitMs[p] = waitMs;
        _pendingWriteMs[p] = writeMs;
        Volatile.Write(ref _pendingOrdinal[p], ordinal);   // published last: a reader seeing it sees the timings too
    }

    // Claims a write that arrived before this frame was recorded. Cheap: the buffer is a handful of entries, since a
    // write can only outrun its frame by the tail of one composite.
    private bool TryClaimPendingWrite(long ordinal, out double waitMs, out double writeMs)
    {
        for (int i = 0; i < _pendingOrdinal.Length; i++)
        {
            if (Volatile.Read(ref _pendingOrdinal[i]) != ordinal) continue;
            waitMs = _pendingWaitMs[i];
            writeMs = _pendingWriteMs[i];
            Volatile.Write(ref _pendingOrdinal[i], 0);
            return true;
        }

        waitMs = writeMs = 0;
        return false;
    }

    /// <summary>Snapshots the cumulative process counters (for the windowed rates).</summary>
    /// <remarks>Called by <see cref="RecordFrame"/> once per frame; exposed for callers/tests without a frame loop.</remarks>
    public void Sample()
    {
        long timestamp = Stopwatch.GetTimestamp();
        long allocated = GC.GetTotalAllocatedBytes();                        // approximate, cheap, no allocation
        double cpuMs = _cpuSupported ? Environment.CpuUsage.TotalTime.TotalMilliseconds : 0;
        double gcPauseMs = GC.GetTotalPauseDuration().TotalMilliseconds;
        long exceptions = Interlocked.Read(ref _exceptions);
        long workingSet = Environment.WorkingSet;                            // instantaneous gauge; cheap, no allocation
        long lockContentions = Monitor.LockContentionCount;                  // cumulative; differenced over the window
        Push(new Snapshot(timestamp, cpuMs, allocated, gcPauseMs, exceptions, workingSet, lockContentions));
    }

    /// <summary>Stops collection and releases resources.</summary>
    public void Dispose()
    {
        Stop();
        GC.SuppressFinalize(this);
    }

    private void OnFirstChanceException(object? sender, FirstChanceExceptionEventArgs e) => Interlocked.Increment(ref _exceptions);

    private static double Avg(double[] ring, int count) { if (count == 0) return 0; double t = 0; for (int i = 0; i < count; i++) t += ring[i]; return t / count; }
    private double Avg(double[] ring) => Avg(ring, _fCount);
    private double Avg(long[] ring) { if (_fCount == 0) return 0; long t = 0; for (int i = 0; i < _fCount; i++) t += ring[i]; return (double)t / _fCount; }
    private double Max(double[] ring) { double p = 0; for (int i = 0; i < _fCount; i++) if (ring[i] > p) p = ring[i]; return p; }

    // The newest cumulative snapshot and the oldest still within the rolling window, plus the wall time between them.
    private bool TryWindow(out Snapshot oldest, out Snapshot newest, out double elapsedMs)
    {
        oldest = default;
        newest = default;
        elapsedMs = 0;
        if (_count < 2) return false;

        newest = At(_count - 1);
        oldest = newest;
        double windowTicks = _windowMs * (Stopwatch.Frequency / 1000.0);
        for (int i = _count - 2; i >= 0; i--)
        {
            var s = At(i);
            if (newest.Timestamp - s.Timestamp <= windowTicks) oldest = s;
            else break;
        }
        elapsedMs = (newest.Timestamp - oldest.Timestamp) * 1000.0 / Stopwatch.Frequency;
        return elapsedMs > 0;
    }

    private void Push(in Snapshot s)
    {
        int idx = (_start + _count) % _samples.Length;
        _samples[idx] = s;
        if (_count < _samples.Length) _count++;
        else _start = (_start + 1) % _samples.Length;
    }

    private Snapshot At(int i) => _samples[(_start + i) % _samples.Length];
    #endregion

    #region Fields
    // Cumulative-snapshot ring (whole-process window rates).
    private readonly Snapshot[] _samples;
    private int _start;
    private int _count;

    // Per-frame render-cost rings (directly measured), pushed together under one index.
    private readonly double[] _frameRenderMs;
    private readonly double[] _framePeriodMs;
    private readonly long[] _frameAlloc;
    private readonly bool[] _frameRedrawn;
    private readonly double[] _frameDirty;
    // The terminal-write half of each frame's record, filled in LATER (the write completes off the UI thread, after
    // the frame that produced it has already been recorded). _frameOrdinal is the join key: a monotonic frame number
    // the writer carries with it, so a completion can find its own slot however many frames have passed since.
    private readonly long[] _frameOrdinal;
    private readonly double[] _frameWaitMs;
    private readonly double[] _frameWriteMs;
    private readonly bool[] _frameWritten;
    private int _fStart;
    private int _fCount;
    // Writes that finished before their frame was recorded, waiting to be claimed. Small on purpose: a write can
    // only outrun its own frame by the tail of one composite, so a few slots cover it.
    private readonly long[] _pendingOrdinal = new long[8];
    private readonly double[] _pendingWaitMs = new double[8];
    private readonly double[] _pendingWriteMs = new double[8];
    private int _pendingIndex = -1;

    private readonly int _windowMs;
    private readonly bool _cpuSupported;
    private long _exceptions;
    private bool _started;
    #endregion

    #region Types
    private readonly struct Snapshot(long timestamp, double cpuMs, long allocated, double gcPauseMs, long exceptions, long workingSet, long lockContentions)
    {
        public readonly long Timestamp = timestamp;     // Stopwatch.GetTimestamp ticks (high resolution)
        public readonly double CpuMs = cpuMs;
        public readonly long Allocated = allocated;
        public readonly double GcPauseMs = gcPauseMs;
        public readonly long Exceptions = exceptions;
        public readonly long WorkingSet = workingSet;   // Environment.WorkingSet at sample time, in bytes
        public readonly long LockContentions = lockContentions;
    }
    #endregion
}
