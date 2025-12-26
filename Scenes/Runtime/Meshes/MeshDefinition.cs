using System.Numerics;
using Raytracer.Scenes.Content.Datas;
using Raytracer.Scenes.Content.Datas.Objects;
using Raytracer.Scenes.Content.Datas.Textures;
using Raytracer.Scenes.Runtime.Meshes.BVH;

namespace Raytracer.Scenes.Runtime.Meshes;

public class MeshDefinition
{
    public int Id { get; set; }
    public Vector3[] Vertices { get; private set; }
    public Triangle[] Triangles { get; private set; }
    public Vector3[] VertexNormals { get; private set; }
    public Vector2[] TexCoords { get; private set; }
    public BoundingVolumeHierarchy BVH { get; set; }

    public MeshDefinition(in MeshData meshData, in VertexData vertexData, in TexCoordData texCoordData)
    {
        var faceIndices = meshData.Faces.Data;
        int vertexOffset = meshData.Faces.VertexOffset;
        int textureOffset = meshData.Faces.TextureOffset;

        var uniqueIndices = faceIndices.Distinct().ToArray();
        Vertices = new Vector3[uniqueIndices.Length];
        TexCoords = new Vector2[uniqueIndices.Length];

        var vertexMap = new Dictionary<int, int>();
        for (int i = 0; i < uniqueIndices.Length; i++)
        {
            try
            {
                int globalId = uniqueIndices[i];
                Vertices[i] = vertexData.At(globalId + vertexOffset);
                if (texCoordData != null) TexCoords[i] = texCoordData.At(globalId + textureOffset);
                vertexMap[globalId] = i;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error processing vertex with global ID {uniqueIndices[i]}: {e.Message}");
                throw;
            }
        }

        int triangleCount = faceIndices.Length / 3;
        Triangles = new Triangle[triangleCount];

        for (int i = 0; i < triangleCount; i++)
        {
            int i0 = vertexMap[faceIndices[i * 3]];
            int i1 = vertexMap[faceIndices[i * 3 + 1]];
            int i2 = vertexMap[faceIndices[i * 3 + 2]];

            Triangles[i] = new Triangle(i, i0, i1, i2);
            Triangles[i].SetMeshDefinition(this);
        }

        Initialize();
    }

    public MeshDefinition(in PlyData plyData)
    {
        Vertices = plyData.vertices;
        Triangles = plyData.triangles;
        TexCoords = plyData.uv;
        foreach (var tri in Triangles)
        {
            tri.SetMeshDefinition(this);
        }

        Initialize();
    }

    private void Initialize()
    {
        VertexNormals = new Vector3[Vertices.Length];

        foreach (var tri in Triangles)
        {
            Vector3 p0 = Vertices[tri.I0];
            Vector3 p1 = Vertices[tri.I1];
            Vector3 p2 = Vertices[tri.I2];

            Vector3 e1 = p1 - p0;
            Vector3 e2 = p2 - p0;

            Vector3 N = Vector3.Cross(e1, e2);

            VertexNormals[tri.I0] += N;
            VertexNormals[tri.I1] += N;
            VertexNormals[tri.I2] += N;
        }

        for (int i = 0; i < Vertices.Length; i++)
        {
            Vector3 N = Vector3.Normalize(VertexNormals[i]);
            VertexNormals[i] = N;
        }
        
        BVH = new BoundingVolumeHierarchy(this);
    }

    public BoundingBox GetBounds()
    {
        return BVH.GetNode(0).Bounds;
    }
}