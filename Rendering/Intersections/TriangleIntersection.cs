using System.Numerics;
using System.Runtime.CompilerServices;
using Raytracer.Core;
using Raytracer.Scenes.Content.Datas;
using Raytracer.Scenes.Content.Datas.Objects;

namespace Raytracer.Rendering.Intersections;

public static class TriangleIntersection
{
    public static IntersectionInfo Intersect(this TriangleData triangle, Ray ray, VertexData vertexData)
    {
        Vector3 v0 = vertexData.At(triangle.indices[0]);
        Vector3 v1 = vertexData.At(triangle.indices[1]);
        Vector3 v2 = vertexData.At(triangle.indices[2]);
        Vector3 normal = Vector3.Normalize(Vector3.Cross(v1 - v0, v2 - v0));
        return Intersect(ray, v0, v1, v2, normal);
    }
    public static IntersectionInfo Intersect(Ray ray, Vector3 v0, Vector3 v1, Vector3 v2, Vector3 normal)
    {
        Vector3 o = ray.Origin;
        Vector3 d = ray.Direction;

        // Backface culling (optional; disable for shadow rays)
        if (!ray.IsShadowRay && Vector3.Dot(normal, d) > 0f)
            return IntersectionInfo.NoHit;

        // Möller–Trumbore barycentric form (faster than determinant)
        Vector3 edge1 = v1 - v0;
        Vector3 edge2 = v2 - v0;

        Vector3 pvec = Vector3.Cross(d, edge2);
        float det = Vector3.Dot(edge1, pvec);

        if (det > -RayTracerRenderer.IntersectionTestEpsilon && det < RayTracerRenderer.IntersectionTestEpsilon)
            return IntersectionInfo.NoHit;

        float invDet = 1f / det;

        Vector3 tvec = o - v0;
        float u = Vector3.Dot(tvec, pvec) * invDet;
        if (u < 0f || u > 1f)
            return IntersectionInfo.NoHit;

        Vector3 qvec = Vector3.Cross(tvec, edge1);
        float v = Vector3.Dot(d, qvec) * invDet;
        if (v < 0f || u + v > 1f)
            return IntersectionInfo.NoHit;

        float t = Vector3.Dot(edge2, qvec) * invDet;
        if (t <= RayTracerRenderer.IntersectionTestEpsilon)
            return IntersectionInfo.NoHit;

        return new IntersectionInfo
        {
            Hit = true,
            HitRay = ray,
            Distance = t,
            Point = o + d * t,
            Normal = normal
        };
    }
}