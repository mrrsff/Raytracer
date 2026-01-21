using System.Numerics;
using Raytracer.Core;
using Raytracer.Core.Lights;
using Raytracer.Rendering;
using Raytracer.Rendering.Intersections;
using Raytracer.Rendering.Sampling;
using Raytracer.Scenes.Content.Datas.Objects;
using Raytracer.Utility;

namespace Raytracer.Scenes.Runtime.Meshes;

public class LightMesh : Mesh, ILight
{
    public override bool IsEmitter => true;
    public override Vector3 Emission => radiance;
    private readonly Vector3 radiance;

    private float totalArea;
    private float[] triangleAreas;
    private Vector3 boundingSphereCenter;
    private float boundingSphereRadius;
    public LightMesh(LightMeshData meshData, Scene scene, Transform transform) : base(meshData, scene, transform)
    {
        radiance = meshData.Radiance;
        
        // Precompute triangle areas for sampling
        triangleAreas = new float[MeshDefinition.Triangles.Length];
        totalArea = 0f;
        for (int i = 0; i < MeshDefinition.Triangles.Length; i++)
        {
            var tri = MeshDefinition.Triangles[i];
            Vector3 v0 = Transform.ToWorldPoint(tri.V0);
            Vector3 v1 = Transform.ToWorldPoint(tri.V1);
            Vector3 v2 = Transform.ToWorldPoint(tri.V2);
            float area = 0.5f * Vector3.Cross(v1 - v0, v2 - v0).Length();
            triangleAreas[i] = area;
            totalArea += area;
        }
        
        // Compute bounding sphere for faster sampling
        boundingSphereCenter = (Bounds!.Min + Bounds.Max) * 0.5f;
        boundingSphereRadius = 0f;
        var corners = Bounds.GetCorners();
        foreach (var corner in corners)
        {
            float dist = (corner - boundingSphereCenter).Length();
            if (dist > boundingSphereRadius)
                boundingSphereRadius = dist;
        }
        
    }

    public bool Sample(in Vector3 P, in Vector3 N, float time, Renderer renderer, out Vector3 L, out Vector3 irradiance)
    {
        SelectPoint(out Vector3 pointOnLight, out Vector3 normalOnLight, out float pdf);
        
        Ray shadowRay = new Ray(
            P + N * renderer.Scene.Content.ShadowRayEpsilon,
            Vector3.Normalize(pointOnLight - P),
            true,
            time);
        IntersectionInfo info = renderer.Scene.Intersect(shadowRay);
        if (info.Hit && info.HitGeometry != this)
        {
            L = irradiance = default;
            return false;
        }
        
        Vector3 wi = Vector3.Normalize(pointOnLight - P);
        
        float solidAngle = pdf * Vector3.Dot(-wi, normalOnLight) / (boundingSphereRadius * boundingSphereRadius);
        if (solidAngle <= 0f || pdf <= 0f)
        {
            L = Vector3.Zero;
            irradiance = Vector3.Zero;
            return false;
        }

        L = wi;
        irradiance = radiance / solidAngle;
        return true;
    }
    
    private void SelectPoint(out Vector3 point, out Vector3 normal, out float pdf)
    {
        SelectTriangle(out int triangleIndex, out float trianglePdf);
        var tri = MeshDefinition.Triangles[triangleIndex];
        Vector3 v0 = Transform.ToWorldPoint(tri.V0);
        Vector3 v1 = Transform.ToWorldPoint(tri.V1);
        Vector3 v2 = Transform.ToWorldPoint(tri.V2);
        
        // Sample point on triangle using barycentric coordinates
        float u = Sampler.OneDimensionalUniform();
        float v = Sampler.OneDimensionalUniform();
        if (u + v > 1f)
        {
            u = 1f - u;
            v = 1f - v;
        }
        point = v0 + u * (v1 - v0) + v * (v2 - v0);
        
        // Compute normal
        normal = Vector3.Normalize(Vector3.Cross(v1 - v0, v2 - v0));
        
        pdf = trianglePdf / (0.5f * Vector3.Cross(v1 - v0, v2 - v0).Length());
    }
    private void SelectTriangle(out int triangleIndex, out float trianglePdf)
    {
        float sample = Sampler.OneDimensionalUniform() * totalArea;
        float cumulative = 0f;
        for (int i = 0; i < triangleAreas.Length; i++)
        {
            cumulative += triangleAreas[i];
            if (sample > cumulative) continue;
            
            triangleIndex = i;
            trianglePdf = triangleAreas[i] / totalArea;
            return;
        }
        triangleIndex = triangleAreas.Length - 1;
        trianglePdf = triangleAreas[triangleIndex] / totalArea;
    }
}