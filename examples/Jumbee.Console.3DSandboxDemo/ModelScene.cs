namespace Jumbee.Console.SandboxDemo;

using System.Numerics;

/// <summary>Anything <see cref="SceneView"/> can read a snapshot from — a live simulation, or a static scene.</summary>
public interface ISceneSource
{
    /// <summary>The scene as it stands. Read every frame; must never be null.</summary>
    SceneSnapshot Snapshot { get; }
}

/// <summary>
/// A single model on a turntable: no physics, no gravity, one body at the origin, sized to fill the view. The
/// scene behind the <c>obj</c> verb.
/// </summary>
/// <remarks>
/// <para>
/// A model at sandbox scale is a few dozen cells across and unreadable — a teapot and a rock look the same. Giving
/// the loader its own scene means the asset can be the whole viewport, which is the only size at which the OBJ
/// pipeline and the renderers can actually be judged.
/// </para>
/// <para>
/// This is also where <b>shear and non-uniform scale</b> live. They are free for rendering here because the
/// rasteriser derives each face normal from the world-space winding of the already-transformed triangle, so any
/// affine map comes out correctly lit with no inverse-transpose. They are deliberately absent from the sandbox:
/// Box3D has no sheared collision shape, so a sheared rigid body would render one way and collide another.
/// </para>
/// </remarks>
public sealed class ModelScene : ISceneSource
{
    #region Constructors
    /// <summary>Creates the scene over the registered meshes, opening on <paramref name="startIndex"/>.</summary>
    public ModelScene(int startIndex = 0)
    {
        MeshId = Meshes.RegisteredCount == 0 ? 0 : Math.Clamp(startIndex, 0, Meshes.RegisteredCount - 1);
        snapshot = new SceneSnapshot(1) { Count = 1 };
        Rebuild();
    }
    #endregion

    #region Properties
    /// <inheritdoc/>
    public SceneSnapshot Snapshot => snapshot;

    /// <summary>Which registered mesh is on show.</summary>
    public int MeshId { get; private set; }

    /// <summary>The mesh currently shown.</summary>
    public Mesh Mesh => Meshes.Get(MeshId);

    /// <summary>Display name of the mesh currently shown.</summary>
    public string Name => Meshes.RegisteredCount > 0 ? Meshes.NameOf(MeshId) : "none";

    /// <summary>Per-axis scale applied before the shear.</summary>
    public Vector3 Scale { get; private set; } = Vector3.One;

    /// <summary>Shear of X by Y, and of Z by Y — the two that read most clearly on an upright model.</summary>
    public Vector2 Shear { get; private set; }

    /// <summary>Continuous turntable spin, in radians per second; 0 holds still.</summary>
    public float SpinRate { get; set; } = 0.35f;
    #endregion

    #region Methods
    /// <summary>Shows the mesh at <paramref name="index"/>, resetting the transform — after models have been loaded
    /// while the viewer is running.</summary>
    public void Reload(int index)
    {
        MeshId = Meshes.RegisteredCount == 0 ? 0 : Math.Clamp(index, 0, Meshes.RegisteredCount - 1);
        ResetTransform();
    }

    /// <summary>Shows the next (or previous) registered mesh.</summary>
    public void Step(int direction)
    {
        if (Meshes.RegisteredCount == 0) return;
        MeshId = ((MeshId + direction) % Meshes.RegisteredCount + Meshes.RegisteredCount) % Meshes.RegisteredCount;
        Rebuild();
    }

    /// <summary>Scales one axis, or all three when <paramref name="axis"/> is -1. Clamped to a legible range.</summary>
    public void ScaleAxis(int axis, float factor)
    {
        var s = Scale;
        if (axis < 0) s *= factor;
        else if (axis == 0) s.X *= factor;
        else if (axis == 1) s.Y *= factor;
        else s.Z *= factor;

        SetScale(s);
    }

    /// <summary>Sets one axis' scale outright — what a sidebar slider does, where the keys multiply.</summary>
    public void SetScaleAxis(int axis, float value)
    {
        var s = Scale;
        if (axis == 0) s.X = value;
        else if (axis == 1) s.Y = value;
        else s.Z = value;

        SetScale(s);
    }

    /// <summary>Adjusts the shear of X (and Z) by height.</summary>
    public void Nudge(float dx, float dz) => SetShear(Shear.X + dx, Shear.Y + dz);

    /// <summary>Sets the shear outright — what a sidebar slider does, where the keys nudge.</summary>
    public void SetShear(float x, float z)
    {
        var next = new Vector2(Math.Clamp(x, -MaxShear, MaxShear), Math.Clamp(z, -MaxShear, MaxShear));
        if (next == Shear) return;
        Shear = next;
        Rebuild();
    }

    /// <summary>Returns the model to an unsheared, unscaled, upright pose.</summary>
    public void ResetTransform()
    {
        Scale = Vector3.One;
        Shear = Vector2.Zero;
        spin = 0f;
        Rebuild();
    }

    private void SetScale(Vector3 scale)
    {
        var next = Vector3.Clamp(scale, new Vector3(MinScale), new Vector3(MaxScale));
        if (next == Scale) return;
        Scale = next;
        Rebuild();
    }

    /// <summary>Advances the turntable by <paramref name="seconds"/> and republishes.</summary>
    public void Advance(double seconds)
    {
        if (SpinRate == 0f) return;
        spin = (float)((spin + (SpinRate * seconds)) % MathF.Tau);
        Rebuild();
    }
    #endregion

    #region Private methods
    // One body, rebuilt whenever anything changes. The full affine map goes into SceneSnapshot.LocalTransforms,
    // which the rasteriser uses in place of the scale-then-rotate path -- a quaternion cannot express a shear.
    private void Rebuild()
    {
        if (Meshes.RegisteredCount == 0)
        {
            snapshot.Count = 0;
            return;
        }

        // Shear first, then scale, then the turntable rotation. Shearing before the spin keeps the distortion fixed
        // in the model's own frame, so it does not appear to slosh around as the model turns.
        var shear = Matrix4x4.Identity;
        shear.M21 = Shear.X;    // x += shear.X * y
        shear.M23 = Shear.Y;    // z += shear.Y * y

        var transform = shear
            * Matrix4x4.CreateScale(Scale)
            * Matrix4x4.CreateRotationY(spin);

        snapshot.Count = 1;
        snapshot.Ids[0] = 1;
        snapshot.Shapes[0] = BodyShape.Mesh;
        snapshot.MeshIds[0] = MeshId;
        snapshot.Positions[0] = Vector3.Zero;
        snapshot.Rotations[0] = Quaternion.Identity;
        snapshot.HalfExtents[0] = new Vector3(ViewRadius);
        snapshot.ColorKeys[0] = 1;
        snapshot.Awake[0] = true;
        snapshot.AwakeCount = 1;
        (snapshot.LocalTransforms ??= new Matrix4x4[1])[0] =
            transform * Matrix4x4.CreateScale(ViewRadius / 0.5f);
    }
    #endregion

    #region Fields
    /// <summary>World-space radius the model is scaled to. Sized against the camera's default distance so the model
    /// fills most of the viewport rather than sitting in it.</summary>
    public const float ViewRadius = 5.5f;

    /// <summary>The per-axis scale range the keys and the sidebar sliders share.</summary>
    public const float MinScale = 0.25f;
    /// <summary>The upper end of <see cref="Scale"/>.</summary>
    public const float MaxScale = 4f;
    /// <summary>How far <see cref="Shear"/> may go in either direction on either axis.</summary>
    public const float MaxShear = 1.5f;

    private readonly SceneSnapshot snapshot;
    private float spin;
    #endregion
}
