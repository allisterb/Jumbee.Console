namespace Jumbee.Console.SandboxDemo;

/// <summary>What the spawn and launch keys produce. UI-thread owned; read when a command is posted.</summary>
public sealed class SpawnSettings
{
    #region Properties
    /// <summary>The shape spawned and launched.</summary>
    public BodyShape Shape { get; set; } = BodyShape.Box;

    /// <summary>Size multiplier on the unit shape.</summary>
    public float Scale { get; private set; } = 1f;

    /// <summary>Speed a launched body leaves the camera at, in world units per second.</summary>
    public float LaunchSpeed { get; private set; } = 20f;

    /// <summary>How far above the target a spawned body appears, so it drops in rather than materialising inside
    /// whatever is already there.</summary>
    public float DropHeight => 2f;

    /// <summary>Radius of the sphere that encloses what a spawn produces — a box's corner is further from its centre
    /// than its face, so a cube of half-extent <c>h</c> needs <c>h·√3</c>. Used to work out how far in front of the
    /// camera a launched body has to start (see <see cref="SceneView.Launch"/>).</summary>
    public float BoundingRadius => Shape == BodyShape.Sphere ? 0.5f * Scale : 0.5f * Scale * 1.7320508f;
    #endregion

    #region Methods
    /// <summary>Switches between box and sphere.</summary>
    public void ToggleShape() => Shape = Shape == BodyShape.Box ? BodyShape.Sphere : BodyShape.Box;

    /// <summary>Scales the spawn size up or down a notch, clamped.</summary>
    public void StepScale(int direction) =>
        Scale = Math.Clamp(Scale * (direction > 0 ? 1.25f : 1 / 1.25f), 0.4f, 3f);

    /// <summary>Steps the launch speed, clamped.</summary>
    public void StepLaunchSpeed(int direction) =>
        LaunchSpeed = Math.Clamp(LaunchSpeed + (direction * 2.5f), 2f, 80f);
    #endregion
}
