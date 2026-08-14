namespace Jumbee.Console.SandboxDemo;

using System.Numerics;

/// <summary>
/// The orbit rig: a point the camera looks at, an azimuth and elevation around it, and a distance from it.
/// </summary>
/// <remarks>
/// Owned by the UI thread — the physics thread never touches it. Everything is <see cref="float"/> because Box3D
/// speaks <see cref="System.Numerics"/>, so there is no conversion anywhere between the engine and the screen.
/// </remarks>
public sealed class OrbitCamera
{
    #region Properties
    /// <summary>Azimuth around the target, in radians. Unbounded — it just wraps.</summary>
    public float Theta { get; set; } = MathF.PI / 4;

    /// <summary>Elevation from straight up, in radians, clamped away from the poles (see <see cref="Orbit"/>).</summary>
    public float Phi { get; set; } = MathF.PI / 3;

    /// <summary>Distance from the target.</summary>
    public float Distance { get; set; } = 20f;

    /// <summary>The point orbited and looked at.</summary>
    public Vector3 Target { get; set; } = new(0, 1, 0);

    /// <summary>Where <see cref="Reset"/> puts the camera back to. Defaults to the sandbox's opening shot; the
    /// model viewer moves it to whatever height the model it is showing sits at.</summary>
    public Vector3 HomeTarget { get; set; } = new(0, 1, 0);

    /// <summary>The distance <see cref="Reset"/> restores.</summary>
    public float HomeDistance { get; set; } = 20f;

    /// <summary>Where the camera sits: spherical coordinates around <see cref="Target"/>.</summary>
    public Vector3 Eye => Target + (Distance * new Vector3(
        MathF.Sin(Phi) * MathF.Cos(Theta),
        MathF.Cos(Phi),
        MathF.Sin(Phi) * MathF.Sin(Theta)));
    #endregion

    #region Methods
    /// <summary>Swings the camera around the target. <paramref name="dPhi"/> is clamped short of either pole, where
    /// the view basis would be degenerate (forward parallel to world up, so the cross product collapses).</summary>
    public void Orbit(float dTheta, float dPhi)
    {
        Theta += dTheta;
        Phi = Math.Clamp(Phi + dPhi, MinPhi, MaxPhi);
    }

    /// <summary>Scales the distance to the target, clamped so the camera can neither enter the scene nor lose it.</summary>
    public void Zoom(float factor) => Distance = Math.Clamp(Distance * factor, MinDistance, MaxDistance);

    /// <summary>Slides the target (and so the camera) in the screen plane.</summary>
    public void Pan(float dRight, float dUp)
    {
        var view = GetView();
        Target += (view.Right * dRight) + (view.Up * dUp);
    }

    /// <summary>Restores the opening shot — the angles, and whatever <see cref="HomeTarget"/> and
    /// <see cref="HomeDistance"/> currently are.</summary>
    public void Reset()
    {
        Theta = MathF.PI / 4;
        Phi = MathF.PI / 3;
        Distance = HomeDistance;
        Target = HomeTarget;
    }

    /// <summary>Builds this frame's orthonormal view basis. Cheap — recompute it per frame rather than caching.</summary>
    public CameraView GetView()
    {
        var eye = Eye;
        var forward = Vector3.Normalize(Target - eye);
        var right = Vector3.Normalize(Vector3.Cross(forward, WorldUp));
        var up = Vector3.Cross(right, forward);
        return new CameraView(eye, right, up, forward);
    }
    #endregion

    #region Fields
    private const float MinPhi = 0.05f;
    private const float MaxPhi = MathF.PI - 0.05f;
    /// <summary>The closest the camera may sit to its target — inside this it would be in the scene.</summary>
    public const float MinDistance = 2f;

    /// <summary>The furthest the camera may sit from its target.</summary>
    public const float MaxDistance = 60f;

    private static readonly Vector3 WorldUp = new(0, 1, 0);
    #endregion
}

/// <summary>An orthonormal camera basis: enough to take a world point into camera space.</summary>
public readonly record struct CameraView(Vector3 Eye, Vector3 Right, Vector3 Up, Vector3 Forward)
{
    /// <summary>World point to camera space — distances along right, up and forward from the eye. Z is depth, and
    /// positive means in front of the camera.</summary>
    public Vector3 Transform(Vector3 world)
    {
        var rel = world - Eye;
        return new Vector3(Vector3.Dot(rel, Right), Vector3.Dot(rel, Up), Vector3.Dot(rel, Forward));
    }
}

