namespace Jumbee.Console.Snapshot;

using System.Text;

using ConsoleGUI;
using ConsoleGUI.Common;
using ConsoleGUI.Data;
using ConsoleGUI.Input;
using ConsoleGUI.Space;

using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using CColor = ConsoleGUI.Data.Color;
using Color = SixLabors.ImageSharp.Color;
using Size = ConsoleGUI.Space.Size;
using JControl = Jumbee.Console.Control;

/// <summary>
/// Renders Jumbee.Console controls headlessly (without a real terminal) to a <see cref="ConsoleBuffer"/>,
/// and converts that buffer to a text or PNG snapshot. Intended for tests and visual verification.
/// </summary>
public static class ConsoleSnapshot
{
    #region Render (headless compose)
    /// <summary>
    /// Composes a control tree into a <see cref="ConsoleBuffer"/> at the given size, without a real console.
    /// </summary>
    public static ConsoleBuffer Render(IControl content, int width, int height)
    {
        // Drive layout: assigning a context + limits initializes the tree (sizes propagate down).
        using var ctx = new DrawingContext(NoopListener.Instance, content);
        ctx.SetLimits(new Size(width, height), new Size(width, height));

        // Paint once so every control renders into its own buffer.
        UI.PaintFrame();

        var size = ctx.Size;
        var w = size.Width > 0 ? size.Width : width;
        var h = size.Height > 0 ? size.Height : height;

        var buffer = new ConsoleBuffer { Size = new Size(w, h) };
        for (var y = 0; y < h; y++)
        {
            for (var x = 0; x < w; x++)
            {
                var pos = new Position(x, y);
                buffer.Write(pos, ctx.Contains(pos) ? ctx[pos] : EmptyCell);
            }
        }

        return buffer;
    }

    /// <summary>Composes a single control (using its frame when present) into a buffer.</summary>
    public static ConsoleBuffer Render(JControl control, int width, int height)
        => Render((IControl)control.FocusableControl, width, height);

    /// <summary>Composes a layout into a buffer.</summary>
    public static ConsoleBuffer Render(ILayout layout, int width, int height)
        => Render(layout.CControl, width, height);

    /// <summary>
    /// Renders <paramref name="control"/> once to establish layout, sends the given keys to it (routed via
    /// <see cref="UI.SendInput(IFocusable, ConsoleKeyInfo, bool)"/>), then renders and returns the result.
    /// </summary>
    /// <remarks>Handy for snapshotting a control after navigation/editing. The keys are delivered to
    /// <paramref name="control"/> itself — <em>not</em> to whatever <see cref="UI.SetFocus"/> last targeted
    /// elsewhere in the tree — so pass the control that actually changes. For a composite app, that's the specific
    /// child under test (e.g. the list), not the root layout.</remarks>
    public static ConsoleBuffer RenderAfter(JControl control, int width, int height, params ConsoleKey[] keys)
        => RenderAfterCore(control, width, height, keys.Select(k => Key(k)));

    /// <summary>
    /// As <see cref="RenderAfter(JControl, int, int, ConsoleKey[])"/> but accepts full key info, so modifier
    /// keys (e.g. <c>Alt+Down</c> via <see cref="Key"/>) can be sent. When <paramref name="routeGlobal"/> is
    /// <see langword="true"/>, each key runs the global hotkey dispatch first (see
    /// <see cref="UI.SendInput(IFocusable, ConsoleKeyInfo, bool)"/>) so a snapshot can exercise hotkeys registered
    /// with <see cref="UI.RegisterHotKey"/> — build the keys the same way they were registered (e.g. with
    /// <see cref="UI.HotKeys"/>). As with the other overload, the keys go to <paramref name="control"/> itself,
    /// not to whatever <see cref="UI.SetFocus"/> designates.
    /// </summary>
    public static ConsoleBuffer RenderAfter(JControl control, int width, int height, IReadOnlyList<ConsoleKeyInfo> keys, bool routeGlobal = false)
        => RenderAfterCore(control, width, height, keys, routeGlobal);

