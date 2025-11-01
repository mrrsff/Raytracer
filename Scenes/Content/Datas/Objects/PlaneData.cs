using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;

namespace Raytracer.Scenes.Content.Datas.Objects;

public struct PlaneData
{
    [JsonPropertyName("_id")] public int Id;
    public int Material;
    public int Point;
    public Vector3 Normal;
    public string Transformations;

    public override string ToString()
    {
        return $"Plane (ID: {Id}) - Point Vertex Index: {Point}, Normal: {Normal}, Material Index: {Material} Transformations: {Transformations}";
    }
}