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
/// stream. Moving one step per press gives a lurch, a stall, then a stutter, which is nothing like walking.
/// </para>
/// <para>
/// So each press opens a sustain window and the frame clock — not the key — integrates movement while it is open.
/// Three things make that feel like holding a key rather than like lag:
/// </para>
/// <list type="number">
/// <item><b>The window belongs to an <see cref="Axis"/>, not to a key.</b> Forward/back, strafe left/right and
/// turn left/right are one axis each, carrying a single direction. Pressing the opposite key <em>reverses</em> the
/// axis at once instead of opening a second window that cancels the first out to nothing. Two independent windows
/// was the first version, and it read as heavy input lag: after turning right, a left press bought exactly zero
/// turn until the right window lapsed.</item>
/// <item><b>The window is measured from the repeat stream, not guessed.</b> Presses arriving closer together than
/// <see cref="Wolf3DTuning.RepeatGapMs"/> are auto-repeat, so their spacing is the real repeat interval; the window becomes a
/// small multiple of it. That is far tighter than any fixed value — release stops the player in about a repeat and
/// a half rather than in a fixed quarter-second.</item>
/// <item><b>Before the first repeat arrives, speed decays instead of stopping.</b> The OS initial repeat delay is
/// longer than any window short enough to make a tap feel like a tap, so a fixed window <em>always</em> stalls
/// once at the start of a hold. Coasting turns that unavoidable gap into a slight slow-down that the first repeat
/// restores. Once repeats are flowing we know the key is genuinely held, so release goes back to being crisp.</item>
/// </list>
/// <para>
/// What remains is a tap moving a little further than the key was down for, which is the irreducible cost of not
/// being told when the key came up.
/// </para>
/// </remarks>
public sealed class Wolf3DView : CompositeControl
{
    #region Constructors
    /// <summary>Creates the viewport over <paramref name="scene"/>, redrawing at <paramref name="fps"/>.</summary>
    public Wolf3DView(Wolf3DScene scene, Wolf3DTuning? tuning = null, int fps = 30)
    {
        Scene = scene;
        Tuning = tuning ?? new Wolf3DTuning();
        Renderer = new Wolf3DRenderer(scene);
        move = new Axis(Tuning);
        strafe = new Axis(Tuning);
        turn = new Axis(Tuning);
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

    /// <summary>The movement and key-handling knobs the Input tab turns.</summary>
    public Wolf3DTuning Tuning { get; }

    /// <summary>
    /// The auto-repeat interval measured from the keys actually being pressed, in ms, or 0 if none has been seen.
    /// </summary>
    /// <remarks>
    /// A property of the machine, not of the demo, and the number every key-handling knob should be set against —
    /// which is why the Input tab shows it live rather than leaving it to be guessed at.
    /// </remarks>
    public int MeasuredRepeatMs =>
        Math.Max(move.MeasuredRepeatMs, Math.Max(turn.MeasuredRepeatMs, strafe.MeasuredRepeatMs));

    /// <summary>Doubles the horizontal sample rate, compositing each 2×2 block into a quadrant glyph.</summary>
    public bool QuadrantSampling
    {
        get => surface.QuadrantSampling;
        set { surface.QuadrantSampling = value; Changed?.Invoke(); }
    }

    /// <summary>
    /// How sub-cell samples are mapped onto glyphs.
    /// </summary>
    /// <remarks>
    /// Both modes get the same <b>two colours per cell</b> — one foreground, one background — which is the hard
    /// ceiling of character-cell rendering. They differ in how many samples share that budget, so this is a trade
    /// rather than a quality dial: see <see cref="SurfaceMode"/>.
    /// </remarks>
    public SurfaceMode Sampling
    {
        get => surface.QuadrantSampling ? SurfaceMode.Quadrant : SurfaceMode.HalfBlock;
        set => QuadrantSampling = value == SurfaceMode.Quadrant;
    }

    /// <summary>Frames actually drawn per second, averaged over the last second.</summary>
    public double FramesPerSecond { get; private set; }

    /// <summary>ANSI-cost telemetry from the last frame: distinct colours and colour runs.</summary>
    public (int Colors, int Runs) LastCost => (Renderer.LastColors, Renderer.LastRuns);

    /// <summary>Raised on the UI thread after each frame, and whenever a setting changes.</summary>
    public event Action? Changed;

    // Load-bearing, despite the viewport having nothing to click ON. Movement keys only arrive while this control
    // holds focus, so once a sidebar widget took it there was no way back: the viewport ignored the mouse, and Tab
    // was registered as a global hotkey, so neither route out existed and the player simply stopped responding.
    /// <inheritdoc/>
    protected override bool WantsMouse => true;

    /// <summary>Clicking the viewport returns focus to it, which is what re-arms the movement keys.</summary>
    /// <inheritdoc/>
    protected override void OnMousePress(Position position) => Focus();
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
        var run = (key.Modifiers & ConsoleModifiers.Shift) != 0;
        var handled = true;
        switch (key.Key)
        {
            case ConsoleKey.UpArrow: Send(Wolf3DCommand.Forward, run); break;
            case ConsoleKey.DownArrow: Send(Wolf3DCommand.Back, run); break;
            case ConsoleKey.LeftArrow: Send(Wolf3DCommand.TurnLeft, run); break;
            case ConsoleKey.RightArrow: Send(Wolf3DCommand.TurnRight, run); break;
            case ConsoleKey.Spacebar: Send(Wolf3DCommand.Open); break;
            case ConsoleKey.Enter: Send(Wolf3DCommand.Open); break;
            default: handled = HandleChar(char.ToLowerInvariant(key.KeyChar), run); break;
        }

        if (handled) inputEvent.Handled = true;
        return handled;
    }