    /// <summary>As <see cref="RenderAfter(JControl, int, int, ConsoleKey[])"/> but for a whole layout, so a
    /// key-driven multi-control screen (e.g. a header plus a plot) can be snapshotted as one unit. The keys go to
    /// <paramref name="layout"/> itself.</summary>
    public static ConsoleBuffer RenderAfter(ILayout layout, int width, int height, params ConsoleKey[] keys)
        => RenderAfterCore(layout, width, height, keys.Select(k => Key(k)));

    /// <summary>As <see cref="RenderAfter(JControl, int, int, IReadOnlyList{ConsoleKeyInfo}, bool)"/> but for a
    /// whole layout. With <paramref name="routeGlobal"/> each key runs the global hotkey dispatch first — the usual
    /// case for a layout, whose behaviour is driven by <see cref="UI.RegisterHotKey"/> rather than a single focused
    /// child.</summary>
    public static ConsoleBuffer RenderAfter(ILayout layout, int width, int height, IReadOnlyList<ConsoleKeyInfo> keys, bool routeGlobal = false)
        => RenderAfterCore(layout, width, height, keys, routeGlobal);

    /// <summary>Builds a <see cref="ConsoleKeyInfo"/> for a key with optional modifiers. For letter and digit keys
    /// the <c>KeyChar</c> is filled in (lowercase, uppercase under Shift, the control char under Ctrl) so the result
    /// matches how a hotkey registered with <see cref="UI.RegisterHotKey"/> — or a real keystroke — is keyed. That
    /// matters for <see cref="RenderAfter(JControl, int, int, IReadOnlyList{ConsoleKeyInfo}, bool)"/> with
    /// <c>routeGlobal</c>: a bare-letter global hotkey only fires when the simulated key's char matches. Non-character
    /// keys (arrows, function keys, …) keep <c>'\0'</c>. For a punctuation hotkey (e.g. <c>'/'</c>), this method's
    /// char is <c>'\0'</c> and won't match — use <c>UI.HotKeys.Char('/')</c> to build the key instead.</summary>
    public static ConsoleKeyInfo Key(ConsoleKey key, bool shift = false, bool alt = false, bool control = false)
        => new(KeyChar(key, shift, control), key, shift, alt, control);

    // The char a real terminal/input decoder produces for a key, for the cases that carry one. Letters: lowercase,
    // uppercase under Shift, the C0 control char under Ctrl (matches UI.HotKeys.Ctrl). Digits: their glyph.
    private static char KeyChar(ConsoleKey key, bool shift, bool control)
    {
        if (key >= ConsoleKey.A && key <= ConsoleKey.Z)
        {
            if (control) return (char)(key - ConsoleKey.A + 1);   // Ctrl+A..Z -> ..
            var lower = (char)('a' + (key - ConsoleKey.A));
            return shift ? char.ToUpperInvariant(lower) : lower;
        }
        if (!control && key >= ConsoleKey.D0 && key <= ConsoleKey.D9) return (char)('0' + (key - ConsoleKey.D0));
        if (!control && key >= ConsoleKey.NumPad0 && key <= ConsoleKey.NumPad9) return (char)('0' + (key - ConsoleKey.NumPad0));
        return '\0';
    }

    private static ConsoleBuffer RenderAfterCore(JControl control, int width, int height, IEnumerable<ConsoleKeyInfo> keys, bool routeGlobal = false)
    {
        // Establish sizing first so input-driven behavior (e.g. auto-scroll) has a viewport to work against.
        Render(control, width, height);
        foreach (var key in keys) UI.SendInput(control, key, routeGlobal);
        return Render(control, width, height);
    }

    private static ConsoleBuffer RenderAfterCore(ILayout layout, int width, int height, IEnumerable<ConsoleKeyInfo> keys, bool routeGlobal = false)
    {
        Render(layout, width, height);
        foreach (var key in keys) UI.SendInput(layout, key, routeGlobal);
        return Render(layout, width, height);
    }
    #endregion

