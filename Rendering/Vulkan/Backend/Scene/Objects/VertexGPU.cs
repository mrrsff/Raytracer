using System.Numerics;
using System.Runtime.InteropServices;

namespace Raytracer.Rendering.Vulkan.Backend.Scene;

[StructLayout(LayoutKind.Sequential)]
public struct VertexGPU
{
    public Vector3 Position; float Padding1;
    public Vector3 Normal;   float Padding2;
    public Vector2 UV;
}