using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Raytracer.Core;

namespace Raytracer.Rendering.Vulkan.Backend.Scenes;

[StructLayout(LayoutKind.Sequential)]
public struct SceneGlobals
{
    public Vector3 AmbientLightColor; public float Padding0;
    public Vector3 BackgroundColor; public float Padding1;
    
    public int NumPointLights; 
    public int NumMeshes;
    public int NumVertices;
    public float _Padding2;
    
    public int NumTriangles;
    public int NumMaterials;
    public int NumMeshInstances;
    public int NumSpheres;

    public override string ToString()
    {
        return $"SceneGlobals(AmbientLightColor={AmbientLightColor}," +
               $" BackgroundColor={BackgroundColor}," +
               $" NumPointLights={NumPointLights}, " +
               $"NumMeshes={NumMeshes}, " +
               $"NumVertices={NumVertices}, " +
               $"NumTriangles={NumTriangles}, " +
               $"NumMaterials={NumMaterials}, " +
               $"NumMeshInstances={NumMeshInstances}), " +
               $"NumSpheres={NumSpheres})";
    }
}