namespace Raytracer.Core;

public struct Rect(float left, float bottom, float right, float top)
{
    public float Left = left;
    public float Bottom = bottom;
    public float Right = right;
    public float Top = top;

    public override string ToString()
    {
        return $"Rect(XMin: {Left}, YMin: {Bottom}, XMax: {Right}, YMax: {Top})";
    }
}