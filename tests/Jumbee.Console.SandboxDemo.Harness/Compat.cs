namespace Jumbee.Console.SandboxDemo;

/// <summary>
/// The draw-a-frame-right-now call this harness was written against, kept alive as a shim.
/// </summary>
/// <remarks>
/// <para>
/// <c>ISceneRenderer.Draw</c> used to take a snapshot and a live camera and put the result on the surface. It now
/// takes a <see cref="FrameRequest"/> — a value captured on the UI thread — and <em>returns</em> a frame for a
/// separate <c>Publish</c>, so that rasterising can run off the UI thread. That refactor is right, and it broke
/// every one of the ~25 call sites here at once; the harness had not compiled since (nor since <c>Ply.Net</c>
/// was added to the demo without being added to <c>render3d.csproj</c>).
/// </para>
/// <para>
/// A shim rather than 25 edits, because the harness wants the two steps back to back in every case: there is no UI
/// loop running to deliver a posted apply, so a check that drew without publishing would assert against the
/// previous frame. Sizing from the surface's <c>ActualWidth</c>/<c>ActualHeight</c> is what the old signature did
/// internally, and it is what <c>Verify.Check</c> in the demo does today.
/// </para>
/// <para>
/// <b>This is the third time the parked harness has been caught rotting</b>, and the first time it was found by a
/// compiler rather than by a wrong answer. It is the standing argument for open question 3 — promote it into a
/// real test project so the solution build keeps it honest.
/// </para>
/// </remarks>
internal static class HarnessRenderer
{
    #region Methods
    /// <summary>Rasterises one frame from <paramref name="camera"/> and installs it, in one call.</summary>
    public static void Draw(this ISceneRenderer renderer, SceneSnapshot snapshot, OrbitCamera camera) =>
        renderer.Publish(renderer.Draw(new FrameRequest(
            snapshot, camera.GetView(), renderer.Surface.ActualWidth, renderer.Surface.ActualHeight)));
    #endregion
}
