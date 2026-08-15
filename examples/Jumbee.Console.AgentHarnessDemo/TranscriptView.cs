namespace Jumbee.Console.AgentHarnessDemo;

using System;
using System.Collections.Generic;

using ConsoleGUI.Input;
using ConsoleGUI.Space;

using Jumbee.Console;

using Spectre.Console.Rendering;

/// <summary>
/// The centre chat pane — a scrollable transcript of <see cref="ChatBlock"/>s: right-aligned user bubbles, styled
/// assistant prose, and inline tool-call chips. Follows <see cref="MarkdownViewer"/>'s offscreen-buffer approach
/// (render the whole transcript to a tall <see cref="ConsoleBuffer"/> once per width/version, then blit the visible
/// window) so a surrounding <c>WithFrame</c> scrolls it. New content sticks the view to the bottom, like a chat log.
/// </summary>
internal sealed class TranscriptView : Control, IScrollable
{
    #region Methods
    /// <summary>Appends a user prompt bubble.</summary>
    public void AddUser(string text)
    {
        _blocks.Add(new UserBlock(text));
        Bump();
    }

    /// <summary>Appends an assistant prose block (Spectre markup) and returns it so the caller can stream into
    /// <see cref="AssistantBlock.Markdown"/> and call <see cref="Refresh"/>.</summary>
    public AssistantBlock AddAssistant(string markup = "")
    {
        var block = new AssistantBlock(markup);
        _blocks.Add(block);
        Bump();
        return block;
    }

    /// <summary>Appends a tool-call chip and returns it so the caller can flip its <see cref="ToolBlock.Status"/>.</summary>
    public ToolBlock AddTool(string glyph, string label, Color accent, string? detail = null)
    {
        var block = new ToolBlock(glyph, label, accent, detail);
        _blocks.Add(block);
        Bump();
        return block;
    }

    /// <summary>Re-renders after a caller mutated a returned block handle, and re-sticks to the bottom.</summary>
    public void Refresh() => Bump();

    /// <summary>Scrolls to the newest message. Call once the UI is live to open pinned to the bottom (the per-add
    /// stick is a no-op during seeding, before the scroll frame is attached).</summary>
    public void ScrollToBottom() => UI.Invoke(() => { StickToBottom(); Invalidate(); });

    public void Clear()
    {
        _blocks.Clear();
        Bump();
    }

    // Content re-render + relayout + stick to the bottom, marshaled to the UI thread (inline before UI.Start).
    private void Bump() => UI.Invoke(() =>
    {
        _version++;
        Initialize();
        StickToBottom();
    });

    // A viewer paints its own content; skip the whole-document focus tint.
    protected override bool RendersOwnFocus => true;
    public override bool HandlesInput => true;

    public int MeasureHeight(int width)
    {
        EnsureRender(Math.Max(1, width));
        return Math.Max(1, _contentHeight);
    }

