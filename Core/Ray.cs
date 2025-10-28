using System.Numerics;

namespace Raytracer.Core;

public struct Ray(Vector3 origin, Vector3 direction, bool isSecondary = false, float tMin = 0.001f, float tMax = float.MaxValue)
{
    public bool IsSecondary = isSecondary;
    public Vector3 Origin = origin;
    public Vector3 Direction = Vector3.Normalize(direction);
    public float TMin = tMin;
    public float TMax = tMax;
    
    public Vector3 At(float t) => Origin + t * Direction;

    public override string ToString()
    {
        return $"Ray(Origin: {Origin}, Direction: {Direction}, TMin: {TMin}, TMax: {TMax})";
    }
}