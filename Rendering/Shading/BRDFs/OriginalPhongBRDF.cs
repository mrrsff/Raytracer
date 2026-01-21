using System.Numerics;
using Raytracer.Scenes.Content.Datas;

namespace Raytracer.Rendering.Shading.BRDFs;

public class OriginalPhongBRDF : IBRDF
{
    private readonly float exponent;
    
    public OriginalPhongBRDF(BRDFDefinition def)
    {
        exponent = def.Exponent;
    }

    public Vector3 Evaluate(Vector3 kd, Vector3 ks, Vector3 wi, Vector3 wo, Vector3 n)
    {
        float cosI = Vector3.Dot(n, wi);
        if (cosI <= 0f) return Vector3.Zero;

        Vector3 r = Vector3.Reflect(-wi, n);
        float cosR = MathF.Max(0f, Vector3.Dot(r, wo));

        return (kd + ks * MathF.Pow(cosR, exponent)) / cosI;
    }
}