namespace Jumbee.Console;

using System;
using System.Collections.Generic;
using System.Linq;

using ConsoleGUI;
using ConsoleGUI.Input;
using ConsoleGUI.Space;

using Spectre.Console.Rendering;

/// <summary>Where a <see cref="Select"/>'s dropdown opens relative to the control.</summary>
public enum SelectPopupPosition
{
    /// <summary>Open below when the dropdown fits under the control, otherwise above — so a Select near the bottom of
    /// the screen still shows all its options.</summary>
    Auto,
    /// <summary>Always open below the control.</summary>
    Below,
    /// <summary>Always open above the control.</summary>
    Above,
}

/// <summary>
/// One option in a <see cref="Select"/>: either plain text, or an <see cref="IRenderable"/> for a row that carries
/// more than a string — a colour swatch beside a name, an icon, a two-column layout.
/// </summary>
/// <remarks>
/// The same either/or a <see cref="ListBoxItem"/> makes, and for the same reason: a text option is rendered as text
/// (so nothing about existing <see cref="Select"/>s changes), and a renderable option is rendered by itself.
/// </remarks>
public sealed class SelectOption
{
    #region Constructors
    /// <summary>Creates a text option.</summary>
    public SelectOption(string text) => Text = text;

    /// <summary>Creates a renderable option. It is drawn on one row, in the closed control and in the drop-down
    /// alike, so a renderable that spans several lines is clipped to its first.</summary>
    public SelectOption(IRenderable content) => Content = content;
    #endregion

    #region Properties
    /// <summary>The option's text, or <see langword="null"/> when it was created from a renderable.</summary>
    public string? Text { get; }

    /// <summary>The option's renderable, or <see langword="null"/> when it was created from text.</summary>
    public IRenderable? Content { get; }

    /// <summary>Application data for this option — typically the value it stands for, so a selection maps back to
    /// your model without a parallel array. Not used by the control.</summary>
    /// <remarks>The obvious companion to a renderable option: the row draws a swatch, and this carries the colour.</remarks>
    public object? Tag { get; set; }
    #endregion
}

/// <summary>
/// A drop-down selector.
/// </summary>
/// <remarks>
/// Closed, it shows the current value with a ▼ marker; clicking it (or Enter/Space while
/// focused) opens its options in the ambient <see cref="UI.Overlay"/>. By default the list opens below the control,
/// flipping above when there isn't room (see <see cref="PopupPosition"/>). Choosing an option (click or Enter)
/// commits it; Escape or a click outside cancels.
/// </remarks>
public class Select : RenderableControl
{
    #region Constructors
    /// <summary>Initializes an empty <see cref="Select"/>.</summary>
    // Explicit, so `new Select()` still resolves here rather than becoming ambiguous between the params overloads.
    public Select()
    {
        _options = [];
        Height = 1;
        Width = PreferredWidth();
        ApplyTheme();
    }

    /// <summary>Initializes a new <see cref="Select"/> with the given text <paramref name="options"/>.</summary>
    public Select(params string[] options) : this() => Load(options.Select(o => new SelectOption(o)));

    /// <summary>Initializes a new <see cref="Select"/> whose options are drawn by <paramref name="options"/> — for
    /// rows that carry more than a string, such as a colour swatch beside a name.</summary>
    public Select(params IRenderable[] options) : this() => Load(options.Select(o => new SelectOption(o)));

    /// <summary>Initializes a new <see cref="Select"/> from options built directly, which is how an option carries
    /// a <see cref="SelectOption.Tag"/>.</summary>
    public Select(params SelectOption[] options) : this() => Load(options);

    // Construction is NOT SetOptions: a new Select starts with nothing selected and shows its placeholder, where
    // SetOptions is a runtime replacement and deliberately keeps (or falls back to) a selection. Routing the
    // constructors through it auto-selected the first option and lost the placeholder.
    private void Load(IEnumerable<SelectOption> options)
    {
        _options = [.. options];
        Width = PreferredWidth();
        Invalidate();
    }
    #endregion

    #region Events
    /// <summary>Raised when a different value is committed.</summary>
    public event EventHandler<string>? SelectionChanged;
    #endregion

    #region Properties
    /// <summary>The selectable options, whether text or renderable.</summary>
    public IReadOnlyList<SelectOption> Items => _options;

    /// <summary>The selectable options as text. Renderable options have none and come back as empty strings — use
    /// <see cref="Items"/> when the list may hold them.</summary>
    public IReadOnlyList<string> Options => [.. _options.Select(o => o.Text ?? string.Empty)];

    /// <summary>Text shown when no option is selected.</summary>
    public string Placeholder { get; set; } = "Select…";

