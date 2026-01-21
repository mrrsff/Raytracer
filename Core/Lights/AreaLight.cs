using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;
using Raytracer.Rendering;
using Raytracer.Rendering.Sampling;
using Raytracer.Utility;

namespace Raytracer.Core.Lights;

public class AreaLight : ILight
{
    [JsonPropertyName("_id")] public int Id;
    public string Transformations;
    public Vector3 Position;
    public Vector3 Normal;
    public float Size;
    public Vector3 Radiance;
    
    public Transform Transform;
    
    public float Area => Size * Size;

    public Vector3 U = Vector3.Zero;
    public Vector3 V = Vector3.Zero;
    
    public override string ToString()
    {
        return new StringBuilder().Append("AreaLight(Id: ")
            .Append(Id)
            .Append(", Position: ")
            .Append(Position)
            .Append(", Normal: ")
            .Append(Normal)
            .Append(", Radiance: ")
            .Append(Radiance)
            .Append(", Size: ")
            .Append(Size)
            .Append(')').ToString();
    }

    public void Initialize()
    {
        // Generate orthonormal basis (U, V) for the area light's plane
        MathUtility.BuildONB(Normal, out U, out V);
    }

    public bool Sample(in Vector3 P, in Vector3 N, float time, Renderer renderer, out Vector3 L, out Vector3 irradiance)
    {
        Vector2 sample = Sampler.UniformRandom();
                
        Vector3 samplePosition =
            Position +
            Size * (sample.X - 0.5f) * U +
            Size * (sample.Y - 0.5f) * V;

        if (renderer.Scene.IsOccluded(P, samplePosition, N, time))
        {
            L = irradiance = default;
            return false;
        }
        
        var lightDelta = samplePosition - P;
        L = Vector3.Normalize(lightDelta);

        float cosLight = Vector3.Dot(Normal, -L);
        cosLight = MathF.Abs(cosLight);
        irradiance = Radiance * (Area * cosLight / lightDelta.LengthSquared());
        return true;
    }
}