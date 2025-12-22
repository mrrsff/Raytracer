using System.Numerics;
using System.Runtime.InteropServices;

namespace Raytracer.Rendering.Vulkan.Backend.Scene;

[StructLayout(LayoutKind.Sequential)]
public struct PointLight
{
    public Vector4 Position;
    public Vector4 Color;
}