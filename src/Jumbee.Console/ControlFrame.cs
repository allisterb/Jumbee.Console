namespace Jumbee.Console;

using System;

using ConsoleGUI.Common;
using ConsoleGUI.Data;
using ConsoleGUI.Input;
using ConsoleGUI.Space;

using Spectre.Console.Rendering;

using SpectreBoxBorder = Spectre.Console.BoxBorder;
using SpectreBoxBorderPart = Spectre.Console.Rendering.BoxBorderPart;

// The public BorderPlacement type is the Jumbee.Console.Styles enum; internally we keep ConsoleGUI's identically-valued
// enum (CBorderPlacement) because the border geometry uses its HasBorder/AsOffset extensions. The public property casts
// between them at the boundary (same underlying values), just like Color <-> CColor.
using CBorderPlacement = ConsoleGUI.Data.BorderPlacement;

// BorderStyle, TitlePos, TitleBorderStyle, TitleColorStyle, TitleStyle and BorderPlacement live in the
// Jumbee.Console.Styles project; a frame composes its defaults from the active themes (see ctor).

/// <summary>
/// Draws a border around a control together with margins and a title bar, and sets the foreground and background colors.
/// </summary>
public sealed class ControlFrame : CControl, IFocusable, IDrawingContextListener, IMouseListener, IMouseWheelListener
{
    #region Constructors
    /// <summary>Initializes a new <see cref="ControlFrame"/> wrapping <paramref name="control"/>, taking the border shape, margin, foreground/background and border colours, title, and title style (defaulting any unset value from the active theme).</summary>
    public ControlFrame(Control control, BorderStyle? borderStyle = null, Offset? margin = null, Color? fgColor = null, Color? bgColor = null, string? title=null, Color? borderFgColor = null, Color? borderBgColor = null, TitleStyle? titleStyle = null)
    {
        // The default border shape comes from the style theme when the caller doesn't specify one.
        _borderStyle = borderStyle ?? UI.StyleTheme.FrameBorder;
        _boxBorder = GetSpectreBoxBorder(_borderStyle);
        // Focus appearance: an optional shape swap (null = keep the same shape) and a border colour, both taken from
        // the theme and applied on the render path while the wrapped control is focused. See GetBorderCell.
        CaptureFocusStyle();
        _margin = margin ?? DefaultMargin;
        // Appearance defaults come from the active style theme when the caller doesn't specify a colour: the
        // frame foreground (used for the title) from the TitleText token, the border colour from the BorderText
        // token. Explicit colours (e.g. WithRoundedBorder(someColor)) still win. Captured once, never on render.
        _foreground = fgColor ?? UI.StyleTheme.TitleText.ForegroundColor;
        _background = bgColor;
        _borderFgColor = borderFgColor ?? UI.StyleTheme.BorderText.ForegroundColor;
        _borderBgColor = borderBgColor;
        _title = title;
        // The default title style comes straight from the style theme. An explicit TitleStyle (e.g.
        // WithTitle(text, style)) overrides it.
        _titleStyle = titleStyle ?? UI.StyleTheme.TitleStyle;
        // Record which themeable values the caller supplied explicitly, so a later runtime theme switch
        // (ApplyTheme) re-themes only the ones left at their theme default.
        if (borderStyle.HasValue) _themeOverrides.Mark(nameof(BorderStyle));
        if (fgColor.HasValue) _themeOverrides.Mark(nameof(Foreground));
        if (borderFgColor.HasValue) _themeOverrides.Mark(nameof(BorderFgColor));
        if (titleStyle.HasValue) _themeOverrides.Mark(nameof(TitleStyle));
        // The scrollbar's default glyphs come from the glyph theme and its colours from the style theme; the
        // frame composes them into its part cells here (override per-control with WithScrollBarGlyphs /
        // WithScrollBarStyle). Captured once, never read on the render path.
        _scrollBarGlyphs = UI.GlyphTheme.ScrollBar;
        _scrollBarStyle = UI.StyleTheme.ScrollBar;
        RecomposeScrollBar();
        _control = control;
        _control.Frame = this;
        BindControl();
    }
    #endregion

