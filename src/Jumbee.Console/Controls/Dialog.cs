namespace Jumbee.Console;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ConsoleGUI.Data;
using ConsoleGUI.Input;
using ConsoleGUI.Space;

using Spectre.Console.Rendering;

/// <summary>The predefined button set a <see cref="Dialog"/> shows along its bottom edge.</summary>
public enum DialogButtons
{
    /// <summary>No buttons.</summary>
    None,
    /// <summary>A single OK button.</summary>
    Ok,
    /// <summary>OK and Cancel buttons.</summary>
    OkCancel,
    /// <summary>Yes and No buttons.</summary>
    YesNo,
    /// <summary>Yes, No and Cancel buttons.</summary>
    YesNoCancel,
    /// <summary>A single Close button.</summary>
    Close,
}

/// <summary>Which button dismissed a <see cref="Dialog"/> (or how it was dismissed).</summary>
public enum DialogResult
{
    /// <summary>Not yet dismissed.</summary>
    None,
    /// <summary>Dismissed with OK.</summary>
    Ok,
    /// <summary>Dismissed with Cancel.</summary>
    Cancel,
    /// <summary>Dismissed with Yes.</summary>
    Yes,
    /// <summary>Dismissed with No.</summary>
    No,
    /// <summary>Dismissed with Close.</summary>
    Close,
}

/// <summary>
/// A modal dialog window shown over the ambient <see cref="UI.Overlay"/>: a titled, bordered box that takes
/// exclusive focus (the layer beneath is dimmed and click-blocked) until dismissed.
/// </summary>
/// <remarks>
/// It hosts either a wrapped text message or <em>any</em> control as its content, plus an optional row of predefined
/// buttons (OK/Cancel, Yes/No, …). Buttons are keyboard-navigable (←/→ or Tab, Enter/Space to activate); Escape
/// cancels. Dismissal raises <see cref="Completed"/> with the chosen <see cref="DialogResult"/>.
/// <para>
/// Use the static helpers for the common cases — <see cref="Confirm"/> (Yes/No), <see cref="Message"/> (OK) — or
/// construct one with a custom content control and call <see cref="Show()"/>. The dialog uses <see cref="UI.Overlay"/>
/// (set automatically by <see cref="UI.Start"/>), so no overlay wiring is needed.
/// </para>
/// </remarks>
public class Dialog : CompositeControl
{
    #region Constructors
    /// <summary>A dialog hosting a custom <paramref name="content"/> control, with the given button set.</summary>
    public Dialog(string title, Control content, DialogButtons buttons = DialogButtons.OkCancel)
        : this(title, content, content.Focusable, buttons) { }

    /// <summary>A dialog showing a wrapped text <paramref name="message"/>, with the given button set.</summary>
    public Dialog(string title, string message, DialogButtons buttons = DialogButtons.OkCancel)
        : this(title, new DialogText(message, DefaultContentWidth), false, buttons) { }

    private Dialog(string title, Control content, bool contentFocusable, DialogButtons buttons)
    {
        _content = content;
        _cancelResult = CancelResultFor(buttons);

        var stops = new List<Control>();
        IFocusable[] children;
        if (buttons == DialogButtons.None)
        {
            if (contentFocusable) stops.Add(content);
            children = [content];
        }
        else
        {
            _bar = new DialogButtonBar(ButtonSpecFor(buttons));
            _bar.Activated += Close;
            if (contentFocusable) stops.Add(content);
            stops.Add(_bar);
            children = [content, _bar];
        }
        _stops = stops;
        _current = stops.FirstOrDefault();

        SetContent(new InterceptStack(children) { Interceptor = OnBodyKey });

        // A fixed, sensible modal size: the composite is the interior; the frame adds the border + title bar.
        Width = content is DialogText ? DefaultContentWidth : (content.Width > 0 ? content.Width : DefaultContentWidth);
        var contentHeight = content is DialogText dt ? dt.PreferredHeight : (content.Height > 0 ? content.Height : DefaultCustomHeight);
        Height = contentHeight + (_bar is null ? 0 : BarHeight);

        // Inline title (on the top border line, centered): opaque — a dedicated title row would leave its right side
        // transparent, letting the dimmed layer behind bleed into the title bar.
        this.WithRoundedBorder().WithTitle(title, new TitleStyle(TitlePos.TopCenter, TitleBorderStyle.Inline, TitleColorStyle.Normal));
        ApplyTheme();
    }
    #endregion

