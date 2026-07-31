namespace Jumbee.Console;

using System;
using System.Collections.Generic;
using System.Linq;

using ConsoleGUI.Input;
using ConsoleGUI.Space;

using Spectre.Console.Rendering;

/// <summary>
/// Attaches type-ahead suggestions to a <see cref="TextInput"/>.
/// </summary>
/// <remarks>
/// As the user types, matching candidates are shown in a <em>passive</em> popup just below the caret (via
/// <see cref="Overlay.ShowPassive"/>) — the field keeps focus and keeps editing. Up/Down move the highlight,
/// Enter/Tab accept it into the field, Escape dismisses, and the popup also closes when the field loses focus or
/// there are no matches. A suggestion can also be chosen by clicking.
/// </remarks>
public sealed class Autocomplete
{
    #region Constructors
    /// <summary>Attaches type-ahead to <paramref name="input"/>, floating suggestions in the ambient
    /// <see cref="UI.Overlay"/> just below the caret.</summary>
    public Autocomplete(TextInput input, Func<string, IEnumerable<string>> suggest)
    {
        _input = input ?? throw new ArgumentNullException(nameof(input));
        _suggest = suggest ?? throw new ArgumentNullException(nameof(suggest));

        _list.Accepted += (_, s) => Accept(s);
        _input.Changed += (_, _) => Refresh();
        _input.OnLostFocus += Close;          // leaving the field dismisses the popup
        _input.KeyInterceptor = OnKey;        // grab nav/accept/dismiss keys while open
    }

    /// <summary>Convenience: suggests from a fixed candidate list (case-insensitive substring match, prefix matches first).</summary>
    public Autocomplete(TextInput input, params string[] candidates)
        : this(input, DefaultFilter(candidates)) { }
    #endregion

    #region Properties
    /// <summary>Maximum suggestions shown at once. Defaults to 8.</summary>
    public int MaxRows { get => _maxRows; set => _maxRows = Math.Max(1, value); }
    #endregion

    #region Methods
    /// <summary>Closes the suggestion popup if open.</summary>
    public void Close()
    {
        if (!_open) return;
        _open = false;
        UI.Overlay?.Hide();
    }

    private static Func<string, IEnumerable<string>> DefaultFilter(string[] candidates) => text =>
        string.IsNullOrEmpty(text)
            ? []
            : candidates
                .Where(c => c.Contains(text, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(c => c.StartsWith(text, StringComparison.OrdinalIgnoreCase));

    private void Refresh()
    {
        if (_accepting) return;   // re-entrancy from setting Text during Accept

        var text = _input.Text;
        var matches = string.IsNullOrEmpty(text) ? [] : _suggest(text).Take(_maxRows).ToList();

        // Nothing useful to offer (or the only match is exactly what's typed) -> hide.
        if (matches.Count == 0 || (matches.Count == 1 && string.Equals(matches[0], text, StringComparison.Ordinal)))
        {
            Close();
            return;
        }

        if (UI.Overlay is not { } host) return;   // no overlay available yet (e.g. before UI.Start)
        _list.SetItems(matches);
        var (x, y) = AnchorBelowCaret();
        host.ShowPassive(_list, x, y);
        _open = true;
    }

    private static (int x, int y) AnchorBelowCaret()
    {
        // The focused field owns the caret, so ConsoleManager reports its screen position; drop the popup one row
        // below it. (Falls back to the top-left when no caret is shown, e.g. in a headless layout test.)
        if (ConsoleGUI.ConsoleManager.CursorPosition is { } c) return (Math.Max(0, c.X), c.Y + 1);
        return (0, 1);
    }

    private bool OnKey(InputEvent e)
    {
        if (!_open) return false;
        switch (e.Key.Key)
        {
            case ConsoleKey.DownArrow: _list.Move(+1); return true;
            case ConsoleKey.UpArrow: _list.Move(-1); return true;
            case ConsoleKey.Enter:
            case ConsoleKey.Tab:
                if (_list.Selected is { } s) { Accept(s); return true; }
                return false;
            case ConsoleKey.Escape: Close(); return true;
            default: return false;   // other keys edit the field (and re-trigger Refresh)
        }
    }

    private void Accept(string suggestion)
    {
        _accepting = true;
        _input.Text = suggestion;   // moves the caret to the end; the resulting Changed is ignored (see Refresh)
        _accepting = false;
        Close();
    }
    #endregion

    #region Fields
    private readonly TextInput _input;
    private readonly Func<string, IEnumerable<string>> _suggest;
    private readonly SuggestionList _list = new();
    private bool _open;
    private bool _accepting;
    private int _maxRows = 8;
    #endregion
}

/// <summary>The popup list rendered by an <see cref="Autocomplete"/>. Non-focusable (the field keeps focus) but
/// mouse-clickable; highlights one row and raises <see cref="Accepted"/> when a row is chosen.</summary>
internal sealed class SuggestionList : RenderableControl
{
    public SuggestionList()
    {
        Focusable = false;          // never steal focus from the text field
        this.WithRoundedBorder();
    }

    public event EventHandler<string>? Accepted;

    /// <summary>Always <see langword="true"/>: suggestion rows receive hover and click even while the control is
    /// unfocused. No opt-in needed — unlike the base default, this is on.</summary>
    protected override bool WantsMouse => true;

    public string? Selected => _highlighted >= 0 && _highlighted < _items.Count ? _items[_highlighted] : null;

    public void SetItems(IReadOnlyList<string> items)
    {
        _items.Clear();
        _items.AddRange(items);
        _highlighted = 0;
        Width = ContentWidth();
        Height = _items.Count;
        Invalidate();
    }

    public void Move(int dir)
    {
        if (_items.Count == 0) return;
        _highlighted = Math.Clamp(_highlighted + dir, 0, _items.Count - 1);
        Invalidate();
    }

    protected override void OnMouseMove(Position position)
    {
        if (position.Y >= 0 && position.Y < _items.Count) { _highlighted = position.Y; Invalidate(); }
    }

    protected override void OnClick(Position position)
    {
        if (position.Y >= 0 && position.Y < _items.Count) Accepted?.Invoke(this, _items[position.Y]);
    }

    protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        var width = Math.Min(ActualWidth > 0 ? ActualWidth : maxWidth, maxWidth);
        if (width <= 0) yield break;

        for (var i = 0; i < _items.Count; i++)
        {
            var label = " " + _items[i];
            label = label.Length > width ? label[..width] : label.PadRight(width);
            var style = i == _highlighted ? UI.StyleTheme.Selection : UI.StyleTheme.Text;
            yield return new Segment(label, style.SpectreConsoleStyle);
            if (i < _items.Count - 1) yield return Segment.LineBreak;
        }
    }

    private int ContentWidth()
    {
        var w = 0;
        foreach (var item in _items) w = Math.Max(w, item.Length);
        return w + 2;
    }

    private readonly List<string> _items = new();
    private int _highlighted;
}
