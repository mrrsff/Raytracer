using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace Raytracer.Rendering.Vulkan.Backend.Allocations;

public sealed unsafe class VkImage : Allocation
{
    public readonly uint Width;
    public readonly uint Height;
    public readonly Format Format;
    public Extent3D ImageExtent => new(Width, Height, 1);

    public readonly Image Image;
    private readonly ImageView _imageView;
    private ImageLayout _currentLayout;

    public VkImage(VkContext context, uint width, uint height, Format format, ImageUsageFlags imageUsageFlags) : base(context)
    {
        Width = width;
        Height = height;
        Format = format;

        //create image
        var imageInfo = new ImageCreateInfo
        {
            SType = StructureType.ImageCreateInfo,
            ImageType = ImageType.Type2D,
            Extent = new Extent3D(Width, Height, 1),
            Format = Format,
            Samples = SampleCountFlags.Count1Bit,
            SharingMode = SharingMode.Exclusive,
            InitialLayout = ImageLayout.Undefined,
            Tiling = ImageTiling.Optimal,
            Usage = imageUsageFlags,
            MipLevels = 1,
            ArrayLayers = 1
        };
        Vk.CreateImage(Device, imageInfo, null, out Image);
        _currentLayout = ImageLayout.Undefined;

        //create and bind memory
        Vk.GetImageMemoryRequirements(Device, Image, out var memReq);
        Memory = VkContext.AllocateMemory(memReq, MemoryPropertyFlags.DeviceLocalBit);
        Vk.BindImageMemory(Device, Image, Memory, 0);

        //create view
        var viewInfo = new ImageViewCreateInfo
        {
            SType = StructureType.ImageViewCreateInfo,
            Image = Image,
            ViewType = ImageViewType.Type2D,
            Format = Format,
            SubresourceRange =
            {
                AspectMask = ImageAspectFlags.ColorBit,
                BaseMipLevel = 0,
                BaseArrayLayer = 0,
                LevelCount = 1,
                LayerCount = 1
            }
        };
        Vk.CreateImageView(Device, viewInfo, null, out _imageView);
    }

    public DescriptorImageInfo GetImageInfo() => new()
    {
        ImageLayout = ImageLayout.General,
        ImageView = _imageView
    };