    #region Indexers
    /// <summary>Gets the composited <see cref="Cell"/> at <paramref name="position"/>, drawing the border, title, scrollbar, and margins around the wrapped control.</summary>
    public override Cell this[Position position]
    {
        get
        {            
            // 1. Calculate Offsets & Viewport
            // We replicate Initialize logic to ensure consistency
            var totalOffset = borderOffset;

            var controlLeft = totalOffset.Left;
            var controlTop = totalOffset.Top;
            var controlRight = Size.Width - 1 - totalOffset.Right;
            var controlBottom = Size.Height - 1 - totalOffset.Bottom;

            // 2. Control & Scrollbar (Inside Viewport)
            if (position.X >= controlLeft && position.X <= controlRight &&
                position.Y >= controlTop && position.Y <= controlBottom)
            {
                // Scrollbar logic: always at the right edge of valid control area
                if (position.X == controlRight)
                {
                    if (Control == null) return ScrollBarForeground;

                    var viewportHeight = controlBottom - controlTop + 1;
                    var controlHeight = ControlContext.Size.Height;

                    // Only draw the scrollbar when the content is taller than the viewport. Tag the cell with a mouse
                    // listener so clicks/drags on the bar reach this frame (thumb drag, arrow step, track page).
                    if (controlHeight > viewportHeight)
                        return new Cell(ScrollBarCell(position.Y - controlTop, viewportHeight, controlHeight)).WithMouseListener(this, position);
                    // Otherwise the reserved column is padding; fall through so the control (or empty) shows.
                }

                if (ControlContext.Contains(position))
                    return ControlContext[position];

                return Character.Empty;
            }

            // 3. Borders & Title (Outside Viewport)
            var left = Margin.Left;
            var top = Margin.Top;
            var right = Size.Width - 1 - Margin.Right;
            var bottom = Size.Height - 1 - Margin.Bottom;

            // The title is drawn in either the top or bottom border, depending on its position.
            var titleEdge = TitleAtTop ? top : bottom;
            var titleEdgeBorder = TitleAtTop ? CBorderPlacement.Top : CBorderPlacement.Bottom;
            var hasTitle = !string.IsNullOrEmpty(Title) && _borderPlacement.HasBorder(titleEdgeBorder);

            // Inline title: drawn within the single (top or bottom) border row, replacing some border line characters.
            if (hasTitle && _titleStyle.BorderStyle == TitleBorderStyle.Inline && position.Y == titleEdge)
            {
                if (GetTitleCell(position.X, left, right) is { } inlineTitleCell)
                    return inlineTitleCell;
            }

            if (position.X == left && position.Y == top && _borderPlacement.HasBorder(CBorderPlacement.Top | CBorderPlacement.Left))
                return GetBorderCell(BoxBorderPart.TopLeft);

            if (position.X == right && position.Y == top && _borderPlacement.HasBorder(CBorderPlacement.Top | CBorderPlacement.Right))
                return GetBorderCell(BoxBorderPart.TopRight);

            if (position.X == left && position.Y == bottom && _borderPlacement.HasBorder(CBorderPlacement.Bottom | CBorderPlacement.Left))
                return GetBorderCell(BoxBorderPart.BottomLeft);

            if (position.X == right && position.Y == bottom && _borderPlacement.HasBorder(CBorderPlacement.Bottom | CBorderPlacement.Right))
                return GetBorderCell(BoxBorderPart.BottomRight);

            if (position.X == left && position.Y >= top && position.Y <= bottom && _borderPlacement.HasBorder(CBorderPlacement.Left))
                return GetBorderCell(BoxBorderPart.Left);

            if (position.X == right && position.Y >= top && position.Y <= bottom && _borderPlacement.HasBorder(CBorderPlacement.Right))
                return GetBorderCell(BoxBorderPart.Right);

            if (position.Y == top && position.X >= left && position.X <= right && _borderPlacement.HasBorder(CBorderPlacement.Top))
                return GetBorderCell(BoxBorderPart.Top);

            if (position.Y == bottom && position.X >= left && position.X <= right && _borderPlacement.HasBorder(CBorderPlacement.Bottom))
                return GetBorderCell(BoxBorderPart.Bottom);

            if (hasTitle && _titleStyle.BorderStyle == TitleBorderStyle.Double)
            {
                // The Double style reserves a title row and a separator row just inside the title's border.
                var titleRow = TitleAtTop ? top + 1 : bottom - 1;
                var separatorRow = TitleAtTop ? top + 2 : bottom - 2;
                var separatorPart = TitleAtTop ? BoxBorderPart.Top : BoxBorderPart.Bottom;

                if (position.Y == titleRow)
                {
                    if (GetTitleCell(position.X, left, right) is { } titleCell)
                        return titleCell;
                }
                else if (position.Y == separatorRow && position.X > left && position.X < right)
                {
                    // The separator's left/right edges are drawn by the vertical-border checks above.
                    return GetBorderCell(separatorPart);
                }
            }

            return Character.Empty;
        }
    }
    
    #endregion

    #region Properties
    /// <summary>The control wrapped by this frame; setting it rebinds the frame to the new control.</summary>
    public Control Control
    {
        get => _control;
        set
        {            
            _control = value;
            _control.Frame = this;  
            BindControl();
        }
    }

    /// <summary>The border shape drawn around the control.</summary>
    public BorderStyle BorderStyle
    {
        get => _borderStyle;
        set
        {
            UI.Invoke(() =>
            {
                _themeOverrides.Mark(nameof(BorderStyle));
                if (_borderStyle == value) return;
                _borderStyle = value;
                _boxBorder = GetSpectreBoxBorder(_borderStyle);
                Initialize();
            });
        }
    }

    /// <summary>The text shown in the frame's title bar, or <see langword="null"/> for no title.</summary>
    public string? Title
    {
        get => _title;
        set
        {
            UI.Invoke(() => 
            {
                if (_title == value) return;
                _title = value;
                Initialize();
            });
        }
    }

    /// <summary>The title's position, border style, and colour.</summary>
    public TitleStyle TitleStyle
    {
        get => _titleStyle;
        set
        {
            UI.Invoke(() =>
            {
                _themeOverrides.Mark(nameof(TitleStyle));
                if (_titleStyle.Equals(value)) return;
                _titleStyle = value;
                Initialize();
            });
        }
    }

    /// <summary>Which edges of the frame draw a border.</summary>
    public BorderPlacement BorderPlacement
    {
        get => (BorderPlacement)_borderPlacement;
        set
        {
            UI.Invoke(() =>
            {
                var placement = (CBorderPlacement)value;
                if (_borderPlacement == placement) return;
                _borderPlacement = placement;
                Initialize();
            });
        }
    }

    /// <summary>The border shape drawn while the frame is focused (or contains focus), overriding the theme's
    /// <see cref="IStyleTheme.FocusedFrameBorder"/>. Set to <see cref="BorderStyle.None"/> to suppress the focus
    /// border entirely — e.g. when the wrapped control already shows focus another way (a text cursor).</summary>
    public BorderStyle? FocusedBorderStyle
    {
        get => _focusedBorderStyle;
        set
        {
            UI.Invoke(() =>
            {
                _themeOverrides.Mark(nameof(FocusedBorderStyle));
                if (_focusedBorderStyle == value) return;
                _focusedBorderStyle = value;
                _focusedBoxBorder = value is { } style ? GetSpectreBoxBorder(style) : null;
                Initialize();
            });
        }
    }

    /// <summary>The margin (empty space) between the border and the wrapped control.</summary>
    public Offset Margin
    {
        get => _margin;
        set
        {
            UI.Invoke(() => 
            {
                if (_margin.Equals(value)) return;
                _margin = value;
                Initialize();
            });
        }
    }

    /// <summary>The foreground colour used for the title text, or <see langword="null"/> for the theme default.</summary>
    public Color? Foreground
    {
        get => _foreground;
        set
        {
            _themeOverrides.Mark(nameof(Foreground));
            if (Equals(_foreground, value)) return;
            _foreground = value;
            _Redraw();
        }
    }

    /// <summary>The background colour filling the frame, or <see langword="null"/> for none.</summary>
    public Color? Background
    {
        get => _background;
        set
        {
            if (Equals(_background, value)) return;
            _background = value;
            _Redraw();
        }
    }

