using System.Numerics;
using Raytracer.Core;
using Raytracer.Rendering.Intersections;
using Raytracer.Rendering.Shading;
using Raytracer.Rendering.Shading.BRDFs;
using Raytracer.Utility;

namespace Raytracer.Rendering.PathTracing.BSDFs;

public class ConductorBSDF : IBSDF
{
    private readonly Vector3 _kd;
    private readonly Vector3 _ks;
    private readonly float _eta;
    private readonly float _k;
    private readonly IBRDF _brdf;
    private readonly Vector3 _mirrorReflectance;

    public ConductorBSDF(Material mat)
    {
        _kd = mat.DiffuseReflectance;
        _ks = mat.SpecularReflectance;
        _eta = mat.RefractionIndex;
        _k = mat.AbsorptionIndex;
        _brdf = mat.Brdf;
        _mirrorReflectance = mat.MirrorReflectance;
    }

    public bool IsDelta => true;

    public BSDFSample Sample(Vector3 wo, IntersectionInfo hit, Vector2 u, bool importanceSample)
    {
        Vector3 n = hit.ShadingNormal;
        if (Vector3.Dot(wo, n) < 0f) n = -n;

        Vector3 wi = Vector3.Reflect(-wo, n);
        float cosOut = MathF.Abs(Vector3.Dot(n, wi));
    
        if (cosOut <= 0f) return BSDFSample.Invalid;

        float cosI = Math.Clamp(MathF.Abs(Vector3.Dot(n, wo)), 0f, 1f);
        Vector3 Fr = FresnelComputation.ComputeFresnelConductor(_eta, _k, cosI);
    
        Vector3 brdfEval = _brdf.Evaluate(_ks * Fr, _kd * (Vector3.One - Fr), wi, wo, n);

        Vector3 f;
        if (brdfEval == Vector3.Zero)
        {
            f = (_mirrorReflectance * Fr) / cosOut;
        }
        else
        {
            f = brdfEval / cosOut;
        }

        return new BSDFSample(wi, f, 1.0f, BSDFSampleFlags.Delta | BSDFSampleFlags.Glossy);
    }

    public Vector3 Evaluate(Vector3 wo, Vector3 wi, IntersectionInfo hit) => Vector3.Zero;
    public float Pdf(Vector3 wo, Vector3 wi, IntersectionInfo hit) => 0.0f;
}