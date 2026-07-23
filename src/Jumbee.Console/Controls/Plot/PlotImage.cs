namespace Jumbee.Console;

using ConsoleGUI.Api;
using ConsoleGUI.Data;
using ConsoleGUI.Space;

using CPlot = ConsolePlot.Plot;
using CColor = ConsoleGUI.Data.Color;

/// <summary>
/// A <see cref="ConsolePlot.Plot"/> that draws <b>straight into</b> a <see cref="ConsoleBuffer"/> — the plot's cell
/// surface <i>is</i> the target buffer, so there's no intermediate pixel array to copy out each frame. Used internally
/// by <see cref="Plot"/>. The y-flip (ConsolePlot is y-up, the buffer top-down) is handled inside ConsolePlot's
/// <c>ConsoleImage</c>; a set <see cref="Plot.Background"/> is applied by <see cref="BackgroundBuffer"/>.
/// </summary>
internal sealed class PlotImage : CPlot
{
    #region Constructors
    /// <summary>Creates a plot that renders into <paramref name="buffer"/>. When <paramref name="background"/> is set,
    /// transparent cells are filled with it (what the old copy pass did); otherwise writes go straight through.</summary>
    public PlotImage(ConsoleBuffer buffer, CColor? background = null)
        : base(background is { } bg ? (IConsoleBuffer)new BackgroundBuffer(buffer, bg) : buffer)
    {
    }
    #endregion

    #region Methods
    /// <summary>No-op: <see cref="ConsolePlot.Plot.Draw"/> already wrote the plot into the target buffer.</summary>
    public override void Render() { }
    #endregion
}

/// <summary>
/// An <see cref="IConsoleBuffer"/> wrapper that fills transparent (null-background) writes with a fixed colour — the
/// plot's overall <see cref="Plot.Background"/>. Only used when a background is set; the transparent-background default
/// passes the underlying <see cref="ConsoleBuffer"/> through with no wrapper.
/// </summary>
internal sealed class BackgroundBuffer : IConsoleBuffer
{
    #region Constructors
    public BackgroundBuffer(ConsoleBuffer inner, CColor background)
    {
        _inner = inner;
        _background = background;
    }
    #endregion

    #region Methods
    public Size Size => _inner.Size;

    public Character CharacterAt(int x, int y) => _inner.CharacterAt(x, y);

    public void Write(int x, int y, in Character character) =>
        _inner.Write(x, y, character.Background is null ? character.WithBackground(_background) : character);
    #endregion

    #region Fields
    private readonly ConsoleBuffer _inner;
    private readonly CColor _background;
    #endregion
}
