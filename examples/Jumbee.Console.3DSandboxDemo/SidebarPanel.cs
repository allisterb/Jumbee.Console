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
public sealed class SidebarPanel : CompositeControl
{
    #region Constructors
    /// <summary>Builds the sidebar over <paramref name="view"/> and its <paramref name="parameters"/>.</summary>
    public SidebarPanel(SceneView view, SandboxParameters parameters)
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

    /// <summary>The rows the spaced layout needs. Below this the sidebar drops to the compact one.</summary>
    public const int SpacedRows = 45;

    // A form of many fields rather than a composite built around one editor, so Tab walks the widgets instead of
    // being handed to whichever one has focus.
    /// <inheritdoc/>
    protected override bool TabNavigatesChildren => true;

    // Roomy when there is room, compact when there is not. The alternative is what the spaced layout does on its
    // own in a short terminal: the Inspector — and with it Delete and Clear — is quietly clipped off the bottom,
    // which reads as the panel not existing rather than as the window being small.
    //
    // Guarded on the mode actually changing, not on the height: this runs on every re-layout, and calling
    // SetContent from inside one that hasn't changed anything is how a layout starts chasing its own tail.
    /// <inheritdoc/>
    protected override void Control_OnInitialization()
    {
        base.Control_OnInitialization();
        var wanted = ActualHeight >= SpacedRows;
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
        return new VerticalStackPanel(
            new Section("Scene", new VerticalStackPanel(status, counts, clock), 3),
            new Section("Render", Stack(spaced, renderer, edges, wrapLighting), 3 + (2 * gap)),
            new Section("Spawn", Stack(spaced, shape, mesh, size, speed, Row(drop, fire)), 5 + (4 * gap)),
            new Section("World", Stack(spaced, gravity, friction, bounce, drag, timeScale), 5 + (4 * gap)),
            // Hand-built rather than Stack'd: its first two rows are one readout, so the gap belongs only before
            // the buttons.
            new Section("Inspector", spaced
                ? new VerticalStackPanel(selection, motion, Spacer(), Row(delete, clear))
                : new VerticalStackPanel(selection, motion, Row(delete, clear)), 3 + gap),
            // Never spaced: the pad's buttons are one control, and a gap through the middle of a D-pad reads as two.
            new Section("Camera", new VerticalStackPanel(camera), CameraPad.Rows));
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

    private static ILayout Row(Button left, Button right) =>
        new Grid([1], [Columns / 2 - 2, Columns / 2 - 2], [[left, right]]);

    private static Button Action(string text) =>
        new Button(text) { Style = ButtonStyle.Secondary with { MinWidth = Columns / 2 - 3 } };


    private static Slider Param(string label, float min, float max, float value, string format = "F2") =>
        new Slider(min, max, value, label) { LabelWidth = 8, ValueFormat = format };
    #endregion

    #region Fields
    private readonly SceneView view;
    private readonly SandboxParameters parameters;

    private readonly TextLabel status = Line("RUN", HeadingColor);
    private readonly TextLabel counts = Line("", TextColor);
    private readonly TextLabel clock = Line("", MutedColor);

    private readonly Select renderer;
    private readonly Select edges = new Select("none", "ink", "glyph") { FitContent = true };
    private readonly Switch wrapLighting = new Switch("Wrap lighting", isOn: true);

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

    private readonly TextLabel selection = Line("no selection", TextColor);
    private readonly TextLabel motion = Line("", MutedColor);
    private readonly Button delete = Action("Delete");
    private readonly Button clear = Action("Clear");

    private readonly CameraPad camera;

    private SceneSnapshot? drawn;
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

        protected override bool TabNavigatesChildren => true;
    }
    #endregion
}
