using System.Numerics;
using Raytracer.Core;
using Raytracer.Rendering.Intersections;
using Raytracer.Rendering.Shading.BRDFs;

namespace Raytracer.Rendering.PathTracing.BSDFs;

public class MirrorBSDF : IBSDF
{
    private readonly Vector3 _ks;
    private readonly IBRDF _brdf;

    public MirrorBSDF(Material mat)
    {
        _ks = mat.SpecularReflectance;
        _brdf = mat.Brdf;
    }

    public bool IsDelta => true;

    public BSDFSample Sample(Vector3 wo, IntersectionInfo hit, Vector2 u, bool importanceSample)
    {
        Vector3 wi = Vector3.Reflect(-wo, hit.ShadingNormal);
        
        float cosTheta = MathF.Abs(Vector3.Dot(hit.ShadingNormal, wi));
        if (cosTheta <= 0) return BSDFSample.Invalid;

        Vector3 f = _brdf.Evaluate(Vector3.Zero, _ks, wi, wo, hit.ShadingNormal) / cosTheta;

        return new BSDFSample(wi, f, 1.0f, BSDFSampleFlags.Delta);
    }

    public Vector3 Evaluate(Vector3 wo, Vector3 wi, IntersectionInfo hit) => Vector3.Zero;
    public float Pdf(Vector3 wo, Vector3 wi, IntersectionInfo hit) => 0.0f;
}