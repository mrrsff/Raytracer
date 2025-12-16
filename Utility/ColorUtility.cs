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

    public static Vector3 Normalize(Vector3 color)
    {
        return Vector3.Clamp(color, Black, White * 255) / 255f;
    }
    
    public static float SRGBToLinear(float c) =>
        c <= 0.04045f ? c / 12.92f : MathF.Pow((c + 0.055f) / 1.055f, 2.4f);
    
    public static Vector3 SRGBToLinear(Vector3 color)
    {
        return new Vector3(
            SRGBToLinear(color.X),
            SRGBToLinear(color.Y),
            SRGBToLinear(color.Z)
        );
    }

    public static float LinearToSRGB(float c) =>
        c <= 0.0031308f ? 12.92f * c : 1.055f * MathF.Pow(c, 1f / 2.4f) - 0.055f;
    
    public static Vector3 LinearToSRGB(Vector3 color) 
    {
        return new Vector3(
            LinearToSRGB(color.X),
            LinearToSRGB(color.Y),
            LinearToSRGB(color.Z)
        );
    }
}