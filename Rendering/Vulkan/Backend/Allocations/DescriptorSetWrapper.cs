using Raytracer.Core;
using Silk.NET.Vulkan;

namespace Raytracer.Rendering.Vulkan.Backend.Allocations;

public struct DescriptorBinding
{
    public uint Binding;
    public DescriptorType Type;
    public ShaderStageFlags Stages;
}

public unsafe class DescriptorSetWrapper : IDisposable
{
    private readonly VkContext _ctx;
    private readonly Vk _vk;
    private readonly Device _device;

    public DescriptorSetLayout Layout;
    public DescriptorPool Pool;
    public DescriptorSet Handle;

    public DescriptorSetWrapper(VkContext ctx, DescriptorBinding[] bindings)
    {
        _ctx = ctx;
        _vk = ctx.Vk;
        _device = ctx.Device;

        CreateLayout(bindings);
        CreatePool(bindings);
        Allocate();
    }

    private void CreateLayout(DescriptorBinding[] bindings)
    {
        var layoutBindings = new DescriptorSetLayoutBinding[bindings.Length];

        for (int i = 0; i < bindings.Length; i++)
        {
            layoutBindings[i] = new DescriptorSetLayoutBinding
            {
                Binding = bindings[i].Binding,
                DescriptorType = bindings[i].Type,
                DescriptorCount = 1,
                StageFlags = bindings[i].Stages
            };
        }

        fixed (DescriptorSetLayoutBinding* pBindings = layoutBindings)
        {
            var info = new DescriptorSetLayoutCreateInfo
            {
                SType = StructureType.DescriptorSetLayoutCreateInfo,
                BindingCount = (uint)bindings.Length,
                PBindings = pBindings
            };

            if (_vk.CreateDescriptorSetLayout(_device, info, null, out Layout) != Result.Success)
                throw new Exception("Failed to create descriptor set layout");
        }
    }
    private void CreatePool(DescriptorBinding[] bindings)
    {
        var poolSizes = bindings
            .GroupBy(b => b.Type)
            .Select(g => new DescriptorPoolSize
            {
                Type = g.Key,
                DescriptorCount = (uint)g.Count()
            })
            .ToArray();

        fixed (DescriptorPoolSize* pSizes = poolSizes)
        {
            var info = new DescriptorPoolCreateInfo
            {
                SType = StructureType.DescriptorPoolCreateInfo,
                MaxSets = 1,
                PoolSizeCount = (uint)poolSizes.Length,
                PPoolSizes = pSizes
            };

            if (_vk.CreateDescriptorPool(_device, info, null, out Pool) != Result.Success)
                throw new Exception("Failed to create descriptor pool");
        }
    }

    private void Allocate()
    {
        fixed (DescriptorSetLayout* pLayout = &Layout)
        {
            var allocInfo = new DescriptorSetAllocateInfo
            {
                SType = StructureType.DescriptorSetAllocateInfo,
                DescriptorPool = Pool,
                DescriptorSetCount = 1,
                PSetLayouts = pLayout
            };

            if (_vk.AllocateDescriptorSets(_device, allocInfo, out Handle) != Result.Success)
                throw new Exception("Failed to allocate descriptor set");
        }
    }

    public void UpdateUBO(uint binding, DescriptorBufferInfo bufferInfo)
    {
        var write = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = Handle,
            DstBinding = binding,
            DescriptorType = DescriptorType.UniformBuffer,
            DescriptorCount = 1,
            PBufferInfo = &bufferInfo
        };

        _vk.UpdateDescriptorSets(_device, 1, write, 0, null);
    }

    public void UpdateSSBO(uint binding, DescriptorBufferInfo bufferInfo)
    {
        var write = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = Handle,
            DstBinding = binding,
            DescriptorType = DescriptorType.StorageBuffer,
            DescriptorCount = 1,
            PBufferInfo = &bufferInfo
        };

        _vk.UpdateDescriptorSets(_device, 1, write, 0, null);
    }
    
    public void UpdateStorageImage(uint binding, DescriptorImageInfo imageInfo)
    {
        var write = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = Handle,
            DstBinding = binding,
            DescriptorType = DescriptorType.StorageImage,
            DescriptorCount = 1,
            PImageInfo = &imageInfo
        };

        _vk.UpdateDescriptorSets(_device, 1, write, 0, null);
    }
    public void Dispose()
    {
        if (Pool.Handle != 0)
            _vk.DestroyDescriptorPool(_device, Pool, null);

        if (Layout.Handle != 0)
            _vk.DestroyDescriptorSetLayout(_device, Layout, null);
    }


    public class Builder
    {
        private readonly List<DescriptorBinding> _bindings = new();

        private Builder AddBinding(uint binding, DescriptorType type, ShaderStageFlags stages)
        {
            _bindings.Add(new DescriptorBinding
            {
                Binding = binding,
                Type = type,
                Stages = stages
            });
            return this;
        }

        public DescriptorSetWrapper Build(VkContext ctx)
        {
            return new DescriptorSetWrapper(ctx, _bindings.ToArray());
        }
        
        public Builder ComputeUniformBuffer(uint binding)
        {
            return AddBinding(binding, DescriptorType.UniformBuffer, ShaderStageFlags.ComputeBit);
        }
        
        public Builder UBOCompute(uint binding)
        {
            return AddBinding(binding, DescriptorType.UniformBuffer, ShaderStageFlags.ComputeBit);
        }
        
        public Builder SSBOCompute(uint binding)
        {
            return AddBinding(binding, DescriptorType.StorageBuffer, ShaderStageFlags.ComputeBit);
        }
        public Builder ComputeStorageBuffer(uint binding)
        {
            return AddBinding(binding, DescriptorType.StorageBuffer, ShaderStageFlags.ComputeBit);
        }
        public Builder ComputeStorageImage(uint binding)
        {
            return AddBinding(binding, DescriptorType.StorageImage, ShaderStageFlags.ComputeBit);
        }

        public Builder Print()
        {
            foreach (var b in _bindings)
            {
                Debug.Log($"Binding: {b.Binding}, Type: {b.Type}, Stages: {b.Stages}");
            }
            return this;
        }
    }
}