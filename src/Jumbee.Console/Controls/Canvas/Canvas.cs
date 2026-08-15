namespace Jumbee.Console;

using System;
using System.Collections.Generic;

using ConsoleGUI.Input;
using ConsoleGUI.Space;

using Jumbee.Console.Drawing;

using Spectre.Console.Interop;
using Spectre.Console.Rendering;

using Character = ConsoleGUI.Data.Character;
using Decoration = ConsoleGUI.Data.Decoration;
using CColor = ConsoleGUI.Data.Color;

/// <summary>
/// A blank drawing surface on which you paint <see cref="IShape"/>s (<see cref="Line"/>, <see cref="Rectangle"/>,
/// <see cref="Circle"/>, <see cref="Points"/>, <see cref="FilledLine"/>) in an arbitrary coordinate system, rendered
/// with sub-cell markers (braille by default).
/// </summary>
/// <remarks>
/// Ported from Ratatui's canvas widget.
/// <para>Shapes accumulate in a retained list: <see cref="Add"/> appends to the current layer, <see cref="Layer()"/>
/// starts a new one (composited top-down per property, so a higher layer's glyph/colour wins each cell while lower
/// layers show through where it doesn't paint), and <see cref="Clear"/> empties the surface. Layers may use different
/// markers via <see cref="Layer(CanvasMarker)"/> — e.g. a <see cref="CanvasMarker.Block"/> backdrop showing through a
/// <see cref="CanvasMarker.Braille"/> overlay; <see cref="Marker"/> sets the starting marker. Set the visible window
/// with <see cref="XBounds"/>/<see cref="YBounds"/> — the canvas origin is the bottom-left corner. Display-only; it
/// fills its container and re-fits on resize.</para>
/// <para>A filled/area chart — the dense look of a terminal system monitor — is one
/// <see cref="FilledLine"/> per column with <c>fillToY</c> at the baseline; <c>Plot</c>'s bar methods take no Braille
/// brush, so this is the way to get it. See the Live Data guide for driving one from a running data source.</para>
/// <para><b>Snapshotting Braille to PNG:</b> the image renderer's default font (Consolas) has no Braille glyphs, so a
/// Braille canvas can rasterise as empty boxes in a PNG while rendering perfectly in the terminal and in a text
/// snapshot. If you see that, set <c>SnapshotImageOptions.FontFamily</c> to <c>"Cascadia Mono"</c> or
/// <c>"DejaVu Sans Mono"</c>.</para>
/// </remarks>
public class Canvas : Control
{
    #region Constructors
    /// <summary>Initializes a new display-only <see cref="Canvas"/> (not focusable).</summary>
    public Canvas() => Focusable = false;   // display-only
    #endregion

    #region Properties
    /// <summary>The horizontal window (left, right) of the coordinate system mapped across the control's width. Default (0, 0).</summary>
    public (double Min, double Max) XBounds
    {
        get => _xBounds;
        set => SetAtomicProperty(ref _xBounds, value, watch: (_, _) => _dirty = true);
    }

    /// <summary>The vertical window (bottom, top) of the coordinate system mapped across the control's height. Default (0, 0). The origin is bottom-left.</summary>
    public (double Min, double Max) YBounds
    {
        get => _yBounds;
        set => SetAtomicProperty(ref _yBounds, value, watch: (_, _) => _dirty = true);
    }

    /// <summary>The marker (and thus sub-cell resolution) shapes are drawn with. Default <see cref="CanvasMarker.Braille"/>.</summary>
    public CanvasMarker Marker
    {
        get => _marker;
        set => SetAtomicProperty(ref _marker, value, watch: (_, _) => _dirty = true);
    }

    /// <summary>The glyph used when <see cref="Marker"/> is <see cref="CanvasMarker.Custom"/>. Default <c>•</c>.</summary>
    public char CustomMarker
    {
        get => _customMarker;
        set => SetAtomicProperty(ref _customMarker, value, watch: (_, _) => _dirty = true);
    }

    /// <summary>Background colour painted behind every cell, or <see langword="null"/> (the default) for transparent.</summary>
    public Color? Background
    {
        get => _background;
        set => SetAtomicProperty(ref _background, value, watch: (_, _) => _dirty = true);
    }

