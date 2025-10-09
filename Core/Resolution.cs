namespace Raytracer.Core;

public struct Resolution(int width, int height)
{
    public int Width = width;
    public int Height = height;

    public override string ToString()
    {
        return $"Resolution(Width: {Width}, Height: {Height})";
    }
}