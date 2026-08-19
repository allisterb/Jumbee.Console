#nullable enable

namespace Jumbee.Console.Wolf3DDemo;

/// <summary>
/// Assembles the demo — viewport, footer and key bindings — and hands back the root layout plus the pieces a caller
/// needs to drive it.
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
        var view = new Wolf3DView(scene, fps);
        var footer = new Wolf3DFooter(view);
        view.Changed += footer.Report;

        _ = new ControlFrame(view, borderStyle: BorderStyle.Rounded, title: " Wolfenstein 3D ");

        var root = new DockPanel(DockedControlPlacement.Bottom, footer, view);

        // Render toggles are global hotkeys; everything that moves the PLAYER lives on the view, so it only fires
        // while the viewport has focus and travels with the control.
        UI.RegisterHotKey(UI.HotKeys.Char('1'), () =>
        {
            view.Renderer.QuantizeLevels = view.Renderer.QuantizeLevels > 1 ? 0 : DefaultQuantizeLevels;
            footer.Report();
        });
        UI.RegisterHotKey(UI.HotKeys.Char('2'), () => view.QuadrantSampling = !view.QuadrantSampling);
        UI.RegisterHotKey(UI.HotKeys.Char('3'), () =>
        {
            view.Renderer.AuthenticFov = !view.Renderer.AuthenticFov;
            footer.Report();
        });
        UI.RegisterHotKey(UI.HotKeys.Escape, UI.Stop);
        UI.RegisterHotKey(UI.HotKeys.Ctrl(ConsoleKey.C), UI.Stop);

        return new Shell(root, view, footer);
    }
    #endregion

    #region Child types
    /// <summary>The assembled demo.</summary>
    /// <param name="Root">The root layout to hand to <see cref="UI.Start"/>.</param>
    /// <param name="View">The viewport, and the control to focus first.</param>
    /// <param name="Footer">The status lines.</param>
    public readonly record struct Shell(ILayout Root, Wolf3DView View, Wolf3DFooter Footer) : IDisposable
    {
        /// <inheritdoc/>
        public void Dispose() => View.Dispose();
    }
    #endregion

    #region Fields
    private const int DefaultQuantizeLevels = 6;
    #endregion
}
