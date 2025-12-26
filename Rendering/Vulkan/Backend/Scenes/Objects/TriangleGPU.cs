using System.Runtime.InteropServices;

namespace Raytracer.Rendering.Vulkan.Backend.Scenes.Objects;

[StructLayout(LayoutKind.Sequential)]
public struct TriangleGPU
{
    public int Vertex0;
    public int Vertex1;
    public int Vertex2;

    public override string ToString()
    {
        return $"TriangleGPU(V0: {Vertex0}, V1: {Vertex1}, V2: {Vertex2})";
    }
}