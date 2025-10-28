using System.Numerics;
using Raytracer.Core;
using Raytracer.Rendering.Intersections;
using Raytracer.Scenes.Content.Datas.Objects;
using Raytracer.Scenes.Runtime.Meshes.BVH;

namespace Raytracer.Scenes.Runtime.Meshes;

public class Mesh : Geometry
{
    public Transform Transform { get; set; }
    public MeshDefinition MeshDefinition { get; set; }
    public ShadingMode ShadingMode { get; set; }
    public BoundingVolumeHierarchy BVH { get; set; }

    public override int GetPrimitiveCount() => MeshDefinition.Triangles.Length;
    private Vector3[] Vertices => MeshDefinition.Vertices;
    private Triangle[] Triangles => MeshDefinition.Triangles;
    private Vector3[] VertexNormals => MeshDefinition.VertexNormals;

    public Mesh(MeshData meshData, Scene scene)
    {
        ShadingMode = meshData.ShadingMode;
        MaterialIndex = meshData.Material;

        MeshDefinition = new MeshDefinition(meshData, scene.Content.VertexData);
        BuildBVH();
    }

    public Mesh(string plyPath, ShadingMode shadingMode, int material)
    {
        ShadingMode = shadingMode;
        MaterialIndex = material;
        
        var data = new PlyData(plyPath);
        MeshDefinition = new MeshDefinition(data);
        BuildBVH();
    }
    private void BuildBVH()
    {
        BVH = new BoundingVolumeHierarchy(MeshDefinition);
    }
    public override bool Intersect(in Ray ray, ref IntersectionInfo info)
    {
        var hit = BVH.Intersect(in ray, ref info);
        if (!hit) return false;
        
        if (ShadingMode == ShadingMode.Flat) return hit;
        
        if (info.PrimitiveIndex < 0 || info.PrimitiveIndex >= Triangles.Length)
            return hit;
        
        var t = Triangles[info.PrimitiveIndex];
        t.CalculateBarycentricCoordinates(info.Point, out var alpha, out var beta, out var gamma);
        
        Vector3 n0 = VertexNormals[t.I0];
        Vector3 n1 = VertexNormals[t.I1];
        Vector3 n2 = VertexNormals[t.I2];
        
        info.Normal = Vector3.Normalize(alpha * n0 + beta * n1 + gamma * n2);
        return hit;
    }
}