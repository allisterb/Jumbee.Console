namespace Jumbee.Console.AgentHarnessDemo;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using Jumbee.Console;

using Spectre.Console;
using Spectre.Console.Rendering;

// Both namespaces define a Style; the demo's controls mean the Jumbee semantic style.
using Style = Jumbee.Console.Style;

/// <summary>The top-right "task list" pane: an animated vertical checklist of agent steps with status glyphs
/// (done / active-spinner / pending / failed). The <c>AgentSimulator</c> mutates a step's <see cref="AgentStep.Status"/>
/// then calls <see cref="Refresh"/>.</summary>
internal sealed class TaskListView : RenderableControl, IScrollable
{
    #region Constructors
    public TaskListView(string title = "")
    {
        _title = title ?? string.Empty;
        Focusable = false;
        _spinnerFrames = Spectre.Console.Spinner.Known.Dots.Frames;
        ApplyTheme();
        // Advance the spinner only while a step is Active, so the pane settles (no repaint) once work stops.
        _animation = Feed(() => { if (HasActive) { _frame++; Invalidate(); } }, 120);
    }
    #endregion

    #region Properties
    /// <summary>Bold header line drawn at the top of the pane.</summary>
    public string Title
    {
        get => _title;
        set { _title = value ?? string.Empty; Invalidate(); }
    }
    #endregion

    #region Methods
    /// <summary>Appends a step and re-lays-out so a surrounding frame re-measures our height. Returns the handle
    /// so the caller can flip its <see cref="AgentStep.Status"/> and call <see cref="Refresh"/>.</summary>
    public AgentStep AddStep(string text, int indent = 0)
    {
        var step = new AgentStep(text, indent);
        UI.Invoke(() => { _steps.Add(step); Initialize(); Invalidate(); });
        return step;
    }

    /// <summary>Re-renders after a caller mutated a step's <see cref="AgentStep.Status"/> (row count unchanged).</summary>
    public void Refresh() => Invalidate();

    /// <summary>Completes the first Active step and promotes the next Pending step to Active. Returns <see langword="true"/>
    /// if a pending step was promoted, <see langword="false"/> when the checklist is finished — a demo helper for
    /// walking the list forward one beat at a time.</summary>
    public bool AdvanceStep()
    {
        var promoted = false;
        UI.Invoke(() =>
        {
            foreach (var s in _steps) if (s.Status == StepStatus.Active) s.Status = StepStatus.Done;
            foreach (var s in _steps)
                if (s.Status == StepStatus.Pending) { s.Status = StepStatus.Active; promoted = true; break; }
            Invalidate();
        });
        return promoted;
    }

    /// <summary>Removes all steps and re-lays-out.</summary>
    public void Clear() => UI.Invoke(() => { _steps.Clear(); Initialize(); Invalidate(); });

    protected override bool RendersInteractiveState => false;

    protected override void ApplyTheme()
    {
        _titleStyle = (Style)Palette.Text | Style.Bold;
        _mutedStyle = Palette.TextMuted;
        _faintStyle = Palette.TextFaint;
    }

    // Measure at the fixed wide LayoutWidth so heights are width-INDEPENDENT — long lines clip rather than wrap, which
    // avoids the layout convergence loop a width-dependent height causes in a scrolling frame (see ListBox).
    public int MeasureHeight(int width)
    {
        var options = new RenderOptions(ansiConsole.Profile.Capabilities, new Spectre.Console.Size(LayoutWidth, 1));
        return Math.Max(1, Segment.SplitLines(((IRenderable)BuildRows()).Render(options, LayoutWidth)).Count);
    }

    protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
        => ((IRenderable)BuildRows()).Render(options, LayoutWidth);

    // Title, a blank spacer row, then one single-line row per step — assembled as a Spectre Rows and rendered at the
    // fixed wide width so each row is its explicit single line (matching MeasureHeight).
    private Rows BuildRows()
    {
        var rows = new List<IRenderable>(_steps.Count + 2);
        if (_title.Length > 0)
        {
            rows.Add(_titleStyle[_title]);   // Style indexer → a Markup with the text escaped
            rows.Add(new Text(" "));
        }
        foreach (var step in _steps) rows.Add(RenderStep(step));
        return new Rows(rows);
    }

    private IRenderable RenderStep(AgentStep step)
    {
        var (glyph, glyphStyle, textStyle) = Visual(step);
        var sb = new StringBuilder();
        sb.Append(' ', step.Indent * 2);
        Append(sb, glyphStyle, glyph);
        sb.Append(' ');
        Append(sb, textStyle, step.Text);
        // Sub-steps (Indent == 1) read as a muted roll-up with a trailing disclosure chevron.
        if (step.Indent >= 1) { sb.Append(' '); Append(sb, _faintStyle, "›"); }
        return new Markup(sb.ToString());
    }

    private static void Append(StringBuilder sb, Style style, string text) =>
        sb.Append('[').Append(style.ToMarkup()).Append(']').Append(Markup.Escape(text)).Append("[/]");

    // Glyph + glyph/text styles for a step. Sub-steps (Indent >= 1) are wholly muted regardless of status; top-level
    // steps colour by status: Done ✓ green/muted, Active spinner coral/bright, Pending ○ faint, Failed ✗ red.
    private (string glyph, Style glyphStyle, Style textStyle) Visual(AgentStep step)
    {
        var glyph = step.Status switch
        {
            StepStatus.Done => "✓",   // ✓
            StepStatus.Failed => "✗", // ✗
            StepStatus.Active => _spinnerFrames.Count > 0 ? _spinnerFrames[_frame % _spinnerFrames.Count] : "•",
            _ => "○",                 // ○
        };

        if (step.Indent >= 1) return (glyph, _mutedStyle, _mutedStyle);

        return step.Status switch
        {
            StepStatus.Done => (glyph, (Style)Palette.Green, _mutedStyle),
            StepStatus.Active => (glyph, (Style)Palette.Coral, (Style)Palette.Text),
            StepStatus.Failed => (glyph, (Style)Palette.Red, (Style)Palette.Red),
            _ => (glyph, _faintStyle, _faintStyle),
        };
    }

    private bool HasActive
    {
        get
        {
            foreach (var step in _steps) if (step.Status == StepStatus.Active) return true;
            return false;
        }
    }
    #endregion

    #region Fields
    private readonly List<AgentStep> _steps = new();
    private readonly IReadOnlyList<string> _spinnerFrames;
    private readonly FeedHandle _animation;
    private string _title;
    private int _frame;
    private Style _titleStyle;
    private Style _mutedStyle;
    private Style _faintStyle;
    // Fixed wide width rows are measured and rendered at, so heights are width-independent (see MeasureHeight).
    private const int LayoutWidth = 1000;
    #endregion
}
