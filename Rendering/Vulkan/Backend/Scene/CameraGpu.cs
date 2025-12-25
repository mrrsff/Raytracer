using System.Numerics;
using System.Runtime.InteropServices;

namespace Raytracer.Rendering.Vulkan.Backend.Scene;

[StructLayout(LayoutKind.Sequential)]
public struct CameraGpu
{
    public Vector3 Position;   float _pad0;

    public Vector3 Forward;    float _pad1;
    public Vector3 Right;      float _pad2;
    public Vector3 Up;         float _pad3;

    public float FovY;  // radians
    public float AspectRatio;
    public Vector2 _pad4;
}