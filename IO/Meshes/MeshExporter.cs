using Raytracer.Rendering.Vulkan.Backend.Scenes.Objects;
using Raytracer.Scenes.Runtime;
using Raytracer.Scenes.Runtime.Meshes;

namespace Raytracer.IO.Meshes;

public static class MeshExporter
{
    public static void ExportToOBJ(string filePath, MeshDefinition meshDefinition)
    {
        using var writer = new StreamWriter(filePath);
        
        // Write vertices
        foreach (var vertex in meshDefinition.Vertices)
        {
            writer.WriteLine($"v {vertex.X} {vertex.Y} {vertex.Z}");
        }
        
        // Write texture coordinates
        foreach (var uv in meshDefinition.TexCoords)
        {
            writer.WriteLine($"vt {uv.X} {uv.Y}");
        }
        
        // Write normals
        foreach (var normal in meshDefinition.VertexNormals)
        {
            writer.WriteLine($"vn {normal.X} {normal.Y} {normal.Z}");
        }
        
        // Write faces
        foreach (var triangle in meshDefinition.Triangles)
        {
            // OBJ format uses 1-based indexing
            int v1 = triangle.I0 + 1;
            int v2 = triangle.I1 + 1;
            int v3 = triangle.I2 + 1;
            writer.WriteLine($"f {v1}/{v1}/{v1} {v2}/{v2}/{v2} {v3}/{v3}/{v3}");
        }
    }
    
    public static void ExportToOBJ(string filePath, MeshGPU meshGpu, VertexGPU[] verticesGpu, TriangleGPU[] trianglesGpu)
    {
        using var writer = new StreamWriter(filePath);
        
        // Write vertices
        foreach (var vertex in verticesGpu)
        {
            writer.WriteLine($"v {vertex.Position.X} {vertex.Position.Y} {vertex.Position.Z}");
        }
        
        // Write texture coordinates
        foreach (var vertex in verticesGpu)
        {
            writer.WriteLine($"vt {vertex.UV.X} {vertex.UV.Y}");
        }
        
        // Write normals
        foreach (var vertex in verticesGpu)
        {
            writer.WriteLine($"vn {vertex.Normal.X} {vertex.Normal.Y} {vertex.Normal.Z}");
        }
        
        // Write faces
        foreach (var triangle in trianglesGpu)
        {
            // OBJ format uses 1-based indexing
            int v1 = triangle.Vertex0 + 1;
            int v2 = triangle.Vertex1 + 1;
            int v3 = triangle.Vertex2 + 1;
            writer.WriteLine($"f {v1}/{v1}/{v1} {v2}/{v2}/{v2} {v3}/{v3}/{v3}");
        }
    }
}