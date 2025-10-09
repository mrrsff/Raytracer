using System.Numerics;
using System.Text.Json.Serialization;

namespace Raytracer.Core.Datas;

public struct PlaneData
{
    [JsonPropertyName("_id")] public int Id;
    public int Material;
    public int Point;
    public Vector3 Normal;
}