using System.Numerics;
using System.Runtime.CompilerServices;
using Raytracer.Core;
using Raytracer.Scenes.Content.Datas.Camera;
using Raytracer.Scenes.Runtime;
using Raytracer.Scenes.Runtime.Textures;

namespace Raytracer.Rendering.Intersections;

public struct IntersectionInfo()
{
    public Vector3 RayOrigin;
    public Material? material = null;
    public Texture[]? Textures = null;
    public int PrimitiveIndex = -1;
    public bool Hit = false;
    public float Distance = float.MaxValue;
    public Vector3 Point = default;
    public Vector3 Normal = default;
    public float IntersectionTestEpsilon;
    public Geometry HitGeometry = null!;
    public int XPixel = 0;
    public int YPixel = 0;
    public Camera Camera;
    public float RayTime = 0f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Reset()
    {
        RayOrigin = default;
        material = null;
        PrimitiveIndex = -1;
        Hit = false;
        Distance = float.MaxValue;
        Point = default;
        Normal = default;
    }

    public static IntersectionInfo NoHit => new IntersectionInfo() { };
    
    public Vector2 GetUVCoordinates(bool tiling)
    {
        return HitGeometry.GetUVCoordinates(Point, PrimitiveIndex, RayTime, tiling);
    }
}