    #region Properties
    /// <summary>The result the dialog was dismissed with (<see cref="DialogResult.None"/> until dismissed).</summary>
    public DialogResult Result { get; private set; }

    /// <summary>The focus target while the dialog is open: the current stop (custom content or the button bar).</summary>
    protected override Control? FocusChild => _current ?? _bar ?? _content;
    #endregion

    #region Indexers
    // Composite every interior cell onto the opaque surface background: a modal must not let the dimmed layer behind
    // bleed through transparent gaps (a short custom control, inter-child spacing, a glyph with no background). Keeps
    // the cell's glyph/foreground/decoration/cursor and its mouse listener; only fills an absent background.
    /// <inheritdoc/>
    public override Cell this[Position position]
    {
        get
        {
            var cell = base[position];
            if (_surfaceBg is not { } bg || cell.Character.Background is not null) return cell;
            return new Cell(cell.Character.WithBackground(bg), cell.MouseListener);
        }
    }
    #endregion

    #region Events
    /// <summary>Raised once when the dialog is dismissed, with the chosen result (Escape / lost focus = the cancel
    /// result for the button set).</summary>
    public event EventHandler<DialogResult>? Completed;
    #endregion

    #region Methods
    /// <summary>Shows the dialog modally over the ambient <see cref="UI.Overlay"/>.</summary>
    public void Show() => Show(UI.Overlay ?? throw new InvalidOperationException(
        "No ambient UI.Overlay is available. Call UI.Start first, or use Show(overlay)."));

    /// <summary>Shows the dialog modally over the given <paramref name="overlay"/>.</summary>
    public void Show(Overlay overlay)
    {
        _overlay = overlay;
        _completed = false;
        Result = DialogResult.None;
        if (!_wired) { OnLostFocus += OnDialogLostFocus; _wired = true; }
        // The dialog owns Escape itself (so it can report the cancel result), rather than the overlay's CloseKey
        // silently hiding it. Saved and restored on dismissal.
        _prevCloseKey = overlay.CloseKey;
        overlay.CloseKey = null;
        overlay.ShowModal(this);
    }

    /// <summary>Dismisses the dialog with <paramref name="result"/> (the same path a button takes).</summary>
    public void Close(DialogResult result) => Complete(result, hide: true);

    /// <summary>Confirmation dialog (Yes/No). Invokes <paramref name="onResult"/> with <see langword="true"/> for Yes.</summary>
    public static Dialog Confirm(string title, string message, Action<bool> onResult)
    {
        var d = new Dialog(title, message, DialogButtons.YesNo);
        d.Completed += (_, r) => onResult(r == DialogResult.Yes);
        d.Show();
        return d;
    }

    /// <summary>Message dialog (a single OK button). <paramref name="onOk"/> runs when dismissed.</summary>
    public static Dialog Message(string title, string message, Action? onOk = null)
    {
        var d = new Dialog(title, message, DialogButtons.Ok);
        if (onOk is not null) d.Completed += (_, _) => onOk();
        d.Show();
        return d;
    }

    /// <summary>Shows a custom-content modal with the given buttons, reporting the result to <paramref name="onResult"/>.</summary>
    public static Dialog Show(string title, Control content, DialogButtons buttons, Action<DialogResult> onResult)
    {
        var d = new Dialog(title, content, buttons);
        d.Completed += (_, r) => onResult(r);
        d.Show();
        return d;
    }

