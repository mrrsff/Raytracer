using System.Numerics;
using Raytracer.Core;
using Raytracer.Scenes.Content.Datas.Objects;

namespace Raytracer.Rendering.Intersections;

public struct IntersectionInfo()
{
    // Data about an intersection
    public Ray HitRay = default;
    public Material material = default;
    public bool Hit = false;
    public float Distance = float.MaxValue;
    public Vector3 Point = default;
    public Vector3 Normal = default;

    public void Reset()
    {
        HitRay = default;
        material = default;
        Hit = false;
        Distance = float.MaxValue;
        Point = default;
        Normal = default;
    }
    public static IntersectionInfo NoHit => new IntersectionInfo() {};
}