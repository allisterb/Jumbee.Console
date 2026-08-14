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
public sealed class ModelSidebarPanel : CompositeControl
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

        scaleX.ValueChanged += (_, v) => Push(() => model.SetScaleAxis(0, (float)v));
        scaleY.ValueChanged += (_, v) => Push(() => model.SetScaleAxis(1, (float)v));
        scaleZ.ValueChanged += (_, v) => Push(() => model.SetScaleAxis(2, (float)v));
        shearX.ValueChanged += (_, v) => Push(() => model.SetShear((float)v, model.Shear.Y));
        shearZ.ValueChanged += (_, v) => Push(() => model.SetShear(model.Shear.X, (float)v));

        previous.Activated += (_, _) => model.Step(-1);
        next.Activated += (_, _) => model.Step(+1);
        resetTransform.Activated += (_, _) => model.ResetTransform();

        // The same pad the sandbox sidebar carries — it is the same camera, and having it in one scene and not the
        // other would be the odd choice.
        camera = new CameraPad(view.Camera, () => UI.SetFocus(view));

        view.RendererChanged += Report;

        // A blank row under every interactive control, as in SidebarPanel and for the same reason; the model's two
        // readout lines stay flush.
        SetContent(new VerticalStackPanel(
            new Section("Model", new VerticalStackPanel(name, geometry, Spacer(), zUp, Spacer(), Row(previous, next)), 6),
            new Section("Render", Spaced(renderer, edges, spin), 5),
            new Section("Scale", Spaced(scaleX, scaleY, scaleZ), 5),
            new Section("Shear", Spaced(shearX, shearZ, Row(resetTransform, null)), 5),
            new Section("Camera", new VerticalStackPanel(camera), CameraPad.Rows)));

        Report();
    }
    #endregion

    #region Properties
    /// <inheritdoc/>
    protected override bool TabNavigatesChildren => true;
    #endregion

    #region Methods
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
            spin.IsChecked = model.SpinRate != 0f;
            zUp.IsChecked = model.UpAxis == ModelUpAxis.Z;
            (scaleX.Value, scaleY.Value, scaleZ.Value) = (model.Scale.X, model.Scale.Y, model.Scale.Z);
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

    // A single button still gets the two-column grid so it keeps the width of a paired one; the second cell is a
    // blank spacer rather than a stretched button.
    private static ILayout Row(Button left, Button? right) =>
        new Grid([1], [Half, Half], [[left, right ?? (Control)Line("", MutedColor)]]);

    private static Button Action(string text) =>
        new Button(text) { Style = ButtonStyle.Secondary with { MinWidth = Half - 1 } };


    private static Slider Axis(string label, float min, float max, float value) =>
        new Slider(min, max, value, label) { LabelWidth = 8 };
    #endregion

    #region Fields
    private const int Half = SidebarPanel.Columns / 2 - 2;
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
    private readonly Select edges = new Select("none", "ink", "glyph") { FitContent = true };
    private readonly Switch spin = new Switch("Turntable", isOn: true);

    private readonly Slider scaleX = Axis("Scale X", ModelScene.MinScale, ModelScene.MaxScale, 1f);
    private readonly Slider scaleY = Axis("Scale Y", ModelScene.MinScale, ModelScene.MaxScale, 1f);
    private readonly Slider scaleZ = Axis("Scale Z", ModelScene.MinScale, ModelScene.MaxScale, 1f);
    private readonly Slider shearX = Axis("Shear X", -ModelScene.MaxShear, ModelScene.MaxShear, 0f);
    private readonly Slider shearZ = Axis("Shear Z", -ModelScene.MaxShear, ModelScene.MaxShear, 0f);
    private readonly Button resetTransform = Action("Reset");

    private readonly CameraPad camera;

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

        protected override bool TabNavigatesChildren => true;
    }
    #endregion
}