    /// <summary>
    /// When <see langword="true"/> (the default), the canvas reports only the cells that actually changed since the
    /// last render, so the compositor skips the unchanged remainder instead of re-scanning the whole panel.
    /// </summary>
    /// <remarks>
    /// This is what makes a mostly-static canvas cheap to update — a world map whose coastline never moves but whose
    /// few markers and labels do costs about what the markers cost, not what the map costs. That is the usual canvas
    /// workload, so it is on by default. It is not free when the picture changes everywhere, though: the recorder
    /// sits in the write path, so a canvas that redraws its whole area every frame pays the bookkeeping and skips
    /// nothing — measurably a few percent worse than leaving it off. Set <see langword="false"/> for that case, or
    /// to A/B the optimization. See <see cref="Control.TracksDamage"/> for the cost model.
    /// </remarks>
    public bool DamageTracking
    {
        get => _damageTracking;
        set => SetAtomicProperty(ref _damageTracking, value, watch: (_, _) => _snapshotValid = false);
    }

    /// <summary>
    /// When <see langword="true"/>, the canvas responds to user input by panning and zooming its
    /// <see cref="XBounds"/>/<see cref="YBounds"/> window. The default (<see langword="false"/>) leaves it display-only.
    /// </summary>
    /// <remarks>
    /// <b>Drag</b> to pan (the content follows the cursor), the <b>mouse wheel</b> to zoom about the pointer, and
    /// (while focused) the <b>arrow keys</b> to pan with <b>+/-</b> to zoom about the centre (Shift = larger step).
    /// Enabling it makes the canvas focusable.
    /// </remarks>
    public bool Interactive
    {
        get => _interactive;
        set
        {
            if (_interactive == value) return;
            _interactive = value;
            Focusable = value;
            Invalidate();
        }
    }
    #endregion

    #region Methods
    /// <summary>Sets the <see cref="XBounds"/> and returns this canvas, for fluent chaining.</summary>
    public Canvas WithXBounds(double min, double max)
    {
        XBounds = (min, max);
        return this;
    }

    /// <summary>Sets the <see cref="YBounds"/> and returns this canvas, for fluent chaining.</summary>
    public Canvas WithYBounds(double min, double max)
    {
        YBounds = (min, max);
        return this;
    }

    /// <summary>Sets the <see cref="Marker"/> and returns this canvas, for fluent chaining.</summary>
    public Canvas WithMarker(CanvasMarker marker)
    {
        Marker = marker;
        return this;
    }

    /// <summary>Sets the <see cref="Background"/> and returns this canvas, for fluent chaining.</summary>
    public Canvas WithBackground(Color? background)
    {
        Background = background;
        return this;
    }

    /// <summary>Appends a shape to the current layer and redraws. Fluent.</summary>
    public Canvas Add(IShape shape)
    {
        UI.Invoke(() =>
        {
            _ops.Add(Op.Draw(shape));
            Rebuild();
        });
        return this;
    }

    /// <summary>Starts a new layer (keeping the current marker) — shapes added after this compose on top of (and can
    /// show through) earlier layers. Fluent.</summary>
    public Canvas Layer()
    {
        UI.Invoke(() =>
        {
            _ops.Add(Op.LayerBreak(null));
            Rebuild();
        });
        return this;
    }

    /// <summary>Starts a new layer drawn with <paramref name="marker"/> (which becomes the current marker for this and
    /// subsequent layers until changed again). Lets layers mix resolutions — e.g. a <see cref="CanvasMarker.Block"/>
    /// backdrop under a <see cref="CanvasMarker.Braille"/> overlay, where the block's background shows through the
    /// braille glyphs. Custom markers use the current <see cref="CustomMarker"/>. Fluent.</summary>
    public Canvas Layer(CanvasMarker marker)
    {
        UI.Invoke(() =>
        {
            _ops.Add(Op.LayerBreak(marker));
            Rebuild();
        });
        return this;
    }

