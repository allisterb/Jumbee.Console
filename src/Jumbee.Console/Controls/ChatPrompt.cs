namespace Jumbee.Console;

using System;
using System.Collections.Generic;

using ConsoleGUI.Data;
using ConsoleGUI.Space;

using Spectre.Console;

/// <summary>
/// The input area of an agent/chat CLI (Claude Code, Gemini CLI): a prompt glyph on the left that turns into an
/// animated <em>busy</em> spinner while an operation runs, and a single-line <see cref="TextInput"/> filling the
/// rest. Submitting (Enter) raises <see cref="Submitted"/>; any edit raises <see cref="Changed"/>.
/// </summary>
/// <remarks>
/// Optional type-ahead is attached with <see cref="WithSuggestions(string[])"/>. Built as a
/// <see cref="CompositeControl"/> — a <see cref="PromptGutter"/> docked left of the input — so it drops into any
/// layout cell and is framed like any control (<c>chat.WithRoundedBorder()</c>). Focus delegates to the input,
/// which keeps the caret; the gutter is a non-focusable adornment.
/// </remarks>
public class ChatPrompt : CompositeControl, IScrollable
{
    #region Constructors
    /// <summary>Initializes a new <see cref="ChatPrompt"/> with an optional <paramref name="placeholder"/> hint.</summary>
    public ChatPrompt(string placeholder = "")
    {
        _input = new TextInput(placeholder: placeholder);
        _gutter = new PromptGutter();

        // Inter-child wiring: relay the field's events out under the composite's identity.
        _input.Submitted += (_, _) => Submitted?.Invoke(this, _input.Text);
        _input.Changed += (_, _) => Changed?.Invoke(this, EventArgs.Empty);

        SetContent(new DockPanel(DockedControlPlacement.Left, _gutter, _input));
    }
    #endregion

    #region Properties
    /// <summary>The wrapped text field (focus it to type; the composite delegates focus here).</summary>
    public TextInput Input => _input;

    /// <summary>The entered text. Setting it moves the caret to the end and raises <see cref="Changed"/>.</summary>
    public string Text
    {
        get => _input.Text;
        set => _input.Text = value;
    }

    /// <summary>Muted hint shown while the field is empty.</summary>
    public string Placeholder
    {
        get => _input.Placeholder;
        set => _input.Placeholder = value;
    }

    /// <summary>When <see langword="true"/>, edits are ignored (caret navigation still works). Set this while an
    /// operation runs if the field should not accept input.</summary>
    public bool ReadOnly
    {
        get => _input.ReadOnly;
        set => _input.ReadOnly = value;
    }

    /// <summary>The prompt glyph shown left of the input when idle (default <c>❯</c>). A trailing space separates it
    /// from the text, so a longer prompt widens the gutter.</summary>
    public string Prompt
    {
        get => _gutter.Prompt;
        set => _gutter.Prompt = value;
    }

    /// <summary>The spinner shown in place of the prompt glyph while <see cref="Busy"/> (default
    /// <see cref="Spectre.Console.Spinner.Known.Dots"/>).</summary>
    public Spectre.Console.Spinner Spinner
    {
        get => _gutter.Spinner;
        set => _gutter.Spinner = value;
    }

    /// <summary>Style of the prompt glyph / spinner. Defaults to <see cref="IStyleTheme.TextAccent"/>.</summary>
    public Style PromptStyle
    {
        get => _gutter.Style;
        set => _gutter.Style = value;
    }

    /// <summary>When <see langword="true"/>, the gutter animates a spinner to signal a running operation; when
    /// <see langword="false"/> it shows the static prompt glyph. The field stays editable either way (queue input
    /// while busy, like Claude Code) — set <see cref="ReadOnly"/> if that's not wanted.</summary>
    public bool Busy
    {
        get => _gutter.Busy;
        set => UI.Invoke(() => _gutter.Busy = value);   // toggles multi-field animation state on the UI thread
    }
    #endregion

    #region Events
    /// <summary>Raised when Enter is pressed. The argument is the submitted <see cref="Text"/>.</summary>
    public event EventHandler<string>? Submitted;

    /// <summary>Raised whenever the text changes.</summary>
    public event EventHandler? Changed;
    #endregion

    #region Methods
    /// <summary>Attaches type-ahead suggestions from a fixed candidate list (returns the <see cref="Autocomplete"/>
    /// for further tuning, e.g. <see cref="Autocomplete.MaxRows"/>).</summary>
    public Autocomplete WithSuggestions(params string[] candidates) => new(_input, candidates);

