namespace Jumbee.Console.SandboxDemo;

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;

using Box3D;

/// <summary>
/// Owns the physics world and steps it on its own thread at a fixed 1/60 s, publishing one
/// <see cref="SceneSnapshot"/> per tick for the UI thread to render.
/// </summary>
/// <remarks>
/// <para>
/// The snapshot-per-tick pattern from <c>docs/controls/Live Data.md</c>: the world, the body list and every
/// <c>Body</c> handle belong to this thread and are never touched from outside it. The UI thread reads
/// <see cref="Snapshot"/> — a reference swap, so it always gets a whole consistent tick — and posts mutations
/// through <see cref="Post"/>, which are drained at the top of a step rather than applied mid-solve.
/// </para>
/// <para>
/// Timing is inertia's: accumulate real elapsed time (scaled), cap the backlog at
/// <see cref="MaxCatchUpSteps"/> steps, then step while there is a step's worth banked — abandoning the backlog if
/// the batch overruns <see cref="StepBudgetMs"/>. The effect is that a scene too heavy to simulate in real time
/// eases into slow motion instead of stalling the app, because the physics thread never spends an unbounded amount
/// of time trying to catch up.
/// </para>
/// </remarks>
public sealed class PhysicsRunner : ISceneSource, IDisposable
{
    #region Constructors
    /// <summary>Creates the runner and starts its thread. <paramref name="build"/> populates the initial scene and
    /// runs on the physics thread before the first step.</summary>
    public PhysicsRunner(Action<PhysicsScene> build)
    {
        this.build = build;
        snapshot = new SceneSnapshot(0);
        thread = new Thread(Run) { IsBackground = true, Name = "physics" };
        thread.Start();
    }
    #endregion

    #region Properties
    /// <summary>The most recently published tick. Never null, never torn — read it as often as you like.</summary>
    public SceneSnapshot Snapshot => Volatile.Read(ref snapshot);

    /// <summary>When set, time stops accumulating and only <see cref="StepOnce"/> advances the world.</summary>
    public bool Paused
    {
        get => Volatile.Read(ref paused);
        set => Volatile.Write(ref paused, value);
    }

    /// <summary>Multiplies elapsed real time before it is banked, for slow motion or fast-forward.</summary>
    public double TimeScale
    {
        get => Volatile.Read(ref timeScale);
        set => Volatile.Write(ref timeScale, Math.Clamp(value, 0.05, 4.0));
    }
    #endregion

    #region Methods
    /// <summary>Queues <paramref name="action"/> to run on the physics thread before the next step, with the world
    /// in a consistent state. This is the only safe way to touch the scene from outside.</summary>
    public void Post(Action<PhysicsScene> action) => commands.Enqueue(action);

    /// <summary>Advances exactly one fixed step while paused.</summary>
    public void StepOnce() => Interlocked.Exchange(ref stepOnce, 1);

    /// <summary>Stops the thread and disposes the world.</summary>
    public void Dispose()
    {
        running = false;
        thread.Join(500);
    }
    #endregion

    #region Private methods
    private void Run()
    {
        using var scene = new PhysicsScene();
        build(scene);
        Publish(scene, 0);

        var clock = Stopwatch.StartNew();
        var last = clock.Elapsed;
        var accumulator = TimeSpan.Zero;
        var stepBatch = new Stopwatch();

        while (running)
        {
            var now = clock.Elapsed;
            var elapsed = now - last;
            last = now;

            while (commands.TryDequeue(out var command)) command(scene);

            var steps = 0;
            stepBatch.Restart();
            if (Paused)
            {
                accumulator = TimeSpan.Zero;
                if (Interlocked.Exchange(ref stepOnce, 0) == 1)
                {
                    scene.Step(FixedStep);
                    steps = 1;
                }
            }
            else
            {
                accumulator += elapsed * TimeScale;
                // Cap the backlog BEFORE stepping: without this, one long stall (a debugger break, a laptop
                // resuming from sleep) banks minutes of time and the sim spends them all at once.
                if (accumulator > MaxBacklog) accumulator = MaxBacklog;
                while (accumulator >= FixedStepSpan)
                {
                    scene.Step(FixedStep);
                    accumulator -= FixedStepSpan;
                    steps++;
                    // Overran the budget: drop what is left rather than fall further behind. This is the slow-motion
                    // easing — the sim runs at whatever rate it can sustain instead of blocking on catch-up.
                    if (stepBatch.Elapsed.TotalMilliseconds >= StepBudgetMs)
                    {
                        accumulator = TimeSpan.Zero;
                        break;
                    }
                }
            }
            stepBatch.Stop();

            if (steps > 0) Publish(scene, stepBatch.Elapsed.TotalMilliseconds);

            // Sleep out the rest of the tick. A step's worth minus what it cost, floored at 1 ms so a scene that
            // cannot keep up still yields rather than spinning a core.
            var slack = FixedStepSpan - (clock.Elapsed - now);
            Thread.Sleep(slack > TimeSpan.Zero ? slack : TimeSpan.FromMilliseconds(1));
        }
    }