    /// <summary>The text colour of the collapsed control.</summary>
    public Color Foreground { get; set; } = Color.White;
    /// <summary>The background colour of the collapsed control.</summary>
    public Color Background { get; set; } = new(50, 50, 70);

    /// <summary>Whether the dropdown opens below or above the control. Defaults to <see cref="SelectPopupPosition.Auto"/>.</summary>
    public SelectPopupPosition PopupPosition { get; set; } = SelectPopupPosition.Auto;

    /// <summary>
    /// Whether the control responds to the user. A disabled <see cref="Select"/> draws its current value in
    /// <see cref="DisabledStyle"/>, does not open on a click or key, and is skipped by Tab.
    /// </summary>
    /// <remarks>
    /// It keeps showing the selected option rather than blanking, so a panel can report a setting that is real but
    /// not currently changeable. Setting <see cref="SelectedIndex"/> in code still works while disabled.
    /// </remarks>
    public bool Enabled
    {
        get => _enabled;
        set => SetAtomicProperty(ref _enabled, value, watch: (_, on) => ApplyEnabledToFocus(on));
    }

    /// <summary>Text style of the collapsed control while disabled; its <see cref="Background"/> is kept, so it
    /// still reads as a control rather than as stray text. Defaults to <see cref="IStyleTheme.TextDisabled"/>.</summary>
    public Style DisabledStyle { get => _disabledStyle; set => SetAtomicProperty(ref _disabledStyle, value, themeOverride: true); }

    /// <summary>
    /// Whether the collapsed control is drawn only as wide as its widest option — the same width as the dropdown —
    /// instead of filling whatever width its layout offers. Default <see langword="false"/>.
    /// </summary>
    /// <remarks>
    /// The default suits a form, where a field filling its column is the convention and the <c>▼</c> sits at the
    /// right edge under the ones above it. Set this in a narrow panel of mixed controls, where a full-width block
    /// of colour for a three-word choice reads as far heavier than the choice is — and where the collapsed control
    /// not matching the list it opens is itself a little jarring.
    /// </remarks>
    public bool FitContent { get => _fitContent; set => SetAtomicProperty(ref _fitContent, value); }

