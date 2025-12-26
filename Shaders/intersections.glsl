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
    
//    // backface culling
//    if (backfaceCulling) {
//        if (a < 1e-8)
//            return hit;
//    }
//    else {
//        if (abs(a) < 1e-8)
//            return hit; // Ray is parallel to triangle
//    }
    
    if (abs(a) < 1e-8)
    return hit; // Ray is parallel to triangle

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
    if (tHit < 0.0)
        return hit; // Triangle is behind ray

    hit.t = tHit;
    hit.position = ray.origin + ray.direction * tHit;
    hit.geometricNormal = normalize(cross(edge1, edge2));
    if (smoothNormals) {
        hit.shadingNormal = normalize((1.0 - u - v) * v0.normal + u * v1.normal + v * v2.normal); // barycentric interpolation   
    }
    else {
        hit.shadingNormal = hit.geometricNormal;
    }
    hit.uv = (1.0 - u - v) * v0.uv + u * v1.uv + v * v2.uv;

    return hit;
}

//Intersection intersectMesh(Ray ray, int meshIndex)
//{
//    Intersection closestHit;
//    closestHit.t = -1.0;
//
//    MeshInstance instance = GetMeshInstance(meshIndex);
//    Mesh mesh = GetMesh(0);
//
//    mat4x3 worldFromLocal = mat4x3(instance.transformRow0, instance.transformRow1, instance.transformRow2);
//    Ray localRay = ray;
//
//    for (int i = 0; i < mesh.triangleCount; ++i)
//    {
//        Triangle tri = GetTriangle(mesh.triangleOffset + i);
//        Vertex v0 = GetVertex(mesh.vertexOffset + tri.v0);
//        Vertex v1 = GetVertex(mesh.vertexOffset + tri.v1);
//        Vertex v2 = GetVertex(mesh.vertexOffset + tri.v2);
//
//        Intersection hit = intersectTriangle(ray, v0, v1, v2, false, true);
//        if (hit.t > 0.0 && (closestHit.t < 0.0 || hit.t < closestHit.t))
//        {
//            closestHit = hit;
//            closestHit.meshIndex = meshIndex;
//            closestHit.primIndex = i;
//            closestHit.materialIndex = instance.materialIndex;
//        }
//    }
//
//    if (closestHit.t > 0.0)
//    {
////        float worldT = length(toWorldPosition(localRay.origin + localRay.direction * closestHit.t, worldFromLocal) - ray.origin);
////        closestHit.t = worldT;
////        closestHit.position = toWorldPosition(closestHit.position, worldFromLocal);
////        closestHit.geometricNormal = toWorldNormal(closestHit.geometricNormal, worldFromLocal);
////        closestHit.shadingNormal = toWorldNormal(closestHit.shadingNormal, worldFromLocal);
//    }
//
//    return closestHit;
//}

Intersection intersectMesh(Ray ray, int meshIndex)
{
    Intersection closestHit;
    closestHit.t = -1.0;

    Mesh mesh = GetMesh(meshIndex);

    for (int i = 0; i < mesh.triangleCount; ++i)
    {
        Triangle tri = GetTriangle(mesh.triangleOffset + i);
        Vertex v0 = GetVertex(mesh.vertexOffset + tri.v0);
        Vertex v1 = GetVertex(mesh.vertexOffset + tri.v1);
        Vertex v2 = GetVertex(mesh.vertexOffset + tri.v2);

        Intersection hit = intersectTriangle(ray, v0, v1, v2, false, false);
        if (hit.t > 0.0)
        {
            return hit;
        }
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
        if (hit.t > 0.0 && (closestHit.t < 0.0 || hit.t < closestHit.t))
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