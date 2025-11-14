using System;
using System.IO;
using System.Linq;
using Raytracer.IO.ImageSavers;
using Raytracer.IO.SceneLoaders;
using Raytracer.Rendering;
using Raytracer.Rendering.SDL2;

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

        AssureOutputDirectory();
        var scene = SceneLoader.Load(scenePath);
        WorkingDirectory = Path.GetDirectoryName(scenePath) ?? "";
        scene.Initialize();

        var renderer = new RayTracerRenderer(scene);
        var buffer = renderer.CreateEmptyImageBuffer(0);
        
        var sdl = new SDLPreview(buffer.Width, buffer.Height);
        
        Task.Run(() =>
        {
            for (int i = 0; i < scene.Content.Cameras.Camera.Count; i++)
            {
                buffer = renderer.CreateEmptyImageBuffer(i);
                renderer.RenderIntoExistingBuffer(i, buffer);
                ImageSaver.SaveImage($"{OutputDirectory}/{buffer.OutputName}", buffer);
            }
        });
        
        while (sdl.PollEvents())
        {
            var bytes = buffer.ToByteBuffer();
            sdl.UpdateFrame(bytes);
            Thread.Sleep(16);
        }
        
        Console.WriteLine("Scene loaded and initialized. Primitive count: " +
                          scene.Geometries.ConvertAll(m => m.GetPrimitiveCount()).Sum());
        
        
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