    // A fresh snapshot per tick. ~200 bodies is ~12 KB, so ~0.7 MB/s of gen-0 at 60 Hz — churn the collector barely
    // notices, and the alternative (recycling buffers) would hand the renderer an array being overwritten under it.
    private void Publish(PhysicsScene scene, double stepMs)
    {
        var bodies = scene.Bodies;
        var next = new SceneSnapshot(bodies.Count) { Count = bodies.Count };
        var awake = 0;
        for (var i = 0; i < bodies.Count; i++)
        {
            var b = bodies[i];
            next.Ids[i] = b.Id;
            next.Positions[i] = b.Handle.Position;
            next.Rotations[i] = b.Handle.Rotation;
            next.Velocities[i] = b.Handle.LinearVelocity;
            next.Masses[i] = b.Handle.Mass;
            next.HalfExtents[i] = b.HalfExtents;
            next.Shapes[i] = b.Shape;
            next.MeshIds[i] = b.MeshId;
            next.ColorKeys[i] = b.ColorKey;
            next.Awake[i] = b.Handle.IsAwake;
            if (next.Awake[i]) awake++;
        }

        next.AwakeCount = awake;
        next.StepCount = scene.StepCount;
        next.SimTime = scene.StepCount * FixedStep;
        next.StepMilliseconds = stepMs;
        Volatile.Write(ref snapshot, next);
    }
    #endregion

    #region Fields
    /// <summary>The fixed simulation step, in seconds. Physics is decoupled from the frame rate.</summary>
    public const float FixedStep = 1f / 60f;

    private const int MaxCatchUpSteps = 5;
    private const double StepBudgetMs = 20;

    private static readonly TimeSpan FixedStepSpan = TimeSpan.FromSeconds(FixedStep);
    private static readonly TimeSpan MaxBacklog = FixedStepSpan * MaxCatchUpSteps;

    private readonly Action<PhysicsScene> build;
    private readonly Thread thread;
    private readonly ConcurrentQueue<Action<PhysicsScene>> commands = new();

    private SceneSnapshot snapshot;
    private volatile bool running = true;
    private bool paused;
    private double timeScale = 1.0;
    private int stepOnce;
    #endregion
}

/// <summary>
/// The physics world plus the bookkeeping Box3D does not keep for us. Lives on the physics thread; reach it only
/// through <see cref="PhysicsRunner.Post"/>.
/// </summary>
public sealed class PhysicsScene : IDisposable
{
    #region Constructors
    /// <summary>Creates an empty world.</summary>
    public PhysicsScene() => world = new PhysicsWorld();
    #endregion

    #region Properties
    /// <summary>The dynamic bodies, in spawn order. Static geometry is not listed — it is drawn as the floor grid.</summary>
    public IReadOnlyList<SandboxBody> Bodies => bodies;

    /// <summary>Fixed steps taken since construction.</summary>
    public long StepCount { get; private set; }
    #endregion

    #region Methods
    /// <summary>Adds an immovable box — the floor, a wall, a ramp.</summary>
    public void AddStaticBox(Vector3 center, Vector3 size)
    {
        var body = world.CreateStaticBody(center);
        body.AddBox(Box.FromSize(size));
    }

    /// <summary>Spawns a dynamic box and remembers the half extents Box3D will not give back.</summary>
    public int AddBox(Vector3 position, Vector3 halfExtents, int colorKey, Vector3 velocity = default)
    {
        var handle = world.CreateDynamicBody(position);
        handle.AddBox(Box.FromSize(halfExtents * 2));
        return Track(handle, BodyShape.Box, halfExtents, colorKey, velocity);
    }

    /// <summary>Spawns a dynamic sphere.</summary>
    public int AddSphere(Vector3 position, float radius, int colorKey, Vector3 velocity = default)
    {
        var handle = world.CreateDynamicBody(position);
        handle.AddSphere(new Sphere(radius));
        return Track(handle, BodyShape.Sphere, new Vector3(radius, radius, radius), colorKey, velocity);
    }

