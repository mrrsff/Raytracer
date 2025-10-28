using System.Numerics;
using Raytracer.Core;
using Raytracer.Rendering;
using Raytracer.Rendering.Intersections;

namespace Raytracer.Scenes;

public partial class Scene
{
    public bool IntersectAny(Ray ray, float maxDistance)
    {
        var intersection = IntersectionInfo.NoHit;

        foreach (var geometry in Geometries)
        {
            if (!geometry.Intersect(ray, ref intersection)) continue;
            if (intersection.Distance < maxDistance)
                return true;
        }
        
        foreach (var triangle in Content.Objects.Triangle)
        {
            if (!triangle.Intersect(ray, Content.VertexData, ref intersection)) continue;
            if (intersection.Distance < maxDistance)
                return true;
        }
        return false;
    }

    public bool IsOccluded(in Vector3 point, in Vector3 lightPos, in Vector3 normal)
    {
        var dir = Vector3.Normalize(lightPos - point);
        float maxT = Vector3.Distance(lightPos, point);
        var ray = new Ray(point + normal * RayTracerRenderer.ShadowRayEpsilon, dir, true);
        return IntersectAny(ray, maxT);
    }
}