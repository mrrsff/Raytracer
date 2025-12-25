#ifndef SCENE_LAYOUT_GLSL
#define SCENE_LAYOUT_GLSL

struct Camera
{
    vec3 position; float _pad0;
    vec3 forward; float _pad1;
    vec3 right; float _pad2;
    vec3 up; float _pad3;
    float fovY; 
    float aspectRatio;
    vec2 _pad4;    
};

struct Sphere
{
    vec3 center;
    float radius;
};

struct PointLight
{
    vec3 position;
    float intensity;
    vec3 color;
    float radius;
};

struct SceneGlobals
{
    vec3 ambientLight;
    float _pad0;
    int numSpheres;
    int numPointLights;
    int _pad1; int _pad2;
};

struct Material
{
    vec3  albedo;        // 12 bytes
    float roughness;     // 4

    vec3  emission;      // 12
    float metallic;      // 4

    uint  type;          // 4
    uint  _pad0;         // padding
    uint  _pad1;
    uint  _pad2;
};

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
layout(set = 1, binding = 3, std430) readonly buffer MaterialBuffer
{
    Material materials[];
};

layout(set = 2, binding = 0, std430) readonly buffer CameraBuffer
{
    Camera camera;
};

#endif // SCENE_LAYOUT_GLSL