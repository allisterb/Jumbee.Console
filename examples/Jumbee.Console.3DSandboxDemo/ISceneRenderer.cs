namespace Jumbee.Console.SandboxDemo;

/// <summary>
/// Everything a renderer needs to draw one frame, captured on the UI thread so the rasterisation can run off it.
/// </summary>
/// <param name="Snapshot">The scene to draw. Published by reference swap and never mutated afterwards, so a
/// renderer may hold it for as long as it likes — see <see cref="SceneSnapshot"/>.</param>
/// <param name="View">The camera basis, resolved at capture time. A value, not the live <see cref="OrbitCamera"/>,
/// so a drag on the UI thread cannot move the camera halfway through a frame.</param>
/// <param name="Cells">Viewport width in character cells, as laid out when the frame was requested.</param>
/// <param name="Rows">Viewport height in character rows.</param>
/// <remarks>
/// A record <b>class</b> rather than a struct on purpose: it is published to the rasterising thread by reference
/// assignment, which is atomic, where a multi-field struct could be read half-updated.
/// </remarks>
public sealed record FrameRequest(SceneSnapshot Snapshot, CameraView View, int Cells, int Rows);

/// <summary>
/// Draws a physics snapshot from a camera. The two implementations reach the screen by different routes — the
/// wireframe through a <see cref="Canvas"/> and its braille sub-cells, the solid one by writing half-block cells
/// itself — so each brings its own <see cref="Surface"/> and <see cref="SceneView"/> swaps between them.
/// </summary>
public interface ISceneRenderer
{
    /// <summary>Shown in the footer, and how the toggle names the mode.</summary>
    string Name { get; }

    /// <summary>The control this renderer draws into. Stable for the renderer's lifetime.</summary>
    Control Surface { get; }

    /// <summary>The projection the last <see cref="Draw"/> used. The view needs it to turn a mouse cell back into a
    /// world ray, and it must be the same one that put the pixels there.</summary>
    Projection Projection { get; }

    /// <summary>The screen window as of the last <see cref="Draw"/>. See <see cref="Viewport"/> for why this is
    /// read from the renderer rather than recomputed by the caller.</summary>
    Viewport Viewport { get; }

    /// <summary>The body to draw as selected, or <see langword="null"/> for none.</summary>
    int? Selected { get; set; }

    /// <summary>
    /// Whether <see cref="Draw"/> may be called off the UI thread, with <see cref="Publish"/> then installing the
    /// result on it.
    /// </summary>
    /// <remarks>
    /// Opt-in rather than assumed, because it is a claim about the renderer's <em>surface</em>: only one whose
    /// buffers are handed over by value (as <see cref="HalfBlockSurface"/>'s frames are) can be filled while the UI
    /// thread paints. A renderer drawing straight into a shared control returns <see langword="false"/> and
    /// <see cref="SceneView"/> draws it inline — correct, and no worse than it was.
    /// </remarks>
    bool DrawsOffThread => false;

    /// <summary>
    /// Rasterises one frame and returns it for <see cref="Publish"/>, or <see langword="null"/> if there was
    /// nothing to draw.
    /// </summary>
    /// <remarks>
    /// Runs off the UI thread when <see cref="DrawsOffThread"/> is set, so it must read <b>only</b>
    /// <paramref name="request"/> and the renderer's own private state — never a live control property. That is why
    /// the viewport size and camera arrive in the request instead of being measured here.
    /// </remarks>
    object? Draw(in FrameRequest request);

    /// <summary>Installs a frame returned by <see cref="Draw"/> and asks for a repaint. <b>UI thread only.</b></summary>
    void Publish(object? frame);

    /// <summary>Screen-space pick: the body whose projected centre is nearest <paramref name="column"/>,
    /// <paramref name="row"/>, or <see langword="null"/> if nothing is close enough.</summary>
    /// <remarks>
    /// <para>
    /// Projection rather than a physics raycast, deliberately: it reads only the snapshot, so picking works on the
    /// UI thread while the physics thread is mid-step, and it needs no round trip through the command queue. The
    /// cost is that it picks by <em>centre</em>, so clicking the far corner of a large box can select a small body
    /// behind it.
    /// </para>
    /// <para>
    /// Implemented once here rather than per renderer: it needs only <see cref="Viewport"/> and
    /// <see cref="Projection"/>, and both renderers agree on those by construction.
    /// </para>
    /// </remarks>
    int? Pick(int column, int row, SceneSnapshot snapshot, OrbitCamera camera)
    {
        if (!Viewport.TryToNdc(column, row, out var nx, out var ny)) return null;

        var view = camera.GetView();
        int? best = null;
        var bestDistance = PickThreshold * PickThreshold;
        for (var i = 0; i < snapshot.Count; i++)
        {
            if (!Projection.TryProject(view.Transform(snapshot.Positions[i]), out var x, out var y)) continue;
            var d = ((x - nx) * (x - nx)) + ((y - ny) * (y - ny));
            if (d >= bestDistance) continue;
            bestDistance = d;
            best = snapshot.Ids[i];
        }

        return best;
    }

    /// <summary>How near a click must land, in NDC, to select a body — inertia's value, about 4% of the view width.
    /// Forgiving enough for a terminal's coarse pointer without grabbing across the screen.</summary>
    const float PickThreshold = 0.08f;
}
