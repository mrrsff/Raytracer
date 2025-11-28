using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using Raytracer.Core;
using Raytracer.IO.ImageSavers;
using Raytracer.Rendering.Filtering;
using Raytracer.Rendering.Intersections;
using Raytracer.Rendering.Sampling;
using Raytracer.Rendering.Shading;
using Raytracer.Scenes;
using Raytracer.Scenes.Content.Datas.Camera;
using Raytracer.Utility;

namespace Raytracer.Rendering;

public class RayTracerRenderer : CPURenderer
{
    public static float IntersectionTestEpsilon;
    public static float ShadowRayEpsilon;

    public RayTracerRenderer(Scene scene) : base(scene)
    {
        IntersectionTestEpsilon = scene.Content.IntersectionTestEpsilon;
        ShadowRayEpsilon = scene.Content.ShadowRayEpsilon;
    }

    protected override void OnRender(ImageBuffer buffer)
    {
        if (Core.Debug.UseDynamicThreading)
            DynamicThreadPoolRender(Camera, buffer);
        else if (Core.Debug.UseMultiThreading)
            MultithreadRender(Camera, buffer);
        else
            SingleThreadRender(Camera, buffer);
    }

    private void ProgressiveRenderPixel(int x, int y, Camera renderCamera, ImageBuffer buffer)
    {
        Func<int, Vector2[]> sampler = Sampler.MultiJittered.Sample;
        Func<int, float[]> timeSampler = Sampler.OneDimensionalUniform;
        Func<float, float, float> filter = Filter.Gaussian.Evaluate;
        
        Vector2[] pixelSamples = sampler(Camera.NumSamples);
        Vector2[] lensSamples = sampler(Camera.NumSamples);
        float[] timeSamples = timeSampler(Camera.NumSamples);
        
        float totalWeight = 0f;
        Vector3 finalColor = Vector3.Zero;
        for (int s = 0; s < Camera.NumSamples; s++)
        {
            float px = x + pixelSamples[s].X;
            float py = y + pixelSamples[s].Y;
            Vector2 lens = lensSamples[s];
            float time = timeSamples[s];
    
            Ray ray = renderCamera.GenerateRayDRT(px, py, lens, time);
            Vector3 sampleColor = TraceRayIterative(ray);
            float weight = filter(pixelSamples[s].X, pixelSamples[s].Y);
    
            finalColor += sampleColor * weight;
            totalWeight += weight;
        }
        
        finalColor /= totalWeight;
        buffer.SetPixel(x, y, ColorUtility.Normalize(finalColor));
    }

    #region Rendering
    private void DynamicThreadPoolRender(Camera camera, ImageBuffer result)
    {
        int width  = result.Width;
        int height = result.Height;

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

        for (int i = 0; i < workerCount; i++)
            tasks[i] = Task.Run(Worker);

        Task.WaitAll(tasks);
        return;

        void Worker()
        {
            while (true)
            {
                int myIndex = Interlocked.Increment(ref nextTile);
                if (myIndex >= tileCount)
                    break;

                var (tileX, tileY) = tiles[myIndex];

                int endX = Math.Min(tileX + tileSize, width);
                int endY = Math.Min(tileY + tileSize, height);

                for (int y = tileY; y < endY; y++)
                for (int x = tileX; x < endX; x++)
                {
                    ProgressiveRenderPixel(x, y, camera, result);
                }
            }
        }
    }
    