    private bool HandleChar(char c, bool run)
    {
        switch (c)
        {
            case 'w': Send(Wolf3DCommand.Forward, run); return true;
            case 's': Send(Wolf3DCommand.Back, run); return true;
            case 'a': Send(Wolf3DCommand.TurnLeft, run); return true;
            case 'd': Send(Wolf3DCommand.TurnRight, run); return true;
            case 'q': Send(Wolf3DCommand.StrafeLeft, run); return true;
            case 'e': Send(Wolf3DCommand.StrafeRight, run); return true;
            case '[': Scene.LoadLevel(Scene.LevelIndex - 1); Changed?.Invoke(); return true;
            case ']': Scene.LoadLevel(Scene.LevelIndex + 1); Changed?.Invoke(); return true;
            case 'f': Send(Wolf3DCommand.Fire); return true;
            case 'r': Scene.Restart(); Changed?.Invoke(); return true;
            default: return false;
        }
    }

    /// <summary>
    /// Applies one movement command, exactly as a key press of the same intent would.
    /// </summary>
    /// <remarks>
    /// The single entry point into the held-key inference: the keyboard and the sidebar pad both come through
    /// here, so a pad click IS a key tap rather than something that merely resembles one, and the Input tab's
    /// knobs govern both. Nothing synthesises a <see cref="ConsoleKeyInfo"/>.
    /// </remarks>
    public void Send(Wolf3DCommand command, bool running = false)
    {
        switch (command)
        {
            case Wolf3DCommand.Forward: Press(move, +1, running); break;
            case Wolf3DCommand.Back: Press(move, -1, running); break;
            case Wolf3DCommand.TurnLeft: Press(turn, -1, running); break;
            case Wolf3DCommand.TurnRight: Press(turn, +1, running); break;
            case Wolf3DCommand.StrafeLeft: Press(strafe, -1, running); break;
            case Wolf3DCommand.StrafeRight: Press(strafe, +1, running); break;

            // Events, not axes: they fire once and have no sustain window, so they bypass Axis entirely. A door
            // does not open further for being asked twice, and a re-fire mid-animation would only restart it.
            case Wolf3DCommand.Open:
                Scene.Use();
                Focus();
                break;
            case Wolf3DCommand.Fire:
                if (fireFrame == 0) fireElapsed = 0;
                fireFrame = Math.Max(fireFrame, 1);
                Focus();
                break;
        }
    }

    // Running widens the step rather than the window, so a run does not also lengthen the overrun.
    private void Press(Axis axis, int direction, bool run)
    {
        axis.Press(direction, clock.ElapsedMilliseconds);
        running = run;
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
        var speed = (running ? Tuning.RunSpeed : Tuning.WalkSpeed) * dt;
        var radians = (running ? Tuning.RunTurnDegrees : Tuning.TurnDegrees) * (Math.PI / 180.0) * dt;

        var rotate = turn.Advance(dt, now) * radians;
        var forward = move.Advance(dt, now) * speed;
        var sideways = strafe.Advance(dt, now) * speed;
        if (rotate != 0) Scene.Turn(rotate);
        if (forward != 0 || sideways != 0) Scene.Move(forward, sideways);
        Scene.TickDoors(dt);
        TickWeapon(dt);

        Renderer.WeaponFrame = fireFrame;
        Renderer.Draw(surface);
        CountFrame(now);
        Changed?.Invoke();
    }

