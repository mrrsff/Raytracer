#ifndef INTERSECTIONS_GLSL
#define INTERSECTIONS_GLSL

#include "scene_layout.glsl"

float intersect_sphere(vec3 rayOrigin, vec3 rayDirection, Sphere sphere)
{
    vec3 oc = rayOrigin - sphere.center;
    float a = dot(rayDirection, rayDirection);
    float b = 2.0 * dot(oc, rayDirection);
    float c = dot(oc, oc) - sphere.radius * sphere.radius;
    float discriminant = b * b - 4.0 * a * c;

    if (discriminant < 0.0)
    {
        return -1.0; // No intersection
    }
    else
    {
        float sqrtDiscriminant = sqrt(discriminant);
        float t1 = (-b - sqrtDiscriminant) / (2.0 * a);
        if (t1 < 0.0)
        {
            float t2 = (-b + sqrtDiscriminant) / (2.0 * a);
            if (t2 < 0.0)
                return -1.0; // Both intersections are behind the ray origin
            return t2;
        }
        return t1;
    }
}
float intersectAny(vec3 rayOrigin, vec3 rayDirection)
{
    for (int i = 0; i < sceneGlobals.numSpheres; ++i)
    {
        float t = intersect_sphere(rayOrigin, rayDirection, spheres[i]);
        if (t > 0.0)
        {
            return t; // Return the first intersection found
        }
    }
    return -1.0; // No intersection
}
#endif // INTERSECTIONS_GLSL