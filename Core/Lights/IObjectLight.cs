using System.Numerics;
using Raytracer.Rendering;

namespace Raytracer.Core.Lights;

public interface IObjectLight : ILight
{
    public float Pdf(Vector3 P, Vector3 N, float time, Renderer renderer);

}