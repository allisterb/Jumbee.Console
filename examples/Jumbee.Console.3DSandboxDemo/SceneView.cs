namespace Jumbee.Console.SandboxDemo;

using System.Diagnostics;
using System.Numerics;

using ConsoleGUI.Input;
using ConsoleGUI.Space;

/// <summary>
/// The viewport: pulls the newest physics snapshot and hands it to whichever <see cref="ISceneRenderer"/> is
/// active, and owns the camera, the selection and the sandbox interactions.
/// </summary>
/// <remarks>
/// <para>
/// A composite rather than a plain control because the renderer brings its own surface — a <see cref="Canvas"/> for
/// wireframe, a cell-writing control for solid — and swapping renderers is swapping that child.
/// </para>
/// <para>
/// Everything here runs on the UI thread and reads only the published snapshot, so no interaction ever touches a
/// live Box3D handle. Changes go the other way, as commands posted to <see cref="PhysicsRunner.Post"/>.
/// </para>
/// </remarks>
public sealed class SceneView : CompositeControl
{
    #region Constructors
    /// <summary>Creates the viewport over <paramref name="runner"/>, starting with <paramref name="renderer"/>.</summary>
    public SceneView(PhysicsRunner runner, ISceneRenderer renderer, int fps = 60)
    {
        this.runner = runner;
        this.renderer = renderer;
        renderers.Add(renderer);
        SetContent(new Boundary(renderer.Surface));

        // The scene changes continuously, so drive redraws on a clock rather than waiting for a state change to
        // invalidate. Feed runs on the UI thread, so Draw and the camera reads it makes need no synchronization.
        Feed(Tick, Math.Max(1, 1000 / Math.Max(1, fps)));
    }
    #endregion

    #region Properties
    /// <summary>The orbit rig. UI-thread owned.</summary>
    public OrbitCamera Camera { get; } = new();

    /// <summary>What the spawn and launch keys produce.</summary>
    public SpawnSettings Spawn { get; } = new();

    /// <summary>The renderer currently drawing the scene.</summary>
    public ISceneRenderer Renderer => renderer;

    /// <summary>The selected body's id, or <see langword="null"/>. Selection is by id, not index, so it survives
    /// bodies being deleted around it.</summary>
    public int? Selected
    {
        get => selected;
        set
        {
            selected = value;
            renderer.Selected = value;
        }
    }

    /// <summary>The snapshot last drawn — what the footer and inspector should report, so their numbers match the
    /// picture rather than running a tick ahead of it.</summary>
    public SceneSnapshot? Drawn { get; private set; }

    /// <summary>Raised on the UI thread after each frame is drawn, carrying the snapshot that was drawn.</summary>
    public event Action<SceneSnapshot>? Drew;
    #endregion

    #region Methods
    /// <summary>Swaps the active renderer, moving its surface into the layout.</summary>
    public void SetRenderer(ISceneRenderer next)
    {
        if (ReferenceEquals(next, renderer)) return;
        renderer = next;
        next.Selected = selected;
        SetContent(new Boundary(next.Surface));
        Invalidate();
    }

    /// <summary>Adds a renderer to the set <c>v</c> cycles through. The first one added is the one in use.</summary>
    public void AddRenderer(ISceneRenderer next)
    {
        if (!renderers.Contains(next)) renderers.Add(next);
    }

    /// <summary>Switches to the next renderer — the same scene, drawn a different way, live.</summary>
    public void NextRenderer()
    {
        if (renderers.Count < 2) return;
        var i = renderers.IndexOf(renderer);
        SetRenderer(renderers[(i + 1) % renderers.Count]);
    }

    /// <summary>Cycles the shaded renderer's silhouette treatment: none, ink outline, edge glyphs. A no-op under
    /// the other renderers, which have none.</summary>
    public void NextEdgeStyle()
    {
        if (renderer is not ShadedRenderer shaded) return;
        shaded.Edges = shaded.Edges switch
        {
            SilhouetteStyle.None => SilhouetteStyle.Ink,
            SilhouetteStyle.Ink => SilhouetteStyle.Glyph,
            _ => SilhouetteStyle.None,
        };
    }

