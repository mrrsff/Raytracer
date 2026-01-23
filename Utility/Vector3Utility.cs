using System.Numerics;

namespace Raytracer.Utility;

public static class Vector3Utility
{
    public static float MaxComponent(this Vector3 v)
    {
        return MathF.Max(v.X, MathF.Max(v.Y, v.Z));
    }
    
    public static float MinComponent(this Vector3 v)
    {
        return MathF.Min(v.X, MathF.Min(v.Y, v.Z));
    }
    
    public static Vector3 Abs(this Vector3 v)
    {
        return new Vector3(MathF.Abs(v.X), MathF.Abs(v.Y), MathF.Abs(v.Z));
    }
    
    public static bool IsNaN(this Vector3 v)
    {
        return float.IsNaN(v.X) || float.IsNaN(v.Y) || float.IsNaN(v.Z);
    }
    
    public static float AbsDot(this Vector3 a, Vector3 b)
    {
        return MathF.Abs(Vector3.Dot(a, b));
    }
    
    public static bool IsZero(this Vector3 v)
    {
        return v.X == 0f && v.Y == 0f && v.Z == 0f;
    }
    
    public static Vector3 Clamp(this Vector3 v, float min, float max)
    {
        return new Vector3(
            Math.Clamp(v.X, min, max),
            Math.Clamp(v.Y, min, max),
            Math.Clamp(v.Z, min, max)
        );
    }
    
    public static Vector3 Lerp(this Vector3 a, Vector3 b, float t)
    {
        return a + t * (b - a);
    }
}