    public unsafe void RecordTransition(Vk vk, CommandBuffer cmd, ImageLayout newLayout )
    {
        if (_currentLayout == newLayout)
            return;

        ImageMemoryBarrier barrier = new()
        {
            SType = StructureType.ImageMemoryBarrier,
            OldLayout = _currentLayout,
            NewLayout = newLayout,
            SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
            DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
            Image = Image,
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

        // --- source ---
        switch (_currentLayout)
        {
            case ImageLayout.Undefined:
                barrier.SrcAccessMask = 0;
                srcStage = PipelineStageFlags.TopOfPipeBit;
                break;

            case ImageLayout.General:
                barrier.SrcAccessMask = AccessFlags.ShaderWriteBit;
                srcStage = PipelineStageFlags.ComputeShaderBit;
                break;

            case ImageLayout.TransferSrcOptimal:
                barrier.SrcAccessMask = AccessFlags.TransferReadBit;
                srcStage = PipelineStageFlags.TransferBit;
                break;

            case ImageLayout.TransferDstOptimal:
                barrier.SrcAccessMask = AccessFlags.TransferWriteBit;
                srcStage = PipelineStageFlags.TransferBit;
                break;

            default:
                throw new InvalidOperationException($"Unsupported src layout {_currentLayout}");
        }

        // --- destination ---
        switch (newLayout)
        {
            case ImageLayout.General:
                barrier.DstAccessMask = AccessFlags.ShaderWriteBit;
                dstStage = PipelineStageFlags.ComputeShaderBit;
                break;

            case ImageLayout.TransferSrcOptimal:
                barrier.DstAccessMask = AccessFlags.TransferReadBit;
                dstStage = PipelineStageFlags.TransferBit;
                break;

            case ImageLayout.TransferDstOptimal:
                barrier.DstAccessMask = AccessFlags.TransferWriteBit;
                dstStage = PipelineStageFlags.TransferBit;
                break;

            default:
                throw new InvalidOperationException($"Unsupported dst layout {newLayout}");
        }

        vk.CmdPipelineBarrier(cmd, srcStage, dstStage, 0, 0, null, 0, null, 1, &barrier);

        _currentLayout = newLayout;
    }
    public void TransitionLayout(ImageLayout newLayout)
    {
        var cmd = VkContext.BeginSingleTimeCommands();
        if (_currentLayout == newLayout)
            return;

        ImageMemoryBarrier barrier = new()
        {
            SType = StructureType.ImageMemoryBarrier,
            OldLayout = _currentLayout,
            NewLayout = newLayout,
            SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
            DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
            Image = Image,
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

        switch (_currentLayout)
        {
            case ImageLayout.Undefined:
                barrier.SrcAccessMask = 0;
                srcStage = PipelineStageFlags.TopOfPipeBit;
                break;

            case ImageLayout.General:
                barrier.SrcAccessMask = AccessFlags.ShaderWriteBit;
                srcStage = PipelineStageFlags.ComputeShaderBit;
                break;

            case ImageLayout.TransferSrcOptimal:
                barrier.SrcAccessMask = AccessFlags.TransferReadBit;
                srcStage = PipelineStageFlags.TransferBit;
                break;

            case ImageLayout.TransferDstOptimal:
                barrier.SrcAccessMask = AccessFlags.TransferWriteBit;
                srcStage = PipelineStageFlags.TransferBit;
                break;

            default:
                throw new InvalidOperationException($"Unsupported src layout {_currentLayout}");
        }
        switch (newLayout)
        {
            case ImageLayout.General:
                barrier.DstAccessMask = AccessFlags.ShaderWriteBit;
                dstStage = PipelineStageFlags.ComputeShaderBit;
                break;

            case ImageLayout.TransferSrcOptimal:
                barrier.DstAccessMask = AccessFlags.TransferReadBit;
                dstStage = PipelineStageFlags.TransferBit;
                break;

            case ImageLayout.TransferDstOptimal:
                barrier.DstAccessMask = AccessFlags.TransferWriteBit;
                dstStage = PipelineStageFlags.TransferBit;
                break;

            default:
                throw new InvalidOperationException($"Unsupported dst layout {newLayout}");
        }

        Vk.CmdPipelineBarrier(cmd, srcStage, dstStage, 0, 0, null, 0, null, 1, &barrier);
        VkContext.EndSingleTimeCommands(cmd);
        _currentLayout = newLayout;
    }

    private void CopyToBuffer(Buffer buffer)
    { 
        //check if usable as transfer source -> transition if not
        var tmpLayout = _currentLayout;
        if(_currentLayout != ImageLayout.TransferSrcOptimal)
            TransitionLayout(ImageLayout.TransferSrcOptimal);

        var cmd = VkContext.BeginSingleTimeCommands();
        var layers = new ImageSubresourceLayers(ImageAspectFlags.ColorBit, 0, 0, 1);
        var copyRegion = new BufferImageCopy(0, 0, 0, layers, default, ImageExtent);
        Vk.CmdCopyImageToBuffer(cmd, Image, _currentLayout, buffer, 1, copyRegion);
        VkContext.EndSingleTimeCommands(cmd);

        //transfer back to original layout if changed
        if(_currentLayout != tmpLayout)
            TransitionLayout(ImageLayout.General);
    }

    public void SetData(void* source)
    {
        var size = Width * Height * 4; 
        using var buffer = new VkBuffer(VkContext, size, BufferUsageFlags.TransferSrcBit, MemoryPropertyFlags.HostVisibleBit);

        //copy data using a staging buffer
        void* mappedData = default;
        buffer.MapMemory(ref mappedData);
        System.Buffer.MemoryCopy(source, mappedData, size, size);
        buffer.UnmapMemory();
        var tmpLayout = _currentLayout;
        if(_currentLayout != ImageLayout.TransferDstOptimal)
            TransitionLayout(ImageLayout.TransferDstOptimal);
        buffer.CopyToImage(this);

        if(_currentLayout != tmpLayout)
            TransitionLayout(tmpLayout);
    }

    public void CopyTo(void* destination)
    {
        //this is valid for R8G8B8A8 formats and permutations only
        var size = Width * Height * 4; 
        using var buffer = new VkBuffer(VkContext, size, BufferUsageFlags.TransferDstBit,
                                        MemoryPropertyFlags.HostVisibleBit);
        CopyToBuffer(buffer.Buffer);

        //copy data using a staging buffer
        void* mappedData = default;
        buffer.MapMemory(ref mappedData);
        System.Buffer.MemoryCopy(mappedData, destination, size, size);
        buffer.UnmapMemory();
    }
    
    public void CopyTo(VkImage dst)
    {
        var cmd = VkContext.BeginSingleTimeCommands();

        TransitionLayout(ImageLayout.TransferSrcOptimal);
        dst.TransitionLayout(ImageLayout.TransferDstOptimal);

        var region = new ImageCopy
        {
            SrcSubresource = new ImageSubresourceLayers(ImageAspectFlags.ColorBit, 0, 0, 1),
            DstSubresource = new ImageSubresourceLayers(ImageAspectFlags.ColorBit, 0, 0, 1),
            Extent = ImageExtent
        };

        Vk.CmdCopyImage(
            cmd,
            Image, ImageLayout.TransferSrcOptimal,
            dst.Image, ImageLayout.TransferDstOptimal,
            1, region
        );

        VkContext.EndSingleTimeCommands(cmd);
    }

    public void Save(string destination)
    {
    }

    public override void Dispose()
    {
        Vk.DestroyImageView(Device, _imageView, null);
        Vk.FreeMemory(Device, Memory, null);
        Vk.DestroyImage(Device, Image, null);
    }
}