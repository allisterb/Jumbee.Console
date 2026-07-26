namespace ScopeTui;

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.Alsa;

/// <summary>
/// One capture endpoint the scope can open, as printed by --list-devices. <see cref="Id"/> is the string the
/// backend actually opens (a WASAPI endpoint ID on Windows, an ALSA PCM name on Linux) and is accepted verbatim by
/// --device; <see cref="Name"/> is the human-readable name that --device substring-matches against.
/// </summary>
public readonly record struct AudioDevice(int Index, string Id, string Name, bool IsDefault);

/// <summary>
/// Captures a live audio endpoint and exposes it through the same <see cref="IAudioSource"/> the file source uses,
/// so the scope can visualise a microphone / line-in / sensor feed, or whatever is currently playing. Cross-platform:
/// it codes to NAudio's <see cref="IWaveIn"/> and picks the backend at runtime — <see cref="AlsaIn"/> on Linux,
/// <see cref="WasapiCapture"/> / <see cref="WasapiLoopbackCapture"/> on Windows. Requires NAudio 3.
/// </summary>
/// <remarks>
/// Capture is push-based (the device raises <see cref="IWaveIn.DataAvailable"/> on its own thread), so this bridges
/// to the scope's pull model with a <em>rolling latest-window</em>: each callback rolls its samples into a fixed
/// buffer holding the most-recent <c>bufferSamplesPerChannel</c> samples, and <see cref="NextFrame"/> snapshots
/// that — latest-wins, matching the <see cref="ChannelBus"/>, so a live feed never backs up. A live device also
/// gives the scope real wall-clock timing for free.
/// <para/>Which endpoint gets opened is set by the <c>device</c> and <c>loopback</c> constructor arguments; see
/// <see cref="ListDevices"/> for what --list-devices enumerates and <see cref="ResolveDevice"/> for how a --device
/// index / partial name / raw ID is matched.
/// <para/>Linux needs <c>libasound2</c> installed (ALSA runtime) and a capture device the container/host exposes;
/// otherwise fall back to the file source.
/// </remarks>
public sealed class RecordingAudioSource : IAudioSource
{
    public int Channels { get; }
    public int SampleRate { get; }

    /// <summary>The endpoint actually opened (a WASAPI endpoint ID or an ALSA PCM name) — shown in the error path
    /// and useful when a fallback candidate, not the first choice, is what succeeded.</summary>
    public string DeviceId { get; }

    /// <summary>Human-readable name of the opened endpoint, for display. On Windows that is the WASAPI friendly
    /// name (<see cref="DeviceId"/> is an unreadable endpoint GUID); on Linux the PCM name is already legible, so
    /// the two match.</summary>
    public string DeviceName { get; }

    readonly IWaveIn capture;
    readonly float[] rolling; // interleaved; always holds the most-recent bufferSamplesPerChannel * Channels samples
    readonly object gate = new();
    readonly int captureChannels;                 // what the DEVICE delivers, which --mono may fold down from
    float[] foldScratch = [];                     // grows to the largest callback seen; only touched under `gate`

    /// <param name="bufferSamplesPerChannel">Size of the rolling latest-window, per channel.</param>
    /// <param name="device">Endpoint to open, already resolved by <see cref="ResolveDevice"/>; <see langword="null"/>
    /// opens the platform default.</param>
    /// <param name="loopback">Capture what is being <em>played</em> rather than what is being recorded.</param>
    /// <param name="mono">Present a single channel. On Linux this asks the backend for a 1-channel stream, so a
    /// mono device is passed through untouched instead of being upmixed to two identical channels; on Windows
    /// shared-mode WASAPI dictates the format, so the device's channels are averaged after capture instead.</param>
    public RecordingAudioSource(int bufferSamplesPerChannel, string? device = null, bool loopback = false, bool mono = false)
    {
        (capture, DeviceId, DeviceName) = CreateCapture(device, loopback, mono);
        // On Windows this is the endpoint's real shared-mode mix format, so the check below is meaningful. On Linux
        // it is not: we ASK for float32 when constructing AlsaIn and it echoes the request straight back, so ALSA /
        // PulseAudio silently convert on our behalf. That conversion is also why a mono source (e.g. WSLg's
        // s16le/1ch RDPSource) arrives here as two IDENTICAL channels — the scope then draws one trace over the
        // other and the vectorscope degenerates to a perfect diagonal. Use --loopback (or --device) to pick a
        // genuinely stereo endpoint if that matters.
        if (capture.WaveFormat.Encoding != WaveFormatEncoding.IeeeFloat)
        {
            capture.Dispose();
            throw new NotSupportedException($"Live capture expects 32-bit IEEE float; '{DeviceId}' reported {capture.WaveFormat}.");
        }
        captureChannels = capture.WaveFormat.Channels;
        // Linux already opened a 1-channel stream if it could, in which case there is nothing left to fold.
        Channels = mono ? 1 : captureChannels;
        SampleRate = capture.WaveFormat.SampleRate;
        rolling = new float[bufferSamplesPerChannel * Channels];
        capture.DataAvailable += OnData;
        capture.StartRecording();
    }

