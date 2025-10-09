using System.Text.Json.Serialization;

namespace Raytracer.Core.Datas;

public struct TriangleData
{
    [JsonPropertyName("_id")] public int Id;
    public int MaterialId;
    public int[] indices;
}