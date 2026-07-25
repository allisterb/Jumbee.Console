namespace Jumbee.Console;

using System.Linq;

using ConsoleGUI.Data;
using ConsoleGUI.Space;

using SCDecoration = Spectre.Console.Decoration;

/// <summary>The layout direction of a <see cref="TextLabel"/>.</summary>
public enum TextLabelOrientation
{
    /// <summary>Text runs left-to-right across a single row.</summary>
    Horizontal,
    /// <summary>Text runs top-to-bottom down a single column.</summary>
    Vertical
}

/// <summary>
/// Displays a single-line text label with a defined horizontal or vertical orientation, foreground and background
/// colour, and optional text decoration (e.g. bold, underline).
/// </summary>
public sealed class TextLabel : Control
{
    #region Constructors
    /// <summary>Initializes a new <see cref="TextLabel"/> with the given <paramref name="orientation"/>, <paramref name="text"/>, optional foreground/background colours, and optional <paramref name="decoration"/>.</summary>
    // Colours are nullable and default to transparent (null): an unset foreground inherits the terminal default and
    // an unset background lets whatever is behind show through. Passing the non-nullable default(Color) here would
    // paint an opaque BLACK background — invisible on a black terminal, but it dims to near-black under an overlay
    // scrim (and blocks compositing), which is rarely what a plain label wants.
    public TextLabel(TextLabelOrientation orientation, string text, Color? fgcolor = null, Color? bgcolor = null, SCDecoration decoration = SCDecoration.None)
    {
        Focusable = false;   // a passive display label: never a focus/tab target, never owns the cursor
        _orientation = orientation;
        _text = text;
        _decoration = decoration;
        // Theme first, then let explicit arguments win AND register as overrides (assigning through the properties
        // is what marks them), so a later theme switch re-colours only the labels that didn't ask for a colour.
        CaptureTheme();
        if (fgcolor is not null) FgColor = fgcolor;
        if (bgcolor is not null) BgColor = bgcolor;
        chars = new Cell[_text.Length];
        size = orientation == TextLabelOrientation.Horizontal ? new Size(_text.Length, 1) :new Size(1, _text.Length);
        Resize(size);
    }
    #endregion

    #region Properties
    /// <summary>Foreground colour, or <see langword="null"/> for the terminal default. Themed from
    /// <see cref="IStyleTheme.LabelText"/> until set explicitly.</summary>
    public Color? FgColor
    {
        get => _fgcolor;
        set => SetAtomicProperty(ref _fgcolor, value, themeOverride: true);
    }

    /// <summary>Background colour, or <see langword="null"/> for transparent (shows whatever is behind). Themed from
    /// <see cref="IStyleTheme.LabelText"/> until set explicitly.</summary>
    public Color? BgColor
    {
        get => _bgcolor;
        set => SetAtomicProperty(ref _bgcolor, value, themeOverride: true);
    }

    /// <summary>Text decoration (e.g. <c>Bold</c>, <c>Underline</c>); <c>None</c> for plain text. Flags combine.</summary>
    public SCDecoration Decoration
    {
        get => _decoration;
        set => SetAtomicProperty(ref _decoration, value);
    }

    /// <summary>The label text. Setting it re-sizes the control when the length changes.</summary>
    public string Text
    {
        get => _text;
        set => SetAtomicProperty(ref _text, value, watch: (old, @new) =>
        {
            chars = new Cell[_text.Length];
            // Only resize when the text length (the extent along the text axis) changes. A same-length update — e.g. a
            // "52%"→"54%" gauge tick — is a content change the following paint's own damage report already covers, so
            // an unconditional Resize here would report this label's whole area a second, redundant time every update.
            if ((old?.Length ?? 0) != (@new?.Length ?? 0))
            {
                size = _orientation == TextLabelOrientation.Horizontal ? new Size(_text.Length, 1) : new Size(1, _text.Length);
                Resize(size);
            }
        });
    }
    #endregion

    #region Indexers
    /// <summary>The rendered cell at <paramref name="position"/>, or an empty cell outside the text.</summary>
    public override Cell this[Position position]
    {
        get
        {
            if (string.IsNullOrEmpty(_text))
            {
                return emptyCell;
            }
            else if (_orientation == TextLabelOrientation.Horizontal)
            {
                if (position.Y >= 1 || position.X >= Text.Length)
                {
                    return emptyCell;
                }
                else
                {
                    return chars[position.X];
                }
            }
            else
            {
                if (position.X >= 1 || position.Y >= Text.Length)
                {
                    return emptyCell;
                }
                else
                {
                    return chars[position.Y];
                }
            }
        }
    }
    #endregion

    #region Methods
    // Both halves of the one LabelText token: a theme that wants labels on a coloured strip supplies a background
    // there rather than needing every caller to pass one. A token half with no colour leaves that side unset
    // (null == terminal default / transparent), which is what the Style.Plain default gives.
    private void CaptureTheme()
    {
        var label = UI.StyleTheme.LabelText;
        if (!IsThemeOverridden(nameof(FgColor))) _fgcolor = label.ForegroundColor;
        if (!IsThemeOverridden(nameof(BgColor))) _bgcolor = label.BackgroundColor;
    }

    /// <inheritdoc/>
    protected override void ApplyTheme() => CaptureTheme();

    /// <summary>Renders each character into the label's cell buffer with the configured colours.</summary>
    // We use a 1D buffer to render instead of the 2D consoleBuffer as it's more efficient to access.
    protected override void Render()
    {
        // Spectre and ConsoleGUI Decoration share flag values; map None to null (no decoration) like the other controls.
        Decoration? deco = _decoration == SCDecoration.None ? null : (Decoration)_decoration;
        for (int i = 0; i < _text.Length; i++)
        {
            chars[i] = (Cell)new Character(_text[i], foreground: _fgcolor, background: _bgcolor, decoration: deco);
        }
    }

    // A label is fixed in its minor axis (a horizontal label is 1 row tall, a vertical one is 1 column wide) and
    // fills along its text axis (returning 0 there). Reporting this as an intrinsic size keeps a label docked on a
    // DockPanel edge from ballooning to fill the panel and collapsing the fill region — see CalculateSize.
    /// <summary>1 for a vertical label (fixed one column wide), otherwise 0 (fills along the text axis).</summary>
    protected override int IntrinsicWidth() => _orientation == TextLabelOrientation.Vertical ? 1 : 0;

    /// <summary>1 for a horizontal label (fixed one row tall), otherwise 0 (fills along the text axis).</summary>
    protected override int IntrinsicHeight() => _orientation == TextLabelOrientation.Horizontal ? 1 : 0;

    #endregion

    #region Fields
    private TextLabelOrientation _orientation;
    private string _text = "";
    private Color? _fgcolor;
    private Color? _bgcolor;
    private SCDecoration _decoration;
    private Size size;
    private Cell[] chars = [];
    #endregion
}
