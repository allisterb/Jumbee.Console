namespace ScopeTui;

using System;
using System.Threading.Tasks;

using Jumbee.Console;

/// <summary>
/// The single decode pump: a non-visual <see cref="Control"/> that owns the one <see cref="AudioSource"/> and runs
/// one <c>Control.Feed</c> to decode frames off the UI thread and publish them to the shared <see cref="ChannelBus"/>,
/// from which the three scope panes fan out. <see cref="AudioSource"/> is not thread-safe (one reader, one stream
/// position), so decoding MUST stay single-threaded -- this is that one producer, and the only thing that touches the
/// disposable reader, so it alone gates the dispose (the panes read only immutable bus frames).
/// </summary>
public sealed class AudioPump : Control
{
    private readonly AudioSource audio;
    private readonly ChannelBus bus;
    private FeedHandle? feed;

    public AudioPump(AudioSource audio, ChannelBus bus)
    {
        this.audio = audio;
        this.bus = bus;
    }

    /// <summary>Starts the decode feed: <c>produce</c> decodes the next frame and publishes it to the bus on a
    /// background thread. It always decodes -- pause is now per-pane (each pane freezes its own display against its
    /// config while the shared decoder keeps advancing), so the single pump can't gate the stream. <c>apply</c> is a
    /// no-op: publishing to the bus is a thread-safe volatile write, so it happens in <c>produce</c> off the UI thread
    /// rather than hopping onto it. Returns the <see cref="FeedHandle"/> for teardown.</summary>
    public FeedHandle Start(TimeSpan interval, Action<Exception>? onError = null) =>
        feed = Feed<object?>(
            produce: () =>
            {
                bus.Publish(audio.NextFrame());
                return null;
            },
            apply: _ => { },
            interval, onError);

    /// <summary>Stops the decode feed and awaits the in-flight decode finishing -- call before <see cref="DisposeAudio"/>
    /// so the reader is never disposed while a decode is mid-read on the feed thread.</summary>
    public Task StopAsync() => feed?.StopAsync() ?? Task.CompletedTask;

    /// <summary>Disposes the underlying reader. Only safe AFTER <see cref="StopAsync"/> has completed.</summary>
    public void DisposeAudio() => audio.Dispose();

    // Non-visual: this control exists only to own the decode feed and the AudioSource, and is never placed in the
    // layout tree, so there is nothing to draw.
    protected override void Render() { }
}