    /// <summary>
    /// Spawns a dynamic body shaped like <paramref name="meshId"/>'s mesh, colliding as that mesh's convex hull.
    /// </summary>
    /// <remarks>
    /// A hull, not the triangles, and not by choice: <c>Body.AddMesh</c> requires a <b>static</b> body, so a
    /// triangle mesh cannot be a dynamic rigid body at all. Rendering the real mesh while colliding with its hull is
    /// the standard arrangement — the cost is that concavities are solid to the solver, so a teapot's handle will
    /// not catch on anything and nothing drops through a torus's hole. Approximating a concave shape properly means
    /// a compound of several hulls (<c>CompoundBuilder.AddHull</c>), which is a bigger piece of work.
    /// </remarks>
    public int AddMeshBody(int meshId, Vector3 position, float scale, int colorKey, Vector3 velocity = default)
    {
        var mesh = Meshes.Get(meshId);
        var points = new Vector3[mesh.Vertices.Length];
        for (var i = 0; i < points.Length; i++) points[i] = mesh.Vertices[i] * scale;

        var handle = world.CreateDynamicBody(position);
        // The vertex budget matters: a 3,600-vertex teapot would otherwise build a hull with far more faces than
        // the solver wants to test every step, for no behavioural gain at this scale.
        using (var hull = ConvexHull.FromPoints(points, HullVertexBudget))
        {
            handle.AddHull(hull);
        }

        var radius = 0.5f * scale;
        return Track(handle, BodyShape.Mesh, new Vector3(radius), colorKey, velocity, meshId);
    }

    /// <summary>Spawns whichever shape <paramref name="shape"/> names, sized by <paramref name="scale"/>.</summary>
    public int Add(BodyShape shape, Vector3 position, float scale, int colorKey, Vector3 velocity = default,
                   int meshId = -1) => shape switch
    {
        BodyShape.Sphere => AddSphere(position, 0.5f * scale, colorKey, velocity),
        BodyShape.Mesh when meshId >= 0 => AddMeshBody(meshId, position, scale, colorKey, velocity),
        _ => AddBox(position, new Vector3(0.5f * scale), colorKey, velocity),
    };

    /// <summary>Destroys one body. Silently does nothing if it has already gone.</summary>
    public void Remove(int id)
    {
        var i = IndexOf(id);
        if (i < 0) return;
        if (grabbed == id) grabbed = null;
        bodies[i].Handle.Destroy();
        bodies.RemoveAt(i);
    }

    /// <summary>Removes every dynamic body, leaving the static geometry.</summary>
    public void ClearBodies()
    {
        grabbed = null;
        foreach (var b in bodies) b.Handle.Destroy();
        bodies.Clear();
    }

    /// <summary>Takes a body out of the solver's hands so the pointer can drive it: kinematic bodies are moved by
    /// the application and push dynamic ones out of the way, rather than being pushed themselves.</summary>
    public void BeginGrab(int id)
    {
        var i = IndexOf(id);
        if (i < 0) return;
        ReleaseGrab(Vector3.Zero);
        grabbed = id;
        // Body is a handle, not the body: it is a small value the world resolves, so copying it out of the record
        // is free and the setter still reaches the one body in the native world.
        var handle = bodies[i].Handle;
        grabTarget = handle.Position;
        handle.Type = BodyType.Kinematic;
    }

    /// <summary>Sets where the held body should be. Re-applied on every step (see <see cref="Step"/>) rather than
    /// once per pointer move, so the body tracks the pointer smoothly between mouse events instead of lurching to
    /// the last target and overshooting it.</summary>
    public void DragTo(int id, Vector3 target)
    {
        if (grabbed != id) return;
        grabTarget = target;
    }

    /// <summary>Hands the body back to the solver, throwing it at <paramref name="velocity"/> — so letting go
    /// mid-swing throws it, which is the whole point of dragging things around.</summary>
    public void ReleaseGrab(Vector3 velocity)
    {
        if (grabbed is not { } id) return;
        grabbed = null;
        var i = IndexOf(id);
        if (i < 0) return;

        var handle = bodies[i].Handle;
        handle.Type = BodyType.Dynamic;
        handle.LinearVelocity = velocity;
    }

