#nullable enable

namespace Jumbee.Console.Wolf3DDemo;

using System.Diagnostics;

using ConsoleGUI.Space;

/// <summary>
/// The viewport: a <see cref="HalfBlockSurface"/> the raycaster draws into, driven by a clock, with the movement
/// keys on it.
/// </summary>
/// <remarks>
/// <para>
/// <b>A terminal reports key presses, not key state.</b> There is no key-up event, so "is W held?" cannot be
/// answered — holding a key produces one press, a pause of a few hundred milliseconds, then the OS auto-repeat
/// stream. Moving one step per press therefore gives a lurch, a stall, then a stutter, which is nothing like
/// walking.
/// </para>
/// <para>
/// So each press opens a <see cref="SustainMs"/> window instead, and the frame clock — not the key — integrates
/// movement while a window is open. Auto-repeat keeps extending it, releasing lets it lapse. The cost is that
/// movement overruns the release by up to that window; the window is sized to just outstrip a typical auto-repeat
/// interval, which is the shortest value that still bridges the gap between repeats.
/// </para>
/// </remarks>
public sealed class Wolf3DView : CompositeControl
{
    #region Constructors
    /// <summary>Creates the viewport over <paramref name="scene"/>, redrawing at <paramref name="fps"/>.</summary>
    public Wolf3DView(Wolf3DScene scene, int fps = 30)
    {
        Scene = scene;
        Renderer = new Wolf3DRenderer(scene);
        SetContent(new Boundary(surface));
        clock.Start();
        feed = Feed(Tick, Math.Max(1, 1000 / Math.Max(1, fps)));
    }
    #endregion

    #region Properties
    /// <summary>The level being walked.</summary>
    public Wolf3DScene Scene { get; }

    /// <summary>The renderer, whose settings the sidebar and keys change.</summary>
    public Wolf3DRenderer Renderer { get; }

    /// <summary>Doubles the horizontal sample rate, compositing each 2×2 block into a quadrant glyph.</summary>
    public bool QuadrantSampling
    {
        get => surface.QuadrantSampling;
        set { surface.QuadrantSampling = value; Changed?.Invoke(); }
    }

    /// <summary>Frames actually drawn per second, averaged over the last second.</summary>
    public double FramesPerSecond { get; private set; }

    /// <summary>ANSI-cost telemetry from the last frame: distinct colours and colour runs.</summary>
    public (int Colors, int Runs) LastCost => (Renderer.LastColors, Renderer.LastRuns);

    /// <summary>Raised on the UI thread after each frame, and whenever a setting changes.</summary>
    public event Action? Changed;

    /// <inheritdoc/>
    protected override bool WantsMouse => false;
    #endregion

    #region Methods
    /// <inheritdoc/>
    public override void Dispose()
    {
        stopping = true;
        feed.StopAsync().Wait(FeedJoinMs);
        base.Dispose();
    }

    // Keys are handled in the TUNNEL. A CompositeControl is treated as a container by the layout route, which
    // dispatches into its content rather than calling its OnInput -- so a composite whose children are not focusable
    // never sees a key through OnInput at all. InterceptInput is the seam both routes call.
    /// <inheritdoc/>
    protected override bool InterceptInput(UI.InputEventArgs inputEventArgs)
    {
        if (inputEventArgs.InputEvent is not { } inputEvent) return false;

        var key = inputEvent.Key;
        var running = (key.Modifiers & ConsoleModifiers.Shift) != 0;
        var handled = true;
        switch (key.Key)
        {
            case ConsoleKey.UpArrow: Press(ref forwardUntil, running); break;
            case ConsoleKey.DownArrow: Press(ref backUntil, running); break;
            case ConsoleKey.LeftArrow: Press(ref turnLeftUntil, running); break;
            case ConsoleKey.RightArrow: Press(ref turnRightUntil, running); break;
            default: handled = HandleChar(char.ToLowerInvariant(key.KeyChar), running); break;
        }

        if (handled) inputEvent.Handled = true;
        return handled;
    }

