namespace Jumbee.Console.SandboxDemo;

using ConsoleGUI.Data;
using ConsoleGUI.Space;

/// <summary>Two lines under the viewport: what the simulation is doing, and what the keys will do to it.</summary>
public sealed class SceneFooter : Control
{
    #region Constructors
    /// <summary>Creates the footer over <paramref name="view"/>, whose state it reports.</summary>
    public SceneFooter(SceneView view)
    {
        this.view = view;
        Focusable = false;
        Height = 2;
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
        var spawn = view.Spawn;

        // Line 1 is the run and the spawn settings -- everything that changes. Kept under ~90 columns so it survives
        // a narrow terminal: an earlier version ran to ~110 and silently clipped the selection readout off the end,
        // which looked exactly like selection not working.
        var status = $" {(paused ? "PAUSED" : "RUN   ")} {view.Renderer.Name}  " +
                     $"{s?.Count ?? 0} bodies · {s?.AwakeCount ?? 0} awake  t={s?.SimTime ?? 0:F1}s  " +
                     $"step {s?.StepMilliseconds ?? 0:F2}ms  " +
                     $"spawn {(spawn.Shape == BodyShape.Sphere ? "sphere" : "box")} x{spawn.Scale:F2}  " +
                     $"launch {spawn.LaunchSpeed:F1}";

        // Line 2 leads with the selection so it is never the thing that gets clipped, then fills the rest with key
        // reminders in falling order of usefulness. F1 has the full list, so losing the tail of this is survivable.
        var line2 = $" {Selection(s)}  │ drag orbit · click select · n drop · f fire · b shape · " +
                    "+- size · [] speed · x del · c clear · space pause · . step · r reset · F1 help · q quit";

        WriteRow(0, status, new Color(200, 205, 215), new Color(28, 30, 38));
        WriteRow(1, line2, new Color(140, 146, 160), new Color(22, 24, 30));
    }
    #endregion

    #region Private methods
    private string Selection(SceneSnapshot? s)
    {
        if (view.Selected is not { } id || s is null) return "no selection";
        var i = s.IndexOf(id);
        if (i < 0) return "no selection";

        var speed = s.Velocities[i].Length();
        return $"#{id} {s.Shapes[i].ToString().ToLowerInvariant()} {s.Masses[i]:F0}kg {speed:F1}m/s " +
               $"{(s.Awake[i] ? "awake" : "asleep")}";
    }

    private void WriteRow(int row, string text, Color fg, Color bg)
    {
        if (row >= ActualHeight) return;
        for (var x = 0; x < ActualWidth; x++)
            consoleBuffer.Write(new Position(x, row), new Character(x < text.Length ? text[x] : ' ', fg, bg));
    }
    #endregion

    #region Fields
    private readonly SceneView view;

    private SceneSnapshot? snapshot;
    private bool paused;
    #endregion
}
