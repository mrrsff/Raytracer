using System.Numerics;
using Raytracer.Core;
using Raytracer.Rendering.Intersections;
using Raytracer.Rendering.Sampling;
using Raytracer.Rendering.Shading.BRDFs;
using Raytracer.Scenes.Content.Datas.Textures;

namespace Raytracer.Rendering.Shading;

public static class BRDFShading
{
    public static Vector3 Shade(
        in IntersectionInfo intersection,
        in float time,
        in Renderer renderer)
    {
        Material material = intersection.material!;
        IBRDF brdf = material.Brdf;

        Vector3 point  = intersection.Point;
        Vector3 normal = intersection.ShadingNormal;
        Vector3 wo     = Vector3.Normalize(intersection.RayOrigin - point);

        // --- Texture handling (affects material reflectance only) ---
        Vector3 kd = material.DiffuseReflectance;
        Vector3 ks = material.SpecularReflectance;

        if (intersection.Textures != null)
        {
            foreach (var texture in intersection.Textures)
            {
                Vector3 tex = texture.Sample(intersection);
                if (Debug.RenderMipLevels)
                    return tex;

                switch (texture.DecalType)
                {
                    case DecalType.ReplaceKD:
                        kd = tex;
                        break;

                    case DecalType.BlendKD:
                        kd = 0.5f * kd + 0.5f * tex;
                        break;

                    case DecalType.ReplaceKS:
                        ks = tex;
                        break;

                    case DecalType.ReplaceAll:
                        return tex;
                }
            }
        }

        Vector3 Lo = Vector3.Zero;

        // --- Ambient (disable this in path tracing mode) ---
        Lo += material.AmbientReflectance *
              renderer.Scene.Content.Lights.AmbientLight;

        // --- Point lights ---
        if (renderer.Scene.Content.Lights.PointLight != null)
        {
            foreach (var light in renderer.Scene.Content.Lights.PointLight)
            {
                Vector3 toLight = light.Position - point;
                float dist2 = toLight.LengthSquared();
                Vector3 wi = Vector3.Normalize(toLight);

                float cosI = Vector3.Dot(normal, wi);
                if (cosI <= 0f)
                    continue;

                if (renderer.Scene.IsOccluded(point, light.Position, normal, time))
                    continue;

                Vector3 Li = light.Intensity / dist2;

                Vector3 fr = brdf.Evaluate(wi, wo, normal);

                Lo += fr * Li * cosI;
            }
        }

        // --- Area lights (single sample) ---
        if (renderer.Scene.Content.Lights.AreaLight != null)
        {
            foreach (var light in renderer.Scene.Content.Lights.AreaLight)
            {
                Vector2 xi = Sampler.UniformRandom();

                Vector3 xL =
                    light.Position +
                    light.Size * (xi.X - 0.5f) * light.U +
                    light.Size * (xi.Y - 0.5f) * light.V;

                Vector3 toLight = xL - point;
                float dist2 = toLight.LengthSquared();
                Vector3 wi = Vector3.Normalize(toLight);

                float cosI = Vector3.Dot(normal, wi);
                if (cosI <= 0f)
                    continue;

                float cosL = MathF.Abs(Vector3.Dot(light.Normal, -wi));

                if (renderer.Scene.IsOccluded(point, xL, normal, time))
                    continue;

                Vector3 Li =
                    light.Radiance *
                    (light.Area * cosL / dist2);

                Vector3 fr = brdf.Evaluate(wi, wo, normal);

                Lo += fr * Li * cosI;
            }
        }

        return Lo;
    }
}