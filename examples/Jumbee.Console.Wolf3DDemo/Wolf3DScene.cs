#nullable enable

namespace Jumbee.Console.Wolf3DDemo;

using Wolfenshine.Game;
using Wolfenshine.Graphics;
using Wolfenshine.Maps;
using Wolfenshine.Rendering;
using Wolfenshine.Resources;

/// <summary>
/// A loaded level and the player walking through it. UI-thread owned.
/// </summary>
/// <remarks>
/// The static-scene subset of what Wolfenshine's <c>GameSession</c> does: a pose, collision against the wall plane,
/// and the plane-one scenery. No actors, no doors opening, no pickups.
/// </remarks>
public sealed class Wolf3DScene
{
    #region Constructors
    /// <summary>Loads the game data in <paramref name="gameDataDirectory"/> and opens on its first level.</summary>
    public Wolf3DScene(string gameDataDirectory)
    {
        var resources = WolfensteinResources.Load(new DirectoryInfo(gameDataDirectory));
        Edition = resources.Edition.ToString();
        Palette = WolfensteinPaletteLoader.Load();
        WallTextures = WolfensteinVSwapLoader.LoadWallTextures(resources);
        Sprites = WolfensteinVSwapLoader.LoadSprites(resources);
        Hud = WolfensteinGraphicsLoader.LoadHudGraphics(resources);
        Levels = WolfensteinMapLoader.Load(resources).Maps;
        LoadLevel(0);
    }
    #endregion

    #region Properties
    /// <summary>Which data set was found — <c>Shareware</c> (one episode) or <c>Full</c> (six).</summary>
    public string Edition { get; }

    /// <summary>Every level in the data set, in map-slot order.</summary>
    public IReadOnlyList<WolfensteinMap> Levels { get; }

    /// <summary>Index into <see cref="Levels"/> of the level being walked.</summary>
    public int LevelIndex { get; private set; }

    /// <summary>The level being walked.</summary>
    public WolfensteinMap Map { get; private set; } = null!;

    /// <summary>Doors, all closed — a static scene renders them as wall faces.</summary>
    public WolfensteinDoors Doors { get; private set; } = null!;

    /// <summary>Plane-one scenery: lamps, barrels, tables, food, treasure.</summary>
    public IReadOnlyList<WorldSprite> StaticObjects { get; private set; } = [];

    public WolfensteinPalette Palette { get; }
    public WolfensteinWallTextures WallTextures { get; }
    public WolfensteinSpriteSet Sprites { get; }

    /// <summary>The status-bar composer: stamps the face, weapon icon, keys and numbers into one 320x40 picture.</summary>
    public WolfensteinHudGraphics Hud { get; }

    /// <summary>Player position in map tiles.</summary>
    public double X { get; private set; }
    public double Y { get; private set; }

    /// <summary>Unit facing vector.</summary>
    public double DirectionX { get; private set; }
    public double DirectionY { get; private set; }

    /// <summary>Compass bearing in degrees, for the readout.</summary>
    public double Bearing => ((Math.Atan2(DirectionX, -DirectionY) * 180.0 / Math.PI) + 360.0) % 360.0;
    #endregion

    #region Methods
    /// <summary>Opens <paramref name="index"/> and puts the player on its start marker. Wraps at either end.</summary>
    public void LoadLevel(int index)
    {
        LevelIndex = ((index % Levels.Count) + Levels.Count) % Levels.Count;
        Map = Levels[LevelIndex];
        Doors = WolfensteinDoors.FromMap(Map);
        StaticObjects = WolfensteinStaticObjects.FromMap(Map);
        Restart();
    }

    /// <summary>Returns the player to the level's start marker.</summary>
    public void Restart()
    {
        var start = RaycastCamera.FromPlayerStart(Map);
        X = start.X;
        Y = start.Y;
        DirectionX = start.DirectionX;
        DirectionY = start.DirectionY;
    }

