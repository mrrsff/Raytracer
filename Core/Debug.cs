using System.Numerics;

namespace Raytracer.Core;

public static class Debug
{
    public static bool DebugPLYLoading = false;

    public static Material DebugMaterial = new Material
    {
        Id = -1,
        Type = MaterialType.None,
        AmbientReflectance = new Vector3(1f, 1f, 1f)
    };

    public static bool UseMultiThreading = true;
    public static bool UseIterativeTracing = true;
    public static bool UseDynamicThreading = true;

    public static bool DebugBVHBuildTime = false;
    public static bool ShowBVHBoxes = false;
    public static bool ShowBVHBoxesLeafNodesOnly = false;
    public static bool ShowTLASBoxes = false;
    public static bool UseParallelBVHBuild = true;
    
    public static bool UseSAH = true;
}