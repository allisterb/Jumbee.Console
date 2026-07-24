namespace ScopeTui;

using System.Runtime.InteropServices;

using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.Alsa;

/// <summary>
/// Captures a live audio input device and exposes it through the same <see cref="IAudioSource"/> the file source
/// uses, so the scope can visualise a microphone / line-in / sensor feed. Cross-platform: it codes to NAudio's
/// <see cref="IWaveIn"/> and picks the backend at runtime — <see cref="AlsaIn"/> on Linux, <see cref="WasapiCapture"/>
/// on Windows (both implement <see cref="IWaveIn"/>). Requires NAudio 3.
/// </summary>
/// <remarks>
/// Capture is push-based (the device raises <see cref="IWaveIn.DataAvailable"/> on its own thread), so this bridges
/// to the scope's pull model with a <em>rolling latest-window</em>: each callback rolls its samples into a fixed
/// buffer holding the most-recent <c>bufferSamplesPerChannel</c> samples, and <see cref="NextFrame"/> snapshots
/// that — latest-wins, matching the <see cref="ChannelBus"/>, so a live feed never backs up. A live device also
/// gives the scope real wall-clock timing for free.
/// <para/>Linux needs <c>libasound2</c> installed (ALSA runtime) and a capture device the container/host exposes;
/// otherwise fall back to the file source.
/// </remarks>
public sealed class RecordingAudioSource : IAudioSource
{
    public int Channels { get; }
    public int SampleRate { get; }

    readonly IWaveIn capture;
    readonly float[] rolling; // interleaved; always holds the most-recent bufferSamplesPerChannel * Channels samples
    readonly object gate = new();

    public RecordingAudioSource(int bufferSamplesPerChannel)
    {
        capture = CreateCapture();
        // Both backends deliver 32-bit IEEE float in shared mode; bail clearly rather than mis-read a PCM device.
        if (capture.WaveFormat.Encoding != WaveFormatEncoding.IeeeFloat)
        {
            capture.Dispose();
            throw new NotSupportedException($"Live capture expects 32-bit IEEE float; the device reported {capture.WaveFormat}.");
        }
        Channels = capture.WaveFormat.Channels;
        SampleRate = capture.WaveFormat.SampleRate;
        rolling = new float[bufferSamplesPerChannel * Channels];
        capture.DataAvailable += OnData;
        capture.StartRecording();
    }

    static IWaveIn CreateCapture()
    {
        if (OperatingSystem.IsLinux())
        {
            // Try the system default PCM first (a native box's hardware/default capture), then the PulseAudio bridge
            // -- on WSL, desktops, and containers "default" resolves to a missing hw card, but "pulse" (provided by
            // libasound2-plugins) routes to PulseAudio/PipeWire. Verified on WSL2: "pulse" captures the desktop mixer.
            foreach (var device in (string[])["default", "pulse"])
            {
                try { return new AlsaIn(device) { WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2) }; }
                catch (AlsaException) { /* device name not available -- try the next */ }
            }
            throw new NotSupportedException(
                "No ALSA capture device ('default' or 'pulse') could be opened. Install libasound2 (+ libasound2-plugins "
                + "for the PulseAudio route) and ensure a capture device is available.");
        }
        if (OperatingSystem.IsWindows())
#pragma warning disable CS0618 // WasapiCapture is obsolete in NAudio 3 (superseded by WasapiRecorder), but it shares
            return new WasapiCapture();                // the IWaveIn contract with AlsaIn -- one cross-platform path
#pragma warning restore CS0618
        throw new PlatformNotSupportedException("Live audio input needs Windows (WASAPI) or Linux (ALSA).");
    }

    // Capture thread. The callback bytes ARE float samples (IEEE float, verified in the ctor), so reinterpret and
    // roll them into `rolling`, keeping only the newest window. The WaveInEventArgs buffer is reused after this
    // returns, so we copy out of it here.
    void OnData(object? sender, WaveInEventArgs e)
    {
        var samples = MemoryMarshal.Cast<byte, float>(e.Buffer.AsSpan(0, e.BytesRecorded));
        lock (gate)
        {
            var n = samples.Length;
            if (n >= rolling.Length)
            {
                samples[^rolling.Length..].CopyTo(rolling);            // callback bigger than a window: keep its tail
            }
            else
            {
                Array.Copy(rolling, n, rolling, 0, rolling.Length - n); // shift older samples left...
                samples.CopyTo(rolling.AsSpan(rolling.Length - n));     // ...and append the newest at the tail
            }
        }
    }

    public double[][] NextFrame()
    {
        float[] snapshot;
        lock (gate) snapshot = (float[])rolling.Clone();
        return FileAudioSource.Deinterleave(snapshot, Channels);
    }

    public void Dispose()
    {
        capture.DataAvailable -= OnData;
        // StopRecording() can BLOCK on some ALSA backends (observed on WSL2, where the capture thread sits in a
        // blocking read), which would hang the app's quit path. Tear the device down on a background thread so Dispose
        // returns immediately -- the process is exiting, so the OS reclaims the device even if the stop never returns.
        var device = capture;
        System.Threading.Tasks.Task.Run(() =>
        {
            try { device.StopRecording(); } catch { /* already stopping / backend quirk */ }
            try { device.Dispose(); } catch { }
        });
    }
}
