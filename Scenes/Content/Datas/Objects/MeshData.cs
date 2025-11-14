using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;

namespace Raytracer.Scenes.Content.Datas.Objects;

public enum ShadingMode
{
    Flat,
    Smooth
}

public struct MeshData
{
    [JsonPropertyName("_id")] public int Id;
    [JsonPropertyName("_shadingMode")] public ShadingMode ShadingMode;
    public int Material;
    public string Transformations;
    public FacesData Faces;
    public Vector3 MotionBlur;


    public override string ToString()
    {
        return new StringBuilder().Append("Mesh(Id: ")
            .Append(Id)
            .Append(", Material Id: ")
            .Append(Material)
            .Append(", Shading Mode: ")
            .Append(ShadingMode)
            .Append(", Transformations: ")
            .Append(Transformations)
            .Append(", Vertex Indices: [")
            .Append(Faces)
            .Append("])")
            .ToString();
    }
}