    /// <summary>The border foreground colour, or <see langword="null"/> for the theme default.</summary>
    public Color? BorderFgColor
    {
        get => _borderFgColor;
        set
        {
            _themeOverrides.Mark(nameof(BorderFgColor));
            if (Equals(_borderFgColor, value)) return;
            _borderFgColor = value;
            _Redraw();
        }
    }

    /// <summary>The border background colour, or <see langword="null"/> for none.</summary>
    public Color? BorderBgColor
    {
        get => _borderBgColor;
        set
        {
            if (Equals(_borderBgColor, value)) return;
            _borderBgColor = value;
            _Redraw();
        }
    }
   
    /// <summary>The vertical scroll offset (topmost visible row of the wrapped control); clamped to the scrollable range and raises <see cref="Scrolled"/> on change.</summary>
    public int Top
    {
        get => _top;
        set
        {
            UI.Invoke(() =>
            {
                var old = _top;
                using (Freeze())
                {
                    _top = value;

                    var viewportHeight = Math.Max(0, Size.Height - borderOffset.Top - borderOffset.Bottom);
                    if (ControlContext?.Size.Height > viewportHeight)
                    {
                        _top = Math.Min(ControlContext.Size.Height - viewportHeight, Math.Max(0, _top));
                        ControlContext.SetOffset(new Vector(borderOffset.Left, borderOffset.Top - _top));
                    }
                    else
                    {
                        _top = 0;
                        ControlContext?.SetOffset(new Vector(borderOffset.Left, borderOffset.Top));
                    }
                }
                // Let adornments outside the frame (e.g. a line-number gutter docked beside it) follow the scroll.
                if (_top != old) Scrolled?.Invoke(this, EventArgs.Empty);
            });
        }
    }
  
    /// <summary>The glyph/cell drawn for the scrollbar thumb (foreground).</summary>
    public Character ScrollBarForeground
    {
        get => _scrollBarForeground;
        set
        {
            if (_scrollBarForeground.Equals(value)) return;
            _scrollBarForeground = value;
            _Redraw(); // Just redraw scrollbar? Or full? Full is easier.
        }
    }
  
    /// <summary>The glyph/cell drawn for the scrollbar track (background).</summary>
    public Character ScrollBarBackground
    {
        get => _scrollBarBackground;
        set
        {
            if (_scrollBarBackground.Equals(value)) return;
            _scrollBarBackground = value;
            _Redraw();
        }
    }

    /// <summary>The glyph/cell drawn for the scrollbar's up arrow.</summary>
    public Character ScrollBarUpArrow
    {
        get => _scrollBarUpArrow;
        set
        {
            UI.Invoke(() => 
            {
                if (_scrollBarUpArrow.Equals(value)) return;
                _scrollBarUpArrow = value;
                Redraw();
            });
        }
    }

    /// <summary>The glyph/cell drawn for the scrollbar's down arrow.</summary>
    public Character ScrollBarDownArrow
    {
        get => _scrollBarDownArrow;
        set
        {
            UI.Invoke(() => 
            {
                if (_scrollBarDownArrow.Equals(value)) return;
                _scrollBarDownArrow = value;
                Redraw();
            });
        }
    }

    /// <summary>
    /// Gets or sets the scrollbar glyphs (thumb, track, up/down arrows).
    /// </summary>
    /// <remarks>Setting it recomposes the part cells with the current <see cref="ScrollBarStyle"/> colours.</remarks>
    public ScrollBarGlyphs ScrollBarGlyphs
    {
        get => _scrollBarGlyphs;
        set => UI.Invoke(() => { _themeOverrides.Mark(nameof(ScrollBarGlyphs)); _scrollBarGlyphs = value; RecomposeScrollBar(); Redraw(); });
    }

    /// <summary>
    /// Gets or sets the scrollbar part colours/decoration.
    /// </summary>
    /// <remarks>Setting it recomposes the part cells with the current <see cref="ScrollBarGlyphs"/> glyphs.</remarks>
    public ScrollBarStyle ScrollBarStyle
    {
        get => _scrollBarStyle;
        set => UI.Invoke(() => { _themeOverrides.Mark(nameof(ScrollBarStyle)); _scrollBarStyle = value; RecomposeScrollBar(); Redraw(); });
    }

    /// <summary>The key that scrolls the frame's content up one row.</summary>
    public ConsoleKeyInfo ScrollUpKey { get; set; } = UI.HotKeys.AltUp;

    /// <summary>The key that scrolls the frame's content down one row.</summary>
    public ConsoleKeyInfo ScrollDownKey { get; set; } = UI.HotKeys.AltDown;

    /// <summary>When <see langword="true"/> (the default), the frame can receive keyboard focus.</summary>
    public bool Focusable { get; set; } = true;

    /// <summary>Whether the frame (and its wrapped control) holds focus; setting it raises the focus events and repaints the border cue.</summary>
    public bool IsFocused
    {
        get => field;
        set
        {
            var old = field;
            field = value;
            if (field && !old)
            {
                OnFocus?.Invoke();    
            }
            else if (!field && old)
            {
                OnLostFocus?.Invoke();
            }
            _control.IsFocused = field;
            // Repaint the border so the focused style/colour appears (or clears) — via the focus-cue path so direct
            // focus and contained (nested-descendant) focus share one repaint rule. Geometry is unaffected (the
            // border offset comes from BorderPlacement, not the shape), so this redraws in place without reflow.
            OnFocusChanged();
        }
    }

    // The frame draws its focus cue when it is directly focused OR when focus is nested inside it — a framed
    // CompositeControl lights up when any descendant is focused (Control.FocusedControl tracks that top-down). Cached
    // in _focusCueVisible so the border repaints only when the cue actually flips.
    private bool ContainsFocus => IsFocused || _control.FocusedControl is not null;

    // Handles a focus change (the frame's own IsFocused, or the global UI.FocusChanged for a nested descendant):
    // repaint the border only when this frame's focus cue flips.
    internal void OnFocusChanged()
    {
        if (ContainsFocus == _focusCueVisible) return;
        _focusCueVisible = ContainsFocus;
        _Redraw();
    }

    /// <summary>The focus target the UI registers for this frame — the frame itself, which routes input to the wrapped control.</summary>
    public IFocusable FocusableControl => this;

