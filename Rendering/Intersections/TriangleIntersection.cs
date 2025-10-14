using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using Raytracer.Core;
using Raytracer.Scenes.Content.Datas;
using Raytracer.Scenes.Content.Datas.Objects;

namespace Raytracer.Rendering.Intersections;

public static class TriangleIntersection
{
    public static bool Intersect(this TriangleData triangle, in Ray ray, in VertexData vertexData, ref IntersectionInfo info)
    {
        Vector3 v0 = vertexData.At(triangle.indices[0]);
        Vector3 v1 = vertexData.At(triangle.indices[1]);
        Vector3 v2 = vertexData.At(triangle.indices[2]);
        Vector3 normal = Vector3.Normalize(Vector3.Cross(v1 - v0, v2 - v0));
        Vector3 e1 = v1 - v0;
        Vector3 e2 = v2 - v0;

        return Intersect(ray, v0, v1, v2, e1, e2, normal, ref info);
    }
    public static bool Intersect(in Ray ray, in Vector3 v0, in Vector3 v1, in Vector3 v2, in Vector3 e1, in Vector3 e2, in Vector3 normal, ref IntersectionInfo info)
    {
        Vector3 o = ray.Origin;
        Vector3 d = ray.Direction;

        if ((Vector3.Dot(normal, d) > 0f) & !ray.IsShadowRay)
            return false;

        Vector3 pvec = Vector3.Cross(d, e2);
        float det = Vector3.Dot(e1, pvec);

        if (MathF.Abs(det) < RayTracerRenderer.IntersectionTestEpsilon)
            return false;

        float invDet = 1f / det;

        Vector3 tvec = o - v0;
        float u = Vector3.Dot(tvec, pvec) * invDet;
        if (u < 0f || u > 1f)
            return false;

        Vector3 qvec = Vector3.Cross(tvec, e1);
        float v = Vector3.Dot(d, qvec) * invDet;
        if (v < 0f || u + v > 1f)
            return false;

        float t = Vector3.Dot(e2, qvec) * invDet;
        if (t <= RayTracerRenderer.IntersectionTestEpsilon)
            return false;

        info.HitRay = ray;
        info.Hit = true;
        info.Distance = t;
        info.Point = o + d * t;
        info.Normal = normal;
        return true;
    }
}