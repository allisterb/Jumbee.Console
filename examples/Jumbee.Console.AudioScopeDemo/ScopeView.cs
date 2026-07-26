namespace ScopeTui;

using Jumbee.Console;

/// <summary>
/// A finished, immutable frame ready to apply to a <see cref="ScopeView"/> -- everything the heavy per-frame
/// transform (mode-specific processing, header text) produces, computed off the UI thread by
/// <see cref="ScopeView.ComputeFrame"/> so applying it (<see cref="ScopeView.Apply"/>) is just cheap Control
/// mutation.
/// </summary>
/// <remarks>
/// Round-4 generalization: round-3's <c>OscilloscopeFrame</c> carried a single <c>XMax</c> plus a symmetric
/// <c>YScale</c>, baking in the oscilloscope's <c>[0,samples] x [-scale,scale]</c> window. This frame instead
/// carries full independent (XMin, XMax, YMin, YMax) -- required for the vectorscope's square <c>[-scale,scale]^2</c>
/// -- plus the axis captions themselves (mode-supplied via <see cref="IDisplayMode.AxisCaptions"/>), so nothing
/// about a specific mode's geometry is hardcoded in <see cref="ScopeView.Apply"/> any more.
///
/// Round-6 adds <see cref="NextModeState"/>: the mode's own cross-frame accumulator (e.g. Spectroscope's FFT
/// history buffer) computed alongside the series by <see cref="IDisplayMode.Process"/>. It rides along in the
/// frame so <c>Program.cs</c> can store it back (marshaled onto the UI thread by virtue of running in the same
/// callback that calls <see cref="ScopeView.Apply"/>) without ScopeView itself needing to know per-mode state
/// shapes.
///
/// Round-8 (item 2) splits the old single <c>Series</c> list into <see cref="References"/> (mode.References --
/// static geometry: zero-lines, crosshairs, the spectroscope's ~29 decade markers) and <see cref="Dynamic"/>
/// (mode.Process -- the live waveform/spectrum/Lissajous data that actually changes every audio tick), plus a
/// <see cref="ReferencesKey"/> the view uses to detect when the STATIC set actually needs recomputing (mode
/// switch, the 'r' toggle, or an axis-bounds-affecting Scale/Samples change) versus every ordinary tick, where it
/// doesn't change at all and <see cref="ScopeView.Apply"/> now skips it entirely -- see Plot.md's remarks on
/// <c>AddLiveSeries</c>, which is the documented lever this splits around.
/// </remarks>
public readonly record struct ScopeFrame(
    IReadOnlyList<Series> References,
    IReadOnlyList<Series> Dynamic,
    object ReferencesKey,
    double XMin,
    double XMax,
    double YMin,
    double YMax,
    string XCaption,
    string YCaption,
    string ModeText,
    string ModuleText,
    string ScaleText,
    string SpfText,
    string FpsText,
    string ScatterGlyph,
    string PauseGlyph,
    bool Scatter,
    Color ModeColor,
    Color LabelsColor,
    Color AxisColor,
    bool ShowUi,
    object? NextModeState);

