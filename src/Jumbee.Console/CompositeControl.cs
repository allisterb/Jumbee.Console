namespace Jumbee.Console;

using System;
using System.Collections.Generic;
using System.Linq;

using ConsoleGUI.Common;
using ConsoleGUI.Data;
using ConsoleGUI.Space;

/// <summary>
/// Base class for <em>composite</em> controls: a <see cref="Control"/> that owns and lays out several child
/// controls and presents them as a single control. It is a real <see cref="Control"/> (so it has its own
/// console buffer, participates in theming/painting, can be framed, and drops into any layout cell), but its
/// content is an internal <see cref="ILayout"/> arranging the children.
/// </summary>
/// <remarks>
/// The children are composited through a <see cref="DrawingContext"/> over the internal layout (the same
/// mechanism <see cref="ControlFrame"/> uses for its single child): each child keeps its own buffer and renders
/// itself, and its cells — carrying its own mouse listener — are surfaced through this control's indexer, so
/// mouse hit-testing and click-to-focus reach the children unchanged. Keyboard input routes in via
/// <see cref="FocusedControl"/>, which returns the focused descendant.
/// <para>
/// Subclasses build their child controls and an arranging layout in their constructor, wire any inter-child
/// behaviour (e.g. <c>editor.Changed += …</c>), and call <see cref="SetContent"/>. The composite draws no content
/// of its own by default; override <see cref="Control.Render"/> to paint a background/chrome into the buffer
/// behind the children.
/// </para>
/// </remarks>
public abstract class CompositeControl : Control, IDrawingContextListener
{
    #region Constructors
    /// <summary>Initializes a new <see cref="CompositeControl"/> with no content; the subclass calls <see cref="SetContent"/> after building its children.</summary>
    protected CompositeControl() : base() { }

    /// <summary>Initializes a new <see cref="CompositeControl"/> with the given <paramref name="content"/> layout arranging its children.</summary>
    protected CompositeControl(ILayout content) : base() => SetContent(content);
    #endregion

    #region Properties
    /// <summary>The internal layout arranging the child controls (set via <see cref="SetContent"/>).</summary>
    protected ILayout? Content => _content;

    /// <summary>The internal layout, exposed so the parent's input routing can deliver keys through it — running any
    /// nested layout tunnels (e.g. a <see cref="TabPanel"/>'s Alt+arrow tab switching) on the way to the focused
    /// child, which a direct <see cref="FocusedControl"/> dispatch would skip.</summary>
    internal ILayout? ContentLayout => _content;

    /// <summary>
    /// Returns the focused descendant so keyboard input routed by the parent layout reaches the right child;
    /// falls back to the composite itself when it is focused and no child is.
    /// </summary>
    public override IFocusable? FocusedControl
    {
        get
        {
            if (_content is not null)
            {
                foreach (var c in _content.Controls)
                {
                    if (c?.FocusedControl is { } focused) return focused;
                }
            }
            return base.FocusedControl;
        }
    }

    /// <summary>A composite is an opaque focusable leaf to the navigation/routing layer: it reports that it handles
    /// input (keys reach its focused child via <see cref="FocusedControl"/>) so <c>ILayout.Leaves</c> treats it as a
    /// single navigable unit rather than descending into its children.</summary>
    public override bool HandlesInput => true;

    /// <summary>The child that receives focus when the composite is focused (and input/caret follow). Defaults to the
    /// child focus was last requested for (a clicked field, or a <see cref="MoveFocusToChild"/> step), else the first
    /// focusable child; override to choose a different default.</summary>
    protected virtual Control? FocusChild => _focusChild ?? _focusables.FirstOrDefault();

    /// <summary>The focusable children, in layout order — the stops <see cref="MoveFocusToChild"/> walks.</summary>
    protected IReadOnlyList<Control> Focusables => _focusables;

    /// <summary>When <see langword="true"/>, Tab / Shift+Tab move focus between the composite's focusable children
    /// (cycling within it) instead of reaching the focused child. Off by default: Tab belongs to the focused control
    /// (a <see cref="TextEditor"/> indents with it). Turn it on for a form — several fields the user tabs between.</summary>
    protected virtual bool TabNavigatesChildren => false;
    #endregion

