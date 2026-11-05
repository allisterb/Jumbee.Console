namespace Jumbee.Console.SandboxDemo;

using Jumbee.Console.Mesh;

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
/// (<see cref="SpawnSettings"/>, <see cref="SandboxParameters"/>, <see cref="SandboxView"/>) own the truth and raise a
/// change event; a widget writes to the state, and <see cref="Refresh"/> reads it back. The <see cref="syncing"/>
/// guard is what stops the round trip from looping: while the panel is pushing state into a widget, that widget's
/// own change event does nothing.
/// </para>
/// <para>
/// The readouts update from <see cref="MeshView.Drew"/> — the frame that was actually <em>drawn</em>, not the
/// newest one — so the numbers always describe the picture beside them.
/// </para>
/// <para>
/// The Render section is the library's <see cref="MeshInspector"/>, folded into one section and following this
/// panel's spacing; the camera pad is the library's <see cref="CameraPad"/>. Everything else here is the sandbox's.
/// </para>
/// </remarks>
public sealed class SidebarPanel : CompositeControl, Jumbee.Console.IScrollable
{
    #region Constructors
    /// <summary>Builds the sidebar over <paramref name="view"/> and its <paramref name="parameters"/>.
    /// <paramref name="resetScene"/> is what the <c>r</c> key and the menu's <c>Reset scene</c> do — repopulating the
    /// world is the shell's job, not the panel's, so it arrives as an action.</summary>
    public SidebarPanel(SandboxView view, SandboxParameters parameters, Action resetScene)
    {
        this.view = view;
        this.parameters = parameters;
        Width = Columns;

        BuildSpawnMeshOptions();

        // The renderer and its settings, as one folded section. It binds itself to the view, so the keys and its
        // widgets agree without this panel in the loop.
        render = new MeshInspector(view, InspectorSections.Render | InspectorSections.ShadedDetail |
                                         InspectorSections.WireframeDetail) { FoldDetail = true };

        // Widget -> state. Each of these is the only place a widget writes, and every one is inert while syncing.
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
        view.SelectionChanged += _ => Refresh();
        view.Spawn.Changed += Refresh;
        parameters.Changed += Refresh;

        SetContent(BuildSections(spaced: true));
        Refresh();
    }
    #endregion

    #region Properties
    /// <summary>The width the sidebar occupies when docked: the inspector's, so its Render section fits.</summary>
    public const int Columns = MeshInspector.Columns;

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
    /// <summary>Updates the readouts from the frame that was just drawn. Wired to <see cref="MeshView.Drew"/>.</summary>
    public void Report(PhysicsFrame snapshot, bool paused)
    {
        drawn = snapshot;
        var edgeName = view.Edges is { } e and not SilhouetteStyle.None ? "/" + e.ToString().ToLowerInvariant() : "";
        status.Text = $"{(paused ? "PAUSED" : "RUN")}  {view.Renderer.Name}{edgeName}";
        counts.Text = $"{snapshot.Count} bodies · {snapshot.AwakeCount} awake";
        clock.Text = $"t={snapshot.SimTime:F1}s  step {snapshot.StepMilliseconds:F2}ms";
        RefreshInspector();
    }

    /// <summary>Rebuilds the mesh drop-down from the spawn library — after a model has been loaded at runtime.</summary>
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
        var rows = render.MeasureHeight(width);
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
            shape.SelectedIndex = (int)spawn.Shape;
            mesh.SelectedIndex = spawn.MeshIndex >= 0 ? spawn.MeshIndex : 0;
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
        var count = view.Spawn.Library.Count;
        if (count == 0) return;
        view.Spawn.MeshIndex = Math.Clamp(mesh.SelectedIndex, 0, count - 1);
        view.Spawn.Shape = BodyShape.Mesh;
    }

    // Read from the library rather than handed a fixed list: models can be loaded while the app is running, and
    // RefreshMeshes puts the new ones in the drop-down without rebuilding the panel.
    private string[] MeshNames() => view.Spawn.Library.Count == 0 ? ["(none)"] : view.Spawn.Library.Names();

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

    // The panels, either spaced or compact. Every INTERACTIVE control gets a blank row under it in the spaced
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

        // The render inspector goes second, after Scene. It is folded into ONE section because a section costs two
        // rows of frame: split out, a 40-row terminal clipped the camera pad off the bottom even when compact.
        render.Spaced = spaced;
        render.Height = render.MeasureHeight(Columns);
        return new VerticalStackPanel([sections[0], render, .. sections[1..]]);
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
        // LabelWidth cells for every slider, so their tracks line up down the panel. It is EXACT, not a minimum:
        // a longer label is ellipsized rather than pushing its own track right, so widening one label widens all.
        new Slider(min, max, value, label) { LabelWidth = LabelWidth, ValueFormat = format };

    #endregion

    #region Fields
    private readonly SandboxView view;
    private readonly SandboxParameters parameters;

    private readonly TextLabel status = Line("RUN", HeadingColor);
    private readonly TextLabel counts = Line("", TextColor);
    private readonly TextLabel clock = Line("", MutedColor);
    private readonly Button clear = Action("Clear");
    private readonly Button reset = Action("Reset");

    // The Render section: the library's inspector, folded, following this panel's spacing.
    private readonly MeshInspector render;

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

    // The label column, shared by the sliders and by Labelled()'s captions so everything lines up down the whole
    // sidebar. 12 because of "Shade Levels", the longest label here; it was 9 while that read "Shades". The
    // caption column is one WIDER, because Slider.BuildLabel appends a gutter cell of its own before the track --
    // which this panel did not account for, so its captions sat a cell left of every track.
    private const int LabelWidth = 12;
    private const int LabelColumn = LabelWidth + 1;
    private PhysicsFrame? drawn;
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
