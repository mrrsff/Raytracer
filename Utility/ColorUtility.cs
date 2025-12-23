using System.Numerics;

namespace Raytracer.Utility;

public static class ColorUtility
{
    // Define some color constants
    public static readonly Vector3 Black = new(0);
    public static readonly Vector3 White = new(255);
    public static readonly Vector3 Yellow = new(1, 1, 0);
    public static readonly Vector3 Cyan = new(0, 1, 1);

    public static Vector3 Normalize(Vector3 color)
    {
        return Vector3.Clamp(color, Black, White) / 255f;
    }
}