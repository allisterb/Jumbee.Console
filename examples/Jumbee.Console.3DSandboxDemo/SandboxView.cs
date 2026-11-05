namespace Jumbee.Console.SandboxDemo;

using System.Diagnostics;
using System.Numerics;

using ConsoleGUI.Space;

using Jumbee.Console.Mesh;

/// <summary>
/// The sandbox's viewport: a <see cref="MeshView"/> over the physics thread, plus everything you can do to a pile of
/// bodies — spawn, launch, grab and throw, delete, clear.
/// </summary>
/// <remarks>
/// Everything here runs on the UI thread and reads only the published frame, so no interaction ever touches a live
/// Box3D handle. Changes go the other way, as commands posted to <see cref="PhysicsRunner.Post"/>. The camera, the
/// renderers and the render keys are all <see cref="MeshView"/>'s; this adds keys through
/// <see cref="MeshView.OnSceneKey"/> and takes body drags through <see cref="MeshView.OnBodyPress"/>.
/// </remarks>
public sealed class SandboxView : MeshView
{
    #region Constructors
    /// <summary>Creates the viewport over <paramref name="runner"/>, drawn with <paramref name="renderer"/> and
    /// spawning meshes from <paramref name="library"/>.</summary>
    public SandboxView(PhysicsRunner runner, ISceneRenderer renderer, MeshLibrary library) : base(runner, renderer)
    {
        this.runner = runner;
        Spawn = new SpawnSettings(library);
        AllowSelection = true;
    }
    #endregion

    #region Properties
    /// <summary>The physics thread this view draws and drives.</summary>
    public PhysicsRunner Runner => runner;

    /// <summary>What the spawn and launch keys produce.</summary>
    public SpawnSettings Spawn { get; }
    #endregion

    #region Methods
    /// <summary>Drops a body in above the camera target, so it lands in view wherever the camera is pointing.</summary>
    public void SpawnAtTarget()
    {
        var position = Camera.Target + new Vector3(0, Spawn.DropHeight, 0);
        var (shape, scale, key, mesh) = (Spawn.Shape, Spawn.Scale, nextColorKey++, Spawn.Mesh);
        runner.Post(scene => scene.Add(shape, position, scale, key, default, mesh));
        selectNewestSpawn = true;
    }

    /// <summary>Fires a body out of the camera along the view direction — the sandbox's blunt instrument.</summary>
    /// <remarks>
    /// The muzzle distance is <b>derived from how big the body is</b>, not a constant. Spawning it a fixed 1 unit in
    /// front of the eye (which is what inertia does) puts it on the lens: a default sphere projects to 141% of the
    /// viewport height on its first frame, so what you see is a screen-filling shape collapsing to a dot rather than
    /// an object being thrown. Solving <c>ndcRadius = focal · r / distance</c> for a target on-screen size instead
    /// keeps the first frame legible, and keeps it legible after <c>+</c>/<c>-</c> changes the spawn size.
    /// </remarks>
    public void Launch()
    {
        var view = Camera.GetView();
        var muzzle = MathF.Max(MinMuzzleDistance, Projection.Focal * Spawn.BoundingRadius / MuzzleNdcRadius);
        var origin = view.Eye + (view.Forward * muzzle);
        var velocity = view.Forward * Spawn.LaunchSpeed;
        var (shape, scale, key, mesh) = (Spawn.Shape, Spawn.Scale, nextColorKey++, Spawn.Mesh);
        runner.Post(scene => scene.Add(shape, origin, scale, key, velocity, mesh));
        selectNewestSpawn = true;
    }

    /// <summary>Deletes the selected body, if there is one.</summary>
    public void DeleteSelected()
    {
        if (Selected is not { } id) return;
        Selected = null;
        runner.Post(scene => scene.Remove(id));
    }

    /// <summary>Removes every dynamic body.</summary>
    public void ClearScene()
    {
        Selected = null;
        runner.Post(scene => scene.ClearBodies());
    }
    #endregion

