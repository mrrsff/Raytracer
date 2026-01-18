#ifndef FRESNEL_GLSL
#define FRESNEL_GLSL

// Compute Fresnel reflectance for conductors (metals)
// Returns RGB reflectance based on complex index of refraction
vec3 ComputeFresnelConductor(Material mat, float cosThetaI)
{
    vec3 n = vec3(mat.refractionIndex);
    vec3 k = vec3(mat.absorptionIndex);

    vec3 cos2 = vec3(cosThetaI * cosThetaI);
    vec3 n2 = n * n;
    vec3 k2 = k * k;
    vec3 twoNCos = 2.0 * n * cosThetaI;

    // Rs: perpendicular polarization
    vec3 Rs = (n2 + k2 - twoNCos + cos2) / (n2 + k2 + twoNCos + cos2);

    // Rp: parallel polarization
    vec3 Rp = (n2 + k2) * cos2 - twoNCos + vec3(1.0);
    Rp /= (n2 + k2) * cos2 + twoNCos + vec3(1.0);

    // Average of both polarizations
    return 0.5 * (Rs + Rp);
}

// Compute Fresnel reflectance for dielectrics (glass, water, etc.)
// Returns scalar reflectance [0, 1]
float ComputeFresnelDielectric(float etai, float etat, float cosThetaI)
{
    float cosI = clamp(cosThetaI, -1.0, 1.0);

    // Calculate sine of transmitted angle using Snell's law
    float sint = (etai / etat) * sqrt(max(0.0, 1.0 - cosI * cosI));

    // Total internal reflection
    if (sint >= 1.0)
    {
        return 1.0;
    }

    // Calculate cosine of transmitted angle
    float cost = sqrt(max(0.0, 1.0 - sint * sint));
    cosI = abs(cosI);

    // Fresnel equations for s and p polarizations
    float Rs = ((etat * cosI) - (etai * cost)) / ((etat * cosI) + (etai * cost));
    float Rp = ((etai * cosI) - (etat * cost)) / ((etai * cosI) + (etat * cost));

    // Average reflectance
    float F = 0.5 * (Rs * Rs + Rp * Rp);

    return clamp(F, 0.0, 1.0);
}

// Schlick's approximation (faster alternative for dielectrics)
// Good approximation but less physically accurate than full Fresnel
float FresnelSchlick(float cosTheta, float F0)
{
    return F0 + (1.0 - F0) * pow(1.0 - cosTheta, 5.0);
}

vec3 FresnelSchlickVec3(float cosTheta, vec3 F0)
{
    return F0 + (1.0 - F0) * pow(1.0 - cosTheta, 5.0);
}

#endif // FRESNEL_GLSL