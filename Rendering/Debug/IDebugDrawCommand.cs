using System.Numerics;
using Raytracer.IO.ImageSavers;
using Raytracer.Scenes.Content.Datas.Camera;

namespace Raytracer.Rendering.Debug;

public interface IDebugDrawCommand
{
    public Vector3 Color { get; set; }
    void Draw(Camera camera, ImageBuffer buffer);
}