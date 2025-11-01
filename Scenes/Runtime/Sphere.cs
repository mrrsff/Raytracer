using System.Numerics;
using Raytracer.Core;
using Raytracer.Rendering;
using Raytracer.Rendering.Intersections;
using Raytracer.Scenes.Content.Datas;
using Raytracer.Scenes.Content.Datas.Objects;

namespace Raytracer.Scenes.Runtime;

public class Sphere : Geometry
{
    public Transform Transform;
    public Vector3 center;
    public float radius;
    public float radiusSquared;

    public Sphere(in SphereData data, in VertexData vertexData)
    {
        center = vertexData.At(data.Center);
        radius = data.Radius;
        MaterialIndex = data.Material;
        radiusSquared = radius * radius;
        Transform = new Transform();
    }
    public override bool Intersect(in Ray ray, ref IntersectionInfo info)
    {
        var localRay = Transform.ToLocalRay(ray);
        Vector3 o = localRay.Origin;
        Vector3 d = localRay.Direction;

        Vector3 oc = o - center;
        float a = Vector3.Dot(d, d);
        float b = 2.0f * Vector3.Dot(oc, d);
        float c = Vector3.Dot(oc, oc) - radiusSquared;
        float discriminant = b * b - 4 * a * c;

        if (discriminant < 0)
            return false;

        float sqrtDiscriminant = MathF.Sqrt(discriminant);
        float t = (-b - sqrtDiscriminant) / (2.0f * a);
        if (t < RayTracerRenderer.ShadowRayEpsilon)
        {
            t = (-b + sqrtDiscriminant) / (2.0f * a);
            if (t < RayTracerRenderer.ShadowRayEpsilon)
                return false;
        }

        Vector3 localHitPoint = o + t * d;
        Vector3 localNormal = Vector3.Normalize(localHitPoint - center);

        Vector3 worldHitPoint = Transform.ToWorldPoint(localHitPoint);
        Vector3 worldNormal = Vector3.Normalize(Transform.ToWorldDirection(localNormal));

        float worldDistance = (worldHitPoint - ray.Origin).Length();

        info.Hit = true;
        info.Point = worldHitPoint;
        info.Normal = worldNormal;
        info.Distance = worldDistance;

        return true;
    }

}