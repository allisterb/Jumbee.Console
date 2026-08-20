#nullable enable

namespace Jumbee.Console.Wolf3DDemo;

/// <summary>
/// One movement intent, independent of how it arrived.
/// </summary>
/// <remarks>
/// The seam that keeps the on-screen pad honest. A button could have synthesised a keystroke, but that would mean
/// re-entering the whole held-key inference — the sustain window, the repeat learning, the coast — through a fake
/// event, and any divergence between a real key and a fake one would surface as the pad feeling subtly different
/// from the keyboard. Instead both routes call <see cref="Wolf3DView.Send"/>, which is the one place that touches
/// an <c>Axis</c>. A pad click is therefore exactly a key tap, and the Input tab's knobs tune both at once.
/// </remarks>
public enum Wolf3DCommand
{
    /// <summary>Walk along the facing direction.</summary>
    Forward,

    /// <summary>Walk backwards.</summary>
    Back,

    /// <summary>Turn left (counter-clockwise).</summary>
    TurnLeft,

    /// <summary>Turn right (clockwise).</summary>
    TurnRight,

    /// <summary>Step left without turning.</summary>
    StrafeLeft,

    /// <summary>Step right without turning.</summary>
    StrafeRight,

    /// <summary>Operate whatever the player is facing — the original's "use". Opens and closes doors.</summary>
    /// <remarks>Unlike the movement commands this is an <em>event</em>, not an axis: it fires once per press and
    /// has no sustain window, because a door does not open further for being asked twice.</remarks>
    Open,

    /// <summary>Fire the weapon. Animates the sprite; there is nothing to shoot in a static scene.</summary>
    Fire,
}
