namespace Jumbee.Console.Documents;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using ConsoleGUI.Input;
using ConsoleGUI.Space;

using Mermaider;

using Jumbee.Console.Documents.Mermaid;

/// <summary>
/// A read-only, scrollable viewer for Mermaid <c>flowchart</c>/<c>graph</c>, <c>stateDiagram</c>,
/// <c>classDiagram</c>, <c>erDiagram</c> and <c>sequenceDiagram</c> diagrams.
/// </summary>
/// <remarks>
/// Parses
/// and lays out the diagram with Mermaider, then rasterizes the positioned graph to box-drawing cells — node boxes,
/// rectilinear edges with arrowheads and labels, and subgraph groups. Wrap it in a <see cref="ControlFrame"/> for a
/// border, title and scrollbar; ↑/↓, PgUp/PgDn, Home/End and the wheel scroll it.
/// <para>
/// Parsing/layout runs on a background thread so setting <see cref="Mermaid"/> never blocks the UI thread; the view
/// fills in when the render completes. Diagram types beyond those listed above are not supported yet and show a
/// short message. The diagram is drawn at its intrinsic size and clips horizontally if wider than the control.
/// </para>
/// </remarks>
public class MermaidViewer : Control
{
    #region Constructors
    /// <summary>Initializes a new <see cref="MermaidViewer"/> showing <paramref name="mermaid"/>.</summary>
    public MermaidViewer(string mermaid = "") => _mermaid = mermaid ?? "";
    #endregion

    #region Properties
    /// <summary>The Mermaid source. Setting it re-parses/re-renders (off the UI thread) and re-lays-out.</summary>
    public string Mermaid
    {
        get => _mermaid;
        set => UI.Invoke(() =>
        {
            var v = value ?? "";
            if (v == _mermaid) return;
            _mermaid = v;
            _version++;
            _hscroll.Reset();   // a new diagram starts scrolled to the left edge
            Initialize();
        });
    }

    /// <summary>The render colours / scale. Defaults to <see cref="MermaidStyles.Default"/>.</summary>
    public MermaidStyles? Styles
    {
        get => _styles;
        set => UI.Invoke(() => { _styles = value; _version++; _hscroll.Reset(); Initialize(); });
    }

    /// <summary>Always <see langword="true"/> — the viewer handles scroll/pan keys.</summary>
    public override bool HandlesInput => true;
    #endregion

    #region Methods
    /// <inheritdoc/>
    protected override bool RendersOwnFocus => true;

    /// <inheritdoc/>
    protected override HelpInfo? GetHelpInfo() => new HelpInfo("Mermaid", "Mermaid Viewer",
        "A read-only, scrollable Mermaid diagram viewer.")
        .WithKey("↑ / ↓", "Scroll a line")
        .WithKey("← / →", "Pan horizontally")
        .WithKey("PgUp / PgDn", "Scroll a page")
        .WithKey("Home / End", "Top / bottom");

    /// <inheritdoc/>
    protected override int MeasureHeight(int width)
    {
        EnsureRender();
        return _renderedVersion == _version ? Math.Max(1, _contentHeight) : EstimateHeight();
    }

    /// <inheritdoc/>
    protected override void Render()
    {
        EnsureRender();
        consoleBuffer.Initialize();
        Blit();
    }

    /// <inheritdoc/>
    protected override void OnInput(InputEvent inputEvent)
    {
        // Horizontal panning is control-managed (Blit offset via HScroll), so it works with or without a frame.
        switch (inputEvent.Key.Key)
        {
            case ConsoleKey.LeftArrow:
                if (_hscroll.Pan(-HScrollStep, _content.Size.Width, consoleBuffer.Size.Width)) Invalidate();
                inputEvent.Handled = true; return;
            case ConsoleKey.RightArrow:
                if (_hscroll.Pan(HScrollStep, _content.Size.Width, consoleBuffer.Size.Width)) Invalidate();
                inputEvent.Handled = true; return;
        }

        // Vertical scrolling is frame-driven.
        if (Frame is not { } frame) return;
        var page = Math.Max(1, frame.ViewportSize.Height - 1);
        switch (inputEvent.Key.Key)
        {
            case ConsoleKey.DownArrow: frame.Scroll(1); inputEvent.Handled = true; break;
            case ConsoleKey.UpArrow: frame.Scroll(-1); inputEvent.Handled = true; break;
            case ConsoleKey.PageDown: frame.Scroll(page); inputEvent.Handled = true; break;
            case ConsoleKey.PageUp: frame.Scroll(-page); inputEvent.Handled = true; break;
            case ConsoleKey.Home: frame.Top = 0; _hscroll.Reset(); Invalidate(); inputEvent.Handled = true; break;
            case ConsoleKey.End: frame.Top = int.MaxValue / 2; inputEvent.Handled = true; break;
        }
    }