    #region Device discovery and selection
    /// <summary>
    /// Lists the endpoints --device can select: recording endpoints normally, or the endpoints carrying playback
    /// when <paramref name="loopback"/> is set (WASAPI render endpoints on Windows; on Linux a single synthetic
    /// entry, since ALSA's device-name hints enumerate PCMs and do not surface PulseAudio's per-source monitors).
    /// </summary>
    public static IReadOnlyList<AudioDevice> ListDevices(bool loopback)
    {
        if (OperatingSystem.IsLinux()) return ListAlsaDevices(loopback);
        if (OperatingSystem.IsWindows()) return ListWasapiDevices(loopback);
        throw new PlatformNotSupportedException("Live audio input needs Windows (WASAPI) or Linux (ALSA).");
    }

    /// <summary>
    /// Turns a --device value into something a backend can open, so the exact name is only ever the last resort.
    /// In order: a <see cref="AudioDevice.Index"/> from <see cref="ListDevices"/>; an exact (case-insensitive)
    /// <see cref="AudioDevice.Id"/>; a unique case-insensitive substring of <see cref="AudioDevice.Name"/>. Failing
    /// all three, a Linux value passes through verbatim — ALSA PCM names are open-ended and this is what reaches a
    /// PulseAudio source directly (e.g. <c>pulse:DEVICE=RDPSink.monitor</c>) — while a Windows value throws.
    /// </summary>
    /// <returns>The backend device string, or <see langword="null"/> for "use the platform default".</returns>
    /// <exception cref="ArgumentException">The value matched nothing, or matched more than one device by name.</exception>
    public static string? ResolveDevice(string? spec, bool loopback)
    {
        if (spec is not { Length: > 0 }) return null;

        var devices = ListDevices(loopback);
        if (int.TryParse(spec, out var index))
        {
            return index >= 0 && index < devices.Count
                ? devices[index].Id
                : throw new ArgumentException($"No device with index {index}. Run with --list-devices to see the {devices.Count} available.");
        }

        if (devices.FirstOrDefault(d => string.Equals(d.Id, spec, StringComparison.OrdinalIgnoreCase)) is { Id.Length: > 0 } exact)
            return exact.Id;

        var matches = devices.Where(d => d.Name.Contains(spec, StringComparison.OrdinalIgnoreCase)).ToList();
        if (matches.Count == 1) return matches[0].Id;
        if (matches.Count > 1)
        {
            var names = string.Join(", ", matches.Select(m => $"[{m.Index}] {m.Name}"));
            throw new ArgumentException($"'{spec}' matches {matches.Count} devices: {names}. Use the index, or a longer substring.");
        }

        // No match. On Linux any string is a legal PCM name, so let it through and let snd_pcm_open have the final
        // say -- that is the escape hatch for names the hints don't enumerate.
        return OperatingSystem.IsLinux()
            ? spec
            : throw new ArgumentException($"No capture device matches '{spec}'. Run with --list-devices to see what is available.");
    }

    // The platform helpers below are each reached only through an OperatingSystem.Is* guard; the attribute is what
    // lets the platform-compatibility analyzer see that (it tracks guards per method, not across calls).
    [SupportedOSPlatform("windows")]
    static IReadOnlyList<AudioDevice> ListWasapiDevices(bool loopback)
    {
        var enumerator = new MMDeviceEnumerator();
        // Loopback captures a RENDER endpoint, so that is the list to choose from when --loopback is set.
        var flow = loopback ? DataFlow.Render : DataFlow.Capture;
        var defaultId = string.Empty;
        try { defaultId = enumerator.GetDefaultAudioEndpoint(flow, Role.Console).ID; }
        catch (COMException) { /* no default endpoint of this flow -- every entry just lists as non-default */ }

        var devices = new List<AudioDevice>();
        foreach (var device in enumerator.EnumerateAudioEndPoints(flow, DeviceState.Active))
            devices.Add(new AudioDevice(devices.Count, device.ID, device.FriendlyName, device.ID == defaultId));
        return devices;
    }

