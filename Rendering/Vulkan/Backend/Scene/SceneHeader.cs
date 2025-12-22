using System.Numerics;
using System.Runtime.InteropServices;

namespace Raytracer.Rendering.Vulkan.Backend.Scene;

[StructLayout(LayoutKind.Sequential)]
public struct SceneHeader
{
    public uint VertexCount;
    public uint IndexCount;
    public uint TextureCount;
    public uint LightCount;

    public uint VertexOffset;
    public uint IndexOffset;
    public uint TextureOffset;
    public uint LightOffset;
}
