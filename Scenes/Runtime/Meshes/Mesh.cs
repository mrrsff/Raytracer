using System.Numerics;
using Raytracer.Core;
using Raytracer.Rendering;
using Raytracer.Rendering.Intersections;
using Raytracer.Rendering.Raytracing;
using Raytracer.Scenes.Content.Datas.Objects;
using Raytracer.Scenes.Runtime.Textures;
using Raytracer.Utility;

namespace Raytracer.Scenes.Runtime.Meshes;

public class Mesh : Geometry
{
    public readonly int baseMeshId;
    protected MeshDefinition MeshDefinition { get; set; }
    private ShadingMode ShadingMode { get; set; }

    public override int GetPrimitiveCount() => MeshDefinition.Triangles.Length;
    public Mesh(MeshData meshData, Scene scene, Transform transform)
    {
        baseMeshId = meshData.Id;
        ShadingMode = meshData.ShadingMode;
        MaterialIndex = meshData.Material;
        MotionBlur = meshData.MotionBlur;
        Transform = transform;
        if (!string.IsNullOrEmpty(meshData.Faces.PlyData))
        {
            var plyData = new PlyData(Params.GetFilePathInSceneDir(meshData.Faces.PlyData));
            MeshDefinition = new MeshDefinition(plyData);
        }
        else
        {
            MeshDefinition = new MeshDefinition(meshData, scene.Content.VertexData, scene.Content.TexCoordData);   
        }
        TextureIndices = meshData.Textures;
        Initialize();
    }

    public Mesh(Mesh originalMesh, Transform newTransform, MeshInstance instance)
    {
        baseMeshId = originalMesh.baseMeshId;
        ShadingMode = originalMesh.ShadingMode;
        MeshDefinition = originalMesh.MeshDefinition;
        MaterialIndex = instance.Material != -1 ? instance.Material : originalMesh.MaterialIndex;
        MotionBlur = instance.MotionBlur.LengthSquared() > 0 ? instance.MotionBlur : originalMesh.MotionBlur;
        
        if (TextureIndices != null && TextureIndices.Length != 0) TextureIndices = instance.Textures;
        else TextureIndices = originalMesh.TextureIndices;
        
        Transform = newTransform;
        
        Initialize();
    }

    private void Initialize()
    {
        var bounds = MeshDefinition.GetBounds();
        Bounds = new BoundingBox(
            Transform.ToWorldPoint(bounds.Min),
            Transform.ToWorldPoint(bounds.Max)
        );
    }

    public override bool Intersect(in Ray ray, ref IntersectionInfo info)
    {
        var finalTransform = GetMotionBlurTransform(ray.Time);
        Ray localRay = finalTransform.ToLocalRay(ray);

        info.IntersectionTestEpsilon = RayTracerRenderer.IntersectionTestEpsilon;
        var hit = MeshDefinition.BVH.Intersect(in localRay, ref info);
        if (!hit) return false;

        var localHitPoint = info.Point;

        info.Point = finalTransform.ToWorldPoint(localHitPoint);
        info.HitGeometry = this;
        info.Distance = Vector3.Distance(ray.Origin, info.Point);
        
        var t = MeshDefinition.Triangles[info.PrimitiveIndex];
        info.GeometricNormal = ShadingMode == ShadingMode.Flat
            ? finalTransform.ToWorldDirection(t.Normal)
            : t.GetNormal(localHitPoint, info.PrimitiveIndex, ray.Time);
        
        return hit;
    }
    public override Vector2 GetUVCoordinates(in Vector3 point, in int primitiveIndex, float rayTime, bool tiling)
    {
        if (primitiveIndex < 0 || primitiveIndex >= MeshDefinition.Triangles.Length)
            return Vector2.Zero;

        var t = MeshDefinition.Triangles[primitiveIndex];
        var localPoint = GetMotionBlurTransform(rayTime).ToLocalPoint(point);
        return t.GetUVCoordinates(localPoint, primitiveIndex, rayTime, tiling);
    }
    public override Vector3 GetNormal(in Vector3 point, in int primitiveIndex, float rayTime)
    {
        var t = MeshDefinition.Triangles[primitiveIndex];

        if (ShadingMode == ShadingMode.Flat)
            return t.Normal;

        var finalTransform = GetMotionBlurTransform(rayTime);
        var localPoint = finalTransform.ToLocalPoint(point);
        var localNormal = t.GetNormal(localPoint, primitiveIndex, rayTime);
        return finalTransform.ToWorldNormal(localNormal);
    }

    public override void GetTBN(in Vector3 point, in int primitiveIndex, float rayTime, out Vector3 tangent, out Vector3 bitangent, out Vector3 normal)
    {
        if (primitiveIndex < 0 || primitiveIndex >= MeshDefinition.Triangles.Length)
        {
            tangent = Vector3.Zero;
            bitangent = Vector3.Zero;
            normal = Vector3.Zero;
            return;
        }
        
        var t = MeshDefinition.Triangles[primitiveIndex];
        var motionTransform = GetMotionBlurTransform(rayTime);
        var localPoint = motionTransform.ToLocalPoint(point);
        t.GetTBN(localPoint, primitiveIndex, rayTime, out tangent, out bitangent, out normal);
        
        tangent = motionTransform.ToWorldDirection(tangent, false);
        bitangent = motionTransform.ToWorldDirection(bitangent, false);
        normal = motionTransform.ToWorldDirection(normal, false);

    }
}