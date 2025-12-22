using System.Runtime.CompilerServices;
using Raytracer.Rendering.Vulkan.Backend.Allocations;
using Raytracer.Rendering.Vulkan.Backend.Commands;
using Raytracer.Rendering.Vulkan.Backend.Pipelines;
using Silk.NET.Vulkan;

namespace Raytracer.Rendering.Vulkan.Backend;

public sealed unsafe class ComputePipeline : IDisposable
{
    public VkContext _context { get; private set; }

    private DescriptorPool _descriptorPool;
    private DescriptorSetLayout _setLayout;
    private DescriptorSetWrapper _descriptorSetWrapper;
    private ShaderModule _shaderModule;
    private PipelineLayoutWrapper _pipelineLayout;
    private Pipeline _pipeline;
    
    private VkImage _outputImage;

    private struct PushConstants
    {
        public float Time;
        public int Width;
        public int Height;
    }

    public ComputePipeline(VkContext context, string shaderPath, VkImage outputImage)
    {
        _context = context;
        _outputImage = outputImage;
        
        _descriptorSetWrapper = new DescriptorSetWrapper(_context, [new DescriptorBinding { Binding = 0, Type = DescriptorType.StorageImage, Stages = ShaderStageFlags.ComputeBit }]);

        var pushRanges = new[] { new PushConstantRange { StageFlags = ShaderStageFlags.ComputeBit, Offset = 0, Size = (uint)Unsafe.SizeOf<PushConstants>() } };

        _pipelineLayout = new PipelineLayoutWrapper(_context, [_descriptorSetWrapper.Layout], pushRanges);

        _shaderModule = _context.LoadShaderModule(shaderPath);
        _pipeline = _context.CreateComputePipeline(_pipelineLayout.Handle, _shaderModule);

        UpdateOutputImage(outputImage);
    }
    public void Record(CommandRecorder cmdRecorder, uint width, uint height)
    {
        cmdRecorder.ImageBarrier(_outputImage.Image, ImageLayout.Undefined, ImageLayout.General, 0, AccessFlags.ShaderWriteBit, PipelineStageFlags.TopOfPipeBit, PipelineStageFlags.ComputeShaderBit);

        cmdRecorder.BindComputePipeline(_pipeline);
        cmdRecorder.BindDescriptorSet(_pipelineLayout.Handle, _descriptorSetWrapper.Handle);

        PushConstants pc = new()
        {
            Time = TimeManager.TotalTime,
            Width = (int)width,
            Height = (int)height
        };

        cmdRecorder.PushConstants(_pipelineLayout.Handle, ShaderStageFlags.ComputeBit, pc);

        uint gx = (width + 7) / 8;
        uint gy = (height + 7) / 8;

        cmdRecorder.Dispatch(gx, gy);
    }
    
    public void UpdateOutputImage(VkImage image)
    {
        _outputImage = image;

        var imageInfo = image.GetImageInfo();

        _descriptorSetWrapper.UpdateStorageImage(
            binding: 0,
            imageInfo
        );
    }
    public void Dispose()
    {
        _context.DestroyPipeline(_pipeline);
        _context.DestroyShaderModule(_shaderModule);

        _pipelineLayout.Dispose();
        _descriptorSetWrapper.Dispose();
    }

}