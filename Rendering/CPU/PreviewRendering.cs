using Raytracer.Core;
using Raytracer.IO.Images;
using Raytracer.Rendering.CPU.Raytracing;
using Raytracer.Rendering.CPU.SDL2;
using Raytracer.Scenes;

namespace Raytracer.Rendering.CPU;

public class PreviewRendering : RenderingTechnique
{
    private const string OutputDirectory = "Outputs";

    public override void StartRendering(params string[] args)
    {
        AssureOutputDirectory();

        var scenes = SceneProvider.GetScenes(Params.ScenePath);
        var firstScene = SceneProvider.GetFirstScene(Params.ScenePath);
        if (firstScene == null)
        {
            Console.WriteLine("No scenes found to render.");
            return;
        }

        Params.OutputDirectory = ResolveOutputDirectory(firstScene);
        using var preview = TryCreatePreview();

        foreach (var scene in scenes)
        {
            RenderScene(scene, preview);
        }
    }

    private void RenderScene(Scene scene, SDLPreview? preview)
    {
        scene.Initialize();
        var renderer = new RayTracerRenderer(scene);

        using var cts = new CancellationTokenSource();
        RayStats.ThroughputMonitor(TimeSpan.FromSeconds(10), cts.Token);

        var bufferLock = new object();
        ImageBuffer? buffer = null;

        var renderTask = Task.Run(() =>
        {
            for (int cameraIndex = 0; cameraIndex < scene.Content.Cameras.Camera.Count; cameraIndex++)
            {
                lock (bufferLock)
                {
                    buffer = renderer.CreateEmptyImageBuffer(cameraIndex);
                    preview?.SetContentDimensions(buffer.Width, buffer.Height);
                }

                RayStats.Reset();
                renderer.RenderIntoExistingBuffer(cameraIndex, buffer);
                SaveOutputs(renderer, buffer);
            }
        });

        RunPreviewLoop(preview, bufferLock, () => buffer);

        cts.Cancel();
        renderTask.Wait();

        Debug.Log($"Finished rendering scene '{scene.GetCamera(0).ImageName}'");
    }

    private static void SaveOutputs(RayTracerRenderer renderer, ImageBuffer buffer)
    {
        var paths = new List<string>();
        foreach (var img in renderer.Camera.ApplyTonemaps(buffer))
            paths.Add(MediaSaver.SaveImage(Params.OutputDirectory, img));
        
        Debug.Log("Saved outputs to:");
        foreach (var path in paths)
            Debug.Log(" - " + path);
    }

    private static void RunPreviewLoop(SDLPreview? preview, object bufferLock, Func<ImageBuffer?> bufferAccessor)
    {
        if (preview == null)
            return;

        int fps = 144;
        int frameDurationMs = 1000 / fps;
        while (preview.PollEvents())
        {
            lock (bufferLock)
            {
                var buffer = bufferAccessor();
                if (buffer == null) continue;

                try
                {
                    var bytes = buffer.ToByteBuffer();
                    preview.UpdateFrame(bytes, buffer.Width, buffer.Height);
                }
                catch (Exception e)
                {
                    Debug.Log("Error updating preview window: " + e.Message);
                }
            }
            
            Thread.Sleep(frameDurationMs);
        }
        // preview.SaveTextureToFile(Path.Combine(Params.OutputDirectory, "preview_final.png"));
        Debug.Log("Preview window closed, stopping rendering.");
    }

    private static SDLPreview? TryCreatePreview()
    {
        if (!Params.EnablePreview)
            return null;

        try
        {
            return new SDLPreview(800, 600);
        }
        catch (Exception e)
        {
            Console.WriteLine("Warning: SDL2 preview window could not be initialized.");
            Console.WriteLine(e.Message);
            return null;
        }
    }

    private static string ResolveOutputDirectory(Scene firstScene)
    {
        if (!Params.IsDirectory)
            return OutputDirectory;

        var baseName = firstScene.GetCamera(0).ImageName
            .Split('_')
            .FirstOrDefault() ?? "Unknown";

        var dir = Path.Combine(OutputDirectory, baseName);
        Directory.CreateDirectory(dir);
        return dir;
    }

    private static void AssureOutputDirectory()
    {
        Directory.CreateDirectory(OutputDirectory);
    }
}