    #region Protected methods
    /// <inheritdoc/>
    protected override bool OnSceneKey(ConsoleKeyInfo key)
    {
        if (key.Key == ConsoleKey.Delete)
        {
            DeleteSelected();
            return true;
        }

        switch (key.KeyChar)
        {
            case 'b': Spawn.ToggleShape(); return true;
            case 'n': SpawnAtTarget(); return true;
            case 'f': Launch(); return true;
            case 'x': DeleteSelected(); return true;
            case 'c': ClearScene(); return true;
            case 'm': Spawn.NextMesh(); return true;
            case '+' or '=': Spawn.StepScale(+1); return true;
            case '-' or '_': Spawn.StepScale(-1); return true;
            case ']': Spawn.StepLaunchSpeed(+1); return true;
            case '[': Spawn.StepLaunchSpeed(-1); return true;
            default: return false;
        }
    }

    // A press on a body grabs it: drag in the plane through the body facing the camera, keeping the grab offset so
    // the body does not snap its centre to the pointer the moment you touch it.
    /// <inheritdoc/>
    protected override bool OnBodyPress(int id, Position position)
    {
        var frame = Drawn ?? Source.Frame;
        var index = frame.IndexOf(id);
        // Still take the press: a body that vanished between the pick and here should neither orbit nor grab.
        if (index < 0) return true;

        var view = Camera.GetView();
        grabPlanePoint = frame.Positions[index];
        grabPlaneNormal = view.Forward;
        grabOffset = Viewport.TryRay(position.X, position.Y, view, Projection, out var o, out var d)
            && Projection.TryPlaneHit(o, d, grabPlanePoint, grabPlaneNormal, out var contact)
                ? grabPlanePoint - contact
                : Vector3.Zero;

        grabbed = id;
        throwSamples.Clear();
        RecordThrowSample(grabPlanePoint);
        runner.Post(scene => scene.BeginGrab(id));
        return true;
    }

    /// <inheritdoc/>
    protected override void OnBodyDrag(Position position)
    {
        if (grabbed is not { } id) return;

        var view = Camera.GetView();
        if (!Viewport.TryRay(position.X, position.Y, view, Projection, out var o, out var d)) return;
        if (!Projection.TryPlaneHit(o, d, grabPlanePoint, grabPlaneNormal, out var contact)) return;

        var target = contact + grabOffset;
        RecordThrowSample(target);
        runner.Post(scene => scene.DragTo(id, target));
    }

    /// <inheritdoc/>
    protected override void OnBodyRelease(Position position)
    {
        if (grabbed is null) return;
        grabbed = null;
        var velocity = ThrowVelocity();
        runner.Post(scene => scene.ReleaseGrab(velocity));
    }

    /// <inheritdoc/>
    protected override void OnSceneTick(SceneFrame frame)
    {
        // Select whatever the last spawn produced, once the physics thread has actually created it. Ids only come
        // back through the frame -- the command that made it ran over there -- so this waits for a tick carrying a
        // higher id than we had. Without it, pressing f gives no feedback beyond a shape appearing.
        if (selectNewestSpawn && frame.Count > 0 && frame.Ids[^1] > highestSeenId)
        {
            Selected = frame.Ids[^1];
            selectNewestSpawn = false;
        }

        if (frame.Count > 0) highestSeenId = Math.Max(highestSeenId, frame.Ids[^1]);
    }

    /// <inheritdoc/>
    protected override HelpInfo? GetHelpInfo() => new HelpInfo("3D viewport")
        .WithKey("Arrows", "Orbit the camera (hold Shift for fine steps; the sidebar's Camera pad does it by mouse)")
        .WithKey("PgUp/PgDn", "Zoom in / out")
        .WithKey("Home", "Reset the camera")
        .WithKey("Drag", "On a body: grab and throw it. On empty space: orbit. Wheel zooms")
        .WithKey("Click", "Select the body under the pointer")
        .WithKey("Tab / Esc", "Select the next body / clear the selection")
        .WithKey("b", "Cycle the spawn shape: box, sphere, mesh")
        .WithKey("m", "Switch to the next loaded mesh")
        .WithKey("n", "Drop one in above the camera target")
        .WithKey("f", "Fire one out of the camera")
        .WithKey("Del / x", "Delete the selected body")
        .WithKey("c", "Clear every body")
        .WithKey("v", "Cycle renderer: wireframe (braille edges), solid (flat-shaded), shaded (point light + edges + AO)")
        .WithKey("e", "Shaded only: cycle silhouettes — off, ink outline, edge glyphs")
        .WithKey("w", "Shaded only: half-Lambert lighting — rescues the unlit side, costs contrast on the lit one")
        .WithKey("+ / -", "Grow / shrink what gets spawned")
        .WithKey("] / [", "Raise / lower the launch speed");
    #endregion

