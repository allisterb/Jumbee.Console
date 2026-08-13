namespace Jumbee.Console;

using System;
using System.Collections.Generic;

using ConsoleGUI.Input;
using ConsoleGUI.Space;

using Spectre.Console.Rendering;

using SStyle = Spectre.Console.Style;

/// <summary>
/// A single-row draggable value control: an optional <see cref="Label"/>, a track filled to <see cref="Value"/>
/// between <see cref="Minimum"/> and <see cref="Maximum"/>, and an optional numeric readout — e.g.
/// <c>Gravity ████▌      9.80</c>.
/// </summary>
/// <remarks>
/// <para>
/// The interactive sibling of <see cref="Gauge"/> and <see cref="ProgressBar"/>, which only display a value. Drag
/// the thumb or click anywhere on the track to set it; arrows step by <see cref="Step"/>, Page Up/Down by ten
/// steps, Home/End jump to the ends, and the wheel steps while the pointer is over the control.
/// </para>
/// <para>
/// The thumb occupies one whole cell at the fill's leading edge, so a stack of sliders reads as a row of controls
/// rather than as a bar chart. Only its <em>position</em> is quantised to a cell — the value itself stays
/// continuous. Set <see cref="SnapToStep"/> for a slider whose value should only ever land on a step (an integer
/// count, say).
/// </para>
/// </remarks>
public class Slider : RenderableControl
{
    #region Constructors
    /// <summary>Initializes a new <see cref="Slider"/> over the range <paramref name="minimum"/>..<paramref name="maximum"/>
    /// at <paramref name="value"/>, with an optional <paramref name="label"/>.</summary>
    public Slider(double minimum = 0, double maximum = 1, double value = 0, string? label = null)
    {
        _minimum = minimum;
        _maximum = maximum > minimum ? maximum : minimum + 1;
        _value = Math.Clamp(value, _minimum, _maximum);
        _step = (_maximum - _minimum) / 100;
        _label = label;
        Height = 1;
        ApplyTheme();
    }
    #endregion

    #region Events
    /// <summary>Raised with the new value whenever <see cref="Value"/> changes.</summary>
    public event EventHandler<double>? ValueChanged;
    #endregion

    #region Properties
    /// <summary>Reports <see langword="true"/> so input routing delivers keys to the control.</summary>
    public override bool HandlesInput => true;

    /// <summary>The current value, clamped to <see cref="Minimum"/>..<see cref="Maximum"/>. Setting it raises
    /// <see cref="ValueChanged"/> when it actually moves.</summary>
    public double Value
    {
        get => _value;
        set => SetAtomicProperty(ref _value, value, validate: Coerce, watch: (_, v) => ValueChanged?.Invoke(this, v));
    }

    /// <summary>The low end of the range. Re-clamps <see cref="Value"/>.</summary>
    public double Minimum
    {
        get => _minimum;
        set => SetAtomicProperty(ref _minimum, value, watch: (_, _) => Value = _value);
    }

    /// <summary>The high end of the range. Coerced above <see cref="Minimum"/> so the range is never empty, and
    /// re-clamps <see cref="Value"/>.</summary>
    public double Maximum
    {
        get => _maximum;
        set => SetAtomicProperty(ref _maximum, value, validate: v => v > _minimum ? v : _minimum + 1,
            watch: (_, _) => Value = _value);
    }

    /// <summary>How far one arrow key moves the value (Page Up/Down move ten of these). Defaults to a hundredth of
    /// the range.</summary>
    public double Step
    {
        get => _step;
        set => SetAtomicProperty(ref _step, value, validate: v => v > 0 ? v : (_maximum - _minimum) / 100);
    }

    /// <summary>Whether every value — including one set by dragging or clicking the track — is quantised to a
    /// multiple of <see cref="Step"/> from <see cref="Minimum"/>. Default <see langword="false"/> (continuous).</summary>
    public bool SnapToStep
    {
        get => _snap;
        set => SetAtomicProperty(ref _snap, value, watch: (_, _) => Value = _value);
    }

