using System.Numerics;
using Raytracer.Core;
using Raytracer.IO.Images;
using Raytracer.Scenes.Content.Datas.CameraData;

namespace Raytracer.Rendering.CPU.Tonemaps.TonemapFunctions;

public abstract class TonemapOperator
{
    protected readonly float alpha = 0.18f;
    protected readonly float burnOutPercentage = 0.0f;
    
    protected float logAverageLuminance = 0.0f;
    protected float exposure = 1.0f;
    protected float burnOutLuminance = 0.0f;
    protected TonemapOperator(TMOOptions options)
    {
        if (options.Params.Length >= 1) alpha = options.Params[0];
        if (options.Params.Length >= 2) burnOutPercentage = options.Params[1];
    }

    public void Prepare(ImageBuffer image)
    {
        const float eps = 1e-6f;
        int pixelCount = image.Width * image.Height;

        float logSum = 0.0f;

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
        
        exposure = alpha / logAverageLuminance;
        
        if (burnOutPercentage > 0) burnOutLuminance = GetBurnoutLuminance(image);
    }

    private float GetBurnoutLuminance(ImageBuffer image)
    {
        int pixelCount = image.Width * image.Height;
        float[] luminances = new float[pixelCount];
        int idx = 0;

        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                float Y = Luminance(image.GetPixel(x, y));
                luminances[idx++] = Y;
            }
        }

        Array.Sort(luminances);
        int burnOutIndex = (int)(pixelCount * (100f - burnOutPercentage));
        var luminance = luminances[Math.Clamp(burnOutIndex, 0, pixelCount - 1)];
        return luminance;
    }
    
    public abstract Vector3 Tonemap(ImageBuffer image, int x, int y);
    public static float Luminance(in Vector3 color)
    {
        return 0.2126f * color.X +
               0.7152f * color.Y +
               0.0722f * color.Z;
    }
    protected static float saturate(float value)
    {
        return MathF.Min(MathF.Max(value, 0.0f), 1.0f);
    }
}