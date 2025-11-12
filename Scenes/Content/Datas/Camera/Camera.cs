using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using Raytracer.Core;

namespace Raytracer.Scenes.Content.Datas.Camera;

public enum CameraType
{
    None,
    LookAt
}

public class Camera
{
    [JsonPropertyName("_id")] public int Id;
    [JsonPropertyName("_type")] public CameraType Type;
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

    public void InitializeCamera()
    {
        if (Type == CameraType.LookAt)
        {
            Gaze = Vector3.Normalize(GazePoint - Position);

            float aspect = (float)ImageResolution.Width / ImageResolution.Height;
            NearPlane.Top = NearDistance * MathF.Tan(FovY * MathF.PI / 360.0f);
            NearPlane.Bottom = -NearPlane.Top;
            NearPlane.Right = aspect * NearPlane.Top;
            NearPlane.Left = -NearPlane.Right;
        }

        var w = Vector3.Normalize(-Gaze);
        var v = Vector3.Normalize(Up - Vector3.Dot(Up, w) * w);
        var u = Vector3.Cross(v, w);

        Forward = -w; // points into scene
        Right = u;
        Up = v;

        m = Position + Forward * NearDistance;
        q = m + (NearPlane.Left * Right) + (NearPlane.Top * Up);

        sUMultiplier = (NearPlane.Right - NearPlane.Left) / ImageResolution.Width;
        sVMultiplier = (NearPlane.Top - NearPlane.Bottom) / ImageResolution.Height;
    }

    public Vector3 Forward;
    public Vector3 Right;
    private Vector3 m;
    private Vector3 q;
    private float sUMultiplier;
    private float sVMultiplier;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Ray GetPrimaryRay(int i, int j)
    {
        float sU = (i + 0.5f) * sUMultiplier;
        float sV = (j + 0.5f) * sVMultiplier;

        Vector3 s = q + (sU * Right) - (sV * Up);
        Vector3 d = Vector3.Normalize(s - Position);
        return new Ray(Position, d);
    }
}