namespace Jumbee.Console;

using System;
using System.Collections.Generic;

using Spectre.Console.Rendering;

using SColor = Spectre.Console.Color;
using SStyle = Spectre.Console.Style;

/// <summary>What the optional time column shows.</summary>
public enum ProgressTimeDisplay
{
    /// <summary>No time column.</summary>
    None,
    /// <summary>Wall-clock time since <see cref="ProgressBar.Start"/>.</summary>
    Elapsed,
    /// <summary>Estimated time to completion, from the elapsed time and the current fraction.</summary>
    Remaining,
}

/// <summary>
/// A single-row task progress display, modelled on one row of a Spectre.Console <c>Progress</c>: a
/// <see cref="Description"/>, a bar filled to <see cref="Value"/> / <see cref="Max"/>, then optional
/// <see cref="ShowPercentage">percentage</see>, <see cref="TimeDisplay">time</see> and
/// <see cref="ShowSpinner">spinner</see> columns — e.g. <c>Consulting the oracle ──── 96% 00:00:00 ⣷</c>.
/// </summary>
/// <remarks>
/// Unlike <see cref="SpectreTaskProgress"/> (the multi-task Spectre widget), this is a plain composable control you
/// place, theme and drive yourself: set <see cref="Value"/> as work advances. It is the task-oriented sibling of
/// <see cref="Gauge"/> (a dashboard meter). Call <see cref="Start"/> to begin the internal clock and, when a spinner
/// or <see cref="IsIndeterminate"/> pulse is shown, its animation; <see cref="Stop"/> freezes both. The bar is a
/// smooth sub-cell band by default; <see cref="Glyphs"/> switches it to a per-cell glyph fill (hatch, segments, ASCII).
/// </remarks>
public class ProgressBar : RenderableControl
{
    #region Constructors
    /// <summary>Initializes a new <see cref="ProgressBar"/> with an optional <paramref name="description"/>, current
    /// <paramref name="value"/> and full-bar <paramref name="max"/>.</summary>
    public ProgressBar(string? description = null, double value = 0, double max = 100)
    {
        Focusable = false;   // a passive display control: never a focus/tab target
        _description = description;
        _value = value;
        _max = max <= 0 ? 1 : max;
        Height = 1;
        _spinnerFrames = _spinner.Frames;
        ApplyTheme();
    }
    #endregion

    #region Properties
    /// <summary>The task status text drawn before the bar. Null/empty draws none. Ellipsis-truncated to keep the bar
    /// at least a few cells wide.</summary>
    public string? Description { get => _description; set => SetAtomicProperty(ref _description, value); }

    /// <summary>The current value. The filled fraction is <see cref="Value"/> / <see cref="Max"/> (clamped to 0..1).</summary>
    public double Value { get => _value; set => SetAtomicProperty(ref _value, value); }

    /// <summary>The value mapped to a full bar. Coerced to at least a tiny positive number so the fraction is defined.</summary>
    public double Max { get => _max; set => SetAtomicProperty(ref _max, value, validate: v => v <= 0 ? 1 : v); }

    /// <summary>Whether the total is unknown: the bar shows a moving pulse rather than a fill, and the percentage is
    /// suppressed. The pulse animates only while <see cref="Start"/>ed. Default <see langword="false"/>.</summary>
    public bool IsIndeterminate { get => _indeterminate; set => SetAtomicProperty(ref _indeterminate, value, watch: (_, _) => SyncAnimation()); }

    /// <summary>Whether to draw the percentage (<c>96%</c>) after the bar. Ignored when
    /// <see cref="IsIndeterminate"/>. Default <see langword="true"/>.</summary>
    public bool ShowPercentage { get => _showPercentage; set => SetAtomicProperty(ref _showPercentage, value); }

    /// <summary>Whether to draw the animated spinner glyph after the other columns. Animates only while
    /// <see cref="Start"/>ed. Default <see langword="false"/>.</summary>
    public bool ShowSpinner { get => _showSpinner; set => SetAtomicProperty(ref _showSpinner, value, watch: (_, _) => SyncAnimation()); }

    /// <summary>What the time column shows (elapsed, estimated-remaining, or nothing). Timed from <see cref="Start"/>.
    /// Default <see cref="ProgressTimeDisplay.None"/>.</summary>
    public ProgressTimeDisplay TimeDisplay { get => _timeDisplay; set => SetAtomicProperty(ref _timeDisplay, value, watch: (_, _) => SyncAnimation()); }

