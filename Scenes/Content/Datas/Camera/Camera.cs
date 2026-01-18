using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using Raytracer.Core;
using Raytracer.Utility;

namespace Raytracer.Scenes.Content.Datas.Camera;

public enum CameraType { None, LookAt }
public enum Handedness { right, left }

public class Camera
{
    [JsonPropertyName("_id")] public int Id;
    [JsonPropertyName("_type")] public CameraType Type;
    private readonly Handedness _handedness = Handedness.right;
    public Vector3 Position;
    public Vector3 Gaze;
    public Vector3 GazePoint;
    public Vector3 Up;
    public float FovY;
    public Rect NearPlane;
    public float NearDistance = 1.0f;
    public int NumSamples = 1;
    public Resolution ImageResolution;
    public string ImageName;
    public string Transformations;
    
    public float ApertureSize = 0;
    public float FocusDistance = 0f;
    public float ShutterOpen = 0.0f;
    public float ShutterClose = 1.0f;
    
    public Transform Transform = new Transform();

    public override string ToString()
    {
        return new StringBuilder().Append("Camera(Id: ")
            .Append(Id)
            .Append(", Position: ")
            .Append(Position)
            .Append(", Gaze: ")
            .Append(Gaze)
            .Append(", Up: ")
            .Append(Up)
            .Append(", NearPlane: ")
            .Append(NearPlane)
            .Append(", NearDistance: ")
            .Append(NearDistance)
            .Append(", NumSamples: ")
            .Append(NumSamples)
            .Append(", ImageResolution: ")
            .Append(ImageResolution)
            .Append(", ImageName: ")
            .Append(ImageName)
            .Append(')').ToString();
    }

    public void Initialize()
    {
        Position = Transform.ToWorldPoint(Position);
        Gaze = Transform.ToWorldDirection(Gaze);
        Up = Transform.ToWorldDirection(Up);
        
        float aspect = (float)ImageResolution.Width / ImageResolution.Height;
        if (Type == CameraType.LookAt)
        {
            Gaze = Vector3.Normalize(GazePoint - Position);

            NearPlane.Top = NearDistance * MathF.Tan(FovY * MathF.PI / 360.0f);
            NearPlane.Bottom = -NearPlane.Top;
            NearPlane.Right = aspect * NearPlane.Top;
            NearPlane.Left = -NearPlane.Right;
        }
        else
        {
            FovY = 2.0f * MathF.Atan(NearPlane.Top / NearDistance) * 180.0f / MathF.PI;
        }

        var w = Vector3.Normalize(-Gaze);
        var v = Vector3.Normalize(Up - Vector3.Dot(Up, w) * w);
        Vector3 u = _handedness == Handedness.right ? Vector3.Cross(v, w) : Vector3.Cross(w, v);

        Forward = -w; // points into scene
        Right = u;
        Up = v;

        M = Position + Forward * NearDistance;
        Q = M + (NearPlane.Left * Right) + (NearPlane.Top * Up);

        SUMultiplier = (NearPlane.Right - NearPlane.Left) / ImageResolution.Width;
        SVMultiplier = (NearPlane.Top - NearPlane.Bottom) / ImageResolution.Height;
    }

    public Vector3 Forward;
    public Vector3 Right;
    private Vector3 M;
    private Vector3 Q;
    private float SUMultiplier;
    private float SVMultiplier;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Ray GenerateRay(float i, float j)
    {
        float sU = i * SUMultiplier;
        float sV = j * SVMultiplier;

        Vector3 s = Q + (sU * Right) - (sV * Up);
        Vector3 d = Vector3.Normalize(s - Position);
        return new Ray(Position, d);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Ray GenerateRayDRT(float pixelX, float pixelY, Vector2 lensSample = default, float time = 0.0f)
    {
        float sU = pixelX * SUMultiplier;
        float sV = pixelY * SVMultiplier;

        Vector3 s = Q + (sU * Right) - (sV * Up);

        Vector3 dir = Vector3.Normalize(s - Position);

        if (ApertureSize <= 0.0f)
        {
            var r = new Ray(Position, dir);
            r.Time = MathUtility.Lerp(ShutterOpen, ShutterClose, time);
            return r;
        }
        
        Vector3 focalPoint = Position + dir * FocusDistance;

        Vector2 disk = MathUtility.ConcentricDiskSample(lensSample) * ApertureSize;
        Vector3 lensPos = Position + disk.X * Right + disk.Y * Up;

        Vector3 newDir = Vector3.Normalize(focalPoint - lensPos);

        var ray = new Ray(lensPos, newDir);
        ray.Time = MathUtility.Lerp(ShutterOpen, ShutterClose, time);
        return ray;
    }
}