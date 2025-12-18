using System.Numerics;
using System.Runtime.CompilerServices;
using Raytracer.Core;
using Raytracer.Scenes.Content.Datas.Camera;
using Raytracer.Scenes.Content.Datas.Textures;
using Raytracer.Scenes.Runtime;
using Raytracer.Scenes.Runtime.Textures;

namespace Raytracer.Rendering.Intersections;

public struct IntersectionInfo()
{
    public Vector3 RayOrigin;
    public Material? material = null;
    public Texture[]? Textures = null;
    public int PrimitiveIndex = -1;
    public bool Hit = false;
    public float Distance = float.MaxValue;
    public Vector3 Point = default;
    public Vector3 Normal = default;
    public float IntersectionTestEpsilon;
    public Geometry HitGeometry = null!;
    public int XPixel = 0;
    public int YPixel = 0;
    public Camera Camera;
    public float RayTime = 0f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Reset()
    {
        RayOrigin = default;
        material = null;
        PrimitiveIndex = -1;
        Hit = false;
        Distance = float.MaxValue;
        Point = default;
        Normal = default;
    }

    public static IntersectionInfo NoHit => new IntersectionInfo() { };
    
    public Vector2 GetUVCoordinates(bool tiling)
    {
        return HitGeometry.GetUVCoordinates(Point, PrimitiveIndex, RayTime, tiling);
    }
    
    public void CalculateNormal()
    {
        if (!Hit) return;
        if (HitGeometry == null)
            return;
        
        var normalTexture = Textures?.FirstOrDefault(t => t.DecalType == DecalType.ReplaceNormal);
        if (normalTexture != null)
        {
            Vector2 uv = GetUVCoordinates(normalTexture.TextureType != TextureType.Image);
            Vector3 normalFromTexture = normalTexture.SampleNormalFromUV(uv);

            HitGeometry.GetTBN(Point, PrimitiveIndex, RayTime, out Vector3 tangent, out Vector3 bitangent,
                out Vector3 normal);

            Vector3 TBNNormal = normalFromTexture.X * tangent +
                                normalFromTexture.Y * bitangent +
                                normalFromTexture.Z * normal;
            Normal = Vector3.Normalize(TBNNormal);
            return;
        }
        
        var bumpTexture = Textures?.FirstOrDefault(t => t.DecalType == DecalType.BumpNormal);        
        if (bumpTexture != null)
        {
            HitGeometry.GetTBN(Point, PrimitiveIndex, RayTime, out Vector3 tangent, out Vector3 bitangent, out Vector3 normal);
            Vector2 uv = GetUVCoordinates(bumpTexture.TextureType != TextureType.Image);
            switch (bumpTexture.TextureType)
            {
                case TextureType.Image:
                    var imageTexture = (Image)bumpTexture;
                    imageTexture.SampleHeightDerivatives(uv, out float dhdu, out float dhdv);
                    
                    dhdu *= imageTexture.BumpFactor;
                    dhdv *= imageTexture.BumpFactor;
                    
                    var dqdu = tangent + dhdu * normal;
                    var dqdv = bitangent + dhdv * normal;
                    
                    Normal = Vector3.Normalize(Vector3.Cross(dqdv, dqdu));
                    if (Vector3.Dot(Normal, normal) < 0f)
                        Normal = -Normal;
                    break;
                case TextureType.Perlin:
                    const float eps = 0.001f;
                    PerlinTexture perlinTexture = (PerlinTexture)bumpTexture;
                    var hitPoint = HitGeometry.GetMotionBlurTransform(RayTime).ToLocalPoint(Point);
                    Vector3 baseNoise = perlinTexture.Sample(hitPoint);
                    Vector3 px = perlinTexture.Sample(hitPoint + new Vector3(eps, 0f, 0f)) - baseNoise;
                    Vector3 py = perlinTexture.Sample(hitPoint + new Vector3(0f, eps, 0f)) - baseNoise;
                    Vector3 pz = perlinTexture.Sample(hitPoint + new Vector3(0f, 0f, eps)) - baseNoise;
                    Vector3 gradient = new Vector3(px.X, py.Y, pz.Z) / eps;
                    Vector3 displacement = gradient.X * tangent + gradient.Y * bitangent + gradient.Z * normal;
                    Normal = Vector3.Normalize(normal - displacement * perlinTexture.BumpFactor);
                    break;
            }
            return;
        }
        Normal = HitGeometry.GetNormal(Point, PrimitiveIndex, RayTime);
    }
}