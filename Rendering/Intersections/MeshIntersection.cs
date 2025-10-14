using Raytracer.Core;
using Raytracer.Scenes;
using Raytracer.Scenes.Content.Datas.Objects;
using Raytracer.Scenes.Runtime;

namespace Raytracer.Rendering.Intersections;

public static class MeshIntersection
{
    public static bool Intersect(this Mesh mesh, in Ray ray, ref IntersectionInfo info)
    {
        if (!mesh.BoundingBox.Intersects(ray))
        {
            return false;
        }
        var triangles = mesh.Triangles;
        info = IntersectionInfo.NoHit;
        foreach (var t in triangles)
        {
            var v0 = t.V0;
            var v1 = t.V1;
            var v2 = t.V2;
            var normal = t.Normal;
            var e1 = t.E1;
            var e2 = t.E2;
            var intersection = IntersectionInfo.NoHit;
            if (TriangleIntersection.Intersect(ray, v0, v1, v2, e1, e2, normal, ref intersection) && intersection.Distance < info.Distance)
            {
                info = intersection;
            }
        }

        return info.Hit;
    }
}