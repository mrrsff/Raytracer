using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;

namespace Raytracer.Core;

public class Material
{
    [JsonPropertyName("_id")] public int Id;
    [JsonPropertyName("_type")] public MaterialType Type;

    public Vector3 AmbientReflectance;
    public Vector3 DiffuseReflectance;
    public Vector3 SpecularReflectance;
    public Vector3 MirrorReflectance;
    public float Roughness;

    public float PhongExponent;

    public Vector3 AbsorptionCoefficient;
    public float RefractionIndex;
    public float AbsorptionIndex;

    public override string ToString()
    {
        return new StringBuilder().Append("Material(Id: ")
            .Append(Id)
            .Append(", Type: ")
            .Append(Type)
            .Append(", AmbientReflectance: ")
            .Append(AmbientReflectance)
            .Append(", DiffuseReflectance: ")
            .Append(DiffuseReflectance)
            .Append(", SpecularReflectance: ")
            .Append(SpecularReflectance)
            .Append(", MirrorReflectance: ")
            .Append(MirrorReflectance)
            .Append(", PhongExponent: ")
            .Append(PhongExponent)
            .Append(", AbsorptionCoefficient: ")
            .Append(AbsorptionCoefficient)
            .Append(", RefractionIndex: ")
            .Append(RefractionIndex)
            .Append(", AbsorptionIndex: ")
            .Append(AbsorptionIndex)
            .Append(')').ToString();
    }
}