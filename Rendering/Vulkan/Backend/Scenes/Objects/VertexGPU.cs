using System.Numerics;
using System.Runtime.InteropServices;
using Silk.NET.Vulkan;

namespace Raytracer.Rendering.Vulkan.Backend.Scenes.Objects;

[StructLayout(LayoutKind.Sequential)]
public struct VertexGPU
{
    public Vector3 Position; float _pad0;
    public Vector3 Normal; float _pad1;
    public Vector2 UV; Vector2 _pad2;

    public override string ToString()
    {
        return $"Pos: {Position}, Normal: {Normal}, UV: {UV}";
    }
}