#ifndef LIGHTS_GLSL
#define LIGHTS_GLSL

struct PointLight
{
    vec3 position;
    float _pad0;
    vec3 intensity;
    float _pad1;
};

#endif // LIGHTS_GLSL