namespace Jumbee.Console;

using System;
using System.Diagnostics;

using Spectre.Console.Rendering;

using S = Spectre.Console;

/// <summary>
/// A translucent "glass" HUD showing live UI telemetry — render, terminal-write and queue-wait times (µs), CPU,
/// working set, allocation rate and, the headline for a no-lock design, monitor lock contentions — floating over
/// the app.
/// </summary>
/// <remarks>
/// <para>The panel is frosted glass (the app shows through as soft tinted smudges, not raw glyphs); the readout is
/// drawn crisply on top; it refreshes itself a few times a second while shown.</para>
/// <para>Timing comes from <see cref="UI.AverageDrawTime"/>/<see cref="UI.AveragePaintTime"/>; process metrics
/// are read directly from <see cref="Process"/>/<see cref="GC"/>/<see cref="System.Threading.Monitor"/> and differenced across
/// refreshes, so no external sampling has to be running. Show it with <see cref="GlassPanel.Show"/> /
/// <see cref="ShowTopRight"/>, toggle with <see cref="GlassPanel.Toggle"/> or <see cref="RegisterToggle"/>.</para>
/// </remarks>
public sealed class PerfHud : GlassPanel
{
    #region Constructors
    /// <param name="tint">Glass colour the app beneath is tinted toward.</param>
    /// <param name="factor">Blend strength (0 = clear, 1 = opaque tint).</param>
    /// <param name="frosted">Frost the app beneath to a colour blur (clean readout, content shows as soft smudges)
    /// rather than letting its raw glyphs bleed through and clutter the readout.</param>
    public PerfHud(Color? tint = null, float factor = 0.6f, bool frosted = true)
        : base(HudWidth, HudHeight, tint ?? new Color(44, 54, 82), factor, frosted)
    {
        Refresh();
        UI.Paint += OnHudPaint;
    }
    #endregion

    #region Methods
    /// <summary>Floats the HUD in the top-right corner of the current UI, <paramref name="margin"/> cells in from
    /// the edges.</summary>
    public void ShowTopRight(int margin = 1, Overlay? overlay = null)
    {
        var ov = overlay ?? UI.Overlay
            ?? throw new InvalidOperationException("No overlay is available to host the HUD; start the UI first.");
        var w = ov.Bottom.CControl.Size.Width;
        Show(Math.Max(margin, w - HudWidth - margin), margin, ov);
    }

    /// <summary>Registers a global hotkey (default <c>Ctrl+G</c>) that toggles the HUD in the top-right corner over
    /// the ambient <see cref="UI.Overlay"/>. Call once after <see cref="UI.Start"/>.</summary>
    public void RegisterToggle(ConsoleKeyInfo? key = null, int margin = 1)
        => UI.RegisterHotKey(key ?? UI.HotKeys.Ctrl(ConsoleKey.G), () => { if (IsShown) Hide(); else ShowTopRight(margin); });

    /// <summary>Rebuilds the telemetry readout from the current metrics. Called automatically while shown.</summary>
    public void Refresh()
    {
        Content = Build(out var rows);
        // Size to what was actually emitted rather than to a constant someone has to remember to update: a panel
        // one row short silently drops its last metric instead of failing.
        var needed = rows + PanelBorderRows;
        if (Height != needed) Height = needed;
    }

