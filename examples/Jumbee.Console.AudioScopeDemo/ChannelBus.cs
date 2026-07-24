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
/// </remarks>
public sealed class ChannelBus
{
    /// <summary>One published snapshot: an immutable channel matrix tagged with a monotonic version.</summary>
    public sealed record Frame(long Version, double[][] Channels);

    /// <summary>The most recently published frame, or <see langword="null"/> before the first publish.</summary>
    public Frame? Latest => latest;

    /// <summary>Publishes a freshly-decoded matrix as the new latest frame. Called only from the single pump thread,
    /// so the version bump needs no interlock; the volatile write makes the new frame visible to every reader.</summary>
    public void Publish(double[][] channels) => latest = new Frame(++version, channels);

    private volatile Frame? latest;
    private long version;
}