    /// <summary>
    /// When something inside the frame is focused, the frame stays the routing node (so it can still intercept
    /// scroll keys) but reports that focus is present.
    /// </summary>
    /// <remarks>
    /// This delegates to the wrapped control's <see cref="Control.FocusedControl"/> rather than the frame's own
    /// <see cref="IsFocused"/>, so focus nested deeper than one level — e.g. a child inside a
    /// <see cref="CompositeControl"/> — still routes correctly.
    /// </remarks>
    public IFocusable? FocusedControl => _control.FocusedControl is not null ? this : null;

    /// <summary>Always <see langword="true"/> — the frame processes input (scroll keys) and tunnels the rest to the wrapped control.</summary>
    public bool HandlesInput => true;

    private DrawingContext ControlContext
    {
        get => _controlContext;
        set
        {
            if (_controlContext == value) return;
            _controlContext?.Dispose();
            _controlContext = value;
            Initialize();
        }
    }

    /// <summary>The size of the visible content area inside the border and margins.</summary>
    public Size ViewportSize => GetViewportSize();

    /// <summary>Whether this frame draws a focus-dependent border cue — i.e. it draws a visible border while focused,
    /// on at least one edge. That's the case when it has a resting border, OR the theme supplies a focus-only border
    /// (<see cref="IStyleTheme.FocusedFrameBorder"/>) that appears when focused. When true the wrapped control skips
    /// the themed default focus tint and defers to this border; a fully borderless frame shows no cue, so the tint
    /// applies instead.</summary>
    internal bool ShowsFocusCue =>
        (_focusedBorderStyle ?? _borderStyle) != BorderStyle.None && _borderPlacement != CBorderPlacement.None;
    #endregion

    #region Methods
    /// <inheritdoc/>
    void IDrawingContextListener.OnRedraw(DrawingContext drawingContext)
    {
        Initialize();
    }

    void IDrawingContextListener.OnUpdate(DrawingContext drawingContext, Rect rect)
    {
        UI.Invoke(() => Update(rect));
    }

    /// <summary>Handles a UI input event: consumes the frame's own scroll keys, then tunnels the rest to the focused descendant (or a composite's navigation).</summary>
    public void OnInput(UI.InputEventArgs inputEventArgs)
    {
        var inputEvent = inputEventArgs.InputEvent!;
        this.OnInput(inputEvent);
        if (!inputEvent.Handled)
        {
            // A framed composite still gets its tunnel: the routing layer hands the key to the frame (the focus
            // node), so dispatching straight to the focused descendant below would skip the composite's own
            // navigation keys (e.g. a form tabbing between its fields).
            if (_control is CompositeControl composite && composite.RouteInterceptInput(inputEventArgs))
            {
                inputEvent.Handled = true;
                return;
            }

            // Forward to the focused descendant (the wrapped control itself for a leaf, or the focused child of a
            // composite), not blindly to the wrapped control — so input reaches the right control when nested.
            (_control.FocusedControl ?? (IFocusable)_control).OnInput(inputEventArgs);
        }
    }

    // Bracketed paste, like OnInput, must tunnel through the frame to the focused descendant — the routing layer
    // delivers it to the frame (the focus node), not the wrapped control. Without this it hits the IFocusable
    // default no-op and pasted text is silently dropped for any framed control.
    /// <summary>Tunnels a bracketed-paste payload through the frame to the focused descendant.</summary>
    public void OnPaste(string text) => (_control.FocusedControl ?? (IFocusable)_control).OnPaste(text);

    /// <summary>Handles a keyboard input event, scrolling the frame when the scroll key is pressed and the frame can scroll in that direction.</summary>
    public void OnInput(InputEvent inputEvent)
    {
        // Only claim the scroll key when this frame can actually scroll in that direction. Otherwise leave it
        // unhandled so the wrapper forwards it inward — a frame with nothing to scroll (its content fills the
        // viewport, e.g. a framed MultiTabCodeEditor) must not swallow the key that a nested scrollable wants.
        if (inputEvent.Key == ScrollUpKey && CanScrollUp)
        {
            Top -= 1;
            inputEvent.Handled = true;
        }
        else if (inputEvent.Key == ScrollDownKey && CanScrollDown)
        {
            Top += 1;
            inputEvent.Handled = true;
        }
    }

    private bool IsScrollable => ControlContext.Size.Height > ViewportSize.Height;
    private bool CanScrollUp => IsScrollable && _top > 0;
    private bool CanScrollDown => IsScrollable && _top < ControlContext.Size.Height - ViewportSize.Height;

    /// <summary>Scrolls the frame's content by <paramref name="n"/> rows (negative up, positive down).</summary>
    public void Scroll(int n) => Top += n;

    /// <summary>
    /// Re-runs the frame's child layout — re-checks whether the wrapped control is <see cref="IScrollable"/> and
    /// re-establishes its size limits.
    /// </summary>
    /// <remarks>
    /// Needed after a change that alters how the child should be sized but does not itself change the child's size
    /// (so no redraw bubbles up to trigger a relayout): e.g. swapping a composite's content between a scrollable
    /// control and a non-scrolling one.
    /// </remarks>
    public void Relayout() => Initialize();

