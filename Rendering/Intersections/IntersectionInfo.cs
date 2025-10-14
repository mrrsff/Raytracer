using System.Numerics;
using Raytracer.Core;

namespace Raytracer.Rendering.Intersections;

public class IntersectionInfo
{
    // Data about an intersection
    public Ray HitRay;
    public Material material;
    public bool Hit;
    public float Distance = float.MaxValue;
    public Vector3 Point;
    public Vector3 Normal;
    public static IntersectionInfo NoHit => new IntersectionInfo() {};
}