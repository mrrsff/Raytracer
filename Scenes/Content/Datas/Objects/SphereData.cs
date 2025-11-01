using System.Text.Json.Serialization;

namespace Raytracer.Scenes.Content.Datas.Objects;

public struct SphereData
{
    [JsonPropertyName("_id")] public int Id;
    public int Center;
    public float Radius;
    public int Material;
    public string Transformations;

    public override string ToString()
    {
        return $"Sphere (ID: {Id}) - Center Vertex Index: {Center}, Radius: {Radius}, Material Index: {Material}";
    }
}