namespace Jumbee.Console;

using System;
using System.Collections.Generic;

using ConsoleGUI.Input;
using ConsoleGUI.Space;

using Spectre.Console.Rendering;

// The button styles its label with the text Style type while exposing a themeable ButtonStyle named `Style`; alias
// the text type so the property name doesn't shadow it inside this file.
using TextStyle = Jumbee.Console.Style;

/// <summary>
/// A focusable, clickable button that renders a fixed-width text label.
/// </summary>
/// <remarks>
/// Activates on a mouse click or on Enter/Space while focused, raising <see cref="Activated"/>. Its appearance —
/// per-state fills, border mode, and width — comes from a themeable <see cref="ButtonStyle"/> (<see cref="Style"/>):
/// a flat single row by default, or a modern raised <see cref="ButtonShape.Modern"/> tile. Use
/// <see cref="Primary"/>/<see cref="Secondary"/> to build one styled from the theme.
/// </remarks>
public class Button : RenderableControl
{
    #region Constructors
    /// <summary>Initializes a new primary-styled <see cref="Button"/> with the given label.</summary>
    public Button(string text) : this(text, ButtonRole.Primary) { }

    private Button(string text, ButtonRole role)
    {
        _text = text;
        _role = role;
        ApplyTheme();
    }
    #endregion

    #region Events
    /// <summary>Raised when the button is activated by a mouse click or by Enter/Space while focused.</summary>
    public event EventHandler? Activated;
    #endregion

    #region Properties
    /// <summary>Always <see langword="true"/>: the button handles Enter/Space to activate. No opt-in needed —
    /// unlike the base default, this is on.</summary>
    public override bool HandlesInput => true;
    /// <inheritdoc/>
    protected override bool RendersOwnFocus => true;   // focus lightens the tile / bolds the label

    /// <inheritdoc/>
    protected internal override HelpInfo? GetHelpInfo() => new HelpInfo("Button", text: "A clickable button.")
        .WithKey("Enter / Space", "Activate when focused")
        .WithKey("Click", "Activate");

    /// <summary>The button's label text.</summary>
    public string Text
    {
        get => _text;
        set => SetAtomicProperty(ref _text, value, updatesLayout: true);
    }

    /// <summary>The button's whole appearance.</summary>
    /// <remarks>Defaults to the theme's <see cref="IStyleTheme.PrimaryButton"/> (or
    /// <see cref="IStyleTheme.SecondaryButton"/> for a <see cref="Secondary"/> button); setting it departs from the
    /// theme. Changing the border or width re-lays the button out.</remarks>
    public ButtonStyle Style
    {
        get => _style;
        set => SetAtomicProperty(ref _style, value, updatesLayout: true, themeOverride: true);
    }
    #endregion

    #region Methods
    /// <summary>Creates a primary-styled button (the theme's <see cref="IStyleTheme.PrimaryButton"/>).</summary>
    public static Button Primary(string text) => new(text, ButtonRole.Primary);

    /// <summary>Creates a secondary-styled button (the theme's <see cref="IStyleTheme.SecondaryButton"/>).</summary>
    public static Button Secondary(string text) => new(text, ButtonRole.Secondary);

    /// <summary>Programmatically activate the button (the same path as a click).</summary>
    public void Activate() => Activated?.Invoke(this, EventArgs.Empty);

    /// <inheritdoc/>
    protected override void ApplyTheme()
    {
        if (!IsThemeOverridden(nameof(Style)))
            _style = _role == ButtonRole.Secondary ? UI.StyleTheme.SecondaryButton : UI.StyleTheme.PrimaryButton;
    }

    /// <inheritdoc/>
    protected override int IntrinsicWidth() => OuterWidth();

    /// <inheritdoc/>
    protected override int IntrinsicHeight() => _style.IsModern ? 3 : 1;

    /// <inheritdoc/>
    protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        var fill = IsMousePressed ? _style.Press : IsMouseOver ? _style.Hover : _style.Normal;
        var label = _style.Bold ? fill | TextStyle.Bold : fill;