    /// <summary>Builds the camera for the current pose; <paramref name="planeLength"/> sets the horizontal FOV.</summary>
    public RaycastCamera GetCamera(double planeLength) =>
        new(X, Y, DirectionX, DirectionY, -DirectionY * planeLength, DirectionX * planeLength);

    /// <summary>Moves <paramref name="forward"/> and <paramref name="strafe"/> tiles, sliding along walls.</summary>
    /// <remarks>Each axis is tested on its own, which is what turns a corner from a full stop into a slide.</remarks>
    public void Move(double forward, double strafe)
    {
        // Strafing is movement along the facing vector rotated a quarter turn.
        var dx = (DirectionX * forward) - (DirectionY * strafe);
        var dy = (DirectionY * forward) + (DirectionX * strafe);
        if (!IsBlocked(X + dx + (Math.Sign(dx) * Radius), Y)) X += dx;
        if (!IsBlocked(X, Y + dy + (Math.Sign(dy) * Radius))) Y += dy;
    }

    /// <summary>Rotates the facing vector by <paramref name="radians"/>, positive being a right turn.</summary>
    public void Turn(double radians)
    {
        var (sin, cos) = Math.SinCos(radians);
        var dx = (DirectionX * cos) - (DirectionY * sin);
        var dy = (DirectionX * sin) + (DirectionY * cos);
        DirectionX = dx;
        DirectionY = dy;
    }

    /// <summary>
    /// Operates the door the player is facing, if one is within reach. Returns whether anything happened.
    /// </summary>
    /// <remarks>
    /// <b>Every key is assumed.</b> A static walkthrough has no pickups, so an authentic empty key ring would make
    /// the locked doors permanently impassable and wall off parts of most levels — which reads as a broken demo
    /// rather than as fidelity. The lock logic itself is the vendored engine's; only the key ring is a fiction.
    /// </remarks>
    public bool Use()
    {
        // One tile ahead, then the tile the player stands in — the original checks the facing tile first so that
        // standing in a doorway and pressing use closes the door behind you rather than re-opening the one ahead.
        var aheadX = (int)(X + (DirectionX * UseReach));
        var aheadY = (int)(Y + (DirectionY * UseReach));
        var door = Doors.Get(aheadX, aheadY) ?? Doors.Get((int)X, (int)Y);
        return door is not null && door.Operate(canClose: true, keyMask: AllKeys);
    }

    /// <summary>Advances every door's slide. Call once a frame.</summary>
    /// <remarks>A door never closes on the player: the predicate refuses the tile they are standing in, which is
    /// the engine's own guard and the reason <c>Update</c> takes one at all.</remarks>
    public void TickDoors(double elapsedSeconds) =>
        Doors.Update(elapsedSeconds, door => door.X != (int)X || door.Y != (int)Y);

    /// <summary>Whether the tile under a point is solid — a wall, or blocking scenery.</summary>
    /// <remarks>
    /// Scenery blocks using the original <c>statinfo</c> flags (see <see cref="WolfensteinStaticObjects"/>), so a
    /// table stops the player while a puddle does not.
    /// </remarks>
    private bool IsBlocked(double x, double y)
    {
        var tileX = (int)x;
        var tileY = (int)y;
        if (tileX < 0 || tileX >= Map.Width || tileY < 0 || tileY >= Map.Height) return true;

        // A door tile reads as solid in plane zero, so it must be asked BEFORE the wall test or an open door stays
        // an invisible wall — the panel slides aside on screen and the player walks into nothing. Passable only
        // once the panel has retracted far enough to fit through, matching what the raycaster is drawing.
        if (Doors.Get(tileX, tileY) is { } door) return door.OpenAmount < DoorPassable;

        if (Map.IsSolid(tileX, tileY)) return true;
        return WolfensteinStaticObjects.BlocksMovement(Map.GetObject(tileX, tileY));
    }
    #endregion

    #region Fields
    private const double Radius = 0.25;
    private const double UseReach = 0.75;
    private const double DoorPassable = 0.8;
    // Gold and silver, the only two the original has. See the note on Use.
    private const int AllKeys = 0b11;
    #endregion
}
