using System.Numerics;
using Raytracer.Scenes.Content.Datas;

namespace Raytracer.Rendering.Shading.BRDFs;

public class ModifiedBlinnPhongBRDF : IBRDF
{
    private readonly float exponent;
    private readonly bool normalized;
    public ModifiedBlinnPhongBRDF(BRDFDefinition def)
    {
        exponent = def.Exponent;
        normalized = def.Normalized;
    }

    public Vector3 Evaluate(Vector3 kd, Vector3 ks, Vector3 wi, Vector3 wo, Vector3 n)
    {
        var cosI = Vector3.Dot(n, wi);
        if (cosI <= 0f) return Vector3.Zero;
        Vector3 wh = Vector3.Normalize(wi + wo);
        
        float ah = MathF.Max(Vector3.Dot(n, wh), 0f);
        float specularFactor = MathF.Pow(ah, exponent);
        
        var diff = kd;
        var spec = ks * specularFactor;
        
        if (normalized)
        {
            diff *= (1 / MathF.PI);
            spec *= specularFactor * ((exponent + 8) / (8 * MathF.PI));
        }
        return diff + spec;
    }
}