    /// <summary>
    /// Prints <paramref name="text"/> at canvas coordinate (<paramref name="x"/>, <paramref name="y"/>), with its
    /// first character at that point and running right. Fluent.
    /// </summary>
    /// <remarks>
    /// Labels are always drawn on top of every layer (they are not part of a layer and are unaffected by the marker),
    /// and clipped at the right edge. <paramref name="fg"/> defaults to white; <paramref name="bg"/> is transparent
    /// when null.
    /// </remarks>
    public Canvas Print(double x, double y, string text, Color? fg = null, Color? bg = null)
    {
        CColor f = fg ?? Color.White;
        CColor? b = bg;
        var chars = new StyledChar[text?.Length ?? 0];
        for (int i = 0; i < chars.Length; i++) chars[i] = new StyledChar(text![i], f, b, Decoration.None);
        UI.Invoke(() =>
        {
            _labels.Add(new Label(x, y, chars));
            Invalidate();   // labels are drawn each render from _labels; no layer rebuild needed
        });
        return this;
    }

    /// <summary>
    /// Prints a <b>Spectre markup</b> label at canvas coordinate (<paramref name="x"/>, <paramref name="y"/>) — e.g.
    /// <c>"[red]⚠ Outage[/] [grey]Tokyo[/]"</c> — so a single label can mix colours and decorations. Fluent.
    /// </summary>
    /// <remarks>
    /// Like <see cref="Print(double, double, string, Color?, Color?)"/> it is drawn on top of every layer and clipped
    /// at the right edge. Use BMP symbols for icons (a single terminal cell can't hold surrogate-pair emoji). Invalid
    /// markup falls back to literal text.
    /// </remarks>
    public Canvas PrintMarkup(double x, double y, string markup)
    {
        var chars = ParseMarkup(markup ?? "");
        UI.Invoke(() =>
        {
            _labels.Add(new Label(x, y, chars));
            Invalidate();
        });
        return this;
    }

    // Renders Spectre markup once (at Print time) into a flat list of styled characters — no wrapping (a large max
    // width) and newlines stripped, so a label stays a single clipped line. Falls back to literal text on bad markup.
    private StyledChar[] ParseMarkup(string markup)
    {
        try
        {
            var options = RenderOptions.Create(ansiConsole);
            var result = new List<StyledChar>(markup.Length);
            IRenderable renderable = new Spectre.Console.Markup(markup);
            foreach (var segment in renderable.Render(options, 100_000))
            {
                if (segment.IsControlCode) continue;
                var style = segment.Style;
                CColor? fg = style.Foreground.ToConsoleGUIColor();
                CColor? bg = style.Background.ToConsoleGUIColor();
                var deco = (Decoration)style.Decoration;
                foreach (char c in segment.Text)
                    if (c is not '\n' and not '\r')
                        result.Add(new StyledChar(c, fg, bg, deco));
            }
            return [.. result];
        }
        catch (Exception)
        {
            // Malformed markup: render it as literal white text rather than throwing from a draw call.
            var chars = new StyledChar[markup.Length];
            for (int i = 0; i < chars.Length; i++) chars[i] = new StyledChar(markup[i], CColor.White, null, Decoration.None);
            return chars;
        }
    }

    /// <summary>Removes every shape, layer and label, leaving a blank canvas. Fluent.</summary>
    public Canvas Clear()
    {
        UI.Invoke(() =>
        {
            _ops.Clear();
            _labels.Clear();
            Rebuild();
        });
        return this;
    }

    /// <summary>
    /// Removes every label, leaving the shapes and layers intact. Fluent.
    /// </summary>
    /// <remarks>
    /// Prefer this to <see cref="Clear"/> when only the annotations change: shapes are rasterized into layers and
    /// cached, and clearing them all forces that raster to be rebuilt from scratch on the next render. For a canvas
    /// whose shapes are an unchanging backdrop — a world map, a floor plan, a chart's axes — with labels moving over
    /// it, re-adding the backdrop each update is most of the frame's cost and buys nothing.
    /// </remarks>
    public Canvas ClearLabels()
    {
        UI.Invoke(() =>
        {
            _labels.Clear();
            Invalidate();   // labels are drawn from _labels each render; the cached layers stay valid
        });
        return this;
    }

