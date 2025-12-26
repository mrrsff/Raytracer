#ifndef DEBUG_GLSL
#define DEBUG_GLSL

vec3 visualizeVec3(vec3 vector)
{
    return vector * 0.5 + 0.5;
}
vec3 visalizeVe2(vec2 uv)
{
    return vec3(uv, 0.0);
}
vec3 visualizeFloat(float value, float minValue, float maxValue)
{
    float normalized = (value - minValue) / (maxValue - minValue);
    return vec3(normalized, normalized, normalized);
}

vec3 visualizeFloat(float value)
{
    return vec3(value);
}
vec3 visualizeBool(bool value)
{
    return value ? vec3(1.0, 1.0, 1.0) : vec3(0.0, 0.0, 0.0);
}

#endif // DEBUG_GLSL