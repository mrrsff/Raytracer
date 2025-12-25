using System.Numerics;
using Raytracer.IO.Images;
using Raytracer.Scenes.Content.Datas.CameraData;

namespace Raytracer.Rendering.CPU.Tonemaps.TonemapFunctions;

public class FilmicOperator : TonemapOperator
{
    private const float a = 0.22f;
    private const float b = 0.30f;
    private const float c = 0.10f;
    private const float d = 0.20f;
    private const float e = 0.01f;
    private const float f = 0.30f;
    public FilmicOperator(TMOOptions options) : base(options) { }

    public override Vector3 Tonemap(ImageBuffer image, int x, int y)
    {
        Vector3 color = image.GetPixel(x, y);
        color *= exposure;
        float L = Luminance(color);
        float Lm = Map(L) / Map(burnOutLuminance);
        return color * (Lm / L);
    }

    private static float Map(float L)
    {
        var x = ((L * (a * L + c * b) + (d * e)) /
            (L * (a * L + b) + (d * f)) - (e / f));
        return saturate(x);
    }
}