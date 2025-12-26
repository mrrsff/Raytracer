#ifndef SCENE_LAYOUT_GLSL
#define SCENE_LAYOUT_GLSL

#define INTERSECTION_TEST_EPSILON 1e-6
#define SHADOW_BIAS 1e-3

#extension GL_EXT_scalar_block_layout : enable

#include "definitions.glsl"
#include "objects.glsl"
#include "lights.glsl"

layout(set = 1, binding = 0, std430) readonly uniform SceneGlobalsBuffer
{
    SceneGlobals sceneGlobals;
};
layout(set = 1, binding = 1, std430) readonly buffer SphereBuffer
{
    Sphere spheres[];
};
layout(set = 1, binding = 2, std430) readonly buffer PointLightBuffer
{
    PointLight pointLights[];
};
layout(set = 1, binding = 3, std430) readonly buffer MeshBuffer
{
    Mesh meshes[];
};
layout(set = 1, binding = 4, std430) readonly buffer VertexBuffer
{
    Vertex vertices[];
};
layout(set = 1, binding = 5, std430) readonly buffer TriangleBuffer
{
    Triangle triangles[];
};
layout(set = 1, binding = 6, std430) readonly buffer MaterialBuffer
{
    Material materials[];
};

layout(set = 2, binding = 0, std430) readonly buffer CameraBuffer
{
    Camera camera;
};
layout(set = 2, binding = 1, std430) readonly buffer MeshInstancesBuffer
{
    MeshInstance meshInstances[];
};

Vertex GetVertex(int index)
{
    return vertices[index];
}

Triangle GetTriangle(int index)
{
    return triangles[index];
}

Mesh GetMesh(int index)
{
    return meshes[index];
}

Material GetMaterial(int index)
{
    return materials[index - 1];
}

MeshInstance GetMeshInstance(int index)
{
    return meshInstances[index];
}

#endif // SCENE_LAYOUT_GLSL