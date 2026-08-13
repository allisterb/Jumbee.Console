namespace Jumbee.Console.SandboxDemo;

/// <summary>
/// The world settings the sidebar tunes: gravity, surface friction and bounce, air drag, and the simulation clock.
/// UI-thread owned; <see cref="Changed"/> tells the panel and the runner that something moved.
/// </summary>
/// <remarks>
/// Every one of these maps onto a real Box3D setting rather than being a readout — gravity is the world's vector,
/// friction and bounce are per-shape material values, drag is per-body damping — so the runner applies a change to
/// every live body and stores it as the default for whatever is spawned next. See
/// <see cref="PhysicsScene.ApplyParameters"/>, which runs on the physics thread.
/// </remarks>
public sealed class SandboxParameters
{
    #region Events
    /// <summary>Raised on the UI thread whenever any value changes.</summary>
    public event Action? Changed;
    #endregion

    #region Properties
    /// <summary>Downward acceleration in m/s². 0 is free-floating; the default is Earth's.</summary>
    public float Gravity { get => gravity; set => Set(ref gravity, Math.Clamp(value, 0f, 30f)); }

    /// <summary>Surface friction, 0 (ice) to 1 (grippy).</summary>
    public float Friction { get => friction; set => Set(ref friction, Math.Clamp(value, 0f, 1f)); }

    /// <summary>Restitution, 0 (dead) to 1 (a superball).</summary>
    public float Bounce { get => bounce; set => Set(ref bounce, Math.Clamp(value, 0f, 1f)); }

    /// <summary>Linear damping — how quickly a body loses speed to the air.</summary>
    public float Drag { get => drag; set => Set(ref drag, Math.Clamp(value, 0f, 4f)); }

    /// <summary>Simulation rate multiplier. Clamped to the range <see cref="PhysicsRunner.TimeScale"/> accepts.</summary>
    public float TimeScale { get => timeScale; set => Set(ref timeScale, Math.Clamp(value, 0.05f, 4f)); }
    #endregion

    #region Methods
    /// <summary>Restores every value to its default, raising <see cref="Changed"/> once.</summary>
    public void Reset()
    {
        (gravity, friction, bounce, drag, timeScale) = (9.8f, 0.6f, 0.3f, 0f, 1f);
        Changed?.Invoke();
    }
    #endregion

    #region Private methods
    // One notification per real change: a slider dragged across a cell it has already reported would otherwise
    // repost the whole parameter set to the physics thread on every mouse move.
    private void Set(ref float field, float value)
    {
        if (field.Equals(value)) return;
        field = value;
        Changed?.Invoke();
    }
    #endregion

    #region Fields
    private float gravity = 9.8f;
    private float friction = 0.6f;
    private float bounce = 0.3f;
    private float drag;
    private float timeScale = 1f;
    #endregion
}
