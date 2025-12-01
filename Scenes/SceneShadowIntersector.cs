using System.Numerics;
using System.Runtime.CompilerServices;
using Raytracer.Core;
using Raytracer.Rendering;
using Raytracer.Rendering.Intersections;

namespace Raytracer.Scenes;

public partial class Scene
{
    public bool IntersectAny(Ray ray, float maxDistance)
    {
        var intersection = IntersectionInfo.NoHit;
        if (TLAS != null && TLAS.Intersect(ray, ref intersection))
        {
            if (intersection.Distance < maxDistance)
                return true;
        }
        else
        {
            foreach (var geometry in Geometries)
            {
                if (!geometry.Intersect(ray, ref intersection)) continue;
                if (intersection.Distance < maxDistance)
                    return true;
            }
        }

        foreach (var triangle in Content.Objects.Triangle)
        {
            if (!triangle.Intersect(ray, Content.VertexData, ref intersection)) continue;
            if (intersection.Distance < maxDistance)
                return true;
        }

        // Disabled due to mirror room bug??
        // foreach (var plane in Planes)
        // {
        //     if (!plane.Intersect(ray, ref intersection)) continue;
        //     if (intersection.Distance < maxDistance)
        //         return true;
        // }

        return false;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsOccluded(in Vector3 point, in Vector3 lightPos, in Vector3 normal, float time)
    {
        var dir = Vector3.Normalize(lightPos - point);
        float maxT = Vector3.Distance(lightPos, point);
        var ray = new Ray(point + normal * RayTracerRenderer.ShadowRayEpsilon, dir, true, time);
        return IntersectAny(ray, maxT);
    }
}