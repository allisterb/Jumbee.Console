namespace Jumbee.Console.SandboxDemo;

/// <summary>
/// The right-hand sidebar: what the scene is doing, and every control needed to change it with the mouse alone.
/// </summary>
/// <remarks>
/// <para>
/// This is the demo's second argument. The first is that a terminal can hold a 60 fps z-buffered viewport at all;
/// this is that the viewport is not a special case — ordinary controls sit beside it, on the same UI thread, in the
/// same layout, and neither gives anything up. Every widget here is a stock <c>Jumbee.Console</c> control.
/// </para>
/// <para>
/// <b>Keys and widgets stay in agreement</b> because neither talks to the other. The state objects
/// (<see cref="SpawnSettings"/>, <see cref="SandboxParameters"/>, <see cref="SceneView"/>) own the truth and raise a
/// change event; a widget writes to the state, and <see cref="Refresh"/> reads it back. The <see cref="syncing"/>
/// guard is what stops the round trip from looping: while the panel is pushing state into a widget, that widget's
/// own change event does nothing.
/// </para>
/// <para>
/// The readouts update from <see cref="SceneView.Drew"/> — the snapshot that was actually <em>drawn</em>, not the
/// newest one — so the numbers always describe the picture beside them.
/// </para>
/// </remarks>
public sealed class SidebarPanel : CompositeControl, Jumbee.Console.IScrollable
{
    #region Constructors
    /// <summary>Builds the sidebar over <paramref name="view"/> and its <paramref name="parameters"/>.
    /// <paramref name="resetScene"/> is what the <c>r</c> key and the menu's <c>Reset scene</c> do — repopulating the
    /// world is the shell's job, not the panel's, so it arrives as an action.</summary>
    public SidebarPanel(SceneView view, SandboxParameters parameters, Action resetScene)
    {
        this.view = view;
        this.parameters = parameters;
        Width = Columns;

        renderer = new Select([.. view.Renderers.Select(r => r.Name)]) { SelectedIndex = 0, FitContent = true };
        BuildSpawnMeshOptions();

        // Widget -> state. Each of these is the only place a widget writes, and every one is inert while syncing.
        renderer.SelectionChanged += (_, _) => Push(() => view.SetRenderer(view.Renderers[renderer.SelectedIndex]));
        edges.SelectionChanged += (_, _) => Push(() => view.SetEdgeStyle((SilhouetteStyle)edges.SelectedIndex));
        wrapLighting.Changed += (_, on) => Push(() => view.SetWrapLighting(on));
        occlusion.ValueChanged += (_, v) => Push(() => view.SetOcclusionStrength((float)v));
        shade.ValueChanged += (_, v) => Push(() => view.SetShadeLevels((float)v));
        quadrants.Changed += (_, on) => Push(() => view.SetQuadrantSampling(on));
        smooth.ValueChanged += (_, v) => Push(() => view.SetEdgeSmoothing((float)v));
        stratify.Changed += (_, on) => Push(() => view.SetStratify(on));
        scanCap.SelectionChanged += (_, _) =>
            Push(() => view.SetScanCap(WireframeRenderer.ScanCapChoices[scanCap.SelectedIndex].Value));
        density.ValueChanged += (_, v) => Push(() => view.SetMeshDensity(SubPixelsOf((float)v)));
        shape.SelectionChanged += (_, _) => Push(() => view.Spawn.Shape = (BodyShape)shape.SelectedIndex);
        mesh.SelectionChanged += (_, _) => Push(SelectMesh);
        size.ValueChanged += (_, v) => Push(() => view.Spawn.Scale = (float)v);
        speed.ValueChanged += (_, v) => Push(() => view.Spawn.LaunchSpeed = (float)v);
        gravity.ValueChanged += (_, v) => Push(() => parameters.Gravity = (float)v);
        friction.ValueChanged += (_, v) => Push(() => parameters.Friction = (float)v);
        bounce.ValueChanged += (_, v) => Push(() => parameters.Bounce = (float)v);
        drag.ValueChanged += (_, v) => Push(() => parameters.Drag = (float)v);
        timeScale.ValueChanged += (_, v) => Push(() => parameters.TimeScale = (float)v);

        drop.Activated += (_, _) => view.SpawnAtTarget();
        fire.Activated += (_, _) => view.Launch();
        delete.Activated += (_, _) => view.DeleteSelected();
        clear.Activated += (_, _) => view.ClearScene();
        reset.Activated += (_, _) => resetScene();
        resetWorld.Activated += (_, _) => parameters.Reset();

        camera = new CameraPad(view.Camera, () => UI.SetFocus(view));

        // State -> widget. Three sources, one refresh: whatever moved, the whole panel re-reads the state, which is
        // cheaper to reason about than tracking which widget a given change should touch.
        view.RendererChanged += Refresh;
        view.SelectionChanged += _ => Refresh();
        view.Spawn.Changed += Refresh;
        parameters.Changed += Refresh;

        SetContent(BuildSections(spaced: true));
        Refresh();
    }
    #endregion

