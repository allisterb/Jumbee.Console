namespace Jumbee.Console.SandboxDemo;

using ConsoleGUI.Input;
using ConsoleGUI.Space;

/// <summary>
/// The viewport: pulls the newest physics snapshot and hands it to whichever <see cref="ISceneRenderer"/> is
/// active, and owns the camera controls.
/// </summary>
/// <remarks>
/// A composite rather than a plain control because the renderer brings its own surface — a <see cref="Canvas"/> for
/// wireframe, a cell-writing control for solid — and swapping renderers is swapping that child.
/// </remarks>
public sealed class SceneView : CompositeControl
{
    #region Constructors
    /// <summary>Creates the viewport over <paramref name="runner"/>, starting with <paramref name="renderer"/>.</summary>
    public SceneView(PhysicsRunner runner, ISceneRenderer renderer, int fps = 60)
    {
        this.runner = runner;
        this.renderer = renderer;
        SetContent(new Boundary(renderer.Surface));

        // The scene changes continuously, so drive redraws on a clock rather than waiting for a state change to
        // invalidate. Feed runs on the UI thread, so Draw and the camera reads it makes need no synchronization.
        Feed(Tick, Math.Max(1, 1000 / Math.Max(1, fps)));
    }
    #endregion

    #region Properties
    /// <summary>The orbit rig. UI-thread owned.</summary>
    public OrbitCamera Camera { get; } = new();

    /// <summary>The renderer currently drawing the scene.</summary>
    public ISceneRenderer Renderer => renderer;

    /// <summary>The snapshot last drawn — what the footer and inspector should report, so their numbers match the
    /// picture rather than running a tick ahead of it.</summary>
    public SceneSnapshot? Drawn { get; private set; }

    /// <summary>Raised on the UI thread after each frame is drawn, carrying the snapshot that was drawn.</summary>
    public event Action<SceneSnapshot>? Drew;
    #endregion

    #region Methods
    /// <summary>Swaps the active renderer, moving its surface into the layout.</summary>
    public void SetRenderer(ISceneRenderer next)
    {
        if (ReferenceEquals(next, renderer)) return;
        renderer = next;
        SetContent(new Boundary(next.Surface));
        Invalidate();
    }
    #endregion

    #region Protected methods
    /// <inheritdoc/>
    protected override bool WantsMouse => true;

    // The viewport is a window onto the scene, not a document: it must be exactly as tall as the frame's visible
    // area. Without this a wrapping ControlFrame offers an unbounded height so a scrollable child can grow, and a
    // control with no intrinsic height fills to the 1000-row clamp instead -- which does not look like a bug, it
    // looks like an empty viewport, because the camera's whole picture then lands off-screen.
    /// <inheritdoc/>
    protected override bool FillsFrameViewport => true;

    /// <inheritdoc/>
    protected override void OnInput(InputEvent inputEvent)
    {
        var key = inputEvent.Key;
        // Shift is the fine-adjust modifier throughout: a fifth of the step, on every axis.
        var scale = (key.Modifiers & ConsoleModifiers.Shift) != 0 ? 0.2f : 1f;
        var handled = true;
        switch (key.Key)
        {
            case ConsoleKey.LeftArrow: Camera.Orbit(-OrbitStep * scale, 0); break;
            case ConsoleKey.RightArrow: Camera.Orbit(OrbitStep * scale, 0); break;
            case ConsoleKey.UpArrow: Camera.Orbit(0, -OrbitStep * scale); break;
            case ConsoleKey.DownArrow: Camera.Orbit(0, OrbitStep * scale); break;
            case ConsoleKey.PageUp: Camera.Zoom(1f - (ZoomStep * scale)); break;
            case ConsoleKey.PageDown: Camera.Zoom(1f + (ZoomStep * scale)); break;
            case ConsoleKey.Home: Camera.Reset(); break;
            default: handled = false; break;
        }

        if (handled) inputEvent.Handled = true;
    }

    /// <inheritdoc/>
    protected override void OnMousePress(Position position)
    {
        dragging = true;
        lastDrag = position;
        CaptureMouse();
    }

    /// <inheritdoc/>
    protected override void OnMouseRelease(Position position)
    {
        dragging = false;
        ReleaseMouse();
    }

    /// <inheritdoc/>
    protected override void OnMouseMove(Position position)
    {
        if (!dragging) return;
        // Cells are about twice as tall as they are wide, so a vertical cell is twice the angle of a horizontal one
        // — halve dY to make a diagonal drag feel diagonal.
        Camera.Orbit((position.X - lastDrag.X) * DragPerCell, (position.Y - lastDrag.Y) * DragPerCell * 0.5f);
        lastDrag = position;
    }

    /// <inheritdoc/>
    protected override void OnMouseWheel(Position position, int delta) =>
        Camera.Zoom(delta > 0 ? 1f - ZoomStep : 1f + ZoomStep);

    /// <inheritdoc/>
    protected override HelpInfo? GetHelpInfo() => new HelpInfo("3D viewport")
        .WithKey("Arrows", "Orbit the camera (hold Shift for fine steps)")
        .WithKey("PgUp/PgDn", "Zoom in / out")
        .WithKey("Home", "Reset the camera")
        .WithKey("Drag", "Orbit with the mouse; wheel zooms");
    #endregion

    #region Private methods
    private void Tick()
    {
        var snapshot = runner.Snapshot;
        renderer.Draw(snapshot, Camera);
        Drawn = snapshot;
        Drew?.Invoke(snapshot);
    }
    #endregion

    #region Fields
    private const float OrbitStep = 0.08f;
    private const float ZoomStep = 0.1f;
    private const float DragPerCell = 0.02f;

    private readonly PhysicsRunner runner;

    private ISceneRenderer renderer;
    private bool dragging;
    private Position lastDrag;
    #endregion
}
