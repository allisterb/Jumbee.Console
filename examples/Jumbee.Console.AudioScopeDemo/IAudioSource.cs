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

    /// <summary>Returns the next block of samples de-interleaved into a <c>[channels][samplesPerChannel]</c> matrix.</summary>
    double[][] NextFrame();
}
