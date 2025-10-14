using System.Numerics;
using Raytracer.Core;

namespace Raytracer.Scenes.Runtime.Meshes;

public class BoundingBox
{
    public Vector3 Min { get; private set; }
    public Vector3 Max { get; private set; }
    
    public BoundingBox(Vector3 min, Vector3 max)
    {
        Min = min;
        Max = max;
    }

    public bool Intersects(Ray ray)
    {
        float tMin = (Min.X - ray.Origin.X) / ray.Direction.X;
        float tMax = (Max.X - ray.Origin.X) / ray.Direction.X;

        if (tMin > tMax)
        {
            (tMin, tMax) = (tMax, tMin);
        }

        float tyMin = (Min.Y - ray.Origin.Y) / ray.Direction.Y;
        float tyMax = (Max.Y - ray.Origin.Y) / ray.Direction.Y;

        if (tyMin > tyMax)
        {
            (tyMin, tyMax) = (tyMax, tyMin);
        }

        if ((tMin > tyMax) || (tyMin > tMax))
            return false;

        if (tyMin > tMin)
            tMin = tyMin;

        if (tyMax < tMax)
            tMax = tyMax;

        float tzMin = (Min.Z - ray.Origin.Z) / ray.Direction.Z;
        float tzMax = (Max.Z - ray.Origin.Z) / ray.Direction.Z;

        if (tzMin > tzMax)
        {
            (tzMin, tzMax) = (tzMax, tzMin);
        }

        if ((tMin > tzMax) || (tzMin > tMax))
            return false;

        if (tzMin > tMin)
            tMin = tzMin;

        if (tzMax < tMax)
            tMax = tzMax;

        return tMax >= MathF.Max(tMin, 0.0f);
    }
}