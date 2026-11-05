namespace Jumbee.Console.Mesh;

/// <summary>The sections a <see cref="MeshInspector"/> shows.</summary>
[Flags]
public enum InspectorSections
{
    /// <summary>No sections.</summary>
    None = 0,

    /// <summary>The model's name and size, its up axis, and previous / next. Needs a <see cref="MeshView.Model"/>.</summary>
    Model = 1,

    /// <summary>The renderer, plus the model's colour and turntable when there is a model.</summary>
    Render = 2,

    /// <summary>The shaded renderer's settings: texture (with a model), edges, quadrant glyphs, lighting, occlusion,
    /// and the shade and texel ramps.</summary>
    ShadedDetail = 4,

    /// <summary>The wireframe's mesh sampling: stratification, detail and the scan cap.</summary>
    WireframeDetail = 8,

    /// <summary>Per-axis and uniform scale sliders. Needs a <see cref="MeshView.Model"/>.</summary>
    Scale = 16,

    /// <summary>Shear sliders. Needs a <see cref="MeshView.Model"/>.</summary>
    Shear = 32,

    /// <summary>A <see cref="CameraPad"/> for the view's camera.</summary>
    Camera = 64,

    /// <summary>Every section.</summary>
    All = Model | Render | ShadedDetail | WireframeDetail | Scale | Shear | Camera,
}

/// <summary>
/// A sidebar of stock controls bound to a <see cref="MeshView"/>: the renderer and its settings, the camera and — over
/// a <see cref="ModelScene"/> — the model, its scale and its shear.
/// </summary>
/// <remarks>
/// <para>
/// <b>Keys and widgets stay in agreement</b> because neither talks to the other. The view and its model own the truth;
/// a widget writes to them, and <see cref="Refresh"/> reads every widget back. It runs on
/// <see cref="MeshView.SettingsChanged"/>, after each widget edit, and — over a model, which raises no event for a
/// transform key — once per drawn frame. A guard stops the round trip from looping: while the inspector is writing
/// into a widget, that widget's own change event does nothing.
/// </para>
/// <para>
/// A control that applies to one renderer is greyed out under the others rather than hidden, so the layout never
/// jumps and the section's title says why. It is <see cref="IScrollable"/>: put it in a borderless frame and it
/// scrolls when the terminal is too short for it. To embed it among sections of your own, set its
/// <see cref="Control.Height"/> to <see cref="MeasureHeight"/>.
/// </para>
/// </remarks>
public sealed class MeshInspector : CompositeControl, IScrollable
{
    #region Constructors
    /// <summary>Builds an inspector over <paramref name="view"/> showing <paramref name="sections"/>. The model
    /// sections and controls appear only when the view has a <see cref="MeshView.Model"/>.</summary>
    public MeshInspector(MeshView view, InspectorSections sections = InspectorSections.All)
    {
        this.view = view;
        model = view.Model;
        Sections = sections;
        Width = Columns;

        renderer = new Select([.. view.Renderers.Select(r => r.Name)]) { SelectedIndex = 0, FitContent = true };
        renderer.SelectionChanged += (_, _) => Push(() => view.SetRenderer(view.Renderers[renderer.SelectedIndex]));
        edges.SelectionChanged += (_, _) => Push(() => view.SetEdgeStyle((SilhouetteStyle)edges.SelectedIndex));
        texture.SelectionChanged += (_, _) => Push(() => view.SetTexture(TextureChoices[texture.SelectedIndex]));
        wrapLighting.Changed += (_, on) => Push(() => view.SetWrapLighting(on));
        occlusion.ValueChanged += (_, v) => Push(() => view.SetOcclusionStrength((float)v));
        shade.ValueChanged += (_, v) => Push(() => view.SetShadeLevels((float)v));
        texels.ValueChanged += (_, v) => Push(() => view.SetTextureLevels((float)v));
        quadrants.Changed += (_, on) => Push(() => view.SetQuadrantSampling(on));
        stratify.Changed += (_, on) => Push(() => view.SetStratify(on));
        scanCap.SelectionChanged += (_, _) =>
            Push(() => view.SetScanCap(WireframeRenderer.ScanCapChoices[scanCap.SelectedIndex].Value));
        density.ValueChanged += (_, v) => Push(() => view.SetMeshDensity(WireframeRenderer.SubPixelsFromDetail((float)v)));

        if (model is { } m)
        {
            spin.Changed += (_, on) => Push(() => m.SpinRate = on ? DefaultSpin : 0f);
            zUp.Changed += (_, on) => Push(() => m.UpAxis = on ? ModelUpAxis.Z : ModelUpAxis.Y);
            color.SelectionChanged += (_, _) => Push(() => m.ColorKey = color.SelectedIndex);

            // The master slider drives all three axes to its own value, so it also pulls apart a non-uniform scale.
            scaleAll.ValueChanged += (_, v) => Push(() => m.SetScaleAxis(-1, (float)v));
            scaleX.ValueChanged += (_, v) => Push(() => m.SetScaleAxis(0, (float)v));
            scaleY.ValueChanged += (_, v) => Push(() => m.SetScaleAxis(1, (float)v));
            scaleZ.ValueChanged += (_, v) => Push(() => m.SetScaleAxis(2, (float)v));
            shearX.ValueChanged += (_, v) => Push(() => m.SetShear((float)v, m.Shear.Y));
            shearZ.ValueChanged += (_, v) => Push(() => m.SetShear(m.Shear.X, (float)v));

            previous.Activated += (_, _) => m.Step(-1);
            next.Activated += (_, _) => m.Step(+1);

            // Each section's Reset undoes that section and nothing else — a button that also cleared the section
            // below it would be a trap. The whole-transform reset is the view's 0 key.
            resetScale.Activated += (_, _) => Push(() => m.SetScaleAxis(-1, 1f));
            resetShear.Activated += (_, _) => Push(() => m.SetShear(0f, 0f));
        }

        camera = new CameraPad(view.Camera, () => UI.SetFocus(view));

        view.SettingsChanged += Refresh;
        // A model has no change event, and its keys mutate it directly: polling it once per drawn frame is simpler
        // than threading an event through every transform method, and costs nothing at this size.
        if (model is not null) view.Drew += OnDrew;

        Rebuild();
        Refresh();
    }
    #endregion