    private void SingleThreadRender(Camera RenderCamera, ImageBuffer result)
    {
        int width = result.Width;
        int height = result.Height;

        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                ProgressiveRenderPixel(i, j, RenderCamera, result);
            }
        }
    }

    private void MultithreadRender(Camera RenderCamera, ImageBuffer result)
    {
        int width = result.Width;
        int height = result.Height;
        Parallel.For(0, height, j =>
        {
            for (int i = 0; i < width; i++)
            {
                ProgressiveRenderPixel(i, j, RenderCamera, result);
            }
        });
    }

    #endregion

    #region Ray Tracing
    private Vector3 TraceRay(in Ray ray, in int depth, out float distanceTraveled)
    {
        distanceTraveled = 0;
        if (depth > Scene.Content.MaxRecursionDepth)
            return ColorUtility.Black;

        IntersectionInfo hit = Scene.Intersect(ray);

        if (!hit.Hit)
            return Scene.Content.BackgroundColor;

        distanceTraveled = hit.Distance;

        Vector3 finalColor = Vector3.Zero;

        bool inFront = Vector3.Dot(ray.Direction, hit.Normal) < 0f; // Ray is entering the material (facing normal)
        if (hit.material?.Type is MaterialType.Dielectric or MaterialType.Conductor && !inFront)
        {
            hit.Normal = -hit.Normal;
        }
        else finalColor = Shade(hit, ray.Time);

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
                float eta = etai / etat;

                if (Refract(ray.Direction, hit.Normal, eta, out Vector3 refrDir))
                {
                    Ray refractedRay = new Ray(hit.Point - hit.Normal * Scene.Content.ShadowRayEpsilon, refrDir, true);
                    Vector3 refractedColor = TraceRay(refractedRay, depth + 1, out float insideDistance);
                    if (inFront) // apply absorption only when the ray is entering the material
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

    private struct RayState
    {
        public Ray Ray;
        public int Depth;
        public Vector3 Weight;
        public float DistanceTraveled;
        public bool InsideObject;
    }

    private Vector3 TraceRayIterative(in Ray initialRay)
    {
        Vector3 finalColor = Vector3.Zero;

        Span<RayState> stack = stackalloc RayState[Scene.Content.MaxRecursionDepth * 2];
        int stackPointer = 0;
        stack[stackPointer++] = new RayState
            { Ray = initialRay, Depth = 0, Weight = Vector3.One, DistanceTraveled = 0f, InsideObject = false };

        while (stackPointer > 0)
        {
            RayState currentState = stack[--stackPointer];
            Ray ray = currentState.Ray;
            int depth = currentState.Depth;
            Vector3 weight = currentState.Weight;
            bool InsideObject = currentState.InsideObject;

            if (depth > Scene.Content.MaxRecursionDepth)
                continue;

            IntersectionInfo hit = Scene.Intersect(ray);

            if (!hit.Hit)
            {
                finalColor += weight * Scene.Content.BackgroundColor;
                continue;
            }

            float distanceTraveled = 0;
            float cosThetaI = 0;
            if (hit.material!.Type is MaterialType.Dielectric or MaterialType.Conductor)
            {
                distanceTraveled = hit.Distance + currentState.DistanceTraveled;
                cosThetaI = MathF.Abs(Vector3.Dot(-ray.Direction, hit.Normal));
            }

            if (InsideObject) hit.Normal = -hit.Normal;

            AddToFinalColor(Shade(hit, ray.Time));

            // Vector3 reflectedDir = Vector3.Normalize(Vector3.Reflect(ray.Direction, hit.Normal));
            // Ray reflectedRay = new Ray(hit.Point + hit.Normal * Scene.Content.ShadowRayEpsilon, reflectedDir, true, ray.Time);
            
            Ray reflectedRay = GetReflectedRay(hit, ray);
            switch (hit.material!.Type)
            {
                case MaterialType.Mirror:
                {
                    stack[stackPointer++] = new RayState
                    {
                        Ray = reflectedRay,
                        Depth = depth + 1,
                        Weight = weight * hit.material.MirrorReflectance
                    };
                    break;
                }
                case MaterialType.Conductor:
                {
                    var fresnel = FresnelComputation.ComputeFresnelConductor(hit.material, cosThetaI);
                    stack[stackPointer++] = new RayState
                    {
                        Ray = reflectedRay,
                        Depth = depth + 1,
                        Weight = weight * fresnel * hit.material.MirrorReflectance
                    };
                    break;
                }
                case MaterialType.Dielectric:
                {
                    const float airRefractionIndex = 1f;
                    float etai = InsideObject ? hit.material.RefractionIndex : airRefractionIndex;
                    float etat = InsideObject ? airRefractionIndex : hit.material.RefractionIndex;
                    float eta = etai / etat;

                    if (Refract(ray.Direction, hit.Normal, eta, out Vector3 refrDir))
                    {
                        float fresnel = FresnelComputation.ComputeFresnelDielectric(etai, etat, cosThetaI);

                        Ray refractedRay = new Ray(hit.Point - hit.Normal * Scene.Content.ShadowRayEpsilon, refrDir, false, ray.Time);

                        Vector3 absorption = InsideObject
                            ? GetAbsorption(hit.material!.AbsorptionCoefficient, hit.Distance)
                            : Vector3.One;

                        stack[stackPointer++] = new RayState // refracted ray
                        {
                            Ray = refractedRay,
                            Depth = depth + 1,
                            Weight = weight * (1f - fresnel) * absorption,
                            InsideObject = !InsideObject,
                            DistanceTraveled = distanceTraveled
                        };

                        stack[stackPointer++] = new RayState // reflected ray
                        {
                            Ray = reflectedRay,
                            Depth = depth + 1,
                            Weight = weight * fresnel,
                            InsideObject = InsideObject,
                            DistanceTraveled = distanceTraveled
                        };
                    }
                    else // total internal reflection
                    {
                        Vector3 absorption = InsideObject
                            ? GetAbsorption(hit.material!.AbsorptionCoefficient, hit.Distance)
                            : Vector3.One;

                        stack[stackPointer++] = new RayState
                        {
                            Ray = reflectedRay,
                            Depth = depth + 1,
                            Weight = weight * absorption,
                            InsideObject = InsideObject,
                            DistanceTraveled = distanceTraveled
                        };
                    }

                    break;
                }
            }

            continue;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void AddToFinalColor(Vector3 color)
            {
                var colorToAdd = color * weight;
                if (InsideObject)
                {
                    colorToAdd *= GetAbsorption(hit.material!.AbsorptionCoefficient, distanceTraveled);
                }

                finalColor += colorToAdd;
            }
        }

        return finalColor;
    }

    #endregion
}