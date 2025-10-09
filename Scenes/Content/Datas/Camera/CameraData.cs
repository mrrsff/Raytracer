using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;
using Raytracer.Core;

namespace Raytracer.Scenes.Content.Datas.Camera;

public struct CameraData
{
    [JsonPropertyName("_id")] public int Id;

    public Vector3 Position;
    public Vector3 Gaze;
    public Vector3 Up;
    public Rect NearPlane;
    public float NearDistance;
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
            .Append(", ImageResolution: ")
            .Append(ImageResolution)
            .Append(", ImageName: ")
            .Append(ImageName)
            .Append(')').ToString();
    }
}