    #region Text snapshot
    /// <summary>Converts a buffer to a plain-text snapshot: one <c>\n</c>-terminated line per row, each row
    /// <b>right-trimmed of trailing spaces</b> (so snapshots are stable regardless of right-padding). Because rows are
    /// trimmed, a flat <c>index → (index % width, index / width)</c> back-mapping to buffer coordinates is wrong — use
    /// <see cref="ToLines(ConsoleBuffer)"/> to index by row, or <see cref="GlyphAt"/> for a specific cell. Colour and
    /// text decoration are NOT captured, so state distinguished only by colour (e.g. a dimmed "read" row) is invisible
    /// to a text assertion — use <see cref="ForegroundAt"/>/<see cref="BackgroundAt"/>, or <c>ToImage</c>/<c>SavePng</c>.</summary>
    public static string ToText(ConsoleBuffer buffer)
    {
        var sb = new StringBuilder();
        foreach (var line in ToLines(buffer)) sb.Append(line).Append('\n');
        return sb.ToString();
    }

    /// <summary>The buffer as one right-trimmed string per row — the safe way to index a text snapshot by row
    /// (<see cref="ToText(ConsoleBuffer)"/> is exactly these joined by, and terminated with, <c>\n</c>). Because rows
    /// are right-trimmed of trailing spaces, a flat <c>index → (index % width, index / width)</c> mapping is wrong;
    /// index the row here, then the column within it. Colour and decoration are not captured — use
    /// <see cref="GlyphAt"/>/<see cref="ForegroundAt"/>/<see cref="BackgroundAt"/> for per-cell checks that need no
    /// row arithmetic at all.</summary>
    public static string[] ToLines(ConsoleBuffer buffer)
    {
        var lines = new string[buffer.Size.Height];
        var sb = new StringBuilder();
        for (var y = 0; y < buffer.Size.Height; y++)
        {
            sb.Clear();
            for (var x = 0; x < buffer.Size.Width; x++)
            {
                var ch = buffer[x, y].Content;
                sb.Append(ch is null or '\0' ? ' ' : ch.Value);
            }
            // Trim trailing spaces so snapshots are stable regardless of right-padding.
            while (sb.Length > 0 && sb[^1] == ' ') sb.Length--;
            lines[y] = sb.ToString();
        }
        return lines;
    }

    /// <summary>Renders a control and returns its text snapshot.</summary>
    public static string ToText(JControl control, int width, int height) => ToText(Render(control, width, height));

    /// <summary>Renders a layout and returns its text snapshot.</summary>
    public static string ToText(ILayout layout, int width, int height) => ToText(Render(layout, width, height));

    /// <summary>Renders a control after sending the given keys and returns its text snapshot.</summary>
    public static string ToTextAfter(JControl control, int width, int height, params ConsoleKey[] keys)
        => ToText(RenderAfter(control, width, height, keys));

    /// <summary>Renders a control after sending the given keys (with modifiers) and returns its text snapshot.
    /// Pass <paramref name="routeGlobal"/> to run each key through the global hotkey dispatch first (see
    /// <see cref="RenderAfter(JControl, int, int, IReadOnlyList{ConsoleKeyInfo}, bool)"/>).</summary>
    public static string ToTextAfter(JControl control, int width, int height, IReadOnlyList<ConsoleKeyInfo> keys, bool routeGlobal = false)
        => ToText(RenderAfter(control, width, height, keys, routeGlobal));

    /// <summary>Renders a layout after sending the given keys and returns its text snapshot.</summary>
    public static string ToTextAfter(ILayout layout, int width, int height, params ConsoleKey[] keys)
        => ToText(RenderAfter(layout, width, height, keys));

    /// <summary>Renders a layout after sending the given keys (with modifiers) and returns its text snapshot. Pass
    /// <paramref name="routeGlobal"/> to run each key through the global hotkey dispatch first — the usual case for a
    /// layout driven by <see cref="UI.RegisterHotKey"/>.</summary>
    public static string ToTextAfter(ILayout layout, int width, int height, IReadOnlyList<ConsoleKeyInfo> keys, bool routeGlobal = false)
        => ToText(RenderAfter(layout, width, height, keys, routeGlobal));
    #endregion

