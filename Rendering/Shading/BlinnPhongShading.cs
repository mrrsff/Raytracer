using System.Numerics;
using Raytracer.Rendering.Intersections;
using Raytracer.Sampling;

namespace Raytracer.Rendering.Shading;

public static class BlinnPhongShading
{
    public static Vector3 Shade(in IntersectionInfo intersection, in float time, in RayTracerRenderer renderer)
    {
        var point = intersection.Point;
        var normal = intersection.Normal;

        Vector3 ambient = intersection.material!.AmbientReflectance * renderer.Scene.Content.Lights.AmbientLight;
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
                diffuse += diff * intersection.material.DiffuseReflectance * irradiance;

                // Specular
                Vector3 halfDir = Vector3.Normalize(lightDir + viewDir);
                float spec = MathF.Pow(MathF.Max(Vector3.Dot(normal, halfDir), 0), intersection.material.PhongExponent);
                specular += spec * intersection.material.SpecularReflectance * irradiance;
            }
        }

        if (renderer.Scene.Content.Lights.AreaLight != null)
        {
            foreach (var light in renderer.Scene.Content.Lights.AreaLight)
            {
                Vector2 sample = Sampler.UniformRandom();
                
                Vector3 samplePosition =
                    light.Position +
                    light.Extent * (sample.X - 0.5f) * light.U +
                    light.Extent * (sample.Y - 0.5f) * light.V;
            
                if (renderer.Scene.IsOccluded(point, samplePosition, normal, time))
                    continue;
        
                var lightDelta = samplePosition - point;
                Vector3 lightDir = Vector3.Normalize(lightDelta);
            
                float nl = MathF.Max(Vector3.Dot(light.Normal, -lightDir), 0.0f);
        
                Vector3 irradiance = light.Radiance * (light.Area * nl / lightDelta.LengthSquared());
        
                // Diffuse
                float diff = MathF.Max(Vector3.Dot(normal, lightDir), 0);
                diffuse += diff * intersection.material.DiffuseReflectance * irradiance;
        
                // Specular
                Vector3 halfDir = Vector3.Normalize(lightDir + viewDir);
                float spec = MathF.Pow(MathF.Max(Vector3.Dot(normal, halfDir), 0), intersection.material.PhongExponent);
                specular += spec * intersection.material.SpecularReflectance * irradiance;
            }
        }
        

        Vector3 color = ambient + diffuse + specular;
        return color;
    }
}