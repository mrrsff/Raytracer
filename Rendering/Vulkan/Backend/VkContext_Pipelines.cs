using System.Runtime.InteropServices;
using Silk.NET.Vulkan;

namespace Raytracer.Rendering.Vulkan.Backend;

public unsafe partial class VkContext
{
    public ShaderModule LoadShaderModule(string filename)
    {
        var shaderCode = File.ReadAllBytes(filename);
        fixed (byte* pShaderCode = shaderCode)
        {
            var createInfo = new ShaderModuleCreateInfo
            {
                SType = StructureType.ShaderModuleCreateInfo,
                CodeSize = (nuint) shaderCode.Length,
                PCode = (uint*)pShaderCode,
            };
            _vk.CreateShaderModule(_device, createInfo, null, out var module);
            return module;
        }
    }
    public void DestroyShaderModule(ShaderModule shaderModule) => _vk.DestroyShaderModule(_device, shaderModule, null);

    public Pipeline CreateComputePipeline(PipelineLayout layout, ShaderModule shaderModule)
    {
        var entryPoint = "main";
        var pEntryPoint = (byte*)Marshal.StringToHGlobalAnsi(entryPoint);
        var stageInfo = new PipelineShaderStageCreateInfo
        {
            SType = StructureType.PipelineShaderStageCreateInfo,
            Stage = ShaderStageFlags.ComputeBit,
            Module = shaderModule,
            PName = pEntryPoint,
            Flags = PipelineShaderStageCreateFlags.None
        };
        var computeInfo = new ComputePipelineCreateInfo
        {
            SType = StructureType.ComputePipelineCreateInfo,
            Layout = layout,
            Stage = stageInfo
        };
        _vk.CreateComputePipelines(_device, default, 1, computeInfo, null, out var pipeline);
        return pipeline;
    }
    public void DestroyPipeline(Pipeline pipeline) => _vk.DestroyPipeline(_device, pipeline, null);
    public void BindComputePipeline(CommandBuffer cmd, Pipeline pipeline) => _vk.CmdBindPipeline(cmd, PipelineBindPoint.Compute, pipeline);
}