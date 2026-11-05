namespace Jumbee.Console.Mesh;

using System.Diagnostics;
using System.Numerics;

using ConsoleGUI.Space;

/// <summary>
/// A 3D viewport: draws an <see cref="ISceneSource"/> through whichever <see cref="ISceneRenderer"/> is active, and
/// owns the orbit camera, the render keys and — over a <see cref="ModelScene"/> — the model keys.
/// </summary>
/// <remarks>
/// <para>
/// <c>new MeshView(ModelLoader.Load("bunny.obj"))</c> is a complete viewer: three renderers, a turntable, a framed
/// camera. Over any other source it is the drawing half of a scene, and a host adds behaviour through
/// <see cref="OnSceneKey"/>, <see cref="OnBodyPress"/> and <see cref="OnSceneTick"/>.
/// </para>
/// <para>
/// A composite rather than a plain control because each renderer brings its own surface — a <see cref="Canvas"/> for
/// the wireframe, a cell-writing control for the solid ones — and swapping renderers is swapping that child.
/// </para>
/// <para>
/// <b>Rasterises off the UI thread.</b> A dense model costs tens of milliseconds a frame; spent on the UI thread, that
/// freezes menus and camera drag for exactly as long. A frame is captured on the UI thread, drawn on a background
/// job and published back, and requests coalesce, so a slow model costs frame <em>rate</em>, never responsiveness.
/// </para>
/// </remarks>
public class MeshView : CompositeControl
{
    #region Constructors
    /// <summary>A complete viewer for <paramref name="mesh"/>: shaded, wireframe and solid renderers, a turntable,
    /// and a camera framed on the model.</summary>
    public MeshView(TriangleMesh mesh) : this(new ModelScene(Single(mesh)), new ShadedRenderer())
    {
        AddRenderer(new WireframeRenderer());
        AddRenderer(new SolidRenderer());
    }

    /// <summary>Creates a viewer over <paramref name="model"/>, drawn with <paramref name="renderer"/>. Enables the
    /// model keys and the turntable, and frames the camera on the model whenever it changes.</summary>
    public MeshView(ModelScene model, ISceneRenderer renderer, int fps = 60) : this((ISceneSource)model, renderer, fps)
    {
        Model = model;
        // Framed on the subject, and re-framed only when the SUBJECT changes (another mesh, or the same one stood
        // up another way): a transform tweak leaves the camera where the user put it.
        model.MeshChanged += FrameModel;
        FrameModel();
    }

    /// <summary>Creates a viewport over <paramref name="source"/>, starting with <paramref name="renderer"/> and
    /// redrawing <paramref name="fps"/> times a second.</summary>
    public MeshView(ISceneSource source, ISceneRenderer renderer, int fps = 60)
    {
        Source = source;
        this.renderer = renderer;
        renderers.Add(renderer);
        SetContent(new Boundary(renderer.Surface));

        // The scene changes continuously, so drive redraws on a clock rather than waiting for a state change to
        // invalidate. Feed runs on the UI thread, so the state Tick reads and the camera it captures need no
        // synchronization -- it hands a fully-described FrameRequest to the render job and returns.
        feed = Feed(Tick, Math.Max(1, 1000 / Math.Max(1, fps)));

        // THE RASTERISER RUNS HERE, not on the UI thread -- see the remarks. See also Control.Job and
        // ISceneRenderer.DrawsOffThread.
        renderJob = Job(
            produce: () =>
            {
                var request = Volatile.Read(ref pending);
                if (request is null) return (Rendered?)null;
                var active = this.renderer;
                return new Rendered(active, active.Draw(request), request.Frame);
            },
            apply: result =>
            {
                if (disposed || result is not { } r) return;
                // Drop a frame produced by a renderer that has since been swapped out: publishing it would put the
                // old renderer's pixels into the surface the new one is now showing.
                if (!ReferenceEquals(r.Renderer, this.renderer)) return;
                r.Renderer.Publish(r.Frame);
                Published(r.Scene);
            });
    }
    #endregion

