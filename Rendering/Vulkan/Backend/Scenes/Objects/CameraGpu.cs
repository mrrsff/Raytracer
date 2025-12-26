using System.Numerics;
using System.Runtime.InteropServices;

namespace Raytracer.Rendering.Vulkan.Backend.Scenes.Objects;

[StructLayout(LayoutKind.Sequential)]
public struct CameraGpu
{
    public Vector3 Position;
    private float _pad0; // Ensures Forward starts at byte 16

    public Vector3 Forward;
    private float _pad1; // Ensures Right starts at byte 32

    public Vector3 Right;
    private float _pad2; // Ensures Up starts at byte 48

    public Vector3 Up;
    private float _pad3; // Ensures FovY starts at byte 64

    public float FovY;
    public float AspectRatio;
    private Vector2 _pad4; // Rounds the whole struct to a multiple of 16 (80 bytes)

    public override string ToString()
    {
        return $"CameraGpu(Position: {Position}, Forward: {Forward}, Right: {Right}, Up: {Up}, FovY: {FovY}, AspectRatio: {AspectRatio})";
    }
}