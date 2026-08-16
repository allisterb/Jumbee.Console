namespace Jumbee.Console;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using ConsoleGUI.Data;
using ConsoleGUI.Input;
using ConsoleGUI.Space;

/// <summary>
/// Base class for all Jumbee.Console controls.
/// </summary>
public abstract class Control : CControl, IFocusable, IDisposable, IMouseListener, IMouseWheelListener
{
    #region Constructors
    /// <summary>Initializes a new <see cref="Control"/>, creating its render buffers and wiring up paint, theme, focus, and initialization handlers.</summary>
    public Control() : base()
    {
        consoleBuffer = new ConsoleBuffer();
        ansiConsole = new AnsiConsoleBuffer(consoleBuffer);
        UI.Paint += OnPaint;
        UI.ThemeChanged += OnThemeChanged;
        OnInitialization += Control_OnInitialization;
        OnFocus += Control_OnFocus;
        OnLostFocus += Control_OnLostFocus;
        CaptureFocusCue();
    }

   
    #endregion

    #region Indexers
    /// <summary>Gets the composited <see cref="Cell"/> at <paramref name="position"/>, applying the default focus cue and attaching this control as the cell's mouse listener where applicable.</summary>
    public override Cell this[Position position]
    {
        get
        {
            if (position.X >= Size.Width || position.Y >= Size.Height)
            {
                return emptyCell;
            }
            else
            {
                var cell = consoleBuffer[position];
                // Default focus cue: make keyboard focus visible on a control that isn't showing it another way.
                // Drawn here at composite time — no reflow, no re-render — like the composite/dialog background fills.
                if (ShowsDefaultFocusCue) cell = ApplyFocusCue(cell, position);
                // Attach this control as the cell's mouse listener so mouse events inside it are routed here
                // (click-to-focus, hover, click). ConsoleGUI's mouse system hit-tests via the cell's listener.
                return Focusable || WantsMouse ? cell.WithMouseListener(this, position) : cell;
            }
        }
    }

    // True when this control should paint the themed default focus cue: it is focused, doesn't render its own focus
    // indication, and no frame is already showing the cue — either there's no frame, or the frame is borderless (a
    // visible-border frame recolours its border on focus, so we defer to it).
    private bool ShowsDefaultFocusCue =>
        IsFocused && !RendersOwnFocus && (Frame is null || !Frame.ShowsFocusCue);

    // Applies the themed focus cue to a cell, per the captured FocusStyle: Tint fills every unpainted cell, Ring
    // fills only the outer-edge cells, Underline underlines the bottom row. Tint/Ring need a focus colour; Underline
    // needs none. Preserves the cell's mouse listener.
    private Cell ApplyFocusCue(Cell cell, Position position)
    {
        var ch = cell.Character;
        switch (_focusStyle)
        {
            case FocusStyle.Underline:
                if (position.Y == Size.Height - 1)
                    ch = new Character(ch.Content, ch.Foreground, ch.Background,
                        (ch.Decoration ?? Decoration.None) | Decoration.Underline, ch.IsCursor);
                break;
            case FocusStyle.Ring:
                var onEdge = position.X == 0 || position.Y == 0 || position.X == Size.Width - 1 || position.Y == Size.Height - 1;
                if (onEdge && ch.Background is null && _focusTint is { } ring) ch = ch.WithBackground(ring);
                break;
            default:   // Tint
                if (ch.Background is null && _focusTint is { } tint) ch = ch.WithBackground(tint);
                break;
        }
        return new Cell(ch, cell.MouseListener);
    }
    #endregion

    #region Properties
    /// <summary>The requested width in cells; setting it resizes the control. 0 (the default) fills the space the parent offers.</summary>
    public virtual int Width
    {
        get => field;
        set
        {
            UI.Invoke(() => 
            {                
                field = value;            
                Resize(new Size(value, Height));
            });
        }
    }

    /// <summary>The control's actual laid-out width in cells.</summary>
    public int ActualWidth => Size.Width;

    /// <summary>The requested height in cells; setting it resizes the control. 0 (the default) fills the space the parent offers.</summary>
    public virtual int Height
    {
        get => field;
        set
        {
            UI.Invoke(() => 
            {
                field = value;
                Resize(new Size(Width, value));
            });
        }
    }

    /// <summary>The control's actual laid-out height in cells.</summary>
    public int ActualHeight => Size.Height;

    /// <summary><see langword="true"/> once the control has a non-empty laid-out size.</summary>
    public bool HasLayout => ActualWidth > 0 && ActualHeight > 0;

    /// <summary>The optional <see cref="ControlFrame"/> drawing borders, margins, scrollbars, and a titlebar around this control, or <see langword="null"/>.</summary>
    public ControlFrame? Frame
    {
        get => field;
        set
        {
            if (ReferenceEquals(field, value)) return;
            // The frame participates in runtime theme switches (UI.ThemeChanged) and reflects focus moving into/out of
            // its subtree (UI.FocusChanged, for framed composites — see ControlFrame.OnFocusChanged). Manage both
            // subscriptions here so their lifecycle follows attachment (and a replaced frame is detached).
            if (field is not null) { UI.ThemeChanged -= field.OnThemeChanged; UI.FocusChanged -= field.OnFocusChanged; }
            field = value;
            if (value is not null) { UI.ThemeChanged += value.OnThemeChanged; UI.FocusChanged += value.OnFocusChanged; }
        }
    }