    [SupportedOSPlatform("linux")]
    static IReadOnlyList<AudioDevice> ListAlsaDevices(bool loopback)
    {
        // ALSA enumerates PCMs via snd_device_name_hint, which reports the pulse PLUGIN ("pulse") but not the
        // individual PulseAudio sources behind it -- so there is nothing to enumerate for loopback. Report the
        // server-resolved default monitor as the one entry; a specific monitor is reachable through --device with
        // the raw pass-through form (pulse:DEVICE=<source>.monitor, listed by `pactl list sources short`).
        if (loopback) return [new AudioDevice(0, DefaultMonitorPcm, "Default playback monitor (PulseAudio/PipeWire)", true)];

        var devices = new List<AudioDevice>();
        foreach (var device in AlsaDeviceEnumerator.GetCaptureDevices())
        {
            // Descriptions are frequently multi-line ("Name\nSubdevice detail"); the first line is the useful part.
            var description = device.Description?.Split('\n')[0].Trim();
            var name = description is { Length: > 0 } ? description : device.Name;
            devices.Add(new AudioDevice(devices.Count, device.Name, name, device.Name == "default"));
        }
        return devices;
    }
    #endregion

    #region Capture backends
    static (IWaveIn Capture, string Id, string Name) CreateCapture(string? device, bool loopback, bool mono)
    {
        // Only ALSA lets us state the format we want; WASAPI shared mode dictates it, so --mono is folded in the
        // capture callback there instead (see FoldToMono).
        if (OperatingSystem.IsLinux()) return CreateAlsaCapture(device, loopback, mono);
        if (OperatingSystem.IsWindows()) return CreateWasapiCapture(device, loopback);
        throw new PlatformNotSupportedException("Live audio input needs Windows (WASAPI) or Linux (ALSA).");
    }

    [SupportedOSPlatform("linux")]
    static (IWaveIn, string, string) CreateAlsaCapture(string? device, bool loopback, bool mono)
    {
        // Candidates, tried in order. An explicit --device is taken at face value (one candidate, so a typo reports
        // the real ALSA error rather than silently landing somewhere else).
        string[] candidates = device is { Length: > 0 } chosen ? [chosen]
            : loopback ? [.. LoopbackCandidates()]
            : DefaultCaptureCandidates();

        foreach (var name in candidates)
        {
            // An ALSA PCM name is already legible ("pulse", "hw:CARD=PCH,DEV=0"), so it doubles as the display name.
            try { return (new AlsaIn(name) { WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(44100, mono ? 1 : 2) }, name, name); }
            catch (AlsaException) { /* not available -- try the next candidate */ }
        }

        throw new NotSupportedException(loopback
            ? $"No playback monitor could be opened (tried {string.Join(", ", candidates)}). Loopback needs PulseAudio or "
              + "PipeWire; find a monitor with `pactl list sources short` and pass it as --device pulse:DEVICE=<name>."
            : $"No ALSA capture device could be opened (tried {string.Join(", ", candidates)}). Install libasound2 "
              + "(+ libasound2-plugins for the PulseAudio route) and ensure a capture device is available.");
    }

    /// <summary>
    /// The PCMs to try when no --device was given: the system default (a native box's hardware capture) and the
    /// PulseAudio bridge from libasound2-plugins, which is what WSL, desktops and containers actually route through.
    /// </summary>
    /// <remarks>
    /// Ordered by a probe rather than fixed, because opening "default" on a machine with no sound card fails only
    /// AFTER libasound has written ~8 lines of config-evaluation errors to stderr — which then scroll underneath the
    /// TUI. <c>snd_device_name_hint</c> is quiet and lists "default" only when a card backs it, so when it doesn't we
    /// put "pulse" first and never touch "default" on the happy path. "default" stays in the list, just behind the
    /// candidate that is going to succeed, so a box whose hints are incomplete still opens exactly as it used to.
    /// </remarks>
    [SupportedOSPlatform("linux")]
    static string[] DefaultCaptureCandidates() => HasDefaultPcm() ? ["default", "pulse"] : ["pulse", "default"];