    /// <summary>The active silhouette style, or <see langword="null"/> when the current renderer has none.</summary>
    public SilhouetteStyle? Edges => renderer is ShadedRenderer s ? s.Edges : null;

    /// <summary>Drops a body in above the camera target, so it lands in view wherever the camera is pointing.</summary>
    public void SpawnAtTarget()
    {
        var position = Camera.Target + new Vector3(0, Spawn.DropHeight, 0);
        var (shape, scale, key, mesh) = (Spawn.Shape, Spawn.Scale, nextColorKey++, Spawn.MeshId);
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
        var muzzle = MathF.Max(MinMuzzleDistance, renderer.Projection.Focal * Spawn.BoundingRadius / MuzzleNdcRadius);
        var origin = view.Eye + (view.Forward * muzzle);
        var velocity = view.Forward * Spawn.LaunchSpeed;
        var (shape, scale, key, mesh) = (Spawn.Shape, Spawn.Scale, nextColorKey++, Spawn.MeshId);
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

    /// <summary>Moves the selection to the next body, in spawn order, wrapping.</summary>
    public void SelectNext(int direction = 1)
    {
        var snapshot = Drawn ?? runner.Snapshot;
        if (snapshot.Count == 0)
        {
            Selected = null;
            return;
        }

        var current = Selected is { } id ? snapshot.IndexOf(id) : -1;
        var next = current < 0 ? 0 : (((current + direction) % snapshot.Count) + snapshot.Count) % snapshot.Count;
        Selected = snapshot.Ids[next];
    }
    #endregion

    #region Protected methods
    /// <inheritdoc/>
    protected override bool WantsMouse => true;

    // The viewport is a window onto the scene, not a document: it must be exactly as tall as the frame's visible
    // area. Without this a wrapping ControlFrame offers an unbounded height so a scrollable child can grow, and a
    // control with no intrinsic height fills to the 1000-row clamp instead -- which does not look like a bug, it
    // looks like an empty viewport, because the camera's whole picture then lands off-screen.
    /// <inheritdoc/>
    protected override bool FillsFrameViewport => true;

    // Keys are handled in the TUNNEL, not in OnInput, and that distinction is load-bearing.
    //
    // A composite is normally a container: the layout route (Layout.OnInput) sees a CompositeControl and dispatches
    // into its CONTENT layout, so the key lands on whichever child has focus -- the composite's own OnInput is never
    // called. This viewport has no focusable children (the Canvas is display-only), so every key would be delivered
    // to the Canvas, which does not handle input, and silently dropped. That is not hypothetical: the arrow keys did
    // nothing at all until this moved.
    //
    // InterceptInput is the seam that works on BOTH routes -- Layout.OnInput calls RouteInterceptInput before
    // descending, and ControlFrame.OnInput calls it before forwarding to the focused descendant. A composite that
    // owns its keys rather than delegating them to children belongs here.
    //
    // Worth knowing when testing: UI.SendInput dispatches to FocusableControl and so takes the ControlFrame route,
    // NOT the layout route the live loop uses. A test built on it passes while the real app receives nothing.
    /// <inheritdoc/>
    protected override bool InterceptInput(UI.InputEventArgs inputEventArgs)
    {
        if (inputEventArgs.InputEvent is not { } inputEvent) return false;

        var key = inputEvent.Key;
        // Shift is the fine-adjust modifier throughout: a fifth of the step, on every axis.
        var scale = (key.Modifiers & ConsoleModifiers.Shift) != 0 ? 0.2f : 1f;
        var handled = true;
        switch (key.Key)
        {
            case ConsoleKey.LeftArrow: Camera.Orbit(-OrbitStep * scale, 0); break;
            case ConsoleKey.RightArrow: Camera.Orbit(OrbitStep * scale, 0); break;
            case ConsoleKey.UpArrow: Camera.Orbit(0, -OrbitStep * scale); break;
            case ConsoleKey.DownArrow: Camera.Orbit(0, OrbitStep * scale); break;
            case ConsoleKey.PageUp: Camera.Zoom(1f - (ZoomStep * scale)); break;
            case ConsoleKey.PageDown: Camera.Zoom(1f + (ZoomStep * scale)); break;
            case ConsoleKey.Home: Camera.Reset(); break;
            case ConsoleKey.Delete: DeleteSelected(); break;
            case ConsoleKey.Escape: Selected = null; break;
            // Explicitly, not via KeyChar: the decoder gives Shift+Tab (CSI Z) a KeyChar of '\0', so a char-based
            // branch would silently handle Tab and drop Shift+Tab.
            case ConsoleKey.Tab: SelectNext((key.Modifiers & ConsoleModifiers.Shift) != 0 ? -1 : +1); break;
            default: handled = HandleChar(key.KeyChar); break;
        }

        if (handled) inputEvent.Handled = true;
        return handled;
    }

    // Press on a body grabs it; press on empty space orbits the camera. One button, and which you get is decided by
    // what is under the pointer -- the same rule as every 3D editor.
    /// <inheritdoc/>
    protected override void OnMousePress(Position position)
    {
        lastDrag = position;
        CaptureMouse();

        var snapshot = Drawn ?? runner.Snapshot;
        var hit = renderer.Pick(position.X, position.Y, snapshot, Camera);
        if (hit is not { } id)
        {
            orbiting = true;
            return;
        }

        Selected = id;
        var index = snapshot.IndexOf(id);
        if (index < 0) return;

        // Drag in the plane through the body facing the camera, and keep the grab offset, so the body does not
        // snap its centre to the pointer the moment you touch it.
        var view = Camera.GetView();
        grabPlanePoint = snapshot.Positions[index];
        grabPlaneNormal = view.Forward;
        grabOffset = renderer.Viewport.TryRay(position.X, position.Y, view, renderer.Projection, out var o, out var d)
            && Projection.TryPlaneHit(o, d, grabPlanePoint, grabPlaneNormal, out var contact)
                ? grabPlanePoint - contact
                : Vector3.Zero;

        grabbed = id;
        throwSamples.Clear();
        RecordThrowSample(grabPlanePoint);
        runner.Post(scene => scene.BeginGrab(id));
    }

    /// <inheritdoc/>
    protected override void OnMouseRelease(Position position)
    {
        ReleaseMouse();
        orbiting = false;
        if (grabbed is not { } id) return;

        grabbed = null;
        var velocity = ThrowVelocity();
        runner.Post(scene => scene.ReleaseGrab(velocity));
    }

    /// <inheritdoc/>
    protected override void OnMouseMove(Position position)
    {
        if (orbiting)
        {
            // Cells are about twice as tall as they are wide, so a vertical cell is twice the angle of a horizontal
            // one -- halve dY to make a diagonal drag feel diagonal.
            Camera.Orbit((position.X - lastDrag.X) * DragPerCell, (position.Y - lastDrag.Y) * DragPerCell * 0.5f);
            lastDrag = position;
            return;
        }

        if (grabbed is not { } id) return;
        lastDrag = position;

        var view = Camera.GetView();
        if (!renderer.Viewport.TryRay(position.X, position.Y, view, renderer.Projection, out var o, out var d)) return;
        if (!Projection.TryPlaneHit(o, d, grabPlanePoint, grabPlaneNormal, out var contact)) return;

        var target = contact + grabOffset;
        RecordThrowSample(target);
        runner.Post(scene => scene.DragTo(id, target));
    }

    /// <inheritdoc/>
    protected override void OnMouseWheel(Position position, int delta) =>
        Camera.Zoom(delta > 0 ? 1f - ZoomStep : 1f + ZoomStep);

    /// <inheritdoc/>
    protected override HelpInfo? GetHelpInfo() => new HelpInfo("3D viewport")
        .WithKey("Arrows", "Orbit the camera (hold Shift for fine steps)")
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
        .WithKey("+ / -", "Grow / shrink what gets spawned")
        .WithKey("] / [", "Raise / lower the launch speed");
    #endregion

    #region Private methods
    private bool HandleChar(char c)
    {
        switch (c)
        {
            case 'b': Spawn.ToggleShape(); return true;
            case 'n': SpawnAtTarget(); return true;
            case 'f': Launch(); return true;
            case 'x': DeleteSelected(); return true;
            case 'c': ClearScene(); return true;
            case 'v': NextRenderer(); return true;
            case 'e': NextEdgeStyle(); return true;
            case 'm': Spawn.NextMesh(); return true;
            case '+' or '=': Spawn.StepScale(+1); return true;
            case '-' or '_': Spawn.StepScale(-1); return true;
            case ']': Spawn.StepLaunchSpeed(+1); return true;
            case '[': Spawn.StepLaunchSpeed(-1); return true;
            default: return false;
        }
    }

    // Throw velocity is measured from where the grab point actually went, not from the pointer delta: a release
    // after a pause should drop the body, not fling it at whatever speed the last mouse event implied.
    private void RecordThrowSample(Vector3 point)
    {
        throwSamples.Enqueue((clock.Elapsed, point));
        while (throwSamples.Count > ThrowSampleCount) throwSamples.Dequeue();
    }

    private Vector3 ThrowVelocity()
    {
        if (throwSamples.Count < 2) return Vector3.Zero;

        var samples = throwSamples.ToArray();
        var (t0, p0) = samples[0];
        var (t1, p1) = samples[^1];
        var seconds = (float)(t1 - t0).TotalSeconds;
        if (seconds < 1e-3f) return Vector3.Zero;

        var velocity = (p1 - p0) / seconds;
        // Cap it: a fast flick across a coarse terminal grid can imply an absurd speed, and the solver deals badly
        // with a body that crosses the world in one step.
        var speed = velocity.Length();
        return speed > MaxThrowSpeed ? velocity * (MaxThrowSpeed / speed) : velocity;
    }

    private void Tick()
    {
        var snapshot = runner.Snapshot;

        // Selection is by id, so a body deleted by anything other than us (a scene clear, a preset reload) just
        // stops resolving -- drop it rather than leave a highlight pointing at nothing.
        if (selected is { } id && snapshot.IndexOf(id) < 0) Selected = null;

        // Select whatever the last spawn produced, once the physics thread has actually created it. Ids only come
        // back through the snapshot -- the command that made it ran over there -- so this waits for a tick carrying
        // a higher id than we had. Without it, pressing f gives no feedback beyond a shape appearing.
        if (selectNewestSpawn && snapshot.Count > 0 && snapshot.Ids[^1] > highestSeenId)
        {
            Selected = snapshot.Ids[^1];
            selectNewestSpawn = false;
        }

        if (snapshot.Count > 0) highestSeenId = Math.Max(highestSeenId, snapshot.Ids[^1]);

        renderer.Draw(snapshot, Camera);
        Drawn = snapshot;
        Drew?.Invoke(snapshot);
    }
    #endregion

    #region Fields
    private const float OrbitStep = 0.08f;
    private const float ZoomStep = 0.1f;
    private const float DragPerCell = 0.02f;
    private const float MaxThrowSpeed = 40f;
    private const int ThrowSampleCount = 5;

    // The on-screen radius a launched body should have on its first frame, in NDC. The viewport's Y half-span is the
    // cell aspect (~0.6 on a wide terminal), so 0.2 is about a third of the half-height -- clearly an object, not a
    // wall. See the remarks on Launch for why this is derived rather than a fixed muzzle distance.
    private const float MuzzleNdcRadius = 0.2f;
    private const float MinMuzzleDistance = 2f;

    private readonly PhysicsRunner runner;
    private readonly List<ISceneRenderer> renderers = [];
    private readonly Stopwatch clock = Stopwatch.StartNew();
    private readonly Queue<(TimeSpan At, Vector3 Point)> throwSamples = new();

    private ISceneRenderer renderer;
    private int? selected;
    private int nextColorKey = 100;
    private bool selectNewestSpawn;
    private int highestSeenId;
    private bool orbiting;
    private int? grabbed;
    private Vector3 grabPlanePoint;
    private Vector3 grabPlaneNormal;
    private Vector3 grabOffset;
    private Position lastDrag;
    #endregion
}
