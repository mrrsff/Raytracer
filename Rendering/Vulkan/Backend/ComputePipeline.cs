using System.Runtime.CompilerServices;
using Raytracer.Rendering.Vulkan.Backend.Allocations;
using Raytracer.Rendering.Vulkan.Backend.Commands;
using Raytracer.Rendering.Vulkan.Backend.Pipelines;
using Silk.NET.Vulkan;

namespace Raytracer.Rendering.Vulkan.Backend;

public sealed unsafe class ComputePipeline : IDisposable
{
    public VkContext _context { get; private set; }

    private DescriptorSetWrapper _sceneSet;
    private DescriptorSetWrapper _imageSet;
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

    public ComputePipeline(VkContext context, string shaderPath, VkImage outputImage, DescriptorSetWrapper sceneSet)
    {
        _imageSet = new DescriptorSetWrapper.Builder().ComputeStorageImage(0).Build(context);
        _sceneSet = sceneSet;
        _context = context;
        _outputImage = outputImage;
        
        var pushRanges = new[] { new PushConstantRange { StageFlags = ShaderStageFlags.ComputeBit, Offset = 0, Size = (uint)Unsafe.SizeOf<PushConstants>() } };

        _pipelineLayout = new PipelineLayoutWrapper(_context, [_imageSet.Layout, _sceneSet.Layout], pushRanges);

        _shaderModule = _context.LoadShaderModule(shaderPath);
        _pipeline = _context.CreateComputePipeline(_pipelineLayout.Handle, _shaderModule);

        UpdateOutputImage(outputImage);
    }
    public void Record(CommandRecorder cmdRecorder, uint width, uint height)
    {
        cmdRecorder.ImageBarrier(_outputImage.Image, ImageLayout.Undefined, ImageLayout.General, 0, AccessFlags.ShaderWriteBit, PipelineStageFlags.TopOfPipeBit, PipelineStageFlags.ComputeShaderBit);

        cmdRecorder.BindComputePipeline(_pipeline);
        cmdRecorder.BindDescriptorSet(_pipelineLayout.Handle, _imageSet.Handle);
        cmdRecorder.BindDescriptorSet(_pipelineLayout.Handle, _sceneSet.Handle, 1);

        PushConstants pc = new()
        {
            Time = TimeManager.TotalTime
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

        _imageSet.UpdateStorageImage(0, imageInfo);
    }
    public void Dispose()
    {
        _context.DestroyPipeline(_pipeline);
        _context.DestroyShaderModule(_shaderModule);

        _pipelineLayout.Dispose();
        _sceneSet.Dispose();
    }

}