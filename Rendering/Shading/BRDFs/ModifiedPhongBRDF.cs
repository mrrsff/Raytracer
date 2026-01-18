using System.Numerics;
using Raytracer.Core;
using Raytracer.Scenes.Content.Datas;

namespace Raytracer.Rendering.Shading.BRDFs;

public class ModifiedPhongBRDF : IBRDF
{
    private readonly Vector3 kd;
    private readonly Vector3 ks;
    private readonly float exponent;
    private readonly bool normalized;
    public ModifiedPhongBRDF(Material mat, BRDFDefinition def)
    {
        kd = mat.DiffuseReflectance;
        ks = mat.SpecularReflectance;
        exponent = def.Exponent;
    }

    public Vector3 Evaluate(Vector3 wi, Vector3 wo, Vector3 n)
    {
        if (normalized)
        {
            Vector3 r = Vector3.Reflect(-wi, n);
            float cosR = MathF.Max(0f, Vector3.Dot(r, wo));

            Vector3 diffuse = kd / MathF.PI;
            Vector3 specular = ks * ((exponent + 2f) / (2f * MathF.PI))
                                  * MathF.Pow(cosR, exponent);

            return diffuse + specular;
        }
        else
        {
            Vector3 r = Vector3.Reflect(-wi, n);
            float cosR = MathF.Max(0f, Vector3.Dot(r, wo));

            return kd + ks * MathF.Pow(cosR, exponent);
        }
    }
}