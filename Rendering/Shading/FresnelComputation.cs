using System.Numerics;
using Raytracer.Core;

namespace Raytracer.Rendering.Shading;

public static class FresnelComputation
{
    public static Vector3 ComputeFresnelConductor(Material mat, float cosThetaI)
    {
        Vector3 n = new Vector3(mat.RefractionIndex);
        Vector3 k = new Vector3(mat.AbsorptionIndex);
        float cosTheta = cosThetaI;

        Vector3 cos2 = new(cosTheta * cosTheta);
        Vector3 n2 = n * n;
        Vector3 k2 = k * k;
        Vector3 twoNCos = 2 * n * cosTheta;

        Vector3 Rs = (n2 + k2 - twoNCos + cos2) / (n2 + k2 + twoNCos + cos2);
        Vector3 Rp = (n2 + k2) * cos2 - twoNCos + Vector3.One;
        Rp /= (n2 + k2) * cos2 + twoNCos + Vector3.One;

        return 0.5f * (Rs + Rp);
    }
    
    public static float ComputeFresnelDielectric(float etai, float etat, float cosThetaI)
    {
        float cosi = Math.Clamp(cosThetaI, -1f, 1f);
        float sint = (etai / etat) * MathF.Sqrt(MathF.Max(0f, 1f - cosi * cosi));
        if (sint >= 1f) return 1f; // Total Internal Reflection

        float cost = MathF.Sqrt(MathF.Max(0f, 1f - sint * sint));
        cosi = MathF.Abs(cosi);

        float Rs = ((etat * cosi) - (etai * cost)) / ((etat * cosi) + (etai * cost));
        float Rp = ((etai * cosi) - (etat * cost)) / ((etai * cosi) + (etat * cost));
        float F = 0.5f * (Rs * Rs + Rp * Rp);
        return Math.Clamp(F, 0f, 1f);
    }
}