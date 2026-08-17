namespace Jumbee.Console.SandboxDemo;

using System.Numerics;

/// <summary>
/// The model viewer's sidebar: which asset is showing, its triangle count, and sliders for the per-axis scale and
/// shear the keyboard drives with <c>xyz</c>/<c>XYZ</c> and <c>,.</c>/<c>;'</c>.
/// </summary>
/// <remarks>
/// Same two-way arrangement as <see cref="SidebarPanel"/> and for the same reason — see the notes there. The
/// difference is that <see cref="ModelScene"/> has no change event, so this refreshes from the frame clock instead:
/// a static scene the keys mutate directly, polled once per drawn frame, is simpler than threading an event through
/// four transform methods and costs nothing at this size.
/// </remarks>
public sealed class ModelSidebarPanel : CompositeControl, Jumbee.Console.IScrollable
{
    #region Constructors
    /// <summary>Builds the viewer sidebar over <paramref name="view"/> and the <paramref name="model"/> it shows.</summary>
    public ModelSidebarPanel(SceneView view, ModelScene model)
    {
        this.view = view;
        this.model = model;
        Width = SidebarPanel.Columns;

        renderer = new Select([.. view.Renderers.Select(r => r.Name)]) { SelectedIndex = 0, FitContent = true };
        renderer.SelectionChanged += (_, _) => Push(() => view.SetRenderer(view.Renderers[renderer.SelectedIndex]));
        edges.SelectionChanged += (_, _) => Push(() => view.SetEdgeStyle((SilhouetteStyle)edges.SelectedIndex));
        spin.Changed += (_, on) => Push(() => model.SpinRate = on ? DefaultSpin : 0f);
        zUp.Changed += (_, on) => Push(() => model.UpAxis = on ? ModelUpAxis.Z : ModelUpAxis.Y);
        color.SelectionChanged += (_, _) => Push(() => model.ColorKey = color.SelectedIndex);

        // The shaded renderer's two lighting dials. Wrap was reachable only from the sandbox until now, which was
        // the wrong way round: the viewer is where you compare renderers on one model, and wrap is the setting that
        // explains most of why the shaded one looks softer than the flat one.
        wrapLighting.Changed += (_, on) => Push(() => view.SetWrapLighting(on));
        occlusion.ValueChanged += (_, v) => Push(() => view.SetOcclusionStrength((float)v));

        // The wireframe's mesh sampling. This panel is where they matter most -- the viewer is the scene that shows
        // ONE loaded model filling the frame, which is exactly the case the thinning has to get right.
        stratify.Changed += (_, on) => Push(() => view.SetStratify(on));
        scanCap.SelectionChanged += (_, _) =>
            Push(() => view.SetScanCap(WireframeRenderer.ScanCapChoices[scanCap.SelectedIndex].Value));
        density.ValueChanged += (_, v) => Push(() => view.SetMeshDensity(WireframeRenderer.SubPixelsFromDetail((float)v)));

        // The master slider drives all three axes to its own value, so it also pulls apart a non-uniform scale.
        scaleAll.ValueChanged += (_, v) => Push(() => model.SetScaleAxis(-1, (float)v));
        scaleX.ValueChanged += (_, v) => Push(() => model.SetScaleAxis(0, (float)v));
        scaleY.ValueChanged += (_, v) => Push(() => model.SetScaleAxis(1, (float)v));
        scaleZ.ValueChanged += (_, v) => Push(() => model.SetScaleAxis(2, (float)v));
        shearX.ValueChanged += (_, v) => Push(() => model.SetShear((float)v, model.Shear.Y));
        shearZ.ValueChanged += (_, v) => Push(() => model.SetShear(model.Shear.X, (float)v));

        previous.Activated += (_, _) => model.Step(-1);
        next.Activated += (_, _) => model.Step(+1);

        // Each section's Reset undoes that section and nothing else, now that there are two of them — a button that
        // also cleared the panel below it would be a trap. The whole-transform reset is still Model ▸ Reset transform
        // and the 0 key.
        resetScale.Activated += (_, _) => Push(() => model.SetScaleAxis(-1, 1f));
        resetShear.Activated += (_, _) => Push(() => model.SetShear(0f, 0f));

        // The same pad the sandbox sidebar carries — it is the same camera, and having it in one scene and not the
        // other would be the odd choice.
        camera = new CameraPad(view.Camera, () => UI.SetFocus(view));

        view.RendererChanged += Report;

        // A blank row under every interactive control, as in SidebarPanel and for the same reason; the model's two
        // readout lines stay flush.
        sections =
        [
            new Section("Model", new VerticalStackPanel(name, geometry, Spacer(), zUp, Spacer(), Row(previous, next)), 6),
            // Render keeps only what applies to ALL of them; everything that belongs to one renderer is grouped
            // under that renderer's name below, so a greyed-out control is self-explanatory.
            new Section("Render", Spaced(Labelled("Renderer", renderer), Labelled("Colour", color), spin), 5),
            // Edges lived in Render, which put a shaded-only control among the general ones. All three of these are
            // the shaded renderer's, so they belong together.
            new Section("Shaded detail", Spaced(Labelled("Edges", edges), wrapLighting, occlusion), 5),
            // Its own section here, where the sandbox folds these into Render: this panel is IScrollable and
            // MeasureHeight is summed from the sections, so an extra one scrolls rather than clipping the ones
            // below it. Named for the renderer it belongs to, because it is greyed out under the other two and a
            // title saying so is cheaper than working out why.
            //
            // Detail sits above Scan because it is the one that changes what you see: Scan only sets a ceiling on
            // how much of a large model is looked at, and does nothing at all to a model below that ceiling.
            new Section("Wireframe mesh detail", Spaced(stratify, density, Labelled("Scan", scanCap)), 5),
            new Section("Scale", Spaced(scaleAll, scaleX, scaleY, scaleZ, Row(resetScale, null)), 9),
            new Section("Shear", Spaced(shearX, shearZ, Row(resetShear, null)), 5),
            new Section("Camera", new VerticalStackPanel(camera), CameraPad.Rows),
        ];
        SetContent(new VerticalStackPanel([.. sections]));

        Report();
    }
    #endregion