/// <summary>
/// Reusable eilmeldung/scope-tui-style scope control: a full-width header row (bold mode name plus status fields
/// spread edge-to-edge, mirroring the Rust <c>make_header</c> column percentages) over a <see cref="Plot"/>
/// waveform/Lissajous/spectrum figure with a minimal scope look (axis lines plus captions only, no grid, no tick
/// marks or labels). Mode-agnostic: <see cref="ComputeFrame"/> dispatches to whichever <see cref="IDisplayMode"/>
/// is active instead of calling a concrete mode directly, so the same view/control renders the oscilloscope,
/// vectorscope, OR spectroscope depending on what is passed in.
/// </summary>
/// <remarks>
/// All per-frame math is a pure static function of value-type snapshots (<see cref="ComputeFrame"/>), so it can
/// run off the UI thread; <see cref="Apply"/> only mutates the Plot and header controls and is the one code path
/// shared by the live app (Program.cs) and the snapshot tests -- no duplicated render script to drift.
///
/// Round-8 (items 1-2, perf): <see cref="Apply"/> no longer does <c>plot.Clear()</c> + re-<c>AddSeries</c> every
/// call -- Plot.md's remarks now document exactly this rebuild cost ("all configuration is replayed on each
/// rebuild") and recommend <c>AddLiveSeries</c> + <c>PlotSeries.SetData</c> for "data that changes every frame".
/// <see cref="Apply"/> keeps two POOLS of live <see cref="PlotSeries"/> handles (one for references, one for the
/// dynamic data) and only recreates a pool's handles when its *shape* (series count, and each series' colour AND
/// scatter-vs-line -- see round-9 below) actually changes -- which happens on Tab/mode-switch or a hotkey toggle,
/// not on every ~20Hz audio tick. On an ordinary tick this reduces to a handful of <c>PlotSeries.SetData</c> calls
/// (mutate in place, no chrome replay) for <see cref="ScopeFrame.Dynamic"/>, and NOTHING for
/// <see cref="ScopeFrame.References"/> when <see cref="ScopeFrame.ReferencesKey"/> hasn't changed.
///
/// Round-9 closes the one gap round-8 flagged: <c>Plot.AddLiveScatter</c> (new this round) is the scatter
/// counterpart of <c>AddLiveSeries</c> that round-8's report said was missing. <see cref="ApplyLive"/> now picks
/// <c>AddLiveScatter</c> vs <c>AddLiveSeries</c> PER SERIES from <see cref="Series.Scatter"/> when a pool rebuild
/// is needed (the existing shape-change detection already treats a Scatter flip as a shape change, since
/// <c>dynamicShape</c> already tracked (Color, Scatter) per index -- round-9 just had to make the REBUILD branch
/// pick the right Add* method instead of always calling <c>AddLiveSeries</c>). The old <c>ApplyRebuild</c>
/// Clear()+AddSeries()/AddScatter()-per-frame fallback (and its <c>ScatterFallbackFrames</c> counter) is deleted
/// entirely -- pressing 's' is now just another (rare) pool rebuild, not a standing per-tick cost. See the
/// round-9 report for how cleanly this closed the gap.
///
/// Round-9 (item 3) also value-gates the per-tick chrome that used to run unconditionally: <see cref="Apply"/>
/// only calls <c>Plot.SetXRange</c>/<c>SetYRange</c> when the bounds actually differ from last frame, and
/// <see cref="ApplyAxisAndHeader"/> only pushes header <c>TextLabel</c> text/colour when a compact
/// <see cref="HeaderSnapshot"/> value differs from last frame (also killing the per-frame <c>TextLabel[]</c>
/// literal allocation -- <see cref="uiLabels"/>/<see cref="allLabels"/> are now built once, in the constructor).
/// Axis/grid/tick COLOUR and the axis title COLOUR are likewise set once in the constructor via the new
/// <c>Plot.SetAxisColor</c>/<c>SetGridColor</c>/<c>SetTickColor</c>/<c>SetAxisTitles</c> convenience setters
/// (Plot.md: "Retained across Clear()... set it once at setup rather than per frame"), replacing round-6's
/// per-frame <c>ConfigureAxis(a => a.Pen = new LinePen(...))</c> allocation entirely -- these take a
/// <see cref="Jumbee.Console.Color"/> directly, no more <c>Color.ToConsoleColor()</c>/<c>LinePen</c> dance. Only
/// the axis caption TEXT (which does change, on a mode switch or Spectroscope's 'l' toggle) is still pushed per
/// frame, and even that is gated on an actual text change.
/// </remarks>
public sealed class ScopeView : CompositeControl
{
    // Column percentages, originally copied verbatim from scope-tui's make_header (app.rs): mode/kind, module
    // (trigger/live) status, scale, samples-per-frame, fps, scatter glyph, pause glyph -- spread across the FULL
    // header width. Two columns are ours and have no scope-tui counterpart: the INPUT source and a channels/ticks
    // pair, both of which describe how this scope was configured rather than what it is currently showing. They
    // are slotted after the mode name (widest, and read first), with the percentages of the scope-tui columns
    // trimmed to make room -- the originals were generous, e.g. "oscillo::scope-tui" needs 18 of its 35%.
    // Budgeted so every column still fits its widest text at a 100-cell header, the narrowest width worth
    // supporting with nine columns: mode "oscillo::scope-tui" 18/20, source 21/22, info "2ch t:24" 8/9, module
    // "^ 0:1 trigger" 13/13, scale "-1.00x+" 7/7, spf "2048/2048 spf" 13/13, fps 6/6, glyphs 3/5 and 2/5. Anything
    // wider than that is slack. Columns abut with no separator (as scope-tui's do), so a column that overruns runs
    // straight into its neighbour rather than being visibly cut -- worth re-checking these if a field grows.
    static readonly int[] headerPercents = [20, 22, 9, 13, 7, 13, 6, 5, 5];

    static int[] HeaderColumnWidths(int width)
    {
        var widths = new int[headerPercents.Length];
        var used = 0;
        for (var i = 0; i < headerPercents.Length; i++) { widths[i] = width * headerPercents[i] / 100; used += widths[i]; }
        widths[1] += width - used; // remainder from integer truncation
        return widths;
    }

    /// <summary>Round-9 (item 3): the compact subset of a frame's header fields that actually drive the header
    /// <c>TextLabel</c>s, used to gate <see cref="ApplyAxisAndHeader"/>'s per-tick text/colour reassignment on a
    /// real change instead of touching every label every call.
    /// Round-10 (item 4): <see cref="LabelsColor"/>/<see cref="AxisColor"/> (the plot chrome + label colours) are
    /// now part of THIS SAME equality-gated snapshot -- reviewer #3's pre-empted bug was that a colour-only change
    /// (no text change) could be gated out and silently never repainted once these became runtime RGB values
    /// instead of build-time constants. Folding them in here means ANY change to mode/module/scale/spf/fps/glyphs
    /// /colours reliably re-touches the Plot chrome AND the header labels together, in one gate.</summary>
    readonly record struct HeaderSnapshot(string Mode, string Module, string Scale, string Spf, string Fps, string ScatterGlyph, string PauseGlyph, Color ModeColor, Color LabelsColor, Color AxisColor, bool ShowUi);

