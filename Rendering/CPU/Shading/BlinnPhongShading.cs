using System.Numerics;
using System.Runtime.CompilerServices;
using Raytracer.Core;
using Raytracer.Rendering.CPU.Intersections;
using Raytracer.Scenes.Content.Datas.Textures;

namespace Raytracer.Rendering.CPU.Shading;

public static class BlinnPhongShading
{
    public static Vector3 Shade(in IntersectionInfo intersection, in float time, in Renderer renderer)
    {
        var kd = intersection.material!.DiffuseReflectance;
        var ks = intersection.material.SpecularReflectance;

        if (intersection.Textures != null)
        {
            foreach (var texture in intersection.Textures)
            {
                var textureColor = texture.Sample(intersection); // [0,1]
                if (Debug.RenderMipLevels) return textureColor;
                switch (texture.DecalType)
                {
                    case DecalType.ReplaceKD: kd = textureColor; break;
                    case DecalType.BlendKD: kd = textureColor * .5f + kd * .5f; break; 
                    case DecalType.ReplaceKS: ks = textureColor; break;
                    case DecalType.ReplaceAll: return textureColor;
                }
            }
        }
        
        Vector3 P = intersection.Point;
        Vector3 N = intersection.ShadingNormal;
        Vector3 V = Vector3.Normalize(intersection.RayOrigin - P);

        Vector3 ambient = intersection.material.AmbientReflectance * renderer.Scene.Content.Lights.AmbientLight;
        Vector3 diffuse = Vector3.Zero;
        Vector3 specular = Vector3.Zero;
        
        if (renderer.Scene.Content.Lights.AllLights.Count == 0)
            return ambient;
        
        foreach (var light in renderer.Scene.Content.Lights.AllLights)
        {
            if (light.Sample(P, N, time, renderer, out Vector3 L, out Vector3 irradiance))
            {
                EvalBlinnPhong(N, V, L, kd, ks, intersection.material.PhongExponent, irradiance, ref diffuse, ref specular);
            }
        }
        Vector3 color = ambient + diffuse + specular;
        return color;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void EvalBlinnPhong(in Vector3 N, in Vector3 V, in Vector3 L, in Vector3 kd, in Vector3 ks, float phongExp, in Vector3 irradiance, ref Vector3 diffuse, ref Vector3 specular)
    {
        float ndotl = MathF.Max(Vector3.Dot(N, L), 0f); // if ndotl <= 0, light is below the surface
        ndotl = Math.Abs(ndotl);

        diffuse += kd * irradiance * ndotl;

        Vector3 H = Vector3.Normalize(L + V);
        float spec = MathF.Pow(MathF.Max(Vector3.Dot(N, H), 0f), phongExp);
        specular += ks * irradiance * spec;
    }
}