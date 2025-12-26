#ifndef INTERSECTIONS_GLSL
#define INTERSECTIONS_GLSL

#include "scene_layout.glsl"
#include "definitions.glsl"
#include "objects.glsl"
#include "transforms.glsl"

struct Intersection
{
    float t;                // ray parameter (distance)
    int meshIndex;          // which mesh was hit
    int primIndex;          // triangle index within the mesh
    int materialIndex;      // resolved material
    vec3 position;          // world-space hit point
    vec3 geometricNormal;   // unmodified triangle normal
    vec3 shadingNormal;     // possibly normal-mapped
    vec2 uv;                // interpolated UV
};

Intersection intersectTriangle(Ray ray, Vertex v0, Vertex v1, Vertex v2, in bool backfaceCulling, in bool smoothNormals)
{
    Intersection hit;
    hit.t = -1.0;

    vec3 edge1 = v1.position - v0.position;
    vec3 edge2 = v2.position - v0.position;
    vec3 h = cross(ray.direction, edge2);
    float a = dot(edge1, h);

    if (abs(a) < 1e-8) return hit;
//    if (backfaceCulling && a < 0.0) return hit;
    
    float f = 1.0 / a;
    vec3 s = ray.origin - v0.position;
    float u = f * dot(s, h);
    if (u < 0.0 || u > 1.0)
        return hit;

    vec3 q = cross(s, edge1);
    float v = f * dot(ray.direction, q);
    if (v < 0.0 || u + v > 1.0)
        return hit;

    float tHit = f * dot(edge2, q);
    if (tHit < INTERSECTION_TEST_EPSILON)
        return hit; // Triangle is behind ray

    hit.t = tHit;
    hit.position = ray.origin + ray.direction * tHit;
    if (smoothNormals) {
        float w = 1.0 - u - v;
        hit.shadingNormal = normalize(w * v0.normal + u * v1.normal + v * v2.normal); // barycentric interpolation
    }
    else {
        hit.shadingNormal = normalize(cross(edge1, edge2));
    }
    hit.geometricNormal = hit.shadingNormal;
    hit.uv = (1.0 - u - v) * v0.uv + u * v1.uv + v * v2.uv;

    return hit;
}

Intersection intersectMesh(Ray ray, int meshIndex)
{
    Intersection closestHit;
    closestHit.t = -1.0;

    MeshInstance instance = GetMeshInstance(meshIndex);
    Mesh mesh = GetMesh(instance.meshIndex);

    Ray localRay = toLocalRay(ray, instance.transform);

    for (int i = 0; i < mesh.triangleCount; ++i)
    {
        Triangle tri = GetTriangle(mesh.triangleOffset + i);
        Vertex v0 = GetVertex(mesh.vertexOffset + tri.v0);
        Vertex v1 = GetVertex(mesh.vertexOffset + tri.v1);
        Vertex v2 = GetVertex(mesh.vertexOffset + tri.v2);

        Intersection hit = intersectTriangle(localRay, v0, v1, v2, true, false);
        if (hit.t > INTERSECTION_TEST_EPSILON  && (closestHit.t < 0.0 || hit.t < closestHit.t))
        {
            closestHit = hit;
            closestHit.meshIndex = meshIndex;
            closestHit.primIndex = i;
            closestHit.materialIndex = instance.materialIndex;
        }
    }

    if (closestHit.t > 0.0)
    {
        closestHit.position = toWorldPosition(closestHit.position, instance.transform);
        closestHit.geometricNormal = toWorldNormal(closestHit.geometricNormal, instance.transform);
        closestHit.shadingNormal = toWorldNormal(closestHit.shadingNormal, instance.transform);
        closestHit.t = length(closestHit.position - ray.origin);
    }

    return closestHit;
}

Intersection intersectScene(Ray ray)
{
    Intersection closestHit;
    closestHit.t = -1.0;

    for (int i = 0; i < sceneGlobals.numMeshes; ++i)
    {
        Intersection hit = intersectMesh(ray, i);
        if (hit.t > INTERSECTION_TEST_EPSILON && (closestHit.t < 0.0 || hit.t < closestHit.t))
        {
            closestHit = hit;
        }
    }
    

    return closestHit;
}

bool IntersectAny(Ray ray, float maxDistance)
{
    for (int i = 0; i < sceneGlobals.numMeshes; ++i)
    {
        Intersection hit = intersectMesh(ray, i);
        if (hit.t > 0.0 && hit.t < maxDistance)
        {
            return true;
        }
    }
    return false;
}
#endif // INTERSECTIONS_GLSL