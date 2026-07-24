namespace ScopeTui;

using Jumbee.Console;

/// <summary>Immutable snapshot of the oscilloscope's trigger/peaks knobs (mutated only by hotkey handlers on the
/// UI thread), taken via <see cref="Oscilloscope.Snapshot"/> before crossing to a background thread.</summary>
public readonly record struct OscilloscopeState(bool Triggering, bool FallingEdge, double Threshold, int Depth, bool Peaks);

/// <summary>
/// Faithful port of scope-tui's Oscilloscope display mode (display/oscilloscope.rs): per-channel triggering,
/// trigger-offset alignment, optional min/max peak markers, and the mode header string.
/// </summary>
/// <remarks>
/// The mutable fields below are hotkey-driven UI state (touched only on the UI thread). The actual per-frame math
/// is exposed as pure <c>static</c> functions of value-type snapshots (<see cref="OscilloscopeState"/>,
/// <see cref="Process"/>) so <see cref="ScopeView.ComputeFrame"/> can run it off the UI thread with no
/// shared-state race, and so the trigger algorithm can be unit-tested directly. Implements <see cref="IDisplayMode"/>
/// via explicit interface members that just forward to these existing statics/instance members. Has no cross-frame
/// state of its own, so <see cref="IDisplayMode.Process"/>'s <c>NextState</c> is always <see langword="null"/>.
/// Round-6 adds <see cref="IDisplayMode.HandleKey"/>, taking over t/e/p/threshold/depth from Program.cs's old
/// concrete-type-checked hotkeys.
///
/// Round-9 (item 2): <see cref="Process"/> takes a <c>gain</c> multiplier applied INLINE, in the same per-sample
/// loop that already copies each channel's samples into new <c>xs</c>/<c>ys</c> lists (and in <see cref="Triggered"/>'s
/// threshold comparison) — replacing round-8's separate <c>Array.ConvertAll</c> pass over the whole channel matrix
/// that <see cref="ScopeView.ComputeFrame"/> used to make just to apply gain before calling <c>Process</c>. Trigger
/// detection and peak min/max are computed from the SAME gained value the loop already produces, so behaviour is
/// unchanged from round-8 (still gained-space detection/display) with one fewer ~32KB copy per oscilloscope tick.
///
/// Round-10 (item 3): the explicit <see cref="IDisplayMode.Process"/> implementation now reads the gain from
/// <see cref="GraphSnapshot.Gain"/> instead of <see cref="ScopeView.ComputeFrame"/> calling the gain-aware static
/// overload directly via a <c>mode is Oscilloscope</c> type check -- there is now exactly ONE processing entry
/// point (<see cref="Process"/>), reached uniformly through the interface for every mode.
/// </remarks>
public class Oscilloscope : IDisplayMode
{
    public bool Triggering;
    public bool FallingEdge;
    public double Threshold; // in normalized -1..1 sample units (scope-tui uses raw int16 units 0..32768)
    public int Depth = 1;
    public bool Peaks = true;

    public const string ModeStr = "oscillo";

    public static string ChannelName(int index) => index switch { 0 => "L", 1 => "R", _ => index.ToString() };

    /// <summary>Cheap copy of the mutable knobs above, safe to read from a background thread.</summary>
    public OscilloscopeState Snapshot() => new(Triggering, FallingEdge, Threshold, int.Max(1, Depth), Peaks);

    /// <summary>Mirrors DisplayMode::header — "live" or the trigger description.</summary>
    public static string Header(OscilloscopeState s) => s.Triggering
        ? $"{(s.FallingEdge ? "v" : "^")} {s.Threshold:0.00}{(s.Depth > 1 ? $":{s.Depth}" : "")} trigger"
        : "live";

    /// <summary>Mirrors Oscilloscope::references — a flat line at amplitude 0 spanning the sample window.</summary>
    public static Series Reference(int samples, Color axisColor) =>
        new(null, [0.0, samples], [0.0, 0.0], Scatter: false, axisColor);

    /// <summary>
    /// Port of the free fn <c>triggered()</c> in oscilloscope.rs: does sample <paramref name="index"/> start a
    /// rising (or, with <paramref name="fallingEdge"/>, falling) edge crossing <paramref name="threshold"/> that
    /// holds for <paramref name="depth"/> subsequent samples (debounce)? Exposed as a plain static function (no
    /// dependency on <see cref="Oscilloscope"/> instance state) so it can be unit-tested directly.
    /// </summary>
    /// <remarks>
    /// Round-9 (item 2): <paramref name="gain"/> is applied to each sample INSIDE the comparison rather than
    /// requiring the caller to pre-multiply <paramref name="data"/> — see the class remarks.
    /// </remarks>
    public static bool Triggered(IReadOnlyList<double> data, int index, double threshold, int depth, bool fallingEdge, double gain = 1.0)
    {
        if (data.Count < index + (1 + depth)) return false;
        if (fallingEdge)
        {
            if (data[index] * gain < threshold) return false;
            for (var i = 1; i <= depth; i++) if (data[index + i] * gain >= threshold) return false;
            return true;
        }
        else
        {
            if (data[index] * gain > threshold) return false;
            for (var i = 1; i <= depth; i++) if (data[index + i] * gain <= threshold) return false;
            return true;
        }
    }

