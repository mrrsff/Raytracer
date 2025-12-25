using System.Numerics;
using System.Text.Json.Serialization;
using Raytracer.Rendering.CPU;
using Raytracer.Scenes.Runtime.Textures;

namespace Raytracer.Core.Lights;

[Serializable]
public class SphericalDirectionalLight : Light
{
    [JsonPropertyName("_id")] public int Id;
    [JsonPropertyName("_type")] public SphericalDirectionalLightType Type;
    public int ImageId;
    public SamplerType Sampler;
    
    private HDRImage _image;
    public void Initialize(HDRImage image)
    {
        _image = image;
        Debug.Log($"Initialized SphericalDirectionalLight Id: {Id} with ImageId: {ImageId}");
    }
    
    public override bool Sample(in Vector3 P, in Vector3 N, float time, Renderer renderer, out Vector3 L, out Vector3 irradiance)
    {
        if (_image == null)
        {
            L = Vector3.Zero;
            irradiance = Vector3.Zero;
            return false;
        }
        
        Vector3 direction = Rendering.CPU.Sampling.Sampler.RandomUnitVectorSphere();
        if (Vector3.Dot(direction, N) < 0)
        {
            direction = -direction;
        }
        L = Vector3.Normalize(direction);
        
        Vector2 uv = GetUV(direction);
        
        Vector3 radiance = _image.SampleFromUV(uv);
        
        float NdotL = MathF.Max(Vector3.Dot(N, L), 0f);
        irradiance = radiance * NdotL * 2 * MathF.PI;
        return irradiance != Vector3.Zero;
    }

    public override string ToString()
    {
        return $"SphericalDirectionalLight(Id: {Id}, Type: {Type}, ImageId: {ImageId}, Sampler: {Sampler})";
    }
    
    public Vector2 GetUV(Vector3 direction)
    {
        return Type switch
        {
            SphericalDirectionalLightType.latlong => LatlongUV(direction),
            SphericalDirectionalLightType.probe => ProbeUV(direction),
            _ => throw new NotImplementedException()
        };
    }
    private Vector2 LatlongUV(Vector3 direction)
    {
        float u = (1 + (MathF.Atan2(direction.Z, direction.X) / MathF.PI)) * 0.5f;
        float v = (MathF.Acos(direction.Y) / MathF.PI);
        return new Vector2(u, v);
    }
    private Vector2 ProbeUV(Vector3 direction)
    {
        float r = MathF.Acos(-direction.Z) / (MathF.PI * (MathF.Pow(direction.X * direction.X + direction.Y * direction.Y, 0.5f)));
        float u = (direction.X * r + 1) / 2f;
        float v = (-direction.Y * r + 1) / 2f;
        return new Vector2(u, v);
    }
    
    public Vector3 SampleFromUV(Vector2 uv)
    {
        return _image.SampleFromUV(uv);
    }
}

public enum SphericalDirectionalLightType
{
    latlong,
    probe
}
public enum SamplerType
{
    cosine
}