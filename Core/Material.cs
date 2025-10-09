using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;

namespace Raytracer.Core;

public class Material
{
    public Vector3 AmbientReflectance;
    public Vector3 DiffuseReflectance;
    [JsonPropertyName("_id")] public int Id;
    public float PhongExponent;
    public Vector3 SpecularReflectance;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MaterialType Type;

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
            .Append(", PhongExponent: ")
            .Append(PhongExponent)
            .Append(')').ToString();
    }
}