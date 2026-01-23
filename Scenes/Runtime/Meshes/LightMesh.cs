using System.Numerics;
using Raytracer.Core;
using Raytracer.Core.Lights;
using Raytracer.Rendering;
using Raytracer.Rendering.Intersections;
using Raytracer.Rendering.Sampling;
using Raytracer.Scenes.Content.Datas.Objects;
using Raytracer.Utility;

namespace Raytracer.Scenes.Runtime.Meshes;

public class LightMesh : Mesh, IObjectLight
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
        L = irradiance = Vector3.Zero;

        SelectPoint(time, out Vector3 x, out Vector3 nL, out float pdfA);

        Vector3 toLight = x - P;
        float dist2 = toLight.LengthSquared();
        float dist = MathF.Sqrt(dist2);
        Vector3 wi = toLight / dist;

        float cosSurface = Vector3.Dot(N, wi);
        if (cosSurface < 0f)
        {
            return false;
        }

        float cosLight = Vector3.Dot(nL, -wi);
        if (cosLight <= 0f)
            return false;
        
        Ray shadowRay = new Ray(
            P + wi * renderer.Scene.Content.ShadowRayEpsilon,
            wi,
            true,
            time
        );

        IntersectionInfo hit = renderer.Scene.Intersect(shadowRay);
        if (hit.Hit)
        {
            if (hit.HitGeometry != this)
                return false;
        }

        // Convert area PDF to solid angle PDF
        float pdfW = (pdfA * dist2) / cosLight * 2.0f;
        if (pdfW <= 0f)
            return false;

        L = wi;
        irradiance = radiance * cosLight / dist2;
        return true;
    }


    public float Pdf(Vector3 P, Vector3 N, float time, Renderer renderer)
    {
        SelectPoint(time, out Vector3 x, out Vector3 nL, out float pdfA);

        Vector3 toLight = x - P;
        float dist2 = toLight.LengthSquared();
        Vector3 wi = Vector3.Normalize(toLight);

        float cosLight = Vector3.Dot(nL, -wi);
        if (cosLight < 0f)
            cosLight = -cosLight;

        float pdfW = (pdfA * dist2) / cosLight * 2.0f;
        return pdfW;
    }


    private void SelectPoint(float time, out Vector3 point, out Vector3 normal, out float pdf)
    {
        SelectTriangle(out int triangleIndex, out float trianglePdf);
        var tri = MeshDefinition.Triangles[triangleIndex];
        
        var tr = GetMotionBlurTransform(time);
        Vector3 v0 = tr.ToWorldPoint(tri.V0);
        Vector3 v1 = tr.ToWorldPoint(tri.V1);
        Vector3 v2 = tr.ToWorldPoint(tri.V2);
        
        // Sample point on triangle using barycentric coordinates
        float u = Sampler.OneDimensionalUniform();
        float v = Sampler.OneDimensionalUniform();
        if (u + v > 1f)
        {
            u = 1f - u;
            v = 1f - v;
        }
        
        point = v0 + u * (v1 - v0) + v * (v2 - v0);
        
        Vector3 e1 = v1 - v0;
        Vector3 e2 = v2 - v0;
        float area = 0.5f * Vector3.Cross(e1, e2).Length();
        normal = Vector3.Normalize(Vector3.Cross(e1, e2));
        
        pdf = trianglePdf / area;
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