    // After the content lays out, shrink a custom-content dialog to its content's natural height so a short control
    // (e.g. a one-line input) doesn't leave a tall empty box. One-shot (guarded), and skipped for message text
    // (already sized exactly). Re-sizing re-centers the dialog in the overlay.
    /// <inheritdoc/>
    protected override void Control_OnInitialization()
    {
        base.Control_OnInitialization();
        if (_sized || _content is DialogText) return;
        var natural = _content.ActualHeight;
        if (natural <= 0) return;
        _sized = true;
        var target = Math.Min(MaxCustomHeight, natural + (_bar is null ? 0 : BarHeight));
        if (target != Height) Height = target;
    }

    /// <inheritdoc/>
    protected override void ApplyTheme()
    {
        _surfaceBg = UI.StyleTheme.Surface.BackgroundColor?.ToConsoleGUIColor();
        if (Frame is { } f)
        {
            if (UI.StyleTheme.Surface.BackgroundColor is { } bg) f.Background = bg;
            if (UI.StyleTheme.Primary.BackgroundColor is { } accent) f.BorderFgColor = accent;
        }
    }

    // A dialog is content-sized (its Width/Height are set in the ctor, and that explicit Height is what
    // Control.CalculateSize honours) and never scrolls, so it does not implement IScrollable: its frame sizes it to
    // the viewport and reserves no scrollbar column.

    private void OnDialogLostFocus()
    {
        if (_completed) return;

        // Losing focus is NOT a dismissal, and assuming it was is a trap worth naming: clicking a field inside the
        // dialog moves focus to that child — a nested composite is its own focus unit, so the dialog itself stops
        // being the focused control — and this fired, completing the dialog with its cancel result on the FIRST
        // click inside it. After that `_completed` swallowed every button, so the dialog sat there with OK and
        // Cancel both dead. Content with no focusable children (the Message/Confirm helpers) never showed it.
        //
        // What this is really a backstop for is the overlay dropping the dialog without going through Close, so
        // ask the overlay that directly instead of inferring it from focus.
        if (_overlay is { } overlay && overlay.IsShowing && ReferenceEquals(overlay.Top, this)) return;

        Complete(_cancelResult, hide: false);
    }

    private void Complete(DialogResult result, bool hide)
    {
        if (_completed) return;
        _completed = true;
        Result = result;
        if (_overlay is not null) _overlay.CloseKey = _prevCloseKey;   // restore the overlay's own close handling
        if (hide) _overlay?.Hide();
        Completed?.Invoke(this, result);
    }

    // The dialog's own key tunnel (runs on the content layout before the key reaches the focused child): Escape
    // cancels (reporting the cancel result), and Tab / Shift+Tab move focus between the stops (custom content <->
    // button bar). ←/→ within the bar are handled by the bar itself.
    private bool OnBodyKey(UI.InputEventArgs e)
    {
        if (e.InputEvent is not { } ev) return false;
        if (ev.Key.Key == ConsoleKey.Escape) { Complete(_cancelResult, hide: true); return true; }
        if (ev.Key.Key == ConsoleKey.Tab && _stops.Count >= 2)
        {
            MoveFocus((ev.Key.Modifiers & ConsoleModifiers.Shift) != 0 ? -1 : +1);
            return true;
        }
        return false;
    }

    private void MoveFocus(int dir)
    {
        var i = _stops.FindIndex(s => s.IsFocused);
        if (i < 0) i = 0;
        var j = ((i + dir) % _stops.Count + _stops.Count) % _stops.Count;
        if (j == i) return;
        _stops[i].IsFocused = false;
        _stops[j].IsFocused = true;
        _current = _stops[j];
    }

    private static (string Label, DialogResult Result)[] ButtonSpecFor(DialogButtons b) => b switch
    {
        DialogButtons.Ok => [("OK", DialogResult.Ok)],
        DialogButtons.OkCancel => [("OK", DialogResult.Ok), ("Cancel", DialogResult.Cancel)],
        DialogButtons.YesNo => [("Yes", DialogResult.Yes), ("No", DialogResult.No)],
        DialogButtons.YesNoCancel => [("Yes", DialogResult.Yes), ("No", DialogResult.No), ("Cancel", DialogResult.Cancel)],
        DialogButtons.Close => [("Close", DialogResult.Close)],
        _ => [],
    };

