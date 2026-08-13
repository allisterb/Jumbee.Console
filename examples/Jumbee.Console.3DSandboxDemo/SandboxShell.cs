namespace Jumbee.Console.SandboxDemo;

/// <summary>
/// Assembles a scene's whole user interface — renderers, viewport, sidebar, menu, footer and key bindings — and
/// hands back the root layout plus the pieces a caller needs to drive it.
/// </summary>
/// <remarks>
/// Separate from <c>Program</c> so the headless harness can exercise the <em>real</em> shell rather than a
/// reconstruction of it. That distinction has already paid for itself once here: keyboard routing differs between
/// the root-layout path the live loop uses and the path a test takes when it builds its own container, so a test
/// against a rebuilt shell can pass while the app receives nothing.
/// </remarks>
public static class SandboxShell
{
    #region Methods
    /// <summary>Builds the sandbox scene over <paramref name="populate"/>, which fills a fresh world.</summary>
    public static Sandbox BuildSandbox(Action<PhysicsScene> populate, Action? onLoadModel = null)
    {
        var runner = new PhysicsRunner(scene =>
        {
            // A wide, thin static slab centred under the origin, its top face at y = 0 so the floor grid the
            // renderer draws at y = 0 sits exactly on it.
            scene.AddStaticBox(new System.Numerics.Vector3(0, -0.5f, 0), new System.Numerics.Vector3(60, 1, 60));
            populate(scene);
        });

        // The three reach the screen by genuinely different routes (a Canvas versus half-block cells with a private
        // z-buffer), which is why each brings its own surface and SceneView swaps the child rather than the drawing
        // code. The two solid ones share their rasteriser through MeshRenderer and differ only in shading.
        var view = new SceneView(runner, new ShadedRenderer());
        view.AddRenderer(new WireframeRenderer());
        view.AddRenderer(new SolidRenderer());

        // Gravity, friction, bounce, drag and the sim clock. Every one is a real Box3D setting rather than a
        // readout, applied to every live body on the physics thread — see PhysicsScene.ApplyParameters.
        var parameters = new SandboxParameters();
        parameters.Changed += () =>
        {
            runner.TimeScale = parameters.TimeScale;
            var (g, f, b, d) = (parameters.Gravity, parameters.Friction, parameters.Bounce, parameters.Drag);
            runner.Post(scene => scene.ApplyParameters(g, f, b, d));
        };

        var sidebar = new SidebarPanel(view, parameters);
        var footer = new SceneFooter(view);

        // The readouts report the snapshot that was actually DRAWN, not the newest one, so their body count and
        // clock always agree with the picture beside them.
        view.Drew += snapshot =>
        {
            footer.Snapshot = snapshot;
            footer.Paused = runner.Paused;
            sidebar.Report(snapshot, runner.Paused);
        };

        _ = new ControlFrame(view, borderStyle: BorderStyle.Rounded);

        void Reset()
        {
            view.Selected = null;
            runner.Post(scene => { scene.ClearBodies(); populate(scene); });
        }

        var loadModel = onLoadModel ?? (() => { });
        var menu = SceneMenu.ForSandbox(view, runner, parameters, Reset, () => ToggleSidebar(sidebar), loadModel);
        var root = Compose(menu, footer, sidebar, view);

        // App-level keys are global hotkeys; everything that acts on the SCENE lives in SceneView instead, so it
        // only fires while the viewport has focus and travels with the control. Every one of these also has a menu
        // entry — the keys are the fast path, not the only path.
        UI.RegisterHotKey(UI.HotKeys.Char(' '), () => runner.Paused = !runner.Paused);
        UI.RegisterHotKey(UI.HotKeys.Char('.'), runner.StepOnce);
        UI.RegisterHotKey(UI.HotKeys.Char('r'), Reset);
        UI.RegisterHotKey(UI.HotKeys.Char('u'), () => ToggleSidebar(sidebar));
        UI.RegisterHotKey(UI.HotKeys.Char('o'), loadModel);
        UI.RegisterHotKey(UI.HotKeys.Char('q'), UI.Stop);
        UI.RegisterHotKey(UI.HotKeys.Ctrl(ConsoleKey.C), UI.Stop);

        return new Sandbox(root, runner, view, sidebar, parameters, menu);
    }

