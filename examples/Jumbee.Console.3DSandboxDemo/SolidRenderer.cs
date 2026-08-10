namespace Jumbee.Console.SandboxDemo;

using System.Numerics;

using CColor = ConsoleGUI.Data.Color;

/// <summary>
/// Solid, lit, depth-correct 3D at its cheapest: flat-shaded triangles from a single directional light, one colour
/// per face.
/// </summary>
/// <remarks>
/// <para>
/// Note what "flat" costs and why no amount of tuning changes it: a face normal is constant across the face and a
/// light at infinity has a constant direction, so <c>N·L</c> is constant and the whole face is one colour <em>by
/// construction</em>. That is not a resolution or shade-level limit. <see cref="ShadedRenderer"/> is the answer to
/// it, at a price.
/// </para>
/// <para>
/// In exchange this is the cheapest of the three renderers per pixel, and its large flat regions coalesce into very
/// few ANSI runs — it emits fewer bytes than either the wireframe or the shaded renderer.
/// </para>
/// </remarks>
public sealed class SolidRenderer : MeshRenderer
{
    #region Properties
    /// <inheritdoc/>
    public override string Name => "solid";
    #endregion

    #region Protected methods
    /// <inheritdoc/>
    protected override bool ShadesPerPixel => false;

    /// <inheritdoc/>
    protected override CColor ShadeFace(Vector3 normal, Color tint)
    {
        var lambert = MathF.Max(0f, Vector3.Dot(normal, -LightDirection));
        return Quantise(tint, Ambient + ((1f - Ambient) * lambert), ShadeLevels);
    }
    #endregion

    #region Fields
    // A fixed key light from over the camera's default shoulder. This points the way the light TRAVELS, so it is
    // negated at use. Plus enough ambient that faces turned away stay readable rather than going black.
    private static readonly Vector3 LightDirection = Vector3.Normalize(new Vector3(-0.4f, -0.85f, -0.35f));
    private const float Ambient = 0.28f;

    // How many distinct brightness levels a face can take. Low on purpose: see MeshRenderer.Quantise.
    private const float ShadeLevels = 5f;
    #endregion
}
