using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace Raytracer.Rendering.Vulkan.Backend.Allocations.BufferObjects;

public unsafe class UBO : Allocation
{
    public Buffer Buffer;
    public ulong Size;

    public DescriptorBufferInfo DescriptorInfo => new()
    {
        Buffer = Buffer,
        Offset = 0,
        Range = Size
    };

    public UBO(VkContext vkContext, ulong size) : base(vkContext)
    {
        Size = size;
        CreateBuffer();
    }

    private void CreateBuffer()
    {
        var bufferInfo = new BufferCreateInfo
        {
            SType = StructureType.BufferCreateInfo,
            Size = Size,
            Usage = BufferUsageFlags.UniformBufferBit,
            SharingMode = SharingMode.Exclusive
        };

        if (Vk.CreateBuffer(Device, bufferInfo, null, out Buffer) != Result.Success)
            throw new Exception("Failed to create UBO buffer");

        Vk.GetBufferMemoryRequirements(Device, Buffer, out var memReq);

        var allocInfo = new MemoryAllocateInfo
        {
            SType = StructureType.MemoryAllocateInfo,
            AllocationSize = memReq.Size,
            MemoryTypeIndex = VkContext.FindMemoryType(memReq.MemoryTypeBits, MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit)
        };

        if (Vk.AllocateMemory(Device, allocInfo, null, out Memory) != Result.Success)
            throw new Exception("Failed to allocate UBO memory");

        Vk.BindBufferMemory(Device, Buffer, Memory, 0);
    }

    public void Update<T>(in T data) where T : unmanaged
    {
        ulong dataSize = (ulong)Unsafe.SizeOf<T>();
        if (dataSize > Size)
            throw new InvalidOperationException("UBO update exceeds allocated size");

        void* mapped = null;
        MapMemory(ref mapped);
        Unsafe.Copy(mapped, ref Unsafe.AsRef(in data));
        UnmapMemory();
    }

    public override void Dispose()
    {
        if (Buffer.Handle != 0)
            Vk.DestroyBuffer(Device, Buffer, null);

        if (Memory.Handle != 0)
            Vk.FreeMemory(Device, Memory, null);
    }
}
