using System.Numerics;
using System.Runtime.InteropServices;

namespace Raytracer.Rendering.Vulkan.Backend.Scenes.Objects;

[StructLayout(LayoutKind.Sequential)]
public struct SphereGPU
{
    public Vector3 Center;
    public float Radius;
    public int MaterialIndex;
    private float _padding1;
    private float _padding2;
    private float _padding3;

    public override string ToString()
    {
        return $"SphereGPU(Center: {Center}, Radius: {Radius}, MaterialIndex: {MaterialIndex})";
    }
}