    protected override void Render()
    {
        EnsureRender(Math.Max(1, Size.Width));
        consoleBuffer.Initialize();
        Blit();
    }

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
            case ConsoleKey.End: StickToBottom(); inputEvent.Handled = true; break;
        }
    }

    protected override HelpInfo? GetHelpInfo() => new HelpInfo("Transcript", "Chat transcript",
        "The scrollable conversation between you and the agent.")
        .WithKey("↑ / ↓", "Scroll a line")
        .WithKey("PgUp / PgDn", "Scroll a page");

    private void StickToBottom()
    {
        if (Frame is { } f) f.Top = int.MaxValue / 2;   // clamps to the last page
    }

    // Renders the whole transcript into the reusable offscreen buffer at `width` (once per width/version). Synchronous:
    // the transcript is small and we control when it changes, so no background thread is needed.
    private void EnsureRender(int width)
    {
        if (width <= 0) return;
        if (_renderedWidth == width && _renderedVersion == _version) return;

        var content = BuildContent();
        var cap = Math.Clamp(_blocks.Count * 6 + 24, 8, MaxRows);
        _content.Size = new Size(width, cap);   // capacity-retentive: no realloc once the high-water mark is reached
        _content.Initialize();
        try
        {
            new AnsiConsoleBuffer(_content).Write(content);
            _contentHeight = MeasureRenderedHeight(_content);
        }
        catch
        {
            _contentHeight = 1;
        }
        _renderedWidth = width;
        _renderedVersion = _version;
    }

    private IRenderable BuildContent()
    {
        var items = new List<IRenderable>(_blocks.Count * 2);
        foreach (var block in _blocks)
        {
            items.Add(RenderBlock(block));
            items.Add(new Spectre.Console.Text(" "));   // one blank row between blocks
        }
        return new Spectre.Console.Rows(items);
    }

    private static IRenderable RenderBlock(ChatBlock block)
    {
        try
        {
            return block switch
            {
                UserBlock u => RenderUser(u),
                AssistantBlock a => new Spectre.Console.Markup(a.Markdown.Length == 0 ? " " : a.Markdown,
                    new Spectre.Console.Style(foreground: Palette.Text)),
                ToolBlock t => new Spectre.Console.Markup(RenderTool(t)),
                _ => new Spectre.Console.Text(""),
            };
        }
        catch
        {
            // A malformed markup string must never take down the pane — fall back to escaped plain text.
            var text = block switch { UserBlock u => u.Text, AssistantBlock a => a.Markdown, ToolBlock t => t.Label, _ => "" };
            return new Spectre.Console.Text(text);
        }
    }

    // A rounded, right-aligned bubble sized to its content — the user's prompt.
    private static IRenderable RenderUser(UserBlock u)
    {
        var panel = new Spectre.Console.Panel(new Spectre.Console.Markup(
            Spectre.Console.Markup.Escape(u.Text), new Spectre.Console.Style(foreground: Palette.Text)))
        {
            Border = Spectre.Console.BoxBorder.Rounded,
            BorderStyle = new Spectre.Console.Style(foreground: Palette.CoralDim),
            Padding = new Spectre.Console.Padding(1, 0, 1, 0),
            Expand = false,
        };
        return new Spectre.Console.Align(panel, Spectre.Console.HorizontalAlignment.Right);
    }

    // "◇ Read 6 files +29 -0 ›" — glyph in the tool's accent, label in text (red when errored), a coloured detail
    // suffix (+adds green / -dels red), and a faint disclosure chevron.
    private static string RenderTool(ToolBlock t)
    {
        var labelColor = Hex(t.Status == ToolStatus.Error ? Palette.Red : Palette.Text);
        var line = $"[{Hex(t.Accent)}]{Escape(t.Glyph)}[/] [{labelColor}]{Escape(t.Label)}[/]";
        if (!string.IsNullOrEmpty(t.Detail)) line += "  " + ColorDetail(t.Detail);
        return line + $" [{Hex(Palette.TextFaint)}]›[/]";
    }

    // Colours a detail like "+29 -0" token-by-token: additions green, deletions red, anything else faint.
    private static string ColorDetail(string detail)
    {
        var parts = detail.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var pieces = new string[parts.Length];
        for (var i = 0; i < parts.Length; i++)
        {
            var color = parts[i].StartsWith('+') ? Palette.Green : parts[i].StartsWith('-') ? Palette.Red : Palette.TextFaint;
            pieces[i] = $"[{Hex(color)}]{Escape(parts[i])}[/]";
        }
        return string.Join(' ', pieces);
    }

    private static string Escape(string s) => Spectre.Console.Markup.Escape(s);
    private static string Hex(Color c) => "#" + ((Spectre.Console.Color)c).ToHex();

    // The last non-blank row + 1 — the true rendered height (scan the buffer; a Spectre renderable's cursor isn't a
    // reliable end marker across every renderable). Mirrors MarkdownViewer.
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
    #endregion

    #region Fields
    private const int MaxRows = 2048;

    private readonly List<ChatBlock> _blocks = new();
    private readonly ConsoleBuffer _content = new();
    private int _contentHeight;
    private int _version;
    private int _renderedWidth = -1;
    private int _renderedVersion = -1;
    #endregion
}