    /// <summary>Attaches type-ahead suggestions from a provider called with the current text.</summary>
    public Autocomplete WithSuggestions(Func<string, IEnumerable<string>> suggest) => new(_input, suggest);

    // A single input row; a surrounding frame sizes us to one row (plus its border) instead of the 1000-row fill.
    /// <inheritdoc/>
    public virtual int MeasureHeight(int width) => 1;

    /// <inheritdoc/>
    protected internal override HelpInfo? GetHelpInfo() => new HelpInfo("Prompt", "Chat prompt",
        "A prompt for entering a message or command, with an optional busy spinner and type-ahead.")
        .WithKey("Arrows", "Move the caret")
        .WithKey("Enter", "Submit");
    #endregion

    #region Fields
    private readonly TextInput _input;
    private readonly PromptGutter _gutter;
    #endregion
}

/// <summary>
/// The fixed-width left adornment of a <see cref="ChatPrompt"/>: draws the static prompt glyph when idle, and
/// animates the <see cref="ChatPrompt.Spinner"/> frames while <see cref="Busy"/>. Non-focusable. It drives the
/// spinner itself (no timer/thread) by repainting each frame only while busy — the same time-accumulator approach
/// as <see cref="AnimatedControl"/>, but rendered as a plain control so the idle glyph shows without forcing a
/// continuous repaint.
/// </summary>
internal sealed class PromptGutter : Control
{
    #region Constructors
    public PromptGutter()
    {
        Focusable = false;
        _frames = _spinner.Frames;
        _interval = _spinner.Interval.Ticks;
        _width = _prompt.Length + 1;
        ApplyTheme();
    }
    #endregion

    #region Properties
    public string Prompt
    {
        get => _prompt;
        set => SetAtomicProperty(ref _prompt, value ?? string.Empty, updatesLayout: true,
            watch: (_, _) => _width = _prompt.Length + 1);
    }

    public Spectre.Console.Spinner Spinner
    {
        get => _spinner;
        set
        {
            _spinner = value;
            _frames = value.Frames;
            _interval = value.Interval.Ticks;
            _frameIndex = 0;
            Invalidate();
        }
    }

    public Style Style
    {
        get => _style;
        set => SetAtomicProperty(ref _style, value, themeOverride: true, watch: (_, _) => _styleMarkup = _style);
    }

    public bool Busy
    {
        get => _busy;
        set
        {
            if (_busy == value) return;
            _busy = value;
            _lastTick = DateTime.Now.Ticks;
            _accumulated = 0L;
            _frameIndex = 0;
            Invalidate();
        }
    }
    #endregion

    #region Methods
    protected override int IntrinsicWidth() => _width;   // glyph + trailing space
    protected override int IntrinsicHeight() => 1;

    protected override void ApplyTheme()
    {
        if (!IsThemeOverridden(nameof(Style))) _style = UI.StyleTheme.TextAccent;
        _styleMarkup = _style;
    }

    // Advance the spinner (time-based) while busy, then draw. When idle this runs once per Invalidate (see Validate).
    protected override void Paint()
    {
        if (_busy && _frames.Count > 0)
        {
            var now = DateTime.Now.Ticks;
            _accumulated += now - _lastTick;
            _lastTick = now;
            if (_accumulated >= _interval)
            {
                _accumulated = 0L;
                _frameIndex = (_frameIndex + 1) % _frames.Count;
            }
        }
        Render();
    }

    // Keep repainting every frame while busy so the spinner animates; settle after a single paint when idle.
    protected override void Validate()
    {
        if (_busy) return;
        base.Validate();
    }

    protected override void Render()
    {
        ansiConsole.Clear(true);
        var glyph = _busy && _frames.Count > 0 ? _frames[_frameIndex % _frames.Count] : _prompt;
        if (glyph.Length == 0) return;
        ansiConsole.Markup($"[{_styleMarkup}]{Markup.Escape(glyph)}[/]");
    }
    #endregion

    #region Fields
    private string _prompt = "❯";
    private int _width;
    private Spectre.Console.Spinner _spinner = Spectre.Console.Spinner.Known.Dots;
    private IReadOnlyList<string> _frames;
    private long _interval;
    private int _frameIndex;
    private bool _busy;
    private long _lastTick;
    private long _accumulated;
    private Style _style;
    private string _styleMarkup = string.Empty;
    #endregion
}