    [SupportedOSPlatform("linux")]
    static bool HasDefaultPcm()
    {
        try { return AlsaDeviceEnumerator.GetCaptureDevices().Any(d => d.Name == "default"); }
        catch (Exception e) when (e is AlsaException or DllNotFoundException)
        {
            return true; // can't tell -- keep the original order and let the open attempt report the real problem
        }
    }

    const string DefaultMonitorPcm = "pulse:DEVICE=@DEFAULT_MONITOR@";

    // @DEFAULT_MONITOR@ is resolved server-side by PulseAudio, so it needs no client tooling and is the first try.
    // If the server does not honour it, fall back to asking pactl for the default sink and monitoring that by name.
    static IEnumerable<string> LoopbackCandidates()
    {
        yield return DefaultMonitorPcm;
        if (DefaultSinkFromPactl() is { Length: > 0 } sink) yield return $"pulse:DEVICE={sink}.monitor";
    }

    static string? DefaultSinkFromPactl()
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo("pactl", "get-default-sink")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            });
            if (process is null) return null;
            var sink = process.StandardOutput.ReadToEnd().Trim();
            return process.WaitForExit(2000) && process.ExitCode == 0 && sink.Length > 0 ? sink : null;
        }
        catch (Exception e) when (e is System.ComponentModel.Win32Exception or InvalidOperationException)
        {
            return null; // pactl not installed / not on PATH -- the caller reports the combined failure
        }
    }

#pragma warning disable CS0618 // WasapiCapture/WasapiLoopbackCapture are obsolete in NAudio 3 (superseded by
    [SupportedOSPlatform("windows")]
    static (IWaveIn, string, string) CreateWasapiCapture(string? deviceId, bool loopback)
    {                                             // WasapiRecorder), but they share the IWaveIn contract with AlsaIn
        var enumerator = new MMDeviceEnumerator(); // -- one cross-platform path
        var device = deviceId is { Length: > 0 } id
            ? enumerator.GetDevice(id)
            : enumerator.GetDefaultAudioEndpoint(loopback ? DataFlow.Render : DataFlow.Capture, Role.Console);
        // Loopback taps a render endpoint's mix. Note WASAPI delivers NOTHING while that endpoint is silent, so an
        // idle machine leaves the scope showing its last window rather than a flat line.
        IWaveIn capture = loopback ? new WasapiLoopbackCapture(device) : new WasapiCapture(device);
        return (capture, device.ID, device.FriendlyName);
    }
#pragma warning restore CS0618
    #endregion

    // Capture thread. The callback bytes ARE float samples (IEEE float, verified in the ctor), so reinterpret and
    // roll them into `rolling`, keeping only the newest window. The WaveInEventArgs buffer is reused after this
    // returns, so we copy out of it here.
    void OnData(object? sender, WaveInEventArgs e)
    {
        var samples = MemoryMarshal.Cast<byte, float>(e.Buffer.AsSpan(0, e.BytesRecorded));
        lock (gate) Roll(Channels == captureChannels ? samples : FoldToMono(samples));
    }

    // --mono against a device that only offers multi-channel (i.e. Windows, where shared-mode WASAPI picks the
    // format): average each frame's channels into one sample. An average, not a channel pick, so nothing audible is
    // dropped -- with the usual caveat that out-of-phase stereo content partially cancels when summed.
    ReadOnlySpan<float> FoldToMono(ReadOnlySpan<float> interleaved)
    {
        var frames = interleaved.Length / captureChannels;
        if (foldScratch.Length < frames) foldScratch = new float[frames];
        for (var f = 0; f < frames; f++)
        {
            var sum = 0f;
            for (var c = 0; c < captureChannels; c++) sum += interleaved[(f * captureChannels) + c];
            foldScratch[f] = sum / captureChannels;
        }
        return foldScratch.AsSpan(0, frames);
    }

    // Caller holds `gate`.
    void Roll(ReadOnlySpan<float> samples)
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

    public double[][] NextFrame()
    {
        float[] snapshot;
        lock (gate) snapshot = (float[])rolling.Clone();
        return FileAudioSource.Deinterleave(snapshot, Channels);
    }

    /// <inheritdoc/>
    /// <remarks>No-op. This source is a rolling latest-window, so how much consecutive frames share is decided
    /// entirely by how often <see cref="NextFrame"/> is called — there is nothing to configure here.</remarks>
    public void SetOverlap(double overlap) { }

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
