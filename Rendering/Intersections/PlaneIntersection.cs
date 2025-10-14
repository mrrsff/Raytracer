using System.Numerics;
using Raytracer.Core;
using Raytracer.Scenes.Content.Datas;
using Raytracer.Scenes.Content.Datas.Objects;

namespace Raytracer.Rendering.Intersections;

public static class PlaneIntersection
{
    public static bool Intersect(this PlaneData planeData, in Ray ray, in VertexData vertexData, ref IntersectionInfo info)
    {
        Vector3 pointOnPlane = vertexData.At(planeData.Point);
        Vector3 planeNormal = planeData.Normal;
        Vector3 o = ray.Origin;
        Vector3 d = ray.Direction;

        float denom = Vector3.Dot(d, planeNormal);

        if (MathF.Abs(denom) < RayTracerRenderer.ShadowRayEpsilon)
            return false;

        float t = Vector3.Dot(pointOnPlane - o, planeNormal) / denom;

        if (t < RayTracerRenderer.ShadowRayEpsilon)
            return false;

        Vector3 intersectionPoint = o + t * d;
        Vector3 normal = Vector3.Normalize(planeNormal);

        info.HitRay = ray;
        info.Hit = true;
        info.Distance = t;
        info.Point = intersectionPoint;
        info.Normal = normal;
        return true;
    }
}