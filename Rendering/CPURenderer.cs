using System.Numerics;
using System.Runtime.CompilerServices;
using Raytracer.Core;
using Raytracer.Rendering.Intersections;
using Raytracer.Rendering.Sampling;
using Raytracer.Rendering.Shading;
using Raytracer.Scenes;

namespace Raytracer.Rendering;

public abstract class CPURenderer(Scene scene) : Renderer(scene)
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected Ray GetReflectedRay(in IntersectionInfo intersection, in Ray incomingRay)
    {
        Vector3 reflectedDir = Vector3.Normalize(Vector3.Reflect(incomingRay.Direction, intersection.ShadingNormal));
        if (intersection.material!.Roughness > 0f)
        {
            reflectedDir = GlossyReflection.PerturbDirection(
                reflectedDir,
                intersection.material.Roughness,
                Sampler.UniformRandom());
        }

        return new Ray(
            intersection.Point + intersection.ShadingNormal * Scene.Content.ShadowRayEpsilon,
            reflectedDir,
            false,
            incomingRay.Time);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected Vector3 Shade(in IntersectionInfo intersection, float time)
    {
        if (intersection.material == null) return Vector3.Zero;
        
        return intersection.material.Brdf == null 
            ? BlinnPhongShading.Shade(intersection, time, this) 
            : BRDFShading.Shade(intersection, time, this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Refract(in Vector3 I, in Vector3 n, float eta, out Vector3 refractedDir)
    {
        float cosi = Math.Clamp(Vector3.Dot(I, n), -1f, 1f);
        float k  = 1f - eta * eta * (1f - cosi * cosi);
        if (k < 0f) // Total Internal Reflection
        {
            refractedDir = Vector3.Zero;
            return false;
        }

        refractedDir = Vector3.Normalize(eta * I - (eta * cosi + MathF.Sqrt(k)) * n);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static Vector3 GetAbsorption(in Vector3 absorptionCoefficient, float distance)
    {
        return new Vector3(
            MathF.Exp(-absorptionCoefficient.X * distance),
            MathF.Exp(-absorptionCoefficient.Y * distance),
            MathF.Exp(-absorptionCoefficient.Z * distance)
        );
    }
}