    #region Properties
    /// <summary>The width the sidebar occupies when docked.</summary>
    public const int Columns = 32;

    /// <summary>The viewport rows the spaced layout needs. Below this the sidebar drops to the compact one; below
    /// what <em>that</em> needs, the enclosing frame scrolls.</summary>
    /// <remarks>
    /// A stale value here is now a cosmetic bug rather than a silent one: it picks the roomy layout in a viewport
    /// that cannot hold it, and the frame scrolls instead of clipping. It used to decide whether the bottom section
    /// existed at all. Still worth keeping honest — the harness's <c>--shell 200xH</c> sweeps both tiers.
    /// </remarks>
    public const int SpacedRows = 63;

    // A form of many fields rather than a composite built around one editor, so Tab walks the widgets instead of
    // being handed to whichever one has focus.
    /// <inheritdoc/>
    protected override bool TabNavigatesChildren => true;

    // Roomy when there is room, compact when there is not — so a short terminal shows the whole panel rather than
    // making you scroll for the camera pad. Scrolling is the backstop under that, not the first answer.
    //
    // Measured against the FRAME'S VIEWPORT, not this control's own ActualHeight, and that is not a detail: inside a
    // scrolling frame a control is laid out at the height IScrollable.MeasureHeight reports, so ActualHeight becomes
    // the content height and the comparison turns into "is the spaced layout at least as tall as the spaced layout".
    // It latches on the first answer and the tier never changes again, at any terminal size, silently. The viewport
    // comes from the frame's own size, which the parent decides, so nothing here feeds back into it.
    //
    // Guarded on the mode actually changing, not on the height: this runs on every re-layout, and calling
    // SetContent from inside one that hasn't changed anything is how a layout starts chasing its own tail.
    /// <inheritdoc/>
    protected override void Control_OnInitialization()
    {
        base.Control_OnInitialization();
        var rows = Frame?.ViewportSize.Height ?? ActualHeight;
        if (rows <= 0) return;   // before the first real layout; deciding from 0 would rebuild twice for nothing

        var wanted = rows >= SpacedRows;
        if (wanted == spaced || rebuilding) return;

        rebuilding = true;
        try
        {
            spaced = wanted;
            SetContent(BuildSections(wanted));
            Refresh();
        }
        finally
        {
            rebuilding = false;
        }
    }
    #endregion

    #region Methods
    /// <summary>Updates the readouts from the frame that was just drawn. Wired to <see cref="SceneView.Drew"/>.</summary>
    public void Report(SceneSnapshot snapshot, bool paused)
    {
        drawn = snapshot;
        var edgeName = view.Edges is { } e and not SilhouetteStyle.None ? "/" + e.ToString().ToLowerInvariant() : "";
        status.Text = $"{(paused ? "PAUSED" : "RUN")}  {view.Renderer.Name}{edgeName}";
        counts.Text = $"{snapshot.Count} bodies · {snapshot.AwakeCount} awake";
        clock.Text = $"t={snapshot.SimTime:F1}s  step {snapshot.StepMilliseconds:F2}ms";
        RefreshInspector();
    }

