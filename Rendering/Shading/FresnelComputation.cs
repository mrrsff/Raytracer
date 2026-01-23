using System.Numerics;
using Raytracer.Core;

namespace Raytracer.Rendering.Shading;

public static class FresnelComputation
{
    public static Vector3 ComputeFresnelConductor(Material mat, float cosThetaI)
    {
        return ComputeFresnelConductor(mat.RefractionIndex, mat.AbsorptionIndex, cosThetaI);
    }

    public static Vector3 ComputeFresnelConductor(float refr, float abs, float cosThetaI)
    {
        Vector3 n = new Vector3(refr);
        Vector3 k = new Vector3(abs);
        
        Vector3 cos2 = new(cosThetaI * cosThetaI);
        Vector3 n2 = n * n;
        Vector3 k2 = k * k;
        Vector3 twoNCos = 2 * n * cosThetaI;

        Vector3 Rs = (n2 + k2 - twoNCos + cos2) / (n2 + k2 + twoNCos + cos2);
        Vector3 Rp = (n2 + k2) * cos2 - twoNCos + Vector3.One;
        Rp /= (n2 + k2) * cos2 + twoNCos + Vector3.One;

        return 0.5f * (Rs + Rp);
    }
    public static float ComputeFresnelDielectric(float etai, float etat, float cosThetaI)
    {
        float cosI = Math.Clamp(cosThetaI, -1f, 1f);
        float sint = (etai / etat) * MathF.Sqrt(MathF.Max(0f, 1f - cosI * cosI));
        if (sint >= 1f) return 1f;

        float cost = MathF.Sqrt(MathF.Max(0f, 1f - sint * sint));
        cosI = MathF.Abs(cosI);

        float Rs = ((etat * cosI) - (etai * cost)) / ((etat * cosI) + (etai * cost));
        float Rp = ((etai * cosI) - (etat * cost)) / ((etai * cosI) + (etat * cost));
        float F = 0.5f * (Rs * Rs + Rp * Rp);
        return Math.Clamp(F, 0f, 1f);
    }
}