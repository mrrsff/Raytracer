using Silk.NET.Vulkan;

namespace Raytracer.Rendering.Vulkan.Backend.Pipelines;

public unsafe class PipelineLayoutWrapper : IDisposable
{
    private readonly VkContext _ctx;
    private readonly Vk _vk;
    private readonly Device _device;

    public PipelineLayout Handle;

    public PipelineLayoutWrapper(VkContext ctx, DescriptorSetLayout[] setLayouts, PushConstantRange[]? pushConstants = null)
    {
        _ctx = ctx;
        _vk = ctx.Vk;
        _device = ctx.Device;

        Create(setLayouts, pushConstants);
    }

    private void Create(DescriptorSetLayout[] setLayouts, PushConstantRange[]? pushConstants)
    {
        fixed (DescriptorSetLayout* pSetLayouts = setLayouts)
        fixed (PushConstantRange* pPush = pushConstants)
        {
            var info = new PipelineLayoutCreateInfo
            {
                SType = StructureType.PipelineLayoutCreateInfo,
                SetLayoutCount = (uint)setLayouts.Length,
                PSetLayouts = pSetLayouts,
                PushConstantRangeCount = pushConstants != null ? (uint)pushConstants.Length : 0,
                PPushConstantRanges = pushConstants != null ? pPush : null
            };

            if (_vk.CreatePipelineLayout(_device, info, null, out Handle) != Result.Success)
                throw new Exception("Failed to create pipeline layout");
        }
    }

    public void Dispose()
    {
        if (Handle.Handle != 0)
            _vk.DestroyPipelineLayout(_device, Handle, null);
    }
}