#nullable enable

namespace Jumbee.Console.Wolf3DDemo;

/// <summary>
/// A mouse control for the player: forward and back, turn either way, and strafe either way.
/// </summary>
/// <remarks>
/// <para>
/// Every button calls <see cref="Wolf3DView.Send"/> — the same entry point the movement keys use — rather than
/// synthesising a keystroke. So <b>a click is exactly a key tap</b>: it opens the same sustain window, coasts the
/// same way, and is governed by the same Input-tab knobs. Nothing here needs to know that the held-key inference
/// exists, and the pad cannot drift away from the keyboard's feel.
/// </para>
/// <para>
/// One consequence worth knowing: a click therefore moves as far as a tap does, which at the shipped tuning is
/// about a tile, or a wide turn. Turn <c>Coast</c> and <c>First press</c> down on the Input tab if you want the
/// pad to step more finely — that tightens the keyboard identically, because there is only one path.
/// </para>
/// <para>
/// <b>Every button hands focus back to the viewport.</b> Leaving focus on a button means the movement keys stop
/// working the moment you nudge the player once with the mouse, which is a dead end you find within seconds.
/// </para>
/// </remarks>
public sealed class Wolf3DPad : CompositeControl
{
    #region Constructors
    /// <summary>Creates a pad driving <paramref name="view"/>.</summary>
    public Wolf3DPad(Wolf3DView view)
    {
        forward.Activated += (_, _) => view.Send(Wolf3DCommand.Forward);
        back.Activated += (_, _) => view.Send(Wolf3DCommand.Back);
        turnLeft.Activated += (_, _) => view.Send(Wolf3DCommand.TurnLeft);
        turnRight.Activated += (_, _) => view.Send(Wolf3DCommand.TurnRight);
        strafeLeft.Activated += (_, _) => view.Send(Wolf3DCommand.StrafeLeft);
        strafeRight.Activated += (_, _) => view.Send(Wolf3DCommand.StrafeRight);
        open.Activated += (_, _) => view.Send(Wolf3DCommand.Open);
        fire.Activated += (_, _) => view.Send(Wolf3DCommand.Fire);

        // A proper cross with an empty centre, in the shape everyone already knows, so the buttons need not be read
        // to be used. The two strafes sit on its middle row rather than under it, which would cost a fourth row for
        // two buttons that are not part of the cross — the same arrangement the sandbox's camera pad uses for its
        // zoom and reset.
        SetContent(new Grid([1, 1, 1], [Arrow, Arrow, Arrow, Strafe, StrafeWide],
            [Gap(), forward, Gap(), open, fire],
            [turnLeft, Gap(), turnRight, strafeLeft, strafeRight],
            [Gap(), back, Gap(), Gap(), Gap()]));
    }
    #endregion

    #region Properties
    /// <summary>The rows the pad occupies, for the section that frames it.</summary>
    public const int Rows = 3;

    /// <inheritdoc/>
    protected override bool TabNavigatesChildren => true;
    #endregion

    #region Private methods
    // A fresh blank each call: a control belongs to one cell, so the empty cells cannot share one.
    private static TextLabel Gap() => Panel.Line("", Panel.Muted);

    private static Button Key(string text, int width) =>
        new Button(text) { Style = ButtonStyle.Secondary with { MinWidth = width - 1 } };
    #endregion

    #region Fields
    // Five columns across the panel interior: three for the cross, then the two strafes. 5+5+5+7+8 = 30.
    private const int Interior = Panel.Columns - 2;
    private const int Arrow = 5;
    private const int Strafe = 7;
    private const int StrafeWide = Interior - (3 * Arrow) - Strafe;

    // U+25C4/U+25BA rather than the ◀/▶ at U+25C0/U+25B6: those carry an emoji presentation that tofus in some
    // terminal fonts, which is why the tree's disclosure glyphs avoid them too.
    private readonly Button forward = Key("▲", Arrow);
    private readonly Button back = Key("▼", Arrow);
    private readonly Button turnLeft = Key("◄", Arrow);
    private readonly Button turnRight = Key("►", Arrow);
    private readonly Button strafeLeft = Key("◄◄", Strafe);
    private readonly Button strafeRight = Key("►►", StrafeWide);
    private readonly Button open = Key("Open", Strafe);
    private readonly Button fire = Key("Fire", StrafeWide);
    #endregion
}
