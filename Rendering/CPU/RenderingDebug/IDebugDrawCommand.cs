using System.Numerics;
using Raytracer.IO.Images;
using Raytracer.Scenes.Content.Datas.CameraData;

namespace Raytracer.Rendering.CPU.RenderingDebug;

public interface IDebugDrawCommand
{
    public Vector3 Color { get; set; }
    void Draw(Camera camera, ImageBuffer buffer);
}