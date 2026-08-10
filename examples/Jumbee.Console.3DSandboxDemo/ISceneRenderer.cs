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

    /// <summary>Draws one frame. Called on the UI thread with the newest snapshot.</summary>
    void Draw(SceneSnapshot snapshot, OrbitCamera camera);
}
