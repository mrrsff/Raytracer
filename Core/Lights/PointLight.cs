using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;
using Raytracer.Rendering;

namespace Raytracer.Core.Lights;

public class PointLight : Light
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

    public override bool Sample(in Vector3 P, in Vector3 N, float time, Renderer renderer, out Vector3 L, out Vector3 irradiance)
    {
        if (renderer.Scene.IsOccluded(P, Position, N, time))
        {
            L = irradiance = default;
            return false;
        }

        Vector3 toLight = Position - P;
        float dist2 = toLight.LengthSquared();
        L = Vector3.Normalize(toLight);
        irradiance = Intensity / dist2;
        return true;
    }

    public void CalculatePosition()
    {
        var transformMatrix = Transform.Matrix;
        Position = Vector3.Transform(Position, transformMatrix);
    }
}