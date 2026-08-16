namespace Jumbee.Console.AgentHarnessDemo;

using System;
using System.Collections.Generic;
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

    #region Events
    /// <inheritdoc/>
    public event EventHandler<RowSpan>? FocusRowChanged;
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
    public void Refresh() => UI.Invoke(() => { Invalidate(); ReportActiveRow(); });

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
            ReportActiveRow();
        });
        return promoted;
    }

    // The pane is deliberately shorter than the checklist, so the step being worked on can sit below the fold.
    // Reporting its row lets the surrounding frame keep it in view as the work advances — the same mechanism a list
    // uses for its selected item. Must run on the UI thread: it reads the step collection.
    private void ReportActiveRow()
    {
        for (var i = 0; i < _steps.Count; i++)
        {
            if (_steps[i].Status == StepStatus.Active)
            {
                FocusRowChanged?.Invoke(this, new RowSpan(TitleRows + i));
                return;
            }
        }
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

    // Every row is exactly one line (see Render), so the height is arithmetic — no need to build and render the whole
    // list just to count the lines it produced, which is what this used to do on every layout pass.
    public int MeasureHeight(int width) => Math.Max(1, TitleRows + _steps.Count);

    // Emits styled segments directly instead of composing a markup STRING and letting Spectre parse it back. The
    // string round-trip cost a Style.ToMarkup + Color.ToMarkup on the way out and a MarkupTokenizer + StyleParser +
    // ParseHexColor on the way back in, per fragment per frame — together the single largest source of allocation in
    // this demo. A Segment already IS text plus a Style, so there is nothing to serialise.
    protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        if (TitleRows > 0)
        {
            yield return new Segment(_title, _titleStyle);
            yield return Segment.LineBreak;
            yield return Segment.LineBreak;   // the blank spacer row under the title
        }

        for (var i = 0; i < _steps.Count; i++)
        {
            foreach (var segment in RenderStep(_steps[i])) yield return segment;
            if (i < _steps.Count - 1) yield return Segment.LineBreak;
        }
    }

    private IEnumerable<Segment> RenderStep(AgentStep step)
    {
        var (glyph, glyphStyle, textStyle) = Visual(step);
        if (step.Indent > 0) yield return new Segment(new string(' ', step.Indent * 2));
        yield return new Segment(glyph, glyphStyle);
        yield return new Segment(" ");
        yield return new Segment(step.Text, textStyle);
        // Sub-steps (Indent == 1) read as a muted roll-up with a trailing disclosure chevron.
        if (step.Indent >= 1)
        {
            yield return new Segment(" ");
            yield return new Segment("›", _faintStyle);
        }
    }

    // The title line plus its blank spacer, or nothing when untitled.
    private int TitleRows => _title.Length > 0 ? 2 : 0;

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
