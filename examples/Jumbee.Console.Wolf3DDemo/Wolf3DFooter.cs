#nullable enable

namespace Jumbee.Console.Wolf3DDemo;

using ConsoleGUI.Data;
using ConsoleGUI.Space;

/// <summary>Two lines under the viewport: where the player is and what the frame cost, then the keys.</summary>
public sealed class Wolf3DFooter : Control
{
    #region Constructors
    /// <summary>Creates the footer over <paramref name="view"/>, whose state it reports.</summary>
    public Wolf3DFooter(Wolf3DView view)
    {
        this.view = view;
        Focusable = false;
        Height = 2;
    }
    #endregion

    #region Methods
    /// <summary>Repaints the readouts. Called from <see cref="Wolf3DView.Changed"/>, so the numbers describe the
    /// frame on screen rather than one the view has not drawn yet.</summary>
    public void Report() => Invalidate();

    /// <inheritdoc/>
    protected override void Render()
    {
        var scene = view.Scene;
        var (colors, runs) = view.LastCost;
        // Runs are the honest predictor of the ANSI cost, so they are what the readout carries -- a colour count
        // looks like the interesting number and is not: the same 15 colours cost wildly different bytes depending
        // on how finely they are interleaved.
        var cost = runs > 0 ? $"{runs} runs" : "quad";
        var status = $" {scene.Map.Name}  [{scene.LevelIndex + 1}/{scene.Levels.Count}]  " +
                     $"x {scene.X,5:F1}  y {scene.Y,5:F1}  {scene.Bearing,3:F0}°  │  " +
                     $"{view.FramesPerSecond,4:F0} fps  {colors} colours  {cost}  " +
                     $"{(view.Renderer.QuantizeLevels > 1 ? $"q{view.Renderer.QuantizeLevels}" : "full")}" +
                     $"{(view.QuadrantSampling ? " · AA" : "")}";

        const string Keys = " w/s move · a/d turn · q/e strafe · shift run · [] level · r restart · " +
                            "1 quantize · 2 AA · 3 fov · F1 keys · esc quit";

        WriteRow(0, status, new Color(206, 210, 220), new Color(28, 30, 38));
        WriteRow(1, Keys, new Color(140, 146, 160), new Color(22, 24, 30));
    }
    #endregion

    #region Private methods
    private void WriteRow(int row, string text, Color fg, Color bg)
    {
        if (row >= ActualHeight) return;
        for (var x = 0; x < ActualWidth; x++)
            consoleBuffer.Write(new Position(x, row), new Character(x < text.Length ? text[x] : ' ', fg, bg));
    }
    #endregion

    #region Fields
    private readonly Wolf3DView view;
    #endregion
}
