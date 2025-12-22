using Raytracer.Core;

namespace Raytracer.Rendering.Vulkan.Backend.Scene;

using System.Runtime.InteropServices;
using Allocations;
using Allocations.BufferObjects;

public sealed class SceneResources : IDisposable
{
    public readonly UBO SceneUbo;
    public readonly SSBO<CameraGpu> CameraSsbo;
    public readonly SSBO<Vertex> VertexSsbo;
    public readonly SSBO<uint> IndexSsbo;
    public readonly SSBO<PointLight> PointLightSsbo;

    public readonly DescriptorSetWrapper DescriptorSet;

    public SceneResources(SceneDefinition sceneDefinition, VkContext ctx)
    {
        SceneUbo = new UBO(ctx, (ulong)Marshal.SizeOf<SceneGlobals>());

        CameraSsbo = new SSBO<CameraGpu>(ctx, capacity: 1, allowOverflow: false);

        VertexSsbo = new SSBO<Vertex>(ctx, capacity: 100_000, allowOverflow: true);

        IndexSsbo = new SSBO<uint>(ctx, capacity: 300_000, allowOverflow: true);

        PointLightSsbo = new SSBO<PointLight>(ctx, capacity: 64, allowOverflow: true);

        DescriptorSet = new DescriptorSetWrapper.Builder()
            .UBOCompute(0) // SceneGlobals
            .SSBOCompute(1) // Camera
            // .SSBOCompute(2) // Vertices
            // .SSBOCompute(3) // Indices
            // .SSBOCompute(4) // Lights
            .Build(ctx);
        
        DescriptorSet.UpdateUBO(0, SceneUbo.DescriptorInfo);
        DescriptorSet.UpdateSSBO(1, CameraSsbo.DescriptorInfo);
        // DescriptorSet.UpdateSSBO(2, VertexSsbo.DescriptorInfo);
        // DescriptorSet.UpdateSSBO(3, IndexSsbo.DescriptorInfo);
        // DescriptorSet.UpdateSSBO(4, PointLightSsbo.DescriptorInfo);
    }

    public void UpdateSceneGlobals(in SceneGlobals globals)
    {
        SceneUbo.Update(globals);
    }

    public void UpdateCamera(in CameraGpu camera)
    {
        CameraSsbo.SetData([camera]);
    }

    public void UpdateMeshes(ReadOnlySpan<Vertex> vertices, ReadOnlySpan<uint> indices)
    {
        VertexSsbo.SetData(vertices);
        IndexSsbo.SetData(indices);
    }

    public void UpdateLights(ReadOnlySpan<PointLight> lights)
    {
        PointLightSsbo.SetData(lights);
    }

    public void Dispose()
    {
        PointLightSsbo.Dispose();
        IndexSsbo.Dispose();
        VertexSsbo.Dispose();
        CameraSsbo.Dispose();
        SceneUbo.Dispose();
        DescriptorSet.Dispose();
    }
}