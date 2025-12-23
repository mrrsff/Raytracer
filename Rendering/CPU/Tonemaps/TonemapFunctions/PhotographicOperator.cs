using System.Numerics;
using Raytracer.Core;
using Raytracer.IO.Images;
using Raytracer.Scenes.Content.Datas.CameraData;

namespace Raytracer.Rendering.CPU.Tonemaps.TonemapFunctions;

public class PhotographicOperator : TonemapOperator
{
    private float alpha = 0.18f;
    private float burnOutPercentage = 1.0f;

    private float logAverageLuminance = 0.0f;
    private float burnOutLuminance = 0.0f;
    private bool calculateBurnOut;
    public PhotographicOperator(TMOOptions options) : base(options)
    {
        if (options.Params.Length >= 1) alpha = options.Params[0];
        if (options.Params.Length >= 2) burnOutPercentage = options.Params[1];
    }

    public override void Prepare(ImageBuffer image)
    {
        const float eps = 1e-6f;
        int pixelCount = image.Width * image.Height;

        float logSum = 0.0f;

        calculateBurnOut = burnOutPercentage > 0;

        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                Vector3 color = image.GetPixel(x, y);
                float Y = Luminance(color);

                logSum += MathF.Log(eps + Y);
            }
        }

        logAverageLuminance = MathF.Exp(logSum / pixelCount);

        if (calculateBurnOut)
        {
            int idx = 0;
            float[] luminances = new float[pixelCount];

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    float Y = Luminance(image.GetPixel(x, y));
                    float Yscaled = alpha * (Y / logAverageLuminance);
                    luminances[idx++] = Yscaled;
                }
            }

            Array.Sort(luminances);

            int index = (int)((1.0f - burnOutPercentage / 100.0f) * (luminances.Length - 1));

            burnOutLuminance = luminances[index];
        }
    }

    public override Vector3 Tonemap(ImageBuffer image, int x, int y)
    {
        Vector3 color = image.GetPixel(x, y);
        float L = Luminance(color);

        if (L <= 0.0f)
            return Vector3.Zero;
        
        float Llocal = FindRegionScale(x, y);
        float L_zoned = alpha * (L / Llocal);

        float burnOutMultiplier = 1.0f;
        if (calculateBurnOut)
        {
            float Lwhite2 = burnOutLuminance * burnOutLuminance;
            burnOutMultiplier = 1.0f + (L_zoned) / Lwhite2;
        }
        
        float L_compressed = (L_zoned * burnOutMultiplier) / (1.0f + L_zoned);

        var result = color * L_compressed / L;
        return result;
    }
    
    private float FindRegionScale(int x, int y)
    {
        return logAverageLuminance;
    }
}