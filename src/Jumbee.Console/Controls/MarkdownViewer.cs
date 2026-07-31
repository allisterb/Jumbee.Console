namespace Jumbee.Console;

using System;
using System.Threading.Tasks;

using ConsoleGUI.Input;
using ConsoleGUI.Space;

using NTokenizers.Extensions.Spectre.Console;
using NTokenizers.Extensions.Spectre.Console.Styles;

/// <summary>
/// A read-only, scrollable Markdown viewer. Renders CommonMark — headings, bold/italic, block-quotes, ordered and
/// unordered lists, links, syntax-highlighted fenced code blocks, and box-drawn tables — via NTokenizers'
/// Spectre.Console markdown writer. Wrap it in a <see cref="ControlFrame"/> (e.g. <c>viewer.WithFrame()</c>) to get a
/// border, title and scrollbar; ↑/↓, PgUp/PgDn, Home/End and the mouse wheel scroll it.
/// </summary>
/// <remarks>
/// The markdown parse/render is comparatively slow, so it runs on a background thread: setting <see cref="Markdown"/>
/// or resizing never blocks the UI thread, and the view fills in when the render completes. Content is re-rendered
/// only when the text/styles change or the width changes (it reflows to the control width).
/// </remarks>
public class MarkdownViewer : Control
{
    #region Constructors
    /// <summary>Initializes a <see cref="MarkdownViewer"/> with the given Markdown source.</summary>
    public MarkdownViewer(string markdown = "") => _markdown = markdown ?? "";
    #endregion

    #region Properties
    /// <summary>The Markdown source. Setting it re-renders (off the UI thread) and re-lays-out.</summary>
    public string Markdown
    {
        get => _markdown;
        set => UI.Invoke(() =>
        {
            var v = value ?? "";
            if (v == _markdown) return;
            _markdown = v;
            _version++;
            Initialize();
        });
    }

    /// <summary>The render styles (heading / code / table colours, …). Defaults to <see cref="MarkdownStyles.Default"/>.</summary>
    public MarkdownStyles? Styles
    {
        get => _styles;
        set => UI.Invoke(() => { _styles = value; _version++; Initialize(); });
    }

    /// <summary>Always <see langword="true"/>: the viewer handles its own scroll keys. No opt-in needed — unlike
    /// the base default, this is on.</summary>
    public override bool HandlesInput => true;
    #endregion

    #region Methods
    /// <inheritdoc/>
    // A viewer paints its own content; don't overlay the themed focus tint over the whole document.
    protected override bool RendersOwnFocus => true;

    /// <inheritdoc/>
    protected internal override HelpInfo? GetHelpInfo() => new HelpInfo("Markdown", "Markdown Viewer",
        "A read-only, scrollable Markdown viewer.")
        .WithKey("↑ / ↓", "Scroll a line")
        .WithKey("PgUp / PgDn", "Scroll a page")
        .WithKey("Home / End", "Top / bottom");

    // Our content height at this width. Rendering (once per width/text) yields the true height; until the render for
    // this width is ready, report a rough estimate so a surrounding frame allocates a sensible height (replaced when
    // the render completes and re-lays-out). Consulted only when the parent leaves the height unbounded (scrolling).
    /// <inheritdoc/>
    protected override int MeasureHeight(int width)
    {
        var w = Math.Max(1, width);
        EnsureRender(w);
        return _renderedWidth == w && _renderedVersion == _version
            ? Math.Max(1, _contentHeight)
            : EstimateHeight();
    }

    /// <inheritdoc/>
    protected override void Render()
    {
        EnsureRender(Math.Max(1, Size.Width));
        consoleBuffer.Initialize();
        Blit();
    }

