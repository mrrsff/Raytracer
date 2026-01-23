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

public class LightSphere : Sphere, IObjectLight
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
        Transform tr = GetMotionBlurTransform(time);

        Vector3 worldCenter = tr.ToWorldPoint(center);
        float worldRadius = tr.ToWorldDirection(new Vector3(radius, 0, 0), false).Length();
        
        L = irradiance = Vector3.Zero;

        Vector3 toC = worldCenter - P;
        float d = toC.Length();

        float sinThetaMax = worldRadius / d;
        float cosThetaMax = MathF.Sqrt(1f - sinThetaMax * sinThetaMax);

        float xi1 = Sampler.OneDimensionalUniform();
        float xi2 = Sampler.OneDimensionalUniform();

        float theta = MathF.Acos(1f - xi1 + xi1 * cosThetaMax);
        float phi = 2f * MathF.PI * xi2;

        // ONB
        Vector3 w = Vector3.Normalize(toC);
        MathUtility.BuildONB(w, out Vector3 u, out Vector3  v);

        Vector3 wi =
            MathF.Sin(theta) * MathF.Cos(phi) * u +
            MathF.Sin(theta) * MathF.Sin(phi) * v +
            MathF.Cos(theta) * w;

        wi = Vector3.Normalize(wi);

        Ray shadowRay = new Ray(
            P + N * renderer.Scene.Content.ShadowRayEpsilon,
            wi,
            true,
            time
        );

        IntersectionInfo info = renderer.Scene.Intersect(shadowRay);
        if (info.Hit && info.HitGeometry != this)
            return false;

        Vector3 pointOnLight = info.Point;
        Vector3 normalOnLight = Vector3.Normalize(pointOnLight - worldCenter);

        float cosLight = Vector3.Dot(-wi, normalOnLight);
        if (cosLight <= 0f)
            return false;

        float pdf = 1f / (2f * MathF.PI * (1f - cosThetaMax));

        L = wi;
        irradiance = radiance * cosLight / pdf;
        return true;
    }

    public float Pdf(Vector3 P, Vector3 N, float time, Renderer renderer)
    {
        Transform tr = GetMotionBlurTransform(time);

        Vector3 worldCenter = tr.ToWorldPoint(center);
        float worldRadius = tr.ToWorldDirection(new Vector3(radius, 0, 0), false).Length();
        
        Vector3 toC = worldCenter - P;
        float d = toC.Length();

        float sinThetaMax = worldRadius / d;
        float cosThetaMax = MathF.Sqrt(1f - sinThetaMax * sinThetaMax);

        float pdf = 1f / (2f * MathF.PI * (1f - cosThetaMax));
        return pdf;
    }
}