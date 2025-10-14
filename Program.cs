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
    private static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: ./raytracer scene.json");
            return;
        }

        var scenePath = args[0];
        var scene = SceneLoader.Load(scenePath);
        WorkingDirectory = Path.GetDirectoryName(scenePath) ?? "";
        int overrideAmount = 1;
        if (args.Length >= 2)
        {
            var overrideInt = args[1];
            if (int.TryParse(overrideInt, out overrideAmount))
                overrideAmount = Math.Max(1, overrideAmount);
            else
                overrideAmount = 1;
        }
        
        scene.Initialize();
        Console.WriteLine("Scene loaded and initialized. Polygon count: " + scene.Meshes.ConvertAll(m => m.Triangles.Length).Sum());
        var renderer = new RayTracerRenderer(scene);
        for (int i = 0; i < scene.Content.Cameras.Camera.Count; i++)
        {
            var result = renderer.Render(i, overrideAmount);
            ImageSaver.SaveImage("Outputs/" + result.OutputName, result);
        }
        
        Console.WriteLine("All renderings complete.");
    }
}