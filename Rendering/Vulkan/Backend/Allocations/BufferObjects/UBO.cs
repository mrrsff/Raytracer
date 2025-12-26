using System.Runtime.CompilerServices;
using Silk.NET.Vulkan;

namespace Raytracer.Rendering.Vulkan.Backend.Allocations.BufferObjects;

public sealed unsafe class UBO<T> : VkBuffer where T : unmanaged
{
    public DescriptorBufferInfo DescriptorInfo => GetBufferInfo();

    public UBO(VkContext ctx) 
        : base(ctx, (uint)Unsafe.SizeOf<T>(), BufferUsageFlags.UniformBufferBit, MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit) { }

    public void SetData(in T data)
    {
        void* mapped = null;
        MapMemory(ref mapped);
        Unsafe.Copy(mapped, ref Unsafe.AsRef(in data));
        UnmapMemory();
    }

    private T? Read()
    {
        void* mapped = null;
        MapMemory(ref mapped);

        T value = Unsafe.Read<T>(mapped);

        UnmapMemory();
        return value;
    }

    public override string ToString()
    {
        return Read()?.ToString() ?? "<null>";
    }
}