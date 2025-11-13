using System.Numerics;

namespace Raytracer.Core;

public struct Ray(Vector3 origin, Vector3 direction, bool isSecondary = false)
{
    public readonly bool IsSecondary = isSecondary;
    public Vector3 Origin = origin;
    public Vector3 Direction = Vector3.Normalize(direction);
    public float Time = 0;

    public override string ToString()
    {
        return $"Ray(Origin: {Origin}, Direction: {Direction})";
    }
}