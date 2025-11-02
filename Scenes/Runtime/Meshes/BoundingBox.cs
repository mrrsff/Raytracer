using System.Numerics;
using System.Runtime.CompilerServices;
using Raytracer.Core;
using Raytracer.Rendering.Intersections;

namespace Raytracer.Scenes.Runtime.Meshes;

public class BoundingBox(Vector3 min, Vector3 max) : Geometry
{
    public Vector3 Min { get; private set; } = min;
    public Vector3 Max { get; private set; } = max;
    public Vector3 Center { get; private set; } = (min + max) * 0.5f;

    public bool Intersects(Ray ray, out float tMin, out float tMax)
    {
        tMin = float.NegativeInfinity;
        tMax = float.PositiveInfinity;

        for (int i = 0; i < 3; i++)
        {
            float origin = ray.Origin[i];
            float direction = ray.Direction[i];
            float min = Min[i];
            float max = Max[i];

            if (MathF.Abs(direction) < 1e-8f)
            {
                // Ray is parallel to slab; if origin not within slab, no hit
                if (origin < min || origin > max)
                    return false;
                continue;
            }

            float t1 = (min - origin) / direction;
            float t2 = (max - origin) / direction;

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
    
    public bool IntersectEdge(in Ray ray, out float t)
    {
        t = 0f;
        if (!Intersects(ray, out float tMin, out float tMax))
            return false;

        if (tMin < 0f) tMin = tMax >= 0 ? tMax : 0f;

        Vector3 hit = ray.Origin + tMin * ray.Direction;
        const float edgeThickness = 0.01f;

        int nearCount = 0;
        for (int i = 0; i < 3; i++)
        {
            float v = hit[i];
            if (MathF.Abs(v - Min[i]) < edgeThickness ||
                MathF.Abs(v - Max[i]) < edgeThickness)
                nearCount++;
        }

        if (nearCount >= 2)
        {
            t = tMin;
            return true;
        }

        // Optional: also check exit point for rays starting inside the box
        Vector3 exit = ray.Origin + tMax * ray.Direction;
        nearCount = 0;
        for (int i = 0; i < 3; i++)
        {
            float v = exit[i];
            if (MathF.Abs(v - Min[i]) < edgeThickness ||
                MathF.Abs(v - Max[i]) < edgeThickness)
                nearCount++;
        }

        if (nearCount >= 2)
        {
            t = tMax;
            return true;
        }

        return false;
    }
}