    private static DialogResult CancelResultFor(DialogButtons b) => b switch
    {
        DialogButtons.OkCancel or DialogButtons.YesNoCancel => DialogResult.Cancel,
        DialogButtons.YesNo => DialogResult.No,
        DialogButtons.Close => DialogResult.Close,
        DialogButtons.Ok => DialogResult.Ok,
        _ => DialogResult.None,
    };
    #endregion

    #region Fields
    internal const int DefaultContentWidth = 44;
    private const int DefaultCustomHeight = 10;   // initial guess; corrected to the content's natural height on layout
    private const int MaxCustomHeight = 30;
    private const int BarHeight = 2;   // a blank spacer row + the button row

    private readonly Control _content;
    private readonly DialogButtonBar? _bar;
    private readonly List<Control> _stops;
    private readonly DialogResult _cancelResult;
    private Control? _current;
    private Overlay? _overlay;
    private ConsoleGUI.Data.Color? _surfaceBg;
    private ConsoleKey? _prevCloseKey;
    private bool _completed;
    private bool _wired;
    private bool _sized;   // custom-content dialog has been shrunk to its content height (one-shot)
    #endregion
}

/// <summary>The wrapped, opaque text body of a message <see cref="Dialog"/>. Non-focusable; each row is padded to
/// the full width with the surface colour so the box reads as a solid panel.</summary>
internal sealed class DialogText : RenderableControl
{
    #region Constructors
    public DialogText(string message, int width)
    {
        Focusable = false;
        ApplyTheme();
        _lines = Wrap(message ?? string.Empty, Math.Max(1, width - 2)).ToArray();
        Width = width;
        Height = PreferredHeight;
    }
    #endregion

    #region Properties
    /// <summary>The height this text wants: a leading blank row plus one row per wrapped line.</summary>
    public int PreferredHeight => Math.Max(1, _lines.Length + 1);
    #endregion

    #region Methods
    protected override bool RendersInteractiveState => false;

    protected override void ApplyTheme()
    {
        _textStyle = UI.StyleTheme.Text | UI.StyleTheme.Surface;   // text foreground over the surface background
        _surface = UI.StyleTheme.Surface;
    }

    protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        var width = Math.Max(1, maxWidth);
        yield return new Segment(new string(' ', width), _surface.SpectreConsoleStyle);   // top padding row
        yield return Segment.LineBreak;
        for (var i = 0; i < _lines.Length; i++)
        {
            var line = " " + _lines[i];
            line = line.Length >= width ? line[..width] : line + new string(' ', width - line.Length);
            yield return new Segment(line, _textStyle.SpectreConsoleStyle);
            if (i < _lines.Length - 1) yield return Segment.LineBreak;
        }
    }

    // Greedy word wrap of plain text (paragraphs split on '\n'), hard-breaking any word longer than the width.
    private static IEnumerable<string> Wrap(string text, int width)
    {
        foreach (var para in text.Replace("\r", string.Empty).Split('\n'))
        {
            var line = new StringBuilder();
            foreach (var word in para.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.Length == 0) line.Append(word);
                else if (line.Length + 1 + word.Length <= width) line.Append(' ').Append(word);
                else { yield return line.ToString(); line.Clear(); line.Append(word); }
                while (line.Length > width)
                {
                    yield return line.ToString(0, width);
                    var rest = line.ToString(width, line.Length - width);
                    line.Clear();
                    line.Append(rest);
                }
            }
            yield return line.ToString();
        }
    }
    #endregion

    #region Fields
    private readonly string[] _lines;
    private Style _textStyle;
    private Style _surface;
    #endregion
}

/// <summary>The button row of a <see cref="Dialog"/>: a single self-drawn, focusable control that lays out its
/// buttons centered on an opaque surface row, moves the highlight with ←/→ (or Tab), and activates on Enter/Space or
/// a click — raising <see cref="Activated"/> with the button's result.</summary>
internal sealed class DialogButtonBar : RenderableControl
{
    #region Constructors
    public DialogButtonBar((string Label, DialogResult Result)[] spec)
    {
        _spec = spec;
        ApplyTheme();
    }
    #endregion

    #region Events
    public event Action<DialogResult>? Activated;
    #endregion