    private void Rebuild()
    {
        _dirty = true;
        Invalidate();
    }

    #region Input (pan/zoom, active only when Interactive)
    /// <summary>Receives mouse events only while <see cref="Interactive"/> (drag-pan / wheel-zoom).</summary>
    protected override bool WantsMouse => _interactive;

    // Receive keyboard input (arrows / +-) only when interactive; otherwise keys pass through for navigation.
    /// <summary>Receives keyboard input only while <see cref="Interactive"/>; otherwise keys pass through for navigation.</summary>
    public override bool HandlesInput => _interactive;

    /// <summary>Begins a drag-pan and captures the mouse when interactive.</summary>
    protected override void OnMousePress(Position position)
    {
        if (!_interactive) return;
        _dragging = true;
        _lastDrag = position;
        CaptureMouse();
    }

    /// <summary>Pans the viewport to follow the cursor while dragging.</summary>
    protected override void OnMouseMove(Position position)
    {
        if (!_dragging) return;
        Pan(position.X - _lastDrag.X, position.Y - _lastDrag.Y);
        _lastDrag = position;
    }

    /// <summary>Ends the drag-pan and releases the mouse capture.</summary>
    protected override void OnMouseRelease(Position position)
    {
        if (!_dragging) return;
        _dragging = false;
        ReleaseMouse();
    }

    /// <summary>Zooms the viewport about the cursor when interactive; otherwise defers to the base scroll behavior.</summary>
    protected override void OnMouseWheel(Position position, int delta)
    {
        if (!_interactive) { base.OnMouseWheel(position, delta); return; }
        // Wheel up (delta < 0) shrinks the window (zoom in); down grows it. Keep the point under the cursor fixed.
        ZoomAround(position.X, position.Y, Math.Pow(ZoomFactorPerNotch, delta));
    }

    /// <summary>Handles arrow-key pan and <c>+</c>/<c>-</c> zoom when interactive.</summary>
    protected override void OnInput(InputEvent inputEvent)
    {
        if (!_interactive) return;
        bool shift = (inputEvent.Key.Modifiers & ConsoleModifiers.Shift) != 0;
        double pan = shift ? 0.4 : 0.15;
        switch (inputEvent.Key.Key)
        {
            case ConsoleKey.LeftArrow: PanFraction(-pan, 0); break;
            case ConsoleKey.RightArrow: PanFraction(pan, 0); break;
            case ConsoleKey.UpArrow: PanFraction(0, pan); break;
            case ConsoleKey.DownArrow: PanFraction(0, -pan); break;
            case ConsoleKey.Add or ConsoleKey.OemPlus: ZoomCentre(shift ? 0.6 : 0.8); break;   // + zooms in
            case ConsoleKey.Subtract or ConsoleKey.OemMinus: ZoomCentre(shift ? 1.0 / 0.6 : 1.0 / 0.8); break;   // - zooms out
            default: return;
        }
        inputEvent.Handled = true;
    }

    // Drag pan: the world point under the cursor follows it. X shifts opposite the drag; Y shifts with it, since the
    // canvas y-axis is flipped (origin bottom-left) versus the screen's top-down rows.
    private void Pan(int dxCells, int dyCells)
    {
        int w = Size.Width, h = Size.Height;
        var (xMin, xMax) = _xBounds;
        var (yMin, yMax) = _yBounds;
        double spanX = xMax - xMin, spanY = yMax - yMin;
        if (w <= 1 || h <= 1 || spanX <= 0 || spanY <= 0) return;
        double wx = dxCells * spanX / (w - 1);
        double wy = dyCells * spanY / (h - 1);
        XBounds = (xMin - wx, xMax - wx);
        YBounds = (yMin + wy, yMax + wy);
    }

    // Keyboard pan: move the viewport by a fraction of its span (Right/Up move the window toward +x/+y).
    private void PanFraction(double fx, double fy)
    {
        var (xMin, xMax) = _xBounds;
        var (yMin, yMax) = _yBounds;
        double spanX = xMax - xMin, spanY = yMax - yMin;
        if (spanX <= 0 || spanY <= 0) return;
        double dx = fx * spanX, dy = fy * spanY;
        XBounds = (xMin + dx, xMax + dx);
        YBounds = (yMin + dy, yMax + dy);
    }

