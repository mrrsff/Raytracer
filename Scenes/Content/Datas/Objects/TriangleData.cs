using System.Text;
using System.Text.Json.Serialization;

namespace Raytracer.Scenes.Content.Datas.Objects;

public struct TriangleData
{
    [JsonPropertyName("_id")] public int Id;
    public int Material;
    public int[] indices;

    public override string ToString()
    {
        return new StringBuilder()
            .AppendLine($"Triangle ID: {Id}")
            .AppendLine($"Material ID: {Material}")
            .AppendLine($"Vertex Indices: [{string.Join(", ", indices)}]")
            .ToString();
    }
}