    /// <summary>The spinner animation (frame set and interval) used when <see cref="ShowSpinner"/> is set.</summary>
    public Spectre.Console.Spinner SpinnerType
    {
        get => _spinner;
        set => SetAtomicProperty(ref _spinner, value, watch: (_, v) => { _spinnerFrames = v.Frames; SyncAnimation(); });
    }

    /// <summary>The per-part colours. Defaults to <see cref="IStyleTheme.ProgressBar"/>.</summary>
    public ProgressBarStyle Style { get => _style; set => SetAtomicProperty(ref _style, value, themeOverride: true); }

    /// <summary>The bar glyphs and fill mode (solid band vs per-cell glyphs like a hatch or segments). Defaults to
    /// <see cref="IGlyphTheme.ProgressBar"/>.</summary>
    public ProgressBarGlyphs Glyphs { get => _glyphs; set => SetAtomicProperty(ref _glyphs, value, themeOverride: true); }
    #endregion

    #region Methods
    /// <summary>Recolours the bar fill (a fluent shorthand for <c>Style = Style.WithFill(color)</c>); marks it an override.</summary>
    public ProgressBar WithFill(Color color) { Style = _style.WithFill(color); return this; }

    /// <summary>Sets the bar glyphs/fill mode fluently (e.g. <c>WithGlyphs(ProgressBarGlyphs.Hatched)</c>); marks it an override.</summary>
    public ProgressBar WithGlyphs(ProgressBarGlyphs glyphs) { Glyphs = glyphs; return this; }

    /// <summary>Starts the internal clock (for the time column) and, when a spinner or indeterminate pulse is shown,
    /// its animation. Idempotent.</summary>
    public void Start()
    {
        if (_running) return;
        _running = true;
        _startTicks = DateTime.Now.Ticks;
        SyncAnimation();
        Invalidate();
    }

    /// <summary>Freezes the clock and the animation at the current frame. Idempotent.</summary>
    public void Stop()
    {
        if (!_running) return;
        _running = false;
        _stopTicks = DateTime.Now.Ticks;
        SyncAnimation();
    }

    /// <inheritdoc/>
    protected override void ApplyTheme()
    {
        if (!IsThemeOverridden(nameof(Style))) _style = UI.StyleTheme.ProgressBar;
        if (!IsThemeOverridden(nameof(Glyphs))) _glyphs = UI.GlyphTheme.ProgressBar;
    }

    // Content-only render (never reads focus/hover): reuse the cached buffer on interactive-state changes.
    /// <summary>Content-only render, so the cached buffer is reused on interactive-state changes.</summary>
    protected override bool RendersInteractiveState => false;

    /// <summary>Fixed one row tall; fills the width its parent offers.</summary>
    protected override int IntrinsicHeight() => 1;

    /// <inheritdoc/>
    protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        int width = Math.Max(1, maxWidth);
        double fraction = Math.Clamp(_max <= 0 ? 0 : _value / _max, 0, 1);

        // The three optional right-hand columns, each carrying its own style, so they cannot be one segment. Each
        // string includes its leading separator space.
        string pct = !_indeterminate && _showPercentage ? $" {fraction * 100:0}%" : "";
        string time = _timeDisplay != ProgressTimeDisplay.None ? " " + FormatTime(fraction) : "";
        string spin = _showSpinner && _spinnerFrames.Count > 0 ? " " + _spinnerFrames[_frame % _spinnerFrames.Count] : "";
        int rightWidth = pct.Length + time.Length + spin.Length;

        // Fit the description into whatever is left after the bar's minimum and the right columns, ellipsizing it
        // rather than starving the bar. A one-cell gap separates the description from the bar.
        string desc = _description ?? "";
        int descBudget = width - rightWidth - MinBarWidth - 1;
        if (desc.Length > 0 && descBudget < desc.Length) desc = descBudget <= 1 ? "" : desc[..(descBudget - 1)] + "…";
        string left = desc.Length == 0 ? "" : desc + " ";

        int barWidth = Math.Max(1, width - left.Length - rightWidth);