    readonly TextLabel modeLabel;
    readonly TextLabel sourceLabel;
    readonly TextLabel infoLabel;
    readonly TextLabel moduleLabel;
    readonly TextLabel scaleLabel;
    readonly TextLabel spfLabel;
    readonly TextLabel fpsLabel;
    readonly TextLabel scatterLabel;
    readonly TextLabel pauseLabel;
    // Round-9 (item 3): cached once instead of an inline `(TextLabel[])[...]` literal reallocated every
    // ApplyAxisAndHeader call -- uiLabels is the visible-branch fields (everything but modeLabel, which is
    // coloured separately by ModeColor), allLabels is the full set used to blank everything when UI is hidden.
    readonly TextLabel[] uiLabels;
    readonly TextLabel[] allLabels;
    readonly Plot plot;
    readonly DockPanel dock;
    int headerBuiltForWidth;
    readonly string sourceText;
    readonly string infoText;

    // Round-8 (item 2): live-series pools + the shape/key bookkeeping that decides whether a pool needs a full
    // Clear()+rebuild (rare: Tab, a toggle, or entering/leaving scatter mode) or just a SetData refresh (every
    // ordinary tick). See the class remarks above and Plot.md's AddLiveSeries/AddLiveScatter guidance.
    readonly List<PlotSeries> referencePool = [];
    readonly List<PlotSeries> dynamicPool = [];
    readonly List<(Color Color, bool Scatter)> dynamicShape = [];
    object? lastReferencesKey;
    int lastReferencesCount = -1;
    bool usingLivePools;

    // Round-9 (item 3): last-applied values, so Apply/ApplyAxisAndHeader can skip a Plot/TextLabel call entirely
    // when nothing actually changed since the previous tick.
    double? lastXMin, lastXMax, lastYMin, lastYMax;
    string? lastXTitle, lastYTitle;
    Color? lastCaptionColor;
    HeaderSnapshot? lastHeader;

    /// <summary>Round-8 test/diagnostic counter: how many times <see cref="ApplyLive"/> had to do a full
    /// <see cref="Plot.Clear"/>+re-<c>AddLiveSeries</c>/<c>AddLiveScatter</c> pool rebuild (mode switch, a toggle
    /// that changes series count/colour/scatter-vs-line) -- as opposed to the common case of a cheap
    /// <see cref="PlotSeries.SetData"/>-only refresh. Lets a snapshot test PROVE the no-rebuild optimization
    /// (see the round-8/9 reports) instead of only eyeballing it. Kept as a SECONDARY signal alongside the
    /// round-9 rendered-output assertions (see Tests/Program.cs).</summary>
    public int LiveShapeRebuilds { get; private set; }
    /// <summary>Round-8 test/diagnostic counter: how many times the STATIC reference series actually got
    /// <see cref="PlotSeries.SetData"/> calls -- gated on <see cref="ScopeFrame.ReferencesKey"/>, so this stays
    /// flat across a run of ordinary audio ticks and only increments on Tab/'r'/a Scale-or-Samples change.</summary>
    public int ReferencesRefreshCount { get; private set; }
    /// <summary>Round-8 test/diagnostic counter: total number of <see cref="Apply"/> calls (every audio tick and
    /// every hotkey), for computing a "rebuild rate" alongside <see cref="LiveShapeRebuilds"/>.</summary>
    public int ApplyCount { get; private set; }

    // A scope fills its frame's viewport (like RunChart/Plot/Log) rather than growing content-tall to be scrolled:
    // without this a framed ScopeView is given unbounded height, so the Plot lays out below the visible area and only
    // its top axis chrome shows through the frame. See ControlFrame's FillsFrameViewport handling.
    protected override bool FillsFrameViewport => true;

    // The Plot/TextLabel children cover the whole pane but aren't focusable, so their cells carry no mouse listener.
    // Opting into the mouse makes CompositeControl attach the pane's own listener to those cells, so a click anywhere
    // on the scope focuses it (click-to-focus) -- which the focused-border cue and per-pane hotkeys key off.
    protected override bool WantsMouse => true;

    /// <summary>The underlying <see cref="Jumbee.Console.Plot"/>, exposed for tests that want to inspect it directly.</summary>
    public Plot Plot => plot;
    /// <summary>The last decode/compute error surfaced via <see cref="SetError"/>, or null.</summary>
    public string? Error { get; private set; }

