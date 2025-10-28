using System;
using System.Diagnostics;
using Raytracer.IO;
using Raytracer.IO.ImageSavers;
using Raytracer.IO.SceneLoaders;
using Raytracer.Rendering;

namespace Raytracer;

public static class Program
{
    public static string WorkingDirectory;
    private const string OutputDirectory = "Outputs";
    private static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: ./raytracer scene.json");
            return;
        }

        var scenePath = args[0];
        if (!File.Exists(scenePath))
        {
            throw new FileNotFoundException("Scene file not found: " + scenePath);
        }

        var x = 896;
        var y = 561;
        AssureOutputDirectory();
        var scene = SceneLoader.Load(scenePath);
        WorkingDirectory = Path.GetDirectoryName(scenePath) ?? "";
        
        scene.Initialize();
        Console.WriteLine("Scene loaded and initialized. Primitive count: " + scene.Geometries.ConvertAll(m => m.GetPrimitiveCount()).Sum());
        var renderer = new RayTracerRenderer(scene);
        for (int i = 0; i < scene.Content.Cameras.Camera.Count; i++)
        {
            var result = renderer.Render(i);
            ImageSaver.SaveImage($"{OutputDirectory}/{result.OutputName}", result);
        }
        
        Console.WriteLine("All renderings complete.");
    }
    
    private static void AssureOutputDirectory()
    {
        if (!Directory.Exists(OutputDirectory))
        {
            Directory.CreateDirectory(OutputDirectory);
        }
    }
}