namespace Jumbee.Console.Tests;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using ConsoleGUI;
using ConsoleGUI.Controls;
using ConsoleGUI.Api;
using ConsoleGUI.Data;
using ConsoleGUI.Space;

using Jumbee.Console;

using Xunit;

/// <summary>
/// The renderer must never rely on the terminal wrapping a line for it.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="UI.Start"/> turns autowrap off (DECAWM, <c>CSI ?7l</c>) for the session, because a full-screen
/// renderer writes the bottom-right cell and, with autowrap on, a terminal that resolves the resulting pending-wrap
/// eagerly instead of deferring it scrolls the screen by a row. After an unnoticed scroll the emitter's diff model
/// is a row out of step with the display, so every cell it believes unchanged is stale — the failure smears across
/// the whole screen rather than costing one frame. ConEmu reproduces it; conhost, Windows Terminal and xterm defer
/// the wrap and do not, which is why it went unnoticed for so long.
/// </para>
/// <para>
/// Turning DECAWM off is only safe while the emitter positions every row explicitly, which is what these tests
/// pin. They assert on the raw escape stream rather than the parsed screen: a parser reconstructs the same picture
/// whether the emitter wrapped or jumped, so the distinction is invisible to <c>AnsiConsoleSnapshot</c> and would
/// be lost the moment someone "optimised" the run-continuation check.
/// </para>
/// </remarks>
public class AnsiWrapIndependenceTests
{
    public AnsiWrapIndependenceTests() => UiTestHarness.EnsureStopped();

    [Fact]
    public async Task Emitter_NeverWritesPastTheLastColumn()
    {
        const int width = 24, height = 4;
        var raw = await CaptureAsync(Filled(), width, height);

        var (maxColumn, printed) = Walk(raw, width);

        Assert.True(printed > 0, "the render emitted no glyphs at all — the test is not exercising anything");
        Assert.True(maxColumn < width,
            $"a glyph was emitted at column {maxColumn} of a {width}-column screen. With autowrap off that glyph " +
            "is dropped; with it on the screen scrolls. Either way the emitter must have issued a CUP instead.");
    }

    // The assertion above is only meaningful if the render actually reaches the last column -- otherwise it passes
    // by never approaching the edge. This pins that the risky case is genuinely covered.
    [Fact]
    public async Task Emitter_DoesWriteTheLastColumn_SoTheGuaranteeIsNotVacuous()
    {
        const int width = 24, height = 4;
        var raw = await CaptureAsync(Filled(), width, height);

        var (maxColumn, _) = Walk(raw, width);

        Assert.Equal(width - 1, maxColumn);
    }

    #region Harness
    // A control that paints a background over its whole area, so every cell differs from the blank the diff starts
    // from and the emitter has to write all of them -- including the bottom-right one.
    private static IControl Filled() => new Background { Color = new ConsoleGUI.Data.Color(20, 30, 40) };

    // Walks the raw escape stream the way a terminal would: CUP sets the column, printable characters advance it.
    // Returns the highest column any glyph landed on, and how many glyphs there were.
    private static (int MaxColumn, int Printed) Walk(string raw, int width)
    {
        var column = 0;
        var maxColumn = -1;
        var printed = 0;

        for (var i = 0; i < raw.Length; i++)
        {
            var c = raw[i];
            if (c == '\x1b')
            {
                // CSI ... final-byte. Only CUP ('H') moves the column; everything else (SGR, cursor show/hide) does
                // not, and OSC/other introducers do not appear on this path.
                var j = i + 1;
                if (j < raw.Length && raw[j] == '[')
                {
                    var start = ++j;
                    while (j < raw.Length && !char.IsLetter(raw[j])) j++;
                    if (j < raw.Length)
                    {
                        if (raw[j] == 'H')
                        {
                            var parts = raw[start..j].Split(';');
                            // CSI row;colH — 1-based, defaulting to 1 when omitted.
                            column = parts.Length > 1 && int.TryParse(parts[1], out var col) ? col - 1 : 0;
                        }

                        i = j;
                        continue;
                    }
                }

                i = j;
                continue;
            }

            if (c is '\n' or '\r') { column = 0; continue; }

            maxColumn = Math.Max(maxColumn, column);
            printed++;
            column++;
        }

        return (maxColumn, printed);
    }

    private static async Task<string> CaptureAsync(IControl content, int width, int height)
    {
        var captured = new StringBuilder();
        var previousOutput = ConsoleManager.AnsiOutput;
        var previousAnsi = ConsoleManager.AnsiEnabled;
        try
        {
            ConsoleManager.AnsiEnabled = true;
            ConsoleManager.AnsiOutput = acsb =>
            {
                var s = acsb.ToString();
                return Task.Run(() => { lock (captured) captured.Append(s); });
            };
            ConsoleManager.Console = new SizedConsole(width, height);
            ConsoleManager.Setup();
            ConsoleManager.Content = content;

            UI.PaintFrame();
            ConsoleManager.Redraw();
            await ConsoleManager.OutputIdle.ConfigureAwait(false);
        }
        finally
        {
            ConsoleManager.AnsiOutput = previousOutput;
            ConsoleManager.AnsiEnabled = previousAnsi;
        }

        lock (captured) return captured.ToString();
    }

    private sealed class SizedConsole(int w, int h) : IConsole
    {
        public Size Size { get; set; } = new Size(w, h);
        public bool KeyAvailable => false;
        public void Initialize() { }
        public void OnRefresh() { }
        public void Write(Position position, in Character character) { }
        public ConsoleKeyInfo ReadKey() => throw new NotSupportedException();
    }
    #endregion
}
