using System.Numerics;
using Raytracer.Core;
using Raytracer.Rendering.CPU.Intersections;
using Raytracer.Scenes.Content.Datas.CameraData;

namespace Raytracer.Scenes.Runtime.Textures;

internal struct Mipmap
{
    public int Width;
    public int Height;
    public Vector3[] Pixels;

    public static Mipmap Create(int level, int originalWidth, int originalHeight, Vector3[] originalPixels)
    {
        int mipWidth = Math.Max(1, originalWidth >> level);
        int mipHeight = Math.Max(1, originalHeight >> level);
        Vector3[] mipPixels = new Vector3[mipWidth * mipHeight];
        int mult = 1 << level;
        for (int y = 0; y < mipHeight; y++)
        {
            for (int x = 0; x < mipWidth; x++)
            {
                Vector3 sum = Vector3.Zero;
                int count = 0;
                for (int dy = 0; dy < mult; dy++)
                {
                    for (int dx = 0; dx < mult; dx++)
                    {
                        int srcX = x * mult + dx;
                        int srcY = y * mult + dy;
                        if (srcX >= originalWidth || srcY >= originalHeight) continue;
                        sum += originalPixels[srcY * originalWidth + srcX];
                        count++;
                    }
                }

                mipPixels[y * mipWidth + x] = sum / count;
            }
        }

        return new Mipmap
        {
            Width = mipWidth,
            Height = mipHeight,
            Pixels = mipPixels
        };
    }
    
    public static float ComputeMipLevel(IntersectionInfo hit, int width, int height)
    {
        Camera cam = hit.Camera;
        int px = hit.XPixel;
        int py = hit.YPixel;
        Vector3 normal = hit.Normal;
        
        // To support mipmapping, we need to compute two vectors and find their maximum
        // Tex coord change per pixel along the horizontal image direction
        // Tex coord change per pixel along the vertical image direction
    
        /*
         * World position change w.r.t. image index i = (dp/di)
         * World position change w.r.t. texture coord u * Tex coord u change w.r.t. image index i + (dp/du * du/di)
         * World position change w.r.t. texture coord v * Tex coord v change w.r.t. image index i + (dp/dv * dv/di)
         * 
         * World position change w.r.t. image index j = (dp/dj)
         * World position change w.r.t. texture coord u * Tex coord u change w.r.t. image index j + (dp/du * du/dj)
         * World position change w.r.t. texture coord v * Tex coord v change w.r.t. image index j + (dp/dv * dv/dj)
         *
         * dp/di and dp/dj can be approximated by generating differential rays rx and ry
         *
         * We need to find du/di, dv/di, du/dj, dv/dj
         *
         * du/di, dv/di can be found by solving: (for z discarded)
         *
         * [ du/di ] = [ dp/du.x   dp/dv.x ]^-1 * [ dp/di.x ]
         * [ dv/di ]   [ dp/du.y   dp/dv.y ]      [ dp/di.y ]
         *
         * or same for j:
         *
         * [ du/dj ] = [ dp/du.x   dp/dv.x ]^-1 * [ dp/dj.x ]
         * [ dv/dj ]   [ dp/du.y   dp/dv.y ]      [ dp/dj.y ]
         *
         */
        
        float nx = MathF.Abs(normal.X);
        float ny = MathF.Abs(normal.Y);
        float nz = MathF.Abs(normal.Z);

        int[] indices;

        if (nz > nx && nz > ny) indices = [0, 1];
        else if (ny > nx) indices = [0, 2];
        else indices = [1, 2];
        
        Ray r1 = cam.GenerateRayDRT(px + 1, py);
        Ray r2 = cam.GenerateRayDRT(px, py + 1);

        Vector3 p = hit.Point;
        if (!IntersectRayWithPlane(r1, p, normal, out Vector3 p_r1) || !IntersectRayWithPlane(r2, p, normal, out Vector3 p_r2))
            return float.MaxValue;

        Vector3 dpd1 = p_r1 - p;
        Vector3 dpd2 = p_r2 - p;
        
        hit.HitGeometry.GetTBN(p, hit.PrimitiveIndex, hit.RayTime, out Vector3 tangent, out Vector3 bitangent, out _);
        
        Vector3 dp_du = tangent;
        Vector3 dp_dv = bitangent;
        
        Vector2 a = new Vector2(dp_du[indices[0]], dp_du[indices[1]]);
        Vector2 b = new Vector2(dp_dv[indices[0]], dp_dv[indices[1]]);
        Vector2 c_d1 = new Vector2(dpd1[indices[0]], dpd1[indices[1]]);
        Vector2 c_d2 = new Vector2(dpd2[indices[0]], dpd2[indices[1]]);
        
        Vector2 duvd1 = SolveSystem(a, b, c_d1);
        Vector2 duvd2 = SolveSystem(a, b, c_d2);

        var duvMax = duvd2.Length() > duvd1.Length() ? duvd2 : duvd1;

        float A = duvMax.X * width;
        float B = duvMax.Y * height;
        float AA = A * A;
        float BB = B * B;
        float level = 0.5f + MathF.Log2(AA + BB);
        float resultLevel = MathF.Max(0f, level);

        return resultLevel;
    }

    private static Vector2 SolveSystem(Vector2 a, Vector2 b, Vector2 c)
    {
        // result = [a b]^-1 * c
        
        float det = a.X * b.Y - a.Y * b.X;
        if (MathF.Abs(det) < 1e-6f)
            return Vector2.Zero; // Singular matrix
        
        float invDet = 1.0f / det;
        float x = (c.X * b.Y - c.Y * b.X) * invDet;
        float y = (a.X * c.Y - a.Y * c.X) * invDet;
        return new Vector2(x, y);
    }
    
    public static bool IntersectRayWithPlane(
        in Ray ray,
        in Vector3 planePoint,
        in Vector3 planeNormal,
        out Vector3 hitPoint)
    {
        float denom = Vector3.Dot(planeNormal, ray.Direction);

        if (MathF.Abs(denom) < 1e-6f)
        {
            Debug.Log("Ray is parallel to the plane, plane normal: " + planeNormal + ", ray direction: " + ray.Direction);
            hitPoint = default;
            return false;
        }

        float t = Vector3.Dot(planePoint - ray.Origin, planeNormal) / denom;

        if (t <= 0.0f)
        {
            hitPoint = default;
            return false;
        }

        hitPoint = ray.Origin + t * ray.Direction;
        return true;
    }
}