namespace ScopeTui;

using System;
using System.Runtime.Versioning;

using NAudio.CoreAudioApi;
using NAudio.Wave;

/// <summary>
/// Adapts NAudio 3's <see cref="WasapiRecorder"/> to the <see cref="IWaveIn"/> shape the rest of the capture path is
/// built on, so a Windows capture can size its buffer instead of taking whatever the default is.
/// </summary>
/// <remarks>
/// <para>
/// The legacy <c>WasapiLoopbackCapture</c> offers only <c>()</c> and <c>(MMDevice)</c> - no buffer length, no event
/// sync - so a loopback stream ran at NAudio's default buffer with polling sync. The scope then sampled its rolling
/// window several times per device callback, and the waveform stepped at the callback rate rather than sliding.
/// <see cref="WasapiRecorderBuilder"/> is the supported replacement and does expose the missing knobs, but
/// <see cref="WasapiRecorder"/> deliberately does not implement <see cref="IWaveIn"/> - its callback hands out a
/// <see cref="ReadOnlySpan{T}"/> rather than an array. Hence this adapter: one copy into a reused buffer, which is
/// what the legacy path did internally anyway.
/// </para>
/// <para>
/// <b>Windows only.</b> The ALSA path has no equivalent: NAudio 3's <c>AlsaIn</c> exposes a settable
/// <c>WaveFormat</c> and read-only latency, and nothing to size a buffer with.
/// </para>
/// </remarks>
[SupportedOSPlatform("windows")]
internal sealed class WasapiRecorderWaveIn : IWaveIn
{
    #region Constructors
    /// <param name="device">Endpoint to open. A render endpoint when <paramref name="loopback"/> is set.</param>
    /// <param name="loopback">Capture what is being played rather than what is being recorded.</param>
    /// <param name="bufferMilliseconds">Requested capture buffer. The shared-mode engine has a floor of its own and
    /// may hand back something longer; <see cref="LatencyMilliseconds"/> reports what was actually granted.</param>
    public WasapiRecorderWaveIn(MMDevice device, bool loopback, int bufferMilliseconds)
    {
        var builder = new WasapiRecorderBuilder()
            .WithDevice(device)
            .WithSharedMode()          // exclusive mode would lock the endpoint against everything else on the box
            .WithEventSync()           // driver-signalled rather than polled, which is what makes a short buffer pay
            .WithBufferLength(bufferMilliseconds);
        if (loopback) builder = builder.WithLoopbackCapture();

        _recorder = builder.Build();
        // Report the format the way the legacy WasapiCapture did. A shared-mode endpoint's mix format is normally
        // WAVE_FORMAT_EXTENSIBLE, whose Encoding is `Extensible` rather than the `IeeeFloat` its SubFormat actually
        // names; WasapiRecorder hands that back raw, where WasapiCapture collapsed it first. Callers test Encoding to
        // decide whether the bytes are floats, so passing the raw form through makes a perfectly good float stream
        // look unsupported. The bytes on the wire are identical either way - this only renames them.
        _waveFormat = _recorder.WaveFormat is WaveFormatExtensible extensible
            ? extensible.ToStandardWaveFormat()
            : _recorder.WaveFormat;
        _recorder.DataAvailable += OnCapture;
        _recorder.RecordingStopped += (_, e) => RecordingStopped?.Invoke(this, e);
    }
    #endregion

    #region Properties
    /// <summary>The format the endpoint actually opened with.</summary>
    /// <remarks>Settable only to satisfy <see cref="IWaveIn"/>; a shared-mode capture takes the engine's mix format
    /// and cannot be told otherwise, so a set is a programming error rather than a request.</remarks>
    public WaveFormat WaveFormat
    {
        get => _waveFormat;
        set => throw new NotSupportedException("A shared-mode WASAPI capture uses the engine's mix format.");
    }

    /// <summary>The buffer the engine actually granted, which is the number worth reporting - a request below the
    /// engine's own period is rounded up, silently.</summary>
    public int LatencyMilliseconds => _recorder.LatencyMilliseconds;

    /// <summary>Mean measured latency over the run, as opposed to the configured buffer above.</summary>
    public TimeSpan AverageLatency => _recorder.AverageLatency;

    /// <summary>Whether the low-latency path engaged, and if not, why.</summary>
    public bool LowLatencyActive => _recorder.LowLatencyActive;

    /// <inheritdoc cref="LowLatencyActive"/>
    public string? LowLatencyUnavailableReason => _recorder.LowLatencyUnavailableReason;
    #endregion

    #region Events
    /// <inheritdoc/>
    public event EventHandler<WaveInEventArgs>? DataAvailable;

    /// <inheritdoc/>
    public event EventHandler<StoppedEventArgs>? RecordingStopped;
    #endregion

    #region Methods
    /// <inheritdoc/>
    public void StartRecording() => _recorder.StartRecording();

    /// <inheritdoc/>
    public void StopRecording() => _recorder.StopRecording();

    /// <inheritdoc/>
    public void Dispose()
    {
        _recorder.DataAvailable -= OnCapture;
        _recorder.Dispose();
    }

    // The span is the engine's own buffer and is valid only for this call, so it has to be copied out before the
    // IWaveIn-shaped event can carry it. `_scratch` is reused across callbacks and handed over by reference exactly
    // as the legacy path did -- a subscriber must consume it synchronously, which the rolling-window writer does.
    private void OnCapture(ReadOnlySpan<byte> buffer, AudioClientBufferFlags flags, long devicePosition, long qpcPosition)
    {
        var handler = DataAvailable;
        if (handler is null || buffer.Length == 0) return;

        if (_scratch.Length < buffer.Length) _scratch = new byte[buffer.Length];
        // WASAPI may flag a run of silence instead of writing zeros, in which case the buffer's contents are
        // undefined -- treat it as the silence it claims to be rather than rendering whatever was left there.
        if ((flags & AudioClientBufferFlags.Silent) != 0) Array.Clear(_scratch, 0, buffer.Length);
        else buffer.CopyTo(_scratch);

        handler(this, new WaveInEventArgs(_scratch, buffer.Length));
    }
    #endregion

    #region Fields
    private readonly WasapiRecorder _recorder;
    private readonly WaveFormat _waveFormat;
    private byte[] _scratch = [];
    #endregion
}
