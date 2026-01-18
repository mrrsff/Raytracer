using System.Numerics;
using Raytracer.Scenes.Content.Datas;

namespace Raytracer.Rendering.Shading.BRDFs;

public class OriginalBlinnPhongBRDF : IBRDF
{
    private readonly float exponent;
    public OriginalBlinnPhongBRDF(BRDFDefinition def)
    {
        exponent = def.Exponent;
    }
    public Vector3 Evaluate(Vector3 kd, Vector3 ks, Vector3 wi, Vector3 wo, Vector3 n)
    {
        var cosI = Vector3.Dot(n, wi);
        if (cosI <= 0f) return Vector3.Zero;
    
        Vector3 wh = Vector3.Normalize(wi + wo);
        float ah = MathF.Max(Vector3.Dot(n, wh), 0f);
        float specularFactor = MathF.Pow(ah, exponent);
    
        return kd + ks * (specularFactor / cosI);
    }
}