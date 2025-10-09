using System.Text;
using System.Text.Json.Serialization;

namespace Raytracer.Scenes.Content.Datas.Objects;

public struct SphereData
{
    [JsonPropertyName("_id")] public int Id;

    public int Center;
    public float Radius;
    public int Material;

    public override string ToString()
    {
        return new StringBuilder().Append("Sphere(Id: ")
            .Append(Id)
            .Append(", Center Vertex Id: ")
            .Append(Center)
            .Append(", Radius: ")
            .Append(Radius)
            .Append(", Material Id: ")
            .Append(Material)
            .Append(")")
            .ToString();
    }
}