namespace ScopeTui;

using Jumbee.Console;

/// <summary>Immutable snapshot of the spectroscope's hotkey-driven knobs, taken via <see cref="Spectroscope.Snapshot"/>
/// before crossing to a background thread. Mirrors the mutable fields on Rust's <c>Spectroscope</c> struct
/// (display/spectroscope.rs) EXCEPT <c>phase_diff</c>, which this port omits (see the round-6 report).</summary>
public readonly record struct SpectroscopeState(int Average, bool Window, bool LogY);

/// <summary>
/// The cross-frame FFT input buffer: the last <c>Average</c> raw sample frames per channel, mirroring Rust's
/// <c>Vec&lt;VecDeque&lt;Vec&lt;f64&gt;&gt;&gt;</c>. Immutable by convention -- <see cref="Spectroscope.Process"/>
/// never mutates an existing instance, it only builds and returns a new one (round-6's state-OUT channel on
/// <see cref="IDisplayMode.Process"/>), so this is safe to hand across the UI/background-thread boundary.
/// </summary>
public sealed record SpectroscopeAccumulator(double[][][] ChannelHistory);

/// <summary>
/// Port of scope-tui's Spectroscope display mode (display/spectroscope.rs): an FFT magnitude spectrum per
/// channel, plotted as (ln(frequency), ln(magnitude)) on ordinary LINEAR plot axes -- the Rust source (and this
/// port) does the ln() itself rather than needing a log-scaled axis from the plotting library. Optional Hann
/// window (<c>w</c>), optional log-Y (<c>l</c>), and N-frame averaging (PageUp/PageDown) that concatenates the
/// last N raw sample frames before taking one longer FFT -- exactly Rust's <c>buffer_size * average</c> approach.
/// </summary>
/// <remarks>
/// NAudio's <see cref="NAudio.Dsp.FastFourierTransform"/> (unlike Rust's <c>rustfft</c>, a mixed-radix FFT that
/// accepts ANY length) only accepts power-of-two lengths. This port zero-pads the (optionally windowed) sample
/// chunk up to the next power of two before the FFT -- <see cref="NextPowerOfTwo"/> -- so averaging still works
/// for arbitrary buffer/average combinations; the ACTUAL frequency resolution used for the X axis is computed
/// from the padded length (spectrally correct for what was actually transformed), while the header's displayed
/// "Hz bins" figure mirrors Rust's own nominal <c>sampling_rate / (buffer_size * average)</c> formula, which is
/// no longer bit-identical to the plotted resolution when padding occurred. See the round-6 report for why this
/// is flagged as a capability gap (NAudio has no documented arbitrary-length transform) rather than silently
/// worked around.
///
/// ROUND-7: <see cref="NAudio.Dsp.FastFourierTransform.FFT"/>'s forward transform ALSO differs from rustfft in
/// scale -- it divides by N where rustfft does not (undocumented on NAudio's side; found empirically, not from
/// any doc page). <see cref="FftMagnitudeSpectrum"/> multiplies the magnitude back by N to match rustfft's
/// convention, which is what the axis bounds/gridlines below assume; see the comment at the multiplication site
/// for the full account of round-6's blank-spectrum bug this caused.
/// </remarks>
public sealed class Spectroscope(int sampleRate, int bufferSize) : IDisplayMode
{
    public int Average = 1;
    public bool Window;
    public bool LogY = true; // Rust's Spectroscope::from(SourceOptions) defaults log_y: true

    public const string ModeStr = "spectro";

    public static string ChannelName(int index) => index switch { 0 => "L", 1 => "R", _ => index.ToString() };

    /// <summary>Cheap copy of the mutable knobs above, safe to read from a background thread.</summary>
    public SpectroscopeState Snapshot() => new(int.Max(1, Average), Window, LogY);

    /// <summary>Mirrors Spectroscope::header exactly (window marker, average count/seconds, bin width).</summary>
    public string Header(SpectroscopeState s)
    {
        var windowMarker = s.Window ? "-|-" : "---";
        return s.Average <= 1
            ? $"live  {windowMarker}  {(double)sampleRate / bufferSize:0.000}Hz bins"
            : $"{s.Average}x avg ({(double)(s.Average * bufferSize) / sampleRate:0.0}s)  {windowMarker}  {(double)sampleRate / (bufferSize * s.Average):0.000}Hz bins";
    }

