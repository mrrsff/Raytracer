using System.Numerics;
using Raytracer.Core;
using Raytracer.Scenes.Content.Datas;

namespace Raytracer.Rendering.Shading.BRDFs;

public class OriginalBlinnPhongBRDF : IBRDF
{
    private readonly Vector3 kd;
    private readonly Vector3 ks;
    private readonly float exponent;
    
    public OriginalBlinnPhongBRDF(Material mat, BRDFDefinition def)
    {
        kd = mat.DiffuseReflectance;
        ks = mat.SpecularReflectance;
        exponent = def.Exponent;
    }

    public Vector3 Evaluate(Vector3 wi, Vector3 wo, Vector3 n)
    {
        float cosI = Vector3.Dot(n, wi);
        if (cosI <= 0f) return Vector3.Zero;

        Vector3 h = Vector3.Normalize(wi + wo);
        float cosH = MathF.Max(0f, Vector3.Dot(n, h));

        return (kd + ks * MathF.Pow(cosH, exponent)) / cosI;
    }
}