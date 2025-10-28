using System.Numerics;
using Raytracer.Core;
using Raytracer.Rendering;
using Raytracer.Rendering.Intersections;
using Raytracer.Scenes.Content.Datas;
using Raytracer.Scenes.Content.Datas.Objects;

namespace Raytracer.Scenes.Runtime;

public class Sphere : Geometry
{
    public Vector3 center;
    public float radius;
    public float radiusSquared;

    public Sphere(in SphereData data, in VertexData vertexData)
    {
        center = vertexData.At(data.Center);
        radius = data.Radius;
        MaterialIndex = data.Material;
        radiusSquared = radius * radius;
    }
    public override bool Intersect(in Ray ray, ref IntersectionInfo info)
    {
        Vector3 o = ray.Origin;
        Vector3 d = ray.Direction;

        Vector3 oc = o - center;
        float a = Vector3.Dot(d, d);
        float b = 2.0f * Vector3.Dot(oc, d);
        float c = Vector3.Dot(oc, oc) - radiusSquared;
        float discriminant = b * b - 4 * a * c;

        if (discriminant < 0)
            return false;
        
        float sqrtDiscriminant = MathF.Sqrt(discriminant);

        float t = (-b - sqrtDiscriminant) / (2.0f * a);
        if (t < RayTracerRenderer.ShadowRayEpsilon) // if the first intersection is behind the ray origin, check the second intersection
        {
            t = (-b + sqrtDiscriminant) / (2.0f * a);
            if (t < RayTracerRenderer.ShadowRayEpsilon) // both intersections are behind the ray origin
                return false;
        }

        Vector3 point = o + t * d;
        Vector3 normal = Vector3.Normalize(point - center);

        info.Hit = true;
        info.Distance = t;
        info.Point = point;
        info.Normal = normal;
        info.HitRay = ray;
        return true;
    }
}