    /// <summary>
    /// Applies the sidebar's world settings: the world's gravity vector, every existing shape's friction and
    /// restitution, and every existing body's damping — and remembers them as the defaults for whatever is spawned
    /// next.
    /// </summary>
    /// <remarks>
    /// Box3D keeps friction and restitution on the <b>shape</b> and damping on the <b>body</b>, and neither is a
    /// world-level default, so changing them means walking what already exists. That walk is why this is posted to
    /// the physics thread rather than being set from the slider's own callback: a <c>Shape</c> handle resolves
    /// against the native world, which only this thread may touch.
    /// </remarks>
    public void ApplyParameters(float gravity, float friction, float bounce, float drag)
    {
        world.Gravity = new Vector3(0, -gravity, 0);
        (material, damping) = ((friction, bounce), drag);

        foreach (var body in bodies)
        {
            var handle = body.Handle;
            handle.LinearDamping = drag;
            handle.AngularDamping = drag;
            ApplyMaterial(handle);
            // A change nobody feels is a change that looks broken: a sleeping body would sit there with its new
            // bounce until something else woke it.
            if (drag > 0 || !handle.IsAwake) handle.IsAwake = true;
        }
    }

    /// <summary>Advances the world one fixed step, first steering any held body toward its target.</summary>
    public void Step(float dt)
    {
        if (grabbed is { } id)
        {
            var i = IndexOf(id);
            // MoveTowards gives the body a real velocity for this step rather than teleporting it, so it shoves
            // the rest of the scene around properly on the way.
            if (i >= 0) bodies[i].Handle.MoveTowards(grabTarget, bodies[i].Handle.Rotation, dt, wake: true);
            else grabbed = null;
        }

        world.Step(dt);
        StepCount++;
    }

    /// <inheritdoc/>
    public void Dispose() => world.Dispose();
    #endregion

    #region Private methods
    private int Track(Body handle, BodyShape shape, Vector3 halfExtents, int colorKey, Vector3 velocity, int meshId = -1)
    {
        var id = ++nextId;
        handle.UserData = (ulong)id;
        if (velocity != Vector3.Zero) handle.LinearVelocity = velocity;
        handle.LinearDamping = damping;
        handle.AngularDamping = damping;
        ApplyMaterial(handle);
        bodies.Add(new SandboxBody(id, handle, shape, halfExtents, colorKey, meshId));
        return id;
    }

    // Every shape on one body takes the current material. A body normally has exactly one shape here, so the span
    // is stack-allocated rather than pooled; ShapeCount is what sizes it.
    private void ApplyMaterial(Body handle)
    {
        var count = handle.ShapeCount;
        if (count <= 0) return;

        Span<Shape> shapes = count <= 8 ? stackalloc Shape[count] : new Shape[count];
        var written = handle.GetShapes(shapes);
        for (var i = 0; i < written; i++)
        {
            shapes[i].Friction = material.Friction;
            shapes[i].Restitution = material.Bounce;
        }
    }

    private int IndexOf(int id)
    {
        for (var i = 0; i < bodies.Count; i++)
        {
            if (bodies[i].Id == id) return i;
        }

        return -1;
    }
    #endregion

    #region Fields
    // Vertex budget for a spawned mesh body's collision hull. Points beyond it are merged by the engine.
    private const int HullVertexBudget = 32;

    private readonly PhysicsWorld world;
    private readonly List<SandboxBody> bodies = [];

    private int nextId;
    private int? grabbed;
    private Vector3 grabTarget;

    // The material and damping newly spawned bodies inherit, kept in step with the sidebar by ApplyParameters.
    // Defaults match SandboxParameters so the two agree before the first slider is touched.
    private (float Friction, float Bounce) material = (0.6f, 0.3f);
    private float damping;
    #endregion
}

/// <summary>A dynamic body plus the shape and colour the renderer needs and the engine does not store.</summary>
/// <param name="Id">Stable identifier — what selection and posted commands refer to.</param>
/// <param name="Handle">The Box3D handle. Valid only on the physics thread.</param>
/// <param name="Shape">How to draw it.</param>
/// <param name="HalfExtents">Extents as spawned — see <see cref="SceneSnapshot.HalfExtents"/>.</param>
/// <param name="ColorKey">Palette index, fixed at spawn so a body keeps its colour.</param>
/// <param name="MeshId">Registry id for a <see cref="BodyShape.Mesh"/> body; -1 otherwise.</param>
public readonly record struct SandboxBody(int Id, Body Handle, BodyShape Shape, Vector3 HalfExtents, int ColorKey, int MeshId = -1);
