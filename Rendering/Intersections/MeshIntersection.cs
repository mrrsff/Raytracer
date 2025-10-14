using Raytracer.Core;
using Raytracer.Scenes;
using Raytracer.Scenes.Content.Datas.Objects;
using Raytracer.Scenes.Runtime;

namespace Raytracer.Rendering.Intersections;

public static class MeshIntersection
{
    public static IntersectionInfo Intersect(this Mesh mesh, Ray ray)
    {
        IntersectionInfo closestIntersection = new IntersectionInfo();
        var triangles = mesh.Triangles;
        for (int i = 0; i < triangles.Length; i++)
        {
            var v0 = triangles[i].V0;
            var v1 = triangles[i].V1;
            var v2 = triangles[i].V2;
            var normal = triangles[i].Normal;
            var intersection = TriangleIntersection.Intersect(ray, v0, v1, v2, normal);
            if (intersection.Hit && intersection.Distance < closestIntersection.Distance)
            {
                closestIntersection = intersection;
            }
        }
        return closestIntersection;
    }
}