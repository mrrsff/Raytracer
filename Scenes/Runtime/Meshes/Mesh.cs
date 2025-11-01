using System.Numerics;
using Raytracer.Core;
using Raytracer.Rendering;
using Raytracer.Rendering.Intersections;
using Raytracer.Scenes.Content.Datas.Objects;
using Raytracer.Scenes.Runtime.Meshes.BVH;

namespace Raytracer.Scenes.Runtime.Meshes;

public class Mesh : Geometry
{
    public Transform Transform { get; set; }
    public MeshDefinition MeshDefinition { get; set; }
    public ShadingMode ShadingMode { get; set; }
    public BoundingBox AABB { get; set; }

    public override int GetPrimitiveCount() => MeshDefinition.Triangles.Length;

    public Mesh(MeshData meshData, Scene scene)
    {
        ShadingMode = meshData.ShadingMode;
        MaterialIndex = meshData.Material;

        MeshDefinition = new MeshDefinition(meshData, scene.Content.VertexData);
        Transform = new Transform();
        Initialize();
    }

    public Mesh(string plyPath, ShadingMode shadingMode, int material)
    {
        ShadingMode = shadingMode;
        MaterialIndex = material;
        
        var data = new PlyData(plyPath);
        MeshDefinition = new MeshDefinition(data);
        Transform = new Transform();
        Initialize();
    }

    public Mesh(Mesh originalMesh, Transform newTransform)
    {
        ShadingMode = originalMesh.ShadingMode;
        MaterialIndex = originalMesh.MaterialIndex;

        MeshDefinition = originalMesh.MeshDefinition;
        Transform = newTransform;
        Initialize();
    }
    private void Initialize()
    {
    }
    public override bool Intersect(in Ray ray, ref IntersectionInfo info)
    {
        Ray localRay = Transform.ToLocalRay(ray);
        
        info.IntersectionTestEpsilon = RayTracerRenderer.IntersectionTestEpsilon;
        var hit = MeshDefinition.BVH.Intersect(in localRay, ref info);
        if (!hit) return false;

        var localHitPoint = info.Point;
        
        info.Point = Transform.ToWorldPoint(localHitPoint);
        info.Normal = Transform.ToWorldDirection(info.Normal);
        
        if (info.Normal.LengthSquared() < 1e-12f)
            return false; // invalid hit
        
        info.Distance = Vector3.Distance(ray.Origin, info.Point);
        
        if (ShadingMode == ShadingMode.Flat || info.PrimitiveIndex < 0 || info.PrimitiveIndex >= MeshDefinition.Triangles.Length) return hit;

        var t = MeshDefinition.Triangles[info.PrimitiveIndex];
        t.CalculateBarycentricCoordinates(localHitPoint, out var alpha, out var beta, out var gamma);
        
        Vector3 n0 = MeshDefinition.VertexNormals[t.I0];
        Vector3 n1 = MeshDefinition.VertexNormals[t.I1];
        Vector3 n2 = MeshDefinition.VertexNormals[t.I2];
        Vector3 localNormal = Vector3.Normalize(alpha * n0 + beta * n1 + gamma * n2);
        
        info.Normal = Vector3.Normalize(Transform.ToWorldDirection(localNormal));
        return hit;
    }
}