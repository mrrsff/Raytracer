using System.Numerics;
using Raytracer.Core;
using Raytracer.Scenes.Content.Datas;

namespace Raytracer.Rendering.Shading.BRDFs;

public class ModifiedBlinnPhongBRDF : IBRDF
{
    private readonly Vector3 kd;
    private readonly Vector3 ks;
    private readonly float exponent;
    private readonly bool normalized;
    
    public ModifiedBlinnPhongBRDF(Material mat, BRDFDefinition def)
    {
        kd = mat.DiffuseReflectance;
        ks = mat.SpecularReflectance;
        exponent = def.Exponent;
    }

    public Vector3 Evaluate(Vector3 wi, Vector3 wo, Vector3 n)
    {
        if (normalized)
        {
            Vector3 h = Vector3.Normalize(wi + wo);
            float cosH = MathF.Max(0f, Vector3.Dot(n, h));

            Vector3 diffuse = kd / MathF.PI;
            Vector3 specular = ks * ((exponent + 8f) / (8f * MathF.PI))
                                  * MathF.Pow(cosH, exponent);

            return diffuse + specular;
        }
        else
        {
            Vector3 h = Vector3.Normalize(wi + wo);
            float cosH = MathF.Max(0f, Vector3.Dot(n, h));

            return kd + ks * MathF.Pow(cosH, exponent);
        }
    }
}