    #region Indexers
    /// <summary>Gets the composited <see cref="Cell"/> at <paramref name="position"/> — a child's cell (keeping its mouse listener) where a child covers it, otherwise the composite's own surface.</summary>
    public override Cell this[Position position]
    {
        get
        {
            // A child covers this cell: return it as-is so it keeps the child's own mouse listener (in child
            // coordinates), letting ConsoleManager route hover/click to the child and click-to-focus work.
            if (_contentContext.Contains(position))
            {
                var childCell = _contentContext[position];
                // If the covering child isn't itself focusable/mouse-listening (its cell carries no listener) but
                // THIS composite opts into the mouse (WantsMouse), attach our own listener so a click anywhere on
                // the composite still reaches it — click-to-focus for a display composite (e.g. a plot pane) whose
                // children aren't focusable. A child that carries its own listener keeps it unchanged.
                return childCell.MouseListener is null && WantsMouse
                    ? childCell.WithMouseListener(this, position)
                    : childCell;
            }

            // Otherwise the composite's own surface (whatever Render drew into the buffer, e.g. a background).
            if (position.X >= 0 && position.Y >= 0 && position.X < Size.Width && position.Y < Size.Height)
            {
                var cell = consoleBuffer[position];
                return WantsMouse ? cell.WithMouseListener(this, position) : cell;
            }
            return emptyCell;
        }
    }
    #endregion

    #region Methods
    /// <summary>
    /// Sets the internal layout that arranges the children.
    /// </summary>
    /// <remarks>
    /// Call once from the subclass constructor after building the child controls and the layout, and after wiring
    /// any inter-child event handlers.
    /// </remarks>
    protected void SetContent(ILayout content)
    {
        _content = content;
        if (!ReferenceEquals(_contentContext, DrawingContext.Dummy)) _contentContext.Dispose();
        _contentContext = new DrawingContext(this, content.CControl);
        foreach (var f in content.Controls) SetOwnership(f);
        Initialize();
    }

    // The composite is the navigable focus unit; route focus inward to a child so keyboard input (via
    // FocusedControl) and the child's caret/selection follow. A no-op when a descendant is already focused (e.g. a
    // child claimed by a more specific path). Overriding FocusChild changes which child this picks.
    /// <summary>Routes focus inward to <see cref="FocusChild"/> when the composite gains focus.</summary>
    protected override void Control_OnFocus()
    {
        if (FocusChild is { } child && !child.IsFocused) child.IsFocused = true;
    }

    // Clear the focused descendant when the composite loses focus (covers the direct UnFocus path; UI.SetFocus
    // already clears registered children when focus moves elsewhere).
    /// <summary>Clears the focused descendant when the composite loses focus.</summary>
    protected override void Control_OnLostFocus()
    {
        if (FocusedControl is Control child && !ReferenceEquals(child, this)) child.IsFocused = false;
    }

    /// <summary>
    /// A first look at each key on its way to the focused child, mirroring a layout's <c>InterceptInput</c> tunnel —
    /// so a composite can define its own navigation keys.
    /// </summary>
    /// <remarks>
    /// Return <see langword="true"/> to consume the key. The base handles Tab / Shift+Tab when
    /// <see cref="TabNavigatesChildren"/> is set.
    /// </remarks>
    protected virtual bool InterceptInput(UI.InputEventArgs inputEventArgs)
    {
        if (!TabNavigatesChildren || inputEventArgs.InputEvent is not { Key: var key } || key.Key != ConsoleKey.Tab)
            return false;
        return MoveFocusToChild((key.Modifiers & ConsoleModifiers.Shift) != 0 ? -1 : +1);
    }

    /// <summary>Moves focus to the next (<c>+1</c>) or previous (<c>-1</c>) focusable child, wrapping. Returns
    /// <see langword="false"/> when there are fewer than two to move between.</summary>
    protected bool MoveFocusToChild(int direction)
    {
        if (_focusables.Count < 2) return false;
        var from = _focusables.FindIndex(c => c.IsFocused);
        var to = ((Math.Max(from, 0) + direction) % _focusables.Count + _focusables.Count) % _focusables.Count;
        if (from >= 0) _focusables[from].IsFocused = false;
        _focusables[to].IsFocused = true;
        _focusChild = _focusables[to];
        return true;
    }

