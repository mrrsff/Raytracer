using System.Numerics;
using System.Runtime.CompilerServices;

namespace Raytracer.Utility;

public static class MathUtility
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Clamp(float value, float min, float max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Lerp(float a, float b, float t)
    {
        return a + t * (b - a);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 ConcentricDiskSample(Vector2 u)
    {
        // map [0,1)^2 to [-1,1]^2
        float sx = 2.0f * u.X - 1.0f;
        float sy = 2.0f * u.Y - 1.0f;

        if (sx == 0 && sy == 0)
            return Vector2.Zero;

        float r, theta;
        if (MathF.Abs(sx) > MathF.Abs(sy))
        {
            r = sx;
            theta = (MathF.PI / 4.0f) * (sy / sx);
        }
        else
        {
            r = sy;
            theta = (MathF.PI / 2.0f) - (MathF.PI / 4.0f) * (sx / sy);
        }

        return new Vector2(r * MathF.Cos(theta), r * MathF.Sin(theta));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 ToEulerAngles(this Quaternion quat)
    {
        Vector3 angles = new Vector3();

        double sinr_cosp = 2 * (quat.W * quat.X + quat.Y * quat.Z);
        double cosr_cosp = 1 - 2 * (quat.X * quat.X + quat.Y * quat.Y);
        angles.X = (float)Math.Atan2(sinr_cosp, cosr_cosp);

        // pitch (y-axis rotation)
        double sinp = 2 * (quat.W * quat.Y - quat.Z * quat.X);
        if (Math.Abs(sinp) >= 1)
            angles.Y = (float)(Math.CopySign(Math.PI / 2, sinp));
        else
            angles.Y = (float)Math.Asin(sinp);

        // yaw (z-axis rotation)
        double siny_cosp = 2 * (quat.W * quat.Z + quat.X * quat.Y);
        double cosy_cosp = 1 - 2 * (quat.Y * quat.Y + quat.Z * quat.Z);
        angles.Z = (float)Math.Atan2(siny_cosp, cosy_cosp);

        return angles;
    }
}