    /// <summary>The caption drawn before the track. Null/empty draws none.</summary>
    public string? Label { get => _label; set => SetAtomicProperty(ref _label, value); }

    /// <summary>Cells reserved for the label, so a stack of sliders lines its tracks up. 0 (the default) sizes to
    /// the label itself.</summary>
    public int LabelWidth { get => _labelWidth; set => SetAtomicProperty(ref _labelWidth, value, validate: v => Math.Max(0, v)); }

    /// <summary>Whether to draw the numeric readout after the track. Default <see langword="true"/>.</summary>
    public bool ShowValue { get => _showValue; set => SetAtomicProperty(ref _showValue, value); }

    /// <summary>The format passed to <see cref="double.ToString(string)"/> for the readout. Default <c>"F2"</c>.</summary>
    public string ValueFormat { get => _valueFormat; set => SetAtomicProperty(ref _valueFormat, value); }

    /// <summary>The per-part colours. Defaults to <see cref="IStyleTheme.Slider"/>.</summary>
    public SliderStyle Style { get => _style; set => SetAtomicProperty(ref _style, value, themeOverride: true); }

    /// <summary>The whole-cell thumb glyph. Defaults to <see cref="IGlyphTheme.SliderThumb"/>.</summary>
    public string ThumbGlyph { get => _thumbGlyph; set => SetAtomicProperty(ref _thumbGlyph, value, themeOverride: true); }

    /// <summary>Style merged into the label and readout while hovered. Defaults to <see cref="IStyleTheme.Hover"/>.</summary>
    public Style HoverStyle { get => _hoverStyle; set => SetAtomicProperty(ref _hoverStyle, value, themeOverride: true); }

    /// <summary>Style merged into the label and readout while focused. Defaults to <see cref="IStyleTheme.Focus"/>.</summary>
    public Style FocusedStyle { get => _focusedStyle; set => SetAtomicProperty(ref _focusedStyle, value, themeOverride: true); }
    #endregion

    #region Methods
    /// <summary>Moves the value by <paramref name="steps"/> × <see cref="Step"/>.</summary>
    public void StepBy(int steps) => Value = _value + (steps * _step);

    /// <summary>Sets the value from a 0..1 position along the track (a fluent shorthand for the drag/click path).</summary>
    public void SetFraction(double fraction) => Value = _minimum + (Math.Clamp(fraction, 0, 1) * (_maximum - _minimum));

    /// <summary>The value as a 0..1 position along the track.</summary>
    public double Fraction => _maximum > _minimum ? Math.Clamp((_value - _minimum) / (_maximum - _minimum), 0, 1) : 0;

    /// <summary>Recolours the filled portion of the track (marks the style an override).</summary>
    public Slider WithFill(Color color) { Style = _style.WithFill(color); return this; }

    /// <summary>Sets the label and the cells reserved for it, so sliders in a stack align.</summary>
    public Slider WithLabel(string label, int width = 0) { Label = label; LabelWidth = width; return this; }

    /// <inheritdoc/>
    protected override void ApplyTheme()
    {
        if (!IsThemeOverridden(nameof(Style))) _style = UI.StyleTheme.Slider;
        if (!IsThemeOverridden(nameof(ThumbGlyph))) _thumbGlyph = UI.GlyphTheme.SliderThumb;
        if (!IsThemeOverridden(nameof(HoverStyle))) _hoverStyle = UI.StyleTheme.Hover;
        if (!IsThemeOverridden(nameof(FocusedStyle))) _focusedStyle = UI.StyleTheme.Focus;
    }

    // The label and readout carry the focus cue themselves (see Render), so the themed whole-control tint would
    // only wash out the track band on top of it.
    /// <inheritdoc/>
    protected override bool RendersOwnFocus => true;

    /// <inheritdoc/>
    protected override bool WantsMouse => true;

    /// <summary>Fixed one row tall; fills the width its parent offers.</summary>
    protected override int IntrinsicHeight() => 1;

