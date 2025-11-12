using System.Numerics;
using Raytracer.Core;
using Raytracer.Rendering;
using Raytracer.Rendering.Intersections;
using Raytracer.Scenes.Content.Datas;
using Raytracer.Scenes.Content.Datas.Objects;

namespace Raytracer.Scenes.Runtime;

public class Plane : Geometry
{
    public Transform Transform;
    public Vector3 point;
    public Vector3 normal;

    public Plane(PlaneData data, VertexData vertexData)
    {
        point = vertexData.At(data.Point);
        normal = Vector3.Normalize(data.Normal);
        MaterialIndex = data.Material;
        Transform = new Transform();
    }

    public override bool Intersect(in Ray ray, ref IntersectionInfo info)
    {
        var localRay = Transform.ToLocalRay(ray);

        // Dot product of ray direction and plane normal
        float denom = Vector3.Dot(localRay.Direction, normal);
        if (MathF.Abs(denom) < RayTracerRenderer.IntersectionTestEpsilon)
            return false; // Ray is parallel to the plane

        // Distance along ray
        float t = Vector3.Dot(point - localRay.Origin, normal) / denom;
        if (t < 0)
            return false; // Intersection behind ray origin

        // Compute hit point in local space
        var localHitPoint = localRay.Origin + localRay.Direction * t;

        info.Hit = true;
        info.Point = Transform.ToWorldPoint(localHitPoint);
        info.Distance = Vector3.Distance(ray.Origin, info.Point);

        var worldNormal = Transform.ToWorldDirection(normal);
        info.Normal = Vector3.Normalize(worldNormal);
        info.HitGeometry = this;
        return true;
    }
}