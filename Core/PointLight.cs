using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;

namespace Raytracer.Core;

public class PointLight
{
    [JsonPropertyName("_id")] public int Id;
    public string Transformations;
    public Vector3 Position;
    public Vector3 Intensity;

    public Transform Transform = new();

    public override string ToString()
    {
        return new StringBuilder().Append("PointLight(Id: ")
            .Append(Id)
            .Append(", Position: ")
            .Append(Position)
            .Append(", Intensity: ")
            .Append(Intensity)
            .Append(')').ToString();
    }
    
    public void CalculatePosition()
    {
        var transformMatrix = Transform.Matrix;
        Position = Vector3.Transform(Position, transformMatrix);
    }
}