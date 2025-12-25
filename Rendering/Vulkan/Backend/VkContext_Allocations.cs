using Raytracer.Rendering.Vulkan.Backend.Allocations;
using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace Raytracer.Rendering.Vulkan.Backend;

public unsafe partial class VkContext
{
    public VkImage CreateStorageImage(uint width, uint height, 
        Format format = Format.R8G8B8A8Unorm, 
        ImageUsageFlags usageFlags = ImageUsageFlags.StorageBit | ImageUsageFlags.TransferSrcBit)
    {
        return new VkImage(this, width, height, format, usageFlags);
    }
    public CommandBuffer BeginSingleTimeCommands()
    {
        var allocInfo = new CommandBufferAllocateInfo
        {
            SType = StructureType.CommandBufferAllocateInfo,
            CommandPool = _commandPool,
            CommandBufferCount = 1,
            Level = CommandBufferLevel.Primary
        };
        _vk.AllocateCommandBuffers(_device, allocInfo, out var commandBuffer);
        var beginInfo = new CommandBufferBeginInfo
        {
            SType = StructureType.CommandBufferBeginInfo,
            Flags = CommandBufferUsageFlags.None
        };
        _vk.BeginCommandBuffer(commandBuffer, beginInfo);
        return commandBuffer; }

    public void EndSingleTimeCommands(CommandBuffer cmd)
    {
        _vk.EndCommandBuffer(cmd);
        var submitInfo = new SubmitInfo
        {
            SType = StructureType.SubmitInfo,
            CommandBufferCount = 1,
            PCommandBuffers = &cmd
        };
        SubmitMainQueue(submitInfo, default);
        WaitForQueue();
        _vk.FreeCommandBuffers(_device, _commandPool, 1, cmd);
    }
    private uint FindMemoryTypeIndex(uint filter, MemoryPropertyFlags flags)
    {
        _vk.GetPhysicalDeviceMemoryProperties(_physicalDevice, out var props);
        for (var i = 0; i < props.MemoryTypeCount; i++)
        {
            if ((filter & (uint)(1 << i)) != 0u && (props.MemoryTypes[i].PropertyFlags & flags) == flags)
                return (uint)i;
        }
        throw new Exception("Unable to find suitable memory type");
    }

    public DeviceMemory AllocateMemory(MemoryRequirements memoryRequirements, MemoryPropertyFlags propertyFlags)
    {
        var size = memoryRequirements.Size;
        var typeIndex = FindMemoryTypeIndex(memoryRequirements.MemoryTypeBits, propertyFlags);
        var allocInfo = new MemoryAllocateInfo
        {
            SType = StructureType.MemoryAllocateInfo,
            AllocationSize = size,
            MemoryTypeIndex = typeIndex
        };
        _vk.AllocateMemory(_device, allocInfo, null, out var deviceMemory);
        return deviceMemory;
    }
    
    private DeviceMemory AllocateImage(Image image, MemoryPropertyFlags propertyFlags)
    {
        _vk.GetImageMemoryRequirements(_device, image, out var memReq);
        return AllocateMemory(memReq, propertyFlags);
    }

    private DeviceMemory AllocateBuffer(Buffer buffer, MemoryPropertyFlags propertyFlags)
    {
        _vk.GetBufferMemoryRequirements(_device, buffer, out var memReq);
        return AllocateMemory(memReq, propertyFlags);
    }
    public uint FindMemoryType(uint typeFilter, MemoryPropertyFlags properties)
    {
        Vk.GetPhysicalDeviceMemoryProperties(PhysicalDevice, out var memProperties);

        for (uint i = 0; i < memProperties.MemoryTypeCount; i++)
        {
            if ((typeFilter & (1u << (int)i)) != 0 &&
                (memProperties.MemoryTypes[(int)i].PropertyFlags & properties) == properties)
            {
                return i;
            }
        }

        throw new Exception("Failed to find suitable memory type");
    }
}