    /// <summary>Rebuilds the mesh drop-down from the registry — after a model has been loaded at runtime.</summary>
    public void RefreshMeshes()
    {
        syncing = true;
        try { mesh.SetOptions(MeshNames()); }
        finally { syncing = false; }
        Refresh();
    }

    /// <summary>The stacked height of every section, so an enclosing frame scrolls the panel when its viewport is
    /// too short even for the compact layout.</summary>
    /// <remarks>
    /// Summed from the sections rather than written down, because a total goes stale the moment one changes size —
    /// and under-reporting does not merely clip the tail, it collapses the last sections to zero height. This is what
    /// makes <see cref="SpacedRows"/> merely cosmetic: getting the tier wrong now costs a scroll, not a section.
    /// </remarks>
    public int MeasureHeight(int width)
    {
        var rows = 0;
        foreach (var section in sections) rows += section.OuterRows;
        return rows;
    }

    /// <summary>Re-reads every widget's value from the state it mirrors.</summary>
    public void Refresh()
    {
        syncing = true;
        try
        {
            var spawn = view.Spawn;
            renderer.SelectedIndex = IndexOfRenderer();
            edges.SelectedIndex = (int)(view.Edges ?? SilhouetteStyle.None);
            wrapLighting.IsChecked = view.WrapLighting ?? false;
            occlusion.Value = view.OcclusionStrength ?? 0f;
            // Both belong to the shaded renderer alone, so they grey out under the other two exactly as the mesh
            // dials do under anything but the wireframe.
            edges.Enabled = wrapLighting.Enabled = occlusion.Enabled = view.OcclusionStrength is not null;
            // Both solid renderers have a ramp, so this one greys out under the WIREFRAME instead -- the only
            // renderer here that draws no shaded surface at all.
            shade.Value = view.ShadeLevels ?? ShadedRenderer.DefaultShadeLevels;
            quadrants.IsChecked = view.QuadrantSampling ?? false;
            shade.Enabled = quadrants.Enabled = view.ShadeLevels is not null;
            smooth.Value = view.EdgeSmoothing ?? 0f;
            smooth.Enabled = view.EdgeSmoothing is not null;
            stratify.IsChecked = view.Stratify ?? false;
            scanCap.SelectedIndex = IndexOfScanCap();
            density.Value = DetailOf(view.MeshDensity ?? WireframeRenderer.DefaultSubPixelsPerTriangle);
            // Greyed out under a renderer that draws every triangle, rather than left live-but-inert: they still
            // report the wireframe's real settings, and Enabled is what says "not while this renderer is drawing".
            stratify.Enabled = scanCap.Enabled = density.Enabled = view.MeshDialsApply;
            shape.SelectedIndex = (int)spawn.Shape;
            mesh.SelectedIndex = spawn.MeshId >= 0 ? spawn.MeshId : 0;
            size.Value = spawn.Scale;
            speed.Value = spawn.LaunchSpeed;
            gravity.Value = parameters.Gravity;
            friction.Value = parameters.Friction;
            bounce.Value = parameters.Bounce;
            drag.Value = parameters.Drag;
            timeScale.Value = parameters.TimeScale;
        }
        finally
        {
            syncing = false;
        }

        RefreshInspector();
    }
    #endregion

    #region Private methods
    // A widget's own change handler. Inert while Refresh is writing into it, so state -> widget -> state cannot
    // loop; when a real edit gets through, the state raises Changed and Refresh runs, which is how a value the
    // state clamped (a slider dragged past a limit) comes straight back to the widget.
    private void Push(Action apply)
    {
        if (syncing) return;
        apply();
    }

    private void SelectMesh()
    {
        if (Meshes.RegisteredCount == 0) return;
        view.Spawn.MeshId = Math.Clamp(mesh.SelectedIndex, 0, Meshes.RegisteredCount - 1);
        view.Spawn.Shape = BodyShape.Mesh;
    }

    private int IndexOfRenderer()
    {
        for (var i = 0; i < view.Renderers.Count; i++)
            if (ReferenceEquals(view.Renderers[i], view.Renderer)) return i;
        return 0;
    }

