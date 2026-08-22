namespace Jumbee.Console;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using ConsoleGUI;
using ConsoleGUI.Common;
using ConsoleGUI.Data;
using ConsoleGUI.Input;
using ConsoleGUI.Space;
using Spectre.Console.Interop;

/// <summary>Common interface for Jumbee.Console layout classes: a 2-D grid of focusable cells over a ConsoleGUI control, with focus navigation and input routing.</summary>
public interface ILayout : IFocusable, IDrawingContextListener
{
    /// <summary>The number of rows in the layout grid.</summary>
    int Rows { get; }

    /// <summary>The number of columns in the layout grid.</summary>
    int Columns { get; }

    /// <summary>The underlying ConsoleGUI control this layout wraps.</summary>
    IControl CControl { get; }

    /// <summary>Gets the focusable at the given grid cell.</summary>
    IFocusable this[int row, int column] { get; }

    /// <summary>The focusables in every grid cell, in row-major order.</summary>
    IEnumerable<IFocusable> Controls { get; }

    // ---- Focus navigation -------------------------------------------------------------------------------------
    // Spatial/region focus navigation over this layout's 2-D cell grid (Rows/Columns/this[r,c]). These compute the
    // focusable to move to; applying focus stays a UI concern (UI.SetFocus). Hoisted here from UI so any layout can
    // drive the same navigation, not only the root.

    /// <summary>Indexer access that tolerates an empty slot (a sparse Grid cell can throw from the underlying
    /// indexer); returns <see langword="null"/> in that case.</summary>
    IFocusable? CellAt(int row, int column)
    {
        try { return this[row, column]; } catch { return null; }
    }

    /// <summary>The (row, column) of the cell whose subtree currently holds focus, or <see langword="null"/> if none.</summary>
    (int Row, int Column)? FocusedCell()
    {
        for (var r = 0; r < Rows; r++)
            for (var c = 0; c < Columns; c++)
                if (CellAt(r, c)?.FocusedControl is not null) return (r, c);
        return null;
    }

    /// <summary>Computes the focusable one cell in the given direction from the focused cell (wraps per axis; skips
    /// empty cells), or <see langword="null"/> if none. Pass a row/column delta (e.g. (0, -1) = left). Landing on a
    /// cell descends to its first focusable leaf.</summary>
    IFocusable? SpatialTarget(int dRow, int dCol) => SpatialTarget(dRow, dCol, wrap: true);

    /// <summary>
    /// The directional move, made recursive so arrows cross cells nested several layouts deep (e.g. panes inside
    /// nested split panels).
    /// </summary>
    /// <remarks>
    /// It first descends into the focused nested layout and lets it move within itself; that nested move never
    /// wraps, so at the nested edge it returns <see langword="null"/> and we step at this level instead — the arrow
    /// "exits" the nested layout to the parent's sibling cell. The top-level call wraps per axis (the
    /// region-to-region behavior); nested calls do not.
    /// </remarks>
    IFocusable? SpatialTarget(int dRow, int dCol, bool wrap)
    {
        if (FocusedCell() is { } focused && CellAt(focused.Row, focused.Column) is ILayout nested
            && nested.SpatialTarget(dRow, dCol, wrap: false) is { } inner)
            return inner;

        int rows = Rows, cols = Columns;
        if (rows <= 0 || cols <= 0) return null;
        var (r, c) = FocusedCell() ?? (0, 0);

        if (wrap)
        {
            for (var step = 0; step < rows * cols; step++)   // walk in the given direction, wrapping, until a focusable cell
            {
                r = ((r + dRow) % rows + rows) % rows;
                c = ((c + dCol) % cols + cols) % cols;
                if (FirstLeaf(CellAt(r, c)) is { } target) return target;
            }
            return null;
        }

        for (r += dRow, c += dCol; r >= 0 && r < rows && c >= 0 && c < cols; r += dRow, c += dCol)   // to the edge, no wrap
            if (FirstLeaf(CellAt(r, c)) is { } target) return target;
        return null;
    }