    // Zoom by scaling the span while keeping the world point under (px, py) fixed on screen.
    private void ZoomAround(int px, int py, double scale)
    {
        int w = Size.Width, h = Size.Height;
        var (xMin, xMax) = _xBounds;
        var (yMin, yMax) = _yBounds;
        double spanX = xMax - xMin, spanY = yMax - yMin;
        if (w <= 1 || h <= 1 || spanX <= 0 || spanY <= 0) return;
        double fx = px / (double)(w - 1), fy = py / (double)(h - 1);
        double curX = xMin + fx * spanX;
        double curY = yMax - fy * spanY;   // screen y is flipped
        double nsx = spanX * scale, nsy = spanY * scale;
        double nxMin = curX - fx * nsx;
        double nyMax = curY + fy * nsy;
        XBounds = (nxMin, nxMin + nsx);
        YBounds = (nyMax - nsy, nyMax);
    }

    // Zoom about the window centre (keyboard +/-).
    private void ZoomCentre(double scale)
    {
        var (xMin, xMax) = _xBounds;
        var (yMin, yMax) = _yBounds;
        double spanX = xMax - xMin, spanY = yMax - yMin;
        if (spanX <= 0 || spanY <= 0) return;
        double cx = (xMin + xMax) / 2, cy = (yMin + yMax) / 2;
        double hx = spanX * scale / 2, hy = spanY * scale / 2;
        XBounds = (cx - hx, cx + hx);
        YBounds = (cy - hy, cy + hy);
    }
    #endregion

    // Deliberately NOT IScrollable: a canvas fills its container and re-fits on resize, so it is sized to the
    // viewport rather than scrolled.

    /// <summary>Rebuilds the composited layers when needed and blits them (with labels) to the buffer.</summary>
    protected override void Render()
    {
        int w = Size.Width, h = Size.Height;
        if (w <= 0 || h <= 0) return;

        // Rebuild the composited layers when the content changed or the control was resized.
        if (_dirty || _layers is null || _builtWidth != w || _builtHeight != h)
        {
            _dirty = false;
            _builtWidth = w;
            _builtHeight = h;
            _layers = BuildLayers(w, h);
        }

        consoleBuffer.Initialize();
        Blit(_layers, w, h);        // diffs the layer content against the last frame and damages what moved
        DrawLabels(w, h);           // damages each label's old and new footprint
        FinishDamage(w, h);
    }

    /// <summary>Whether partial-redraw damage tracking is enabled (see <see cref="DamageTracking"/>).</summary>
    protected override bool TracksDamage => _damageTracking;

    // Damage is reported from the two places that write cells — Blit (layers) and DrawLabels — rather than by
    // re-reading the finished buffer afterwards. An after-the-fact readback of every cell was the obvious way to do
    // it and cost ~100us per render on a 120x20 map, which ate a third of what the skipped composite saved: the
    // point of the API is to touch FEWER cells, so paying a full extra pass to find that out defeats it. Both writers
    // already visit exactly the cells they change, so the diff rides along for almost nothing.

    // A cell of blitted layer content — what Blit resolves before it writes. Compared against the previous frame's.
    // Deliberately a plain struct with a hand-written comparison: a record struct's synthesized equality routes each
    // field through EqualityComparer<T>.Default, i.e. three virtual calls per cell, which cost ~20us per frame on a
    // 120x20 map — a third of what skipping the composite saves. The lifted == operators below compile to inline
    // compares instead.
    private readonly struct Cel(char? sym, CColor? fg, CColor? bg)
    {
        private readonly char? _sym = sym;
        private readonly CColor? _fg = fg;
        private readonly CColor? _bg = bg;

        public bool Matches(char? s, CColor? f, CColor? b) => _sym == s && _fg == f && _bg == b;
    }

    // True when there is a previous frame to diff against; otherwise this render reports everything once.
    private bool _canDiff;

