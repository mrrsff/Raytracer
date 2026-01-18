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
    public float AbsorptionIndex;

    public Vector4 AmbientReflectance;
    public Vector4 DiffuseReflectance;
    public Vector4 SpecularReflectance;
    public Vector4 MirrorReflectance;
    public Vector4 AbsorptionCoefficient;

    public float RefractionIndex;
    private float _pad0; // Padding to ensure 16-byte alignment
    private float _pad1; // Padding to ensure 16-byte alignment
    private float _pad2; // Padding to ensure 16-byte alignment

    public override string ToString()
    {
        return $"MaterialGPU(Type={Type}, Roughness={Roughness}, PhongExponent={PhongExponent}, " +
               $"AmbientReflectance={AmbientReflectance}, " +
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
            AmbientReflectance = material.AmbientReflectance.AsVector4(),
            DiffuseReflectance = material.DiffuseReflectance.AsVector4(),
            SpecularReflectance = material.SpecularReflectance.AsVector4(),
            MirrorReflectance = material.MirrorReflectance.AsVector4(),
            AbsorptionCoefficient = material.AbsorptionCoefficient.AsVector4(),
            RefractionIndex = material.RefractionIndex,
            AbsorptionIndex = material.AbsorptionIndex
        };
    }
}