    #region Colour readback
    /// <summary>The glyph rendered at (<paramref name="x"/>, <paramref name="y"/>), or a space for an empty cell —
    /// the per-cell counterpart of <see cref="ForegroundAt"/>/<see cref="BackgroundAt"/>. Read a cell's glyph and
    /// colour together this way instead of mapping a <see cref="ToText(ConsoleBuffer)"/> index back to coordinates
    /// (rows are right-trimmed, so that mapping is error-prone).</summary>
    public static char GlyphAt(ConsoleBuffer buffer, int x, int y) =>
        buffer[x, y].Content is { } c && c != '\0' ? c : ' ';

    /// <summary>The foreground colour of the rendered cell at (<paramref name="x"/>, <paramref name="y"/>) as a
    /// <see cref="Jumbee.Console.Color"/>, or <see langword="null"/> for the terminal default — for asserting a
    /// rendered colour in a test without reaching into the buffer's internal cell type. Text snapshots
    /// (<see cref="ToText(ConsoleBuffer)"/>) drop colour, so this is how you check it without a PNG.</summary>
    public static Jumbee.Console.Color? ForegroundAt(ConsoleBuffer buffer, int x, int y) =>
        buffer[x, y].Foreground is { } c ? Jumbee.Console.Color.FromConsoleGUIColor(c) : null;

    /// <summary>The background colour of the rendered cell at (<paramref name="x"/>, <paramref name="y"/>), or
    /// <see langword="null"/> for transparent/default. See <see cref="ForegroundAt"/>.</summary>
    public static Jumbee.Console.Color? BackgroundAt(ConsoleBuffer buffer, int x, int y) =>
        buffer[x, y].Background is { } c ? Jumbee.Console.Color.FromConsoleGUIColor(c) : null;
    #endregion

    #region Image snapshot
    /// <summary>Renders a buffer to an image, drawing each cell's glyph and colors.</summary>
    public static Image<Rgba32> ToImage(ConsoleBuffer buffer, SnapshotImageOptions? options = null)
    {
        options ??= new SnapshotImageOptions();
        var family = ResolveFontFamily(options);
        var fontCache = new Dictionary<FontStyle, Font>();
        // Per-glyph fallback. ResolveFontFamily picks ONE family for the whole image, and the fallback list applied
        // only when the named family wasn't installed — so on Windows the default (Consolas: installed, and verified
        // to have no Braille coverage) always won, and every Braille cell rasterised as the same missing-glyph box.
        // Text snapshots were unaffected, so it failed silently: three separate ports hit it, one shipping a whole
        // review package whose charts could not be seen. Handing the fallbacks to the text renderer lets it
        // substitute per glyph, so Braille comes from Cascadia Mono while ordinary text stays in the chosen font.
        var glyphFallbacks = ResolveFallbackFamilies(options, family);

        var cols = buffer.Size.Width;
        var rows = buffer.Size.Height;
        var imgW = Math.Max(1, options.Padding * 2 + cols * options.CellWidth);
        var imgH = Math.Max(1, options.Padding * 2 + rows * options.CellHeight);

        var image = new Image<Rgba32>(imgW, imgH);
        image.Mutate(ctx =>
        {
            ctx.Fill(options.DefaultBackground);

            for (var y = 0; y < rows; y++)
            {
                for (var x = 0; x < cols; x++)
                {
                    var cell = buffer[x, y];
                    var px = options.Padding + x * options.CellWidth;
                    var py = options.Padding + y * options.CellHeight;

                    var decoration = cell.Character.Decoration ?? Decoration.None;

                    // Resolve effective colors, honoring Invert (swap fg/bg) and Dim (blend fg toward bg).
                    var hasBackground = cell.Background is not null;
                    var fg = cell.Foreground is { } f ? ToColor(f) : options.DefaultForeground;
                    var bg = cell.Background is { } b ? ToColor(b) : options.DefaultBackground;

                    if ((decoration & Decoration.Invert) != 0)
                    {
                        (fg, bg) = (bg, fg);
                        hasBackground = true; // an inverted cell always paints a background
                    }

                    if (hasBackground)
                    {
                        ctx.Fill(bg, new RectangleF(px, py, options.CellWidth, options.CellHeight));
                    }

                    if ((decoration & Decoration.Dim) != 0)
                    {
                        fg = Blend(fg, bg, 0.5f);
                    }

                    // Note: blink decorations (SlowBlink/RapidBlink) cannot be represented in a static image.
                    var ch = cell.Content;
                    var concealed = (decoration & Decoration.Conceal) != 0;
                    if (!concealed && ch is { } c && c != ' ' && c != '\0')
                    {
                        var glyphFont = GetStyledFont(family, options.FontSize, MapFontStyle(decoration), fontCache);
                        var text = c.ToString();

                        var textOptions = new RichTextOptions(glyphFont)
                        {
                            Origin = new PointF(px, py + (options.CellHeight - options.FontSize) / 2f),
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Top,
                            FallbackFontFamilies = glyphFallbacks,
                        };

                        var textDecorations = MapTextDecorations(decoration);
                        if (textDecorations != TextDecorations.None)
                        {
                            textOptions.TextRuns = [new RichTextRun { Start = 0, End = text.Length, TextDecorations = textDecorations }];
                        }

                        ctx.DrawText(textOptions, text, fg);
                    }
                }
            }
        });

        return image;
    }

