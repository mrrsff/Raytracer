#ifndef TRANSFORMS_GLSL
#define TRANSFORMS_GLSL

#include "objects.glsl"
#include "definitions.glsl"

mat3 rotationPart(mat4x3 m)
{
    return mat3(m);
}

vec3 translationPart(mat4x3 m)
{
    return m[3];
}

vec3 toWorldPosition(vec3 localPos, mat4x3 transform)
{
    return transform * vec4(localPos, 1.0);
}

vec3 toLocalPosition(vec3 worldPos, mat4x3 transform)
{
    mat3 R = rotationPart(transform);
    vec3 T = translationPart(transform);

    mat3 invR = inverse(R);
    return invR * (worldPos - T);
}

vec3 toWorldDirection(vec3 localDir, mat4x3 transform)
{
    return normalize(rotationPart(transform) * localDir);
}

vec3 toLocalDirection(vec3 worldDir, mat4x3 transform)
{
    mat3 invR = inverse(rotationPart(transform));
    return normalize(invR * worldDir);
}

vec3 toWorldNormal(vec3 localNormal, mat4x3 transform)
{
    mat3 R = rotationPart(transform);
    mat3 invTransR = transpose(inverse(R));
    return normalize(invTransR * localNormal);
}

vec3 toLocalNormal(vec3 worldNormal, mat4x3 transform)
{
    mat3 R = rotationPart(transform);
    return normalize(R * worldNormal);
}

Ray toWorldRay(Ray ray, mat4x3 transform)
{
    Ray outRay;
    outRay.origin = toWorldPosition(ray.origin, transform);
    outRay.direction = toWorldDirection(ray.direction, transform);
    return outRay;
}

Ray toLocalRay(Ray ray, mat4x3 transform)
{
    Ray outRay;
    outRay.origin = toLocalPosition(ray.origin, transform);
    outRay.direction = toLocalDirection(ray.direction, transform);
    return outRay;
}
#endif // TRANSFORMS_GLSL