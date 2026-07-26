namespace Jumbee.Console.Tests;

using System;
using System.Linq;

using ConsoleGUI.Api;
using ConsoleGUI.Data;
using ConsoleGUI.Space;

using Xunit;

using CPlot = ConsolePlot.Plot;

/// <summary>
/// Guards the cost of redrawing a plot. <c>PlotRenderer.Draw</c> opens with a clear, and that clear used to blank
/// every cell in the surface — one write per cell, on every redraw, however little was actually drawn. A live plot
/// (<see cref="Plot.AddLiveSeries"/>) redraws on every data tick, so that was the dominant per-frame cost for a
/// sparse figure. <c>ConsoleImage.ClearDrawn</c> now erases only the cells drawn since the previous clear.
/// </summary>
/// <remarks>
/// These assert the SHAPE of the cost (writes scale with content, not area) rather than an exact number, so they
/// survive changes to how a series rasterises but still fail if the whole-surface clear comes back.
/// </remarks>
public class PlotClearCostTests
{
    private const int Width = 240;
    private const int Height = 30;
    private const int Area = Width * Height;   // 7200 cells

    /// <summary>An <see cref="IConsoleBuffer"/> that counts the writes a draw performs.</summary>
    private sealed class CountingBuffer : IConsoleBuffer
    {
        private readonly Character[] _cells = new Character[Area];

        public int Writes { get; private set; }

        public void ResetCount() => Writes = 0;

        public Size Size => new(Width, Height);

        public Character CharacterAt(int x, int y) => _cells[(y * Width) + x];

        public void Write(int x, int y, in Character character)
        {
            Writes++;
            _cells[(y * Width) + x] = character;
        }
    }

    private static CPlot SparsePlot(CountingBuffer buffer)
    {
        var plot = new CPlot(buffer);
        plot.Grid.IsVisible = false;
        plot.Ticks.IsVisible = false;
        plot.Ticks.Labels.IsVisible = false;
        // A short flat line: a handful of cells out of 7200.
        plot.AddSeries(Enumerable.Range(0, 20).Select(i => (double)i).ToArray(),
                       Enumerable.Repeat(0.5, 20).ToArray());
        return plot;
    }

    [Fact]
    public void Redraw_WritesScaleWithContent_NotSurfaceArea()
    {
        var buffer = new CountingBuffer();
        var plot = SparsePlot(buffer);

        plot.Draw();                       // first draw blanks the whole surface (nothing is known about it yet)
        Assert.True(buffer.Writes >= Area, $"the first draw should clear everything, wrote {buffer.Writes}");

        buffer.ResetCount();
        plot.Draw();                       // steady state: erase only what the previous draw touched, then redraw
        var redraw = buffer.Writes;

        Assert.True(redraw < Area / 4,
            $"a sparse redraw should cost far less than the {Area}-cell surface, but wrote {redraw}");
    }

    [Fact]
    public void FirstDrawAfterConstruction_StillBlanksEverything()
    {
        // A plot built over a buffer it did not fill cannot know what is already there — for the hosted control that
        // is the previous figure, left behind when a resize or rebuild replaced the image. Skipping the full clear
        // here would leave that content on screen.
        var buffer = new CountingBuffer();
        for (var y = 0; y < Height; y++)
            for (var x = 0; x < Width; x++)
                buffer.Write(x, y, new Character('#'));
        buffer.ResetCount();

        SparsePlot(buffer).Draw();

        var stale = 0;
        for (var y = 0; y < Height; y++)
            for (var x = 0; x < Width; x++)
                if (buffer.CharacterAt(x, y).Content == '#') stale++;
        Assert.Equal(0, stale);
    }
}
