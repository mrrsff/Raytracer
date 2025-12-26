using System.Numerics;
using System.Runtime.InteropServices;
using Raytracer.Core;

namespace Raytracer.Rendering.Vulkan.Backend.Scenes.Objects;

[StructLayout(LayoutKind.Sequential)]
public struct MaterialGPU
{
    public uint Type;
    public float Roughness;
    public float PhongExponent;
    public float _pad0;

    public Vector3 DiffuseReflectance;
    public float _pad1;

    public Vector3 SpecularReflectance;
    public float _pad2;

    public Vector3 MirrorReflectance;
    public float _pad3;

    // Dielectric
    public Vector3 AbsorptionCoefficient;
    public float RefractionIndex;

    public float AbsorptionIndex;
    public float _pad4;
    public float _pad5;
    public float _pad6;

    public override string ToString()
    {
        return $"MaterialGPU(Type={Type}, Roughness={Roughness}, PhongExponent={PhongExponent}, " +
               $"DiffuseReflectance={DiffuseReflectance}, SpecularReflectance={SpecularReflectance}, " +
               $"MirrorReflectance={MirrorReflectance}, AbsorptionCoefficient={AbsorptionCoefficient}, " +
               $"RefractionIndex={RefractionIndex}, AbsorptionIndex={AbsorptionIndex})";
    }

    public static MaterialGPU FromMaterial(Material material)
    {
        return new MaterialGPU
        {
            Type = (uint)material.Type,
            Roughness = material.Roughness,
            PhongExponent = material.PhongExponent,
            DiffuseReflectance = material.DiffuseReflectance,
            SpecularReflectance = material.SpecularReflectance,
            MirrorReflectance = material.MirrorReflectance,
            AbsorptionCoefficient = material.AbsorptionCoefficient,
            RefractionIndex = material.RefractionIndex,
            AbsorptionIndex = material.AbsorptionIndex
        };
    }
}