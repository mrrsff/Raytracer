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

        // Check if both directions are above the surface
        if (cosI <= 0f || cosO <= 0f)
            return Vector3.Zero;

        Vector3 h = Vector3.Normalize(wi + wo);
        float cosAlpha = MathF.Max(0f, Vector3.Dot(n, h));
        float D_alpha = D(cosAlpha);
        float woDotH = MathF.Max(0f, Vector3.Dot(wo, h));
        float G_term = G(cosO, cosI, woDotH, cosAlpha);
        
        float cosBeta = woDotH;
        
        // Compute Fresnel based on material type
        Vector3 F_beta;
        if (mat.AbsorptionIndex > 0f)
        {
            // Conductor material - use full Fresnel computation
            F_beta = FresnelComputation.ComputeFresnelConductor(mat, cosBeta);
        }
        else
        {
            // Dielectric material - use Schlick's approximation
            float R0 = ComputeR0(mat.RefractionIndex);
            float fresnelScalar = ComputeSchlickFresnel(R0, cosBeta);
            F_beta = new Vector3(fresnelScalar);
        }
        
        Vector3 diffuse = kd * (1f / MathF.PI);
        
        float denominator = 4f * cosI * cosO;
        Vector3 specular = Vector3.Zero;
        
        if (denominator > 0f)
        {
            float specularCoeff = D_alpha * G_term / denominator;
            specular = ks * F_beta * specularCoeff;
        }
        if (kdFresnel)
        {
            diffuse = Vector3.Multiply(diffuse, Vector3.One - F_beta);
        }
        
        return diffuse + specular;
    }
    private float D(float cosAlpha)
    {
        return (exponent + 2f) / (2f * MathF.PI) * MathF.Pow(cosAlpha, exponent);
    }
    private float G(float cosO, float cosI, float woDotH, float cosH)
    {
        if (woDotH <= 0f)
            return 0f;
            
        float G1 = 2f * cosH * cosO / woDotH;
        float G2 = 2f * cosH * cosI / woDotH;
        return MathF.Min(1f, MathF.Min(G1, G2));
    }
    private float ComputeSchlickFresnel(float R0, float cosBeta)
    {
        float oneMinusCos = 1f - cosBeta;
        float oneMinusCos5 = oneMinusCos * oneMinusCos * oneMinusCos * oneMinusCos * oneMinusCos;
        return R0 + (1f - R0) * oneMinusCos5;
    }
    private float ComputeR0(float eta)
    {
        float etaMinusOne = eta - 1f;
        float etaPlusOne = eta + 1f;
        return (etaMinusOne * etaMinusOne) / (etaPlusOne * etaPlusOne);
    }
}