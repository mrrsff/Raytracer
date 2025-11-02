using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using Raytracer.Core;
using Raytracer.IO.ImageSavers;
using Raytracer.Rendering.Intersections;
using Raytracer.Rendering.Shading;
using Raytracer.Scenes;
using Raytracer.Scenes.Content.Datas.Camera;
using Raytracer.Utility;
using Debug = Raytracer.Core.Debug;

namespace Raytracer.Rendering;

public class RayTracerRenderer
{
    public static float IntersectionTestEpsilon;
    public static float ShadowRayEpsilon;
    public Scene Scene { get; }
    private Camera Camera { get; set; } = null!;

    public RayTracerRenderer(Scene scene)
    {
        Scene = scene;
        IntersectionTestEpsilon = scene.Content.IntersectionTestEpsilon;
        ShadowRayEpsilon = scene.Content.ShadowRayEpsilon;
    }
    
    public RenderResult Render(int cameraIndex)
    {
        Camera = Scene.GetCamera(cameraIndex);
        Camera.InitializeCamera();

        Stopwatch sw = Stopwatch.StartNew();
        Console.WriteLine($"Rendering started for {Camera.ImageName}... TIME: {DateTime.Now:HH:mm:ss}");
        RenderResult result = new RenderResult(Camera.ImageResolution, Scene.Content.BackgroundColor);
        result = Debug.UseDynamicThreading
            ? DynamicThreadPoolRender(Camera, result)
            : (Debug.UseMultiThreading ? MultithreadRender(Camera, result) : SingleThreadRender(Camera, result));

        
        sw.Stop();
        Console.WriteLine($"\nRendering finished for {Camera.ImageName} in {sw.Elapsed.TotalSeconds:F2} seconds. TIME: {DateTime.Now:HH:mm:ss}");
        
        result.OutputName = Camera.ImageName;
        return result;
    }
    private RenderResult DynamicThreadPoolRender(Camera renderCamera, RenderResult result)
    {
        int width = result.Width;
        int height = result.Height;

        int totalRows = height;
        int completedRows = 0;
        bool done = false;
        const int timesPerSecond = 5;
        const int interval = 1000 / timesPerSecond;

        Task progressTask = Task.Run(() =>
        {
            int lastPercent = -1;
            while (!done)
            {
                int percent = (int)(Volatile.Read(ref completedRows) * 100.0 / totalRows);
                if (percent != lastPercent)
                {
                    lastPercent = percent;
                    Console.Write($"\rProgress: {percent,3}%");
                }
                Thread.Sleep(interval);
            }
            Console.Write("\rProgress: 100%\n");
        });

        // Dynamic worker thread count
        int maxThreads = Environment.ProcessorCount;
        int minThreads = Math.Max(2, maxThreads / 2);
        int activeThreads = minThreads;

        var queue = new ConcurrentQueue<int>(Enumerable.Range(0, height));
        var tasks = new List<Task>();

        for (int t = 0; t < activeThreads; t++)
        {
            tasks.Add(Task.Run(() =>
            {
                while (queue.TryDequeue(out int j))
                {
                    Vector3[] rowBuffer = new Vector3[width];
                    for (int i = 0; i < width; i++)
                    {
                        Ray primaryRay = renderCamera.GetPrimaryRay(i, j);
                        Vector3 color = Debug.UseIterativeTracing ? TraceRayIterative(primaryRay) : TraceRay(primaryRay, 0, out _);
                        rowBuffer[i] = ColorUtility.Normalize(color);
                    }

                    for (int i = 0; i < width; i++)
                        result.SetPixel(i, j, rowBuffer[i]);

                    Interlocked.Increment(ref completedRows);

                    // Adaptive expansion logic
                    if (completedRows % (height / 10) == 0 && activeThreads < maxThreads)
                    {
                        Interlocked.Increment(ref activeThreads);
                        queue.Enqueue(j + 1);
                    }
                }
            }));
        }

        Task.WaitAll(tasks.ToArray());
        done = true;
        progressTask.Wait();
        return result;
    }
    private RenderResult SingleThreadRender(Camera RenderCamera, RenderResult result)
    {
        int width = result.Width;
        int height = result.Height;


        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                Ray primaryRay = RenderCamera.GetPrimaryRay(i, j);
                Vector3 color = TraceRay(primaryRay, 0, out _);
                result.SetPixel(i, j, ColorUtility.Normalize(color));
            }
            Console.Write($"\rProgress: {(j + 1) * 100 / height,3}%");
        }

        
        return result;
    }
    private RenderResult MultithreadRender(Camera RenderCamera, RenderResult result)
    {
        int width = result.Width;
        int height = result.Height;

        int totalRows = height;
        int completedRows = 0;
        bool done = false;
        const int timesPerSecond = 5;
        const int interval = 1000 / timesPerSecond;

        Task progressTask = Task.Run(() =>
        {
            int lastPercent = -1;
            while (!done)
            {
                int percent = (int)(Volatile.Read(ref completedRows) * 100.0 / totalRows);
                if (percent != lastPercent)
                {
                    lastPercent = percent;
                    Console.Write($"\rProgress: {percent,3}%");
                }
                Thread.Sleep(interval); // check 4 times per second
            }
            Console.Write("\rProgress: 100%\n");
        });

        Parallel.For(0, height, j =>
        {
            Vector3[] rowBuffer = new Vector3[width];
            for (int i = 0; i < width; i++)
            {
                Ray primaryRay = RenderCamera.GetPrimaryRay(i, j);
                Vector3 color = Debug.UseIterativeTracing ? TraceRayIterative(primaryRay) : TraceRay(primaryRay, 0, out _);
                rowBuffer[i] = ColorUtility.Normalize(color);
            }

            for (int i = 0; i < width; i++)
                result.SetPixel(i, j, rowBuffer[i]);

            Interlocked.Increment(ref completedRows);
        });

        done = true;
        progressTask.Wait();
        return result;
    }
    private Vector3 TraceRay(in Ray ray, in int depth, out float distanceTraveled)
    {
        distanceTraveled = 0;
        if (depth > Scene.Content.MaxRecursionDepth)
            return ColorUtility.Black;
        
        IntersectionInfo hit = Scene.Intersect(ray);
        
        if (!hit.Hit)
            return Scene.Content.BackgroundColor;
        
        distanceTraveled = hit.Distance;
        
        if (Debug.RenderNormals) return hit.Normal * 255f;
        
        Vector3 finalColor;
        
        bool inFront = Vector3.Dot(ray.Direction, hit.Normal) < 0f; // Ray is entering the material (facing normal)
        if (hit.material?.Type is MaterialType.Dielectric or MaterialType.Conductor)
        {
            if (!inFront) hit.Normal = -hit.Normal;
            finalColor = inFront ? Shade(hit) : ColorUtility.Magenta;
        }
        else finalColor = Shade(hit);
        
        float cosThetaI = MathF.Abs(Vector3.Dot(-ray.Direction, hit.Normal));

        Vector3 reflectedColor = ColorUtility.Black;
        Vector3 reflectedDir = Vector3.Normalize(Vector3.Reflect(ray.Direction, hit.Normal));
        Ray reflectedRay = new Ray(hit.Point + hit.Normal * Scene.Content.ShadowRayEpsilon, reflectedDir, true);
        if (inFront)
        {
            reflectedColor = TraceRay(reflectedRay, depth + 1, out _);
        }
        
        switch (hit.material?.Type)
        {
            case MaterialType.Mirror:
                finalColor += hit.material.MirrorReflectance * reflectedColor;
                break;
            case MaterialType.Conductor:
            {
                var fresnel = FresnelComputation.ComputeFresnelConductor(hit.material, cosThetaI);
                finalColor += fresnel * hit.material.MirrorReflectance * reflectedColor;
                break;
            }
            case MaterialType.Dielectric:
            {
                const float airRefractionIndex = 1f;
                float etai = inFront ? airRefractionIndex : hit.material.RefractionIndex;
                float etat = inFront ? hit.material.RefractionIndex : airRefractionIndex;
                float eta  = etai / etat;
    
                if (Refract(ray.Direction, hit.Normal, eta, out Vector3 refrDir))
                {
                    Ray refractedRay = new Ray(hit.Point - hit.Normal * Scene.Content.ShadowRayEpsilon, refrDir, true);
                    Vector3 refractedColor = TraceRay(refractedRay, depth + 1, out float insideDistance);
                    if (inFront) // apply absorption only when the ray is inside the material
                    {
                        refractedColor *= GetAbsorption(hit.material.AbsorptionCoefficient, insideDistance);
                    }

                    float fresnel = FresnelComputation.ComputeFresnelDielectric(etai, etat, cosThetaI);
        
                    finalColor += fresnel * reflectedColor + (1f - fresnel) * refractedColor;
                }
                else // total internal reflection
                {
                    Vector3 tirColor = TraceRay(reflectedRay, depth + 1, out float traveled);
                    distanceTraveled += traveled;

                    Vector3 absorption = GetAbsorption(hit.material.AbsorptionCoefficient, traveled);
                    finalColor += tirColor * absorption;
                }
                break;
            }
        }

        return finalColor;
    }
    private Vector3 TraceRayIterative(in Ray primaryRay)
    {
        Vector3 finalColor = Vector3.Zero;

        // Stack for rays to process
        Stack<(Ray ray, int depth, Vector3 weight)> rayStack = new();
        rayStack.Push((primaryRay, 0, Vector3.One));

        while (rayStack.Count > 0)
        {
            var (ray, depth, weight) = rayStack.Pop();
            if (depth > Scene.Content.MaxRecursionDepth)
                continue;

            IntersectionInfo hit = Scene.Intersect(ray);
            if (!hit.Hit)
            {
                // add background scaled by current weight
                finalColor += Scene.Content.BackgroundColor * weight;
                continue;
            }

            if (Debug.RenderNormals)
            {
                finalColor += hit.Normal * 255f * weight;
                continue;
            }
            bool inFront = true;
            if (hit.material?.Type is MaterialType.Dielectric or MaterialType.Conductor)
            {
                inFront = Vector3.Dot(ray.Direction, hit.Normal) < 0f;
                if (!inFront) hit.Normal = -hit.Normal;
            }

            // Local shading
            Vector3 localColor = Shade(hit) * weight;

            // Reflection ray
            Vector3 reflectedDir = Vector3.Normalize(Vector3.Reflect(ray.Direction, hit.Normal));
            Ray reflectedRay = new Ray(hit.Point + hit.Normal * Scene.Content.ShadowRayEpsilon, reflectedDir, true);

            float cosThetaI = MathF.Abs(Vector3.Dot(-ray.Direction, hit.Normal));

            switch (hit.material?.Type)
            {
                case MaterialType.Mirror:
                {
                    finalColor += localColor;
                    Vector3 newWeight = hit.material.MirrorReflectance * weight;
                    rayStack.Push((reflectedRay, depth + 1, newWeight));
                    break;
                }

                case MaterialType.Conductor:
                {
                    var fresnel = FresnelComputation.ComputeFresnelConductor(hit.material, cosThetaI);
                    finalColor += localColor;
                    Vector3 newWeight = fresnel * hit.material.MirrorReflectance * weight;
                    rayStack.Push((reflectedRay, depth + 1, newWeight));
                    break;
                }

                case MaterialType.Dielectric:
                {
                    const float airRefIndex = 1f;
                    float etai = inFront ? airRefIndex : hit.material.RefractionIndex;
                    float etat = inFront ? hit.material.RefractionIndex : airRefIndex;
                    float eta = etai / etat;

                    if (Refract(ray.Direction, hit.Normal, eta, out Vector3 refrDir))
                    {
                        Ray refrRay = new Ray(hit.Point - hit.Normal * Scene.Content.ShadowRayEpsilon, refrDir, true);
                        float fresnel = FresnelComputation.ComputeFresnelDielectric(etai, etat, cosThetaI);

                        // push refraction and reflection with weighted contributions
                        Vector3 reflectedWeight = fresnel * weight;
                        Vector3 refractedWeight = (1f - fresnel) * weight;

                        rayStack.Push((reflectedRay, depth + 1, reflectedWeight));
                        rayStack.Push((refrRay, depth + 1, refractedWeight));
                    }
                    else
                    {
                        // total internal reflection
                        Vector3 tirWeight = weight * hit.material.MirrorReflectance;
                        rayStack.Push((reflectedRay, depth + 1, tirWeight));
                    }

                    finalColor += localColor;
                    break;
                }

                default:
                    finalColor += localColor;
                    break;
            }
        }

        return finalColor;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Vector3 Shade(in IntersectionInfo intersection)
    {
        return BlinnPhongShading.Shade(intersection, this);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool Refract(in Vector3 I, in Vector3 n, in float eta, out Vector3 refractedDir)
    {
        float cosi = Math.Clamp(Vector3.Dot(I, n), -1f, 1f);
        float k = 1f - eta * eta * (1f - cosi * cosi);
        if (k < 0f) // Total Internal Reflection
        {
            refractedDir = Vector3.Zero;
            return false;
        }
        refractedDir = Vector3.Normalize(eta * I - (eta * cosi + MathF.Sqrt(k)) * n);
        return true;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector3 GetAbsorption(in Vector3 absorptionCoefficient, in float distance)
    {
        return new Vector3(
            MathF.Exp(-absorptionCoefficient.X * distance),
            MathF.Exp(-absorptionCoefficient.Y * distance),
            MathF.Exp(-absorptionCoefficient.Z * distance)
        );
    }
    
    public RenderResult RenderLowRes(int factor)
    {
        Camera = Scene.GetCamera(0);
        var lowResWidth = Camera.ImageResolution.Width / factor;
        var lowResHeight = Camera.ImageResolution.Height / factor;
        var result = new RenderResult(lowResWidth, lowResHeight, Scene.Content.BackgroundColor);

        int totalRows = lowResHeight;
        int completedRows = 0;
        bool done = false;
        const int timesPerSecond = 5;
        const int interval = 1000 / timesPerSecond;

        Task progressTask = Task.Run(() =>
        {
            int lastPercent = -1;
            while (!done)
            {
                int percent = (int)(Volatile.Read(ref completedRows) * 100.0 / totalRows);
                if (percent != lastPercent)
                {
                    lastPercent = percent;
                    Console.Write($"\rProgress: {percent,3}%");
                }
                Thread.Sleep(interval); // check 4 times per second
            }
            Console.Write("\rProgress: 100%\n");
        });

        Parallel.For(0, lowResHeight, j =>
        {
            Vector3[] rowBuffer = new Vector3[lowResWidth];
            for (int i = 0; i < lowResWidth; i++)
            {
                Ray primaryRay = Camera.GetPrimaryRay(i * factor, j * factor);
                Vector3 color = Debug.UseIterativeTracing ? TraceRayIterative(primaryRay) : TraceRay(primaryRay, 0, out _);
                rowBuffer[i] = ColorUtility.Normalize(color);
            }

            for (int i = 0; i < lowResWidth; i++)
                result.SetPixel(i, j, rowBuffer[i]);

            Interlocked.Increment(ref completedRows);
        });

        done = true;
        progressTask.Wait();

        var name = Camera.ImageName.Split('.').First();
        result.OutputName = name + $"_lowres{factor}x.{Camera.ImageName.Split('.').Last()}";
        return result;
    }
}