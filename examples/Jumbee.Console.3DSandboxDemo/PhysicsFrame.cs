namespace Jumbee.Console.SandboxDemo;

using System.Numerics;

using Jumbee.Console.Mesh;

/// <summary>
/// One tick of physics state: the <see cref="SceneFrame"/> the renderers draw, plus the columns only the sandbox's
/// footer and inspector read.
/// </summary>
/// <remarks>
/// Derived rather than published alongside, so the statistics can never describe a different tick from the picture:
/// the footer resolves a selected body's index in this frame and reads its velocity from the same object.
/// </remarks>
public sealed class PhysicsFrame : SceneFrame
{
    #region Constructors
    /// <summary>Allocates a frame sized for <paramref name="capacity"/> bodies.</summary>
    public PhysicsFrame(int capacity) : base(capacity)
    {
        Velocities = new Vector3[capacity];
        Masses = new float[capacity];
        Awake = new bool[capacity];
    }
    #endregion

    #region Properties
    /// <summary>Simulated time in seconds — steps taken times the fixed step, not wall clock.</summary>
    public double SimTime { get; set; }

    /// <summary>Fixed steps taken since the world was created.</summary>
    public long StepCount { get; set; }

    /// <summary>How many bodies the engine still considers awake.</summary>
    public int AwakeCount { get; set; }

    /// <summary>Wall-clock milliseconds the last batch of steps cost, for the HUD.</summary>
    public double StepMilliseconds { get; set; }
    #endregion

    #region Fields
    /// <summary>Linear velocities, for the inspector.</summary>
    public readonly Vector3[] Velocities;

    /// <summary>Body masses, for the inspector.</summary>
    public readonly float[] Masses;

    /// <summary>Whether the engine still considers each body awake.</summary>
    public readonly bool[] Awake;
    #endregion
}
