namespace Jumbee.Console.SandboxDemo;

using System.Numerics;

using CColor = ConsoleGUI.Data.Color;

/// <summary>
/// The expensive renderer: a point light with distance falloff and a specular highlight, evaluated per sub-pixel,
/// plus silhouette outlines and ambient occlusion found in the depth buffer.
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
/// fall out of already having a distance function. The <see cref="OcclusionStrength">occlusion pass</see> here is
/// the screen-space equivalent, working from the depth buffer; real shadows would need a second depth pass from the
/// light, which this does not have.
/// </para>
/// </remarks>
public sealed class ShadedRenderer : MeshRenderer
{
    #region Constructors
    /// <summary>Creates the shaded renderer at its default shade-ramp resolution.</summary>
    public ShadedRenderer() : base(DefaultShadeLevels) { }
    #endregion

    #region Properties
    /// <inheritdoc/>
    public override string Name => "shaded";

    /// <summary>How silhouettes and creases are drawn. See <see cref="SilhouetteStyle"/>.</summary>
    public SilhouetteStyle Edges
    {
        get => HalfBlocks.EdgeStyle;
        set => HalfBlocks.EdgeStyle = value;
    }

    /// <summary>Strength of the screen-space ambient occlusion pass — how far a fully enclosed sub-pixel is
    /// darkened. 0 skips the pass entirely, 1 takes it to black. See <see cref="HalfBlockSurface.ApplyOcclusion"/>
    /// for what it measures.</summary>
    /// <remarks>
    /// Worth knowing what this is and is not responsible for. Turning it <em>off</em> makes the picture flatter,
    /// not sharper — measured on the bunny, mean contrast between neighbouring body cells goes 8.99 → 7.73 and the
    /// distinct shade levels collapse from 26 to 5, because the pass multiplies the quantised levels by a
    /// continuous per-cell factor and so hands back gradation the quantiser removed. If the shaded renderer looks
    /// softer than the flat one, the cause is almost always <see cref="WrapLighting"/> instead.
    /// </remarks>
    public float OcclusionStrength { get; set; } = DefaultOcclusionStrength;

    /// <summary>The default for <see cref="OcclusionStrength"/>, so a UI offering the dial can mark it.</summary>
    public const float DefaultOcclusionStrength = 0.55f;

    /// <summary>
    /// When set, diffuse light <b>wraps</b> around the object (<c>N·L·0.5 + 0.5</c>) instead of being clamped at
    /// zero — the "half-lambert" trick, taken from <c>reference/projects/voxcii-main</c>. Off by default.
    /// </summary>
    /// <remarks>
    /// <para>
    /// It buys the <em>unlit</em> half of an object and pays for it out of the <em>lit</em> half. Clamped, every
    /// face turned past perpendicular gets the same flat black, so the shaded side collapses to one value and loses
    /// its shape. Wrapping remaps <c>N·L</c> from [-1, 1] into [0, 1] so that half spreads across the low levels
    /// instead — but the lit half now has only the upper half of the range to work with, which at seven quantised
    /// levels is a real loss of the contrast that reads as surface detail.
    /// </para>
    /// <para>
    /// <b>Default was flipped to off</b> after comparing the two on the reference models: the lit side is what you
    /// are usually looking at, and the extra contrast there is worth more than rescuing a shaded side the occlusion
    /// pass and the silhouettes already give shape to. Measured on the bunny, mean contrast between neighbouring
    /// lit cells is 9.0 wrapped against 14.3 clamped, where the flat renderer scores 11.5 — which is why the flat
    /// renderer looked <em>sharper</em> than this one while wrapping was on. It costs ~12% more ANSI bytes a frame
    /// (17,034 → 19,112 at 200×50), because a wider spread of levels coalesces into fewer runs.
    /// </para>
    /// </remarks>
    public bool WrapLighting { get; set; }

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
        // Darken first, then outline. Outlining after the occlusion pass keeps edge glyphs at full brightness
        // instead of having it mute the very lines that define the shape.
        HalfBlocks.ApplyOcclusion(OcclusionStrength);
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
    /// <summary>The default for <see cref="MeshRenderer.ShadeLevels"/>, so a UI offering the dial can mark it.</summary>
    public const float DefaultShadeLevels = 7f;

    // How sharply the inverse-depth field must bend, relative to local depth, to count as an edge. Low enough to
    // catch a box crease seen face-on, high enough that a sphere's curvature does not light up its whole interior.
    private const float EdgeThreshold = 0.09f;
    #endregion
}
