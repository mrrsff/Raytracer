using System.Diagnostics;
using System.Numerics;
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
        result = Debug.UseMultiThreading ? MultithreadRender(Camera, result) : SingleThreadRender(Camera, result);
        
        sw.Stop();
        Console.WriteLine($"\nRendering finished for {Camera.ImageName} in {sw.Elapsed.TotalSeconds:F2} seconds. TIME: {DateTime.Now:HH:mm:ss}");
        
        result.OutputName = Camera.ImageName;
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
                Vector3 color = TraceRay(primaryRay, 0, out _);
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
        
        Vector3 finalColor = Vector3.Zero;
        
        bool entering = Vector3.Dot(ray.Direction, hit.Normal) < 0f;
        if (hit.material?.Type is MaterialType.Dielectric or MaterialType.Conductor)
        {
            if (!entering) hit.Normal = -hit.Normal;
            finalColor = entering ? Shade(hit) : ColorUtility.Magenta;
        }
        else finalColor = Shade(hit);
        
        float cosThetaI = MathF.Abs(Vector3.Dot(-ray.Direction, hit.Normal));
        
        Vector3 reflectedDir = Vector3.Normalize(Vector3.Reflect(ray.Direction, hit.Normal));
        Ray reflectedRay = new Ray(hit.Point + hit.Normal * Scene.Content.ShadowRayEpsilon, reflectedDir, true);
        Vector3 reflectedColor = TraceRay(reflectedRay, depth + 1, out _);
        
        switch (hit.material.Type)
        {
            case MaterialType.Mirror:
                finalColor = Shade(hit);
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
                float etai = entering ? airRefractionIndex : hit.material.RefractionIndex;
                float etat = entering ? hit.material.RefractionIndex : airRefractionIndex;
                float eta  = etai / etat;
    
                if (Refract(ray.Direction, hit.Normal, eta, out Vector3 refrDir))
                {
                    Ray refractedRay = new Ray(hit.Point - hit.Normal * Scene.Content.ShadowRayEpsilon, refrDir, true);
                    Vector3 refractedColor = TraceRay(refractedRay, depth + 1, out float insideDistance);
        
                    if (entering && insideDistance > 0)
                    {
                        refractedColor *= GetAbsorption(hit.material.AbsorptionCoefficient, insideDistance);
                    }

                    float fresnel = FresnelComputation.ComputeFresnelDielectric(ray.Direction, hit.Normal, etai, etat, cosThetaI);
        
                    finalColor += fresnel * hit.material.MirrorReflectance * reflectedColor + (1f - fresnel) * refractedColor;
                }
                else
                {
                    Vector3 tirColor = TraceRay(reflectedRay, depth + 1, out var traveled);

                    traveled += distanceTraveled;
                    Vector3 absorption = GetAbsorption(hit.material.AbsorptionCoefficient, traveled);
        
                    finalColor += hit.material.MirrorReflectance * tirColor * absorption;
                }

                break;
            }
            default:
                finalColor = Shade(hit);
                break;
        }

        return finalColor;
    }
    
    private Vector3 Shade(in IntersectionInfo intersection)
    {
        return BlinnPhongShading.Shade(intersection, this);
    }
    
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
    
    private static Vector3 GetAbsorption(in Vector3 absorptionCoefficient, in float distance)
    {
        return new Vector3(
            MathF.Exp(-absorptionCoefficient.X * distance),
            MathF.Exp(-absorptionCoefficient.Y * distance),
            MathF.Exp(-absorptionCoefficient.Z * distance)
        );
    }

    public RenderResult RenderPartition(int minX, int minY, int maxX, int maxY)
    {
        Camera = Scene.GetCamera(0);
        var result = new RenderResult(maxX - minX, maxY - minY, Scene.Content.BackgroundColor);

        for (int i = minX; i < maxX; i++)
        {
            for (int j = minY; j < maxY; j++)
            {
                Ray primaryRay = Camera.GetPrimaryRay(i, j);
                Vector3 color = TraceRay(primaryRay, 0, out _);
                color = ColorUtility.Normalize(color);
                result.SetPixel(i - minX, j - minY, color);
            }
        }

        result.OutputName = Camera.ImageName;
        return result;
    }
}