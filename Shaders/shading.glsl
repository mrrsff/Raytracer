#ifndef SHADING_GLSL
#define SHADING_GLSL

#include "scene_layout.glsl"
#include "intersections.glsl"

void evalBlinnPhong(vec3 N, vec3 V, vec3 L, vec3 kd, vec3 ks, float phongExp, vec3 irradiance, inout vec3 diffuse, inout vec3 specular)
{
    float ndotl = max(dot(N, L), 0.0);
    ndotl = abs(ndotl);

    diffuse += kd * irradiance * ndotl;

    vec3 H = normalize(L + V);
    float spec = pow(max(dot(N, H), 0.0), phongExp);
    specular += ks * irradiance * spec;
}

bool samplePointLight(PointLight light, vec3 P, vec3 N, out vec3 L, out vec3 irradiance)
{
    vec3 toLight = light.position - P;

    float dist2 = dot(toLight, toLight);
    L = normalize(toLight);

    irradiance = light.intensity * light.color / dist2;
    return true;
}

vec3 shadePointLights(vec3 P, vec3 N, vec3 V, vec3 kd, vec3 ks, float phongExp)
{
    vec3 diffuse = vec3(0.0);
    vec3 specular = vec3(0.0);

    for (uint i = 0; i < sceneGlobals.numPointLights; ++i)
    {
        vec3 L;
        vec3 irradiance;

        if (samplePointLight(pointLights[i], P, N, L, irradiance))
        {
            // shadow test must be here
            if (intersectAny(P + N * 0.001, L) >= 0.0)
                continue;

            evalBlinnPhong(N, V, L, kd, ks, phongExp, irradiance, diffuse, specular);
        }
    }
    return diffuse + specular;
}

vec3 shade(vec3 hitPoint, vec3 normal)
{
    vec3 viewDir = normalize(camera.position - hitPoint);

    vec3 kd = vec3(0.8); // diffuse albedo
    vec3 ks = vec3(0.5); // specular albedo
    float phongExp = 32.0; // shininess

    return shadePointLights(hitPoint, normal, viewDir, kd, ks, phongExp);
}
#endif // SHADING_GLSL