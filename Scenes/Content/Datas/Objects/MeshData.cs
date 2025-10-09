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
        return new StringBuilder().Append("Mesh(Id: ")
            .Append(Id)
            .Append(", Material Id: ")
            .Append(Material)
            .Append(", Vertex Indices: [")
            .Append(Faces)
            .Append("])")
            .ToString();
    }
}