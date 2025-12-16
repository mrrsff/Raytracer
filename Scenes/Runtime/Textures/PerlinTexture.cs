using System.Numerics;
using Raytracer.Core;
using Raytracer.Rendering.Intersections;
using Raytracer.Scenes.Content.Datas.Textures;
using Raytracer.Scenes.Runtime.Textures.Procedural;

namespace Raytracer.Scenes.Runtime.Textures;

public class PerlinTexture : Texture
{
    public override TextureType TextureType => TextureType.Perlin;
    public readonly float NoiseScale;
    public readonly NoiseConversionType NoiseConversionType;
    public readonly int NumOctaves;

    public PerlinTexture(TextureInfo textureInfo) : base(textureInfo)
    {
        NoiseScale = textureInfo.NoiseScale != 0 ? textureInfo.NoiseScale : 1f;
        NoiseConversionType = textureInfo.NoiseConversion.ToNoiseConversionType();
        NumOctaves = textureInfo.NumOctaves <= 0 ? 1 : textureInfo.NumOctaves;
    }
    public override Vector3 Sample(IntersectionInfo info)
    {
        Vector2 uv = info.GetUVCoordinates(true);
        return SampleUV(uv);
    }

    public override Vector3 SampleUV(Vector2 uv)
    {
        float noiseValue = 0f;
        for (int octave = 0; octave < NumOctaves; octave++)
        {
            float frequency = MathF.Pow(2, octave);
            float amplitude = 1f / frequency;
            noiseValue += Perlin.Noise(uv.X * NoiseScale * frequency, uv.Y * NoiseScale * frequency) * amplitude;
        }
        
        float displayValue = NoiseConversionType == NoiseConversionType.Absolute ? MathF.Abs(noiseValue) : (noiseValue + 1f) * 0.5f;
        
        return new Vector3(displayValue, displayValue, displayValue);
    }
}