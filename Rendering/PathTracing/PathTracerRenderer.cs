using System.Numerics;
using Raytracer.Core;
using Raytracer.Core.Lights;
using Raytracer.IO.Images;
using Raytracer.Rendering.Filtering;
using Raytracer.Rendering.Intersections;
using Raytracer.Rendering.Sampling;
using Raytracer.Scenes;
using Raytracer.Scenes.Content.Datas.Camera;
using Raytracer.Utility;

namespace Raytracer.Rendering.PathTracing;

public sealed class PathTracerRenderer : CPURenderer
{
    public PathTracerRenderer(Scene scene) : base(scene)
    {
    }

    protected override void OnRender(ImageBuffer buffer)
    {
        DynamicThreadPoolRender(Camera, buffer);
    }

    private void DynamicThreadPoolRender(Camera camera, ImageBuffer buffer)
    {
        int width = buffer.Width;
        int height = buffer.Height;

        const int tileSize = 16;

        int tilesX = (width + tileSize - 1) / tileSize;
        int tilesY = (height + tileSize - 1) / tileSize;
        int tileCount = tilesX * tilesY;

        var tiles = new (int x, int y)[tileCount];

        int idx = 0;
        for (int ty = 0; ty < height; ty += tileSize)
        for (int tx = 0; tx < width; tx += tileSize)
            tiles[idx++] = (tx, ty);

        for (int i = tileCount - 1; i > 0; i--)
        {
            int j = ThreadRng.NextInt(i + 1);
            (tiles[i], tiles[j]) = (tiles[j], tiles[i]);
        }

        int nextTile = -1; // will be incremented before use

        int workerCount = Environment.ProcessorCount; // usually fastest in practice
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
                        ProgressiveRenderPixel(x, y, camera, buffer);
                    }
                }
            });
        }

        Task.WaitAll(tasks);
    }

    private void ProgressiveRenderPixel(int x, int y, Camera renderCamera, ImageBuffer buffer)
    {
        Vector2[] pixelSamples = Sampler.MultiJittered.Sample(Camera.NumSamples);
        Vector2[] lensSamples = Sampler.MultiJittered.Sample(Camera.NumSamples);
        float[] timeSamples = Sampler.OneDimensionalUniform(Camera.NumSamples);

        float totalWeight = 0f;
        Vector3 finalColor = Vector3.Zero;
        for (int s = 0; s < Camera.NumSamples; s++)
        {
            float px = x + pixelSamples[s].X;
            float py = y + pixelSamples[s].Y;
            Vector2 lens = lensSamples[s];
            float time = timeSamples[s];

            Ray ray = renderCamera.GenerateRayDRT(px, py, lens, time);

            Vector3 sampleColor = TracePixel(x, y, ray);
            float weight = Filter.Gaussian.Evaluate(pixelSamples[s].X, pixelSamples[s].Y);

            finalColor += sampleColor * weight;
            totalWeight += weight;
        }

        finalColor /= totalWeight;
        buffer.SetPixel(x, y, finalColor);
    }

    private Vector3 TracePixel(int x, int y, Ray ray)
    {
        return Li(x, y, ray);
    }


    private struct PathState
    {
        public Ray Ray;
        public Vector3 Throughput;
        public int Depth;
    }

    private Vector3 Li(int x, int y, Ray ray)
    {
        var stack = new Stack<PathState>();
        stack.Push(new PathState
        {
            Ray = ray,
            Throughput = Vector3.One,
            Depth = 0
        });
        
        Vector3 totalL = Vector3.Zero;
        
        while (stack.Count > 0)
        {
            var state = stack.Pop();
            if (!Has(RendererParams.RussianRoulette) && state.Depth >= Camera.MaxRecursionDepth)
            {
                continue;
            }

            var hit = Scene.Intersect(state.Ray);
            if (!hit.Hit)
            {
                totalL += state.Throughput * Scene.GetBackgroundColor(hit, state.Ray);
                continue;
            }

            hit.Camera = Camera;
            hit.XPixel = x;
            hit.YPixel = y;
            
            if (hit.HitGeometry.IsEmitter && (state.Depth == 0 || !Has(RendererParams.NextEventEstimation)))
            {
                totalL += state.Throughput * hit.HitGeometry.Emission;
            }
            if (Has(RendererParams.NextEventEstimation))
            {
                totalL += state.Throughput * NEE(hit, state.Ray);
            }
            
            int split = (state.Depth == 0 && Camera.SplittingFactor > 1) ? Camera.SplittingFactor : 1;
            for (int i = 0; i < split; i++)
            {
                if (!SampleBSDF(hit, state.Ray, out BSDFSample bsdfSample)) continue;
                
                float cosTheta = MathF.Abs(Vector3.Dot(bsdfSample.Wi, hit.ShadingNormal));
                Vector3 bounceWeight = (bsdfSample.F * cosTheta) / bsdfSample.Pdf;
                Vector3 bounceThroughput = state.Throughput * bounceWeight;
                
                if (!RR(state, bounceThroughput, out Vector3 nextThroughput))
                    continue;
                
                if (nextThroughput == Vector3.Zero)
                    continue;
                
                Ray bounceRay = SpawnRay(hit, bsdfSample);
                stack.Push(new PathState
                {
                    Ray = bounceRay,
                    Throughput = nextThroughput / split,
                    Depth = state.Depth + 1
                });
            }
        }

        return totalL;
    }

    private bool RR(PathState state, Vector3 throughput, out Vector3 nextThroughput)
    {
        nextThroughput = throughput;

        if (Has(RendererParams.RussianRoulette) && state.Depth >= Camera.MinRecursionDepth)
        {
            float q = MathF.Min(throughput.MaxComponent(), 0.99f);

            if (ThreadRng.NextFloat() > q)
            {
                return false;
            }

            nextThroughput /= q;
        }

        return true;
    }
    private bool Has(RendererParams param)
    {
        return Camera.Has(param);
    }

    private bool SampleBSDF(IntersectionInfo hit, Ray ray, out BSDFSample sample)
    {
        Vector3 wo = -ray.Direction;
        
        float woDotN = Vector3.Dot(wo, hit.ShadingNormal);
        if (woDotN <= 0)
        {
            sample = BSDFSample.Invalid;
            return false;
        }

        Vector2 u = new Vector2(ThreadRng.NextFloat(), ThreadRng.NextFloat());
        sample = hit.material!.Bsdf.Sample(wo, hit, u, Has(RendererParams.ImportanceSampling));

        return BSDFSample.IsValidSample(sample);
    }

    private Vector3 NEE(IntersectionInfo hit, Ray ray)
    {
        float pdf;
        var light = Has(RendererParams.ImportanceSampling) ? SampleImportance(hit, ray, out pdf) : SampleUniform(out pdf);
        if (!light.Sample(hit.Point, hit.ShadingNormal, ray.Time, this, out Vector3 wi, out Vector3 Li))
            return Vector3.Zero;
            
        Vector3 wo = -ray.Direction;
        Vector3 f = hit.material!.Bsdf.Evaluate(wo, wi, hit);
        float bsdfPdf = hit.material!.Bsdf.Pdf(wo, wi, hit);

        if (bsdfPdf <= 0f || pdf <= 0f) 
            return Vector3.Zero;

        float cosTheta = MathF.Max(0f, Vector3.Dot(wi, hit.ShadingNormal));
        Vector3 color = (f * Li * cosTheta);
        
        color = Vector3.Clamp(color, Vector3.Zero, Camera.SampleMaxVal * Vector3.One);

        if (Has(RendererParams.MIS_BALANCE)) // Multiple Importance Sampling with Balance Heuristic
        {
            float weight = (pdf) / (pdf + bsdfPdf);
            return color * weight;
        }

        return color / pdf;
    }

    private Ray SpawnRay(IntersectionInfo hit, BSDFSample sample)
    {
        Vector3 direction = sample.Wi;
    
        Vector3 origin = Vector3.Dot(sample.Wi, hit.ShadingNormal) > 0
            ? hit.Point + hit.ShadingNormal * Scene.Content.IntersectionTestEpsilon
            : hit.Point - hit.ShadingNormal * Scene.Content.IntersectionTestEpsilon;
    
        return new Ray(origin, Vector3.Normalize(direction), true, hit.RayTime);
    }
    
    private IObjectLight SampleImportance(IntersectionInfo hit, Ray ray, out float pdf)
    {
        float totalPdf = Scene.ObjectLights.Sum(light => light.Pdf(hit.Point, hit.ShadingNormal, ray.Time, this));

        float r = ThreadRng.NextFloat() * totalPdf;
        float cumulativePdf = 0f;

        foreach (var light in Scene.ObjectLights)
        {
            float lightPdf = light.Pdf(hit.Point, hit.ShadingNormal, ray.Time, this);
            cumulativePdf += lightPdf;
            if (r > cumulativePdf) continue;
            
            pdf = lightPdf / totalPdf;
            return light;
        }

        var lastLight = Scene.ObjectLights.Last();
        pdf = lastLight.Pdf(hit.Point, hit.ShadingNormal, ray.Time, this) / totalPdf;
        return lastLight;
    }
    
    private IObjectLight SampleUniform(out float pdf)
    {
        int lightCount = Scene.ObjectLights.Count;
        int index = ThreadRng.NextInt(lightCount);
        var light = Scene.ObjectLights[index];
        pdf = 1f / lightCount;
        return light;
    }
}