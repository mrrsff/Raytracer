using System.Numerics;
using System.Runtime.InteropServices;

namespace Raytracer.Rendering.Vulkan.Backend.Scene.Objects;

[StructLayout(LayoutKind.Sequential)]
public struct PointLightGPU
{
    public Vector3 position;
    public float intensity;
    public Vector3 color;
    public float radius;
}