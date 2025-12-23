using System.Numerics;
using Raytracer.Core;
using Raytracer.IO.Images;
using Raytracer.Rendering.CPU.Tonemaps.TonemapFunctions;
using Raytracer.Scenes.Content.Datas.CameraData;
using Raytracer.Scenes.Content.Datas.Textures;

namespace Raytracer.Rendering.CPU.Tonemaps;

public class Tonemapper
{
    private readonly TonemapOperator _operator;
    private readonly float saturation;
    private readonly float gamma;

    public Tonemapper(TonemapOperator tonemapOperator, TonemapData data)
    {
        _operator = tonemapOperator;
        saturation = data.Saturation;
        gamma = data.Gamma;
    }
    
    public void Prepare(ImageBuffer image)
    {
        _operator.Prepare(image);
    }

    public Vector3 Apply(ImageBuffer buffer, int x, int y)
    {
        Vector3 c = _operator.Tonemap(buffer, x, y);

        const float TOLERANCE = 1e-3f;
        if (Math.Abs(saturation - 1.0f) > TOLERANCE)
        {
            float Ld = TonemapOperator.Luminance(c);

            if (Ld > 0.0f)
            {
                c = new Vector3(
                    Ld * MathF.Pow(c.X / Ld, saturation),
                    Ld * MathF.Pow(c.Y / Ld, saturation),
                    Ld * MathF.Pow(c.Z / Ld, saturation)
                );
            }
        }

        c.X = Math.Clamp(c.X, 0.0f, 1.0f);
        c.Y = Math.Clamp(c.Y, 0.0f, 1.0f);
        c.Z = Math.Clamp(c.Z, 0.0f, 1.0f);

        float invGamma = 1.0f / gamma;
        c = new Vector3(
            MathF.Pow(c.X, invGamma),
            MathF.Pow(c.Y, invGamma),
            MathF.Pow(c.Z, invGamma)
        );
        return c;
    }
}