    #region Properties
    /// <inheritdoc/>
    protected override bool TabNavigatesChildren => true;
    #endregion

    #region Methods
    /// <summary>Which colour option the swatch drop-down is showing. For tests: asserts the widget follows the
    /// model rather than the other way round.</summary>
    internal int SelectedColourIndex => color.SelectedIndex;

    /// <summary>The stacked height of every section, so an enclosing frame scrolls the panel when the terminal is
    /// too short for it.</summary>
    /// <remarks>
    /// Summed from the sections rather than written down as a total, because a total goes stale the moment a section
    /// changes size — and under-reporting does not merely clip the tail, it collapses the last sections to zero
    /// height. Nothing here reports which row the focused control is on: the frame works that out itself.
    /// </remarks>
    public int MeasureHeight(int width)
    {
        var rows = 0;
        foreach (var section in sections) rows += section.OuterRows;
        return rows;
    }

    /// <summary>Re-reads the model's state into the widgets. Called once per drawn frame.</summary>
    public void Report()
    {
        var mesh = model.Mesh;
        name.Text = model.Name;
        geometry.Text = $"{mesh.TriangleCount} tris · {mesh.Vertices.Length} verts";

        syncing = true;
        try
        {
            renderer.SelectedIndex = IndexOfRenderer();
            edges.SelectedIndex = (int)(view.Edges ?? SilhouetteStyle.None);
            stratify.IsChecked = view.Stratify ?? false;
            scanCap.SelectedIndex = IndexOfScanCap();
            density.Value = WireframeRenderer.DetailFromSubPixels(
                view.MeshDensity ?? WireframeRenderer.DefaultSubPixelsPerTriangle);
            // Greyed out under a renderer that draws every triangle. They keep reporting the wireframe's real
            // settings; Enabled is what says "not while this renderer is drawing".
            stratify.Enabled = scanCap.Enabled = density.Enabled = view.MeshDialsApply;
            spin.IsChecked = model.SpinRate != 0f;
            color.SelectedIndex = model.ColorKey;
            wrapLighting.IsChecked = view.WrapLighting ?? false;
            occlusion.Value = view.OcclusionStrength ?? 0f;
            edges.Enabled = wrapLighting.Enabled = occlusion.Enabled = view.OcclusionStrength is not null;
            zUp.IsChecked = model.UpAxis == ModelUpAxis.Z;
            (scaleX.Value, scaleY.Value, scaleZ.Value) = (model.Scale.X, model.Scale.Y, model.Scale.Z);
            // The master only follows a uniform scale (a reset, or the unqualified scale keys). Once the axes
            // disagree there is no value it could honestly show, so it keeps the last one it was set to — which is
            // what the next drag will flatten them all back to.
            if (model.Scale.X == model.Scale.Y && model.Scale.Y == model.Scale.Z) scaleAll.Value = model.Scale.X;
            (shearX.Value, shearZ.Value) = (model.Shear.X, model.Shear.Y);
        }
        finally
        {
            syncing = false;
        }
    }
    #endregion

    #region Private methods
    private void Push(Action apply)
    {
        if (syncing) return;
        apply();
        Report();
    }

    private int IndexOfRenderer()
    {
        for (var i = 0; i < view.Renderers.Count; i++)
            if (ReferenceEquals(view.Renderers[i], view.Renderer)) return i;
        return 0;
    }

    // Parks on the default's slot rather than 0 when the active renderer has no scan cap, so switching to shaded
    // does not leave the drop-down showing "5k" and then apply that on the way back.
    private int IndexOfScanCap()
    {
        var cap = view.ScanCap ?? WireframeRenderer.DefaultScanCap;
        for (var i = 0; i < WireframeRenderer.ScanCapChoices.Length; i++)
            if (WireframeRenderer.ScanCapChoices[i].Value == cap) return i;
        return Array.FindIndex(WireframeRenderer.ScanCapChoices, c => c.Value == WireframeRenderer.DefaultScanCap);
    }

