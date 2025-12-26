using System.Numerics;
using Raytracer.Core;
using Raytracer.Rendering.CPU.Intersections;
using Raytracer.Scenes.Runtime.Meshes;
using Raytracer.Scenes.Runtime.Textures;

namespace Raytracer.Scenes.Runtime;

public abstract class Geometry
{
    public Transform Transform = Transform.Identity;
    public BoundingBox Bounds;
    public int MaterialIndex;
    public Vector3 MotionBlur = Vector3.Zero;
    public int[] TextureIndices = [];
    
    public bool HasMotionBlur => MotionBlur != Vector3.Zero;
    public Vector3 Centroid => Bounds != null ? (Bounds.Min + Bounds.Max) * 0.5f : Vector3.Zero;
    public Transform GetMotionBlurTransform(float time)
    {
        if (!HasMotionBlur)
            return Transform;

        var motionTransform = Transform.Copy();
        motionTransform.Translate(MotionBlur * time);
        return motionTransform;
    }
    
    public abstract bool Intersect(in Ray ray, ref IntersectionInfo info);
    public virtual int GetPrimitiveCount() => 1;
    public abstract Vector2 GetUVCoordinates(in Vector3 point, in int primitiveIndex, float rayTime, bool tiling);
    public abstract Vector3 GetNormal(in Vector3 point, in int primitiveIndex, float rayTime);
    public abstract void GetTBN(in Vector3 point, in int primitiveIndex, float rayTime, out Vector3 tangent, out Vector3 bitangent, out Vector3 normal);
}