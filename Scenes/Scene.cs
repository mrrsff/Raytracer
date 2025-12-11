using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Raytracer.Core;
using Raytracer.Rendering.Intersections;
using Raytracer.Scenes.Content;
using Raytracer.Scenes.Content.Datas.Camera;
using Raytracer.Scenes.Runtime;
using Raytracer.Scenes.Runtime.Meshes;
using Raytracer.Scenes.Runtime.Meshes.BVH;
using Plane = Raytracer.Scenes.Runtime.Plane;

namespace Raytracer.Scenes;

public partial class Scene
{
    [JsonPropertyName("Scene")] public SceneContent Content;

    public List<Geometry> Geometries = [];
    public List<Plane> Planes = [];
    public BoundingVolumeHierarchy TLAS;

    public Scene()
    {
    }

    public Scene(SceneContent content)
    {
        Content = content;
    }

    public void Initialize()
    {
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
                Content.Transformations.ApplyTransformations(transform, meshData.Transformations);

            var mesh = string.IsNullOrEmpty(meshData.Faces.PlyData)
                ? new Mesh(meshData, this, transform)
                : new Mesh(meshData.Faces.PlyData, meshData.ShadingMode, meshData.Material, transform, meshData.MotionBlur);
            
            Geometries.Add(mesh);
            originalMeshes.Add(meshData.Id, mesh);
        }

        foreach (var meshInstance in Content.Objects.MeshInstance)
        {
            if (!originalMeshes.TryGetValue(meshInstance.BaseMeshId, out var mesh)) continue;

            var transform = meshInstance.ResetTransform ? new Transform() : mesh.Transform.Copy();

            if (meshInstance.Transformations != null)
                Content.Transformations.ApplyTransformations(transform, meshInstance.Transformations);

            var instancedMesh = new Mesh(mesh, transform);
            instancedMesh.MotionBlur = meshInstance.MotionBlur;
            instancedMesh.MaterialIndex = meshInstance.Material != -1 ? meshInstance.Material : mesh.MaterialIndex;
            Geometries.Add(instancedMesh);
            originalMeshes.Add(meshInstance.Id, instancedMesh);
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
        if (Geometries.Count > 16)
            TLAS = new BoundingVolumeHierarchy(this);

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

        return closestIntersection;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Material GetMaterial(int index)
    {
        var clamped = Math.Clamp(index - 1, 0, Content.Materials.Material.Count - 1);
        return Content.Materials.Material[clamped];
    }
}