using System.Numerics;

namespace Raytracer.Utility;

public static class ColorUtility
{
    // Define some color constants
    public static readonly Vector3 Black = new(0, 0, 0);
    public static readonly Vector3 White = new(1, 1, 1);
    public static readonly Vector3 Red = new(1, 0, 0);
    public static readonly Vector3 Green = new(0, 1, 0);
    public static readonly Vector3 Blue = new(0, 0, 1);
    public static readonly Vector3 Yellow = new(1, 1, 0);
    public static readonly Vector3 Cyan = new(0, 1, 1);
    public static readonly Vector3 Magenta = new(1, 0, 1);
    public static readonly Vector3 Gray = new(0.5f, 0.5f, 0.5f);
    
    public static Vector3 Clamp(Vector3 color)
    {
        return Vector3.Clamp(color, Black, White * 255) / 255f;
    }
}