    /// <param name="xTickStep">Desired spacing, in cells, between horizontal ticks and their grid lines — bigger is
    /// sparser. 0 drops the grid, tick marks and labels entirely (scope-tui draws none of them). Defaults to the
    /// library's own 11.</param>
    /// <param name="source">Where the audio comes from, e.g. <c>live/Microphone Array</c> or <c>file/track.mp3</c>,
    /// shown verbatim in the header after <c>in:</c>. Empty hides the column.</param>
    /// <param name="channels">Channel count of the source, shown as <c>2ch</c>. 0 hides it.</param>
    public ScopeView(int width = 110, int plotHeight = 24, int xTickStep = 11, string source = "", int channels = 0)
    {
        // The source/channels/tick-step strings describe the RUN, not the frame -- nothing mutates them at runtime --
        // so they are formatted once here rather than carried through ScopeFrame/HeaderSnapshot every tick. They
        // still live in uiLabels so the 'h' hide-UI toggle blanks and restores them with everything else.
        // No "in:" label -- the live/ or file/ prefix already says what it is, and the four cells it would cost are
        // the difference between the device name fitting and being clipped on a narrow header.
        sourceText = source;
        infoText = string.Join(' ', new[]
        {
            channels > 0 ? $"{channels}ch" : "",
            xTickStep > 0 ? $"ticks:{xTickStep}" : "ticks:off",
        }.Where(s => s.Length > 0));

        modeLabel = new TextLabel(TextLabelOrientation.Horizontal, "", Color.Red1, decoration: Spectre.Console.Decoration.Bold);
        sourceLabel = new TextLabel(TextLabelOrientation.Horizontal, "", Color.Cyan1);
        infoLabel = new TextLabel(TextLabelOrientation.Horizontal, "", Color.Cyan1);
        moduleLabel = new TextLabel(TextLabelOrientation.Horizontal, "", Color.Cyan1);
        scaleLabel = new TextLabel(TextLabelOrientation.Horizontal, "", Color.Cyan1);
        spfLabel = new TextLabel(TextLabelOrientation.Horizontal, "", Color.Cyan1);
        fpsLabel = new TextLabel(TextLabelOrientation.Horizontal, "", Color.Cyan1);
        scatterLabel = new TextLabel(TextLabelOrientation.Horizontal, "", Color.Cyan1);
        pauseLabel = new TextLabel(TextLabelOrientation.Horizontal, "", Color.Cyan1);
        uiLabels = [sourceLabel, infoLabel, moduleLabel, scaleLabel, spfLabel, fpsLabel, scatterLabel, pauseLabel];
        allLabels = [modeLabel, sourceLabel, infoLabel, moduleLabel, scaleLabel, spfLabel, fpsLabel, scatterLabel, pauseLabel];

        // The header strip's background used to be painted here, once, from PlotSurface. TextLabel now themes both
        // its colours from IStyleTheme.LabelText, so the strip re-paints on a runtime theme switch by itself -- and
        // setting BgColor here would MARK an override and freeze it at the startup theme.

        // The plot takes its surface, axis, grid, tick and series colours from the style theme on construction --
        // nothing is passed in and nothing is set per frame (see ApplyAxisAndHeader).
        //
        // DamageTracking is the textbook case here: a scope pane is mostly empty, and what moves between frames is a
        // thin trace. The grid, axes and tick labels are redrawn every frame but with identical values, so they cost
        // nothing in damage and only the trace's old-plus-new extent is re-composited -- 120 cells instead of 1200 on
        // a 60x20 plot with the trace jumping top-to-bottom.
        //
        // Measured on the live SetData path this pane uses (`Benchmarks -- --damage`, 220x53). A quiet trace at 15%
        // of the Y range: 11660 -> 1966 dirty cells, and the composite 1336us -> 271us, roughly halving the frame.
        // A full-scale trace at 95%: no saving at all (it dirties every cell either way) and ~4% worse overall.
        // Left on because a scope signal spends most of its time nowhere near the rails.
        //
        // What it does NOT buy is terminal load: the ANSI payload was 15620 vs 15739 bytes/frame, unchanged. The
        // renderer already diffs per cell before emitting, so damage narrows what is SCANNED, never what is SENT.
        plot = new Plot { DamageTracking = true };
        // Show the background grid, tick marks, and numeric tick labels (ConfigureTicks's IsVisible and
        // Labels.IsVisible are two SEPARATE flags -- Plot.md remarks). The grid pen is dashed by default (a faint
        // dotted grid); SetGridColor/SetTickColor below recolour them to the scope's AxisColor/LabelsColor while
        // keeping that dashed brush. The tick labels are recomputed whenever the axis range changes (a Scale/Samples
        // hotkey calls Plot.SetXRange/SetYRange, which rebuilds the plot) -- an ordinary per-tick SetData does not
        // rebuild, but the range is unchanged then, so the labels stay correct and are redrawn every frame.
        //
        // xTickStep feeds TickSettings.DesiredXStep, which the library reads as "a tick roughly every N cells":
        // CalculateTickStep is NiceNumber((max - min) / (axisCells / DesiredXStep)), so raising it widens both the
        // tick labels AND the vertical grid lines they anchor. The clamp keeps the step inside the pane, where it
        // still means something -- ConsolePlot now degrades a wider step to a single tick rather than a NaN axis,
        // but asking for a step nothing can honour is not a useful request. 0 means "no chrome at all", matching
        // scope-tui, whose axis() sets only title/style/bounds and never calls .labels(), so ratatui draws no grid
        // and no tick numbers.
        var showChrome = xTickStep > 0;
        plot.ConfigureGrid(g => g.IsVisible = showChrome);
        plot.ConfigureTicks(t =>
        {
            t.IsVisible = showChrome;
            t.Labels.IsVisible = showChrome;
            if (showChrome) t.DesiredXStep = Math.Clamp(xTickStep, 1, Math.Max(1, width));
        });

        // Round-9 -> Round-10 (item 4): axis/grid/tick colour and the axis title colour used to be set ONCE here
        // as build-time constants (Plot.md: "Retained across Clear()... set it once at setup rather than per
        // frame"). Round-10 makes them true runtime RGB values sourced from GraphConfig's AxisColor/LabelsColor
        // (see that class's remarks), so they now belong in the equality-gated <see cref="HeaderSnapshot"/> path
        // in <see cref="ApplyAxisAndHeader"/> instead -- a colour-only change must still repaint, which a
        // constructor-only call could never do. Nothing is set here beyond grid/tick VISIBILITY (which never
        // changes at runtime); the first <see cref="Apply"/> call primes the colours via that gate (lastHeader
        // starts null, so the very first frame always applies).

        // Round-5 fidelity fix (item 4): Grid.md is explicit that a Grid's own size is the FIXED sum of its
        // row/column cells -- "no proportional/star sizing and no auto-fill" -- so round-4's root
        // `Grid(rowHeights:[1,plotHeight], columnWidths:[width])` could never be more than `width` cells wide,
        // capping the waveform on a wider real terminal instead of filling it. DockPanel is the documented fill
        // primitive instead: it pins `headerRow` to the Top edge (a fixed 1-row strip) and gives the REST of
        // whatever space the parent offers to `plot`, whose own Width/Height default to 0 -- "fills the space the
        // parent offers" per Control.md's Width/Height remarks -- so the plot now fills AND re-fills on resize
        // with no further code; the framework re-lays-out automatically, we only had to pick a filling layout.
        headerBuiltForWidth = width;
        var headerRow = new Grid(rowHeights: [1], columnWidths: HeaderColumnWidths(width),
            controls: [[modeLabel, sourceLabel, infoLabel, moduleLabel, scaleLabel, spfLabel, fpsLabel, scatterLabel, pauseLabel]]);
        dock = new DockPanel(DockedControlPlacement.Top, headerRow, plot);
        SetContent(dock);
    }

