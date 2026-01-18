using System.Runtime.CompilerServices;

namespace Raytracer.Core;

public static class Debug
{
    public static bool PrintRenderTime = true;
    public static bool PrintSceneInfo = true;
    
    public static bool MeasureRayThroughput = true;
    
    public static bool DebugPLYLoading = false;
    
    public static bool DebugBVHBuildTime = false;
    public static bool ShowBVHBoxes = false;
    public static bool ShowTLASBoxes = false;
    
    public static bool UseParallelBVHBuild = true;
    public static bool UseSAH = true;
    
    public static bool EnableDebugRendering = false;
    public static bool RenderUVs = false;
    public static bool RenderNormals = false;
    
    public static void Log(object msg,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        Console.WriteLine($"[{Path.GetFileNameWithoutExtension(filePath)}::{memberName}:{lineNumber}] {msg}");
    }
}