    // Falls back to the default's slot rather than 0 when the active renderer has no scan cap, so switching to the
    // shaded renderer does not park the drop-down on "5k" and then apply that on the way back.
    private int IndexOfScanCap()
    {
        var cap = view.ScanCap ?? WireframeRenderer.DefaultScanCap;
        for (var i = 0; i < WireframeRenderer.ScanCapChoices.Length; i++)
            if (WireframeRenderer.ScanCapChoices[i].Value == cap) return i;
        return Array.FindIndex(WireframeRenderer.ScanCapChoices, c => c.Value == WireframeRenderer.DefaultScanCap);
    }

    // Read from the registry rather than handed a fixed list: models can be loaded while the app is running, and
    // RefreshMeshes puts the new ones in the drop-down without rebuilding the panel.
    private static string[] MeshNames() => Meshes.RegisteredCount == 0
        ? ["(none)"]
        : [.. Enumerable.Range(0, Meshes.RegisteredCount).Select(Meshes.NameOf)];

    private void BuildSpawnMeshOptions() => mesh = new Select(MeshNames()) { FitContent = true };

    private void RefreshInspector()
    {
        var i = view.Selected is { } sel && drawn is not null ? drawn.IndexOf(sel) : -1;
        if (view.Selected is not { } id || drawn is not { } s || i < 0)
        {
            selection.Text = "no selection";
            motion.Text = "";
            return;
        }

        selection.Text = $"#{id} {s.Shapes[i].ToString().ToLowerInvariant()}  {s.Masses[i]:F0} kg";
        motion.Text = $"{s.Velocities[i].Length():F1} m/s  {(s.Awake[i] ? "awake" : "asleep")}";
    }

    private static TextLabel Line(string text, Color color) =>
        new(TextLabelOrientation.Horizontal, text, color) { Focusable = false, Height = 1 };

    private static TextLabel Spacer() => Line("", MutedColor);

    // The five panels, either spaced or compact. Every INTERACTIVE control gets a blank row under it in the spaced
    // form; the readout lines (Scene, and the inspector's two) stay flush either way, because those are one block
    // of text rather than things to aim at. Stacked flush, a column of drop-downs and sliders reads as one
    // undifferentiated slab and it is genuinely hard to tell which track belongs to which label.
    private ILayout BuildSections(bool spaced)
    {
        var gap = spaced ? 1 : 0;
        sections =
        [
            // Hand-built rather than Stack'd for the same reason the Inspector is: the three readout lines are one
            // block of text, so the gap belongs only before the buttons. Clear and Reset are the c and r keys — the
            // two scene-wide actions, in the section the menu also files them under.
            new Section("Scene", spaced
                ? new VerticalStackPanel(status, counts, clock, Spacer(), Row(clear, reset))
                : new VerticalStackPanel(status, counts, clock, Row(clear, reset)), 4 + gap),
            // The wireframe's three mesh-sampling dials live here rather than in a section of their own, and the
            // reason is purely that a section costs two rows of frame: with them split out, a 40-row terminal
            // clipped the camera pad off the bottom even in the compact layout.
            // Detail before Scan, as in the viewer's panel: Detail changes what you see, Scan only caps how much of
            // a large model is examined and does nothing to a model below that cap.
            new Section("Render", Stack(spaced, Labelled("Renderer", renderer), Labelled("Edges", edges),
                wrapLighting, occlusion, shade, quadrants, smooth, stratify, density, Labelled("Scan", scanCap)),
                10 + (9 * gap)),
            new Section("Spawn", Stack(spaced, Labelled("Shape", shape), Labelled("Mesh", mesh), size, speed,
                Row(drop, fire)), 5 + (4 * gap)),
            new Section("World", Stack(spaced, gravity, friction, bounce, drag, timeScale, Row(resetWorld)),
                6 + (5 * gap)),
            // Hand-built rather than Stack'd: its first two rows are one readout, so the gap belongs only before
            // the button. Delete alone — it acts on the selection, which is what an inspector is for; Clear acts on
            // the whole scene and lives up in Scene beside Reset.
            new Section("Inspector", spaced
                ? new VerticalStackPanel(selection, motion, Spacer(), Row(delete))
                : new VerticalStackPanel(selection, motion, Row(delete)), 3 + gap),
            // Never spaced: the pad's buttons are one control, and a gap through the middle of a D-pad reads as two.
            new Section("Camera", new VerticalStackPanel(camera), CameraPad.Rows),
        ];

        return new VerticalStackPanel([.. sections]);
    }

