using System.Numerics;
using Raytracer.Scenes.Content.Datas;
using Raytracer.Scenes.Content.Datas.Objects;
using Raytracer.Scenes.Runtime.Meshes.BVH;

namespace Raytracer.Scenes.Runtime.Meshes;

public class MeshDefinition
{
    public Vector3[] Vertices { get; private set; }
    public Triangle[] Triangles { get; private set; }
    public Vector3[] VertexNormals { get; private set; }
    public BoundingVolumeHierarchy BVH { get; set; }
    public MeshDefinition(in MeshData meshData, in VertexData vertexData)
    {
        var faceIndices = meshData.Faces.Data;

        var uniqueIndices = faceIndices.Distinct().ToArray();
        Vertices = new Vector3[uniqueIndices.Length];
        
        var vertexMap = new Dictionary<int, int>();
        for (int i = 0; i < uniqueIndices.Length; i++)
        {
            int globalId = uniqueIndices[i];
            Vertices[i] = vertexData.At(globalId);
            vertexMap[globalId] = i;
        }
        
        int triangleCount = faceIndices.Length / 3;
        Triangles = new Triangle[triangleCount];

        for (int i = 0; i < triangleCount; i++)
        {
            int i0 = vertexMap[faceIndices[i * 3]];
            int i1 = vertexMap[faceIndices[i * 3 + 1]];
            int i2 = vertexMap[faceIndices[i * 3 + 2]];

            Triangles[i] = new Triangle(i, i0, i1, i2, this);
        }
        ComputeSmoothNormals();
    }

    public MeshDefinition(in PlyData plyData)
    {
        Vertices = plyData.vertices;
        Triangles = plyData.triangles;
        foreach (var tri in Triangles)
        {
            tri.SetMeshDefinition(this);
        }
        
        ComputeSmoothNormals();
    }
    private void ComputeSmoothNormals()
    {
        VertexNormals = new Vector3[Vertices.Length];
        foreach (var tri in Triangles)
        {
            Vector3 n = Vector3.Cross(tri.V1 - tri.V0, tri.V2 - tri.V0);
            VertexNormals[tri.I0] += n;
            VertexNormals[tri.I1] += n;
            VertexNormals[tri.I2] += n;
        }

        for (int i = 0; i < VertexNormals.Length; i++)
            VertexNormals[i] = Vector3.Normalize(VertexNormals[i]);
        BVH = new BoundingVolumeHierarchy(this);
    }
    public BoundingBox GetBounds()
    {
        return BVH.GetNode(0).Bounds;
    }
}