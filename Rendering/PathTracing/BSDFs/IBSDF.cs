using System.Numerics;
using Raytracer.Rendering.Intersections;

namespace Raytracer.Rendering.PathTracing.BSDFs;

public interface IBSDF
{
    /// <summary>
    /// Samples an incoming direction wi given an outgoing direction wo and intersection info.
    /// </summary>
    /// <param name="wo">The outgoing direction (view direction).</param>
    /// <param name="hit">The intersection information at the surface point.</param>
    /// <param name="u">A 2D random sample in [0,1)^2.</param>
    /// <param name="importanceSample">Whether to use importance sampling.</param>
    /// <returns>A BSDFSample containing the sampled direction, BSDF value, PDF, and flags.</returns>
    BSDFSample Sample(Vector3 wo, IntersectionInfo hit, Vector2 u, bool importanceSample);

    /// <summary>
    /// Evaluates the BSDF for given outgoing and incoming directions at the intersection point.
    /// </summary>
    /// <param name="wo">The outgoing direction (view direction).</param>
    /// <param name="wi">The incoming direction (light direction).</param>
    /// <param name="hit">The intersection information at the surface point.</param>
    /// <returns>The BSDF value as a Vector3.</returns>
    Vector3 Evaluate(Vector3 wo, Vector3 wi, IntersectionInfo hit);
    
    /// <summary>
    /// Computes the PDF for sampling the incoming direction wi given the outgoing direction wo.
    /// </summary>
    /// <param name="wo">The outgoing direction (view direction).</param>
    /// <param name="wi">The incoming direction (light direction).</param>
    /// <param name="hit">The intersection information at the surface point.</param>
    /// <returns>The PDF value as a float.</returns>
    float Pdf(Vector3 wo, Vector3 wi, IntersectionInfo hit);

    /// <summary>
    /// Indicates whether the BSDF represents a delta distribution (e.g., perfect mirror or glass).
    /// </summary>
    bool IsDelta { get; }
}