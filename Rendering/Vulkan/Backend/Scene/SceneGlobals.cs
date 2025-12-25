using System.Numerics;
using System.Runtime.InteropServices;

namespace Raytracer.Rendering.Vulkan.Backend.Scene;

[StructLayout(LayoutKind.Sequential)]
public struct SceneGlobals
{
    public Vector3 AmbientLightColor; public float Padding0;
    public int NumSpheres;
    public int NumPointLights; 
    public int Padding1, Padding2;
}