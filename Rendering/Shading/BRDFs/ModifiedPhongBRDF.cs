using System.Numerics;
using Raytracer.Scenes.Content.Datas;

namespace Raytracer.Rendering.Shading.BRDFs;

public class ModifiedPhongBRDF : IBRDF
{
    private readonly float exponent;
    private readonly bool normalized;

    public ModifiedPhongBRDF(BRDFDefinition def)
    {
        exponent = def.Exponent;
        normalized = def.Normalized;
    }

    public Vector3 Evaluate(Vector3 kd, Vector3 ks, Vector3 wi, Vector3 wo, Vector3 n)
    {
        Vector3 r = Vector3.Reflect(-wi, n);
        float cosR = MathF.Max(0f, Vector3.Dot(r, wo));

        if (normalized)
        {
            Vector3 diffuse = kd / MathF.PI;
            Vector3 specular = ks * ((exponent + 2f) / (2f * MathF.PI))
                                         * MathF.Pow(cosR, exponent);

            return diffuse + specular;
        }
        else
        {
            return kd + ks * MathF.Pow(cosR, exponent);
        }
    }
}