    #region Mouse (headless pointer simulation)
    // The runtime path is ConsoleManager: set MousePosition (which hit-tests its composited buffer and fires
    // enter/leave/move), then flip MouseDown for press/release, or call MouseWheel. ConsoleSnapshot composes into
    // its own buffer rather than ConsoleManager's, so it can't drive that static — but the cells it produces carry
    // the same mouse listeners (Control tags them when Focusable || WantsMouse), so the same hit-test is just a
    // cell lookup. These methods reproduce ConsoleManager's dispatch order exactly.
    private static MouseContext? _mouseContext;

    // The hit-test ConsoleManager does against its private buffer, against ours.
    private static MouseContext? HitTest(ConsoleBuffer buffer, int x, int y)
    {
        var size = buffer.Size;
        if (x < 0 || y < 0 || x >= size.Width || y >= size.Height) return null;
        return buffer[x, y].MouseListener;
    }

    /// <summary>
    /// Moves the pointer to (<paramref name="x"/>, <paramref name="y"/>) in <paramref name="buffer"/>, firing
    /// enter/leave/move on the controls under the old and new positions.
    /// </summary>
    /// <remarks>Hover state persists across calls (as it does at runtime), so call <see cref="ResetMouse"/> between
    /// tests to avoid leaving a control hovered. Returns <see langword="true"/> if a control is under the pointer —
    /// a <see langword="false"/> here usually means the target doesn't opt into the mouse (see the control's
    /// <c>WantsMouse</c>) or the coordinates are outside it.</remarks>
    public static bool MouseMove(ConsoleBuffer buffer, int x, int y)
    {
        var next = HitTest(buffer, x, y);

        if (next?.MouseListener != _mouseContext?.MouseListener)
        {
            _mouseContext?.MouseListener.OnMouseLeave();
            next?.MouseListener.OnMouseEnter();
            next?.MouseListener.OnMouseMove(next.Value.RelativePosition);
        }
        else if (next.HasValue && next.Value.RelativePosition != _mouseContext?.RelativePosition)
        {
            next.Value.MouseListener.OnMouseMove(next.Value.RelativePosition);
        }

        _mouseContext = next;
        return next.HasValue;
    }