    // The routing layer's entry to the tunnel above (Layout.OnInput, and ControlFrame for a framed composite).
    internal bool RouteInterceptInput(UI.InputEventArgs inputEventArgs) => InterceptInput(inputEventArgs);

    // Remembers the child focus was requested for, so Control_OnFocus delegates to it rather than the first child.
    // Called by UI.SetFocus (click-to-focus and Control.Focus both resolve a child up to its composite).
    internal void OnChildFocusRequest(Control child)
    {
        _focusChild = child;
        if (!IsFocused) return;   // not focused yet: Control_OnFocus applies FocusChild when focus arrives
        // Already focused, so SetFocus's own `IsFocused = true` is a no-op and won't re-run Control_OnFocus —
        // move focus between the children here instead.
        if (FocusedControl is Control current && !ReferenceEquals(current, child)) current.IsFocused = false;
        child.IsFocused = true;
    }

    // Claim focusable descendants as ours so focus resolves to this composite (the navigable unit); they become the
    // focus stops, the first being the default FocusChild. Stops at nested composites (they own their own children).
    // Runs at construction before layout, so it walks structure rather than laid-out leaves (it can't depend on
    // HasLayout).
    private void SetOwnership(IFocusable? node)
    {
        switch (node)
        {
            case null or CompositeControl: return;                   // a nested composite owns its own children
            case ILayout nested: foreach (var c in nested.Controls) SetOwnership(c); return;
            case ControlFrame frame: SetOwnership(frame.Control); return;
            case Control control when control.Focusable:
                control.Owner ??= this;
                if (!_focusables.Contains(control)) _focusables.Add(control);
                return;
        }
    }

    // Children render themselves into their own buffers; the composite paints nothing by default. Subclasses may
    // override to draw a background/chrome behind the children (the indexer composites children over the buffer).
    /// <summary>Renders the composite's own surface; the default draws nothing (children render themselves). Override to paint a background or chrome behind the children.</summary>
    protected override void Render() { }

    // Runs on the UI thread after Control.Initialize has resized the composite (Control subscribes this to its
    // OnInitialization event). Size the internal layout to fill the composite's current area.
    /// <summary>Sizes the internal content layout to fill the composite's current area after initialization.</summary>
    protected override void Control_OnInitialization()
    {
        base.Control_OnInitialization();
        _contentContext.SetOffset(new Vector(0, 0));
        _contentContext.SetLimits(Size, Size);
    }

    void IDrawingContextListener.OnRedraw(DrawingContext drawingContext) => Initialize();

    // Propagate the child's damaged region upward (translated into this composite's coordinates by the
    // DrawingContext) instead of invalidating the whole composite. The composite draws its children live through its
    // indexer, so re-compositing just that region this frame reads the child's fresh buffer — no need to repaint the
    // composite's own (static) chrome. Escalating to Invalidate() instead deferred the child's change to the
    // composite's *next* repaint, which showed up as a one-frame input lag once the renderer stopped doing a full
    // redraw every frame. A subclass whose chrome depends on child state can still override this to Invalidate().
    void IDrawingContextListener.OnUpdate(DrawingContext drawingContext, Rect rect) => Update(rect);

    /// <summary>Disposes the content drawing context and the base control.</summary>
    public override void Dispose()
    {
        if (!ReferenceEquals(_contentContext, DrawingContext.Dummy)) _contentContext.Dispose();
        base.Dispose();
    }
    #endregion

    #region Fields
    private ILayout? _content;
    private DrawingContext _contentContext = DrawingContext.Dummy;
    // The focusable descendants claimed in SetContent, in layout order — the focus stops. The first is the default
    // FocusChild (e.g. a CodeEditor's editor).
    private readonly List<Control> _focusables = [];
    // The child focus was last requested for (clicked, or tabbed to); overrides the default FocusChild.
    private Control? _focusChild;
    #endregion
}
