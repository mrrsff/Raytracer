using System.Numerics;
using System.Text.Json.Serialization;
using Raytracer.Core;
using Raytracer.Rendering.Intersections;
using Raytracer.Scenes.Content;
using Raytracer.Scenes.Content.Datas.Camera;
using Raytracer.Scenes.Runtime;
using Raytracer.Scenes.Runtime.Meshes;
using Plane = Raytracer.Scenes.Runtime.Plane;

namespace Raytracer.Scenes;

public partial class Scene
{
    [JsonPropertyName("Scene")] public SceneContent Content;
    
    public List<Geometry> Geometries = [];
    public List<Plane> Planes = [];
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
            {
                var mesh = new Mesh(meshData.Faces.PlyData, meshData.ShadingMode, meshData.Material);
                Geometries.Add(mesh);
            }
            else
            {
                var mesh = new Mesh(meshData, this);
                Geometries.Add(mesh);
            }
        }
        
        foreach (var sphereData in Content.Objects.Sphere)
        {
            Geometries.Add(new Sphere(sphereData, Content.VertexData));
        }

        foreach (var planeData in Content.Objects.Plane)
        {
            Planes.Add(new Plane(planeData, Content.VertexData));
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
        IntersectionInfo intersection = IntersectionInfo.NoHit;

        foreach (var geometry in Geometries)
        {
            intersection.Reset();
            if (geometry.Intersect(ray, ref intersection) && (intersection.Distance < closestIntersection.Distance))
            {
                intersection.material ??= GetMaterial(geometry.MaterialIndex);
                closestIntersection = intersection;
            } 
        }
        
        foreach (var plane in Planes)
        {
            intersection.Reset();
            if (plane.Intersect(ray, ref intersection) &&
                (intersection.Distance < closestIntersection.Distance))
            {
                intersection.material = GetMaterial(plane.MaterialIndex);
                closestIntersection = intersection;
            }
        }

        foreach (var triangle in Content.Objects.Triangle)
        {
            intersection.Reset();
            if (triangle.Intersect(ray, Content.VertexData, ref intersection) &&
                (intersection.Distance < closestIntersection.Distance))
            {
                intersection.material = GetMaterial(triangle.Material);
                closestIntersection = intersection;
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