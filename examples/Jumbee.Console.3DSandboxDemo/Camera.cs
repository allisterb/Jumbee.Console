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

    /// <summary>Restores the opening shot.</summary>
    public void Reset()
    {
        Theta = MathF.PI / 4;
        Phi = MathF.PI / 3;
        Distance = 20f;
        Target = new Vector3(0, 1, 0);
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
    private const float MinDistance = 2f;
    private const float MaxDistance = 60f;

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
    #endregion
}
