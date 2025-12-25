using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace Raytracer.Rendering.Vulkan.Backend.Allocations.BufferObjects;

public unsafe class UBO : VkBuffer
{
    public UBO(VkContext ctx, uint size) : base(ctx, size, BufferUsageFlags.UniformBufferBit, MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit)
    { }

    public DescriptorBufferInfo DescriptorInfo => GetBufferInfo();

    public void Update<T>(in T data) where T : unmanaged
    {
        uint dataSize = (uint)Unsafe.SizeOf<T>();
        if (dataSize > Size)
            throw new InvalidOperationException("UBO overflow");

        void* mapped = null;
        MapMemory(ref mapped);
        Unsafe.Copy(mapped, ref Unsafe.AsRef(in data));
        UnmapMemory();
    }
}
