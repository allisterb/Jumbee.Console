#nullable enable

namespace Jumbee.Console.Wolf3DDemo;

/// <summary>
/// The Display tab: what a frame looks like, and what it costs to send.
/// </summary>
/// <remarks>
/// The cost readouts are in the footer rather than here — they are true of the app, not of this page, and the
/// footer is the one thing that cannot be hidden. Dragging Quantize and watching the footer's run count halve
/// while the picture barely changes is this demo's central finding in one gesture.
/// </remarks>
public sealed class DisplayPanel : CompositeControl
{
    #region Constructors
    /// <summary>Builds the page over <paramref name="view"/>; <paramref name="push"/> guards writes back to state.</summary>
    public DisplayPanel(Wolf3DView view, Action<Action> push)
    {
        this.view = view;

        // The switch owns on/off and the slider owns the level, so "off" is not a magic value hiding at one end of
        // a track. The slider keeps its last level while disabled, which is what makes toggling back and forth a
        // usable comparison rather than a reset.
        quantizeOn.Changed += (_, on) => push(() =>
            view.Renderer.QuantizeLevels = on ? (int)Math.Round(quantize.Value) : 0);
        quantize.ValueChanged += (_, v) => push(() =>
        {
            if (quantizeOn.IsChecked) view.Renderer.QuantizeLevels = (int)Math.Round(v);
        });
        sampling.SelectionChanged += (_, _) => push(() => view.Sampling = (SurfaceMode)sampling.SelectedIndex);
        rowFilter.SelectionChanged += (_, _) => push(() => view.Renderer.RowFilter = (RowFilter)rowFilter.SelectedIndex);
        authenticFov.Changed += (_, on) => push(() => view.Renderer.AuthenticFov = on);
        sprites.Changed += (_, on) => push(() => view.Renderer.DrawSprites = on);
        weapon.Changed += (_, on) => push(() => view.Renderer.DrawWeapon = on);

        Build(spaced: true);
    }
    #endregion

    #region Properties
    /// <summary>Rows the page needs at its current spacing, for the sidebar's layout decision.</summary>
    public int Rows { get; private set; }

    /// <summary>The row-filter dial, so checks can drive it rather than the property behind it.</summary>
    public Select RowFilterDial => rowFilter;
    #endregion

    #region Methods
    /// <summary>Lays the page out, roomy or compact.</summary>
    public void Build(bool spaced)
    {
        var gap = spaced ? 1 : 0;
        Panel.Section[] sections =
        [
            // The note is a readout of the slider above it, so it stays flush against it either way — the gap
            // belongs between CONTROLS, not inside one control's own block.
            new Panel.Section("Colour", spaced
                ? new VerticalStackPanel(quantizeOn, Panel.Spacer(), quantize, quantizeNote)
                : new VerticalStackPanel(quantizeOn, quantize, quantizeNote), 3 + gap),
            // Row filter directly under Surface, and above the three content toggles: those say what is DRAWN,
            // these two say how it is sampled. The note stays flush against Surface as a readout of it.
            new Panel.Section("Sampling",
                spaced
                    ? new VerticalStackPanel(Panel.Labelled("Surface", sampling), samplingNote, Panel.Spacer(),
                        Panel.Labelled("Row filter", rowFilter), Panel.Spacer(), authenticFov, Panel.Spacer(), sprites,
                        Panel.Spacer(), weapon)
                    : new VerticalStackPanel(Panel.Labelled("Surface", sampling), samplingNote,
                        Panel.Labelled("Row filter", rowFilter), authenticFov, sprites, weapon),
                6 + (4 * gap)),
        ];

        Rows = sections.Sum(s => s.OuterRows);
        SetContent(new VerticalStackPanel([.. sections]));
        // Deliberately NOT Refresh() here: Build runs from the constructor, and pushing state into a widget fires
        // its change event straight back at a sidebar whose fields do not exist yet. The sidebar refreshes once
        // both pages are up.
    }

    /// <summary>Re-reads every value from the renderer and view.</summary>
    public void Refresh()
    {
        var levels = view.Renderer.QuantizeLevels;
        var on = levels > 1;
        quantizeOn.IsChecked = on;
        // Only follow the renderer while quantising; when off it holds the level it would return to.
        if (on) quantize.Value = levels;
        quantize.Enabled = on;
        quantizeNote.Text = on ? $" {levels} levels/channel" : " off — full palette";
        sampling.SelectedIndex = (int)view.Sampling;
        // Says what the mode COSTS, not what it is -- both give two colours a cell, so the samples-per-cell figure
        // is the whole difference and "more" is not automatically "better".
        samplingNote.Text = view.Sampling == SurfaceMode.Quadrant
            ? " 4 samples · 2 colours/cell"
            : " 2 samples · 2 colours/cell";
        rowFilter.SelectedIndex = (int)view.Renderer.RowFilter;
        rowFilter.Enabled = view.Sampling == SurfaceMode.Quadrant;
        authenticFov.IsChecked = view.Renderer.AuthenticFov;
        sprites.IsChecked = view.Renderer.DrawSprites;
        weapon.IsChecked = view.Renderer.DrawWeapon;
    }
    #endregion

    #region Fields
    private readonly Wolf3DView view;

    private readonly Switch quantizeOn = new("Quantize", true);
    // From 2 up: "off" is the switch's job now, so the track need not reserve its left end for it.
    private readonly Slider quantize =
        Panel.Knob("Levels", 2, 16, Wolf3DRenderer.DefaultQuantizeLevels, 1, "0");
    private readonly Select sampling = new Select("Half block", "Quadrant") { FitContent = true };
    private readonly TextLabel samplingNote = Panel.Line("", Panel.Muted);

    // Sits with Surface because it only exists because of it: the extra rows it reduces are the ones quadrant
    // sampling renders to keep a framebuffer pixel square. Greyed out under half block, where there are none --
    // an inert control that still looks live is worse than one that says why it is not.
    private readonly Select rowFilter = new Select("Nearest", "Box") { FitContent = true };
    private readonly Switch authenticFov = new("Authentic FOV");
    private readonly Switch sprites = new("Scenery sprites");
    private readonly Switch weapon = new("Weapon");
    private readonly TextLabel quantizeNote = Panel.Line("", Panel.Muted);
    #endregion
}
