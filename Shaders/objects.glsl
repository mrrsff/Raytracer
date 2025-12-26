#ifndef OBJECTS_GLSL
#define OBJECTS_GLSL

#include "scene_layout.glsl"
#include "definitions.glsl"

struct Sphere
{
    vec3 center;
    float radius;
};

struct Vertex
{
    vec3 position;
    vec3 normal;
    vec2 uv;
};

struct Triangle
{
    int v0; // these are indices of vertices
    int v1;
    int v2;
};

struct Mesh
{
    int vertexOffset;
    int vertexCount;
    
    int triangleOffset;
    int triangleCount;
    
    int flags;
};

struct MeshInstance
{
    int meshIndex;
    int materialIndex;
    vec4 transformRow0;
    vec4 transformRow1;
    vec4 transformRow2;
};
#endif // OBJECTS_GLSL