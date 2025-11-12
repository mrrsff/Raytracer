using System.Numerics;
using System.Runtime.CompilerServices;
using Raytracer.Core;
using Raytracer.Scenes.Content.Datas.Objects;
using Raytracer.Scenes.Runtime;

namespace Raytracer.Rendering.Intersections;

public struct IntersectionInfo()
{
    public Vector3 RayOrigin;
    public Vector3 RayDirection;
    public Material? material = null;
    public int PrimitiveIndex = -1;
    public bool Hit = false;
    public float Distance = float.MaxValue;
    public Vector3 Point = default;
    public Vector3 Normal = default;
    public float IntersectionTestEpsilon;
    public Geometry? HitGeometry = null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Reset()
    {
        RayOrigin = default;
        RayDirection = default;
        material = null;
        PrimitiveIndex = -1;
        Hit = false;
        Distance = float.MaxValue;
        Point = default;
        Normal = default;
    }

    public static IntersectionInfo NoHit => new IntersectionInfo() { };
}