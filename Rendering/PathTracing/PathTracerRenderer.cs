using System.Numerics;
using Raytracer.Core;
using Raytracer.IO.Images;
using Raytracer.Rendering.Intersections;
using Raytracer.Rendering.Sampling;
using Raytracer.Scenes;
using Raytracer.Scenes.Content.Datas.Camera;

namespace Raytracer.Rendering.PathTracing;

public sealed class PathTracerRenderer : CPURenderer
{
    public PathTracerRenderer(Scene scene) : base(scene)
    {
    }

    protected override void OnRender(ImageBuffer buffer)
    {
        Debug.Log("PathTracerRenderer started with Camera: " + Camera);
        DynamicThreadPoolRender(Camera, buffer);
    }

    private void DynamicThreadPoolRender(Camera camera, ImageBuffer buffer)
    {
        int width = buffer.Width;
        int height = buffer.Height;
        
        const int tileSize = 16;

        int tilesX = (width  + tileSize - 1) / tileSize;
        int tilesY = (height + tileSize - 1) / tileSize;
        int tileCount = tilesX * tilesY;

        var tiles = new (int x, int y)[tileCount];

        int idx = 0;
        for (int ty = 0; ty < height; ty += tileSize)
        for (int tx = 0; tx < width;  tx += tileSize)
            tiles[idx++] = (tx, ty);

        for (int i = tileCount - 1; i > 0; i--)
        {
            int j = ThreadRng.NextInt(i + 1);
            (tiles[i], tiles[j]) = (tiles[j], tiles[i]);
        }

        int nextTile = -1;  // will be incremented before use

        int workerCount = Environment.ProcessorCount;  // usually fastest in practice
        var tasks = new Task[workerCount];
        
        for (int w = 0; w < workerCount; w++)
        {
            tasks[w] = Task.Run(() =>
            {
                while (true)
                {
                    int tileIndex = Interlocked.Increment(ref nextTile);
                    if (tileIndex >= tileCount)
                        break;

                    var (startX, startY) = tiles[tileIndex];
                    int endX = Math.Min(startX + tileSize, width);
                    int endY = Math.Min(startY + tileSize, height);

                    for (int y = startY; y < endY; y++)
                    for (int x = startX; x < endX; x++)
                    {
                        Ray ray = camera.GenerateRay(x, y);
                        Vector3 color = TracePixel(x, y, ray, camera.RendererParams);
                        buffer.AddSample(x, y, color);
                    }
                }
            });
        }
    }

    private Vector3 TracePixel(int x, int y, Ray ray, List<RendererParams> extensions)
    {
        return Li(ray);
    }

    private Vector3 Li(Ray ray)
    {
        Vector3 L = Vector3.Zero;
        Vector3 beta = Vector3.One; 

        for (int depth = 0; depth < Scene.Content.MaxRecursionDepth; depth++)
        {
            IntersectionInfo hit = Scene.Intersect(ray);
            if (!hit.Hit)
                break;
        }

        return L;
    }
}