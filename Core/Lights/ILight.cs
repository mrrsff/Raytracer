using System.Numerics;
using Raytracer.Rendering;
using Raytracer.Scenes.Content;
using Raytracer.Scenes.Content.Datas;

namespace Raytracer.Core.Lights;

public interface ILight
{
    /// <summary>
    /// Samples the light to get the direction and irradiance at point P with normal N.
    /// </summary>
    /// <param name="P"> The point being shaded.</param>
    /// <param name="N"> The normal at point P.</param>
    /// <param name="time"> The time for motion blur effects.</param>
    /// <param name="renderer"> The renderer instance.</param>
    /// <param name="L"> The output direction towards the light.</param>
    /// <param name="irradiance"> The output irradiance from the light at point P.</param>
    /// <returns> True if the light contributes to point P, false otherwise.</returns>
    public bool Sample(in Vector3 P, in Vector3 N, float time, Renderer renderer, out Vector3 L,
        out Vector3 irradiance);
    
    public virtual void Initialize()
    {
        // Optional initialization logic for lights can be added here.
    }
}