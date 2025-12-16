using System.Text;
using System.Text.Json.Serialization;

namespace Raytracer.Scenes.Content.Datas;
public enum FaceType
{
    Triangle,
    Quad
}
public struct FacesData
{
    [JsonPropertyName("_data")] public int[] Data;
    [JsonPropertyName("_type")] public FaceType Type;
    [JsonPropertyName("_plyFile")] public string PlyData;
    [JsonPropertyName("_vertexOffset")] public int VertexOffset;
    [JsonPropertyName("_textureOffset")] public int TextureOffset;
    

    public override string ToString()
    {
        return new StringBuilder()
            .AppendLine($"Face Type: {Type}")
            .AppendLine($"Data: [{string.Join(", ", Data)}]")
            .ToString();
    }
}