    /// <summary>Port of the free fn <c>hann_window()</c> in spectroscope.rs: a pure function of the sample array,
    /// independent of any instance/mode state, so it can be unit-tested directly against known values.</summary>
    public static double[] HannWindow(double[] samples)
    {
        var n = samples.Length;
        var windowed = new double[n];
        for (var i = 0; i < n; i++)
        {
            var twoPiI = 2.0 * Math.PI * i;
            var multiplier = 0.5 * (1.0 - Math.Cos(twoPiI / n));
            windowed[i] = samples[i] * multiplier;
        }
        return windowed;
    }

    /// <summary>Rounds <paramref name="n"/> up to the next power of two (minimum 1) -- see the class remarks for
    /// why the FFT needs this (NAudio's transform, unlike Rust's rustfft, only accepts power-of-two lengths).</summary>
    public static int NextPowerOfTwo(int n)
    {
        var m = 1;
        while (m < n) m <<= 1;
        return m;
    }

    /// <summary>
    /// Port of the FFT/normalization core of Spectroscope::process for ONE channel's chunk: normalizes by the
    /// chunk's own peak (floored at 1.0, matching Rust), zero-pads to a power of two, runs NAudio's FFT, and
    /// returns (ln(frequency), possibly-ln(magnitude)) pairs, skipping the DC bin (index 0, whose ln(0*resolution)
    /// is -infinity and isn't meaningful to plot anyway) and the upper (mirror-image) half of the spectrum, since
    /// a real-valued input signal's FFT is symmetric. A pure static function of its inputs -- no NAudio.Dsp state
    /// persists between calls -- so it is unit-testable and safe to call from a background thread.
    /// </summary>
    public static (double[] Freq, double[] Mag) FftMagnitudeSpectrum(double[] chunk, int sampleRate, bool logY)
    {
        if (chunk.Length == 0) return ([], []);

        var maxVal = chunk.Max();
        if (maxVal < 1.0) maxVal = 1.0;

        var n = NextPowerOfTwo(chunk.Length);
        var complex = new NAudio.Dsp.Complex[n];
        for (var i = 0; i < chunk.Length; i++) complex[i].X = (float)(chunk[i] / maxVal);
        NAudio.Dsp.FastFourierTransform.FFT(true, (int)Math.Log2(n), complex);

        var resolution = (double)sampleRate / n;
        var bins = n / 2;
        if (bins <= 1) return ([], []);

        var freq = new double[bins - 1];
        var mag = new double[bins - 1];
        for (var i = 1; i < bins; i++)
        {
            // ROUND-7 FIX: NAudio.Dsp.FastFourierTransform.FFT(forward: true, ...) normalizes the forward transform
            // by 1/N (undocumented in NAudio's own XML docs -- confirmed empirically: a known tone's raw magnitude
            // came out ~1/N of the expected value). Rust's rustfft (the reference, spectroscope.rs) does NOT
            // normalize its forward transform. Every downstream constant in this port -- the log-Y axis bounds
            // (IDisplayMode.AxisBounds: yMin=0, yMax=Scale*7.5) and the decade-marker reference line heights -- was
            // carried over verbatim from Rust and so assumes UNNORMALIZED magnitudes. Left as NAudio produces it,
            // ln(magnitude) for a normal-amplitude tone lands around ln(peak/N), which is comfortably BELOW yMin=0
            // and gets clipped off the bottom of the plot entirely -- the spectroscope rendered nothing but its
            // reference grid. Multiplying back by N here restores rustfft's convention so the existing axis
            // bounds/gridlines (unchanged, to stay faithful to spectroscope.rs) are the right window again.
            var magnitude = Math.Sqrt(complex[i].X * complex[i].X + complex[i].Y * complex[i].Y) * n;
            freq[i - 1] = Math.Log(i * resolution);
            mag[i - 1] = logY ? Math.Log(double.Max(magnitude, 1e-9)) : magnitude;
        }
        return (freq, mag);
    }

