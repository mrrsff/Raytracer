using System.Collections.Concurrent;
using Raytracer.Core;
using Raytracer.IO.ImageSavers;
using Raytracer.Rendering;
using Raytracer.Rendering.SDL2;
using Raytracer.Scenes;

namespace Raytracer;

public static class Program
{
    private const string OutputDirectory = "Outputs";

    private static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: ./raytracer scene.json");
            return;
        }
        
        Params.FromArgs(args);

        AssureOutputDirectory();

        var scenes = SceneProvider.GetScenes(Params.ScenePath).ToList();
        
        SDLPreview preview = null;
        if (Params.EnablePreview)
        {
            preview = new SDLPreview(800, 600);
        }
        
        var imagePaths = new ConcurrentBag<string>();

        // Write progress
        Task.Run(() =>
        {
            var totalCameras = scenes.Sum(s => s.Content.Cameras.Camera.Count);
            while (imagePaths.Count < totalCameras)
            {
                Console.Write($"\rRendering progress: {imagePaths.Count}/{totalCameras} images rendered.");
                Thread.Sleep(200);
            }
            Console.WriteLine($"\rRendering progress: {totalCameras}/{totalCameras} images rendered.");
        });
        
        foreach (var scene in scenes)
        {
            scene.Initialize();
            var renderer = new RayTracerRenderer(scene);
            
            object bufferLock = new object();
            ImageBuffer buffer = null!;
            var renderTask = Task.Run(() =>
            {
                for (int i = 0; i < scene.Content.Cameras.Camera.Count; i++)
                {
                    lock (bufferLock)
                    {
                        buffer = renderer.CreateEmptyImageBuffer(i);
                        preview?.SetContentDimensions(buffer.Width, buffer.Height);
                    }
                    renderer.RenderIntoExistingBuffer(i, buffer);

                    string path;
                    if (Params.IsDirectory)
                    {
                        var strippedOutputName = buffer.OutputName.Split('_').FirstOrDefault() ?? "Unknown";
                        var dirPath = Path.Combine(OutputDirectory, strippedOutputName);
                        if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
                        path = MediaSaver.SaveImage(dirPath, buffer);
                    }
                    else
                    {
                        path = MediaSaver.SaveImage(OutputDirectory, buffer);
                    }
                    imagePaths.Add(path);
                }
            });
            
            if (preview != null)
            {
                bool running = true;
                while (running)
                {
                    running = preview.PollEvents();
                    lock (bufferLock)
                    {
                        if (buffer != null)
                        {
                            var bytes = buffer.ToByteBuffer();
                            preview.UpdateFrame(bytes, buffer.Width, buffer.Height);
                        }
                    }
                    Thread.Sleep(25);
                }
            }
            renderTask.Wait();
            Debug.Log($"Finished rendering scene '{scene.GetCamera(0).ImageName}'");
        }
        
        if (Params.IsDirectory)
        {
            MediaSaver.SaveGIF(OutputDirectory, imagePaths.ToList(), 30);
        }
    }

    private static void AssureOutputDirectory()
    {
        if (!Directory.Exists(OutputDirectory))
        {
            Directory.CreateDirectory(OutputDirectory);
        }
    }
}