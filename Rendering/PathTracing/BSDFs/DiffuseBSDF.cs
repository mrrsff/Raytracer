using System.Numerics;
using Raytracer.Core;
using Raytracer.Rendering.Intersections;
using Raytracer.Rendering.Sampling;
using Raytracer.Rendering.Shading.BRDFs;

namespace Raytracer.Rendering.PathTracing.BSDFs;

public class DiffuseBSDF : IBSDF
{
    private readonly IBRDF _brdf;
    private readonly Vector3 _kd;
    private readonly Vector3 _ks;

    public DiffuseBSDF(Material mat)
    {
        _kd = mat.DiffuseReflectance;
        _ks = mat.SpecularReflectance;
        _brdf = mat.Brdf;
    }

    public bool IsDelta => false;

    public BSDFSample Sample(Vector3 wo, IntersectionInfo hit, Vector2 u, bool importanceSample)
    {
        Vector3 localWi = Sampler.CosineSampleHemisphere(u);

        Vector3 wi = AlignToNormal(localWi, hit.ShadingNormal); 

        float pdf = Pdf(wo, wi, hit);
        if (pdf <= 0f) return BSDFSample.Invalid;

        Vector3 f = Evaluate(wo, wi, hit);
        return new BSDFSample(wi, f, pdf, BSDFSampleFlags.Diffuse);
    }
    private Vector3 AlignToNormal(Vector3 localDir, Vector3 normal)
    {
        Vector3 binormal = Math.Abs(normal.X) > 0.1f ? Vector3.UnitY : Vector3.UnitX;
        Vector3 tangent = Vector3.Normalize(Vector3.Cross(binormal, normal));
        binormal = Vector3.Cross(normal, tangent);

        return localDir.X * tangent + localDir.Y * binormal + localDir.Z * normal;
    }
    public Vector3 Evaluate(Vector3 wo, Vector3 wi, IntersectionInfo hit)
    {
        float cosI = Vector3.Dot(hit.ShadingNormal, wi);
        if (cosI <= 0f) return Vector3.Zero;

        return _brdf?.Evaluate(_kd, _ks, wi, wo, hit.ShadingNormal) ?? _kd / MathF.PI;
    }

    public float Pdf(Vector3 wo, Vector3 wi, IntersectionInfo hit)
    {
        float cosI = Vector3.Dot(hit.ShadingNormal, wi);
        if (cosI <= 0f) return 0f;
        return cosI / MathF.PI;
    }
}