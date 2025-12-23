using System.Numerics;
using Raytracer.Core;
using Raytracer.Rendering.CPU.Intersections;
using Raytracer.Scenes.Content.Datas.Textures;
using Raytracer.Scenes.Runtime.Textures.Procedural;

namespace Raytracer.Scenes.Runtime.Textures;

public class PerlinTexture : Texture
{
    public override TextureType TextureType => TextureType.Perlin;
    private readonly float NoiseScale;
    public readonly NoiseConversionType NoiseConversionType;
    private readonly int NumOctaves;

    public PerlinTexture(TextureInfo textureInfo) : base(textureInfo)
    {
        NoiseScale = textureInfo.NoiseScale != 0 ? textureInfo.NoiseScale : 1f;
        NoiseConversionType = textureInfo.NoiseConversion.ToNoiseConversionType();
        NumOctaves = textureInfo.NumOctaves <= 0 ? 1 : textureInfo.NumOctaves;
    }
    public override Vector3 Sample(IntersectionInfo info)
    {
        // Sample in world space coordinates
        Vector3 point = info.Point;
        float displayValue = Noise3D(point);
        return new Vector3(displayValue, displayValue, displayValue);
    }
    
    public Vector3 Sample(Vector3 point)
    {
        float displayValue = Noise3D(point);
        return new Vector3(displayValue, displayValue, displayValue);
    }

    public override Vector3 SampleFromUV(Vector2 uv)
    {
        float displayValue = Noise2D(uv);
        return new Vector3(displayValue, displayValue, displayValue);
    }
    
    private float Noise3D(Vector3 point)
    {
        float noiseValue = 0f;
        for (int octave = 0; octave < NumOctaves; octave++)
        {
            float frequency = MathF.Pow(2, octave);
            float amplitude = 1f / frequency;
            var samplePoint = point * NoiseScale * frequency;
            noiseValue += Perlin.Noise(samplePoint.X, samplePoint.Y, samplePoint.Z) * amplitude;
        }
        float displayValue = NoiseConversionType == NoiseConversionType.Absolute ? MathF.Abs(noiseValue) : (noiseValue + 1f) * 0.5f;
        
        return displayValue;
    }
    
    private float Noise2D(Vector2 uv)
    {
        float noiseValue = 0f;
        for (int octave = 0; octave < NumOctaves; octave++)
        {
            float frequency = MathF.Pow(2, octave);
            float amplitude = 1f / frequency;
            noiseValue += Perlin.Noise(uv.X * NoiseScale * frequency, uv.Y * NoiseScale * frequency) * amplitude;
        }
        float displayValue = NoiseConversionType == NoiseConversionType.Absolute ? MathF.Abs(noiseValue) : (noiseValue + 1f) * 0.5f;
        
        return displayValue;
    }
}