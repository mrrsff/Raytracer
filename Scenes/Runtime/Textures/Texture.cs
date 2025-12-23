using System.Numerics;
using System.Runtime.CompilerServices;
using Raytracer.Core;
using Raytracer.Rendering.CPU.Intersections;
using Raytracer.Scenes.Content.Datas.Textures;

namespace Raytracer.Scenes.Runtime.Textures;

public abstract class Texture
{
    public abstract TextureType TextureType { get; }
    public DecalType DecalType;
    public InterpolationType InterpolationType;
    public float BumpFactor;
    public int Id;

    protected Texture(TextureInfo textureInfo)
    {
        DecalType = textureInfo.DecalMode.ToDecalType();
        InterpolationType = textureInfo.Interpolation.ToInterpolationType();
        BumpFactor = textureInfo.BumpFactor == 0f ? 1f : textureInfo.BumpFactor;
        Id = textureInfo.Id;
    }
    public abstract Vector3 Sample(IntersectionInfo info);
    public abstract Vector3 SampleFromUV(Vector2 uv);
    public Vector3 SampleNormalFromUV(Vector2 uv) => ColorToNormal(SampleFromUV(uv)); // [-1, 1]

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector3 ColorToNormal(Vector3 color)
    {
        return Vector3.Normalize((color * 2f) - Vector3.One);
    }
}