    #region Events
    /// <summary>Raised on the UI thread after each frame is drawn, carrying the scene that was drawn.</summary>
    public event Action<SceneFrame>? Drew;

    /// <summary>Raised when the active renderer or any of its settings changes — however it was changed.</summary>
    /// <remarks>An inspector's widgets and the <c>v</c>/<c>e</c>/<c>w</c> keys both go through here, which is what
    /// keeps a widget and a key agreeing without either knowing about the other.</remarks>
    public event Action? SettingsChanged;

    /// <summary>Raised when the selection moves, carrying the new body id (or <see langword="null"/>).</summary>
    public event Action<int?>? SelectionChanged;
    #endregion

    #region Properties
    /// <summary>The orbit rig. UI-thread owned.</summary>
    public OrbitCamera Camera { get; } = new();

    /// <summary>Where the frames come from.</summary>
    public ISceneSource Source { get; }

    /// <summary>The model scene, when this view was created over one; otherwise <see langword="null"/>.</summary>
    public ModelScene? Model { get; }

    /// <summary>Whether the model keys (<c>[ ]</c> to step, <c>x X y Y z Z</c> to scale, <c>, . ; '</c> to shear,
    /// <c>0</c>, <c>a</c>, <c>p</c>) are live. Only meaningful over a <see cref="Model"/>. Defaults to on.</summary>
    public bool ModelKeys { get; set; } = true;

    /// <summary>The renderer currently drawing the scene.</summary>
    public ISceneRenderer Renderer => renderer;

    /// <summary>The renderers <c>v</c> cycles, in order.</summary>
    public IReadOnlyList<ISceneRenderer> Renderers => renderers;

    /// <summary>The active renderer's screen window, as of its last frame — what a host needs to turn a cell into a
    /// world ray.</summary>
    public Viewport Viewport => renderer.Viewport;

    /// <summary>The active renderer's projection, as of its last frame.</summary>
    public Projection Projection => renderer.Projection;

    /// <summary>
    /// Whether bodies can be selected — by click, by <c>Tab</c>, or by setting <see cref="Selected"/>. Off by default.
    /// </summary>
    /// <remarks>
    /// Turn it on where a selection means something: a scene whose bodies can be grabbed, thrown or deleted. A
    /// single-model viewer must leave it off. A selected body is tinted <see cref="Palette.Selection"/> (white), which
    /// silently overrides the model's colour, and a viewer with nothing in its key map that clears a selection kept
    /// its model white for the rest of the session — and a press on the model selected it instead of orbiting.
    /// Turning it off clears the selection.
    /// </remarks>
    public bool AllowSelection
    {
        get => allowSelection;
        set
        {
            allowSelection = value;
            if (!value) Selected = null;
        }
    }

    /// <summary>The selected body's id, or <see langword="null"/>. Selection is by id, not index, so it survives
    /// bodies being removed around it. Always <see langword="null"/> unless <see cref="AllowSelection"/> is set.</summary>
    public int? Selected
    {
        get => selected;
        set
        {
            // Refused here rather than at each caller: the mouse, Tab and a host all write this, and a guard on one
            // of them is a bug waiting for the next route to be added.
            if (!allowSelection) value = null;
            var changed = selected != value;
            selected = value;
            renderer.Selected = value;
            if (changed) SelectionChanged?.Invoke(value);
        }
    }

    /// <summary>The frame last drawn — what a footer or an inspector should report, so their numbers match the
    /// picture rather than running a tick ahead of it.</summary>
    public SceneFrame? Drawn { get; private set; }

    /// <summary>The active silhouette style, or <see langword="null"/> when the current renderer has none.</summary>
    public SilhouetteStyle? Edges => renderer is ShadedRenderer s ? s.Edges : null;

    /// <summary>Whether the shaded renderer wraps its lighting, or <see langword="null"/> under a renderer that has
    /// no lighting to wrap.</summary>
    public bool? WrapLighting => renderer is ShadedRenderer s ? s.WrapLighting : null;