    /// <summary>
    /// Pure computation mirroring Spectroscope::process: rolls each channel's history forward by one frame
    /// (unless paused -- exactly Rust's <c>if !cfg.pause { push_back; trim to average }</c>), flattens the
    /// history into one chunk, optionally windows it, computes its FFT magnitude spectrum, and emits one series
    /// per channel (highest channel index first, matching Rust's <c>.rev()</c>). Returns the NEW accumulator
    /// alongside the series -- see the state-OUT remark on <see cref="IDisplayMode"/>.
    /// </summary>
    public static (List<Series> Series, SpectroscopeAccumulator NextAccumulator) Process(
        GraphSnapshot g, SpectroscopeState state, SpectroscopeAccumulator? prior, double[][] data, int sampleRate)
    {
        var priorHistory = prior?.ChannelHistory ?? [];
        var history = new double[data.Length][][];
        for (var c = 0; c < data.Length; c++)
        {
            var chanHistory = c < priorHistory.Length ? priorHistory[c] : [];
            if (!g.Pause)
            {
                chanHistory = [.. chanHistory, data[c]];
                if (chanHistory.Length > state.Average) chanHistory = chanHistory[^state.Average..];
            }
            history[c] = chanHistory;
        }

        var outSeries = new List<Series>();
        for (var n = data.Length - 1; n >= 0; n--)
        {
            var chunk = history[n].SelectMany(x => x).ToArray();
            if (chunk.Length == 0) continue;
            var windowed = state.Window ? HannWindow(chunk) : chunk;
            var (freq, mag) = FftMagnitudeSpectrum(windowed, sampleRate, state.LogY);
            outSeries.Add(new Series(ChannelName(n), freq, mag, g.Scatter, g.PaletteFor(n)));
        }

        return (outSeries, new SpectroscopeAccumulator(history));
    }

    /// <summary>Round-6: PageUp/PageDown (average), 'w' (window), 'l' (log-Y). Mirrors Spectroscope::handle.</summary>
    public bool HandleKey(ConsoleKeyInfo key, double magnitude)
    {
        switch (key.Key)
        {
            case ConsoleKey.PageUp: GraphConfig.UpdateI(ref Average, true, 1, magnitude, 1, 65535); return true;
            case ConsoleKey.PageDown: GraphConfig.UpdateI(ref Average, false, 1, magnitude, 1, 65535); return true;
        }
        switch (key.KeyChar)
        {
            case 'w': Window = !Window; return true;
            case 'l': LogY = !LogY; return true;
        }
        return false;
    }

    // Decade/marker frequencies for the reference gridlines below -- verbatim from spectroscope.rs::references.
    static readonly double[] markerFrequencies =
    [
        20, 30, 40, 50, 60, 70, 80, 90, 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000,
        2000, 3000, 4000, 5000, 6000, 7000, 8000, 9000, 10000, 20000,
    ];

    #region IDisplayMode
    string IDisplayMode.ModeStr => ModeStr;
    (string X, string Y) IDisplayMode.AxisCaptions(object? modeState) =>
        ("frequency -", ((SpectroscopeState)modeState!).LogY ? "| level" : "| amplitude");
    string IDisplayMode.ChannelName(int index) => ChannelName(index);
    object? IDisplayMode.Snapshot() => Snapshot();
    string IDisplayMode.Header(object? modeState) => Header((SpectroscopeState)modeState!);
    (double XMin, double XMax, double YMin, double YMax) IDisplayMode.AxisBounds(GraphSnapshot g, object? modeState) =>
        (Math.Log(20.0), Math.Log(g.Samples / (double)g.Width * 20000.0), 0.0, g.Scale * 7.5);
    IReadOnlyList<Series> IDisplayMode.References(GraphSnapshot g)
    {
        var lower = 0.0;
        var upper = g.Scale * 7.5;
        List<Series> refs = [new(null, [0.0, Math.Log(g.Samples)], [0.0, 0.0], Scatter: false, g.AxisColor)];
        refs.AddRange(markerFrequencies.Select(f =>
            new Series(null, [Math.Log(f), Math.Log(f)], [lower, upper], Scatter: false, g.AxisColor)));
        return refs;
    }
    (IReadOnlyList<Series> Series, object? NextState) IDisplayMode.Process(GraphSnapshot g, object? modeState, object? priorState, double[][] channels)
    {
        var (series, next) = Process(g, (SpectroscopeState)modeState!, priorState as SpectroscopeAccumulator, channels, sampleRate);
        return (series, next);
    }
    #endregion
}
