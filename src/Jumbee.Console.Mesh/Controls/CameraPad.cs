namespace Jumbee.Console.SandboxDemo;

/// <summary>
/// A mouse control for the orbit camera: a four-way D-pad, with zoom and reset alongside its middle row.
/// </summary>
/// <remarks>
/// <para>
/// One click is a coarse step — about 15°, against the arrow keys' 4.6°. A key is held or tapped repeatedly and a
/// button is clicked, so a step small enough to feel right on the keyboard would need eighty clicks to turn the
/// scene once, which is not a control. Fine adjustment stays on the keys and the drag.
/// </para>
/// <para>
/// <b>Every button hands focus back to whatever <paramref name="moved"/> refocuses</b> — the viewport. Leaving
/// focus on the button means the arrow keys stop orbiting the moment you nudge the camera once with the mouse,
/// which is a dead end you meet within seconds of finding the pad.
/// </para>
/// </remarks>
public sealed class CameraPad : CompositeControl
{
    #region Constructors
    /// <summary>Creates a pad driving <paramref name="camera"/>, running <paramref name="moved"/> after each nudge
    /// so the caller can put focus back where it belongs.</summary>
    public CameraPad(OrbitCamera camera, Action moved)
    {
        this.moved = moved;

        orbitLeft.Activated += (_, _) => Nudge(() => camera.Orbit(-Orbit, 0));
        orbitRight.Activated += (_, _) => Nudge(() => camera.Orbit(Orbit, 0));
        orbitUp.Activated += (_, _) => Nudge(() => camera.Orbit(0, -Orbit));
        orbitDown.Activated += (_, _) => Nudge(() => camera.Orbit(0, Orbit));
        zoomIn.Activated += (_, _) => Nudge(() => camera.Zoom(1f - Zoom));
        zoomOut.Activated += (_, _) => Nudge(() => camera.Zoom(1f + Zoom));
        reset.Activated += (_, _) => Nudge(camera.Reset);

        // The D-pad on the left in the shape everyone already knows, so the buttons do not have to be read to be
        // used; zoom and reset sit on its middle row rather than under it, which would cost a fourth row for three
        // buttons that are not part of the pad.
        SetContent(new Grid([1, 1, 1], [Arrow, Arrow, Arrow, Step, Step, Wide],
            [Gap(), orbitUp, Gap(), Gap(), Gap(), Gap()],
            [orbitLeft, Gap(), orbitRight, zoomIn, zoomOut, reset],
            [Gap(), orbitDown, Gap(), Gap(), Gap(), Gap()]));
    }
    #endregion

    #region Properties
    /// <summary>The rows the pad occupies, for the panel that frames it.</summary>
    public const int Rows = 3;

    /// <inheritdoc/>
    protected override bool TabNavigatesChildren => true;
    #endregion

    #region Private methods
    private void Nudge(Action move)
    {
        move();
        moved();
    }

    // A fresh blank each call: a control belongs to one cell, so the empty cells cannot share one.
    private static TextLabel Gap() =>
        new(TextLabelOrientation.Horizontal, "", MutedColor) { Focusable = false, Height = 1 };

    private static Button Key(string text, int width) =>
        new Button(text) { Style = ButtonStyle.Secondary with { MinWidth = width } };
    #endregion

    #region Fields
    private const float Orbit = 0.26f;    // ~15° a click
    private const float Zoom = 0.12f;

    private readonly Action moved;

    // Six columns across the panel interior: three for the pad, then zoom and reset. 5+5+5+4+4+7 = 30.
    private const int Interior = SidebarPanel.Columns - 2;
    private const int Arrow = 5;
    private const int Step = 4;
    private const int Wide = Interior - (3 * Arrow) - (2 * Step);

    // U+25C4/U+25BA rather than the ◀/▶ at U+25C0/U+25B6: those carry an emoji presentation that tofus in some
    // terminal fonts, which is why the tree's disclosure glyphs avoid them too.
    private readonly Button orbitLeft = Key("◄", Arrow - 1);
    private readonly Button orbitUp = Key("▲", Arrow - 1);
    private readonly Button orbitDown = Key("▼", Arrow - 1);
    private readonly Button orbitRight = Key("►", Arrow - 1);
    private readonly Button zoomIn = Key("+", Step - 1);
    private readonly Button zoomOut = Key("−", Step - 1);
    private readonly Button reset = Key("Reset", Wide - 1);

    private static readonly Color MutedColor = new(130, 136, 150);
    #endregion
}
