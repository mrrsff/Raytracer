using System.Collections.Concurrent;
using Raytracer.Core;
using Raytracer.IO.Images;
using Raytracer.Rendering.Raytracing;
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
        
        SDLPreview? preview = null;
        if (Params.EnablePreview)
        {
            try
            {
                preview = new SDLPreview(800, 600);
            }
            catch (Exception e)
            {
                Console.WriteLine("Warning: SDL2 preview window could not be initialized.");
                Console.WriteLine("If you want to enable the preview window, please ensure that SDL2 is installed on your system.");

                Console.WriteLine("For Debian/Ubuntu/WSL");
                Console.WriteLine("  sudo apt-get update && sudo apt-get install libsdl2-dev libsdl2-gfx-dev libsdl2-image-dev");
                
                Console.WriteLine("For Windows, download the SDL2 runtime from https://www.libsdl.org/download-2.0.php and ensure the DLLs are in your PATH.");
                
                Console.WriteLine();
                Console.WriteLine($"Error: {e.Message}");
                Console.WriteLine("Continuing render without preview window.");
            }
        }
        
        var imagePaths = new ConcurrentBag<string>();
        
        var firstScene = scenes.FirstOrDefault();
        if (firstScene == null)
        {
            Console.WriteLine("No scenes found to render.");
            return;
        }
        
        bool isDirectory = Params.IsDirectory; 
        string outputName = firstScene.GetCamera(0).ImageName;
        string outputDir = isDirectory
            ? Path.Combine(OutputDirectory, outputName.Split('_').FirstOrDefault() ?? "Unknown")
            : OutputDirectory;
        if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);
        Params.OutputDirectory = outputDir;

        var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;
        RayStats.ThroughputMonitor(TimeSpan.FromSeconds(10), cancellationToken);
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

                    try
                    {
                        RayStats.Reset();
                        renderer.RenderIntoExistingBuffer(i, buffer);
                        var path = MediaSaver.SaveImage(outputDir, buffer);
                        imagePaths.Add(path);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                }
            });
            
            if (preview != null)
            {
                if (!isDirectory)
                {
                    bool running = true;
                    bool finished = false;
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

                        if (renderTask.IsCompleted && !finished)
                        {
                            finished = true;
                            cts.Cancel();
                        }

                        Thread.Sleep(25);
                    }
                }
                else
                {
                    bool running = true;
                    while (!renderTask.IsCompleted && running)
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
            }
            
            renderTask.Wait();
            
            cts.Cancel();
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