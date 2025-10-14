using System.Text.Json.Serialization;
using Raytracer.Core;
using Raytracer.Rendering.Intersections;
using Raytracer.Scenes.Content;
using Raytracer.Scenes.Content.Datas.Camera;
using Raytracer.Scenes.Content.Datas.Objects;
using Raytracer.Scenes.Runtime;
using Raytracer.Scenes.Runtime.Meshes;

namespace Raytracer.Scenes;

public partial class Scene
{
    [JsonPropertyName("Scene")] public SceneContent Content;
    
    public List<Mesh> Meshes = [];
    public List<Sphere> Spheres = [];
    public Scene() { }

    public Scene(SceneContent content)
    {
        Content = content;
    }
    
    public void Initialize()
    {
        foreach (var meshData in Content.Objects.Mesh)
        {
            if (!string.IsNullOrEmpty(meshData.Faces.PlyData))
                Meshes.Add(new Mesh(meshData.Faces.PlyData, meshData.ShadingMode, meshData.Material));
            else
                Meshes.Add(new Mesh(meshData, this));
        }
        
        foreach (var sphereData in Content.Objects.Sphere)
        {
            Spheres.Add(new Sphere(sphereData));
        } 
    }
    
    public Camera GetCamera(int index)
    {
        var clamped = Math.Clamp(index, 0, Content.Cameras.Camera.Count - 1);
        return Content.Cameras.Camera[clamped];
    }

    public IntersectionInfo Intersect(Ray ray)
    {
        IntersectionInfo closestIntersection = new IntersectionInfo();
        var triangles = Content.Objects.Triangle;
        var planes = Content.Objects.Plane;
        var intersection = IntersectionInfo.NoHit;
        
        foreach (var mesh in Meshes)
        {
            if (!mesh.Intersect(ray, ref intersection) || intersection.Distance > closestIntersection.Distance) continue;
                
            intersection.material = GetMaterial(mesh.Material);
            closestIntersection = intersection;
        }
        
        foreach (var sphere in Spheres)
        {
            if (!sphere.Intersect(ray, Content.VertexData, ref intersection) || intersection.Distance > closestIntersection.Distance) continue;
                
            intersection.material = GetMaterial(sphere.data.Material);
            closestIntersection = intersection;
        }

        foreach (var triangle in triangles)
        {
            if (!triangle.Intersect(ray, Content.VertexData, ref intersection) || intersection.Distance > closestIntersection.Distance) continue;
                
            intersection.material = GetMaterial(triangle.Material);
            closestIntersection = intersection;
        }

        foreach (var plane in planes)
        {
            if (!plane.Intersect(ray, Content.VertexData, ref intersection) || intersection.Distance > closestIntersection.Distance) continue;
                
            intersection.material = GetMaterial(plane.Material);
            closestIntersection = intersection;
        }

        return closestIntersection;
    }
    
    public Material GetMaterial(int index)
    {
        var clamped = Math.Clamp(index - 1, 0, Content.Materials.Material.Count - 1);
        return Content.Materials.Material[clamped];
    }
}