    // Parse+layout+rasterize depend only on (text, styles), not the control width, so the render is cached by
    // version alone. Background while the UI is live; synchronous when headless (tests / first layout).
    private void EnsureRender()
    {
        if (_renderedVersion == _version) return;
        if (_renderingVersion == _version) return;

        var version = _version;
        var text = _mermaid;
        var styles = _styles ?? MermaidStyles.Default;
        _renderingVersion = version;

        if (!UI.IsRunning)
        {
            var (buffer, height) = RenderDiagram(text, styles);
            Apply(buffer, height, version, relayout: false);
            return;
        }

        Task.Run(() =>
        {
            var (buffer, height) = RenderDiagram(text, styles);
            UI.Post(() => Apply(buffer, height, version, relayout: true));
        });
    }

    private void Apply(ConsoleBuffer buffer, int height, int version, bool relayout)
    {
        _renderingVersion = -1;
        if (version == _version)
        {
            _content = buffer;
            _contentHeight = Math.Max(1, height);
            _renderedVersion = version;
        }
        if (relayout) { Initialize(); Invalidate(); }
    }

    // Parses, lays out, and rasterizes the diagram into a fresh buffer sized to the diagram. Resilient: a parse
    // failure or unsupported diagram type yields a one-line message buffer rather than throwing.
    private static (ConsoleBuffer buffer, int height) RenderDiagram(string text, MermaidStyles styles)
    {
        try
        {
            var canvas = MermaidCanvas.Build(text, styles);
            var height = Math.Min(canvas.Height, MaxRows);
            var buffer = new ConsoleBuffer { Size = new Size(Math.Max(1, canvas.Width), Math.Max(1, height)) };
            buffer.Initialize();
            canvas.Blit(buffer);
            return (buffer, height);
        }
        catch (MermaidParseException ex)
        {
            return RenderMessage(ex.Message);
        }
        catch
        {
            return RenderMessage("Unable to render Mermaid diagram.");
        }
    }

    private static (ConsoleBuffer buffer, int height) RenderMessage(string message)
    {
        var w = Math.Max(1, message.Length);
        var buffer = new ConsoleBuffer { Size = new Size(w, 1) };
        buffer.Initialize();
        var fg = Color.Red.ToConsoleGUIColor();
        for (var i = 0; i < message.Length; i++)
            buffer.Write(new Position(i, 0), new ConsoleGUI.Data.Character(message[i], fg, null, null));
        return (buffer, 1);
    }

    private void Blit()
    {
        var src = _content;
        var w = consoleBuffer.Size.Width;
        var left = _hscroll.Clamp(src.Size.Width, w);   // re-clamp in case a resize widened the viewport
        var h = Math.Min(consoleBuffer.Size.Height, src.Size.Height);
        for (var y = 0; y < h; y++)
            for (var x = 0; x < w; x++)
            {
                var sx = x + left;
                if (sx < src.Size.Width) consoleBuffer.Write(new Position(x, y), src[sx, y]);
            }
    }

    private int EstimateHeight()
    {
        var n = 1;
        foreach (var c in _mermaid) if (c == '\n') n++;
        return Math.Clamp(n * 3, 3, MaxRows);
    }
    #endregion

    #region Fields
    private const int MaxRows = 1024;
    private const int HScrollStep = 3;   // columns panned per ← / → press

    private string _mermaid;
    private MermaidStyles? _styles;
    private int _version;
    private HScroll _hscroll;   // horizontal pan offset (the frame only scrolls vertically)

    private ConsoleBuffer _content = new();
    private int _contentHeight;
    private int _renderedVersion = -1;
    private int _renderingVersion = -1;
    #endregion
}