    /// <summary>
    /// Pure computation: dispatches to <paramref name="mode"/> to build the series set, axis bounds/captions, and
    /// header fields from a config snapshot plus the mode's own state snapshot, its prior cross-frame accumulator
    /// (round-6; <see langword="null"/> the first time), and the current channel matrix. Touches no
    /// <see cref="Control"/> -- safe to call from a background thread. <paramref name="modeState"/> must be taken
    /// (via <see cref="IDisplayMode.Snapshot"/>) on the UI thread BEFORE crossing threads, exactly like
    /// <paramref name="g"/> and <paramref name="channels"/>/<paramref name="framerate"/> below; <paramref name="priorState"/>
    /// is whatever the PREVIOUS call's <see cref="ScopeFrame.NextModeState"/> was.
    /// </summary>
    /// <remarks>
    /// Round-10 (item 3): the gain-aware Oscilloscope special case is gone. <see cref="GraphSnapshot.Gain"/> now
    /// carries the round-6 calibration gain (see Program.cs, which sets <c>cfg.Gain</c>) on the SAME snapshot every
    /// mode already receives; <see cref="Oscilloscope.Process"/> (the <see cref="IDisplayMode.Process"/>
    /// implementation) reads <c>g.Gain</c> itself and folds it into its existing per-sample xs/ys loop (round-9,
    /// item 2), so this method calls the single uniform <see cref="IDisplayMode.Process"/> for every mode -- no
    /// <c>mode is Oscilloscope</c> concrete-type check, and no second gain-blind entry point on Oscilloscope.
    /// Vectorscope/Spectroscope ignore <c>Gain</c>, exactly like they already ignore other fields they do not need.
    /// </remarks>
    public static ScopeFrame ComputeFrame(GraphSnapshot g, IDisplayMode mode, object? modeState, object? priorState, double[][] channels, int framerate)
    {
        var references = g.References ? mode.References(g) : [];

        var (processed, nextState) = mode.Process(g, modeState, priorState, channels);


        var (xMin, xMax, yMin, yMax) = mode.AxisBounds(g, modeState);
        var (xCaption, yCaption) = mode.AxisCaptions(modeState);

        // Round-8 (item 2): a cheap value-tuple identifying whether the STATIC reference geometry could possibly
        // have changed since the last frame -- mode identity, the 'r' toggle, and the axis bounds it's drawn
        // against (Scale/Samples changes move e.g. the vectorscope's crosshair extent and the spectroscope's
        // decade-marker height). Equal key => ScopeView.Apply skips references entirely (no SetData, no rebuild).
        var referencesKey = (mode.GetType(), g.References, xMin, xMax, yMin, yMax, g.AxisColor);

        var m = mode.Header(modeState);

        return new ScopeFrame(
            References: references,
            Dynamic: processed,
            ReferencesKey: referencesKey,
            XMin: xMin,
            XMax: xMax,
            YMin: yMin,
            YMax: yMax,
            XCaption: xCaption,
            YCaption: yCaption,
            ModeText: $"{mode.ModeStr}",
            ModuleText: m == "live" ? "" : m,
            ScaleText: $"-{g.Scale:0.00}x+",
            SpfText: $"{g.Samples}/{g.Width} spf",
            FpsText: $"{framerate}fps",
            ScatterGlyph: g.Scatter ? "***" : "---",
            PauseGlyph: g.Pause ? "||" : "|>",
            Scatter: g.Scatter,
            ModeColor: g.Palette[0],
            LabelsColor: g.LabelsColor,
            AxisColor: g.AxisColor,
            ShowUi: g.ShowUi,
            NextModeState: nextState);
    }

