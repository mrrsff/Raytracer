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
        var sceneDir = Path.GetDirectoryName(scenePath)?.Split(Path.DirectorySeparatorChar).Last();
        if (!File.Exists(scenePath))
        {
            throw new FileNotFoundException("Scene file not found: " + scenePath);
        }
        
        AssureOutputDirectory(sceneDir ?? "");
        var scene = SceneLoader.Load(scenePath);
        WorkingDirectory = Path.GetDirectoryName(scenePath) ?? "";
        
        scene.Initialize();
        Console.WriteLine("Scene loaded and initialized. Polygon count: " + scene.Meshes.ConvertAll(m => m.Triangles.Length).Sum());
        var renderer = new RayTracerRenderer(scene);
        for (int i = 0; i < scene.Content.Cameras.Camera.Count; i++)
        {
            var result = renderer.Render(i);
            ImageSaver.SaveImage("Outputs/" + sceneDir + "/" + result.OutputName, result);
        }
        
        Console.WriteLine("All renderings complete.");
    }
    
    private static void AssureOutputDirectory(string sceneDir)
    {
        var outputDir = Path.Combine("Outputs", sceneDir);
        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }
    }
}