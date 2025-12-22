using Raytracer.Rendering.Vulkan.Backend.Allocations;
using Raytracer.Rendering.Vulkan.Backend.Commands;
using Raytracer.Rendering.Vulkan.Utility;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Semaphore = Silk.NET.Vulkan.Semaphore;

namespace Raytracer.Rendering.Vulkan.Backend;

public sealed unsafe class SwapchainPresenter : IDisposable
{
    private readonly VkContext _ctx;
    private readonly VKWindow _vkWindow;

    private readonly KhrSurface _khrSurface;
    private readonly SurfaceKHR _surfaceKHR;
    private KhrSwapchain _khrSwapchain;

    private SwapchainKHR _swapchain;
    private Image[] _images;
    private ImageView[] _imageViews;

    private CommandBuffer[] _cmds;

    private Semaphore _imageAvailable;
    private Semaphore[] _renderFinished;
    private Fence _inFlight;

    private ImageLayout[] _swapchainLayouts;
    public SwapchainPresenter(VkContext ctx, VKWindow vkWindow)
    {
        _ctx = ctx;
        _khrSurface = ctx.KhrSurface;
        _surfaceKHR = ctx.Surface;
        _vkWindow = vkWindow;
        CreateSwapchain();
        _swapchainLayouts = new ImageLayout[_images.Length];
        for (int i = 0; i < _swapchainLayouts.Length; i++)
            _swapchainLayouts[i] = ImageLayout.Undefined;
    }
    public void Present(Action<CommandRecorder> record, VkImage computeImage)
    {
        if (!BeginFrame(out uint imageIndex))
            return;

        RecordFrame(imageIndex, record, computeImage);
        SubmitAndPresent(imageIndex);
    }

