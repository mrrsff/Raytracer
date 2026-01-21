using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using Raytracer.IO.SceneLoaders.Converters;
using Raytracer.Scenes.Content.Datas.Objects;

namespace Raytracer.Scenes.Content;

public struct Objects
{
    [JsonConverter(typeof(SingleOrListConverter<SphereData>))]
    public List<SphereData> Sphere = new();
    
    [JsonConverter(typeof(SingleOrListConverter<LightSphereData>))]
    public List<LightSphereData> LightSphere = new();

    [JsonConverter(typeof(SingleOrListConverter<TriangleData>))]
    public List<TriangleData> Triangle = new();

    [JsonConverter(typeof(SingleOrListConverter<MeshData>))]
    public List<MeshData> Mesh = new();
    
    [JsonConverter(typeof(SingleOrListConverter<LightMeshData>))]
    public List<LightMeshData> LightMesh = new();

    [JsonConverter(typeof(SingleOrListConverter<MeshInstance>))]
    public List<MeshInstance> MeshInstance = new();

    [JsonConverter(typeof(SingleOrListConverter<PlaneData>))]
    public List<PlaneData> Plane = new();

    public Objects()
    {
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        if (Triangle != null)
        {
            foreach (var triangle in Triangle)
                sb.AppendLine(triangle.ToString());
        }

        if (Sphere != null)
        {
            foreach (var sphere in Sphere)
                sb.AppendLine(sphere.ToString());
        }

        if (LightSphere != null)
        {
            foreach (var lightSphere in LightSphere)
                sb.AppendLine(lightSphere.ToString());
        }
        
        if (Mesh != null)
        {
            foreach (var mesh in Mesh)
                sb.AppendLine(mesh.ToString());
        }
        
        if (LightMesh != null)
        {
            foreach (var lightMesh in LightMesh)
                sb.AppendLine(lightMesh.ToString());
        }

        if (Plane != null)
        {
            foreach (var plane in Plane)
                sb.AppendLine(plane.ToString());
        }

        return sb.ToString();
    }
}