    /// <summary>Re-lays-out the frame on the UI thread: recomputes the border offset and re-establishes the wrapped control's size limits and scroll position.</summary>
    protected override void Initialize()
    {
        UI.Invoke(() =>
        {
            UpdateBorderOffsetField();
            using (Freeze())
            {
                var totalOffset = borderOffset;

                // Available space for control (excluding scrollbar for now)
                // We reserve 1 column for scrollbar at the right of control
                var controlLimitsMin = MinSize.AsRect().Remove(totalOffset).Size;
                var controlLimitsMax = MaxSize.AsRect().Remove(totalOffset).Size;
                       
                // Allow infinite height for scrolling, but constrain width to make space for scrollbar
                // If MaxSize.Width is infinite, we don't constrain width (except by MinSize/Control)
                // But we generally want to fit in MaxSize.                        
                // Only an IScrollable is scrolled by its frame, and only it pays for the scrollbar column. Anything
                // else gets the full width and the bounded viewport height, so it sizes to the visible area — which
                // is what a control that fits its space, or manages its own viewport (Log, DataTable,
                // TerminalEmulator), wants. Opting in is the deliberate act; doing nothing is safe.
                var scrolls = _control is IScrollable;
                var limitWidth = Math.Max(0, controlLimitsMax.Width - (scrolls ? 1 : 0));

                // A scrollable child gets unbounded height so it can grow past the viewport and be scrolled; its
                // IScrollable.MeasureHeight then decides the real height (see Control.CalculateSize).
                var limitHeight = scrolls ? int.MaxValue : Math.Max(0, controlLimitsMax.Height);

                ControlContext?.SetLimits(
                    new Size(Math.Max(0, controlLimitsMin.Width - 1), Math.Max(0, controlLimitsMin.Height)),
                    new Size(limitWidth, limitHeight)
                );

                // Clamp Top
                var viewportHeight = Math.Max(0, Size.Height - totalOffset.Top - totalOffset.Bottom);

                // Note: Size.Height is current size. During Resize sequence, this might be stale?
                // VerticalScrollPanel uses Size.Height (which is current).
                // But here we are about to Resize.
                // If we are about to Resize to MaxSize (if control is large), then viewport will be larger.
                // Let's rely on Redraw loop?
                // Actually, we should probably use 'controlLimitsMax.Height' as the viewport constraint if we are expanding?
                // But MaxSize might be infinite.
                // Let's stick to simple clamping against current control size vs current viewport estimate?
                // Or just allow Top to be set, and Resize will clip?
                if (ControlContext != null)
                {
                    var controlHeight = ControlContext.Size.Height;
                    
                    // If we expand to MaxSize, the viewport height will be at most MaxSize - Offsets.
                    var maxViewportHeight = Math.Max(0, MaxSize.Height - totalOffset.Top - totalOffset.Bottom);

                    // If MaxSize is infinite, maxViewportHeight is infinite?
                    if (MaxSize.Height == int.MaxValue) maxViewportHeight = int.MaxValue;
                            
                    // Actual viewport height used for clamping depends on what size we WILL be.
                    // But we don't know yet.
                    // However, 'Top' only matters if we are scrolling.
                    // We scroll if ControlHeight > ViewportHeight.
                            
                    _top = Math.Max(0, Math.Min(_top, controlHeight - 1)); // Ensure at least within control? 

                    // Better: _top = Math.Max(0, Math.Min(_top, controlHeight - (currentViewportHeight)));
                    // But we don't know currentViewportHeight easily before Resize.
                }
                        
                ControlContext?.SetOffset(new Vector(totalOffset.Left, totalOffset.Top - _top));    
                var controlSize = ControlContext?.Size ?? Size.Empty;

                        
                // Calculate desired size including margins, borders, and — only for an IScrollable — the scrollbar
                // column. A non-scrolling control reserves none (it draws its own within its width, or has none), so
                // don't widen the frame for it: that would leave a blank gutter down the right.
                var desiredControlSize = controlSize.Expand(scrolls ? 1 : 0, 0);
                var sizeRect = desiredControlSize.AsRect().Add(totalOffset);    
                Resize(Size.Clip(MinSize, sizeRect.Size, MaxSize));
                        
                // Re-clamp Top after resize? 
                // If we resized, Size.Height is now updated (if Resize is immediate? No, Resize schedules/updates Size property).
                // Actually 'Control.Resize' updates 'Size' immediately in ConsoleGUI?
                // Checking ConsoleGUI source (mental): Resize usually updates Size.
                        
                // Post-Resize Clamping:
                if (ControlContext != null)
                {
                        viewportHeight = Math.Max(0, Size.Height - totalOffset.Top - totalOffset.Bottom);
                        if (ControlContext.Size.Height > viewportHeight)
                        {
                            _top = Math.Min(ControlContext.Size.Height - viewportHeight, Math.Max(0, _top));
                            // Update offset again with clamped Top?
                            ControlContext.SetOffset(new Vector(totalOffset.Left, totalOffset.Top - _top));
                        }
                        else
                        {
                            _top = 0;
                            ControlContext.SetOffset(new Vector(totalOffset.Left, totalOffset.Top));
                        }
                }
            }            
        });        
    }

    private bool TitleAtTop => _titleStyle.Pos is TitlePos.TopLeft or TitlePos.TopCenter or TitlePos.TopRight;

    private Offset GetBorderOffset()
    {
        var borderOffset = _borderPlacement.AsOffset();

        // The Double title style reserves two extra rows (title row + separator row) inside the title's
        // border. The Inline style draws the title within the existing single border row, so needs no extra space.
        var titleEdgeBorder = TitleAtTop ? CBorderPlacement.Top : CBorderPlacement.Bottom;
        if (!string.IsNullOrEmpty(Title) && _borderPlacement.HasBorder(titleEdgeBorder)
            && _titleStyle.BorderStyle == TitleBorderStyle.Double)
        {
            borderOffset = TitleAtTop
                ? new Offset(borderOffset.Left, borderOffset.Top + 2, borderOffset.Right, borderOffset.Bottom)
                : new Offset(borderOffset.Left, borderOffset.Top, borderOffset.Right, borderOffset.Bottom + 2);
        }

        return new Offset(
            borderOffset.Left + Margin.Left,
            borderOffset.Top + Margin.Top,
            borderOffset.Right + Margin.Right,
            borderOffset.Bottom + Margin.Bottom);
    }

   

    private void UpdateBorderOffsetField() => borderOffset = GetBorderOffset();

    private Size GetViewportSize()
    {
        var totalOffset = borderOffset;
        return new Size(
            Math.Max(0, Size.Width - totalOffset.Left - totalOffset.Right),
            Math.Max(0, Size.Height - totalOffset.Top - totalOffset.Bottom));
    }


    private static SpectreBoxBorder GetSpectreBoxBorder(BorderStyle style)
    {
        return style switch
        {
            BorderStyle.Ascii => SpectreBoxBorder.Ascii,
            BorderStyle.Double => SpectreBoxBorder.Double,
            BorderStyle.Heavy => SpectreBoxBorder.Heavy,
            BorderStyle.Rounded => SpectreBoxBorder.Rounded,
            BorderStyle.Square => SpectreBoxBorder.Square,
            BorderStyle.None => SpectreBoxBorder.None,
            _ => throw new ArgumentOutOfRangeException(nameof(style), style, null)
        };
    }