    #region Properties
    /// <summary>The width the inspector is laid out for: a sidebar docked at this width fits it exactly.</summary>
    public const int Columns = 32;

    /// <summary>The sections shown.</summary>
    public InspectorSections Sections { get; }

    /// <summary>Whether every interactive control gets a blank row under it. On by default; turn it off to fit a
    /// short terminal. Readout lines stay flush either way.</summary>
    /// <remarks>Stacked flush, a column of drop-downs and sliders reads as one slab, and it is genuinely hard to tell
    /// which track belongs to which label — so spaced is the default and compact the fallback.</remarks>
    public bool Spaced
    {
        get => spaced;
        set
        {
            if (spaced == value) return;
            spaced = value;
            Rebuild();
        }
    }

    /// <summary>Whether <see cref="InspectorSections.ShadedDetail"/> and <see cref="InspectorSections.WireframeDetail"/>
    /// are folded into the Render section rather than given sections of their own. Off by default.</summary>
    /// <remarks>Each section costs two rows of frame, so folding saves four — the difference between a camera pad on
    /// screen and one scrolled off it in a 40-row terminal.</remarks>
    public bool FoldDetail
    {
        get => foldDetail;
        set
        {
            if (foldDetail == value) return;
            foldDetail = value;
            Rebuild();
        }
    }

    /// <summary>Which colour option the swatch drop-down is showing — so a test can assert the widget follows the
    /// model rather than the other way round.</summary>
    internal int SelectedColourIndex => color.SelectedIndex;

    // A form of many fields rather than a composite built around one editor, so Tab walks the widgets instead of
    // being handed to whichever one has focus.
    /// <inheritdoc/>
    protected override bool TabNavigatesChildren => true;
    #endregion

    #region Methods
    /// <summary>The stacked height of every section.</summary>
    /// <remarks>
    /// Summed from the sections rather than written down as a total, because a total goes stale the moment a section
    /// changes size — and under-reporting does not merely clip the tail, it collapses the last sections to zero
    /// height.
    /// </remarks>
    public int MeasureHeight(int width)
    {
        var rows = 0;
        foreach (var section in sections) rows += section.OuterRows;
        return rows;
    }

