using Raytracer.Core;
using Raytracer.Rendering.Vulkan.Backend.Scene.Objects;
using Silk.NET.Maths;

namespace Raytracer.Rendering.Vulkan.Backend.Scene;

using System.Runtime.InteropServices;
using Allocations;
using Allocations.BufferObjects;

public sealed class SceneResources : IDisposable
{
    public readonly UBO SceneUbo;
    public readonly SSBO<CameraGpu> CameraSsbo;
    public readonly SSBO<SphereGPU> SphereSsbo;
    public readonly SSBO<PointLightGPU> PointLightSsbo;

    public DescriptorSetWrapper SceneDescriptorSet;
    public DescriptorSetWrapper PerFrameDescriptorSet;
    public SceneResources(SceneDefinition sceneDefinition, VkContext ctx)
    {
        SceneUbo = new UBO(ctx, (uint)Marshal.SizeOf<SceneGlobals>());
        SphereSsbo = new SSBO<SphereGPU>(ctx, capacity: 10, allowOverflow: false);
        PointLightSsbo = new SSBO<PointLightGPU>(ctx, capacity: 64, allowOverflow: true);

        SceneDescriptorSet = new DescriptorSetWrapper.Builder().
            UBOCompute(0).  // SceneUBO
            SSBOCompute(1). // SphereSSBO
            SSBOCompute(2). // PointLightSSBO
            Build(ctx);
        
        SceneDescriptorSet.UpdateUBO(0, SceneUbo.DescriptorInfo);
        SceneDescriptorSet.UpdateSSBO(1, SphereSsbo.DescriptorInfo);
        SceneDescriptorSet.UpdateSSBO(2, PointLightSsbo.DescriptorInfo);

        // -----------------------------
        // Per-frame resources
        // -----------------------------
        
        CameraSsbo = new SSBO<CameraGpu>(ctx, capacity: 1, allowOverflow: false);
        
        PerFrameDescriptorSet = new DescriptorSetWrapper.Builder().SSBOCompute(0).Build(ctx); // Only CameraSSBO (it updates every frame)
        
        PerFrameDescriptorSet.UpdateSSBO(0, CameraSsbo.DescriptorInfo);
    }
    public void UpdateSceneGlobals(in SceneGlobals globals)
    {
        SceneUbo.Update(globals);
    }
    
    public void UpdateSpheres(ReadOnlySpan<SphereGPU> spheres)
    {
        SphereSsbo.SetData(spheres);
    }

    public void UpdateCamera(in CameraGpu camera)
    {
        CameraSsbo.SetData([camera]);
    }

    public void UpdateLights(ReadOnlySpan<PointLightGPU> lights)
    {
        PointLightSsbo.SetData(lights);
    }

    public void Dispose()
    {
        PointLightSsbo.Dispose();
        CameraSsbo.Dispose();
        SceneUbo.Dispose();
        SceneDescriptorSet.Dispose();
        PerFrameDescriptorSet.Dispose();
    }
}