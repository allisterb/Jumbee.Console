namespace ScopeTui;

/// <summary>
/// A pull-based source of audio for the scope: each <see cref="NextFrame"/> returns the next (or, for a live
/// device, the most recent) block of samples as a channels x samples matrix. Implemented by
/// <see cref="FileAudioSource"/> (decode a file) and <see cref="RecordingAudioSource"/> (capture a live device),
/// so the pump/bus/scopes are agnostic to where the audio comes from.
/// </summary>
public interface IAudioSource : IDisposable
{
    /// <summary>Number of channels in each frame (e.g. 2 for stereo).</summary>
    int Channels { get; }

    /// <summary>The PCM sample rate in Hz (used by the spectroscope's frequency-bin math).</summary>
    int SampleRate { get; }

    /// <summary>
    /// Returns the next block of samples de-interleaved into a <c>[channels][samplesPerChannel]</c> matrix, or
    /// <see langword="null"/> when no new audio has arrived since the last call.
    /// </summary>
    /// <remarks>
    /// <b>Only a PUSH source ever returns null</b>, and the distinction is the point. A file is pulled: every call
    /// advances the stream, so there is always a new frame and <see cref="FileAudioSource"/> never returns null. A
    /// capture device is pushed on its own callback clock, and the pump samples faster than that clock delivers, so
    /// most calls find the same rolling window they saw last time. Answering null costs nothing; building the frame
    /// means cloning the window and de-interleaving it into a fresh matrix that the bus would then discard.
    /// <para>
    /// The first call always returns a frame, even from a device that has captured nothing yet, so the display has
    /// something to draw before any audio arrives.
    /// </para>
    /// </remarks>
    double[][]? NextFrame();

    /// <summary>
    /// Sets what fraction of each frame the next one re-uses, 0 (none) to just under 1. The caller must also tick
    /// this source proportionally faster, or overlapping simply slows the audio down.
    /// </summary>
    /// <remarks>
    /// Only meaningful for a source that reads sequentially: <see cref="FileAudioSource"/> has to retain the previous
    /// frame's tail to overlap at all. A live device already exposes a rolling latest-window, so how much consecutive
    /// frames share is decided purely by how often it is sampled — <see cref="RecordingAudioSource"/> ignores this.
    /// <para>Must not be called while a read is in flight; stop the pump first.</para>
    /// </remarks>
    void SetOverlap(double overlap);
}
