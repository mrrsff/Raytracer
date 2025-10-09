using System;
using Raytracer.IO;
using Raytracer.Rendering;

namespace Raytracer;

static class Program
{
    static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Usage: ./raytracer scene.json");
            return;
        }

        string scenePath = args[0];
        var scene = SceneLoader.Load(scenePath);

        var renderer = new RayTracerRenderer(scene);
        renderer.Render();

        Console.WriteLine("Rendering complete!");
    }
}