    private bool BeginDamage(int count, int w)
    {
        if (!_damageTracking) { _snapshotValid = false; return false; }
        bool resized = _cells is null || _cells.Length < count || _snapshotWidth != w;
        if (resized) { _cells = new Cel[count]; _snapshotWidth = w; }
        _canDiff = _snapshotValid && !resized;
        return true;
    }

    private void FinishDamage(int w, int h)
    {
        if (!_damageTracking) return;
        if (!_canDiff) { DamageAll(); }   // first render, a resize, or tracking just turned on
        _snapshotValid = true;
    }

    private List<Layer> BuildLayers(int width, int height)
    {
        var context = new CanvasContext(width, height, _xBounds, _yBounds, _marker, _customMarker);
        foreach (var op in _ops)
        {
            if (op.Shape is not null) context.Draw(op.Shape);
            else if (op.NewMarker is CanvasMarker m) context.Marker(m, _customMarker);
            else context.Layer();
        }
        context.Finish();
        return [.. context.Layers];
    }

    // Composites the layers into the buffer. Each of symbol/foreground/background is taken from the top-most layer
    // that supplies it (per-property merge, so a lower layer's background can show under a higher layer's glyph);
    // cells with nothing set fall back to the canvas Background, or stay blank when that is null.
    private void Blit(IReadOnlyList<Layer> layers, int width, int height)
    {
        int count = width * height;
        EnsureWorkArrays(count);
        Array.Clear(_sym, 0, count);
        Array.Clear(_fg, 0, count);
        Array.Clear(_bg, 0, count);

        foreach (var layer in layers)
        {
            var contents = layer.Contents;
            int n = Math.Min(count, contents.Length);
            for (int i = 0; i < n; i++)
            {
                var cell = contents[i];
                if (cell.Symbol.HasValue) _sym[i] = cell.Symbol;
                if (cell.Fg.HasValue) _fg[i] = cell.Fg;
                if (cell.Bg.HasValue) _bg[i] = cell.Bg;
            }
        }

        bool diff = BeginDamage(count, width);
        for (int y = 0, i = 0; y < height; y++)
        {
            int first = -1, last = -1;
            for (int x = 0; x < width; x++, i++)
            {
                char? symbol = _sym[i];
                CColor? fg = _fg[i];
                CColor? bg = _bg[i] ?? _background;

                if (diff)
                {
                    // Compare and record as we go — this loop already resolves every cell, so the diff is a few
                    // field compares on top, not a second pass over the panel.
                    if (_canDiff && !_cells![i].Matches(symbol, fg, bg)) { if (first < 0) first = x; last = x; }
                    _cells![i] = new Cel(symbol, fg, bg);
                }

                if (symbol is null && fg is null && bg is null) continue;   // leave the blank cell Initialize wrote
                consoleBuffer.Write(new Position(x, y), new Character(symbol ?? ' ', fg, bg));
            }
            // One rect per changed row, spanning its first-to-last change: canvas changes are runs (a label) or
            // points (a marker), so a row span bounds them tightly and there are at most `height` rects.
            if (first >= 0) Damage(new Rect(first, y, last - first + 1, 1));
        }
    }

    private void EnsureWorkArrays(int count)
    {
        if (_sym.Length >= count) return;
        _sym = new char?[count];
        _fg = new CColor?[count];
        _bg = new CColor?[count];
    }

