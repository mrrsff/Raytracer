using System.Numerics;
using Raytracer.IO.Ply;
using Raytracer.IO.SceneLoaders;
using Raytracer.Scenes.Runtime;

namespace Raytracer.Scenes.Content.Datas.Objects;

public class PlyData
{
    public Vector3[] vertices { get; private set; }
    public int[][] faces { get; private set; }
    public Triangle[] triangles { get; private set; }
    public PlyData(string filePath)
    {
        // NOT WORKING
        (vertices, faces) = PlyImporter.Parse(filePath);
        Console.WriteLine($"Loaded PLY: {filePath} with {vertices.Length} vertices and {faces.Length} faces.");
        InitializeTriangles();
    }
    
    private void InitializeTriangles()
    {
        triangles = new Triangle[faces.Length];
        for (int i = 0; i < faces.Length; i++)
        {
            var f = faces[i];
            if (f.Length < 3) continue; // Skip invalid faces
            triangles[i] = new Triangle(vertices[f[0]], vertices[f[1]], vertices[f[2]]);
        }
    }
}