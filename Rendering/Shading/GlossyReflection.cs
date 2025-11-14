using System.Numerics;
using System.Runtime.CompilerServices;
using Raytracer.Utility;

namespace Raytracer.Rendering.Shading;

public static class GlossyReflection
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 PerturbDirection(Vector3 direction, float roughness, Vector2 sample)
    {
        return SampleConeAround(direction, roughness, sample);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector3 SampleConeAround(Vector3 direction, float roughness, Vector2 sample)
    {
        MathUtility.BuildONB(direction, out var u, out var v);
        var rd = direction + roughness * ((sample.X - 0.5f) * u + (sample.Y - 0.5f) * v);
        return Vector3.Normalize(rd);
    }
}