    /// <summary>Computes the next (<paramref name="direction"/> &gt; 0) or previous focusable within the currently
    /// focused region, relative to <paramref name="current"/>, wrapping — or <see langword="null"/>.</summary>
    /// <remarks>A no-op (null) unless the focused cell is a multi-focusable nested layout (enter/leave a single
    /// control or composite via the spatial arrows instead).</remarks>
    IFocusable? RegionCycleTarget(int direction, IFocusable? current)
    {
        if (FocusedCell() is not { } cell) return null;
        if (CellAt(cell.Row, cell.Column) is not ILayout region) return null;

        var ring = Leaves(region).ToList();
        if (ring.Count <= 1) return null;
        var index = ring.FindIndex(f => ReferenceEquals(f, current));
        var next = index < 0
            ? (direction > 0 ? 0 : ring.Count - 1)
            : ((index + direction) % ring.Count + ring.Count) % ring.Count;
        return ring[next];
    }

    /// <summary>The first focusable leaf reachable from <paramref name="node"/> (see <see cref="Leaves"/>), or null.</summary>
    static IFocusable? FirstLeaf(IFocusable? node) => Leaves(node).FirstOrDefault();

    /// <summary>The focusable leaves reachable from <paramref name="node"/>, in order: descends nested layouts and
    /// frames; a leaf is an interactive, laid-out Control (Focusable + HandlesInput + HasLayout). A composite is an
    /// opaque leaf here (it reports HandlesInput) and manages its own children's focus internally.</summary>
    static IEnumerable<IFocusable> Leaves(IFocusable? node)
    {
        if (node is null) yield break;
        if (node is ILayout nested)
        {
            for (var r = 0; r < nested.Rows; r++)
                for (var c = 0; c < nested.Columns; c++)
                    foreach (var f in Leaves(nested.CellAt(r, c))) yield return f;
        }
        else if (node is ControlFrame frame)
        {
            foreach (var f in Leaves(frame.Control)) yield return f;
        }
        else if (node is Control control && control.Focusable && control.HandlesInput && control.HasLayout)
        {
            yield return control;
        }
    }
}

/// <summary>Base class for Jumbee.Console layouts wrapping a ConsoleGUI layout control <typeparamref name="T"/> and exposing it through <see cref="ILayout"/>.</summary>
public abstract class Layout<T> : ILayout where T:CControl, IDrawingContextListener
{
    #region Constructors
    /// <summary>Initializes a new <see cref="Layout{T}"/> wrapping the given ConsoleGUI <paramref name="control"/>.</summary>
    protected Layout(T control)
    {
        this.control = control;
    }
    #endregion

    #region Indexers
    /// <summary>Gets the focusable at the given grid cell.</summary>
    public abstract IFocusable this[int row, int column] { get; }
    #endregion

    #region Properties
    /// <summary>The number of rows in the layout grid.</summary>
    public abstract int Rows { get; }

    /// <summary>The number of columns in the layout grid.</summary>
    public abstract int Columns { get; }

    /// <summary>Gets the composited <see cref="Cell"/> at <paramref name="position"/> from the wrapped control.</summary>
    public Cell this[Position position] => control[position];

    /// <summary>The wrapped control's laid-out size.</summary>
    public Size Size => control.Size;

    /// <summary>The underlying ConsoleGUI control this layout wraps.</summary>
    public IControl CControl => control;

    /// <summary>The wrapped control's drawing context.</summary>
    public IDrawingContext Context
    {
        get => ((IControl) control).Context;
        set => ((IControl)control).Context = value;
    }

