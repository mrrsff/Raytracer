using System.Numerics;
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
            float aspect = (float)ImageResolution.Width / ImageResolution.Height;
            float halfHeight = (float)Math.Tan(FovY * 0.5f * MathF.PI / 180f) * NearDistance;
            float halfWidth = aspect * halfHeight;
            NearPlane = new Rect(-halfWidth, halfWidth, -halfHeight, halfHeight);
            Forward = Vector3.Normalize(GazePoint - Position);
            Gaze = -Forward;
        }
        else
        {
            Forward = Vector3.Normalize(Gaze);
        }

        // Check if gaze and up are perpendicular
        if (MathF.Abs(Vector3.Dot(Forward, Up)) > float.Epsilon)
        {
            // Recompute Up to be perpendicular to Forward
            var w = -Gaze / Gaze.Length();
            var u2 = Up / Up.Length();
            var u = Vector3.Cross(u2, w);
            var v = Vector3.Cross(w, u);
            Up = v;
        }
        
        Right = Vector3.Normalize(Vector3.Cross(Forward, Up));
        
        m = Position + (Forward * NearDistance);
        q = m + (NearPlane.Left * Right) + (NearPlane.Top * Up);
    }

    private Vector3 Forward;
    private Vector3 Right;
    private Vector3 m;
    private Vector3 q;
    public Ray GetPrimaryRay(int i, int j)
    {
        float sU = (i + 0.5f) * (NearPlane.Right - NearPlane.Left) / ImageResolution.Width;
        float sV = (j + 0.5f) * (NearPlane.Top - NearPlane.Bottom) / ImageResolution.Height;
        
        Vector3 s = q + (sU * Right) - (sV * Up);
        Vector3 d = Vector3.Normalize(s - Position);
        return new Ray(Position, d);
    }
}