    /// <summary>
    /// Clicks at (<paramref name="x"/>, <paramref name="y"/>) in <paramref name="buffer"/>: moves the pointer there,
    /// then presses and releases <paramref name="clicks"/> times.
    /// </summary>
    /// <remarks>
    /// Pass <c>clicks: 2</c> for a double-click (the presses land within the double-click window, so a control that
    /// distinguishes them — e.g. <c>DataTable</c>'s row activation — sees a double-click). Pass
    /// <paramref name="button"/> to simulate a right-click: the dispatch itself carries only a position, so this
    /// latches <c>UI.MouseButton</c> the way the live input path does, which is what a control reads to tell the
    /// buttons apart (e.g. <c>ListBox</c> opening its <c>ContextMenu</c>). The pointer is left hovering the target
    /// afterwards, as it would be after a real click. Returns <see langword="false"/> if nothing is under it.
    /// </remarks>
    public static bool Click(ConsoleBuffer buffer, int x, int y, int clicks = 1, TerminalMouseButton button = TerminalMouseButton.Left)
    {
        if (!MouseMove(buffer, x, y) || _mouseContext is not { } context) return false;

        var previous = UI.MouseButton;
        UI.MouseButton = button;   // latched on press by the live path; controls read it from OnClick/OnDoubleClick
        try
        {
            for (var i = 0; i < clicks; i++)
            {
                context.MouseListener.OnMouseDown(context.RelativePosition);
                context.MouseListener.OnMouseUp(context.RelativePosition);
            }
        }
        finally
        {
            UI.MouseButton = previous;
        }

        return true;
    }

    /// <summary>
    /// Drags from (<paramref name="fromX"/>, <paramref name="fromY"/>) to (<paramref name="toX"/>,
    /// <paramref name="toY"/>) in <paramref name="buffer"/>: press at the start, <paramref name="steps"/> moves
    /// along the way, release at the end.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Honours mouse capture the way the live path does: if the control takes the capture on press (a splitter, a
    /// scrollbar thumb, a <c>Slider</c>), every later move and the release go to <em>it</em>, in its own frame,
    /// with no hit-test — so a drag that wanders off the control still steers it. Without that, a test drag would
    /// silently retarget whatever cell it passed over, and pass for the wrong reason.
    /// </para>
    /// <para>
    /// The capture origin is computed here from the press hit-test rather than read from <c>ConsoleManager</c>,
    /// which latches its own from a pointer position this headless path never sets. Returns <see langword="false"/>
    /// if nothing is under the start point.
    /// </para>
    /// </remarks>
    public static bool Drag(ConsoleBuffer buffer, int fromX, int fromY, int toX, int toY, int steps = 4)
    {
        if (!MouseMove(buffer, fromX, fromY) || _mouseContext is not { } start) return false;

        var origin = new Position(fromX - start.RelativePosition.X, fromY - start.RelativePosition.Y);
        Position Captured(int x, int y) => new(x - origin.X, y - origin.Y);

        start.MouseListener.OnMouseDown(start.RelativePosition);

        for (var i = 1; i <= Math.Max(1, steps); i++)
        {
            var x = fromX + ((toX - fromX) * i / Math.Max(1, steps));
            var y = fromY + ((toY - fromY) * i / Math.Max(1, steps));
            if (ConsoleGUI.ConsoleManager.MouseCapture is { } holding) holding.OnMouseMove(Captured(x, y));
            else MouseMove(buffer, x, y);
        }

        // Read the capture BEFORE the release: the handler that gets it is the one that gives it up.
        var capture = ConsoleGUI.ConsoleManager.MouseCapture;
        if (capture is not null)
        {
            capture.OnMouseUp(Captured(toX, toY));
            MouseMove(buffer, toX, toY);   // settle hover where the pointer ended up, as it would be after a real drag
        }
        else if (_mouseContext is { } end)
        {
            end.MouseListener.OnMouseUp(end.RelativePosition);
        }

        return true;
    }

    /// <summary>
    /// Rotates the wheel by <paramref name="delta"/> notches over (<paramref name="x"/>, <paramref name="y"/>) —
    /// negative scrolls up, positive scrolls down.
    /// </summary>
    /// <remarks>Only reaches controls that implement <c>IMouseWheelListener</c>; returns <see langword="false"/>
    /// otherwise (and when nothing is under the pointer).</remarks>
    public static bool Wheel(ConsoleBuffer buffer, int x, int y, int delta)
    {
        if (!MouseMove(buffer, x, y) || _mouseContext is not { } context) return false;
        if (context.MouseListener is not IMouseWheelListener wheel) return false;

        wheel.OnMouseWheel(context.RelativePosition, delta);
        return true;
    }