    /// <inheritdoc/>
    protected override void OnInput(InputEvent inputEvent)
    {
        if (Frame is not { } frame) return;
        var page = Math.Max(1, frame.ViewportSize.Height - 1);
        switch (inputEvent.Key.Key)
        {
            case ConsoleKey.DownArrow: frame.Scroll(1); inputEvent.Handled = true; break;
            case ConsoleKey.UpArrow: frame.Scroll(-1); inputEvent.Handled = true; break;
            case ConsoleKey.PageDown: frame.Scroll(page); inputEvent.Handled = true; break;
            case ConsoleKey.PageUp: frame.Scroll(-page); inputEvent.Handled = true; break;
            case ConsoleKey.Home: frame.Top = 0; inputEvent.Handled = true; break;
            case ConsoleKey.End: frame.Top = int.MaxValue / 2; inputEvent.Handled = true; break;   // clamps to the bottom
        }
    }

    // Kicks off (or reuses) a render at `width` for the current text/styles. Runs on a background thread while the UI
    // is live so a slow parse never stalls a frame; runs synchronously when headless (tests / first layout) so the
    // result is available immediately to the MeasureHeight that requested it.
    //
    // At most one background render is in flight at a time (the `_rendering` gate). A width/version change while a
    // render runs is simply dropped here; when that render completes, ApplyAndContinue re-lays-out, and the resulting
    // measure/paint calls back in to start exactly one follow-up at the now-current width/version. This serialization
    // is what makes the ping-pong buffer reuse (`_back`) race-free: only the one in-flight render writes `_back`, and
    // `_back` is never the front buffer the UI thread blits. It also collapses a resize storm's many concurrent
    // renders into one-at-a-time targeting only the latest size.
    private void EnsureRender(int width)
    {
        if (width <= 0) return;
        if (_renderedWidth == width && _renderedVersion == _version) return;   // already up to date

        if (!UI.IsRunning)
        {
            // Headless: render synchronously into the back buffer and publish it, so the MeasureHeight that requested
            // this reads _contentHeight straight back.
            var version = _version;
            var height = RenderMarkdown(_markdown, _styles ?? MarkdownStyles.Default, width, _back);
            Publish(height, width, version);
            return;
        }

        if (_rendering) return;   // one already in flight; its completion re-evaluates and starts the next if needed
        StartRender(width, _version);
    }

    private void StartRender(int width, int version)
    {
        _rendering = true;
        var text = _markdown;
        var styles = _styles ?? MarkdownStyles.Default;   // shared singleton (see MarkdownStyles.Default — do not mutate)
        var target = _back;   // the spare buffer; never the front (_content) the UI thread blits
        Task.Run(() =>
        {
            var height = RenderMarkdown(text, styles, width, target);
            UI.Post(() => ApplyAndContinue(height, width, version));
        });
    }

    private void ApplyAndContinue(int height, int width, int version)
    {
        _rendering = false;
        // Discard a render superseded by a newer text/style; only publish one that still matches the current version.
        if (version == _version && width > 0)
            Publish(height, width, version);
        // Re-lay-out / repaint: the resulting MeasureHeight / Render calls back into EnsureRender, which (with
        // _rendering now clear) starts exactly one follow-up render if the current width/version isn't yet satisfied.
        Initialize();
        Invalidate();
    }

    // Publishes the just-rendered back buffer as the displayed content, swapping the old front buffer into the spare
    // slot for the next render to reuse. Capacity-retentive ConsoleBuffer.Resize then makes subsequent same-or-smaller
    // renders allocation-free — instead of a fresh multi-row Cell[][] per render.
    private void Publish(int height, int width, int version)
    {
        (_content, _back) = (_back, _content);
        _contentHeight = Math.Max(1, height);
        _renderedWidth = width;
        _renderedVersion = version;
    }

    /// <summary>Discards the cached render so the next layout re-renders — for a subclass that adds render inputs
    /// (e.g. diagram styles) beyond <see cref="Markdown"/>/<see cref="Styles"/>. Call on the UI thread.</summary>
    protected void InvalidateContent() { _version++; Initialize(); }