    /// <summary><see langword="true"/> when this control has a <see cref="Frame"/>.</summary>
    public bool HasFrame => Frame is not null;

    /// <summary>The composite that owns this control as one of its children (set by <see cref="CompositeControl"/>),
    /// or null for a top-level control. Lets focus resolve to the composite — the navigable unit — so clicking or
    /// navigating to a child focuses the composite, which then delegates focus back to the child.</summary>
    internal CompositeControl? Owner { get; set; }

    /// <summary>The outermost navigable focus unit for this control: the topmost owning composite, or itself.</summary>
    internal Control FocusRoot => Owner?.FocusRoot ?? this;

    /// <summary>When <see langword="true"/> (the default), this control can receive keyboard focus.</summary>
    public bool Focusable { get; set; } = true;

    /// <summary>Whether this control currently holds keyboard focus; setting it raises the focus events and repaints so the terminal cursor moves.</summary>
    public bool IsFocused
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                Frame?.IsFocused = value;
                if (value)
                    OnFocus?.Invoke();
                else
                    OnLostFocus?.Invoke();
                // Notify framed composites so an ancestor frame can light/clear its border when focus moves into or
                // out of its subtree — its own IsFocused doesn't change, and the Owner chain doesn't reach it for a
                // dynamically-added descendant, so a global signal is the reliable trigger.
                UI.RaiseFocusChanged();
                // Repaint so RenderCursor runs for both the old and new focus: only the focused control owns the
                // terminal cursor, so the defocused one must clear its IsCursor cell.
                InvalidateInteractive();
            }
        }
    }

    /// <summary>The focus target the UI registers for this control — its <see cref="Frame"/> when framed (so the frame handles input routing), otherwise the control itself.</summary>
    public IFocusable FocusableControl => this.Frame is not null ? this.Frame : this;

    /// <summary>
    /// The focused control within this one — itself (<em>not</em> its <see cref="FocusableControl"/>/frame, so a
    /// frame forwarding input inward doesn't loop back to itself) when focused, otherwise <see langword="null"/>.
    /// </summary>
    /// <remarks>
    /// A composite (<see cref="CompositeControl"/>) overrides this to return its focused descendant, letting
    /// keyboard input route through the composite to the right child.
    /// </remarks>
    public virtual IFocusable? FocusedControl => Focusable && IsFocused ? this : null;

    /// <summary>When <see langword="true"/>, this control processes keyboard input dispatched to it; the default (<see langword="false"/>) ignores it.</summary>
    public virtual bool HandlesInput { get; } = false;

    /// <summary>Dispatches a UI input event to <see cref="OnInput(InputEvent)"/> when <see cref="HandlesInput"/> is set.</summary>
    public void OnInput(UI.InputEventArgs inputEventArgs)
    {
        // Input is dispatched on the UI thread (the input reader posts it there), so no lock is needed.
        if (HandlesInput)
        {
            this.OnInput(inputEventArgs.InputEvent!);
        }
    }

    /// <summary>Handles a keyboard input event; override on input-handling controls. The default is a no-op.</summary>
    protected virtual void OnInput(InputEvent inputEvent) {}

    /// <summary><see langword="true"/> while the pointer is over this control (between enter and leave).</summary>
    protected bool IsMouseOver { get; private set; }

    /// <summary><see langword="true"/> while a press started on this control and has not yet been released.</summary>
    protected bool IsMousePressed { get; private set; }

    /// <summary>
    /// When <see langword="true"/>, the control's cells are tagged with a mouse listener even if it is not
    /// <see cref="Focusable"/>, so it still receives hover/click (e.g. a non-focusable clickable Link).
    /// </summary>
    protected virtual bool WantsMouse => false;

    /// <summary>
    /// When <see langword="true"/>, this control indicates keyboard focus in its own way (e.g. a button's fill
    /// change, a tab's underline, an editor's cursor), so the base class does <em>not</em> paint the themed default
    /// focus tint over it.
    /// </summary>
    /// <remarks>
    /// Override and return <see langword="true"/> on controls with their own focus styling; the
    /// default (<see langword="false"/>) gives unstyled focusable controls an automatic, always-visible focus cue.
    /// </remarks>
    protected virtual bool RendersOwnFocus => false;

    #region Mouse hooks (override to react; defaults are no-ops)
    /// <summary>Called when the pointer enters the control.</summary>
    protected virtual void OnMouseEnter() {}
    /// <summary>Called when the pointer leaves the control.</summary>
    protected virtual void OnMouseLeave() {}
    /// <summary>Called as the pointer moves within the control (relative position).</summary>
    protected virtual void OnMouseMove(Position position) {}
    /// <summary>Called when a button is pressed over the control (relative position).</summary>
    protected virtual void OnMousePress(Position position) {}
    /// <summary>Called when a button is released over the control (relative position).</summary>
    protected virtual void OnMouseRelease(Position position) {}
    /// <summary>Called on a press+release on this control (relative position).</summary>
    protected virtual void OnClick(Position position) {}
    /// <summary>Called on two clicks within <see cref="DoubleClickMs"/> at the same position.</summary>
    protected virtual void OnDoubleClick(Position position) {}

    /// <summary>
    /// Handles a wheel notch over the control (<paramref name="delta"/>: negative up, positive down). Default
    /// scrolls the surrounding <see cref="Frame"/> if there is one; override to consume the wheel directly.
    /// </summary>
    protected virtual void OnMouseWheel(Position position, int delta) => Frame?.Scroll(delta);

    /// <summary>
    /// Scrolls the surrounding <see cref="Frame"/> so content rows <paramref name="start"/> through
    /// <paramref name="start"/> + <paramref name="height"/> are visible. A no-op when unframed or already visible.
    /// </summary>
    /// <remarks>
    /// Nothing scrolls on its own, so an <see cref="IScrollable"/> whose selection, caret or focus can move must call
    /// this when it does — otherwise the interesting row can sit outside the viewport with the cue drawn where it
    /// cannot be seen. Rows are in this control's own content coordinates, the space
    /// <see cref="IScrollable.MeasureHeight"/> measures.
    /// </remarks>
    protected void ScrollIntoView(int start, int height = 1) => Frame?.ScrollIntoView(start, height);

    /// <summary>
    /// Grabs the mouse so this control receives all subsequent move/press/release (in its own frame) until
    /// <see cref="ReleaseMouse"/>, even when the pointer leaves its cells — for drags (a splitter divider, a
    /// scrollbar thumb, a slider). Call from <see cref="OnMousePress"/>; pair with <see cref="ReleaseMouse"/> in
    /// <see cref="OnMouseRelease"/>.
    /// </summary>
    protected void CaptureMouse() => ConsoleGUI.ConsoleManager.CaptureMouse(this);

    /// <summary>Releases a capture taken by <see cref="CaptureMouse"/>.</summary>
    protected void ReleaseMouse() => ConsoleGUI.ConsoleManager.ReleaseMouseCapture();
    #endregion

    #region IMouseListener (dispatch sink: ConsoleManager calls these on the UI thread)
    void IMouseListener.OnMouseEnter()
    {
        IsMouseOver = true;
        OnMouseEnter();
        MouseEntered?.Invoke(this, EventArgs.Empty);
        InvalidateInteractive();
    }

    void IMouseListener.OnMouseLeave()
    {
        IsMouseOver = false;
        IsMousePressed = false;
        OnMouseLeave();
        MouseLeft?.Invoke(this, EventArgs.Empty);
        InvalidateInteractive();
    }

    void IMouseListener.OnMouseMove(Position position)
    {
        OnMouseMove(position);
        MouseMoved?.Invoke(this, position);
    }

    void IMouseListener.OnMouseDown(Position position)
    {
        IsMousePressed = true;
        if (Focusable) UI.SetFocus(this);
        OnMousePress(position);
        MousePressed?.Invoke(this, position);
        InvalidateInteractive();
    }

    void IMouseListener.OnMouseUp(Position position)
    {
        OnMouseRelease(position);
        MouseReleased?.Invoke(this, position);
        if (!IsMousePressed) return;   // released without a matching press on us -> not a click
        IsMousePressed = false;

        var now = Environment.TickCount64;
        bool isDouble = now - _lastClickMs <= DoubleClickMs
            && _lastClickPos.X == position.X && _lastClickPos.Y == position.Y;
        _lastClickMs = isDouble ? 0 : now;   // reset so a triple-click isn't read as a second double
        _lastClickPos = position;

        if (isDouble)
        {
            OnDoubleClick(position);
            DoubleClicked?.Invoke(this, position);
        }
        else
        {
            OnClick(position);
            Clicked?.Invoke(this, position);
        }
        InvalidateInteractive();
    }
    #endregion

    #region IMouseWheelListener
    void IMouseWheelListener.OnMouseWheel(Position position, int delta)
    {
        OnMouseWheel(position, delta);
        MouseWheeled?.Invoke(this, delta);
    }
    #endregion

    /// <summary>
    /// Handles a bracketed-paste payload.
    /// </summary>
    /// <remarks>
    /// Default replays it as character key events so existing text controls receive it; controls that can insert
    /// text in bulk (e.g. <see cref="TextEditor"/>) should override this.
    /// </remarks>
    public virtual void OnPaste(string text)
    {
        if (!HandlesInput) return;
        foreach (var c in text)
            OnInput(new InputEvent(new ConsoleKeyInfo(c, (ConsoleKey)0, shift: false, alt: false, control: false)));
    }
    #endregion

    #region Methods
    /// <summary>Cancels any live feeds and detaches the control's paint, theme, and frame event handlers.</summary>
    public virtual void Dispose()
    {
        CancelFeeds();
        UI.Paint -= OnPaint;
        UI.ThemeChanged -= OnThemeChanged;
        if (Frame is not null) UI.ThemeChanged -= Frame.OnThemeChanged;
    }

    /// <summary>
    /// Re-captures this control's themed colours/glyphs from the current <see cref="UI.StyleTheme"/>/
    /// <see cref="UI.GlyphTheme"/>. The default is a no-op for controls that don't use the theme.
    /// </summary>
    /// <remarks>
    /// Called by themed controls from their constructor and again on a runtime theme switch (<see cref="UI.SetTheme(IStyleTheme, IGlyphTheme)"/>).
    /// Must read the themes <em>only here</em> (and in the constructor), never on the render path.
    /// </remarks>
    protected virtual void ApplyTheme() {}

    /// <summary>
    /// <see langword="true"/> if <paramref name="property"/> was explicitly set by the caller (a themeable token
    /// setter passed it to <see cref="SetAtomicProperty"/>).
    /// </summary>
    /// <remarks>
    /// A control's <see cref="ApplyTheme"/> guards each themed field with this so a runtime theme switch re-themes
    /// only the properties the caller left at default.
    /// </remarks>
    protected bool IsThemeOverridden(string property) => _themeOverrides.IsOverridden(property);

    // Runtime theme switch: re-capture themed fields (clobbering any explicit overrides) and repaint. Glyph-width
    // changes flow through the control's own ApplyTheme (which re-measures and resizes), so a relayout follows.
    private void OnThemeChanged(object? sender, EventArgs e)
    {
        CaptureFocusCue();
        ApplyTheme();
        Invalidate();
    }

    // Capture the themed default focus cue (colour + mode) once, off the render path. Read in the base constructor
    // and on a runtime theme switch, like other themed values.
    private void CaptureFocusCue()
    {
        _focusTint = UI.StyleTheme.Focus.BackgroundColor?.ToConsoleGUIColor();
        _focusStyle = UI.StyleTheme.FocusStyle;
    }
    
    // Move focus here *exclusively*, the same as click-to-focus. Setting IsFocused directly would leave any
    // previously-focused control focused too (only UI.SetFocus clears the others), and Layout input routing then
    // delivers keys to every focused control. Always go through UI.SetFocus so single-focus is preserved.
    /// <summary>Moves keyboard focus to this control exclusively (via <see cref="UI.SetFocus"/>), clearing focus from any other control.</summary>
    public void Focus() => UI.SetFocus(this);

    /// <summary>Removes keyboard focus from this control.</summary>
    public void UnFocus() => IsFocused = false;

    /// <summary>
    /// The help shown for this control in the global help dialog (F1), or <see langword="null"/> for no help.
    /// </summary>
    /// <remarks>
    /// Override to describe the control and its keys. The result is deduplicated across the UI by
    /// <see cref="HelpInfo.Name"/>, so give controls of the same kind the same name. <see cref="OnHelp"/> handlers
    /// can further modify (or create) it.
    /// </remarks>
    protected internal virtual HelpInfo? GetHelpInfo() => null;

    /// <summary>The effective help for this control: <see cref="GetHelpInfo"/> with any <see cref="OnHelp"/>
    /// handlers applied (they mutate it in place, and may supply help even when <see cref="GetHelpInfo"/> returned
    /// <see langword="null"/> — a blank entry named after the type is created first). <see langword="null"/> when the
    /// control has no help.</summary>
    public HelpInfo? CompileHelp()
    {
        var info = GetHelpInfo();
        if (OnHelp is { } handlers)
        {
            info ??= new HelpInfo(GetType().Name);
            handlers(info);
        }
        return info;
    }

    /// <summary>
    /// Fired when a control's Initialize method is called.
    /// </summary>
    /// <remarks>This method is always called inside UI.Invoke.</remarks>
    protected virtual void Control_OnInitialization() {}

    /// <summary>Called when this control loses focus; override to react. The default is a no-op.</summary>
    protected virtual void Control_OnLostFocus() {}

    /// <summary>Called when this control gains focus; override to react. The default is a no-op.</summary>
    protected virtual void Control_OnFocus() {}

    /// <summary>
    /// This method renders the control's content to the console buffer.
    /// </summary>
    /// <remarks>Note that this does not actually draw the control on the console screen. 
    /// </remarks>
    protected abstract void Render();

    /// <summary>Lays the control out on the UI thread: computes and applies its size, sizes the buffer, invalidates, and raises <see cref="OnInitialization"/>.</summary>
    protected override void Initialize()
    {
        UI.Invoke((() =>
        {
            var (width, height) = CalculateSize();
            var size = new Size(width, height);
            Resize(size);
            consoleBuffer.Size = Size;
            Invalidate();
            OnInitialization?.Invoke();
        }));
    }                 
            
    /// <summary>
    /// Invoked in the control's OnPaint event handler.
    /// </summary>
    protected virtual void Paint() => Render();

    /// <summary>
    /// Indicates the control should be repainted on the next UI update tick.
    /// </summary>
    protected virtual void Invalidate()
    {
        Interlocked.Increment(ref paintRequests);
        UI.MarkDirty();
    }

    /// <summary>
    /// Requests a repaint in response to an <em>interactive-state</em> change (focus gained/lost, mouse
    /// enter/leave/press/release) rather than a content change.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="Invalidate"/>, so controls behave exactly as before. <see cref="RenderableControl"/>
    /// overrides this to skip the (expensive) re-render of its wrapped renderable when that renderable's output
    /// does not depend on interactive state.
    /// </remarks>
    protected virtual void InvalidateInteractive() => Invalidate();

    #region Damage tracking
    // Opt-in partial redraw. By default a control reports its WHOLE rect as damaged after each paint (see OnPaint),
    // so the compositor scans/diffs the entire control every frame. A control that changes only part of its area
    // (a moving sprite, a single digit, an animation with a bounded footprint) can opt into TracksDamage and report
    // just the changed sub-rect(s) via Damage(), letting the compositor skip the unchanged remainder. The renderer's
    // per-cell diff is still the correctness backstop, so over-reporting only wastes scan time — but UNDER-reporting
    // drops updates, so an opt-in control must report every cell it changed (or DamageAll() when unsure).
    //
    // FOOTGUN — a MOVING or RESIZING region must report the UNION of its OLD and NEW extent. Reporting only the new
    // extent leaves the cells it vacated un-scanned, so the compositor keeps their stale glyphs and the region ghosts
    // a trail. (Globe unions this frame's disc box with last frame's; the bouncing-balls example unions each ball's
    // old and new cell.) Also report the whole control on the first paint and after any resize/full-content change —
    // though a resize already forces a global full redraw, so DamageAll() there is belt-and-braces.
    // NOTE: the compositor collapses to a full redraw past ~32 distinct dirty rects in a frame (MaxDirtyRects), so
    // this pays off for a control with a FEW localized changes, not one with dozens of scattered ones.
    /// <summary>
    /// When <see langword="true"/>, this control reports only the sub-rect(s) it changed each paint — accumulated via
    /// <see cref="Damage(in Rect)"/> during <see cref="Render"/> — instead of its whole area, so the compositor skips
    /// the unchanged remainder. Default <see langword="false"/> (report the full rect every paint, as before).
    /// </summary>
    /// <remarks>
    /// <para>
    /// A control that opts in MUST report every changed cell; over-reporting is safe, under-reporting drops updates.
    /// </para>
    /// <para>
    /// Opt in only when the control has a <em>large drawing area of which little changes per frame</em> — a live
    /// trace, a moving sprite, a marker set over a static backdrop. It is not a free win, for two reasons. The scan
    /// it replaces is a <em>linear</em> walk of the double buffer, the friendliest access pattern there is, whereas
    /// damage bookkeeping is scattered by nature (save the previous value on first touch, then flush a list of
    /// touched indices); a cell avoided is therefore cheaper than a cell tracked, so trading N scanned cells for N
    /// tracked ones is a net loss and only a large ratio wins. And the recorder sits in the write path, so every
    /// cell write pays for it whether or not the frame benefits. A control that redraws its whole area every frame
    /// should leave this off — there the bookkeeping is pure overhead.
    /// </para>
    /// <para>
    /// It also does not reduce terminal output. The renderer already diffs per cell before emitting, so the escape
    /// sequences sent are decided by which cells genuinely differ, never by which rects were declared damaged.
    /// Damage narrows what is <em>scanned</em>, never what is <em>sent</em>. Measure rather than assume; see
    /// "When damage tracking pays" in <c>docs/internal/Rendering Model.md</c>.
    /// </para>
    /// </remarks>
    protected virtual bool TracksDamage => false;

    /// <summary>
    /// Records a screen region (in this control's local coordinates) changed by the current paint.
    /// </summary>
    /// <remarks>
    /// Clipped to the control; empty rects are ignored. Call from <see cref="Render"/>. No effect unless
    /// <see cref="TracksDamage"/> is <see langword="true"/>.
    /// </remarks>
    protected void Damage(in Rect rect)
    {
        if (!TracksDamage) return;
        var clipped = Rect.Intersect(rect, Rect.OfSize(Size));
        if (clipped.Width > 0 && clipped.Height > 0) _damage.Add(clipped);
    }

    /// <summary>Records the whole control as damaged — use on the first paint or after a full-content change so the
    /// entire area is re-composited. See <see cref="Damage(in Rect)"/>.</summary>
    protected void DamageAll() => Damage(Rect.OfSize(Size));
    #endregion

    #region Feeds
    /// <summary>
    /// Starts a repeating feed that runs <paramref name="tick"/> <b>on the UI thread</b> every <paramref name="interval"/>
    /// — for animations and periodic UI updates, without hand-rolling a timer loop.
    /// </summary>
    /// <remarks>
    /// The returned <see cref="FeedHandle"/> stops the feed when cancelled/disposed; the control also cancels every
    /// live feed when it is <see cref="Dispose">disposed</see>. Await <see cref="FeedHandle.Completion"/> (or
    /// <see cref="FeedHandle.StopAsync"/>) to join the in-flight tick before tearing down a resource it uses. The
    /// first tick fires after one interval.
    /// <para/><paramref name="tick"/> <b>always</b> runs on the UI thread — it may read and mutate control state directly (no
    /// marshaling), but it also means heavy work in it runs at frame start and delays the frame. For a tick that needs
    /// expensive <em>off-thread</em> work, use the <see cref="Feed{T}(Func{T}, Action{T}, TimeSpan, Action{Exception})"/> overload
    /// instead, which runs the work on a background thread and only applies the result on the UI thread.
    /// <para/>Implementation note: the tick is delivered via <see cref="UI.Post"/> (not a direct call) so its redraw
    /// request and state change land together in the same dispatcher drain, before that frame's paint — see the note
    /// on <see cref="UI.Post"/> vs <see cref="UI.Invoke"/>.
    /// </remarks>
    protected FeedHandle Feed(Action tick, TimeSpan interval, Action<Exception>? onError = null) =>
        StartFeed(interval, () => UI.Post(tick), onError);

    /// <summary>Convenience overload taking the interval in milliseconds. See <see cref="Feed(Action, TimeSpan, Action{Exception})"/>.</summary>
    protected FeedHandle Feed(Action tick, int intervalMs, Action<Exception>? onError = null) =>
        Feed(tick, TimeSpan.FromMilliseconds(intervalMs), onError);

    /// <summary>
    /// A producer/consumer feed: every <paramref name="interval"/>, <paramref name="produce"/> runs on the feed's
    /// <b>background thread</b> and its result is posted to <paramref name="apply"/> on the <b>UI thread</b>.
    /// </summary>
    /// <remarks>
    /// Use this when each tick needs expensive off-thread work (querying the OS, hitting the network, heavy
    /// computation) whose result should update the control — only the cheap <paramref name="apply"/> touches the UI
    /// thread, so the frame isn't blocked. Cancellation and disposal behave as in <see cref="Feed(Action, TimeSpan, Action{Exception})"/>.
    /// <para/>If <paramref name="produce"/> throws, the feed stops and — when <paramref name="onError"/> is supplied —
    /// the exception is marshaled to it via <see cref="UI.Post"/> (so it runs on the UI thread and can surface the
    /// failure as visible state); without <paramref name="onError"/> the throw silently ends the feed. A throw in
    /// <paramref name="apply"/> is not caught here (it also runs on the UI thread via <see cref="UI.Post"/>). Because
    /// the callback is posted, it is delivered only while a <see cref="UI.Start"/> loop is running to drain the queue —
    /// a headless test must run a real UI loop to observe it (<c>ConsoleSnapshot</c>/<c>UI.PaintFrame</c> paint without
    /// draining posted work).
    /// </remarks>
    protected FeedHandle Feed<T>(Func<T> produce, Action<T> apply, TimeSpan interval, Action<Exception>? onError = null) =>
        StartFeed(interval, () => { var result = produce(); UI.Post(() => apply(result)); }, onError);

    /// <summary>Convenience overload taking the interval in milliseconds. See <see cref="Feed{T}(Func{T}, Action{T}, TimeSpan, Action{Exception})"/>.</summary>
    protected FeedHandle Feed<T>(Func<T> produce, Action<T> apply, int intervalMs, Action<Exception>? onError = null) =>
        Feed(produce, apply, TimeSpan.FromMilliseconds(intervalMs), onError);

    // Shared feed loop: every interval, run `pump` on the background task (for a plain feed it posts the tick; for a
    // producer feed it produces off-thread then posts the apply). Registers/unregisters the CTS so Dispose can cancel.
    private FeedHandle StartFeed(TimeSpan interval, Action pump, Action<Exception>? onError)
    {
        var cts = new CancellationTokenSource();
        lock (_feeds) _feeds.Add(cts);
        var ct = cts.Token;
        var loop = Task.Run(async () =>
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    try { await Task.Delay(interval, ct).ConfigureAwait(false); }
                    catch (TaskCanceledException) { break; }
                    if (ct.IsCancellationRequested) break;
                    try { pump(); }
                    catch (Exception ex)
                    {
                        // A throwing producer ends the feed (a dead source rarely recovers). Surface it to the caller
                        // on the UI thread when they asked; otherwise it ends silently rather than as an unobserved
                        // task exception. Either way we stop.
                        if (onError is not null) UI.Post(() => onError(ex));
                        break;
                    }
                }
            }
            finally
            {
                // The feed registry is background-task bookkeeping, not UI/render state, so it takes a small lock
                // rather than being marshaled onto the UI thread — cancellation must still work during shutdown, when
                // the dispatcher no longer drains posted work.
                lock (_feeds) _feeds.Remove(cts);
            }
        });
        return new FeedHandle(cts, loop);
    }

    // Cancels every live feed. Called from Dispose; each cancelled feed's loop removes itself from the registry. The
    // CTS is left undisposed on purpose — a caller may still hold the returned handle and call Cancel(), which is safe
    // (idempotent) on a live CTS but throws on a disposed one.
    private void CancelFeeds()
    {
        CancellationTokenSource[] snapshot;
        lock (_feeds) { snapshot = [.. _feeds]; _feeds.Clear(); }
        foreach (var cts in snapshot) cts.Cancel();
    }

    /// <summary>
    /// A thread-safe snapshot of this control's currently live feed handles (each <see cref="Feed(Action, int, Action{Exception})"/> call
    /// registers one; it self-unregisters when cancelled or completed).
    /// </summary>
    /// <remarks>
    /// Cancelling these stops the feeds without disposing the control — handy for pausing background work while the
    /// control is hidden. Feeds are also cancelled automatically on <see cref="Dispose"/>. Returns a copy, so
    /// iterating it never races the background feed threads.
    /// </remarks>
    protected IReadOnlyList<CancellationTokenSource> Feeds
    {
        get { lock (_feeds) return [.. _feeds]; }
    }
    #endregion

    /// <summary>
    /// Assigns a backing field and requests a redraw, but only when the value actually changes.
    /// </summary>
    /// <remarks>
    /// Centralizes the (optional coerce) + equality-check + assign + (optional notify) + invalidate pattern
    /// required of visual property setters.
    /// <para/>Only valid for atomically-assignable fields (a single value or reference store). State that cannot be
    /// written atomically (e.g. collections inside a wrapped control) must use a copy-on-write update instead.
    /// When supplied, <paramref name="validate"/> runs first, so the equality check and stored value use the
    /// coerced value; <paramref name="watch"/> then runs after assignment and before the invalidate/initialize.
    /// </remarks>
    /// <param name="field">The backing field to assign.</param>
    /// <param name="value">The new value.</param>
    /// <param name="updatesLayout">
    /// When <see langword="true"/>, the change affects layout and <see cref="Initialize"/> is called (which
    /// re-lays-out on the UI thread and invalidates). Otherwise <see cref="Invalidate"/> is called.
    /// </param>
    /// <param name="validate">Optional coercion (e.g. clamp) applied before the equality check and assignment.</param>
    /// <param name="watch">Optional change callback receiving the old and new values, run before the invalidate/initialize.</param>
    /// <param name="themeOverride">
    /// When <see langword="true"/>, marks the calling property (captured automatically via
    /// <paramref name="propertyName"/>) as an explicit theme override, so a later runtime theme switch through
    /// <see cref="ApplyTheme"/> leaves it alone. Themeable token setters pass <c>themeOverride: true</c>.
    /// </param>
    /// <param name="propertyName">
    /// The calling member's name, supplied automatically by the compiler (<see cref="CallerMemberNameAttribute"/>).
    /// Inside a property setter this is the property name (e.g. <c>AccentStyle</c>), so it matches the
    /// <c>nameof(AccentStyle)</c> used by <see cref="ApplyTheme"/>. Do not pass it explicitly.
    /// </param>
    /// <returns>The resulting field value (the coerced new value when changed, otherwise the existing one).</returns>
    protected T SetAtomicProperty<T>(ref T field, T value, bool updatesLayout = false, Func<T, T>? validate = null, Action<T, T>? watch = null, bool themeOverride = false, [CallerMemberName] string? propertyName = null)
    {
        if (themeOverride && propertyName is not null) _themeOverrides.Mark(propertyName);
        if (validate is not null) value = validate(value);
        if (EqualityComparer<T>.Default.Equals(field, value)) return field;

        var old = field;
        field = value;
        watch?.Invoke(old, value);

        if (updatesLayout)
            Initialize();
        else
            Invalidate();

        return field;
    }

    /// <summary>
    /// Indicates that any pending paint requests have been handled and the control does not need re-painting.
    /// </summary>
    protected virtual void Validate() => Interlocked.Exchange(ref paintRequests, 0u);

    /// <summary>
    /// Calculates the size of the control based on its own dimensions and the maximum and minimum size constraints set by its parent.
    /// </summary>
    /// <returns></returns>
    protected (int, int) CalculateSize()
    {
        // Handle the case when negative or overflow sizes may get passed down by parent containers
        int maxWidth = Math.Clamp(MaxSize.Width, 0 ,1000);
        int maxHeight = Math.Clamp(MaxSize.Height, 0, 1000);
        int minWidth = Math.Clamp(MinSize.Width, 0 ,1000);
        int minHeight = Math.Clamp(MinSize.Height, 0, 1000);

        // Width first (an intrinsic height may depend on it, e.g. wrapped text). An intrinsic width — a genuine
        // fixed extent reported by an adornment control (e.g. a vertical TextLabel that is one column wide) — is
        // honored even under a finite parent, ahead of the fill-to-parent default, so the control doesn't balloon
        // to fill the space a docking/layout parent offers.
        var intrinsicWidth = IntrinsicWidth();
        var preferredWidth = Width > 0 ? Width
            : intrinsicWidth > 0 ? intrinsicWidth
            : Size.Width > 0 ? Size.Width
            : maxWidth;
        var width = Math.Clamp(preferredWidth, minWidth, maxWidth);

        // An intrinsic height is likewise authoritative even under a finite parent (a horizontal TextLabel is one
        // row tall and must stay one row when docked, not eat the whole panel). Distinct from IScrollable's content
        // height, which is honored only when the parent is unbounded (for scrolling).
        var intrinsicHeight = IntrinsicHeight();
        // When the parent leaves the height unbounded — a scrolling ControlFrame passes int.MaxValue to an
        // IScrollable child so it can grow and be scrolled — size to the reported content height instead of filling
        // to the 1000 clamp. That makes the frame's scrollbar and scroll range reflect real content. A finite parent
        // (e.g. a Grid cell) still fills, exactly as before. Only an IScrollable is asked: a control that never opted
        // into scrolling has no content height to give, and a frame never leaves it unbounded in the first place.
        var unbounded = MaxSize.Height >= UnboundedHeight;
        var contentHeight = unbounded && this is IScrollable scrollable ? scrollable.MeasureHeight(width) : 0;
        var preferredHeight = Height > 0 ? Height
            : intrinsicHeight > 0 ? intrinsicHeight
            : contentHeight > 0 ? contentHeight
            : Size.Height > 0 ? Size.Height
            : maxHeight;

        var height = Math.Clamp(preferredHeight, minHeight, maxHeight);
        return (width, height);
    }

    /// <summary>
    /// An intrinsic, fixed width in cells this control always wants regardless of the space its parent offers, or
    /// 0 (the default) to fill the parent's width.
    /// </summary>
    /// <remarks>
    /// Unlike <see cref="IScrollable.MeasureHeight"/> — a content height honored only when the parent is unbounded —
    /// an intrinsic size is authoritative even under a finite parent. Override on adornment controls with a genuine
    /// fixed extent (e.g. a vertical <see cref="TextLabel"/>, one column wide) so a docking/layout parent can't
    /// stretch them to fill the region.
    /// </remarks>
    protected virtual int IntrinsicWidth() => 0;

    /// <summary>The intrinsic, fixed height counterpart of <see cref="IntrinsicWidth"/> (e.g. a horizontal
    /// <see cref="TextLabel"/> is one row tall). Returns 0 to fill the parent's height (the default).</summary>
    protected virtual int IntrinsicHeight() => 0;

    // A height limit this large only comes from a scrolling ControlFrame (which passes int.MaxValue); any real
    // viewport is far smaller, so it cleanly distinguishes "unbounded for scrolling" from a finite parent.
    private const int UnboundedHeight = 100_000;

    /// <summary>Clamps <paramref name="width"/> to the range [0, this control's width].</summary>
    public int ClampWidth(int width) => Math.Clamp(width, 0, Size.Width);

    /// <summary>Clamps <paramref name="height"/> to the range [0, this control's height].</summary>
    public int ClampHeight(int height) => Math.Clamp(height, 0, Size.Height);
    /// <summary>
    /// Handles the paint event triggered by the UI timer. If one or more paint requests are pending, it runs the painting process and resets the paint request count.
    /// </summary>
    /// <remarks>
    /// Painting runs on the UI thread (driven by the dispatcher's frame), so no lock is required.
    /// </remarks>
    private void OnPaint(object? sender, UI.PaintEventArgs e)
    {
        if (paintRequests > 0)
        {
            var timer = UI.controlPaintTimers[this];
            timer.Restart();
            Paint();
            Validate();
            timer.Stop();
            // Fractional ms: most controls repaint in well under 1 ms; ElapsedMilliseconds would truncate them to 0.
            UI.controlPaintTimes[this][UI.paintTimeIndex] = timer.Elapsed.TotalMilliseconds;
            // Report the damaged screen region(s). The ConsoleGUI base Control.Update bubbles each rect up the
            // DrawingContext tree, translating it to screen coordinates, so this frame's flush re-composites only
            // those regions instead of the whole screen. The bubbled rect is itself the redraw signal (the frame
            // loop composites when there is damage), so no separate MarkDirty is needed — and adding one here would
            // set needsDraw for the *next*, undamaged frame and trip the loop's full-redraw fallback.
            if (TracksDamage)
            {
                // Opt-in: composite only the sub-rects this paint reported (possibly none → nothing to composite).
                for (int i = 0; i < _damage.Count; i++) Update(_damage[i]);
                _damage.Clear();
            }
            else if (Size.Width > 0 && Size.Height > 0)
            {
                Update(ConsoleGUI.Space.Rect.OfSize(Size));
            }
        }
        else
        {
            UI.controlPaintTimes[this][UI.paintTimeIndex] = null;
        }
    }
    #endregion

    #region Events
    /// <summary>Raised when the control is initialized (laid out); always invoked on the UI thread.</summary>
    public event InitializationHandler OnInitialization;
    /// <summary>Raised when the control gains focus.</summary>
    public event FocusableEventHandler? OnFocus;
    /// <summary>Raised when the control loses focus.</summary>
    public event FocusableEventHandler? OnLostFocus;

    /// <summary>Raised while the global help dialog is compiled, letting code add or modify this control's help
    /// without subclassing. The handler receives a mutable <see cref="HelpInfo"/> (created if
    /// <see cref="GetHelpInfo"/> returned <see langword="null"/>) and edits it in place.</summary>
    public event Action<HelpInfo>? OnHelp;

    /// <summary>Raised when the pointer enters the control.</summary>
    public event EventHandler? MouseEntered;
    /// <summary>Raised when the pointer leaves the control.</summary>
    public event EventHandler? MouseLeft;
    /// <summary>Raised as the pointer moves within the control (relative position).</summary>
    public event EventHandler<Position>? MouseMoved;
    /// <summary>Raised when a button is pressed over the control (relative position).</summary>
    public event EventHandler<Position>? MousePressed;
    /// <summary>Raised when a button is released over the control (relative position).</summary>
    public event EventHandler<Position>? MouseReleased;
    /// <summary>Raised on a press+release on this control (relative position).</summary>
    public event EventHandler<Position>? Clicked;
    /// <summary>Raised on two clicks within <see cref="DoubleClickMs"/> at the same position.</summary>
    public event EventHandler<Position>? DoubleClicked;
    /// <summary>Raised on a wheel notch over the control (negative up, positive down).</summary>
    public event EventHandler<int>? MouseWheeled;
    #endregion

    #region Fields
    /// <summary>A shared empty <see cref="Character"/>.</summary>
    public static Character emptyChar = new Character();
    /// <summary>A shared empty <see cref="Cell"/>, returned for positions outside the control's size.</summary>
    protected static readonly Cell emptyCell = new Cell(Character.Empty);
    /// <summary>Count of pending paint requests; a non-zero value triggers a repaint on the next paint tick.</summary>
    protected internal uint paintRequests;
    /// <summary>The buffer the control renders its cells into.</summary>
    protected readonly ConsoleBuffer consoleBuffer;
    /// <summary>The Spectre.Console <see cref="AnsiConsoleBuffer"/> that writes styled output into <see cref="consoleBuffer"/>.</summary>
    protected readonly AnsiConsoleBuffer ansiConsole;
    // Damaged sub-rects reported during the current paint by a TracksDamage control; drained in OnPaint. UI-thread only.
    private readonly List<Rect> _damage = new();
    // Live background feeds (see Feed); cancelled en masse on Dispose. Guarded by its own lock (cross-thread registry).
    private readonly List<CancellationTokenSource> _feeds = new();
    // The themed default focus cue (IStyleTheme.Focus background + IStyleTheme.FocusStyle mode), captured off the
    // render path; applied to a focused control that shows focus no other way. Tint null = theme sets no focus bg.
    private ConsoleGUI.Data.Color? _focusTint;
    private FocusStyle _focusStyle;
    // Tracks which theme-token properties the caller explicitly set, so ApplyTheme re-themes only the rest.
    private readonly ThemeOverrides _themeOverrides = new();

    /// <summary>Maximum gap (ms) between two clicks for them to register as a double-click.</summary>
    protected const long DoubleClickMs = 400;
    private long _lastClickMs;
    private Position _lastClickPos;
    #endregion

    #region Types
    /// <summary>Delegate for the <see cref="OnInitialization"/> event.</summary>
    public delegate void InitializationHandler();
    #endregion
}
