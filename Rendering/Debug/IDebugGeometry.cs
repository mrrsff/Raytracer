using System.Numerics;

namespace Raytracer.Rendering.Debug;

public interface IDebugGeometry
{
    Vector3 Min { get; }
    Vector3 Max { get; }
}