    // Draws the retained labels on top of the composited layers. A label maps to a cell with the same y-flip as the
    // shapes but at cell resolution (w-1, h-1) — matching ratatui — and its text runs right from that cell, clipped
    // to the buffer's right edge.
    private void DrawLabels(int width, int height)
    {
        // Labels sit on top of the layers, so the layer diff in Blit can't see them: a label that moved away leaves
        // blit content that is identical to last frame's, reporting nothing, and the old text would stay on screen.
        // Damage where the labels WERE as well as where they are now — the union-of-old-and-new rule any moving
        // region has to follow here.
        bool trackLabels = _damageTracking && _canDiff;
        if (trackLabels) foreach (var r in _prevLabelRects) Damage(r);
        _prevLabelRects.Clear();

        if (_labels.Count == 0) return;
        var (left, right) = _xBounds;
        var (bottom, top) = _yBounds;
        double spanX = Math.Abs(right - left), spanY = Math.Abs(top - bottom);
        if (spanX <= 0 || spanY <= 0) return;
        double resX = width - 1, resY = height - 1;

        foreach (var label in _labels)
        {
            if (label.X < left || label.X > right || label.Y < bottom || label.Y > top) continue;
            int x = (int)((label.X - left) * resX / spanX);
            int y = (int)((top - label.Y) * resY / spanY);
            if (y < 0 || y >= height) continue;
            var chars = label.Chars;
            for (int col = x, k = 0; k < chars.Length; k++, col++)
            {
                if (col < 0) continue;
                if (col >= width) break;
                var sc = chars[k];
                consoleBuffer.Write(new Position(col, y), new Character(sc.C, sc.Fg, sc.Bg, sc.Deco));
            }

            if (!_damageTracking) continue;
            int from = Math.Max(x, 0), to = Math.Min(x + chars.Length - 1, width - 1);
            if (to < from) continue;
            var rect = new Rect(from, y, to - from + 1, 1);
            _prevLabelRects.Add(rect);
            if (trackLabels) Damage(rect);
        }
    }
    #endregion

    #region Child types
    // An entry in the retained op list: a shape to draw on the current layer, a plain layer break, or a layer break
    // that also switches the marker for the new layer.
    private readonly struct Op
    {
        private Op(IShape? shape, CanvasMarker? newMarker)
        {
            Shape = shape;
            NewMarker = newMarker;
        }

        public IShape? Shape { get; }                 // non-null => draw this shape
        public CanvasMarker? NewMarker { get; }       // set (with Shape null) => layer break switching to this marker

        public static Op Draw(IShape shape) => new(shape, null);
        public static Op LayerBreak(CanvasMarker? marker) => new(null, marker);
    }

    // One styled character of a label (a plain label is all one style; a markup label carries per-run styles).
    private readonly struct StyledChar
    {
        public StyledChar(char c, CColor? fg, CColor? bg, Decoration deco)
        {
            C = c;
            Fg = fg;
            Bg = bg;
            Deco = deco;
        }

        public char C { get; }
        public CColor? Fg { get; }
        public CColor? Bg { get; }
        public Decoration Deco { get; }
    }

    // A text label anchored at a canvas coordinate, drawn on top of all layers (see DrawLabels).
    private readonly struct Label
    {
        public Label(double x, double y, StyledChar[] chars)
        {
            X = x;
            Y = y;
            Chars = chars;
        }

        public double X { get; }
        public double Y { get; }
        public StyledChar[] Chars { get; }
    }
    #endregion

    #region Fields
    // The ordered op list of shapes and layer breaks (see Op); replayed each rebuild.
    private readonly List<Op> _ops = [];
    // Text labels drawn on top of every layer each render (not part of the layer/marker system).
    private readonly List<Label> _labels = [];
    private List<Layer>? _layers;
    private (double Min, double Max) _xBounds;
    private (double Min, double Max) _yBounds;
    private CanvasMarker _marker = CanvasMarker.Braille;
    private char _customMarker = '•';
    private CColor? _background;
    private bool _dirty = true;

    // Interactive pan/zoom state. ZoomFactorPerNotch > 1: a wheel-down notch grows the window (zoom out).
    private const double ZoomFactorPerNotch = 1.1;
    private bool _interactive;
    private bool _dragging;
    private Position _lastDrag;
    private int _builtWidth = -1;
    private int _builtHeight = -1;

    // Reused per-property compositing scratch (grown to the largest buffer seen), cleared each blit.
    private char?[] _sym = [];
    private CColor?[] _fg = [];
    private CColor?[] _bg = [];
    private bool _damageTracking = true;
    // Last frame's blitted layer content, diffed in Blit to report damage. Invalid until the first full report.
    private Cel[]? _cells;
    private int _snapshotWidth = -1;
    private bool _snapshotValid;
    // Where the labels were drawn last frame, so a label that moves damages the cells it vacated.
    private readonly List<Rect> _prevLabelRects = [];
    #endregion
}
