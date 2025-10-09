using System.Text;
using System.Text.Json.Serialization;

namespace Raytracer.Scenes.Content.Datas.Objects;

public struct MeshData
{
    [JsonPropertyName("_id")] public int Id;
    public int Material;
    public FacesData Faces;

    public override string ToString()
    {
        return new StringBuilder()
            .AppendLine($"Mesh ID: {Id}")
            .AppendLine($"Material ID: {Material}")
            .AppendLine($"Faces: {Faces}")
            .ToString();
    }
}