    /// <summary>Re-reads the view's and the model's state into every widget.</summary>
    public void Refresh()
    {
        if (model is { } m)
        {
            var mesh = m.Mesh;
            name.Text = m.Name;
            geometry.Text = $"{mesh.TriangleCount} tris · {mesh.Vertices.Length} verts";
        }

        syncing = true;
        try
        {
            renderer.SelectedIndex = IndexOfRenderer();
            edges.SelectedIndex = (int)(view.Edges ?? SilhouetteStyle.None);
            wrapLighting.IsChecked = view.WrapLighting ?? false;
            occlusion.Value = view.OcclusionStrength ?? 0f;
            // The shaded renderer's alone, so they grey out under the other two.
            edges.Enabled = wrapLighting.Enabled = occlusion.Enabled = view.OcclusionStrength is not null;
            // Both solid renderers have a ramp, so these grey out under the WIREFRAME instead -- the only renderer
            // that draws no shaded surface at all.
            shade.Value = view.ShadeLevels ?? ShadedRenderer.DefaultShadeLevels;
            quadrants.IsChecked = view.QuadrantSampling ?? false;
            shade.Enabled = quadrants.Enabled = view.ShadeLevels is not null;
            stratify.IsChecked = view.Stratify ?? false;
            scanCap.SelectedIndex = IndexOfScanCap();
            density.Value = WireframeRenderer.DetailFromSubPixels(view.MeshDensity ?? WireframeRenderer.DefaultSubPixelsPerTriangle);
            // Greyed out under a renderer that draws every triangle, rather than left live-but-inert: they still
            // report the wireframe's real settings, and Enabled is what says "not while this renderer is drawing".
            stratify.Enabled = scanCap.Enabled = density.Enabled = view.MeshDialsApply;

            // Texture greys out under the renderers that do not texture AND on a model with no materials -- most of
            // them -- because a control that visibly does nothing reads as broken. With no model (a multi-body scene)
            // there is no one mesh to judge by, so only the renderer decides. Texel Levels follows: only where a map
            // can actually be drawn.
            var hasMaterials = model is null || model.Mesh.Materials is not null;
            texture.SelectedIndex = Math.Max(0, Array.IndexOf(TextureChoices, view.Texture ?? TextureMode.Auto));
            texture.Enabled = view.Texture is not null && hasMaterials;
            texels.Value = view.TextureLevels ?? MeshRenderer.DefaultTextureLevels;
            texels.Enabled = view.Texture is { } mode && mode != TextureMode.None && hasMaterials;

            if (model is { } mm)
            {
                spin.IsChecked = mm.SpinRate != 0f;
                color.SelectedIndex = mm.ColorKey;
                // Colour greys out the other way: while materials are drawn the body colour is not used, and a
                // drop-down that changes nothing is exactly how the white-model bug presented. Off gives it back.
                color.Enabled = !(mm.Mesh.Materials is not null && (view.Texture ?? TextureMode.None) != TextureMode.None);
                zUp.IsChecked = mm.UpAxis == ModelUpAxis.Z;
                (scaleX.Value, scaleY.Value, scaleZ.Value) = (mm.Scale.X, mm.Scale.Y, mm.Scale.Z);
                // The master only follows a uniform scale (a reset, or the unqualified scale keys). Once the axes
                // disagree there is no value it could honestly show, so it keeps the last one it was set to — which
                // is what the next drag will flatten them all back to.
                if (mm.Scale.X == mm.Scale.Y && mm.Scale.Y == mm.Scale.Z) scaleAll.Value = mm.Scale.X;
                (shearX.Value, shearZ.Value) = (mm.Shear.X, mm.Shear.Y);
            }
        }
        finally
        {
            syncing = false;
        }
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        view.SettingsChanged -= Refresh;
        view.Drew -= OnDrew;
        base.Dispose();
    }
    #endregion

    #region Private methods
    private void OnDrew(SceneFrame frame) => Refresh();

    private bool Has(InspectorSections section) => (Sections & section) != 0;