    private Cell GetBorderCell(BoxBorderPart part)
    {
        // While the frame shows its focus cue (directly focused OR containing a focused descendant), draw the focused
        // shape (when the theme supplies one) and prefer the focused border colour.
        var box = _focusCueVisible && _focusedBoxBorder is { } focusedBox ? focusedBox : _boxBorder;
        var str = box.GetPart(part);
        var ch = string.IsNullOrEmpty(str) ? ' ' : str[0];

        var character = new Character(ch);

        var fg = (_focusCueVisible ? _focusedBorderFgColor : null) ?? _borderFgColor ?? _foreground;
        if (fg.HasValue) character = character.WithForeground(fg.Value);

        var bg = _borderBgColor ?? _background;
        if (bg.HasValue) character = character.WithBackground(bg.Value);

        return new Cell(character);
    }

    /// <summary>
    /// Re-applies theme defaults to every themeable property the caller has <em>not</em> explicitly overridden
    /// (tracked via <see cref="ThemeOverrides"/>), then re-lays-out.
    /// </summary>
    /// <remarks>
    /// Invoked on a runtime theme switch through <see cref="UI.ThemeChanged"/> (wired by the owning control's
    /// <see cref="Control.Frame"/> setter).
    /// </remarks>
    public void ApplyTheme() => UI.Invoke(() =>
    {
        if (!_themeOverrides.IsOverridden(nameof(BorderStyle)))
        {
            _borderStyle = UI.StyleTheme.FrameBorder;
            _boxBorder = GetSpectreBoxBorder(_borderStyle);
        }
        if (!_themeOverrides.IsOverridden(nameof(Foreground))) _foreground = UI.StyleTheme.TitleText.ForegroundColor;
        if (!_themeOverrides.IsOverridden(nameof(BorderFgColor))) _borderFgColor = UI.StyleTheme.BorderText.ForegroundColor;
        if (!_themeOverrides.IsOverridden(nameof(TitleStyle))) _titleStyle = UI.StyleTheme.TitleStyle;
        if (!_themeOverrides.IsOverridden(nameof(ScrollBarGlyphs))) _scrollBarGlyphs = UI.GlyphTheme.ScrollBar;
        if (!_themeOverrides.IsOverridden(nameof(ScrollBarStyle))) _scrollBarStyle = UI.StyleTheme.ScrollBar;
        CaptureFocusStyle();
        RecomposeScrollBar();
        Initialize();
    });

    internal void OnThemeChanged(object? sender, EventArgs e) => ApplyTheme();

    // Captures the focus appearance from the theme: the optional focused border shape (null = no shape change) and
    // the focused border colour. The shape honours an explicit FocusedBorderStyle override (so a control that opted
    // out of the focus border keeps it across theme switches); the colour is theme-only.
    private void CaptureFocusStyle()
    {
        if (!_themeOverrides.IsOverridden(nameof(FocusedBorderStyle)))
        {
            _focusedBorderStyle = UI.StyleTheme.FocusedFrameBorder;
            _focusedBoxBorder = _focusedBorderStyle is { } style ? GetSpectreBoxBorder(style) : null;
        }
        _focusedBorderFgColor = UI.StyleTheme.BorderFocusedText.ForegroundColor;
    }

    // Rebuilds the scrollbar part cells from the current glyphs (glyph theme) and styles (style theme). Composes both
    // the classic four-part cells and the smooth-bar solid thumb/track blocks + raw thumb/track colours, so the
    // render path only reads precomputed values.
    private void RecomposeScrollBar()
    {
        _scrollBarSmooth = _scrollBarGlyphs.Mode == ScrollBarMode.Smooth;

        _scrollBarForeground = Compose(_scrollBarGlyphs.Thumb, _scrollBarStyle.Thumb);
        _scrollBarBackground = Compose(_scrollBarGlyphs.Track, _scrollBarStyle.Track);
        _scrollBarUpArrow = Compose(_scrollBarGlyphs.UpArrow, _scrollBarStyle.UpArrow);
        _scrollBarDownArrow = Compose(_scrollBarGlyphs.DownArrow, _scrollBarStyle.DownArrow);

        // Smooth-bar colours + solid cells. A fully-covered cell is drawn as a space with the colour in the
        // BACKGROUND (not a '█' glyph) so it fills seamlessly regardless of the terminal font — some render stacked
        // full blocks with vertical seams, which turns a solid track into a dashed line. Only the fractional edge
        // cells (below) use eighth-block glyphs, where a tiny seam on the moving edge is imperceptible.
        _thumbColor = _scrollBarStyle.Thumb.ForegroundColor is { } tf ? tf.ToConsoleGUIColor() : null;
        _trackColor = _scrollBarStyle.Track.ForegroundColor is { } rf ? rf.ToConsoleGUIColor() : null;
        var deco = _scrollBarStyle.Thumb.SpectreConsoleStyle?.Decoration ?? Spectre.Console.Decoration.None;
        _scrollBarDeco = deco == Spectre.Console.Decoration.None ? null : (Decoration)deco;
        _scrollBarThumbFull = new Character(' ', null, _thumbColor, _scrollBarDeco);
        _scrollBarTrackFull = new Character(' ', null, _trackColor, _scrollBarDeco);
    }

    // The scrollbar cell at viewport row <paramref name="relY"/> (0 at the top), given the viewport and content
    // heights. Dispatches to the smooth block bar or the classic three-part bar.
    private Character ScrollBarCell(int relY, int viewportHeight, int controlHeight)
    {
        var maxScroll = controlHeight - viewportHeight;
        var currentScroll = Math.Clamp(_top, 0, maxScroll);

        if (_scrollBarSmooth)
            return SmoothScrollBarCell(relY, viewportHeight, controlHeight, currentScroll, maxScroll);

        // Classic: end arrows, then a whole-cell thumb on the inner track.
        if (relY == 0) return _scrollBarUpArrow;
        if (relY == viewportHeight - 1) return _scrollBarDownArrow;

        var trackHeight = viewportHeight - 2;
        if (trackHeight > 0)
        {
            var thumbSize = Math.Max(1, (int)((long)trackHeight * viewportHeight / controlHeight));
            var availableTrack = trackHeight - thumbSize;
            var thumbStart = 1 + (maxScroll > 0 ? (int)((long)currentScroll * availableTrack / maxScroll) : 0);
            if (relY >= thumbStart && relY < thumbStart + thumbSize) return _scrollBarForeground;
        }
        return _scrollBarBackground;
    }

