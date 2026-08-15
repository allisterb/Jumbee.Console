namespace Jumbee.Console.SandboxDemo;

using System.Numerics;

/// <summary>Anything <see cref="SceneView"/> can read a snapshot from — a live simulation, or a static scene.</summary>
public interface ISceneSource
{
    /// <summary>The scene as it stands. Read every frame; must never be null.</summary>
    SceneSnapshot Snapshot { get; }
}

/// <summary>Which axis a model file treats as up.</summary>
/// <remarks>
/// OBJ does not record it, so it has to be told. Y-up is the common convention and what these renderers use;
/// <see cref="Z"/> is what 3ds Max, Blender's native orientation and most CAD tools export, and a Z-up model loaded
/// as Y-up stands on its nose.
/// </remarks>
public enum ModelUpAxis
{
    /// <summary>Y is up — no conversion.</summary>
    Y,

    /// <summary>Z is up: the model is rotated a quarter turn about X to bring it upright.</summary>
    Z,
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
        upAxis = AuthoredUpAxis();
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

    /// <summary>How far the model is raised so its underside rests on the ground plane — and so the height its
    /// middle sits at, which is where a camera should look.</summary>
    public float Elevation { get; private set; }

    /// <summary>Which axis the file treats as up. Setting it re-poses the model.</summary>
    public ModelUpAxis UpAxis
    {
        get => upAxis;
        set
        {
            if (upAxis == value) return;
            upAxis = value;
            Rebuild();
            // Counts as the subject changing, not a tweak: standing the model up alters both its height and how
            // much room it needs, so the camera should re-frame rather than keep a shot chosen for the old pose.
            MeshChanged?.Invoke();
        }
    }

    /// <summary>The world-space radius of the posed model's bounding box, about its own centre.</summary>
    /// <remarks>Per-axis scale is folded in, so a stretched model reports the size it actually is. Shear is not:
    /// it only ever arrives as a user tweak, and the camera deliberately does not re-frame on those.</remarks>
    public float BoundingRadius => PosedExtents.Length();

    /// <summary>Raised when the subject changes — a different mesh, or the same one stood up a different way. Not
    /// raised for a transform tweak: the camera should re-aim when the subject changes, and stay where the user put
    /// it while they are stretching one.</summary>
    public event Action? MeshChanged;
    #endregion

    #region Methods
    /// <summary>Shows the mesh at <paramref name="index"/>, resetting the transform — after models have been loaded
    /// while the viewer is running.</summary>
    public void Reload(int index)
    {
        MeshId = Meshes.RegisteredCount == 0 ? 0 : Math.Clamp(index, 0, Meshes.RegisteredCount - 1);
        upAxis = AuthoredUpAxis();
        ResetTransform();
        MeshChanged?.Invoke();
    }

