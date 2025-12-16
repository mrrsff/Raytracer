using System.Numerics;
using Raytracer.Core;
using Raytracer.Rendering;
using Raytracer.Rendering.Intersections;
using Raytracer.Rendering.Raytracing;
using Raytracer.Scenes.Content.Datas.Objects;

namespace Raytracer.Scenes.Runtime.Meshes;

public class Mesh : Geometry
{
    public readonly int baseMeshId;
    private MeshDefinition MeshDefinition { get; set; }
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
        info.Normal = finalTransform.ToWorldDirection(info.Normal);
        info.HitGeometry = this;

        if (info.Normal.LengthSquared() < 1e-12f)
            return false; // invalid hit

        info.Distance = Vector3.Distance(ray.Origin, info.Point);
        if (ShadingMode == ShadingMode.Flat || info.PrimitiveIndex < 0 ||
            info.PrimitiveIndex >= MeshDefinition.Triangles.Length) return hit;

        var t = MeshDefinition.Triangles[info.PrimitiveIndex];
        t.CalculateBarycentricCoordinates(localHitPoint, out var alpha, out var beta, out var gamma);

        Vector3 n0 = MeshDefinition.VertexNormals[t.I0];
        Vector3 n1 = MeshDefinition.VertexNormals[t.I1];
        Vector3 n2 = MeshDefinition.VertexNormals[t.I2];
        Vector3 localNormal = Vector3.Normalize(alpha * n0 + beta * n1 + gamma * n2);

        info.Normal = Vector3.Normalize(finalTransform.ToWorldDirection(localNormal));
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
}