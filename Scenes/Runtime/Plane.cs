using System.Numerics;
using Raytracer.Core;
using Raytracer.Rendering.CPU.Intersections;
using Raytracer.Rendering.CPU.Raytracing;
using Raytracer.Scenes.Content.Datas;
using Raytracer.Scenes.Content.Datas.Objects;
using Raytracer.Utility;

namespace Raytracer.Scenes.Runtime;

public class Plane : Geometry
{
    public Vector3 _point;
    public Vector3 _normal;

    public Plane(PlaneData data, VertexData vertexData)
    {
        _point = vertexData.At(data.Point);
        _normal = Vector3.Normalize(data.Normal);
        MaterialIndex = data.Material;
        Transform = new Transform();
        TextureIndices = data.Textures;
    }

    public override bool Intersect(in Ray ray, ref IntersectionInfo info)
    {
        var localRay = Transform.ToLocalRay(ray);

        // Dot product of ray direction and plane normal
        float denom = Vector3.Dot(localRay.Direction, _normal);
        if (MathF.Abs(denom) < RayTracerRenderer.IntersectionTestEpsilon)
            return false; // Ray is parallel to the plane

        // Distance along ray
        float t = Vector3.Dot(_point - localRay.Origin, _normal) / denom;
        if (t < 0)
            return false; // Intersection behind ray origin

        // Compute hit point in local space
        var localHitPoint = localRay.Origin + localRay.Direction * t;

        info.Hit = true;
        info.Point = Transform.ToWorldPoint(localHitPoint);
        info.Distance = Vector3.Distance(ray.Origin, info.Point);
        info.HitGeometry = this;
        info.GeometricNormal = Transform.ToWorldDirection(_normal);
        return true;
    }

    public override Vector2 GetUVCoordinates(in Vector3 point, in int primitiveIndex, float rayTime, bool tiling)
    {
        // Simple planar mapping
        Vector3 localPoint = Transform.ToLocalPoint(point);
        float u = localPoint.X - MathF.Floor(localPoint.X);
        float v = localPoint.Z - MathF.Floor(localPoint.Z);
        return new Vector2(u, v);
    }

    public override Vector3 GetNormal(in Vector3 point, in int primitiveIndex, float rayTime)
    {
        return Transform.ToWorldDirection(_normal);
    }

    public override void GetTBN(in Vector3 point, in int primitiveIndex, float rayTime, out Vector3 tangent, out Vector3 bitangent, out Vector3 normal)
    {
        normal = GetNormal(point, primitiveIndex, rayTime);
        
        Vector3 reference = MathF.Abs(normal.Y) < 0.999f ? Vector3.UnitY : Vector3.UnitX;

        tangent = Vector3.Normalize(Vector3.Cross(normal, reference));
        bitangent = Vector3.Cross(normal, tangent);
    }
}