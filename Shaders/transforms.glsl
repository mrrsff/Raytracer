#ifndef TRANSFORMS_GLSL
#define TRANSFORMS_GLSL

#include "definitions.glsl"

mat3 rotationPart(mat4 m)
{
    return mat3(m);
}

vec3 translationPart(mat4 m)
{
    return m[3].xyz;
}

vec3 toWorldPosition(vec3 localPos, mat4 m)
{
    return (m * vec4(localPos, 1.0)).xyz;
}

vec3 toLocalPosition(vec3 worldPos, mat4 m)
{
    mat3 R = mat3(m);
    vec3 T = m[3].xyz;
    return inverse(R) * (worldPos - T);
}

vec3 toWorldDirection(vec3 localDir, mat4 m)
{
    return normalize(mat3(m) * localDir);
}

vec3 toLocalDirection(vec3 worldDir, mat4 m)
{
    return normalize(inverse(mat3(m)) * worldDir);
}

vec3 toWorldNormal(vec3 localNormal, mat4 m)
{
    return normalize(transpose(inverse(mat3(m))) * localNormal);
}

vec3 toLocalNormal(vec3 worldNormal, mat4 m)
{
    return normalize(transpose(mat3(m)) * worldNormal);
}

Ray toWorldRay(Ray ray, mat4 m)
{
    Ray outRay;
    outRay.origin = toWorldPosition(ray.origin, m);
    outRay.direction = toWorldDirection(ray.direction, m);
    return outRay;
}

Ray toLocalRay(Ray ray, mat4 m)
{
    mat3 invR = inverse(mat3(m));
    Ray outRay;
    outRay.origin = invR * (ray.origin - m[3].xyz);
    outRay.direction = normalize(invR * ray.direction);
    return outRay;
}
#endif // TRANSFORMS_GLSL