        var outer = Math.Clamp(OuterWidth(), 0, maxWidth);
        if (outer <= 0) return [];

        return _style.IsModern ? RenderBevel(fill, label, outer) : RenderFlat(label, outer);
    }

    // A single-row text button (no border). Focus shows by reversing the label.
    private IEnumerable<Segment> RenderFlat(TextStyle label, int outer)
    {
        yield return new Segment(Center(_text, outer), Focused(label));
    }

    // The modern, raised-tile look: a solid 3-row fill with a lighter top edge and darker bottom edge (the bevel
    // colours derive from the fill background unless set). Pressing inverts the bevel for a "pushed in" feel; focus
    // brightens the whole tile (a clean cue) and bolds the label.
    private IEnumerable<Segment> RenderBevel(TextStyle fill, TextStyle label, int outer)
    {
        if (fill.BackgroundColor is not { } fillBg)
        {
            // No fill background to brighten/bevel — fall back to a plain 3-row fill, reversing the label on focus.
            var flatLabel = Focused(label);
            yield return new Segment(new string(' ', outer), fill);
            yield return Segment.LineBreak;
            yield return new Segment(Center(_text, outer), flatLabel);
            yield return Segment.LineBreak;
            yield return new Segment(new string(' ', outer), fill);
            yield break;
        }

        // Focus lightens the tile instead of inverting the label to a stark white band; a raised bevel derives from
        // the (possibly brightened) fill. Pressing keeps its own darker fill (crisp press feedback), so focus doesn't
        // brighten while pressed.
        var bg = IsFocused && !IsMousePressed ? fillBg.Lighten(0.22) : fillBg;
        var faceLabel = label | TextStyle.Bg(bg);
        if (IsFocused) faceLabel |= TextStyle.Bold;

        var light = _style.BevelLight ?? bg.Lighten(0.30);
        var dark = _style.BevelDark ?? bg.Darken(0.30);
        if (IsMousePressed) (light, dark) = (dark, light);

        var top = (TextStyle)light | TextStyle.Bg(bg);
        var bottom = (TextStyle)dark | TextStyle.Bg(bg);

        yield return new Segment(new string('▔', outer), top);
        yield return Segment.LineBreak;
        yield return new Segment(Center(_text, outer), faceLabel);
        yield return Segment.LineBreak;
        yield return new Segment(new string('▁', outer), bottom);
    }

    // Reverse the label while focused a strong, font-independent focus cue.
    private TextStyle Focused(TextStyle label) => IsFocused ? label | TextStyle.Invert : label;

    // Mouse click (press+release on this control) is synthesized by the base Control.
    /// <inheritdoc/>
    protected override void OnClick(Position position) => Activate();

    /// <inheritdoc/>
    // The second click of a rapid pair arrives here, not at OnClick — without this a double-click would activate
    // only once.
    protected override void OnDoubleClick(Position position) => OnClick(position);

    /// <inheritdoc/>
    protected override void OnInput(InputEvent inputEvent)
    {
        if (inputEvent.Key.Key is ConsoleKey.Enter or ConsoleKey.Spacebar)
        {
            Activate();
            inputEvent.Handled = true;
        }
    }

    // The outer width: an explicit ButtonStyle.Width, else the label plus padding, never below ButtonStyle.MinWidth.
    private int OuterWidth()
    {
        if (_style.Width > 0) return _style.Width;
        return Math.Max(_text.Length + 2, _style.MinWidth);   // a space of padding either side
    }

    // Centres text within width, padding with spaces (or truncating when it doesn't fit).
    private static string Center(string text, int width)
    {
        if (width <= 0) return string.Empty;
        if (text.Length >= width) return text[..width];
        var pad = width - text.Length;
        var left = pad / 2;
        return new string(' ', left) + text + new string(' ', pad - left);
    }
    #endregion

    #region Fields
    private string _text;
    private ButtonStyle _style;
    private readonly ButtonRole _role;
    #endregion

    #region Types
    private enum ButtonRole { Primary, Secondary }
    #endregion
}
