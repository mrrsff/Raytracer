using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Raytracer.Core;
using Raytracer.Rendering.CPU.Intersections;
using Raytracer.Scenes.Content;
using Raytracer.Scenes.Content.Datas.CameraData;
using Raytracer.Scenes.Runtime;
using Raytracer.Scenes.Runtime.Meshes;
using Raytracer.Scenes.Runtime.Meshes.BVH;
using Raytracer.Scenes.Runtime.Textures;
using Raytracer.Utility;
using Plane = Raytracer.Scenes.Runtime.Plane;

namespace Raytracer.Scenes;

public partial class Scene
{
    [JsonPropertyName("Scene")] public SceneContent Content;

    public List<Geometry> Geometries = [];
    public List<Plane> Planes = [];
    public BoundingVolumeHierarchy TLAS;
    public TextureManager TextureManager = new();
    private Texture? backgroundTexture;

    public Scene()
    {
    }

    public Scene(SceneContent content)
    {
        Content = content;
    }

    public void Initialize()
    {
        // Load textures
        TextureManager.LoadTextures(Content);
        if (TextureManager.TryGetBackgroundTexture(out backgroundTexture))
        {
            
        }
        
        if (Content.Lights.PointLight != null)
        {
            foreach (var pLight in Content.Lights.PointLight)
            {
                if (pLight.Transformations != null)
                {
                    Content.Transformations.ApplyTransformations(pLight.Transform, pLight.Transformations);
                    pLight.CalculatePosition();
                }
            }
        }
        if (Content.Lights.AreaLight != null)
        {
            foreach (var dLight in Content.Lights.AreaLight)
            {
                if (dLight.Transformations != null)
                {
                    Content.Transformations.ApplyTransformations(dLight.Transform, dLight.Transformations);
                }
                dLight.CalculateValues();
            }
        }

        foreach (var camera in Content.Cameras.Camera)
        {
            if (camera.Transformations != null)
            {
                Content.Transformations.ApplyTransformations(camera.Transform, camera.Transformations);
            }
        }

        var originalMeshes = new Dictionary<int, Mesh>();
        foreach (var meshData in Content.Objects.Mesh)
        {
            var transform = new Transform();
            if (meshData.Transformations != null)
            {
                Content.Transformations.ApplyTransformations(transform, meshData.Transformations);
            }

            var mesh = new Mesh(meshData, this, transform);

            
            Geometries.Add(mesh);
            originalMeshes.Add(meshData.Id, mesh);
        }

        foreach (var meshInstance in Content.Objects.MeshInstance)
        {
            if (!originalMeshes.TryGetValue(meshInstance.BaseMeshId, out var mesh)) continue;

            var transform = meshInstance.ResetTransform ? new Transform() : mesh.Transform.Copy();

            if (meshInstance.Transformations != null)
            {
                Content.Transformations.ApplyTransformations(transform, meshInstance.Transformations);
            }
            var instancedMesh = new Mesh(mesh, transform, meshInstance);
            
            Geometries.Add(instancedMesh);
            originalMeshes.TryAdd(meshInstance.Id, instancedMesh);
        }

        foreach (var sphereData in Content.Objects.Sphere)
        {
            var sphere = new Sphere(sphereData, Content.VertexData);
            if (sphereData.Transformations != null)
                Content.Transformations.ApplyTransformations(sphere.Transform, sphereData.Transformations);
            Geometries.Add(sphere);
        }

        foreach (var planeData in Content.Objects.Plane)
        {
            var plane = new Plane(planeData, Content.VertexData);
            if (planeData.Transformations != null)
                Content.Transformations.ApplyTransformations(plane.Transform, planeData.Transformations);

            Planes.Add(plane);
        }

        // Build TLAS if there are enough geometries
        // if (Geometries.Count > 16)
        //     TLAS = new BoundingVolumeHierarchy(this);

        
        if (Debug.PrintSceneInfo)
        {
            Debug.Log($"Scene initialized with {Content.Cameras.Camera.Count} cameras, " +
                      $"{Content.Cameras.Camera.FirstOrDefault()!.NumSamples} samples per pixel, " +
                      $"{Content.Lights.PointLight?.Count ?? 0} point lights, " +
                      $"{Content.Lights.AreaLight?.Count ?? 0} area lights, " +
                      $"{Geometries.Sum(g => g.GetPrimitiveCount())} geometric primitives, " +
                      $"{Planes.Count} planes, " + 
                      $"{Content.Objects.Triangle.Count} triangles.");
        }
    }

    public Camera GetCamera(int index)
    {
        var clamped = Math.Clamp(index, 0, Content.Cameras.Camera.Count - 1);
        return Content.Cameras.Camera[clamped];
    }

    public IntersectionInfo Intersect(Ray ray)
    {
        IntersectionInfo closestIntersection = IntersectionInfo.NoHit;
        IntersectionInfo intersection = IntersectionInfo.NoHit;

        if (TLAS != null && TLAS.Intersect(in ray, ref intersection))
        {
            if (intersection.Distance < closestIntersection.Distance)
            {
                var index = intersection.HitGeometry?.MaterialIndex ?? 0;
                intersection.material = GetMaterial(index);
                closestIntersection = intersection;
            }
        }
        else
        {
            foreach (var geometry in Geometries)
            {
                intersection.Reset();
                if (geometry.Intersect(ray, ref intersection) && (intersection.Distance < closestIntersection.Distance))
                {
                    intersection.material ??= GetMaterial(geometry.MaterialIndex);
                    closestIntersection = intersection;
                }
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

        closestIntersection.RayOrigin = ray.Origin;
        closestIntersection.RayTime = ray.Time;
        closestIntersection.Textures = GetTextures(closestIntersection.HitGeometry?.TextureIndices ?? []);
        closestIntersection.CalculateNormal();

        return closestIntersection;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Material GetMaterial(int index)
    {
        var clamped = Math.Clamp(index - 1, 0, Content.Materials.Material.Count - 1);
        return Content.Materials.Material[clamped];
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Texture[] GetTextures(int[] textureIndices)
    {
        var textures = new Texture[textureIndices.Length];
        for (int i = 0; i < textureIndices.Length; i++)
        {
            textures[i] = TextureManager.GetTexture(textureIndices[i]);
        }
        return textures;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3 GetBackgroundColor(int x, int y, Camera cam)
    {
        if (backgroundTexture == null)
        {
            return Content.BackgroundColor;
        }

        // Get UV coordinates based on pixel position
        float u = (x + 0.5f) / cam.ImageResolution.Width;
        float v = (y + 0.5f) / cam.ImageResolution.Height;

        // Stretch (clamp), do NOT wrap
        u = Math.Clamp(u, 0f, 1f);
        v = Math.Clamp(v, 0f, 1f);

        Vector3 sample = backgroundTexture.SampleFromUV(new Vector2(u, v)) * 255f;
        return sample;
    }
}