using System.Numerics;
using Raytracer.Rendering.Intersections;

namespace Raytracer.Rendering.PathTracing;

[Flags]
public enum BSDFSampleFlags
{
    None         = 0,
    Delta        = 1 << 0,   // mirror, glass
    Diffuse      = 1 << 1,
    Glossy       = 1 << 2,
    Transmission = 1 << 3,
}

public sealed class BSDFSample
{
    /// <summary>
    /// The incoming direction.
    /// </summary>
    public Vector3 Wi;
    
    /// <summary>
    /// The BSDF value for the sample. (f(wi, wo)) - radiance reflected from wi to wo
    /// </summary>
    public Vector3 F;
    
    /// <summary>
    /// The PDF value for the sample. (probability density function)
    /// </summary>
    public float Pdf;

    /// <summary>
    /// The flags describing the sample type.
    /// </summary>
    public readonly BSDFSampleFlags Flags;

    /// <summary>
    /// Indicates whether the sample is a delta distribution. (e.g., perfect mirror or glass)
    /// </summary>
    public bool IsDelta => (Flags & BSDFSampleFlags.Delta) != 0;

    /// <summary>
    /// Creates a new BSDF sample.
    /// </summary>
    /// <param name="wi"> The incoming direction.</param>
    /// <param name="f"> The BSDF value for the sample.</param>
    /// <param name="pdf"> The PDF value for the sample.</param>
    /// <param name="flags"> The flags describing the sample type.</param>
    public BSDFSample(Vector3 wi, Vector3 f, float pdf, BSDFSampleFlags flags)
    {
        Wi = wi;
        F = f;
        Pdf = pdf;
        Flags = flags;
    }
    
    public static BSDFSample Invalid => new BSDFSample(Vector3.Zero, Vector3.Zero, -1, BSDFSampleFlags.None);
    public static bool IsValidSample(BSDFSample sample) => sample.Pdf > 0f;
}
