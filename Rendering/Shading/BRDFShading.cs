using System.Numerics;
using System.Runtime.CompilerServices;
using Raytracer.Core;
using Raytracer.Rendering.Intersections;
using Raytracer.Rendering.Shading.BRDFs;
using Raytracer.Scenes.Content.Datas.Textures;

namespace Raytracer.Rendering.Shading;

public static class BRDFShading
{
    public static Vector3 Shade(in IntersectionInfo intersection, in float time, in Renderer renderer)
    {
        Material material = intersection.material!;
        IBRDF brdf = material.Brdf;

        Vector3 kd = material.DiffuseReflectance;
        Vector3 ks = material.SpecularReflectance;

        if (intersection.Textures != null)
        {
            foreach (var texture in intersection.Textures)
            {
                Vector3 tex = texture.Sample(intersection);

                switch (texture.DecalType)
                {
                    case DecalType.ReplaceKD: kd = tex; break;
                    case DecalType.BlendKD: kd = 0.5f * kd + 0.5f * tex; break;
                    case DecalType.ReplaceKS: ks = tex; break;
                    case DecalType.ReplaceAll: return tex;
                }
            }
        }

        Vector3 point = intersection.Point;
        Vector3 normal = intersection.ShadingNormal;
        Vector3 wo = Vector3.Normalize(intersection.RayOrigin - point);

        Vector3 ambient = material.AmbientReflectance * renderer.Scene.Content.Lights.AmbientLight;
        Vector3 Lo = ambient;
        
        foreach (var light in renderer.Scene.Lights)
        {
            if (light.Sample(point, normal, time, renderer, out Vector3 wi, out Vector3 irradiance))
            {
                EvalBRDF(brdf, kd, ks, wi, wo, normal, irradiance, ref Lo);
            }
        }

        return Lo;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void EvalBRDF(IBRDF brdf, in Vector3 kd, in Vector3 ks, in Vector3 wi, in Vector3 wo, 
        in Vector3 normal, in Vector3 irradiance, ref Vector3 Lo)
    {
        float cosI = Vector3.Dot(normal, wi);
        if (cosI <= 0f)
            return;

        Vector3 fr = brdf.Evaluate(kd, ks, wi, wo, normal);
        Lo += fr * irradiance * cosI;
    }
}