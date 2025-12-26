using System.Numerics;
using System.Runtime.InteropServices;

namespace Raytracer.Rendering.Vulkan.Backend.Scenes.Objects;

[StructLayout(LayoutKind.Sequential)]
public struct PointLightGPU
{
    public Vector3 position;
    public Vector3 intensity;

    public override string ToString()
    {
        return $"PointLightGPU(Position={position}, Intensity={intensity})";
    }
}