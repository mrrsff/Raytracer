using System.Numerics;
using Raytracer.Core;
using Raytracer.Scenes.Content.Datas.Objects;

namespace Raytracer.Rendering.Intersections;

public struct IntersectionInfo()
{
    public Ray HitRay = default;
    public Material? material = null;
    public int PrimitiveIndex = -1;
    public bool Hit = false;
    public float Distance = float.MaxValue;
    public Vector3 Point = default;
    public Vector3 Normal = default;
    public float IntersectionTestEpsilon;

    public void Reset()
    {
        HitRay = default;
        material = null;
        PrimitiveIndex = -1;
        Hit = false;
        Distance = float.MaxValue;
        Point = default;
        Normal = default;
    }
    public static IntersectionInfo NoHit => new IntersectionInfo() {};
}