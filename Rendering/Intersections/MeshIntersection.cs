using System.Numerics;
using Raytracer.Core;
using Raytracer.Scenes;
using Raytracer.Scenes.Content.Datas.Objects;
using Raytracer.Scenes.Runtime;
using Raytracer.Scenes.Runtime.Meshes;
using Raytracer.Utility;
using SixLabors.ImageSharp;

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
        var intersection = IntersectionInfo.NoHit;
        foreach (var t in triangles)
        {
            var v0 = t.V0;
            var v1 = t.V1;
            var v2 = t.V2;
            var normal = t.Normal;
            intersection.Reset();
            switch (mesh.ShadingMode)
            {
                case ShadingMode.Flat:
                {
                    if (!TriangleIntersection.Intersect(ray, v0, v1, v2, normal, ref intersection) || 
                        intersection.Distance > info.Distance) continue;
                    info = intersection;
                    break;
                }
                case ShadingMode.Smooth:
                {
                    if (!TriangleIntersection.IntersectBarycentric(ray, v0, v1, v2, normal, ref intersection, out var beta, out var gamma) || 
                        intersection.Distance > info.Distance) continue;
                    info = intersection;
                    var alpha = 1.0f - beta - gamma;
                
                    Vector3 n0 = mesh.VertexNormals[t.I0];
                    Vector3 n1 = mesh.VertexNormals[t.I1];
                    Vector3 n2 = mesh.VertexNormals[t.I2];

                    info.Normal = Vector3.Normalize(alpha * n0 + beta * n1 + gamma * n2);
                    if (info.Normal == Vector3.Zero)
                    {
                        Console.WriteLine("Zero normal");
                    }
                    break;
                }
                default:
                    Console.WriteLine($"[Warning] Unsupported shading mode {mesh.ShadingMode} in MeshIntersection.");
                    break;
            }
        }
        return info.Hit;
    }
}