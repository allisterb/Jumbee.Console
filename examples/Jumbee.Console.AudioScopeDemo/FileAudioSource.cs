namespace ScopeTui;

using System.Diagnostics;

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
    readonly Stopwatch clock = new();
    TimeSpan lastCall;            // when NextFrame last ran, so the next one can advance by the audio that went by
    int lastAdvance;              // interleaved samples of NEW audio in the frame just handed out
    bool primed;

    // Ceiling on how much audio one call will decode-and-discard to catch up after a stall. A pause longer than this
    // (a debugger break, a resize storm) resynchronises to the present instead of decoding the whole backlog in one
    // tick, which would stall the pump thread for as long as the pause itself.
    const double MaxCatchUpSeconds = 0.5;

    /// <param name="overlap">Fraction of each frame re-used by the next one, 0 (none) to just under 1. Overlapping
    /// makes consecutive frames share audio so the display can refresh faster than one whole buffer at a time —
    /// what a spectrum analyser does to keep a large FFT watchable.</param>
    public FileAudioSource(string path, int bufferSamplesPerChannel, double overlap = 0.0)
    {
        reader = OpenReader(path);
        sampler = reader.ToSampleProvider();
        Channels = reader.WaveFormat.Channels;
        SampleRate = reader.WaveFormat.SampleRate;
        window = new float[bufferSamplesPerChannel * Channels];
        lastAdvance = window.Length;
        // Retain-and-shift rather than seeking the reader back: a shift is exact and codec-agnostic, where rewinding
        // an MP3 snaps to a frame boundary. How much of each frame is new is decided per call by the clock (see
        // NextAdvance), so `overlap` is now only a hint about how often the caller intends to read -- the pump sizes
        // its interval from the same number, and the overlap that actually results is AchievedOverlap.
        _ = overlap;
    }

    /// <summary>Opens a decoder for a supported audio file. Shared with <see cref="PlayingFileAudioSource"/>, which
    /// reads the same formats but hands the samples to an output device instead of pulling them itself.</summary>
    internal static WaveStream OpenReader(string path) => Path.GetExtension(path).ToLowerInvariant() switch
    {
        ".mp3" => new Mp3FileReaderBase(path, waveFormat => new Mp3FrameDecompressor(waveFormat)),
        ".wav" => new WaveFileReader(path),
        var ext => throw new NotSupportedException(
            $"Unsupported audio file type '{ext}'. Use .mp3 or .wav, or capture a device with the 'live' input."),
    };

    /// <summary>
    /// When <see langword="true"/> (the default) each frame advances by the audio that has actually elapsed since
    /// the last call, so the track plays at real time. Set <see langword="false"/> to read a whole fresh window per
    /// call as fast as it is called, for scanning a file rather than watching it.
    /// </summary>
    /// <remarks>
    /// Mirrors scope-tui's <c>limit_rate</c>. The scan in <c>--verify</c> needs it off: it pulls frames in a tight
    /// loop looking for the first signal, and a paced source would hand it the same near-identical window every time
    /// and report the track as silent.
    /// </remarks>
    public bool LimitRate { get; set; } = true;

    /// <summary>
    /// The fraction of the frame just handed out that was re-used from the previous one, 0 (all new) to just under 1.
    /// </summary>
    /// <remarks>
    /// Under <see cref="LimitRate"/> the overlap is an OUTCOME, not a setting: it is whatever the gap between calls
    /// left over. Worth reporting rather than echoing what was requested, because the two differ — a feed cannot ask
    /// the OS for an interval shorter than its timer tick, so a request below that silently becomes less overlap
    /// than it looks.
    /// </remarks>
    public double AchievedOverlap => 1.0 - ((double)lastAdvance / window.Length);

    /// <inheritdoc/>
    /// <remarks>
    /// A no-op, as it is for a live device. The frame advance now comes from the clock rather than a setting, so
    /// overlap is governed by how often this source is read — ask for it by ticking faster, not by telling the
    /// source. See <see cref="AchievedOverlap"/>.
    /// </remarks>
    public void SetOverlap(double overlap) { }

    /// <summary>
    /// Reads the next buffer's worth of samples (looping back to the start at end-of-file, since this is a
    /// decode-only demo source, not a live device) and de-interleaves it into a channels x samples matrix. Any
    /// decode failure propagates to the caller, which surfaces it as visible UI state (see the pump in Program.cs).
    /// </summary>
    public double[][] NextFrame()
    {
        var advance = NextAdvance();
        lastAdvance = Math.Min(advance, window.Length);

        if (!primed || advance >= window.Length)
        {
            // First frame, or the caller was away for longer than a whole window: the entire window is new audio.
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

    /// <summary>
    /// Interleaved samples of new audio this frame should carry: the audio that has actually gone by since the last
    /// call.
    /// </summary>
    /// <remarks>
    /// <b>Why the clock and not a fixed step.</b> Advancing by a constant per call only plays at real time if the
    /// caller is punctual, and a timer-driven caller is not: a feed waits with <c>Task.Delay</c>, which Windows
    /// rounds up to its ~15.6 ms system tick, so an 11 ms request lands at ~15 ms. A fixed step then carries 11 ms
    /// of audio per 15 ms of wall clock and the track plays at ~71% speed — measurably slow, and silently so, since
    /// every frame still looks perfectly well formed. Reading the clock instead makes a late call carry MORE audio,
    /// which is the right way to be wrong: coarser steps at the true speed, rather than smooth steps at the wrong
    /// one. A live device gets this for free because its window advances on the device's clock however often it is
    /// sampled; this is the file source catching up with that.
    /// </remarks>
    int NextAdvance()
    {
        if (!LimitRate) return window.Length;   // scanning, not watching: a whole fresh window per call

        if (!clock.IsRunning)
        {
            clock.Start();
            lastCall = clock.Elapsed;
            return window.Length;               // first frame is all new audio
        }

        var now = clock.Elapsed;
        var elapsed = now - lastCall;
        lastCall = now;

        // At least one frame, so a caller that spins faster than a single sample still makes progress rather than
        // handing out the same window forever.
        var frames = (long)Math.Round(elapsed.TotalSeconds * SampleRate);
        var interleaved = Math.Max(Channels, frames * Channels);
        if (interleaved <= window.Length) return (int)interleaved;

        // Away for longer than a window: decode and discard the audio in between so playback stays anchored to the
        // clock rather than drifting by every hiccup, then take a whole fresh window. Capped (see MaxCatchUpSeconds).
        var skip = Math.Min(interleaved - window.Length, (long)(MaxCatchUpSeconds * SampleRate) * Channels);
        while (skip > 0)
        {
            var chunk = (int)Math.Min(skip, window.Length);
            ReadLooping(window.AsSpan(0, chunk));
            skip -= chunk;
        }

        return window.Length;
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
