using System.Numerics;
using Raytracer.Core;
using Raytracer.IO.Images;
using Raytracer.Rendering.Filtering;
using Raytracer.Rendering.Intersections;
using Raytracer.Rendering.Sampling;
using Raytracer.Rendering.Shading;
using Raytracer.Scenes;
using Raytracer.Scenes.Content.Datas.Camera;
using Raytracer.Utility;
using Debug = Raytracer.Core.Debug;

namespace Raytracer.Rendering.Raytracing;

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
        DynamicThreadPoolRender(Camera, buffer);
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
            // Ray ray = renderCamera.GenerateRay(x, y);
            
            Vector3 sampleColor = TraceRayIterative(x, y, ray);
            // Vector3 sampleColor = TraceRay(x, y, ray, 0, out _);
            float weight = Filter.Gaussian.Evaluate(pixelSamples[s].X, pixelSamples[s].Y);
    
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

        try
        {
            Task.WaitAll(tasks);
        }
        catch (AggregateException ae)
        {
            foreach (var ex in ae.Flatten().InnerExceptions)
            {
                Debug.Log(ex.ToString());
            }
            throw; // rethrow if you want the render to fail hard
        }
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
                        try
                        {
                            ProgressiveRenderPixel(x, y, camera, result);
                        }
                        catch (Exception ex)
                        {
                            Debug.Log($"Exception at pixel ({x},{y}) in tile ({tileX},{tileY})\n{ex}");
                            throw; // rethrow so Task becomes faulted
                        }
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
    // private Vector3 TraceRay(int x, int y, in Ray ray, in int depth, out float distanceTraveled)
    // {
    //     distanceTraveled = 0;
    //     if (depth > Scene.Content.MaxRecursionDepth)
    //         return ColorUtility.Black;
    //
    //     IntersectionInfo hit = Scene.Intersect(ray);
    //
    //     if (!hit.Hit)
    //         return Scene.Content.BackgroundColor;
    //
    //     distanceTraveled = hit.Distance;
    //
    //     Vector3 finalColor = Vector3.Zero;
    //
    //     bool inFront = Vector3.Dot(ray.Direction, hit.Normal) < 0f; // Ray is entering the material (facing normal)
    //     if (hit.material?.Type is MaterialType.Dielectric or MaterialType.Conductor && !inFront)
    //     {
    //         hit.Normal = -hit.Normal;
    //     }
    //     else finalColor = Shade(hit, ray.Time);
    //
    //     float cosThetaI = MathF.Abs(Vector3.Dot(-ray.Direction, hit.Normal));
    //
    //     Vector3 reflectedColor = ColorUtility.Black;
    //     Ray reflectedRay = GetReflectedRay(hit, ray);
    //     if (inFront)
    //     {
    //         reflectedColor = TraceRay(x, y, reflectedRay, depth + 1, out _);
    //     }
    //
    //     switch (hit.material?.Type)
    //     {
    //         case MaterialType.Mirror:
    //             finalColor += hit.material.MirrorReflectance * reflectedColor;
    //             break;
    //         case MaterialType.Conductor:
    //         {
    //             var fresnel = FresnelComputation.ComputeFresnelConductor(hit.material, cosThetaI);
    //             finalColor += fresnel * hit.material.MirrorReflectance * reflectedColor;
    //             break;
    //         }
    //         case MaterialType.Dielectric:
    //         {
    //             const float airRefractionIndex = 1f;
    //             float etai = inFront ? airRefractionIndex : hit.material.RefractionIndex;
    //             float etat = inFront ? hit.material.RefractionIndex : airRefractionIndex;
    //             float eta = etai / etat;
    //
    //             if (Refract(ray.Direction, hit.Normal, eta, out Vector3 refrDir))
    //             {
    //                 if (hit.material.Roughness > 0f)
    //                 {
    //                     refrDir = GlossyReflection.PerturbDirection(refrDir, hit.material.Roughness, Sampler.UniformRandom());
    //                 }
    //
    //                 Ray refractedRay = new Ray(hit.Point - hit.Normal * Scene.Content.ShadowRayEpsilon, refrDir, true);
    //                 Vector3 refractedColor = TraceRay(x, y, refractedRay, depth + 1, out float insideDistance);
    //                 if (inFront) // apply absorption only when the ray is entering the material
    //                 {
    //                     refractedColor *= GetAbsorption(hit.material.AbsorptionCoefficient, insideDistance);
    //                 }
    //
    //                 float fresnel = FresnelComputation.ComputeFresnelDielectric(etai, etat, cosThetaI);
    //
    //                 finalColor += fresnel * reflectedColor + (1f - fresnel) * refractedColor;
    //             }
    //             else // total internal reflection
    //             {
    //                 Vector3 tirColor = TraceRay(x, y, reflectedRay, depth + 1, out float traveled);
    //                 distanceTraveled += traveled;
    //                 
    //                 Vector3 absorption = GetAbsorption(hit.material.AbsorptionCoefficient, traveled);
    //                 tirColor *= absorption;
    //                 finalColor += tirColor;
    //             }
    //
    //             break;
    //         }
    //     }
    //
    //     return finalColor;
    // }

    private struct RayState
    {
        public Ray Ray;
        public int Depth;
        public Vector3 Weight;
        public float DistanceTraveled;
        public bool IsInside;
    }

    private Vector3 TraceRayIterative(int x, int y, in Ray initialRay)
    {
        Vector3 finalColor = Vector3.Zero;

        Span<RayState> stack = stackalloc RayState[Scene.Content.MaxRecursionDepth * 2];
        int stackPointer = 0;
        stack[stackPointer++] = new RayState
            { Ray = initialRay, Depth = 0, Weight = Vector3.One, DistanceTraveled = 0f};
        RayStats.IncrementPrimary();
        while (stackPointer > 0)
        {
            RayState currentState = stack[--stackPointer];
            Ray ray = currentState.Ray;
            int depth = currentState.Depth;
            Vector3 weight = currentState.Weight;
            bool IsInside = currentState.IsInside;

            if (depth > Scene.Content.MaxRecursionDepth)
                continue;

            IntersectionInfo hit = Scene.Intersect(ray);
            if (!hit.Hit)
            {
                finalColor += weight * Scene.GetBackgroundColor(x, y, Camera);
                continue;
            }

            if (Debug.RenderUVs)
            {
                var uv = hit.GetUVCoordinates(true);
                var color = new Vector3(uv.X, uv.Y, 0f) * 255f;
                return color;
            }
            if (Debug.RenderNormals)
            {
                var color = (hit.ShadingNormal + Vector3.One) * 0.5f * 255f;
                // Debug.Log("Rendering normals : " + hit.Normal + " -> " + color);
                return color;
            }
            hit.Camera = Camera;
            hit.XPixel = x;
            hit.YPixel = y;
            
            float currentDistanceTraveled = currentState.DistanceTraveled + hit.Distance;
            float cosThetaI = 0;
            
            if (hit.material!.Type == MaterialType.Dielectric || hit.material.Type == MaterialType.Conductor)
            {
                cosThetaI = Vector3.Dot(-ray.Direction, hit.ShadingNormal);
                if (IsInside)
                {
                    hit.ShadingNormal = -hit.ShadingNormal;
                    weight *= GetAbsorption(hit.material!.AbsorptionCoefficient, currentDistanceTraveled);
                }
            }

            if (Debug.RenderMipLevels)
            {
                return Shade(hit, ray.Time) * 255f;
            }

            finalColor += Shade(hit, ray.Time) * weight;
            
            Ray reflectedRay = GetReflectedRay(hit, ray);
            switch (hit.material.Type)
            {
                case MaterialType.Mirror:
                {
                    RayStats.IncrementPrimary();
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
                    RayStats.IncrementPrimary();
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
                    float etai = IsInside ? hit.material.RefractionIndex : airRefractionIndex;
                    float etat = IsInside ? airRefractionIndex : hit.material.RefractionIndex;
                    float eta = etai / etat;
                    
                    if (Refract(ray.Direction, hit.ShadingNormal, eta, out Vector3 refrDir))
                    {
                        if (hit.material.Roughness > 0f)
                        {
                            refrDir = GlossyReflection.PerturbDirection(refrDir, hit.material.Roughness, Sampler.UniformRandom());
                        }
                        
                        Ray refractedRay = new Ray(hit.Point - hit.ShadingNormal * Scene.Content.ShadowRayEpsilon, refrDir, true);
                        
                        float fresnel = FresnelComputation.ComputeFresnelDielectric(etai, etat, cosThetaI);
                        
                        RayStats.IncrementPrimary();
                        stack[stackPointer++] = new RayState // refracted ray
                        {
                            Ray = refractedRay,
                            Depth = depth + 1,
                            Weight = weight * (1f - fresnel),
                            DistanceTraveled = 0f,
                            IsInside = !IsInside
                        };
                        
                        weight *= fresnel;
                    }
                    
                    RayStats.IncrementPrimary();
                    stack[stackPointer++] = new RayState // reflected ray
                    {
                        Ray = reflectedRay,
                        Depth = depth + 1,
                        Weight = weight,
                        DistanceTraveled = IsInside ? currentDistanceTraveled : 0f,
                        IsInside = IsInside
                    };
                    break;
                }
            }
        }

        return finalColor;
    }

    #endregion
}