    /// <summary>Scans channel 0 for the first triggering sample. Returns 0 (no shift) when triggering is off, or
    /// when it never fires within the buffer — mirrors scope-tui, which falls through to the un-shifted window
    /// rather than blanking the display.</summary>
    public static int FindTriggerOffset(IReadOnlyList<double> channel0, bool triggering, double threshold, int depth, bool fallingEdge, double gain = 1.0)
    {
        if (!triggering) return 0;
        for (var i = 0; i < channel0.Count; i++)
            if (Triggered(channel0, i, threshold, depth, fallingEdge, gain)) return i;
        return 0;
    }

    /// <summary>
    /// Pure computation mirroring Oscilloscope::process: finds the trigger offset (if triggering), then emits one
    /// series per channel (aligned to the trigger), optional min/max peak markers, and a trigger-threshold marker.
    /// A function of value-type inputs only — no <see cref="Oscilloscope"/>/<see cref="GraphConfig"/> instance is
    /// touched, so this is safe to call from any thread.
    /// </summary>
    /// <remarks>
    /// Round-9 (item 2): <paramref name="gain"/> folds into the per-sample loop below (which already allocates a
    /// fresh <c>xs</c>/<c>ys</c> pair per channel to build the series) instead of requiring a separate gained copy
    /// of <paramref name="data"/> — see the class remarks. <paramref name="data"/> itself is untouched/ungained;
    /// <paramref name="gain"/> defaults to 1.0 for callers (tests) that don't care about calibration gain.
    /// </remarks>
    public static List<Series> Process(OscilloscopeState state, Color[] palette, Color labelsColor, bool scatter, double[][] data, double gain = 1.0)
    {
        var outSeries = new List<Series>();
        var triggerOffset = data.Length > 0
            ? FindTriggerOffset(data[0], state.Triggering, state.Threshold, state.Depth, state.FallingEdge, gain)
            : 0;

        if (state.Triggering)
            outSeries.Add(new Series("T", [0.0], [state.Threshold], Scatter: true, labelsColor));

        for (var n = data.Length - 1; n >= 0; n--)
        {
            var channel = data[n];
            double min = 0, max = 0;
            var xs = new List<double>(channel.Length);
            var ys = new List<double>(channel.Length);
            for (var i = 0; i < channel.Length; i++)
            {
                var sample = channel[i] * gain;
                if (sample < min) min = sample;
                if (sample > max) max = sample;
                if (i >= triggerOffset) { xs.Add(i - triggerOffset); ys.Add(sample); }
            }

            var color = palette[n % palette.Length];
            if (state.Peaks)
                outSeries.Add(new Series(null, [0.0, 0.0], [min, max], Scatter: true, color));

            outSeries.Add(new Series(ChannelName(n), [.. xs], [.. ys], scatter, color));
        }

        return outSeries;
    }

    /// <summary>Round-6: t/e/p/threshold/depth, taken over from Program.cs's old concrete-type-checked hotkeys.
    /// Mirrors Oscilloscope::handle in oscilloscope.rs.</summary>
    public bool HandleKey(ConsoleKeyInfo key, double magnitude)
    {
        switch (key.Key)
        {
            case ConsoleKey.PageUp: GraphConfig.UpdateF(ref Threshold, 250.0 / 32768.0, magnitude, 0.0, 1.0); return true;
            case ConsoleKey.PageDown: GraphConfig.UpdateF(ref Threshold, -250.0 / 32768.0, magnitude, 0.0, 1.0); return true;
        }
        switch (key.KeyChar)
        {
            case 't': Triggering = !Triggering; return true;
            case 'e': FallingEdge = !FallingEdge; return true;
            case 'p': Peaks = !Peaks; return true;
            case '=': GraphConfig.UpdateI(ref Depth, true, 1, magnitude, 1, 65535); return true;
            case '-': GraphConfig.UpdateI(ref Depth, false, 1, magnitude, 1, 65535); return true;
            case '+': GraphConfig.UpdateI(ref Depth, true, 10, magnitude, 1, 65535); return true;
            case '_': GraphConfig.UpdateI(ref Depth, false, 10, magnitude, 1, 65535); return true;
        }
        return false;
    }

    #region IDisplayMode
    string IDisplayMode.ModeStr => ModeStr;
    (string X, string Y) IDisplayMode.AxisCaptions(object? modeState) => ("time -", "| amplitude");
    string IDisplayMode.ChannelName(int index) => ChannelName(index);
    object? IDisplayMode.Snapshot() => Snapshot();
    string IDisplayMode.Header(object? modeState) => Header((OscilloscopeState)modeState!);
    (double XMin, double XMax, double YMin, double YMax) IDisplayMode.AxisBounds(GraphSnapshot g, object? modeState) =>
        (0, g.Samples, -g.Scale, g.Scale);
    IReadOnlyList<Series> IDisplayMode.References(GraphSnapshot g) => [Reference(g.Samples, g.AxisColor)];
    (IReadOnlyList<Series> Series, object? NextState) IDisplayMode.Process(GraphSnapshot g, object? modeState, object? priorState, double[][] channels) =>
        (Process((OscilloscopeState)modeState!, g.Palette, g.LabelsColor, g.Scatter, channels, g.Gain), null);
    #endregion
}