    #region Properties
    public override bool HandlesInput => true;
    protected override bool WantsMouse => true;
    protected override bool RendersOwnFocus => true;   // highlights the active button

    protected internal override HelpInfo? GetHelpInfo() => new HelpInfo("Dialog", "Dialog buttons", "The dialog's actions.")
        .WithKey("← / →", "Move between buttons")
        .WithKey("Enter / Space", "Activate")
        .WithKey("Esc", "Cancel");
    #endregion

    #region Methods
    protected override int IntrinsicHeight() => 2;   // a blank spacer row + the button row

    protected override void ApplyTheme()
    {
        _active = UI.StyleTheme.Primary;
        _inactive = UI.StyleTheme.Secondary;
        _surface = UI.StyleTheme.Surface;
    }

    protected override void OnInput(InputEvent e)
    {
        switch (e.Key.Key)
        {
            case ConsoleKey.LeftArrow: Move(-1); e.Handled = true; break;
            case ConsoleKey.RightArrow or ConsoleKey.Tab: Move(+1); e.Handled = true; break;
            case ConsoleKey.Enter or ConsoleKey.Spacebar: Activated?.Invoke(_spec[_focused].Result); e.Handled = true; break;
        }
    }

    protected override void OnMouseMove(Position position)
    {
        var i = HitTest(position.X);
        if (i >= 0 && i != _focused) { _focused = i; Invalidate(); }
    }

    protected override void OnClick(Position position)
    {
        var i = HitTest(position.X);
        if (i >= 0) Activated?.Invoke(_spec[i].Result);
    }

    // The second click of a rapid pair arrives here, not at OnClick — without this a double-click on a dialog
    // button would be swallowed.
    protected override void OnDoubleClick(Position position) => OnClick(position);

    private void Move(int dir)
    {
        _focused = ((_focused + dir) % _spec.Length + _spec.Length) % _spec.Length;
        Invalidate();
    }

    private int HitTest(int x)
    {
        for (var i = 0; i < _bounds.Length; i++)
            if (x >= _bounds[i].Start && x < _bounds[i].End) return i;
        return -1;
    }

    protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        var width = Math.Max(1, maxWidth);
        var surface = _surface.SpectreConsoleStyle;

        yield return new Segment(new string(' ', width), surface);   // spacer row above the buttons
        yield return Segment.LineBreak;

        var labels = _spec.Select(s => $"  {s.Label}  ").ToArray();
        var total = labels.Sum(l => l.Length) + Math.Max(0, labels.Length - 1);   // 1-space gaps between buttons
        var col = Math.Max(0, (width - total) / 2);
        var bounds = new (int Start, int End)[labels.Length];

        if (col > 0) yield return new Segment(new string(' ', col), surface);
        for (var i = 0; i < labels.Length; i++)
        {
            var style = (i == _focused && IsFocused) ? _active : _inactive;
            yield return new Segment(labels[i], style.SpectreConsoleStyle);
            bounds[i] = (col, col + labels[i].Length);
            col += labels[i].Length;
            if (i < labels.Length - 1) { yield return new Segment(" ", surface); col++; }
        }
        if (width - col > 0) yield return new Segment(new string(' ', width - col), surface);
        _bounds = bounds;
    }
    #endregion

    #region Fields
    private readonly (string Label, DialogResult Result)[] _spec;
    private int _focused;
    private (int Start, int End)[] _bounds = [];
    private Style _active;
    private Style _inactive;
    private Style _surface;
    #endregion
}

/// <summary>A <see cref="VerticalStackPanel"/> that lets its owner tunnel input (e.g. a <see cref="Dialog"/>'s
/// Tab focus-cycling) before it routes to the focused child.</summary>
internal sealed class InterceptStack : VerticalStackPanel
{
    public InterceptStack(params IFocusable[] children) : base(children) { }

    public Func<UI.InputEventArgs, bool>? Interceptor { get; set; }

    protected override bool InterceptInput(UI.InputEventArgs inputEventArgs) => Interceptor?.Invoke(inputEventArgs) ?? false;
}
