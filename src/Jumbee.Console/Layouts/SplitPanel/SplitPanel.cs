namespace Jumbee.Console;

using System;

/// <summary>How a <see cref="SplitPanel"/> arranges its two panes.</summary>
public enum SplitOrientation
{
    /// <summary>Panes side by side (first = left, second = right), separated by a vertical divider.</summary>
    Horizontal,

    /// <summary>Panes stacked (first = top, second = bottom), separated by a horizontal divider.</summary>
    Vertical
}

/// <summary>
/// A container that splits its area between two panes with a draggable divider between them.
/// </summary>
/// <remarks>
/// The first pane has a fixed extent (<see cref="SplitPosition"/>, in cells) and the second fills the rest, so
/// resizing the container keeps the first pane put and grows/shrinks the second. Resize by dragging the divider, or by
/// focusing it and pressing the arrow keys (Shift = larger step). Nest split panels for richer layouts (e.g. a sidebar
/// beside a vertically-split editor/terminal). Composes with the same focus-routing model as <see cref="TabPanel"/>.
/// </remarks>
public class SplitPanel : Layout<SplitPanelDockPanel>
{
    #region Constructors
    /// <summary>Initializes a new <see cref="SplitPanel"/> splitting <paramref name="first"/> and <paramref name="second"/> along <paramref name="orientation"/>, with the first pane given <paramref name="splitPosition"/> cells.</summary>
    public SplitPanel(SplitOrientation orientation, IFocusable first, IFocusable second, int splitPosition = 20)
        : base(new SplitPanelDockPanel(orientation, first.RenderNode(), second.RenderNode(),
            Math.Max(DefaultMin, splitPosition)))
    {
        _orientation = orientation;
        _first = first;
        _second = second;
        _divider = control.Divider;
        _splitPosition = Math.Max(DefaultMin, splitPosition);
        _divider.Dragged += delta => SplitPosition += delta;
        _divider.Nudged += delta => SplitPosition += delta;
    }
    #endregion

    #region Events
    /// <summary>Raised after <see cref="SplitPosition"/> changes, with the new first-pane extent.</summary>
    public event EventHandler<int>? SplitChanged;
    #endregion

    #region Properties
    /// <summary>How the two panes are arranged (fixed at construction).</summary>
    public SplitOrientation Orientation => _orientation;

    /// <summary>The draggable divider (for theming or focusing).</summary>
    public SplitDivider Divider => _divider;

    /// <summary>The first pane (left/top).</summary>
    public IFocusable First => _first;

    /// <summary>The second pane (right/bottom).</summary>
    public IFocusable Second => _second;

    /// <summary>The first pane's extent in cells (width for a horizontal split, height for a vertical one).</summary>
    /// <remarks>Clamped to <see cref="MinFirst"/> and to leaving the divider plus <see cref="MinSecond"/> for the
    /// second pane; raises <see cref="SplitChanged"/> when it actually changes. Set it to <see cref="MinFirst"/> to
    /// collapse the first pane to a sliver (a "focus"/zen toggle); save the previous value and restore it to expand
    /// again — the simplest runtime layout change, since it's just a resize.</remarks>
    public int SplitPosition
    {
        get => _splitPosition;
        set
        {
            var total = _orientation == SplitOrientation.Horizontal ? control.Size.Width : control.Size.Height;
            // Once laid out, don't let the first pane crowd out the divider + the second pane's minimum.
            var max = total > 0 ? total - 1 - _minSecond : int.MaxValue;
            var clamped = Math.Clamp(value, _minFirst, Math.Max(_minFirst, max));
            if (clamped == _splitPosition) return;
            _splitPosition = clamped;
            ApplyExtent();
            SplitChanged?.Invoke(this, clamped);
        }
    }

    // Applies the divider position to the layout. A drag delivers several mouse-moves per frame, and each
    // SetFirstExtent triggers a full subtree re-layout of the first pane (Boundary.Initialize -> every control
    // resizes and re-renders). Coalesce to one re-layout per frame: the model (_splitPosition) is already updated
    // synchronously by the caller, so only the layout apply is deferred to the next dispatcher drain, collapsing a
    // frame's worth of moves into a single re-layout. Headless (no running loop) applies inline so a following
    // render/read reflects it immediately — which is what the tests rely on.
    private void ApplyExtent()
    {
        if (!UI.IsRunning) { control.SetFirstExtent(_splitPosition); return; }
        if (_extentDirty) return;
        _extentDirty = true;
        UI.Post(() => { _extentDirty = false; control.SetFirstExtent(_splitPosition); });
    }

    /// <summary>Minimum extent of the first pane in cells (default 3). Clamped to at least <c>1</c>, so
    /// <see cref="SplitPosition"/> can never reach <c>0</c> — a "fully collapsed" first pane is really a 1-cell
    /// sliver (set <c>MinFirst = 1</c> for the thinnest zen collapse).</summary>
    public int MinFirst
    {
        get => _minFirst;
        set { _minFirst = Math.Max(1, value); SplitPosition = _splitPosition; }
    }

    /// <summary>Minimum extent of the second pane in cells (default 3).</summary>
    public int MinSecond
    {
        get => _minSecond;
        set { _minSecond = Math.Max(1, value); SplitPosition = _splitPosition; }
    }

    // Logical children for input routing (like TabPanel): first pane, divider, second pane. Both panes and the
    // divider are focusable; the divider takes arrow-key resizes when focused. Laid out along the split axis — a
    // horizontal (side-by-side) split exposes them as columns, a vertical (stacked) split as rows — so spatial focus
    // navigation moves between the panes with the matching arrow keys (Ctrl+←/→ vs Ctrl+↑/↓).
    private bool IsHorizontal => _orientation == SplitOrientation.Horizontal;

    /// <summary>Number of rows in the layout grid (3 for a vertical split's stacked panes+divider, otherwise 1).</summary>
    public override int Rows => IsHorizontal ? 1 : 3;

    /// <summary>Number of columns in the layout grid (3 for a horizontal split's side-by-side panes+divider, otherwise 1).</summary>
    public override int Columns => IsHorizontal ? 3 : 1;

    /// <summary>Gets the logical child at the given <paramref name="row"/> and <paramref name="column"/>: first pane, divider, or second pane.</summary>
    public override IFocusable this[int row, int column]
    {
        get
        {
            var (index, cross) = IsHorizontal ? (column, row) : (row, column);
            if (cross != 0) throw new ArgumentOutOfRangeException(IsHorizontal ? nameof(row) : nameof(column));
            return index switch
            {
                0 => _first,
                1 => _divider,
                2 => _second,
                _ => throw new ArgumentOutOfRangeException(IsHorizontal ? nameof(column) : nameof(row))
            };
        }
    }

    // Surface the focused descendant so a parent layout routing through this single IFocusable reaches it.
    /// <inheritdoc/>
    public override IFocusable? FocusedControl => _first.FocusedControl ?? _divider.FocusedControl ?? _second.FocusedControl;
    #endregion

    #region Fields
    private const int DefaultMin = 3;
    private readonly SplitOrientation _orientation;
    private readonly IFocusable _first;
    private readonly IFocusable _second;
    private readonly SplitDivider _divider;
    private int _splitPosition;
    private int _minFirst = DefaultMin;
    private int _minSecond = DefaultMin;
    private bool _extentDirty;   // a coalesced SetFirstExtent is queued for the next frame (see ApplyExtent)
    #endregion
}
