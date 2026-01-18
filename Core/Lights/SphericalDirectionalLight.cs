using System.Numerics;
using System.Text.Json.Serialization;
using Raytracer.Rendering;
using Raytracer.Rendering.Sampling;
using Raytracer.Scenes.Runtime.Textures;
using Raytracer.Utility;

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
        // Debug.Log($"Initialized SphericalDirectionalLight Id: {Id} with ImageId: {ImageId}");
    }
    
    public override bool Sample(in Vector3 P, in Vector3 N, float time, Renderer renderer, out Vector3 L, out Vector3 irradiance)
    {
        if (_image == null)
        {
            L = Vector3.Zero;
            irradiance = Vector3.Zero;
            return false;
        }
        
        Vector3 local = SampleLocal();
        
        MathUtility.BuildONB(N, out Vector3 T, out Vector3 B);
        L = Vector3.Normalize(local.X * T + local.Y * B + local.Z * N);
        if (Vector3.Dot(L, N) <= 0f)
        {
            irradiance = Vector3.Zero;
            return false;
        }
        Vector2 uv = GetUV(L);
        
        Vector3 radiance = _image.SampleFromUV(uv);
        
        if (Sampler == SamplerType.cosine)
        {
            irradiance = radiance * MathF.PI;
        }
        else
        {
            float NdotL = Vector3.Dot(N, L);
            irradiance = radiance * NdotL * 2f * MathF.PI;
        }

        return irradiance != Vector3.Zero;
    }

    private Vector3 SampleLocal()
    {
        float u1 = ThreadRng.NextFloat();
        float u2 = ThreadRng.NextFloat();

        if (Sampler == SamplerType.cosine)
        {
            // cosine-weighted hemisphere (z >= 0)
            float r = MathF.Sqrt(u1);
            float phi = 2f * MathF.PI * u2;

            float x = r * MathF.Cos(phi);
            float y = r * MathF.Sin(phi);
            float z = MathF.Sqrt(1f - u1);

            return new Vector3(x, y, z);
        }
        else
        {
            // uniform hemisphere (z >= 0)
            float z = u1;
            float r = MathF.Sqrt(MathF.Max(0f, 1f - z * z));
            float phi = 2f * MathF.PI * u2;

            float x = r * MathF.Cos(phi);
            float y = r * MathF.Sin(phi);

            return new Vector3(x, y, z);
        }
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
        float u = (1f + MathF.Atan2(direction.X, -direction.Z) / MathF.PI) * 0.5f;
        float v = MathF.Acos(direction.Y) / MathF.PI;
        return new Vector2(u, v);
    }
    private Vector2 ProbeUV(Vector3 direction)
    {
        float x = direction.X;
        float y = direction.Y;
        float z = -direction.Z; 

        float d = MathF.Sqrt(x * x + y * y);

        if (d < 0.0001f) 
        {
            return new Vector2(0.5f, 0.5f);
        }

        float r = (1.0f / MathF.PI) * MathF.Acos(z) / d;

        float u = x * r;
        float v = y * r;

        return new Vector2(0.5f * (u + 1.0f), 1 - 0.5f * (v + 1.0f));
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
    uniform,
    cosine
}