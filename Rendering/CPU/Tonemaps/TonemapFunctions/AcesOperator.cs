using System.Numerics;
using Raytracer.IO.Images;
using Raytracer.Scenes.Content.Datas.CameraData;

namespace Raytracer.Rendering.CPU.Tonemaps.TonemapFunctions;

public class AcesOperator : TonemapOperator
{
    private const float a = 2.51f;
    private const float b = 0.03f;
    private const float c = 2.43f;
    private const float d = 0.59f;
    private const float e = 0.14f;
    public AcesOperator(TMOOptions options) : base(options) { }
    public override Vector3 Tonemap(ImageBuffer image, int x, int y)
    {
        Vector3 color = image.GetPixel(x, y);
        color *= exposure;
        float L = Luminance(color);
        float Lm = Map(L);
        return color * (Lm / L);
    }

    private static float Map(float L)
    {
        var x = (L * (a * L + b) /
            (L * (L * c + d)) + e);
        return saturate(x);
    }
}