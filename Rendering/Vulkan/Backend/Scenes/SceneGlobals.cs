using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Raytracer.Core;

namespace Raytracer.Rendering.Vulkan.Backend.Scenes;

[StructLayout(LayoutKind.Sequential)]
public struct SceneGlobals
{
    public Vector3 AmbientLightColor; public float Padding0;
    
    public int NumPointLights; 
    public int NumMeshes;
    public int NumVertices;
    
    public int NumTriangles;
    public int NumMaterials;
    public int NumMeshInstances;
    public float _Padding2;

    public override string ToString()
    {
        return $"SceneGlobals(AmbientLightColor={AmbientLightColor}, NumPointLights={NumPointLights}, NumMeshes={NumMeshes}, NumVertices={NumVertices}, NumTriangles={NumTriangles}, NumMaterials={NumMaterials}, NumMeshInstances={NumMeshInstances})";
    }
}