    // A smooth bar cell: the thumb spans the full [0, size) column with a fractional length/position, and the
    // rows where its top/bottom edge falls mid-cell are drawn with an eighth-block so the thumb glides smoothly.
    private Character SmoothScrollBarCell(int relY, int size, int controlHeight, int currentScroll, int maxScroll)
    {
        // Fractional thumb: length proportional to the visible fraction (min one cell), top proportional to scroll.
        double thumbLen = Math.Clamp((double)size * size / controlHeight, 1.0, size);
        double top = maxScroll > 0 ? currentScroll / (double)maxScroll * (size - thumbLen) : 0.0;
        double bottom = top + thumbLen;

        double covered = Math.Min(relY + 1, bottom) - Math.Max(relY, top);   // thumb coverage of this cell (0..1)
        if (covered <= 0.0) return _scrollBarTrackFull;
        if (covered >= 1.0) return _scrollBarThumbFull;

        var eighths = (int)Math.Round(covered * 8);
        if (eighths <= 0) return _scrollBarTrackFull;
        if (eighths >= 8) return _scrollBarThumbFull;

        // The thumb's top edge sits in this cell -> it fills the LOWER part: a lower block in thumb colour over the
        // track. Otherwise its bottom edge sits here -> it fills the UPPER part: draw the leftover track as a lower
        // block (track colour) over a thumb background, so the thumb shows above it.
        return top > relY
            ? new Character(Eighths[eighths], _thumbColor, _trackColor, _scrollBarDeco)
            : new Character(Eighths[8 - eighths], _trackColor, _thumbColor, _scrollBarDeco);
    }

    // Composes a single scrollbar cell: the first cell of the glyph string carrying the token's fg/bg/decoration.
    private static Character Compose(string glyph, Style style)
    {
        char? content = string.IsNullOrEmpty(glyph) ? ' ' : glyph[0];
        ConsoleGUI.Data.Color? fg = style.ForegroundColor is { } f ? f.ToConsoleGUIColor() : null;
        ConsoleGUI.Data.Color? bg = style.BackgroundColor is { } b ? b.ToConsoleGUIColor() : null;
        var spectreDecoration = style.SpectreConsoleStyle?.Decoration ?? Spectre.Console.Decoration.None;
        Decoration? decoration = spectreDecoration == Spectre.Console.Decoration.None ? null : (Decoration)spectreDecoration;
        return new Character(content, fg, bg, decoration);
    }
    /// <summary>
    /// Returns the title <see cref="Cell"/> for column <paramref name="x"/> on the title row, or
    /// <c>null</c> when the column falls outside the (aligned) title span. <paramref name="left"/> and
    /// <paramref name="right"/> are the frame's left/right edge columns (including any borders).
    /// </summary>
    private Cell? GetTitleCell(int x, int left, int right)
    {
        if (string.IsNullOrEmpty(Title)) return null;

        // Inline titles are padded with a space on each side so they read clearly against the border line.
        var display = _titleStyle.BorderStyle == TitleBorderStyle.Inline ? $" {Title} " : Title;

        // The title is laid out within the columns between the vertical borders (when present).
        var innerLeft = _borderPlacement.HasBorder(CBorderPlacement.Left) ? left + 1 : left;
        var innerRight = _borderPlacement.HasBorder(CBorderPlacement.Right) ? right - 1 : right;
        var innerWidth = innerRight - innerLeft + 1;
        if (innerWidth <= 0) return null;

        var length = Math.Min(display.Length, innerWidth);
        var start = _titleStyle.Pos switch
        {
            TitlePos.TopRight or TitlePos.BottomRight => innerRight - length + 1,
            TitlePos.TopCenter or TitlePos.BottomCenter => innerLeft + (innerWidth - length) / 2,
            _ => innerLeft // TopLeft, BottomLeft
        };

        var index = x - start;
        if (index < 0 || index >= length) return null;

        var character = new Character(display[index]);
        if (_titleStyle.Color == TitleColorStyle.Reverse)
        {
            // Swap the border colors: the title foreground takes the border background and vice versa.
            var borderFg = _borderFgColor ?? _foreground;
            var borderBg = _borderBgColor ?? _background;
            if (borderBg.HasValue) character = character.WithForeground(borderBg.Value);
            if (borderFg.HasValue) character = character.WithBackground(borderFg.Value);
        }
        else
        {
            if (_foreground.HasValue) character = character.WithForeground(_foreground.Value);
            if (_background.HasValue) character = character.WithBackground(_background.Value);
        }
        return new Cell(character);
    }

    private void _Redraw() => UI.Invoke(Redraw);

    private void BindControl()
    {
        if (Control != null)
            ControlContext = new DrawingContext(this, Control);
        else
            ControlContext = DrawingContext.Dummy;
    }

    // --- Scrollbar mouse interaction: drag the thumb (captures so the drag survives leaving the 1-col bar), click
    // the classic end arrows to step, click the track above/below the thumb to page. ---

    private bool TryGetScrollMetrics(out int controlTop, out int viewportHeight, out int controlHeight)
    {
        controlTop = borderOffset.Top;
        var controlBottom = Size.Height - 1 - borderOffset.Bottom;
        viewportHeight = controlBottom - controlTop + 1;
        controlHeight = ControlContext?.Size.Height ?? 0;
        return Control != null && viewportHeight > 0 && controlHeight > viewportHeight;
    }

