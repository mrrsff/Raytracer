using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace Raytracer.Rendering.Vulkan.Backend.Commands;

public sealed unsafe class CommandRecorder
{
    private readonly VkContext _ctx;
    private readonly Vk _vk;

    public CommandBuffer Cmd { get; private set; }
    private bool _recording;

    public CommandRecorder(VkContext ctx, CommandBuffer cmd)
    {
        _ctx = ctx;
        _vk = ctx.Vk;
        Cmd = cmd;
    }

    public void Begin()
    {
        if (_recording)
            throw new InvalidOperationException("Command buffer already recording");

        var beginInfo = new CommandBufferBeginInfo
        {
            SType = StructureType.CommandBufferBeginInfo,
            Flags = CommandBufferUsageFlags.OneTimeSubmitBit
        };

        _vk.BeginCommandBuffer(Cmd, beginInfo);
        _recording = true;
    }

    public void End()
    {
        if (!_recording)
            throw new InvalidOperationException("Command buffer not recording");

        _vk.EndCommandBuffer(Cmd);
        _recording = false;
    }
    public void BindComputePipeline(Pipeline pipeline)
    {
        EnsureRecording();
        _vk.CmdBindPipeline(Cmd, PipelineBindPoint.Compute, pipeline);
    }

    public void BindDescriptorSet(PipelineLayout layout, DescriptorSet descriptorSet, uint setIndex = 0)
    {
        EnsureRecording();
        _vk.CmdBindDescriptorSets(Cmd, PipelineBindPoint.Compute, layout, setIndex, 1, descriptorSet, 0, null);
    }

    public void PushConstants<T>(PipelineLayout layout, ShaderStageFlags stages, in T data) where T : unmanaged
    {
        EnsureRecording();

        fixed (T* pData = &data)
        {
            _vk.CmdPushConstants(Cmd, layout, stages, 0, (uint)sizeof(T), pData);
        }
    }

    public void Dispatch(uint groupX, uint groupY, uint groupZ = 1)
    {
        EnsureRecording();
        _vk.CmdDispatch(Cmd, groupX, groupY, groupZ);
    }

    public void BufferBarrier(Buffer buffer, AccessFlags srcAccess, AccessFlags dstAccess, PipelineStageFlags srcStage, PipelineStageFlags dstStage)
    {
        EnsureRecording();

        var barrier = new BufferMemoryBarrier
        {
            SType = StructureType.BufferMemoryBarrier,
            SrcAccessMask = srcAccess,
            DstAccessMask = dstAccess,
            SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
            DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
            Buffer = buffer,
            Offset = 0,
            Size = Vk.WholeSize
        };

        _vk.CmdPipelineBarrier(Cmd, srcStage, dstStage, 0, 0, null, 1, barrier, 0, null);
    }

    public void ImageBarrier(
        Image image,
        ImageLayout oldLayout,
        ImageLayout newLayout,
        AccessFlags srcAccess,
        AccessFlags dstAccess,
        PipelineStageFlags srcStage,
        PipelineStageFlags dstStage,
        ImageAspectFlags aspect = ImageAspectFlags.ColorBit
    )
    {
        EnsureRecording();

        var barrier = new ImageMemoryBarrier
        {
            SType = StructureType.ImageMemoryBarrier,
            OldLayout = oldLayout,
            NewLayout = newLayout,
            SrcAccessMask = srcAccess,
            DstAccessMask = dstAccess,
            SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
            DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
            Image = image,
            SubresourceRange = new ImageSubresourceRange
            {
                AspectMask = aspect,
                BaseMipLevel = 0,
                LevelCount = 1,
                BaseArrayLayer = 0,
                LayerCount = 1
            }
        };

        _vk.CmdPipelineBarrier(Cmd, srcStage, dstStage, 0, 0, null, 0, null, 1, barrier);
    }

    private void EnsureRecording()
    {
        if (!_recording)
            throw new InvalidOperationException("Command buffer is not recording");
    }
}
