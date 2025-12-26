using System.Runtime.CompilerServices;

namespace Raytracer.Core;

public static class Dump
{
    public static void CreateDump(object msg, string fileName, 
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        using var writer = new StreamWriter(fileName);
        
        string info = $"[{Path.GetFileNameWithoutExtension(filePath)}::{memberName}:{lineNumber}] Dump created on {DateTime.Now}";
        writer.WriteLine(info);
        writer.WriteLine(msg.ToString());
    }
}