namespace ScopeTui;

using System;
using System.Collections.Generic;

using Jumbee.Console;

using Spectre.Console.Rendering;

/// <summary>
/// The always-on footer: the values that belong to the RUN on the left — input device, channel count, feed overlap,
/// paint rate — and the global key hints on the right.
/// </summary>
/// <remarks>
/// These four were previously repeated in all three pane headers, which was both redundant and expensive: the
/// vectorscope is about a fifth of the width and still had to fit nine columns. Everything a pane hotkey can change
/// (scale, samples, trigger, averaging, scatter, pause) stays in that pane's own header; everything here is either
/// fixed for the run or global to it, so one copy is the honest number of copies.
/// <para>Only the hints for keys that are genuinely GLOBAL are listed. The per-pane keys (space, s, r, t, w, l,
/// PageUp/PageDown, the arrows) act on whichever pane has focus and differ between panes, so they belong in F1's
/// per-pane help rather than on a shared line that cannot say which pane it means.</para>
/// </remarks>
public sealed class ScopeStatusBar : RenderableControl
{
    #region Constructors
    /// <param name="source">Where the audio comes from, e.g. <c>device:live/Microphone Array</c>.</param>
    /// <param name="channels">Channel count of the source.</param>
    public ScopeStatusBar(string source, int channels)
    {
        _source = source;
        _channels = channels > 0 ? $"{channels}ch" : "";
        Focusable = false;   // display only: never a Tab stop, and it owns no hotkeys of its own
        ApplyTheme();
    }
    #endregion

    #region Methods
    /// <summary>Sets the overlap readout; blank when there is none, so a default run says nothing about it.</summary>
    public double Overlap { set { _overlap = value; Invalidate(); } }

    /// <summary>Sets the paint-rate readout. This is the UI loop's rate, NOT how often the audio advances — the feed
    /// is derived from --buffer and --overlap (see Program.cs).</summary>
    public int Framerate { set { if (value != _framerate) { _framerate = value; Invalidate(); } } }

    /// <summary>One row, always — the footer never grows, and saying so is what keeps a docked parent from handing
    /// it the whole layout (a Control's default height means "fill what the parent offers").</summary>
    protected override int IntrinsicHeight() => 1;

    /// <inheritdoc/>
    protected override void ApplyTheme() => _style = UI.StyleTheme.TextMuted | UI.StyleTheme.Surface;

    /// <inheritdoc/>
    protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        var width = Math.Max(1, maxWidth);

        var left = $" {_source}";
        if (_channels.Length > 0) left += $"  {_channels}";
        // Threshold rather than > 0: the feed period is a whole number of milliseconds, so even a run that asked for
        // no overlap re-uses ~1% of each frame (a 46ms tick against a 46.44ms window). That is real but it is
        // quantisation, not a setting, and reporting it would imply the run was configured for it.
        if (_overlap >= 0.02) left += $"  overlap:{_overlap * 100:0}";
        left += $"  {_framerate}fps";

        // Right-align the hints when there is room; otherwise concatenate and let the line clip.
        var line = left.Length + Hints.Length + 2 <= width
            ? left.PadRight(width - Hints.Length) + Hints
            : left + "  " + Hints;
        line = line.Length < width ? line.PadRight(width) : line[..width];

        yield return new Segment(line, _style.SpectreConsoleStyle);
    }
    #endregion

    #region Fields
    // Global keys only. Pane switching here is Tab/Shift+Tab rather than the examples browser's Ctrl+arrows, because
    // Ctrl+arrows are already the x5 magnitude tier for this app's scale/samples adjustment.
    const string Hints = "F1 help · Tab pane · [ ] overlap · Ctrl+T theme · q quit ";

    readonly string _source;
    readonly string _channels;
    double _overlap;
    int _framerate;
    Style _style;
    #endregion
}
