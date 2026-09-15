namespace ScopeTui;

/// <summary>
/// The single fan-out point for the three scope panes: one producer (the <see cref="AudioPump"/>) publishes the
/// most recently decoded sample matrix, and each pane reads the newest frame on its own feed clock. Deliberately a
/// latest-value register, NOT a queue -- a scope wants the current frame, not to consume every one, so a pane whose
/// feed ticks slower than the pump simply skips the intermediate frames (latest-wins).
/// </summary>
/// <remarks>
/// Thread-safety is a single volatile reference swap, no lock. Every <see cref="AudioSource.NextFrame"/> allocates a
/// fresh immutable matrix, so a published <see cref="Frame"/> is never mutated after the fact -- a pane can hold and
/// read it on its background thread with no torn state. The monotonic <see cref="Frame.Version"/> lets a pane tell
/// whether the data actually advanced since it last computed, so it can idle when nothing changed rather than
/// recompute an identical frame (e.g. while the pump is paused).
/// <para/>
/// That version counts frames whose CONTENT differs, not calls to <see cref="Publish"/> -- see the note there. A
/// pull source (a file) produces new audio on every call, so the two are the same thing; a push source (any capture
/// device) does not, and the difference is the whole point of the check.
/// </remarks>
public sealed class ChannelBus
{
    /// <summary>One published snapshot: an immutable channel matrix tagged with a monotonic version.</summary>
    public sealed record Frame(long Version, double[][] Channels);

    /// <summary>The most recently published frame, or <see langword="null"/> before the first publish.</summary>
    public Frame? Latest => latest;

    /// <summary>Raised after a new frame is published, on the pump thread. Lets a pane render when audio actually
    /// arrives instead of polling on its own clock; see ScopeView.StartComputeJob.</summary>
    /// <remarks>Handlers run on the pump thread and must be cheap and thread-safe - JobHandle.Request is both. A
    /// pane driven by a feed ignores this entirely and keeps polling Latest.</remarks>
    public event Action? Published;

    /// <summary>How many <see cref="Publish"/> calls carried audio identical to the frame already on the bus, and
    /// were dropped. Diagnostics: how far the pump is outrunning the source that feeds it.</summary>
    public long Unchanged => unchanged;

    /// <summary>
    /// Publishes a freshly-decoded matrix as the new latest frame, unless it is identical to the one already here.
    /// Called only from the single pump thread, so the version bump needs no interlock; the volatile write makes the
    /// new frame visible to every reader.
    /// </summary>
    /// <remarks>
    /// <b>An unchanged matrix is dropped: no version bump, no event.</b> The version is what every pane's idle-skip
    /// tests, so bumping it for identical audio defeated that skip entirely and made all three panes recompute --
    /// the spectroscope's FFT included -- for a window that had not moved.
    /// <para/>
    /// It only ever fired for a PUSH source. A file is pulled: each read advances the stream, so consecutive frames
    /// always differ and this check costs one mismatched element. A capture device pushes on its own callback
    /// clock, and the pump snapshots faster than that clock delivers, so it re-reads the same rolling window several
    /// times per callback -- and those repeats were being published as new audio. Dropping them also means a
    /// job-driven pane is never even woken, which is the cheapest kind of work not to do.
    /// <para/>
    /// The comparison is a vectorised <see cref="MemoryExtensions.SequenceEqual{T}(ReadOnlySpan{T}, ReadOnlySpan{T})"/>
    /// per channel that quits at the first difference, so the common "it did change" path stops almost immediately.
    /// NaN never equals itself, so a window containing one always counts as changed -- a wasted recompute rather
    /// than a stuck display, which is the right way round to be wrong.
    /// </remarks>
    public void Publish(double[][] channels)
    {
        if (latest is { } previous && SameAudio(previous.Channels, channels)) { unchanged++; return; }

        latest = new Frame(++version, channels);
        Published?.Invoke();   // after the swap, so a handler that reads Latest sees this frame
    }

    private static bool SameAudio(double[][] a, double[][] b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (a.Length != b.Length) return false;
        for (var c = 0; c < a.Length; c++)
            if (!a[c].AsSpan().SequenceEqual(b[c])) return false;
        return true;
    }

    private volatile Frame? latest;
    private long version;
    private long unchanged;   // pump thread only, like version
}
