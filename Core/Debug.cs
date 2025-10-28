using System.Numerics;

namespace Raytracer.Core;

public static class Debug
{
    public static bool ShowBVHBoxes = false;
    public static bool UseMultiThreading = true;
    public static Material DebugMaterial = new Material
    {
        Id = -1,
        Type = MaterialType.None,
        AmbientReflectance = new Vector3(1f, 1f, 1f)
    };
}