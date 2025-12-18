using System.Numerics;
using Raytracer.Core;
using Raytracer.Rendering.Intersections;
using Raytracer.Rendering.Raytracing;
using Raytracer.Scenes.Content.Datas;
using Raytracer.Scenes.Content.Datas.Objects;
using Raytracer.Scenes.Runtime.Meshes;

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
        Transform = new Transform();
        Bounds = new BoundingBox(
            Transform.ToWorldPoint(center - new Vector3(radius)),
            Transform.ToWorldPoint(center + new Vector3(radius))
        );
        TextureIndices = data.Textures;
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
        Vector3 worldHitPoint = Transform.ToWorldPoint(localHitPoint);
        float worldDistance = (worldHitPoint - ray.Origin).Length();

        info.Hit = true;
        info.Point = worldHitPoint;
        info.Distance = worldDistance;
        info.HitGeometry = this;

        return true;
    }
    public override Vector3 GetNormal(in Vector3 point, in int primitiveIndex, float rayTime)
    {
        Transform tr = GetMotionBlurTransform(rayTime);
        Vector3 localPoint = tr.ToLocalPoint(point);
        Vector3 localNormal = Vector3.Normalize(localPoint - center);
        Vector3 worldNormal = tr.ToWorldDirection(localNormal);
        return Vector3.Normalize(worldNormal);
    }

    public override void GetTBN(in Vector3 point, in int primitiveIndex, float rayTime, out Vector3 tangent, out Vector3 bitangent, out Vector3 normal)
    {
        Transform tr = GetMotionBlurTransform(rayTime);
        Vector3 localPoint = tr.ToLocalPoint(point);
        Vector3 P = localPoint - center;

        float r = radius;

        float theta = MathF.Acos(P.Y / r);
        float phi = MathF.Atan2(P.Z, P.X);

        tangent = new Vector3(
            2.0f * MathF.PI * P.Z,
            0.0f,
            -2.0f * MathF.PI * P.X
        );

        bitangent = new Vector3(
            MathF.PI * P.Y * MathF.Cos(phi),
            -MathF.PI * r * MathF.Sin(theta),
            MathF.PI * P.Y * MathF.Sin(phi)
        );

        normal = Vector3.Normalize(P);
        
        tangent = tr.ToWorldDirection(tangent);
        bitangent = tr.ToWorldDirection(bitangent);
        normal = tr.ToWorldDirection(normal);
    }

    public override Vector2 GetUVCoordinates(in Vector3 point, in int _, float rayTime, bool tiling)
    {
        Transform tr = GetMotionBlurTransform(rayTime);
        Vector3 localPoint = tr.ToLocalPoint(point);
        Vector3 P = localPoint - center;
    
        float r = radius;
    
        float theta = MathF.Acos(P.Y / r);
        float phi = MathF.Atan2(P.Z, P.X);
    
        float u = (-phi + MathF.PI) / (2.0f * MathF.PI);
        float v = theta / MathF.PI;
    
        if (!tiling)
            return new Vector2(u, v);
    
        float halfCircumference = MathF.PI * r;
        return new Vector2(u * halfCircumference * 2f, v * halfCircumference);
    }
    
}