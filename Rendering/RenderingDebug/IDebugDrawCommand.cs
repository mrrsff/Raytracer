using System.Numerics;
using Raytracer.IO.Images;
using Raytracer.Scenes.Content.Datas.Camera;

namespace Raytracer.Rendering.RenderingDebug;

public interface IDebugDrawCommand
{
    public Vector3 Color { get; set; }
    void Draw(Camera camera, ImageBuffer buffer);
}