    private static IRenderable Build(out int rowCount)
    {
        var m = UI.ProcessMetrics;
        // "render"/"busy" are the high-resolution per-frame RENDER cost (peak-over-window) — near-0 for retained
        // rendering, which is the point. "cpu" is whole-process (matches Task Manager); it captures work outside the
        // render cycle (input, dispatcher, other threads) that the per-frame numbers don't.
        // render/busy show the AVERAGE (the typical frame — low for retained rendering) with the PEAK as a separate
        // row (the worst frame in the window — a resize/paste burst).
        // Named "render", not "frame": it is the UI-thread work that ENDS when the ANSI bytes are built. The write to
        // the terminal is the separate "write" row below, and the two overlap — see the note there.
        // Averaged over the frames that actually DREW, not over all frames. An idle frame costs almost nothing, so
        // including them scales the number down by the redraw rate — and then "render" no longer adds up with the
        // write/wait rows into the latency below it, which is confusing precisely when the numbers matter.
        double renderUs = m.RenderTimeMsDrawnAvg * 1000.0;
        double renderPeakUs = m.RenderTimeMsPeak * 1000.0;
        double busy = m.BusyPercentAvg;
        double busyPeak = m.BusyPercentPeak;
        // Fraction of frames that took the full draw path (vs idled) — a retained UI keeps this low.
        double redraw = m.RedrawPercent;
        // Fraction of the SCREEN re-composited per drawn frame — dirty-rect rendering keeps this tiny (a status-bar
        // tick redraws only its own rows), spiking to 100 only on resize/theme switch.
        double dirty = m.DirtyAreaPercentAvg;
        double dirtyPeak = m.DirtyAreaPercentPeak;
        double cpu = m.CpuUsagePercent;
        // mem is a sticky gauge: the average tracks the current footprint and the peak is the window high-water mark.
        double memMb = m.WorkingSetBytesAvg / 1048576.0;
        double memPeakMb = m.WorkingSetBytesPeak / 1048576.0;
        // Average = the steady per-frame allocation (near-zero for retained rendering, even at fullscreen); peak =
        // the worst single frame in the window (a resize/paste burst). Showing both makes "is it flat" obvious.
        double allocKb = m.AllocatedBytesPerFrame / 1024.0;
        double allocPeakKb = m.PeakAllocatedBytesPerFrame / 1024.0;
        double exc = m.ExceptionsPerSecond;
        // A rate, not the lifetime total: the total only ever climbs, so a few contentions during startup left the
        // dagger showing red for the rest of the run even on a completely quiet UI.
        double locks = m.LockContentionsPerSecond;
        // The terminal write happens off the render loop, so none of the numbers above include it. It runs CONCURRENT
        // with the next frame, so it doesn't add to "render" — the throughput ceiling is whichever is larger. queue is
        // the frames rendered but not yet written: 1 is healthy, a climbing depth means the terminal is the limiter
        // and the display is behind what has been drawn.
        double writeUs = UI.AverageWriteTime * 1000.0;
        double waitUs = UI.AverageWriteWaitTime * 1000.0;
        // The PEAK backlog over the window, not the instant depth: the queue drains between frames, so an instant
        // reading is 0 almost every time even while frames are demonstrably waiting (a non-zero "wait" proves it).
        int queuePeak = UI.WriteQueueDepthPeak;
        // End-to-end: render + wait + write, summed PER FRAME (the write carries its frame's ordinal home, so the two
        // halves are the same frame's). LATENCY, not a frame budget — the write overlaps the next frame's render, so
        // this is deliberately NOT what caps the frame rate. The peak is worth having on its own: it is the worst a
        // frame took to reach the screen, which no combination of separate averages could tell you.
        double latencyUs = UI.AverageFrameLatency * 1000.0;
        double latencyPeakUs = UI.PeakFrameLatency * 1000.0;

        var g = new S.Grid();
        g.AddColumn(new S.GridColumn { Padding = new S.Padding(0, 0, 2, 0) });
        g.AddColumn();
        // Every row goes through Row() so the panel's height is derived from what was actually emitted. The height
        // used to be a hand-kept constant, and when it fell behind the row count the last metric was simply clipped
        // away — no error, just a number silently missing from the readout.
        int rows = 0;
        void Row(string label, string value)
        {
            g.AddRow(new S.Markup($"[grey62]{label}[/]"), new S.Markup(value));
            rows++;
        }
        // Each metric on one row: the AVERAGE (the typical/steady value) in bright ink, then the PEAK — the worst
        // frame in the window (a resize/paste burst) — dimmed after a slash. redraw/cpu are single gauges.
        // The three µs timings are kept adjacent so they read as one group: render (UI-thread work) then write and
        // wait (the terminal side, concurrent with the next frame). busy follows as the percentage view of render.
        Row("render", $"[#e8f0ff]{renderUs,5:F0} µs[/] [grey50]/ {renderPeakUs:F0}[/]");
        Row("write", $"[#e8f0ff]{writeUs,5:F0} µs[/] [grey50]/ q{queuePeak}[/]");
        Row("wait", $"[#e8f0ff]{waitUs,5:F0} µs[/]");
        Row("latency", $"[#e8f0ff]{latencyUs,5:F0} µs[/] [grey50]/ {latencyPeakUs:F0}[/]");
        Row("busy", $"[#e8f0ff]{busy,5:F0} %[/] [grey50]/ {busyPeak:F0}[/]");
        Row("redraw", $"[#e8f0ff]{redraw,5:F0} %[/]");
        Row("dirty", $"[#e8f0ff]{dirty,5:F1} %[/] [grey50]/ {dirtyPeak:F0}[/]");
        Row("cpu", $"[#e8f0ff]{cpu,5:F1} %[/]");
        Row("mem", $"[#e8f0ff]{memMb,5:F1} MB[/] [grey50]/ {memPeakMb:F0}[/]");
        Row("alloc", $"[#e8f0ff]{allocKb,5:F1} KB/f[/] [grey50]/ {allocPeakKb:F0}[/]");
        Row("exc/s", exc > 0 ? $"[bold #ff6b6b]{exc,5:F0}[/]" : "[#e8f0ff]    0[/]");
        // The dagger: a no-lock UI design holds contention at zero. Green 0 when true, red rate otherwise — and it
        // returns to green once contention stops, which a lifetime total never could.
        Row("locks", locks <= 0 ? "[bold #7CFC00]0 ✓[/]" : $"[bold #ff6b6b]{locks,5:F1}/s[/]");

        rowCount = rows;
        return new S.Panel(g)
        {
            Border = S.BoxBorder.Rounded,
            Padding = new S.Padding(1, 0, 1, 0),
            Expand = true,
            Header = new S.PanelHeader("[#8fd0ff] ◈ perf · glass [/]"),
            BorderStyle = new S.Style(foreground: S.Color.SkyBlue1),
        };
    }

