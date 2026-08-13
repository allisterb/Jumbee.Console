namespace Jumbee.Console.SandboxDemo;

/// <summary>
/// Builds the application menu — a mouse-reachable home for every command the key bindings already offer, and the
/// place a user finds out those bindings exist.
/// </summary>
/// <remarks>
/// Every menu is registered with the <c>Func&lt;MenuItem[]&gt;</c> overload, so it is rebuilt from live state each
/// time it opens. That is what lets <see cref="MenuItem.Checked"/> report the active renderer, edge style and
/// spawn shape: press <c>v</c> and the next time the Render menu opens the tick has moved, with nothing keeping
/// the two in step but the fact that both read the same state.
/// </remarks>
public static class SceneMenu
{
    #region Methods
    /// <summary>The sandbox menu: scene, render, spawn, camera and help.</summary>
    public static MenuBar ForSandbox(SceneView view, PhysicsRunner runner, SandboxParameters parameters,
                                     Action reset, Action toggleSidebar, Action loadMesh)
    {
        var bar = new MenuBar();

        bar.Add("Scene", () =>
        [
            new MenuItem(runner.Paused ? "Resume" : "Pause", () => runner.Paused = !runner.Paused) { Shortcut = "Space" },
            new MenuItem("Step once", runner.StepOnce) { Shortcut = ".", Enabled = runner.Paused },
            MenuItem.Separator,
            new MenuItem("Reset scene", reset) { Shortcut = "r" },
            new MenuItem("Clear bodies", view.ClearScene) { Shortcut = "c" },
            new MenuItem("Reset world settings", parameters.Reset),
            MenuItem.Separator,
            new MenuItem("Load model…", loadMesh) { Shortcut = "o" },
            MenuItem.Separator,
            new MenuItem("Quit", UI.Stop) { Shortcut = "q" },
        ]);

        bar.Add("Render", () =>
        [
            .. view.Renderers.Select(r => new MenuItem(r.Name, () => view.SetRenderer(r))
            {
                Checked = ReferenceEquals(r, view.Renderer),
            }),
            MenuItem.Separator,
            // The edge styles belong to the shaded renderer alone, so under the other two they are shown disabled
            // rather than hidden — a menu whose length changes under you is harder to learn than one that greys out.
            .. EdgeItems(view),
            MenuItem.Separator,
            new MenuItem("Wrap lighting", () => view.SetWrapLighting(!(view.WrapLighting ?? false)))
            {
                Checked = view.WrapLighting ?? false,
                Enabled = view.WrapLighting is not null,
                Shortcut = "w",
            },
        ]);

        bar.Add("Spawn", () =>
        [
            new MenuItem("Box", () => view.Spawn.Shape = BodyShape.Box) { Checked = view.Spawn.Shape == BodyShape.Box },
            new MenuItem("Sphere", () => view.Spawn.Shape = BodyShape.Sphere) { Checked = view.Spawn.Shape == BodyShape.Sphere },
            new MenuItem("Mesh", view.Spawn.NextMesh)
            {
                Checked = view.Spawn.Shape == BodyShape.Mesh,
                Enabled = Meshes.RegisteredCount > 0,
            },
            MenuItem.Separator,
            new MenuItem("Drop one in", view.SpawnAtTarget) { Shortcut = "n" },
            new MenuItem("Fire one", view.Launch) { Shortcut = "f" },
            MenuItem.Separator,
            new MenuItem("Delete selected", view.DeleteSelected) { Shortcut = "x", Enabled = view.Selected is not null },
        ]);

        bar.Add("View", () =>
        [
            new MenuItem("Reset camera", view.Camera.Reset) { Shortcut = "Home" },
            new MenuItem("Sidebar", toggleSidebar) { Shortcut = "u" },
            MenuItem.Separator,
            new MenuItem("Keys and help", () => UI.ShowHelp()) { Shortcut = "F1" },
        ]);

        return bar;
    }

    /// <summary>The model viewer's menu: no simulation to pause, and the transforms the sandbox does not have.</summary>
    public static MenuBar ForViewer(SceneView view, ModelScene model, Action toggleSidebar, Action openModels)
    {
        var bar = new MenuBar();

        bar.Add("Model", () =>
        [
            new MenuItem("Open…", openModels) { Shortcut = "o" },
            MenuItem.Separator,
            new MenuItem("Previous", () => model.Step(-1)) { Shortcut = "[" },
            new MenuItem("Next", () => model.Step(+1)) { Shortcut = "]" },
            MenuItem.Separator,
            new MenuItem(model.SpinRate == 0 ? "Start turntable" : "Stop turntable",
                () => model.SpinRate = model.SpinRate == 0f ? 0.35f : 0f) { Shortcut = "p" },
            new MenuItem("Reset transform", model.ResetTransform) { Shortcut = "0" },
            MenuItem.Separator,
            new MenuItem("Quit", UI.Stop) { Shortcut = "q" },
        ]);

        bar.Add("Render", () =>
        [
            .. view.Renderers.Select(r => new MenuItem(r.Name, () => view.SetRenderer(r))
            {
                Checked = ReferenceEquals(r, view.Renderer),
            }),
            MenuItem.Separator,
            .. EdgeItems(view),
        ]);

        bar.Add("View", () =>
        [
            new MenuItem("Reset camera", view.Camera.Reset) { Shortcut = "Home" },
            new MenuItem("Sidebar", toggleSidebar) { Shortcut = "u" },
            MenuItem.Separator,
            new MenuItem("Keys and help", () => UI.ShowHelp()) { Shortcut = "F1" },
        ]);

        return bar;
    }
    #endregion

    #region Private methods
    private static IEnumerable<MenuItem> EdgeItems(SceneView view) =>
        new[] { SilhouetteStyle.None, SilhouetteStyle.Ink, SilhouetteStyle.Glyph }.Select(style =>
            new MenuItem("Edges: " + style.ToString().ToLowerInvariant(), () => view.SetEdgeStyle(style))
            {
                Checked = view.Edges == style,
                Enabled = view.Edges is not null,
            });
    #endregion
}
