using System.Numerics;
using Raytracer.Core;
using Raytracer.Scenes;
using Raytracer.Scenes.Content.Datas.Objects;
using Raytracer.Scenes.Runtime;
using Raytracer.Scenes.Runtime.Meshes;

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
            var e1 = t.E1;
            var e2 = t.E2;
            intersection.Reset();
            switch (mesh.ShadingMode)
            {
                case ShadingMode.Flat:
                {
                    if (!TriangleIntersection.Intersect(ray, v0, v1, v2, e1, e2, normal, ref intersection) || 
                        intersection.Distance > info.Distance) continue;
                    info = intersection;
                    break;
                }
                case ShadingMode.Smooth:
                {
                    if (!TriangleIntersection.IntersectBarycentric(ray, v0, v1, v2, e1, e2, normal, ref intersection, out var beta, out var gamma) || 
                        intersection.Distance > info.Distance) continue;
                    info = intersection;
                    var alpha = 1.0f - beta - gamma;
                
                    Vector3 n0 = mesh.VertexNormals[t.I0];
                    Vector3 n1 = mesh.VertexNormals[t.I1];
                    Vector3 n2 = mesh.VertexNormals[t.I2];

                    info.Normal = Vector3.Normalize(alpha * n0 + beta * n1 + gamma * n2);
                    break;
                }
                default:
                    Console.WriteLine($"[Warning] Unsupported shading mode {mesh.ShadingMode} in MeshIntersection.");
                    break;
            }
        }
        return info.Hit;
    }
    
    private static void CalculateBarycentricCoordinates(Triangle triangle, Vector3 point, out float alpha, out float beta, out float gamma)
    {
        Vector3 v0 = triangle.V1 - triangle.V0;
        Vector3 v1 = triangle.V2 - triangle.V0;
        Vector3 v2 = point - triangle.V0;

        float d00 = Vector3.Dot(v0, v0);
        float d01 = Vector3.Dot(v0, v1);
        float d11 = Vector3.Dot(v1, v1);
        float d20 = Vector3.Dot(v2, v0);
        float d21 = Vector3.Dot(v2, v1);

        float denom = d00 * d11 - d01 * d01;
        beta = (d11 * d20 - d01 * d21) / denom;
        gamma = (d00 * d21 - d01 * d20) / denom;
        alpha = 1.0f - beta - gamma;
    }
}