    /// <summary>The active renderer's shade-ramp resolution, or <see langword="null"/> under the wireframe, which has
    /// no ramp to quantise.</summary>
    /// <remarks>Unlike the shaded-only dials, BOTH solid renderers have one, and each keeps its own value across a
    /// renderer swap — the two defaults differ on purpose (see <see cref="SolidRenderer.DefaultShadeLevels"/>).</remarks>
    public float? ShadeLevels => renderer is MeshRenderer m ? m.ShadeLevels : null;

    /// <summary>The shaded renderer's texture-colour ramp, or <see langword="null"/> under any other renderer, which
    /// does not texture.</summary>
    public float? TextureLevels => renderer is ShadedRenderer s ? s.TextureLevels : null;

    /// <summary>How the shaded renderer draws a model's materials, or <see langword="null"/> under any other renderer,
    /// which does not texture.</summary>
    public TextureMode? Texture => renderer is ShadedRenderer s ? s.Texture : null;

    /// <summary>How hard the shaded renderer darkens creases and contacts, or <see langword="null"/> under a
    /// renderer that has no such pass.</summary>
    public float? OcclusionStrength => renderer is ShadedRenderer s ? s.OcclusionStrength : null;

    /// <summary>Whether the active renderer samples twice per column, or <see langword="null"/> under the wireframe,
    /// which draws no half-block cells to composite.</summary>
    /// <remarks>Like <see cref="ShadeLevels"/>, BOTH solid renderers have this and each keeps its own setting across
    /// a renderer swap.</remarks>
    public bool? QuadrantSampling => renderer is MeshRenderer m ? m.QuadrantSampling : null;

    // The wireframe's mesh-sampling dials read and write the WIREFRAME wherever it is in the renderer list, not "the
    // active renderer if it happens to be the wireframe" -- which is how the shaded renderer's options work, and it
    // would be wrong here. Those return null under another renderer so a widget can show a neutral state; but with no
    // disabled state on Switch/Select/Slider, a null-to-false fallback does not read as "not applicable", it reads as
    // OFF. The settings genuinely do persist on the wireframe across a swap, so the dials are always live and always
    // truthful, and MeshDialsApply is what a menu (which does have a disabled state) uses to grey them.

    /// <summary>Whether the mesh dials affect what is on screen right now — false when a renderer that draws every
    /// triangle is active. They remain settable either way.</summary>
    public bool MeshDialsApply => renderer is WireframeRenderer;

    /// <summary>Whether the wireframe spreads a mesh's budget over the screen rather than over its triangle list,
    /// or <see langword="null"/> when there is no wireframe renderer at all.</summary>
    public bool? Stratify => Wireframe?.Stratify;

    /// <summary>How many triangles of a mesh the wireframe examines per frame, or <see langword="null"/> when there
    /// is no wireframe renderer at all.</summary>
    public int? ScanCap => Wireframe?.ScanCap;