    // The sections, in a fixed order. Rebuilt whole when the spacing or folding changes: the widgets are fields and
    // survive, only their frames and spacers are new.
    private void Rebuild()
    {
        var list = new List<Section>();
        var withModel = model is not null;

        if (withModel && Has(InspectorSections.Model))
            list.Add(MakeSection("Model", [name, geometry], zUp, Row(previous, next)));

        IFocusable[] render = withModel
            ? [Labelled("Renderer", renderer), Labelled("Colour", color), spin]
            : [Labelled("Renderer", renderer)];
        // Texture first: on a model with materials it changes the picture more than anything else here.
        IFocusable[] shaded = [.. withModel ? [Labelled("Texture", texture)] : Array.Empty<IFocusable>(),
            Labelled("Edges", edges), quadrants, wrapLighting, occlusion, shade, texels];
        // Detail before Scan: Detail changes what you see, Scan only caps how much of a large model is examined and
        // does nothing to a model below that cap.
        IFocusable[] wire = [stratify, density, Labelled("Scan", scanCap)];

        if (foldDetail && Has(InspectorSections.Render))
        {
            list.Add(MakeSection("Render", [], [.. render,
                .. Has(InspectorSections.ShadedDetail) ? shaded : [],
                .. Has(InspectorSections.WireframeDetail) ? wire : []]));
        }
        else
        {
            if (Has(InspectorSections.Render)) list.Add(MakeSection("Render", [], render));
            // Named for the renderer each belongs to, because it is greyed out under the others and a title saying so
            // is cheaper than working out why.
            if (Has(InspectorSections.ShadedDetail)) list.Add(MakeSection("Shaded detail", [], shaded));
            if (Has(InspectorSections.WireframeDetail)) list.Add(MakeSection("Wireframe mesh detail", [], wire));
        }

        if (withModel && Has(InspectorSections.Scale))
            list.Add(MakeSection("Scale", [], scaleAll, scaleX, scaleY, scaleZ, Row(resetScale, null)));
        if (withModel && Has(InspectorSections.Shear))
            list.Add(MakeSection("Shear", [], shearX, shearZ, Row(resetShear, null)));
        // Never spaced: the pad's buttons are one control, and a gap through the middle of a D-pad reads as two.
        if (Has(InspectorSections.Camera))
            list.Add(new Section("Camera", new VerticalStackPanel(camera), CameraPad.Rows));

        sections = [.. list];
        SetContent(new VerticalStackPanel([.. sections]));
    }

    // A section of readout lines, stacked flush, then interactive items with a blank row before each when spaced —
    // so a section never ends on a gap and a block of text is never split.
    private Section MakeSection(string title, IFocusable[] readouts, params IFocusable[] items)
    {
        var rows = new List<IFocusable>(readouts);
        for (var i = 0; i < items.Length; i++)
        {
            if (spaced && rows.Count > 0) rows.Add(Spacer());
            rows.Add(items[i]);
        }

        return new Section(title, new VerticalStackPanel([.. rows]), rows.Count);
    }

