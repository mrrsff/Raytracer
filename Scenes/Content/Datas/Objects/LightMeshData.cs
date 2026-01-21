using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;

namespace Raytracer.Scenes.Content.Datas.Objects;

public class LightMeshData : MeshData
{
    public Vector3 Radiance;
    public override string ToString()
    {
        return new StringBuilder().Append("LightMesh(Id: ")
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
            .Append(", Radiance: ")
            .Append(Radiance)
            .ToString();
    }
}