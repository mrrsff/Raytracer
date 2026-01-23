using System.Numerics;
using Raytracer.Core;
using Raytracer.Rendering.Intersections;
using Raytracer.Rendering.Sampling;
using Raytracer.Rendering.Shading;
using Raytracer.Rendering.Shading.BRDFs;

namespace Raytracer.Rendering.PathTracing.BSDFs;

public class DielectricBSDF : IBSDF
{
    private readonly float _ior;
    private readonly Vector3 _mirrorReflectance;
    private readonly Vector3 _absorptionCoefficient;
    private readonly IBRDF _brdf;
    private readonly Vector3 _ks;

    public DielectricBSDF(Material mat)
    {
        _ior = mat.RefractionIndex;
        _mirrorReflectance = mat.MirrorReflectance;
        _absorptionCoefficient = mat.AbsorptionCoefficient;
        _ks = mat.SpecularReflectance;
        _brdf = mat.Brdf;
    }

    public bool IsDelta => true;

    public BSDFSample Sample(Vector3 wo, IntersectionInfo hit, Vector2 u, bool importanceSample)
    {
        Vector3 n = hit.ShadingNormal;
        float cosThetaI = Vector3.Dot(wo, n);
        bool entering = cosThetaI > 0f;
        float etaI = 1f;
        float etaT = _ior;
        if (!entering)
        {
            n = -n;
            etaI = _ior;
            etaT = 1f;
            cosThetaI = -cosThetaI;
        }
        float eta = etaI / etaT;

        float fresnel = FresnelComputation.ComputeFresnelDielectric(etaI, etaT, cosThetaI);
        fresnel = Math.Clamp(fresnel, 0f, 1f);

        if (u.X < fresnel) // Reflection
        {
            Vector3 wi = Vector3.Reflect(-wo, n);
            float cosThetaO = MathF.Abs(Vector3.Dot(wi, hit.ShadingNormal));
            Vector3 f = _mirrorReflectance / cosThetaO;
            return new BSDFSample(wi, f, 1f, BSDFSampleFlags.Delta);
        }
        else
        {
            if (CPURenderer.Refract(wo, n, eta, out Vector3 wi)) // Refraction occurs
            {
                float cosThetaT = MathF.Abs(Vector3.Dot(wi, hit.ShadingNormal));
                Vector3 absorption = Vector3.Exp(-_absorptionCoefficient * hit.Distance);
                Vector3 f = (1f - fresnel) * absorption / cosThetaT;
                return new BSDFSample(wi, f, 1f, BSDFSampleFlags.Delta);
            }
            else // Total Internal Reflection
            {
                Vector3 wi_tir = Vector3.Reflect(-wo, n);
                float cosThetaO = MathF.Abs(Vector3.Dot(wi_tir, hit.ShadingNormal));
                Vector3 f = _mirrorReflectance / cosThetaO;
                return new BSDFSample(wi_tir, f, 1f, BSDFSampleFlags.Delta);
            }
        }
    }
    public Vector3 Evaluate(Vector3 wo, Vector3 wi, IntersectionInfo hit) => Vector3.Zero;
    public float Pdf(Vector3 wo, Vector3 wi, IntersectionInfo hit) => 0.0f;
}