    /// <summary>
    /// Applies a precomputed frame: Plot/header mutation only. Must run on the UI thread -- marshal with
    /// <see cref="Jumbee.Console.UI.Invoke(System.Action)"/>/<see cref="Jumbee.Console.UI.InvokeAsync(System.Action)"/>
    /// when calling from a background thread.
    /// </summary>
    public void Apply(ScopeFrame frame)
    {
        Error = null;
        ApplyCount++;

        // Round-5 (item 4, continued): the header Grid's column widths are still FIXED at construction (Grid.md
        // has no settable ColumnWidths -- there is no reflow API for a Grid), so on a real resize the header row
        // would stay pinned at its original nominal width while the plot below correctly re-fills. We work around
        // the missing API by rebuilding the header Grid (cheap: 7 cells) whenever the DockPanel's own ActualWidth
        // has changed since the last build, and reassigning it via DockPanel.DockedControl -- which DockPanel.md
        // documents as settable at runtime specifically "to swap the docked pane in place". This keeps the
        // header's percentage layout proportional across resizes even though Grid itself can't reflow.
        var currentWidth = ActualWidth;
        if (currentWidth > 0 && currentWidth != headerBuiltForWidth)
        {
            headerBuiltForWidth = currentWidth;
            dock.DockedControl = new Grid(rowHeights: [1], columnWidths: HeaderColumnWidths(currentWidth),
                controls: [[modeLabel, sourceLabel, infoLabel, moduleLabel, scaleLabel, spfLabel, fpsLabel, scatterLabel, pauseLabel]]);
        }

        ApplyLive(frame);

        // Pin the axis ranges AFTER ApplyLive. SetXRange/SetYRange record the fixed range in the plot's per-DATA
        // config, which a shape-rebuild's plot.Clear() (inside ApplyLive) wipes -- so applying them BEFORE ApplyLive
        // let a rebuild drop the pin, and the plot then auto-scaled to each frame's data (a jumpy axis that tracked
        // the music's amplitude). ApplyLive resets last*Min/Max to null on a rebuild, so this block re-pins after the
        // Clear; on an ordinary tick the pin is untouched and the gate skips (the bounds only move on a Scale/Samples
        // hotkey or a mode switch). (Axis/grid/tick COLOURS survive Clear -- they live in the plot's chrome list.)
        if (frame.XMin != lastXMin || frame.XMax != lastXMax)
        {
            plot.SetXRange(frame.XMin, frame.XMax);
            lastXMin = frame.XMin;
            lastXMax = frame.XMax;
        }
        if (frame.YMin != lastYMin || frame.YMax != lastYMax)
        {
            plot.SetYRange(frame.YMin, frame.YMax);
            lastYMin = frame.YMin;
            lastYMax = frame.YMax;
        }

        ApplyAxisAndHeader(frame);
    }

    /// <summary>
    /// Keeps persistent <see cref="PlotSeries"/> handles and only touches <see cref="PlotSeries.SetData"/> -- no
    /// <see cref="Plot.Clear"/>, no re-<c>Add*</c>, so no chrome replay -- on an ordinary tick where the series
    /// count/colours/scatter-vs-line haven't changed. See the class remarks: round-9 folds the old scatter-mode
    /// <c>ApplyRebuild</c> fallback into this single path via <c>Plot.AddLiveScatter</c>.
    /// </summary>
    void ApplyLive(ScopeFrame frame)
    {
        var shapeChanged = !usingLivePools || dynamicShape.Count != frame.Dynamic.Count;
        if (!shapeChanged)
            for (var i = 0; i < frame.Dynamic.Count; i++)
                if (dynamicShape[i].Color != frame.Dynamic[i].Color || dynamicShape[i].Scatter != frame.Dynamic[i].Scatter)
                { shapeChanged = true; break; }
        if (!shapeChanged && lastReferencesCount != frame.References.Count) shapeChanged = true;

        if (shapeChanged)
        {
            LiveShapeRebuilds++;
            plot.Clear();
            referencePool.Clear();
            dynamicPool.Clear();
            dynamicShape.Clear();
            // References are always drawn as connected lines (zero-line/crosshair/decade markers -- none of the
            // three IDisplayMode implementations ever set Scatter:true on a reference Series).
            foreach (var s in frame.References) referencePool.Add(plot.AddLiveSeries(s.Color));
            // Round-9: pick AddLiveScatter vs AddLiveSeries PER SERIES from Series.Scatter -- a single frame can
            // mix both (Oscilloscope's threshold/peaks markers are always Scatter:true regardless of the global
            // 's' toggle, while the waveform itself follows it), which is exactly why this is decided per-index
            // rather than once for the whole pool.
            foreach (var s in frame.Dynamic)
            {
                dynamicPool.Add(s.Scatter ? plot.AddLiveScatter(s.Color) : plot.AddLiveSeries(s.Color));
                dynamicShape.Add((s.Color, s.Scatter));
            }
            usingLivePools = true;
            lastReferencesKey = null; // force the references refresh below too
            lastReferencesCount = frame.References.Count;
            // plot.Clear() above wiped the pinned axis ranges (they live in the plot's per-data config, unlike the
            // chrome colours) -- null the caches so Apply's range block re-pins them after this rebuild instead of
            // gate-skipping (their values are unchanged) and leaving the plot to auto-scale to the data.
            lastXMin = lastXMax = lastYMin = lastYMax = null;
        }

        if (!Equals(lastReferencesKey, frame.ReferencesKey))
        {
            ReferencesRefreshCount++;
            for (var i = 0; i < frame.References.Count; i++) referencePool[i].SetData(frame.References[i].Xs, frame.References[i].Ys);
            lastReferencesKey = frame.ReferencesKey;
        }

        // PlotSeries.md: "Passing empty lists is valid and draws nothing" -- so, unlike round-8's now-deleted
        // ApplyRebuild (which had to skip zero-length series before calling the non-live AddSeries/AddScatter),
        // SetData needs no such guard; an empty series (e.g. Peaks off) just renders as nothing.
        for (var i = 0; i < frame.Dynamic.Count; i++) dynamicPool[i].SetData(frame.Dynamic[i].Xs, frame.Dynamic[i].Ys);
    }

