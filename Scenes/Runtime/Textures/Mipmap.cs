using System.Numerics;
using Raytracer.Core;
using Raytracer.Rendering.Intersections;
using Raytracer.Scenes.Content.Datas.Camera;

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
        Vector3 normal = hit.GeometricNormal;
        
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
        
        Ray ri = cam.GenerateRayDRT(px + 1, py);
        Ray rj = cam.GenerateRayDRT(px, py + 1);
        
        float nx = MathF.Abs(normal.X);
        float ny = MathF.Abs(normal.Y);
        float nz = MathF.Abs(normal.Z);

        int[] indices;

        if (nz > nx && nz > ny)
        {
            indices = [0, 1];
        }
        else if (ny > nx) 
        {
            indices = [0, 2];
        }
        else 
        {
            indices = [1, 2];
        }
        
        Vector3 p = hit.Point;
        Vector3 p_ri = IntersectRayWithPlane(ri, p, normal);
        Vector3 p_rj = IntersectRayWithPlane(rj, p, normal);
        
        Vector3 dpdi = p_ri - p;
        Vector3 dpdj = p_rj - p;
        
        hit.HitGeometry.GetTBN(p, hit.PrimitiveIndex, hit.RayTime, out Vector3 tangent, out Vector3 bitangent, out _);
        
        // Debug.Log($"Tangent Length: {tangent.Length()}, Bitangent Length: {bitangent.Length()}");
        
        Vector3 dp_du = tangent;
        Vector3 dp_dv = bitangent;
        
        Vector2 a = new Vector2(dp_du[indices[0]], dp_du[indices[1]]);
        Vector2 b = new Vector2(dp_dv[indices[0]], dp_dv[indices[1]]);
        Vector2 c_di = new Vector2(dpdi[indices[0]], dpdi[indices[1]]);
        Vector2 c_dj = new Vector2(dpdj[indices[0]], dpdj[indices[1]]);
        
        Vector2 duvdi = SolveSystem(a, b, c_di);
        Vector2 duvdj = SolveSystem(a, b, c_dj);

        var duvMax = duvdj.Length() > duvdi.Length() ? duvdj : duvdi;

        float A = duvMax.X * width;
        float B = duvMax.Y * height;
        float AA = A * A;
        float BB = B * B;
        float level = 0.5f + MathF.Log2(AA + BB);
        float resultLevel = MathF.Max(0f, level);
        // Debug.Log($"Computed Mip Level: {level} (AA: {AA}, BB: {BB})");
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
    
    private static Vector3 IntersectRayWithPlane(Ray ray, Vector3 planePoint, Vector3 planeNormal)
    {
        float denom = Vector3.Dot(planeNormal, ray.Direction);
        if (MathF.Abs(denom) < 1e-6f)
        {
            return ray.Origin; // Ray is parallel to the plane
        }

        float t = Vector3.Dot(planePoint - ray.Origin, planeNormal) / denom;
        return ray.Origin + t * ray.Direction;
    }
}