#ifndef SHADING_GLSL
#define SHADING_GLSL

#include "scene_layout.glsl"
#include "intersections.glsl"

void evalBlinnPhong(in vec3 N, in vec3 V, in vec3 L, in vec3 kd, in vec3 ks, in float phongExp, in vec3 irradiance, inout vec3 diffuse, inout vec3 specular)
{
    float ndotl = max(dot(N, L), 0.0);
    ndotl = abs(ndotl);

    diffuse += kd * irradiance * ndotl;

    vec3 H = normalize(L + V);
    float spec = pow(max(dot(N, H), 0.0), phongExp);
    specular += ks * irradiance * spec;
}

bool isOccluded(in vec3 point, in vec3 lightPos, in vec3 normal)
{
    vec3 toLight = lightPos - point;
    float maxDistance = length(toLight);
    vec3 L = normalize(toLight);

    vec3 shadowOrigin = point + normal * SHADOW_BIAS;
    Ray ray;
    ray.origin = shadowOrigin;
    ray.direction = L;

    return IntersectAny(ray, maxDistance);
}

bool samplePointLight(PointLight light, vec3 P, vec3 N, out vec3 L, out vec3 irradiance)
{
    // shadow test
    if (isOccluded(P, light.position, N))
    {
        irradiance = vec3(0);
        return false;
    }
    
    vec3 toLight = light.position - P;

    float dist2 = dot(toLight, toLight);
    L = normalize(toLight);

    irradiance = light.intensity / dist2;
    return true;
}

void shadePointLights(vec3 P, vec3 N, vec3 V, vec3 kd, vec3 ks, float phongExp, inout vec3 diffuse, inout vec3 specular)
{
    for (uint i = 0; i < sceneGlobals.numPointLights; ++i)
    {
        vec3 L;
        vec3 irradiance;

        if (samplePointLight(pointLights[i], P, N, L, irradiance))
        {
            evalBlinnPhong(N, V, L, kd, ks, phongExp, irradiance, diffuse, specular);
        }
    }
}

vec3 shade(Intersection intersection)
{
    Material mat = GetMaterial(intersection.materialIndex);

    vec3 kd = mat.diffuseReflectance.xyz;
    vec3 ks = mat.specularReflectance.xyz;

    vec3 V = normalize(camera.position - intersection.position);

    vec3 ambient = sceneGlobals.ambientLight * mat.ambientReflectance.xyz;
    vec3 diffuse = vec3(0);
    vec3 specular = vec3(0);
    
    shadePointLights(intersection.position, intersection.shadingNormal, V, kd, ks, mat.phongExponent, diffuse, specular);
    
    vec3 color = ambient + diffuse + specular;
    return color;
}
#endif // SHADING_GLSL