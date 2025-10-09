namespace Raytracer.Core;

public struct Rect(float xMin, float yMin, float xMax, float yMax)
{
    public float XMin = xMin;
    public float YMin = yMin;
    public float XMax = xMax;
    public float YMax = yMax;

    public override string ToString()
    {
        return $"Rect(XMin: {XMin}, YMin: {YMin}, XMax: {XMax}, YMax: {YMax})";
    }
}