namespace ScopeTui;

using System;
using System.Runtime.Versioning;

using NAudio.CoreAudioApi;
using NAudio.Wave;

/// <summary>
/// Plays an audio file to the default output device and scopes the samples on their way there.
/// </summary>
/// <remarks>
/// <para>
/// <b>The scope taps the playback chain rather than decoding alongside it.</b> The file is decoded once, and a
/// pass-through provider copies each block into the rolling window as the sound card pulls it:
/// </para>
/// <code>file -> decoder -> loop -> [tap] -> output device -> speakers</code>
/// <para>
/// Two consequences, and they are the reason for doing it this way. What you see is the audio being handed to the
/// DAC, the same samples at the same moment, so picture and sound agree by construction rather than by tuning. And
/// the sound card becomes the clock: this source is PUSHED at exactly real time, so it needs none of the wall-clock
/// pacing <see cref="FileAudioSource"/> has to do when nothing is playing, and it behaves like a capture device in
/// every other respect — same <see cref="RollingWindow"/>, same latest-wins snapshot, same null when nothing is new.
/// </para>
/// <para>
/// The tap sees a block when the device REQUESTS it, which is one output buffer before it is audible, so the scope
/// leads the speakers by <see cref="OutputLatencyMs"/>. That is reported rather than hidden: it is small (the buffer
/// is sized to two device periods) and, unlike a loopback tap, it is a number we actually know.
/// </para>
/// </remarks>
public sealed class PlayingFileAudioSource : IAudioSource
{
    #region Constructors
    /// <param name="path">File to play and scope (.mp3 or .wav).</param>
    /// <param name="bufferSamplesPerChannel">Size of the rolling latest-window, per channel.</param>
    public PlayingFileAudioSource(string path, int bufferSamplesPerChannel)
    {
        _reader = FileAudioSource.OpenReader(path);
        Channels = _reader.WaveFormat.Channels;
        SampleRate = _reader.WaveFormat.SampleRate;
        _window = new RollingWindow(bufferSamplesPerChannel * Channels);

        var chain = new TapSampleProvider(new LoopingSampleProvider(_reader), _window.Write);
        (_output, OutputName) = CreateOutput();
        _output.Init(chain);
        _output.Play();
        // Read the granted latency only after Play: the client is not initialised until the stream is running.
        OutputLatencyMs = _output is WasapiPlayer player ? player.LatencyMilliseconds : null;
    }
    #endregion

    #region Properties
    /// <inheritdoc/>
    public int Channels { get; }

    /// <inheritdoc/>
    public int SampleRate { get; }

    /// <summary>The output endpoint the file is playing through.</summary>
    public string OutputName { get; }

    /// <summary>
    /// The output buffer the device granted, in milliseconds, or <see langword="null"/> where the backend does not
    /// report one (ALSA). This is how far the picture LEADS the sound, since the tap runs one buffer early.
    /// </summary>
    public int? OutputLatencyMs { get; }
    #endregion

    #region Methods
    /// <inheritdoc/>
    public double[][]? NextFrame() =>
        _window.TrySnapshot() is { } snapshot ? FileAudioSource.Deinterleave(snapshot, Channels) : null;

    /// <inheritdoc/>
    /// <remarks>A no-op: this source is pushed by the sound card, so how much consecutive frames share is decided by
    /// how often it is read, exactly as for a capture device.</remarks>
    public void SetOverlap(double overlap) { }

    /// <inheritdoc/>
    public void Dispose()
    {
        try { _output.Stop(); } catch { /* already stopping, or the endpoint vanished */ }
        _output.Dispose();
        _reader.Dispose();
    }

    private static (IWavePlayer Output, string Name) CreateOutput()
    {
        if (OperatingSystem.IsWindows()) return CreateWasapiOutput();
        if (OperatingSystem.IsLinux()) return (new NAudio.Wave.Alsa.AlsaOut(), "default");
        throw new PlatformNotSupportedException("Playback needs Windows (WASAPI) or Linux (ALSA).");
    }

    [SupportedOSPlatform("windows")]
    private static (IWavePlayer, string) CreateWasapiOutput()
    {
        // Role.Multimedia rather than Console: this is music, and on a box with separate comms/media defaults that
        // is the endpoint the user means.
        var device = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        // Two device periods, for the same reason the capture buffer is: enough to bridge one pull to the next
        // without paying for depth that only turns into lead time.
        using var client = device.CreateAudioClient();   // a property access would create (and leak) one per read
        var periodMs = client.DefaultDevicePeriod / 10_000.0;
        var player = new WasapiPlayerBuilder()
            .WithDevice(device)
            .WithSharedMode()
            .WithEventSync()
            .WithLatency(Math.Max(2, (int)Math.Ceiling(periodMs * 2)))
            .Build();
        return (player, device.FriendlyName);
    }
    #endregion

    #region Fields
    private readonly WaveStream _reader;
    private readonly IWavePlayer _output;
    private readonly RollingWindow _window;
    #endregion

    #region Types
    /// <summary>Repeats the file instead of stopping at the end, matching <see cref="FileAudioSource"/>.</summary>
    private sealed class LoopingSampleProvider(WaveStream stream) : ISampleProvider
    {
        public WaveFormat WaveFormat => _inner.WaveFormat;

        public int Read(Span<float> buffer)
        {
            var read = 0;
            while (read < buffer.Length)
            {
                var n = _inner.Read(buffer[read..]);
                if (n > 0) { read += n; continue; }
                if (stream.Position == 0) break;   // empty or unreadable: stop rather than spin forever
                stream.Position = 0;
            }

            return read;
        }

        private readonly ISampleProvider _inner = stream.ToSampleProvider();
    }

    /// <summary>Receives a block of audio on its way to the device. A span, so the tap allocates nothing.</summary>
    private delegate void SampleTap(ReadOnlySpan<float> samples);

    /// <summary>Copies everything on its way to the speakers into the scope's window, and passes it through.</summary>
    /// <remarks>This runs on the audio render thread, so it does exactly one memcpy and nothing else — no
    /// allocation, no de-interleave, no rendering. Anything slow here is a dropout you can hear.</remarks>
    private sealed class TapSampleProvider(ISampleProvider source, SampleTap tap) : ISampleProvider
    {
        public WaveFormat WaveFormat => source.WaveFormat;

        public int Read(Span<float> buffer)
        {
            var read = source.Read(buffer);
            if (read > 0) tap(buffer[..read]);
            return read;
        }
    }
    #endregion
}
