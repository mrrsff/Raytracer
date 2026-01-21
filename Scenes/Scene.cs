using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Raytracer.Core;
using Raytracer.Core.Lights;
using Raytracer.Rendering;
using Raytracer.Rendering.Intersections;
using Raytracer.Rendering.PathTracing;
using Raytracer.Rendering.Raytracing;
using Raytracer.Scenes.Content;
using Raytracer.Scenes.Content.Datas.Camera;
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
    public List<ILight> Lights = [];
    public List<Plane> Planes = [];
    public BoundingVolumeHierarchy? TLAS;
    public TextureManager TextureManager = new();
    private Texture? backgroundTexture;
    private SphericalDirectionalLight? sphericalDirectionalLight;

    public Scene()
    {
    }

    public Scene(SceneContent content)
    {
        Content = content;
    }
    public void Initialize()
    {
        InitializeTextures();
        InitializeMaterials();
        InitGeometries();
        InitializeLights();
        InitializeCameras();

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

    private void InitializeMaterials()
    {
        // create brdf's for materials
        Content.Materials.CreateBRDFs(Content.BRDFs);
    }

    private void InitializeTextures()
    {
        TextureManager.LoadTextures(Content);
        TextureManager.TryGetBackgroundTexture(out backgroundTexture);
    }
    private void InitializeLights()
    {
        Content.Lights.Initialize();
        Lights.AddRange(Content.Lights.AllLights);
        
        var pLightList = Content.Lights.GetPointLights();
        foreach (var pLight in pLightList) { ApplyTransformations(pLight.Transform, pLight.Transformations); pLight.CalculatePosition(); }
        
        var aLightList = Content.Lights.GetAreaLights();
        foreach (var aLight in aLightList) ApplyTransformations(aLight.Transform, aLight.Transformations);
        
        var sphDirLightList = Content.Lights.GetSphericalDirectionalLights();
        foreach (var sphDirLight in sphDirLightList)
        {
            if (TextureManager.GetTexture(sphDirLight.ImageId) is HDRImage image)
            {
                sphDirLight.Initialize(image);
                sphericalDirectionalLight = sphDirLight;
            }
            else
            {
                Debug.Log($"Failed to initialize SphericalDirectionalLight with Id {sphDirLight.Id}: HDR image with Id {sphDirLight.ImageId} not found.");
            }
        }
    }
    private void InitializeCameras()
    {
        foreach (var camera in Content.Cameras.Camera)
        {
            ApplyTransformations(camera.Transform, camera.Transformations);
            camera.Initialize();
        }
    }
    private void InitGeometries()
    {
        var originalMeshes = new Dictionary<int, Mesh>();
        foreach (var meshData in Content.Objects.Mesh)
        {
            var transform = new Transform();
            
            ApplyTransformations(transform, meshData.Transformations);
            var mesh = new Mesh(meshData, this, transform);
            Geometries.Add(mesh);
            originalMeshes.TryAdd(meshData.Id, mesh);
        }
        
        foreach (var lightMeshData in Content.Objects.LightMesh)
        {
            var transform = new Transform();
            
            ApplyTransformations(transform, lightMeshData.Transformations);
            var lightMesh = new LightMesh(lightMeshData, this, transform);
            Geometries.Add(lightMesh);
            originalMeshes.TryAdd(lightMeshData.Id, lightMesh);
            Lights.Add(lightMesh);
        }

        foreach (var meshInstance in Content.Objects.MeshInstance)
        {
            if (!originalMeshes.TryGetValue(meshInstance.BaseMeshId, out var mesh)) continue;

            var transform = meshInstance.ResetTransform ? new Transform() : mesh.Transform.Copy();

            ApplyTransformations(transform, meshInstance.Transformations);
            var instancedMesh = new Mesh(mesh, transform, meshInstance);
            
            Geometries.Add(instancedMesh);
            originalMeshes.TryAdd(meshInstance.Id, instancedMesh);
        }
        foreach (var sphereData in Content.Objects.Sphere)
        {
            var sphere = new Sphere(sphereData, Content.VertexData);
            ApplyTransformations(sphere.Transform, sphereData.Transformations);
            Geometries.Add(sphere);
        }
        foreach (var lightSphereData in Content.Objects.LightSphere)
        {
            var lightSphere = new LightSphere(lightSphereData, Content.VertexData);
            ApplyTransformations(lightSphere.Transform, lightSphereData.Transformations);
            Geometries.Add(lightSphere);
            Lights.Add(lightSphere);
        }

        foreach (var planeData in Content.Objects.Plane)
        {
            var plane = new Plane(planeData, Content.VertexData);
            ApplyTransformations(plane.Transform, planeData.Transformations);
            Planes.Add(plane);
        }
        
        // Build TLAS if there are enough geometries
        // if (Geometries.Count > 16)
        //     TLAS = new BoundingVolumeHierarchy(this);
    }
    private void ApplyTransformations(Transform transform, string? transformationList)
    {
        if (transformationList == null) return;
        Content.Transformations.ApplyTransformations(transform, transformationList);
    }
    public Camera GetCamera(int index)
    {
        var clamped = Math.Clamp(index, 0, Content.Cameras.Camera.Count - 1);
        return Content.Cameras.Camera[clamped];
    }

    public Renderer GetRenderer(int cameraIndex)
    {
        return GetCamera(cameraIndex).Renderer switch
        {
            RendererType.RayTracing => new RayTracerRenderer(this),
            RendererType.PathTracing => new PathTracerRenderer(this),
            _ => new RayTracerRenderer(this),
        };
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
    public Vector3 GetBackgroundColor(IntersectionInfo info, Ray ray)
    {
        if (sphericalDirectionalLight != null) // use spherical directional light mapping
        {
            Vector3 direction = Vector3.Normalize(ray.Origin + ray.Direction);
            Vector2 uv = sphericalDirectionalLight.GetUV(direction);

            Vector3 sample = sphericalDirectionalLight.SampleFromUV(uv);
            return sample;
        }

        if (backgroundTexture != null)
        {
            Camera cam = info.Camera;
            int x = info.XPixel;
            int y = info.YPixel;
        
            float u = (x + 0.5f) / cam.ImageResolution.Width;
            float v = (y + 0.5f) / cam.ImageResolution.Height;

            u = Math.Clamp(u, 0f, 1f);
            v = Math.Clamp(v, 0f, 1f);

            Vector3 sample = backgroundTexture.SampleFromUV(new Vector2(u, v)) * 255f;
            return sample;
        }

        return Content.BackgroundColor;
    }
}