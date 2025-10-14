using System.Numerics;
using Raytracer.Scenes.Content.Datas.Objects;

namespace Raytracer.Scenes.Runtime.Meshes;

public class Mesh
{
    public Vector3[] Vertices { get; private set; }
    public Triangle[] Triangles { get; private set; }
    public int Material { get; set; }
    public BoundingBox BoundingBox { get; private set; }
    
    public Mesh(MeshData data, Scene scene)
    {
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

        int triangleCount = faceIndices.Length / 3;
        Triangles = new Triangle[triangleCount];

        for (int i = 0; i < triangleCount; i++)
        {
            int i0 = vertexMap[faceIndices[i * 3]];
            int i1 = vertexMap[faceIndices[i * 3 + 1]];
            int i2 = vertexMap[faceIndices[i * 3 + 2]];

            Triangles[i] = new Triangle(
                Vertices[i0],
                Vertices[i1],
                Vertices[i2]
            );
        }
        
        BoundingBox = new BoundingBox(min, max);
    }

    public Mesh(string plyPath, int material = 0)
    {
        Material = material;
        var data = new PlyData(plyPath); // Load the PLY data from the file
        Triangles = data.triangles;
        Vertices = data.vertices;
    }
}