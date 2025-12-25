using System.Numerics;
using System.Text.Json.Serialization;
using Raytracer.Rendering.CPU;

namespace Raytracer.Core.Lights;

[Serializable]
public class SpotLight : Light
{
    [JsonPropertyName("_id")] public int Id;
    public Vector3 Position;
    public Vector3 Direction;
    public Vector3 Intensity;
    public float CoverageAngle;
    public float FalloffAngle;
    public float cosFalloff;
    public float cosCoverage;

    public override string ToString()
    {
        return $"SpotLight(Id: {Id}, Position: {Position}, Direction: {Direction}, Intensity: {Intensity}, CoverageAngle: {CoverageAngle}, FalloffAngle: {FalloffAngle})";
    }
    
    public override void Initialize()
    {
        cosFalloff = MathF.Cos(FalloffAngle * 0.5f / 180.0f * MathF.PI);
        cosCoverage = MathF.Cos(CoverageAngle * 0.5f / 180.0f * MathF.PI);
        Direction = Vector3.Normalize(Direction);
    }

    public override bool Sample(in Vector3 P, in Vector3 N, float time, Renderer renderer, out Vector3 L, out Vector3 irradiance)
    {
        Vector3 toLight = Position - P;
        float dist2 = toLight.LengthSquared();
        L = Vector3.Normalize(toLight);

        if (renderer.Scene.IsOccluded(P, Position, N, time))
        {
            irradiance = default;
            return false;
        }

        Vector3 D = Vector3.Normalize(-Direction);
        float cosTheta = Vector3.Dot(L, D);
        if (cosTheta < cosCoverage)
        {
            irradiance = default;
            return false;
        }

        irradiance = Intensity / dist2;

        if (cosTheta < cosFalloff)
        {
            float t = (cosTheta - cosCoverage) /
                      (cosFalloff - cosCoverage);
            t = Math.Clamp(t, 0f, 1f);
            irradiance *= t * t * t * t;
        }

        return true;
    }
}