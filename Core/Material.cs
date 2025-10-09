using System.Text.Json.Serialization;

namespace Raytracer.Core.Datas;
public enum MaterialType
{
    Mirror,
    Conductor,
    Dielectric
}
public class Material
{
    [JsonPropertyName("_id")] public int Id;
    
    [JsonConverter(typeof(JsonStringEnumConverter))] public MaterialType Type;
    
    public float AmbientReflectance;
    public float DiffuseReflectance;
    public float SpecularReflectance;
    public float PhongExponent;

}