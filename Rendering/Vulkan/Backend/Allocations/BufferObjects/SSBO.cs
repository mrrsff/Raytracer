using System.Runtime.CompilerServices;
using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace Raytracer.Rendering.Vulkan.Backend.Allocations.BufferObjects;

public unsafe class SSBO<T> : Allocation where T : unmanaged
{
    public Buffer Buffer;

    public uint Capacity;
    public uint Count;

    public ulong SizeInBytes => Capacity * (ulong)Unsafe.SizeOf<T>();

    private readonly bool _allowOverflow;

    public DescriptorBufferInfo DescriptorInfo => new()
    {
        Buffer = Buffer,
        Offset = 0,
        Range = SizeInBytes
    };

    public SSBO(
        VkContext vkContext,
        uint capacity,
        bool allowOverflow
    ) : base(vkContext)
    {
        Capacity = capacity;
        _allowOverflow = allowOverflow;
        CreateBuffer();
    }
    private void CreateBuffer()
    {
        var bufferInfo = new BufferCreateInfo
        {
            SType = StructureType.BufferCreateInfo,
            Size = SizeInBytes,
            Usage = BufferUsageFlags.StorageBufferBit,
            SharingMode = SharingMode.Exclusive
        };

        if (Vk.CreateBuffer(Device, bufferInfo, null, out Buffer) != Result.Success)
            throw new Exception("Failed to create SSBO buffer");

        Vk.GetBufferMemoryRequirements(Device, Buffer, out var memReq);

        var allocInfo = new MemoryAllocateInfo
        {
            SType = StructureType.MemoryAllocateInfo,
            AllocationSize = memReq.Size,
            MemoryTypeIndex = VkContext.FindMemoryType(memReq.MemoryTypeBits, MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit)
        };

        if (Vk.AllocateMemory(Device, allocInfo, null, out Memory) != Result.Success)
            throw new Exception("Failed to allocate SSBO memory");

        Vk.BindBufferMemory(Device, Buffer, Memory, 0);
    }

    public void SetData(ReadOnlySpan<T> data)
    {
        if (data.Length > Capacity)
        {
            if (!_allowOverflow)
                throw new InvalidOperationException("SSBO capacity exceeded and overflow is disabled");

            Resize((uint)data.Length);
        }

        Count = (uint)data.Length;

        void* mapped = null;
        MapMemory(ref mapped);

        fixed (T* src = data)
        {
            Unsafe.CopyBlock(
                destination: mapped,
                source: src,
                byteCount: (uint)(Count * Unsafe.SizeOf<T>()));
        }

        UnmapMemory();
    }

    public void UpdateElement(uint index, in T value)
    {
        if (index >= Capacity)
            throw new ArgumentOutOfRangeException(nameof(index));

        void* mapped = null;
        MapMemory(ref mapped);

        byte* dst = (byte*)mapped + index * Unsafe.SizeOf<T>();
        Unsafe.Copy(dst, ref Unsafe.AsRef(value));

        UnmapMemory();
    }
    private void Resize(uint newCapacity)
    {
        // Save old
        Buffer oldBuffer = Buffer;
        DeviceMemory oldMemory = Memory;
        uint oldCapacity = Capacity;

        // Create new
        Capacity = newCapacity;
        CreateBuffer();

        // Copy old contents
        void* oldMapped = null;
        void* newMapped = null;

        Vk.MapMemory(Device, oldMemory, 0, Vk.WholeSize, 0, ref oldMapped);
        Vk.MapMemory(Device, Memory, 0, Vk.WholeSize, 0, ref newMapped);

        Unsafe.CopyBlock(
            destination: newMapped,
            source: oldMapped,
            byteCount: (uint)(oldCapacity * Unsafe.SizeOf<T>())
        );

        Vk.UnmapMemory(Device, oldMemory);
        Vk.UnmapMemory(Device, Memory);

        // Destroy old
        Vk.DestroyBuffer(Device, oldBuffer, null);
        Vk.FreeMemory(Device, oldMemory, null);
    }
    public override void Dispose()
    {
        if (Buffer.Handle != 0)
            Vk.DestroyBuffer(Device, Buffer, null);

        if (Memory.Handle != 0)
            Vk.FreeMemory(Device, Memory, null);
    }
}
