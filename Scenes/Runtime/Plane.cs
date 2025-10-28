using System.Numerics;
using Raytracer.Core;
using Raytracer.Rendering;
using Raytracer.Rendering.Intersections;
using Raytracer.Scenes.Content.Datas;
using Raytracer.Scenes.Content.Datas.Objects;

namespace Raytracer.Scenes.Runtime;

public class Plane : Geometry
{
    public Vector3 point;
    public Vector3 normal;

    public Plane(PlaneData data, VertexData vertexData)
    {
        point = vertexData.At(data.Point);
        normal = Vector3.Normalize(data.Normal);
        MaterialIndex = data.Material;
    }
    public override bool Intersect(in Ray ray, ref IntersectionInfo info)
    {
        float denom = Vector3.Dot(ray.Direction, normal);
        
        if (-denom > RayTracerRenderer.IntersectionTestEpsilon)
        {
            float t = Vector3.Dot(point - ray.Origin, normal) / denom;
        
            if (t < 0)
                return false;
        
            info.Hit = true;
            info.Distance = t;
            info.Point = ray.Origin + ray.Direction * t;
            info.Normal = normal;
            info.HitRay = ray;
        
            return true;
        }

        return false;
    }
}