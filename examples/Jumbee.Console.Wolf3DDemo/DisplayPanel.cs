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

        quantize.ValueChanged += (_, v) => push(() => view.Renderer.QuantizeLevels = (int)Math.Round(v));
        antiAliasing.Changed += (_, on) => push(() => view.QuadrantSampling = on);
        authenticFov.Changed += (_, on) => push(() => view.Renderer.AuthenticFov = on);
        sprites.Changed += (_, on) => push(() => view.Renderer.DrawSprites = on);

        Build(spaced: true);
    }
    #endregion

    #region Properties
    /// <summary>Rows the page needs at its current spacing, for the sidebar's layout decision.</summary>
    public int Rows { get; private set; }
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
                ? new VerticalStackPanel(quantize, quantizeNote)
                : new VerticalStackPanel(quantize, quantizeNote), 2),
            new Panel.Section("Sampling", Panel.Stack(spaced, antiAliasing, authenticFov, sprites), 3 + (2 * gap)),
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
        quantize.Value = levels;
        quantizeNote.Text = levels > 1 ? $" {levels} levels/channel" : " off — full palette";
        antiAliasing.IsChecked = view.QuadrantSampling;
        authenticFov.IsChecked = view.Renderer.AuthenticFov;
        sprites.IsChecked = view.Renderer.DrawSprites;
    }
    #endregion

    #region Fields
    private readonly Wolf3DView view;

    // 0 and 1 both mean "no quantisation", so dragging to the left end turns it off rather than clamping at 1.
    private readonly Slider quantize =
        Panel.Knob("Quantize", 0, 12, Wolf3DRenderer.DefaultQuantizeLevels, 1, "0");
    private readonly Switch antiAliasing = new("Anti-Aliasing");
    private readonly Switch authenticFov = new("Authentic FOV");
    private readonly Switch sprites = new("Scenery sprites");
    private readonly TextLabel quantizeNote = Panel.Line("", Panel.Muted);
    #endregion
}