    void ApplyAxisAndHeader(ScopeFrame frame)
    {
        // Round-9 (item 3): gate the WHOLE header update -- including, as of round-10 (item 4), the Plot chrome
        // colours (axis/grid/tick/caption) -- behind a value-equality check on a compact snapshot struct, instead
        // of reassigning every TextLabel.Text/FgColor (and reallocating a TextLabel[] literal to iterate over) on
        // every ordinary audio tick, where none of these fields actually change. Folding AxisColor/LabelsColor in
        // here (reviewer #3, pre-empting a stale-colour bug) means a colour-only change -- with no text change --
        // still trips the gate and repaints, instead of being silently dropped.
        var header = new HeaderSnapshot(frame.ModeText, frame.ModuleText, frame.ScaleText, frame.SpfText, frame.FpsText,
            frame.ScatterGlyph, frame.PauseGlyph, frame.ModeColor, frame.LabelsColor, frame.AxisColor, frame.ShowUi);

        // Axis caption TEXT is still tracked separately (it can change -- Spectroscope's 'l' toggle -- on a tick
        // where the header snapshot as a whole is unchanged) but now carries the caption COLOUR along too, so a
        // LabelsColor-only change also reaches SetAxisTitles even if the header gate above already returns early.
        // SetAxisTitles.md: "A null title is left unchanged" -- so hiding captions passes an explicit empty
        // string, not null, to actually clear them; null would leave whatever caption was showing before untouched.
        var xTitle = frame.ShowUi ? frame.XCaption : "";
        var yTitle = frame.ShowUi ? frame.YCaption : "";
        if (xTitle != lastXTitle || yTitle != lastYTitle || frame.LabelsColor != lastCaptionColor)
        {
            plot.SetAxisTitles(xTitle, yTitle, frame.LabelsColor);
            lastXTitle = xTitle;
            lastYTitle = yTitle;
            lastCaptionColor = frame.LabelsColor;
        }

        if (lastHeader == header) return;
        lastHeader = header;

        // The plot's axis/grid/tick colours used to be pushed here every time this gate tripped. They are now theme
        // tokens the Plot reads for itself, so this does nothing per frame -- and calling the setters would actively
        // hurt: each one MARKS a theme override, which would pin the plot to these values and make a runtime theme
        // switch a no-op for the very colours the theme is supposed to own.

        if (frame.ShowUi)
        {
            modeLabel.Text = frame.ModeText;
            modeLabel.FgColor = frame.ModeColor;
            // Constant for the run, but re-set here so the 'h' hide-UI toggle (which blanks every label) restores
            // them along with the per-frame ones -- ShowUi is part of the snapshot, so toggling it trips this gate.
            sourceLabel.Text = sourceText;
            infoLabel.Text = infoText;
            moduleLabel.Text = frame.ModuleText;
            scaleLabel.Text = frame.ScaleText;
            spfLabel.Text = frame.SpfText;
            fpsLabel.Text = frame.FpsText;
            scatterLabel.Text = frame.ScatterGlyph;
            pauseLabel.Text = frame.PauseGlyph;
            foreach (var l in uiLabels) l.FgColor = frame.LabelsColor;
        }
        else
        {
            foreach (var l in allLabels) l.Text = "";
        }
    }

    /// <summary>Surfaces a decode/compute error as visible UI state (header replaced with "ERROR: ...") instead of
    /// a silently dead feed or an unobserved faulted background task.</summary>
    public void SetError(string message)
    {
        Error = message;
        modeLabel.Text = "ERROR";
        modeLabel.FgColor = Color.Red1;
        moduleLabel.Text = message;
        moduleLabel.FgColor = Color.Red1;
        sourceLabel.Text = infoLabel.Text = "";
        scaleLabel.Text = spfLabel.Text = fpsLabel.Text = scatterLabel.Text = pauseLabel.Text = "";
        lastHeader = null; // force the next ordinary Apply() to repaint the header even if its fields look unchanged
    }

