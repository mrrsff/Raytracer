using System.Numerics;
using System.Runtime.CompilerServices;
using Raytracer.Scenes.Runtime;

namespace Raytracer.Utility;

public static class MathUtility
{
    public static float Clamp(float value, float min, float max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    public static Vector3 ToEulerAngles(this Quaternion quat)
    {
        Vector3 angles = new Vector3();

        // roll (x-axis rotation)
        double sinr_cosp = 2 * (quat.W * quat.X + quat.Y * quat.Z);
        double cosr_cosp = 1 - 2 * (quat.X * quat.X + quat.Y * quat.Y);
        angles.X = (float)Math.Atan2(sinr_cosp, cosr_cosp);

        // pitch (y-axis rotation)
        double sinp = 2 * (quat.W * quat.Y - quat.Z * quat.X);
        if (Math.Abs(sinp) >= 1)
            angles.Y = (float)(Math.CopySign(Math.PI / 2, sinp)); // use 90 degrees if out of range
        else
            angles.Y = (float)Math.Asin(sinp);

        // yaw (z-axis rotation)
        double siny_cosp = 2 * (quat.W * quat.Z + quat.X * quat.Y);
        double cosy_cosp = 1 - 2 * (quat.Y * quat.Y + quat.Z * quat.Z);
        angles.Z = (float)Math.Atan2(siny_cosp, cosy_cosp);

        return angles;
    }
}