        var textStyle = _style.Description.SpectreConsoleStyle;
        if (left.Length > 0) yield return new Segment(left, textStyle);

        var fillColor = _style.Fill.SpectreConsoleStyle?.Foreground ?? SColor.Green;
        var trackColor = _style.Track.SpectreConsoleStyle?.Foreground ?? SColor.Grey;
        // Solid mode paints background-colour bands (with a sub-cell edge); Glyph mode paints per-cell foreground
        // glyphs. Each returns the same left-to-right run of the bar's cells.
        var barSegments = _glyphs.Mode == ProgressBarFillMode.Glyph
            ? (_indeterminate ? PulseBarGlyph(barWidth, fillColor, trackColor) : FillBarGlyph(barWidth, fraction, fillColor, trackColor))
            : (_indeterminate ? PulseBar(barWidth, fillColor, trackColor) : FillBar(barWidth, fraction, fillColor, trackColor));
        foreach (var segment in barSegments)
            yield return segment;

        if (pct.Length > 0) yield return new Segment(pct, _style.Percentage.SpectreConsoleStyle);
        if (time.Length > 0) yield return new Segment(time, _style.Time.SpectreConsoleStyle);
        if (spin.Length > 0) yield return new Segment(spin, _style.Spinner.SpectreConsoleStyle);
    }

    // A fill proportional to fraction: full fill cells, a fractional eighth-block edge (fill on the left, track on
    // the right), then the remaining track — the smooth sub-cell bar shared with Gauge.
    private static IEnumerable<Segment> FillBar(int barWidth, double fraction, SColor fillColor, SColor trackColor)
    {
        var fillBand = new SStyle(background: fillColor);
        var trackBand = new SStyle(background: trackColor);

        double exact = fraction * barWidth;
        int full = (int)Math.Floor(exact);
        int eighths = (int)Math.Round((exact - full) * 8);
        if (eighths >= 8) { full++; eighths = 0; }
        full = Math.Min(full, barWidth);
        bool hasEdge = eighths > 0 && full < barWidth;
        int trackCells = barWidth - full - (hasEdge ? 1 : 0);

        if (full > 0) yield return new Segment(new string(' ', full), fillBand);
        if (hasEdge) yield return new Segment(LeftBlocks[eighths].ToString(), new SStyle(foreground: fillColor, background: trackColor));
        if (trackCells > 0) yield return new Segment(new string(' ', trackCells), trackBand);
    }

    // Indeterminate: a band of fill ping-pongs across the track, its position driven by the animation frame. A
    // ping-pong (rather than a wrap) keeps each frame three contiguous runs, so nothing has to split across the seam.
    private IEnumerable<Segment> PulseBar(int barWidth, SColor fillColor, SColor trackColor)
    {
        var fillBand = new SStyle(background: fillColor);
        var trackBand = new SStyle(background: trackColor);

        // Band is about a quarter of the track, at least 3 cells — but never wider than the track itself, which is
        // as narrow as one cell during the first layout pass (a naive Clamp(_, 3, barWidth) throws there, min > max).
        int bandWidth = Math.Min(Math.Max(3, barWidth / 4), barWidth);
        int span = barWidth - bandWidth;
        int pos;
        if (span <= 0)
        {
            pos = 0;
        }
        else
        {
            int t = _frame % (2 * span);
            pos = t <= span ? t : 2 * span - t;
        }

        int after = barWidth - pos - bandWidth;
        if (pos > 0) yield return new Segment(new string(' ', pos), trackBand);
        yield return new Segment(new string(' ', bandWidth), fillBand);
        if (after > 0) yield return new Segment(new string(' ', after), trackBand);
    }

    // Glyph mode: whole-cell fill (no sub-cell edge — the glyphs occupy full cells), each cell the fill or track
    // glyph in the fill/track colour as foreground.
    private IEnumerable<Segment> FillBarGlyph(int barWidth, double fraction, SColor fillColor, SColor trackColor)
    {
        int filled = Math.Clamp((int)Math.Round(fraction * barWidth), 0, barWidth);
        if (filled > 0) yield return new Segment(Repeat(_glyphs.Fill, filled), new SStyle(foreground: fillColor));
        if (filled < barWidth) yield return new Segment(Repeat(_glyphs.Track, barWidth - filled), new SStyle(foreground: trackColor));
    }

    // Glyph-mode pulse: the same ping-pong band as PulseBar, but drawn with the fill/track glyphs.
    private IEnumerable<Segment> PulseBarGlyph(int barWidth, SColor fillColor, SColor trackColor)
    {
        int bandWidth = Math.Min(Math.Max(3, barWidth / 4), barWidth);
        int span = barWidth - bandWidth;
        int pos = span <= 0 ? 0 : _frame % (2 * span) is var t && t <= span ? t : 2 * span - t;
        int after = barWidth - pos - bandWidth;
        var fillStyle = new SStyle(foreground: fillColor);
        var trackStyle = new SStyle(foreground: trackColor);
        if (pos > 0) yield return new Segment(Repeat(_glyphs.Track, pos), trackStyle);
        yield return new Segment(Repeat(_glyphs.Fill, bandWidth), fillStyle);
        if (after > 0) yield return new Segment(Repeat(_glyphs.Track, after), trackStyle);
    }

    // Repeat a bar glyph across n cells. Fast path for the common single-char glyph; empty glyph degrades to a space
    // so a misconfigured set can't throw or emit nothing.
    private static string Repeat(string glyph, int n)
    {
        if (string.IsNullOrEmpty(glyph)) return new string(' ', n);
        if (glyph.Length == 1) return new string(glyph[0], n);
        return string.Concat(System.Linq.Enumerable.Repeat(glyph, n));
    }

    private string FormatTime(double fraction)
    {
        var elapsed = TimeSpan.FromTicks((_running ? DateTime.Now.Ticks : _stopTicks == 0 ? _startTicks : _stopTicks) - _startTicks);
        TimeSpan? value = _timeDisplay switch
        {
            ProgressTimeDisplay.Elapsed => elapsed,
            // Estimate from the observed rate; undefined until there is both elapsed time and some progress.
            ProgressTimeDisplay.Remaining when fraction > 0 && elapsed > TimeSpan.Zero =>
                TimeSpan.FromTicks((long)(elapsed.Ticks * (1 - fraction) / fraction)),
            _ => null,
        };
        return value is { } t && t >= TimeSpan.Zero
            ? $"{(int)t.TotalHours:00}:{t.Minutes:00}:{t.Seconds:00}"
            : "--:--:--";
    }

    // Start/stop the single animation feed to match current state: it runs only while Started AND something on the
    // row actually animates (a spinner, an indeterminate pulse, or a live clock). The spinner/pulse want a fast tick;
    // a clock alone only needs to tick each second. Recreated (not just left running) on any change so the interval
    // tracks the current spinner.
    private void SyncAnimation()
    {
        _feed?.Cancel();
        _feed = null;
        if (!_running) return;
        bool animates = _showSpinner || _indeterminate;
        if (!animates && _timeDisplay == ProgressTimeDisplay.None) return;
        int interval = animates ? Math.Max(30, (int)_spinner.Interval.TotalMilliseconds) : 250;
        _feed = Feed(OnTick, interval);
    }

    // On the UI thread (Feed posts here): advance the animation counter and re-render. Invalidate marks the content
    // dirty so the next paint re-runs the Spectre pipeline — the spinner frame, pulse position and clock all update.
    private void OnTick()
    {
        _frame++;
        Invalidate();
    }
    #endregion

    #region Fields
    private string? _description;
    private double _value;
    private double _max;
    private bool _indeterminate;
    private bool _showPercentage = true;
    private bool _showSpinner;
    private ProgressTimeDisplay _timeDisplay = ProgressTimeDisplay.None;
    private Spectre.Console.Spinner _spinner = Spectre.Console.Spinner.Known.Dots;
    private IReadOnlyList<string> _spinnerFrames;
    private ProgressBarStyle _style;
    private ProgressBarGlyphs _glyphs;

    private bool _running;
    private long _startTicks;
    private long _stopTicks;
    private int _frame;
    private FeedHandle? _feed;

    // Bar never shrinks below this, even under a long description (which ellipsizes instead).
    private const int MinBarWidth = 4;

    // Left-anchored eighth blocks for the fractional fill edge (index = eighths filled, 0 = empty .. 8 = full).
    private static readonly char[] LeftBlocks = [' ', '▏', '▎', '▍', '▌', '▋', '▊', '▉', '█'];
    #endregion
}