    /// <summary>Screen area per drawn mesh triangle — lower draws more — or <see langword="null"/> when there is no
    /// wireframe renderer at all.</summary>
    public float? MeshDensity => Wireframe?.SubPixelsPerTriangle;
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
        SettingsChanged?.Invoke();
    }

    /// <summary>Adds a renderer to the set <c>v</c> cycles through. The first one is the one in use.</summary>
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
        SettingsChanged?.Invoke();
    }

    /// <summary>Turns half-Lambert lighting on or off. A no-op under a renderer that does not light per pixel.</summary>
    public void SetWrapLighting(bool wrap)
    {
        if (renderer is not ShadedRenderer shaded || shaded.WrapLighting == wrap) return;
        shaded.WrapLighting = wrap;
        SettingsChanged?.Invoke();
    }

    /// <summary>Sets the active renderer's shade-ramp resolution. A no-op under the wireframe.</summary>
    public void SetShadeLevels(float levels)
    {
        if (renderer is not MeshRenderer mesh || mesh.ShadeLevels == levels) return;
        mesh.ShadeLevels = levels;
        SettingsChanged?.Invoke();
    }

    /// <summary>Sets the shaded renderer's texture-colour ramp. A no-op under any other renderer.</summary>
    public void SetTextureLevels(float levels)
    {
        if (renderer is not ShadedRenderer shaded || shaded.TextureLevels == levels) return;
        shaded.TextureLevels = levels;
        SettingsChanged?.Invoke();
    }

    /// <summary>Sets how the shaded renderer draws materials: <see cref="TextureMode.None"/>,
    /// <see cref="TextureMode.Auto"/> or <see cref="TextureMode.Image"/>. A no-op under any other renderer.</summary>
    public void SetTexture(TextureMode mode)
    {
        if (renderer is not ShadedRenderer shaded || shaded.Texture == mode) return;
        shaded.Texture = mode;
        SettingsChanged?.Invoke();
    }

    /// <summary>Sets the contact-darkening strength; 0 disables the pass. See
    /// <see cref="ShadedRenderer.OcclusionStrength"/>.</summary>
    public void SetOcclusionStrength(float strength)
    {
        if (renderer is not ShadedRenderer shaded || shaded.OcclusionStrength == strength) return;
        shaded.OcclusionStrength = strength;
        SettingsChanged?.Invoke();
    }

    /// <summary>Turns quadrant sampling on or off. A no-op under the wireframe. See
    /// <see cref="MeshRenderer.QuadrantSampling"/>.</summary>
    public void SetQuadrantSampling(bool on)
    {
        if (renderer is not MeshRenderer mesh || mesh.QuadrantSampling == on) return;
        mesh.QuadrantSampling = on;
        SettingsChanged?.Invoke();
    }

    /// <summary>Turns screen stratification on or off. See <see cref="WireframeRenderer.Stratify"/> for the trade.</summary>
    public void SetStratify(bool on)
    {
        if (Wireframe is not { } w || w.Stratify == on) return;
        w.Stratify = on;
        SettingsChanged?.Invoke();
    }

    /// <summary>Sets the per-frame triangle scan ceiling. See <see cref="WireframeRenderer.ScanCap"/>.</summary>
    public void SetScanCap(int cap)
    {
        if (Wireframe is not { } w || w.ScanCap == cap) return;
        w.ScanCap = cap;
        SettingsChanged?.Invoke();
    }

    /// <summary>Sets the mesh draw density. See <see cref="WireframeRenderer.SubPixelsPerTriangle"/>.</summary>
    public void SetMeshDensity(float subPixelsPerTriangle)
    {
        if (Wireframe is not { } w || w.SubPixelsPerTriangle == subPixelsPerTriangle) return;
        w.SubPixelsPerTriangle = subPixelsPerTriangle;
        SettingsChanged?.Invoke();
    }

    /// <summary>The body under <paramref name="position"/>, in the view's own cells, or <see langword="null"/>.
    /// Picks against the frame last drawn, so it agrees with the picture.</summary>
    /// <remarks>Works whether or not <see cref="AllowSelection"/> is set; selecting the result is the caller's call.</remarks>
    public int? Pick(Position position) => renderer.Pick(position.X, position.Y, Drawn ?? Source.Frame, Camera);

    /// <summary>Moves the selection to the next body, in frame order, wrapping. A no-op unless
    /// <see cref="AllowSelection"/> is set.</summary>
    public void SelectNext(int direction = 1)
    {
        var frame = Drawn ?? Source.Frame;
        if (frame.Count == 0)
        {
            Selected = null;
            return;
        }

        var current = Selected is { } id ? frame.IndexOf(id) : -1;
        var next = current < 0 ? 0 : (((current + direction) % frame.Count) + frame.Count) % frame.Count;
        Selected = frame.Ids[next];
    }

    /// <summary>Aims the camera at the <see cref="Model"/> and sets that as its home, so <c>Home</c> returns to the
    /// framed shot. Runs on its own whenever the model's subject changes; a no-op with no model.</summary>
    /// <remarks>
    /// Framed on the model rather than on the floor: the model STANDS on the ground (see
    /// <see cref="ModelScene.Elevation"/>), so the camera looks at the height its middle actually sits at — which
    /// differs per model. The viewport is only real once the tree has laid out, and this first runs before that, so it
    /// falls back to the aspect of a typical wide terminal and the opening shot is framed rather than arbitrary.
    /// </remarks>
    public void FrameModel()
    {
        if (Model is not { } model) return;
        var halfSpan = renderer.Viewport.CellAspect is > 0 and var aspect ? aspect : DefaultCellAspect;
        Camera.HomeTarget = new Vector3(0, model.Elevation, 0);
        Camera.HomeDistance = model.FramingDistance(renderer.Projection.Focal, halfSpan);
        Camera.Reset();
    }

    /// <summary>Stops the render feed and the render job, and unwires the control. After this the view never draws
    /// again, whatever is still queued for it.</summary>
    /// <remarks>
    /// <para>
    /// Two steps, because cancelling a feed stops the <em>producer</em> and not the work it has already handed over.
    /// <see cref="FeedHandle.StopAsync"/> joins the tick in flight; the flag makes any tick already sitting in the
    /// dispatcher queue a no-op.
    /// </para>
    /// <para>
    /// Both are invisible in an app that disposes on the way out, and exactly visible in one that starts a second UI
    /// afterwards. The queue is <b>not</b> cleared between a <see cref="UI.Stop"/> and the next <see cref="UI.Start"/>
    /// — deliberately, since that is how you post work for the next session to pick up — so a feed left running for
    /// even a few milliseconds after the stop lands its ticks in the <em>next</em> session, where they draw a scene
    /// nobody is looking at.
    /// </para>
    /// </remarks>
    public override void Dispose()
    {
        disposed = true;
        // Both are joined, and the render job matters more than the feed: a rasterisation in flight is holding a
        // frame and a mesh, and switching scenes disposes this view while the next one is being built. Letting it
        // finish is cheaper than making every renderer safe to tear down mid-frame.
        feed.StopAsync().Wait(FeedJoinMs);
        renderJob.StopAsync().Wait(FeedJoinMs);
        base.Dispose();
    }
    #endregion

    #region Protected methods
    /// <inheritdoc/>
    protected override bool WantsMouse => true;

    /// <summary>A host's own keys, tried <b>before</b> the built-in ones so a host can reclaim a letter. Return
    /// <see langword="true"/> to mark the key handled.</summary>
    protected virtual bool OnSceneKey(ConsoleKeyInfo key) => false;

    /// <summary>
    /// A press landed on body <paramref name="id"/>, which is now <see cref="Selected"/>. Return
    /// <see langword="true"/> to take the drag that follows (it then arrives at <see cref="OnBodyDrag"/> and
    /// <see cref="OnBodyRelease"/>); <see langword="false"/>, the default, lets the drag orbit the camera.
    /// </summary>
    /// <remarks>Only called when <see cref="AllowSelection"/> is set.</remarks>
    protected virtual bool OnBodyPress(int id, Position position) => false;

    /// <summary>The pointer moved during a drag that <see cref="OnBodyPress"/> took.</summary>
    protected virtual void OnBodyDrag(Position position) { }

    /// <summary>The pointer was released at the end of a drag that <see cref="OnBodyPress"/> took.</summary>
    protected virtual void OnBodyRelease(Position position) { }

    /// <summary>Called on the UI thread each tick with the frame about to be drawn, after a selection that no longer
    /// resolves has been dropped and before the frame is captured — so a selection made here shows in this frame.</summary>
    protected virtual void OnSceneTick(SceneFrame frame) { }

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
        var handled = OnSceneKey(key) || HandleKey(key);
        if (handled) inputEvent.Handled = true;
        return handled;
    }

    // Press on a body a host takes (OnBodyPress) hands it the drag; anywhere else the drag orbits the camera. One
    // button, and which you get is decided by what is under the pointer -- the same rule as every 3D editor.
    /// <inheritdoc/>
    protected override void OnMousePress(Position position)
    {
        lastDrag = position;
        CaptureMouse();

        // Not merely "the pick is ignored": a scene with nothing to grab must ORBIT here, or a press on the model is
        // dead -- neither selecting nor turning it. Which you got used to depend on whether the pointer landed within
        // the pick radius of the model's projected CENTRE, so the bunny's ears orbited and its body did not.
        if (allowSelection && Pick(position) is { } id)
        {
            Selected = id;
            if (OnBodyPress(id, position))
            {
                draggingBody = true;
                return;
            }
        }

        orbiting = true;
    }

    /// <inheritdoc/>
    protected override void OnMouseRelease(Position position)
    {
        ReleaseMouse();
        orbiting = false;
        if (!draggingBody) return;
        draggingBody = false;
        OnBodyRelease(position);
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

        if (draggingBody) OnBodyDrag(position);
    }

    /// <inheritdoc/>
    // A wheel notch reports delta < 0 for up, so up pulls the camera in and down pushes it out.
    protected override void OnMouseWheel(Position position, int delta) =>
        Camera.Zoom(delta < 0 ? 1f - ZoomStep : 1f + ZoomStep);

    /// <inheritdoc/>
    protected override HelpInfo? GetHelpInfo()
    {
        var help = new HelpInfo("3D viewport")
            .WithKey("Arrows", "Orbit the camera (hold Shift for fine steps)")
            .WithKey("PgUp/PgDn", "Zoom in / out")
            .WithKey("Home", "Reset the camera")
            .WithKey("Drag", allowSelection
                ? "On a body: select it. On empty space: orbit. Wheel zooms"
                : "Orbit the camera. Wheel zooms");
        if (allowSelection)
        {
            help.WithKey("Click", "Select the body under the pointer")
                .WithKey("Tab / Esc", "Select the next body / clear the selection");
        }

        help.WithKey("v", "Cycle renderer: wireframe (braille edges), solid (flat-shaded), shaded (point light + edges + AO)")
            .WithKey("e", "Shaded only: cycle silhouettes — off, ink outline, edge glyphs")
            .WithKey("w", "Shaded only: half-Lambert lighting — rescues the unlit side, costs contrast on the lit one");
        if (Model is not null && ModelKeys)
        {
            help.WithKey("[ / ]", "Previous / next model")
                .WithKey("x X  y Y  z Z", "Shrink / stretch along X, Y or Z")
                .WithKey(", .  ; '", "Shear X / Z by height")
                .WithKey("0", "Reset the model's scale and shear")
                .WithKey("a", "Toggle the model's up axis between Y and Z")
                .WithKey("p", "Pause / resume the turntable");
        }

        return help;
    }
    #endregion

    #region Private methods
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

    private static MeshLibrary Single(TriangleMesh mesh)
    {
        var library = new MeshLibrary();
        library.Add(mesh, "model");
        return library;
    }

    private bool HandleKey(ConsoleKeyInfo key)
    {
        // Shift is the fine-adjust modifier throughout: a fifth of the step, on every axis.
        var scale = (key.Modifiers & ConsoleModifiers.Shift) != 0 ? 0.2f : 1f;
        switch (key.Key)
        {
            case ConsoleKey.LeftArrow: Camera.Orbit(-OrbitStep * scale, 0); return true;
            case ConsoleKey.RightArrow: Camera.Orbit(OrbitStep * scale, 0); return true;
            case ConsoleKey.UpArrow: Camera.Orbit(0, -OrbitStep * scale); return true;
            case ConsoleKey.DownArrow: Camera.Orbit(0, OrbitStep * scale); return true;
            case ConsoleKey.PageUp: Camera.Zoom(1f - (ZoomStep * scale)); return true;
            case ConsoleKey.PageDown: Camera.Zoom(1f + (ZoomStep * scale)); return true;
            case ConsoleKey.Home: Camera.Reset(); return true;
            // Only with a selection to move: otherwise Tab is focus navigation and Esc is the app's, and taking them
            // would trap focus in the viewport for nothing.
            case ConsoleKey.Escape when allowSelection: Selected = null; return true;
            // Explicitly, not via KeyChar: the decoder gives Shift+Tab (CSI Z) a KeyChar of '\0', so a char-based
            // branch would silently handle Tab and drop Shift+Tab.
            case ConsoleKey.Tab when allowSelection: SelectNext((key.Modifiers & ConsoleModifiers.Shift) != 0 ? -1 : +1); return true;
        }

        if (HandleModelChar(key.KeyChar)) return true;
        switch (key.KeyChar)
        {
            case 'v': NextRenderer(); return true;
            case 'e': NextEdgeStyle(); return true;
            case 'w': SetWrapLighting(!(WrapLighting ?? false)); return true;
            default: return false;
        }
    }

    private bool HandleModelChar(char c)
    {
        if (Model is not { } model || !ModelKeys) return false;
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

    private void Tick()
    {
        if (disposed) return;

        // The turntable is driven from wall clock, not a frame count, so it turns at the same rate whatever the
        // paint rate or the terminal size.
        var now = clock.Elapsed;
        Model?.Advance((now - lastTick).TotalSeconds);
        lastTick = now;

        var frame = Source.Frame;

        // Selection is by id, so a body removed by anything other than us (a scene clear, a preset reload) just stops
        // resolving -- drop it rather than leave a highlight pointing at nothing.
        if (selected is { } id && frame.IndexOf(id) < 0) Selected = null;
        OnSceneTick(frame);

        // Capture everything the rasteriser needs WHILE ON THE UI THREAD: the frame (already immutable once
        // published), the camera resolved to a value, and the laid-out viewport size. After this line the frame is
        // fully described and nothing it reads can move.
        var active = renderer;
        var request = new FrameRequest(frame, Camera.GetView(), active.Surface.ActualWidth, active.Surface.ActualHeight);

        if (!active.DrawsOffThread)
        {
            active.Publish(active.Draw(request));
            Published(frame);
            return;
        }

        // Off-thread: hand the request over and ask the queue for a pass. Requests coalesce, so a rasteriser slower
        // than the tick rate simply produces fewer frames -- it never accumulates a backlog of stale ones, and the
        // UI thread returns here immediately whatever the model costs.
        Volatile.Write(ref pending, request);
        renderJob.Request();
    }

    // Runs on the UI thread, either inline above or from the render job's apply.
    private void Published(SceneFrame frame)
    {
        Drawn = frame;
        Drew?.Invoke(frame);
    }
    #endregion

    #region Fields
    private const float OrbitStep = 0.08f;
    private const float ZoomStep = 0.1f;
    private const float DragPerCell = 0.02f;

    // The NDC half-span of a viewport on a wide terminal, used only before the first layout. See FrameModel.
    private const double DefaultCellAspect = 0.6;

    // Bound on joining the render feed at Dispose. Generous: a tick is sub-millisecond, and the wait exists to avoid
    // hanging a teardown on a wedged one, not to time a normal frame.
    private const int FeedJoinMs = 500;

    private readonly FeedHandle feed;

    // The off-thread rasteriser and the frame it should draw next. `pending` is written by the UI thread and read by
    // the job, so it is a reference (atomic to assign) rather than a struct that could be read half-updated.
    private readonly JobHandle renderJob;
    private FrameRequest? pending;

    private readonly List<ISceneRenderer> renderers = [];
    private readonly Stopwatch clock = Stopwatch.StartNew();
    private TimeSpan lastTick;

    private ISceneRenderer renderer;
    private bool allowSelection;
    private int? selected;
    private bool orbiting;
    private bool draggingBody;
    private volatile bool disposed;
    private Position lastDrag;
    #endregion

    #region Child types
    /// <param name="Renderer">Which renderer produced this — checked before publishing, in case <c>v</c> swapped
    /// renderers while the frame was in flight.</param>
    /// <param name="Frame">The renderer's opaque frame, or <see langword="null"/> if it drew inline.</param>
    /// <param name="Scene">The scene the frame shows, for the <see cref="Drew"/> listeners.</param>
    private readonly record struct Rendered(ISceneRenderer Renderer, object? Frame, SceneFrame Scene);
    #endregion
}
