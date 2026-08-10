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
public sealed class PhysicsRunner : IDisposable
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
            next.Positions[i] = b.Handle.Position;
            next.Rotations[i] = b.Handle.Rotation;
            next.Velocities[i] = b.Handle.LinearVelocity;
            next.HalfExtents[i] = b.HalfExtents;
            next.Shapes[i] = b.Shape;
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
    public SandboxBody AddBox(Vector3 position, Vector3 halfExtents, int colorKey)
    {
        var handle = world.CreateDynamicBody(position);
        handle.AddBox(Box.FromSize(halfExtents * 2));
        return Track(new SandboxBody(handle, BodyShape.Box, halfExtents, colorKey));
    }

    /// <summary>Spawns a dynamic sphere.</summary>
    public SandboxBody AddSphere(Vector3 position, float radius, int colorKey)
    {
        var handle = world.CreateDynamicBody(position);
        handle.AddSphere(new Sphere(radius));
        return Track(new SandboxBody(handle, BodyShape.Sphere, new Vector3(radius, radius, radius), colorKey));
    }

    /// <summary>Removes every dynamic body, leaving the static geometry.</summary>
    public void ClearBodies()
    {
        foreach (var b in bodies) b.Handle.Destroy();
        bodies.Clear();
    }

    /// <summary>Advances the world one fixed step.</summary>
    public void Step(float dt)
    {
        world.Step(dt);
        StepCount++;
    }

    /// <inheritdoc/>
    public void Dispose() => world.Dispose();
    #endregion

    #region Private methods
    private SandboxBody Track(SandboxBody body)
    {
        bodies.Add(body);
        return body;
    }
    #endregion

    #region Fields
    private readonly PhysicsWorld world;
    private readonly List<SandboxBody> bodies = [];
    #endregion
}

/// <summary>A dynamic body plus the shape and colour the renderer needs and the engine does not store.</summary>
/// <param name="Handle">The Box3D handle. Valid only on the physics thread.</param>
/// <param name="Shape">How to draw it.</param>
/// <param name="HalfExtents">Extents as spawned — see <see cref="SceneSnapshot.HalfExtents"/>.</param>
/// <param name="ColorKey">Palette index, fixed at spawn so a body keeps its colour.</param>
public readonly record struct SandboxBody(Body Handle, BodyShape Shape, Vector3 HalfExtents, int ColorKey);
