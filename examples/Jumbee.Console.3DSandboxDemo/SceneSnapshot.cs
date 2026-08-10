namespace Jumbee.Console.SandboxDemo;

using System.Numerics;

/// <summary>The collision shape a body is drawn as.</summary>
public enum BodyShape : byte
{
    /// <summary>An oriented box, drawn as 12 projected edges.</summary>
    Box,

    /// <summary>A sphere, drawn as one screen-space circle.</summary>
    Sphere,

    /// <summary>A capsule, drawn as its two end spheres joined by the segment between them.</summary>
    Capsule,
}

/// <summary>
/// One tick of physics state, frozen: everything the renderer and the inspector need, and nothing that has to be
/// read back off a live <c>Body</c> while the physics thread is mid-step.
/// </summary>
/// <remarks>
/// <para>
/// <b>Parallel arrays, not an array of structs.</b> It costs nothing today and it is the layout a vectorised
/// rasteriser would need later; retrofitting it is the expensive part. A snapshot is written once by the physics
/// thread, published whole, and then only read — so it needs no locking and the renderer can hold one across as
/// many frames as it likes.
/// </para>
/// <para>
/// Extents cannot be read back from Box3D (<c>Shape.Bounds</c> is a world AABB, which for a rotated box is not the
/// same thing), so <see cref="HalfExtents"/> carries what the body was spawned with. For a
/// <see cref="BodyShape.Sphere"/> only X is meaningful and holds the radius; for a
/// <see cref="BodyShape.Capsule"/>, X is the radius and Y the half-length of the segment.
/// </para>
/// </remarks>
public sealed class SceneSnapshot
{
    #region Constructors
    /// <summary>Allocates a snapshot sized for <paramref name="capacity"/> bodies.</summary>
    public SceneSnapshot(int capacity)
    {
        Ids = new int[capacity];
        Positions = new Vector3[capacity];
        Rotations = new Quaternion[capacity];
        HalfExtents = new Vector3[capacity];
        Velocities = new Vector3[capacity];
        Masses = new float[capacity];
        Shapes = new BodyShape[capacity];
        Awake = new bool[capacity];
        ColorKeys = new int[capacity];
    }
    #endregion

    #region Properties
    /// <summary>How many entries of each array are live.</summary>
    public int Count { get; set; }

    /// <summary>Simulated time in seconds — steps taken times the fixed step, not wall clock.</summary>
    public double SimTime { get; set; }

    /// <summary>Fixed steps taken since the world was created.</summary>
    public long StepCount { get; set; }

    /// <summary>How many bodies the engine still considers awake.</summary>
    public int AwakeCount { get; set; }

    /// <summary>Wall-clock milliseconds the last batch of steps cost, for the HUD.</summary>
    public double StepMilliseconds { get; set; }
    #endregion

    #region Methods
    /// <summary>Finds the entry for <paramref name="id"/>, or -1 if that body is gone. Selection is by id, so this
    /// is how the UI resolves it against whichever snapshot it happens to be holding.</summary>
    public int IndexOf(int id)
    {
        for (var i = 0; i < Count; i++)
        {
            if (Ids[i] == id) return i;
        }

        return -1;
    }
    #endregion

    #region Fields
    /// <summary>Stable per-body identifiers, assigned at spawn. Selection and every posted scene command key off
    /// these rather than an array index, which shifts when a body is deleted.</summary>
    public readonly int[] Ids;

    /// <summary>Body centres in world space.</summary>
    public readonly Vector3[] Positions;

    /// <summary>Body orientations.</summary>
    public readonly Quaternion[] Rotations;

    /// <summary>Per-shape extents — see the remarks on <see cref="SceneSnapshot"/> for what each component means.</summary>
    public readonly Vector3[] HalfExtents;

    /// <summary>Linear velocities, for the velocity colour mode and the inspector.</summary>
    public readonly Vector3[] Velocities;

    /// <summary>Body masses, for the inspector.</summary>
    public readonly float[] Masses;

    /// <summary>Which shape each body is drawn as.</summary>
    public readonly BodyShape[] Shapes;

    /// <summary>Whether the engine still considers each body awake.</summary>
    public readonly bool[] Awake;

    /// <summary>An index into the renderer's palette, assigned at spawn so a body keeps its colour.</summary>
    public readonly int[] ColorKeys;
    #endregion
}