    private bool HandleChar(char c, bool running)
    {
        switch (c)
        {
            case 'w': Press(ref forwardUntil, running); return true;
            case 's': Press(ref backUntil, running); return true;
            case 'a': Press(ref turnLeftUntil, running); return true;
            case 'd': Press(ref turnRightUntil, running); return true;
            case 'q': Press(ref strafeLeftUntil, running); return true;
            case 'e': Press(ref strafeRightUntil, running); return true;
            case '[': Scene.LoadLevel(Scene.LevelIndex - 1); Changed?.Invoke(); return true;
            case ']': Scene.LoadLevel(Scene.LevelIndex + 1); Changed?.Invoke(); return true;
            case 'r': Scene.Restart(); Changed?.Invoke(); return true;
            default: return false;
        }
    }

    // Opens (or extends) this axis's sustain window. Running widens the step rather than the window, so a run does
    // not also make the overrun longer.
    private void Press(ref long until, bool running)
    {
        until = clock.ElapsedMilliseconds + SustainMs;
        this.running = running;
        Focus();
    }

    /// <summary>
    /// Advances and draws one frame over a fixed step, instead of over however long the last frame took.
    /// </summary>
    /// <remarks>
    /// For headless checks: <see cref="Feed"/> only ticks under a live UI loop, so without this a snapshot test
    /// renders a viewport that has never drawn. A fixed step also makes "press w, step, read the position"
    /// reproducible, which a wall clock would not be.
    /// </remarks>
    public void DrawFrame(double seconds = 1.0 / 30.0) => Step(seconds);

    private void Tick()
    {
        if (stopping) return;

        var now = clock.ElapsedMilliseconds;
        var dt = Math.Min((now - lastTickMs) / 1000.0, MaxStepSeconds);
        lastTickMs = now;
        Step(dt);
    }

    private void Step(double dt)
    {
        var now = clock.ElapsedMilliseconds;
        var speed = (running ? RunTilesPerSecond : WalkTilesPerSecond) * dt;
        var turn = (running ? RunTurnRadians : WalkTurnRadians) * dt;
        var forward = (Held(forwardUntil, now) ? speed : 0) - (Held(backUntil, now) ? speed : 0);
        var strafe = (Held(strafeRightUntil, now) ? speed : 0) - (Held(strafeLeftUntil, now) ? speed : 0);
        var rotate = (Held(turnRightUntil, now) ? turn : 0) - (Held(turnLeftUntil, now) ? turn : 0);
        if (rotate != 0) Scene.Turn(rotate);
        if (forward != 0 || strafe != 0) Scene.Move(forward, strafe);

        Renderer.Draw(surface);
        CountFrame(now);
        Changed?.Invoke();

        static bool Held(long until, long now) => now < until;
    }

    private void CountFrame(long now)
    {
        framesThisSecond++;
        if (now - fpsWindowMs < 1000) return;
        FramesPerSecond = framesThisSecond * 1000.0 / (now - fpsWindowMs);
        framesThisSecond = 0;
        fpsWindowMs = now;
    }
    #endregion

    #region Fields
    // Just past a typical auto-repeat interval (~30-50 ms), with headroom for a frame the UI thread was late to.
    private const int SustainMs = 220;
    private const int FeedJoinMs = 500;
    private const double WalkTilesPerSecond = 3.4;
    private const double RunTilesPerSecond = 6.0;
    private const double WalkTurnRadians = 2.2;
    private const double RunTurnRadians = 3.4;
    // Clamps a long stall so a late frame cannot teleport the player through a wall.
    private const double MaxStepSeconds = 0.1;

    private readonly HalfBlockSurface surface = new() { Background = new ConsoleGUI.Data.Color(0, 0, 0) };
    private readonly FeedHandle feed;
    private readonly Stopwatch clock = new();
    private long forwardUntil, backUntil, strafeLeftUntil, strafeRightUntil, turnLeftUntil, turnRightUntil;
    private long lastTickMs, fpsWindowMs;
    private int framesThisSecond;
    private bool running;
    private volatile bool stopping;
    #endregion
}