    /// <summary>The focusables in every grid cell, in row-major order.</summary>
    public IEnumerable<IFocusable> Controls
    {
        get
        {
            // Through CellAt, and skipping the empties: a Grid cell may hold nothing, and walking every coordinate
            // blind threw straight out of the underlying grid's GetChild. CellAt already exists for exactly this and
            // documents it; Controls was simply the one walker that did not use it. Anything consuming Controls —
            // focus routing, input dispatch, the composite's child list — wants the controls that are there, not a
            // sequence with holes in it.
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Columns; c++)
                {
                    if (((ILayout)this).CellAt(r, c) is { } cell) yield return cell;
                }
            }
        }
    }

    /// <summary>When <see langword="true"/> (the default), this layout can hold focus.</summary>
    public bool Focusable { get; set; } = true;

    /// <summary>The focus target the UI registers for this layout — the layout itself.</summary>
    public IFocusable FocusableControl => this;

    /// <summary>Whether this layout holds focus; setting it raises the focus events.</summary>
    public bool IsFocused
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                if (value)
                    OnFocus?.Invoke();
                else
                    OnLostFocus?.Invoke();
            }
        }
    }

    /// <summary>Always <see langword="true"/> — a layout routes input to its focused descendant.</summary>
    public bool HandlesInput => true;

    /// <summary>
    /// The focused descendant within this layout (or <see langword="null"/>), so a parent can tell that this layout
    /// is on the focus path and route input into it.
    /// </summary>
    /// <remarks>
    /// Walks <see cref="Controls"/> for the focused leaf; this is what lets keyboard input — and each ancestor
    /// layout's tunnel (<see cref="InterceptInput"/>) — reach a control even when the layout is nested several
    /// levels deep.
    /// </remarks>
    public virtual IFocusable? FocusedControl
    {
        get
        {
            foreach (var f in Controls)
                if (f?.FocusedControl is { } focused) return focused;
            return Focusable && IsFocused ? FocusableControl : null;
        }
    }
    #endregion

    #region Events
    /// <summary>Raised when the layout gains focus.</summary>
    public event FocusableEventHandler? OnFocus;

    /// <summary>Raised when the layout loses focus.</summary>
    public event FocusableEventHandler? OnLostFocus;
    #endregion

    #region Methods
    /// <inheritdoc/>
    public void OnRedraw(DrawingContext drawingContext) => control.OnRedraw(drawingContext);

    /// <inheritdoc/>
    public void OnUpdate(DrawingContext drawingContext, Rect rect) => control.OnUpdate(drawingContext, rect);

    /// <summary>Routes a UI input event: this layout's tunnel gets a first look, then the event is delivered to the focused descendant.</summary>
    public void OnInput(UI.InputEventArgs inputEventArgs)
    {
        // Tunnel phase: let this layout consume the input before it routes down (e.g. an Overlay closing on its
        // CloseKey, or a TabPanel switching tabs on Alt+arrows). A consumed key is marked handled so ancestor
        // layouts stop routing it. This runs for every layout on the focus path — the root always, and each nested
        // layout reached below — so a layout can define its own navigation keys even when deeply nested.
        if (InterceptInput(inputEventArgs))
        {
            if (inputEventArgs.InputEvent is { } consumed) consumed.Handled = true;
            return;
        }

        // Deliver to the focused descendant. For a nested layout, recurse through its OwnInput so it gets its own
        // tunnel pass (and routes on to its focused child); for a leaf/frame/composite, dispatch via FocusedControl.
        // Cells may be null (an empty slot in a custom layout), so guard. Stop once the event is handled.
        foreach (var f in Controls)
        {
            if (f is ILayout nested)
            {
                if (nested.FocusedControl is not null) nested.OnInput(inputEventArgs);
            }
            else if (f is CompositeControl composite && composite.ContentLayout is { } content)
            {
                // Let the composite's own tunnel consume the key first (e.g. a form tabbing between its fields),
                // then route through its internal layout so nested tunnels (e.g. a TabPanel's Alt+arrow tab
                // switching) run before the key reaches the focused child — a direct FocusedControl dispatch would
                // skip them.
                if (composite.FocusedControl is not null)
                {
                    if (composite.RouteInterceptInput(inputEventArgs))
                    {
                        if (inputEventArgs.InputEvent is { } consumed) consumed.Handled = true;
                        return;
                    }
                    content.OnInput(inputEventArgs);
                }
            }
            else
            {
                f?.FocusedControl?.OnInput(inputEventArgs);
            }
            if (inputEventArgs.InputEvent?.Handled == true) return;
        }
    }

    /// <summary>Lets a layout intercept input before it routes to the focused control. Return true if handled.</summary>
    protected virtual bool InterceptInput(UI.InputEventArgs inputEventArgs) => false;

    /// <summary>Forwards a bracketed-paste payload to each cell's focused descendant.</summary>
    public void OnPaste(string text) => Controls.ForEach(f => f?.FocusedControl?.OnPaste(text));
    #endregion

    #region Fields
    /// <summary>The wrapped ConsoleGUI layout control.</summary>
    public readonly T control;
    #endregion
}
