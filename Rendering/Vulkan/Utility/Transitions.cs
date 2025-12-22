using Raytracer.Rendering.Vulkan.Backend;
using Silk.NET.Vulkan;

namespace Raytracer.Rendering.Vulkan.Utility;

public static class Transitions
{
    public static unsafe void TransitionSwapchainImage(VkContext ctx, CommandBuffer cmd, Image image, ImageLayout oldLayout, ImageLayout newLayout)
    {
        ImageMemoryBarrier barrier = new()
        {
            SType = StructureType.ImageMemoryBarrier,
            OldLayout = oldLayout,
            NewLayout = newLayout,
            SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
            DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
            Image = image,
            SubresourceRange = new ImageSubresourceRange
            {
                AspectMask = ImageAspectFlags.ColorBit,
                BaseMipLevel = 0,
                LevelCount = 1,
                BaseArrayLayer = 0,
                LayerCount = 1
            }
        };

        PipelineStageFlags srcStage;
        PipelineStageFlags dstStage;
        
        switch (oldLayout)
        {
            case ImageLayout.Undefined when
                newLayout == ImageLayout.TransferDstOptimal:
                barrier.SrcAccessMask = 0;
                barrier.DstAccessMask = AccessFlags.TransferWriteBit;

                srcStage = PipelineStageFlags.TopOfPipeBit;
                dstStage = PipelineStageFlags.TransferBit;
                break;
            // --- PRESENT → TRANSFER_DST ---
            case ImageLayout.PresentSrcKhr when
                newLayout == ImageLayout.TransferDstOptimal:
                barrier.SrcAccessMask = AccessFlags.MemoryReadBit;
                barrier.DstAccessMask = AccessFlags.TransferWriteBit;

                srcStage = PipelineStageFlags.BottomOfPipeBit;
                dstStage = PipelineStageFlags.TransferBit;
                break;
            // --- TRANSFER_DST → PRESENT ---
            case ImageLayout.TransferDstOptimal when
                newLayout == ImageLayout.PresentSrcKhr:
                barrier.SrcAccessMask = AccessFlags.TransferWriteBit;
                barrier.DstAccessMask = AccessFlags.MemoryReadBit;

                srcStage = PipelineStageFlags.TransferBit;
                dstStage = PipelineStageFlags.BottomOfPipeBit;
                break;
            default:
                throw new NotSupportedException(
                    $"Unsupported layout transition {oldLayout} → {newLayout}");
        }

        ctx.Vk.CmdPipelineBarrier(cmd, srcStage, dstStage, 0, 0, null, 0, null, 1, &barrier);
    }
}