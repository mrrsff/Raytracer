using System.Numerics;
using Raytracer.Rendering.CPU;
using Raytracer.Scenes.Content;
using Raytracer.Scenes.Content.Datas;

namespace Raytracer.Core.Lights;

public abstract class Light
{
    public abstract bool Sample(in Vector3 P, in Vector3 N, float time, Renderer renderer, out Vector3 L,
        out Vector3 irradiance);
    
    public virtual void Initialize()
    {
        // Optional initialization logic for lights can be added here.
    }
}