using System.Numerics;
using System.Runtime.InteropServices;

namespace Raytracer.Rendering.Vulkan.Backend.Scenes.Objects;

[StructLayout(LayoutKind.Sequential)]
public struct PointLightGPU
{
    public Vector3 position; float _pad0;
    public Vector3 intensity; float _pad1;

    public override string ToString()
    {
        return $"PointLightGPU(Position={position}, Intensity={intensity})";
    }
}