    // Thumb/track geometry in viewport-row units, matching the render math in ScrollBarCell / SmoothScrollBarCell.
    private (int TrackTop, int TrackLen, double ThumbTop, double ThumbLen) ScrollbarGeometry(int viewportHeight, int controlHeight)
    {
        var maxScroll = Math.Max(0, controlHeight - viewportHeight);
        var scroll = Math.Clamp(_top, 0, maxScroll);
        if (_scrollBarSmooth)
        {
            var thumbLen = Math.Clamp((double)viewportHeight * viewportHeight / controlHeight, 1.0, viewportHeight);
            var thumbTop = maxScroll > 0 ? scroll / (double)maxScroll * (viewportHeight - thumbLen) : 0.0;
            return (0, viewportHeight, thumbTop, thumbLen);
        }

        var trackLen = Math.Max(0, viewportHeight - 2);   // classic: rows between the two end arrows
        var thumbSize = trackLen > 0 ? Math.Max(1, (int)((long)trackLen * viewportHeight / controlHeight)) : 0;
        var available = trackLen - thumbSize;
        var thumbStart = 1 + (maxScroll > 0 ? (int)((long)scroll * available / maxScroll) : 0);
        return (1, trackLen, thumbStart, thumbSize);
    }

    void IMouseListener.OnMouseEnter() { }

    void IMouseListener.OnMouseLeave() { }

    void IMouseListener.OnMouseMove(Position position)
    {
        if (!_scrollDragging) return;
        if (!TryGetScrollMetrics(out var controlTop, out var vh, out var ch)) return;
        var (trackTop, trackLen, _, thumbLen) = ScrollbarGeometry(vh, ch);
        var available = trackLen - thumbLen;
        if (available <= 0) return;
        var desiredThumbTop = (position.Y - controlTop) - _scrollGrabOffset;
        Top = (int)Math.Round((desiredThumbTop - trackTop) / available * (ch - vh));
    }

    void IMouseListener.OnMouseDown(Position position)
    {
        if (!TryGetScrollMetrics(out var controlTop, out var vh, out var ch)) return;
        var relY = position.Y - controlTop;
        if (relY < 0 || relY >= vh) return;

        // Classic end arrows: single-row step.
        if (!_scrollBarSmooth && relY == 0) { Top -= 1; return; }
        if (!_scrollBarSmooth && relY == vh - 1) { Top += 1; return; }

        var (_, _, thumbTop, thumbLen) = ScrollbarGeometry(vh, ch);
        var thumbFirst = (int)Math.Floor(thumbTop);
        var thumbLast = (int)Math.Ceiling(thumbTop + thumbLen) - 1;

        if (relY >= thumbFirst && relY <= thumbLast)
        {
            _scrollDragging = true;
            _scrollGrabOffset = relY - thumbTop;   // grab point within the thumb, so the drag doesn't jump
            ConsoleGUI.ConsoleManager.CaptureMouse(this);
        }
        else if (relY < thumbFirst) Top -= Math.Max(1, vh - 1);   // page up
        else Top += Math.Max(1, vh - 1);                          // page down
    }

    void IMouseListener.OnMouseUp(Position position)
    {
        if (!_scrollDragging) return;
        _scrollDragging = false;
        ConsoleGUI.ConsoleManager.ReleaseMouseCapture();
    }

    void IMouseWheelListener.OnMouseWheel(Position position, int delta) => Scroll(delta);
    #endregion

    #region Events
    /// <summary>Raised when the frame gains focus.</summary>
    public event FocusableEventHandler? OnFocus;
    /// <summary>Raised when the frame loses focus.</summary>
    public event FocusableEventHandler? OnLostFocus;
    /// <summary>Raised after the vertical scroll position (<see cref="Top"/>) changes.</summary>
    public event EventHandler? Scrolled;
    #endregion

    #region Fields
    private bool _scrollDragging;
    private double _scrollGrabOffset;
    /// <summary>The default frame margin (zero on all sides) used when none is supplied.</summary>
    public static Offset DefaultMargin { get; } = new Offset(0, 0, 0, 0);
    private SpectreBoxBorder _boxBorder;
    private BorderStyle _borderStyle;
    private Control _control;
    private CBorderPlacement _borderPlacement = CBorderPlacement.All;
    private Offset borderOffset;
    private Offset _margin;
    private Color? _foreground;
    private Color? _background;
    private Color? _borderFgColor;
    private Color? _borderBgColor;
    // Focus appearance (captured from the theme by CaptureFocusStyle): an optional focused border shape (null = keep
    // the resting shape) and the focused border colour, applied on the render path while focused (see GetBorderCell).
    private BorderStyle? _focusedBorderStyle;
    private SpectreBoxBorder? _focusedBoxBorder;
    // Whether the border currently shows its focus cue (see ContainsFocus). Cached so OnFocusChanged repaints only on
    // an actual flip, and so GetBorderCell reads a value consistent with the last repaint.
    private bool _focusCueVisible;
    private Color? _focusedBorderFgColor;
    private DrawingContext _controlContext = DrawingContext.Dummy;
    private string? _title;
    private TitleStyle _titleStyle = TitleStyle.Default;
    private int _top;
    
    // Tracks which themeable properties were explicitly set, so a runtime theme switch re-themes only the rest.
    private readonly ThemeOverrides _themeOverrides = new();
    // Source glyphs (from the glyph theme) and colours (from the style theme); the four Character fields below
    // are composed from them by RecomposeScrollBar and are what the renderer reads.
    private ScrollBarGlyphs _scrollBarGlyphs;
    private ScrollBarStyle _scrollBarStyle;
    private Character _scrollBarForeground;
    private Character _scrollBarBackground;
    private Character _scrollBarUpArrow;
    private Character _scrollBarDownArrow;
    // Smooth-bar state, also precomputed by RecomposeScrollBar: whether the smooth renderer is active, the solid
    // thumb/track blocks, and the raw thumb/track colours + decoration the eighth-block edge cells blend.
    private bool _scrollBarSmooth;
    private Character _scrollBarThumbFull;
    private Character _scrollBarTrackFull;
    private Color? _thumbColor;
    private Color? _trackColor;
    private Decoration? _scrollBarDeco;
    #endregion

    #region Types
    // Lower eighth-blocks indexed by eighths filled from the bottom (0 = empty, 8 = full block); used for the
    // fractional thumb edges. Fully-covered cells use a background-filled space instead (see RecomposeScrollBar).
    private static readonly char[] Eighths = [' ', '▁', '▂', '▃', '▄', '▅', '▆', '▇', '█'];
    #endregion
}
