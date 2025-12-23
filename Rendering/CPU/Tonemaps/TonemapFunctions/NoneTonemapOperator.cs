using System.Numerics;
using Raytracer.IO.Images;
using Raytracer.Scenes.Content.Datas.CameraData;

namespace Raytracer.Rendering.CPU.Tonemaps.TonemapFunctions;

public class NoneTonemapOperator : TonemapOperator
{
    public NoneTonemapOperator(TMOOptions options) : base(options)
    {
    }

    public override Vector3 Tonemap(ImageBuffer image, int x, int y)
    {
        return image.GetPixel(x, y);
    }
}