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
    /// <summary>Builds the sandbox scene over <paramref name="populate"/>, which fills a fresh world.
    /// <paramref name="onSwitch"/> is invoked with the scene to change to; see <see cref="ShellType"/>.</summary>
    public static Sandbox BuildSandbox(Action<PhysicsScene> populate, Action? onLoadModel = null,
                                       Action<ShellType>? onSwitch = null)
    {
        var runner = new PhysicsRunner(scene =>
        {
            // A wide, thin static slab centred under the origin, its top face at y = 0 so the floor grid the
            // renderer draws at y = 0 sits exactly on it.
            scene.AddStaticBox(new System.Numerics.Vector3(0, -0.5f, 0), new System.Numerics.Vector3(60, 1, 60));
            // Where that top face is, so a dragged body is steered along the floor rather than through it.
            scene.GroundY = 0f;
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

        // Declared before the sidebar because the sidebar's Reset button runs it, and the menu's "Reset scene" and
        // the r key run the same one — repopulating the world is this method's job, since only it holds populate.
        void Reset()
        {
            view.Selected = null;
            runner.Post(scene => { scene.ClearBodies(); populate(scene); });
        }

        var sidebar = new SidebarPanel(view, parameters, Reset);
        // A borderless frame purely to scroll, exactly as the viewer's sidebar is wrapped: each section already draws
        // its own border, so the frame contributes only the scrollbar column and the reveal-on-focus that walks Tab
        // down the panel. The panel still picks a compact layout in a short terminal — scrolling is what catches the
        // terminals too short even for that, which used to clip the camera pad off the bottom with no way back.
        sidebar.WithFrame(borderStyle: BorderStyle.None, borderPlacement: BorderPlacement.None);
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

        var loadModel = onLoadModel ?? (() => { });
        var switchTo = onSwitch ?? (_ => { });
        var menu = SceneMenu.ForSandbox(view, runner, parameters, Reset, () => ToggleSidebar(sidebar), loadModel,
            () => switchTo(ShellType.ModelViewer));
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

    /// <summary>Builds the model viewer over the mesh at <paramref name="startIndex"/>.
    /// <paramref name="onSwitch"/> is invoked with the scene to change to; see <see cref="ShellType"/>.</summary>
    public static Viewer BuildViewer(int startIndex, Action? onOpenModels = null, Action<ShellType>? onSwitch = null)
    {
        var model = new ModelScene(startIndex);
        var view = new SceneView(model, new ShadedRenderer()) { Model = model };
        view.AddRenderer(new WireframeRenderer());
        view.AddRenderer(new SolidRenderer());

        // A borderless frame purely to scroll: the sidebar is taller than a short terminal, and only a frame can
        // window it. No border, because each section already draws its own — the frame contributes just the
        // scrollbar column, and reveals whichever control Tab moves focus to.
        var sidebar = new ModelSidebarPanel(view, model);
        sidebar.WithFrame(borderStyle: BorderStyle.None, borderPlacement: BorderPlacement.None);
        var footer = new SceneFooter(view);
        view.Drew += s =>
        {
            footer.Snapshot = s;
            sidebar.Report();
        };

        _ = new ControlFrame(view, borderStyle: BorderStyle.Rounded);

        var openModels = onOpenModels ?? (() => { });
        var switchTo = onSwitch ?? (_ => { });
        var menu = SceneMenu.ForViewer(view, model, () => ToggleSidebar(sidebar), openModels,
            () => switchTo(ShellType.Sandbox));
        var root = Compose(menu, footer, sidebar, view);

        UI.RegisterHotKey(UI.HotKeys.Char('u'), () => ToggleSidebar(sidebar));
        UI.RegisterHotKey(UI.HotKeys.Char('o'), openModels);
        UI.RegisterHotKey(UI.HotKeys.Char('q'), UI.Stop);
        UI.RegisterHotKey(UI.HotKeys.Ctrl(ConsoleKey.C), UI.Stop);

        // Framed on the model rather than on the floor. The model STANDS on the ground rather than being centred on
        // the origin (see ModelScene.Elevation), so the camera looks at the height its middle actually sits at —
        // which differs per model, since a tall one is lifted further than a flat one.
        void AimAtModel()
        {
            // The viewport is only real once the tree has laid out, and this runs before the first one — fall back
            // to the aspect of a typical wide terminal so the opening shot is framed rather than arbitrary.
            var halfSpan = view.Renderer.Viewport.CellAspect is > 0 and var aspect ? aspect : DefaultCellAspect;
            view.Camera.HomeTarget = new System.Numerics.Vector3(0, model.Elevation, 0);
            view.Camera.HomeDistance = model.FramingDistance(view.Renderer.Projection.Focal, halfSpan);
            view.Camera.Reset();
        }

        model.MeshChanged += AimAtModel;
        AimAtModel();

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
    // Disposing a Control cancels its background feeds and unsubscribes it from UI.Paint / UI.ThemeChanged, both of
    // which are STATIC and both of which every Control subscribes to in its constructor. That costs nothing in an app
    // that builds one tree and exits, and it is the whole game in one that builds a second: without it SceneView's
    // 60 Hz feed keeps rendering a scene nobody is looking at, into the next session's dispatcher, once per switch.
    //
    // Only the pieces the shell records hold are reachable here. The leaves (labels, sliders, sections) stay
    // subscribed to UI.Paint and repaint into a detached buffer — bounded, cheap, and the reason a tree-walking
    // dispose is on the library backlog rather than being faked here.
    private static void Teardown(params IDisposable[] parts)
    {
        foreach (var part in parts) part.Dispose();
    }

    // The NDC half-span of a viewport on a wide terminal, used only before the first layout. See AimAtModel.
    private const double DefaultCellAspect = 0.6;

    // Nested DockPanels, never a Grid at the root: the menu takes its row, the footer its two lines, the sidebar its
    // fixed column, and the viewport fills whatever is left — at every terminal size, with no split positions to
    // recompute.
    private static ILayout Compose(MenuBar menu, SceneFooter footer, Control sidebar, SceneView view) =>
        new DockPanel(DockedControlPlacement.Top, menu,
            new DockPanel(DockedControlPlacement.Bottom, footer,
                new DockPanel(DockedControlPlacement.Right, sidebar, view)));
    #endregion

    #region Child types
    /// <summary>Which of the app's two scenes is running.</summary>
    /// <remarks>
    /// A switch is a request, not a call: the menu item hands this back and stops the UI, and the caller's loop
    /// builds the other shell after the current one has fully torn down. Building the next shell from inside the menu
    /// handler would start a UI from within the running one's frame loop, and nest a shell per switch.
    /// </remarks>
    public enum ShellType
    {
        /// <summary>The physics sandbox.</summary>
        Sandbox,

        /// <summary>The model viewer.</summary>
        ModelViewer,
    }

    /// <summary>The assembled sandbox scene.</summary>
    /// <param name="Root">The root layout to hand to <see cref="UI.Start"/>.</param>
    /// <param name="Runner">The physics thread. Dispose it when the UI stops.</param>
    /// <param name="View">The viewport, and the control to focus first.</param>
    /// <param name="Sidebar">The right-hand panel.</param>
    /// <param name="Parameters">The world settings the sidebar tunes.</param>
    /// <param name="Menu">The application menu.</param>
    public readonly record struct Sandbox(ILayout Root, PhysicsRunner Runner, SceneView View, SidebarPanel Sidebar,
                                          SandboxParameters Parameters, MenuBar Menu) : IDisposable
    {
        /// <summary>Stops the physics thread and unwires the controls. Call it once the UI has stopped, and always
        /// before building another shell in the same process — see <see cref="Teardown"/>.</summary>
        public void Dispose()
        {
            Runner.Dispose();
            Teardown(View, Sidebar, Menu);
        }
    }

    /// <summary>The assembled model viewer.</summary>
    /// <param name="Root">The root layout to hand to <see cref="UI.Start"/>.</param>
    /// <param name="Model">The static scene being shown.</param>
    /// <param name="View">The viewport, and the control to focus first.</param>
    /// <param name="Sidebar">The right-hand panel.</param>
    /// <param name="Menu">The application menu.</param>
    public readonly record struct Viewer(ILayout Root, ModelScene Model, SceneView View, ModelSidebarPanel Sidebar,
                                         MenuBar Menu) : IDisposable
    {
        /// <summary>Unwires the controls. Call it once the UI has stopped — see <see cref="Teardown"/>.</summary>
        public void Dispose() => Teardown(View, Sidebar, Menu);
    }
    #endregion
}
