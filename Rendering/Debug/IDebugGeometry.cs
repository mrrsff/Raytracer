using System.Numerics;

namespace Raytracer.Rendering;

public interface IDebugGeometry
{
    Vector3 Min { get; }
    Vector3 Max { get; }
}