    private void OnHudPaint(object? sender, UI.PaintEventArgs e)
    {
        // Only sample metrics / rebuild the readout while the HUD is actually on screen. OnHudPaint stays subscribed
        // to UI.Paint for the control's whole lifetime (ctor→Dispose), so without this guard Build() — a Process/GC/
        // Monitor sample plus a fresh Grid/Panel/Markup graph — and its subsequent Spectre render (GetSegments) run
        // ~4×/s from launch even while hidden. _refresh keeps ticking while hidden, so the first paint after Show()
        // is already past RefreshMs and refreshes immediately (no staleness on show).
        if (!IsShown) return;
        if (_refresh.ElapsedMilliseconds >= RefreshMs)
        {
            _refresh.Restart();
            Refresh();
        }
    }

    /// <summary>Unsubscribes from the paint loop and releases the base control's resources.</summary>
    public override void Dispose()
    {
        UI.Paint -= OnHudPaint;
        base.Dispose();
    }
    #endregion

    #region Fields
    private const int HudWidth = 34;
    // The panel's top and bottom border rows, which sit outside the metric rows.
    private const int PanelBorderRows = 2;
    // Only the height the panel is CONSTRUCTED at; Refresh immediately re-derives it from the rows Build emitted,
    // so adding or removing a metric needs no change here.
    private const int HudHeight = 13;
    private const long RefreshMs = 250;
    private readonly Stopwatch _refresh = Stopwatch.StartNew();
    #endregion
}
