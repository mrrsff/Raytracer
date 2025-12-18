using System.Numerics;
using System.Runtime.CompilerServices;
using Raytracer.Core;
using Raytracer.Rendering.Intersections;
using Raytracer.Scenes.Content.Datas.Camera;
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
    protected static float ComputeMipLevel(IntersectionInfo hit, int width, int height)
    {
        Camera cam = hit.Camera;
        int px = hit.XPixel;
        int py = hit.YPixel;
        Vector3 p = hit.Point;
        Vector3 n = hit.Normal;

        // Generate differential rays
        Ray rx = cam.GenerateRay(px + 1, py);
        Ray ry = cam.GenerateRay(px, py + 1);
        
        // Plane is defined with point p and normal n
        // Find intersection of rx with the plane
        float t_x = Vector3.Dot(n, p - rx.Origin) / Vector3.Dot(n, rx.Direction);
        Vector3 pxHit = rx.Origin + t_x * rx.Direction;
        
        // Find intersection of ry with the plane
        float t_y = Vector3.Dot(n, p - ry.Origin) / Vector3.Dot(n, ry.Direction);
        Vector3 pyHit = ry.Origin + t_y * ry.Direction;

        // Compute UV coordinates of differential points
        Vector2 uv = hit.HitGeometry.GetUVCoordinates(hit.Point, hit.PrimitiveIndex, 0, true);
        Vector2 uv_x = hit.HitGeometry.GetUVCoordinates(pxHit, hit.PrimitiveIndex, 0, true);
        Vector2 uv_y = hit.HitGeometry.GetUVCoordinates(pyHit, hit.PrimitiveIndex, 0, true);

        // Rate of change
        float du_di = Math.Abs(uv_x.X - uv.X);
        float dv_dj = Math.Abs(uv_y.Y - uv.Y);

        // Footprint
        float a = du_di * width;
        float b = dv_dj * height;

        // Mip level
        float L = 0.5f * MathF.Log2(a * a + b * b);
        return MathF.Max(L, 0f);
    }
}