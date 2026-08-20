#nullable enable

namespace Jumbee.Console.Wolf3DDemo;

/// <summary>
/// Assembles the demo — viewport, sidebar, footer and key bindings — and hands back the root layout plus the pieces
/// a caller needs to drive it.
/// </summary>
/// <remarks>
/// Separate from <c>Program</c> so a headless check can drive the <em>real</em> shell rather than a reconstruction:
/// keyboard routing differs between the root-layout path the live loop takes and the path a test takes when it
/// builds its own container, so a test against a rebuilt shell can pass while the app receives nothing.
/// </remarks>
public static class Wolf3DShell
{
    #region Methods
    /// <summary>Builds the shell over <paramref name="scene"/>.</summary>
    public static Shell Build(Wolf3DScene scene, int fps = 30)
    {
        var tuning = new Wolf3DTuning();
        var view = new Wolf3DView(scene, tuning, fps);
        var footer = new Wolf3DFooter(view);
        var sidebar = new Wolf3DSidebar(view);
        view.Changed += footer.Report;

        // No title: the border is there to show focus, and the footer already names the level.
        _ = new ControlFrame(view, borderStyle: BorderStyle.Rounded);

        // A borderless frame purely to scroll: the Input tab is taller than a short terminal, and only a frame can
        // window it. The tab strip already delimits the panel, so the frame contributes just the scrollbar column
        // and the reveal-on-focus that walks Tab down the page.
        sidebar.WithFrame(borderStyle: BorderStyle.None, borderPlacement: BorderPlacement.None);

        var root = new DockPanel(DockedControlPlacement.Bottom, footer,
            new DockPanel(DockedControlPlacement.Right, sidebar, view));

        // The number keys stay as the fast path for the three display toggles the sidebar also carries -- they cost
        // nothing, and reaching for a slider to flip a switch while walking is worse than pressing a key. Everything
        // that moves the PLAYER lives on the view instead, so it only fires while the viewport has focus.
        UI.RegisterHotKey(UI.HotKeys.Char('1'), () =>
        {
            view.Renderer.QuantizeLevels =
                view.Renderer.QuantizeLevels > 1 ? 0 : Wolf3DRenderer.DefaultQuantizeLevels;
            Report();
        });
        UI.RegisterHotKey(UI.HotKeys.Char('2'), () => view.QuadrantSampling = !view.QuadrantSampling);
        UI.RegisterHotKey(UI.HotKeys.Char('3'), () =>
        {
            view.Renderer.AuthenticFov = !view.Renderer.AuthenticFov;
            Report();
        });
        UI.RegisterHotKey(UI.HotKeys.Char('u'), () => ToggleSidebar(sidebar));
        UI.RegisterHotKey(UI.HotKeys.Char('\t'), () => sidebar.Tabs.SelectedIndex =
            (sidebar.Tabs.SelectedIndex + 1) % Math.Max(1, sidebar.Tabs.TabCount));
        UI.RegisterHotKey(UI.HotKeys.Escape, UI.Stop);
        UI.RegisterHotKey(UI.HotKeys.Ctrl(ConsoleKey.C), UI.Stop);

        void Report()
        {
            footer.Report();
            sidebar.Refresh();
        }

        return new Shell(root, view, sidebar, footer, tuning);
    }

    /// <summary>Collapses the sidebar to nothing and back, giving the viewport the full width.</summary>
    /// <remarks>
    /// Collapse-to-size rather than removing it from the tree: a removed control loses its widgets' state and its
    /// focus position, so bringing it back would reset every knob the user had just set.
    /// </remarks>
    public static void ToggleSidebar(Wolf3DSidebar sidebar) =>
        sidebar.Width = sidebar.Width == 0 ? Wolf3DSidebar.Columns : 0;
    #endregion

    #region Child types
    /// <summary>The assembled demo.</summary>
    /// <param name="Root">The root layout to hand to <see cref="UI.Start"/>.</param>
    /// <param name="View">The viewport, and the control to focus first.</param>
    /// <param name="Sidebar">The right-hand tabbed panel.</param>
    /// <param name="Footer">The status lines.</param>
    /// <param name="Tuning">The movement and key-handling knobs.</param>
    public readonly record struct Shell(
        ILayout Root, Wolf3DView View, Wolf3DSidebar Sidebar, Wolf3DFooter Footer, Wolf3DTuning Tuning) : IDisposable
    {
        /// <inheritdoc/>
        public void Dispose() => View.Dispose();
    }
    #endregion
}
