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
    readonly float[] scratch;

    public FileAudioSource(string path, int bufferSamplesPerChannel)
    {
        reader = Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".mp3" => new Mp3FileReaderBase(path, waveFormat => new Mp3FrameDecompressor(waveFormat)),
            ".wav" => new WaveFileReader(path),
            var ext => throw new NotSupportedException(
                $"Unsupported audio file type '{ext}'. Use .mp3 or .wav, or capture live with --input mic."),
        };
        sampler = reader.ToSampleProvider();
        Channels = reader.WaveFormat.Channels;
        SampleRate = reader.WaveFormat.SampleRate;
        scratch = new float[bufferSamplesPerChannel * Channels];
    }

    /// <summary>
    /// Reads the next buffer's worth of samples (looping back to the start at end-of-file, since this is a
    /// decode-only demo source, not a live device) and de-interleaves it into a channels x samples matrix. Any
    /// decode failure propagates to the caller, which surfaces it as visible UI state (see the pump in Program.cs).
    /// </summary>
    public double[][] NextFrame()
    {
        // NAudio 3's ISampleProvider.Read is Span-based (no offset/count) -- read into the whole buffer, then top up
        // from the start of the stream if we hit EOF mid-buffer.
        var read = sampler.Read(scratch);
        if (read < scratch.Length)
        {
            reader.Position = 0;
            read += sampler.Read(scratch.AsSpan(read));
        }
        return Deinterleave(scratch, Channels, read);
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
