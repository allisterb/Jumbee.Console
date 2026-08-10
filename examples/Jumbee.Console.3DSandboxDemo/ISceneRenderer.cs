namespace Jumbee.Console.SandboxDemo;

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

    /// <summary>Draws one frame. Called on the UI thread with the newest snapshot.</summary>
    void Draw(SceneSnapshot snapshot, OrbitCamera camera);

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
