using System.Numerics;
using Raytracer.Core;
using Raytracer.Core.Lights;
using Raytracer.Rendering;
using Raytracer.Rendering.Intersections;
using Raytracer.Rendering.Sampling;
using Raytracer.Scenes.Content.Datas;
using Raytracer.Scenes.Content.Datas.Objects;
using Raytracer.Utility;

namespace Raytracer.Scenes.Runtime;

public class LightSphere : Sphere, ILight
{
    public override bool IsEmitter => true;
    public override Vector3 Emission => radiance;
    private readonly Vector3 radiance;
    public LightSphere(LightSphereData data, in VertexData vertexData) : base(data, in vertexData)
    {
        radiance = data.Radiance;
    }
    public bool Sample(in Vector3 P, in Vector3 N, float time, Renderer renderer, out Vector3 L, out Vector3 irradiance)
    {
        SelectPoint(out Vector3 pointOnLight, out Vector3 normalOnLight, out float pdf);
        
        Ray shadowRay = new Ray(
            P + N * renderer.Scene.Content.ShadowRayEpsilon,
            Vector3.Normalize(pointOnLight - P),
            true,
            time);
        IntersectionInfo info = renderer.Scene.Intersect(shadowRay);
        if (info.Hit && info.HitGeometry != this)
        {
            L = irradiance = default;
            return false;
        }

        Vector3 wi = Vector3.Normalize(pointOnLight - P);
        
        float solidAngle = pdf * Vector3.Dot(-wi, normalOnLight) / (radius * radius);
        if (solidAngle <= 0f || pdf <= 0f)
        {
            L = Vector3.Zero;
            irradiance = Vector3.Zero;
            return false;
        }

        L = wi;
        irradiance = radiance / solidAngle;
        return true;
    }
    
    private void SelectPoint(out Vector3 pointOnLight, out Vector3 normalOnLight, out float pdf)
    {
        Vector3 dir = Sampler.UniformSampleSphere();
        pointOnLight = center + dir * radius;
        normalOnLight = dir;
        pdf = 1f / (4f * MathF.PI * radius * radius);
    }
}