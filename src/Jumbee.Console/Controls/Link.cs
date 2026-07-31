namespace Jumbee.Console;

using System;
using System.Collections.Generic;
using System.Diagnostics;

using ConsoleGUI.Input;
using ConsoleGUI.Space;

using Spectre.Console.Rendering;

/// <summary>
/// A focusable, clickable text link. Activating it (a mouse click, or Enter/Space while focused) opens
/// <see cref="Url"/> with the system default handler and raises <see cref="Activated"/>.
/// </summary>
/// <remarks>
/// Leave <see cref="Url"/> <see langword="null"/> to use it purely as an action link and handle
/// <see cref="Activated"/> yourself.
/// </remarks>
public class Link : RenderableControl
{
    #region Constructors
    /// <summary>Initializes a <see cref="Link"/> with the given display text and optional URL.</summary>
    public Link(string text, string? url = null)
    {
        _text = text;
        _url = url;
        Width = text.Length;
        Height = 1;
        ApplyTheme();
    }
    #endregion

    #region Events
    /// <summary>Raised when the link is activated by a mouse click or by Enter/Space while focused.</summary>
    public event EventHandler? Activated;
    #endregion

    #region Properties
    /// <summary>Always <see langword="true"/>: the link handles Enter/Space to follow it. No opt-in needed —
    /// unlike the base default, this is on.</summary>
    public override bool HandlesInput => true;
    /// <inheritdoc/>
    protected override bool RendersOwnFocus => true;   // underlines/highlights on focus

    /// <summary>The link's display text.</summary>
    public string Text
    {
        get => _text;
        set => SetAtomicProperty(ref _text, value, updatesLayout: true, watch: (_, _) => Width = _text.Length);
    }

    /// <summary>The URL opened on activation, or <see langword="null"/> for an action-only link.</summary>
    public string? Url { get => _url; set => SetAtomicProperty(ref _url, value); }

    /// <summary>Style at rest. Defaults to <see cref="IStyleTheme.Info"/>, underlined.</summary>
    public Style LinkStyle { get => _linkStyle; set => SetAtomicProperty(ref _linkStyle, value, themeOverride: true); }

    /// <summary>Style while hovered or focused. Defaults to <see cref="IStyleTheme.Info"/>, underlined and
    /// reverse-video so the focused link is clearly highlighted.</summary>
    public Style HoverStyle { get => _hoverStyle; set => SetAtomicProperty(ref _hoverStyle, value, themeOverride: true); }
    #endregion

    #region Methods
    /// <summary>Programmatically activate the link (the same path as a click): open the URL and raise <see cref="Activated"/>.</summary>
    public void Activate()
    {
        Open();
        Activated?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc/>
    protected override void ApplyTheme()
    {
        if (!IsThemeOverridden(nameof(LinkStyle))) _linkStyle = UI.StyleTheme.Info | Style.Underline;
        if (!IsThemeOverridden(nameof(HoverStyle))) _hoverStyle = UI.StyleTheme.Info | Style.Underline | Style.Invert;
    }

    /// <inheritdoc/>
    protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        var style = (IsMouseOver || IsFocused) ? _hoverStyle : _linkStyle;
        var label = _text.Length > maxWidth ? _text[..Math.Max(0, maxWidth)] : _text;
        yield return new Segment(label, style.SpectreConsoleStyle);
    }

    /// <inheritdoc/>
    protected override void OnClick(Position position) => Activate();

    /// <inheritdoc/>
    // The second click of a rapid pair arrives here, not at OnClick — without this a double-click would follow the
    // link only once.
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

    private void Open()
    {
        if (string.IsNullOrWhiteSpace(_url)) return;
        try
        {
            // UseShellExecute routes the URL to the OS default handler (browser). Best-effort: a bad URL or a
            // missing handler must not crash the UI.
            Process.Start(new ProcessStartInfo(_url) { UseShellExecute = true });
        }
        catch
        {
        }
    }
    #endregion

    #region Fields
    private string _text;
    private string? _url;
    private Style _linkStyle;
    private Style _hoverStyle;
    #endregion
}
