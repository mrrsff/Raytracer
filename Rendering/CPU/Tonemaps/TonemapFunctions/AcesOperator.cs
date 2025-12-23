using System.Numerics;
using Raytracer.IO.Images;
using Raytracer.Scenes.Content.Datas.CameraData;

namespace Raytracer.Rendering.CPU.Tonemaps.TonemapFunctions;

public class AcesOperator : TonemapOperator
{
    public AcesOperator(TMOOptions options) : base(options)
    {
    }

    public override Vector3 Tonemap(ImageBuffer image, int x, int y)
    {
        Vector3 color = image.GetPixel(x, y);
        
        const float a = 2.51f;
        const float b = 0.03f;
        const float c = 2.43f;
        const float d = 0.59f;
        const float e = 0.14f;

        return (color * (a * color + new Vector3(b))) /
               (color * (c * color + new Vector3(d)) + new Vector3(e));
    }
}