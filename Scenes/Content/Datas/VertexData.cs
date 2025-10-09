using System.Text.Json.Serialization;

namespace Raytracer.Scenes.Content.Datas;

public struct VertexData
{
    [JsonPropertyName("_data")] public float[] Positions;

    public override string ToString()
    {
        return $"VertexData(Positions: [{string.Join(", ", Positions)}])";
    }
}