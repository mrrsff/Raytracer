using System.Text.Json.Serialization;
using Raytracer.Core;
using Raytracer.Rendering.Intersections;
using Raytracer.Scenes.Content;
using Raytracer.Scenes.Content.Datas.Camera;
using Raytracer.Scenes.Runtime;

namespace Raytracer.Scenes;

public class Scene
{
    [JsonPropertyName("Scene")] public SceneContent Content;
    
    public List<Mesh> Meshes = new List<Mesh>();
    public Scene() { }

    public Scene(SceneContent content)
    {
        Content = content;
    }
    
    public void Initialize()
    {
        if (Content.Objects.Mesh == null) return;
        
        foreach (var meshData in Content.Objects.Mesh)
        {
            if (!string.IsNullOrEmpty(meshData.Faces.PlyData))
                Meshes.Add(new Mesh(meshData.Faces.PlyData));
            else
                Meshes.Add(new Mesh(meshData, this));
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
        var spheres = Content.Objects.Sphere;
        var triangles = Content.Objects.Triangle;
        var planes = Content.Objects.Plane;
        var meshDatas = Content.Objects.Mesh;

        if (spheres != null)
        {
            foreach (var sphere in spheres)
            {
                var intersection = sphere.Intersect(ray, Content.VertexData);
                if (!intersection.Hit || intersection.Distance > closestIntersection.Distance) continue;
                
                intersection.material = GetMaterial(sphere.Material);
                closestIntersection = intersection;
            }
        }

        if (triangles != null)
        {
            foreach (var triangle in triangles)
            {
                var intersection = triangle.Intersect(ray, Content.VertexData);
                if (!intersection.Hit || intersection.Distance > closestIntersection.Distance) continue;
                
                intersection.material = GetMaterial(triangle.Material);
                closestIntersection = intersection;
            }
        }

        if (planes != null)
        {
            foreach (var plane in planes)
            {
                var intersection = plane.Intersect(ray, Content.VertexData);
                if (!intersection.Hit || intersection.Distance > closestIntersection.Distance) continue;
                
                intersection.material = GetMaterial(plane.Material);
                closestIntersection = intersection;
            }
        }

        if (meshDatas != null)
        {
            foreach (var mesh in Meshes)
            {
                var meshIntersection = mesh.Intersect(ray);
                if (!meshIntersection.Hit || meshIntersection.Distance > closestIntersection.Distance) continue;
                
                meshIntersection.material = GetMaterial(mesh.Material);
                closestIntersection = meshIntersection;
            }
        }

        return closestIntersection;
    }
    
    public Material GetMaterial(int index)
    {
        var clamped = Math.Clamp(index - 1, 0, Content.Materials.Material.Count - 1);
        return Content.Materials.Material[clamped];
    }
}