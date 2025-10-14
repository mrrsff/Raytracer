using System.Numerics;
using Raytracer.Core;
using Raytracer.Scenes.Content.Datas;
using Raytracer.Scenes.Content.Datas.Objects;

namespace Raytracer.Rendering.Intersections;

public static class SphereIntersection
{
    public static IntersectionInfo Intersect(this SphereData sphereData, Ray ray, VertexData vertexData)
    {
        Vector3 center = vertexData.At(sphereData.Center);
        float radius = sphereData.Radius;
        
        Vector3 o = ray.Origin;
        Vector3 d = ray.Direction;

        Vector3 oc = o - center;
        float a = Vector3.Dot(d, d);
        float b = 2.0f * Vector3.Dot(oc, d);
        float c = Vector3.Dot(oc, oc) - radius * radius;
        float discriminant = b * b - 4 * a * c;

        if (discriminant < 0)
            return IntersectionInfo.NoHit;

        float sqrtDiscriminant = MathF.Sqrt(discriminant);

        float t = (-b - sqrtDiscriminant) / (2.0f * a);
        if (t < RayTracerRenderer.ShadowRayEpsilon) // if the first intersection is behind the ray origin, check the second intersection
        {
            t = (-b + sqrtDiscriminant) / (2.0f * a);
            if (t < RayTracerRenderer.ShadowRayEpsilon) // both intersections are behind the ray origin
                return IntersectionInfo.NoHit;
        }

        Vector3 point = ray.At(t);
        Vector3 normal = Vector3.Normalize(point - center);

        return new IntersectionInfo()
        {
            HitRay = ray,
            Hit = true,
            Distance = t,
            Point = point,
            Normal = normal
        };
    }
}