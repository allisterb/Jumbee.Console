namespace Jumbee.Console.SandboxDemo;

/// <summary>What the spawn and launch keys produce. UI-thread owned; read when a command is posted.</summary>
/// <remarks>Raises <see cref="Changed"/> on every mutation so the sidebar's widgets can follow the keys, and the
/// keys the widgets — see <c>SidebarPanel</c>, which is the only subscriber that matters.</remarks>
public sealed class SpawnSettings
{
    #region Events
    /// <summary>Raised on the UI thread whenever any setting changes.</summary>
    public event Action? Changed;
    #endregion

    #region Properties
    /// <summary>The shape spawned and launched.</summary>
    public BodyShape Shape
    {
        get => shape;
        set { if (shape != value) { shape = value; Changed?.Invoke(); } }
    }

    /// <summary>Size multiplier on the unit shape.</summary>
    public float Scale
    {
        get => scale;
        set { var v = Math.Clamp(value, MinScale, MaxScale); if (scale != v) { scale = v; Changed?.Invoke(); } }
    }

    /// <summary>Speed a launched body leaves the camera at, in world units per second.</summary>
    public float LaunchSpeed
    {
        get => launchSpeed;
        set { var v = Math.Clamp(value, MinSpeed, MaxSpeed); if (launchSpeed != v) { launchSpeed = v; Changed?.Invoke(); } }
    }

    /// <summary>How far above the target a spawned body appears, so it drops in rather than materialising inside
    /// whatever is already there.</summary>
    public float DropHeight => 2f;

    /// <summary>Radius of the sphere that encloses what a spawn produces — a box's corner is further from its centre
    /// than its face, so a cube of half-extent <c>h</c> needs <c>h·√3</c>. Used to work out how far in front of the
    /// camera a launched body has to start (see <see cref="SceneView.Launch"/>).</summary>
    public float BoundingRadius => Shape == BodyShape.Sphere ? 0.5f * Scale : 0.5f * Scale * 1.7320508f;

    /// <summary>Which registered mesh a <see cref="BodyShape.Mesh"/> spawn uses, or -1 when none is loaded.</summary>
    public int MeshId
    {
        get => meshId;
        set { if (meshId != value) { meshId = value; Changed?.Invoke(); } }
    }

    /// <summary>The size range the sliders and the <c>+</c>/<c>-</c> keys share.</summary>
    public const float MinScale = 0.4f;
    /// <summary>The upper end of <see cref="Scale"/>.</summary>
    public const float MaxScale = 3f;
    /// <summary>The lower end of <see cref="LaunchSpeed"/>.</summary>
    public const float MinSpeed = 2f;
    /// <summary>The upper end of <see cref="LaunchSpeed"/>.</summary>
    public const float MaxSpeed = 80f;

    /// <summary>What the next spawn will be called in the footer.</summary>
    public string ShapeName => Shape switch
    {
        BodyShape.Sphere => "sphere",
        BodyShape.Mesh when MeshId >= 0 => Meshes.NameOf(MeshId),
        _ => "box",
    };
    #endregion

    #region Methods
    /// <summary>Cycles box → sphere → mesh, skipping mesh entirely when nothing has been registered.</summary>
    public void ToggleShape() => Shape = Shape switch
    {
        BodyShape.Box => BodyShape.Sphere,
        BodyShape.Sphere when Meshes.RegisteredCount > 0 => BodyShape.Mesh,
        _ => BodyShape.Box,
    };

    /// <summary>Steps to the next registered mesh, switching the spawn shape to it.</summary>
    public void NextMesh()
    {
        if (Meshes.RegisteredCount == 0) return;
        MeshId = (MeshId + 1) % Meshes.RegisteredCount;
        Shape = BodyShape.Mesh;
    }

    /// <summary>Scales the spawn size up or down a notch, clamped.</summary>
    public void StepScale(int direction) => Scale = Scale * (direction > 0 ? 1.25f : 1 / 1.25f);

    /// <summary>Steps the launch speed, clamped.</summary>
    public void StepLaunchSpeed(int direction) => LaunchSpeed = LaunchSpeed + (direction * 2.5f);
    #endregion

    #region Fields
    private BodyShape shape = BodyShape.Box;
    private float scale = 1f;
    private float launchSpeed = 20f;
    private int meshId = -1;
    #endregion
}
