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
    public SceneView(ISceneSource source, ISceneRenderer renderer, int fps = 60)
    {
        this.source = source;
        this.runner = source as PhysicsRunner;
        this.renderer = renderer;
        renderers.Add(renderer);
        SetContent(new Boundary(renderer.Surface));

        // The scene changes continuously, so drive redraws on a clock rather than waiting for a state change to
        // invalidate. Feed runs on the UI thread, so Draw and the camera reads it makes need no synchronization.
        feed = Feed(Tick, Math.Max(1, 1000 / Math.Max(1, fps)));
    }
    #endregion

    #region Properties
    /// <summary>The orbit rig. UI-thread owned.</summary>
    public OrbitCamera Camera { get; } = new();

    /// <summary>What the spawn and launch keys produce.</summary>
    public SpawnSettings Spawn { get; } = new();

    /// <summary>The renderer currently drawing the scene.</summary>
    public ISceneRenderer Renderer => renderer;

    /// <summary>
    /// Whether this scene has a selection at all — true for the sandbox, false for the model viewer.
    /// </summary>
    /// <remarks>
    /// Derived from the source rather than set by the shell, because it is a fact about the scene and not a
    /// preference: selecting is only meaningful where something can be grabbed, thrown or deleted, and all three go
    /// through a <see cref="PhysicsRunner"/>. A scene without one has a single subject that is always the subject.
    /// <para>
    /// It exists because selecting in the viewer was <b>actively harmful</b>: a selected body is tinted
    /// <see cref="Palette.Selection"/> (white), which silently overrode the Colour drop-down — and since nothing in
    /// the viewer's key map clears a selection, the model stayed white for the rest of the session and through every
    /// model after it.
    /// </para>
    /// </remarks>
    public bool SupportsSelection => runner is not null;

    /// <summary>The selected body's id, or <see langword="null"/>. Selection is by id, not index, so it survives
    /// bodies being deleted around it. Always <see langword="null"/> when <see cref="SupportsSelection"/> is not
    /// set.</summary>
    public int? Selected
    {
        get => selected;
        set
        {
            // Refused here rather than at each caller: the mouse, Tab and the select-the-newest-spawn tick all
            // write this, and a guard on one of the three is a bug waiting for the next route to be added.
            if (!SupportsSelection) value = null;
            var changed = selected != value;
            selected = value;
            renderer.Selected = value;
            if (changed) SelectionChanged?.Invoke(value);
        }
    }

    /// <summary>The snapshot last drawn — what the footer and inspector should report, so their numbers match the
    /// picture rather than running a tick ahead of it.</summary>
    public SceneSnapshot? Drawn { get; private set; }

    /// <summary>Raised on the UI thread after each frame is drawn, carrying the snapshot that was drawn.</summary>
    public event Action<SceneSnapshot>? Drew;

    /// <summary>Raised when the active renderer or its edge style changes — however it was changed.</summary>
    /// <remarks>The sidebar's drop-downs and the <c>v</c>/<c>e</c> keys both go through here, which is what keeps
    /// the widget and the key agreeing without either knowing about the other.</remarks>
    public event Action? RendererChanged;

    /// <summary>Raised when the selection moves, carrying the new body id (or <see langword="null"/>).</summary>
    public event Action<int?>? SelectionChanged;

    /// <summary>The renderers <c>v</c> cycles, in order — what the sidebar's drop-down lists.</summary>
    public IReadOnlyList<ISceneRenderer> Renderers => renderers;
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
        RendererChanged?.Invoke();
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
        SetEdgeStyle(shaded.Edges switch
        {
            SilhouetteStyle.None => SilhouetteStyle.Line,
            SilhouetteStyle.Line => SilhouetteStyle.Glyph,
            _ => SilhouetteStyle.None,
        });
    }

    /// <summary>Sets the silhouette treatment directly. A no-op under a renderer that has none.</summary>
    public void SetEdgeStyle(SilhouetteStyle style)
    {
        if (renderer is not ShadedRenderer shaded || shaded.Edges == style) return;
        shaded.Edges = style;
        RendererChanged?.Invoke();
    }

    /// <summary>The active silhouette style, or <see langword="null"/> when the current renderer has none.</summary>
    public SilhouetteStyle? Edges => renderer is ShadedRenderer s ? s.Edges : null;

    /// <summary>Whether the shaded renderer wraps its lighting, or <see langword="null"/> under a renderer that has
    /// no lighting to wrap.</summary>
    public bool? WrapLighting => renderer is ShadedRenderer s ? s.WrapLighting : null;

    /// <summary>Turns half-Lambert lighting on or off. A no-op under a renderer that does not light per pixel.</summary>
    public void SetWrapLighting(bool wrap)
    {
        if (renderer is not ShadedRenderer shaded || shaded.WrapLighting == wrap) return;
        shaded.WrapLighting = wrap;
        RendererChanged?.Invoke();
    }

    /// <summary>The active renderer's shade-ramp resolution, or <see langword="null"/> under the wireframe, which has
    /// no ramp to quantise.</summary>
    /// <remarks>Unlike the shaded-only dials above, BOTH solid renderers have one, and each keeps its own value
    /// across a renderer swap — the two defaults differ on purpose (see <see cref="SolidRenderer.DefaultShadeLevels"/>).</remarks>
    public float? ShadeLevels => renderer is MeshRenderer m ? m.ShadeLevels : null;

    /// <summary>Sets the active renderer's shade-ramp resolution. A no-op under the wireframe.</summary>
    public void SetShadeLevels(float levels)
    {
        if (renderer is not MeshRenderer mesh || mesh.ShadeLevels == levels) return;
        mesh.ShadeLevels = levels;
        RendererChanged?.Invoke();
    }

    /// <summary>How hard the shaded renderer darkens creases and contacts, or <see langword="null"/> under a
    /// renderer that has no such pass.</summary>
    public float? OcclusionStrength => renderer is ShadedRenderer s ? s.OcclusionStrength : null;

    /// <summary>Sets the contact-darkening strength; 0 disables the pass. See
    /// <see cref="ShadedRenderer.OcclusionStrength"/>.</summary>
    public void SetOcclusionStrength(float strength)
    {
        if (renderer is not ShadedRenderer shaded || shaded.OcclusionStrength == strength) return;
        shaded.OcclusionStrength = strength;
        RendererChanged?.Invoke();
    }


    /// <summary>Whether the active renderer samples twice per column, or <see langword="null"/> under the wireframe,
    /// which draws no half-block cells to composite.</summary>
    /// <remarks>Like <see cref="ShadeLevels"/> and unlike the shaded-only dials, BOTH solid renderers have this and
    /// each keeps its own setting across a renderer swap.</remarks>
    public bool? QuadrantSampling => renderer is MeshRenderer m ? m.QuadrantSampling : null;

    /// <summary>Turns quadrant sampling on or off. A no-op under the wireframe. See
    /// <see cref="MeshRenderer.QuadrantSampling"/>.</summary>
    public void SetQuadrantSampling(bool on)
    {
        if (renderer is not MeshRenderer mesh || mesh.QuadrantSampling == on) return;
        mesh.QuadrantSampling = on;
        RendererChanged?.Invoke();
    }

    // The wireframe's mesh-sampling dials.
    //
    // These read and write the WIREFRAME wherever it is in the renderer list, not "the active renderer if it
    // happens to be the wireframe" -- which is how the shaded renderer's edge and lighting options work, and it
    // would be wrong here. Those return null under another renderer so a widget can show a neutral state; but with
    // no disabled state on Switch/Select/Slider, a null-to-false fallback does not read as "not applicable", it
    // reads as OFF. Showing a switch off while the setting is on is a lie, and the settings genuinely do persist on
    // the wireframe instance across a renderer swap. So the dials are always live and always truthful, and
    // MeshDialsApply is what a MENU (which does have a disabled state) uses to grey them.

    private WireframeRenderer? Wireframe
    {
        get
        {
            if (renderer is WireframeRenderer active) return active;
            foreach (var candidate in renderers)
            {
                if (candidate is WireframeRenderer w) return w;
            }

            return null;
        }
    }

    /// <summary>Whether the mesh dials affect what is on screen right now — false when a renderer that draws every
    /// triangle is active. They remain settable either way.</summary>
    public bool MeshDialsApply => renderer is WireframeRenderer;

    /// <summary>Whether the wireframe spreads a mesh's budget over the screen rather than over its triangle list,
    /// or <see langword="null"/> when there is no wireframe renderer at all.</summary>
    public bool? Stratify => Wireframe?.Stratify;

    /// <summary>Turns screen stratification on or off. See <see cref="WireframeRenderer.Stratify"/> for the trade.</summary>
    public void SetStratify(bool on)
    {
        if (Wireframe is not { } w || w.Stratify == on) return;
        w.Stratify = on;
        RendererChanged?.Invoke();
    }

    /// <summary>How many triangles of a mesh the wireframe examines per frame, or <see langword="null"/> when there
    /// is no wireframe renderer at all.</summary>
    public int? ScanCap => Wireframe?.ScanCap;

    /// <summary>Sets the per-frame triangle scan ceiling. See <see cref="WireframeRenderer.ScanCap"/>.</summary>
    public void SetScanCap(int cap)
    {
        if (Wireframe is not { } w || w.ScanCap == cap) return;
        w.ScanCap = cap;
        RendererChanged?.Invoke();
    }

    /// <summary>Screen area per drawn mesh triangle — lower draws more — or <see langword="null"/> when there is no
    /// wireframe renderer at all.</summary>
    public float? MeshDensity => Wireframe?.SubPixelsPerTriangle;

    /// <summary>Sets the mesh draw density. See <see cref="WireframeRenderer.SubPixelsPerTriangle"/>.</summary>
    public void SetMeshDensity(float subPixelsPerTriangle)
    {
        if (Wireframe is not { } w || w.SubPixelsPerTriangle == subPixelsPerTriangle) return;
        w.SubPixelsPerTriangle = subPixelsPerTriangle;
        RendererChanged?.Invoke();
    }

    /// <summary>Set when this view is showing a <see cref="ModelScene"/> rather than a simulation: enables the
    /// model-picking and transform keys, and turns the turntable.</summary>
    public ModelScene? Model { get; init; }

    /// <summary>Drops a body in above the camera target, so it lands in view wherever the camera is pointing.</summary>
    public void SpawnAtTarget()
    {
        var position = Camera.Target + new Vector3(0, Spawn.DropHeight, 0);
        var (shape, scale, key, mesh) = (Spawn.Shape, Spawn.Scale, nextColorKey++, Spawn.MeshId);
        runner?.Post(scene => scene.Add(shape, position, scale, key, default, mesh));
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
        runner?.Post(scene => scene.Add(shape, origin, scale, key, velocity, mesh));
        selectNewestSpawn = true;
    }

    /// <summary>Deletes the selected body, if there is one.</summary>
    public void DeleteSelected()
    {
        if (Selected is not { } id) return;
        Selected = null;
        runner?.Post(scene => scene.Remove(id));
    }

    /// <summary>Removes every dynamic body.</summary>
    public void ClearScene()
    {
        Selected = null;
        runner?.Post(scene => scene.ClearBodies());
    }

    /// <summary>Moves the selection to the next body, in spawn order, wrapping.</summary>
    public void SelectNext(int direction = 1)
    {
        var snapshot = Drawn ?? source.Snapshot;
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

    // The viewport is a window onto the scene, not a document, so it is not IScrollable: a frame sizes it to the
    // visible area rather than scrolling it.

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

        var snapshot = Drawn ?? source.Snapshot;
        // Not merely "the pick is ignored": a scene with nothing to grab must ORBIT here, or a press on the model
        // is dead — neither selecting nor turning it, which is how this read as "sometimes it rotates and sometimes
        // it goes white". Which you got depended on whether the pointer landed within the pick radius of the
        // model's projected CENTRE, so the bunny's ears orbited and its body did not.
        var hit = SupportsSelection ? renderer.Pick(position.X, position.Y, snapshot, Camera) : null;
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
        runner?.Post(scene => scene.BeginGrab(id));
    }

    /// <inheritdoc/>
    protected override void OnMouseRelease(Position position)
    {
        ReleaseMouse();
        orbiting = false;
        if (grabbed is not { } id) return;

        grabbed = null;
        var velocity = ThrowVelocity();
        runner?.Post(scene => scene.ReleaseGrab(velocity));
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
        runner?.Post(scene => scene.DragTo(id, target));
    }

    /// <summary>Stops the render feed and unwires the control. After this the view never draws again, whatever is
    /// still queued for it.</summary>
    /// <remarks>
    /// <para>
    /// Two steps, because cancelling a feed stops the <em>producer</em> and not the work it has already handed over.
    /// <see cref="FeedHandle.StopAsync"/> joins the tick in flight; the flag makes any tick already sitting in the
    /// dispatcher queue a no-op.
    /// </para>
    /// <para>
    /// Both are invisible in an app that disposes on the way out, and exactly visible in one that starts a second UI
    /// afterwards. The queue is <b>not</b> cleared between a <see cref="UI.Stop"/> and the next <see cref="UI.Start"/>
    /// — deliberately, since that is how you post work for the next session to pick up (the demo sets its initial
    /// focus that way) — so a feed left running for even a few milliseconds after the stop lands its ticks in the
    /// <em>next</em> session, where they draw a scene nobody is looking at.
    /// </para>
    /// </remarks>
    public override void Dispose()
    {
        disposed = true;
        feed.StopAsync().Wait(FeedJoinMs);
        base.Dispose();
    }

    /// <inheritdoc/>
    // A wheel notch reports delta < 0 for up, so up pulls the camera in and down pushes it out.
    protected override void OnMouseWheel(Position position, int delta) =>
        Camera.Zoom(delta < 0 ? 1f - ZoomStep : 1f + ZoomStep);

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
        .WithKey("u", "Show or hide the sidebar")
        .WithKey("+ / -", "Grow / shrink what gets spawned")
        .WithKey("] / [", "Raise / lower the launch speed");
    #endregion

    #region Private methods
    // The viewer's own keys, checked first so they can retarget characters the sandbox uses for things that do not
    // exist here (nothing to delete, no launch speed to set).
    private bool HandleModelChar(char c)
    {
        if (Model is not { } model) return false;
        switch (c)
        {
            case '[': model.Step(-1); return true;
            case ']': model.Step(+1); return true;
            case 'x': model.ScaleAxis(0, 1 / 1.15f); return true;
            case 'X': model.ScaleAxis(0, 1.15f); return true;
            case 'y': model.ScaleAxis(1, 1 / 1.15f); return true;
            case 'Y': model.ScaleAxis(1, 1.15f); return true;
            case 'z': model.ScaleAxis(2, 1 / 1.15f); return true;
            case 'Z': model.ScaleAxis(2, 1.15f); return true;
            case ',': model.Nudge(-0.08f, 0); return true;
            case '.': model.Nudge(+0.08f, 0); return true;
            case ';': model.Nudge(0, -0.08f); return true;
            case '\'': model.Nudge(0, +0.08f); return true;
            case '0': model.ResetTransform(); return true;
            case 'a': model.UpAxis = model.UpAxis == ModelUpAxis.Z ? ModelUpAxis.Y : ModelUpAxis.Z; return true;
            case 'p': model.SpinRate = model.SpinRate == 0f ? 0.35f : 0f; return true;
            default: return false;
        }
    }

    private bool HandleChar(char c)
    {
        if (HandleModelChar(c)) return true;
        switch (c)
        {
            case 'b': Spawn.ToggleShape(); return true;
            case 'n': SpawnAtTarget(); return true;
            case 'f': Launch(); return true;
            case 'x': DeleteSelected(); return true;
            case 'c': ClearScene(); return true;
            case 'v': NextRenderer(); return true;
            case 'e': NextEdgeStyle(); return true;
            case 'w': SetWrapLighting(!(WrapLighting ?? false)); return true;
            case 'm': Spawn.NextMesh(); return true;
            case '+' or '=': Spawn.StepScale(+1); return true;
            case '-' or '_': Spawn.StepScale(-1); return true;
            case ']': Spawn.StepLaunchSpeed(+1); return true;
            case '[': Spawn.StepLaunchSpeed(-1); return true;
            default: return false;
        }
    }

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

    private void Tick()
    {
        if (disposed) return;

        // The turntable is driven from wall clock, not a frame count, so it turns at the same rate whatever the
        // paint rate or the terminal size.
        var now = clock.Elapsed;
        Model?.Advance((now - lastTick).TotalSeconds);
        lastTick = now;

        var snapshot = source.Snapshot;

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

    // Bound on joining the render feed at Dispose. Generous: a tick is sub-millisecond, and the wait exists to avoid
    // hanging a teardown on a wedged one, not to time a normal frame.
    private const int FeedJoinMs = 500;

    private readonly ISceneSource source;
    private readonly FeedHandle feed;

    // The simulation, when the source IS one. Null for a static scene (the model viewer), where spawning, grabbing
    // and deleting have nothing to act on and quietly do nothing.
    private readonly PhysicsRunner? runner;
    private readonly List<ISceneRenderer> renderers = [];
    private readonly Stopwatch clock = Stopwatch.StartNew();
    private TimeSpan lastTick;
    private readonly Queue<(TimeSpan At, Vector3 Point)> throwSamples = new();

    private ISceneRenderer renderer;
    private int? selected;
    private int nextColorKey = 100;
    private bool selectNewestSpawn;
    private int highestSeenId;
    private bool orbiting;
    private volatile bool disposed;
    private int? grabbed;
    private Vector3 grabPlanePoint;
    private Vector3 grabPlaneNormal;
    private Vector3 grabOffset;
    private Position lastDrag;
    #endregion
}
