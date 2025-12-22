using System.Numerics;
using System.Runtime.InteropServices;

namespace Raytracer.Rendering.Vulkan.Backend.Scene;

[StructLayout(LayoutKind.Sequential)]
public struct SceneGlobals
{
    public Vector3 AmbientLightColor; float _pad0;
}