    #region Private methods
    // Throw velocity is measured from where the grab point actually went, not from the pointer delta.
    private void RecordThrowSample(Vector3 point)
    {
        throwSamples.Enqueue((clock.Elapsed, point));
        while (throwSamples.Count > ThrowSampleCount) throwSamples.Dequeue();
    }

    /// <summary>
    /// The velocity a release hands back to the solver: how far the grab point travelled over a fixed window of real
    /// time ending at the release.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A window of <em>time</em>, not of pointer events, is the whole point. A terminal reports the pointer in whole
    /// cells and in bursts, so two adjacent events can be several world units and a couple of milliseconds apart;
    /// dividing one by the other implies a speed nothing on screen ever had, which is what made a short flick fire
    /// the body off the edge of the world.
    /// </para>
    /// <para>
    /// Three guards, one per gesture that used to fling. A release <em>after</em> the pointer stopped drops the body,
    /// because samples arrive only on movement and the last delta would otherwise still be sitting there waiting to
    /// be believed. A gesture shorter than <see cref="MinThrowSeconds"/> is divided by that floor anyway, so it is
    /// bounded by how far it went rather than extrapolated from how briefly it took. And the result is capped, both
    /// because the solver deals badly with a body that crosses the world in one step and because a hand toss is not
    /// the cannon that <c>f</c> fires.
    /// </para>
    /// </remarks>
    private Vector3 ThrowVelocity()
    {
        if (throwSamples.Count < 2) return Vector3.Zero;

        var samples = throwSamples.ToArray();
        var (t1, p1) = samples[^1];
        if (clock.Elapsed - t1 > ThrowStaleAfter) return Vector3.Zero;

        // Walk back to the oldest sample still inside the window, but always take at least one step: if the pointer
        // was reporting more slowly than the window, the honest reading is that slow speed, not no throw at all.
        var first = samples.Length - 2;
        while (first > 0 && t1 - samples[first - 1].At <= ThrowWindow) first--;
        var (t0, p0) = samples[first];

        var velocity = (p1 - p0) / MathF.Max((float)(t1 - t0).TotalSeconds, MinThrowSeconds);
        var speed = velocity.Length();
        return speed > MaxThrowSpeed ? velocity * (MaxThrowSpeed / speed) : velocity;
    }
    #endregion

    #region Fields
    // The ceiling on a thrown body, in world units per second. A hand toss, deliberately well under the cannon that
    // f fires (SpawnSettings.MaxSpeed is 80): at 15 a body still crosses the visible scene in about a second.
    private const float MaxThrowSpeed = 15f;
    private const float MinThrowSeconds = 0.04f;
    private const int ThrowSampleCount = 16;

    // Long enough to average out the terminal's burstiness, short enough to measure the flick rather than the drag
    // that preceded it. Stale is the pause before a release that should drop the body instead of throwing it.
    private static readonly TimeSpan ThrowWindow = TimeSpan.FromMilliseconds(120);
    private static readonly TimeSpan ThrowStaleAfter = TimeSpan.FromMilliseconds(160);

    // The on-screen radius a launched body should have on its first frame, in NDC. The viewport's Y half-span is the
    // cell aspect (~0.6 on a wide terminal), so 0.2 is about a third of the half-height -- clearly an object, not a
    // wall. See the remarks on Launch for why this is derived rather than a fixed muzzle distance.
    private const float MuzzleNdcRadius = 0.2f;
    private const float MinMuzzleDistance = 2f;

    private readonly PhysicsRunner runner;
    private readonly Stopwatch clock = Stopwatch.StartNew();
    private readonly Queue<(TimeSpan At, Vector3 Point)> throwSamples = new();

    private int nextColorKey = 100;
    private bool selectNewestSpawn;
    private int highestSeenId;
    private int? grabbed;
    private Vector3 grabPlanePoint;
    private Vector3 grabPlaneNormal;
    private Vector3 grabOffset;
    #endregion
}
