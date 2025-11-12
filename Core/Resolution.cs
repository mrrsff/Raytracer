namespace Raytracer.Core;

public struct Resolution(int width, int height)
{
    public int Width = width;
    public int Height = height;
    public int X => Width;
    public int Y => Height;

    public override string ToString()
    {
        return $"Resolution(Width: {Width}, Height: {Height})";
    }
}