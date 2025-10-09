using System.Drawing;
using System.Numerics;
using System.Text.Json.Serialization;

namespace Raytracer.Core.Datas;

public struct PointLight
{
    [JsonPropertyName("_id")] public int Id;
    public Vector3 Position;
    public Color Intensity;
}