    /// <inheritdoc/>
    protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        var width = Math.Max(1, maxWidth);

        // The readout is right-aligned in a field wide enough for either end of the range, so the track keeps its
        // width as the digits change — otherwise dragging past 9.99 shunts the whole track one cell left.
        var readout = _showValue ? " " + Format(_value).PadLeft(ReadoutWidth()) : "";

        // The label gets its reserved width (or its natural one), but never so much that the track is starved: it
        // ellipsizes instead. Includes the single-space gap before the track.
        var label = BuildLabel(width, readout.Length, out var labelCells);

        var trackWidth = Math.Max(1, width - labelCells - readout.Length);

        // Cached for the mouse path, which only knows an x within the control and has no other way to learn where
        // the track begins.
        _trackStart = labelCells;
        _trackWidth = trackWidth;

        var text = _style.Label;
        var value = _style.Value;
        if (IsMouseOver) { text |= _hoverStyle; value |= _hoverStyle; }
        if (IsFocused) { text |= _focusedStyle; value |= _focusedStyle; }

        if (label.Length > 0) yield return new Segment(label, text);
        foreach (var segment in Track(trackWidth)) yield return segment;
        if (readout.Length > 0) yield return new Segment(readout, value);
    }

    /// <inheritdoc/>
    protected override void OnInput(InputEvent inputEvent)
    {
        // Shift is the fine-adjust modifier, matching the rest of the library's nudge controls.
        var scale = (inputEvent.Key.Modifiers & ConsoleModifiers.Shift) != 0 ? 0.2 : 1;
        switch (inputEvent.Key.Key)
        {
            case ConsoleKey.LeftArrow: Value = _value - (_step * scale); break;
            case ConsoleKey.RightArrow: Value = _value + (_step * scale); break;
            case ConsoleKey.DownArrow: Value = _value - (_step * scale); break;
            case ConsoleKey.UpArrow: Value = _value + (_step * scale); break;
            case ConsoleKey.PageDown: Value = _value - (_step * 10); break;
            case ConsoleKey.PageUp: Value = _value + (_step * 10); break;
            case ConsoleKey.Home: Value = _minimum; break;
            case ConsoleKey.End: Value = _maximum; break;
            default: return;
        }

        inputEvent.Handled = true;
    }

    // Press sets the value immediately and captures, so the pointer keeps steering the thumb even once it leaves
    // the one-row control — which it will, the moment the drag wanders vertically.
    /// <inheritdoc/>
    protected override void OnMousePress(Position position)
    {
        _dragging = true;
        CaptureMouse();
        SetFromX(position.X);
    }

    /// <inheritdoc/>
    protected override void OnMouseMove(Position position)
    {
        if (_dragging) SetFromX(position.X);
    }

    /// <inheritdoc/>
    protected override void OnMouseRelease(Position position)
    {
        if (!_dragging) return;
        _dragging = false;
        ReleaseMouse();
    }

    /// <inheritdoc/>
    protected override void OnMouseWheel(Position position, int delta) => StepBy(delta > 0 ? -1 : 1);

    /// <inheritdoc/>
    protected internal override HelpInfo? GetHelpInfo() => new HelpInfo("Slider", "Slider", "A draggable value.")
        .WithKey("Left / Right", "Step down / up (hold Shift for a fifth of a step)")
        .WithKey("PgUp / PgDn", "Step by ten")
        .WithKey("Home / End", "Jump to the minimum / maximum")
        .WithKey("Drag / click", "Set the value from the track; the wheel steps");
    #endregion

    #region Private methods
    private double Coerce(double value)
    {
        var clamped = Math.Clamp(value, _minimum, _maximum);
        if (!_snap || _step <= 0) return clamped;
        var snapped = _minimum + (Math.Round((clamped - _minimum) / _step) * _step);
        return Math.Clamp(snapped, _minimum, _maximum);
    }

    private string Format(double value) => value.ToString(_valueFormat, System.Globalization.CultureInfo.CurrentCulture);

    private int ReadoutWidth() => Math.Max(Format(_minimum).Length, Format(_maximum).Length);

    // The label plus its trailing gap, and the cells it occupies. Ellipsized rather than allowed to squeeze the
    // track below its minimum, and dropped entirely if there is not even room for that.
    private string BuildLabel(int width, int readoutWidth, out int cells)
    {
        var text = _label ?? "";
        if (text.Length == 0 && _labelWidth == 0) { cells = 0; return ""; }

        var budget = width - readoutWidth - MinTrackWidth - 1;
        if (budget <= 0) { cells = 0; return ""; }

        var target = _labelWidth > 0 ? _labelWidth : text.Length;
        target = Math.Min(target, budget);
        if (text.Length > target) text = target <= 1 ? "" : text[..(target - 1)] + "…";

        cells = target + 1;
        return text.PadRight(target) + " ";
    }

    // The track: a filled band, a WHOLE-CELL handle, then the empty band. The handle is always drawn, at both ends
    // of the range too — an empty track with no marker gives no clue there is anything to drag, and a full one gives
    // no clue where the value is.
    //
    // The obvious refinement — a sub-cell handle from the eighth-block ramp, as ProgressBar uses for its fill edge —
    // was tried and reverted. It works for a FILL, whose leading edge is meant to grow; for a HANDLE it means the
    // marker is one eighth of a cell wide at some values and a full cell at others, so a stack of sliders reads as a
    // ragged bar chart rather than as a row of controls you can grab. Consistent width beats sub-cell precision here,
    // and it costs nothing in accuracy: only the DRAWING quantises to a cell, the value stays continuous.
    private IEnumerable<Segment> Track(int trackWidth)
    {
        var fillColor = _style.Fill.ForegroundColor ?? DefaultFill;
        var trackColor = _style.Track.ForegroundColor ?? DefaultTrack;
        var thumbColor = _style.Thumb.ForegroundColor ?? DefaultThumb;

        // The exact inverse of SetFromX, so the handle lands under the pointer that placed it.
        var handle = trackWidth <= 1 ? 0 : (int)Math.Round(Fraction * (trackWidth - 1));
        handle = Math.Clamp(handle, 0, trackWidth - 1);

        if (handle > 0) yield return new Segment(new string(' ', handle), new SStyle(background: fillColor));
        yield return new Segment(_thumbGlyph, new SStyle(thumbColor, handle > 0 ? fillColor : trackColor));
        var rest = trackWidth - handle - 1;
        if (rest > 0) yield return new Segment(new string(' ', rest), new SStyle(background: trackColor));
    }

    // Map an x within the control to a value. Uses the CELL INDEX across the track rather than the cell's centre, so
    // clicking the first and last cells reaches the exact minimum and maximum — which cell-centre mapping cannot do,
    // and which is the first thing anyone tries.
    private void SetFromX(int x)
    {
        if (_trackWidth <= 0) return;
        if (_trackWidth == 1) { SetFraction(0); return; }
        SetFraction((x - _trackStart) / (double)(_trackWidth - 1));
    }
    #endregion

    #region Fields
    private double _value;
    private double _minimum;
    private double _maximum;
    private double _step;
    private bool _snap;
    private string? _label;
    private int _labelWidth;
    private bool _showValue = true;
    private string _valueFormat = "F2";
    private SliderStyle _style;
    private string _thumbGlyph = "█";
    private Style _hoverStyle;
    private Style _focusedStyle;

    private bool _dragging;
    private int _trackStart;
    private int _trackWidth;

    // The track never shrinks below this; a long label ellipsizes instead.
    private const int MinTrackWidth = 4;

    // Fallbacks when a style leaves a colour unset (the shipped themes always set them).
    private static readonly Color DefaultFill = new(90, 160, 240);
    private static readonly Color DefaultTrack = new(48, 48, 58);
    private static readonly Color DefaultThumb = new(235, 240, 250);
    #endregion
}
