namespace Jumbee.Console.SandboxDemo;

using System.Numerics;

using CColor = ConsoleGUI.Data.Color;

/// <summary>
/// The expensive renderer: a point light with distance falloff and a specular highlight, evaluated per sub-pixel,
/// plus silhouette outlines and contact darkening found in the depth buffer.
/// </summary>
/// <remarks>
/// <para>
/// Prompted by <c>reference/projects/c_ascii_render-main</c>, whose lighting is visibly richer than flat shading.
/// The difference turns out <b>not</b> to be ray marching — that project marches a single cube SDF and its busy
/// scene is 2D backdrop. It is two things a rasteriser can have just as easily:
/// </para>
/// <list type="number">
/// <item>a <b>point</b> light rather than a directional one, so the direction to the lamp changes across a surface
/// and a flat face picks up a real gradient; and</item>
/// <item>shading evaluated <b>per pixel</b> rather than per triangle, without which (1) buys nothing.</item>
/// </list>
/// <para>
/// What genuinely does belong to signed distance fields is their soft shadows and ambient occlusion, both of which
/// fall out of already having a distance function. <see cref="ContactStrength">Contact darkening</see> here is the
/// cheap screen-space stand-in; real shadows would need a second depth pass from the light.
/// </para>
/// </remarks>
public sealed class ShadedRenderer : MeshRenderer
{
    #region Properties
    /// <inheritdoc/>
    public override string Name => "shaded";

    /// <summary>How silhouettes and creases are drawn. See <see cref="SilhouetteStyle"/>.</summary>
    public SilhouetteStyle Edges
    {
        get => HalfBlocks.EdgeStyle;
        set => HalfBlocks.EdgeStyle = value;
    }

    /// <summary>How strongly creases and contacts are darkened; 0 disables the pass.</summary>
    public float ContactStrength { get; set; } = 0.55f;

    /// <summary>
    /// When set, diffuse light <b>wraps</b> around the object (<c>N·L·0.5 + 0.5</c>) instead of being clamped at
    /// zero — the "half-lambert" trick, taken from <c>reference/projects/voxcii-main</c>.
    /// </summary>
    /// <remarks>
    /// Worth having <em>specifically</em> because a terminal has so few shade levels. Clamping sends every face
    /// turned past perpendicular to the same flat black, so the whole unlit half of an object collapses into one
    /// value and loses its shape; wrapping spreads that half across the lower levels instead. The cost is contrast
    /// on the lit side, which compresses into the upper half of the range.
    /// </remarks>
    public bool WrapLighting { get; set; } = true;
    #endregion

    #region Protected methods
    /// <inheritdoc/>
    protected override bool ShadesPerPixel => true;

    /// <inheritdoc/>
    protected override CColor ShadeFace(Vector3 normal, Color tint) => ShadePixel(Vector3.Zero, normal, tint);

    /// <inheritdoc/>
    protected override CColor ShadePixel(Vector3 world, Vector3 normal, Color tint)
    {
        var toLight = LightPosition - world;
        var distance = toLight.Length();
        if (distance < 1e-4f) distance = 1e-4f;
        var lightDir = toLight / distance;

        var facing = Vector3.Dot(normal, lightDir);
        var lambert = WrapLighting ? (facing * 0.5f) + 0.5f : MathF.Max(0f, facing);

        // Inverse-square falls off far too fast at sandbox scale (the scene is ~24 units across and the lamp hangs
        // just above it), so this is the usual games fudge: a soft, bounded falloff over a tunable radius.
        var attenuation = 1f / (1f + (distance * distance / (LightRadius * LightRadius)));

        var specular = 0f;
        // Gate on the RAW dot, not the wrapped one: wrapping never returns zero for a face that is merely turned
        // away, so testing `lambert > 0` would put a highlight on surfaces pointing into shadow.
        if (facing > 0f)
        {
            var half = Vector3.Normalize(lightDir + Vector3.Normalize(View.Eye - world));
            specular = MathF.Pow(MathF.Max(0f, Vector3.Dot(normal, half)), SpecularPower) * SpecularStrength;
        }

        return Quantise(tint, Ambient + ((1f - Ambient) * lambert * attenuation) + (specular * attenuation), ShadeLevels);
    }

    /// <inheritdoc/>
    protected override void PostProcess()
    {
        // Order matters: darken first, then outline. Outlining last keeps edge glyphs at full brightness instead of
        // having the contact pass mute the very lines that define the shape.
        HalfBlocks.ApplyContactShading(ContactStrength);
        HalfBlocks.DetectEdges(EdgeThreshold);
    }
    #endregion

    #region Fields
    // A lamp hung above and to one side of the scene. Close enough that its falloff is visible across the ground —
    // a light at infinity would give a flat plane one uniform colour, which is exactly the problem being solved.
    private static readonly Vector3 LightPosition = new(6f, 11f, 5f);

    // Generous on purpose. The point light is here for the GRADIENT ACROSS A FACE, not for dramatic distance
    // falloff — at 14 the far checkerboard collapsed to near-black and took the recession cue with it, which is
    // the very thing that makes the flat renderer read as 3D. Wide enough that distance reads as a gentle dimming.
    private const float LightRadius = 40f;
    private const float SpecularPower = 24f;
    private const float SpecularStrength = 0.55f;
    private const float Ambient = 0.28f;

    // More levels than SolidRenderer: with a genuine gradient to represent there is something for them to do. Each
    // extra level costs ANSI bytes, so this is deliberately still small.
    private const float ShadeLevels = 7f;

    // How sharply the inverse-depth field must bend, relative to local depth, to count as an edge. Low enough to
    // catch a box crease seen face-on, high enough that a sphere's curvature does not light up its whole interior.
    private const float EdgeThreshold = 0.09f;
    #endregion
}
