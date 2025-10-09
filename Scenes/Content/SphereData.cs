using System.Text.Json.Serialization;

namespace Raytracer.Core.Datas;

public struct SphereData
{
    [JsonPropertyName("_id")] public int Id;

    public int Center;
    public float Radius;
    public int Material;
}