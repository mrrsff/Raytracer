using Raytracer.Rendering;
using Raytracer.Rendering.CPU;
using Raytracer.Rendering.Vulkan;

namespace Raytracer;

public static class Program
{
    private static RenderingTechnique _technique;
    private static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: ./raytracer scene.json");
            return;
        }
        
        Params.FromArgs(args);

        _technique = new PreviewRendering();
        // _technique = new GPURendering();
        _technique.StartRendering(args);
    }
}