    // A block in the colour itself, then its name — the swatch reads faster than any name does, and having both
    // means the drop-down is still usable where the two hues are close.
    private static SelectOption ColorOption((string Name, Color Color) entry) =>
        new(new Spectre.Console.Markup(
            $"[#{entry.Color.R:x2}{entry.Color.G:x2}{entry.Color.B:x2}]███[/] {entry.Name}"))
        { Tag = entry.Color };

    private static TextLabel Line(string text, Color color) =>
        new(TextLabelOrientation.Horizontal, text, color) { Focusable = false, Height = 1 };

    private static TextLabel Spacer() => Line("", MutedColor);

    // The items with a blank row between each — one fewer spacer than items, so a section never ends on a gap.
    private static ILayout Spaced(params IFocusable[] items)
    {
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
        new Grid([1], [LabelColumn, SidebarPanel.Columns - 2 - LabelColumn],
            [[Line(caption, MutedColor), control]]);

    // A single button still gets the two-column grid so it keeps the width of a paired one; the second cell is a
    // blank spacer rather than a stretched button.
    private static ILayout Row(Button left, Button? right) =>
        new Grid([1], [Half, Half], [[left, right ?? (Control)Line("", MutedColor)]]);

    private static Button Action(string text) =>
        new Button(text) { Style = ButtonStyle.Secondary with { MinWidth = Half - 1 } };


    // 9 cells, the width of the longest caption ("Scale All"), so nothing is ellipsized and every track starts in
    // the same column.
    private static Slider Axis(string label, float min, float max, float value) =>
        new Slider(min, max, value, label) { LabelWidth = 9 };
    #endregion

    #region Fields
    private const int Half = SidebarPanel.Columns / 2 - 2;

    // Matches the sliders' LabelWidth plus their gap, so captions line up down the whole sidebar.
    private const int LabelColumn = 10;
    private const float DefaultSpin = 0.35f;

    private readonly SceneView view;
    private readonly ModelScene model;

    private readonly TextLabel name = Line("", HeadingColor);
    private readonly TextLabel geometry = Line("", MutedColor);
    // OBJ does not record which way is up, so it has to be told: a model exported from 3ds Max or CAD is Z-up and
    // stands on its nose until this is flipped.
    private readonly Switch zUp = new Switch("Z-up file");
    private readonly Button previous = Action("◀ Prev");
    private readonly Button next = Action("Next ▶");

    private readonly Select renderer;
    private readonly Select edges = new Select("none", "line", "glyph") { FitContent = true };
    private readonly Switch spin = new Switch("Turntable", isOn: true);

    // Renderable options rather than text: the row is a swatch in the colour itself plus its name, which needs two
    // styles on one row — a text option gets one, and the closed control does not parse markup.
    private readonly Select color = new Select([.. Palette.Named.Select(ColorOption)]) { FitContent = true };

    private readonly Switch wrapLighting = new Switch("Half-Lambert light");
    private readonly Slider occlusion = Axis("Occlusion", 0f, 1f, ShadedRenderer.DefaultOcclusionStrength);

    private readonly Switch stratify = new Switch("Even over screen", isOn: true);
    private readonly Select scanCap =
        new Select([.. WireframeRenderer.ScanCapChoices.Select(c => c.Label)]) { FitContent = true };
    private readonly Slider density = Axis("Detail", WireframeRenderer.MinDetail, WireframeRenderer.MaxDetail,
        WireframeRenderer.DetailFromSubPixels(WireframeRenderer.DefaultSubPixelsPerTriangle));

    private readonly Slider scaleAll = Axis("Scale All", ModelScene.MinScale, ModelScene.MaxScale, 1f);
    private readonly Slider scaleX = Axis("Scale X", ModelScene.MinScale, ModelScene.MaxScale, 1f);
    private readonly Slider scaleY = Axis("Scale Y", ModelScene.MinScale, ModelScene.MaxScale, 1f);
    private readonly Slider scaleZ = Axis("Scale Z", ModelScene.MinScale, ModelScene.MaxScale, 1f);
    private readonly Button resetScale = Action("Reset");

    private readonly Slider shearX = Axis("Shear X", -ModelScene.MaxShear, ModelScene.MaxShear, 0f);
    private readonly Slider shearZ = Axis("Shear Z", -ModelScene.MaxShear, ModelScene.MaxShear, 0f);
    private readonly Button resetShear = Action("Reset");

    private readonly CameraPad camera;
    private readonly Section[] sections;

    private bool syncing;

    private static readonly Color HeadingColor = new(200, 205, 215);
    private static readonly Color MutedColor = new(130, 136, 150);
    private static readonly Color BorderColor = new(70, 78, 96);
    #endregion

    #region Child types
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