    /// <summary>Clears the remembered pointer position, firing a leave on whatever is currently hovered.</summary>
    /// <remarks>Call between tests: hover state is static (as at runtime), so it would otherwise carry over.</remarks>
    public static void ResetMouse()
    {
        _mouseContext?.MouseListener.OnMouseLeave();
        _mouseContext = null;
    }

    /// <summary>Renders <paramref name="layout"/>, clicks at (<paramref name="x"/>, <paramref name="y"/>), then
    /// re-renders and returns the result.</summary>
    public static ConsoleBuffer RenderAfterClick(ILayout layout, int width, int height, int x, int y, int clicks = 1)
    {
        Click(Render(layout, width, height), x, y, clicks);
        return Render(layout, width, height);
    }

    /// <summary>Renders <paramref name="control"/>, clicks at (<paramref name="x"/>, <paramref name="y"/>), then
    /// re-renders and returns the result.</summary>
    /// <remarks>Coordinates are relative to the rendered snapshot, so they include the control's own frame when it
    /// has one — a click on the first row of a bordered control's content is <c>y: 1</c>, not <c>y: 0</c>.</remarks>
    public static ConsoleBuffer RenderAfterClick(JControl control, int width, int height, int x, int y, int clicks = 1)
    {
        Click(Render(control, width, height), x, y, clicks);
        return Render(control, width, height);
    }

    /// <summary>Renders a layout, clicks, and returns the resulting screen as text.</summary>
    public static string ToTextAfterClick(ILayout layout, int width, int height, int x, int y, int clicks = 1)
        => ToText(RenderAfterClick(layout, width, height, x, y, clicks));

    /// <summary>Renders a control, clicks, and returns the resulting screen as text.</summary>
    public static string ToTextAfterClick(JControl control, int width, int height, int x, int y, int clicks = 1)
        => ToText(RenderAfterClick(control, width, height, x, y, clicks));
    #endregion

    /// <summary>Renders a buffer to a PNG file.</summary>
    public static void SavePng(ConsoleBuffer buffer, string path, SnapshotImageOptions? options = null)
    {
        var dir = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

        using var image = ToImage(buffer, options);
        image.SaveAsPng(path);
    }

    /// <summary>Renders a control and saves it to a PNG file.</summary>
    public static void SavePng(JControl control, int width, int height, string path, SnapshotImageOptions? options = null)
        => SavePng(Render(control, width, height), path, options);

    /// <summary>Renders a layout and saves it to a PNG file.</summary>
    public static void SavePng(ILayout layout, int width, int height, string path, SnapshotImageOptions? options = null)
        => SavePng(Render(layout, width, height), path, options);

    /// <summary>Renders a control after sending the given keys and saves it to a PNG file.</summary>
    public static void SavePngAfter(JControl control, int width, int height, string path, params ConsoleKey[] keys)
        => SavePng(RenderAfter(control, width, height, keys), path);

    /// <summary>Renders a control after sending the given keys (with modifiers) and saves it to a PNG file.</summary>
    /// <remarks>Pass <paramref name="routeGlobal"/> to fire hotkeys registered with <c>UI.RegisterHotKey</c>, the
    /// same as <see cref="RenderAfter(JControl, int, int, IReadOnlyList{ConsoleKeyInfo}, bool)"/>.</remarks>
    public static void SavePngAfter(JControl control, int width, int height, string path, IReadOnlyList<ConsoleKeyInfo> keys, bool routeGlobal = false)
        => SavePng(RenderAfter(control, width, height, keys, routeGlobal), path);

    /// <summary>Renders a layout after sending the given keys and saves it to a PNG file.</summary>
    public static void SavePngAfter(ILayout layout, int width, int height, string path, params ConsoleKey[] keys)
        => SavePng(RenderAfter(layout, width, height, keys), path);

    /// <summary>Renders a layout after sending the given keys (with modifiers) and saves it to a PNG file.</summary>
    /// <remarks>Use this to capture an <c>Overlay</c> — a modal's frame is only in the overlay, not in the root
    /// layout. Pass <paramref name="routeGlobal"/> to fire hotkeys registered with <c>UI.RegisterHotKey</c>.</remarks>
    public static void SavePngAfter(ILayout layout, int width, int height, string path, IReadOnlyList<ConsoleKeyInfo> keys, bool routeGlobal = false)
        => SavePng(RenderAfter(layout, width, height, keys, routeGlobal), path);
    #endregion

