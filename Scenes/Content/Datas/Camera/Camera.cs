using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using Raytracer.Core;
using Raytracer.IO.SceneLoaders.Converters;
using Raytracer.Utility;

namespace Raytracer.Scenes.Content.Datas.Camera;

public enum CameraType { None, LookAt }
public enum Handedness { right, left }
public enum RendererType { RayTracing, PathTracing }
public enum RendererParams { NextEventEstimation, MIS_BALANCE, ImportanceSampling, RussianRoulette }

public class Camera
{
    [JsonPropertyName("_id")] public int Id;
    [JsonPropertyName("_type")] public CameraType Type;
    [JsonPropertyName("_handedness")] public Handedness Handedness = Handedness.right;
    public RendererType Renderer = RendererType.RayTracing;
    
    [JsonConverter(typeof(HashSetEnumConverter<RendererParams>))]
    public HashSet<RendererParams> RendererParams = [];

    public int SplittingFactor = 0;
    public int MaxRecursionDepth = 0;
    public int MinRecursionDepth = 0;
    public float SampleMaxVal = float.MaxValue;
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
        var sb = new StringBuilder();
        sb.Append("Camera(Id: ");
        sb.Append(Id);
        sb.Append(", Type: ");
        sb.Append(Type);
        sb.Append(", Handedness: ");
        sb.Append(Handedness);
        sb.Append(", Renderer: ");
        sb.Append(Renderer);
        sb.Append(", Parameters: [");
        foreach (var p in RendererParams)
        {
            sb.Append(p);
            sb.Append(", ");
        }
        sb.Append(']');
        sb.Append(", SplittingFactor: ");
        sb.Append(SplittingFactor);
        sb.Append(", Position: ");
        sb.Append(Position);
        sb.Append(", Gaze: ");
        sb.Append(Gaze);
        sb.Append(", Up: ");
        sb.Append(Up);
        sb.Append(", NearPlane: ");
        sb.Append(NearPlane);
        sb.Append(", NearDistance: ");
        sb.Append(NearDistance);
        sb.Append(", NumSamples: ");
        sb.Append(NumSamples);
        sb.Append(", ImageResolution: ");
        sb.Append(ImageResolution);
        sb.Append(", ImageName: ");
        sb.Append(ImageName);
        sb.Append(')');
        return sb.ToString();
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
        Vector3 u = Handedness == Handedness.right ? Vector3.Cross(v, w) : Vector3.Cross(w, v);

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
    public bool Has(RendererParams p) => RendererParams.Contains(p);
}