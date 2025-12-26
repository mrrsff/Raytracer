using System.Numerics;
using System.Runtime.InteropServices;

namespace Raytracer.Rendering.Vulkan.Backend.Scenes.Objects;

[StructLayout(LayoutKind.Sequential)]
public struct VertexGPU
{
    public Vector3 Position;
    public Vector3 Normal;
    public Vector2 UV;

    public override string ToString()
    {
        return $"Pos: {Position}, Normal: {Normal}, UV: {UV}";
    }
}