    #region Helpers
    private static Color ToColor(CColor c) => Color.FromRgb(c.Red, c.Green, c.Blue);

    /// <summary>Linearly blends <paramref name="a"/> toward <paramref name="b"/> by <paramref name="t"/> (0..1).</summary>
    private static Color Blend(Color a, Color b, float t)
    {
        var pa = a.ToPixel<Rgba32>();
        var pb = b.ToPixel<Rgba32>();
        static byte Lerp(byte x, byte y, float t) => (byte)Math.Clamp(x + (y - x) * t, 0, 255);
        return Color.FromRgb(Lerp(pa.R, pb.R, t), Lerp(pa.G, pb.G, t), Lerp(pa.B, pb.B, t));
    }

    private static FontStyle MapFontStyle(Decoration decoration)
    {
        var bold = (decoration & Decoration.Bold) != 0;
        var italic = (decoration & Decoration.Italic) != 0;
        return (bold, italic) switch
        {
            (true, true) => FontStyle.BoldItalic,
            (true, false) => FontStyle.Bold,
            (false, true) => FontStyle.Italic,
            _ => FontStyle.Regular
        };
    }

    private static TextDecorations MapTextDecorations(Decoration decoration)
    {
        var result = TextDecorations.None;
        if ((decoration & Decoration.Underline) != 0) result |= TextDecorations.Underline;
        if ((decoration & Decoration.Strikethrough) != 0) result |= TextDecorations.Strikeout;
        return result;
    }

    /// <summary>Creates (and caches) a font in the requested style, degrading gracefully when the family lacks it.</summary>
    private static Font GetStyledFont(FontFamily family, float size, FontStyle style, Dictionary<FontStyle, Font> cache)
    {
        if (cache.TryGetValue(style, out var cached)) return cached;

        var available = family.GetAvailableStyles().ToHashSet();
        var resolved = style switch
        {
            _ when available.Contains(style) => style,
            FontStyle.BoldItalic when available.Contains(FontStyle.Bold) => FontStyle.Bold,
            FontStyle.BoldItalic when available.Contains(FontStyle.Italic) => FontStyle.Italic,
            _ => FontStyle.Regular
        };

        var font = family.CreateFont(size, resolved);
        cache[style] = font;
        return font;
    }

    private static FontFamily ResolveFontFamily(SnapshotImageOptions options)
    {
        if (TryGetFamily(options.FontFamily, out var family) ||
            options.FallbackFontFamilies.Any(name => TryGetFamily(name, out family)))
        {
            return family;
        }

        // Last resort: first installed family (may not be monospace, but avoids a hard failure).
        var any = SystemFonts.Families.FirstOrDefault();
        if (any == default)
        {
            throw new InvalidOperationException("No system fonts are available to render the snapshot image.");
        }
        return any;
    }

    private static bool TryGetFamily(string name, out FontFamily family) => SystemFonts.TryGet(name, out family);

    // The installed families from FallbackFontFamilies, minus the one already chosen as primary. Handed to the text
    // renderer so a glyph the primary font lacks (Braille, box-drawing, an emoji) is drawn from one that has it
    // rather than as a missing-glyph box.
    private static FontFamily[] ResolveFallbackFamilies(SnapshotImageOptions options, FontFamily primary)
    {
        var resolved = new List<FontFamily>();
        foreach (var name in options.FallbackFontFamilies)
        {
            if (TryGetFamily(name, out var f) && f != primary && !resolved.Contains(f)) resolved.Add(f);
        }
        return [.. resolved];
    }

    private static readonly Cell EmptyCell = new Cell(' ');

    private sealed class NoopListener : IDrawingContextListener
    {
        public static readonly NoopListener Instance = new();
        public void OnRedraw(DrawingContext drawingContext) { }
        public void OnUpdate(DrawingContext drawingContext, Rect rect) { }
    }
    #endregion
}
