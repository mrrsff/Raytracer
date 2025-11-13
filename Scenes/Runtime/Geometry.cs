using System.Numerics;
using Raytracer.Core;
using Raytracer.Rendering.Intersections;
using Raytracer.Scenes.Runtime.Meshes;

namespace Raytracer.Scenes.Runtime;

public abstract class Geometry
{
    public Transform Transform { get; set; } = new Transform();
    public BoundingBox? Bounds { get; protected set; }
    public Vector3 Centroid => Bounds != null ? (Bounds.Min + Bounds.Max) * 0.5f : Vector3.Zero;
    public int MaterialIndex { get; set; }
    
    public bool HasMotionBlur => MotionBlur != Vector3.Zero;
    public Vector3 MotionBlur = Vector3.Zero;
    
    protected Transform GetMotionBlurTransform(float time)
    {
        if (!HasMotionBlur)
            return Transform;

        var motionTransform = Transform.Copy();
        motionTransform.Translate(MotionBlur * time);
        return motionTransform;
    }
    
    public abstract bool Intersect(in Ray ray, ref IntersectionInfo info);
    public virtual int GetPrimitiveCount() => 1;
}