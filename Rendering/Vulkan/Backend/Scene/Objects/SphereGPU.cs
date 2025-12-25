using System.Numerics;
using System.Runtime.InteropServices;

namespace Raytracer.Rendering.Vulkan.Backend.Scene.Objects;

[StructLayout(LayoutKind.Sequential)]
public struct SphereGPU
{
    public Vector3 Center;
    public float Radius;
}