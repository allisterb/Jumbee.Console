namespace ScopeTui;

using NAudio.Wave;
using NLayer.NAudioSupport;

/// <summary>
/// Decodes an audio file to interleaved float samples and de-interleaves into a channel matrix, mirroring
/// scope-tui's stream_to_matrix (src/input/mod.rs). No audio is played back — this only reads/decodes samples,
/// looping back to the start at end-of-file.
/// </summary>
/// <remarks>
/// MP3s are decoded with NLayer's fully-managed decoder (via <see cref="Mp3FrameDecompressor"/>) — no native
/// dependency and no Windows-only ACM/DMO codec — so MP3 decode is cross-platform (Linux/macOS/Docker). WAV is read
/// with <see cref="WaveFileReader"/> (also managed, in NAudio.Core). Both keep the demo fully portable.
/// </remarks>
public sealed class FileAudioSource : IAudioSource
{
    public int Channels { get; }
    public int SampleRate { get; }

    readonly WaveStream reader;
    readonly ISampleProvider sampler;
    readonly float[] window;      // interleaved; the frame handed out, retained across calls when overlapping
    int advance;                  // interleaved samples of NEW audio per frame (window.Length when not overlapping)
    bool primed;

    /// <param name="overlap">Fraction of each frame re-used by the next one, 0 (none) to just under 1. Overlapping
    /// makes consecutive frames share audio so the display can refresh faster than one whole buffer at a time —
    /// what a spectrum analyser does to keep a large FFT watchable.</param>
    public FileAudioSource(string path, int bufferSamplesPerChannel, double overlap = 0.0)
    {
        reader = Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".mp3" => new Mp3FileReaderBase(path, waveFormat => new Mp3FrameDecompressor(waveFormat)),
            ".wav" => new WaveFileReader(path),
            var ext => throw new NotSupportedException(
                $"Unsupported audio file type '{ext}'. Use .mp3 or .wav, or capture a device with the 'live' input."),
        };
        sampler = reader.ToSampleProvider();
        Channels = reader.WaveFormat.Channels;
        SampleRate = reader.WaveFormat.SampleRate;
        window = new float[bufferSamplesPerChannel * Channels];
        // Retain-and-shift rather than seeking the reader back: a shift is exact and codec-agnostic, where rewinding
        // an MP3 snaps to a frame boundary. The pump ticks proportionally faster (see Program.cs), so the file still
        // advances at realtime -- overlapping changes how much of each frame is NEW, not how fast the track plays.
        SetOverlap(overlap);
    }

    /// <inheritdoc/>
    public void SetOverlap(double overlap) =>
        // At least one frame of audio per call, so a read always makes progress however extreme the overlap.
        advance = Math.Max(Channels, (int)Math.Round(window.Length * (1.0 - Math.Clamp(overlap, 0.0, 0.95))));

    /// <summary>
    /// Reads the next buffer's worth of samples (looping back to the start at end-of-file, since this is a
    /// decode-only demo source, not a live device) and de-interleaves it into a channels x samples matrix. Any
    /// decode failure propagates to the caller, which surfaces it as visible UI state (see the pump in Program.cs).
    /// </summary>
    public double[][] NextFrame()
    {
        if (!primed || advance >= window.Length)
        {
            // First frame, or no overlap: the whole window is new audio.
            primed = true;
            var read = ReadLooping(window);
            return Deinterleave(window, Channels, read);
        }

        // Overlapping: keep the tail of the previous frame, slide it to the front, and read only the new audio in
        // behind it. Every sample is still valid, so no partial-length de-interleave.
        Array.Copy(window, advance, window, 0, window.Length - advance);
        ReadLooping(window.AsSpan(window.Length - advance));
        return Deinterleave(window, Channels);
    }

    // NAudio 3's ISampleProvider.Read is Span-based (no offset/count) -- read into the whole span, then top up from
    // the start of the stream if we hit EOF mid-read (this is a decode-only demo source, so it loops).
    int ReadLooping(Span<float> destination)
    {
        var read = sampler.Read(destination);
        if (read < destination.Length)
        {
            reader.Position = 0;
            read += sampler.Read(destination[read..]);
        }
        return read;
    }

    /// <summary>
    /// De-interleaves a flat sample buffer (channel-interleaved, e.g. LRLRLR...) into a channels x
    /// samples-per-channel matrix. A pure static function so the de-interleave logic can be unit-tested directly and
    /// reused by <see cref="RecordingAudioSource"/>.
    /// </summary>
    public static double[][] Deinterleave(float[] interleaved, int channels, int? validLength = null)
    {
        var read = validLength ?? interleaved.Length;
        var samplesPerChannel = interleaved.Length / channels;
        var matrix = new double[channels][];
        for (var c = 0; c < channels; c++) matrix[c] = new double[samplesPerChannel];

        var channel = 0;
        for (var i = 0; i < read; i++)
        {
            var idx = i / channels;
            if (idx < samplesPerChannel) matrix[channel][idx] = interleaved[i];
            channel = (channel + 1) % channels;
        }

        return matrix;
    }

    public void Dispose() => reader.Dispose();
}
