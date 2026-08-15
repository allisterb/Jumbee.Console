namespace Jumbee.Console;

using System;
using System.Collections.Generic;

using Spectre.Console;
using Spectre.Console.Rendering;

/// <summary>
/// Displays a block of multi-line Spectre <see cref="Markup"/> text — the counterpart to the single-line
/// <see cref="TextLabel"/> and the append-only <see cref="Log"/>.
/// </summary>
/// <remarks>
/// For static readouts, ASCII art, and small formatted panels (a weather box, a key/value summary). Content is
/// <see cref="Markup"/>, so colours and styles work; literal <c>[</c>/<c>]</c> must be escaped as <c>[[</c>/<c>]]</c>
/// (or via <see cref="Escape"/>).
/// </remarks>
public class TextPanel : RenderableControl, IScrollable
{
    #region Constructors
    /// <summary>Initializes a new <see cref="TextPanel"/> displaying the given Spectre <paramref name="markup"/>.</summary>
    public TextPanel(string markup = "")
    {
        Focusable = false;   // a passive display control: never a focus/tab target
        _markup = markup ?? "";
    }
    #endregion

    #region Properties
    /// <summary>The Spectre markup to display (may contain newlines). Setting it re-measures the content height.</summary>
    public string Markup
    {
        get => _markup;
        set => SetAtomicProperty(ref _markup, value ?? "", updatesLayout: true);
    }
    #endregion

    #region Methods
    /// <summary>Escapes markup control characters so arbitrary text can be shown verbatim (Spectre's
    /// <c>Markup.Escape</c>).</summary>
    public static string Escape(string text) => Spectre.Console.Markup.Escape(text ?? "");

    /// <inheritdoc/>
    // Content-only render (never reads focus/hover): reuse the cached buffer on interactive-state changes.
    protected override bool RendersInteractiveState => false;

    /// <inheritdoc/>
    // Report the rendered line count so a wrapping ControlFrame can size/scroll it; in a fixed cell it fills and
    // overflow clips. Consulted only when the parent leaves the height unbounded (see Control.CalculateSize).
    public virtual int MeasureHeight(int width)
    {
        if (string.IsNullOrEmpty(_markup)) return 1;
        var w = Math.Max(1, width);
        var options = new RenderOptions(ansiConsole.Profile.Capabilities, new Spectre.Console.Size(w, 1));
        var segments = Build().Render(options, w);
        return Math.Max(1, Segment.SplitLines(segments).Count);
    }

    /// <inheritdoc/>
    protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        if (string.IsNullOrEmpty(_markup)) return [];
        return Build().Render(options, maxWidth);
    }

    private IRenderable Build() => new Markup(_markup);
    #endregion

    #region Fields
    private string _markup;
    #endregion
}
