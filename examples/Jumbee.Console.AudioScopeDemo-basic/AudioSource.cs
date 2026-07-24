namespace ScopeTui;

using NAudio.Wave;

/// <summary>
/// Decodes an MP3 to interleaved float samples via NAudio and de-interleaves into a channel matrix, mirroring
/// scope-tui's stream_to_matrix (src/input/mod.rs). No audio is played back — this only reads/decodes samples.
/// </summary>
public sealed class AudioSource : IDisposable
{
    public int Channels { get; }
    public int SampleRate { get; }

    readonly AudioFileReader reader;
    readonly ISampleProvider sampler;
    readonly float[] scratch;

    public AudioSource(string path, int bufferSamplesPerChannel)
    {
        reader = new AudioFileReader(path);
        sampler = reader.ToSampleProvider();
        Channels = reader.WaveFormat.Channels;
        SampleRate = reader.WaveFormat.SampleRate;
        scratch = new float[bufferSamplesPerChannel * Channels];
    }

    /// <summary>
    /// Reads the next buffer's worth of interleaved samples (looping back to the start at end-of-file, since this
    /// is a decode-only demo source, not a live device) and de-interleaves it into a channels x samples matrix.
    /// Any decode failure (corrupt frame, I/O error) propagates to the caller, which surfaces it as visible UI
    /// state rather than silently dropping frames — see the background loop in Program.cs.
    /// </summary>
    public double[][] NextFrame()
    {
        var read = sampler.Read(scratch, 0, scratch.Length);
        if (read < scratch.Length)
        {
            reader.Position = 0;
            var more = sampler.Read(scratch, read, scratch.Length - read);
            read += more;
        }

        return Deinterleave(scratch, Channels, read);
    }

    /// <summary>
    /// De-interleaves a flat sample buffer (channel-major-interleaved, e.g. LRLRLR...) into a channels x
    /// samples-per-channel matrix. A pure static function (no NAudio/file dependency) so the de-interleave logic
    /// can be unit-tested directly with synthetic data.
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