    /// <summary>
    /// Starts this pane's own self-driving compute feed: every <paramref name="interval"/>, on a background thread,
    /// it reads the latest decoded frame from the shared <paramref name="bus"/> and (if anything actually changed
    /// since its last tick) runs the pure <see cref="ComputeFrame"/> for its FIXED <paramref name="mode"/>, then
    /// marshals the cheap <see cref="Apply"/> onto the UI thread. Three panes each run one of these off the ONE
    /// <see cref="ChannelBus"/> -- the fan-out that lets a single audio source drive three independent scopes.
    /// </summary>
    /// <remarks>
    /// This is the whole point of the milestone: the heavy per-mode transform (the spectroscope's FFT, the
    /// oscilloscope's trigger scan) runs on each pane's OWN feed thread, so the three computes overlap instead of
    /// serializing, and only the mode-agnostic <see cref="Apply"/> touches the UI thread.
    /// <para/>Threading: <c>Control.Feed</c>'s loop never overlaps a feed's own <c>produce</c> with itself, so the
    /// per-mode cross-frame accumulator (the spectroscope's history) lives in a producer-local variable here --
    /// thread-confined to this one feed thread, no marshaling, no race. The shared knobs it reads are immutable
    /// versioned snapshots taken on the UI thread (<see cref="ChannelBus.Frame"/>, <see cref="GraphConfig.Current"/>),
    /// so a producer never sees a torn read. The mode's own hotkey knobs are simple scalars read live via
    /// <see cref="IDisplayMode.Snapshot"/> (a benign one-frame tear at worst, self-healing next tick).
    /// <para/>Idle-skip: when the bus version, the config version, and this mode's snapshot are all unchanged since
    /// the last computed frame, <c>produce</c> returns <see langword="null"/> and <c>Apply</c> is skipped -- so a
    /// pane does no work when nothing moved, yet a paused hotkey edit (which bumps the config version without
    /// advancing the audio) still re-renders. <paramref name="framerate"/> is read live (an atomic int) for the
    /// header's fps field. Cancellation/teardown are the usual <see cref="FeedHandle"/> contract (auto-cancel on
    /// <see cref="Control.Dispose"/>); the panes never touch the disposable reader, so only the pump gates its dispose.
    /// </remarks>
    public FeedHandle StartComputeFeed(ChannelBus bus, IDisplayMode mode, GraphConfig cfg, Func<int> framerate,
        TimeSpan interval, Action<Exception>? onError = null)
    {
        // Producer-local state, touched ONLY on this feed's background thread (which never overlaps its own produce).
        long lastChannelVersion = -1, lastConfigVersion = -1;
        object? lastModeSnapshot = null;
        object? accumulator = null;       // e.g. the spectroscope's FFT history -- threaded produce->produce, never crosses threads
        double[][]? frozenChannels = null; // the last live frame this pane computed with -- reused while paused

        return Feed<ScopeFrame?>(
            produce: () =>
            {
                var config = cfg.Current;
                var modeSnapshot = mode.Snapshot();
                var paused = config.Snapshot.Pause;

                // While paused this pane freezes on the last live frame; otherwise it follows the shared bus (the pump
                // keeps decoding regardless, since pause is per-pane). The version compared against is the channel
                // DATA's -- frozen data keeps its version, so a paused pane only re-renders on a config/mode change
                // (e.g. rescaling the frozen waveform), never on new audio.
                var busFrame = bus.Latest;
                var channels = paused ? frozenChannels : busFrame?.Channels;
                var channelVersion = paused ? lastChannelVersion : busFrame?.Version ?? -1;
                if (channels is null) return null; // no audio yet (or paused before any live frame)

                if (channelVersion == lastChannelVersion && config.Version == lastConfigVersion
                    && Equals(modeSnapshot, lastModeSnapshot))
                    return null; // nothing changed since our last frame -- idle, no recompute, no repaint

                lastChannelVersion = channelVersion;
                lastConfigVersion = config.Version;
                lastModeSnapshot = modeSnapshot;
                if (!paused) frozenChannels = channels; // remember the live frame so a later pause freezes on it

                var computed = ComputeFrame(config.Snapshot, mode, modeSnapshot, accumulator, channels, framerate());
                accumulator = computed.NextModeState;
                return computed;
            },
            apply: computed => { if (computed is { } f) Apply(f); },
            interval, onError);
    }

    /// <summary>
    /// Global-help entry for the mode-agnostic keys every scope-tui mode shares. Mode-specific keys (oscilloscope's
    /// trigger/edge/peaks/threshold/depth; spectroscope's window/log-y/average; vectorscope has none) are appended
    /// by <see cref="Control.OnHelp"/> in Program.cs, since only the app (not this reusable control) knows which
    /// mode is currently active.
    /// </summary>
    protected override HelpInfo? GetHelpInfo() =>
        new HelpInfo("Scope", "scope-tui", "One scope pane (oscilloscope, vectorscope, or spectroscope) of three shown together. Click a pane or Tab to focus it; these keys act on the focused pane.")
            .WithKey("Up/Down", "Zoom the vertical scale (amplitude) in/out")
            .WithKey("Left/Right", "More/fewer samples per frame (time window)")
            .WithKey("Shift/Ctrl/Alt + arrow", "Same as above, larger/smaller step size")
            .WithKey("Esc", "Reset scale and samples to defaults")
            .WithKey("Space", "Pause/resume the live feed")
            .WithKey("s", "Toggle scatter-plot rendering (points instead of connected lines)")
            .WithKey("r", "Toggle reference lines (zero line / crosshair / decade markers)")
            .WithKey("h", "Hide/show the header and axis captions")
            .WithKey("Tab / Shift+Tab", "Move focus between the three scope panes")
            .WithKey("q / Ctrl+C / Ctrl+Q / Ctrl+W", "Quit");
}
