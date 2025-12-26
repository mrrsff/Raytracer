using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Raytracer.Core;
using Raytracer.Core.Lights;
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

    public readonly List<MeshDefinition> MeshDefinitions = [];
    public List<Geometry> Geometries = [];
    public List<Plane> Planes = [];
    public BoundingVolumeHierarchy? TLAS;
    public TextureManager TextureManager = new();
    private Texture? backgroundTexture;
    private SphericalDirectionalLight? sphericalDirectionalLight;
    public Scene() { }

    public Scene(SceneContent content)
    {
        Content = content;
    }

    public void Initialize()
    {
        InitializeTextures();
        InitializeLights();
        InitGeometries();
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
    private void InitializeTextures()
    {
        TextureManager.LoadTextures(Content);
        TextureManager.TryGetBackgroundTexture(out backgroundTexture);
    }
    private void InitializeLights()
    {
        Content.Lights.Initialize();
        
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

        foreach (var meshInstance in Content.Objects.MeshInstance)
        {
            if (!originalMeshes.TryGetValue(meshInstance.BaseMeshId, out var mesh)) continue;

            var transform = meshInstance.ResetTransform ? new Transform() : mesh.Transform.Copy();

            ApplyTransformations(transform, meshInstance.Transformations);
            var instancedMesh = new Mesh(mesh, transform, meshInstance);
            
            Geometries.Add(instancedMesh);
            originalMeshes.TryAdd(meshInstance.Id, instancedMesh);
        }
        
        // Convert original meshes to MeshDefinitions
        HashSet<int> addedMeshIds = [];
        foreach (var (_, mesh) in originalMeshes)
        {
            if (addedMeshIds.Contains(mesh.baseMeshId)) continue;
            
            MeshDefinitions.Add(mesh.MeshDefinition);
            addedMeshIds.Add(mesh.baseMeshId);
        }
        foreach (var sphereData in Content.Objects.Sphere)
        {
            var sphere = new Sphere(sphereData, Content.VertexData);
            ApplyTransformations(sphere.Transform, sphereData.Transformations);
            Geometries.Add(sphere);
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

    public IntersectionInfo Intersect(Ray ray)
    {
        IntersectionInfo best = IntersectionInfo.NoHit;
        IntersectionInfo hit  = IntersectionInfo.NoHit;

        if (TLAS != null && TLAS.Intersect(in ray, ref hit))
        {
            hit.material = GetMaterial(hit.HitGeometry?.MaterialIndex ?? 0);
            TryAcceptHit();
        }
        else
        {
            foreach (var g in Geometries)
            {
                hit.Reset();
                if (!g.Intersect(ray, ref hit)) continue;

                hit.material ??= GetMaterial(g.MaterialIndex);
                TryAcceptHit();
            }
        }
        foreach (var p in Planes)
        {
            hit.Reset();
            if (!p.Intersect(ray, ref hit)) continue;

            hit.material = GetMaterial(p.MaterialIndex);
            TryAcceptHit();
        }
        foreach (var t in Content.Objects.Triangle)
        {
            hit.Reset();
            if (!t.Intersect(ray, Content.VertexData, ref hit)) continue;

            hit.material = GetMaterial(t.Material);
            TryAcceptHit();
        }

        best.RayOrigin = ray.Origin;
        best.RayTime = ray.Time;
        best.Textures = GetTextures(best.HitGeometry?.TextureIndices ?? []);
        best.CalculateNormal();

        return best;

        bool TryAcceptHit()
        {
            if (hit.Distance >= best.Distance)
                return false;

            best = hit;
            return true;
        }
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