/// <summary>
/// The screen window the scene is drawn into: how many character cells, and the NDC rectangle they map to.
/// </summary>
/// <remarks>
/// <para>
/// X always spans [-1, 1] and Y spans ±<see cref="CellAspect"/>, where the aspect is <c>2·rows/columns</c> because a
/// character cell is about twice as tall as it is wide. That keeps world units square on screen — a circle stays a
/// circle — and it letterboxes a wide terminal rather than stretching it.
/// </para>
/// <para>
/// <b>One place, deliberately.</b> The renderer uses this to set the canvas bounds and the view uses it to turn a
/// mouse cell back into a world ray; if the two ever disagreed, picking would land next to the thing you clicked
/// and nothing would say why.
/// </para>
/// </remarks>
public readonly struct Viewport
{
    #region Constructors
    /// <summary>Builds the viewport for a control of <paramref name="width"/> × <paramref name="height"/> cells.</summary>
    public Viewport(int width, int height)
    {
        Width = width;
        Height = height;
        CellAspect = width > 0 ? 2.0 * height / width : 1.0;
    }
    #endregion

    #region Properties
    /// <summary>Width in character cells.</summary>
    public int Width { get; }

    /// <summary>Height in character cells.</summary>
    public int Height { get; }

    /// <summary>Half the NDC height: Y spans ±this. See the remarks on <see cref="Viewport"/>.</summary>
    public double CellAspect { get; }

    /// <summary><see langword="true"/> when the viewport is big enough to map coordinates into.</summary>
    public bool IsValid => Width > 1 && Height > 1;
    #endregion

    #region Methods
    /// <summary>Maps a cell in this control to NDC, or returns <see langword="false"/> if it lies outside.</summary>
    public bool TryToNdc(int column, int row, out float x, out float y)
    {
        x = y = 0;
        if (!IsValid || column < 0 || row < 0 || column >= Width || row >= Height) return false;
        x = -1f + ((float)column / (Width - 1) * 2f);
        y = (float)(CellAspect - ((double)row / (Height - 1) * 2.0 * CellAspect));
        return true;
    }

    /// <summary>The world-space ray through a cell: back out of the projection to a camera-space direction, then
    /// into world space through the view basis. The inverse of <see cref="Projection.TryProject"/>.</summary>
    public bool TryRay(int column, int row, in CameraView view, in Projection projection, out Vector3 origin, out Vector3 direction)
    {
        origin = view.Eye;
        direction = default;
        if (!TryToNdc(column, row, out var nx, out var ny)) return false;

        var f = projection.Focal;
        direction = Vector3.Normalize((view.Right * (nx / f)) + (view.Up * (ny / f)) + view.Forward);
        return true;
    }
    #endregion
}

/// <summary>
/// A pinhole perspective projection: camera space to normalized device coordinates, with a near-plane reject.
/// </summary>
public readonly struct Projection
{
    #region Constructors
    /// <summary>Builds a projection with the given vertical field of view, in degrees.</summary>
    public Projection(float fovYDegrees)
    {
        Focal = 1f / MathF.Tan(fovYDegrees * (MathF.PI / 180f) / 2f);
        Near = 0.1f;
    }
    #endregion

    #region Properties
    /// <summary>The focal length implied by the field of view.</summary>
    public float Focal { get; }

    /// <summary>Points at or behind this depth do not project.</summary>
    public float Near { get; }
    #endregion

    #region Methods
    /// <summary>Projects a camera-space point, returning <see langword="false"/> for anything at or behind the near
    /// plane — which is why lines are clipped per endpoint and a body with any corner behind the camera simply loses
    /// those edges rather than drawing a wild one.</summary>
    public bool TryProject(Vector3 view, out float x, out float y)
    {
        if (view.Z <= Near)
        {
            x = y = 0;
            return false;
        }

        x = Focal * view.X / view.Z;
        y = Focal * view.Y / view.Z;
        return true;
    }

    /// <summary>Where a ray meets the plane through <paramref name="point"/> with normal <paramref name="normal"/>,
    /// or <see langword="false"/> if it runs parallel to it or would hit behind the origin. The drag plane and the
    /// ground plane are both found this way.</summary>
    public static bool TryPlaneHit(Vector3 origin, Vector3 direction, Vector3 point, Vector3 normal, out Vector3 hit)
    {
        hit = default;
        var denominator = Vector3.Dot(direction, normal);
        if (MathF.Abs(denominator) < 1e-6f) return false;

        var t = Vector3.Dot(point - origin, normal) / denominator;
        if (t <= 0) return false;

        hit = origin + (direction * t);
        return true;
    }
    #endregion
}
