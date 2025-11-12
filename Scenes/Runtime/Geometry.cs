using System.Numerics;
using Raytracer.Core;
using Raytracer.Rendering.Intersections;
using Raytracer.Scenes.Runtime.Meshes;

namespace Raytracer.Scenes.Runtime;

public abstract class Geometry
{
    public BoundingBox? Bounds { get; protected set; }
    public Vector3 Centroid => Bounds != null ? (Bounds.Min + Bounds.Max) * 0.5f : Vector3.Zero;
    public int MaterialIndex { get; set; }
    public abstract bool Intersect(in Ray ray, ref IntersectionInfo info);
    public virtual int GetPrimitiveCount() => 1;
}