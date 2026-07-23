namespace ScopeTui;

using Jumbee.Console;

/// <summary>Immutable, cheap-to-copy snapshot of the <see cref="GraphConfig"/> knobs the heavy per-frame transform
/// reads. Taken on the UI thread (a plain field copy) so a background thread can safely compute a frame without
/// racing the hotkey handlers that mutate <see cref="GraphConfig"/> on the UI thread.</summary>
/// <remarks>
/// Round-10 (item 3): <see cref="Gain"/> rides along on the shared snapshot every mode already receives, instead
/// of a separate <c>oscilloscopeGain</c> parameter threaded only into <see cref="ScopeView.ComputeFrame"/>'s
/// special-cased Oscilloscope branch. Only <see cref="Oscilloscope.Process"/> reads it; Vectorscope/Spectroscope
/// ignore the field, exactly like they already ignore <see cref="Scatter"/>/<see cref="Palette"/> fields they
/// don't need. This restores a single, uniform <see cref="IDisplayMode.Process"/> call in <c>ComputeFrame</c> --
/// no more <c>mode is Oscilloscope ? Oscilloscope.Process(...) : mode.Process(...)</c> concrete-type check, and no
/// more second gain-blind entry point on Oscilloscope.
/// </remarks>
public readonly record struct GraphSnapshot(
    bool Pause, int Samples, int Width, double Scale, bool Scatter, bool References, bool ShowUi,
    Color[] Palette, Color LabelsColor, Color AxisColor, double Gain = 1.0)
{
    public Color PaletteFor(int index) => Palette[index % Palette.Length];
}

/// <summary>Shared graph state mirroring scope-tui's Rust GraphConfig — the knobs every display mode reads.
/// Mutated only by hotkey handlers on the UI thread; take a <see cref="Snapshot"/> before reading from any other
/// thread.</summary>
public class GraphConfig
{
    public bool Pause;
    public int Samples;
    public int SampleRate;
    public double Scale = 1.0;
    public int Width;
    public bool Scatter;
    public bool References = true;
    public bool ShowUi = true;

    // scope-tui palette: L=red, R=yellow, then green, magenta.
    public Color[] Palette = [Color.Red1, Color.Yellow1, Color.Green1, Color.Magenta1];
    // Round-10 (item 1): true 24-bit RGB, not one of the 16 console colours -- a muted teal-cyan for tick/caption
    // text, distinct from the old Color.Cyan1 16-colour name so a snapshot test can prove the readback is exact
    // RGB, not snapped to a console colour (see ForegroundAt in Tests/Program.cs).
    public Color LabelsColor = new(90, 220, 200);
    // Round-10 (item 1): true 24-bit RGB dim blue-grey for the plot axis/grid/tick lines, replacing the 16-colour
    // Color.Grey -- kept muted (low, close-together channel values) to preserve the reference's understated
    // oscilloscope-bezel look, just now in full colour.
    public Color AxisColor = new(70, 100, 140);
    /// <summary>Round-10 (item 3): the oscilloscope calibration gain, now a GraphConfig field so it rides the
    /// shared <see cref="GraphSnapshot"/> instead of a bespoke <c>ComputeFrame</c> parameter. See the
    /// <see cref="GraphSnapshot"/> remarks.</summary>
    public double Gain = 1.0;

    public GraphSnapshot Snapshot() => new(Pause, Samples, Width, Scale, Scatter, References, ShowUi, Palette, LabelsColor, AxisColor, Gain);

    /// <summary>Clamped increment/decrement helper, mirroring update_value_f in display/mod.rs.</summary>
    public static void UpdateF(ref double val, double baseAmount, double magnitude, double min, double max)
    {
        var delta = baseAmount * magnitude;
        val = double.Clamp(val + delta, min, max);
    }

    /// <summary>Clamped increment/decrement helper, mirroring update_value_i in display/mod.rs.</summary>
    public static void UpdateI(ref int val, bool inc, int baseAmount, double magnitude, int min, int max)
    {
        var delta = (int)(baseAmount * magnitude);
        val = inc ? int.Min(max, val + delta) : int.Max(min, val - delta);
    }
}
