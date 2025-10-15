using System.Numerics;
using Raytracer.Rendering;
using Raytracer.Scenes.Content.Datas.Objects;

namespace Raytracer.Scenes.Runtime.Meshes;

public class Mesh
{
    public Vector3[] Vertices { get; private set; }
    public Triangle[] Triangles { get; private set; }
    public ShadingMode ShadingMode { get; set; }
    public int Material { get; set; }
    public BoundingBox BoundingBox { get; private set; }
    public Vector3[] VertexNormals { get; private set; }
    
    public Mesh(MeshData data, Scene scene)
    {
        ShadingMode = data.ShadingMode;
        Material = data.Material;

        var vertexData = scene.Content.VertexData;
        var faceIndices = data.Faces.Data;

        var uniqueIndices = faceIndices.Distinct().ToArray();
        Vertices = new Vector3[uniqueIndices.Length];

        // Compute the bounding box
        Vector3 min = new Vector3(float.MaxValue);
        Vector3 max = new Vector3(float.MinValue);
        
        var vertexMap = new Dictionary<int, int>();
        for (int i = 0; i < uniqueIndices.Length; i++)
        {
            int globalId = uniqueIndices[i];
            Vertices[i] = vertexData.At(globalId);
            vertexMap[globalId] = i;
            
            min = Vector3.Min(min, Vertices[i]);
            max = Vector3.Max(max, Vertices[i]);
        }
        BoundingBox = new BoundingBox(min, max);

        int triangleCount = faceIndices.Length / 3;
        Triangles = new Triangle[triangleCount];

        for (int i = 0; i < triangleCount; i++)
        {
            int i0 = vertexMap[faceIndices[i * 3]];
            int i1 = vertexMap[faceIndices[i * 3 + 1]];
            int i2 = vertexMap[faceIndices[i * 3 + 2]];

            Triangles[i] = new Triangle(
                i0,
                i1,
                i2,
                Vertices[i0],
                Vertices[i1],
                Vertices[i2]
            );
        }
        
        
        if (ShadingMode == ShadingMode.Smooth)
        {
            ComputeSmoothNormals();
        }
    }

    public Mesh(string plyPath, ShadingMode shadingMode, int material)
    {
        ShadingMode = shadingMode;
        Material = material;
        var data = new PlyData(plyPath); // Load the PLY data from the file
        Triangles = data.triangles;
        Vertices = data.vertices;
        
        // Compute the bounding box
        Vector3 min = new Vector3(float.MaxValue);
        Vector3 max = new Vector3(float.MinValue);
        foreach (var vertex in Vertices)
        {
            min = Vector3.Min(min, vertex);
            max = Vector3.Max(max, vertex);
        }
        
        BoundingBox = new BoundingBox(min, max);
        
        if (ShadingMode == ShadingMode.Smooth)
        {
            ComputeSmoothNormals();
        }
    }
    
    private void ComputeSmoothNormals()
    {
        VertexNormals = new Vector3[Vertices.Length];
        var accum = new Vector3[Vertices.Length];

        // accumulate area-weighted face normals
        foreach (var tri in Triangles)
        {
            Vector3 v0 = tri.V0;
            Vector3 v1 = tri.V1;
            Vector3 v2 = tri.V2;

            Vector3 n = Vector3.Cross(v1 - v0, v2 - v0);
            float area = n.Length() * 0.5f;
            n = Vector3.Normalize(n);

            accum[tri.I0] += n * area;
            accum[tri.I1] += n * area;
            accum[tri.I2] += n * area;
        }

        for (int i = 0; i < Vertices.Length; i++)
            VertexNormals[i] = Vector3.Normalize(accum[i]);
    }
}