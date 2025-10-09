using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using Raytracer.IO.SceneLoaders.Converters;
using Raytracer.Scenes.Content.Datas.Objects;

namespace Raytracer.Scenes.Content;

public struct Objects
{
    [JsonConverter(typeof(SingleOrListConverter<SphereData>))]
    public List<SphereData> Sphere;

    [JsonConverter(typeof(SingleOrListConverter<TriangleData>))]
    public List<TriangleData> Triangle;

    [JsonConverter(typeof(SingleOrListConverter<MeshData>))]
    public List<MeshData> Mesh;

    [JsonConverter(typeof(SingleOrListConverter<PlaneData>))]
    public List<PlaneData> Plane;

    public override string ToString()
    {
        var sb = new StringBuilder();
        foreach (var sphere in Sphere)
            sb.AppendLine(sphere.ToString());
        foreach (var triangle in Triangle)
            sb.AppendLine(triangle.ToString());
        foreach (var mesh in Mesh)
            sb.AppendLine(mesh.ToString());
        foreach (var plane in Plane)
            sb.AppendLine(plane.ToString());
        return sb.ToString();
    }
}