    private bool BeginFrame(out uint imageIndex)
    {
        _ctx.Vk.WaitForFences(_ctx.Device, 1, _inFlight, true, ulong.MaxValue);
        _ctx.Vk.ResetFences(_ctx.Device, 1, _inFlight);

        fixed (uint* pImageIndex = &imageIndex)
        {
            Result res = _khrSwapchain.AcquireNextImage(
                _ctx.Device,
                _swapchain,
                ulong.MaxValue,
                _imageAvailable,
                default,
                pImageIndex
            );
            
            return res != Result.ErrorOutOfDateKhr && res != Result.SuboptimalKhr;
        }
    }
    private void RecordFrame(uint imageIndex, Action<CommandRecorder> record, VkImage computeImage)
    {
        CommandBuffer cmd = _cmds[imageIndex];
        _ctx.Vk.ResetCommandBuffer(cmd, 0);

        var recorder = new CommandRecorder(_ctx, cmd);
        recorder.Begin();

        // user compute pass
        record(recorder);

        // copy compute → swapchain
        RecordCopyToSwapchain(cmd, imageIndex, computeImage);

        recorder.End();
    }
    private void RecordCopyToSwapchain(CommandBuffer cmd, uint imageIndex, VkImage computeImage)
    {
        computeImage.RecordTransition(_ctx.Vk, cmd, ImageLayout.TransferSrcOptimal);

        Transitions.TransitionSwapchainImage(_ctx, cmd, _images[imageIndex], ImageLayout.Undefined, ImageLayout.TransferDstOptimal);

        var region = new ImageCopy
        {
            SrcSubresource = new ImageSubresourceLayers(ImageAspectFlags.ColorBit, 0, 0, 1),
            DstSubresource = new ImageSubresourceLayers(ImageAspectFlags.ColorBit, 0, 0, 1),
            Extent = new Extent3D(computeImage.Width, computeImage.Height, 1)
        };

        _ctx.Vk.CmdCopyImage(cmd, computeImage.Image, ImageLayout.TransferSrcOptimal, _images[imageIndex], ImageLayout.TransferDstOptimal, 1, &region);

        computeImage.RecordTransition(_ctx.Vk, cmd, ImageLayout.General);

        Transitions.TransitionSwapchainImage(_ctx, cmd, _images[imageIndex], ImageLayout.TransferDstOptimal, ImageLayout.PresentSrcKhr);
    }
    private void SubmitAndPresent(uint imageIndex)
    {
        CommandBuffer cmd = _cmds[imageIndex];
        PipelineStageFlags waitStage = PipelineStageFlags.TransferBit;

        fixed (Semaphore* wait = &_imageAvailable)
        fixed (Semaphore* signal = &_renderFinished[imageIndex])
        {
            var submit = new SubmitInfo
            {
                SType = StructureType.SubmitInfo,
                WaitSemaphoreCount = 1,
                PWaitSemaphores = wait,
                PWaitDstStageMask = &waitStage,
                CommandBufferCount = 1,
                PCommandBuffers = &cmd,
                SignalSemaphoreCount = 1,
                PSignalSemaphores = signal
            };

            _ctx.Vk.QueueSubmit(_ctx.MainQueue, 1, &submit, _inFlight);
        }

        fixed (SwapchainKHR* swap = &_swapchain)
        fixed (Semaphore* wait = &_renderFinished[imageIndex])
        {
            var present = new PresentInfoKHR
            {
                SType = StructureType.PresentInfoKhr,
                WaitSemaphoreCount = 1,
                PWaitSemaphores = wait,
                SwapchainCount = 1,
                PSwapchains = swap,
                PImageIndices = &imageIndex
            };

            _khrSwapchain.QueuePresent(_ctx.MainQueue, &present);
        }
    }
    private void CreateSwapchain()
    {
        _khrSwapchain = new KhrSwapchain(_ctx.Vk.Context);
        QuerySurfaceInfo(out var caps, out var format, out var extent, out var imageCount);

        CreateSwapchainKHR(caps, format, extent, imageCount);
        CreateSwapchainImages();
        CreateImageViews(format.Format);
        CreateFrameResources();
    }
    private void QuerySurfaceInfo(out SurfaceCapabilitiesKHR caps, out SurfaceFormatKHR chosenFormat, out Extent2D extent, out uint imageCount)
    {
        _khrSurface.GetPhysicalDeviceSurfaceCapabilities(_ctx.PhysicalDevice, _surfaceKHR, out caps);

        uint formatCount = 0;
        _khrSurface.GetPhysicalDeviceSurfaceFormats(_ctx.PhysicalDevice, _surfaceKHR, &formatCount, null);

        var formats = new SurfaceFormatKHR[formatCount];
        fixed (SurfaceFormatKHR* pFormats = formats)
            _khrSurface.GetPhysicalDeviceSurfaceFormats(_ctx.PhysicalDevice, _surfaceKHR, ref formatCount, pFormats);

        chosenFormat = formats.First(f => f.Format == Format.B8G8R8A8Unorm && f.ColorSpace == ColorSpaceKHR.PaceSrgbNonlinearKhr);

        extent = caps.CurrentExtent.Width != uint.MaxValue
            ? caps.CurrentExtent
            : new Extent2D((uint)_vkWindow._window.FramebufferSize.X, (uint)_vkWindow._window.FramebufferSize.Y);

        imageCount = caps.MinImageCount + 1;
        if (caps.MaxImageCount > 0 && imageCount > caps.MaxImageCount)
            imageCount = caps.MaxImageCount;
    }
    private void CreateSwapchainKHR(SurfaceCapabilitiesKHR caps, SurfaceFormatKHR format, Extent2D extent, uint imageCount)
    {
        var createInfo = new SwapchainCreateInfoKHR
        {
            SType = StructureType.SwapchainCreateInfoKhr,
            Surface = _surfaceKHR,
            MinImageCount = imageCount,
            ImageFormat = format.Format,
            ImageColorSpace = format.ColorSpace,
            ImageExtent = extent,
            ImageArrayLayers = 1,
            ImageUsage = ImageUsageFlags.TransferDstBit | ImageUsageFlags.ColorAttachmentBit,
            ImageSharingMode = SharingMode.Exclusive,
            PreTransform = caps.CurrentTransform,
            CompositeAlpha = CompositeAlphaFlagsKHR.OpaqueBitKhr,
            PresentMode = PresentModeKHR.FifoKhr,
            Clipped = true
        };

        _khrSwapchain.CreateSwapchain(_ctx.Device, createInfo, null, out _swapchain);
    }
    private void CreateSwapchainImages()
    {
        uint count = 0;
        _khrSwapchain.GetSwapchainImages(_ctx.Device, _swapchain, &count, null);
        _images = new Image[count];
        fixed (Image* pImages = _images)
            _khrSwapchain.GetSwapchainImages(_ctx.Device, _swapchain, ref count, pImages);
    }
    private void CreateImageViews(Format format)
    {
        _imageViews = new ImageView[_images.Length];

        for (int i = 0; i < _images.Length; i++)
        {
            var viewInfo = new ImageViewCreateInfo
            {
                SType = StructureType.ImageViewCreateInfo,
                Image = _images[i],
                ViewType = ImageViewType.Type2D,
                Format = format,
                SubresourceRange = new ImageSubresourceRange
                {
                    AspectMask = ImageAspectFlags.ColorBit,
                    BaseMipLevel = 0,
                    LevelCount = 1,
                    BaseArrayLayer = 0,
                    LayerCount = 1
                }
            };

            _ctx.Vk.CreateImageView(_ctx.Device, viewInfo, null, out _imageViews[i]);
        }
    }
    private void CreateFrameResources()
    {
        _cmds = new CommandBuffer[_images.Length];
        var allocInfo = new CommandBufferAllocateInfo
        {
            SType = StructureType.CommandBufferAllocateInfo,
            CommandPool = _ctx.CommandPool,
            Level = CommandBufferLevel.Primary,
            CommandBufferCount = (uint)_cmds.Length
        };

        fixed (CommandBuffer* pCmds = _cmds)
            _ctx.Vk.AllocateCommandBuffers(_ctx.Device, allocInfo, pCmds);
        
        var semInfo = new SemaphoreCreateInfo { SType = StructureType.SemaphoreCreateInfo };
        var fenceInfo = new FenceCreateInfo { SType = StructureType.FenceCreateInfo, Flags = FenceCreateFlags.SignaledBit };

        _renderFinished = new Semaphore[_images.Length];
        for (int i = 0; i < _images.Length; i++)
        {
            _ctx.Vk.CreateSemaphore(_ctx.Device, semInfo, null, out _renderFinished[i]);
        }
        _ctx.Vk.CreateSemaphore(_ctx.Device, semInfo, null, out _imageAvailable);
        _ctx.Vk.CreateFence(_ctx.Device, fenceInfo, null, out _inFlight);
    }
    private void RecreateSwapchain()
    {
        _ctx.Vk.DeviceWaitIdle(_ctx.Device);

        foreach (var view in _imageViews)
        {
            _ctx.Vk.DestroyImageView(_ctx.Device, view, null);
        }

        _khrSwapchain.DestroySwapchain(_ctx.Device, _swapchain, null);

        CreateSwapchain();
    }
    public void Resize()
    {
        RecreateSwapchain();
    }
    
    public void Dispose()
    {
        _ctx.Vk.DeviceWaitIdle(_ctx.Device);

        for (int i = 0; i < _images.Length; i++)
        {
            _ctx.Vk.DestroySemaphore(_ctx.Device, _renderFinished[i], null);
        }
        _ctx.Vk.DestroySemaphore(_ctx.Device, _imageAvailable, null);
        _ctx.Vk.DestroyFence(_ctx.Device, _inFlight, null);

        foreach (var view in _imageViews)
        {
            _ctx.Vk.DestroyImageView(_ctx.Device, view, null);
        }

        _khrSwapchain.DestroySwapchain(_ctx.Device, _swapchain, null);
    }
}