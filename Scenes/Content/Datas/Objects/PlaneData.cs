using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;

namespace Raytracer.Scenes.Content.Datas.Objects;

public struct PlaneData
{
    [JsonPropertyName("_id")] public int Id;
    public int Material;
    public int Point;
    public Vector3 Normal;

    public override string ToString()
    {
        return new StringBuilder()
            .AppendLine($"Plane ID: {Id}")
            .AppendLine($"Material ID: {Material}")
            .AppendLine($"Point: {Point}")
            .AppendLine($"Normal: {Normal}")
            .ToString();
    }
}