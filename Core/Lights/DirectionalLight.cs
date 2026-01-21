using System.Numerics;
using System.Text.Json.Serialization;
using Raytracer.Rendering;

namespace Raytracer.Core.Lights;

[Serializable]
public class DirectionalLight : ILight
{
    [JsonPropertyName("_id")] public int Id;
    public Vector3 Direction;
    public Vector3 Radiance;

    public override string ToString()
    {
        return $"DirectionalLight(Direction: {Direction}, Radiance: {Radiance})";
    }

    public void Initialize()
    {
        Direction = Vector3.Normalize(Direction);
    }

    public bool Sample(in Vector3 P, in Vector3 N, float time, Renderer renderer, out Vector3 L, out Vector3 irradiance)
    {
        Vector3 fakePos = P - Direction * 1e6f;
        if (renderer.Scene.IsOccluded(P, fakePos, N, time))
        {
            L = irradiance = default;
            return false;
        }

        L = Vector3.Normalize(-Direction);
        irradiance = Radiance;
        return true;
    }
}