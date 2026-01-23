using System.Numerics;
using System.Runtime.CompilerServices;
using Raytracer.Core;
using Raytracer.Rendering;
using Raytracer.Rendering.Intersections;
using Raytracer.Rendering.Raytracing;

namespace Raytracer.Scenes.Runtime.Meshes;

public class BoundingBox(Vector3 min, Vector3 max) : Geometry
{
    public static BoundingBox Invalid => new(Vector3.PositiveInfinity, Vector3.NegativeInfinity);
    public Vector3 Min { get; private set; } = min;
    public Vector3 Max { get; private set; } = max;
    public Vector3 Size { get; private set; } = max - min;
    public Vector3 Center { get; private set; } = (min + max) * 0.5f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Intersects(Ray ray, out float tMin, out float tMax)
    {
        tMin = 0f;
        tMax = float.MaxValue;
        Vector3 invDir = new Vector3(
            1f / ray.Direction.X,
            1f / ray.Direction.Y,
            1f / ray.Direction.Z
        );

        for (int i = 0; i < 3; i++)
        {
            float origin = ray.Origin[i];
            float direction = ray.Direction[i];
            float min = Min[i];
            float max = Max[i];

            if (MathF.Abs(direction) < Renderer.IntersectionTestEpsilon)
            {
                // Ray is parallel to slab; if origin not within slab, no hit
                if (origin < min || origin > max)
                    return false;
                continue;
            }

            float t1 = (min - origin) * invDir[i];
            float t2 = (max - origin) * invDir[i];

            if (t1 > t2)
                (t1, t2) = (t2, t1);

            if (t1 > tMin) tMin = t1;
            if (t2 < tMax) tMax = t2;

            if (tMin > tMax)
                return false;
        }

        return tMax >= MathF.Max(tMin, 0.0f);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Intersect(in Ray ray, ref IntersectionInfo info)
    {
        return Intersects(ray, out _, out _);
    }

    public override Vector2 GetUVCoordinates(in Vector3 point, in int primitiveIndex, float rayTime, bool tiling)
    {
        return Vector2.Zero;
    }

    public override Vector3 GetNormal(in Vector3 point, in int primitiveIndex, float rayTime)
    {
        return Vector3.Zero;
    }
    public override void GetTBN(in Vector3 point, in int primitiveIndex, float rayTime, out Vector3 tangent, out Vector3 bitangent,
        out Vector3 normal)
    {
        tangent = Vector3.Zero;
        bitangent = Vector3.Zero;
        normal = Vector3.Zero;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Encapsulate(Vector3 point)
    {
        Min = Vector3.Min(Min, point);
        Max = Vector3.Max(Max, point);
        Size = Max - Min;
        Center = (Min + Max) * 0.5f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Encapsulate(Triangle triangle)
    {
        Encapsulate(triangle.V0);
        Encapsulate(triangle.V1);
        Encapsulate(triangle.V2);
    }

    public override string ToString()
    {
        return $"BoundingBox(Min: {Min}, Max: {Max})";
    }
    
    public Vector3[] GetCorners()
    {
        return
        [
            Min,
            new Vector3(Min.X, Min.Y, Max.Z),
            new Vector3(Min.X, Max.Y, Min.Z),
            new Vector3(Max.X, Min.Y, Min.Z),
            new Vector3(Min.X, Max.Y, Max.Z),
            new Vector3(Max.X, Min.Y, Max.Z),
            new Vector3(Max.X, Max.Y, Min.Z),
            Max
        ];
    }
}