    // Frames 1..4 are the swing; 0 is the weapon at rest and is what it returns to. Driven by elapsed time rather
    // than by frame count so the animation lasts the same wall-clock time whatever the frame rate is set to.
    private void TickWeapon(double dt)
    {
        if (fireFrame == 0) return;
        fireElapsed += dt;
        var frame = 1 + (int)(fireElapsed / FireFrameSeconds);
        fireFrame = frame > WeaponFrames - 1 ? 0 : frame;
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

    #region Child types
    /// <summary>
    /// One movement axis — forward/back, strafe, or turn — driven by presses that never say when they stopped.
    /// </summary>
    /// <remarks>
    /// Holds a single direction, so the opposite key reverses rather than cancelling; learns the auto-repeat
    /// interval from the presses themselves; and coasts rather than stopping until the first repeat proves the key
    /// is being held. See the notes on <see cref="Wolf3DView"/>.
    /// </remarks>
    private sealed class Axis(Wolf3DTuning tuning)
    {
        /// <summary>The auto-repeat interval measured on this axis, or 0 if no repeat has been seen yet.</summary>
        /// <remarks>Surfaced because it is the number every other key-handling constant should be set against, and
        /// it is a property of the machine rather than of the demo — see <see cref="Wolf3DTuning.RepeatGapMs"/>.</remarks>
        public int MeasuredRepeatMs => sawRepeat ? repeatMs : 0;

        /// <summary>Registers a press in <paramref name="direction"/> (-1 or +1) at <paramref name="now"/> ms.</summary>
        public void Press(int direction, long now)
        {
            if (direction != this.direction)
            {
                // A reversal, or the first press after a stop. Either way the repeat stream has restarted, so what
                // was learned about the old one says nothing about this one.
                this.direction = direction;
                sawRepeat = false;
                repeatMs = 0;
                scale = 1.0;
                until = now + (long)tuning.FirstPressMs;
            }
            else if (now - lastPressMs <= tuning.RepeatGapMs)
            {
                // Close enough to be auto-repeat. Track the SHORTEST gap seen: the first repeat after the initial
                // delay can arrive late, and a window sized from that would overrun every release.
                var gap = (int)Math.Max(1, now - lastPressMs);
                repeatMs = sawRepeat ? Math.Min(repeatMs, gap) : gap;
                sawRepeat = true;
                until = now + (long)Math.Clamp(repeatMs * tuning.RepeatWindows, MinSustainMs, MaxSustainMs);
            }
            else
            {
                until = now + (long)tuning.FirstPressMs;
            }

            lastPressMs = now;
        }

        /// <summary>The signed speed multiplier for this frame: -1..+1, and exactly 0 when the axis is idle.</summary>
        public double Advance(double dt, long now)
        {
            if (direction == 0) return 0.0;
            if (now < until)
            {
                scale = 1.0;
            }
            else if (sawRepeat)
            {
                // Repeats were flowing and have stopped: the key is genuinely up. Stop crisply.
                direction = 0;
                scale = 0.0;
            }
            else if (tuning.CoastSeconds <= 0.0)
            {
                direction = 0;
                scale = 0.0;
            }
            else
            {
                // Still inside the OS initial repeat delay, or it was a tap — indistinguishable until the delay
                // elapses. Coast down so a hold sags briefly instead of stalling, and a tap eases to a halt.
                scale *= Math.Exp(-dt / tuning.CoastSeconds);
                if (scale < 0.02) { direction = 0; scale = 0.0; }
            }

            return direction * scale;
        }

        private const int MinSustainMs = 90;
        private const int MaxSustainMs = 320;

        private int direction;
        private long until;
        private long lastPressMs = long.MinValue / 2;
        private int repeatMs;
        private bool sawRepeat;
        private double scale;
    }
    #endregion

    #region Fields
    private const int FeedJoinMs = 500;
    private const int WeaponFrames = 5;
    private const double FireFrameSeconds = 0.07;
    // Clamps a long stall so a late frame cannot teleport the player through a wall.
    private const double MaxStepSeconds = 0.1;

    private readonly HalfBlockSurface surface = new() { Background = new ConsoleGUI.Data.Color(0, 0, 0) };
    private readonly FeedHandle feed;
    private readonly Stopwatch clock = new();
    private readonly Axis move;
    private readonly Axis strafe;
    private readonly Axis turn;
    private long lastTickMs, fpsWindowMs;
    private int fireFrame;
    private double fireElapsed;
    private int framesThisSecond;
    private bool running;
    private volatile bool stopping;
    #endregion
}
