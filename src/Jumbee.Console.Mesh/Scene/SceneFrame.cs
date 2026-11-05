namespace Jumbee.Console.Mesh;

using System.Numerics;

/// <summary>The shape a body is drawn as.</summary>
public enum BodyShape : byte
{
    /// <summary>An oriented box, drawn as 12 projected edges.</summary>
    Box,

    /// <summary>A sphere, drawn as one screen-space circle.</summary>
    Sphere,

    /// <summary>A capsule, drawn as its two end spheres joined by the segment between them.</summary>
    Capsule,

    /// <summary>A triangle mesh, the one in <see cref="SceneFrame.Meshes"/>. Drawn as a box when that is
    /// <see langword="null"/>.</summary>
    Mesh,
}

/// <summary>
/// One frame of a scene, frozen: every body the renderers draw, and nothing that has to be read back off a live
/// object while its owner is changing it.
/// </summary>
/// <remarks>
/// <para>
/// <b>Built fresh, published whole, never mutated after publishing.</b> A renderer may read a frame on a background
/// thread and hold it across as many draws as it likes, so a source must hand over a new frame rather than edit the
/// one it last published. That is what lets it cross threads with no lock.
/// </para>
/// <para>
/// <b>Parallel arrays, not an array of structs</b>: the layout a vectorised rasteriser would need, and the expensive
/// part to retrofit. For a <see cref="BodyShape.Sphere"/> only <see cref="HalfExtents"/>.X is meaningful and holds the
/// radius; for a <see cref="BodyShape.Capsule"/>, X is the radius and Y the half-length of the segment; for a
/// <see cref="BodyShape.Mesh"/>, X is the radius the mesh's unit half-extent of 0.5 is scaled to.
/// </para>
/// <para>
/// Not sealed: a host can derive a frame that carries its own columns — a physics scene's velocities, say — and they
/// stay consistent with the render columns because both are published as one object.
/// </para>
/// </remarks>
public class SceneFrame
{
    #region Constructors
    /// <summary>Allocates a frame sized for <paramref name="capacity"/> bodies.</summary>
    public SceneFrame(int capacity)
    {
        Ids = new int[capacity];
        Positions = new Vector3[capacity];
        Rotations = new Quaternion[capacity];
        HalfExtents = new Vector3[capacity];
        Meshes = new TriangleMesh?[capacity];
        Shapes = new BodyShape[capacity];
        ColorKeys = new int[capacity];
    }
    #endregion

    #region Properties
    /// <summary>How many entries of each array are live.</summary>
    public int Count { get; set; }
    #endregion

    #region Methods
    /// <summary>Finds the entry for <paramref name="id"/>, or -1 if that body is gone. Selection is by id, so this
    /// is how the UI resolves it against whichever frame it happens to be holding.</summary>
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
    /// <summary>Stable per-body identifiers. Selection keys off these rather than an array index, which shifts when a
    /// body is removed.</summary>
    public readonly int[] Ids;

    /// <summary>Body centres in world space.</summary>
    public readonly Vector3[] Positions;

    /// <summary>Body orientations.</summary>
    public readonly Quaternion[] Rotations;

    /// <summary>Per-shape extents — see the remarks on <see cref="SceneFrame"/> for what each component means.</summary>
    public readonly Vector3[] HalfExtents;

    /// <summary>For a <see cref="BodyShape.Mesh"/> body, the mesh it is drawn as; ignored otherwise.</summary>
    /// <remarks>A reference, not an id into a shared list: a frame holds what it draws, so nothing a renderer reads
    /// can change under it, and a mesh nothing refers to any more can be collected.</remarks>
    public readonly TriangleMesh?[] Meshes;

    /// <summary>Which shape each body is drawn as.</summary>
    public readonly BodyShape[] Shapes;

    /// <summary>An index into the renderer's palette, so a body keeps its colour from frame to frame.</summary>
    public readonly int[] ColorKeys;

    /// <summary>
    /// Optional full affine transform per body, replacing the position/rotation/scale path when present.
    /// </summary>
    /// <remarks>
    /// For shear and non-uniform scale, which a quaternion cannot express. <see cref="ModelScene"/> sets it; a
    /// physics scene leaves it null, since no rigid body can be sheared.
    /// </remarks>
    public Matrix4x4[]? LocalTransforms;
    #endregion
}