    private void Push(Action apply)
    {
        if (syncing) return;
        apply();
        Refresh();
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

    // A caption beside a control, in the same column the sliders put theirs.
    //
    // A Grid, NOT a HorizontalStackPanel, and the difference is a trap worth knowing: a stack asks each child in
    // turn how wide it wants to be and offers it everything still unclaimed. A TextLabel's answer is "all of it"
    // (IntrinsicWidth 0 means fill), so it takes the whole row and every later child is laid out at zero width and
    // never appears. Columns of fixed width are a Grid's job.
    private static ILayout Labelled(string caption, Control control) =>
        new Grid([1], [LabelColumn, Columns - 2 - LabelColumn], [[Line(caption, MutedColor), control]]);

    // A single button still gets the two-column grid so it keeps the width of a paired one; the second cell is a
    // blank spacer rather than a stretched button.
    private static ILayout Row(Button left, Button? right) =>
        new Grid([1], [Half, Half], [[left, right ?? (Control)Spacer()]]);

    private static Button Action(string text) =>
        new Button(text) { Style = ButtonStyle.Secondary with { MinWidth = Half - 1 } };

    // LabelWidth cells for every slider, so their tracks line up down the panel. It is EXACT, not a minimum: a longer
    // label is ellipsized rather than pushing its own track right, so widening one label widens all.
    private static Slider Param(string label, float min, float max, float value, string format = "F2") =>
        new Slider(min, max, value, label) { LabelWidth = LabelWidth, ValueFormat = format };
    #endregion

    #region Fields
    private const int Half = Columns / 2 - 2;

    // The label column, shared by the sliders and by Labelled()'s captions so everything lines up down the whole
    // sidebar. 12 because of "Shade Levels", the longest label here. The caption column is one WIDER, for the gutter
    // cell Slider.BuildLabel appends before the track.
    private const int LabelWidth = 12;
    private const int LabelColumn = LabelWidth + 1;
    private const float DefaultSpin = 0.35f;

    private static readonly TextureMode[] TextureChoices = [TextureMode.None, TextureMode.Auto, TextureMode.Image];
    private static readonly Color HeadingColor = new(200, 205, 215);
    private static readonly Color MutedColor = new(130, 136, 150);
    private static readonly Color BorderColor = new(70, 78, 96);

    private readonly MeshView view;
    private readonly ModelScene? model;

    private readonly TextLabel name = Line("", HeadingColor);
    private readonly TextLabel geometry = Line("", MutedColor);
    // OBJ does not record which way is up, so it has to be told: a model exported from 3ds Max or CAD is Z-up and
    // stands on its nose until this is flipped.
    private readonly Switch zUp = new Switch("Z-up file");
    private readonly Button previous = Action("◀ Prev");
    private readonly Button next = Action("Next ▶");

    private readonly Select renderer;
    private readonly Switch spin = new Switch("Turntable", isOn: true);

    // Renderable options rather than text: the row is a swatch in the colour itself plus its name, which needs two
    // styles on one row — a text option gets one, and the closed control does not parse markup.
    private readonly Select color = new Select([.. Palette.Named.Select(ColorOption)]) { FitContent = true };

    // off / auto / on. Auto is the default: it draws a map only where the gate says it reads as an image, and a flat
    // material colour where it would read as noise. On draws every map; Off draws no materials at all.
    private readonly Select texture = new Select("off", "auto", "on") { FitContent = true };
    private readonly Select edges = new Select("none", "line", "glyph") { FitContent = true };

    // Half-cell horizontal resolution on every boundary: the surface samples twice per column and each 2x2 block
    // becomes the quadrant glyph that best fits it. Named for the mechanism rather than the effect -- nothing is
    // blended, so it is not antialiasing. First of the switches: it moves the picture more than anything else here.
    //
    // There was a second control beside it, a Smooth slider blending detected edge sub-pixels -- the one thing that
    // WAS antialiasing. It went once this existed: measured, it made silhouette placement slightly WORSE while
    // costing 43% more distinct fg/bg pairs, and two controls where one does nothing you can see is worse than one.
    private readonly Switch quadrants = new Switch("Quadrant glyphs");
    private readonly Switch wrapLighting = new Switch("Half-Lambert light");

    // Screen-space ambient occlusion: it samples a depth ring per sub-pixel and darkens by how much of that ring sits
    // nearer than a flat surface would.
    private readonly Slider occlusion = Param("Occlusion", 0f, 1f, ShadedRenderer.DefaultOcclusionStrength);

    // How many brightness bands a curved surface shows: the quality dial that moves the picture most at this
    // resolution, and the renderer's largest performance lever.
    private readonly Slider shade = Param("Shade Levels", MeshRenderer.MinShadeLevels, MeshRenderer.MaxShadeLevels,
        ShadedRenderer.DefaultShadeLevels, "F0");

    // The texture's own ramp, levels per channel. Shade Levels never touches a texel, so without this a map's
    // gradients band at a fixed step however fine the lighting is.
    private readonly Slider texels = Param("Texel Levels", MeshRenderer.MinTextureLevels, MeshRenderer.MaxTextureLevels,
        MeshRenderer.DefaultTextureLevels, "F0");

    private readonly Switch stratify = new Switch("Even over screen", isOn: true);
    private readonly Select scanCap =
        new Select([.. WireframeRenderer.ScanCapChoices.Select(c => c.Label)]) { FitContent = true };

    // Labelled by what it DOES rather than by its unit: the renderer's value is sub-pixels per triangle, so smaller is
    // denser, and a slider that draws less as you push it right is a small cruelty. Inverted at the boundary instead.
    private readonly Slider density = Param("Detail", WireframeRenderer.MinDetail, WireframeRenderer.MaxDetail,
        WireframeRenderer.DetailFromSubPixels(WireframeRenderer.DefaultSubPixelsPerTriangle), "F1");

    private readonly Slider scaleAll = Param("Scale All", ModelScene.MinScale, ModelScene.MaxScale, 1f);
    private readonly Slider scaleX = Param("Scale X", ModelScene.MinScale, ModelScene.MaxScale, 1f);
    private readonly Slider scaleY = Param("Scale Y", ModelScene.MinScale, ModelScene.MaxScale, 1f);
    private readonly Slider scaleZ = Param("Scale Z", ModelScene.MinScale, ModelScene.MaxScale, 1f);
    private readonly Button resetScale = Action("Reset");

    private readonly Slider shearX = Param("Shear X", -ModelScene.MaxShear, ModelScene.MaxShear, 0f);
    private readonly Slider shearZ = Param("Shear Z", -ModelScene.MaxShear, ModelScene.MaxShear, 0f);
    private readonly Button resetShear = Action("Reset");

    private readonly CameraPad camera;
    private Section[] sections = [];
    private bool spaced = true;
    private bool foldDetail;
    private bool syncing;
    #endregion

    #region Child types
    // One titled block of the stack. A framed VerticalStackPanel is not itself a Control, so each section is a small
    // composite that can carry the frame and the title.
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
