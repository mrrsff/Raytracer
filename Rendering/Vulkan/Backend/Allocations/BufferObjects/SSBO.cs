using System.Runtime.CompilerServices;
using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace Raytracer.Rendering.Vulkan.Backend.Allocations.BufferObjects;

public sealed unsafe class SSBO<T> : VkBuffer where T : unmanaged
{
    public uint Capacity { get; private set; }
    public uint Count { get; private set; }

    private readonly bool _allowOverflow;

    public ulong SizeInBytes => Capacity * (ulong)Unsafe.SizeOf<T>();

    public DescriptorBufferInfo DescriptorInfo => GetBufferInfo();

    public SSBO(VkContext ctx, uint capacity, bool allowOverflow) 
        : base(ctx, capacity * (uint)Unsafe.SizeOf<T>(), BufferUsageFlags.StorageBufferBit, MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit)
    {
        Capacity = capacity;
        _allowOverflow = allowOverflow;
    }
    public void SetData(ReadOnlySpan<T> data)
    {
        if (data.Length > Capacity)
        {
            if (!_allowOverflow)
                throw new InvalidOperationException(
                    "SSBO capacity exceeded and overflow is disabled");

            Resize((uint)data.Length);
        }

        Count = (uint)data.Length;

        void* mapped = null;
        MapMemory(ref mapped);

        fixed (T* src = data)
        {
            System.Buffer.MemoryCopy(
                src,
                mapped,
                Size,
                Count * (ulong)Unsafe.SizeOf<T>()
            );
        }

        UnmapMemory();
    }

    // -----------------------------
    // Element update
    // -----------------------------
    public void UpdateElement(uint index, in T value)
    {
        if (index >= Capacity)
            throw new ArgumentOutOfRangeException(nameof(index));

        void* mapped = null;
        MapMemory(ref mapped);

        byte* dst = (byte*)mapped + index * Unsafe.SizeOf<T>();
        Unsafe.Copy(dst, ref Unsafe.AsRef(in value));

        UnmapMemory();
    }

    // -----------------------------
    // Resize (reallocate)
    // -----------------------------
    private void Resize(uint newCapacity)
    {
        // Save old
        var oldBuffer = Buffer;
        var oldMemory = Memory;
        var oldSize   = Size;

        // Create new VkBuffer
        Capacity = newCapacity;
        Size = newCapacity * (uint)Unsafe.SizeOf<T>();

        RecreateBuffer(
            BufferUsageFlags.StorageBufferBit,
            MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit
        );

        // Copy old contents
        void* oldMapped = null;
        void* newMapped = null;

        Vk.MapMemory(Device, oldMemory, 0, Vk.WholeSize, 0, ref oldMapped);
        Vk.MapMemory(Device, Memory, 0, Vk.WholeSize, 0, ref newMapped);

        System.Buffer.MemoryCopy(
            oldMapped,
            newMapped,
            Size,
            oldSize
        );

        Vk.UnmapMemory(Device, oldMemory);
        Vk.UnmapMemory(Device, Memory);

        // Destroy old
        Vk.DestroyBuffer(Device, oldBuffer, null);
        Vk.FreeMemory(Device, oldMemory, null);
    }
    protected void RecreateBuffer(BufferUsageFlags usage, MemoryPropertyFlags memoryFlags)
    {
        // destroy current
        Vk.DestroyBuffer(Device, Buffer, null);
        Vk.FreeMemory(Device, Memory, null);

        // create new
        var bufferInfo = new BufferCreateInfo
        {
            SType = StructureType.BufferCreateInfo,
            Size = Size,
            Usage = usage,
            SharingMode = SharingMode.Exclusive
        };

        Vk.CreateBuffer(Device, bufferInfo, null, out Buffer);

        Vk.GetBufferMemoryRequirements(Device, Buffer, out var memReq);
        Memory = VkContext.AllocateMemory(memReq, memoryFlags);
        Vk.BindBufferMemory(Device, Buffer, Memory, 0);
    }
    
    public string ReadGPUDataAsString()
    {
        void* mapped = null;
        MapMemory(ref mapped);

        Span<T> dataSpan = new Span<T>(mapped, (int)Count);
        string result = string.Join(Environment.NewLine, dataSpan.ToArray());

        UnmapMemory();

        return result;
    }
}
