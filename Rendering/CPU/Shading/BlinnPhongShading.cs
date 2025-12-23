using System.Numerics;
using Raytracer.Core;
using Raytracer.Rendering.CPU.Intersections;
using Raytracer.Rendering.CPU.Sampling;
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
                    case DecalType.ReplaceKD:
                        kd = textureColor;
                        break;
                    case DecalType.BlendKD:
                        kd = textureColor * .5f + kd * .5f;
                        break;
                    case DecalType.ReplaceKS:
                        ks = textureColor;
                        break;
                    case DecalType.ReplaceAll:
                        return textureColor;
                }
            }
        }

        var point = intersection.Point;
        var normal = intersection.ShadingNormal;

        Vector3 ambient = intersection.material.AmbientReflectance * renderer.Scene.Content.Lights.AmbientLight;
        Vector3 diffuse = Vector3.Zero;
        Vector3 specular = Vector3.Zero;
        Vector3 viewDir = Vector3.Normalize(intersection.RayOrigin - point);

        if (renderer.Scene.Content.Lights.PointLight != null)
        {
            foreach (var light in renderer.Scene.Content.Lights.PointLight)
            {
                if (renderer.Scene.IsOccluded(point, light.Position, normal, time))
                    continue;

                var lightDelta = light.Position - point;
                Vector3 lightDir = Vector3.Normalize(lightDelta);
                Vector3 irradiance = light.Intensity / (lightDelta.LengthSquared());

                // Diffuse
                float diff = MathF.Max(Vector3.Dot(normal, lightDir), 0);
                diffuse += diff * kd * irradiance;

                // Specular
                Vector3 halfDir = Vector3.Normalize(lightDir + viewDir);
                float spec = MathF.Pow(MathF.Max(Vector3.Dot(normal, halfDir), 0), intersection.material.PhongExponent);
                specular += spec * ks * irradiance;
            }
        }

        if (renderer.Scene.Content.Lights.AreaLight != null)
        {
            foreach (var light in renderer.Scene.Content.Lights.AreaLight)
            {
                Vector2 sample = Sampler.UniformRandom();
                
                Vector3 samplePosition =
                    light.Position +
                    light.Size * (sample.X - 0.5f) * light.U +
                    light.Size * (sample.Y - 0.5f) * light.V;

                if (renderer.Scene.IsOccluded(point, samplePosition, normal, time))
                {
                    continue;
                }
        
                var lightDelta = samplePosition - point;
                Vector3 lightDir = Vector3.Normalize(lightDelta);

                float cosLight = Vector3.Dot(light.Normal, -lightDir);
                cosLight = MathF.Abs(cosLight);
                Vector3 irradiance = light.Radiance * (light.Area * cosLight / lightDelta.LengthSquared());
        
                // Diffuse
                float diff = MathF.Max(Vector3.Dot(normal, lightDir), 0);
                diffuse += diff * kd * irradiance;
        
                // Specular
                Vector3 halfDir = Vector3.Normalize(lightDir + viewDir);
                float spec = MathF.Pow(MathF.Max(Vector3.Dot(normal, halfDir), 0), intersection.material.PhongExponent);
                specular += spec * ks * irradiance;
            }
        }
        Vector3 color = ambient + diffuse + specular;
        return color;
    }
}