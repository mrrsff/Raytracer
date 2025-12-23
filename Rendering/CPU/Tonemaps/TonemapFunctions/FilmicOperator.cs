using System.Numerics;
using Raytracer.IO.Images;
using Raytracer.Scenes.Content.Datas.CameraData;

namespace Raytracer.Rendering.CPU.Tonemaps.TonemapFunctions;

public class FilmicOperator : TonemapOperator
{
    private const float A = 0.22f;
    private const float B = 0.30f;
    private const float C = 0.10f;
    private const float D = 0.20f;
    private const float E = 0.01f;
    private const float F = 0.30f;

    private const float WhitePoint = 11.2f;
    private readonly float whiteScale;

    public FilmicOperator(TMOOptions options) : base(options)
    {
        // Precompute normalization factor
        whiteScale = 1.0f / FilmicCurve(WhitePoint);
    }

    public override Vector3 Tonemap(ImageBuffer image, int x, int y)
    {
        Vector3 color = image.GetPixel(x, y);
        return new Vector3(
            FilmicCurve(color.X) * whiteScale,
            FilmicCurve(color.Y) * whiteScale,
            FilmicCurve(color.Z) * whiteScale
        );
    }

    private static float FilmicCurve(float x)
    {
        return ((x * (A * x + C * B) + D * E) / (x * (A * x + B) + D * F)) - (E / F);
    }
}