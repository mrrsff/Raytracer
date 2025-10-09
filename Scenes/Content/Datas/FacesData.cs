using System.Text;
using System.Text.Json.Serialization;

namespace Raytracer.Scenes.Content.Datas;

public struct FacesData
{
    [JsonPropertyName("_data")] public int[] Data;
    [JsonPropertyName("_type")] public FaceType Type;

    public override string ToString()
    {
        return new StringBuilder()
            .AppendLine($"Face Type: {Type}")
            .AppendLine($"Data: [{string.Join(", ", Data)}]")
            .ToString();
    }
}