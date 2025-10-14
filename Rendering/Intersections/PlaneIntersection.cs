using System.Numerics;
using Raytracer.Core;
using Raytracer.Scenes.Content.Datas;
using Raytracer.Scenes.Content.Datas.Objects;

namespace Raytracer.Rendering.Intersections;

public static class PlaneIntersection
{
    public static IntersectionInfo Intersect(this PlaneData planeData, Ray ray, VertexData vertexData)
    {
        Vector3 pointOnPlane = vertexData.At(planeData.Point);
        Vector3 planeNormal = planeData.Normal;
        Vector3 o = ray.Origin;
        Vector3 d = ray.Direction;

        float denom = Vector3.Dot(d, planeNormal);
        
        if (MathF.Abs(denom) < RayTracerRenderer.ShadowRayEpsilon)
            return IntersectionInfo.NoHit;

        float t = Vector3.Dot(pointOnPlane - o, planeNormal) / denom;

        if (t < RayTracerRenderer.ShadowRayEpsilon)
            return IntersectionInfo.NoHit;

        Vector3 intersectionPoint = o + t * d;
        Vector3 normal = Vector3.Normalize(planeNormal);

        return new IntersectionInfo()
        {
            HitRay = ray,
            Hit = true,
            Distance = t,
            Point = intersectionPoint,
            Normal = normal
        };
    }
}