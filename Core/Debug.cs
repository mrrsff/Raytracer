using System.Numerics;
using System.Runtime.CompilerServices;

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
    
    public static void Log(string msg,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        Console.WriteLine($"[{Path.GetFileNameWithoutExtension(filePath)}::{memberName}:{lineNumber}] {msg}");
    }
}