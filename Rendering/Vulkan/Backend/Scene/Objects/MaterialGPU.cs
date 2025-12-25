using System.Numerics;
using System.Runtime.InteropServices;

namespace Raytracer.Rendering.Vulkan.Backend.Scene.Objects;

[StructLayout(LayoutKind.Sequential)]
public struct MaterialGPU
{
    public Vector3 Albedo;
    public float Roughness;

    public Vector3 Emission;
    public float Metallic;

    public uint Type;
    private uint _pad0;
    private uint _pad1;
    private uint _pad2;
}