    // The items stacked, with a blank row between each when spaced — one fewer spacer than items, so a section
    // never ends on a gap.
    private static ILayout Stack(bool spaced, params IFocusable[] items)
    {
        if (!spaced) return new VerticalStackPanel(items);

        var rows = new List<IFocusable>(items.Length * 2);
        for (var i = 0; i < items.Length; i++)
        {
            if (i > 0) rows.Add(Spacer());
            rows.Add(items[i]);
        }

        return new VerticalStackPanel([.. rows]);
    }

    // A caption beside a control, in the same column the sliders put theirs.
    //
    // A Grid, NOT a HorizontalStackPanel, and the difference is a trap worth knowing: a stack asks each child in
    // turn how wide it wants to be and offers it everything still unclaimed. A TextLabel's answer is "all of it"
    // (IntrinsicWidth 0 means fill), so it takes the whole row and every later child is laid out at zero width and
    // never appears. Columns of fixed width are a Grid's job.
    private static ILayout Labelled(string caption, Control control) =>
        new Grid([1], [LabelColumn, Columns - 2 - LabelColumn], [[Line(caption, MutedColor), control]]);

    private static ILayout Row(Button left, Button right) =>
        new Grid([1], [Columns / 2 - 2, Columns / 2 - 2], [[left, right]]);

    // One button on a two-button row, so a lone action lines up with the left-hand column everywhere else.
    private static ILayout Row(Button only) =>
        new Grid([1], [Columns / 2 - 2, Columns / 2 - 2], [[only, Spacer()]]);

    private static Button Action(string text) =>
        new Button(text) { Style = ButtonStyle.Secondary with { MinWidth = Columns / 2 - 3 } };


    private static Slider Param(string label, float min, float max, float value, string format = "F2") =>
        // 9 cells, the width of the longest label ("Occlusion"). Every slider reserves the same, so the tracks line
        // up down the panel — LabelWidth is exact, not a minimum, so a longer label would be ellipsized instead.
        new Slider(min, max, value, label) { LabelWidth = 9, ValueFormat = format };

    // Detail is the reciprocal of the renderer's sub-pixels-per-triangle -- see WireframeRenderer.DetailFromSubPixels
    // for why. It lives there rather than here so this panel and the viewer's agree on what a given number means.
    private static float DetailOf(float subPixelsPerTriangle) =>
        WireframeRenderer.DetailFromSubPixels(subPixelsPerTriangle);

    private static float SubPixelsOf(float detail) => WireframeRenderer.SubPixelsFromDetail(detail);
    #endregion

    #region Fields
    private readonly SceneView view;
    private readonly SandboxParameters parameters;

    private readonly TextLabel status = Line("RUN", HeadingColor);
    private readonly TextLabel counts = Line("", TextColor);
    private readonly TextLabel clock = Line("", MutedColor);
    private readonly Button clear = Action("Clear");
    private readonly Button reset = Action("Reset");

    private readonly Select renderer;
    private readonly Select edges = new Select("none", "line", "glyph") { FitContent = true };
    private readonly Switch wrapLighting = new Switch("Half-Lambert light");

    // The shaded renderer's screen-space ambient occlusion. Named for what it is rather than for the field behind
    // it (ShadedRenderer.OcclusionStrength): it samples a depth ring per sub-pixel and darkens by how much of that
    // ring sits nearer than a flat surface would, which is AO, not a contact fudge.
    private readonly Slider occlusion = Param("Occlusion", 0f, 1f, ShadedRenderer.DefaultOcclusionStrength, "F2");

