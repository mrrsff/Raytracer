#ifndef DEFINITIONS_GLSL
#define DEFINITIONS_GLSL

struct Camera
{
    vec3 position; float _pad0;
    vec3 forward; float _pad1;
    vec3 right; float _pad2; 
    vec3 up; float _pad3;
    float fovY;
    float aspectRatio; vec2 _pad4;
};

struct Material
{
    uint type;
    float roughness;
    float phongExponent;
    float absorptionIndex;

    vec4 ambientReflectance;
    vec4 diffuseReflectance;
    vec4 specularReflectance;
    vec4 mirrorReflectance;
    vec4 absorptionCoefficient;

    float refractionIndex;
    float _pad0;
    float _pad1;
    float _pad2;
};

struct SceneGlobals
{
    vec3 ambientLight;
    float _pad0;
    
    vec3 backgroundColor;
    float _pad1;
    
    int numPointLights;
    int numMeshes;
    int numVertices;
    float _pad2;
    
    int numTriangles;
    int numMaterials;
    int numMeshInstances;
    int numSpheres;
};

struct Ray
{
    vec3 origin;
    vec3 direction;
};

Ray generateCameraRay(int pixelX, int pixelY, int imageWidth, int imageHeight, Camera cam)
{
    float imageAspectRatio = float(imageWidth) / float(imageHeight);
    float u = (float(pixelX) + 0.5) / float(imageWidth);
    float v = (float(pixelY) + 0.5) / float(imageHeight);

    float fovY = cam.fovY > 0 ? cam.fovY : 60.0;
    float tanFovY = tan(radians(fovY) / 2.0);
    float Px = (2.0 * u - 1.0) * tanFovY * imageAspectRatio;
    float Py = (1.0 - 2.0 * v) * tanFovY;

    Ray ray;
    ray.origin = cam.position;
    ray.direction = normalize(Px * cam.right + Py * cam.up + cam.forward);

    return ray;
}

#endif // DEFINITIONS_GLSL