    /// <summary>Builds the model viewer over the mesh at <paramref name="startIndex"/>.</summary>
    public static Viewer BuildViewer(int startIndex, Action? onOpenModels = null)
    {
        var model = new ModelScene(startIndex);
        var view = new SceneView(model, new ShadedRenderer()) { Model = model };
        view.AddRenderer(new WireframeRenderer());
        view.AddRenderer(new SolidRenderer());

        var sidebar = new ModelSidebarPanel(view, model);
        var footer = new SceneFooter(view);
        view.Drew += s =>
        {
            footer.Snapshot = s;
            sidebar.Report();
        };

        _ = new ControlFrame(view, borderStyle: BorderStyle.Rounded);

        var openModels = onOpenModels ?? (() => { });
        var menu = SceneMenu.ForViewer(view, model, () => ToggleSidebar(sidebar), openModels);
        var root = Compose(menu, footer, sidebar, view);

        UI.RegisterHotKey(UI.HotKeys.Char('u'), () => ToggleSidebar(sidebar));
        UI.RegisterHotKey(UI.HotKeys.Char('o'), openModels);
        UI.RegisterHotKey(UI.HotKeys.Char('q'), UI.Stop);
        UI.RegisterHotKey(UI.HotKeys.Ctrl(ConsoleKey.C), UI.Stop);

        // Framed on the model rather than on a floor: it is scaled to ModelScene.ViewRadius and centred at the
        // origin.
        view.Camera.Distance = 16f;
        view.Camera.Target = System.Numerics.Vector3.Zero;

        return new Viewer(root, model, view, sidebar, menu);
    }

    /// <summary>
    /// Collapses the sidebar to a one-column stub, or restores it.
    /// </summary>
    /// <remarks>A stub rather than nothing, deliberately: a docked control at <c>Width = 0</c> <b>fills</b> instead
    /// of vanishing, which blanks the viewport — and a stub keeps the way back on screen.</remarks>
    public static void ToggleSidebar(Control sidebar) =>
        sidebar.Width = sidebar.Width > 1 ? 1 : SidebarPanel.Columns;
    #endregion

    #region Private methods
    // Nested DockPanels, never a Grid at the root: the menu takes its row, the footer its two lines, the sidebar its
    // fixed column, and the viewport fills whatever is left — at every terminal size, with no split positions to
    // recompute.
    private static ILayout Compose(MenuBar menu, SceneFooter footer, Control sidebar, SceneView view) =>
        new DockPanel(DockedControlPlacement.Top, menu,
            new DockPanel(DockedControlPlacement.Bottom, footer,
                new DockPanel(DockedControlPlacement.Right, sidebar, view)));
    #endregion

    #region Child types
    /// <summary>The assembled sandbox scene.</summary>
    /// <param name="Root">The root layout to hand to <see cref="UI.Start"/>.</param>
    /// <param name="Runner">The physics thread. Dispose it when the UI stops.</param>
    /// <param name="View">The viewport, and the control to focus first.</param>
    /// <param name="Sidebar">The right-hand panel.</param>
    /// <param name="Parameters">The world settings the sidebar tunes.</param>
    /// <param name="Menu">The application menu.</param>
    public readonly record struct Sandbox(ILayout Root, PhysicsRunner Runner, SceneView View, SidebarPanel Sidebar,
                                          SandboxParameters Parameters, MenuBar Menu);

    /// <summary>The assembled model viewer.</summary>
    /// <param name="Root">The root layout to hand to <see cref="UI.Start"/>.</param>
    /// <param name="Model">The static scene being shown.</param>
    /// <param name="View">The viewport, and the control to focus first.</param>
    /// <param name="Sidebar">The right-hand panel.</param>
    /// <param name="Menu">The application menu.</param>
    public readonly record struct Viewer(ILayout Root, ModelScene Model, SceneView View, ModelSidebarPanel Sidebar,
                                         MenuBar Menu);
    #endregion
}
