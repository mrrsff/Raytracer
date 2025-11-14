using System;
using System.IO;
using System.Linq;
using Raytracer.IO.ImageSavers;
using Raytracer.IO.SceneLoaders;
using Raytracer.Rendering;
using Raytracer.Rendering.SDL2;
using Raytracer.Scenes;

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
        Params parameters = Params.FromArgs(args);
        if (!File.Exists(parameters.ScenePath))
        {
            throw new FileNotFoundException("Scene file not found: " + parameters.ScenePath);
        }

        AssureOutputDirectory();
        var scene = SceneLoader.Load(parameters.ScenePath);
        WorkingDirectory = Path.GetDirectoryName(parameters.ScenePath) ?? "";
        scene.Initialize();

        var renderer = new RayTracerRenderer(scene);
        
        Console.WriteLine("Scene loaded and initialized. Primitive count: " +
                          scene.Geometries.ConvertAll(m => m.GetPrimitiveCount()).Sum());

        if (parameters.EnablePreview)
        {
            CreatePreview(scene, renderer);
        }
        else
        {
            for (int i = 0; i < scene.Content.Cameras.Camera.Count; i++)
            {
                var buffer = renderer.CreateEmptyImageBuffer(i);
                renderer.RenderIntoExistingBuffer(i, buffer);
                ImageSaver.SaveImage($"{OutputDirectory}/{buffer.OutputName}", buffer);
            }
        }
    }

    private static void AssureOutputDirectory()
    {
        if (!Directory.Exists(OutputDirectory))
        {
            Directory.CreateDirectory(OutputDirectory);
        }
    }
    
    private static void CreatePreview(Scene scene, RayTracerRenderer renderer)
    {
        ImageBuffer buffer = renderer.CreateEmptyImageBuffer(0);
        var sdl = new SDLPreview(buffer.Width, buffer.Height);

        var renderTask = Task.Run(() =>
        {
            for (int i = 0; i < scene.Content.Cameras.Camera.Count; i++)
            {
                buffer = renderer.CreateEmptyImageBuffer(i);
                renderer.RenderIntoExistingBuffer(i, buffer);
                ImageSaver.SaveImage($"{OutputDirectory}/{buffer.OutputName}", buffer);
            }
        });

        const int targetFps = 144;
        const int frameDelay = 1000 / targetFps;
        while (sdl.PollEvents())
        {
            var bytes = buffer.ToByteBuffer();
            sdl.UpdateFrame(bytes);
            Thread.Sleep(frameDelay);
        }

        renderTask.Wait();
    }
}