    // How many brightness bands a curved surface shows. The quality dial that actually moves the picture at this
    // resolution -- and the renderer's largest performance lever, so it is deliberately not buried in a menu.
    private readonly Slider shade = Param("Shades", MeshRenderer.MinShadeLevels, MeshRenderer.MaxShadeLevels,
        ShadedRenderer.DefaultShadeLevels, "F0");

    // The other half of the antialiasing story, and deliberately next to Smooth: this one buys HORIZONTAL
    // resolution (a half-cell boundary rather than a whole-cell one) and pays in fill, where Smooth buys softness
    // and pays in colours. Both solid renderers composite through the surface, so it greys out only under the
    // wireframe -- same gating as Shades.
    private readonly Switch quadrants = new Switch("Quadrant AA");

    // Cheap antialiasing along silhouettes. Shaded-only, like Edges and Occlusion, and off by default -- see
    // ShadedRenderer.EdgeSmoothing for why it is not simply free.
    private readonly Slider smooth = Param("Smooth", 0f, 1f, 0f, "F2");
    private readonly Switch stratify = new Switch("Even over screen", isOn: true);
    private readonly Select scanCap =
        new Select([.. WireframeRenderer.ScanCapChoices.Select(c => c.Label)]) { FitContent = true };

    // Labelled by what it DOES rather than by its unit: the underlying value is sub-pixels per triangle, so
    // smaller is denser, and a slider that draws less as you push it right is a small cruelty. Inverted at the
    // boundary instead -- see the density handlers.
    private readonly Slider density = Param("Detail", WireframeRenderer.MinDetail, WireframeRenderer.MaxDetail,
        DetailOf(WireframeRenderer.DefaultSubPixelsPerTriangle), "F1");

    private readonly Select shape = new Select("box", "sphere", "mesh") { FitContent = true };
    private Select mesh = new Select("(none)") { FitContent = true };
    private readonly Slider size = Param("Size", SpawnSettings.MinScale, SpawnSettings.MaxScale, 1f);
    private readonly Slider speed = Param("Speed", SpawnSettings.MinSpeed, SpawnSettings.MaxSpeed, 20f, "F0");
    private readonly Button drop = Action("Drop");
    private readonly Button fire = Action("Fire");

    private readonly Slider gravity = Param("Gravity", 0f, 30f, 9.8f);
    private readonly Slider friction = Param("Friction", 0f, 1f, 0.6f);
    private readonly Slider bounce = Param("Bounce", 0f, 1f, 0.3f);
    private readonly Slider drag = Param("Drag", 0f, 4f, 0f);
    private readonly Slider timeScale = Param("Time", 0.05f, 4f, 1f);
    private readonly Button resetWorld = Action("Reset");

    private readonly TextLabel selection = Line("no selection", TextColor);
    private readonly TextLabel motion = Line("", MutedColor);
    private readonly Button delete = Action("Delete");

    private readonly CameraPad camera;

    // Matches the sliders' LabelWidth plus their gap, so captions line up down the whole sidebar.
    private const int LabelColumn = 9;

    private SceneSnapshot? drawn;
    private Section[] sections = [];
    private bool syncing;
    private bool spaced = true;
    private bool rebuilding;

    private static readonly Color HeadingColor = new(200, 205, 215);
    private static readonly Color TextColor = new(170, 176, 190);
    private static readonly Color MutedColor = new(130, 136, 150);
    private static readonly Color BorderColor = new(70, 78, 96);
    #endregion

    #region Child types
    // One titled block of the stack. A framed VerticalStackPanel is not itself a Control, so each section is a
    // small composite that can carry the frame and the title.
    private sealed class Section : CompositeControl
    {
        public Section(string title, ILayout content, int rows)
        {
            Height = rows;
            SetContent(content);
            this.WithFrame(borderStyle: BorderStyle.Rounded, borderFgColor: BorderColor)
                .WithTitle(title, new TitleStyle(TitlePos.TopLeft, TitleBorderStyle.Inline));
        }

        // The rows this section occupies in the stack: its content plus the frame's top and bottom border. An inline
        // title lives IN the top border row, so it costs nothing extra.
        public int OuterRows => Height + 2;

        protected override bool TabNavigatesChildren => true;
    }
    #endregion
}