    // Renders the markdown into the reusable `target` buffer at `width` and returns its measured content height. The
    // caller owns `target` and reuses it across renders (see Publish), so this resizes it in place rather than
    // allocating. Resilient: any failure in the third-party writer yields a blank buffer rather than throwing.
    // Instance-virtual so a subclass can post-process the document (e.g. rasterize embedded diagrams); the base body
    // uses only its arguments (no instance state), so it stays safe to call on the background render thread.
    /// <summary>Renders <paramref name="text"/> into <paramref name="target"/> at <paramref name="width"/> using
    /// <paramref name="styles"/> and returns the measured content height. Overridable to post-process the document.</summary>
    protected virtual int RenderMarkdown(string text, MarkdownStyles styles, int width, ConsoleBuffer target)
    {
        // Height budget: paragraphs word-wrap to `width`, so a long paragraph spans several rows — estimate from the
        // text length over the width plus the explicit line count, so wrapped content isn't clipped at the buffer's
        // bottom (capacity-retentive, so an over-estimate costs nothing after the first render).
        var cap = Math.Clamp(text.Length / Math.Max(1, width) + LineCount(text) * 2 + 40, 8, MaxRows);
        target.Size = new Size(width, cap);   // capacity-retentive: no realloc once the high-water mark is reached
        target.Initialize();
        try
        {
            var console = new AnsiConsoleBuffer(target) { wrapWords = true };   // paragraphs reflow to `width`
            console.WriteMarkdown(text, styles);
            return MeasureRenderedHeight(target);
        }
        catch
        {
            return 1;
        }
    }

    // The last non-blank row + 1 — the true rendered height (the writer's cursor isn't a reliable end marker across
    // every renderable, so scan the buffer).
    private static int MeasureRenderedHeight(ConsoleBuffer buffer)
    {
        for (var y = buffer.Size.Height - 1; y >= 0; y--)
            for (var x = 0; x < buffer.Size.Width; x++)
            {
                var c = buffer[x, y].Character.Content;
                if (c is not null && c != ' ' && c != '\0') return y + 1;
            }
        return 1;
    }

    private void Blit()
    {
        var src = _content;
        var h = Math.Min(consoleBuffer.Size.Height, src.Size.Height);
        var w = Math.Min(consoleBuffer.Size.Width, src.Size.Width);
        for (var y = 0; y < h; y++)
            for (var x = 0; x < w; x++)
                consoleBuffer.Write(new Position(x, y), src[x, y]);
    }

    private int EstimateHeight() => Math.Clamp(LineCount(_markdown), 1, MaxRows);

    private static int LineCount(string text)
    {
        if (string.IsNullOrEmpty(text)) return 1;
        var n = 1;
        foreach (var c in text) if (c == '\n') n++;
        return n;
    }
    #endregion

    #region Fields
    // The rendered content is capped at this many rows — beyond the control's own ~1000-row size clamp nothing is
    // reachable anyway, so a taller document simply clips at the bottom.
    /// <summary>The maximum number of rows the rendered document is capped at.</summary>
    protected const int MaxRows = 1024;

    private string _markdown;
    private MarkdownStyles? _styles;
    private int _version;                       // bumped when the text/styles change, invalidating a cached render

    // Ping-pong pair: _content is the front buffer blitted into consoleBuffer each paint (read on the UI thread);
    // _back is the spare the next render writes into (the in-flight render's target). Publish swaps them. Reusing
    // these across renders keeps ConsoleBuffer's capacity-retention alive, so a render no longer allocates a fresh
    // Cell[][] every call.
    private ConsoleBuffer _content = new();
    private ConsoleBuffer _back = new();
    private int _contentHeight;
    private int _renderedWidth = -1;            // (width, version) the cached _content was rendered for
    private int _renderedVersion = -1;
    private bool _rendering;                     // true while the single background render is in flight
    #endregion
}
