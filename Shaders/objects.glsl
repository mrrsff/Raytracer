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
    vec3 position; float _pad0;
    vec3 normal; float _pad1;
    vec2 uv; vec2 _pad2;
};

struct Triangle
{
    int v0; // these are indices of vertices
    int v1;
    int v2;
    int _pad0;
};

struct Mesh
{
    int vertexOffset;
    int vertexCount;
    
    int triangleOffset;
    int triangleCount;
    
    int flags; vec3 _pad0;
};

struct MeshInstance
{
    int meshIndex;
    int materialIndex; vec2 _pad0;
    mat4 transform;
};

#endif // OBJECTS_GLSL