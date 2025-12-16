using System.Text;
using System.Text.Json.Serialization;

namespace Raytracer.Scenes.Content.Datas.Objects;

public struct TriangleData
{
    [JsonPropertyName("_id")] public int Id;
    public int Material;
    public int[] indices;
    public int[] Textures;

    public override string ToString()
    {
        return new StringBuilder().Append("Triangle(Id: ")
            .Append(Id)
            .Append(", Material Id: ")
            .Append(Material)
            .Append(", Vertex Indices: [")
            .Append(string.Join(", ", indices))
            .Append("])")
            .ToString();
    }
}