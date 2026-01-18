using System.Numerics;
using Raytracer.Core;
using Raytracer.Scenes.Content.Datas;

namespace Raytracer.Rendering.Shading.BRDFs;

public class TorranceSparrowBRDF : IBRDF
{
    private readonly float exponent;
    private readonly bool kdFresnel;
    private readonly Material mat;
    
    public TorranceSparrowBRDF(Material mat, BRDFDefinition def)
    {
        exponent = def.Exponent;
        this.mat = mat;
        kdFresnel = def.KDFresnel;
    }

    public Vector3 Evaluate(Vector3 kd, Vector3 ks, Vector3 wi, Vector3 wo, Vector3 n)
    {
        float cosI = Vector3.Dot(n, wi);
        float cosO = Vector3.Dot(n, wo);

        if (cosI <= 0f || cosO <= 0f)
            return Vector3.Zero;

        Vector3 h = Vector3.Normalize(wi + wo);
        float cosH = MathF.Max(0f, Vector3.Dot(n, h));
        float woDotH = MathF.Max(0f, Vector3.Dot(wo, h));

        float D = (exponent + 2f) / (2f * MathF.PI) * MathF.Pow(cosH, exponent);

        float G1 = 2f * cosH * cosO / woDotH;
        float G2 = 2f * cosH * cosI / woDotH;
        float G = MathF.Min(1f, MathF.Min(G1, G2));

        float eta = mat.RefractionIndex;
        float R0 = MathF.Pow((eta - 1f) / (eta + 1f), 2f);
        float F = R0 + (1f - R0) * MathF.Pow(1f - woDotH, 5f);

        Vector3 specular = ks * (D * G * F) / (4f * cosI * cosO);

        Vector3 diffuse = kdFresnel
                ? (Vector3.One - new Vector3(F)) * kd / MathF.PI
                : kd / MathF.PI;

        return diffuse + specular;
    }
}