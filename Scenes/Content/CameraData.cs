using System.Numerics;
using System.Text.Json.Serialization;

namespace Raytracer.Scenes.Datas;

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
}