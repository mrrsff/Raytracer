using System.Numerics;

namespace Raytracer.Rendering.Shading.BRDFs;

public interface IBRDF
{
    Vector3 Evaluate(
        Vector3 kd,
        Vector3 ks,
        Vector3 wi,   // incoming light direction (towards surface)
        Vector3 wo,   // outgoing/view direction
        Vector3 n     // shading normal
    );
}