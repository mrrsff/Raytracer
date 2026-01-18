#ifndef INTERSECTIONS_GLSL
#define INTERSECTIONS_GLSL

#include "scene_layout.glsl"
#include "definitions.glsl"
#include "objects.glsl"
#include "transforms.glsl"

struct Intersection
{
    float t;                // ray parameter (distance)
    bool hit;               // did we hit something
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
    hit.hit = false;

    vec3 edge1 = v1.position - v0.position;
    vec3 edge2 = v2.position - v0.position;
    vec3 n = cross(edge1, edge2);
    float a = dot(n, ray.direction);
    if (backfaceCulling && a > 0)
        return hit; // Backface or parallel
    
    vec3 pvec = cross(ray.direction, edge2);
    float det = dot(edge1, pvec);

    if (abs(det) < INTERSECTION_TEST_EPSILON) return hit; // Ray is parallel to triangle
    
    float invDet = 1.0 / det;
    vec3 tvec = ray.origin - v0.position;
    float u = dot(tvec, pvec) * invDet;
    if (u < 0.0 || u > 1.0)
    {
        return hit;
    }

    vec3 qvec = cross(tvec, edge1);
    float v = dot(ray.direction, qvec) * invDet;
    if (v < 0.0 || u + v > 1.0) 
    {
        return hit;
    }

    float tHit = invDet * dot(edge2, qvec);
    if (tHit <= INTERSECTION_TEST_EPSILON)
    {
        return hit; // Triangle is behind ray
    }

    hit.t = tHit;
    hit.hit = true;
    hit.position = ray.origin + ray.direction * tHit;
    float w = 1.0 - u - v;
    if (smoothNormals) {
        hit.shadingNormal = normalize(w * v0.normal + u * v1.normal + v * v2.normal); // barycentric interpolation
    }
    else {
        hit.shadingNormal = normalize(cross(edge1, edge2));
    }
    hit.geometricNormal = hit.shadingNormal;
    hit.uv = w * v0.uv + u * v1.uv + v * v2.uv;

    return hit;
}

Intersection intersectMeshInstance(Ray ray, int instanceIndex)
{
    Intersection closestHit;
    closestHit.t = 1e30;
    closestHit.hit = false;

    MeshInstance instance = GetMeshInstance(instanceIndex);
    Mesh mesh = GetMesh(instance.meshIndex);
    
    Ray localRay = toLocalRay(ray, instance.transform);
    
    for (int i = 0; i < mesh.triangleCount; ++i)
    {
        Triangle tri = GetTriangle(mesh.triangleOffset + i);
        Vertex v0 = GetVertex(mesh.vertexOffset + tri.v0);
        Vertex v1 = GetVertex(mesh.vertexOffset + tri.v1);
        Vertex v2 = GetVertex(mesh.vertexOffset + tri.v2);
        
        Intersection hit = intersectTriangle(localRay, v0, v1, v2, false, false); 
        
        if (hit.hit)
        {
            vec3 worldPos = toWorldPosition(hit.position, instance.transform);
            float worldT = length(worldPos - ray.origin);
            
            if (!closestHit.hit || worldT < closestHit.t)
            {
                closestHit = hit;
                closestHit.t = worldT;
                closestHit.position = worldPos;
                closestHit.meshIndex = instanceIndex;
                closestHit.primIndex = i;
                closestHit.materialIndex = instance.materialIndex;
            }
        }
    }
    
    if (closestHit.hit)
    {
        closestHit.geometricNormal = normalize(toWorldNormal(closestHit.geometricNormal, instance.transform));
        closestHit.shadingNormal = normalize(toWorldNormal(closestHit.shadingNormal, instance.transform));
    }
    
    return closestHit;
}

Intersection intersectScene(Ray ray)
{
    Intersection closestHit;
    closestHit.t = 1e30;
    closestHit.hit = false;

    const int MAX_INSTANCES = 64;
    for (int i = 0; i < MAX_INSTANCES; ++i)
    {
        if (i >= sceneGlobals.numMeshInstances) break;
        Intersection hit = intersectMeshInstance(ray, i);
        if (hit.hit)
        {
            if (!closestHit.hit || hit.t < closestHit.t)
            {
                closestHit = hit;
            }
        }
    }
    
    return closestHit;
}

bool IntersectAny(Ray ray, float maxDistance)
{
    for (int i = 0; i < sceneGlobals.numMeshInstances; ++i)
    {
        Intersection hit = intersectMeshInstance(ray, i);
        if (hit.hit && hit.t < maxDistance)
        {
            return true;
        }
    }
    return false;
}
#endif // INTERSECTIONS_GLSL