using System.Runtime.InteropServices;
using Raytracer.Core;
using Raytracer.Rendering.Vulkan.Backend.Allocations;
using Raytracer.Rendering.Vulkan.Backend.Allocations.BufferObjects;
using Raytracer.Rendering.Vulkan.Backend.Scenes.Objects;

namespace Raytracer.Rendering.Vulkan.Backend.Scenes;

public sealed class SceneResources : IDisposable
{
    public readonly UBO<SceneGlobals> SceneGlobalsUbo;
    public readonly SSBO<CameraGpu> CameraSsbo;
    public readonly SSBO<SphereGPU> SphereSsbo;
    public readonly SSBO<PointLightGPU> PointLightSsbo;
    public readonly SSBO<MeshGPU> MeshSsbo;
    public readonly SSBO<VertexGPU> VertexSsbo;
    public readonly SSBO<TriangleGPU> TriangleSsbo;
    public readonly SSBO<MaterialGPU> MaterialSsbo;
    public readonly SSBO<MeshInstanceGPU> MeshInstanceSsbo;
    public DescriptorSetWrapper SceneDescriptorSet;
    public DescriptorSetWrapper PerFrameDescriptorSet;
    public SceneResources(VkContext ctx)
    {
        SceneGlobalsUbo = new UBO<SceneGlobals>(ctx);
        SphereSsbo = new SSBO<SphereGPU>(ctx, capacity: 10, allowOverflow: false);
        PointLightSsbo = new SSBO<PointLightGPU>(ctx, capacity: 64, allowOverflow: true);
        MeshSsbo = new SSBO<MeshGPU>(ctx, capacity: 32, allowOverflow: false);
        VertexSsbo = new SSBO<VertexGPU>(ctx, capacity: 20_000, allowOverflow: true);
        TriangleSsbo = new SSBO<TriangleGPU>(ctx, capacity: 60_000, allowOverflow: true);
        MaterialSsbo = new SSBO<MaterialGPU>(ctx, capacity: 16, allowOverflow: false);

        SceneDescriptorSet = new DescriptorSetWrapper.Builder().
            UBOCompute(0).  // SceneUBO
            SSBOCompute(1). // SphereSSBO
            SSBOCompute(2). // PointLightSSBO
            SSBOCompute(3). // MeshSSBO
            SSBOCompute(4). // VertexSSBO
            SSBOCompute(5). // TriangleSSBO
            SSBOCompute(6). // MaterialSSBO
            Build(ctx);
        
        SceneDescriptorSet.UpdateUBO(0, SceneGlobalsUbo.DescriptorInfo);
        SceneDescriptorSet.UpdateSSBO(1, SphereSsbo.DescriptorInfo);
        SceneDescriptorSet.UpdateSSBO(2, PointLightSsbo.DescriptorInfo);
        SceneDescriptorSet.UpdateSSBO(3, MeshSsbo.DescriptorInfo);
        SceneDescriptorSet.UpdateSSBO(4, VertexSsbo.DescriptorInfo);
        SceneDescriptorSet.UpdateSSBO(5, TriangleSsbo.DescriptorInfo);
        SceneDescriptorSet.UpdateSSBO(6, MaterialSsbo.DescriptorInfo);

        // -----------------------------
        // Per-frame resources
        // -----------------------------
        
        CameraSsbo = new SSBO<CameraGpu>(ctx, capacity: 1, allowOverflow: false);
        MeshInstanceSsbo = new SSBO<MeshInstanceGPU>(ctx, capacity: 128, allowOverflow: false);
        
        PerFrameDescriptorSet = new DescriptorSetWrapper.Builder().
            SSBOCompute(0). // CameraSSBO
            SSBOCompute(1). // MeshInstanceSSBO
            Build(ctx);
        
        PerFrameDescriptorSet.UpdateSSBO(0, CameraSsbo.DescriptorInfo);
        PerFrameDescriptorSet.UpdateSSBO(1, MeshInstanceSsbo.DescriptorInfo);
    }

    public void Dispose()
    {
        PointLightSsbo.Dispose();
        CameraSsbo.Dispose();
        SceneGlobalsUbo.Dispose();
        SceneDescriptorSet.Dispose();
        PerFrameDescriptorSet.Dispose();
    }
    
    public void SetFromScene(SceneDefinition sceneDef)
    {
        SceneGlobalsUbo.SetData(sceneDef.Globals);
        CameraSsbo.SetData([sceneDef.Camera]);
        PointLightSsbo.SetData(sceneDef.PointLights);
        MeshSsbo.SetData(sceneDef.Meshes);
        VertexSsbo.SetData(sceneDef.Vertices);
        TriangleSsbo.SetData(sceneDef.Triangles);
        MaterialSsbo.SetData(sceneDef.Materials);
        MeshInstanceSsbo.SetData(sceneDef.MeshInstances);
        SphereSsbo.SetData(sceneDef.Spheres);
        Debug.Log(SceneGlobalsUbo.ToString());
        Debug.Log(sceneDef.Camera);
        Debug.Log($"Point light count: {sceneDef.PointLights.Length},[0] => {sceneDef.PointLights[0]}");
        Debug.Log($"Mesh count: {sceneDef.Meshes.Length}, [0] => {sceneDef.Meshes[0]}, [-1] => {sceneDef.Meshes[^1]}");
        Debug.Log($"Vertex count: {sceneDef.Vertices.Length}, [0] => {sceneDef.Vertices[0]}, [-1] => {sceneDef.Vertices[^1]}");
        Debug.Log($"Triangle count: {sceneDef.Triangles.Length}, [0] => {sceneDef.Triangles[0]}, [-1] => {sceneDef.Triangles[^1]}");
        Debug.Log($"Material count: {sceneDef.Materials.Length}, [0] => {sceneDef.Materials[0]}");
        Debug.Log($"MeshInstance count: {sceneDef.MeshInstances.Length}, [0] => {sceneDef.MeshInstances[0]}");
    }
}