    /// <summary>Shows the next (or previous) registered mesh.</summary>
    public void Step(int direction)
    {
        if (Meshes.RegisteredCount == 0) return;
        MeshId = ((MeshId + direction) % Meshes.RegisteredCount + Meshes.RegisteredCount) % Meshes.RegisteredCount;
        // Per model, not carried over: the previous model's setting — whether guessed or chosen by hand — says
        // nothing about this one, and each file brings its own answer or the Y-up default.
        upAxis = AuthoredUpAxis();
        Rebuild();
        MeshChanged?.Invoke();
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

    /// <summary>
    /// Sets one axis' scale outright, or all three when <paramref name="axis"/> is -1 — what a sidebar slider does,
    /// where the keys multiply.
    /// </summary>
    public void SetScaleAxis(int axis, float value)
    {
        var s = Scale;
        if (axis < 0) s = new Vector3(value);
        else if (axis == 0) s.X = value;
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

    // The mesh's half-extents as posed: the up-axis swap first (a quarter turn about X exchanges the Y and Z
    // extents), then the per-axis scale and the view scale.
    private Vector3 PosedExtents
    {
        get
        {
            var e = Meshes.Get(MeshId).Extents;
            if (upAxis == ModelUpAxis.Z) e = new Vector3(e.X, e.Z, e.Y);
            return e * Scale * ViewScale;
        }
    }

    private Matrix4x4 Upright =>
        upAxis == ModelUpAxis.Z ? Matrix4x4.CreateRotationX(-MathF.PI / 2) : Matrix4x4.Identity;

    // What the file suggests, or Y-up. The suggestion is only ever a starting point — it lands on a switch the
    // user can see and flip.
    private ModelUpAxis AuthoredUpAxis() =>
        Meshes.RegisteredCount == 0 ? ModelUpAxis.Y : Meshes.Get(MeshId).AuthoredUpAxis ?? ModelUpAxis.Y;

    private void SetScale(Vector3 scale)
    {
        var next = Vector3.Clamp(scale, new Vector3(MinScale), new Vector3(MaxScale));
        if (next == Scale) return;
        Scale = next;
        Rebuild();
    }

    /// <summary>
    /// How far back a camera of the given <paramref name="focal"/> length must sit for the posed model to fill
    /// <see cref="FrameFill"/> of a view whose vertical half-span is <paramref name="verticalHalfSpan"/> in NDC.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The projection is <c>ndc = focal · r / distance</c> — the same relation the sandbox uses to decide how far
    /// in front of the lens to spawn a launched body — solved the other way, and applied to each axis rather than
    /// to a bounding sphere. The sphere is what you reach for first and it frames badly: its radius is the
    /// box's <em>corner</em>, which for anything rounded is a large overestimate, and the model ends up a quarter
    /// of the frame surrounded by floor.
    /// </para>
    /// <para>
    /// The two constraints are exact for a turntable. Height never changes as the model spins, so the vertical one
    /// is just <c>e.Y</c> against the view's vertical half-span. The horizontal one has to assume the worst
    /// rotation, which is the XZ diagonal. Whichever needs more room wins.
    /// </para>
    /// <para>
    /// The <c>+ depth</c> is what stops a model that fits on paper from spilling out of the frame: the solved
    /// distance places the model's <em>centre</em>, and its near half is closer than that, so it projects larger.
    /// Pushing back by the depth the model reaches toward the camera fits the near face rather than the middle.
    /// Without it the reference bunny lost an ear off the top of the frame.
    /// </para>
    /// </remarks>
    public float FramingDistance(float focal, double verticalHalfSpan)
    {
        var e = PosedExtents;
        var depth = MathF.Sqrt((e.X * e.X) + (e.Z * e.Z));   // the worst XZ reach as the turntable spins
        var vertical = focal * e.Y / (float)(verticalHalfSpan * FrameFill);
        var horizontal = focal * depth / FrameFill;
        var distance = MathF.Max(vertical, horizontal) + depth;
        return Math.Clamp(distance, OrbitCamera.MinDistance, OrbitCamera.MaxDistance);
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

        // The up-axis conversion goes FIRST, so everything after it works in the viewer's own frame — which is what
        // makes "Scale Y" mean "taller on screen" for a Z-up model as much as for a Y-up one. Rotating -90° about X
        // maps (x, y, z) to (x, z, -y): the file's up axis becomes the viewer's.
        var transform = Upright
            * shear
            * Matrix4x4.CreateScale(Scale)
            * Matrix4x4.CreateRotationY(spin);

        // Stand the model ON the floor rather than through it. A loaded mesh is centred on its bounding box — right
        // for a turntable, which should spin about the model's own middle — but the renderers draw their ground at
        // y = 0, so a centred model is buried to the waist. Lifting by its underside is the whole fix, and it is a
        // constant per pose: only the vertical scale moves it, because the spin is about Y and the shear displaces
        // X and Z by height without touching Y.
        var low = Meshes.Get(MeshId).Min;
        Elevation = -(upAxis == ModelUpAxis.Z ? low.Z : low.Y) * Scale.Y * ViewScale;

        snapshot.Count = 1;
        snapshot.Ids[0] = 1;
        snapshot.Shapes[0] = BodyShape.Mesh;
        snapshot.MeshIds[0] = MeshId;
        snapshot.Positions[0] = new Vector3(0, Elevation, 0);
        snapshot.Rotations[0] = Quaternion.Identity;
        snapshot.HalfExtents[0] = new Vector3(ViewRadius);
        snapshot.ColorKeys[0] = 1;
        snapshot.Awake[0] = true;
        snapshot.AwakeCount = 1;
        (snapshot.LocalTransforms ??= new Matrix4x4[1])[0] = transform * Matrix4x4.CreateScale(ViewScale);
    }
    #endregion

    #region Fields
    /// <summary>World-space radius the model is scaled to. Sized against the camera's default distance so the model
    /// fills most of the viewport rather than sitting in it.</summary>
    public const float ViewRadius = 5.5f;

    /// <summary>The uniform scale from the loader's unit space (half-extent 0.5) up to <see cref="ViewRadius"/>.</summary>
    public const float ViewScale = ViewRadius / 0.5f;

    /// <summary>The share of each half-span the model's bounding box is framed to fill. Short of 1 so the shot has
    /// a margin, and because the camera looks down at the model rather than square-on.</summary>
    private const float FrameFill = 0.9f;

    /// <summary>The per-axis scale range the keys and the sidebar sliders share.</summary>
    public const float MinScale = 0.25f;
    /// <summary>The upper end of <see cref="Scale"/>.</summary>
    public const float MaxScale = 4f;
    /// <summary>How far <see cref="Shear"/> may go in either direction on either axis.</summary>
    public const float MaxShear = 1.5f;

    private readonly SceneSnapshot snapshot;
    private float spin;
    private ModelUpAxis upAxis = ModelUpAxis.Y;
    #endregion
}
