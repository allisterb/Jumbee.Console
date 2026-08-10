namespace Jumbee.Console.SandboxDemo;

using ConsoleGUI.Data;
using ConsoleGUI.Space;

/// <summary>One line under the viewport: what the simulation is doing, and the keys that change it.</summary>
public sealed class SceneFooter : Control
{
    #region Constructors
    /// <summary>Creates the footer.</summary>
    public SceneFooter()
    {
        Focusable = false;
        Height = 1;
    }
    #endregion

    #region Properties
    /// <summary>The snapshot to report. Set from <see cref="SceneView.Drew"/>, so the numbers describe the frame on
    /// screen rather than a tick that has not been drawn yet.</summary>
    public SceneSnapshot? Snapshot
    {
        get => snapshot;
        set => SetAtomicProperty(ref snapshot, value);
    }

    /// <summary>The active renderer's name.</summary>
    public string Mode
    {
        get => mode;
        set => SetAtomicProperty(ref mode, value);
    }

    /// <summary>Whether the simulation is paused.</summary>
    public bool Paused
    {
        get => paused;
        set => SetAtomicProperty(ref paused, value);
    }
    #endregion

    #region Methods
    /// <inheritdoc/>
    protected override void Render()
    {
        var s = snapshot;
        var bodies = s?.Count ?? 0;
        var awake = s?.AwakeCount ?? 0;
        var text = $" {(paused ? "PAUSED" : "RUN")}  {mode}  {bodies} bodies ({awake} awake)  " +
                   $"t={s?.SimTime ?? 0:F1}s  step {s?.StepMilliseconds ?? 0:F2}ms   " +
                   "arrows/drag orbit · PgUp/PgDn zoom · Home reset · Space pause · . step · r reset scene · F1 help · q quit";

        var fg = new Color(190, 195, 205);
        var bg = new Color(28, 30, 38);
        for (var x = 0; x < ActualWidth; x++)
            consoleBuffer.Write(new Position(x, 0), new Character(x < text.Length ? text[x] : ' ', fg, bg));
    }
    #endregion

    #region Fields
    private SceneSnapshot? snapshot;
    private string mode = "";
    private bool paused;
    #endregion
}
