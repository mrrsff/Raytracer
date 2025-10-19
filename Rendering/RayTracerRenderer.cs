using System.Diagnostics;
using System.Numerics;
using Raytracer.Core;
using Raytracer.IO.ImageSavers;
using Raytracer.Rendering.Intersections;
using Raytracer.Rendering.Shading;
using Raytracer.Scenes;
using Raytracer.Scenes.Content.Datas.Camera;
using Raytracer.Utility;

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
        IntersectionTestEpsilon = scene.Content.IntersectionTestEpsilon == 0 ? 1e-6f : scene.Content.IntersectionTestEpsilon;
        ShadowRayEpsilon = scene.Content.ShadowRayEpsilon == 0 ? 1e-3f : scene.Content.ShadowRayEpsilon;
    }
    
    public RenderResult Render(int cameraIndex)
    {
        Camera = Scene.GetCamera(cameraIndex);
        Camera.InitializeCamera();

        RenderResult result = new RenderResult(Camera.ImageResolution, Scene.Content.BackgroundColor);
        int width = result.Width;
        int height = result.Height;

        Stopwatch sw = Stopwatch.StartNew();
        Console.WriteLine($"Rendering started for {Camera.ImageName}... TIME: {DateTime.Now:HH:mm:ss}");

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
                Ray primaryRay = Camera.GetPrimaryRay(i, j);
                Vector3 color = TraceRay(primaryRay, 0, out _);
                rowBuffer[i] = ColorUtility.Clamp(color);
            }

            for (int i = 0; i < width; i++)
                result.SetPixel(i, j, rowBuffer[i]);

            Interlocked.Increment(ref completedRows);
        });

        done = true;
        progressTask.Wait();

        sw.Stop();
        Console.WriteLine($"Rendering finished for {Camera.ImageName} in {sw.Elapsed.TotalSeconds:F2} seconds. TIME: {DateTime.Now:HH:mm:ss}");
        
        result.OutputName = Camera.ImageName;
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

        bool entering = Vector3.Dot(ray.Direction, hit.Normal) < 0f;
        if (!entering)
        {
            hit.Normal = -hit.Normal;
        }

        Vector3 finalColor = entering ? Shade(hit) : ColorUtility.Green;

        
        float cosThetaI = MathF.Abs(Vector3.Dot(-ray.Direction, hit.Normal));
        
        Vector3 reflectedDir = Vector3.Normalize(Vector3.Reflect(ray.Direction, hit.Normal));
        Ray reflectedRay = new Ray(hit.Point + hit.Normal * Scene.Content.ShadowRayEpsilon, reflectedDir, true);
        Vector3 reflectedColor = TraceRay(reflectedRay, depth + 1, out _);
        
        if (hit.material.Type == MaterialType.Mirror)
        {
            finalColor += hit.material.MirrorReflectance * reflectedColor;
        }
        else if (hit.material.Type == MaterialType.Conductor)
        {
            var fresnel = FresnelComputation.ComputeFresnelConductor(hit.material, cosThetaI);
            finalColor += fresnel * hit.material.MirrorReflectance * reflectedColor;
        }
        else if (hit.material.Type == MaterialType.Dielectric)
        {
            const float airRefractionIndex = 1.00029f;
            float etai = entering ? airRefractionIndex : hit.material.RefractionIndex;
            float etat = entering ? hit.material.RefractionIndex : airRefractionIndex;
            float eta  = etai / etat;
    
            // Try to compute refraction
            if (Refract(ray.Direction, hit.Normal, eta, out Vector3 refrDir))
            {
                Ray refractedRay = new Ray(hit.Point - hit.Normal * Scene.Content.ShadowRayEpsilon, refrDir, true);
                Vector3 refractedColor = TraceRay(refractedRay, depth + 1, out float insideDistance);
        
                if (entering && insideDistance > 0)
                {
                    refractedColor *= GetAbsorption(hit.material.AbsorptionCoefficient, insideDistance);
                }
        
                float fresnel = FresnelComputation.ComputeFresnelDielectric(ray.Direction, hit.Normal, etai, etat);
        
                finalColor += fresnel * hit.material.MirrorReflectance * reflectedColor + (1f - fresnel) * refractedColor;
            }
            else
            {
                Vector3 tirColor = TraceRay(reflectedRay, depth + 1, out var traveled);

                traveled += distanceTraveled;
                Vector3 absorption = GetAbsorption(hit.material.AbsorptionCoefficient, traveled);
        
                finalColor += hit.material.MirrorReflectance * tirColor * absorption;
            }
        }

        return finalColor;
    }
    
    private Vector3 Shade(in IntersectionInfo intersection)
    {
        return BlinnPhongShading.Shade(intersection, this);
    }
    
    private static bool Refract(in Vector3 I, in Vector3 n, in float eta, out Vector3 refractedRay)
    {
        float cosi = Math.Clamp(Vector3.Dot(I, n), -1f, 1f);
        float k = 1f - eta * eta * (1f - cosi * cosi);
        if (k < 0f) // Total Internal Reflection
        {
            refractedRay = Vector3.Zero;
            return false;
        }
        refractedRay = Vector3.Normalize(eta * I - (eta * cosi + MathF.Sqrt(k)) * n);
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
}