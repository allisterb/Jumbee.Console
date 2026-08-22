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

        // Three bands, each a whole grid row, so the cross is centred by construction rather than by padding: the
        // strafes above it, the cross, then Open and Fire below, with a one-row space either side of the cross.
        // Because the outer bands mirror each other the cross lands in the middle vertically, and its own three
        // columns put it in the middle horizontally — nothing here computes an offset.
        //
        // The strafes and the action pair sit in the OUTER columns, aligned with the turn buttons, so the panel
        // reads as three tidy columns rather than six buttons at six positions. Leaving the middle column empty on
        // those rows is what spaces them, with no spacer control involved.
        // Five columns, not three: the cross keeps its own narrow arrow columns in the middle, and the pairs that
        // flank it — the strafes above, the verbs below — live in the wider OUTER columns, which is what puts them
        // at the edges with the whole cross between them. One column set serves both, so nothing has to span.
        //
        // Empty cells are null and an all-empty row is `[]` -- a sparse grid needs no filler controls. This used to
        // be 21 calls to a Gap() helper returning a blank TextLabel per cell, which buried the shape it was drawing.
        // The library change that allows this came out of writing it the other way first.
        SetContent(new Grid([Row, Space, Row, Row, Row, Space, Row], [Wide, Arrow, Arrow, Arrow, WideEnd],
            [strafeLeft, null, null, null, strafeRight],
            [],
            [null, null, forward, null, null],
            [null, turnLeft, null, turnRight, null],
            [null, null, back, null, null],
            [],
            [fire, null, null, null, open]));
    }
    #endregion

    #region Properties
    /// <summary>The rows the pad occupies, for the section that frames it.</summary>
    public const int Rows = (5 * Row) + (2 * Space);

    /// <inheritdoc/>
    protected override bool TabNavigatesChildren => true;
    #endregion

    #region Private methods
    private static Button Key(string text, int width) =>
        new Button(text) { Style = ButtonStyle.Secondary with { MinWidth = width - 1 } };

    // The same one-row button in a colour of its own, for the two buttons that act rather than move.
    private static Button Verb(string text, int width, Color fill, Color hover) =>
        new Button(text)
        {
            Style = ButtonStyle.Secondary with
            {
                MinWidth = width - 1,
                Normal = Style.White | Style.Bg(fill),
                Hover = Style.White | Style.Bg(hover),
                Press = Style.White | Style.Bg(hover),
            },
        };
    #endregion

    #region Fields
    // 8 + 5 + 5 + 5 + 7 = 30, the panel interior. The three middle columns carry the cross at its original arrow
    // width; the outer two are wide enough for "Open" and a doubled strafe glyph.
    private const int Interior = Panel.Columns - 2;
    private const int Arrow = 5;
    private const int Wide = 8;
    private const int WideEnd = Interior - Wide - (3 * Arrow);
    private const int Row = 1;      // a Flat button's intrinsic height
    private const int Space = 1;    // the blank row above and below the cross

    // U+25C4/U+25BA rather than the ◀/▶ at U+25C0/U+25B6: those carry an emoji presentation that tofus in some
    // terminal fonts, which is why the tree's disclosure glyphs avoid them too.
    private readonly Button forward = Key("▲", Arrow);
    private readonly Button back = Key("▼", Arrow);
    private readonly Button turnLeft = Key("◄", Arrow);
    private readonly Button turnRight = Key("►", Arrow);
    private readonly Button strafeLeft = Key("◄◄", Wide);
    private readonly Button strafeRight = Key("►►", WideEnd);

    // The two verbs are the only buttons here that DO something rather than move you, so they are the only ones
    // carrying a colour: blue to open, orange to fire. Everything else keeps the neutral secondary fill, which is
    // what makes these two findable without reading them.
    private readonly Button open = Verb("Open", Wide, new Color(40, 70, 120), new Color(60, 95, 160));
    private readonly Button fire = Verb("Fire", WideEnd, new Color(150, 70, 20), new Color(195, 100, 35));
    #endregion
}
