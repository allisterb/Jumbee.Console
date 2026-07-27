namespace ScopeTui;

using Jumbee.Console;

/// <summary>A single series to plot: xs/ys plus how it should be drawn. Mirrors the Rust DataSet in display/mod.rs.</summary>
public readonly record struct Series(string? Name, double[] Xs, double[] Ys, bool Scatter, Color Color);

/// <summary>
/// Strategy for one display mode (oscilloscope, vectorscope, spectroscope), mirroring scope-tui's Rust
/// <c>DisplayMode</c> trait (display/mod.rs). A mode instance lives on the UI thread and owns whatever hotkey-driven
/// knobs it needs (e.g. <see cref="Oscilloscope"/>'s trigger state); <see cref="Snapshot"/> captures those knobs
/// into an immutable value BEFORE crossing to a background thread, so every other member here is a pure function
/// of (<see cref="GraphSnapshot"/>, that snapshot, the channel matrix) and can run off the UI thread with no race.
/// </summary>
/// <remarks>
/// <c>modeState</c>/<c>priorState</c> are <see langword="object"/>? rather than a generic type parameter: <see cref="Program"/>'s
/// mode array is heterogeneous (Oscilloscope's snapshot is an <see cref="OscilloscopeState"/> struct; Vectorscope has no
/// mutable knobs at all, so its snapshot is <see langword="null"/>), and a single array of <c>IDisplayMode&lt;TState&gt;</c>
/// can't hold mixed <c>TState</c>s. Each implementation casts its own state back out; <see cref="ScopeView.ComputeFrame"/>
/// only ever forwards the objects between <see cref="Snapshot"/>, <see cref="Process"/> and <see cref="AxisCaptions"/>,
/// never inspects them.
/// </remarks>
public interface IDisplayMode
{
    /// <summary>Short mode name shown in the header, e.g. "oscillo", "vector", or "spectro".</summary>
    string ModeStr { get; }
    /// <summary>Fixed, screen-anchored axis captions (X, Y) -- see <see cref="Plot.ConfigureAxis(System.Action{ConsolePlot.Plotting.AxisSettings})"/>.
    /// Takes <paramref name="modeState"/> because a caption can depend on mode state (Spectroscope's Y caption
    /// flips "| level" / "| amplitude" with its log-Y toggle); modes with a fixed caption just ignore the param.</summary>
    (string X, string Y) AxisCaptions(object? modeState);
    /// <summary>Per-mode channel label, e.g. Oscilloscope's "L"/"R" vs Vectorscope's plain "0"/"1".</summary>
    string ChannelName(int index);
    /// <summary>Captures this mode's own mutable knobs into an immutable value safe to read from any thread.
    /// Must be called on the UI thread.</summary>
    object? Snapshot();
    /// <summary>Mirrors DisplayMode::header -- "live" or a mode-specific status string.</summary>
    string Header(object? modeState);
    /// <summary>Fixed plot bounds (xMin, xMax, yMin, yMax) for this mode/config.</summary>
    (double XMin, double XMax, double YMin, double YMax) AxisBounds(GraphSnapshot g, object? modeState);
    /// <summary>Mirrors DisplayMode::references -- static reference geometry (e.g. a 0-line, a crosshair, decade markers).</summary>
    IReadOnlyList<Series> References(GraphSnapshot g);
    /// <summary>
    /// Mirrors DisplayMode::process -- the heavy per-frame transform from the channel matrix to series. Pure:
    /// touches no instance field, reads only <paramref name="priorState"/> (this mode's own cross-frame
    /// accumulator, opaque, <see langword="null"/> the first time) and returns the accumulator to carry into the
    /// NEXT call alongside the computed series -- see the state-OUT remark on this interface.
    /// </summary>
    (IReadOnlyList<Series> Series, object? NextState) Process(GraphSnapshot g, object? modeState, object? priorState, double[][] channels);
    /// <summary>
    /// Mirrors DisplayMode::handle -- handles one mode-specific keypress (<paramref name="magnitude"/> is the
    /// same Shift/Ctrl/Alt step-size tier Program.cs already computes for the shared arrow keys). Returns
    /// <see langword="true"/> if this mode consumed the key (caller should request a rebuild); a mode that
    /// doesn't recognize the key returns <see langword="false"/> and does nothing -- this is what lets Program.cs
    /// register every mode-specific key exactly once and forward blindly, with no concrete-type check.
    /// </summary>
    bool HandleKey(ConsoleKeyInfo key, double magnitude);
}