    /// <summary>The index of the selected option, or -1 when none is selected. Setting it raises <see cref="SelectionChanged"/>.</summary>
    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            var clamped = _options.Count == 0 ? -1 : Math.Clamp(value, 0, _options.Count - 1);
            if (clamped == _selectedIndex) return;
            _selectedIndex = clamped;
            Invalidate();
            // Raised for a renderable option too, which has no text to carry — the payload is empty there and the
            // handler reads SelectedIndex/SelectedItem. Guarding on the text instead would mean a Select of
            // renderables never announced a change at all.
            if (_selectedIndex >= 0) SelectionChanged?.Invoke(this, SelectedValue ?? string.Empty);
        }
    }

    /// <summary>The selected option, or <see langword="null"/> when nothing is selected.</summary>
    public SelectOption? SelectedItem =>
        _selectedIndex >= 0 && _selectedIndex < _options.Count ? _options[_selectedIndex] : null;

    /// <summary>The selected option's text, or <see langword="null"/> when nothing is selected <em>or</em> the
    /// selection is a renderable option, which has no text. Read <see cref="SelectedIndex"/> or
    /// <see cref="SelectedItem"/> in that case.</summary>
    public string? SelectedValue => SelectedItem?.Text;

    /// <summary>Reports <see langword="true"/> so input routing delivers keys to the control.</summary>
    public override bool HandlesInput => true;

    // Only while fitting: the themed cue fills the control's unpainted cells, which is right for a Select that
    // fills its column and wrong for one that doesn't. See Render.
    /// <inheritdoc/>
    protected override bool RendersOwnFocus => _fitContent;
    #endregion

    #region Methods
    /// <summary>Replaces the options, keeping the current value selected if it is still among them (otherwise the
    /// first, or nothing when the list is empty). Re-sizes the control to the new widest option.</summary>
    /// <remarks>For a drop-down over a list that changes at runtime — files that have been loaded, devices that
    /// have appeared. Raises <see cref="SelectionChanged"/> only if the selected <em>value</em> actually changes,
    /// so a rebuild that keeps the current choice is silent.</remarks>
    public void SetOptions(params IEnumerable<string> options) =>
        SetOptions([.. options.Select(o => new SelectOption(o))]);

    /// <summary>Replaces the options with renderable ones. See <see cref="SetOptions(IEnumerable{string})"/>.</summary>
    public void SetOptions(params IEnumerable<IRenderable> options) =>
        SetOptions([.. options.Select(o => new SelectOption(o))]);

    /// <summary>Replaces the options outright. See <see cref="SetOptions(IEnumerable{string})"/>.</summary>
    /// <remarks>
    /// The "keep the current choice" rule can only work by text, so a list of renderable options falls back to
    /// keeping the current <em>index</em> — there is nothing to compare two arbitrary renderables by, and an
    /// identity comparison would fail the moment a caller rebuilt equivalent rows.
    /// </remarks>
    public void SetOptions(params IEnumerable<SelectOption> options)
    {
        var previous = SelectedValue;
        var previousIndex = _selectedIndex;
        _options = [.. options];

        var index = previous is null ? -1 : _options.FindIndex(o => o.Text == previous);
        if (index < 0 && previous is null && previousIndex >= 0) index = previousIndex;
        if (index < 0) index = _options.Count > 0 ? 0 : -1;
        index = _options.Count == 0 ? -1 : Math.Min(index, _options.Count - 1);

        // Assigned directly rather than through SelectedIndex: the property short-circuits when the index is
        // unchanged, which is exactly the case where the value underneath it may have changed.
        _selectedIndex = index;
        Width = PreferredWidth();
        Invalidate();

        if (SelectedValue is { } current && current != previous) SelectionChanged?.Invoke(this, current);
    }

    /// <summary>Opens the dropdown into the ambient <see cref="UI.Overlay"/> (no-op before <see cref="UI.Start"/>
    /// or with no options).</summary>
    public void Open()
    {
        if (UI.Overlay is not { } host || _options.Count == 0) return;

        var rows = Math.Min(_options.Count, MaxDropdownRows);
        var list = new ListBox
        {
            SelectedForegroundColor = Color.White,
            SelectedBackgroundColor = new Color(40, 90, 160),
            Width = PreferredWidth(),
            Height = rows,
        };

        // Added one at a time and by KIND, so a text option still takes ListBox's text path (markup, per-item
        // colours, the markup-based selection highlight) exactly as it did before renderables existed.
        foreach (var option in _options)
        {
            if (option.Content is { } content) list.AddItem(content);
            else list.AddItem(option.Text ?? string.Empty);
        }

        list.SelectedIndex = Math.Max(0, _selectedIndex);
        list.WithRoundedBorder(Color.Grey);

        // Mapped back by POSITION, not by text: a renderable option has no text to look up, and two text options
        // can legitimately read the same.
        list.Committed += (_, item) =>
        {
            var index = list.Items.ToList().IndexOf(item);
            if (index >= 0) SelectedIndex = index;
            Close();
        };
        list.Cancelled += (_, _) => Close();

        if (_controlLeft >= 0) host.Show(list, _controlLeft, ResolveTop(rows + 2));   // + rounded border rows
        else host.Show(list);
    }

    // The y at which the dropdown opens, honouring PopupPosition. Auto opens below when the popup fits under the
    // control, else above when there's room there — so a Select near the bottom edge still shows every option.
    private int ResolveTop(int popupHeight)
    {
        var below = _controlTop + 1;                 // just under the one-row control
        var above = _controlTop - popupHeight;       // directly above it
        var screenHeight = ConsoleManager.WindowSize.Height;
        return PopupPosition switch
        {
            SelectPopupPosition.Below => below,
            SelectPopupPosition.Above => Math.Max(0, above),
            _ => below + popupHeight <= screenHeight ? below : above >= 0 ? above : below,
        };
    }

    /// <inheritdoc/>
    // Only the disabled style is themed here; Foreground/Background are plain colour properties with their own
    // defaults, so a runtime theme switch leaves a caller's palette alone.
    protected override void ApplyTheme()
    {
        if (!IsThemeOverridden(nameof(DisabledStyle))) _disabledStyle = UI.StyleTheme.TextDisabled;
    }

    /// <inheritdoc/>
    protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        var label = SelectedValue ?? Placeholder;

        // Fitting leaves the rest of the row untouched (transparent), so the control ends where the dropdown it
        // opens would — never wider than the room actually offered.
        var width = _fitContent ? Math.Min(maxWidth, PreferredWidth()) : maxWidth;

        // Focus is drawn into the box itself when fitting. The themed default cue fills every UNPAINTED cell of the
        // control, which for a fitted Select is the whole rest of the row — so choosing an option, which hands
        // focus back, made the control look like it had sprung back to full width.
        var background = _fitContent && IsFocused ? Background.Mix(Color.White, FocusLift) : Background;
        // Disabled keeps the box background and mutes only the text — combining puts the background back over the
        // themed disabled style, which carries no background of its own.
        Style style = _enabled
            ? new Spectre.Console.Style(Foreground, background)
            : _disabledStyle | new Spectre.Console.Style(background: background);

        // A renderable option is drawn by itself; a text one keeps the single-segment path it has always had, so
        // nothing about an existing Select changes — including that its text is NOT treated as markup.
        if (SelectedItem?.Content is { } content)
        {
            foreach (var segment in RenderContent(content, options, width, style)) yield return segment;
            yield break;
        }

        var inner = $" {label}";
        if (inner.Length > width - 2) inner = inner[..Math.Max(0, width - 2)];
        var text = inner.PadRight(Math.Max(0, width - 1)) + "▼";   // value left, arrow at the right edge

        yield return new Segment(text, style);
    }

    // The closed row for a renderable option: a leading space, the renderable's FIRST line clipped to the room
    // available, blank fill, then the arrow at the right edge — the same shape as the text path.
    private static IEnumerable<Segment> RenderContent(IRenderable content, RenderOptions options, int width, Style row)
    {
        var available = Math.Max(0, width - 2);   // leading space + arrow
        yield return new Segment(" ", row);

        // Only the first line: the control is one row tall, and a renderable that wraps or spans lines would
        // otherwise push the arrow off the end and overrun into whatever is below.
        var lines = Segment.SplitLines(content.Render(options, available));
        var used = 0;
        if (lines.Count > 0)
        {
            foreach (var segment in Segment.Truncate(lines[0], available))
            {
                // The row's own colours UNDER the item's, so the box reads as continuous and an option that styles
                // only part of itself picks up the control's foreground for the rest — Combine keeps whichever
                // colours the item actually sets and falls back to the row's for the ones it leaves default.
                yield return new Segment(segment.Text, row | segment.Style);
                used += segment.CellCount();
            }
        }

        if (used < available) yield return new Segment(new string(' ', available - used), row);
        yield return new Segment("▼", row);
    }

    /// <inheritdoc/>
    protected override void OnClick(Position position)
    {
        if (!_enabled) return;

        // Record this control's top-left on screen: the click's absolute position minus its position relative to us.
        // Open() turns that into the dropdown anchor per PopupPosition.
        if (ConsoleManager.MousePosition is { } m)
        {
            _controlLeft = m.X - position.X;
            _controlTop = m.Y - position.Y;
        }
        Open();
    }

    /// <inheritdoc/>
    // The second click of a rapid pair arrives here, not at OnClick — without this a double-click on the closed
    // control would be swallowed instead of toggling the dropdown.
    protected override void OnDoubleClick(Position position) => OnClick(position);

    /// <inheritdoc/>
    protected override void OnInput(InputEvent inputEvent)
    {
        if (!_enabled) return;

        // Enter/Space or Down/Up open the dropdown (the standard combobox keys); the open list then navigates.
        if (inputEvent.Key.Key is ConsoleKey.Enter or ConsoleKey.Spacebar or ConsoleKey.DownArrow or ConsoleKey.UpArrow)
        {
            Open();
            inputEvent.Handled = true;
        }
    }

    private void Close()
    {
        UI.Overlay?.Hide();
        UI.SetFocus(this);
    }

    private int PreferredWidth()
    {
        var longest = 0;
        RenderOptions? measureOptions = null;
        foreach (var option in _options)
        {
            if (option.Content is { } content)
            {
                // Measured at a width no option would reach, so the answer is the renderable's natural width rather
                // than whatever it would wrap to — the same trick ListBox uses for width-independent measurement.
                measureOptions ??= new RenderOptions(ansiConsole.Profile.Capabilities, new Spectre.Console.Size(MeasureWidth, 1));
                longest = Math.Max(longest, content.Measure(measureOptions, MeasureWidth).Max);
            }
            else
            {
                longest = Math.Max(longest, option.Text?.Length ?? 0);
            }
        }

        return Math.Max(longest, Placeholder.Length) + 3;   // leading space + arrow + a little padding
    }
    #endregion

    #region Fields
    private const int MaxDropdownRows = 8;

    // A width no real option reaches, so a renderable measures to its natural width instead of a wrapped one.
    private const int MeasureWidth = 1000;

    // How far the focused fill is lifted toward white. Derived from the control's own Background rather than taken
    // from the theme so it stays visible whatever colour the caller set.
    private const double FocusLift = 0.22;

    private bool _fitContent;
    private List<SelectOption> _options;
    private int _selectedIndex = -1;
    private bool _enabled = true;
    private Style _disabledStyle;
    private int _controlLeft = -1;
    private int _controlTop = -1;
    #endregion
}
