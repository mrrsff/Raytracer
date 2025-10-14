using System.Numerics;
using Raytracer.Core;
using Raytracer.Rendering.Intersections;
using Raytracer.Scenes.Content.Datas.Camera;
using Raytracer.Utility;

namespace Raytracer.Rendering.Shading;

public static class BlinnPhongShading
{
    public static Vector3 Shade(IntersectionInfo intersection, RayTracerRenderer renderer)
    {
        Vector3 ambient = intersection.material.AmbientReflectance * renderer.Scene.Content.Lights.AmbientLight;
        Vector3 diffuse = Vector3.Zero;
        Vector3 specular = Vector3.Zero;
        Vector3 viewDir = Vector3.Normalize(intersection.HitRay.Origin - intersection.Point);

        foreach (var light in renderer.Scene.Content.Lights.PointLight)
        {
            var lightDelta = light.Position - intersection.Point;
            Vector3 lightDir = Vector3.Normalize(lightDelta);
            
            Ray shadowRay = new Ray(intersection.Point + intersection.Normal * RayTracerRenderer.ShadowRayEpsilon, lightDir);
            shadowRay.IsShadowRay = true;
            IntersectionInfo shadowHit = renderer.Scene.Intersect(shadowRay);
            float lightDistance = lightDelta.Length();
            if (shadowHit.Hit && shadowHit.Distance < lightDistance)
                continue;

            Vector3 irradiance = light.Intensity / (lightDistance * lightDistance);
            // Diffuse
            float diff = MathF.Max(Vector3.Dot(intersection.Normal, lightDir), 0);
            diffuse += diff * intersection.material.DiffuseReflectance * irradiance;

            // Specular
            Vector3 halfDir = Vector3.Normalize(lightDir + viewDir);
            float spec = MathF.Pow(MathF.Max(Vector3.Dot(intersection.Normal, halfDir), 0), intersection.material.PhongExponent);
            specular += spec * intersection.material.SpecularReflectance * irradiance;
        }

        Vector3 color = ambient + diffuse + specular;
        return color;
    }
}