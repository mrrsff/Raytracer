using System;
using Raytracer.IO.SceneLoaders;
using Raytracer.Rendering;

namespace Raytracer;

internal static class Program
{
    private static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Usage: ./raytracer scene.json");
            return;
        }

        var scenePath = args[0];
        var scene = SceneLoader.Load(scenePath);

        Console.WriteLine(scene.Content);

        var renderer = new RayTracerRenderer(scene);
        renderer.Render();
    }
}